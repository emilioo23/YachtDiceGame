using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Registers the passkeys of the players and lets them log in with a passkey. A passkey proves the device and,
    /// because the authenticator checks the face, the fingerprint or the PIN, also the player; so the login with
    /// a passkey does not ask for the code of the email.
    /// </summary>
    public class PasskeyService : IPasskeyService
    {
        private const string NullDependencyMessage = "A dependency of the passkey service cannot be null.";
        private const string NullAttestationMessage = "The answer of the authenticator cannot be null.";
        private const string NullAssertionMessage = "The answer of the authenticator cannot be null.";
        private const string BadRegistrationLogMessage =
            "A passkey could not be saved because its answer is not valid.";
        private const string ExpiredLogMessage = "A passkey answer arrived with a challenge that is not valid.";
        private const string UnknownCredentialLogMessage = "Login attempt with a passkey that is not registered.";
        private const string BadAssertionLogMessage = "Login attempt with a passkey answer that is not valid.";
        private const string ClonedLogMessage = "Login attempt with a passkey whose counter went back.";
        private const string LockedLogMessage = "Login attempt with a passkey while the account is locked.";
        private const string WrongPasswordLogMessage = "Passkey removal with a wrong password.";
        private const string DeviceNameFormat = "Passkey {0}";
        private const string RegisteredLogFormat = "Player {0} registered a passkey.";
        private const string RemovedLogFormat = "Player {0} removed a passkey.";
        private const string LoggedInLogFormat = "Player {0} logged in with a passkey.";
        private const string DateFormat = "yyyy-MM-dd";
        private const string AttestationAuthDataKey = "authData";
        private const int HandleSize = 4;
        private const int ByteMask = 0xFF;
        private const int BitsPerByte = 8;

        private readonly IPlayerRepository _playerRepository;
        private readonly IPasskeyRepository _passkeyRepository;
        private readonly PasskeyChallengeStore _challengeStore;
        private readonly LoginAttemptTracker _attemptTracker;
        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read players.</param>
        /// <param name="passkeyRepository">Repository used to read and save passkeys.</param>
        /// <param name="challengeStore">Store of the challenges that wait for an answer.</param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public PasskeyService(
            IPlayerRepository playerRepository,
            IPasskeyRepository passkeyRepository,
            PasskeyChallengeStore challengeStore)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            _passkeyRepository = passkeyRepository
                ?? throw new ArgumentNullException(nameof(passkeyRepository), NullDependencyMessage);
            _challengeStore = challengeStore
                ?? throw new ArgumentNullException(nameof(challengeStore), NullDependencyMessage);
            _attemptTracker = new LoginAttemptTracker(playerRepository);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Issues the challenge to create a passkey for a player who is logged in.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <returns>The challenge and the data of the player.</returns>
        public PasskeyChallenge BeginRegistration(string displayName)
        {
            Player player = FindPlayer(displayName);
            PasskeyChallenge challenge;

            if (player == null)
            {
                challenge = new PasskeyChallenge(PasskeyStartStatus.PlayerNotFound);
            }
            else
            {
                challenge = CreateRegistrationChallenge(player);
            }

            return challenge;
        }

        /// <summary>
        /// Checks the answer of the authenticator and saves the new passkey.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <param name="attestation">Answer of the authenticator.</param>
        /// <returns>Outcome of the registration.</returns>
        /// <exception cref="ArgumentNullException">The answer is null.</exception>
        public PasskeyRegistrationStatus CompleteRegistration(string displayName, PasskeyAttestation attestation)
        {
            if (attestation == null)
            {
                throw new ArgumentNullException(nameof(attestation), NullAttestationMessage);
            }

            Player player = FindPlayer(displayName);
            PasskeyRegistrationStatus status;

            if (player == null)
            {
                status = PasskeyRegistrationStatus.PlayerNotFound;
            }
            else if (!TryTakeRegistrationChallenge(attestation, player, out PendingChallenge pending))
            {
                _logger.Warning(ExpiredLogMessage);
                status = PasskeyRegistrationStatus.ChallengeExpired;
            }
            else
            {
                status = SaveNewPasskey(player, attestation, pending);
            }

            return status;
        }

        /// <summary>
        /// Counts the passkeys of a player.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <returns>The number of passkeys; zero if the player does not exist.</returns>
        public int CountPasskeys(string displayName)
        {
            Player player = FindPlayer(displayName);
            int count = 0;

            if (player != null)
            {
                count = _passkeyRepository.FindByPlayer(player.Id).Count;
            }

            return count;
        }

        /// <summary>
        /// Lists the passkeys of a player.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <returns>The passkeys; empty if the player does not exist.</returns>
        public List<PasskeySummary> ListPasskeys(string displayName)
        {
            Player player = FindPlayer(displayName);
            var summaries = new List<PasskeySummary>();

            if (player != null)
            {
                summaries = _passkeyRepository
                    .FindByPlayer(player.Id)
                    .Select(record => new PasskeySummary { Id = record.Id, Name = record.DeviceName })
                    .ToList();
            }

            return summaries;
        }

        /// <summary>
        /// Removes a passkey of the player after checking the password.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <param name="passkeyId">Identifier of the passkey to remove.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the removal.</returns>
        public PasskeyRemovalStatus RemovePasskey(string displayName, int passkeyId, string password)
        {
            Player player = FindPlayer(displayName);
            PasskeyRemovalStatus status;

            if (player == null)
            {
                status = PasskeyRemovalStatus.PlayerNotFound;
            }
            else if (string.IsNullOrEmpty(password) || !PasswordHasher.Verify(password, player.PwdHash))
            {
                _logger.Warning(WrongPasswordLogMessage);
                status = PasskeyRemovalStatus.InvalidPassword;
            }
            else
            {
                status = DeletePasskeyOf(player, passkeyId);
                LogRemoval(status, player);
            }

            return status;
        }

        /// <summary>
        /// Issues the challenge to log in with a passkey. The player is not known yet.
        /// </summary>
        /// <returns>The challenge.</returns>
        public PasskeyChallenge BeginLogin()
        {
            PendingChallenge pending = _challengeStore.Issue(PasskeyPurpose.Login, 0);
            PasskeyChallenge challenge;

            if (pending == null)
            {
                challenge = new PasskeyChallenge(PasskeyStartStatus.Busy);
            }
            else
            {
                challenge = new PasskeyChallenge(PasskeyStartStatus.Created)
                {
                    ChallengeId = pending.Id,
                    Challenge = Base64Url.Encode(pending.Challenge)
                };
            }

            return challenge;
        }

        /// <summary>
        /// Checks the answer of the authenticator and, if it is valid, completes the login.
        /// </summary>
        /// <param name="assertion">Answer of the authenticator.</param>
        /// <returns>Result of the login attempt.</returns>
        /// <exception cref="ArgumentNullException">The answer is null.</exception>
        public LoginResult CompleteLogin(PasskeyAssertion assertion)
        {
            if (assertion == null)
            {
                throw new ArgumentNullException(nameof(assertion), NullAssertionMessage);
            }

            LoginResult result;

            if (!_challengeStore.TryTake(assertion.ChallengeId, PasskeyPurpose.Login, out PendingChallenge pending))
            {
                result = Reject(ExpiredLogMessage);
            }
            else
            {
                result = CheckAssertion(assertion, pending);
            }

            return result;
        }

        private bool TryTakeRegistrationChallenge(
            PasskeyAttestation attestation,
            Player player,
            out PendingChallenge pending)
        {
            bool isUsable = _challengeStore.TryTake(attestation.ChallengeId, PasskeyPurpose.Registration, out pending)
                && pending.PlayerId == player.Id;

            return isUsable;
        }

        private void LogRemoval(PasskeyRemovalStatus status, Player player)
        {
            if (status == PasskeyRemovalStatus.Removed)
            {
                _logger.Info(string.Format(CultureInfo.InvariantCulture, RemovedLogFormat, player.Id));
            }
        }

        private PasskeyRemovalStatus DeletePasskeyOf(Player player, int passkeyId)
        {
            PasskeyRecord record = _passkeyRepository
                .FindByPlayer(player.Id)
                .FirstOrDefault(key => key.Id == passkeyId);
            PasskeyRemovalStatus status = PasskeyRemovalStatus.PasskeyNotFound;

            if (record != null)
            {
                _passkeyRepository.Delete(record.Id);
                status = PasskeyRemovalStatus.Removed;
            }

            return status;
        }

        private Player FindPlayer(string displayName)
        {
            Player player = null;

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                player = _playerRepository.FindByName(displayName).FirstOrDefault();
            }

            return player;
        }

        private PasskeyChallenge CreateRegistrationChallenge(Player player)
        {
            List<PasskeyRecord> existing = _passkeyRepository.FindByPlayer(player.Id);
            PasskeyChallenge challenge;

            if (existing.Count >= PasskeyRules.MaxPasskeysPerPlayer)
            {
                challenge = new PasskeyChallenge(PasskeyStartStatus.TooManyPasskeys);
            }
            else
            {
                PendingChallenge pending = _challengeStore.Issue(PasskeyPurpose.Registration, player.Id);

                challenge = pending == null
                    ? new PasskeyChallenge(PasskeyStartStatus.Busy)
                    : DescribeRegistration(player, pending, existing);
            }

            return challenge;
        }

        private static PasskeyChallenge DescribeRegistration(
            Player player,
            PendingChallenge pending,
            List<PasskeyRecord> existing)
        {
            return new PasskeyChallenge(PasskeyStartStatus.Created)
            {
                ChallengeId = pending.Id,
                Challenge = Base64Url.Encode(pending.Challenge),
                UserHandle = EncodeHandle(player.Id),
                UserName = player.DisplayName,
                ExcludedCredentialIds = existing.Select(record => record.CredentialId).ToList()
            };
        }

        private PasskeyRegistrationStatus SaveNewPasskey(
            Player player,
            PasskeyAttestation attestation,
            PendingChallenge pending)
        {
            AuthenticatorData data = ReadAttestation(attestation, pending);
            PasskeyPublicKey key = null;
            PasskeyRegistrationStatus status;

            if (data == null || !PasskeyPublicKey.TryParse(data.CredentialPublicKey, out key))
            {
                _logger.Warning(BadRegistrationLogMessage);
                status = PasskeyRegistrationStatus.InvalidResponse;
            }
            else
            {
                status = Store(player, data, key);
            }

            return status;
        }

        private PasskeyRegistrationStatus Store(Player player, AuthenticatorData data, PasskeyPublicKey key)
        {
            string credentialId = Base64Url.Encode(data.CredentialId);
            PasskeyRegistrationStatus status;

            if (_passkeyRepository.FindByPlayer(player.Id).Count >= PasskeyRules.MaxPasskeysPerPlayer)
            {
                status = PasskeyRegistrationStatus.TooManyPasskeys;
            }
            else if (_passkeyRepository.FindByCredentialId(credentialId) != null)
            {
                status = PasskeyRegistrationStatus.AlreadyRegistered;
            }
            else
            {
                _passkeyRepository.Add(new PasskeyRecord
                {
                    PlayerId = player.Id,
                    CredentialId = credentialId,
                    PublicKey = Base64Url.Encode(key.ToBytes()),
                    SignCount = data.SignCount,
                    DeviceName = string.Format(
                        CultureInfo.InvariantCulture,
                        DeviceNameFormat,
                        DateTime.UtcNow.ToString(DateFormat, CultureInfo.InvariantCulture))
                });
                status = PasskeyRegistrationStatus.Registered;
                _logger.Info(string.Format(CultureInfo.InvariantCulture, RegisteredLogFormat, player.Id));
            }

            return status;
        }

        private static AuthenticatorData ReadAttestation(PasskeyAttestation attestation, PendingChallenge pending)
        {
            AuthenticatorData data = null;

            if (Base64Url.TryDecode(attestation.ClientDataJson, out byte[] clientData)
                && Base64Url.TryDecode(attestation.AttestationObject, out byte[] attestationObject)
                && ClientDataValidator.IsValid(clientData, PasskeyRules.CreateClientDataType, pending.Challenge)
                && TryReadAuthenticatorData(attestationObject, out AuthenticatorData parsed)
                && IsAcceptable(parsed)
                && parsed.CredentialId != null)
            {
                data = parsed;
            }

            return data;
        }

        private static bool TryReadAuthenticatorData(byte[] attestationObject, out AuthenticatorData data)
        {
            bool isValid = false;

            data = null;

            try
            {
                var map = new CborReader(attestationObject).ReadItem() as Dictionary<object, object>;

                isValid = map != null
                    && map.TryGetValue(AttestationAuthDataKey, out object authData)
                    && authData is byte[]
                    && AuthenticatorData.TryParse((byte[])authData, out data);
            }
            catch (FormatException)
            {
                isValid = false;
            }

            return isValid;
        }

        private static bool IsAcceptable(AuthenticatorData data)
        {
            byte[] expectedHash = ComputeHash(Encoding.UTF8.GetBytes(PasskeyRules.RelyingPartyId));

            return data.RelyingPartyHash.SequenceEqual(expectedHash) && data.IsUserPresent && data.IsUserVerified;
        }

        private static byte[] ComputeHash(byte[] data)
        {
            byte[] hash;

            using (SHA256 algorithm = SHA256.Create())
            {
                hash = algorithm.ComputeHash(data);
            }

            return hash;
        }

        private LoginResult CheckAssertion(PasskeyAssertion assertion, PendingChallenge pending)
        {
            PasskeyRecord record = FindRecord(assertion);
            ParsedAssertion parsed = ParseAssertion(assertion, pending);
            LoginResult result;

            if (record == null)
            {
                result = Reject(UnknownCredentialLogMessage);
            }
            else if (parsed == null || !IsSignatureValid(parsed, record))
            {
                result = Reject(BadAssertionLogMessage);
            }
            else
            {
                result = CompleteWith(record, parsed.Data);
            }

            return result;
        }

        private PasskeyRecord FindRecord(PasskeyAssertion assertion)
        {
            PasskeyRecord record = null;

            if (Base64Url.TryDecode(assertion.CredentialId, out byte[] credentialId))
            {
                record = _passkeyRepository.FindByCredentialId(Base64Url.Encode(credentialId));
            }

            return record;
        }

        private static ParsedAssertion ParseAssertion(PasskeyAssertion assertion, PendingChallenge pending)
        {
            ParsedAssertion parsed = null;

            if (Base64Url.TryDecode(assertion.ClientDataJson, out byte[] clientData)
                && Base64Url.TryDecode(assertion.AuthenticatorData, out byte[] authenticatorBytes)
                && Base64Url.TryDecode(assertion.Signature, out byte[] signature)
                && ClientDataValidator.IsValid(clientData, PasskeyRules.GetClientDataType, pending.Challenge)
                && AuthenticatorData.TryParse(authenticatorBytes, out AuthenticatorData data)
                && IsAcceptable(data))
            {
                parsed = new ParsedAssertion
                {
                    Data = data,
                    UserHandle = assertion.UserHandle,
                    Signature = signature,
                    SignedData = BuildSignedData(authenticatorBytes, clientData)
                };
            }

            return parsed;
        }

        private static bool IsSignatureValid(ParsedAssertion parsed, PasskeyRecord record)
        {
            return IsHandleValid(parsed.UserHandle, record.PlayerId)
                && Base64Url.TryDecode(record.PublicKey, out byte[] coseKey)
                && PasskeyPublicKey.TryParse(coseKey, out PasskeyPublicKey key)
                && key.Verify(parsed.SignedData, parsed.Signature);
        }

        private static byte[] BuildSignedData(byte[] authenticatorBytes, byte[] clientData)
        {
            byte[] clientDataHash = ComputeHash(clientData);
            var signedData = new byte[authenticatorBytes.Length + clientDataHash.Length];

            Array.Copy(authenticatorBytes, 0, signedData, 0, authenticatorBytes.Length);
            Array.Copy(clientDataHash, 0, signedData, authenticatorBytes.Length, clientDataHash.Length);

            return signedData;
        }

        private static bool IsHandleValid(string userHandle, int playerId)
        {
            bool isValid = true;

            if (!string.IsNullOrEmpty(userHandle))
            {
                isValid = userHandle == EncodeHandle(playerId);
            }

            return isValid;
        }

        private static string EncodeHandle(int playerId)
        {
            var handle = new byte[HandleSize];

            for (int index = 0; index < HandleSize; index++)
            {
                handle[HandleSize - 1 - index] = (byte)((playerId >> (index * BitsPerByte)) & ByteMask);
            }

            return Base64Url.Encode(handle);
        }

        private LoginResult CompleteWith(PasskeyRecord record, AuthenticatorData data)
        {
            LoginLock currentLock = _attemptTracker.GetLock(record.PlayerId, DateTime.UtcNow);
            bool isCounterBack = (record.SignCount > 0 || data.SignCount > 0) && data.SignCount <= record.SignCount;
            LoginResult result;

            if (isCounterBack)
            {
                result = Reject(ClonedLogMessage);
            }
            else if (currentLock != LoginLock.None)
            {
                result = Reject(LockedLogMessage, LockStatusMapper.ToStatus(currentLock));
            }
            else
            {
                _passkeyRepository.SaveSignCount(record.Id, data.SignCount);
                _logger.Info(string.Format(CultureInfo.InvariantCulture, LoggedInLogFormat, record.PlayerId));
                result = new LoginResult(
                    LoginStatus.LoggedIn,
                    record.PlayerDisplayName,
                    record.PlayerFirstName ?? string.Empty);
            }

            return result;
        }

        private LoginResult Reject(string logMessage)
        {
            return Reject(logMessage, LoginStatus.InvalidPasskey);
        }

        private LoginResult Reject(string logMessage, LoginStatus status)
        {
            _logger.Warning(logMessage);

            return new LoginResult(status, string.Empty);
        }

        private sealed class ParsedAssertion
        {
            public AuthenticatorData Data { get; set; }

            public string UserHandle { get; set; }

            public byte[] Signature { get; set; }

            public byte[] SignedData { get; set; }
        }
    }
}

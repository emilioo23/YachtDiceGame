using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Logging;
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
        private const string DeviceNameFormat = "{0} {1}";
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
                ?? throw new ArgumentNullException(nameof(playerRepository), LogMessages.PasskeyNullDependencyMessage);
            _passkeyRepository = passkeyRepository
                ?? throw new ArgumentNullException(nameof(passkeyRepository), LogMessages.PasskeyNullDependencyMessage);
            _challengeStore = challengeStore
                ?? throw new ArgumentNullException(nameof(challengeStore), LogMessages.PasskeyNullDependencyMessage);
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
                throw new ArgumentNullException(nameof(attestation), LogMessages.NullAttestationMessage);
            }

            Player player = FindPlayer(displayName);
            PasskeyRegistrationStatus status;

            if (player == null)
            {
                status = PasskeyRegistrationStatus.PlayerNotFound;
            }
            else if (!TryTakeRegistrationChallenge(attestation, player, out PendingChallenge pending))
            {
                _logger.Warning(LogMessages.PasskeyExpiredLogMessage);
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
            else if (string.IsNullOrEmpty(password) || !CredentialRules.Verify(password, player.PwdHash))
            {
                _logger.Warning(LogMessages.PasskeyWrongPasswordLogMessage);
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
                    Challenge = PasskeyRules.Encode(pending.Challenge)
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
                throw new ArgumentNullException(nameof(assertion), LogMessages.NullAttestationMessage);
            }

            LoginResult result;

            if (!_challengeStore.TryTake(assertion.ChallengeId, PasskeyPurpose.Login, out PendingChallenge pending))
            {
                result = Reject(LogMessages.PasskeyExpiredLogMessage);
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
                _logger.Info(string.Format(CultureInfo.InvariantCulture, LogMessages.RemovedLogFormat, player.Id));
            }
        }

        private PasskeyRemovalStatus DeletePasskeyOf(Player player, int passkeyId)
        {
            Func<PasskeyRecord, bool> isPasskeyToRemove = key => key.Id == passkeyId;
            PasskeyRecord record = _passkeyRepository
                .FindByPlayer(player.Id)
                .FirstOrDefault(isPasskeyToRemove);
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

                if (pending == null)
                {
                    challenge = new PasskeyChallenge(PasskeyStartStatus.Busy);
                }
                else
                {
                    challenge = DescribeRegistration(player, pending, existing);
                }
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
                Challenge = PasskeyRules.Encode(pending.Challenge),
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
                _logger.Warning(LogMessages.BadRegistrationLogMessage);
                status = PasskeyRegistrationStatus.InvalidResponse;
            }
            else
            {
                PasskeyRecord record = BuildRecord(player, data, key);

                record.DeviceName = BuildDeviceName(attestation.Transport);
                status = Store(player, record);
            }

            return status;
        }

        private static PasskeyRecord BuildRecord(Player player, AuthenticatorData data, PasskeyPublicKey key)
        {
            return new PasskeyRecord
            {
                PlayerId = player.Id,
                CredentialId = PasskeyRules.Encode(data.CredentialId),
                PublicKey = PasskeyRules.Encode(key.ToBytes()),
                SignCount = data.SignCount
            };
        }

        private static string BuildDeviceName(int transport)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                DeviceNameFormat,
                PasskeyRules.GetName(transport),
                DateTime.UtcNow.ToString(DateFormat, CultureInfo.InvariantCulture));
        }

        private PasskeyRegistrationStatus Store(Player player, PasskeyRecord record)
        {
            PasskeyRegistrationStatus status;

            if (_passkeyRepository.FindByPlayer(player.Id).Count >= PasskeyRules.MaxPasskeysPerPlayer)
            {
                status = PasskeyRegistrationStatus.TooManyPasskeys;
            }
            else if (_passkeyRepository.FindByCredentialId(record.CredentialId) != null)
            {
                status = PasskeyRegistrationStatus.AlreadyRegistered;
            }
            else
            {
                _passkeyRepository.Add(record);
                status = PasskeyRegistrationStatus.Registered;
                _logger.Info(string.Format(CultureInfo.InvariantCulture, LogMessages.RegisteredLogFormat, player.Id));
            }

            return status;
        }

        private static AuthenticatorData ReadAttestation(PasskeyAttestation attestation, PendingChallenge pending)
        {
            AuthenticatorData data = null;
            byte[] clientData = null;
            byte[] attestationObject = null;
            AuthenticatorData parsed = null;

            bool isDecoded = PasskeyRules.TryDecode(attestation.ClientDataJson, out clientData)
                && PasskeyRules.TryDecode(attestation.AttestationObject, out attestationObject);
            bool isClientDataValid = isDecoded
                && PasskeyRules.IsValid(clientData, PasskeyRules.CreateClientDataType, pending.Challenge);
            bool isReadable = isClientDataValid && TryReadAuthenticatorData(attestationObject, out parsed);

            if (isReadable && IsAcceptable(parsed) && parsed.CredentialId != null)
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
                result = Reject(LogMessages.UnknownCredentialLogMessage);
            }
            else if (parsed == null || !IsSignatureValid(parsed, record))
            {
                result = Reject(LogMessages.BadAssertionLogMessage);
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

            if (PasskeyRules.TryDecode(assertion.CredentialId, out byte[] credentialId))
            {
                record = _passkeyRepository.FindByCredentialId(PasskeyRules.Encode(credentialId));
            }

            return record;
        }

        private static ParsedAssertion ParseAssertion(PasskeyAssertion assertion, PendingChallenge pending)
        {
            ParsedAssertion parsed = null;
            byte[] clientData = null;
            byte[] authenticatorBytes = null;
            byte[] signature = null;
            AuthenticatorData data = null;

            bool isDecoded = PasskeyRules.TryDecode(assertion.ClientDataJson, out clientData)
                && PasskeyRules.TryDecode(assertion.AuthenticatorData, out authenticatorBytes)
                && PasskeyRules.TryDecode(assertion.Signature, out signature);
            bool isClientDataValid = isDecoded
                && PasskeyRules.IsValid(clientData, PasskeyRules.GetClientDataType, pending.Challenge);
            bool isReadable = isClientDataValid && AuthenticatorData.TryParse(authenticatorBytes, out data);

            if (isReadable && IsAcceptable(data))
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
                && PasskeyRules.TryDecode(record.PublicKey, out byte[] coseKey)
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

            return PasskeyRules.Encode(handle);
        }

        private LoginResult CompleteWith(PasskeyRecord record, AuthenticatorData data)
        {
            LoginLock currentLock = _attemptTracker.GetLock(record.PlayerId, DateTime.UtcNow);
            bool isCounterBack = (record.SignCount > 0 || data.SignCount > 0) && data.SignCount <= record.SignCount;
            LoginResult result;

            if (isCounterBack)
            {
                result = Reject(LogMessages.ClonedLogMessage);
            }
            else if (currentLock != LoginLock.None)
            {
                result = Reject(LogMessages.PasskeyLockedLogMessage, LoginAttemptTracker.ToStatus(currentLock));
            }
            else
            {
                _passkeyRepository.SaveSignCount(record.Id, data.SignCount);
                _logger.Info(string.Format(CultureInfo.InvariantCulture, LogMessages.PasskeyLoggedInLogFormat,
                    record.PlayerId));
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

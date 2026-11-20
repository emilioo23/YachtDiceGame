using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using YachtDice.Client.Logic.Passkeys;
using YachtDice.TwoFactorManagementServiceReference;
using YachtDice.Utils;
using static YachtDice.Passkeys.WebAuthnNative;

namespace YachtDice.Passkeys
{
    /// <summary>
    /// Creates and uses passkeys through the WebAuthn API of Windows. Windows shows its own dialog, where the
    /// player chooses Windows Hello or a phone, which asks for Face ID, a fingerprint or the PIN.
    /// </summary>
    public class WindowsPasskeyAuthenticator
    {
        private const int CancelledByUser = unchecked((int)0x80090036);
        private const int CancelledByWindows = unchecked((int)0x800704C7);
        private const int TimedOut = unchecked((int)0x800705B4);
        private const int EcdsaAlgorithm = PasskeyRules.EcdsaAlgorithm;
        private const int RsaAlgorithm = PasskeyRules.RsaAlgorithm;
        private const string NullChallengeMessage = "The challenge of the server cannot be null.";
        private const string InvalidBase64Message = "The server sent a value that is not valid base64url.";
        private const string CallFailedMessage = "Windows could not complete the passkey operation. HRESULT: 0x{0:X8}.";
        private const string NoAnswerMessage = "Windows did not return the answer of the passkey operation.";
        private const string UnavailableLogMessage = "This Windows does not have the passkey API, so passkeys are off.";

        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the authenticator with its own logger.
        /// </summary>
        public WindowsPasskeyAuthenticator()
        {
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Says whether this Windows has the WebAuthn API.
        /// </summary>
        /// <returns>True if the passkeys can be used.</returns>
        public bool IsAvailable()
        {
            bool isAvailable;

            try
            {
                isAvailable = WebAuthNGetApiVersionNumber() >= StructureVersion;
            }
            catch (DllNotFoundException ex)
            {
                _logger.Warning(ex, UnavailableLogMessage);
                isAvailable = false;
            }
            catch (EntryPointNotFoundException ex)
            {
                _logger.Warning(ex, UnavailableLogMessage);
                isAvailable = false;
            }

            return isAvailable;
        }

        /// <summary>
        /// Asks Windows to create a passkey for the player.
        /// </summary>
        /// <param name="windowHandle">Window that owns the Windows dialog.</param>
        /// <param name="challenge">Challenge and data of the player that the server gave.</param>
        /// <returns>The answer that the server verifies, or null if the player cancelled.</returns>
        /// <exception cref="PasskeyException">Windows failed.</exception>
        public PasskeyAttestationData CreateCredential(IntPtr windowHandle, PasskeyChallengeData challenge)
        {
            if (challenge == null)
            {
                throw new ArgumentNullException(nameof(challenge), NullChallengeMessage);
            }

            byte[] clientDataJson = PasskeyClientData.Create(PasskeyRules.CreateClientDataType, challenge.Challenge);
            IntPtr attestationPointer;
            PasskeyAttestationData attestation = null;
            int result;

            using (var scope = new NativeScope())
            {
                RelyingPartyInfo relyingParty = BuildRelyingParty(scope);
                UserInfo user = BuildUser(scope, challenge);
                CoseParameters parameters = BuildParameters(scope);
                ClientDataInfo clientData = BuildClientData(scope, clientDataJson);
                MakeOptions options = BuildMakeOptions(scope, challenge);

                result = WebAuthNAuthenticatorMakeCredential(
                    windowHandle,
                    ref relyingParty,
                    ref user,
                    ref parameters,
                    ref clientData,
                    ref options,
                    out attestationPointer);
            }

            if (!IsCancelled(result))
            {
                var ceremony = new Ceremony(challenge.ChallengeId, clientDataJson);
                attestation = ReadAttestation(result, attestationPointer, ceremony);
            }

            return attestation;
        }

        /// <summary>
        /// Asks Windows to sign the challenge with a passkey of the player.
        /// </summary>
        /// <param name="windowHandle">Window that owns the Windows dialog.</param>
        /// <param name="challenge">Challenge that the server gave.</param>
        /// <returns>The answer that the server verifies, or null if the player cancelled.</returns>
        /// <exception cref="PasskeyException">Windows failed.</exception>
        public PasskeyAssertionData GetAssertion(IntPtr windowHandle, PasskeyChallengeData challenge)
        {
            if (challenge == null)
            {
                throw new ArgumentNullException(nameof(challenge), NullChallengeMessage);
            }

            byte[] clientDataJson = PasskeyClientData.Create(PasskeyRules.GetClientDataType, challenge.Challenge);
            IntPtr assertionPointer;
            PasskeyAssertionData assertion = null;
            int result;

            using (var scope = new NativeScope())
            {
                ClientDataInfo clientData = BuildClientData(scope, clientDataJson);
                GetOptions options = BuildGetOptions();

                result = WebAuthNAuthenticatorGetAssertion(
                    windowHandle,
                    PasskeyRules.RelyingPartyId,
                    ref clientData,
                    ref options,
                    out assertionPointer);
            }

            if (!IsCancelled(result))
            {
                var ceremony = new Ceremony(challenge.ChallengeId, clientDataJson);
                assertion = ReadAssertion(result, assertionPointer, ceremony);
            }

            return assertion;
        }

        private static RelyingPartyInfo BuildRelyingParty(NativeScope scope)
        {
            return new RelyingPartyInfo
            {
                Version = StructureVersion,
                Id = scope.Text(PasskeyRules.RelyingPartyId),
                Name = scope.Text(PasskeyRules.RelyingPartyName)
            };
        }

        private static UserInfo BuildUser(NativeScope scope, PasskeyChallengeData challenge)
        {
            byte[] handle = DecodeOrThrow(challenge.UserHandle);

            return new UserInfo
            {
                Version = StructureVersion,
                IdSize = (uint)handle.Length,
                Id = scope.Bytes(handle),
                Name = scope.Text(challenge.UserName),
                DisplayName = scope.Text(challenge.UserName)
            };
        }

        private static CoseParameters BuildParameters(NativeScope scope)
        {
            IntPtr type = scope.Text(PublicKeyType);
            var items = new List<CoseParameter>
            {
                new CoseParameter { Version = StructureVersion, CredentialType = type, Algorithm = EcdsaAlgorithm },
                new CoseParameter { Version = StructureVersion, CredentialType = type, Algorithm = RsaAlgorithm }
            };

            return new CoseParameters { Count = (uint)items.Count, Items = scope.Array(items) };
        }

        private static ClientDataInfo BuildClientData(NativeScope scope, byte[] clientDataJson)
        {
            return new ClientDataInfo
            {
                Version = StructureVersion,
                JsonSize = (uint)clientDataJson.Length,
                Json = scope.Bytes(clientDataJson),
                HashAlgorithm = scope.Text(HashAlgorithm)
            };
        }

        private static MakeOptions BuildMakeOptions(NativeScope scope, PasskeyChallengeData challenge)
        {
            var excluded = new List<CredentialInfo>();
            IntPtr type = scope.Text(PublicKeyType);

            foreach (string credentialId in challenge.ExcludedCredentialIds ?? new string[0])
            {
                byte[] id = DecodeOrThrow(credentialId);

                excluded.Add(new CredentialInfo
                {
                    Version = StructureVersion,
                    IdSize = (uint)id.Length,
                    Id = scope.Bytes(id),
                    CredentialType = type
                });
            }

            return new MakeOptions
            {
                Version = StructureVersion,
                TimeoutMilliseconds = PasskeyRules.TimeoutMilliseconds,
                ExcludedCredentials = new CredentialList
                {
                    Count = (uint)excluded.Count,
                    Items = scope.Array(excluded)
                },
                Attachment = AttachmentAny,
                RequireResidentKey = NativeTrue,
                UserVerification = UserVerificationRequired,
                Attestation = AttestationNone
            };
        }

        private static GetOptions BuildGetOptions()
        {
            return new GetOptions
            {
                Version = StructureVersion,
                TimeoutMilliseconds = PasskeyRules.TimeoutMilliseconds,
                Attachment = AttachmentAny,
                UserVerification = UserVerificationRequired
            };
        }

        private static PasskeyAttestationData ReadAttestation(int result, IntPtr pointer, Ceremony ceremony)
        {
            PasskeyAttestationData attestation;

            ThrowIfFailed(result, pointer);

            try
            {
                var native = (AttestationResult)Marshal.PtrToStructure(pointer, typeof(AttestationResult));

                attestation = new PasskeyAttestationData
                {
                    ChallengeId = ceremony.ChallengeId,
                    ClientDataJson = Base64Url.Encode(ceremony.ClientDataJson),
                    AttestationObject = ToText(native.AttestationObject, native.AttestationObjectSize),
                    Transport = ReadTransport(pointer, native.Version)
                };
            }
            finally
            {
                WebAuthNFreeCredentialAttestation(pointer);
            }

            return attestation;
        }

        private static int ReadTransport(IntPtr pointer, uint version)
        {
            int transport = UnknownTransport;

            if (version >= TransportAnswerVersion)
            {
                var detailed = (AttestationResultWithTransport)Marshal.PtrToStructure(
                    pointer,
                    typeof(AttestationResultWithTransport));

                transport = (int)detailed.UsedTransport;
            }

            return transport;
        }

        private static PasskeyAssertionData ReadAssertion(int result, IntPtr pointer, Ceremony ceremony)
        {
            PasskeyAssertionData assertion;

            ThrowIfFailed(result, pointer);

            try
            {
                var native = (AssertionResult)Marshal.PtrToStructure(pointer, typeof(AssertionResult));

                assertion = new PasskeyAssertionData
                {
                    ChallengeId = ceremony.ChallengeId,
                    CredentialId = ToText(native.Credential.Id, native.Credential.IdSize),
                    ClientDataJson = Base64Url.Encode(ceremony.ClientDataJson),
                    AuthenticatorData = ToText(native.AuthenticatorData, native.AuthenticatorDataSize),
                    Signature = ToText(native.Signature, native.SignatureSize),
                    UserHandle = ToText(native.UserId, native.UserIdSize)
                };
            }
            finally
            {
                WebAuthNFreeAssertion(pointer);
            }

            return assertion;
        }

        private static bool IsCancelled(int result)
        {
            return result == CancelledByUser || result == CancelledByWindows || result == TimedOut;
        }

        private static void ThrowIfFailed(int result, IntPtr pointer)
        {
            if (result < 0)
            {
                throw new PasskeyException(string.Format(CallFailedMessage, result));
            }

            if (pointer == IntPtr.Zero)
            {
                throw new PasskeyException(NoAnswerMessage);
            }
        }

        private static string ToText(IntPtr pointer, uint size)
        {
            var data = new byte[size];

            if (size > 0)
            {
                Marshal.Copy(pointer, data, 0, data.Length);
            }

            return Base64Url.Encode(data);
        }

        private static byte[] DecodeOrThrow(string text)
        {
            if (!Base64Url.TryDecode(text, out byte[] data))
            {
                throw new PasskeyException(InvalidBase64Message);
            }

            return data;
        }

        private sealed class Ceremony
        {
            public Ceremony(string challengeId, byte[] clientDataJson)
            {
                ChallengeId = challengeId;
                ClientDataJson = clientDataJson;
            }

            public string ChallengeId { get; }

            public byte[] ClientDataJson { get; }
        }
    }
}

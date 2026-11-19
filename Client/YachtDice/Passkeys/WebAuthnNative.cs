using System;
using System.Runtime.InteropServices;

namespace YachtDice.Passkeys
{
    /// <summary>
    /// Declarations of webauthn.dll, the library of Windows that talks to Windows Hello and to the phones
    /// that a player pairs with the computer. The structures use version 1 of the API, which every Windows 10
    /// and Windows 11 that has the library understands.
    /// </summary>
    internal static class WebAuthnNative
    {
        /// <summary>Version of the structures that the game writes.</summary>
        public const uint StructureVersion = 1;

        /// <summary>First version of the answer of a new passkey that tells which transport was used.</summary>
        public const uint TransportAnswerVersion = 3;

        /// <summary>Transport that is reported when Windows does not say which authenticator answered.</summary>
        public const int UnknownTransport = 0;

        /// <summary>Type of the credentials, as Windows names it.</summary>
        public const string PublicKeyType = "public-key";

        /// <summary>Hash algorithm that Windows applies to the client data.</summary>
        public const string HashAlgorithm = "SHA-256";

        /// <summary>The authenticator must verify the player, with a face, a fingerprint or a PIN.</summary>
        public const uint UserVerificationRequired = 1;

        /// <summary>The server does not need a proof of the model of the authenticator.</summary>
        public const uint AttestationNone = 1;

        /// <summary>Any authenticator: the computer itself, a security key or a paired phone.</summary>
        public const uint AttachmentAny = 0;

        /// <summary>Value of true in the BOOL fields of Windows.</summary>
        public const int NativeTrue = 1;

        private const string Library = "webauthn.dll";

        /// <summary>
        /// Gets the version of the API. The call fails with DllNotFoundException in a Windows without the library.
        /// </summary>
        /// <returns>The version number.</returns>
        [DllImport(Library)]
        public static extern uint WebAuthNGetApiVersionNumber();

        /// <summary>
        /// Asks the authenticator to create a passkey. The signature is the one that Windows defines, so it has
        /// more than three parameters.
        /// </summary>
        /// <param name="windowHandle">Window that owns the dialog.</param>
        /// <param name="relyingParty">The game.</param>
        /// <param name="user">The player.</param>
        /// <param name="parameters">Algorithms that the server accepts.</param>
        /// <param name="clientData">Data that the authenticator signs.</param>
        /// <param name="options">Options of the ceremony.</param>
        /// <param name="attestation">Pointer to the answer.</param>
        /// <returns>An HRESULT; a negative value is a failure.</returns>
        [DllImport(Library)]
        public static extern int WebAuthNAuthenticatorMakeCredential(
            IntPtr windowHandle,
            ref RelyingPartyInfo relyingParty,
            ref UserInfo user,
            ref CoseParameters parameters,
            ref ClientDataInfo clientData,
            ref MakeOptions options,
            out IntPtr attestation);

        /// <summary>
        /// Asks the authenticator to sign the challenge with a passkey. The signature is the one that Windows
        /// defines, so it has more than three parameters.
        /// </summary>
        /// <param name="windowHandle">Window that owns the dialog.</param>
        /// <param name="relyingPartyId">Identifier of the game.</param>
        /// <param name="clientData">Data that the authenticator signs.</param>
        /// <param name="options">Options of the ceremony.</param>
        /// <param name="assertion">Pointer to the answer.</param>
        /// <returns>An HRESULT; a negative value is a failure.</returns>
        [DllImport(Library, CharSet = CharSet.Unicode)]
        public static extern int WebAuthNAuthenticatorGetAssertion(
            IntPtr windowHandle,
            string relyingPartyId,
            ref ClientDataInfo clientData,
            ref GetOptions options,
            out IntPtr assertion);

        /// <summary>
        /// Frees the answer of a creation.
        /// </summary>
        /// <param name="attestation">Pointer that Windows gave.</param>
        [DllImport(Library)]
        public static extern void WebAuthNFreeCredentialAttestation(IntPtr attestation);

        /// <summary>
        /// Frees the answer of a login.
        /// </summary>
        /// <param name="assertion">Pointer that Windows gave.</param>
        [DllImport(Library)]
        public static extern void WebAuthNFreeAssertion(IntPtr assertion);

        /// <summary>WEBAUTHN_RP_ENTITY_INFORMATION.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RelyingPartyInfo
        {
            public uint Version;
            public IntPtr Id;
            public IntPtr Name;
            public IntPtr Icon;
        }

        /// <summary>WEBAUTHN_USER_ENTITY_INFORMATION.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct UserInfo
        {
            public uint Version;
            public uint IdSize;
            public IntPtr Id;
            public IntPtr Name;
            public IntPtr Icon;
            public IntPtr DisplayName;
        }

        /// <summary>WEBAUTHN_COSE_CREDENTIAL_PARAMETER.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CoseParameter
        {
            public uint Version;
            public IntPtr CredentialType;
            public int Algorithm;
        }

        /// <summary>WEBAUTHN_COSE_CREDENTIAL_PARAMETERS.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CoseParameters
        {
            public uint Count;
            public IntPtr Items;
        }

        /// <summary>WEBAUTHN_CLIENT_DATA.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ClientDataInfo
        {
            public uint Version;
            public uint JsonSize;
            public IntPtr Json;
            public IntPtr HashAlgorithm;
        }

        /// <summary>WEBAUTHN_CREDENTIAL.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CredentialInfo
        {
            public uint Version;
            public uint IdSize;
            public IntPtr Id;
            public IntPtr CredentialType;
        }

        /// <summary>WEBAUTHN_CREDENTIALS.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CredentialList
        {
            public uint Count;
            public IntPtr Items;
        }

        /// <summary>WEBAUTHN_EXTENSIONS.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ExtensionList
        {
            public uint Count;
            public IntPtr Items;
        }

        /// <summary>WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS, version 1.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MakeOptions
        {
            public uint Version;
            public uint TimeoutMilliseconds;
            public CredentialList ExcludedCredentials;
            public ExtensionList Extensions;
            public uint Attachment;
            public int RequireResidentKey;
            public uint UserVerification;
            public uint Attestation;
            public uint Flags;
        }

        /// <summary>WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS, version 1.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct GetOptions
        {
            public uint Version;
            public uint TimeoutMilliseconds;
            public CredentialList AllowedCredentials;
            public ExtensionList Extensions;
            public uint Attachment;
            public uint UserVerification;
            public uint Flags;
        }

        /// <summary>WEBAUTHN_CREDENTIAL_ATTESTATION, version 1.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct AttestationResult
        {
            public uint Version;
            public IntPtr FormatType;
            public uint AuthenticatorDataSize;
            public IntPtr AuthenticatorData;
            public uint AttestationSize;
            public IntPtr Attestation;
            public uint AttestationDecodeType;
            public IntPtr AttestationDecode;
            public uint AttestationObjectSize;
            public IntPtr AttestationObject;
            public uint CredentialIdSize;
            public IntPtr CredentialId;
        }

        /// <summary>
        /// WEBAUTHN_CREDENTIAL_ATTESTATION, version 3: the fields of version 1, the extensions and the transport that
        /// the authenticator used. It is only read when the answer says that it has this version.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct AttestationResultWithTransport
        {
            public uint Version;
            public IntPtr FormatType;
            public uint AuthenticatorDataSize;
            public IntPtr AuthenticatorData;
            public uint AttestationSize;
            public IntPtr Attestation;
            public uint AttestationDecodeType;
            public IntPtr AttestationDecode;
            public uint AttestationObjectSize;
            public IntPtr AttestationObject;
            public uint CredentialIdSize;
            public IntPtr CredentialId;
            public ExtensionList Extensions;
            public uint UsedTransport;
        }

        /// <summary>WEBAUTHN_ASSERTION, version 1.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct AssertionResult
        {
            public uint Version;
            public uint AuthenticatorDataSize;
            public IntPtr AuthenticatorData;
            public uint SignatureSize;
            public IntPtr Signature;
            public CredentialInfo Credential;
            public uint UserIdSize;
            public IntPtr UserId;
        }
    }
}

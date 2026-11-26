using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.TwoFactorManagementServiceReference;
using TwoFactorFault = YachtDice.TwoFactorManagementServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the two-factor management operations of the server.
    /// </summary>
    public class TwoFactorManagementGateway : GatewayBase<TwoFactorManagementServiceClient, ITwoFactorManagementService>
    {
        /// <summary>
        /// Asks the server whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        public async Task<bool> IsTwoFactorEnabledAsync(string sessionToken)
        {
            bool isEnabled = await ExecuteAsync(client => client.IsTwoFactorEnabled(sessionToken));

            return isEnabled;
        }

        /// <summary>
        /// Asks the server to send the code that confirms the activation of the two-step verification.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        public async Task<TwoFactorActivationData> StartTwoFactorActivationAsync(string sessionToken, string culture)
        {
            TwoFactorActivationData activation =
                await ExecuteAsync(client => client.StartTwoFactorActivation(sessionToken, culture));

            return activation;
        }

        /// <summary>
        /// Sends the code entered by the player to turn on the two-step verification.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="verificationCode">Code entered by the player and the challenge of the activation.</param>
        /// <returns>Result of the validation.</returns>
        public async Task<VerificationResultCode> ConfirmActivationAsync(
            string sessionToken,
            VerificationCode verificationCode)
        {
            VerificationResultCode result =
                await ExecuteAsync(client => client.ConfirmTwoFactorActivation(sessionToken, verificationCode));

            return result;
        }

        /// <summary>
        /// Asks the server to send a new code while the player turns on the two-step verification.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="challengeId">Challenge whose code is sent again.</param>
        /// <param name="culture">Culture preferred by the player for the email.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public async Task<ResendResultCode> ResendActivationCodeAsync(
            string sessionToken,
            string challengeId,
            string culture)
        {
            var delivery = new CodeDelivery { ChallengeId = challengeId, Culture = culture };
            ResendResultCode result = await ExecuteAsync(client => client.ResendActivationCode(sessionToken, delivery));

            return result;
        }

        /// <summary>
        /// Asks the server to turn off the two-step verification of the player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public async Task<TwoFactorChangeResultCode> DisableTwoFactorAsync(string sessionToken, string password)
        {
            TwoFactorChangeResultCode result =
                await ExecuteAsync(client => client.DisableTwoFactor(sessionToken, password));

            return result;
        }

        /// <summary>
        /// Asks the server to change the password of the player who is logged in.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="change">Current password and new password.</param>
        /// <returns>Outcome of the request.</returns>
        public async Task<PasswordChangeResultCode> ChangePasswordAsync(string sessionToken, PasswordChange change)
        {
            PasswordChangeResultCode result =
                await ExecuteAsync(client => client.ChangePassword(sessionToken, change));

            return result;
        }

        /// <summary>
        /// Asks the server to create new backup codes for the player. The codes created before stop working.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request and the new codes.</returns>
        public async Task<BackupCodesData> CreateBackupCodesAsync(string sessionToken, string password)
        {
            BackupCodesData codesData = await ExecuteAsync(client => client.CreateBackupCodes(sessionToken, password));

            return codesData;
        }

        /// <summary>
        /// Asks the server how many backup codes the player has not used.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The number of backup codes left.</returns>
        public async Task<int> CountBackupCodesAsync(string sessionToken)
        {
            int codeCount = await ExecuteAsync(client => client.CountBackupCodes(sessionToken));

            return codeCount;
        }

        /// <summary>
        /// Asks the server for the last change of password and the last lock of the account.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The summary of the security of the account.</returns>
        public async Task<SecuritySummary> GetSecuritySummaryAsync(string sessionToken)
        {
            SecuritySummary summary = await ExecuteAsync(client => client.GetSecuritySummary(sessionToken));

            return summary;
        }

        /// <summary>
        /// Asks the server for the challenge to create a passkey for the player who is logged in.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The challenge and the data of the player for the authenticator.</returns>
        public async Task<PasskeyChallengeData> BeginPasskeyRegistrationAsync(string sessionToken)
        {
            PasskeyChallengeData challenge = await ExecuteAsync(
                client => client.BeginPasskeyRegistration(sessionToken));

            return challenge;
        }

        /// <summary>
        /// Sends the new passkey that the authenticator created.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="attestation">Answer of the authenticator.</param>
        /// <returns>Outcome of the registration.</returns>
        public async Task<PasskeyRegistrationResultCode> CompletePasskeyRegistrationAsync(
            string sessionToken,
            PasskeyAttestationData attestation)
        {
            PasskeyRegistrationResultCode result =
                await ExecuteAsync(client => client.CompletePasskeyRegistration(sessionToken, attestation));

            return result;
        }

        /// <summary>
        /// Asks the server how many passkeys the player has.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The number of passkeys.</returns>
        public async Task<int> CountPasskeysAsync(string sessionToken)
        {
            int passkeyCount = await ExecuteAsync(client => client.CountPasskeys(sessionToken));

            return passkeyCount;
        }

        /// <summary>
        /// Asks the server for the passkeys that the player has.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The passkeys of the player.</returns>
        public async Task<PasskeyInfoData[]> ListPasskeysAsync(string sessionToken)
        {
            PasskeyInfoData[] passkeys = await ExecuteAsync(client => client.ListPasskeys(sessionToken));

            return passkeys;
        }

        /// <summary>
        /// Asks the server to remove a passkey of the player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="passkeyId">Identifier of the passkey to remove.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the removal.</returns>
        public async Task<PasskeyRemovalResultCode> RemovePasskeyAsync(
            string sessionToken,
            int passkeyId,
            string password)
        {
            PasskeyRemovalResultCode result =
                await ExecuteAsync(client => client.RemovePasskey(sessionToken, passkeyId, password));

            return result;
        }

        /// <summary>
        /// Asks the server for the challenge to log in with a passkey.
        /// </summary>
        /// <returns>The challenge.</returns>
        public async Task<PasskeyChallengeData> BeginPasskeyLoginAsync()
        {
            PasskeyChallengeData challenge = await ExecuteAsync(client => client.BeginPasskeyLogin());

            return challenge;
        }

        /// <summary>
        /// Sends the answer of the authenticator to log in.
        /// </summary>
        /// <param name="assertion">Answer of the authenticator.</param>
        /// <returns>Outcome of the login and the data of the new session.</returns>
        public async Task<LoginData> CompletePasskeyLoginAsync(PasskeyAssertionData assertion)
        {
            LoginData loginData = await ExecuteAsync(client => client.CompletePasskeyLogin(assertion));

            return loginData;
        }

        /// <summary>
        /// Translates the WCF fault exception into a specific gateway error code.
        /// </summary>
        /// <param name="fault">The exception thrown by the service.</param>
        /// <returns>The translated gateway error code.</returns>
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<TwoFactorFault>;

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }
    }
}
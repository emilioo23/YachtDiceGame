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
        /// <param name="displayName">Name of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        public async Task<bool> IsTwoFactorEnabledAsync(string displayName)
        {
            bool isEnabled = await ExecuteAsync(client => client.IsTwoFactorEnabled(displayName));

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
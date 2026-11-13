using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.TwoFactorManagementServiceReference;
using ResendInput = YachtDice.Client.Logic.Authentication.ResendCodeInput;
using TwoFactorFault = YachtDice.TwoFactorManagementServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the two-factor management operations of the server.
    /// </summary>
    public class TwoFactorManagementGateway : GatewayBase<TwoFactorManagementServiceClient, ITwoFactorManagementService>
    {
        private const string NullResendInputMessage = "The data to resend the code cannot be null.";

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
        /// <param name="displayName">Name of the player.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        public async Task<TwoFactorActivationData> StartTwoFactorActivationAsync(string displayName, string culture)
        {
            TwoFactorActivationData activation =
                await ExecuteAsync(client => client.StartTwoFactorActivation(displayName, culture));

            return activation;
        }

        /// <summary>
        /// Sends the code entered by the player to turn on the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="verificationCode">Code entered by the player and the challenge of the activation.</param>
        /// <returns>Result of the validation.</returns>
        public async Task<VerificationResultCode> ConfirmActivationAsync(
            string displayName,
            VerificationCode verificationCode)
        {
            VerificationResultCode result =
                await ExecuteAsync(client => client.ConfirmTwoFactorActivation(displayName, verificationCode));

            return result;
        }

        /// <summary>
        /// Asks the server to send a new code while the player turns on the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="input">Challenge of the activation and culture preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        /// <exception cref="ArgumentNullException">The input is null.</exception>
        public async Task<ResendResultCode> ResendActivationCodeAsync(string displayName, ResendInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), NullResendInputMessage);
            }

            var delivery = new CodeDelivery { ChallengeId = input.ChallengeId, Culture = input.Culture };
            ResendResultCode result = await ExecuteAsync(client => client.ResendActivationCode(displayName, delivery));

            return result;
        }

        /// <summary>
        /// Asks the server to turn off the two-step verification of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public async Task<TwoFactorChangeResultCode> DisableTwoFactorAsync(string displayName, string password)
        {
            TwoFactorChangeResultCode result =
                await ExecuteAsync(client => client.DisableTwoFactor(displayName, password));

            return result;
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
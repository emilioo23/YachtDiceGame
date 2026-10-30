using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.TwoFactorManagementServiceReference;
using AuthErrorCode = YachtDice.TwoFactorManagementServiceReference.ServiceErrorCode;
using AuthFault = YachtDice.TwoFactorManagementServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the two-factor management operations of the server.
    /// </summary>
    public class TwoFactorManagementGateway : GatewayBase<TwoFactorManagementServiceClient>
    {
        private static readonly Dictionary<AuthErrorCode, GatewayErrorCode> _errorCodes =
            new Dictionary<AuthErrorCode, GatewayErrorCode>
            {
                { AuthErrorCode.UnexpectedError, GatewayErrorCode.UnexpectedError },
                { AuthErrorCode.DatabaseUnavailable, GatewayErrorCode.DatabaseUnavailable },
                { AuthErrorCode.ValidationFailed, GatewayErrorCode.ValidationFailed }
            };

        /// <summary>
        /// Asks the server whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        public Task<bool> IsTwoFactorEnabledAsync(string displayName)
        {
            return ExecuteAsync(client => client.IsTwoFactorEnabled(displayName));
        }

        /// <summary>
        /// Asks the server to send the code that confirms the activation of the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        public Task<TwoFactorActivationData> StartTwoFactorActivationAsync(string displayName, string culture)
        {
            return ExecuteAsync(client => client.StartTwoFactorActivation(displayName, culture));
        }

        /// <summary>
        /// Sends the code entered by the player to turn on the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Unique challenge identifier of the activation.</param>
        /// <returns>Result of the validation.</returns>
        public Task<VerificationResultCode> ConfirmActivationAsync(string displayName, string code, string challengeId)
        {
            return ExecuteAsync(client => client.ConfirmTwoFactorActivation(displayName, code, challengeId));
        }

        /// <summary>
        /// Asks the server to send a new code while the player turns on the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="challengeId">Unique challenge identifier of the activation.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public Task<ResendResultCode> ResendActivationCodeAsync(string displayName, string challengeId, string culture)
        {
            return ExecuteAsync(client => client.ResendActivationCode(displayName, challengeId, culture));
        }

        /// <summary>
        /// Asks the server to turn off the two-step verification of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public Task<TwoFactorChangeResultCode> DisableTwoFactorAsync(string displayName, string password)
        {
            return ExecuteAsync(client => client.DisableTwoFactor(displayName, password));
        }

        /// <summary>
        /// Translates the WCF fault exception into a specific gateway error code.
        /// </summary>
        /// <param name="fault">The exception thrown by the service.</param>
        /// <returns>The translated gateway error code.</returns>
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<AuthFault>;
            GatewayErrorCode errorCode;

            if (typedFault == null || !_errorCodes.TryGetValue(typedFault.Detail.Code, out errorCode))
            {
                errorCode = GatewayErrorCode.UnexpectedError;
            }

            return errorCode;
        }
    }
}
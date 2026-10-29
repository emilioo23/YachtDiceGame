using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.AuthenticationServiceReference;
using AuthErrorCode = YachtDice.AuthenticationServiceReference.ServiceErrorCode;
using AuthFault = YachtDice.AuthenticationServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the authentication operations of the server.
    /// </summary>
    public class AuthenticationGateway : GatewayBase<AuthenticationServiceClient>
    {
        private static readonly Dictionary<AuthErrorCode, GatewayErrorCode> _errorCodes =
            new Dictionary<AuthErrorCode, GatewayErrorCode>
            {
                { AuthErrorCode.UnexpectedError, GatewayErrorCode.UnexpectedError },
                { AuthErrorCode.DatabaseUnavailable, GatewayErrorCode.DatabaseUnavailable },
                { AuthErrorCode.ValidationFailed, GatewayErrorCode.ValidationFailed }
            };

        /// <summary>
        /// Checks the credentials and asks the server to send the verification code.
        /// </summary>
        /// <param name="credentials">Login credentials of the player.</param>
        /// <returns>Outcome and initial session data of the login attempt.</returns>
        public Task<LoginData> LoginAsync(LoginCredentials credentials)
        {
            return ExecuteAsync(client => client.LoginPlayer(credentials));
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="email">Email address the code was sent to.</param>
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <returns>Result of the validation.</returns>
        public Task<VerificationResultCode> VerifyCodeAsync(string email, string code, string challengeId)
        {
            return ExecuteAsync(client => client.VerifyPlayerCode(email, code, challengeId));
        }

        /// <summary>
        /// Asks the server to send a new verification code.
        /// </summary>
        /// <param name="email">Email address to send the new code to.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public Task<ResendResultCode> ResendCodeAsync(string email, string challengeId, string culture)
        {
            return ExecuteAsync(client => client.ResendPlayerCode(email, challengeId, culture));
        }

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

        /// <inheritdoc />
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
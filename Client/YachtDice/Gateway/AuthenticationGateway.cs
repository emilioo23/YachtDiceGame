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
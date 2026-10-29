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
        public Task<LoginData> LoginAsync(LoginCredentials request)
        {
            return ExecuteAsync(client => client.Login(request));
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        public Task<VerificationResultCode> VerifyCodeAsync(string email, string code, string challengeId)
        {
            return ExecuteAsync(client => client.VerifyCode(email, code, challengeId));
        }

        /// <summary>
        /// Asks the server to send a new verification code.
        /// </summary>
        public Task<ResendResultCode> ResendCodeAsync(string email, string challengeId, string culture)
        {
            return ExecuteAsync(client => client.ResendCode(email, challengeId, culture));
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
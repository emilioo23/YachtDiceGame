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
                { AuthErrorCode.InvalidRequest, GatewayErrorCode.InvalidRequest }
            };

        /// <summary>
        /// Checks the credentials and asks the server to send the verification code.
        /// </summary>
        /// <param name="request">Credentials and language of the player.</param>
        /// <returns>Outcome of the login attempt.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            return ExecuteAsync(client => client.AuthenticatePlayerAsync(request));
        }

        /// <summary>
        /// Registers a new player.
        /// </summary>
        /// <param name="request">Data of the new player.</param>
        /// <returns>Outcome of the registration.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public Task<RegisterResultCode> RegisterAsync(RegisterRequest request)
        {
            return ExecuteAsync(client => client.RegisterAsync(request));
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        ///<param name="request">Email and code entered by the player.</param>
        /// <returns>Result of the validation.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public Task<VerificationResultCode> VerifyCodeAsync(VerifyCodeRequest request)
        {
            return ExecuteAsync(client => client.VerifyCodeAsync(request));
        }

        /// <summary>
        /// Asks the server to send a new verification code.
        /// </summary>
        /// <param name="request">Email and language of the player.</param>
        /// <returns>Outcome of the request.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public Task<ResendResultCode> ResendCodeAsync(ResendCodeRequest request)
        {
            return ExecuteAsync(client => client.ResendCodeAsync(request));
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
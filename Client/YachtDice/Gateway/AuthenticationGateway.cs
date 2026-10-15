using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.AuthenticationServiceReference;
using YachtDice.Utils;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the authentication operations of the server.
    /// </summary>
    public class AuthenticationGateway
    {
        private const string FaultLogMessage = "The server reported an error.";
        private const string UnavailableLogMessage = "The server could not be reached.";
        private const string FaultMessage = "The server could not complete the request.";
        private const string UnavailableMessage = "The server is not available.";

        private static readonly Dictionary<ServiceErrorCode, GatewayErrorCode> _errorCodes =
            new Dictionary<ServiceErrorCode, GatewayErrorCode>
            {
                { ServiceErrorCode.UnexpectedError, GatewayErrorCode.UnexpectedError },
                { ServiceErrorCode.DatabaseUnavailable, GatewayErrorCode.DatabaseUnavailable },
                { ServiceErrorCode.InvalidRequest, GatewayErrorCode.InvalidRequest }
            };

        private readonly AppLogger _logger = new AppLogger(typeof(AuthenticationGateway));

        /// <summary>
        /// Checks the credentials and asks the server to send the verification code.
        /// </summary>
        /// <param name="request">Credentials and language of the player.</param>
        /// <returns>Outcome of the login attempt.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            return ExecuteAsync(client => client.LoginAsync(request));
        }

        /// <summary>
        /// Registers a new player.
        /// </summary>
        /// <param name="request">Data of the new player.</param>
        /// <returns>Outcome of the registration.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public Task<RegisterResultCode> RegisterAsync(RegisterRequestDto request)
        {
            return ExecuteAsync(client => client.RegisterAsync(request));
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="request">Email and code entered by the player.</param>
        /// <returns>Result of the validation.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public Task<VerificationResultCode> VerifyCodeAsync(VerifyCodeRequestDto request)
        {
            return ExecuteAsync(client => client.VerifyCodeAsync(request));
        }

        /// <summary>
        /// Asks the server to send a new verification code.
        /// </summary>
        /// <param name="request">Email and language of the player.</param>
        /// <returns>Outcome of the request.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public Task<ResendResultCode> ResendCodeAsync(ResendCodeRequestDto request)
        {
            return ExecuteAsync(client => client.ResendCodeAsync(request));
        }

        private async Task<TResponse> ExecuteAsync<TResponse>(
            Func<AuthenticationServiceClient, Task<TResponse>> operation)
        {
            var client = new AuthenticationServiceClient();
            TResponse response;

            try
            {
                response = await operation(client);
                client.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                _logger.Error(ex, FaultLogMessage);
                throw new GatewayException(_errorCodes[ex.Detail.Code], FaultMessage, ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(ex, UnavailableLogMessage);
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, UnavailableMessage, ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(ex, UnavailableLogMessage);
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, UnavailableMessage, ex);
            }
            finally
            {
                if (client.State != CommunicationState.Closed)
                {
                    client.Abort();
                }
            }

            return response;
        }
    }
}
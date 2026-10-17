using System;
using System.ServiceModel;
using YachtDice.Server.Logic.Authentication;

namespace YachtDice.Server.Services
{
    /// <summary>
    /// Manages the authentication operations for the players.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILoginService _loginService;
        private readonly IVerificationService _verificationService;

        /// <summary>
        /// Initializes a new instance of the AuthenticationService class.
        /// </summary>
        /// <param name="loginService">The service responsible for login logic.</param>
        /// <param name="verificationService">The service responsible for two-factor verification.</param>
        public AuthenticationService(ILoginService loginService, IVerificationService verificationService)
        {
            _loginService = loginService;
            _verificationService = verificationService;
        }

        /// <summary>
        /// Checks the credentials and, if they are correct, sends the verification code by email.
        /// </summary>
        /// <param name="request">Credentials and language of the player.</param>
        /// <returns>Outcome of the login attempt.</returns>
        public Contracts.LoginData Login(Contracts.LoginCredentials request)
        {
            return Execute(request, () => ProcessLogin(request));
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="request">Email and code entered by the player.</param>
        /// <returns>Result of the validation.</returns>
        public Contracts.VerificationResultCode VerifyCode(Contracts.VerificationDetails request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                var fault = new Contracts.ServiceFault
                {
                    Code = Contracts.ServiceErrorCode.UnexpectedError,
                    Message = "Invalid verification details."
                };

                throw new FaultException<Contracts.ServiceFault>(fault, new FaultReason("Invalid verification details."));
            }

            return Execute(request, () => ProcessVerifyCode(request));
        }

        /// <summary>
        /// Resends a verification code to the player's email.
        /// </summary>
        /// <param name="request">Details required to resend the code.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public Contracts.ResendResultCode ResendCode(Contracts.ResendCodeDetails request)
        {
            return Execute(request, () => ProcessResendCode(request));
        }

        /// <summary>
        /// Processes the login logic synchronously by mapping contracts to domain models.
        /// </summary>
        /// <param name="request">The login credentials from the client.</param>
        /// <returns>The mapped login data result.</returns>
        private Contracts.LoginData ProcessLogin(Contracts.LoginCredentials request)
        {
            var logicCredentials = new Logic.Authentication.LoginCredentials
            {
                Email = request.Email,
                Password = request.Password,
                Culture = Mapping.CultureParser.Parse(request.CultureName)
            };

            Logic.Authentication.LoginResult logicResult = _loginService.Login(logicCredentials);

            return new Contracts.LoginData
            {
                ChallengeId = logicResult.ChallengeId.ToString(),
                DisplayName = logicResult.DisplayName,
                Code = (Contracts.LoginResultCode)(int)logicResult.Status
            };
        }

        /// <summary>
        /// Processes the verification code logic synchronously.
        /// </summary>
        /// <param name="request">The verification details.</param>
        /// <returns>The verification result code.</returns>
        private Contracts.VerificationResultCode ProcessVerifyCode(Contracts.VerificationDetails request)
        {
            var logicDetails = new Logic.Authentication.VerificationDetails
            {
                ChallengeId = request.ChallengeId,
                Code = request.Code,
                Email = request.Email
            };

            Logic.Authentication.VerificationResultCode logicResult = _verificationService.VerifyCode(logicDetails);

            return (Contracts.VerificationResultCode)(int)logicResult;
        }

        /// <summary>
        /// Processes the resend code logic synchronously.
        /// </summary>
        /// <param name="request">The resend code details.</param>
        /// <returns>The resend result code.</returns>
        private Contracts.ResendResultCode ProcessResendCode(Contracts.ResendCodeDetails request)
        {
            var logicDetails = new Logic.Authentication.ResendCodeDetails
            {
                ChallengeId = request.ChallengeId,
                Culture = Mapping.CultureParser.Parse(request.CultureName),
                Email = request.Email
            };

            Logic.Authentication.ResendResultCode logicResult = _verificationService.ResendCode(logicDetails);

            return (Contracts.ResendResultCode)(int)logicResult;
        }

        /// <summary>
        /// Executes a service operation safely by catching exceptions and wrapping them in a FaultException.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="request">The request payload.</param>
        /// <param name="logic">The business logic to execute.</param>
        /// <returns>The result of the executed logic.</returns>
        private TResult Execute<TRequest, TResult>(TRequest request, Func<TResult> logic)
        {
            try
            {
                return logic();
            }
            catch (Exception ex)
            {
                var fault = new Contracts.ServiceFault
                {
                    Code = Contracts.ServiceErrorCode.UnexpectedError,
                    Message = ex.Message
                };

                throw new FaultException<Contracts.ServiceFault>(fault, new FaultReason(ex.Message));
            }
        }
    }
}
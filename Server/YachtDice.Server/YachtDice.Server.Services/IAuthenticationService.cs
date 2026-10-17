using System.ServiceModel;
using YachtDice.Server.Services.Contracts;

namespace YachtDice.Server.Services
{
    /// <summary>
    /// Defines the service contract for player authentication.
    /// </summary>
    [ServiceContract]
    public interface IAuthenticationService
    {
        /// <summary>
        /// Checks the credentials and, if they are correct, sends the verification code.
        /// </summary>
        /// <param name="request">Credentials and language of the player.</param>
        /// <returns>Outcome of the login attempt.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        LoginData Login(LoginCredentials request);

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="request">Email and code entered by the player.</param>
        /// <returns>Result of the validation.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        VerificationResultCode VerifyCode(VerificationDetails request);

        /// <summary>
        /// Resends a verification code to the player's email.
        /// </summary>
        /// <param name="request">Details required to resend the code.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResendResultCode ResendCode(ResendCodeDetails request);
    }
}
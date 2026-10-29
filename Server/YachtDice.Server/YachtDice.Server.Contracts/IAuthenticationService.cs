using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Defines the service contract for player authentication operations.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IAuthenticationService
    {
        /// <summary>
        /// Checks the credentials and, if correct, sends a verification code.
        /// </summary>
        /// <param name="credentials">Login credentials of the player.</param>
        /// <returns>Outcome and initial session data of the login attempt.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        LoginData Login(LoginCredentials credentials);

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="email">Email address the code was sent to.</param>
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <returns>Result of the validation.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        VerificationResultCode VerifyCode(string email, string code, string challengeId);

        /// <summary>
        /// Resends a verification code to the player's email address.
        /// </summary>
        /// <param name="email">Email address to send the new code to.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResendResultCode ResendCode(string email, string challengeId, string culture);
    }
}
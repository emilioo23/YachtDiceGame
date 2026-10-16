using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Server.Services.Contracts;

namespace YachtDice.Server.Services
{
    /// <summary>
    /// Operations that the clients can use to authenticate.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IAuthenticationService
    {
        /// <summary>
        /// Checks the credentials and, if they are correct, sends the verification code by email.
        /// </summary>
        /// <param name="request">Credentials and language of the player.</param>
        /// <returns>Outcome of the login attempt.</returns>
        [OperationContract(Name = "AuthenticatePlayer")]
        [FaultContract(typeof(ServiceFault))]
        Task<LoginData> LoginAsync(LoginCredentials request);

        /// <summary>
        /// Validates the data and registers a new player.
        /// </summary>
        /// <param name="request">Data of the new player.</param>
        /// <returns>Outcome of the registration.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<RegisterResultCode> RegisterAsync(PlayerRegistrationInfo request);

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="request">Email and code entered by the player.</param>
        /// <returns>Result of the validation.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<VerificationResultCode> VerifyCodeAsync(VerificationDetails request);

        /// <summary>
        /// Generates a new verification code and sends it by email.
        /// </summary>
        /// <param name="request">Email and language of the player.</param>
        /// <returns>Outcome of the request.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<ResendResultCode> ResendCodeAsync(ResendCodeDetails request);
    }
}
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
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    }
}
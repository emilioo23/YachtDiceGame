using System.ServiceModel;

namespace YachtDice.Server.Services.Contracts
{
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IAuthenticationManager
    {
        [OperationContract]
        LoginResponse Login(LoginCredentials credentials);

        [OperationContract]
        VerificationResultCode VerifyCode(VerificationDetails details);

        [OperationContract]
        ResendResultCode ResendCode(ResendCodeDetails details);
    }
}
namespace YachtDice.Server.Logic.Authentication
{
    public interface IVerificationService
    {
        VerificationResultCode VerifyCode(VerificationDetails details);

        ResendResultCode ResendCode(ResendCodeDetails details);
    }
}
using System.Globalization;

namespace YachtDice.Server.Logic.Authentication
{
    public class VerificationDetails
    {
        public string ChallengeId { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
    }

    public enum VerificationResultCode
    {
        Success,
        InvalidCode,
        Expired,
        MaxAttemptsReached,
        ServerError
    }

    public class ResendCodeDetails
    {
        public string ChallengeId { get; set; }
        public CultureInfo Culture { get; set; }
        public string Email { get; set; }
    }

    public enum ResendResultCode
    {
        Success,
        TooManyRequests,
        ServerError
    }
}
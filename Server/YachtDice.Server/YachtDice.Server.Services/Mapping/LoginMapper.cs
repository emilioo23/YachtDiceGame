using System.Collections.Generic;

namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts;
    using LogicAuth = YachtDice.Server.Logic.Authentication;

    /// <summary>
    /// Converts between login data contracts and logic layer entities.
    /// </summary>
    internal static class LoginMapper
    {
        private static readonly Dictionary<LogicAuth.LoginStatus, Contracts.LoginResultCode> _resultCodes =
            new Dictionary<LogicAuth.LoginStatus, Contracts.LoginResultCode>
            {
                { LogicAuth.LoginStatus.VerificationCodeSent, Contracts.LoginResultCode.VerificationCodeSent },
                { LogicAuth.LoginStatus.MissingRequiredField, Contracts.LoginResultCode.MissingRequiredField },
                { LogicAuth.LoginStatus.InvalidCredentials, Contracts.LoginResultCode.InvalidCredentials },
                { LogicAuth.LoginStatus.VerificationLocked, Contracts.LoginResultCode.VerificationLocked },
                { LogicAuth.LoginStatus.CodeSendFailed, Contracts.LoginResultCode.CodeSendFailed }
            };

        /// <summary>
        /// Converts client credentials into logic-layer credentials.
        /// </summary>
        /// <param name="credentials">Login credentials provided by the client.</param>
        /// <returns>Credentials model for the logic layer.</returns>
        public static LogicAuth.LoginCredentials ToLogicCredentials(Contracts.LoginCredentials credentials)
        {
            return new LogicAuth.LoginCredentials
            {
                Email = credentials.Email,
                Password = credentials.Password,
                Culture = CultureParser.Parse(credentials.CultureName)
            };
        }

        /// <summary>
        /// Converts the logic-layer login result into a contract data model for the client.
        /// </summary>
        /// <param name="result">Result produced by the login logic service.</param>
        /// <returns>Login data contract containing the challenge and status.</returns>
        public static Contracts.LoginData ToContract(LogicAuth.LoginResult result)
        {
            return new Contracts.LoginData
            {
                Code = _resultCodes[result.Status],
                DisplayName = result.DisplayName,
                ChallengeId = result.ChallengeId.ToString()
            };
        }
    }
}
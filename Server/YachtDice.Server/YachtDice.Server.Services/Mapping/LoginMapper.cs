using System.Collections.Generic;
using System.Globalization;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Services.Contracts;

namespace YachtDice.Server.Services.Mapping
{
    /// <summary>
    /// Converts between the login data contracts and the classes of the logic layer.
    /// </summary>
    internal static class LoginMapper
    {
        private static readonly Dictionary<LoginStatus, LoginResultCode> _resultCodes =
            new Dictionary<LoginStatus, LoginResultCode>
            {
                { LoginStatus.VerificationCodeSent, LoginResultCode.VerificationCodeSent },
                { LoginStatus.MissingRequiredField, LoginResultCode.MissingRequiredField },
                { LoginStatus.InvalidCredentials, LoginResultCode.InvalidCredentials },
                { LoginStatus.VerificationLocked, LoginResultCode.VerificationLocked },
                { LoginStatus.CodeSendFailed, LoginResultCode.CodeSendFailed }
            };

        /// <summary>
        /// Creates the logic-layer request from the data sent by the client.
        /// </summary>
        /// <param name="dto">Data sent by the client.</param>
        /// <returns>Request for the logic layer.</returns>
        public static LoginRequest ToLoginRequest(LoginRequestDto dto)
        {
            return new LoginRequest
            {
                Email = dto.Email,
                Password = dto.Password,
                Culture = ParseCulture(dto.CultureName)
            };
        }

        /// <summary>
        /// Creates the data contract that is sent back to the client.
        /// </summary>
        /// <param name="result">Result produced by the logic layer.</param>
        /// <returns>Answer for the client.</returns>
        public static LoginResponseDto ToResponse(LoginResult result)
        {
            return new LoginResponseDto
            {
                Code = _resultCodes[result.Status],
                DisplayName = result.DisplayName
            };
        }

        private static CultureInfo ParseCulture(string cultureName)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            if (!string.IsNullOrWhiteSpace(cultureName))
            {
                try
                {
                    culture = CultureInfo.GetCultureInfo(cultureName);
                }
                catch (CultureNotFoundException)
                {
                    // An unknown language name falls back to the default language
                    culture = CultureInfo.InvariantCulture;
                }
            }

            return culture;
        }
    }
}
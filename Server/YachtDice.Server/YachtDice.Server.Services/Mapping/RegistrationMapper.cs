using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Services.Contracts;

namespace YachtDice.Server.Services.Mapping
{
    /// <summary>
    /// Converts the registration data contract into the request of the logic layer.
    /// </summary>
    internal static class RegistrationMapper
    {
        /// <summary>
        /// Creates the logic-layer request from the data sent by the client.
        /// </summary>
        /// <param name="dto">Data sent by the client.</param>
        /// <returns>Request for the logic layer.</returns>
        public static RegisterPlayerRequest ToRegisterRequest(RegisterRequestDto dto)
        {
            return new RegisterPlayerRequest
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password
            };
        }
    }
}
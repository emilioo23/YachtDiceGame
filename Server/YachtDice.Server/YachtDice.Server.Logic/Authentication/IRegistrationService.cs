using YachtDice.Server.Data;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Defines the contract for registering new players in the system.
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>
        /// Registers a new player with the given information and password.
        /// </summary>
        /// <param name="player">The player entity containing registration data.</param>
        /// <param name="plainPassword">The player's plain text password.</param>
        /// <returns>The result of the registration attempt.</returns>
        RegistrationResult Register(Player player, string plainPassword);
    }
}
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
        /// <param name="newPlayer">The player entity containing registration data.</param>
        /// <param name="plainPassword">The player's plain text password.</param>
        /// <returns>The result of the registration attempt.</returns>
        RegistrationResult RegisterPlayer(Player newPlayer, string plainPassword);
    }
}
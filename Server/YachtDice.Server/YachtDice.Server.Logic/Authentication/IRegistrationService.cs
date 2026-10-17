namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Defines the business logic operations for player registration.
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>
        /// Registers a new player in the system.
        /// </summary>
        /// <param name="info">The registration details of the new player.</param>
        /// <returns>The result of the registration attempt.</returns>
        RegistrationResult Register(PlayerRegistrationInfo info);
    }
}
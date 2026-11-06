namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts;
    using Data = YachtDice.Server.Data;

    /// <summary>
    /// Converts player registration contracts into logic layer models.
    /// </summary>
    internal static class RegistrationMapper
    {
        /// <summary>
        /// Creates the data model from the player entity provided by the client.
        /// </summary>
        /// <param name="player">Player entity provided by the client.</param>
        /// <returns>Player entity for the logic and data layers.</returns>
        public static Data.Player ToLogicModel(Contracts.Player player)
        {
            return new Data.Player
            {
                FirstName = player.FirstName,
                LastName = player.LastName,
                Username = player.Username,
                Email = player.Email
            };
        }
    }
}
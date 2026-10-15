namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Connection state of a player, stored as text in the database.
    /// </summary>
    public enum PlayerState
    {
        /// <summary>The player is connected and active.</summary>
        Active,

        /// <summary>The player is not connected.</summary>
        Disconnected
    }
}
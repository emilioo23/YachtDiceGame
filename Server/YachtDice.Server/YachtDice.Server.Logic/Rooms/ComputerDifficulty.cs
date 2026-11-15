namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// How well a computer player plays. Each value has its rules in <see cref="Game.ComputerSkill"/>, so a new
    /// difficulty is one value here and one line there.
    /// </summary>
    public enum ComputerDifficulty
    {
        /// <summary>Rolls once and writes the best box.</summary>
        Easy,

        /// <summary>Keeps the dice that repeat the most and stops when the dice are worth keeping.</summary>
        Normal,

        /// <summary>Plays like normal and also keeps the dice of a straight that is almost complete.</summary>
        Hard
    }
}

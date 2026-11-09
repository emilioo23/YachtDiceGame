namespace YachtDice.Client.Logic.Rooms
{
    /// <summary>
    /// A player of a match, reduced to what is needed to notice who came and who went.
    /// </summary>
    public class MatchParticipant
    {
        /// <summary>
        /// Initializes a participant.
        /// </summary>
        /// <param name="id">Identifier that the server gave to the player.</param>
        /// <param name="name">Name that the player shows.</param>
        public MatchParticipant(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Gets the identifier that the server gave to the player.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name that the player shows.
        /// </summary>
        public string Name { get; }
    }
}

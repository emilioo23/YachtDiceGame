namespace YachtDice.Models
{
    /// <summary>
    /// Place of the waiting room: a player who is inside or a free place waiting for one.
    /// </summary>
    public class LobbySlot
    {
        /// <summary>
        /// Gets or sets the name of the player in the place. It is empty if the place is free.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the initial of the name, used for the avatar.
        /// </summary>
        public string Initial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player in the place is the host of the room.
        /// </summary>
        public bool IsHost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the place is free.
        /// </summary>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Gets a value indicating whether a player is in the place.
        /// </summary>
        public bool IsOccupied => !IsEmpty;
    }
}

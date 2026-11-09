namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Rules to invite a friend to a room: only a player who is inside the room can invite, and only a friend.
    /// </summary>
    public interface IRoomInvitationService
    {
        /// <summary>
        /// Checks whether the player can invite the friend to the room.
        /// </summary>
        /// <param name="playerId">Identifier that the room gave to the player who invites.</param>
        /// <param name="friendName">Name of the friend to invite.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>The outcome of the check and the name that the room shows for the player who invites.</returns>
        InvitationResult Validate(string playerId, string friendName, string roomCode);
    }
}

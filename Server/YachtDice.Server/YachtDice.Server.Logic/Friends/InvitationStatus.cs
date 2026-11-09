namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Outcome of checking whether a player can invite a friend to a room.
    /// </summary>
    public enum InvitationStatus
    {
        /// <summary>The player can invite the friend.</summary>
        Allowed,

        /// <summary>The player who invites is not inside the room.</summary>
        NotInRoom,

        /// <summary>The player and the invited player are not friends.</summary>
        NotFriends
    }
}

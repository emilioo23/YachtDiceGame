using System.Collections.Generic;
using YachtDice.Server.Logic.Rooms;

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
        /// <param name="sender">Room and identifier that the room gave to the player who invites.</param>
        /// <param name="friendName">Name of the friend to invite.</param>
        /// <returns>The outcome of the check and the name that the room shows for the player who invites.</returns>
        InvitationResult CheckInvitation(RoomMember sender, string friendName);

        /// <summary>
        /// Saves an invitation that reached a friend, if both players have an account.
        /// </summary>
        /// <param name="senderName">Name of the player who invited.</param>
        /// <param name="friendName">Name of the friend who was invited.</param>
        /// <param name="roomCode">Code of the room.</param>
        void SaveInvitation(string senderName, string friendName, string roomCode);

        /// <summary>
        /// Gets the invitations that the player received in the last day, from the newest to the oldest.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The invitations, or an empty list if there is none.</returns>
        List<ReceivedInvitation> GetReceivedInvitations(string displayName);
    }
}

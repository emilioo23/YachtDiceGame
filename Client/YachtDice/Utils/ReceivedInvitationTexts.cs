using YachtDice.FriendServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Writes the sentence that the list of received invitations shows under the name of the friend.
    /// </summary>
    public static class ReceivedInvitationTexts
    {
        private const string HourFormat = "t";

        /// <summary>
        /// Says the room and the hour of the invitation, and whether the room is already closed.
        /// </summary>
        /// <param name="invitation">Invitation that the server sent.</param>
        /// <returns>The sentence in the language of the player.</returns>
        public static string Describe(ReceivedInvitation invitation)
        {
            string hour = invitation.SentAt.ToLocalTime().ToString(HourFormat);
            string format = Strings.Friends_Invitation_ClosedFormat;

            if (invitation.IsRoomOpen)
            {
                format = Strings.Friends_Invitation_OpenFormat;
            }

            return string.Format(format, invitation.RoomCode, hour);
        }
    }
}

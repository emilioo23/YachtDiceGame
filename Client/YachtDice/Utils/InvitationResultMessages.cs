using YachtDice.FriendServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player after inviting a friend to the room.
    /// </summary>
    public static class InvitationResultMessages
    {
        /// <summary>
        /// Gets the localized message that matches the outcome of the invitation.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <param name="friendName">Name of the friend who was invited.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(InvitationResultCode resultCode, string friendName)
        {
            string message;

            switch (resultCode)
            {
                case InvitationResultCode.Sent:
                    message = string.Format(Strings.Invitations_Result_Sent, friendName);
                    break;
                case InvitationResultCode.NotInRoom:
                    message = Strings.Invitations_Result_NotInRoom;
                    break;
                case InvitationResultCode.NotFriends:
                    message = Strings.Invitations_Result_NotFriends;
                    break;
                case InvitationResultCode.FriendOffline:
                    message = string.Format(Strings.Invitations_Result_FriendOffline, friendName);
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }
    }
}

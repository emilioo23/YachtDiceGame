using YachtDice.FriendServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player after sending a friend request.
    /// </summary>
    public static class FriendRequestMessages
    {
        /// <summary>
        /// Gets the localized message that matches the outcome of the friend request.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(FriendRequestResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case FriendRequestResultCode.RequestSent:
                    message = Strings.Friends_Result_RequestSent;
                    break;
                case FriendRequestResultCode.RequestAlreadyReceived:
                    message = Strings.Friends_Result_RequestAlreadyReceived;
                    break;
                case FriendRequestResultCode.InvalidUsername:
                    message = Strings.Friends_Result_InvalidUsername;
                    break;
                case FriendRequestResultCode.UsernameNotFound:
                    message = Strings.Friends_Result_UsernameNotFound;
                    break;
                case FriendRequestResultCode.SelfRequest:
                    message = Strings.Friends_Result_SelfRequest;
                    break;
                case FriendRequestResultCode.AlreadyFriends:
                    message = Strings.Friends_Result_AlreadyFriends;
                    break;
                case FriendRequestResultCode.AlreadyRequested:
                    message = Strings.Friends_Result_AlreadyRequested;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }
    }
}

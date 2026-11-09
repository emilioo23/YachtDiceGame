using YachtDice.Resources;
using YachtDice.RoomServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player when an operation on a room does not succeed.
    /// </summary>
    public static class RoomResultMessages
    {
        /// <summary>
        /// Gets the localized message that matches the outcome of the operation.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(RoomResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case RoomResultCode.RoomNotFound:
                    message = Strings.Lobby_Result_RoomNotFound;
                    break;
                case RoomResultCode.RoomFull:
                    message = Strings.Lobby_Result_RoomFull;
                    break;
                case RoomResultCode.GameAlreadyStarted:
                    message = Strings.Lobby_Result_GameAlreadyStarted;
                    break;
                case RoomResultCode.NotHost:
                    message = Strings.Lobby_Result_NotHost;
                    break;
                case RoomResultCode.NotEnoughPlayers:
                    message = Strings.Lobby_Result_NotEnoughPlayers;
                    break;
                case RoomResultCode.InvalidSettings:
                    message = Strings.Lobby_Result_InvalidSettings;
                    break;
                case RoomResultCode.InvalidAccessCode:
                    message = Strings.Lobby_Result_InvalidAccessCode;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }
    }
}

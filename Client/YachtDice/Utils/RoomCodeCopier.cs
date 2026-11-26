using System;
using System.Runtime.InteropServices;
using System.Windows;
using YachtDice.Client.Logic.Logging;
using YachtDice.Resources;
using YachtDice.Views;

namespace YachtDice.Utils
{
    /// <summary>
    /// Copies the code of a room to the clipboard and tells the player that it was copied, so the players of a
    /// room can give the code to somebody who has to enter it.
    /// </summary>
    public static class RoomCodeCopier
    {
        private static readonly AppLogger Logger = new AppLogger(typeof(RoomCodeCopier));

        /// <summary>
        /// Puts the code in the clipboard and shows a message with the result.
        /// </summary>
        /// <param name="owner">Window that shows the message.</param>
        /// <param name="roomCode">Code of the room to copy.</param>
        public static void Copy(Window owner, string roomCode)
        {
            string message;

            try
            {
                Clipboard.SetText(roomCode);
                message = string.Format(Strings.Lobby_NotificationCodeCopied, roomCode);
            }
            catch (ExternalException ex)
            {
                Logger.Error(ex, LogMessages.CopyFailedLogMessage);
                message = Strings.Dialogs_UnexpectedError;
            }

            CustomDialogWindow.ShowMessage(owner, message);
        }
    }
}

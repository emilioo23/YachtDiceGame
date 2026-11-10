using System;
using System.Collections.Generic;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player when an operation on a room does not succeed.
    /// </summary>
    public static class RoomResultMessages
    {
        private static readonly Dictionary<RoomResultCode, Func<string>> MessageByResult =
            new Dictionary<RoomResultCode, Func<string>>
            {
                { RoomResultCode.RoomNotFound, () => Strings.Lobby_Result_RoomNotFound },
                { RoomResultCode.RoomFull, () => Strings.Lobby_Result_RoomFull },
                { RoomResultCode.GameAlreadyStarted, () => Strings.Lobby_Result_GameAlreadyStarted },
                { RoomResultCode.NotHost, () => Strings.Lobby_Result_NotHost },
                { RoomResultCode.NotEnoughPlayers, () => Strings.Lobby_Result_NotEnoughPlayers },
                { RoomResultCode.InvalidSettings, () => Strings.Lobby_Result_InvalidSettings },
                { RoomResultCode.InvalidAccessCode, () => Strings.Lobby_Result_InvalidAccessCode },
                { RoomResultCode.PlayerNotFound, () => Strings.Lobby_Result_PlayerNotFound }
            };

        /// <summary>
        /// Gets the localized message that matches the outcome of the operation.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(RoomResultCode resultCode)
        {
            Func<string> getText;

            if (!MessageByResult.TryGetValue(resultCode, out getText))
            {
                getText = () => Strings.Dialogs_UnexpectedError;
            }

            return getText();
        }
    }
}

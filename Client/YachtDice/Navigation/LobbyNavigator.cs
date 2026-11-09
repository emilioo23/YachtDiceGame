using System;
using System.Threading.Tasks;
using System.Windows;
using YachtDice.Client.Logic.Rooms;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;
using YachtDice.Views;

namespace YachtDice.Navigation
{
    /// <summary>
    /// Takes the player from any window to the waiting room of a new room or of a room that already exists.
    /// If it cannot be done, it tells the player why and the current window stays open.
    /// </summary>
    public static class LobbyNavigator
    {
        /// <summary>
        /// Creates a room where the player is the host and opens its waiting room.
        /// </summary>
        /// <param name="currentWindow">Window that is open now; it closes if the room is created.</param>
        /// <param name="session">Session of the player who creates the room.</param>
        /// <returns>A task that finishes when the waiting room is open or the player was told the problem.</returns>
        public static async Task CreateRoomAndOpenAsync(Window currentWindow, PlayerSession session)
        {
            var roomGateway = new RoomGateway(session);

            try
            {
                await roomGateway.CreateRoomAsync();
                await OpenLobbyAsync(currentWindow, roomGateway);
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(currentWindow, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        /// <summary>
        /// Enters the room that has the code and opens its waiting room.
        /// </summary>
        /// <param name="currentWindow">Window that is open now; it closes if the player enters the room.</param>
        /// <param name="session">Session of the player who enters the room.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>A task that finishes when the waiting room is open or the player was told the problem.</returns>
        public static async Task JoinRoomAndOpenAsync(Window currentWindow, PlayerSession session, string roomCode)
        {
            var roomGateway = new RoomGateway(session);

            await JoinAndOpenAsync(currentWindow, roomGateway, () => roomGateway.JoinRoomAsync(roomCode));
        }

        /// <summary>
        /// Enters a room of the list of rooms and opens its waiting room.
        /// </summary>
        /// <param name="currentWindow">Window that is open now; it closes if the player enters the room.</param>
        /// <param name="session">Session of the player who enters the room.</param>
        /// <param name="room">Row of the list that the player chose, with the code typed for a private room.</param>
        /// <returns>A task that finishes when the waiting room is open or the player was told the problem.</returns>
        public static async Task JoinListedRoomAndOpenAsync(
            Window currentWindow,
            PlayerSession session,
            RoomListItem room)
        {
            var roomGateway = new RoomGateway(session);
            string accessCode = room.AccessCode.Trim();

            await JoinAndOpenAsync(
                currentWindow,
                roomGateway,
                () => roomGateway.JoinListedRoomAsync(room.RoomId, accessCode));
        }

        private static async Task JoinAndOpenAsync(
            Window currentWindow,
            RoomGateway roomGateway,
            Func<Task<RoomResultCode>> join)
        {
            try
            {
                RoomResultCode result = await join();

                if (result == RoomResultCode.Success)
                {
                    await OpenLobbyAsync(currentWindow, roomGateway);
                }
                else
                {
                    CustomDialogWindow.ShowMessage(currentWindow, RoomResultMessages.GetMessage(result));
                }
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(currentWindow, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        // The player joins the chat of the room before the window opens, so the waiting room and the game
        // share the same chat connection and its messages. A player who comes back to a match in progress
        // goes straight to the game.
        private static async Task OpenLobbyAsync(Window currentWindow, RoomGateway roomGateway)
        {
            var chatGateway = new ChatGateway(roomGateway.PlayerId, roomGateway.CurrentRoom.Code);
            bool isChatAvailable = await chatGateway.TryJoinAsync();

            ConnectionLossHandler.Watch(roomGateway);
            ConnectionLossHandler.Watch(chatGateway);
            WindowNavigator.SwitchTo(currentWindow, CreateRoomWindow(roomGateway, chatGateway, isChatAvailable));
        }

        private static Window CreateRoomWindow(RoomGateway roomGateway, ChatGateway chatGateway, bool isChatAvailable)
        {
            Window roomWindow;

            if (roomGateway.CurrentRoom.IsStarted)
            {
                roomWindow = new GameWindow(roomGateway, chatGateway, isChatAvailable);
            }
            else
            {
                roomWindow = new LobbyWindow(roomGateway, chatGateway, isChatAvailable);
            }

            return roomWindow;
        }
    }
}

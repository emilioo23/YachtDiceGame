using System;
using System.Linq;
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
            var roomToJoin = new RoomAccess
            {
                RoomId = room.RoomId,
                AccessCode = room.AccessCode.Trim()
            };

            await JoinAndOpenAsync(
                currentWindow,
                roomGateway,
                () => roomGateway.JoinListedRoomAsync(roomToJoin));
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

        /// <summary>
        /// Takes the player out of the chat and of the room. The window of the room is already closed, so if the
        /// server cannot be told, the player is told on the window that is open now.
        /// </summary>
        /// <param name="chat">Chat of the room that the player leaves.</param>
        /// <param name="roomGateway">Connection of the player to the room that the player leaves.</param>
        /// <returns>A task that finishes when the player left or was told the problem.</returns>
        public static async Task LeaveRoomAsync(RoomChat chat, RoomGateway roomGateway)
        {
            try
            {
                await Task.WhenAll(chat.Gateway.LeaveAsync(), roomGateway.LeaveRoomAsync());
            }
            catch (GatewayException ex)
            {
                Window openWindow = Application.Current.Windows.OfType<Window>().LastOrDefault();

                CustomDialogWindow.ShowMessage(openWindow, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private static async Task OpenLobbyAsync(Window currentWindow, RoomGateway roomGateway)
        {
            var chatGateway = new ChatGateway(new RoomMembership(roomGateway.PlayerId, roomGateway.CurrentRoom.Code));
            bool isChatAvailable = await chatGateway.TryJoinAsync();

            ConnectionLossHandler.Watch(roomGateway);
            ConnectionLossHandler.Watch(chatGateway);
            var chat = new RoomChat(chatGateway, isChatAvailable);
            WindowNavigator.SwitchTo(currentWindow, CreateRoomWindow(roomGateway, chat));
        }

        private static Window CreateRoomWindow(RoomGateway roomGateway, RoomChat chat)
        {
            Window roomWindow;

            if (roomGateway.CurrentRoom.IsStarted)
            {
                roomWindow = new GameWindow(roomGateway, chat);
            }
            else
            {
                roomWindow = new LobbyWindow(roomGateway, chat);
            }

            return roomWindow;
        }
    }
}

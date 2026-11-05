using System.Threading.Tasks;
using System.Windows;
using YachtDice.Gateway;
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
        /// <param name="playerName">Name of the player who creates the room.</param>
        /// <returns>A task that finishes when the waiting room is open or the player was told the problem.</returns>
        public static async Task CreateRoomAndOpenAsync(Window currentWindow, string playerName)
        {
            var roomGateway = new RoomGateway(playerName);

            try
            {
                await roomGateway.CreateRoomAsync();
                OpenLobby(currentWindow, roomGateway);
            }
            catch (GatewayException ex)
            {
                ShowMessage(currentWindow, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        /// <summary>
        /// Enters the room that has the code and opens its waiting room.
        /// </summary>
        /// <param name="currentWindow">Window that is open now; it closes if the player enters the room.</param>
        /// <param name="playerName">Name of the player who enters the room.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>A task that finishes when the waiting room is open or the player was told the problem.</returns>
        public static async Task JoinRoomAndOpenAsync(Window currentWindow, string playerName, string roomCode)
        {
            var roomGateway = new RoomGateway(playerName);

            try
            {
                RoomResultCode result = await roomGateway.JoinRoomAsync(roomCode);

                if (result == RoomResultCode.Success)
                {
                    OpenLobby(currentWindow, roomGateway);
                }
                else
                {
                    ShowMessage(currentWindow, RoomResultMessages.GetMessage(result));
                }
            }
            catch (GatewayException ex)
            {
                ShowMessage(currentWindow, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private static void OpenLobby(Window currentWindow, RoomGateway roomGateway)
        {
            var lobbyWindow = new LobbyWindow(roomGateway);

            lobbyWindow.Show();
            currentWindow.Close();
        }

        private static void ShowMessage(Window owner, string message)
        {
            var dialog = new CustomDialogWindow(new DialogContent { Message = message })
            {
                Owner = owner
            };

            dialog.ShowDialog();
        }
    }
}

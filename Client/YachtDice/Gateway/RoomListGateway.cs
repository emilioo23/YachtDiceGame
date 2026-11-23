using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.RoomServiceReference;
using RoomFault = YachtDice.RoomServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Asks the server for the rooms that wait for players. It opens a connection only while it asks, so the
    /// player is not inside any room when the list arrives.
    /// </summary>
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class RoomListGateway : DuplexGatewayBase<RoomServiceClient>, IRoomServiceCallback
    {
        /// <summary>
        /// Gets the rooms whose game did not start, public and private.
        /// </summary>
        /// <returns>A short description of each room.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<List<RoomSummary>> GetRoomsAsync()
        {
            RoomSummary[] rooms = await ConnectAndExecuteAsync(client => client.GetRoomList());

            await CloseAsync();

            return rooms.ToList();
        }

        /// <summary>
        /// Gets the latest expulsions where the player was the host or the expelled player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The expulsions from the newest to the oldest.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<List<KickHistoryEntry>> GetKickHistoryAsync(string sessionToken)
        {
            KickHistoryEntry[] history = await ConnectAndExecuteAsync(client => client.GetKickHistory(sessionToken));

            await CloseAsync();

            return history.ToList();
        }

        /// <summary>
        /// Looks for a match in progress that the player left and can still join again.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The code of the room, or an empty string if there is no pending match.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<string> FindPendingRoomAsync(string displayName)
        {
            string roomCode = await ConnectAndExecuteAsync(client => client.FindPendingRoom(displayName));

            await CloseAsync();

            return roomCode;
        }

        /// <inheritdoc />
        protected override RoomServiceClient CreateClient()
        {
            var client = new RoomServiceClient(new InstanceContext(this));
            ServerAddress.Apply(client.Endpoint);

            return client;
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<RoomFault>;

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }

        /// <summary>
        /// Ignores the changes of a room: this gateway only asks for the list and is never inside a room.
        /// </summary>
        /// <param name="room">State of the room after the change.</param>
        /// <param name="eventType">What happened in the room.</param>
        void IRoomServiceCallback.RoomChanged(RoomState room, RoomEventType eventType)
        {
            // The list of rooms does not follow the changes of a room, so there is nothing to do here.
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Callbacks;
using YachtDice.RoomServiceReference;
using RoomFault = YachtDice.RoomServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Asks the server for the rooms that wait for players. It opens a connection only while it asks, so the
    /// player is not inside any room when the list arrives.
    /// </summary>
    public class RoomListGateway : DuplexGatewayBase<RoomServiceClient>
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

        /// <inheritdoc />
        protected override RoomServiceClient CreateClient()
        {
            var client = new RoomServiceClient(new InstanceContext(new RoomCallbackHandler()));
            ServerAddress.Apply(client.Endpoint);

            return client;
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<RoomFault>;

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }
    }
}

using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Game;
    using LogicRooms = YachtDice.Server.Logic.Rooms;
    using RoomContracts = YachtDice.Server.Contracts.Rooms;

    /// <summary>
    /// Lets the players of a game in progress roll the dice and choose the boxes of their scorecards. The
    /// server decides the dice, the turns and the points, and tells every player what changed.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class GameService : ServiceBase, Contracts.IGameService
    {
        private readonly GameCoordinator _coordinator;

        /// <summary>
        /// Initializes a new instance of the GameService class.
        /// </summary>
        /// <param name="coordinator">The games and their notifications. It is shared with the room service.</param>
        public GameService(GameCoordinator coordinator)
        {
            _coordinator = coordinator
                ?? throw new ArgumentNullException(nameof(coordinator));
        }

        /// <summary>
        /// Subscribes the player to the changes of the game of the room and gives the current state.
        /// </summary>
        /// <param name="player">Room and identifier of the player who joins the game.</param>
        /// <returns>Whether the player joined and the state of the game.</returns>
        public Contracts.JoinGameResult JoinGame(RoomContracts.RoomMember player)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(player);
            Contracts.JoinGameResult result = Execute(player, () => ProcessJoin(member));

            return result;
        }

        /// <summary>
        /// Rolls the dice of the turn and tells every player.
        /// </summary>
        /// <param name="player">Room and identifier of the player who rolls.</param>
        /// <param name="heldDice">One value for each die: true if the die keeps its value.</param>
        /// <returns>Outcome of the roll.</returns>
        public Contracts.GameResultCode RollDice(RoomContracts.RoomMember player, bool[] heldDice)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(player);
            Contracts.GameResultCode result = Execute(heldDice, () => _coordinator.Roll(member, heldDice));

            return result;
        }

        /// <summary>
        /// Writes the points of the current dice in a box and passes the turn to the next player.
        /// </summary>
        /// <param name="player">Room and identifier of the player who chooses the box.</param>
        /// <param name="category">Box to use.</param>
        /// <returns>Outcome of the move.</returns>
        public Contracts.GameResultCode SelectCategory(
            RoomContracts.RoomMember player,
            Contracts.ScoreCategory category)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(player);
            Contracts.GameResultCode result = Execute(player, () => _coordinator.Score(member, category));

            return result;
        }

        private Contracts.JoinGameResult ProcessJoin(LogicRooms.RoomMember member)
        {
            Contracts.IGameCallback callback = OperationContext.Current.GetCallbackChannel<Contracts.IGameCallback>();
            Contracts.JoinGameResult result = _coordinator.Join(member, callback);

            if (result.Code == Contracts.GameResultCode.Success)
            {
                WatchChannel(member, callback);
            }

            return result;
        }

        private void WatchChannel(LogicRooms.RoomMember member, Contracts.IGameCallback callback)
        {
            ICommunicationObject channel = OperationContext.Current.Channel;

            channel.Closed += (sender, e) => _coordinator.Unsubscribe(member, callback);
            channel.Faulted += (sender, e) => _coordinator.Unsubscribe(member, callback);
        }
    }
}

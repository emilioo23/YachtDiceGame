using System;
using System.Collections.Generic;

namespace YachtDice.Server.Services
{
    using LogicRooms = YachtDice.Server.Logic.Rooms;

    /// <summary>
    /// Counts the time that a player who left a game in progress has to come back. When the time is over it
    /// runs the action given at the start. The private steps only run while the lock is held.
    /// </summary>
    internal sealed class ReconnectTimer
    {
        private const int NoTurnNumber = 0;

        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, Dictionary<string, TurnClock>> _clocksByRoom =
            new Dictionary<string, Dictionary<string, TurnClock>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Starts counting the time of a player. The countdown that the player already had is cancelled.
        /// </summary>
        /// <param name="member">Room and identifier of the player who has to come back.</param>
        /// <param name="seconds">Seconds that the player has.</param>
        /// <param name="onExpired">Action to run if the player does not come back in time.</param>
        public void Start(LogicRooms.RoomMember member, int seconds, Action onExpired)
        {
            var clock = new TurnClock(NoTurnNumber, seconds, onExpired);

            lock (_syncRoot)
            {
                DisposeClock(member);

                if (!_clocksByRoom.TryGetValue(member.RoomCode, out Dictionary<string, TurnClock> clocks))
                {
                    clocks = new Dictionary<string, TurnClock>(StringComparer.Ordinal);
                    _clocksByRoom[member.RoomCode] = clocks;
                }

                clocks[member.PlayerId] = clock;
            }
        }

        /// <summary>
        /// Stops counting the time of a player, for example because the player came back.
        /// </summary>
        /// <param name="member">Room and identifier of the player.</param>
        public void Cancel(LogicRooms.RoomMember member)
        {
            lock (_syncRoot)
            {
                DisposeClock(member);
            }
        }

        /// <summary>
        /// Stops counting the time of every player of a room.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        public void CancelRoom(string roomCode)
        {
            lock (_syncRoot)
            {
                if (_clocksByRoom.TryGetValue(roomCode, out Dictionary<string, TurnClock> clocks))
                {
                    foreach (TurnClock clock in clocks.Values)
                    {
                        clock.Dispose();
                    }

                    _clocksByRoom.Remove(roomCode);
                }
            }
        }

        private void DisposeClock(LogicRooms.RoomMember member)
        {
            bool hasClock = _clocksByRoom.TryGetValue(member.RoomCode, out Dictionary<string, TurnClock> clocks)
                && clocks.ContainsKey(member.PlayerId);

            if (hasClock)
            {
                clocks[member.PlayerId].Dispose();
                clocks.Remove(member.PlayerId);
            }

            if (hasClock && clocks.Count == 0)
            {
                _clocksByRoom.Remove(member.RoomCode);
            }
        }
    }
}

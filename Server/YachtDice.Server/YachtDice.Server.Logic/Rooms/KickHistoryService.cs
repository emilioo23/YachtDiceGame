using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Logging;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Saves the expulsions that the hosts make and shows each player the expulsions where they took part. An
    /// expulsion is only saved when both players have an account, because a guest has no history.
    /// </summary>
    public class KickHistoryService
    {
        private const int MaxHistoryEntries = 20;

        private readonly IPlayerRepository _playerRepository;
        private readonly IKickLogRepository _kickLogRepository;

        /// <summary>
        /// Initializes the service with the repositories it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to find the accounts by their name.</param>
        /// <param name="kickLogRepository">Repository where the expulsions are saved.</param>
        /// <exception cref="ArgumentNullException">Any of the repositories is null.</exception>
        public KickHistoryService(IPlayerRepository playerRepository, IKickLogRepository kickLogRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository),
                    LogMessages.KickHistoryNullDependencyMessage);
            _kickLogRepository = kickLogRepository
                ?? throw new ArgumentNullException(nameof(kickLogRepository),
                    LogMessages.KickHistoryNullDependencyMessage);
        }

        /// <summary>
        /// Saves an expulsion if both players have an account.
        /// </summary>
        /// <param name="roomCode">Code of the room where it happened.</param>
        /// <param name="hostName">Name of the host who expelled the player.</param>
        /// <param name="targetName">Name of the player who was expelled.</param>
        /// <returns>True if the expulsion was saved; false if one of them is a guest.</returns>
        public bool Record(string roomCode, string hostName, string targetName)
        {
            Player host = FindAccount(hostName);
            Player target = FindAccount(targetName);
            bool areRegistered = host != null && target != null;

            if (areRegistered)
            {
                _kickLogRepository.Add(new KickLog
                {
                    HostId = host.Id,
                    TargetId = target.Id,
                    RoomCode = roomCode,
                    Date = DateTime.UtcNow
                });
            }

            return areRegistered;
        }

        /// <summary>
        /// Gets the latest expulsions where the player was the host or the expelled player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The expulsions from the newest to the oldest, or an empty list if there is none.</returns>
        public List<KickHistoryEntry> GetHistory(string displayName)
        {
            Player player = FindAccount(displayName);
            var history = new List<KickHistoryEntry>();

            if (player != null)
            {
                history = _kickLogRepository.FindByPlayer(player.Id, MaxHistoryEntries).Select(ToEntry).ToList();
            }

            return history;
        }

        private static KickHistoryEntry ToEntry(KickLogRecord record)
        {
            return new KickHistoryEntry
            {
                HostName = record.HostName,
                TargetName = record.TargetName,
                RoomCode = record.RoomCode,
                Date = record.Date
            };
        }

        private Player FindAccount(string displayName)
        {
            Player player = null;

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                player = _playerRepository.FindByName(displayName).FirstOrDefault();
            }

            return player;
        }
    }
}

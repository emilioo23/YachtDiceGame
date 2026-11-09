using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Models;
using YachtDice.RoomServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Builds the list of players shown at the top of the scorecard of a match.
    /// </summary>
    public static class MatchPlayerFactory
    {
        /// <summary>
        /// Creates the players of a match. The local player goes first, so the scorecard always shows the own
        /// score in the same column, and the others keep the order of the room.
        /// </summary>
        /// <param name="roomPlayers">Players inside the room.</param>
        /// <param name="localPlayerId">Identifier of the player who uses the window.</param>
        /// <returns>The local player followed by the opponents.</returns>
        public static List<MatchPlayer> Create(IEnumerable<RoomPlayer> roomPlayers, string localPlayerId)
        {
            List<MatchPlayer> players = roomPlayers
                .OrderByDescending(player => IsSameId(player.Id, localPlayerId))
                .Select(player => CreatePlayer(player, IsSameId(player.Id, localPlayerId)))
                .ToList();

            return players;
        }

        /// <summary>
        /// Marks the players who are no longer inside the room. They stay in the list because the scorecard
        /// keeps their column until the match ends.
        /// </summary>
        /// <param name="players">Players of the match.</param>
        /// <param name="roomPlayers">Players who are inside the room now.</param>
        /// <returns>A new list with the same players and the updated presence.</returns>
        public static List<MatchPlayer> UpdatePresence(
            IEnumerable<MatchPlayer> players,
            IEnumerable<RoomPlayer> roomPlayers)
        {
            var presentIds = new HashSet<string>(roomPlayers.Select(player => player.Id), StringComparer.Ordinal);
            List<MatchPlayer> updatedPlayers = players
                .Select(player => CopyWithPresence(player, presentIds))
                .ToList();

            return updatedPlayers;
        }

        private static MatchPlayer CreatePlayer(RoomPlayer roomPlayer, bool isLocalPlayer)
        {
            var player = new MatchPlayer
            {
                Id = roomPlayer.Id,
                Name = roomPlayer.DisplayName,
                Initial = PlayerInitials.GetInitial(roomPlayer.DisplayName),
                IsLocalPlayer = isLocalPlayer
            };

            return player;
        }

        private static MatchPlayer CopyWithPresence(MatchPlayer player, HashSet<string> presentIds)
        {
            var updatedPlayer = new MatchPlayer
            {
                Id = player.Id,
                Name = player.Name,
                Initial = player.Initial,
                IsLocalPlayer = player.IsLocalPlayer,
                HasLeft = !presentIds.Contains(player.Id)
            };

            return updatedPlayer;
        }

        private static bool IsSameId(string firstId, string secondId)
        {
            return string.Equals(firstId, secondId, StringComparison.Ordinal);
        }
    }
}

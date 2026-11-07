using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Models;

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
        /// <param name="roomPlayers">Names of the players inside the room.</param>
        /// <param name="localPlayerName">Name of the player who uses the window.</param>
        /// <returns>The local player followed by the opponents.</returns>
        public static List<MatchPlayer> Create(IEnumerable<string> roomPlayers, string localPlayerName)
        {
            var players = new List<MatchPlayer> { CreatePlayer(localPlayerName, true) };

            players.AddRange(roomPlayers
                .Where(name => !IsSameName(name, localPlayerName))
                .Select(name => CreatePlayer(name, false)));

            return players;
        }

        /// <summary>
        /// Marks the players who are no longer inside the room. They stay in the list because the scorecard
        /// keeps their column until the match ends.
        /// </summary>
        /// <param name="players">Players of the match.</param>
        /// <param name="roomPlayers">Names of the players who are inside the room now.</param>
        /// <returns>A new list with the same players and the updated presence.</returns>
        public static List<MatchPlayer> UpdatePresence(
            IEnumerable<MatchPlayer> players,
            IEnumerable<string> roomPlayers)
        {
            var presentNames = new HashSet<string>(roomPlayers, StringComparer.OrdinalIgnoreCase);
            List<MatchPlayer> updatedPlayers = players
                .Select(player => CopyWithPresence(player, presentNames))
                .ToList();

            return updatedPlayers;
        }

        private static MatchPlayer CreatePlayer(string name, bool isLocalPlayer)
        {
            var player = new MatchPlayer
            {
                Name = name,
                Initial = PlayerInitials.GetInitial(name),
                IsLocalPlayer = isLocalPlayer
            };

            return player;
        }

        private static MatchPlayer CopyWithPresence(MatchPlayer player, HashSet<string> presentNames)
        {
            var updatedPlayer = new MatchPlayer
            {
                Name = player.Name,
                Initial = player.Initial,
                IsLocalPlayer = player.IsLocalPlayer,
                HasLeft = !presentNames.Contains(player.Name)
            };

            return updatedPlayer;
        }

        private static bool IsSameName(string firstName, string secondName)
        {
            return string.Equals(firstName, secondName, StringComparison.OrdinalIgnoreCase);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the players in memory so the tests do not need the database. Texts are compared ignoring the
    /// case, as a case-insensitive collation does.
    /// </summary>
    internal sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly List<Player> _players = new List<Player>();
        private readonly HashSet<int> _playersWithTwoFactor = new HashSet<int>();

        /// <summary>
        /// Gets the players saved so far.
        /// </summary>
        public IReadOnlyList<Player> Players => _players;

        /// <inheritdoc />
        public List<Player> FindByEmail(string email)
        {
            List<Player> matches = _players.Where(player => IsSameText(player.Email, email)).ToList();

            return matches;
        }

        /// <inheritdoc />
        public List<Player> FindByName(string name)
        {
            Func<Player, bool> hasName =
                player => IsSameText(player.DisplayName, name) || IsSameText(player.Username, name);
            List<Player> matches = _players
                .Where(hasName)
                .Take(1)
                .ToList();

            return matches;
        }

        /// <inheritdoc />
        public List<Player> FindByUsername(string username)
        {
            List<Player> matches = _players.Where(player => IsSameText(player.Username, username)).Take(1).ToList();

            return matches;
        }

        /// <inheritdoc />
        public bool EmailExists(string email)
        {
            bool isRegistered = _players.Any(player => IsSameText(player.Email, email));

            return isRegistered;
        }

        /// <inheritdoc />
        public bool UsernameExists(string username)
        {
            bool isTaken = _players.Any(player => IsSameText(player.Username, username));

            return isTaken;
        }

        /// <inheritdoc />
        public void Add(Player player)
        {
            _players.Add(player);
        }

        /// <inheritdoc />
        public bool HasTwoFactorEnabled(int playerId)
        {
            return _playersWithTwoFactor.Contains(playerId);
        }

        /// <inheritdoc />
        public void SaveTwoFactorEnabled(int playerId, bool isEnabled)
        {
            if (isEnabled)
            {
                _playersWithTwoFactor.Add(playerId);
            }
            else
            {
                _playersWithTwoFactor.Remove(playerId);
            }
        }

        public void SaveAvatar(int playerId, int avatarId)
        {
            Func<Player, bool> isSameId = candidate => candidate.Id == playerId;
            Player player = _players.First(isSameId);

            player.AvatarId = avatarId;
        }

        private static bool IsSameText(string firstText, string secondText)
        {
            return string.Equals(firstText, secondText, StringComparison.OrdinalIgnoreCase);
        }
    }
}

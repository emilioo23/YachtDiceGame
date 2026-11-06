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
            return _players.Where(player => IsSameText(player.Email, email)).ToList();
        }

        /// <inheritdoc />
        public List<Player> FindByName(string name)
        {
            return _players
                .Where(player => IsSameText(player.DisplayName, name) || IsSameText(player.Username, name))
                .Take(1)
                .ToList();
        }

        /// <inheritdoc />
        public List<Player> FindByUsername(string username)
        {
            return _players.Where(player => IsSameText(player.Username, username)).Take(1).ToList();
        }

        /// <inheritdoc />
        public bool EmailExists(string email)
        {
            return _players.Any(player => IsSameText(player.Email, email));
        }

        /// <inheritdoc />
        public bool UsernameExists(string username)
        {
            return _players.Any(player => IsSameText(player.Username, username));
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

        private static bool IsSameText(string firstText, string secondText)
        {
            return string.Equals(firstText, secondText, StringComparison.OrdinalIgnoreCase);
        }
    }
}

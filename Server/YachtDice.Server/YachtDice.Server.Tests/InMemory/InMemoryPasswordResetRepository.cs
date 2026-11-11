using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the password recovery codes in memory so the tests do not need the database. The new password is
    /// saved in the players of the in-memory repository it receives.
    /// </summary>
    internal sealed class InMemoryPasswordResetRepository : IPasswordResetRepository
    {
        private readonly InMemoryPlayerRepository _players;
        private readonly List<PwdResetToken> _tokens = new List<PwdResetToken>();

        /// <summary>
        /// Initializes the repository with the players whose password can change.
        /// </summary>
        /// <param name="players">In-memory players of the test.</param>
        public InMemoryPasswordResetRepository(InMemoryPlayerRepository players)
        {
            _players = players;
        }

        /// <summary>
        /// Gets the recovery codes saved so far.
        /// </summary>
        public IReadOnlyList<PwdResetToken> Tokens => _tokens;

        /// <inheritdoc />
        public List<PwdResetToken> FindByPlayerId(int playerId)
        {
            Func<PwdResetToken, bool> isSamePlayerId = token => token.PlayerId == playerId;
            List<PwdResetToken> matches = _tokens.Where(isSamePlayerId).ToList();

            return matches;
        }

        /// <inheritdoc />
        public void Save(int playerId, string tokenHash, DateTime expireDate)
        {
            Delete(playerId);
            _tokens.Add(new PwdResetToken { PlayerId = playerId, Token = tokenHash, ExpireDate = expireDate });
        }

        /// <inheritdoc />
        public void Delete(int playerId)
        {
            Predicate<PwdResetToken> isSamePlayerId = token => token.PlayerId == playerId;
            _tokens.RemoveAll(isSamePlayerId);
        }

        /// <inheritdoc />
        public void CompleteReset(int playerId, string newPasswordHash)
        {
            Func<Player, bool> isSameId = row => row.Id == playerId;
            Player player = _players.Players.First(isSameId);

            player.PwdHash = newPasswordHash;
            Delete(playerId);
        }
    }
}

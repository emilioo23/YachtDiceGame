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
            List<PwdResetToken> matches = _tokens.Where(token => token.PlayerId == playerId).ToList();

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
            _tokens.RemoveAll(token => token.PlayerId == playerId);
        }

        /// <inheritdoc />
        public void CompleteReset(int playerId, string newPasswordHash)
        {
            Player player = _players.Players.First(row => row.Id == playerId);

            player.PwdHash = newPasswordHash;
            Delete(playerId);
        }
    }
}

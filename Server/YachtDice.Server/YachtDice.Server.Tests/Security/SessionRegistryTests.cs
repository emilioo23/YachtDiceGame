using System;
using Xunit;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// The sessions of the players: the token that proves who is calling the server.
    /// </summary>
    public class SessionRegistryTests
    {
        private const string PlayerName = "gabrielhernandez";
        private const string NewPlayerName = "gabriel_hm";
        private const string ChallengeId = "challenge-1";
        private const string UnknownToken = "token-inventado";

        private readonly SessionRegistry _sessions = new SessionRegistry();

        [Fact]
        public void TryGetPlayerName_TokenOfAStartedSession_ReturnsThePlayer()
        {
            string token = _sessions.Start(PlayerName);

            _sessions.TryGetPlayerName(token, out string playerName);

            Assert.Equal(PlayerName, playerName);
        }

        [Fact]
        public void TryGetPlayerName_UnknownToken_ReturnsFalse()
        {
            bool isFound = _sessions.TryGetPlayerName(UnknownToken, out _);

            Assert.False(isFound);
        }

        [Fact]
        public void TryGetPlayerName_PendingSession_ReturnsFalse()
        {
            string token = _sessions.StartPending(PlayerName, ChallengeId);

            bool isFound = _sessions.TryGetPlayerName(token, out _);

            Assert.False(isFound);
        }

        [Fact]
        public void Activate_PendingSession_MakesTheTokenWork()
        {
            string token = _sessions.StartPending(PlayerName, ChallengeId);

            _sessions.Activate(ChallengeId);

            Assert.True(_sessions.TryGetPlayerName(token, out _));
        }

        [Fact]
        public void TryGetPendingPlayerName_PendingChallenge_ReturnsThePlayer()
        {
            _sessions.StartPending(PlayerName, ChallengeId);

            _sessions.TryGetPendingPlayerName(ChallengeId, out string playerName);

            Assert.Equal(PlayerName, playerName);
        }

        [Fact]
        public void TryGetPendingPlayerName_ChallengeThatWasNotIssued_ReturnsFalse()
        {
            bool isFound = _sessions.TryGetPendingPlayerName(ChallengeId, out _);

            Assert.False(isFound);
        }

        [Fact]
        public void TryGetPendingPlayerName_CompletedChallenge_ReturnsFalse()
        {
            _sessions.StartPending(PlayerName, ChallengeId);
            _sessions.Activate(ChallengeId);

            bool isFound = _sessions.TryGetPendingPlayerName(ChallengeId, out _);

            Assert.False(isFound);
        }

        [Fact]
        public void Start_SecondLogin_EndsTheFirstSession()
        {
            string firstToken = _sessions.Start(PlayerName);

            _sessions.Start(PlayerName);

            Assert.False(_sessions.TryGetPlayerName(firstToken, out _));
        }

        [Fact]
        public void EndFor_PlayerWithSession_EndsTheSession()
        {
            string token = _sessions.Start(PlayerName);

            _sessions.EndFor(PlayerName);

            Assert.False(_sessions.TryGetPlayerName(token, out _));
        }

        [Fact]
        public void Rename_PlayerWithSession_KeepsTheSessionWithTheNewName()
        {
            string token = _sessions.Start(PlayerName);

            _sessions.Rename(PlayerName, NewPlayerName);

            _sessions.TryGetPlayerName(token, out string playerName);
            Assert.Equal(NewPlayerName, playerName);
        }

        [Fact]
        public void Start_TwoPlayers_GivesDifferentTokens()
        {
            string firstToken = _sessions.Start(PlayerName);

            string secondToken = _sessions.Start(NewPlayerName);

            Assert.NotEqual(firstToken, secondToken);
        }

        [Fact]
        public void Start_BlankName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _sessions.Start(" "));
        }
    }
}

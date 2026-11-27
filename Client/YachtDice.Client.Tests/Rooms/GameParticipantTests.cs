using System;
using Xunit;
using YachtDice.Client.Logic.Rooms;

namespace YachtDice.Client.Tests.Rooms
{
    /// <summary>
    /// A player of a game as the client shows it: the identifier in the room and the name.
    /// </summary>
    public class GameParticipantTests
    {
        private const string PlayerId = "player-1";
        private const string PlayerName = "gabrielhernandez";

        [Fact]
        public void Constructor_NullId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GameParticipant(null, PlayerName));
        }

        [Fact]
        public void Constructor_NullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GameParticipant(PlayerId, null));
        }

        [Fact]
        public void Constructor_ValidValues_KeepsTheId()
        {
            var participant = new GameParticipant(PlayerId, PlayerName);

            Assert.Equal(PlayerId, participant.Id);
        }

        [Fact]
        public void Constructor_ValidValues_KeepsTheName()
        {
            var participant = new GameParticipant(PlayerId, PlayerName);

            Assert.Equal(PlayerName, participant.Name);
        }
    }
}

using Xunit;
using YachtDice.Server.Services;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Logging
{
    /// <summary>
    /// How a player is written in the log: by the identifier of the account and never by the name.
    /// </summary>
    public class PlayerLogNameTests
    {
        private const string PlayerName = "Marina";
        private const string GuestName = "Guest";
        private const int PlayerId = 12;
        private const string PlayerWithAccountExpected = "Player 12";
        private const string UnknownPlayerExpected = "A player";

        [Fact]
        public void Describe_PlayerWithAccount_WritesTheIdentifier()
        {
            var finder = new InMemoryFriendshipService();
            finder.AddAccount(PlayerName, PlayerId);

            string textResult = PlayerLogName.Describe(finder, PlayerName);

            Assert.Equal(PlayerWithAccountExpected, textResult);
        }

        [Fact]
        public void Describe_NameWithoutAccount_WritesAGenericPlayer()
        {
            var finder = new InMemoryFriendshipService();

            string textResult = PlayerLogName.Describe(finder, GuestName);

            Assert.Equal(UnknownPlayerExpected, textResult);
        }

        [Fact]
        public void Describe_DatabaseNotAvailable_WritesAGenericPlayer()
        {
            var finder = new InMemoryFailingPlayerIdFinder();

            string textResult = PlayerLogName.Describe(finder, PlayerName);

            Assert.Equal(UnknownPlayerExpected, textResult);
        }
    }
}

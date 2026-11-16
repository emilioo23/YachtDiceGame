using Xunit;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Sentences that describe an expulsion from the point of view of the player who reads the history.
    /// </summary>
    public class KickHistoryTextsTests
    {
        private const string OwnName = "gabriel";
        private const string OtherName = "emilio";
        private const string RoomCode = "ABC123";

        [Fact]
        public void Describe_PlayerWasTheHost_SaysThatThePlayerExpelledTheOther()
        {
            var entry = new KickHistoryEntry { HostName = OwnName, TargetName = OtherName, RoomCode = RoomCode };
            string textExpected = string.Format(Strings.History_KickedByYouFormat, OtherName, RoomCode);

            string text = KickHistoryTexts.Describe(entry, OwnName);

            Assert.Equal(textExpected, text);
        }

        [Fact]
        public void Describe_PlayerWasExpelled_SaysWhoExpelledThePlayer()
        {
            var entry = new KickHistoryEntry { HostName = OtherName, TargetName = OwnName, RoomCode = RoomCode };
            string textExpected = string.Format(Strings.History_KickedYouFormat, OtherName, RoomCode);

            string text = KickHistoryTexts.Describe(entry, OwnName);

            Assert.Equal(textExpected, text);
        }
    }
}

using Xunit;
using YachtDice.Client.Logic.Players;

namespace YachtDice.Client.Tests.Players
{
    /// <summary>
    /// What the history of games shows when the rivals have a name and when they were guests.
    /// </summary>
    public class HistoryDisplayTests
    {
        private const string OpponentNames = "Gabriel, Carlos";
        private const string GuestText = "Invitado";
        private const string ComputerText = "la computadora";
        private const string NoNames = "";
        private const string OnlySpaces = "   ";

        [Fact]
        public void ChooseOpponents_NamesOfRegisteredRivals_ReturnsTheNames()
        {
            string text = HistoryDisplay.ChooseOpponents(OpponentNames, GuestText);

            Assert.Equal(OpponentNames, text);
        }

        [Fact]
        public void ChooseOpponents_EmptyText_ReturnsTheGuestText()
        {
            string text = HistoryDisplay.ChooseOpponents(NoNames, GuestText);

            Assert.Equal(GuestText, text);
        }

        [Fact]
        public void ChooseOpponents_NullText_ReturnsTheGuestText()
        {
            string text = HistoryDisplay.ChooseOpponents(null, GuestText);

            Assert.Equal(GuestText, text);
        }

        [Fact]
        public void AddComputer_NoNames_ReturnsOnlyTheComputerText()
        {
            string text = HistoryDisplay.AddComputer(NoNames, ComputerText);

            Assert.Equal(ComputerText, text);
        }

        [Fact]
        public void AddComputer_NamesOfRegisteredRivals_ReturnsTheNamesFollowedByTheComputer()
        {
            string text = HistoryDisplay.AddComputer(OpponentNames, ComputerText);

            Assert.Equal(OpponentNames + ", " + ComputerText, text);
        }

        [Fact]
        public void HasRegisteredOpponents_NamesOfRegisteredRivals_ReturnsTrue()
        {
            bool hasOpponents = HistoryDisplay.HasRegisteredOpponents(OpponentNames);

            Assert.True(hasOpponents);
        }

        [Fact]
        public void HasRegisteredOpponents_OnlySpaces_ReturnsFalse()
        {
            bool hasOpponents = HistoryDisplay.HasRegisteredOpponents(OnlySpaces);

            Assert.False(hasOpponents);
        }
    }
}

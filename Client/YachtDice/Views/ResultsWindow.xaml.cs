using System;
using System.Windows;
using System.Windows.Controls;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Window that shows the final scoreboard and the winner when a game ends.
    /// </summary>
    public partial class ResultsWindow : Window
    {
        private const int MinOpponentScoreGap = 10;
        private const int MaxOpponentScoreGapExclusive = 60;
        private const int MinimumPossibleScore = 0;

        private readonly PlayerSession _session;

        /// <summary>
        /// Initializes the results window with the player and the score obtained.
        /// </summary>
        /// <param name="session">Session of the player who played the game.</param>
        /// <param name="playerScore">Final score obtained by the player.</param>
        public ResultsWindow(PlayerSession session, int playerScore)
        {
            InitializeComponent();
            _session = session;
            LanguageSwitcherControl.ReopenWindowFunc = () => new ResultsWindow(session, playerScore);

            WinnerTitleTextBlock.Text = string.Format(Strings.Results_TitleWinner, session.DisplayName);
            ScoreSubtitleTextBlock.Text = string.Format(Strings.Results_SubtitleScore, playerScore);

            // TODO: Simulated opponent data; it will come from the backend later.
            var random = new Random();
            int scoreGap = random.Next(MinOpponentScoreGap, MaxOpponentScoreGapExclusive);
            OpponentScoreTextBlock.Text = Math.Max(MinimumPossibleScore, playerScore - scoreGap).ToString();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }

        private async void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            button.IsEnabled = false;
            await LobbyNavigator.CreateRoomAndOpenAsync(this, _session);
            button.IsEnabled = true;
        }
    }
}

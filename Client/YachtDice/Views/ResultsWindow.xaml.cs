using System;
using System.Windows;
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

        private readonly string _playerName;

        /// <summary>
        /// Initializes the results window with the player and the score obtained.
        /// </summary>
        /// <param name="playerName">Name of the player who played the game.</param>
        /// <param name="playerScore">Final score obtained by the player.</param>
        public ResultsWindow(string playerName, int playerScore)
        {
            InitializeComponent();
            _playerName = playerName;
            LanguageSwitcherControl.ReopenWindowFunc = () => new ResultsWindow(playerName, playerScore);

            WinnerTitleTextBlock.Text = string.Format(Strings.Results_TitleWinner, playerName);
            ScoreSubtitleTextBlock.Text = string.Format(Strings.Results_SubtitleScore, playerScore);

            // TODO: Simulated opponent data; it will come from the backend later.
            var random = new Random();
            int scoreGap = random.Next(MinOpponentScoreGap, MaxOpponentScoreGapExclusive);
            OpponentScoreTextBlock.Text = Math.Max(MinimumPossibleScore, playerScore - scoreGap).ToString();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new GameWindow(_playerName);
            gameWindow.Show();
            this.Close();
        }
    }
}
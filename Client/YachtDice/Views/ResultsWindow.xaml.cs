using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YachtDice.Client.Logic.Game;
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
        private const int SecondPlace = 2;
        private const int ThirdPlace = 3;
        private const int SingleWinner = 1;

        private readonly PlayerSession _session;

        /// <summary>
        /// Initializes the results window with the final standings that the server calculated.
        /// </summary>
        /// <param name="session">Session of the player who played the game.</param>
        /// <param name="standings">Players ordered from the first place to the last one.</param>
        public ResultsWindow(PlayerSession session, IReadOnlyList<RankedStanding> standings)
        {
            InitializeComponent();
            _session = session;
            LanguageSwitcherControl.ReopenWindowFunc = () => new ResultsWindow(session, standings);

            ShowWinner(standings);
            StandingsItemsControl.ItemsSource = standings.Select(CreateRow).ToList();
        }

        private static StandingRow CreateRow(RankedStanding standing)
        {
            return new StandingRow
            {
                PlaceText = GetPlaceText(standing),
                Name = standing.Name,
                Total = standing.Total,
                IsWinner = standing.IsWinner
            };
        }

        private static string GetPlaceText(RankedStanding standing)
        {
            var medals = new Dictionary<int, string>
            {
                { StandingsBuilder.FirstRank, Strings.Global_SymbolMedalGold },
                { SecondPlace, Strings.Global_SymbolMedalSilver },
                { ThirdPlace, Strings.Global_SymbolMedalBronze }
            };
            string text;

            if (!medals.TryGetValue(standing.Rank, out text))
            {
                text = standing.Rank.ToString();
            }

            return text;
        }

        private void ShowWinner(IReadOnlyList<RankedStanding> standings)
        {
            List<RankedStanding> winners = standings.Where(standing => standing.IsWinner).ToList();

            if (winners.Count == SingleWinner)
            {
                WinnerTitleTextBlock.Text = string.Format(Strings.Results_TitleWinner, winners[0].Name);
            }
            else
            {
                WinnerTitleTextBlock.Text = Strings.Results_TitleTie;
            }

            ScoreSubtitleTextBlock.Text = string.Format(
                Strings.Results_SubtitleScore,
                winners.Select(winner => winner.Total).DefaultIfEmpty().Max());
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

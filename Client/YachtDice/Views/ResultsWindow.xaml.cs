using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using YachtDice.Client.Logic.Game;
using YachtDice.Client.Logic.Players;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window that shows the final scoreboard and the winner when a game ends. A registered player also sees
    /// the experience bar of the level filling with the experience earned in the game.
    /// </summary>
    public partial class ResultsWindow : Window
    {
        private const int SecondPlace = 2;
        private const int ThirdPlace = 3;
        private const int SingleWinner = 1;
        private const double ExperienceDelaySeconds = 0.5;
        private const double ExperienceFillSeconds = 1.4;

        private readonly PlayerSession _session;
        private readonly PlayerGateway _playerGateway = new PlayerGateway();
        private readonly AppLogger _logger = new AppLogger(typeof(ResultsWindow));
        private readonly int _experienceGained;

        /// <summary>
        /// Initializes the results window with the final standings that the server calculated.
        /// </summary>
        /// <param name="session">Session of the player who played the game.</param>
        /// <param name="standings">Players ordered from the first place to the last one.</param>
        /// <param name="playerId">Identifier of the local player in the game.</param>
        public ResultsWindow(PlayerSession session, IReadOnlyList<RankedStanding> standings, string playerId)
        {
            InitializeComponent();
            _session = session;
            _experienceGained = FindExperienceGained(standings, playerId);
            LanguageSwitcherControl.ReopenWindowFunc = () => new ResultsWindow(session, standings, playerId);

            ShowWinner(standings);
            StandingsItemsControl.ItemsSource = standings.Select(CreateRow).ToList();

            if (!session.IsGuest)
            {
                _ = GatewayCallRunner.RunAsync(this, _logger, ShowExperienceAsync);
            }
        }

        private static StandingRow CreateRow(RankedStanding standing)
        {
            return new StandingRow
            {
                PlaceText = GetPlaceText(standing),
                Name = standing.Name,
                Total = standing.Total,
                ExperienceText = string.Format(Strings.Results_ExperienceGainedFormat, standing.ExperienceGained),
                IsWinner = standing.IsWinner
            };
        }

        private static int FindExperienceGained(IReadOnlyList<RankedStanding> standings, string playerId)
        {
            Func<RankedStanding, bool> isLocalPlayer = standing => standing.PlayerId == playerId;

            return standings.Where(isLocalPlayer).Select(standing => standing.ExperienceGained).FirstOrDefault();
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

        private async Task ShowExperienceAsync()
        {
            PlayerProfile profile = await _playerGateway.GetProfileAsync(_session.DisplayName);
            int levelStart = profile.LevelStartExperience;
            int nextLevelStart = profile.NextLevelExperience;
            int experienceBefore = profile.Experience - _experienceGained;
            double startFraction = LevelProgress.GetFraction(experienceBefore, levelStart, nextLevelStart);
            double endFraction = LevelProgress.GetFraction(profile.Experience, levelStart, nextLevelStart);
            bool isNewLevel = LevelProgress.IsNewLevel(profile.Experience, _experienceGained, levelStart);

            LevelTextBlock.Text = string.Format(Strings.Results_LevelFormat, profile.Level);
            ExperienceProgressTextBlock.Text = string.Format(
                Strings.Results_ExperienceProgressFormat,
                profile.Experience,
                nextLevelStart);
            LevelUpTextBlock.Visibility = isNewLevel ? Visibility.Visible : Visibility.Collapsed;
            ExperiencePanel.Visibility = Visibility.Visible;

            AnimateExperienceBar(startFraction, endFraction);
        }

        private void AnimateExperienceBar(double startFraction, double endFraction)
        {
            var fill = new DoubleAnimation(startFraction, endFraction, TimeSpan.FromSeconds(ExperienceFillSeconds))
            {
                BeginTime = TimeSpan.FromSeconds(ExperienceDelaySeconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            ExperienceFillScale.ScaleX = startFraction;
            ExperienceFillScale.BeginAnimation(ScaleTransform.ScaleXProperty, fill);
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

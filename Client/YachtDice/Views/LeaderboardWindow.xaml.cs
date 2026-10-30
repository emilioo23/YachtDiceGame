using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YachtDice.Gateway;
using YachtDice.Resources;
using YachtDice.Utils;
using ServerLeaderboardEntry = YachtDice.PlayerServiceReference.LeaderboardEntry;

namespace YachtDice.Views
{
    /// <summary>
    /// Window that shows the ranking of the best players.
    /// </summary>
    public partial class LeaderboardWindow : Window
    {
        private const int TopRankGold = 1;
        private const int TopRankSilver = 2;
        private const int TopRankBronze = 3;
        private const int RankColumnWidth = 60;
        private const int StandardColumnWidth = 120;
        private const int MedalColumnIndex = 0;
        private const int NameColumnIndex = 1;
        private const int PointsColumnIndex = 2;
        private const int WinsColumnIndex = 3;
        private const int RowBottomMargin = 2;
        private const int MedalFontSize = 16;
        private const int MedalLeftPadding = 14;
        private const int CellFontSize = 13;
        private const int CellVerticalPadding = 10;
        private const string GoldMedal = "\U0001F947";
        private const string SilverMedal = "\U0001F948";
        private const string BronzeMedal = "\U0001F949";
        private const string PointsFormat = "N0";

        private readonly string _playerName;
        private readonly FriendGateway _friendGateway = new FriendGateway();
        private List<LeaderboardEntry> _ranking = new List<LeaderboardEntry>();

        /// <summary>
        /// Initializes the leaderboard window and starts loading the ranking.
        /// </summary>
        /// <param name="playerName">Name of the player who opens the leaderboard.</param>
        public LeaderboardWindow(string playerName)
        {
            InitializeComponent();
            _playerName = playerName;
            LanguageSwitcherControl.ReopenWindowFunc = () => new LeaderboardWindow(playerName);

            _ = LoadRankingAsync();
        }

        private void GlobalFilterButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalFilterButton.Style = (Style)FindResource(ResourceKeys.ActiveTabStyleKey);
            FriendsFilterButton.Style = (Style)FindResource(ResourceKeys.OutlineButtonStyleKey);
            RenderRanking(_ranking);
        }

        private async void FriendsFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FriendsFilterButton.Style = (Style)FindResource(ResourceKeys.ActiveTabStyleKey);
            GlobalFilterButton.Style = (Style)FindResource(ResourceKeys.OutlineButtonStyleKey);

            await ShowFriendsRankingAsync();
        }

        private void BackLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }

        private static string GetMedalForRank(int rank)
        {
            string medal = rank.ToString();

            if (rank == TopRankGold)
            {
                medal = GoldMedal;
            }
            else if (rank == TopRankSilver)
            {
                medal = SilverMedal;
            }
            else if (rank == TopRankBronze)
            {
                medal = BronzeMedal;
            }

            return medal;
        }

        private async Task LoadRankingAsync()
        {
            try
            {
                var gateway = new PlayerGateway();
                List<ServerLeaderboardEntry> leaderboardData = await gateway.GetLeaderboardAsync();
                var ranking = new List<LeaderboardEntry>();

                foreach (ServerLeaderboardEntry serverEntry in leaderboardData)
                {
                    ranking.Add(new LeaderboardEntry
                    {
                        Name = serverEntry.DisplayName,
                        Points = serverEntry.TotalPoints,
                        Wins = serverEntry.Wins
                    });
                }

                _ranking = ranking;
                RenderRanking(_ranking);
            }
            catch (GatewayException)
            {
                ShowDialogMessage(Strings.Leaderboard_ConnectionError);
            }
        }

        private async Task ShowFriendsRankingAsync()
        {
            try
            {
                List<string> friendNames = await _friendGateway.GetFriendsAsync(_playerName);
                var allowedNames = new HashSet<string>(friendNames, StringComparer.OrdinalIgnoreCase) { _playerName };

                RenderRanking(_ranking.Where(entry => allowedNames.Contains(entry.Name)).ToList());
            }
            catch (GatewayException)
            {
                ShowDialogMessage(Strings.Leaderboard_ConnectionError);
            }
        }

        private void ShowDialogMessage(string message)
        {
            var dialog = new CustomDialogWindow(new DialogContent { Message = message })
            {
                Owner = this
            };

            dialog.ShowDialog();
        }

        private void RenderRanking(List<LeaderboardEntry> ranking)
        {
            var currentRank = 0;
            var viewerRank = 0;

            RankingPanel.Children.Clear();

            foreach (LeaderboardEntry entry in ranking)
            {
                currentRank++;
                entry.Medal = GetMedalForRank(currentRank);
                AddRankingRow(entry);

                if (entry.Name == _playerName)
                {
                    viewerRank = currentRank;
                }
            }

            YourPositionTextBlock.Text = string.Format(Strings.Leaderboard_YourPosition, viewerRank);
        }

        private TextBlock CreateCellLabel(string text, Brush textBrush)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = CellFontSize,
                Foreground = textBrush,
                Padding = new Thickness(0, CellVerticalPadding, 0, CellVerticalPadding),
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void AddRankingRow(LeaderboardEntry entry)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, RowBottomMargin) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(RankColumnWidth) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(StandardColumnWidth) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(StandardColumnWidth) });

            var textBrush = (Brush)FindResource(ResourceKeys.TextBrushKey);
            var medalLabel = new TextBlock
            {
                Text = entry.Medal,
                FontSize = MedalFontSize,
                Padding = new Thickness(MedalLeftPadding, CellVerticalPadding, 0, CellVerticalPadding)
            };
            TextBlock nameLabel = CreateCellLabel(entry.Name, textBrush);
            nameLabel.FontWeight = FontWeights.SemiBold;
            TextBlock pointsLabel = CreateCellLabel(entry.Points.ToString(PointsFormat), textBrush);
            TextBlock winsLabel = CreateCellLabel(entry.Wins.ToString(), textBrush);

            Grid.SetColumn(medalLabel, MedalColumnIndex);
            Grid.SetColumn(nameLabel, NameColumnIndex);
            Grid.SetColumn(pointsLabel, PointsColumnIndex);
            Grid.SetColumn(winsLabel, WinsColumnIndex);

            row.Children.Add(medalLabel);
            row.Children.Add(nameLabel);
            row.Children.Add(pointsLabel);
            row.Children.Add(winsLabel);

            RankingPanel.Children.Add(row);
        }
    }
}

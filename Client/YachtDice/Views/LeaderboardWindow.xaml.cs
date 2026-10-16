using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YachtDice.Gateway;
using YachtDice.Resources;

namespace YachtDice.Views
{
    public class LeaderboardEntry
    {
        public string Medal { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int Wins { get; set; }
    }

    public partial class LeaderboardWindow : Window
    {
        private const int TopRankGold = 1;
        private const int TopRankSilver = 2;
        private const int TopRankBronze = 3;
        private const int RankColumnWidth = 60;
        private const int StandardColumnWidth = 120;
        private readonly string _playerName;

        public LeaderboardWindow(string playerName)
        {
            InitializeComponent();
            _playerName = playerName;
            LanguageSwitcherControl.ReopenWindowFunc = () => new LeaderboardWindow(playerName);

            _ = LoadRankingAsync();
        }

        private void GlobalFilterButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalFilterButton.Style = (Style)FindResource("TabButtonActive");
            FriendsFilterButton.Style = (Style)FindResource("BtnOutline");
        }

        private void FriendsFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FriendsFilterButton.Style = (Style)FindResource("TabButtonActive");
            GlobalFilterButton.Style = (Style)FindResource("BtnOutline");
        }

        private void BackLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }

        private async Task LoadRankingAsync()
        {
            try
            {
                var gateway = new PlayerGateway();
                var leaderboardData = await gateway.GetLeaderboardAsync();
                var ranking = new List<LeaderboardEntry>();

                foreach (var dto in leaderboardData)
                {
                    ranking.Add(new LeaderboardEntry
                    {
                        Name = dto.DisplayName,
                        Points = dto.TotalPoints,
                        Wins = dto.Wins
                    });
                }

                RenderRanking(ranking);
            }
            catch (GatewayException)
            {
                MessageBox.Show(Strings.Leaderboard_ConnectionError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenderRanking(List<LeaderboardEntry> ranking)
        {
            int currentRank = 0;
            int viewerRank = 0;

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

        private string GetMedalForRank(int rank)
        {
            if (rank == TopRankGold) return "🥇";
            if (rank == TopRankSilver) return "🥈";
            if (rank == TopRankBronze) return "🥉";

            return rank.ToString();
        }

        private void AddRankingRow(LeaderboardEntry entry)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 2) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(RankColumnWidth) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(StandardColumnWidth) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(StandardColumnWidth) });

            var textBrush = (Brush)FindResource("BrushText");
            var medalLabel = new TextBlock { Text = entry.Medal, FontSize = 16, Padding = new Thickness(14, 10, 0, 10) };
            var nameLabel = new TextBlock { Text = entry.Name, FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = textBrush, Padding = new Thickness(0, 10, 0, 10), VerticalAlignment = VerticalAlignment.Center };
            var pointsLabel = new TextBlock { Text = entry.Points.ToString("N0"), FontSize = 13, Foreground = textBrush, Padding = new Thickness(0, 10, 0, 10), VerticalAlignment = VerticalAlignment.Center };
            var winsLabel = new TextBlock { Text = entry.Wins.ToString(), FontSize = 13, Foreground = textBrush, Padding = new Thickness(0, 10, 0, 10), VerticalAlignment = VerticalAlignment.Center };

            Grid.SetColumn(medalLabel, 0);
            Grid.SetColumn(nameLabel, 1);
            Grid.SetColumn(pointsLabel, 2);
            Grid.SetColumn(winsLabel, 3);

            row.Children.Add(medalLabel);
            row.Children.Add(nameLabel);
            row.Children.Add(pointsLabel);
            row.Children.Add(winsLabel);

            RankingPanel.Children.Add(row);
        }
    }
}
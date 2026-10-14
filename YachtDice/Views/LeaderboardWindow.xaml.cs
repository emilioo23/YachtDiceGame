using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YachtDice.Resources;
using YachtDiceGame.Data;

namespace YachtDice.Views
{
    /// <summary>
    /// Represents a computed row of the global leaderboard, the result of
    /// grouping the game history by player.
    /// </summary>
    public class LeaderboardEntry
    {
        /// <summary>
        /// Gets or sets the medal or position of the player (e.g. 🥇, 🥈, 🥉).
        /// </summary>
        public string Medal { get; set; }
        /// <summary>
        /// Gets or sets the display name of the player in the ranking.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the current level of the player.
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// Gets or sets the total accumulated score.
        /// </summary>
        public int Points { get; set; }
        /// <summary>
        /// Gets or sets the number of games won.
        /// </summary>
        public int Wins { get; set; }
    }

    /// <summary>
    /// Window that shows the leaderboard table with the ranks and scores of the players.
    /// </summary>
    public partial class LeaderboardWindow : Window
    {
        private const int TopRankGold = 1;
        private const int TopRankSilver = 2;
        private const int TopRankBronze = 3;
        private const int RankColumnWidth = 60;
        private const int StandardColumnWidth = 120;
        private const int DefaultPlayerLevel = 0;
        private readonly string _playerName;

        /// <summary>
        /// Initializes the global leaderboard window for the given player.
        /// </summary>
        /// <param name="playerName">Name of the player who views the leaderboard.</param>
        public LeaderboardWindow(string playerName)
        {
            InitializeComponent();
            _playerName = playerName;
            LanguageSwitcherControl.ReopenWindowFunc = () => new LeaderboardWindow(playerName);

            _ = LoadRankingAsync(); // Asynchronous execution of the query
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
            using (var context = new YachtDiceContext())
            {
                var ranking = await context.GameHistoryPlayer
                    .GroupBy(entry => entry.PlayerId)
                    .Select(group => new
                    {
                        PlayerId = group.Key,
                        TotalScore = group.Sum(entry => entry.FinalScore),
                        TotalWins = group.Count(entry => entry.IsWinner == true)
                    })
                    .Join(context.Player,
                        stats => stats.PlayerId,
                        player => player.Id,
                        (stats, player) => new LeaderboardEntry
                        {
                            Name = player.DisplayName,
                            Level = player.Level ?? DefaultPlayerLevel,
                            Points = stats.TotalScore,
                            Wins = stats.TotalWins
                        })
                    .OrderByDescending(entry => entry.Points)
                    .ToListAsync();

                RenderRanking(ranking);
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
            if (rank == TopRankGold)
            {
                return "🥇";
            }

            if (rank == TopRankSilver)
            {
                return "🥈";
            }

            if (rank == TopRankBronze)
            {
                return "🥉";
            }

            return rank.ToString();
        }

        private void AddRankingRow(LeaderboardEntry entry)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 2) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(RankColumnWidth) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(StandardColumnWidth) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(StandardColumnWidth) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(StandardColumnWidth) });

            var textBrush = (Brush)FindResource("BrushText");
            var medalLabel = new TextBlock { Text = entry.Medal, FontSize = 16, Padding = new Thickness(14, 10, 0, 10) };
            var nameLabel = new TextBlock { Text = entry.Name, FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = textBrush, Padding = new Thickness(0, 10, 0, 10), VerticalAlignment = VerticalAlignment.Center };
            var levelLabel = new TextBlock { Text = entry.Level.ToString(), FontSize = 13, Foreground = textBrush, Padding = new Thickness(0, 10, 0, 10), VerticalAlignment = VerticalAlignment.Center };
            var pointsLabel = new TextBlock { Text = entry.Points.ToString("N0"), FontSize = 13, Foreground = textBrush, Padding = new Thickness(0, 10, 0, 10), VerticalAlignment = VerticalAlignment.Center };
            var winsLabel = new TextBlock { Text = entry.Wins.ToString(), FontSize = 13, Foreground = textBrush, Padding = new Thickness(0, 10, 0, 10), VerticalAlignment = VerticalAlignment.Center };

            Grid.SetColumn(medalLabel, 0);
            Grid.SetColumn(nameLabel, 1);
            Grid.SetColumn(levelLabel, 2);
            Grid.SetColumn(pointsLabel, 3);
            Grid.SetColumn(winsLabel, 4);

            row.Children.Add(medalLabel);
            row.Children.Add(nameLabel);
            row.Children.Add(levelLabel);
            row.Children.Add(pointsLabel);
            row.Children.Add(winsLabel);

            RankingPanel.Children.Add(row);
        }
    }
}
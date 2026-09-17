using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Modelo simple para mostrar datos de ejemplo en el marcador global.
    /// Mas adelante se reemplazara por datos reales del backend.
    /// </summary>
    public class LeaderboardEntry
    {
        public string Medal { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Points { get; set; }
        public int Wins { get; set; }
    }

    public partial class LeaderboardWindow : Window
    {
        private const int YourMockRank = 47;

        private readonly string _playerName;

        /// <summary>
        /// Inicializa la ventana de marcador global para el jugador indicado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador que consulta el marcador.</param>
        public LeaderboardWindow(string playerName)
        {
            InitializeComponent();
            _playerName = playerName;
            LanguageSwitcherControl.ReopenWindowFunc = () => new LeaderboardWindow(playerName);

            LoadSampleRanking();
            YourPositionTextBlock.Text = string.Format(Strings.Leaderboard_YourPosition, YourMockRank);
        }

        private void LoadSampleRanking()
        {
            var ranking = new List<LeaderboardEntry>
            {
                new LeaderboardEntry { Medal = "🥇", Name = "DiceKing99",  Level = 42, Points = 125400, Wins = 156 },
                new LeaderboardEntry { Medal = "🥈", Name = "YachtMaster", Level = 38, Points = 110200, Wins = 203 },
                new LeaderboardEntry { Medal = "🥉", Name = "SofiaR",      Level = 35, Points = 98700,  Wins = 141 },
            };

            foreach (var entry in ranking)
            {
                AddRankingRow(entry);
            }
        }

        private void AddRankingRow(LeaderboardEntry entry)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 2) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

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
    }
}
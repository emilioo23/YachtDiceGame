using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YachtDice.Data;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Representa una fila calculada del marcador global, resultado de
    /// agrupar el historial de partidas por jugador.
    /// </summary>
    public class LeaderboardEntry
    {
        public string Medal { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Points { get; set; }
        public int Wins { get; set; }
    }

    /// <summary>
    /// Ventana del marcador global, mostrando el ranking de jugadores
    /// segun el puntaje acumulado en partidas terminadas.
    /// </summary>
    public partial class LeaderboardWindow : Window
    {
        private const int TopRankGold = 1;
        private const int TopRankSilver = 2;
        private const int TopRankBronze = 3;

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

            LoadRanking();
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

        private void LoadRanking()
        {
            using (var context = new YachtDiceContext())
            {
                var ranking = context.GameHistoryPlayers
                    .GroupBy(entry => entry.PlayerId)
                    .Select(group => new
                    {
                        PlayerId = group.Key,
                        TotalScore = group.Sum(entry => entry.FinalScore),
                        TotalWins = group.Count(entry => entry.IsWinner == true)
                    })
                    .Join(context.Players,
                        stats => stats.PlayerId,
                        player => player.Id,
                        (stats, player) => new LeaderboardEntry
                        {
                            Name = player.DisplayName,
                            Level = player.Level ?? 0,
                            Points = stats.TotalScore,
                            Wins = stats.TotalWins
                        })
                    .OrderByDescending(entry => entry.Points)
                    .ToList();

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
    }
}
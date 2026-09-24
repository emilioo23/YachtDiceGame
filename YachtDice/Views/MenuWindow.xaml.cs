using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YachtDice.Resources;
using YachtDiceGame.Data;

namespace YachtDice.Views
{
    /// <summary>
    /// Modelo simple para mostrar datos de ejemplo en la lista de amigos.
    /// </summary>
    public class FriendItem
    {
        /// <summary>
        /// Nombre a mostrar del amigo.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Inicial del nombre para el avatar.
        /// </summary>
        public string Initial { get; set; }

        /// <summary>
        /// Estado actual de conexión o actividad.
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// Ventana principal de navegación del juego tras iniciar sesión.
    /// </summary>
    public partial class MenuWindow : Window
    {
        private const int DefaultPlayerLevel = 1;
        private const int EmptyLength = 0;
        private const int FirstCharacterIndex = 0;
        private const int MatchRoundMock = 5;
        private const int PercentageMultiplier = 100;
        private const int PlayerLevelMock = 3;
        private const int RoundDecimals = 0;
        private const int SingleCharacterLength = 1;

        /// <summary>
        /// Inicializa el menu principal para el jugador indicado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador que entra al menu.</param>
        public MenuWindow(string playerName)
        {
            InitializeComponent();

            PlayerNameTextBlock.Text = playerName;
            PlayerNameHeadingTextBlock.Text = playerName;

            if (playerName.Length > EmptyLength)
            {
                PlayerInitialTextBlock.Text = playerName.Substring(FirstCharacterIndex, SingleCharacterLength).ToUpper();
            }
            else
            {
                PlayerInitialTextBlock.Text = "J";
            }

            PlayerLevelTextBlock.Text = string.Format(Strings.MainMenu_PlayerLevel, PlayerLevelMock);
            LanguageSwitcherControl.ReopenWindowFunc = () => new MenuWindow(playerName);

            LoadSampleFriends();

            _ = LoadPlayerStatisticsAsync(playerName);
        }

        private async Task LoadPlayerStatisticsAsync(string playerName)
        {
            try
            {
                using (var context = new YachtDiceContext())
                {
                    // Corrección: Uso de la entidad 'Player' en PascalCase generada por EF
                    Player player = await context.Player.FirstOrDefaultAsync(p => p.DisplayName == playerName || p.Username == playerName);

                    if (player != null)
                    {
                        // Corrección: Uso de la entidad 'GameHistoryPlayer' en PascalCase generada por EF
                        List<GameHistoryPlayer> playerStatistics = await context.GameHistoryPlayer
                            .Where(g => g.PlayerId == player.Id)
                            .ToListAsync();

                        int totalGames = playerStatistics.Count;
                        int totalWins = playerStatistics.Count(g => g.IsWinner == true);
                        int bestScore = EmptyLength;

                        if (playerStatistics.Any())
                        {
                            bestScore = playerStatistics.Max(g => g.FinalScore);
                        }

                        double winRate = EmptyLength;

                        if (totalGames > EmptyLength)
                        {
                            winRate = ((double)totalWins / totalGames) * PercentageMultiplier;
                        }

                        StatGamesTextBlock.Text = totalGames.ToString();
                        StatWinsTextBlock.Text = totalWins.ToString();
                        StatBestScoreTextBlock.Text = bestScore.ToString();
                        StatWinRateTextBlock.Text = $"{Math.Round(winRate, RoundDecimals)}%";

                        int level = player.Level ?? DefaultPlayerLevel;
                        PlayerLevelTextBlock.Text = string.Format(Strings.MainMenu_PlayerLevel, level);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Fallo critico al intentar cargar las estadisticas de la base de datos.", exception);
            }
        }

        private void LoadSampleFriends()
        {
            var inMatchFriends = new List<FriendItem>
            {
                new FriendItem { Name = "SofiaR", Initial = "S", Status = string.Format(Strings.MainMenu_Friends_StatusInMatchRound, MatchRoundMock) },
                new FriendItem { Name = "DiegoL", Initial = "D", Status = Strings.MainMenu_Friends_StatusInLobby }
            };

            InMatchFriendsList.ItemsSource = inMatchFriends;

            var onlineFriends = new List<FriendItem>
            {
                new FriendItem { Name = "CarlosM",  Initial = "C", Status = Strings.MainMenu_Friends_StatusOnline },
                new FriendItem { Name = "AnaP",     Initial = "A", Status = Strings.MainMenu_Friends_StatusOnline },
                new FriendItem { Name = "RobertoK", Initial = "R", Status = Strings.MainMenu_Friends_StatusOnline }
            };

            OnlineFriendsList.ItemsSource = onlineFriends;
            OnlineCountBadgeTextBlock.Text = string.Format(Strings.MainMenu_Friends_OnlineCount, onlineFriends.Count);
        }

        private void PlayNavButton_Click(object sender, RoutedEventArgs e)
        {
            var lobbyWindow = new LobbyWindow(PlayerNameTextBlock.Text);
            lobbyWindow.Show();
            this.Close();
        }

        private void LobbyNavButton_Click(object sender, RoutedEventArgs e)
        {
            var lobbyWindow = new LobbyWindow(PlayerNameTextBlock.Text);
            lobbyWindow.Show();
            this.Close();
        }

        private void LeaderboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            var leaderboardWindow = new LeaderboardWindow(PlayerNameTextBlock.Text);
            leaderboardWindow.Show();
            this.Close();
        }

        private void SettingsNavButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(PlayerNameTextBlock.Text);
            settingsWindow.Show();
            this.Close();
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileWindow(PlayerNameTextBlock.Text);
            profileWindow.Show();
            this.Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            var lobbyWindow = new LobbyWindow(PlayerNameTextBlock.Text);
            lobbyWindow.Show();
            this.Close();
        }

        private void JoinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(JoinRoomCodeTextBox.Text))
            {
                MessageBox.Show(Strings.MainMenu_Play_ErrorRequiredRoomCode);
            }
            else
            {
                MessageBox.Show(string.Format(Strings.MainMenu_Play_JoiningRoomMessage, JoinRoomCodeTextBox.Text));
            }
        }
    }
}
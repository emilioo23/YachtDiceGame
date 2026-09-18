using System.Collections.Generic;
using System.Windows;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Modelo simple para mostrar datos de ejemplo en la lista de amigos.
    /// Mas adelante se reemplazara por datos reales del backend.
    /// </summary>
    public class FriendItem
    {
        public string Name { get; set; }
        public string Initial { get; set; }
        public string Status { get; set; }
    }

    public partial class MenuWindow : Window
    {
        private const int PlayerLevelMock = 3;

        /// <summary>
        /// Inicializa el menu principal para el jugador indicado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador que entra al menu.</param>
        public MenuWindow(string playerName)
        {
            InitializeComponent();
            PlayerNameTextBlock.Text = playerName;
            PlayerNameHeadingTextBlock.Text = playerName;
            PlayerInitialTextBlock.Text = playerName.Length > 0 ? playerName.Substring(0, 1).ToUpper() : "J";
            PlayerLevelTextBlock.Text = string.Format(Strings.MainMenu_PlayerLevel, PlayerLevelMock);
            LanguageSwitcherControl.ReopenWindowFunc = () => new MenuWindow(playerName);
            LoadSampleFriends();
        }

        private void LoadSampleFriends()
        {
            var inMatchFriends = new List<FriendItem>
            {
                new FriendItem { Name = "SofiaR", Initial = "S", Status = string.Format(Strings.MainMenu_Friends_StatusInMatchRound, 5) },
                new FriendItem { Name = "DiegoL", Initial = "D", Status = Strings.MainMenu_Friends_StatusInLobby },
            };
            InMatchFriendsList.ItemsSource = inMatchFriends;

            var onlineFriends = new List<FriendItem>
            {
                new FriendItem { Name = "CarlosM",  Initial = "C", Status = Strings.MainMenu_Friends_StatusOnline },
                new FriendItem { Name = "AnaP",     Initial = "A", Status = Strings.MainMenu_Friends_StatusOnline },
                new FriendItem { Name = "RobertoK", Initial = "R", Status = Strings.MainMenu_Friends_StatusOnline },
            };
            OnlineFriendsList.ItemsSource = onlineFriends;

            OnlineCountBadgeTextBlock.Text = string.Format(Strings.MainMenu_Friends_OnlineCount, onlineFriends.Count);
        }

        private void PlayNavButton_Click(object sender, RoutedEventArgs e)
        {
            // Ya estamos en esta vista; no se hace nada.
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
                return;
            }

            MessageBox.Show(string.Format(Strings.MainMenu_Play_JoiningRoomMessage, JoinRoomCodeTextBox.Text));
        }
    }
}
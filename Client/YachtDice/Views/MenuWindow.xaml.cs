using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using YachtDice.Gateway;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Main navigation window of the game after logging in.
    /// </summary>
    public partial class MenuWindow : Window
    {
        private const int DefaultPlayerLevel = 1;
        private const int FirstCharacterIndex = 0;
        private const int SampleMatchRound = 5;
        private const int SingleCharacterLength = 1;
        private const string PercentageFormat = "{0}%";
        private const string UnexpectedErrorLogMessage = "Unexpected exception while loading the player statistics.";

        private readonly AppLogger _logger = new AppLogger(typeof(MenuWindow));
        private readonly PlayerGateway _gateway = new PlayerGateway();

        /// <summary>
        /// Initializes the main menu for the given player.
        /// </summary>
        /// <param name="playerName">Name of the player who enters the menu.</param>
        public MenuWindow(string playerName)
        {
            InitializeComponent();

            PlayerNameTextBlock.Text = playerName;
            PlayerNameHeadingTextBlock.Text = playerName;

            if (!string.IsNullOrEmpty(playerName))
            {
                PlayerInitialTextBlock.Text = playerName.Substring(FirstCharacterIndex, SingleCharacterLength).ToUpper();
            }

            PlayerLevelTextBlock.Text = string.Format(Strings.MainMenu_PlayerLevel, DefaultPlayerLevel);
            LanguageSwitcherControl.ReopenWindowFunc = () => new MenuWindow(playerName);

            LoadSampleFriends();

            _ = LoadPlayerStatisticsAsync(playerName);
        }

        private async Task LoadPlayerStatisticsAsync(string playerName)
        {
            try
            {
                var request = new PlayerStatisticsRequestDto { PlayerName = playerName };
                PlayerStatisticsDto statistics = await _gateway.GetStatisticsAsync(request);

                ShowStatistics(statistics);
            }
            catch (GatewayException ex)
            {
                ShowDialogMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, UnexpectedErrorLogMessage);
                ShowDialogMessage(Strings.Dialogs_UnexpectedError);
            }
        }

        private void ShowStatistics(PlayerStatisticsDto statistics)
        {
            if (statistics.IsPlayerFound)
            {
                StatGamesTextBlock.Text = statistics.GamesPlayed.ToString();
                StatWinsTextBlock.Text = statistics.Wins.ToString();
                StatBestScoreTextBlock.Text = statistics.BestScore.ToString();
                StatWinRateTextBlock.Text = string.Format(PercentageFormat, statistics.WinRatePercentage);
                PlayerLevelTextBlock.Text = string.Format(Strings.MainMenu_PlayerLevel, statistics.Level);
            }
        }

        private void ShowDialogMessage(string message)
        {
            var dialog = new CustomDialogWindow(new DialogContentDto { Message = message })
            {
                Owner = this
            };

            dialog.ShowDialog();
        }

        private void LoadSampleFriends()
        {
            var inMatchFriends = new List<FriendItem>
            {
                new FriendItem
                {
                    Name = "SofiaR",
                    Initial = "S",
                    Status = string.Format(Strings.MainMenu_Friends_StatusInMatchRound, SampleMatchRound)
                },
                new FriendItem { Name = "DiegoL", Initial = "D", Status = Strings.MainMenu_Friends_StatusInLobby }
            };

            InMatchFriendsList.ItemsSource = inMatchFriends;

            var onlineFriends = new List<FriendItem>
            {
                new FriendItem { Name = "CarlosM", Initial = "C", Status = Strings.MainMenu_Friends_StatusOnline },
                new FriendItem { Name = "AnaP", Initial = "A", Status = Strings.MainMenu_Friends_StatusOnline },
                new FriendItem { Name = "RobertoK", Initial = "R", Status = Strings.MainMenu_Friends_StatusOnline }
            };

            OnlineFriendsList.ItemsSource = onlineFriends;
            OnlineCountBadgeTextBlock.Text = string.Format(Strings.MainMenu_Friends_OnlineCount, onlineFriends.Count);
        }

        private void PlayNavButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLobbyWindow();
        }

        private void LobbyNavButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLobbyWindow();
        }

        private void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLobbyWindow();
        }

        private void LeaderboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            var leaderboardWindow = new LeaderboardWindow(PlayerNameTextBlock.Text);
            leaderboardWindow.Show();
            Close();
        }

        private void SettingsNavButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(PlayerNameTextBlock.Text);
            settingsWindow.Show();
            Close();
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileWindow(PlayerNameTextBlock.Text);
            profileWindow.Show();
            Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void JoinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            string message = Strings.MainMenu_Play_ErrorRequiredRoomCode;

            if (!string.IsNullOrWhiteSpace(JoinRoomCodeTextBox.Text))
            {
                message = string.Format(Strings.MainMenu_Play_JoiningRoomMessage, JoinRoomCodeTextBox.Text);
            }

            ShowDialogMessage(message);
        }

        private void OpenLobbyWindow()
        {
            var lobbyWindow = new LobbyWindow(PlayerNameTextBlock.Text);
            lobbyWindow.Show();
            Close();
        }
    }
}
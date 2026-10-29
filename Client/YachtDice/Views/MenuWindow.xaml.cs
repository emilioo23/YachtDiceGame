using System;
using System.Collections.Generic;
using System.Linq;
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
        private const int SingleCharacterLength = 1;
        private const string PercentageFormat = "{0}%";
        private const string UnexpectedErrorLogMessage = "Unexpected exception while loading the player statistics.";
        private const string PresenceFailedLogMessage = "The player could not be connected to the presence service.";

        private readonly AppLogger _logger = new AppLogger(typeof(MenuWindow));
        private readonly PlayerGateway _gateway = new PlayerGateway();
        private readonly string _playerName;

        /// <summary>
        /// Initializes the main menu for the given player.
        /// </summary>
        /// <param name="playerName">Name of the player who enters the menu.</param>
        public MenuWindow(string playerName)
        {
            InitializeComponent();

            _playerName = playerName;
            PlayerNameTextBlock.Text = playerName;
            PlayerNameHeadingTextBlock.Text = playerName;

            if (!string.IsNullOrEmpty(playerName))
            {
                PlayerInitialTextBlock.Text = GetInitial(playerName);
            }

            PlayerLevelTextBlock.Text = string.Format(Strings.MainMenu_PlayerLevel, DefaultPlayerLevel);
            LanguageSwitcherControl.ReopenWindowFunc = () => new MenuWindow(playerName);

            ShowOnlinePlayers();
            PresenceGateway.Current.PlayersChanged += HandlePlayersChanged;
            Closed += (sender, args) => PresenceGateway.Current.PlayersChanged -= HandlePlayersChanged;

            _ = ConnectPresenceAsync(playerName);
            _ = LoadPlayerStatisticsAsync(playerName);
        }

        private static string GetInitial(string name)
        {
            return name.Substring(FirstCharacterIndex, SingleCharacterLength).ToUpper();
        }

        private static FriendItem CreateFriend(string name, string status)
        {
            return new FriendItem { Name = name, Initial = GetInitial(name), Status = status };
        }

        private async Task ConnectPresenceAsync(string playerName)
        {
            if (playerName != Strings.Auth_GuestDisplayName)
            {
                try
                {
                    await PresenceGateway.Current.ConnectAsync(playerName);
                }
                catch (GatewayException)
                {
                    _logger.Warning(PresenceFailedLogMessage);
                }
            }
        }

        private void HandlePlayersChanged()
        {
            Dispatcher.BeginInvoke(new Action(ShowOnlinePlayers));
        }

        private async Task LoadPlayerStatisticsAsync(string playerName)
        {
            try
            {
                PlayerStatistics statistics = await _gateway.GetStatisticsAsync(playerName);
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

        private void ShowStatistics(PlayerStatistics statistics)
        {
            if (statistics != null)
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
            var dialog = new CustomDialogWindow(new DialogContent { Message = message })
            {
                Owner = this
            };

            dialog.ShowDialog();
        }

        private void ShowOnlinePlayers()
        {
            List<FriendItem> onlineFriends = PresenceGateway.Current.GetOnlinePlayers()
                .Where(name => !string.Equals(name, _playerName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name)
                .Select(name => CreateFriend(name, Strings.MainMenu_Friends_StatusOnline))
                .ToList();

            InMatchFriendsList.ItemsSource = new List<FriendItem>();
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

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => PresenceGateway.Current.Disconnect());

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
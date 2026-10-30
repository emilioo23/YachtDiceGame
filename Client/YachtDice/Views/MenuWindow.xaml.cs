using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YachtDice.Gateway;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;
using FriendRequestResultCode = YachtDice.FriendServiceReference.FriendRequestResultCode;

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
        private const string FriendsFailedLogMessage = "The friends of the player could not be loaded.";

        private readonly AppLogger _logger = new AppLogger(typeof(MenuWindow));
        private readonly PlayerGateway _gateway = new PlayerGateway();
        private readonly FriendGateway _friendGateway = new FriendGateway();
        private readonly string _playerName;
        private List<string> _friendNames = new List<string>();

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
            PresenceGateway.Current.FriendsChanged += HandleFriendsChanged;
            Closed += HandleWindowClosed;

            _ = ConnectPresenceAsync(playerName);
            _ = LoadPlayerStatisticsAsync(playerName);
            _ = LoadFriendsAsync();
        }

        private bool IsGuest => _playerName == Strings.Auth_GuestDisplayName;

        private static string GetInitial(string name)
        {
            return name.Substring(FirstCharacterIndex, SingleCharacterLength).ToUpper();
        }

        private static FriendItem CreateFriend(string name, string status, bool isOnline)
        {
            return new FriendItem { Name = name, Initial = GetInitial(name), Status = status, IsOnline = isOnline };
        }

        private static List<FriendItem> CreateFriendItems(IEnumerable<string> names, string status, bool isOnline)
        {
            return names
                .OrderBy(name => name)
                .Select(name => CreateFriend(name, status, isOnline))
                .ToList();
        }

        private static string ReadTag(object sender)
        {
            return (string)((FrameworkElement)sender).Tag;
        }

        private static string GetRequestMessage(FriendRequestResultCode code)
        {
            string message;

            switch (code)
            {
                case FriendRequestResultCode.RequestSent:
                    message = Strings.Friends_Result_RequestSent;
                    break;
                case FriendRequestResultCode.FriendAdded:
                    message = Strings.Friends_Result_FriendAdded;
                    break;
                case FriendRequestResultCode.InvalidCode:
                    message = Strings.Friends_Result_InvalidCode;
                    break;
                case FriendRequestResultCode.CodeNotFound:
                    message = Strings.Friends_Result_CodeNotFound;
                    break;
                case FriendRequestResultCode.SelfRequest:
                    message = Strings.Friends_Result_SelfRequest;
                    break;
                case FriendRequestResultCode.AlreadyFriends:
                    message = Strings.Friends_Result_AlreadyFriends;
                    break;
                case FriendRequestResultCode.AlreadyRequested:
                    message = Strings.Friends_Result_AlreadyRequested;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        private async Task ConnectPresenceAsync(string playerName)
        {
            if (!IsGuest)
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

        private void HandleFriendsChanged()
        {
            Dispatcher.BeginInvoke(new Action(RefreshFriends));
        }

        private void HandleWindowClosed(object sender, EventArgs e)
        {
            PresenceGateway.Current.PlayersChanged -= HandlePlayersChanged;
            PresenceGateway.Current.FriendsChanged -= HandleFriendsChanged;
        }

        private void RefreshFriends()
        {
            _ = LoadFriendsAsync();
        }

        private async Task LoadFriendsAsync()
        {
            if (!IsGuest)
            {
                try
                {
                    _friendNames = await _friendGateway.GetFriendsAsync(_playerName);
                    List<string> requesterNames = await _friendGateway.GetRequestsAsync(_playerName);
                    OwnFriendCodeTextBox.Text = await _friendGateway.GetFriendCodeAsync(_playerName);

                    RequestsList.ItemsSource = CreateFriendItems(
                        requesterNames,
                        Strings.Friends_Status_WantsFriend,
                        false);
                    ShowOnlinePlayers();
                }
                catch (GatewayException)
                {
                    _logger.Warning(FriendsFailedLogMessage);
                }
            }
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

        private CustomDialogWindow CreateDialog(string message)
        {
            return new CustomDialogWindow(new DialogContent { Message = message })
            {
                Owner = this
            };
        }

        private void ShowDialogMessage(string message)
        {
            CreateDialog(message).ShowDialog();
        }

        private bool AskConfirmation(string message)
        {
            return CreateDialog(message).ShowDialog() == true;
        }

        private void ShowOnlinePlayers()
        {
            var onlineNames = new HashSet<string>(
                PresenceGateway.Current.OnlinePlayers,
                StringComparer.OrdinalIgnoreCase);
            List<FriendItem> onlineFriends = CreateFriendItems(
                _friendNames.Where(name => onlineNames.Contains(name)),
                Strings.MainMenu_Friends_StatusOnline,
                true);
            List<FriendItem> offlineFriends = CreateFriendItems(
                _friendNames.Where(name => !onlineNames.Contains(name)),
                Strings.MainMenu_Friends_StatusOffline,
                false);

            InMatchFriendsList.ItemsSource = new List<FriendItem>();
            OnlineFriendsList.ItemsSource = onlineFriends;
            OfflineFriendsList.ItemsSource = offlineFriends;
            string onlineCountText = string.Format(Strings.MainMenu_Friends_OnlineCount, onlineFriends.Count);
            OnlineCountBadgeTextBlock.Text = onlineCountText;
            OnlineHeaderTextBlock.Text = onlineCountText;
        }

        private async void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendFriendRequestAsync();
        }

        private async void AcceptRequestButton_Click(object sender, RoutedEventArgs e)
        {
            await RespondToRequestAsync(ReadTag(sender), true);
        }

        private async void RejectRequestButton_Click(object sender, RoutedEventArgs e)
        {
            await RespondToRequestAsync(ReadTag(sender), false);
        }

        private async void RemoveFriendButton_Click(object sender, RoutedEventArgs e)
        {
            string friendName = ReadTag(sender);

            if (AskConfirmation(Strings.Dialogs_D08_DeleteFriend))
            {
                await RemoveFriendAsync(friendName);
            }
        }

        private async Task SendFriendRequestAsync()
        {
            try
            {
                FriendRequestResultCode result =
                    await _friendGateway.SendRequestAsync(_playerName, FriendCodeTextBox.Text);

                ShowDialogMessage(GetRequestMessage(result));

                if (result == FriendRequestResultCode.RequestSent || result == FriendRequestResultCode.FriendAdded)
                {
                    FriendCodeTextBox.Clear();
                    await LoadFriendsAsync();
                }
            }
            catch (GatewayException ex)
            {
                ShowDialogMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async Task RespondToRequestAsync(string requesterName, bool isAccepted)
        {
            try
            {
                await _friendGateway.RespondAsync(_playerName, requesterName, isAccepted);
                await LoadFriendsAsync();
            }
            catch (GatewayException ex)
            {
                ShowDialogMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async Task RemoveFriendAsync(string friendName)
        {
            try
            {
                await _friendGateway.RemoveFriendAsync(_playerName, friendName);
                await LoadFriendsAsync();
            }
            catch (GatewayException ex)
            {
                ShowDialogMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
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
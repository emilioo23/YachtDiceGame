using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;
using FriendRequestAnswer = YachtDice.FriendServiceReference.FriendRequestAnswer;
using FriendRequestResultCode = YachtDice.FriendServiceReference.FriendRequestResultCode;

namespace YachtDice.Views
{
    /// <summary>
    /// Main navigation window of the game after logging in.
    /// </summary>
    public partial class MenuWindow : Window
    {
        private const int DefaultPlayerLevel = 1;
        private const string PresenceFailedLogMessage = "The player could not be connected to the presence service.";
        private const string FriendsFailedLogMessage = "The friends of the player could not be loaded.";
        private const string PendingMatchFailedLogMessage = "The pending match of the player could not be checked.";
        private const string AvatarFailedLogMessage = "The avatar of the player could not be loaded.";

        private readonly AppLogger _logger;
        private readonly PlayerGateway _gateway = new PlayerGateway();
        private readonly FriendGateway _friendGateway = new FriendGateway();
        private readonly PlayerSession _session;
        private readonly InvitationListener _invitationListener;
        private List<string> _friendNames = new List<string>();

        /// <summary>
        /// Initializes the main menu for the given player.
        /// </summary>
        /// <param name="session">Session of the player who enters the menu.</param>
        public MenuWindow(PlayerSession session)
        {
            InitializeComponent();

            _logger = new AppLogger(GetType());
            _session = session;
            PlayerNameTextBlock.Text = session.DisplayName;
            PlayerNameHeadingTextBlock.Text = GetGreetingName(session);
            PlayerInitialTextBlock.Text = PlayerInitials.GetInitial(session.DisplayName);
            PlayerLevelTextBlock.Text = string.Format(Strings.MainMenu_PlayerLevel, DefaultPlayerLevel);
            LanguageSwitcherControl.ReopenWindowFunc = () => new MenuWindow(session);

            _invitationListener = new InvitationListener(this, session);
            _invitationListener.Start();

            ShowOnlinePlayers();
            _session.Presence.PlayersChanged += HandlePlayersChanged;
            _session.Presence.FriendsChanged += HandleFriendsChanged;
            Closed += HandleWindowClosed;

            _ = ConnectPresenceAsync(session.DisplayName);
            _ = LoadPlayerStatisticsAsync(session.DisplayName);
            _ = LoadAvatarAsync(session.DisplayName);
            _ = LoadFriendsAsync();
            _ = OfferPendingMatchAsync();
        }

        private static string GetGreetingName(PlayerSession session)
        {
            return string.IsNullOrWhiteSpace(session.FirstName) ? session.DisplayName : session.FirstName;
        }

        private async Task OfferPendingMatchAsync()
        {
            if (!_session.IsGuest)
            {
                try
                {
                    string roomCode = await new RoomListGateway().FindPendingRoomAsync(_session.DisplayName);

                    await AskToReturnAsync(roomCode);
                }
                catch (GatewayException)
                {
                    _logger.Warning(PendingMatchFailedLogMessage);
                }
            }
        }

        private async Task AskToReturnAsync(string roomCode)
        {
            bool isNewOffer = roomCode.Length > 0 && roomCode != _session.LastOfferedMatchCode;

            if (isNewOffer)
            {
                _session.LastOfferedMatchCode = roomCode;

                if (CustomDialogWindow.Confirm(this, Strings.Menu_PendingMatchPrompt))
                {
                    await LobbyNavigator.JoinRoomAndOpenAsync(this, _session, roomCode);
                }
            }
        }

        private static string ReadTag(object sender)
        {
            return (string)((FrameworkElement)sender).Tag;
        }

        private async Task ConnectPresenceAsync(string playerName)
        {
            if (!_session.IsGuest)
            {
                try
                {
                    await _session.Presence.ConnectAsync(playerName);
                    ConnectionLossHandler.Watch(_session.Presence);
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
            _session.Presence.PlayersChanged -= HandlePlayersChanged;
            _session.Presence.FriendsChanged -= HandleFriendsChanged;
            _invitationListener.Stop();
        }

        private void RefreshFriends()
        {
            _ = LoadFriendsAsync();
        }

        private async Task LoadFriendsAsync()
        {
            if (!_session.IsGuest)
            {
                try
                {
                    _friendNames = await _friendGateway.GetFriendsAsync(_session.DisplayName);
                    List<string> requesterNames = await _friendGateway.GetRequestsAsync(_session.DisplayName);
                    OwnUsernameTextBox.Text = await _friendGateway.GetUsernameAsync(_session.DisplayName);

                    var requestPresence = new FriendPresence(Strings.Friends_Status_WantsFriend, false);

                    RequestsList.ItemsSource = FriendItemFactory.Create(requesterNames, requestPresence);
                    ShowOnlinePlayers();
                }
                catch (GatewayException)
                {
                    _logger.Warning(FriendsFailedLogMessage);
                }
            }
        }

        private Task LoadPlayerStatisticsAsync(string playerName)
        {
            return GatewayCallRunner.RunAsync(this, _logger, () => ShowPlayerStatisticsAsync(playerName));
        }

        private async Task ShowPlayerStatisticsAsync(string playerName)
        {
            PlayerStatistics statistics = await _gateway.GetStatisticsAsync(playerName);
            ShowStatistics(statistics);
        }

        private async Task LoadAvatarAsync(string playerName)
        {
            if (!_session.IsGuest)
            {
                try
                {
                    PlayerProfile profile = await _gateway.GetProfileAsync(playerName);

                    PlayerAvatarImage.Source = AvatarImages.Get(profile.AvatarId);
                    PlayerAvatarImage.Visibility = Visibility.Visible;
                    PlayerInitialTextBlock.Visibility = Visibility.Collapsed;
                }
                catch (GatewayException)
                {
                    _logger.Warning(AvatarFailedLogMessage);
                }
            }
        }

        private void ShowStatistics(PlayerStatistics statistics)
        {
            if (statistics != null)
            {
                StatGamesTextBlock.Text = statistics.GamesPlayed.ToString();
                StatWinsTextBlock.Text = statistics.Wins.ToString();
                StatBestScoreTextBlock.Text = statistics.BestScore.ToString();
                StatWinRateTextBlock.Text = string.Format(
                    Strings.Global_FormatPercentage,
                    statistics.WinRatePercentage);
                PlayerLevelTextBlock.Text = string.Format(Strings.MainMenu_PlayerLevel, statistics.Level);
            }
        }

        private void ShowOnlinePlayers()
        {
            List<string> onlineNames = _session.Presence.OnlinePlayers;
            List<FriendItem> onlineFriends = FriendItemFactory.CreateOnline(_friendNames, onlineNames);
            List<FriendItem> offlineFriends = FriendItemFactory.CreateOffline(_friendNames, onlineNames);

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

            if (CustomDialogWindow.Confirm(this, Strings.Dialogs_DeleteFriend))
            {
                await RemoveFriendAsync(friendName);
            }
        }

        private async Task SendFriendRequestAsync()
        {
            try
            {
                FriendRequestResultCode result =
                    await _friendGateway.SendRequestAsync(_session.DisplayName, FriendUsernameTextBox.Text);

                CustomDialogWindow.ShowMessage(this, FriendRequestMessages.GetMessage(result));

                if (result == FriendRequestResultCode.RequestSent)
                {
                    FriendUsernameTextBox.Clear();
                    await LoadFriendsAsync();
                }
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async Task RespondToRequestAsync(string requesterName, bool isAccepted)
        {
            try
            {
                var answer = new FriendRequestAnswer { RequesterName = requesterName, IsAccepted = isAccepted };

                await _friendGateway.RespondAsync(_session.DisplayName, answer);
                await LoadFriendsAsync();
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async Task RemoveFriendAsync(string friendName)
        {
            try
            {
                await _friendGateway.RemoveFriendAsync(_session.DisplayName, friendName);
                await LoadFriendsAsync();
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            button.IsEnabled = false;
            await LobbyNavigator.CreateRoomAndOpenAsync(this, _session);
            button.IsEnabled = true;
        }

        private void RoomsNavButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new RoomsWindow(_session));
        }

        private void LeaderboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new LeaderboardWindow(_session));
        }

        private void HowToPlayNavButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new HowToPlayWindow(_session));
        }

        private void SettingsNavButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new SettingsWindow(_session));
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new ProfileWindow(_session));
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomDialogWindow.Confirm(this, Strings.Dialogs_ConfirmLogout))
            {
                await Task.Run(() => _session.Presence.Disconnect());

                WindowNavigator.SwitchTo(this, new LoginWindow());
            }
        }

        private async void JoinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            string roomCode = JoinRoomCodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(roomCode))
            {
                CustomDialogWindow.ShowMessage(this, Strings.MainMenu_Play_ErrorRequiredRoomCode);
            }
            else
            {
                var button = (Button)sender;

                button.IsEnabled = false;
                await LobbyNavigator.JoinRoomAndOpenAsync(this, _session, roomCode);
                button.IsEnabled = true;
            }
        }
    }
}

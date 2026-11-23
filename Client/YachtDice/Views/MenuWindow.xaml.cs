using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using YachtDice.Client.Logic.Authentication;
using YachtDice.Client.Logic.Players;
using YachtDice.Client.Logic.Rooms;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;
using FriendActionResultCode = YachtDice.FriendServiceReference.FriendActionResultCode;
using FriendRequestAnswer = YachtDice.FriendServiceReference.FriendRequestAnswer;
using FriendRequestResultCode = YachtDice.FriendServiceReference.FriendRequestResultCode;
using ReceivedInvitation = YachtDice.FriendServiceReference.ReceivedInvitation;

namespace YachtDice.Views
{
    /// <summary>
    /// Main navigation window of the game after logging in.
    /// </summary>
    public partial class MenuWindow : Window
    {
        private const int DefaultPlayerLevel = 1;
        private const double LevelBarAnimationSeconds = 0.8;
        private const string NullSessionMessage = "The session of the player cannot be null.";

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
            JoinRoomCodeTextBox.MaxLength = RoomListItem.AccessCodeLength;
            OwnUsernameTextBox.MaxLength = RegistrationLimits.UsernameMaxLength;
            FriendUsernameTextBox.MaxLength = RegistrationLimits.UsernameMaxLength;

            _logger = new AppLogger(GetType());
            _session = session ?? throw new ArgumentNullException(nameof(session), NullSessionMessage);
            PlayerNameTextBlock.Text = session.DisplayName;
            EditProfileButton.IsEnabled = !session.IsGuest;
            PlayerNameHeadingTextBlock.Text = GetGreetingName(session);
            PlayerInitialTextBlock.Text = InitialLetter.From(session.DisplayName, Strings.Global_UnknownInitial);
            PlayerLevelTextBlock.Text = string.Format(Strings.MainMenu_PlayerLevel, DefaultPlayerLevel);
            LanguageSwitcherControl.ReopenWindowFunc = () => new MenuWindow(session);

            _invitationListener = new InvitationListener(this, session);
            _invitationListener.Start();

            ShowOnlinePlayers();
            _session.Presence.PlayersChanged += HandlePlayersChanged;
            _session.Presence.FriendsChanged += HandleFriendsChanged;
            Closed += HandleWindowClosed;

            _ = GatewayCallRunner.RunAsync(this, _logger, () => LoadPlayerDataAsync(session.DisplayName));
        }

        /// <summary>
        /// Loads what the menu shows about the player one call after the other, so if the server is not
        /// available the player is told only once.
        /// </summary>
        /// <param name="playerName">Name of the player who entered the menu.</param>
        /// <returns>A task that finishes when everything is loaded.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        private async Task LoadPlayerDataAsync(string playerName)
        {
            await ConnectPresenceAsync(playerName);
            await ShowPlayerStatisticsAsync(playerName);
            await LoadAvatarAsync(playerName);
            await new AchievementNotifier(_session).CheckAsync();
            await LoadFriendsAsync();
            await OfferPendingMatchAsync();
        }

        private static string GetGreetingName(PlayerSession session)
        {
            return string.IsNullOrWhiteSpace(session.FirstName) ? session.DisplayName : session.FirstName;
        }

        private async Task OfferPendingMatchAsync()
        {
            if (!_session.IsGuest)
            {
                string roomCode = await new RoomListGateway().FindPendingRoomAsync(_session.DisplayName);

                await AskToReturnAsync(roomCode);
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
                await _session.Presence.ConnectAsync(playerName);
                _session.Services.ConnectionLoss.Watch(_session.Presence);
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
            _ = GatewayCallRunner.RunAsync(this, _logger, LoadFriendsAsync);
        }

        private async Task LoadFriendsAsync()
        {
            if (!_session.IsGuest)
            {
                _friendNames = await _friendGateway.GetFriendsAsync(_session.DisplayName);
                List<string> requesterNames = await _friendGateway.GetRequestsAsync(_session.DisplayName);
                OwnUsernameTextBox.Text = await _friendGateway.GetUsernameAsync(_session.DisplayName);

                var requestPresence = new FriendPresence(Strings.Friends_Status_WantsFriend, false);

                RequestsList.ItemsSource = FriendItemFactory.Create(requesterNames, requestPresence);
                ShowOnlinePlayers();
                await LoadReceivedInvitationsAsync();
                await LoadBlockedPlayersAsync();
            }
        }

        private async Task LoadReceivedInvitationsAsync()
        {
            List<ReceivedInvitation> invitations =
                await _friendGateway.GetReceivedInvitationsAsync(_session.SessionToken);
            List<ReceivedInvitationRow> rows = invitations.Select(ToInvitationRow).ToList();

            ReceivedInvitationsList.ItemsSource = rows;
            InvitationsHeaderTextBlock.Visibility = rows.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private async Task LoadBlockedPlayersAsync()
        {
            List<string> blockedNames = await _friendGateway.GetBlockedPlayersAsync(_session.SessionToken);
            var blockedPresence = new FriendPresence(Strings.Friends_Status_Blocked, false);
            Visibility headerVisibility = Visibility.Visible;

            if (blockedNames.Count == 0)
            {
                headerVisibility = Visibility.Collapsed;
            }

            BlockedPlayersList.ItemsSource = FriendItemFactory.Create(blockedNames, blockedPresence);
            BlockedHeaderTextBlock.Visibility = headerVisibility;
        }

        private static ReceivedInvitationRow ToInvitationRow(ReceivedInvitation invitation)
        {
            return new ReceivedInvitationRow
            {
                SenderName = invitation.SenderName,
                Initial = InitialLetter.From(invitation.SenderName, Strings.Global_UnknownInitial),
                DetailText = SocialMessages.Describe(invitation),
                RoomCode = invitation.RoomCode,
                CanJoin = invitation.IsRoomOpen
            };
        }

        private async void JoinInvitationButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            button.IsEnabled = false;
            await LobbyNavigator.JoinRoomAndOpenAsync(this, _session, ReadTag(sender));
            button.IsEnabled = true;
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
                PlayerProfile profile = await _gateway.GetProfileAsync(playerName);

                AnimateLevelBar(LevelProgress.GetFraction(
                    profile.Experience,
                    profile.LevelStartExperience,
                    profile.NextLevelExperience));
                PlayerTitleTextBlock.Text = ProgressTexts.GetTitleName(profile.TitleId);
                ShowFlag(profile.FlagCode);
                PlayerAvatarImage.Source = AvatarImages.Get(profile.AvatarId);
                PlayerAvatarImage.Visibility = Visibility.Visible;
                PlayerInitialTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void AnimateLevelBar(double fraction)
        {
            var growth = new DoubleAnimation(
                LevelProgress.EmptyFraction,
                fraction,
                TimeSpan.FromSeconds(LevelBarAnimationSeconds))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            LevelBarScale.BeginAnimation(ScaleTransform.ScaleXProperty, growth);
        }

        private void ShowFlag(string flagCode)
        {
            bool hasFlag = FlagCodes.IsKnown(flagCode);

            PlayerFlagImage.Source = null;
            PlayerFlagImage.Visibility = Visibility.Collapsed;

            if (hasFlag)
            {
                PlayerFlagImage.Source = FlagImages.Get(flagCode);
                PlayerFlagImage.Visibility = Visibility.Visible;
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
                StatComputerTextBlock.Text = string.Format(
                    Strings.MainMenu_Play_StatComputer,
                    statistics.GamesAgainstComputer,
                    statistics.WinsAgainstComputer);
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

        private async void BlockFriendButton_Click(object sender, RoutedEventArgs e)
        {
            string friendName = ReadTag(sender);

            if (CustomDialogWindow.Confirm(this, string.Format(Strings.Dialogs_BlockPlayer, friendName)))
            {
                await BlockPlayerAsync(friendName);
            }
        }

        private async void UnblockPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            string playerName = ReadTag(sender);

            if (CustomDialogWindow.Confirm(this, string.Format(Strings.Dialogs_UnblockPlayer, playerName)))
            {
                await UnblockPlayerAsync(playerName);
            }
        }

        private async Task UnblockPlayerAsync(string playerName)
        {
            try
            {
                await _friendGateway.UnblockPlayerAsync(_session.SessionToken, playerName);
                await LoadFriendsAsync();
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async Task BlockPlayerAsync(string playerName)
        {
            try
            {
                await _friendGateway.BlockPlayerAsync(_session.SessionToken, playerName);
                await LoadFriendsAsync();
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async Task SendFriendRequestAsync()
        {
            try
            {
                FriendRequestResultCode result =
                    await _friendGateway.SendRequestAsync(_session.SessionToken, FriendUsernameTextBox.Text);

                CustomDialogWindow.ShowMessage(this, SocialMessages.GetMessage(result));

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

                FriendActionResultCode result = await _friendGateway.RespondAsync(_session.SessionToken, answer);

                if (result == FriendActionResultCode.FriendLimitReached)
                {
                    CustomDialogWindow.ShowMessage(this, Strings.Friends_Result_FriendLimitReached);
                }

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
                await _friendGateway.RemoveFriendAsync(_session.SessionToken, friendName);
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

        private void HistoryNavButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new HistoryWindow(_session));
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

                WindowNavigator.SwitchTo(this, new LoginWindow(_session.Services));
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

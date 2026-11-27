using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.Client.Logic.Logging;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Waiting room window where the players of a room get ready before a game starts. The host chooses the
    /// options of the room and starts the game; the other players see the changes as the server sends them.
    /// </summary>
    public partial class LobbyWindow : Window
    {
        private const int EmptyLength = 0;
        private const int MaxRoomNameLength = 30;
        private const double SelectedBorderThickness = 2.0;
        private const double UnselectedBorderThickness = 1.5;
        private const string DifficultyKeyPrefix = "Lobby_Difficulty_";
        private const string ComputerLabelFormat = "{0} \u00B7 {1}";

        private readonly RoomGateway _roomGateway;
        private readonly ChatGateway _chat;
        private readonly InvitationListener _invitationListener;

        private bool _isShowingRoom = true;
        private bool _isLocalPlayerReady;
        private bool _isLeavingOnClose = true;
        private bool _isOpeningGame;
        private bool _isClosed;

        /// <summary>
        /// Initializes the waiting room window of a room where the player already is.
        /// </summary>
        /// <param name="roomGateway">Connection of the player to the room, already inside it.</param>
        /// <param name="chat">Connection of the player to the chat of the room and whether it is available.</param>
        public LobbyWindow(RoomGateway roomGateway, ChatGateway chat)
        {
            InitializeComponent();
            RoomNameTextBox.MaxLength = MaxRoomNameLength;
            FillComputerDifficulties();

            _roomGateway = roomGateway ?? throw new ArgumentNullException(nameof(roomGateway),
                LogMessages.NullRoomGatewayMessage);
            _chat = chat ?? throw new ArgumentNullException(nameof(chat), LogMessages.NullChatMessage);
            LanguageSwitcherControl.ReopenWindowFunc = ReopenWindow;
            _roomGateway.RoomChanged += HandleRoomChanged;
            Closed += HandleWindowClosed;

            FriendsPanel.Attach(_roomGateway);
            _invitationListener = new InvitationListener(this, _roomGateway.Session)
            {
                CurrentRoomCode = _roomGateway.CurrentRoom.Code
            };
            _invitationListener.Start();

            StartChat();
            ShowRoom(_roomGateway.CurrentRoom);
        }

        private static List<LobbySlot> CreateSlots(RoomState room, string localPlayerId)
        {
            var slots = new List<LobbySlot>();
            bool isLocalPlayerHost = room.HostId == localPlayerId;

            foreach (RoomPlayer player in room.Players)
            {
                bool canManage = isLocalPlayerHost && player.Id != localPlayerId;

                slots.Add(CreateOccupiedSlot(player, player.Id == room.HostId, canManage));
            }

            for (int slotIndex = room.Players.Length; slotIndex < room.Settings.MaxPlayers; slotIndex++)
            {
                slots.Add(new LobbySlot { Name = string.Empty, IsEmpty = true, CanAddComputer = isLocalPlayerHost });
            }

            return slots;
        }

        private static LobbySlot CreateOccupiedSlot(RoomPlayer player, bool isHost, bool canManage)
        {
            var slot = new LobbySlot
            {
                PlayerId = player.Id,
                Name = player.DisplayName,
                AvatarId = player.AvatarId,
                IsHost = isHost,
                IsReady = player.IsReady && !isHost,
                CanManage = canManage,
                IsComputer = player.IsComputer,
                ComputerLabel = CreateComputerLabel(player),
                IsEmpty = false
            };

            return slot;
        }

        private static string GetDifficultyName(ComputerDifficulty difficulty)
        {
            return Strings.ResourceManager.GetString(DifficultyKeyPrefix + difficulty, Strings.Culture);
        }

        private static string CreateComputerLabel(RoomPlayer player)
        {
            string label = string.Empty;

            if (player.IsComputer)
            {
                label = string.Format(
                    CultureInfo.CurrentCulture,
                    ComputerLabelFormat,
                    Strings.Lobby_ComputerPlayer,
                    GetDifficultyName(player.Difficulty));
            }

            return label;
        }

        private void FillComputerDifficulties()
        {
            foreach (ComputerDifficulty difficulty in Enum.GetValues(typeof(ComputerDifficulty)))
            {
                var item = new ComboBoxItem { Content = GetDifficultyName(difficulty), Tag = difficulty };

                ComputerDifficultyComboBox.Items.Add(item);

                if (difficulty == ComputerDifficulty.Normal)
                {
                    ComputerDifficultyComboBox.SelectedItem = item;
                }
            }
        }

        private static string ReadTag(object sender)
        {
            return (string)((FrameworkElement)sender).Tag;
        }

        private static async Task<string> TryRunAsync(Func<Task<RoomResultCode>> operation)
        {
            string errorMessage = string.Empty;

            try
            {
                RoomResultCode result = await operation();

                if (result != RoomResultCode.Success)
                {
                    errorMessage = RoomResultMessages.GetMessage(result);
                }
            }
            catch (GatewayException ex)
            {
                errorMessage = GatewayErrorMessages.GetMessage(ex.ErrorCode);
            }

            return errorMessage;
        }

        private static int ParseMaxPlayers(ComboBoxItem item)
        {
            return int.Parse((string)item.Content, CultureInfo.InvariantCulture);
        }

        private Window ReopenWindow()
        {
            _isLeavingOnClose = false;

            return new LobbyWindow(_roomGateway, _chat);
        }

        private void StartChat()
        {
            ChatPanelControl.AddMessage(
                Strings.Game_Chat_SystemName,
                Strings.Lobby_Chat_WelcomeMessage,
                ChatMessageKind.System);
            ChatPanelControl.Attach(_chat, _roomGateway.PlayerId);
        }

        private void ShowRoom(RoomState room)
        {
            _isShowingRoom = true;

            RoomCodeTextBlock.Text = string.Format(Strings.Lobby_RoomCodeFormat, room.Code);
            PlayerCountTextBlock.Text = string.Format(
                Strings.Lobby_PlayerCountFormat,
                room.Players.Length,
                room.Settings.MaxPlayers);
            PlayerSlotsItemsControl.ItemsSource = CreateSlots(room, _roomGateway.PlayerId);

            ShowSettings(room.Settings);
            ApplyHostPermissions(room.HostId == _roomGateway.PlayerId);
            ShowReadyButton(room);

            _isShowingRoom = false;
        }

        private void ShowSettings(RoomSettings settings)
        {
            PrivacyComboBox.SelectedItem = settings.IsPrivate ? PrivateOptionItem : PublicOptionItem;
            SelectMaxPlayers(settings.MaxPlayers);

            if (!RoomNameTextBox.IsKeyboardFocused)
            {
                RoomNameTextBox.Text = settings.Name ?? string.Empty;
            }

            if (settings.Speed == GameSpeed.Fast)
            {
                SelectModeBorder(FastModeBorder, NormalModeBorder);
            }
            else
            {
                SelectModeBorder(NormalModeBorder, FastModeBorder);
            }
        }

        private void SelectMaxPlayers(int maxPlayers)
        {
            foreach (ComboBoxItem item in MaxPlayersComboBox.Items)
            {
                if (ParseMaxPlayers(item) == maxPlayers)
                {
                    MaxPlayersComboBox.SelectedItem = item;
                }
            }
        }

        private void ApplyHostPermissions(bool isHost)
        {
            PrivacyComboBox.IsEnabled = isHost;
            ComputerDifficultyPanel.Visibility = isHost ? Visibility.Visible : Visibility.Collapsed;
            RoomNameTextBox.IsReadOnly = !isHost;
            MaxPlayersComboBox.IsEnabled = isHost;
            NormalModeBorder.IsHitTestVisible = isHost;
            FastModeBorder.IsHitTestVisible = isHost;
            StartGameButton.IsEnabled = true;
            StartGameButton.Content = Strings.Lobby_ButtonStartMatch;
        }

        private void ShowReadyButton(RoomState room)
        {
            bool isHost = room.HostId == _roomGateway.PlayerId;
            Func<RoomPlayer, bool> isLocalPlayer = player => player.Id == _roomGateway.PlayerId;
            _isLocalPlayerReady = room.Players
                .Where(isLocalPlayer)
                .Select(player => player.IsReady)
                .FirstOrDefault();

            if (!isHost)
            {
                StartGameButton.Content = _isLocalPlayerReady
                    ? Strings.Lobby_ButtonNotReady
                    : Strings.Lobby_ButtonReady;
            }
        }

        private void SelectModeBorder(Border selected, Border unselected)
        {
            selected.Background = (SolidColorBrush)FindResource(ThemeResourceKeys.Cream2BrushKey);
            selected.BorderBrush = (SolidColorBrush)FindResource(ThemeResourceKeys.FeltLightBrushKey);
            selected.BorderThickness = new Thickness(SelectedBorderThickness);

            unselected.Background = Brushes.Transparent;
            unselected.BorderBrush = (SolidColorBrush)FindResource(ThemeResourceKeys.BorderBrushKey);
            unselected.BorderThickness = new Thickness(UnselectedBorderThickness);
        }

        private RoomSettings ReadSettings(GameSpeed speed)
        {
            var settings = new RoomSettings
            {
                IsPrivate = PrivacyComboBox.SelectedItem == PrivateOptionItem,
                MaxPlayers = ParseMaxPlayers((ComboBoxItem)MaxPlayersComboBox.SelectedItem),
                Speed = speed,
                Name = RoomNameTextBox.Text.Trim()
            };

            return settings;
        }

        private async Task SendSettingsAsync(RoomSettings settings)
        {
            string errorMessage = await TryRunAsync(() => _roomGateway.UpdateSettingsAsync(settings));

            if (errorMessage.Length > EmptyLength)
            {
                ShowRoom(_roomGateway.CurrentRoom);
                CustomDialogWindow.ShowMessage(this, errorMessage);
            }
        }

        private async Task RunHostActionAsync(Func<Task<RoomResultCode>> action)
        {
            string errorMessage = await TryRunAsync(action);

            if (errorMessage.Length > EmptyLength)
            {
                CustomDialogWindow.ShowMessage(this, errorMessage);
            }
        }

        private string FindPlayerName(string playerId)
        {
            Func<RoomPlayer, bool> isSameId = candidate => candidate.Id == playerId;
            RoomPlayer player = _roomGateway.CurrentRoom.Players.FirstOrDefault(isSameId);

            return player == null ? string.Empty : player.DisplayName;
        }

        private bool IsInside(RoomState room)
        {
            Func<RoomPlayer, bool> isSameId = player => player.Id == _roomGateway.PlayerId;

            return room.Players.Any(isSameId);
        }

        private void LeaveBecauseKicked()
        {
            CustomDialogWindow.ShowMessage(this, Strings.Lobby_KickedMessage);
            WindowNavigator.SwitchTo(this, new MenuWindow(_roomGateway.Session));
        }

        private void LeaveBecauseLeftOut()
        {
            CustomDialogWindow.ShowMessage(this, Strings.Lobby_LeftOutMessage);
            WindowNavigator.SwitchTo(this, new MenuWindow(_roomGateway.Session));
        }

        private Task<RoomResultCode> ToggleReadyAsync()
        {
            return _roomGateway.SetReadyAsync(!_isLocalPlayerReady);
        }

        private void LeaveBecauseClosed()
        {
            CustomDialogWindow.ShowMessage(this, Strings.Lobby_ClosedMessage);
            WindowNavigator.SwitchTo(this, new MenuWindow(_roomGateway.Session));
        }

        private void AnnounceHostChange(RoomState room)
        {
            Func<RoomPlayer, bool> isSameId = player => player.Id == room.HostId;
            RoomPlayer host = room.Players.First(isSameId);

            ChatPanelControl.AddMessage(
                Strings.Game_Chat_SystemName,
                string.Format(Strings.Lobby_Chat_NewHost, host.DisplayName),
                ChatMessageKind.System);
        }

        private void HandleRoomChanged(RoomState room, RoomEventType eventType)
        {
            Dispatcher.BeginInvoke(new Action(() => ApplyRoomChange(room, eventType)));
        }

        private void ApplyRoomChange(RoomState room, RoomEventType eventType)
        {
            if (eventType == RoomEventType.PlayerKicked && !IsInside(room))
            {
                LeaveBecauseKicked();
            }
            else if (eventType == RoomEventType.LeftOutOfGame)
            {
                LeaveBecauseLeftOut();
            }
            else if (eventType == RoomEventType.RoomClosed)
            {
                LeaveBecauseClosed();
            }
            else
            {
                ShowRoom(room);
                AnnounceChange(room, eventType);
            }
        }

        private void AnnounceChange(RoomState room, RoomEventType eventType)
        {
            if (eventType == RoomEventType.GameStarted)
            {
                OpenGameWindow();
            }
            else if (eventType == RoomEventType.HostChanged)
            {
                AnnounceHostChange(room);
            }
        }

        private void OpenGameWindow()
        {
            if (!_isOpeningGame && !_isClosed)
            {
                _isOpeningGame = true;
                _isLeavingOnClose = false;
                WindowNavigator.SwitchTo(this, new GameWindow(_roomGateway, _chat));
            }
        }

        private async void HandleWindowClosed(object sender, EventArgs e)
        {
            _isClosed = true;
            _roomGateway.RoomChanged -= HandleRoomChanged;
            ChatPanelControl.Detach();
            FriendsPanel.Detach();
            _invitationListener.Stop();

            if (_isLeavingOnClose)
            {
                await LobbyNavigator.LeaveRoomAsync(_chat, _roomGateway);
            }
        }

        private async void SettingsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isShowingRoom)
            {
                await SendSettingsAsync(ReadSettings(_roomGateway.CurrentRoom.Settings.Speed));
            }
        }

        private async void KickButton_Click(object sender, RoutedEventArgs e)
        {
            string targetPlayerId = ReadTag(sender);
            string question = string.Format(Strings.Lobby_ConfirmKick, FindPlayerName(targetPlayerId));

            if (CustomDialogWindow.Confirm(this, question))
            {
                await RunHostActionAsync(() => _roomGateway.KickPlayerAsync(targetPlayerId));
            }
        }

        private async void RemoveComputerButton_Click(object sender, RoutedEventArgs e)
        {
            string computerId = ReadTag(sender);

            await RunHostActionAsync(() => _roomGateway.RemoveComputerAsync(computerId));
        }

        private async void TransferButton_Click(object sender, RoutedEventArgs e)
        {
            string newHostId = ReadTag(sender);

            await RunHostActionAsync(() => _roomGateway.TransferHostAsync(newHostId));
        }

        private async void AddComputerButton_Click(object sender, RoutedEventArgs e)
        {
            var difficulty = (ComputerDifficulty)((ComboBoxItem)ComputerDifficultyComboBox.SelectedItem).Tag;

            await RunHostActionAsync(() => _roomGateway.AddComputerAsync(difficulty));
        }

        private async void NormalMode_Click(object sender, MouseButtonEventArgs e)
        {
            await SendSettingsAsync(ReadSettings(GameSpeed.Normal));
        }

        private async void FastMode_Click(object sender, MouseButtonEventArgs e)
        {
            await SendSettingsAsync(ReadSettings(GameSpeed.Fast));
        }

        private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_roomGateway.Session));
        }

        private void HowToPlayButton_Click(object sender, RoutedEventArgs e)
        {
            new HowToPlayWindow(this).ShowDialog();
        }

        private void CopyCodeButton_Click(object sender, RoutedEventArgs e)
        {
            RoomCodeCopier.Copy(this, _roomGateway.CurrentRoom.Code);
        }

        private async void RoomNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            await SaveRoomNameAsync();
        }

        private async void RoomNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SaveRoomNameAsync();
            }
        }

        private async Task SaveRoomNameAsync()
        {
            RoomSettings current = _roomGateway.CurrentRoom.Settings;
            bool isHost = _roomGateway.CurrentRoom.HostId == _roomGateway.PlayerId;
            bool hasChanged = !string.Equals(RoomNameTextBox.Text.Trim(), current.Name ?? string.Empty);

            if (isHost && hasChanged)
            {
                await SendSettingsAsync(ReadSettings(current.Speed));
            }
        }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            bool isHost = _roomGateway.CurrentRoom.HostId == _roomGateway.PlayerId;
            Func<Task<RoomResultCode>> startOrConfirm = isHost
                ? (Func<Task<RoomResultCode>>)_roomGateway.StartGameAsync
                : ToggleReadyAsync;

            StartGameButton.IsEnabled = false;

            string errorMessage = await TryRunAsync(startOrConfirm);

            if (errorMessage.Length > EmptyLength)
            {
                StartGameButton.IsEnabled = true;
                CustomDialogWindow.ShowMessage(this, errorMessage);
            }
        }
    }
}

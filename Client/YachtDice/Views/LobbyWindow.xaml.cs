using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private const double SelectedBorderThickness = 2.0;
        private const double UnselectedBorderThickness = 1.5;
        private const string ChatJoinFailedLogMessage = "The player could not join the chat of the room.";

        private readonly AppLogger _logger;
        private readonly RoomGateway _roomGateway;
        private readonly InvitationListener _invitationListener;

        // The window is created with this flag on because InitializeComponent raises SelectionChanged events
        // for the initial values of the combo boxes, and those events must not be sent to the server.
        private bool _isShowingRoom = true;
        private bool _isLeavingOnClose = true;
        private bool _isOpeningGame;
        private bool _isClosed;

        /// <summary>
        /// Initializes the waiting room window of a room where the player already is.
        /// </summary>
        /// <param name="roomGateway">Connection of the player to the room, already inside it.</param>
        public LobbyWindow(RoomGateway roomGateway)
        {
            InitializeComponent();

            _logger = new AppLogger(GetType());
            _roomGateway = roomGateway;
            LanguageSwitcherControl.ReopenWindowFunc = ReopenWindow;
            _roomGateway.RoomChanged += HandleRoomChanged;
            Closed += HandleWindowClosed;

            FriendsPanel.Attach(_roomGateway);
            _invitationListener = new InvitationListener(this, _roomGateway.Session)
            {
                CurrentRoomCode = _roomGateway.CurrentRoom.Code
            };
            _invitationListener.Start();

            ShowRoom(_roomGateway.CurrentRoom);
        }

        private static List<LobbySlot> CreateSlots(RoomState room)
        {
            var slots = new List<LobbySlot>();

            foreach (RoomPlayer player in room.Players)
            {
                slots.Add(CreateOccupiedSlot(player.DisplayName, player.Id == room.HostId));
            }

            for (int slotIndex = room.Players.Length; slotIndex < room.Settings.MaxPlayers; slotIndex++)
            {
                slots.Add(new LobbySlot { Name = string.Empty, Initial = string.Empty, IsEmpty = true });
            }

            return slots;
        }

        private static LobbySlot CreateOccupiedSlot(string playerName, bool isHost)
        {
            var slot = new LobbySlot
            {
                Name = playerName,
                Initial = PlayerInitials.GetInitial(playerName),
                IsHost = isHost,
                IsEmpty = false
            };

            return slot;
        }

        private static int ParseMaxPlayers(ComboBoxItem item)
        {
            return int.Parse((string)item.Content, CultureInfo.InvariantCulture);
        }

        private Window ReopenWindow()
        {
            _isLeavingOnClose = false;

            return new LobbyWindow(_roomGateway);
        }

        private void ShowRoom(RoomState room)
        {
            _isShowingRoom = true;

            RoomCodeTextBlock.Text = string.Format(Strings.Lobby_RoomCodeFormat, room.Code);
            PlayerCountTextBlock.Text = string.Format(
                Strings.Lobby_PlayerCountFormat,
                room.Players.Length,
                room.Settings.MaxPlayers);
            PlayerSlotsItemsControl.ItemsSource = CreateSlots(room);

            ShowSettings(room.Settings);
            ApplyHostPermissions(room.HostId == _roomGateway.PlayerId);

            _isShowingRoom = false;
        }

        private void ShowSettings(RoomSettings settings)
        {
            PrivacyComboBox.SelectedItem = settings.IsPrivate ? PrivateOptionItem : PublicOptionItem;
            SelectMaxPlayers(settings.MaxPlayers);

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
            MaxPlayersComboBox.IsEnabled = isHost;
            NormalModeBorder.IsHitTestVisible = isHost;
            FastModeBorder.IsHitTestVisible = isHost;
            StartGameButton.IsEnabled = isHost;
            StartGameButton.Content = isHost ? Strings.Lobby_ButtonStartMatch : Strings.Lobby_WaitingForHost;
        }

        private void SelectModeBorder(Border selected, Border unselected)
        {
            selected.Background = (SolidColorBrush)FindResource(ResourceKeys.Cream2BrushKey);
            selected.BorderBrush = (SolidColorBrush)FindResource(ResourceKeys.FeltLightBrushKey);
            selected.BorderThickness = new Thickness(SelectedBorderThickness);

            unselected.Background = Brushes.Transparent;
            unselected.BorderBrush = (SolidColorBrush)FindResource(ResourceKeys.BorderBrushKey);
            unselected.BorderThickness = new Thickness(UnselectedBorderThickness);
        }

        private RoomSettings ReadSettings(GameSpeed speed)
        {
            var settings = new RoomSettings
            {
                IsPrivate = PrivacyComboBox.SelectedItem == PrivateOptionItem,
                MaxPlayers = ParseMaxPlayers((ComboBoxItem)MaxPlayersComboBox.SelectedItem),
                Speed = speed
            };

            return settings;
        }

        private async Task SendSettingsAsync(RoomSettings settings)
        {
            string errorMessage = string.Empty;

            try
            {
                RoomResultCode result = await _roomGateway.UpdateSettingsAsync(settings);

                if (result != RoomResultCode.Success)
                {
                    errorMessage = RoomResultMessages.GetMessage(result);
                }
            }
            catch (GatewayException ex)
            {
                errorMessage = GatewayErrorMessages.GetMessage(ex.ErrorCode);
            }

            if (errorMessage.Length > EmptyLength)
            {
                ShowRoom(_roomGateway.CurrentRoom);
                CustomDialogWindow.ShowMessage(this, errorMessage);
            }
        }

        private async Task<string> TryStartGameAsync()
        {
            string errorMessage = string.Empty;

            try
            {
                RoomResultCode result = await _roomGateway.StartGameAsync();

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

        private void HandleRoomChanged(RoomState room, RoomEventType eventType)
        {
            Dispatcher.BeginInvoke(new Func<Task>(() => ApplyRoomChangeAsync(room, eventType)));
        }

        private async Task ApplyRoomChangeAsync(RoomState room, RoomEventType eventType)
        {
            ShowRoom(room);

            if (eventType == RoomEventType.GameStarted)
            {
                await OpenGameWindowAsync();
            }
        }

        private async Task OpenGameWindowAsync()
        {
            if (!_isOpeningGame)
            {
                _isOpeningGame = true;

                var chatGateway = new ChatGateway(_roomGateway.PlayerId, _roomGateway.CurrentRoom.Code);
                bool isChatAvailable = await TryJoinChatAsync(chatGateway);

                await ShowGameWindowAsync(chatGateway, isChatAvailable);
            }
        }

        private async Task<bool> TryJoinChatAsync(ChatGateway chatGateway)
        {
            bool isJoined = true;

            try
            {
                await chatGateway.JoinAsync();
            }
            catch (GatewayException)
            {
                _logger.Warning(ChatJoinFailedLogMessage);
                isJoined = false;
            }

            return isJoined;
        }

        private async Task ShowGameWindowAsync(ChatGateway chatGateway, bool isChatAvailable)
        {
            if (_isClosed)
            {
                await chatGateway.LeaveAsync();
            }
            else
            {
                _isLeavingOnClose = false;
                WindowNavigator.SwitchTo(this, new GameWindow(_roomGateway, chatGateway, isChatAvailable));
            }
        }

        private async void HandleWindowClosed(object sender, EventArgs e)
        {
            _isClosed = true;
            _roomGateway.RoomChanged -= HandleRoomChanged;
            FriendsPanel.Detach();
            _invitationListener.Stop();

            if (_isLeavingOnClose)
            {
                await _roomGateway.LeaveRoomAsync();
            }
        }

        private async void SettingsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isShowingRoom)
            {
                await SendSettingsAsync(ReadSettings(_roomGateway.CurrentRoom.Settings.Speed));
            }
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

        private void CopyCodeButton_Click(object sender, RoutedEventArgs e)
        {
            string roomCode = _roomGateway.CurrentRoom.Code;
            string message;

            try
            {
                Clipboard.SetText(roomCode);
                message = string.Format(Strings.Lobby_NotificationCodeCopied, roomCode);
            }
            catch (ExternalException)
            {
                message = Strings.Dialogs_UnexpectedError;
            }

            CustomDialogWindow.ShowMessage(this, message);
        }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            StartGameButton.IsEnabled = false;

            string errorMessage = await TryStartGameAsync();

            if (errorMessage.Length > EmptyLength)
            {
                StartGameButton.IsEnabled = true;
                CustomDialogWindow.ShowMessage(this, errorMessage);
            }
        }
    }
}

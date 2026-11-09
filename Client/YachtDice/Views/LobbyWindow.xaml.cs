using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        private readonly RoomGateway _roomGateway;
        private readonly ChatGateway _chatGateway;
        private readonly bool _isChatAvailable;
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
        /// <param name="chatGateway">Connection of the player to the chat of the room.</param>
        /// <param name="isChatAvailable">Whether the player could join the chat of the room.</param>
        public LobbyWindow(RoomGateway roomGateway, ChatGateway chatGateway, bool isChatAvailable)
        {
            InitializeComponent();

            _roomGateway = roomGateway;
            _chatGateway = chatGateway;
            _isChatAvailable = isChatAvailable;
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
                slots.Add(new LobbySlot { Name = string.Empty, Initial = string.Empty, IsEmpty = true });
            }

            return slots;
        }

        private static LobbySlot CreateOccupiedSlot(RoomPlayer player, bool isHost, bool canManage)
        {
            var slot = new LobbySlot
            {
                PlayerId = player.Id,
                Name = player.DisplayName,
                Initial = PlayerInitials.GetInitial(player.DisplayName),
                IsHost = isHost,
                CanManage = canManage,
                IsEmpty = false
            };

            return slot;
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

            return new LobbyWindow(_roomGateway, _chatGateway, _isChatAvailable);
        }

        private void StartChat()
        {
            ChatPanelControl.AddMessage(
                Strings.Game_Chat_SystemName,
                Strings.Lobby_Chat_WelcomeMessage,
                ChatMessageKind.System);
            ChatPanelControl.Attach(_chatGateway, _roomGateway.PlayerId, _isChatAvailable);
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
            RoomPlayer player = _roomGateway.CurrentRoom.Players.FirstOrDefault(candidate => candidate.Id == playerId);

            return player == null ? string.Empty : player.DisplayName;
        }

        private bool IsInside(RoomState room)
        {
            return room.Players.Any(player => player.Id == _roomGateway.PlayerId);
        }

        private void LeaveBecauseKicked()
        {
            CustomDialogWindow.ShowMessage(this, Strings.Lobby_KickedMessage);
            WindowNavigator.SwitchTo(this, new MenuWindow(_roomGateway.Session));
        }

        private void AnnounceHostChange(RoomState room)
        {
            RoomPlayer host = room.Players.First(player => player.Id == room.HostId);

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
                WindowNavigator.SwitchTo(this, new GameWindow(_roomGateway, _chatGateway, _isChatAvailable));
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
                await _chatGateway.LeaveAsync();
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

        private async void KickLink_Click(object sender, MouseButtonEventArgs e)
        {
            string targetPlayerId = ReadTag(sender);
            string question = string.Format(Strings.Lobby_ConfirmKick, FindPlayerName(targetPlayerId));

            if (CustomDialogWindow.Confirm(this, question))
            {
                await RunHostActionAsync(() => _roomGateway.KickPlayerAsync(targetPlayerId));
            }
        }

        private async void TransferLink_Click(object sender, MouseButtonEventArgs e)
        {
            string newHostId = ReadTag(sender);

            await RunHostActionAsync(() => _roomGateway.TransferHostAsync(newHostId));
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

            string errorMessage = await TryRunAsync(_roomGateway.StartGameAsync);

            if (errorMessage.Length > EmptyLength)
            {
                StartGameButton.IsEnabled = true;
                CustomDialogWindow.ShowMessage(this, errorMessage);
            }
        }
    }
}

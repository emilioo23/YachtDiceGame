using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YachtDice.FriendServiceReference;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// List of the friends of the player inside the waiting room. A right click on a friend who is online
    /// offers to invite the friend to the room. A guest has no friends, so the panel only says so.
    /// </summary>
    public partial class InviteFriendsPanel : UserControl
    {
        private readonly FriendGateway _friendGateway = new FriendGateway();
        private RoomGateway _roomGateway;
        private List<string> _friendNames = new List<string>();

        /// <summary>
        /// Initializes the panel without friends.
        /// </summary>
        public InviteFriendsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Connects the panel with the room where the player is and loads the friends of the player.
        /// </summary>
        /// <param name="roomGateway">Connection of the player to the room, already inside it.</param>
        public void Attach(RoomGateway roomGateway)
        {
            _roomGateway = roomGateway;

            if (roomGateway.Session.IsGuest)
            {
                ShowNotice(Strings.Lobby_Friends_GuestNotice);
            }
            else
            {
                roomGateway.Session.Presence.PlayersChanged += HandlePresenceChanged;
                roomGateway.Session.Presence.FriendsChanged += HandlePresenceChanged;
                _ = LoadFriendsAsync();
            }
        }

        /// <summary>
        /// Stops listening to the changes of the friends. The window calls it when it closes.
        /// </summary>
        public void Detach()
        {
            if (_roomGateway != null)
            {
                _roomGateway.Session.Presence.PlayersChanged -= HandlePresenceChanged;
                _roomGateway.Session.Presence.FriendsChanged -= HandlePresenceChanged;
            }
        }

        private void HandlePresenceChanged()
        {
            Dispatcher.BeginInvoke(new Func<Task>(LoadFriendsAsync));
        }

        private async Task LoadFriendsAsync()
        {
            try
            {
                _friendNames = await _friendGateway.GetFriendsAsync(_roomGateway.Session.DisplayName);
                ShowFriends();
            }
            catch (GatewayException ex)
            {
                ShowNotice(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private void ShowFriends()
        {
            List<string> onlineNames = _roomGateway.Session.Presence.OnlinePlayers;
            List<FriendItem> friends = FriendItemFactory.CreateOnline(_friendNames, onlineNames)
                .Concat(FriendItemFactory.CreateOffline(_friendNames, onlineNames))
                .ToList();

            FriendsItemsControl.ItemsSource = friends;

            if (friends.Any())
            {
                NoticeTextBlock.Visibility = Visibility.Collapsed;
                HintTextBlock.Visibility = Visibility.Visible;
                FriendsScrollViewer.Visibility = Visibility.Visible;
            }
            else
            {
                ShowNotice(Strings.Lobby_Friends_Empty);
            }
        }

        private void ShowNotice(string notice)
        {
            NoticeTextBlock.Text = notice;
            NoticeTextBlock.Visibility = Visibility.Visible;
            FriendsScrollViewer.Visibility = Visibility.Collapsed;
            HintTextBlock.Visibility = Visibility.Collapsed;
        }

        private async Task InviteAsync(string friendName)
        {
            string message;

            try
            {
                var sender = new RoomMember
                {
                    PlayerId = _roomGateway.PlayerId,
                    RoomCode = _roomGateway.CurrentRoom.Code
                };
                InvitationResultCode result = await _friendGateway.InviteToRoomAsync(sender, friendName);

                message = InvitationResultMessages.GetMessage(result, friendName);
            }
            catch (GatewayException ex)
            {
                message = GatewayErrorMessages.GetMessage(ex.ErrorCode);
            }

            CustomDialogWindow.ShowMessage(Window.GetWindow(this), message);
        }

        private async void InviteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await InviteAsync((string)((FrameworkElement)sender).Tag);
        }
    }
}

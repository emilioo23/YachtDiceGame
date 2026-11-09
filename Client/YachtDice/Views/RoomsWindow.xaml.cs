using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window that lists the rooms that wait for players, public and private. A public room accepts the player
    /// right away; a private room also asks for its code. The list updates itself every few seconds and the
    /// player can update it by hand.
    /// </summary>
    public partial class RoomsWindow : Window
    {
        private const int RefreshIntervalSeconds = 5;
        private const string AutoRefreshFailedLogMessage = "The list of rooms could not be updated.";

        private readonly AppLogger _logger;
        private readonly RoomListGateway _roomListGateway = new RoomListGateway();
        private readonly PlayerSession _session;
        private readonly InvitationListener _invitationListener;
        private readonly DispatcherTimer _refreshTimer;
        private List<RoomListItem> _rooms = new List<RoomListItem>();
        private bool _isRefreshing;
        private bool _isClosed;

        /// <summary>
        /// Initializes the window for the player and loads the list of rooms.
        /// </summary>
        /// <param name="session">Session of the player who looks for a room.</param>
        public RoomsWindow(PlayerSession session)
        {
            InitializeComponent();

            _logger = new AppLogger(GetType());
            _session = session;
            LanguageSwitcherControl.ReopenWindowFunc = () => new RoomsWindow(session);

            _invitationListener = new InvitationListener(this, session);
            _invitationListener.Start();

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(RefreshIntervalSeconds) };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            Closed += HandleWindowClosed;

            _ = RefreshAsync(true);
        }

        private static void KeepTypedCodes(IEnumerable<RoomListItem> oldRooms, IEnumerable<RoomListItem> newRooms)
        {
            Dictionary<string, string> codes = oldRooms.ToDictionary(room => room.RoomId, room => room.AccessCode);

            foreach (RoomListItem room in newRooms)
            {
                if (codes.TryGetValue(room.RoomId, out string code))
                {
                    room.AccessCode = code;
                }
            }
        }

        private static bool HasSameContent(List<RoomListItem> oldRooms, List<RoomListItem> newRooms)
        {
            return oldRooms.Count == newRooms.Count
                && oldRooms
                    .Zip(newRooms, (oldRoom, newRoom) => oldRoom.HasSameContentAs(newRoom))
                    .All(isSame => isSame);
        }

        private async Task RefreshAsync(bool isRequestedByPlayer)
        {
            if (!_isRefreshing)
            {
                _isRefreshing = true;

                try
                {
                    List<RoomSummary> summaries = await _roomListGateway.GetRoomsAsync();

                    ShowRooms(RoomListItemFactory.Create(summaries));
                }
                catch (GatewayException ex)
                {
                    ReportRefreshFailure(ex, isRequestedByPlayer);
                }
                finally
                {
                    _isRefreshing = false;
                }
            }
        }

        private void ReportRefreshFailure(GatewayException exception, bool isRequestedByPlayer)
        {
            if (isRequestedByPlayer && !_isClosed)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(exception.ErrorCode));
            }
            else
            {
                _logger.Warning(AutoRefreshFailedLogMessage);
            }
        }

        // The automatic update leaves the list as it is when nothing changed, so the code that the player is
        // typing in a row does not lose the focus.
        private void ShowRooms(List<RoomListItem> newRooms)
        {
            if (!_isClosed && !HasSameContent(_rooms, newRooms))
            {
                KeepTypedCodes(_rooms, newRooms);
                _rooms = newRooms;
                RoomsItemsControl.ItemsSource = _rooms;
            }

            EmptyTextBlock.Visibility = _rooms.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void HandleWindowClosed(object sender, EventArgs e)
        {
            _isClosed = true;
            _refreshTimer.Stop();
            _invitationListener.Stop();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            _ = RefreshAsync(false);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAsync(true);
        }

        private async void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var room = (RoomListItem)button.DataContext;

            button.IsEnabled = false;
            await LobbyNavigator.JoinListedRoomAndOpenAsync(this, _session, room);

            if (!_isClosed)
            {
                button.IsEnabled = true;
                await RefreshAsync(false);
            }
        }

        private void BackLink_Click(object sender, MouseButtonEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }
    }
}

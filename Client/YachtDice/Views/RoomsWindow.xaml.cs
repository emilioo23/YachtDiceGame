using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using YachtDice.Client.Logic.Rooms;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Resources;
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
        private List<RoomListItem> _allRooms = new List<RoomListItem>();
        private List<RoomListItem> _visibleRooms = new List<RoomListItem>();
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

            SearchTextBox.TextChanged += HandleFiltersChanged;
            PrivacyFilterComboBox.SelectionChanged += HandleFiltersChanged;

            _ = RefreshAsync(true);
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

        private RoomPrivacyFilter GetSelectedPrivacyFilter()
        {
            RoomPrivacyFilter filter = RoomPrivacyFilter.All;

            if (PrivacyFilterComboBox.SelectedItem == PublicFilterItem)
            {
                filter = RoomPrivacyFilter.Public;
            }
            else if (PrivacyFilterComboBox.SelectedItem == PrivateFilterItem)
            {
                filter = RoomPrivacyFilter.Private;
            }

            return filter;
        }

        private void ShowRooms(List<RoomListItem> newRooms)
        {
            if (!_isClosed && !RoomListUpdater.HasSameContent(_allRooms, newRooms))
            {
                RoomListUpdater.KeepTypedCodes(_allRooms, newRooms);
                _allRooms = newRooms;
                ApplyFilters();
            }

            UpdateEmptyNotice();
        }

        private void ApplyFilters()
        {
            _visibleRooms = RoomListItemFilter.Apply(_allRooms, GetSelectedPrivacyFilter(), SearchTextBox.Text);
            RoomsItemsControl.ItemsSource = _visibleRooms;
        }

        private void UpdateEmptyNotice()
        {
            EmptyTextBlock.Text = _allRooms.Any() ? Strings.Rooms_NoResults : Strings.Rooms_Empty;
            EmptyTextBlock.Visibility = _visibleRooms.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void HandleFiltersChanged(object sender, EventArgs e)
        {
            ApplyFilters();
            UpdateEmptyNotice();
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YachtDice.Client.Logic.Players;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;
using KickHistoryEntry = YachtDice.RoomServiceReference.KickHistoryEntry;

namespace YachtDice.Views
{
    /// <summary>
    /// Window that shows the latest games that the player played, with who won each one.
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private const string DateFormat = "g";

        private readonly PlayerSession _session;
        private readonly PlayerGateway _gateway = new PlayerGateway();
        private readonly AppLogger _logger = new AppLogger(typeof(HistoryWindow));

        /// <summary>
        /// Opens the history of the player and starts loading the games.
        /// </summary>
        /// <param name="session">Session of the player who opens the history.</param>
        public HistoryWindow(PlayerSession session)
        {
            InitializeComponent();
            _session = session;
            LanguageSwitcherControl.ReopenWindowFunc = () => new HistoryWindow(session);

            _ = GatewayCallRunner.RunAsync(this, _logger, LoadHistoryAsync);
        }

        private async Task LoadHistoryAsync()
        {
            List<GameHistoryEntry> games = await _gateway.GetGameHistoryAsync(_session.DisplayName);
            Func<GameHistoryEntry, HistoryRow> createRow = CreateRow;

            HistoryItemsControl.ItemsSource = games.Select(createRow).ToList();
            EmptyTextBlock.Visibility = games.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (!_session.IsGuest)
            {
                await LoadKicksAsync();
            }
        }

        private async Task LoadKicksAsync()
        {
            List<KickHistoryEntry> kicks = await new RoomListGateway().GetKickHistoryAsync(_session.SessionToken);
            Func<KickHistoryEntry, KickRow> createRow = CreateKickRow;

            KicksItemsControl.ItemsSource = kicks.Select(createRow).ToList();
            KicksTitleTextBlock.Visibility = kicks.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private KickRow CreateKickRow(KickHistoryEntry kick)
        {
            return new KickRow
            {
                Text = KickHistoryTexts.Describe(kick, _session.DisplayName),
                DateText = kick.Date.ToLocalTime().ToString(DateFormat)
            };
        }

        private static HistoryRow CreateRow(GameHistoryEntry game)
        {
            return new HistoryRow
            {
                IsWin = game.IsWin,
                ResultText = game.IsWin ? Strings.History_Victory : Strings.History_Defeat,
                OpponentsText = string.Format(Strings.History_VersusFormat, ReadOpponents(game)),
                ScoreText = ReadScore(game),
                DateText = game.PlayedAt.ToLocalTime().ToString(DateFormat)
            };
        }

        private static string ReadOpponents(GameHistoryEntry game)
        {
            return game.IsAgainstComputer
                ? HistoryDisplay.AddComputer(game.Opponents, Strings.History_ComputerOpponent)
                : HistoryDisplay.ChooseOpponents(game.Opponents, Strings.History_GuestOpponent);
        }

        private static string ReadScore(GameHistoryEntry game)
        {
            return HistoryDisplay.HasRegisteredOpponents(game.Opponents)
                ? string.Format(Strings.History_ScoreFormat, game.OwnScore, game.BestOpponentScore)
                : game.OwnScore.ToString();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }
    }
}

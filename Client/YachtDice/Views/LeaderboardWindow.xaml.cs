using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YachtDice.Client.Logic.Logging;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Resources;
using YachtDice.Utils;
using ServerLeaderboardEntry = YachtDice.PlayerServiceReference.LeaderboardEntry;

namespace YachtDice.Views
{
    /// <summary>
    /// Window that shows the ranking of the best players.
    /// </summary>
    public partial class LeaderboardWindow : Window
    {
        private const int TopRankGold = 1;
        private const int TopRankSilver = 2;
        private const int TopRankBronze = 3;
        private const int RankColumnWidth = 60;
        private const int LevelColumnWidth = 80;
        private const int PointsColumnWidth = 110;
        private const int WinsColumnWidth = 90;
        private const int GamesColumnWidth = 100;
        private const int RowBottomMargin = 2;
        private const int MedalFontSize = 16;
        private const int MedalLeftPadding = 14;
        private const int CellFontSize = 13;
        private const int CellVerticalPadding = 10;
        private const int NameColumnWeight = 1;
        private const int NoSpacing = 0;
        private const int NoRank = 0;
        private const int AvatarSize = 28;
        private const int AvatarGap = 8;
        private const string PointsFormat = "N0";
        private const string ViewerRowBrushKey = "BrushGoldButton";
        private const string ViewerTextBrushKey = "BrushFeltDark";

        private static readonly GridLength[] ColumnWidths =
        {
            new GridLength(RankColumnWidth),
            new GridLength(NameColumnWeight, GridUnitType.Star),
            new GridLength(LevelColumnWidth),
            new GridLength(PointsColumnWidth),
            new GridLength(WinsColumnWidth),
            new GridLength(GamesColumnWidth)
        };

        private readonly PlayerSession _session;
        private readonly FriendGateway _friendGateway = new FriendGateway();
        private readonly PlayerGateway _playerGateway = new PlayerGateway();
        private readonly AppLogger _logger = new AppLogger(typeof(LeaderboardWindow));
        private List<LeaderboardEntry> _ranking = new List<LeaderboardEntry>();

        /// <summary>
        /// Initializes the leaderboard window and starts loading the ranking.
        /// </summary>
        /// <param name="session">Session of the player who opens the leaderboard.</param>
        public LeaderboardWindow(PlayerSession session)
        {
            InitializeComponent();
            _session = session ?? throw new ArgumentNullException(nameof(session), LogMessages.NullSessionMessage);
            LanguageSwitcherControl.ReopenWindowFunc = () => new LeaderboardWindow(session);

            _ = GatewayCallRunner.RunAsync(this, _logger, LoadRankingAsync);
        }

        private void GlobalFilterButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalFilterButton.Style = (Style)FindResource(ThemeResourceKeys.ActiveTabStyleKey);
            FriendsFilterButton.Style = (Style)FindResource(ThemeResourceKeys.OutlineButtonStyleKey);
            RenderRanking(_ranking);
        }

        private async void FriendsFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FriendsFilterButton.Style = (Style)FindResource(ThemeResourceKeys.ActiveTabStyleKey);
            GlobalFilterButton.Style = (Style)FindResource(ThemeResourceKeys.OutlineButtonStyleKey);

            await GatewayCallRunner.RunAsync(this, _logger, ShowFriendsRankingAsync);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }

        private static string GetMedalForRank(int rank)
        {
            string medal = rank.ToString();

            if (rank == TopRankGold)
            {
                medal = Strings.Global_SymbolMedalGold;
            }
            else if (rank == TopRankSilver)
            {
                medal = Strings.Global_SymbolMedalSilver;
            }
            else if (rank == TopRankBronze)
            {
                medal = Strings.Global_SymbolMedalBronze;
            }

            return medal;
        }

        private static StackPanel CreateNameCell(LeaderboardEntry leaderboardEntry, TextBlock nameLabel)
        {
            var avatar = new Image
            {
                Source = AvatarImages.Get(leaderboardEntry.AvatarId),
                Width = AvatarSize,
                Height = AvatarSize,
                Margin = new Thickness(NoSpacing, NoSpacing, AvatarGap, NoSpacing)
            };
            var cell = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            cell.Children.Add(avatar);
            cell.Children.Add(nameLabel);

            return cell;
        }

        private static LeaderboardEntry CreateEntry(ServerLeaderboardEntry serverEntry)
        {
            return new LeaderboardEntry
            {
                Position = serverEntry.Position,
                Medal = GetMedalForRank(serverEntry.Position),
                Name = serverEntry.DisplayName,
                AvatarId = serverEntry.AvatarId,
                Points = serverEntry.TotalPoints,
                Wins = serverEntry.Wins,
                GamesPlayed = serverEntry.GamesPlayed,
                Level = serverEntry.Level
            };
        }

        private async Task LoadRankingAsync()
        {
            List<ServerLeaderboardEntry> leaderboardData =
                await _playerGateway.GetLeaderboardAsync(_session.DisplayName);

            _ranking = leaderboardData.Select(CreateEntry).ToList();
            RenderRanking(_ranking);
        }

        private async Task ShowFriendsRankingAsync()
        {
            List<string> friendNames = await _friendGateway.GetFriendsAsync(
                _session.SessionToken,
                _session.DisplayName);
            var allowedNames = new HashSet<string>(friendNames, StringComparer.OrdinalIgnoreCase)
            {
                _session.DisplayName
            };
            Func<LeaderboardEntry, bool> isAllowed = leaderboardEntry => allowedNames.Contains(leaderboardEntry.Name);

            RenderRanking(_ranking.Where(isAllowed).ToList());
        }

        private void RenderRanking(List<LeaderboardEntry> ranking)
        {
            int viewerPosition = NoRank;

            RankingPanel.Children.Clear();

            foreach (LeaderboardEntry leaderboardEntry in ranking)
            {
                bool isViewer = IsViewer(leaderboardEntry);

                AddRankingRow(leaderboardEntry, isViewer);

                if (isViewer)
                {
                    viewerPosition = leaderboardEntry.Position;
                }
            }

            YourPositionTextBlock.Text = string.Format(Strings.Leaderboard_YourPosition, viewerPosition);
        }

        private bool IsViewer(LeaderboardEntry leaderboardEntry)
        {
            return string.Equals(leaderboardEntry.Name, _session.DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        private TextBlock CreateCellLabel(string text, Brush textBrush)
        {
            var label = new TextBlock
            {
                Text = text,
                FontSize = CellFontSize,
                Foreground = textBrush,
                Padding = new Thickness(NoSpacing, CellVerticalPadding, NoSpacing, CellVerticalPadding),
                VerticalAlignment = VerticalAlignment.Center
            };

            return label;
        }

        private void AddRankingRow(LeaderboardEntry leaderboardEntry, bool isViewer)
        {
            string textBrushKey = isViewer ? ViewerTextBrushKey : ThemeResourceKeys.TextBrushKey;
            var textBrush = (Brush)FindResource(textBrushKey);
            var row = new Grid { Margin = new Thickness(NoSpacing, NoSpacing, NoSpacing, RowBottomMargin) };
            TextBlock nameLabel = CreateCellLabel(leaderboardEntry.Name, textBrush);

            nameLabel.FontWeight = FontWeights.SemiBold;

            if (isViewer)
            {
                row.Background = (Brush)FindResource(ViewerRowBrushKey);
            }

            var cells = new List<UIElement>
            {
                CreateMedalLabel(leaderboardEntry, textBrush),
                CreateNameCell(leaderboardEntry, nameLabel),
                CreateCellLabel(leaderboardEntry.Level.ToString(), textBrush),
                CreateCellLabel(leaderboardEntry.Points.ToString(PointsFormat), textBrush),
                CreateCellLabel(leaderboardEntry.Wins.ToString(), textBrush),
                CreateCellLabel(leaderboardEntry.GamesPlayed.ToString(), textBrush)
            };

            for (int column = 0; column < cells.Count; column++)
            {
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = ColumnWidths[column] });
                Grid.SetColumn(cells[column], column);
                row.Children.Add(cells[column]);
            }

            RankingPanel.Children.Add(row);
        }

        private TextBlock CreateMedalLabel(LeaderboardEntry leaderboardEntry, Brush textBrush)
        {
            return new TextBlock
            {
                Text = leaderboardEntry.Medal,
                FontSize = MedalFontSize,
                Foreground = textBrush,
                Padding = new Thickness(MedalLeftPadding, CellVerticalPadding, NoSpacing, CellVerticalPadding)
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.GameServiceReference;
using YachtDice.Models;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Draws the scorecard of the game: one row for each box and one column for each player. It draws the
    /// whole table each time the server sends a new state, so it never keeps points of its own.
    /// </summary>
    internal sealed class ScorecardGrid
    {
        private const int CategoryColumnWidth = 200;
        private const int PlayerColumnWeight = 1;
        private const int LabelPaddingHorizontal = 10;
        private const int LabelPaddingVertical = 8;
        private const int FontSizeSmall = 12;
        private const int FontSizeNormal = 13;
        private const int RowMarginBottom = 2;
        private const int NoPadding = 0;
        private const int FirstPlayerColumn = 1;
        private const double FilledRowOpacity = 0.75;
        private const double PreviewOpacity = 0.6;
        private const double NormalOpacity = 1;
        private const string ResourceBrushText = "BrushText";
        private const string ResourceBrushTextMuted = "BrushTextMuted";
        private const string ResourceBrushCream2 = "BrushCream2";
        private const string ResourceBrushFeltLight = "BrushFeltLight";
        private const string ResourceBrushWood = "BrushWood";

        private readonly Panel _panel;
        private readonly FrameworkElement _resources;

        /// <summary>
        /// Initializes the scorecard inside a panel.
        /// </summary>
        /// <param name="panel">Panel that receives the rows.</param>
        /// <param name="resources">Element that owns the brushes used by the drawing.</param>
        public ScorecardGrid(Panel panel, FrameworkElement resources)
        {
            _panel = panel;
            _resources = resources;
        }

        /// <summary>
        /// Draws the scorecard again with the state of the game.
        /// </summary>
        /// <param name="state">State of the game that the server sent.</param>
        /// <param name="columns">Players in the order of the columns; the local player goes first.</param>
        /// <param name="onCategoryClick">Action to run when the player clicks a box.</param>
        public void Render(GameState state, IReadOnlyList<MatchPlayer> columns, Action<ScoreCategory> onCategoryClick)
        {
            var context = new RenderContext(state, columns, onCategoryClick);

            _panel.Children.Clear();

            foreach (ScoreCategory category in Enum.GetValues(typeof(ScoreCategory)))
            {
                AddCategoryRow(category, context);
                AddMetaRowAfter(category, context);
            }

            AddMetaRow(Strings.Game_Category_YachtBonus, context, card => card.YachtBonus);
            AddMetaRow(Strings.Game_Scorecard_Total, context, card => card.Total);
        }

        private static PlayerScorecard FindCard(GameState state, string playerId)
        {
            return state.Players.FirstOrDefault(card => card.PlayerId == playerId) ?? new PlayerScorecard
            {
                Scores = new CategoryScore[] { }
            };
        }

        private static string GetPoints(PlayerScorecard card, ScoreCategory category)
        {
            CategoryScore entry = card.Scores.FirstOrDefault(score => score.Category == category);

            string points = string.Empty;

            if (entry != null && entry.IsFilled)
            {
                points = entry.Points.ToString();
            }

            return points;
        }

        private void AddMetaRowAfter(ScoreCategory category, RenderContext context)
        {
            if (category == ScoreCategory.Sixes)
            {
                AddMetaRow(Strings.Game_Category_Sum, context, card => card.UpperSubtotal);
                AddMetaRow(Strings.Game_Category_Bonus, context, card => card.UpperBonus);
            }
        }

        private void AddCategoryRow(ScoreCategory category, RenderContext context)
        {
            string title = CategoryNames.GetName(category);
            Grid row = CreateRow(title, context.Columns.Count, FindBrush(ResourceBrushCream2));
            var canPick = context.IsLocalTurn && context.FindOption(category) != null;
            var isFilledByLocal = false;

            for (var index = 0; index < context.Columns.Count; index++)
            {
                MatchPlayer player = context.Columns[index];
                var isFilled = GetPoints(FindCard(context.State, player.Id), category).Length > 0;

                isFilledByLocal |= player.IsLocalPlayer && isFilled;
                AddCell(row, CreateCell(GetCellText(player, category, context), player.IsLocalPlayer, canPick), index);
            }

            row.Opacity = isFilledByLocal ? FilledRowOpacity : NormalOpacity;
            row.Cursor = canPick ? Cursors.Hand : Cursors.Arrow;
            row.MouseLeftButtonUp += (sender, e) => context.OnCategoryClick(category);
            _panel.Children.Add(row);
        }

        private string GetCellText(MatchPlayer player, ScoreCategory category, RenderContext context)
        {
            string text = GetPoints(FindCard(context.State, player.Id), category);
            ScoreChoice option = context.FindOption(category);
            bool isLocalPlayerTurn = player.IsLocalPlayer && context.IsLocalTurn;
            bool isEmptyCell = text.Length == 0;
            bool canPreviewPoints = isEmptyCell && isLocalPlayerTurn && option != null;

            if (canPreviewPoints)
            {
                text = option.Points.ToString();
            }

            return text;
        }

        private void AddMetaRow(string label, RenderContext context, Func<PlayerScorecard, int> getValue)
        {
            Grid row = CreateRow(label, context.Columns.Count, FindBrush(ResourceBrushWood));

            ((TextBlock)row.Children[0]).Foreground = Brushes.White;

            for (var index = 0; index < context.Columns.Count; index++)
            {
                string text = getValue(FindCard(context.State, context.Columns[index].Id)).ToString();

                AddCell(row, CreateLabel(text, Brushes.White, FontWeights.Bold), index);
            }

            _panel.Children.Add(row);
        }

        private Grid CreateRow(string title, int playerCount, Brush background)
        {
            var row = new Grid
            {
                Background = background,
                Margin = new Thickness(NoPadding, NoPadding, NoPadding, RowMarginBottom)
            };

            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CategoryColumnWidth) });

            for (var index = 0; index < playerCount; index++)
            {
                row.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(PlayerColumnWeight, GridUnitType.Star)
                });
            }

            var label = new TextBlock
            {
                Text = title,
                FontSize = FontSizeSmall,
                FontWeight = FontWeights.SemiBold,
                Foreground = FindBrush(ResourceBrushText),
                Padding = new Thickness(LabelPaddingHorizontal, LabelPaddingVertical, NoPadding, LabelPaddingVertical),
                VerticalAlignment = VerticalAlignment.Center
            };

            row.Children.Add(label);

            return row;
        }

        private TextBlock CreateCell(string text, bool isLocalPlayer, bool isPreview)
        {
            Brush foreground = FindBrush(ResourceBrushTextMuted);
            FontWeight weight = FontWeights.Normal;

            if (isLocalPlayer)
            {
                foreground = FindBrush(ResourceBrushFeltLight);
                weight = FontWeights.Bold;
            }

            TextBlock cell = CreateLabel(text, foreground, weight);

            cell.Opacity = isLocalPlayer && isPreview ? PreviewOpacity : NormalOpacity;

            return cell;
        }

        private static TextBlock CreateLabel(string text, Brush foreground, FontWeight weight)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = FontSizeNormal,
                FontWeight = weight,
                Foreground = foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static void AddCell(Grid row, TextBlock cell, int playerIndex)
        {
            Grid.SetColumn(cell, playerIndex + FirstPlayerColumn);
            row.Children.Add(cell);
        }

        private Brush FindBrush(string resourceKey)
        {
            return (SolidColorBrush)_resources.FindResource(resourceKey);
        }

        /// <summary>
        /// Everything that a render needs to know about the state and the player who looks at it.
        /// </summary>
        private sealed class RenderContext
        {
            /// <summary>
            /// Initializes the context of one render.
            /// </summary>
            /// <param name="state">State of the game that the server sent.</param>
            /// <param name="columns">Players in the order of the columns.</param>
            /// <param name="onClick">Action to run when the player clicks a box.</param>
            public RenderContext(GameState state, IReadOnlyList<MatchPlayer> columns, Action<ScoreCategory> onClick)
            {
                State = state;
                Columns = columns;
                OnCategoryClick = onClick;
                IsLocalTurn = columns.Any(player => player.IsLocalPlayer && player.Id == state.CurrentPlayerId)
                    && state.Status == GameStatus.InProgress;
            }

            /// <summary>
            /// Gets the state of the game that the server sent.
            /// </summary>
            public GameState State { get; }

            /// <summary>
            /// Gets the players in the order of the columns.
            /// </summary>
            public IReadOnlyList<MatchPlayer> Columns { get; }

            /// <summary>
            /// Gets the action to run when the player clicks a box.
            /// </summary>
            public Action<ScoreCategory> OnCategoryClick { get; }

            /// <summary>
            /// Gets a value indicating whether it is the turn of the player who uses the window.
            /// </summary>
            public bool IsLocalTurn { get; }

            /// <summary>
            /// Finds the points that the current dice give in a box.
            /// </summary>
            /// <param name="category">Box to look for.</param>
            /// <returns>The option of the box, or nothing if the box cannot be used now.</returns>
            public ScoreChoice FindOption(ScoreCategory category)
            {
                return State.Options.FirstOrDefault(choice => choice.Category == category);
            }
        }
    }
}

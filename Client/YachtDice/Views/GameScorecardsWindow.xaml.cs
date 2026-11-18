using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using YachtDice.Models;
using YachtDice.PlayerServiceReference;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Dialog that shows the scorecards of every registered player of a past game.
    /// </summary>
    public partial class GameScorecardsWindow : Window
    {
        private const string NullValueMessage = "A value that the game scorecards window needs cannot be null.";

        /// <summary>
        /// Initializes the dialog with the scorecards that the server sent.
        /// </summary>
        /// <param name="scorecards">Scorecards of the game; it can be empty for old games.</param>
        public GameScorecardsWindow(IReadOnlyList<SavedScorecard> scorecards)
        {
            if (scorecards == null)
            {
                throw new ArgumentNullException(nameof(scorecards), NullValueMessage);
            }

            InitializeComponent();

            Func<SavedScorecard, ScorecardView> createView = CreateView;

            ScorecardsItemsControl.ItemsSource = scorecards.Select(createView).ToList();
            EmptyTextBlock.Visibility = scorecards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static ScorecardView CreateView(SavedScorecard scorecard)
        {
            return new ScorecardView
            {
                Title = SavedScorecardTexts.CreateTitle(scorecard),
                Lines = SavedScorecardTexts.CreateLines(scorecard)
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}

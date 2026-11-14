using System;
using System.Windows;
using System.Windows.Threading;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Small notice that appears in the top right corner of the screen when a player unlocks an achievement.
    /// </summary>
    public partial class AchievementToastWindow : Window
    {
        private const int VisibleSeconds = 5;
        private const double ScreenMargin = 8;
        private const double ToastSpacing = 96;

        private readonly DispatcherTimer _closeTimer = new DispatcherTimer();

        /// <summary>
        /// Prepares the notice for an achievement.
        /// </summary>
        /// <param name="achievementId">Identifier of the achievement that was unlocked.</param>
        /// <param name="position">Position of the notice in the pile, so several notices do not cover each other.</param>
        public AchievementToastWindow(int achievementId, int position)
        {
            InitializeComponent();

            HeaderTextBlock.Text = Strings.Achievements_Unlocked;
            NameTextBlock.Text = ProgressTexts.GetAchievementName(achievementId);
            DescriptionTextBlock.Text = ProgressTexts.GetAchievementDescription(achievementId);
            Left = SystemParameters.WorkArea.Right - Width - ScreenMargin;
            Top = SystemParameters.WorkArea.Top + ScreenMargin + (position * ToastSpacing);

            _closeTimer.Interval = TimeSpan.FromSeconds(VisibleSeconds);
            _closeTimer.Tick += HandleCloseTimerTick;
            Loaded += HandleLoaded;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            _closeTimer.Start();
        }

        private void HandleCloseTimerTick(object sender, EventArgs e)
        {
            _closeTimer.Stop();
            Close();
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Tells the player the most characters that a box accepts when the player tries to type past it. Without
    /// the notice the box ignores the extra keys and the player does not know why.
    /// </summary>
    public static class TextLimitNotice
    {
        private const int NoLimit = 0;
        private const int NoticeSeconds = 3;

        private static readonly ToolTip Notice = new ToolTip { Placement = PlacementMode.Bottom };
        private static readonly DispatcherTimer HideTimer = CreateHideTimer();

        /// <summary>
        /// Starts watching every text box and password box of the application.
        /// </summary>
        public static void Register()
        {
            EventManager.RegisterClassHandler(
                typeof(TextBox),
                UIElement.PreviewTextInputEvent,
                new TextCompositionEventHandler(HandleTextBoxInput));
            EventManager.RegisterClassHandler(
                typeof(PasswordBox),
                UIElement.PreviewTextInputEvent,
                new TextCompositionEventHandler(HandlePasswordBoxInput));
        }

        private static DispatcherTimer CreateHideTimer()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(NoticeSeconds) };

            timer.Tick += HandleHideTimerTick;

            return timer;
        }

        private static void HandleHideTimerTick(object sender, EventArgs e)
        {
            HideTimer.Stop();
            Notice.IsOpen = false;
        }

        private static void HandleTextBoxInput(object sender, TextCompositionEventArgs e)
        {
            var box = (TextBox)sender;
            bool hasLimit = box.MaxLength > NoLimit && box.SelectionLength == NoLimit;

            if (hasLimit && box.Text.Length >= box.MaxLength)
            {
                Show(box, box.MaxLength);
            }
        }

        private static void HandlePasswordBoxInput(object sender, TextCompositionEventArgs e)
        {
            var box = (PasswordBox)sender;

            if (box.MaxLength > NoLimit && box.Password.Length >= box.MaxLength)
            {
                Show(box, box.MaxLength);
            }
        }

        private static void Show(UIElement box, int maxLength)
        {
            Notice.Content = string.Format(Strings.Global_TextLimitFormat, maxLength);
            Notice.PlacementTarget = box;
            Notice.IsOpen = true;
            HideTimer.Stop();
            HideTimer.Start();
        }
    }
}

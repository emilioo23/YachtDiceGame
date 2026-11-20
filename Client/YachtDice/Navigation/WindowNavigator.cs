using System.Windows;

namespace YachtDice.Navigation
{
    /// <summary>
    /// Changes the window that the player sees.
    /// </summary>
    public static class WindowNavigator
    {
        /// <summary>
        /// Opens a window and closes the one that was open.
        /// </summary>
        /// <param name="currentWindow">Window that is open now.</param>
        /// <param name="nextWindow">Window to open.</param>
        public static void SwitchTo(Window currentWindow, Window nextWindow)
        {
            nextWindow.Show();
            Close(currentWindow);
        }

        /// <summary>
        /// Closes a window because the program decided it, so a window that asks the player before closing
        /// does not ask.
        /// </summary>
        /// <param name="window">Window to close.</param>
        public static void Close(Window window)
        {
            (window as IClosePromptingWindow)?.AllowClose();
            window.Close();
        }
    }
}

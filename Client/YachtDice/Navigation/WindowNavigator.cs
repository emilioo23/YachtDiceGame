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
            currentWindow.Close();
        }
    }
}

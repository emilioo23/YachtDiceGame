using System;
using System.Linq;
using System.Windows;
using YachtDice.Client.Logic.Connection;
using YachtDice.Gateway;
using YachtDice.Resources;
using YachtDice.Utils;
using YachtDice.Views;

namespace YachtDice.Navigation
{
    /// <summary>
    /// Tells the player that the connection with the server was lost and takes them back to the login window,
    /// without waiting for the server. Several connections can break together, but the player is told once.
    /// </summary>
    public static class ConnectionLossHandler
    {
        private static readonly ConnectionLossGate Gate = new ConnectionLossGate();

        /// <summary>
        /// Starts to listen to a connection. When it is lost, the player goes back to the login window. Listening
        /// twice to the same connection has no extra effect.
        /// </summary>
        /// <param name="source">Connection to listen to.</param>
        public static void Watch(IConnectionLossSource source)
        {
            source.ConnectionLost -= HandleConnectionLost;
            source.ConnectionLost += HandleConnectionLost;
        }

        private static void HandleConnectionLost()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(ReturnToLogin));
        }

        private static void ReturnToLogin()
        {
            Window[] openWindows = Application.Current.Windows.OfType<Window>().ToArray();

            if (openWindows.Any() && Gate.TryEnter())
            {
                Window activeWindow = openWindows.FirstOrDefault(window => window.IsActive) ?? openWindows.Last();

                CustomDialogWindow.ShowMessage(activeWindow, Strings.Dialogs_ConnectionLost);
                new LoginWindow().Show();
                CloseWindows(openWindows);
                Gate.Release();
            }
        }

        private static void CloseWindows(Window[] windows)
        {
            foreach (Window window in windows)
            {
                window.Close();
            }
        }
    }
}

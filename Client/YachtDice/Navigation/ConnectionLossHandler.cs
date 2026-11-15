using System;
using System.Linq;
using System.Threading.Tasks;
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
    /// While a match is open, the window can register a recovery: the player goes back to the login window
    /// only if the recovery does not get the connection back.
    /// </summary>
    public static class ConnectionLossHandler
    {
        private static readonly ConnectionLossGate Gate = new ConnectionLossGate();
        private static readonly ConnectionLossGate RecoveryGate = new ConnectionLossGate();

        private static Func<Task<bool>> _recovery;

        /// <summary>
        /// Registers what to do when the connection is lost before sending the player to the login window.
        /// Only one recovery can be registered, the one of the window that is open.
        /// </summary>
        /// <param name="recovery">Tries to get the connection back; it tells whether it worked.</param>
        public static void UseRecovery(Func<Task<bool>> recovery)
        {
            _recovery = recovery;
        }

        /// <summary>
        /// Removes the recovery, unless another window already registered its own one.
        /// </summary>
        /// <param name="recovery">The recovery that the window registered before.</param>
        public static void ReleaseRecovery(Func<Task<bool>> recovery)
        {
            if (_recovery == recovery)
            {
                _recovery = null;
            }
        }

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
            Application.Current.Dispatcher.BeginInvoke(new Func<Task>(HandleLossAsync));
        }

        private static async Task HandleLossAsync()
        {
            Func<Task<bool>> recovery = _recovery;

            if (recovery == null)
            {
                ReturnToLogin();
            }
            else if (RecoveryGate.TryEnter())
            {
                var isRecovered = false;

                try
                {
                    isRecovered = await recovery();
                }
                finally
                {
                    RecoveryGate.Release();

                    if (!isRecovered)
                    {
                        ReturnToLogin();
                    }
                }
            }
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
                WindowNavigator.Close(window);
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YachtDice.Client.Logic.Connection;
using YachtDice.Client.Logic.Logging;
using YachtDice.Composition;
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
    /// only if the recovery does not get the connection back. The application has one instance, shared by the
    /// windows through <see cref="ClientServices"/>.
    /// </summary>
    public sealed class ConnectionLossHandler
    {
        private readonly ConnectionLossGate _gate = new ConnectionLossGate();
        private readonly ConnectionLossGate _recoveryGate = new ConnectionLossGate();
        private readonly ClientServices _services;
        private Func<Task<bool>> _recovery;

        /// <summary>
        /// Initializes the handler of the lost connections.
        /// </summary>
        /// <param name="services">Shared components that the login window needs when the player goes back.</param>
        public ConnectionLossHandler(ClientServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services), LogMessages.NullServicesMessage);
        }

        /// <summary>
        /// Registers what to do when the connection is lost before sending the player to the login window.
        /// Only one recovery can be registered, the one of the window that is open.
        /// </summary>
        /// <param name="recovery">Tries to get the connection back; it tells whether it worked.</param>
        public void UseRecovery(Func<Task<bool>> recovery)
        {
            _recovery = recovery;
        }

        /// <summary>
        /// Removes the recovery, unless another window already registered its own one.
        /// </summary>
        /// <param name="recovery">The recovery that the window registered before.</param>
        public void ReleaseRecovery(Func<Task<bool>> recovery)
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
        public void Watch(IConnectionLossSource source)
        {
            source.ConnectionLost -= HandleConnectionLost;
            source.ConnectionLost += HandleConnectionLost;
        }

        private void HandleConnectionLost()
        {
            Application.Current.Dispatcher.BeginInvoke(new Func<Task>(HandleLossAsync));
        }

        private async Task HandleLossAsync()
        {
            Func<Task<bool>> recovery = _recovery;

            if (recovery == null)
            {
                ReturnToLogin();
            }
            else if (_recoveryGate.TryEnter())
            {
                var isRecovered = false;

                try
                {
                    isRecovered = await recovery();
                }
                finally
                {
                    _recoveryGate.Release();

                    if (!isRecovered)
                    {
                        ReturnToLogin();
                    }
                }
            }
        }

        private void ReturnToLogin()
        {
            Window[] openWindows = Application.Current.Windows.OfType<Window>().ToArray();

            if (openWindows.Any() && _gate.TryEnter())
            {
                Window activeWindow = openWindows.FirstOrDefault(window => window.IsActive) ?? openWindows.Last();

                CustomDialogWindow.ShowMessage(activeWindow, Strings.Dialogs_ConnectionLost);
                new LoginWindow(_services).Show();
                CloseWindows(openWindows);
                _gate.Release();
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

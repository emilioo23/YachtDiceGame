using System;
using System.Threading.Tasks;
using System.Windows;
using YachtDice.Gateway;
using YachtDice.Resources;
using YachtDice.Views;

namespace YachtDice.Utils
{
    /// <summary>
    /// Runs a call to the server from a window and shows the player a message when it fails, so each window
    /// does not repeat the same error handling.
    /// </summary>
    public static class GatewayCallRunner
    {
        private const string UnexpectedErrorLogMessage = "Unexpected exception during a call to the server.";

        /// <summary>
        /// Runs the call. A failure of the server is shown with its own message; any other failure is logged
        /// and shown as an unexpected error.
        /// </summary>
        /// <param name="owner">Window that shows the message when the call fails.</param>
        /// <param name="logger">Logger used for the unexpected failures.</param>
        /// <param name="call">Call to run.</param>
        /// <returns>A task that finishes when the call and its error handling finish.</returns>
        public static async Task RunAsync(Window owner, AppLogger logger, Func<Task> call)
        {
            try
            {
                await call();
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(owner, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
            catch (Exception ex)
            {
                logger.Error(ex, UnexpectedErrorLogMessage);
                CustomDialogWindow.ShowMessage(owner, Strings.Dialogs_UnexpectedError);
            }
        }
    }
}

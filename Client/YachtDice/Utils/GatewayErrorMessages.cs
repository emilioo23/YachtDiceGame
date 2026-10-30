using YachtDice.Gateway;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player when a request to the server fails.
    /// </summary>
    public static class GatewayErrorMessages
    {
        /// <summary>
        /// Gets the localized message that matches the kind of failure.
        /// </summary>
        /// <param name="errorCode">Kind of failure reported by the gateway.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(GatewayErrorCode errorCode)
        {
            string message = Strings.Dialogs_UnexpectedError;

            if (errorCode == GatewayErrorCode.ServerUnavailable || errorCode == GatewayErrorCode.DatabaseUnavailable)
            {
                message = Strings.Dialogs_DatabaseConnectionError;
            }

            return message;
        }
    }
}
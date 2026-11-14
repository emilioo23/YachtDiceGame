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
            string message;

            switch (errorCode)
            {
                case GatewayErrorCode.ServerUnavailable:
                    message = Strings.Dialogs_ServerUnavailable;
                    break;
                case GatewayErrorCode.DatabaseUnavailable:
                    message = Strings.Dialogs_DatabaseUnavailable;
                    break;
                case GatewayErrorCode.ValidationFailed:
                    message = Strings.Dialogs_InvalidData;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }
    }
}
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message that explains why a username could not be changed.
    /// </summary>
    public static class UsernameChangeMessages
    {
        /// <summary>
        /// Gets the message for a result of the change of username.
        /// </summary>
        /// <param name="resultCode">Result that the server answered.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(UsernameChangeResultCode resultCode)
        {
            string message = Strings.Profile_UsernameSaveError;

            switch (resultCode)
            {
                case UsernameChangeResultCode.InvalidUsername:
                    message = Strings.Auth_ErrorInvalidUsername;
                    break;
                case UsernameChangeResultCode.OffensiveUsername:
                    message = Strings.Auth_ErrorOffensiveUsername;
                    break;
                case UsernameChangeResultCode.UsernameAlreadyTaken:
                    message = Strings.Dialogs_UsernameTaken;
                    break;
            }

            return message;
        }
    }
}

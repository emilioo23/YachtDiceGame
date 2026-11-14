using YachtDice.PlayerServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player when an avatar could not be changed.
    /// </summary>
    public static class AvatarChangeMessages
    {
        /// <summary>
        /// Gets the localized message that matches the outcome of the change.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <param name="requiredLevel">Level that the avatar needs; used by the message of a locked avatar.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(AvatarChangeResultCode resultCode, int requiredLevel)
        {
            string message = Strings.Profile_AvatarSaveError;

            if (resultCode == AvatarChangeResultCode.AvatarLocked)
            {
                message = string.Format(Strings.Profile_AvatarLocked, requiredLevel);
            }

            return message;
        }
    }
}

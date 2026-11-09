using System.Globalization;

namespace YachtDice.Client.Logic.Avatars
{
    /// <summary>
    /// Builds the name under which the drawing of each avatar is saved in the resources of the application.
    /// </summary>
    public static class AvatarResourceKey
    {
        /// <summary>
        /// Identifier of the avatar that is shown when the drawing of another one is missing.
        /// </summary>
        public const int DefaultAvatarId = 1;

        private const string KeyFormat = "Avatar_{0:D2}";

        /// <summary>
        /// Gets the resource name of the drawing of an avatar.
        /// </summary>
        /// <param name="avatarId">Identifier of the avatar.</param>
        /// <returns>The resource name, for example Avatar_05.</returns>
        public static string Create(int avatarId)
        {
            return string.Format(CultureInfo.InvariantCulture, KeyFormat, avatarId);
        }
    }
}

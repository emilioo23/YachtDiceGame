using System.Windows.Media;
using YachtDice.Client.Logic.Avatars;

namespace YachtDice.Utils
{
    /// <summary>
    /// Gives the drawing of each avatar.
    /// </summary>
    public static class AvatarImages
    {
        private const string DrawingsSource = "Styles/Avatars.xaml";

        private static readonly DrawingCatalog Catalog = new DrawingCatalog(DrawingsSource);

        /// <summary>
        /// Gets the drawing of an avatar.
        /// </summary>
        /// <param name="avatarId">Identifier of the avatar.</param>
        /// <returns>The drawing; the default avatar if there is no drawing for the identifier.</returns>
        public static ImageSource Get(int avatarId)
        {
            return Catalog.Get(
                AvatarResourceKey.Create(avatarId),
                AvatarResourceKey.Create(AvatarResourceKey.DefaultAvatarId));
        }
    }
}

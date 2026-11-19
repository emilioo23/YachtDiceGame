using System.Windows.Media;
using YachtDice.Client.Logic.Avatars;

namespace YachtDice.Utils
{
    /// <summary>
    /// Gives the image of each avatar.
    /// </summary>
    public static class AvatarImages
    {
        private const string ImagesFolder = "Resources/Avatars";

        private static readonly ImageCatalog Catalog = new ImageCatalog(ImagesFolder);

        /// <summary>
        /// Gets the image of an avatar.
        /// </summary>
        /// <param name="avatarId">Identifier of the avatar.</param>
        /// <returns>The image; the default avatar if there is no image for the identifier.</returns>
        public static ImageSource Get(int avatarId)
        {
            return Catalog.Get(
                AvatarResourceKey.Create(avatarId),
                AvatarResourceKey.Create(AvatarResourceKey.DefaultAvatarId));
        }
    }
}

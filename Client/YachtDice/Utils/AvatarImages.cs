using System;
using System.Windows;
using System.Windows.Media;
using YachtDice.Client.Logic.Avatars;

namespace YachtDice.Utils
{
    /// <summary>
    /// Gives the drawing of each avatar. The drawings are in their own resource dictionary, so they do not
    /// depend on the theme that is active.
    /// </summary>
    public static class AvatarImages
    {
        private const string DrawingsSource = "Styles/Avatars.xaml";

        private static readonly ResourceDictionary Drawings =
            new ResourceDictionary { Source = new Uri(DrawingsSource, UriKind.Relative) };

        /// <summary>
        /// Gets the drawing of an avatar.
        /// </summary>
        /// <param name="avatarId">Identifier of the avatar.</param>
        /// <returns>The drawing; the default avatar if there is no drawing for the identifier.</returns>
        public static ImageSource Get(int avatarId)
        {
            string key = AvatarResourceKey.Create(avatarId);

            if (!Drawings.Contains(key))
            {
                key = AvatarResourceKey.Create(AvatarResourceKey.DefaultAvatarId);
            }

            return (ImageSource)Drawings[key];
        }
    }
}

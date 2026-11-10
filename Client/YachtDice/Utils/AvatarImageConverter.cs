using System;
using System.Globalization;
using System.Windows.Data;
using YachtDice.Client.Logic.Avatars;

namespace YachtDice.Utils
{
    /// <summary>
    /// Lets a window bind the identifier of an avatar to the picture that shows it.
    /// </summary>
    public sealed class AvatarImageConverter : IValueConverter
    {
        private const string ConvertBackMessage = "A picture cannot be converted back to an avatar identifier.";

        /// <summary>
        /// Converts the identifier of an avatar into its drawing.
        /// </summary>
        /// <param name="value">Identifier of the avatar, as an integer.</param>
        /// <param name="targetType">Type of the property that receives the picture.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>The drawing of the avatar.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int avatarId = AvatarResourceKey.DefaultAvatarId;

            if (value is int)
            {
                avatarId = (int)value;
            }

            return AvatarImages.Get(avatarId);
        }

        /// <summary>
        /// Not supported: a picture cannot be turned back into an identifier.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Never returns.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(ConvertBackMessage);
        }
    }
}

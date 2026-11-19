using System.Windows.Media.Media3D;

namespace YachtDice.Views
{
    /// <summary>
    /// What a die does in one throw: where and how it lands and how it turns in the air.
    /// </summary>
    internal sealed class DieThrow
    {
        /// <summary>
        /// Gets or sets the position of the landing place, to the right of the center of the table.
        /// </summary>
        public double EndX { get; set; }

        /// <summary>
        /// Gets or sets the position of the landing place, toward the player.
        /// </summary>
        public double EndZ { get; set; }

        /// <summary>
        /// Gets or sets the angle in degrees that the die has around the vertical axis when it lands.
        /// </summary>
        public double YawDegrees { get; set; }

        /// <summary>
        /// Gets or sets the axis that the die turns around in the air.
        /// </summary>
        public Vector3D TumbleAxis { get; set; }

        /// <summary>
        /// Gets or sets the degrees that the die turns around the axis before it lands.
        /// </summary>
        public double TumbleDegrees { get; set; }
    }
}

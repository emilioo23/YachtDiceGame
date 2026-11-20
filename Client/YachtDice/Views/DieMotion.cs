using System.Windows.Media.Media3D;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Views
{
    /// <summary>
    /// What a die does in one movement: a throw, which goes to the hand and flies to the landing place turning
    /// in the air, or a glide, which is a short hop to another place.
    /// </summary>
    internal sealed class DieMotion
    {
        /// <summary>
        /// Gets or sets a value indicating whether the movement is a throw; false is a glide.
        /// </summary>
        public bool IsThrow { get; set; }

        /// <summary>
        /// Gets or sets the place where the movement ends.
        /// </summary>
        public DiceSlot End { get; set; }

        /// <summary>
        /// Gets or sets where the die ends: on the table or in the row under it.
        /// </summary>
        public DiceLocation Location { get; set; }

        /// <summary>
        /// Gets or sets the angle in degrees that the die has around the vertical axis when it ends.
        /// </summary>
        public double YawDegrees { get; set; }

        /// <summary>
        /// Gets or sets the place of the hand, where a thrown die goes before it flies.
        /// </summary>
        public DiceSlot Hand { get; set; }

        /// <summary>
        /// Gets or sets the axis that a thrown die turns around in the air.
        /// </summary>
        public Vector3D TumbleAxis { get; set; }

        /// <summary>
        /// Gets or sets the degrees that a thrown die turns around the axis before it lands.
        /// </summary>
        public double TumbleDegrees { get; set; }

        /// <summary>
        /// Gets or sets the milliseconds that pass after the start of the animation before the die moves.
        /// </summary>
        public double DelayMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the milliseconds that the movement lasts.
        /// </summary>
        public double DurationMilliseconds { get; set; }

        /// <summary>
        /// Gets the milliseconds, from the start of the animation, when the movement ends.
        /// </summary>
        public double FinishMilliseconds => DelayMilliseconds + DurationMilliseconds;
    }
}

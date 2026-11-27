using System;
using YachtDice.Client.Logic.Logging;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// A turn of a die around an axis. The scene applies it to the cube to leave the face of the roll on top.
    /// </summary>
    public sealed class FaceUpRotation
    {
        private const double HalfTurnDegrees = 180;

        /// <summary>
        /// Initializes the rotation.
        /// </summary>
        /// <param name="axis">Direction of the axis; it must have a length of one.</param>
        /// <param name="degrees">Angle of the turn, positive in the direction of the right-hand rule.</param>
        public FaceUpRotation(DiceVector axis, double degrees)
        {
            Axis = axis ?? throw new ArgumentNullException(nameof(axis), LogMessages.NullAxisMessage);
            Degrees = degrees;
        }

        /// <summary>
        /// Gets the direction of the axis of the turn.
        /// </summary>
        public DiceVector Axis { get; }

        /// <summary>
        /// Gets the angle of the turn in degrees.
        /// </summary>
        public double Degrees { get; }

        /// <summary>
        /// Turns a vector with this rotation.
        /// </summary>
        /// <param name="vector">Vector to turn.</param>
        /// <returns>The vector after the turn.</returns>
        public DiceVector Rotate(DiceVector vector)
        {
            double radians = Degrees * Math.PI / HalfTurnDegrees;
            double cosine = Math.Cos(radians);
            double sine = Math.Sin(radians);
            double dot = (Axis.X * vector.X) + (Axis.Y * vector.Y) + (Axis.Z * vector.Z);
            double crossX = (Axis.Y * vector.Z) - (Axis.Z * vector.Y);
            double crossY = (Axis.Z * vector.X) - (Axis.X * vector.Z);
            double crossZ = (Axis.X * vector.Y) - (Axis.Y * vector.X);
            double kept = dot * (1 - cosine);

            return new DiceVector(
                (vector.X * cosine) + (crossX * sine) + (Axis.X * kept),
                (vector.Y * cosine) + (crossY * sine) + (Axis.Y * kept),
                (vector.Z * cosine) + (crossZ * sine) + (Axis.Z * kept));
        }
    }
}

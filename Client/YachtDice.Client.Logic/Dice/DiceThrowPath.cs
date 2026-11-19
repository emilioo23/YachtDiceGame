using System;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// The movement of a die that is thrown on the table: how it rises, bounces and slows down, and how its
    /// turns stop. Every method receives the progress of the throw from zero, when the die leaves its place,
    /// to one, when it stays still; a value out of that range is moved to the nearest limit.
    /// </summary>
    public static class DiceThrowPath
    {
        private const double StartProgress = 0;
        private const double EndProgress = 1;
        private const double PeakHeight = 2.4;
        private const double BounceCycles = 2.5;
        private const double RiseShare = 0.12;
        private const double HeightDecayPower = 2;
        private const double TumbleDecayPower = 3;
        private const double TravelDecayPower = 3;

        /// <summary>
        /// Gets the height of the center of the die. It rises from its place, falls, bounces two times with less
        /// height each time and ends at the height it has when it lies on the table.
        /// </summary>
        /// <param name="progress">Progress of the throw, from zero to one.</param>
        /// <param name="restHeight">Height of the center of the die when it lies on the table.</param>
        /// <returns>The height of the center of the die.</returns>
        public static double Height(double progress, double restHeight)
        {
            double position = Clamp(progress);
            double rise = Math.Min(EndProgress, position / RiseShare);
            double bounce = Math.Abs(Math.Cos(BounceCycles * Math.PI * position));
            double decay = Math.Pow(EndProgress - position, HeightDecayPower);

            return restHeight + (rise * PeakHeight * decay * bounce);
        }

        /// <summary>
        /// Gets how much of the way to the landing place the die has made: it moves fast at first and slows down.
        /// </summary>
        /// <param name="progress">Progress of the throw, from zero to one.</param>
        /// <returns>A value from zero, at the start place, to one, at the landing place.</returns>
        public static double Travel(double progress)
        {
            return EndProgress - Math.Pow(EndProgress - Clamp(progress), TravelDecayPower);
        }

        /// <summary>
        /// Gets the angle that is left of the turns of the die in the air. It is the total at the start and zero
        /// at the end, so the die lands with the face of the roll on top.
        /// </summary>
        /// <param name="totalDegrees">Degrees that the die turns in the air.</param>
        /// <param name="progress">Progress of the throw, from zero to one.</param>
        /// <returns>The degrees that the die still has to turn.</returns>
        public static double TumbleDegrees(double totalDegrees, double progress)
        {
            return totalDegrees * Math.Pow(EndProgress - Clamp(progress), TumbleDecayPower);
        }

        private static double Clamp(double progress)
        {
            return Math.Max(StartProgress, Math.Min(EndProgress, progress));
        }
    }
}

using System;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// The movement of the dice. In a throw the die first goes to the hand, which is a place near the edge of
    /// the table, and then it flies from there to its landing place: it turns in the air, bounces two times and
    /// slows down. A glide is the short hop that moves a die to another place, for example to the row under the
    /// table. Every method receives the progress of the movement from zero, when it starts, to one, when it
    /// ends; a value out of that range is moved to the nearest limit.
    /// </summary>
    public static class DiceThrowPath
    {
        private const double StartProgress = 0;
        private const double EndProgress = 1;
        private const double GatherShare = 0.2;
        private const double PeakHeight = 2;
        private const double HopHeight = 0.9;
        private const double BounceCycles = 2.5;
        private const double RiseShare = 0.12;
        private const double HeightDecayPower = 2;
        private const double TumbleDecayPower = 3;
        private const double TravelDecayPower = 3;
        private const double SmoothstepCubic = 3;
        private const double SmoothstepLinear = 2;

        /// <summary>
        /// Gets how much of the way to the hand the die has made. It reaches one at the end of the first part of
        /// the throw and stays there.
        /// </summary>
        /// <param name="progress">Progress of the throw, from zero to one.</param>
        /// <returns>A value from zero, at the place where the die was, to one, at the hand.</returns>
        public static double Gather(double progress)
        {
            return Smooth(Math.Min(EndProgress, Clamp(progress) / GatherShare));
        }

        /// <summary>
        /// Gets how much of the way from the hand to the landing place the die has made. It stays at zero while
        /// the die goes to the hand, moves fast at first and slows down.
        /// </summary>
        /// <param name="progress">Progress of the throw, from zero to one.</param>
        /// <returns>A value from zero, at the hand, to one, at the landing place.</returns>
        public static double Scatter(double progress)
        {
            return EndProgress - Math.Pow(EndProgress - Flight(progress), TravelDecayPower);
        }

        /// <summary>
        /// Gets the height of the center of the die in a throw. The die lies on the table while it goes to the
        /// hand, then it rises, falls, bounces two times with less height each time and ends at the height it has
        /// when it lies on the table.
        /// </summary>
        /// <param name="progress">Progress of the throw, from zero to one.</param>
        /// <param name="restHeight">Height of the center of the die when it lies on the table.</param>
        /// <returns>The height of the center of the die.</returns>
        public static double Height(double progress, double restHeight)
        {
            double position = Flight(progress);
            double rise = Math.Min(EndProgress, position / RiseShare);
            double bounce = Math.Abs(Math.Cos(BounceCycles * Math.PI * position));
            double decay = Math.Pow(EndProgress - position, HeightDecayPower);

            return restHeight + (rise * PeakHeight * decay * bounce);
        }

        /// <summary>
        /// Gets the angle that is left of the turns of the die in the air. It is the total until the die leaves
        /// the hand and zero at the end, so the die lands with the face of the roll on top.
        /// </summary>
        /// <param name="totalDegrees">Degrees that the die turns in the air.</param>
        /// <param name="progress">Progress of the throw, from zero to one.</param>
        /// <returns>The degrees that the die still has to turn.</returns>
        public static double TumbleDegrees(double totalDegrees, double progress)
        {
            return totalDegrees * Math.Pow(EndProgress - Flight(progress), TumbleDecayPower);
        }

        /// <summary>
        /// Gets how much of the way a die has made in a glide: it starts and ends slowly.
        /// </summary>
        /// <param name="progress">Progress of the glide, from zero to one.</param>
        /// <returns>A value from zero, at the start place, to one, at the end place.</returns>
        public static double GlideTravel(double progress)
        {
            return Smooth(Clamp(progress));
        }

        /// <summary>
        /// Gets the height of the center of the die in a glide: it makes a small hop between the two places.
        /// </summary>
        /// <param name="progress">Progress of the glide, from zero to one.</param>
        /// <param name="restHeight">Height of the center of the die when it lies still.</param>
        /// <returns>The height of the center of the die.</returns>
        public static double GlideHeight(double progress, double restHeight)
        {
            return restHeight + (HopHeight * Math.Sin(Math.PI * Clamp(progress)));
        }

        private static double Flight(double progress)
        {
            return Clamp((Clamp(progress) - GatherShare) / (EndProgress - GatherShare));
        }

        private static double Smooth(double step)
        {
            return step * step * (SmoothstepCubic - (SmoothstepLinear * step));
        }

        private static double Clamp(double progress)
        {
            return Math.Max(StartProgress, Math.Min(EndProgress, progress));
        }
    }
}

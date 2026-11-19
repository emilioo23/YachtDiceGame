using System;
using System.Collections.Generic;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// Chooses where a die lands on the table: a random place inside the disc of the table that is far enough
    /// from the dice that are already there, so two dice never cross even if they land turned.
    /// </summary>
    public static class DiceLanding
    {
        /// <summary>
        /// Smallest distance between the centers of two dice on the table. It is a little more than the diagonal
        /// of a cube.
        /// </summary>
        public const double MinimumGap = 1.6;

        private const int MaxAttempts = 60;
        private const double FullTurnRadians = 2 * Math.PI;
        private const double NoClearance = -1;

        /// <summary>
        /// Chooses a landing place. It tries random places until one is far enough from the others; if none is
        /// after several tries, it uses the one that was farthest.
        /// </summary>
        /// <param name="occupied">Places of the dice that are already on the table.</param>
        /// <param name="nextUnit">Gives a random number from zero up to one that is not included.</param>
        /// <returns>The place where the die lands.</returns>
        public static DiceSlot Choose(IReadOnlyList<DiceSlot> occupied, Func<double> nextUnit)
        {
            DiceSlot best = null;
            double bestClearance = NoClearance;
            var attempt = 0;

            while (attempt < MaxAttempts && bestClearance < MinimumGap)
            {
                DiceSlot candidate = Sample(nextUnit);
                double clearance = FindClearance(candidate, occupied);

                if (clearance > bestClearance)
                {
                    best = candidate;
                    bestClearance = clearance;
                }

                attempt++;
            }

            return best;
        }

        private static DiceSlot Sample(Func<double> nextUnit)
        {
            double radiusUnit = nextUnit();
            double angleUnit = nextUnit();
            double radius = DiceTableLayout.LandingRadius * Math.Sqrt(radiusUnit);
            double angle = FullTurnRadians * angleUnit;

            return new DiceSlot(radius * Math.Cos(angle), radius * Math.Sin(angle));
        }

        private static double FindClearance(DiceSlot candidate, IReadOnlyList<DiceSlot> occupied)
        {
            double clearance = double.MaxValue;

            foreach (DiceSlot other in occupied)
            {
                double deltaX = candidate.X - other.X;
                double deltaZ = candidate.Z - other.Z;

                clearance = Math.Min(clearance, Math.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ)));
            }

            return clearance;
        }
    }
}

namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// Rule that tells when the Yacht celebration is shown: the moment a player writes the fifty points in
    /// the Yacht box.
    /// </summary>
    public static class YachtCelebrationRule
    {
        private const int YachtPoints = 50;

        /// <summary>
        /// Tells whether a change in the Yacht box deserves the celebration.
        /// </summary>
        /// <param name="isFilledBefore">Whether the Yacht box already had points before the change.</param>
        /// <param name="isFilled">Whether the Yacht box has points after the change.</param>
        /// <param name="points">Points of the Yacht box after the change.</param>
        /// <returns>True if the box was just filled with the fifty points of a Yacht.</returns>
        public static bool IsCelebrationDue(bool isFilledBefore, bool isFilled, int points)
        {
            bool isJustFilled = !isFilledBefore && isFilled;

            return isJustFilled && points == YachtPoints;
        }
    }
}

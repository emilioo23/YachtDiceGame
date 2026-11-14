using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Client.Logic.Players
{
    /// <summary>
    /// Finds which achievements a player unlocked since the last time they were checked.
    /// </summary>
    public static class AchievementChanges
    {
        /// <summary>
        /// Gets the achievements that are unlocked now and were not known before.
        /// </summary>
        /// <param name="knownIds">Identifiers of the achievements that were unlocked the last time.</param>
        /// <param name="currentIds">Identifiers of the achievements that are unlocked now.</param>
        /// <returns>The identifiers of the achievements that were just unlocked.</returns>
        public static List<int> FindNewlyUnlocked(IEnumerable<int> knownIds, IEnumerable<int> currentIds)
        {
            return currentIds.Except(knownIds).ToList();
        }
    }
}

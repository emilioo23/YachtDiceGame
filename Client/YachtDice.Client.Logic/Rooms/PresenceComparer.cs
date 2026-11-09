using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Client.Logic.Rooms
{
    /// <summary>
    /// Compares who was inside a match before and after a change of the room, to tell the player who left and
    /// who came back.
    /// </summary>
    public static class PresenceComparer
    {
        /// <summary>
        /// Finds the players who were inside before and are not inside now.
        /// </summary>
        /// <param name="before">Players inside the room before the change.</param>
        /// <param name="after">Players inside the room after the change.</param>
        /// <returns>The players who left, in the order they had.</returns>
        public static List<MatchParticipant> FindDeparted(
            IEnumerable<MatchParticipant> before,
            IEnumerable<MatchParticipant> after)
        {
            return FindMissing(before, after);
        }

        /// <summary>
        /// Finds the players who are inside now and were not inside before.
        /// </summary>
        /// <param name="before">Players inside the room before the change.</param>
        /// <param name="after">Players inside the room after the change.</param>
        /// <returns>The players who arrived, in the order they have.</returns>
        public static List<MatchParticipant> FindArrived(
            IEnumerable<MatchParticipant> before,
            IEnumerable<MatchParticipant> after)
        {
            return FindMissing(after, before);
        }

        private static List<MatchParticipant> FindMissing(
            IEnumerable<MatchParticipant> source,
            IEnumerable<MatchParticipant> reference)
        {
            var referenceIds = new HashSet<string>(reference.Select(player => player.Id), StringComparer.Ordinal);

            return source.Where(player => !referenceIds.Contains(player.Id)).ToList();
        }
    }
}

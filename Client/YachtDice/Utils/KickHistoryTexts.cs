using System;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Writes the sentence that describes an expulsion from the point of view of the player who reads it.
    /// </summary>
    public static class KickHistoryTexts
    {
        /// <summary>
        /// Describes an expulsion: the player either expelled someone or was expelled by someone.
        /// </summary>
        /// <param name="entry">Expulsion that the server sent.</param>
        /// <param name="ownName">Name of the player who reads the history.</param>
        /// <returns>The sentence in the language of the player.</returns>
        public static string Describe(KickHistoryEntry entry, string ownName)
        {
            bool isOwnExpulsion = string.Equals(entry.HostName, ownName, StringComparison.OrdinalIgnoreCase);
            string text;

            if (isOwnExpulsion)
            {
                text = string.Format(Strings.History_KickedByYouFormat, entry.TargetName, entry.RoomCode);
            }
            else
            {
                text = string.Format(Strings.History_KickedYouFormat, entry.HostName, entry.RoomCode);
            }

            return text;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Client.Logic.Players;
using YachtDice.Models;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Builds the rows of the lists of friends shown in the windows.
    /// </summary>
    public static class FriendItemFactory
    {
        /// <summary>
        /// Creates the rows for the players who share the same status, ordered by name.
        /// </summary>
        /// <param name="names">Names of the players.</param>
        /// <param name="presence">Status and connection that all of them share.</param>
        /// <returns>One row for each name.</returns>
        public static List<FriendItem> Create(IEnumerable<string> names, FriendPresence presence)
        {
            List<FriendItem> friendItems = names
                .OrderBy(name => name)
                .Select(name => CreateItem(name, presence))
                .ToList();

            return friendItems;
        }

        /// <summary>
        /// Creates the rows of the friends who are online.
        /// </summary>
        /// <param name="friendNames">Names of all the friends.</param>
        /// <param name="onlineNames">Names of all the players who are online.</param>
        /// <returns>One row for each friend who is online, ordered by name.</returns>
        public static List<FriendItem> CreateOnline(IEnumerable<string> friendNames, ICollection<string> onlineNames)
        {
            var presence = new FriendPresence(Strings.MainMenu_Friends_StatusOnline, true);

            return Create(friendNames.Where(name => IsOnline(name, onlineNames)), presence);
        }

        /// <summary>
        /// Creates the rows of the friends who are not online.
        /// </summary>
        /// <param name="friendNames">Names of all the friends.</param>
        /// <param name="onlineNames">Names of all the players who are online.</param>
        /// <returns>One row for each friend who is offline, ordered by name.</returns>
        public static List<FriendItem> CreateOffline(IEnumerable<string> friendNames, ICollection<string> onlineNames)
        {
            Func<string, bool> isNotOnline = name => !IsOnline(name, onlineNames);
            var presence = new FriendPresence(Strings.MainMenu_Friends_StatusOffline, false);

            return Create(friendNames.Where(isNotOnline), presence);
        }

        private static bool IsOnline(string name, ICollection<string> onlineNames)
        {
            return onlineNames.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        private static FriendItem CreateItem(string name, FriendPresence presence)
        {
            var friend = new FriendItem
            {
                Name = name,
                Initial = InitialLetter.From(name, Strings.Global_UnknownInitial),
                Status = presence.Status,
                IsOnline = presence.IsOnline
            };

            return friend;
        }
    }
}

using System.Globalization;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Services
{
    using LogicFriends = YachtDice.Server.Logic.Friends;

    /// <summary>
    /// Writes who a player is in the log. A player with an account is written with the numeric identifier of the
    /// account, never with the name or the email, so the lines of the log can be linked to each other without
    /// exposing personal data. A player without account, such as a guest, is written as a generic player.
    /// </summary>
    public static class PlayerLogName
    {
        /// <summary>
        /// Gets the text that identifies the player in a line of the log.
        /// </summary>
        /// <param name="finder">Finds the identifier of the account of a name.</param>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The identifier of the account, or a generic text when the player has no account.</returns>
        public static string Describe(LogicFriends.IPlayerIdFinder finder, string displayName)
        {
            string description = LogMessages.UnknownText;

            try
            {
                int playerId = finder.FindPlayerId(displayName);

                if (playerId != LogicFriends.FriendshipService.NoPlayerId)
                {
                    description = string.Format(CultureInfo.InvariantCulture, LogMessages.KnownFormat, playerId);
                }
            }
            catch (DataAccessException)
            {
                description = LogMessages.UnknownText;
            }

            return description;
        }
    }
}

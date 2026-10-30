using System.Collections.Generic;

namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Rules to add friends, answer friend requests and list the friends of a player.
    /// </summary>
    public interface IFriendshipService
    {
        /// <summary>
        /// Sends a friend request to the player who owns the friend code.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="friendCode">Friend code of the player who will receive the request.</param>
        /// <returns>Outcome of the request and the name of the player who owns the code.</returns>
        FriendRequestResult SendRequest(string displayName, string friendCode);

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="displayName">Name of the player who answers the request.</param>
        /// <param name="requesterName">Name of the player who sent the request.</param>
        /// <param name="isAccepted">True to accept the request, false to reject it.</param>
        /// <returns>Outcome of the answer.</returns>
        FriendActionStatus RespondToRequest(string displayName, string requesterName, bool isAccepted);

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="displayName">Name of the player who removes the friend.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        FriendActionStatus RemoveFriend(string displayName, string friendName);

        /// <summary>
        /// Gets the names of the friends of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the friends, or an empty list if the player has none.</returns>
        List<string> GetFriendNames(string displayName);

        /// <summary>
        /// Gets the names of the players who sent a friend request that the player has not answered.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the players who sent a request, or an empty list if there is none.</returns>
        List<string> GetRequesterNames(string displayName);
    }
}

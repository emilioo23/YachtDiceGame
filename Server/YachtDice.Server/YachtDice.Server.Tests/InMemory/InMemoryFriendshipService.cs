using System;
using System.Collections.Generic;
using YachtDice.Server.Logic.Friends;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Friendships that live in memory, so the tests can say who is a friend of whom without a database. Only
    /// the list of friends works; the rest of the operations are not needed by the tests that use it.
    /// </summary>
    public class InMemoryFriendshipService : IFriendshipService
    {
        private const string NotSupportedMessage = "The tests that use this service do not need this operation.";

        private readonly Dictionary<string, List<string>> _friendsByPlayer =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Makes the two players friends of each other.
        /// </summary>
        /// <param name="firstName">Name of the first player.</param>
        /// <param name="secondName">Name of the second player.</param>
        public void AddFriends(string firstName, string secondName)
        {
            GetOrCreateList(firstName).Add(secondName);
            GetOrCreateList(secondName).Add(firstName);
        }

        /// <inheritdoc />
        public List<string> GetFriendNames(string displayName)
        {
            return new List<string>(GetOrCreateList(displayName));
        }

        /// <inheritdoc />
        public FriendRequestResult SendRequest(string displayName, string targetUsername)
        {
            throw new NotSupportedException(NotSupportedMessage);
        }

        /// <inheritdoc />
        public FriendActionStatus RespondToRequest(string displayName, FriendRequestAnswer answer)
        {
            throw new NotSupportedException(NotSupportedMessage);
        }

        /// <inheritdoc />
        public FriendActionStatus RemoveFriend(string displayName, string friendName)
        {
            throw new NotSupportedException(NotSupportedMessage);
        }

        /// <inheritdoc />
        public string GetUsername(string displayName)
        {
            throw new NotSupportedException(NotSupportedMessage);
        }

        /// <inheritdoc />
        public List<string> GetRequesterNames(string displayName)
        {
            throw new NotSupportedException(NotSupportedMessage);
        }

        private List<string> GetOrCreateList(string displayName)
        {
            if (!_friendsByPlayer.TryGetValue(displayName, out List<string> friends))
            {
                friends = new List<string>();
                _friendsByPlayer[displayName] = friends;
            }

            return friends;
        }
    }
}

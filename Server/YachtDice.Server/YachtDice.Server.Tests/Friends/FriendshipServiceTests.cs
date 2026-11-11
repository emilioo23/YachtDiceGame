using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Friends;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Friends
{
    /// <summary>
    /// Rules to send a friend request by username.
    /// </summary>
    public class FriendshipServiceTests
    {
        private const int RequesterId = 1;
        private const int TargetId = 2;
        private const string RequesterName = "DiceKing99";
        private const string TargetUsername = "GabrielHM";
        private const string TargetDisplayName = "Gabriel";
        private const string UnknownUsername = "NobodyHere";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryFriendRepository _friends = new InMemoryFriendRepository();
        private readonly FriendshipService _service;

        /// <summary>
        /// Prepares a requester and a target whose username and display name are different.
        /// </summary>
        public FriendshipServiceTests()
        {
            _players.Add(new Player { Id = RequesterId, Username = RequesterName, DisplayName = RequesterName });
            _players.Add(new Player { Id = TargetId, Username = TargetUsername, DisplayName = TargetDisplayName });
            _service = new FriendshipService(_players, _friends);
        }

        [Fact]
        public void SendRequest_ExistingUsername_ReturnsRequestSent()
        {
            FriendRequestResult result = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.RequestSent, result.Status);
        }

        [Fact]
        public void SendRequest_ExistingUsername_SavesAPendingRequestForTheTarget()
        {
            var expected = new { AddresseeId = TargetId, Status = FriendshipStatus.Pending };

            _service.SendRequest(RequesterName, TargetUsername);

            Friendship savedRequest = _friends.Friendships.Single();
            var actual = new { savedRequest.AddresseeId, savedRequest.Status };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SendRequest_UnknownUsername_ReturnsUsernameNotFound()
        {
            FriendRequestResult result = _service.SendRequest(RequesterName, UnknownUsername);

            Assert.Equal(FriendRequestStatus.UsernameNotFound, result.Status);
        }

        [Fact]
        public void SendRequest_DisplayNameInsteadOfUsername_ReturnsUsernameNotFound()
        {
            FriendRequestResult result = _service.SendRequest(RequesterName, TargetDisplayName);

            Assert.Equal(FriendRequestStatus.UsernameNotFound, result.Status);
        }

        [Fact]
        public void SendRequest_BlankUsername_ReturnsInvalidUsername()
        {
            FriendRequestResult result = _service.SendRequest(RequesterName, string.Empty);

            Assert.Equal(FriendRequestStatus.InvalidUsername, result.Status);
        }

        [Fact]
        public void SendRequest_OwnUsername_ReturnsSelfRequest()
        {
            FriendRequestResult result = _service.SendRequest(RequesterName, RequesterName);

            Assert.Equal(FriendRequestStatus.SelfRequest, result.Status);
        }

        [Fact]
        public void SendRequest_RequestAlreadySent_ReturnsAlreadyRequested()
        {
            _service.SendRequest(RequesterName, TargetUsername);

            FriendRequestResult result = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.AlreadyRequested, result.Status);
        }

        [Fact]
        public void SendRequest_RequestAlreadySent_KeepsOnlyOneRequest()
        {
            _service.SendRequest(RequesterName, TargetUsername);

            _service.SendRequest(RequesterName, TargetUsername);

            Assert.Single(_friends.Friendships);
        }

        [Theory]
        [InlineData(UnknownUsername)]
        [InlineData(TargetDisplayName)]
        [InlineData("")]
        [InlineData(RequesterName)]
        public void SendRequest_RejectedUsername_SavesNothing(string username)
        {
            _service.SendRequest(RequesterName, username);

            Assert.Empty(_friends.Friendships);
        }
    }
}

using System;
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
        private const string UnknownRequesterName = "NobodySent";

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
            var expected = new Friendship
            {
                RequesterId = RequesterId,
                AddresseeId = TargetId,
                Status = FriendshipStatus.Pending
            };

            _service.SendRequest(RequesterName, TargetUsername);

            Friendship savedRequest = _friends.Friendships.Single();
            Assert.Equivalent(expected, savedRequest);
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

        [Fact]
        public void RespondToRequest_AcceptedRequest_ReturnsSuccessAndSavesTheFriendshipAsAccepted()
        {
            var expected = new Friendship
            {
                RequesterId = RequesterId,
                AddresseeId = TargetId,
                Status = FriendshipStatus.Accepted
            };
            _service.SendRequest(RequesterName, TargetUsername);

            FriendActionStatus status = RespondAsTarget(RequesterName, true);

            Assert.Equal(FriendActionStatus.Success, status);
            Assert.Equivalent(expected, _friends.Friendships.Single());
        }

        [Fact]
        public void RespondToRequest_RejectedRequest_ReturnsSuccessAndRemovesTheRequest()
        {
            _service.SendRequest(RequesterName, TargetUsername);

            FriendActionStatus status = RespondAsTarget(RequesterName, false);

            Assert.Equal(FriendActionStatus.Success, status);
            Assert.Empty(_friends.Friendships);
        }

        [Fact]
        public void RespondToRequest_RequesterWithoutRequest_ReturnsRelationNotFound()
        {
            FriendActionStatus status = RespondAsTarget(RequesterName, true);

            Assert.Equal(FriendActionStatus.RelationNotFound, status);
        }

        [Fact]
        public void RespondToRequest_UnknownRequester_ReturnsPlayerNotFound()
        {
            FriendActionStatus status =
                RespondAsTarget(UnknownRequesterName, true);

            Assert.Equal(FriendActionStatus.PlayerNotFound, status);
        }

        [Fact]
        public void RespondToRequest_NullAnswer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.RespondToRequest(TargetDisplayName, null));
        }

        private FriendActionStatus RespondAsTarget(string requesterName, bool isAccepted)
        {
            var answer = new FriendRequestAnswer { RequesterName = requesterName, IsAccepted = isAccepted };

            return _service.RespondToRequest(TargetDisplayName, answer);
        }
    }
}

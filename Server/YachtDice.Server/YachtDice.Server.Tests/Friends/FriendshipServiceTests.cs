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
        private const int NoFriendships = 0;
        private const string RequesterName = "DiceKing99";
        private const string TargetUsername = "GabrielHM";
        private const string TargetDisplayName = "Gabriel";
        private const string UnknownUsername = "CarlosRamirez";
        private const string UnknownRequesterName = "LuisHernandez";
        private const int FirstOtherPlayerId = 100;
        private const int ConfiguredMaxFriends = 2;
        private const int EightDays = 8;
        private const int FiveRequests = 5;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryFriendRepository _friends;
        private readonly FriendshipService _service;

        /// <summary>
        /// Prepares a requester and a target whose username and display name are different.
        /// </summary>
        public FriendshipServiceTests()
        {
            _players.Add(new Player { Id = RequesterId, Username = RequesterName, DisplayName = RequesterName });
            _players.Add(new Player { Id = TargetId, Username = TargetUsername, DisplayName = TargetDisplayName });
            _friends = new InMemoryFriendRepository(_players);
            _service = new FriendshipService(_players, _friends, new InMemoryPlayerReportRepository());
        }

        [Fact]
        public void SendRequest_FiveRequestsOlderThanSevenDays_ReturnsRequestSent()
        {
            AddOldPendingRequests(RequesterId, FiveRequests);

            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.RequestSent, requestResult.Status);
        }

        [Fact]
        public void SendRequest_OldRequestToTheSamePlayer_ReturnsRequestSent()
        {
            _friends.Add(new Friendship
            {
                RequesterId = RequesterId,
                AddresseeId = TargetId,
                Status = FriendshipStatus.Pending,
                CreateDate = DateTime.UtcNow.AddDays(-EightDays)
            });

            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.RequestSent, requestResult.Status);
        }

        [Fact]
        public void RespondToRequest_RequestOlderThanSevenDays_ReturnsRelationNotFound()
        {
            _service.SendRequest(RequesterName, TargetUsername);
            _friends.FindBetween(RequesterId, TargetId).Single().CreateDate = DateTime.UtcNow.AddDays(-EightDays);
            var answer = new FriendRequestAnswer { RequesterName = RequesterName, IsAccepted = true };

            FriendActionStatus status = _service.RespondToRequest(TargetDisplayName, answer);

            Assert.Equal(FriendActionStatus.RelationNotFound, status);
        }

        [Fact]
        public void BlockPlayer_Friend_RemovesTheFriendFromTheList()
        {
            AcceptRequestBetweenThePlayers();

            _service.BlockPlayer(TargetDisplayName, RequesterName);

            Assert.Empty(_service.GetFriendNames(RequesterName));
        }

        [Fact]
        public void SendRequest_PlayerWhoBlockedTheRequester_ReturnsBlocked()
        {
            _service.BlockPlayer(TargetDisplayName, RequesterName);

            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.Blocked, requestResult.Status);
        }

        [Fact]
        public void SendRequest_PlayerWithAPendingRequestThenBlocked_ReturnsBlocked()
        {
            _service.SendRequest(RequesterName, TargetUsername);
            _service.BlockPlayer(TargetDisplayName, RequesterName);

            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.Blocked, requestResult.Status);
        }

        [Fact]
        public void BlockPlayer_Themselves_ReturnsInvalidTarget()
        {
            FriendActionStatus status = _service.BlockPlayer(RequesterName, RequesterName);

            Assert.Equal(FriendActionStatus.InvalidTarget, status);
        }

        [Fact]
        public void UnblockPlayer_PlayerWhoBlocked_LetsTheRequestBeSentAgain()
        {
            _service.BlockPlayer(TargetDisplayName, RequesterName);
            _service.UnblockPlayer(TargetDisplayName, RequesterName);

            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.RequestSent, requestResult.Status);
        }

        [Fact]
        public void UnblockPlayer_FriendWhoReceivedTheRequestBlocked_ReturnsSuccess()
        {
            AcceptRequestBetweenThePlayers();
            _service.BlockPlayer(TargetDisplayName, RequesterName);

            FriendActionStatus status = _service.UnblockPlayer(TargetDisplayName, RequesterName);

            Assert.Equal(FriendActionStatus.Success, status);
        }

        [Fact]
        public void UnblockPlayer_PlayerWhoWasBlocked_ReturnsRelationNotFound()
        {
            _service.BlockPlayer(TargetDisplayName, RequesterName);

            FriendActionStatus status = _service.UnblockPlayer(RequesterName, TargetDisplayName);

            Assert.Equal(FriendActionStatus.RelationNotFound, status);
        }

        [Fact]
        public void UnblockPlayer_PlayerWhoIsNotBlocked_ReturnsRelationNotFound()
        {
            FriendActionStatus status = _service.UnblockPlayer(TargetDisplayName, RequesterName);

            Assert.Equal(FriendActionStatus.RelationNotFound, status);
        }

        [Fact]
        public void UnblockPlayer_UnknownPlayer_ReturnsPlayerNotFound()
        {
            FriendActionStatus status = _service.UnblockPlayer(RequesterName, UnknownRequesterName);

            Assert.Equal(FriendActionStatus.PlayerNotFound, status);
        }

        [Fact]
        public void GetBlockedNames_PlayerWhoBlocked_ReturnsTheBlockedPlayer()
        {
            _service.BlockPlayer(TargetDisplayName, RequesterName);

            string blockedName = _service.GetBlockedNames(TargetDisplayName).Single();

            Assert.Equal(RequesterName, blockedName);
        }

        [Fact]
        public void GetBlockedNames_AfterUnblocking_ReturnsAnEmptyList()
        {
            _service.BlockPlayer(TargetDisplayName, RequesterName);
            _service.UnblockPlayer(TargetDisplayName, RequesterName);

            Assert.Empty(_service.GetBlockedNames(TargetDisplayName));
        }

        [Fact]
        public void BlockPlayer_UnknownPlayer_ReturnsPlayerNotFound()
        {
            FriendActionStatus status = _service.BlockPlayer(RequesterName, UnknownRequesterName);

            Assert.Equal(FriendActionStatus.PlayerNotFound, status);
        }

        [Fact]
        public void SendRequest_ExistingUsername_ReturnsRequestSent()
        {
            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.RequestSent, requestResult.Status);
        }

        [Fact]
        public void SendRequest_FivePendingRequestsAlready_ReturnsTooManyPendingRequests()
        {
            AddFriendships(RequesterId, _service.MaxPendingRequests, FriendshipStatus.Pending);

            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.TooManyPendingRequests, requestResult.Status);
        }

        [Fact]
        public void SendRequest_RequesterWithAHundredFriends_ReturnsFriendLimitReached()
        {
            AddFriendships(RequesterId, _service.MaxFriends, FriendshipStatus.Accepted);

            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.FriendLimitReached, requestResult.Status);
        }

        [Fact]
        public void SendRequest_ConfiguredLimitOfTwoFriendsReached_ReturnsFriendLimitReached()
        {
            _service.MaxFriends = ConfiguredMaxFriends;
            AddFriendships(RequesterId, ConfiguredMaxFriends, FriendshipStatus.Accepted);

            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.FriendLimitReached, requestResult.Status);
        }

        [Fact]
        public void MaxFriends_NewService_UsesTheDefaultLimit()
        {
            Assert.Equal(FriendshipService.DefaultMaxFriends, _service.MaxFriends);
        }

        [Fact]
        public void RespondToRequest_AddresseeWithAHundredFriends_ReturnsFriendLimitReached()
        {
            _service.SendRequest(RequesterName, TargetUsername);
            AddFriendships(TargetId, _service.MaxFriends, FriendshipStatus.Accepted);
            var answer = new FriendRequestAnswer { RequesterName = RequesterName, IsAccepted = true };

            FriendActionStatus status = _service.RespondToRequest(TargetDisplayName, answer);

            Assert.Equal(FriendActionStatus.FriendLimitReached, status);
        }

        [Fact]
        public void SendRequest_ExistingUsername_SavesAPendingRequestForTheTarget()
        {
            var savedRequestExpected = new
            {
                RequesterId = RequesterId,
                AddresseeId = TargetId,
                Status = FriendshipStatus.Pending
            };

            _service.SendRequest(RequesterName, TargetUsername);

            Friendship savedRequest = _friends.Friendships.Single();
            Assert.Equivalent(savedRequestExpected, savedRequest);
        }

        [Fact]
        public void SendRequest_UnknownUsername_ReturnsUsernameNotFound()
        {
            FriendRequestResult requestResult = _service.SendRequest(RequesterName, UnknownUsername);

            Assert.Equal(FriendRequestStatus.UsernameNotFound, requestResult.Status);
        }

        [Fact]
        public void SendRequest_DisplayNameInsteadOfUsername_ReturnsUsernameNotFound()
        {
            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetDisplayName);

            Assert.Equal(FriendRequestStatus.UsernameNotFound, requestResult.Status);
        }

        [Fact]
        public void SendRequest_BlankUsername_ReturnsInvalidUsername()
        {
            FriendRequestResult requestResult = _service.SendRequest(RequesterName, string.Empty);

            Assert.Equal(FriendRequestStatus.InvalidUsername, requestResult.Status);
        }

        [Fact]
        public void SendRequest_OwnUsername_ReturnsSelfRequest()
        {
            FriendRequestResult requestResult = _service.SendRequest(RequesterName, RequesterName);

            Assert.Equal(FriendRequestStatus.SelfRequest, requestResult.Status);
        }

        [Fact]
        public void SendRequest_RequestAlreadySent_ReturnsAlreadyRequested()
        {
            _service.SendRequest(RequesterName, TargetUsername);

            FriendRequestResult requestResult = _service.SendRequest(RequesterName, TargetUsername);

            Assert.Equal(FriendRequestStatus.AlreadyRequested, requestResult.Status);
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
            var friendshipExpected = new
            {
                RequesterId = RequesterId,
                AddresseeId = TargetId,
                Status = FriendshipStatus.Accepted
            };
            var outcomeExpected = new { Status = FriendActionStatus.Success, Friendship = friendshipExpected };
            _service.SendRequest(RequesterName, TargetUsername);

            FriendActionStatus status = RespondAsTarget(RequesterName, true);

            var outcomeResult = new { Status = status, Friendship = _friends.Friendships.Single() };
            Assert.Equivalent(outcomeExpected, outcomeResult);
        }

        [Fact]
        public void RespondToRequest_RejectedRequest_ReturnsSuccessAndRemovesTheRequest()
        {
            var outcomeExpected = new { Status = FriendActionStatus.Success, SavedFriendships = NoFriendships };
            _service.SendRequest(RequesterName, TargetUsername);

            FriendActionStatus status = RespondAsTarget(RequesterName, false);

            var outcomeResult = new { Status = status, SavedFriendships = _friends.Friendships.Count };
            Assert.Equal(outcomeExpected, outcomeResult);
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

        private void AddFriendships(int playerId, int count, string status)
        {
            for (int friendIndex = 0; friendIndex < count; friendIndex++)
            {
                _friends.Add(new Friendship
                {
                    RequesterId = playerId,
                    AddresseeId = FirstOtherPlayerId + friendIndex,
                    Status = status
                });
            }
        }

        private void AddOldPendingRequests(int requesterId, int count)
        {
            for (int number = 0; number < count; number++)
            {
                _friends.Add(new Friendship
                {
                    RequesterId = requesterId,
                    AddresseeId = FirstOtherPlayerId + number,
                    Status = FriendshipStatus.Pending,
                    CreateDate = DateTime.UtcNow.AddDays(-EightDays)
                });
            }
        }

        private void AcceptRequestBetweenThePlayers()
        {
            _service.SendRequest(RequesterName, TargetUsername);
            _service.RespondToRequest(
                TargetDisplayName,
                new FriendRequestAnswer { RequesterName = RequesterName, IsAccepted = true });
        }
    }
}

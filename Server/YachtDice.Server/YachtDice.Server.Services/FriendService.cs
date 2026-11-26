using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Friends;
    using LogicFriends = YachtDice.Server.Logic.Friends;
    using LogicRooms = YachtDice.Server.Logic.Rooms;
    using PresenceContracts = YachtDice.Server.Contracts.Presence;
    using RoomContracts = YachtDice.Server.Contracts.Rooms;

    /// <summary>
    /// Manages the friends of the players: requests, answers, removals and lists.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class FriendService : ServiceBase, Contracts.IFriendService
    {
        private readonly LogicFriends.IFriendshipService _friendshipService;
        private readonly LogicFriends.IRoomInvitationService _invitationService;
        private readonly IPlayerNotifier _notifier;

        /// <summary>
        /// Initializes a new instance of the FriendService class.
        /// </summary>
        /// <param name="friendshipService">The service that applies the rules of the friendships.</param>
        /// <param name="invitationService">The service that applies the rules of the invitations to a room.</param>
        /// <param name="notifier">The component that warns the players who are online.</param>
        public FriendService(
            LogicFriends.IFriendshipService friendshipService,
            LogicFriends.IRoomInvitationService invitationService,
            IPlayerNotifier notifier)
        {
            _friendshipService = friendshipService
                ?? throw new ArgumentNullException(nameof(friendshipService), LogMessages.FriendNullDependencyMessage);
            _invitationService = invitationService
                ?? throw new ArgumentNullException(nameof(invitationService), LogMessages.FriendNullDependencyMessage);
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier),
                LogMessages.FriendNullDependencyMessage);
        }

        /// <summary>
        /// Sends a friend request to the player who owns the username.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="targetUsername">Username of the player who will receive the request.</param>
        /// <returns>Outcome of the request.</returns>
        public Contracts.FriendRequestResultCode SendFriendRequest(string sessionToken, string targetUsername)
        {
            Contracts.FriendRequestResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessRequest(playerName, targetUsername));

            return result;
        }

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="answer">Name of the player who sent the request and whether it is accepted.</param>
        /// <returns>Outcome of the answer.</returns>
        public Contracts.FriendActionResultCode RespondToFriendRequest(
            string sessionToken,
            Contracts.FriendRequestAnswer answer)
        {
            Contracts.FriendActionResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessResponse(playerName, answer));

            return result;
        }

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        public Contracts.FriendActionResultCode RemoveFriend(string sessionToken, string friendName)
        {
            Contracts.FriendActionResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessRemoval(playerName, friendName));

            return result;
        }

        /// <summary>
        /// Blocks another player and updates the friend list of the blocked player if they were friends.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="blockedName">Name of the player who is blocked.</param>
        /// <returns>Outcome of the block.</returns>
        public Contracts.FriendActionResultCode BlockPlayer(string sessionToken, string blockedName)
        {
            Contracts.FriendActionResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessBlock(playerName, blockedName));

            return result;
        }

        /// <summary>
        /// Removes a block that the player made. The two players do not become friends again.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="blockedName">Name of the player who was blocked.</param>
        /// <returns>Outcome of the removal of the block.</returns>
        public Contracts.FriendActionResultCode UnblockPlayer(string sessionToken, string blockedName)
        {
            Contracts.FriendActionResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessUnblock(playerName, blockedName));

            return result;
        }

        /// <summary>
        /// Gets the names of the players that the player blocked.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The names of the blocked players, or an empty list if there is none.</returns>
        public List<string> GetBlockedPlayers(string sessionToken)
        {
            List<string> blockedNames =
                ExecuteForPlayer(sessionToken, playerName => _friendshipService.GetBlockedNames(playerName));

            return blockedNames;
        }

        /// <summary>
        /// Invites a friend who is online to join the room where the player is.
        /// </summary>
        /// <param name="sender">Room and identifier of the player who invites.</param>
        /// <param name="friendName">Name of the friend to invite.</param>
        /// <returns>Whether the invitation reached the friend or why not.</returns>
        public Contracts.InvitationResultCode InviteToRoom(RoomContracts.RoomMember sender, string friendName)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(sender);
            Contracts.InvitationResultCode result = Execute(sender, () => ProcessInvitation(member, friendName));

            return result;
        }

        /// <summary>
        /// Reports another player who finished a game with the player in the last day or who plays the same game
        /// now. If the report suspends the reported player, the sessions of that player end, so they cannot keep
        /// using the server.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player who reports.</param>
        /// <param name="report">Name of the player who is reported, the reason and an optional comment.</param>
        /// <returns>Outcome of the report.</returns>
        public Contracts.ReportResultCode ReportPlayer(string sessionToken, Contracts.PlayerReport report)
        {
            Contracts.ReportResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessReport(playerName, report));

            return result;
        }

        /// <summary>
        /// Gets the invitations to a room that the player received in the last day, from the newest to the oldest.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The invitations, or an empty list if there is none.</returns>
        public List<Contracts.ReceivedInvitation> GetReceivedInvitations(string sessionToken)
        {
            List<Contracts.ReceivedInvitation> invitations =
                ExecuteForPlayer(sessionToken, playerName => ProcessReceivedInvitations(playerName));

            return invitations;
        }

        /// <summary>
        /// Gets the username that other players use to send a friend request to the player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The username of the player, or an empty text if the player does not exist.</returns>
        public string GetUsername(string sessionToken)
        {
            string username = ExecuteForPlayer(sessionToken, _friendshipService.GetUsername);

            return username;
        }

        /// <summary>
        /// Gets the names of the friends of the player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player, or an empty text for a guest.</param>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the friends, or an empty list if the player has none.</returns>
        public List<string> GetFriends(string sessionToken, string displayName)
        {
            List<string> friendNames = Execute(
                displayName,
                () => _friendshipService.GetFriendNames(ResolvePlayerName(sessionToken, displayName)));

            return friendNames;
        }

        /// <summary>
        /// Gets the names of the players who sent a friend request that the player has not answered.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The names of the players who sent a request, or an empty list if there is none.</returns>
        public List<string> GetFriendRequests(string sessionToken)
        {
            List<string> requesterNames = ExecuteForPlayer(sessionToken, _friendshipService.GetRequesterNames);

            return requesterNames;
        }

        private Contracts.FriendRequestResultCode ProcessRequest(string displayName, string targetUsername)
        {
            LogicFriends.FriendRequestResult result = _friendshipService.SendRequest(displayName, targetUsername);

            if (result.Status == LogicFriends.FriendRequestStatus.RequestSent)
            {
                _notifier.SendEvent(
                    result.FriendName,
                    displayName,
                    PresenceContracts.PresenceEventType.FriendRequestReceived);
            }

            return Mapping.EnumMapper.Map<LogicFriends.FriendRequestStatus, Contracts.FriendRequestResultCode>(
                result.Status);
        }

        private Contracts.FriendActionResultCode ProcessResponse(
            string displayName,
            Contracts.FriendRequestAnswer answer)
        {
            LogicFriends.FriendActionStatus status =
                _friendshipService.RespondToRequest(displayName, answer.RequesterName, answer.IsAccepted);

            if (status == LogicFriends.FriendActionStatus.Success && answer.IsAccepted)
            {
                _notifier.SendEvent(
                    answer.RequesterName,
                    displayName,
                    PresenceContracts.PresenceEventType.FriendRequestAccepted);
            }

            return Mapping.EnumMapper.Map<LogicFriends.FriendActionStatus, Contracts.FriendActionResultCode>(status);
        }

        private Contracts.InvitationResultCode ProcessInvitation(LogicRooms.RoomMember sender, string friendName)
        {
            LogicFriends.InvitationResult result = _invitationService.CheckInvitation(sender, friendName);
            Contracts.InvitationResultCode code;

            if (result.Status == LogicFriends.InvitationStatus.Allowed)
            {
                var invitation = new PresenceContracts.RoomInvitation
                {
                    SenderName = result.SenderName,
                    RoomCode = sender.RoomCode
                };

                code = Deliver(friendName, invitation);

                if (code == Contracts.InvitationResultCode.Sent)
                {
                    _invitationService.SaveInvitation(result.SenderName, friendName, sender.RoomCode);
                }
            }
            else
            {
                code = Mapping.EnumMapper.Map<LogicFriends.InvitationStatus, Contracts.InvitationResultCode>(
                    result.Status);
            }

            return code;
        }

        private List<Contracts.ReceivedInvitation> ProcessReceivedInvitations(string displayName)
        {
            Func<LogicFriends.ReceivedInvitation, Contracts.ReceivedInvitation> toContract = invitation =>
                new Contracts.ReceivedInvitation
                {
                    SenderName = invitation.SenderName,
                    RoomCode = invitation.RoomCode,
                    SentAt = invitation.SentAt,
                    IsRoomOpen = invitation.IsRoomOpen
                };

            return _invitationService.GetReceivedInvitations(displayName).Select(toContract).ToList();
        }

        private Contracts.ReportResultCode ProcessReport(string displayName, Contracts.PlayerReport report)
        {
            var details = new LogicFriends.ReportDetails
            {
                ReportedName = report?.ReportedName,
                Reason = Mapping.EnumMapper.Map<Contracts.ReportReason, LogicFriends.ReportReason>(
                    report?.Reason ?? Contracts.ReportReason.Other),
                Comment = report?.Comment,
                IsDuringGame = _invitationService.ArePlayingTogether(displayName, report?.ReportedName)
            };

            LogicFriends.ReportStatus status = _friendshipService.ReportPlayer(displayName, details);

            if (status == LogicFriends.ReportStatus.SentAndSuspended)
            {
                Sessions.EndFor(details.ReportedName.Trim());
                status = LogicFriends.ReportStatus.Sent;
            }

            return Mapping.EnumMapper.Map<LogicFriends.ReportStatus, Contracts.ReportResultCode>(status);
        }

        private Contracts.InvitationResultCode Deliver(string friendName, PresenceContracts.RoomInvitation invitation)
        {
            bool isDelivered = _notifier.SendInvitation(friendName, invitation);
            Contracts.InvitationResultCode code;

            if (isDelivered)
            {
                code = Contracts.InvitationResultCode.Sent;
            }
            else
            {
                code = Contracts.InvitationResultCode.FriendOffline;
            }

            return code;
        }

        private Contracts.FriendActionResultCode ProcessRemoval(string displayName, string friendName)
        {
            LogicFriends.FriendActionStatus status = _friendshipService.RemoveFriend(displayName, friendName);

            if (status == LogicFriends.FriendActionStatus.Success)
            {
                _notifier.SendEvent(friendName, displayName, PresenceContracts.PresenceEventType.FriendRemoved);
            }

            return Mapping.EnumMapper.Map<LogicFriends.FriendActionStatus, Contracts.FriendActionResultCode>(status);
        }

        private Contracts.FriendActionResultCode ProcessUnblock(string displayName, string blockedName)
        {
            LogicFriends.FriendActionStatus status = _friendshipService.UnblockPlayer(displayName, blockedName);

            return Mapping.EnumMapper.Map<LogicFriends.FriendActionStatus, Contracts.FriendActionResultCode>(status);
        }

        private Contracts.FriendActionResultCode ProcessBlock(string displayName, string blockedName)
        {
            LogicFriends.FriendActionStatus status = _friendshipService.BlockPlayer(displayName, blockedName);

            if (status == LogicFriends.FriendActionStatus.Success)
            {
                _notifier.SendEvent(blockedName, displayName, PresenceContracts.PresenceEventType.FriendRemoved);
            }

            return Mapping.EnumMapper.Map<LogicFriends.FriendActionStatus, Contracts.FriendActionResultCode>(status);
        }
    }
}

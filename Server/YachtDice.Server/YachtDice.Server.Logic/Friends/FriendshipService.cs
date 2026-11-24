using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Logging;

namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Rules of the relations between players: add friends, answer friend requests, list the friends of a player,
    /// block and unblock a player and report a player who behaved badly in a game. A player reported by three
    /// different players in one day cannot log in for a day.
    /// </summary>
    public class FriendshipService : IFriendshipService
    {
        /// <summary>
        /// Most friends that a player can have when the server does not configure another limit.
        /// </summary>
        public const int DefaultMaxFriends = 100;

        /// <summary>
        /// Most friend requests waiting for an answer when the server does not configure another limit.
        /// </summary>
        public const int DefaultMaxPendingRequests = 5;

        /// <summary>
        /// Identifier returned for a name that does not belong to any account.
        /// </summary>
        public const int NoPlayerId = 0;

        private const int RequestLifetimeDays = 7;
        private const int ReportWindowHours = 24;
        private const int MaxReportCommentLength = 300;
        private const int NoGameId = 0;
        private const int ReportersForSuspension = 3;
        private const int SuspensionHours = 24;
        private const string ReportSentLogFormat = "Player {0} reported player {1}.";
        private const string SuspendedLogFormat = "Player {0} was suspended after reports from several players.";
        private const string NullReportMessage = "The details of the report cannot be null.";
        private const string NullDependencyMessage = "A dependency of the friendship service cannot be null.";

        private readonly AppLogger _logger = new AppLogger(typeof(FriendshipService));
        private readonly IPlayerRepository _playerRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IPlayerReportRepository _reportRepository;

        /// <summary>
        /// Initializes the service with the repositories it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to find the players.</param>
        /// <param name="friendRepository">Repository used to read and save the friendships.</param>
        /// <param name="reportRepository">Repository used to save the reports between players.</param>
        public FriendshipService(
            IPlayerRepository playerRepository,
            IFriendRepository friendRepository,
            IPlayerReportRepository reportRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            _friendRepository = friendRepository
                ?? throw new ArgumentNullException(nameof(friendRepository), NullDependencyMessage);
            _reportRepository = reportRepository
                ?? throw new ArgumentNullException(nameof(reportRepository), NullDependencyMessage);
        }

        /// <summary>
        /// Gets or sets the most friends that a player can have. The server reads it from its configuration.
        /// </summary>
        public int MaxFriends { get; set; } = DefaultMaxFriends;

        /// <summary>
        /// Gets or sets the most friend requests that a player can have waiting for an answer at the same time.
        /// The server reads it from its configuration.
        /// </summary>
        public int MaxPendingRequests { get; set; } = DefaultMaxPendingRequests;

        /// <summary>
        /// Sends a friend request to the player who owns the username.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="targetUsername">Username of the player who will receive the request.</param>
        /// <returns>Outcome of the request and the name of the player who owns the username.</returns>
        public FriendRequestResult SendRequest(string displayName, string targetUsername)
        {
            var result = new FriendRequestResult(FriendRequestStatus.InvalidUsername, string.Empty);

            ForgetExpiredRequests();

            if (!string.IsNullOrWhiteSpace(targetUsername))
            {
                Player requester = _playerRepository.FindByName(displayName).FirstOrDefault();
                Player target = _playerRepository.FindByUsername(targetUsername.Trim()).FirstOrDefault();
                FriendRequestStatus status = ResolveRequest(requester, target);

                result = new FriendRequestResult(status, target?.DisplayName ?? string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="displayName">Name of the player who answers the request.</param>
        /// <param name="requesterName">Name of the player who sent the request.</param>
        /// <param name="isAccepted">True to accept the request, false to reject it.</param>
        /// <returns>Outcome of the answer.</returns>
        public FriendActionStatus RespondToRequest(string displayName, string requesterName, bool isAccepted)
        {
            ForgetExpiredRequests();

            Player addressee = _playerRepository.FindByName(displayName).FirstOrDefault();
            Player requester = _playerRepository.FindByName(requesterName).FirstOrDefault();
            FriendActionStatus status = FriendActionStatus.PlayerNotFound;

            if (addressee != null && requester != null)
            {
                Func<Friendship, bool> isPendingFromRequester =
                    friendship => friendship.RequesterId == requester.Id
                    && friendship.Status == FriendshipStatus.Pending;
                Friendship pendingFriendship = _friendRepository.FindBetween(requester.Id, addressee.Id)
                    .FirstOrDefault(isPendingFromRequester);

                status = AnswerRequest(pendingFriendship, isAccepted);
            }

            return status;
        }

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="displayName">Name of the player who removes the friend.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        public FriendActionStatus RemoveFriend(string displayName, string friendName)
        {
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();
            Player friend = _playerRepository.FindByName(friendName).FirstOrDefault();
            FriendActionStatus status = FriendActionStatus.PlayerNotFound;

            if (player != null && friend != null)
            {
                Func<Friendship, bool> isAccepted = row => row.Status == FriendshipStatus.Accepted;
                Friendship friendship = _friendRepository.FindBetween(player.Id, friend.Id)
                    .FirstOrDefault(isAccepted);

                status = DeleteFriendship(friendship);
            }

            return status;
        }

        /// <inheritdoc />
        public FriendActionStatus BlockPlayer(string displayName, string blockedName)
        {
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();
            Player blocked = _playerRepository.FindByName(blockedName).FirstOrDefault();
            FriendActionStatus status = FriendActionStatus.PlayerNotFound;

            if (player != null && blocked != null && player.Id == blocked.Id)
            {
                status = FriendActionStatus.InvalidTarget;
            }
            else if (player != null && blocked != null)
            {
                SaveBlock(player, blocked);
                status = FriendActionStatus.Success;
            }

            return status;
        }

        /// <inheritdoc />
        public FriendActionStatus UnblockPlayer(string displayName, string blockedName)
        {
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();
            Player blocked = _playerRepository.FindByName(blockedName).FirstOrDefault();
            FriendActionStatus status = FriendActionStatus.PlayerNotFound;

            if (player != null && blocked != null)
            {
                status = DeleteFriendship(FindBlockMadeBy(player, blocked));
            }

            return status;
        }

        /// <inheritdoc />
        public ReportStatus ReportPlayer(string displayName, ReportDetails details)
        {
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details), NullReportMessage);
            }

            Player reporter = _playerRepository.FindByName(displayName).FirstOrDefault();
            Player reported = null;
            ReportStatus status = ReportStatus.PlayerNotFound;

            if (!string.IsNullOrWhiteSpace(details.ReportedName))
            {
                reported = _playerRepository.FindByName(details.ReportedName.Trim()).FirstOrDefault();
            }

            if (reporter != null && reported != null)
            {
                status = SaveReportIfAllowed(reporter, reported, details);
            }

            return status;
        }

        /// <inheritdoc />
        public int FindPlayerId(string displayName)
        {
            int playerId = NoPlayerId;

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                Player player = _playerRepository.FindByName(displayName).FirstOrDefault();

                if (player != null)
                {
                    playerId = player.Id;
                }
            }

            return playerId;
        }

        /// <summary>
        /// Gets the username that other players use to send a friend request to the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The username of the player, or an empty text if the player does not exist.</returns>
        public string GetUsername(string displayName)
        {
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();

            return player?.Username ?? string.Empty;
        }

        /// <summary>
        /// Gets the names of the friends of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the friends, or an empty list if the player has none.</returns>
        public List<string> GetFriendNames(string displayName)
        {
            var names = new List<string>();
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();

            if (player != null)
            {
                names = ToNames(_friendRepository.FindFriends(player.Id));
            }

            return names;
        }

        /// <summary>
        /// Gets the names of the players who sent a friend request that the player has not answered.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the players who sent a request, or an empty list if there is none.</returns>
        public List<string> GetRequesterNames(string displayName)
        {
            ForgetExpiredRequests();

            var names = new List<string>();
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();

            if (player != null)
            {
                names = ToNames(_friendRepository.FindPendingRequesters(player.Id));
            }

            return names;
        }

        /// <summary>
        /// Gets the names of the players that the player blocked.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the blocked players, or an empty list if there is none.</returns>
        public List<string> GetBlockedNames(string displayName)
        {
            var names = new List<string>();
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();

            if (player != null)
            {
                names = ToNames(_friendRepository.FindBlockedBy(player.Id));
            }

            return names;
        }

        private static List<string> ToNames(List<Player> players)
        {
            Func<string, bool> hasName = name => !string.IsNullOrWhiteSpace(name);
            List<string> names = players
                .Select(player => player.DisplayName)
                .Where(hasName)
                .OrderBy(name => name)
                .ToList();

            return names;
        }

        private FriendRequestStatus ResolveRequest(Player requester, Player target)
        {
            FriendRequestStatus status;

            if (requester == null)
            {
                status = FriendRequestStatus.PlayerNotFound;
            }
            else if (target == null)
            {
                status = FriendRequestStatus.UsernameNotFound;
            }
            else if (target.Id == requester.Id)
            {
                status = FriendRequestStatus.SelfRequest;
            }
            else
            {
                status = CreateRequest(requester, target);
            }

            return status;
        }

        private FriendRequestStatus CreateRequest(Player requester, Player target)
        {
            Friendship existing = _friendRepository.FindBetween(requester.Id, target.Id).FirstOrDefault();
            FriendRequestStatus status;

            if (existing == null)
            {
                status = AddRequestIfAllowed(requester, target);
            }
            else if (existing.Status == FriendshipStatus.Blocked)
            {
                status = FriendRequestStatus.Blocked;
            }
            else if (existing.Status == FriendshipStatus.Accepted)
            {
                status = FriendRequestStatus.AlreadyFriends;
            }
            else if (existing.RequesterId == requester.Id)
            {
                status = FriendRequestStatus.AlreadyRequested;
            }
            else
            {
                status = FriendRequestStatus.RequestAlreadyReceived;
            }

            return status;
        }

        private FriendRequestStatus AddRequestIfAllowed(Player requester, Player target)
        {
            FriendRequestStatus status;

            if (_friendRepository.CountPendingSent(requester.Id) >= MaxPendingRequests)
            {
                status = FriendRequestStatus.TooManyPendingRequests;
            }
            else if (HasMaxFriends(requester.Id) || HasMaxFriends(target.Id))
            {
                status = FriendRequestStatus.FriendLimitReached;
            }
            else
            {
                var pendingFriendship = new Friendship
                {
                    RequesterId = requester.Id,
                    AddresseeId = target.Id,
                    Status = FriendshipStatus.Pending,
                    CreateDate = DateTime.UtcNow
                };

                _friendRepository.Add(pendingFriendship);
                status = FriendRequestStatus.RequestSent;
            }

            return status;
        }

        private bool HasEitherMaxFriends(Friendship pendingFriendship)
        {
            return HasMaxFriends(pendingFriendship.RequesterId) || HasMaxFriends(pendingFriendship.AddresseeId);
        }

        private bool HasMaxFriends(int playerId)
        {
            return _friendRepository.CountFriends(playerId) >= MaxFriends;
        }

        private FriendActionStatus AnswerRequest(Friendship pendingFriendship, bool isAccepted)
        {
            FriendActionStatus status = FriendActionStatus.RelationNotFound;

            bool isAcceptedRequest = pendingFriendship != null && isAccepted;
            bool isOverLimit = isAcceptedRequest && HasEitherMaxFriends(pendingFriendship);

            if (isOverLimit)
            {
                status = FriendActionStatus.FriendLimitReached;
            }
            else if (isAcceptedRequest)
            {
                pendingFriendship.Status = FriendshipStatus.Accepted;
                _friendRepository.UpdateStatus(pendingFriendship);
                status = FriendActionStatus.Success;
            }
            else if (pendingFriendship != null)
            {
                _friendRepository.Remove(pendingFriendship);
                status = FriendActionStatus.Success;
            }

            return status;
        }

        private ReportStatus SaveReportIfAllowed(Player reporter, Player reported, ReportDetails details)
        {
            string comment = details.Comment?.Trim() ?? string.Empty;
            ReportStatus status;

            if (reporter.Id == reported.Id)
            {
                status = ReportStatus.CannotReportSelf;
            }
            else if (comment.Length > MaxReportCommentLength)
            {
                status = ReportStatus.InvalidComment;
            }
            else if (_reportRepository.HasReportSince(reporter.Id, reported.Id, FindReportWindowStart()))
            {
                status = ReportStatus.AlreadyReported;
            }
            else
            {
                status = SaveSharedGameReport(reporter, reported, details);
            }

            return status;
        }

        private ReportStatus SaveSharedGameReport(Player reporter, Player reported, ReportDetails details)
        {
            DateTime since = FindReportWindowStart();
            int gameId = _reportRepository.FindLastSharedGameId(reporter.Id, reported.Id, since);
            ReportStatus status = ReportStatus.NoRecentGame;

            if (gameId != NoGameId || details.IsDuringGame)
            {
                PlayerReport report = CreateReport(reporter, reported, details);

                if (gameId != NoGameId)
                {
                    report.GameId = gameId;
                }

                _reportRepository.Add(report);
                _logger.Info(string.Format(
                    CultureInfo.InvariantCulture,
                    ReportSentLogFormat,
                    reporter.Id,
                    reported.Id));
                SuspendIfReportedByMany(reported.Id, since);
                status = ReportStatus.Sent;
            }

            return status;
        }

        private static DateTime FindReportWindowStart()
        {
            return DateTime.UtcNow.AddHours(-ReportWindowHours);
        }

        private void SuspendIfReportedByMany(int reportedId, DateTime since)
        {
            if (_reportRepository.CountReportersSince(reportedId, since) >= ReportersForSuspension)
            {
                DateTime suspendedUntil = DateTime.UtcNow.AddHours(SuspensionHours);
                _playerRepository.SaveLock(reportedId, LoginLock.Reported.ToString(), suspendedUntil);
                _logger.Warning(string.Format(CultureInfo.InvariantCulture, SuspendedLogFormat, reportedId));
            }
        }

        private static PlayerReport CreateReport(Player reporter, Player reported, ReportDetails details)
        {
            string comment = details.Comment?.Trim();
            var report = new PlayerReport
            {
                ReporterId = reporter.Id,
                ReportedId = reported.Id,
                Reason = details.Reason.ToString(),
                Comment = string.IsNullOrEmpty(comment) ? null : comment,
                ReportDate = DateTime.UtcNow
            };

            return report;
        }

        private void ForgetExpiredRequests()
        {
            _friendRepository.RemoveExpiredRequests(DateTime.UtcNow.AddDays(-RequestLifetimeDays));
        }

        private void SaveBlock(Player player, Player blocked)
        {
            Friendship existing = _friendRepository.FindBetween(player.Id, blocked.Id).FirstOrDefault();
            bool isAlreadyBlocked = existing != null && existing.Status == FriendshipStatus.Blocked;

            if (existing != null && !isAlreadyBlocked)
            {
                _friendRepository.Remove(existing);
            }

            if (!isAlreadyBlocked)
            {
                _friendRepository.Add(new Friendship
                {
                    RequesterId = player.Id,
                    AddresseeId = blocked.Id,
                    Status = FriendshipStatus.Blocked
                });
            }
        }

        private Friendship FindBlockMadeBy(Player player, Player blocked)
        {
            Func<Friendship, bool> isBlockMadeByPlayer =
                row => row.Status == FriendshipStatus.Blocked && row.RequesterId == player.Id;

            return _friendRepository.FindBetween(player.Id, blocked.Id).FirstOrDefault(isBlockMadeByPlayer);
        }

        private FriendActionStatus DeleteFriendship(Friendship friendship)
        {
            FriendActionStatus status = FriendActionStatus.RelationNotFound;

            if (friendship != null)
            {
                _friendRepository.Remove(friendship);
                status = FriendActionStatus.Success;
            }

            return status;
        }
    }
}

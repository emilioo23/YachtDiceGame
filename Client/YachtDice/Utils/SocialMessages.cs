using System;
using YachtDice.FriendServiceReference;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Texts that tell the player the result of the operations with friends, invitations, reports and kicks.
    /// </summary>
    public static class SocialMessages
    {
        private const string HourFormat = "t";

        /// <summary>
        /// Gets the localized message that matches the outcome of the friend request.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(FriendRequestResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case FriendRequestResultCode.RequestSent:
                    message = Strings.Friends_Result_RequestSent;
                    break;
                case FriendRequestResultCode.RequestAlreadyReceived:
                    message = Strings.Friends_Result_RequestAlreadyReceived;
                    break;
                case FriendRequestResultCode.InvalidUsername:
                    message = Strings.Friends_Result_InvalidUsername;
                    break;
                case FriendRequestResultCode.UsernameNotFound:
                    message = Strings.Friends_Result_UsernameNotFound;
                    break;
                case FriendRequestResultCode.SelfRequest:
                    message = Strings.Friends_Result_SelfRequest;
                    break;
                case FriendRequestResultCode.AlreadyFriends:
                    message = Strings.Friends_Result_AlreadyFriends;
                    break;
                case FriendRequestResultCode.AlreadyRequested:
                    message = Strings.Friends_Result_AlreadyRequested;
                    break;
                case FriendRequestResultCode.FriendLimitReached:
                    message = Strings.Friends_Result_FriendLimitReached;
                    break;
                case FriendRequestResultCode.TooManyPendingRequests:
                    message = Strings.Friends_Result_TooManyPendingRequests;
                    break;
                case FriendRequestResultCode.Blocked:
                    message = Strings.Friends_Result_Blocked;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        /// <summary>
        /// Gets the localized message that matches the outcome of the invitation.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <param name="friendName">Name of the friend who was invited.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(InvitationResultCode resultCode, string friendName)
        {
            string message;

            switch (resultCode)
            {
                case InvitationResultCode.Sent:
                    message = string.Format(Strings.Invitations_Result_Sent, friendName);
                    break;
                case InvitationResultCode.NotInRoom:
                    message = Strings.Invitations_Result_NotInRoom;
                    break;
                case InvitationResultCode.NotFriends:
                    message = Strings.Invitations_Result_NotFriends;
                    break;
                case InvitationResultCode.FriendOffline:
                    message = string.Format(Strings.Invitations_Result_FriendOffline, friendName);
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        /// <summary>
        /// Says the room and the hour of the invitation, and whether the room is already closed.
        /// </summary>
        /// <param name="invitation">Invitation that the server sent.</param>
        /// <returns>The sentence in the language of the player.</returns>
        public static string Describe(ReceivedInvitation invitation)
        {
            string hour = invitation.SentAt.ToLocalTime().ToString(HourFormat);
            string format = Strings.Friends_Invitation_ClosedFormat;

            if (invitation.IsRoomOpen)
            {
                format = Strings.Friends_Invitation_OpenFormat;
            }

            return string.Format(format, invitation.RoomCode, hour);
        }

        /// <summary>
        /// Gets the message for a result of a report.
        /// </summary>
        /// <param name="resultCode">Result that the server answered.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(ReportResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case ReportResultCode.Sent:
                    message = Strings.Report_Result_Sent;
                    break;
                case ReportResultCode.PlayerNotFound:
                    message = Strings.Report_Result_NotAllowed;
                    break;
                case ReportResultCode.NoRecentGame:
                    message = Strings.Report_Result_NotAllowed;
                    break;
                case ReportResultCode.AlreadyReported:
                    message = Strings.Report_Result_AlreadyReported;
                    break;
                case ReportResultCode.CannotReportSelf:
                    message = Strings.Report_Result_CannotReportSelf;
                    break;
                case ReportResultCode.InvalidComment:
                    message = Strings.Report_Result_InvalidComment;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        /// <summary>
        /// Gets the text that the screens show for a reason of a report.
        /// </summary>
        /// <param name="reason">Reason chosen by the player.</param>
        /// <returns>The name of the reason in the language of the player.</returns>
        public static string GetReasonName(ReportReason reason)
        {
            string name;

            switch (reason)
            {
                case ReportReason.OffensiveLanguage:
                    name = Strings.Report_Reason_OffensiveLanguage;
                    break;
                case ReportReason.Cheating:
                    name = Strings.Report_Reason_Cheating;
                    break;
                case ReportReason.UnsportingBehavior:
                    name = Strings.Report_Reason_UnsportingBehavior;
                    break;
                default:
                    name = Strings.Report_Reason_Other;
                    break;
            }

            return name;
        }

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

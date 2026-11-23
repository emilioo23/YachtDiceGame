using YachtDice.FriendServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message that tells the player how a report about another player ended.
    /// </summary>
    public static class ReportResultMessages
    {
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
    }
}

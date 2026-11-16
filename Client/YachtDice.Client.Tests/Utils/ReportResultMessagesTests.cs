using Xunit;
using YachtDice.FriendServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages that tell the player how a report ended, and the names of the reasons.
    /// </summary>
    public class ReportResultMessagesTests
    {
        private const int UnknownResultCode = 99;

        [Fact]
        public void GetMessage_Sent_ReturnsTheThankYouMessage()
        {
            string message = ReportResultMessages.GetMessage(ReportResultCode.Sent);

            Assert.Equal(Strings.Report_Result_Sent, message);
        }

        [Theory]
        [InlineData(ReportResultCode.PlayerNotFound)]
        [InlineData(ReportResultCode.NoRecentGame)]
        public void GetMessage_PlayerThatCannotBeReported_ReturnsTheNotAllowedMessage(ReportResultCode resultCode)
        {
            string message = ReportResultMessages.GetMessage(resultCode);

            Assert.Equal(Strings.Report_Result_NotAllowed, message);
        }

        [Fact]
        public void GetMessage_AlreadyReported_ReturnsTheAlreadyReportedMessage()
        {
            string message = ReportResultMessages.GetMessage(ReportResultCode.AlreadyReported);

            Assert.Equal(Strings.Report_Result_AlreadyReported, message);
        }

        [Fact]
        public void GetMessage_CannotReportSelf_ReturnsTheSelfReportMessage()
        {
            string message = ReportResultMessages.GetMessage(ReportResultCode.CannotReportSelf);

            Assert.Equal(Strings.Report_Result_CannotReportSelf, message);
        }

        [Fact]
        public void GetMessage_InvalidComment_ReturnsTheLongCommentMessage()
        {
            string message = ReportResultMessages.GetMessage(ReportResultCode.InvalidComment);

            Assert.Equal(Strings.Report_Result_InvalidComment, message);
        }

        [Fact]
        public void GetMessage_UnknownResult_ReturnsTheUnexpectedErrorMessage()
        {
            string message = ReportResultMessages.GetMessage((ReportResultCode)UnknownResultCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetReasonName_Cheating_ReturnsTheCheatingName()
        {
            string name = ReportResultMessages.GetReasonName(ReportReason.Cheating);

            Assert.Equal(Strings.Report_Reason_Cheating, name);
        }
    }
}

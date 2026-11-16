using System;
using Xunit;
using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Sentences about the last change of password and the last lock that the settings show.
    /// </summary>
    public class SecuritySummaryTextsTests
    {
        private const string DateFormat = "g";
        private const string ReportedLockReason = "Reported";
        private const string DailyLockReason = "Daily";

        private static readonly DateTime SampleDate = new DateTime(2026, 11, 16, 18, 30, 0, DateTimeKind.Utc);

        [Fact]
        public void DescribePasswordChange_NeverChanged_SaysItNeverChanged()
        {
            string text = SecuritySummaryTexts.DescribePasswordChange(new SecuritySummary());

            Assert.Equal(Strings.Settings_Security_PasswordNeverChanged, text);
        }

        [Fact]
        public void DescribePasswordChange_Changed_ShowsTheDate()
        {
            var summary = new SecuritySummary { HasPasswordChange = true, LastPasswordChange = SampleDate };
            string textExpected =
                string.Format(Strings.Settings_Security_PasswordChangedFormat, FormatDate(SampleDate));

            string text = SecuritySummaryTexts.DescribePasswordChange(summary);

            Assert.Equal(textExpected, text);
        }

        [Fact]
        public void DescribeLock_NoRecentLock_ReturnsAnEmptyText()
        {
            string text = SecuritySummaryTexts.DescribeLock(new SecuritySummary());

            Assert.Equal(string.Empty, text);
        }

        [Fact]
        public void DescribeLock_LockByReports_SaysThatPlayersReportedTheAccount()
        {
            var summary = new SecuritySummary
            {
                HasRecentLock = true,
                LockedUntil = SampleDate,
                LockReason = ReportedLockReason
            };
            string textExpected =
                string.Format(Strings.Settings_Security_LockByReportsFormat, FormatDate(SampleDate));

            string text = SecuritySummaryTexts.DescribeLock(summary);

            Assert.Equal(textExpected, text);
        }

        [Fact]
        public void DescribeLock_LockByWrongPasswords_SaysThatThereWereFailedAttempts()
        {
            var summary = new SecuritySummary
            {
                HasRecentLock = true,
                LockedUntil = SampleDate,
                LockReason = DailyLockReason
            };
            string textExpected =
                string.Format(Strings.Settings_Security_LockByAttemptsFormat, FormatDate(SampleDate));

            string text = SecuritySummaryTexts.DescribeLock(summary);

            Assert.Equal(textExpected, text);
        }

        private static string FormatDate(DateTime universalDate)
        {
            return universalDate.ToLocalTime().ToString(DateFormat);
        }
    }
}

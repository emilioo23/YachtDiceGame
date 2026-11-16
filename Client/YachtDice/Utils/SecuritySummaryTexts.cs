using System;
using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Writes the sentences of the security of the account that the settings show.
    /// </summary>
    public static class SecuritySummaryTexts
    {
        private const string DateFormat = "g";
        private const string ReportedLockReason = "Reported";

        /// <summary>
        /// Says when the password changed for the last time, or that it never changed.
        /// </summary>
        /// <param name="summary">Summary that the server sent.</param>
        /// <returns>The sentence in the language of the player.</returns>
        public static string DescribePasswordChange(SecuritySummary summary)
        {
            string text = Strings.Settings_Security_PasswordNeverChanged;

            if (summary.HasPasswordChange)
            {
                string changeDate = FormatDate(summary.LastPasswordChange);
                text = string.Format(Strings.Settings_Security_PasswordChangedFormat, changeDate);
            }

            return text;
        }

        /// <summary>
        /// Says until when the account was locked and why, or nothing if there was no recent lock.
        /// </summary>
        /// <param name="summary">Summary that the server sent.</param>
        /// <returns>The sentence, or an empty text if there was no recent lock.</returns>
        public static string DescribeLock(SecuritySummary summary)
        {
            string text = string.Empty;

            if (summary.HasRecentLock)
            {
                bool isByReports = string.Equals(summary.LockReason, ReportedLockReason, StringComparison.Ordinal);
                string format = isByReports
                    ? Strings.Settings_Security_LockByReportsFormat
                    : Strings.Settings_Security_LockByAttemptsFormat;

                text = string.Format(format, FormatDate(summary.LockedUntil));
            }

            return text;
        }

        private static string FormatDate(DateTime universalDate)
        {
            return universalDate.ToLocalTime().ToString(DateFormat);
        }
    }
}

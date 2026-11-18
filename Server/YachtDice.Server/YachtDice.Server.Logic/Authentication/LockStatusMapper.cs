namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Chooses the login status that tells the player why the account is locked.
    /// </summary>
    internal static class LockStatusMapper
    {
        /// <summary>
        /// Gives the status that matches a lock.
        /// </summary>
        /// <param name="currentLock">Lock that is running.</param>
        /// <returns>The status to answer to the player.</returns>
        public static LoginStatus ToStatus(LoginLock currentLock)
        {
            LoginStatus status;

            switch (currentLock)
            {
                case LoginLock.Daily:
                    status = LoginStatus.LockedForTheDay;
                    break;
                case LoginLock.Reported:
                    status = LoginStatus.SuspendedByReports;
                    break;
                default:
                    status = LoginStatus.TemporarilyLocked;
                    break;
            }

            return status;
        }
    }
}

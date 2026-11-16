namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Values that the status of a backup code can have in the database.
    /// </summary>
    public static class BackupCodeStatus
    {
        /// <summary>The code has not been used, so it can still replace the code sent by email.</summary>
        public const string Active = "Active";

        /// <summary>The code was already used to log in and cannot be used again.</summary>
        public const string Used = "Used";
    }
}

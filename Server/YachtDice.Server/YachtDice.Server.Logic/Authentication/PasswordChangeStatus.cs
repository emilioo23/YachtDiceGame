namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Result of a password change that a player asks for while being logged in.
    /// </summary>
    public enum PasswordChangeStatus
    {
        /// <summary>The password was changed.</summary>
        Success,

        /// <summary>The account does not exist anymore.</summary>
        PlayerNotFound,

        /// <summary>The current password that the player typed is not the right one.</summary>
        WrongCurrentPassword,

        /// <summary>The new password is too short, too long or does not have a letter and a digit.</summary>
        InvalidNewPassword,

        /// <summary>The new password is the current one or one of the last passwords of the player.</summary>
        PasswordUsedBefore,

        /// <summary>The account is locked because of too many wrong passwords.</summary>
        TooManyAttempts
    }
}

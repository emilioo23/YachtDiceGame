namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Result of checking what the player wrote to change the password from the settings, before asking the
    /// server.
    /// </summary>
    public enum PasswordChangeInputStatus
    {
        /// <summary>Everything the player wrote can be sent to the server.</summary>
        Valid,

        /// <summary>The current password was not written.</summary>
        EmptyCurrentPassword,

        /// <summary>The new password was not written.</summary>
        EmptyNewPassword,

        /// <summary>The confirmation is different from the new password.</summary>
        PasswordsDoNotMatch
    }
}

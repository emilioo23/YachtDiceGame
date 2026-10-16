namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Result of a player registration attempt.
    /// </summary>
    public enum RegistrationResult
    {
        /// <summary>The player was registered.</summary>
        Success,

        /// <summary>At least one required field is empty.</summary>
        MissingRequiredField,

        /// <summary>At least one field is longer than allowed.</summary>
        FieldTooLong,

        /// <summary>The email address does not have a valid format.</summary>
        InvalidEmail,

        /// <summary>Another player already uses the email.</summary>
        EmailAlreadyRegistered,

        /// <summary>Another player already uses the username.</summary>
        UsernameAlreadyTaken
    }
}
using System;
using System.Net.Mail;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Checks that a text has the shape of an email address.
    /// </summary>
    public static class EmailAddressValidator
    {
        /// <summary>
        /// Tells whether the text is an email address written correctly.
        /// </summary>
        /// <param name="email">Text to check, without spaces around it.</param>
        /// <returns>True if the text is a valid email address; false for empty or malformed text.</returns>
        public static bool IsValid(string email)
        {
            var isValid = false;

            if (!string.IsNullOrWhiteSpace(email) && email.Length <= CredentialRules.EmailMaxLength)
            {
                try
                {
                    var address = new MailAddress(email);
                    isValid = address.Address == email;
                }
                catch (FormatException)
                {
                    isValid = false;
                }
            }

            return isValid;
        }
    }
}

using System.Globalization;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Defines the operations to recover the password of an account with a code sent by email.
    /// </summary>
    public interface IPasswordRecoveryService
    {
        /// <summary>
        /// Generates a recovery code and sends it to the email of the account.
        /// </summary>
        /// <param name="email">Email of the account to recover.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <returns>Outcome of the request.</returns>
        PasswordResetRequestStatus RequestReset(string email, CultureInfo culture);

        /// <summary>
        /// Checks the recovery code and, if it is correct, saves the new password.
        /// </summary>
        /// <param name="confirmation">Email, code and new password written by the player.</param>
        /// <returns>Outcome of the confirmation.</returns>
        PasswordResetStatus ConfirmReset(PasswordResetConfirmation confirmation);
    }
}

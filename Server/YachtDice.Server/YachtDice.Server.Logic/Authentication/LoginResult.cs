using System;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Represents the outcome of an authentication attempt.
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class.
        /// </summary>
        /// <param name="status">Status of the login.</param>
        /// <param name="displayName">Player's display name.</param>
        public LoginResult(LoginStatus status, string displayName)
        {
            Status = status;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName),
                LogMessages.NullPlayerNameMessage);
            FirstName = string.Empty;
            Email = string.Empty;
            ChallengeId = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class for a player whose password is correct.
        /// </summary>
        /// <param name="status">Status of the login.</param>
        /// <param name="player">Player who was authenticated.</param>
        /// <param name="challengeId">Identifier of the verification challenge; empty if no code was sent.</param>
        public LoginResult(LoginStatus status, Player player, string challengeId)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), LogMessages.NullPlayerMessage);
            }

            Status = status;
            DisplayName = player.DisplayName;
            FirstName = player.FirstName;
            Email = player.Email ?? string.Empty;
            ChallengeId = challengeId ?? throw new ArgumentNullException(nameof(challengeId),
                LogMessages.NullChallengeIdMessage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class for a player who proved who they are
        /// without a password, so no verification code is needed.
        /// </summary>
        /// <param name="status">Status of the login.</param>
        /// <param name="displayName">Player's display name.</param>
        /// <param name="firstName">Player's first name.</param>
        public LoginResult(LoginStatus status, string displayName, string firstName)
            : this(status, displayName)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName),
                LogMessages.NullFirstNameMessage);
        }

        /// <summary>
        /// Status of the login attempt.
        /// </summary>
        public LoginStatus Status { get; }

        /// <summary>
        /// Player's display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// First name of the player, used only to greet them; empty if the password was not verified.
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        /// Email of the account; empty if the password was not verified.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Identifier of the verification challenge; empty if no code was sent.
        /// </summary>
        public string ChallengeId { get; }
    }
}
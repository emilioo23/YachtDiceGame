using System;
using YachtDice.Server.Data;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Represents the outcome of an authentication attempt.
    /// </summary>
    public class LoginResult
    {
        private const string NullDisplayNameMessage = "The name of the player cannot be null.";
        private const string NullFirstNameMessage = "The first name of the player cannot be null.";
        private const string NullPlayerMessage = "The player cannot be null.";
        private const string NullChallengeIdMessage = "The identifier of the challenge cannot be null.";

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class.
        /// </summary>
        /// <param name="status">Status of the login.</param>
        /// <param name="displayName">Player's display name.</param>
        public LoginResult(LoginStatus status, string displayName)
        {
            Status = status;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName), NullDisplayNameMessage);
            FirstName = string.Empty;
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
                throw new ArgumentNullException(nameof(player), NullPlayerMessage);
            }

            Status = status;
            DisplayName = player.DisplayName;
            FirstName = player.FirstName;
            ChallengeId = challengeId ?? throw new ArgumentNullException(nameof(challengeId), NullChallengeIdMessage);
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
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName), NullFirstNameMessage);
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
        /// Identifier of the verification challenge; empty if no code was sent.
        /// </summary>
        public string ChallengeId { get; }
    }
}
using System;
using YachtDice.Server.Data;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Represents the outcome of an authentication attempt.
    /// </summary>
    public class LoginResult
    {
        private const string NullValueMessage = "A value that the login result needs cannot be null.";

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class.
        /// </summary>
        /// <param name="status">Status of the login.</param>
        /// <param name="displayName">Player's display name.</param>
        public LoginResult(LoginStatus status, string displayName)
        {
            Status = status;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName), NullValueMessage);
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
                throw new ArgumentNullException(nameof(player), NullValueMessage);
            }

            Status = status;
            DisplayName = player.DisplayName;
            FirstName = player.FirstName;
            ChallengeId = challengeId ?? throw new ArgumentNullException(nameof(challengeId), NullValueMessage);
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
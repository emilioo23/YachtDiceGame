using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// The sessions of the players who logged in. Each session has a secret token that the client sends instead
    /// of the name of the player, so nobody can act in the name of another player only by knowing the name.
    /// A player has one session at a time: a new login ends the old one, and the registry remembers the tokens
    /// that ended this way so the old client can be told why. A session that waits for the code of
    /// the two-step verification does not work until the code is confirmed.
    /// </summary>
    public sealed class SessionRegistry
    {
        private const int TokenBytes = 32;
        private const string NullNameMessage = "The name of the player cannot be empty.";

        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, string> _playerNamesByToken =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<string> _replacedTokens = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, KeyValuePair<string, string>> _pendingByChallenge =
            new Dictionary<string, KeyValuePair<string, string>>(StringComparer.Ordinal);

        /// <summary>
        /// Starts a session that works right away, for a login that does not need a code.
        /// </summary>
        /// <param name="playerName">Name of the player who logged in.</param>
        /// <returns>The secret token of the session.</returns>
        /// <exception cref="ArgumentException">The name is empty.</exception>
        public string Start(string playerName)
        {
            RejectBlankName(playerName);
            string token = CreateToken();

            lock (_syncRoot)
            {
                ReplaceSessionsOf(playerName);
                _playerNamesByToken[token] = playerName;
            }

            return token;
        }

        /// <summary>
        /// Starts a session that only works after the player confirms the code of the two-step verification.
        /// </summary>
        /// <param name="playerName">Name of the player who logged in.</param>
        /// <param name="challengeId">Challenge that the code answers.</param>
        /// <returns>The secret token of the session.</returns>
        /// <exception cref="ArgumentException">The name is empty.</exception>
        public string StartPending(string playerName, string challengeId)
        {
            RejectBlankName(playerName);
            string token = CreateToken();

            lock (_syncRoot)
            {
                _pendingByChallenge[challengeId ?? string.Empty] = new KeyValuePair<string, string>(token, playerName);
            }

            return token;
        }

        /// <summary>
        /// Makes the pending session of a challenge work, after the player confirmed the code.
        /// </summary>
        /// <param name="challengeId">Challenge that the confirmed code answered.</param>
        public void Activate(string challengeId)
        {
            lock (_syncRoot)
            {
                bool isPending = _pendingByChallenge.TryGetValue(
                    challengeId ?? string.Empty,
                    out KeyValuePair<string, string> pending);

                if (isPending)
                {
                    _pendingByChallenge.Remove(challengeId);
                    ReplaceSessionsOf(pending.Value);
                    _playerNamesByToken[pending.Key] = pending.Value;
                }
            }
        }

        /// <summary>
        /// Finds the player that a verification challenge was issued to, while the verification is pending.
        /// </summary>
        /// <param name="challengeId">Identifier of the challenge that the client sent.</param>
        /// <param name="playerName">Name of the player, when the challenge is pending; empty otherwise.</param>
        /// <returns>True if the server issued the challenge and it was not completed yet.</returns>
        public bool TryGetPendingPlayerName(string challengeId, out string playerName)
        {
            bool isFound;

            lock (_syncRoot)
            {
                isFound = _pendingByChallenge.TryGetValue(
                    challengeId ?? string.Empty,
                    out KeyValuePair<string, string> pending);
                playerName = isFound ? pending.Value : string.Empty;
            }

            return isFound;
        }

        /// <summary>
        /// Finds the player of a session that works.
        /// </summary>
        /// <param name="sessionToken">Token that the client sent.</param>
        /// <param name="playerName">Name of the player, when the session works.</param>
        /// <returns>True if the token belongs to a session that works.</returns>
        public bool TryGetPlayerName(string sessionToken, out string playerName)
        {
            var isFound = false;

            playerName = string.Empty;

            lock (_syncRoot)
            {
                isFound = sessionToken != null && _playerNamesByToken.TryGetValue(sessionToken, out playerName);
            }

            return isFound;
        }

        /// <summary>
        /// Tells whether a token belonged to a session that ended because the same player logged in again.
        /// </summary>
        /// <param name="sessionToken">Token that the client sent.</param>
        /// <returns>True if a later login of the same player replaced the session of the token.</returns>
        public bool WasReplaced(string sessionToken)
        {
            bool wasReplaced;

            lock (_syncRoot)
            {
                wasReplaced = sessionToken != null && _replacedTokens.Contains(sessionToken);
            }

            return wasReplaced;
        }

        /// <summary>
        /// Ends every session of a player, for example when the player logs out or deletes the account.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        public void EndFor(string playerName)
        {
            lock (_syncRoot)
            {
                RemoveSessionsOf(playerName);
            }
        }

        /// <summary>
        /// Keeps the sessions of a player who changed the name.
        /// </summary>
        /// <param name="currentName">Name that the player had.</param>
        /// <param name="newName">New name of the player.</param>
        public void Rename(string currentName, string newName)
        {
            lock (_syncRoot)
            {
                List<string> tokens = FindTokensOf(currentName);

                foreach (string token in tokens)
                {
                    _playerNamesByToken[token] = newName;
                }
            }
        }

        private static void RejectBlankName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                throw new ArgumentException(NullNameMessage, nameof(playerName));
            }
        }

        private static string CreateToken()
        {
            var bytes = new byte[TokenBytes];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes);
        }

        private List<string> FindTokensOf(string playerName)
        {
            Func<KeyValuePair<string, string>, bool> isOfPlayer =
                session => string.Equals(session.Value, playerName, StringComparison.OrdinalIgnoreCase);

            return _playerNamesByToken.Where(isOfPlayer).Select(session => session.Key).ToList();
        }

        private void ReplaceSessionsOf(string playerName)
        {
            foreach (string token in FindTokensOf(playerName))
            {
                _replacedTokens.Add(token);
                _playerNamesByToken.Remove(token);
            }
        }

        private void RemoveSessionsOf(string playerName)
        {
            foreach (string token in FindTokensOf(playerName))
            {
                _playerNamesByToken.Remove(token);
            }
        }
    }
}

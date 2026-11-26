using System;
using System.Text;
using YachtDice.Client.Logic.Logging;

namespace YachtDice.Client.Logic.Passkeys
{
    /// <summary>
    /// Builds the client data JSON that the authenticator signs. In a browser the browser writes it; here the game
    /// writes it, with the origin that the server expects.
    /// </summary>
    public static class PasskeyClientData
    {
        private const string Format =
            "{{\"type\":\"{0}\",\"challenge\":\"{1}\",\"origin\":\"{2}\",\"crossOrigin\":false}}";

        /// <summary>
        /// Writes the client data of a ceremony.
        /// </summary>
        /// <param name="type">Type of the ceremony, create or get.</param>
        /// <param name="challenge">Challenge of the server, as base64url text.</param>
        /// <returns>The JSON as UTF-8 bytes.</returns>
        public static byte[] Create(string type, string challenge)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException(LogMessages.EmptyTypeMessage, nameof(type));
            }

            if (string.IsNullOrEmpty(challenge))
            {
                throw new ArgumentException(LogMessages.EmptyChallengeMessage, nameof(challenge));
            }

            return Encoding.UTF8.GetBytes(string.Format(Format, type, challenge, PasskeyRules.Origin));
        }
    }
}

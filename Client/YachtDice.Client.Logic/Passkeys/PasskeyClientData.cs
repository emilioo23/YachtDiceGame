using System;
using System.Text;

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
        private const string EmptyTypeMessage = "The type of the ceremony cannot be empty.";
        private const string EmptyChallengeMessage = "The challenge of the server cannot be empty.";

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
                throw new ArgumentException(EmptyTypeMessage, nameof(type));
            }

            if (string.IsNullOrEmpty(challenge))
            {
                throw new ArgumentException(EmptyChallengeMessage, nameof(challenge));
            }

            return Encoding.UTF8.GetBytes(string.Format(Format, type, challenge, PasskeyRules.Origin));
        }
    }
}

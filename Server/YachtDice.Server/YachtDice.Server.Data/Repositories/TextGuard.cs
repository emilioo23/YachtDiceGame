using System;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Guard clause shared by the repositories to stop a query or a change with an empty text before it reaches
    /// the database.
    /// </summary>
    internal static class TextGuard
    {
        private const string BlankValueMessage = "The value cannot be empty.";

        /// <summary>
        /// Throws when the text is null, empty or only spaces.
        /// </summary>
        /// <param name="value">Text received by the repository.</param>
        /// <param name="parameterName">Name of the parameter that carries the text.</param>
        /// <exception cref="ArgumentException">The text is empty.</exception>
        internal static void RejectBlank(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(BlankValueMessage, parameterName);
            }
        }
    }
}

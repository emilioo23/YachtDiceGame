namespace YachtDice.Server.Logic.Email
{
    /// <summary>
    /// Texts of an email that carries a temporary code: the keys of its subject and body in the resources, and
    /// the minutes that the code lasts, which the body shows to the player.
    /// </summary>
    public sealed class CodeEmailTemplate
    {
        /// <summary>
        /// Initializes the template.
        /// </summary>
        /// <param name="subjectResourceKey">Key of the subject in the email resources.</param>
        /// <param name="bodyResourceKey">Key of the body in the email resources.</param>
        /// <param name="expirationMinutes">Minutes that the code lasts.</param>
        public CodeEmailTemplate(string subjectResourceKey, string bodyResourceKey, int expirationMinutes)
        {
            SubjectResourceKey = subjectResourceKey;
            BodyResourceKey = bodyResourceKey;
            ExpirationMinutes = expirationMinutes;
        }

        /// <summary>
        /// Gets the key of the subject in the email resources.
        /// </summary>
        public string SubjectResourceKey { get; }

        /// <summary>
        /// Gets the key of the body in the email resources.
        /// </summary>
        public string BodyResourceKey { get; }

        /// <summary>
        /// Gets the minutes that the code lasts.
        /// </summary>
        public int ExpirationMinutes { get; }
    }
}

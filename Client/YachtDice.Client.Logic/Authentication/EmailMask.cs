namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Hides most of an email address before it is shown on screen, so a person who looks at the window cannot
    /// read the full address of the player.
    /// </summary>
    public static class EmailMask
    {
        private const int VisibleLocalPartChars = 2;
        private const int FirstCharacterIndex = 0;
        private const char MaskCharacter = '*';
        private const char AtSign = '@';

        /// <summary>
        /// Keeps the first characters of the address and the domain, and replaces the rest with asterisks.
        /// </summary>
        /// <param name="email">Email address to hide.</param>
        /// <returns>The hidden address, or the same address if its local part is too short to hide.</returns>
        public static string Mask(string email)
        {
            int atIndex = email.IndexOf(AtSign);
            string maskedEmail = email;

            if (atIndex > VisibleLocalPartChars)
            {
                string visiblePart = email.Substring(FirstCharacterIndex, VisibleLocalPartChars);
                string hiddenPart = new string(MaskCharacter, atIndex - VisibleLocalPartChars);

                maskedEmail = visiblePart + hiddenPart + email.Substring(atIndex);
            }

            return maskedEmail;
        }
    }
}

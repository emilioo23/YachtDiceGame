namespace YachtDice.Navigation
{
    /// <summary>
    /// Window that asks the player to confirm before the player closes it. When the program closes it, for
    /// example to change to another window, it must not ask.
    /// </summary>
    public interface IClosePromptingWindow
    {
        /// <summary>
        /// Makes the window close without asking the player the next time it is closed.
        /// </summary>
        void AllowClose();
    }
}

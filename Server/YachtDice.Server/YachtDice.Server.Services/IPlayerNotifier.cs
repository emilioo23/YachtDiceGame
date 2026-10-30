namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;

    /// <summary>
    /// Lets a service warn a player who is online that something happened with another player.
    /// </summary>
    public interface IPlayerNotifier
    {
        /// <summary>
        /// Sends the event to the player, if the player is online.
        /// </summary>
        /// <param name="recipientName">Name of the player who receives the event.</param>
        /// <param name="subjectName">Name of the player the event is about.</param>
        /// <param name="eventType">What happened with the subject.</param>
        void SendEvent(string recipientName, string subjectName, Contracts.PresenceEventType eventType);
    }
}

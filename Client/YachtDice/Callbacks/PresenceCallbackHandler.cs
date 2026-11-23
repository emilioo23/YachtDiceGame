using System;
using System.ServiceModel;
using YachtDice.PresenceServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles the presence callbacks from the server: players who enter or leave the game and invitations to
    /// a room.
    /// </summary>
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class PresenceCallbackHandler : IPresenceServiceCallback
    {
        /// <summary>
        /// Event triggered when another player enters or leaves the game.
        /// </summary>
        public event Action<string, PresenceEventType> PresenceChanged;

        /// <summary>
        /// Event triggered when a friend invites the player to join a room.
        /// </summary>
        public event Action<RoomInvitation> InvitationReceived;

        /// <summary>
        /// Raises the presence changed event when the server reports a change.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="eventType">What happened to the player.</param>
        void IPresenceServiceCallback.PresenceChanged(string displayName, PresenceEventType eventType)
        {
            PresenceChanged?.Invoke(displayName, eventType);
        }

        /// <summary>
        /// Raises the invitation received event when the server delivers an invitation.
        /// </summary>
        /// <param name="invitation">Who invites and the code of the room.</param>
        void IPresenceServiceCallback.InvitationReceived(RoomInvitation invitation)
        {
            InvitationReceived?.Invoke(invitation);
        }
    }
}

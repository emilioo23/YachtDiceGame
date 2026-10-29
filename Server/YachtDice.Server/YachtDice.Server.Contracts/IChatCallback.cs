﻿using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Notifications that the server sends back to a client subscribed to <see cref="IChatService"/>.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IChatCallback
    {
        /// <summary>
        /// Called when a new chat message is received.
        /// </summary>
        /// <param name="message">Message that was sent.</param>
        [OperationContract(IsOneWay = true)]
        void MessageReceived(ChatMessage message);
    }
}
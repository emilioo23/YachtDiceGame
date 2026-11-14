using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using LogicLogging = YachtDice.Server.Logic.Logging;

    /// <summary>
    /// Delivers one notification to a client callback channel. The room registry and the presence service use it,
    /// so the rule of what counts as a lost client lives in one place.
    /// </summary>
    internal static class CallbackDelivery
    {
        private const string DeliveryFailedMessage = "A notification could not be delivered to a client: ";

        /// <summary>
        /// Makes the call on the channel if it is open. A channel that is closed, broken or too slow counts as a
        /// client that cannot be reached; any other error is a bug and is not caught.
        /// </summary>
        /// <typeparam name="TCallback">Callback contract the client implements.</typeparam>
        /// <param name="callback">Callback channel of the client.</param>
        /// <param name="notification">Call to make on the channel.</param>
        /// <param name="logger">Logger of the class that sends the notification.</param>
        /// <returns>True if the notification reached the client.</returns>
        public static bool TryDeliver<TCallback>(
            TCallback callback,
            Action<TCallback> notification,
            LogicLogging.AppLogger logger)
        {
            var isDelivered = false;
            var communicationObject = (ICommunicationObject)callback;

            if (communicationObject.State == CommunicationState.Opened)
            {
                try
                {
                    notification(callback);
                    isDelivered = true;
                }
                catch (CommunicationException ex)
                {
                    logger.Warning(DeliveryFailedMessage + ex.GetType().Name);
                }
                catch (TimeoutException ex)
                {
                    logger.Warning(DeliveryFailedMessage + ex.GetType().Name);
                }
            }

            return isDelivered;
        }
    }
}

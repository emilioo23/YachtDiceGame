using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Error information sent to the client. It never includes the stack trace.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class ServiceFault
    {
        /// <summary>
        /// Code that the client translates into a message for the player.
        /// </summary>
        [DataMember]
        public ServiceErrorCode Code { get; set; }

        /// <summary>
        /// Short technical description, only useful for the logs.
        /// </summary>
        [DataMember]
        public string Message { get; set; }
    }
}

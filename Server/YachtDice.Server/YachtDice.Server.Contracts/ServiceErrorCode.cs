using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Kinds of unexpected failures that the server reports to the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum ServiceErrorCode
    {
        /// <summary>A failure that does not fit in any other category.</summary>
        [EnumMember]
        UnexpectedError,

        /// <summary>The server could not reach the database.</summary>
        [EnumMember]
        DatabaseUnavailable,

        /// <summary>The operation encountered invalid or missing data.</summary>
        [EnumMember]
        ValidationFailed
    }
}
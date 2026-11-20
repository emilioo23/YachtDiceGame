using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// Outcome of the attempt of a player to join the game of a room.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class JoinGameResult
    {
        /// <summary>
        /// Whether the player joined or why not.
        /// </summary>
        [DataMember]
        public GameResultCode Code { get; set; }

        /// <summary>
        /// State of the game when the player joined. It is empty if the player could not join.
        /// </summary>
        [DataMember]
        public GameState State { get; set; }
    }
}

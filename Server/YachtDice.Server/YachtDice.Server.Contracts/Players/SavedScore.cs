using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// Points of one category of a scorecard of a past game.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class SavedScore
    {
        /// <summary>
        /// Name of the category, the same name as the categories of the game.
        /// </summary>
        [DataMember]
        public string Category { get; set; }

        /// <summary>
        /// Points written in the category.
        /// </summary>
        [DataMember]
        public int Points { get; set; }

        /// <summary>
        /// Whether the player filled the category.
        /// </summary>
        [DataMember]
        public bool IsUsed { get; set; }
    }
}

using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// The twelve boxes of the scorecard, in the order of the scorecard.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum ScoreCategory
    {
        /// <summary>Sum of the dice that show one.</summary>
        [EnumMember]
        Ones,

        /// <summary>Sum of the dice that show two.</summary>
        [EnumMember]
        Twos,

        /// <summary>Sum of the dice that show three.</summary>
        [EnumMember]
        Threes,

        /// <summary>Sum of the dice that show four.</summary>
        [EnumMember]
        Fours,

        /// <summary>Sum of the dice that show five.</summary>
        [EnumMember]
        Fives,

        /// <summary>Sum of the dice that show six.</summary>
        [EnumMember]
        Sixes,

        /// <summary>Three dice of one value and two of another: sum of the five dice.</summary>
        [EnumMember]
        FullHouse,

        /// <summary>At least four dice of the same value: sum of four of them.</summary>
        [EnumMember]
        FourOfAKind,

        /// <summary>Dice from one to five: thirty points.</summary>
        [EnumMember]
        LittleStraight,

        /// <summary>Dice from two to six: thirty points.</summary>
        [EnumMember]
        BigStraight,

        /// <summary>Any dice: sum of the five dice.</summary>
        [EnumMember]
        Choice,

        /// <summary>Five dice of the same value: fifty points.</summary>
        [EnumMember]
        Yacht
    }
}

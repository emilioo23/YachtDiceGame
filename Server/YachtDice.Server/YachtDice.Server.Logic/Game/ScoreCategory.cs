namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// The twelve boxes of the scorecard. The first six are the upper section and the rest are the lower
    /// section. The order of the members is the order of the scorecard.
    /// </summary>
    public enum ScoreCategory
    {
        /// <summary>Sum of the dice that show one.</summary>
        Ones,

        /// <summary>Sum of the dice that show two.</summary>
        Twos,

        /// <summary>Sum of the dice that show three.</summary>
        Threes,

        /// <summary>Sum of the dice that show four.</summary>
        Fours,

        /// <summary>Sum of the dice that show five.</summary>
        Fives,

        /// <summary>Sum of the dice that show six.</summary>
        Sixes,

        /// <summary>Three dice of one value and two dice of another value: sum of the five dice.</summary>
        FullHouse,

        /// <summary>At least four dice of the same value: sum of four of them.</summary>
        FourOfAKind,

        /// <summary>Dice from one to five: thirty points.</summary>
        LittleStraight,

        /// <summary>Dice from two to six: thirty points.</summary>
        BigStraight,

        /// <summary>Any dice: sum of the five dice.</summary>
        Choice,

        /// <summary>Five dice of the same value: fifty points.</summary>
        Yacht
    }
}

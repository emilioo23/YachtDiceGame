namespace YachtDice.Models
{
    /// <summary>
    /// The explanation of one category of the scorecard, as the "How to play" window shows it.
    /// </summary>
    public class CategoryRule
    {
        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the explanation of how the category scores.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the dice of the example, written as the <c>DiceRow</c> control reads them. It is empty
        /// when the rule has no example.
        /// </summary>
        public string Dice { get; set; }

        /// <summary>
        /// Gets or sets the address of the picture shown in place of the dice, for a rule that has no dice of
        /// its own, such as the bonus or the joker. It is null when the rule shows dice.
        /// </summary>
        public string ImageSource { get; set; }
    }
}

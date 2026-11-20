namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// A cell of the grid of three rows and three columns where the pips of a face are drawn.
    /// </summary>
    public sealed class PipCell
    {
        /// <summary>
        /// Initializes the cell.
        /// </summary>
        /// <param name="row">Row of the grid, from zero at the top to two at the bottom.</param>
        /// <param name="column">Column of the grid, from zero on the left to two on the right.</param>
        public PipCell(int row, int column)
        {
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Gets the row of the grid, from zero at the top.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Gets the column of the grid, from zero on the left.
        /// </summary>
        public int Column { get; }
    }
}

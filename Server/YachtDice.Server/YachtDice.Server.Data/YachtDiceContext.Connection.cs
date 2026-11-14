namespace YachtDice.Server.Data
{
    /// <summary>
    /// Part of the context that is not generated from the model. It lets the server open the context with a
    /// connection string built at run time, so the credentials of the database do not live in a file.
    /// </summary>
    public partial class YachtDiceContext
    {
        /// <summary>
        /// Initializes the context with the given connection string.
        /// </summary>
        /// <param name="connectionString">Entity Framework connection string of the database.</param>
        public YachtDiceContext(string connectionString)
            : base(connectionString)
        {
        }
    }
}

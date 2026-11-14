namespace YachtDice.Server.Data
{
    /// <summary>
    /// Part of the context that lets the repositories open the database with a connection string built at run time.
    /// </summary>
    public partial class YachtDiceContext
    {
        /// <summary>
        /// Creates the context with an explicit connection string.
        /// </summary>
        /// <param name="connectionString">Connection string of Entity Framework.</param>
        public YachtDiceContext(string connectionString)
            : base(connectionString)
        {
        }
    }
}

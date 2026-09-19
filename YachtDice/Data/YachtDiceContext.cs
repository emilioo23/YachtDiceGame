using System.Data.Entity;
using YachtDice.Models;

namespace YachtDice.Data
{
    /// <summary>
    /// Contexto de Entity Framework que conecta con la base de datos YachtDiceDB.
    /// El nombre de esta clase coincide con el connection string en App.config.
    /// </summary>
    public class YachtDiceContext : DbContext
    {
        /// <summary>
        /// Inicializa el contexto usando el connection string YachtDiceContext.
        /// </summary>
        public YachtDiceContext() : base("name=YachtDiceContext")
        {
        }

        /// <summary>
        /// Obtiene o establece la colección de jugadores registrados en la base de datos.
        /// </summary>
        public DbSet<Player> Players { get; set; }
        /// <summary>
        /// Obtiene o establece la colección del historial de partidas por jugador.
        /// </summary>
        public DbSet<GameHistoryPlayer> GameHistoryPlayers { get; set; }

        /// <summary>
        /// Mapea las entidades a los nombres reales de tabla y evita que
        /// Entity Framework intente crear o alterar el esquema existente.
        /// </summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<YachtDiceContext>(null);

            modelBuilder.Entity<Player>().ToTable("PLAYER");
            modelBuilder.Entity<Player>().HasKey(player => player.Id);
            modelBuilder.Entity<Player>().Property(player => player.PasswordHash).HasColumnName("PwdHash");
            modelBuilder.Entity<Player>().Property(player => player.RegistrationDate).HasColumnName("RegDate");
            modelBuilder.Entity<Player>().Property(player => player.Xp).HasColumnName("XP");

            modelBuilder.Entity<GameHistoryPlayer>().ToTable("GAME_HISTORY_PLAYER");
            modelBuilder.Entity<GameHistoryPlayer>().HasKey(entry => new { entry.GameId, entry.PlayerId });

            base.OnModelCreating(modelBuilder);
        }
    }
}
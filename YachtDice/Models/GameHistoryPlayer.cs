namespace YachtDice.Models
{
    /// <summary>
    /// Representa el resultado de un jugador en una partida terminada,
    /// mapeado a la tabla GAME_HISTORY_PLAYER. Se usa para calcular
    /// puntos totales y victorias en el marcador global.
    /// </summary>
    public class GameHistoryPlayer
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int FinalScore { get; set; }
        public bool? IsWinner { get; set; }
        public string Status { get; set; }
    }
}
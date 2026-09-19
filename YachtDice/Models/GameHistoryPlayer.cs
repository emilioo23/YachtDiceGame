namespace YachtDice.Models
{
    /// <summary>
    /// Representa el resultado de un jugador en una partida terminada,
    /// mapeado a la tabla GAME_HISTORY_PLAYER. Se usa para calcular
    /// puntos totales y victorias en el marcador global.
    /// </summary>
    public class GameHistoryPlayer
    {
        /// <summary>
        /// Obtiene o establece el identificador de la partida jugada.
        /// </summary>
        public int GameId { get; set; }
        /// <summary>
        /// Obtiene o establece el identificador del jugador participante.
        /// </summary>
        public int PlayerId { get; set; }
        /// <summary>
        /// Obtiene o establece el puntaje total obtenido en la partida.
        /// </summary>
        public int FinalScore { get; set; }
        /// <summary>
        /// Obtiene o establece un valor que indica si el jugador fue el ganador.
        /// </summary>
        public bool? IsWinner { get; set; }
        /// <summary>
        /// Obtiene o establece el estado con el que el jugador finalizó la partida.
        /// </summary>
        public string Status { get; set; }
    }
}
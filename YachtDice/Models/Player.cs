using System;

namespace YachtDice.Models
{
    /// <summary>
    /// Representa un jugador registrado, mapeado a la tabla PLAYER.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Obtiene o establece el identificador único del jugador.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Obtiene o establece el identificador del avatar seleccionado.
        /// </summary>
        public int AvatarId { get; set; }
        /// <summary>
        /// Obtiene o establece el nombre de usuario único.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Obtiene o establece el correo electrónico registrado.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Obtiene o establece el hash de la contraseña.
        /// </summary>
        public string PasswordHash { get; set; }
        /// <summary>
        /// Obtiene o establece el estado actual de la cuenta del jugador.
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Obtiene o establece la fecha y hora de registro.
        /// </summary>
        public DateTime? RegistrationDate { get; set; }
        /// <summary>
        /// Obtiene o establece los puntos de experiencia acumulados.
        /// </summary>
        public int? Xp { get; set; }
        /// <summary>
        /// Obtiene o establece el nivel actual de progresión.
        /// </summary>
        public int? Level { get; set; }
        /// <summary>
        /// Obtiene o establece el código para agregar amigos.
        /// </summary>
        public string FriendCode { get; set; }
        /// <summary>
        /// Obtiene o establece el nombre visible en el juego.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Obtiene o establece la fecha de la última modificación del perfil.
        /// </summary>
        public DateTime? LastUserChange { get; set; }
        /// <summary>
        /// Obtiene o establece el nombre real del jugador.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Obtiene o establece los apellidos del jugador.
        /// </summary>
        public string LastName { get; set; }
    }
}
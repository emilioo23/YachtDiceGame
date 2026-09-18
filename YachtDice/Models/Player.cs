using System;

namespace YachtDice.Models
{
    /// <summary>
    /// Representa un jugador registrado, mapeado a la tabla PLAYER.
    /// </summary>
    public class Player
    {
        public int Id { get; set; }
        public int AvatarId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string State { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int? Xp { get; set; }
        public int? Level { get; set; }
        public string FriendCode { get; set; }
        public string DisplayName { get; set; }
        public DateTime? LastUserChange { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
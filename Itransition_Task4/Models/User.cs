using System.ComponentModel.DataAnnotations;

namespace Itransition_Task4.Models
{
    public enum UserStatus
    {
        Unverified = 0,
        Active = 1,
        Blocked = 2
    }

    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserStatus Status { get; set; } = UserStatus.Unverified;
        public DateTime RegistrationTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginTime { get; set; }
        public string? VerificationToken { get; set; }
    }
}
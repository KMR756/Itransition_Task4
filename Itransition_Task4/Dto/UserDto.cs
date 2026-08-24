using System.ComponentModel.DataAnnotations;

namespace Itransition_Task4.Dto
{
    public class UserDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string PasswordHash { get; set; } = null!;
    }
}
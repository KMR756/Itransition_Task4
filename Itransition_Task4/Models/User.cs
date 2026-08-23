using System.ComponentModel.DataAnnotations;

namespace Itransition_Task4.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }

}

using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class UserModel
    {
        [Key]
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace MoviesAdmin.Models
{
    public class UserSnapshot
    {
        public int Id { get; set; }

        [Required]
        public string Auth0UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
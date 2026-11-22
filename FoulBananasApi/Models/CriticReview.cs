
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MoviesAdmin.Models
{
    public class CriticReview
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 5)]
        public int Rating { get; set; }

        public bool IsPublished { get; set; } = false;

        [Required]
        [StringLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string ReviewerName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public DateTime? PublishedDate { get; set; }

        public int MovieId { get; set; }

        [ValidateNever]
        public Movie Movie { get; set; } = null!;
    }
}


using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MoviesAdmin.Models
{
    // Represents a movie and its associated information.
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Director { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }

        [Required]
        [StringLength(2000)]
        public string Synopsis { get; set; } = string.Empty;

        [Display(Name = "Poster URL")]
        public string? PosterUrl { get; set; }

        [Range(1, 600)]
        [Display(Name = "Runtime (minutes)")]
        public int RuntimeMinutes { get; set; }

        // Each movie belongs to one genre.
        [Range(1, int.MaxValue, ErrorMessage = "Select a genre.")]
        [Display(Name = "Genre")]
        public int GenreId { get; set; }

        [ValidateNever]
        public Genre Genre { get; set; } = null!;

        // Each movie has one content classification.
        [Range(1, int.MaxValue, ErrorMessage = "Select a content rating.")]
        [Display(Name = "Content Rating")]
        public int AudienceRatingId { get; set; }

        [ValidateNever]
        public AudienceRating AudienceRating { get; set; } = null!;

        // Optional percentage score from viewers.
        [Range(0, 100)]
        [Display(Name = "Audience Score (%)")]
        public int? AudienceScore { get; set; }
    }
}

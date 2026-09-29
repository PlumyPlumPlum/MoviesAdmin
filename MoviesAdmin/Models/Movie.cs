using System.ComponentModel.DataAnnotations;

namespace MoviesAdmin.Models
{
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
        [StringLength(50)]
        public string Genre { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Synopsis { get; set; } = string.Empty;

        [Url]
        [Display(Name = "Poster URL")]
        public string? PosterUrl { get; set; }

        [Required]
        [StringLength(10)]
        public string Rating { get; set; } = string.Empty;

        [Range(1, 600)]
        [Display(Name = "Runtime (minutes)")]
        public int RuntimeMinutes { get; set; }
    }
}
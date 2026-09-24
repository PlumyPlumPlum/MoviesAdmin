namespace MoviesAdmin.Models
{
    public class Movie
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Director { get; set; } = string.Empty;

        public int ReleaseYear { get; set; }

        public string Genre { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Optional URL for displaying movie poster artwork.
        public string? PosterUrl { get; set; }

        public string Rating { get; set; } = string.Empty;

        // Runtime is stored as total minutes for consistency.
        public int RuntimeMinutes { get; set; }
    }
}
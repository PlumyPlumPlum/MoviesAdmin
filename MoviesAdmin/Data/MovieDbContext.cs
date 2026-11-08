using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Models;

namespace MoviesAdmin.Data
{
    public class MovieDbContext : DbContext // MovieDbContext = app class DbContext = EF Core: represents a DB session...allows querying and saving data.
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) // Constructor that takes DbContextOptions<MovieDbContext> as a param. Allows configuration of the context, such as the database provider and connection string.
            : base(options)
        {
        }

        public DbSet<Movie> Movies => Set<Movie>(); // DbSet<Movie> = collection of all Movie entities that can be queried from the DB. Allows CRUD operations on the Movie entities.

        public DbSet<Genre> Genres => Set<Genre>(); // DbSet<Genre> = collection of all Genre entities that can be queried from the DB. Allows CRUD operations on the Genre entities.

        public DbSet<AudienceRating> AudienceRatings => Set<AudienceRating>(); // DbSet<AudienceRating> = collection of all AudienceRating entities that can be queried from the DB. Allows CRUD operations on the AudienceRating entities.

        public DbSet<CriticReview> CriticReviews => Set<CriticReview>(); // DbSet<CriticReview> = collection of all CriticReview entities that can be queried from the DB. Allows CRUD operations on the CriticReview entities.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Establish the relationship between a  review and its movie
            modelBuilder.Entity<CriticReview>(entity =>
            {
                entity.HasOne(review => review.Movie)
                    .WithMany(movie => movie.CriticReviews)
                    .HasForeignKey(review => review.MovieId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ensures each critic can review a particular movie only once.
                entity.HasIndex(review => new
                {
                    review.MovieId,
                    review.CreatedBy
                }).IsUnique();

                entity.Property(review => review.IsPublished)
                    .IsConcurrencyToken();

                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_CriticReview_Rating",
                    "[Rating] BETWEEN 0 AND 5"));
            });

            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Genre)
                .WithMany(g => g.Movies)
                .HasForeignKey(m => m.GenreId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Movie>()
                .HasOne(m => m.AudienceRating)
                .WithMany(a => a.Movies)
                .HasForeignKey(m => m.AudienceRatingId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
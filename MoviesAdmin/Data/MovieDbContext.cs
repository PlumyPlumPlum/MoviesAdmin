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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
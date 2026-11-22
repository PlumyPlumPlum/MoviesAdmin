
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Data;

namespace MoviesAdmin.Controllers.Api
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/movies")]
    public class MoviesApiController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public MoviesApiController(MovieDbContext context)
        {
            _context = context;
        }

        // GET: api/movies
        // Public catalogue with search, filtering and pagination.
        [HttpGet]
        public async Task<IActionResult> GetMovies(
            [FromQuery] string? search,
            [FromQuery] int? genreId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.Movies.AsNoTracking();

            // Optional title search.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(m =>
                    m.Title.Contains(term));
            }

            // Optional genre filter.
            if (genreId.HasValue)
            {
                query = query.Where(m =>
                    m.GenreId == genreId.Value);
            }

            var totalCount = await query.CountAsync();

            // Return only the fields our public website needs.
            var movies = await query
                .OrderBy(m => m.Title)
                .ThenBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Director,
                    m.ReleaseDate,
                    m.PosterUrl,
                    m.RuntimeMinutes,

                    m.GenreId,
                    Genre = m.Genre.Title,

                    m.AudienceRatingId,
                    ContentRating = m.AudienceRating.Title,
                    m.AudienceScore,

                    PublishedReviewCount =
                        m.CriticReviews.Count(r =>
                            r.IsPublished),

                    AverageCriticRating =
                        m.CriticReviews
                            .Where(r => r.IsPublished)
                            .Average(r => (double?)r.Rating)
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages =
                    (int)Math.Ceiling(
                        (double)totalCount / pageSize),
                movies
            });
        }

        // GET: api/movies/4
        // Full movie information and published critic reviews.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMovie(int id)
        {
            var movie = await _context.Movies
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Director,
                    m.ReleaseDate,
                    m.Synopsis,
                    m.PosterUrl,
                    m.RuntimeMinutes,

                    m.GenreId,
                    Genre = m.Genre.Title,

                    m.AudienceRatingId,
                    ContentRating = m.AudienceRating.Title,
                    m.AudienceScore,

                    PublishedReviewCount =
                        m.CriticReviews.Count(r =>
                            r.IsPublished),

                    AverageCriticRating =
                        m.CriticReviews
                            .Where(r => r.IsPublished)
                            .Average(r => (double?)r.Rating)
                })
                .FirstOrDefaultAsync();

            if (movie == null)
                return NotFound(new
                {
                    message = "Movie not found."
                });

            // Never expose drafts or Auth0 ownership identifiers.
            var reviews = await _context.CriticReviews
                .AsNoTracking()
                .Where(r =>
                    r.MovieId == id &&
                    r.IsPublished)
                .OrderByDescending(r => r.PublishedDate)
                .ThenByDescending(r => r.Id)
                .Select(r => new
                {
                    r.Id,
                    r.ReviewerName,
                    r.Rating,
                    r.Description,
                    r.PublishedDate
                })
                .ToListAsync();

            return Ok(new
            {
                movie.Id,
                movie.Title,
                movie.Director,
                movie.ReleaseDate,
                movie.Synopsis,
                movie.PosterUrl,
                movie.RuntimeMinutes,
                movie.GenreId,
                movie.Genre,
                movie.AudienceRatingId,
                movie.ContentRating,
                movie.AudienceScore,
                movie.PublishedReviewCount,
                movie.AverageCriticRating,
                Reviews = reviews
            });
        }
    }
}


using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Data;
using MoviesAdmin.Models;
using MoviesAdmin.ViewModels;

namespace MoviesAdmin.Controllers
{
    [Authorize(Roles = "critic")]
    public class CriticReviewsController : Controller
    {
        private readonly MovieDbContext _context;

        public CriticReviewsController(MovieDbContext context)
        {
            _context = context;
        }

        // Get the unique identifier of the logged-in critic.
        private string? CurrentCriticId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        // GET: CriticReviews
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var criticId = CurrentCriticId;

            if (string.IsNullOrWhiteSpace(criticId))
                return Forbid();

            // Retrieve only this critic's reviews.
            // Unpublished drafts appear first.
            var reviews = await _context.CriticReviews
                .AsNoTracking()
                .Include(r => r.Movie)
                .Where(r => r.CreatedBy == criticId)
                .OrderBy(r => r.IsPublished)
                .ThenByDescending(r =>
                    r.UpdatedDate ?? r.CreatedDate)
                .ToListAsync();

            return View(reviews);
        }

        // GET: CriticReviews/Create?movieId=1
        [HttpGet]
        public async Task<IActionResult> Create(int movieId)
        {
            var criticId = CurrentCriticId;

            if (string.IsNullOrWhiteSpace(criticId))
                return Forbid();

            var movie = await _context.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null)
                return NotFound();

            // A critic may only review a movie once.
            var alreadyReviewed = await _context.CriticReviews
                .AnyAsync(r => r.MovieId == movieId
                    && r.CreatedBy == criticId);

            if (alreadyReviewed)
            {
                TempData["ReviewNotice"] =
                    "You already have a review for this movie.";

                return RedirectToAction(
                    "Details", "Movies", new { id = movieId });
            }

            var model = new CriticReviewFormViewModel
            {
                MovieId = movie.Id,
                MovieTitle = movie.Title
            };

            return View(model);
        }

        // POST: CriticReviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CriticReviewFormViewModel form)
        {
            var criticId = CurrentCriticId;

            if (string.IsNullOrWhiteSpace(criticId))
                return Forbid();

            // Verify the movie exists.
            var movie = await _context.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == form.MovieId);

            if (movie == null)
                return NotFound();

            form.MovieTitle = movie.Title;

            // Reject reviews containing only whitespace.
            if (string.IsNullOrWhiteSpace(form.Description))
            {
                ModelState.AddModelError(
                    nameof(form.Description),
                    "Please enter a review.");
            }

            if (!ModelState.IsValid)
                return View(form);

            // Check whether this critic already reviewed the movie.
            var alreadyReviewed = await _context.CriticReviews
                .AnyAsync(r => r.MovieId == form.MovieId
                    && r.CreatedBy == criticId);

            if (alreadyReviewed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You already have a review for this movie.");

                return View(form);
            }

            var reviewerName =
                User.Identity?.Name
                ?? User.FindFirstValue(ClaimTypes.Email)
                ?? "Critic";

            if (reviewerName.Length > 120)
                reviewerName = reviewerName[..120];

            // All ownership and publication fields are server-controlled.
            var review = new CriticReview
            {
                MovieId = movie.Id,
                Description = form.Description.Trim(),
                Rating = form.Rating!.Value,
                CreatedBy = criticId,
                ReviewerName = reviewerName,
                CreatedDate = DateTime.UtcNow,
                IsPublished = false
            };

            _context.CriticReviews.Add(review);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException
                { Number: 2601 or 2627 })
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You already have a review for this movie.");

                return View(form);
            }

            TempData["ReviewNotice"] =
                "Draft saved successfully.";

            return RedirectToAction(
                "Details", "Movies", new { id = movie.Id });
        }

        // GET: CriticReviews/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var criticId = CurrentCriticId;

            if (string.IsNullOrWhiteSpace(criticId))
                return Forbid();

            // Only retrieve a review belonging to this critic.
            var review = await _context.CriticReviews
                .AsNoTracking()
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.CreatedBy == criticId);

            if (review == null)
                return NotFound();

            // Published reviews cannot be changed.
            if (review.IsPublished)
            {
                TempData["ReviewNotice"] =
                    "Published reviews cannot be edited.";

                return RedirectToAction(
                    "Details", "Movies",
                    new { id = review.MovieId });
            }

            ViewData["ReviewId"] = review.Id;

            var model = new CriticReviewFormViewModel
            {
                MovieId = review.MovieId,
                MovieTitle = review.Movie.Title,
                Rating = review.Rating,
                Description = review.Description
            };

            return View(model);
        }

        // POST: CriticReviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CriticReviewFormViewModel form)
        {
            var criticId = CurrentCriticId;

            if (string.IsNullOrWhiteSpace(criticId))
                return Forbid();

            var review = await _context.CriticReviews
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.CreatedBy == criticId);

            if (review == null)
                return NotFound();

            if (review.IsPublished)
            {
                TempData["ReviewNotice"] =
                    "Published reviews cannot be edited.";

                return RedirectToAction(
                    "Details", "Movies",
                    new { id = review.MovieId });
            }

            // The submitted movie must match the stored review.
            if (form.MovieId != review.MovieId)
                return BadRequest();

            form.MovieTitle = review.Movie.Title;
            ViewData["ReviewId"] = review.Id;

            if (string.IsNullOrWhiteSpace(form.Description))
            {
                ModelState.AddModelError(
                    nameof(form.Description),
                    "Please enter a review.");
            }

            if (!ModelState.IsValid)
                return View(form);

            // Only the editable fields are updated.
            review.Rating = form.Rating!.Value;
            review.Description = form.Description.Trim();
            review.UpdatedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ReviewNotice"] =
                    "This review changed while you were editing. " +
                    "Please review its latest version.";

                return RedirectToAction(
                    "Details", "Movies",
                    new { id = review.MovieId });
            }

            TempData["ReviewNotice"] =
                "Draft updated successfully.";

            return RedirectToAction(
                "Details", "Movies",
                new { id = review.MovieId });
        }

        // GET: CriticReviews/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var criticId = CurrentCriticId;

            if (string.IsNullOrWhiteSpace(criticId))
                return Forbid();

            // Find the review and confirm it belongs to this critic.
            var review = await _context.CriticReviews
                .AsNoTracking()
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.CreatedBy == criticId);

            if (review == null)
                return NotFound();

            // Published reviews cannot be deleted.
            if (review.IsPublished)
            {
                TempData["ReviewNotice"] =
                    "Published reviews cannot be deleted.";

                return RedirectToAction(
                    "Details", "Movies",
                    new { id = review.MovieId });
            }

            return View(review);
        }

        // POST: CriticReviews/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var criticId = CurrentCriticId;

            if (string.IsNullOrWhiteSpace(criticId))
                return Forbid();

            // Load a tracked review belonging to the current critic.
            var review = await _context.CriticReviews
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.CreatedBy == criticId);

            if (review == null)
                return NotFound();

            if (review.IsPublished)
            {
                TempData["ReviewNotice"] =
                    "Published reviews cannot be deleted.";

                return RedirectToAction(
                    "Details", "Movies",
                    new { id = review.MovieId });
            }

            var movieId = review.MovieId;

            _context.CriticReviews.Remove(review);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ReviewNotice"] =
                    "The review changed while you were deleting it. " +
                    "No deletion was completed by this request.";

                return RedirectToAction(
                    "Details", "Movies",
                    new { id = movieId });
            }

            TempData["ReviewNotice"] =
                "Your draft review was deleted successfully.";

            return RedirectToAction(
                "Details", "Movies",
                new { id = movieId });
        }

        // GET: CriticReviews/Publish/5
        [HttpGet]
        public async Task<IActionResult> Publish(int id)
        {
            var criticId = CurrentCriticId;

            if (string.IsNullOrWhiteSpace(criticId))
                return Forbid();

            var review = await _context.CriticReviews
                .AsNoTracking()
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.CreatedBy == criticId);

            if (review == null)
                return NotFound();

            if (review.IsPublished)
            {
                TempData["ReviewNotice"] =
                    "This review is already published.";

                return RedirectToAction(
                    "Details", "Movies",
                    new { id = review.MovieId });
            }

            return View(review);
        }

        // POST: CriticReviews/Publish/5
        [HttpPost]
        [ActionName("Publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishConfirmed(int id)
        {
            var criticId = CurrentCriticId;

            if (string.IsNullOrWhiteSpace(criticId))
                return Forbid();

            var review = await _context.CriticReviews
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.CreatedBy == criticId);

            if (review == null)
                return NotFound();

            if (review.IsPublished)
            {
                TempData["ReviewNotice"] =
                    "This review is already published.";

                return RedirectToAction(
                    "Details", "Movies",
                    new { id = review.MovieId });
            }

            // Publishing is permanent.
            review.IsPublished = true;
            review.PublishedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ReviewNotice"] =
                    "This review was changed by another request. " +
                    "Please check its current status.";

                return RedirectToAction(
                    "Details", "Movies",
                    new { id = review.MovieId });
            }

            TempData["ReviewNotice"] =
                "Your review has been published successfully.";

            return RedirectToAction(
                "Details", "Movies",
                new { id = review.MovieId });
        }
    }
}

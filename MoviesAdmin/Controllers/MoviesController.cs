
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Models;
using MoviesAdmin.Data;
using Azure;
using MoviesAdmin.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

public class MoviesController : Controller
{
    private readonly MovieDbContext _context; // Reference to the MovieDbContext instance. The controller interacts with the DB through EF Core.

    private readonly PosterStorageService _posterStorage;

    public MoviesController(
    MovieDbContext context,
    PosterStorageService posterStorage)
    {
        _context = context;
        _posterStorage = posterStorage;
    } // Constructor for MoviesController class. Takes MovieDbContext instance as a parameter: dependency injection. Constructor assigns this instance to the private _context field, the controller uses it for database operations.

    // GET: MOVIES
    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movies
            .Include(m => m.Genre)
            .Include(m => m.AudienceRating)
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();

        return View(movies);
    } // Index action method: handles GET requests to the /MOVIES endpoint. Retrieves movie list from DB, returns a view that displays this list. Asynchronous, allows non-blocking database access.

    // GET: MOVIES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies
            .Include(m => m.Genre)
            .Include(m => m.AudienceRating)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    } // Details action method...handles GET requests to the /MOVIES/Details/{id} endpoint. Takes an optional integer parameter id, which represents the ID of the movie to be displayed. If the id is null, it returns a NotFound result. Then queries the database for a movie with the specified ID. If no movie exists, returns a NotFound result. If movie is found, returns a view that displays movie details.

    // GET: MOVIES/Create
    public async Task<IActionResult> Create()
    {
        await PopulateMovieDropdownsAsync();

        return View();
    } // Create action method...handles GET requests to the /MOVIES/Create endpoint. Returns a view that contains a form for creating a new movie. Does not perform any database operations...it only prepares the view for user input.

    // POST: MOVIES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Title,Director,ReleaseDate,GenreId,Synopsis,AudienceRatingId,AudienceScore,RuntimeMinutes")]
    Movie movie,
        IFormFile? posterFile)
    {
        if (!ModelState.IsValid)
        {
            await PopulateMovieDropdownsAsync(movie);

            return View(movie);
        }

        try
        {
            // Upload the image if one was selected.
            if (posterFile != null)
            {
                movie.PosterUrl = await _posterStorage.UploadAsync(posterFile, movie.Title);
            }

            // Add the movie to the database.
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidDataException ex)
        {
            ModelState.AddModelError("posterFile", ex.Message);
        }
        catch (RequestFailedException)
        {
            ModelState.AddModelError(
                "posterFile",
                "Image upload failed. Check that Azurite is running.");
        }

        await PopulateMovieDropdownsAsync(movie);

        return View(movie);
    }


    // GET: MOVIES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
        {
            return NotFound();
        }

        await PopulateMovieDropdownsAsync(movie);

        return View(movie);
    }

    // POST: MOVIES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id,
        [Bind("Id,Title,Director,ReleaseDate,GenreId,Synopsis,AudienceRatingId,AudienceScore,RuntimeMinutes")]
    Movie movie,
        IFormFile? posterFile)
    {
        if (id == null || id != movie.Id)
        {
            return NotFound();
        }

        // Retrieve the existing movie from the database.
        var existingMovie = await _context.Movies.FindAsync(id);

        if (existingMovie == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            movie.PosterUrl = existingMovie.PosterUrl;

            await PopulateMovieDropdownsAsync(movie);

            return View(movie);
        }

        try
        {
            // Replace the poster only if a new file was chosen.
            if (posterFile != null)
            {
                existingMovie.PosterUrl =
                    await _posterStorage.UploadAsync(posterFile, movie.Title);
            }

            // Update the movie's information
            existingMovie.Title = movie.Title;
            existingMovie.Director = movie.Director;
            existingMovie.ReleaseDate = movie.ReleaseDate;
            existingMovie.GenreId = movie.GenreId;
            existingMovie.Synopsis = movie.Synopsis;
            existingMovie.AudienceRatingId = movie.AudienceRatingId;
            existingMovie.AudienceScore = movie.AudienceScore;
            existingMovie.RuntimeMinutes = movie.RuntimeMinutes;


            // Save the changes to SQL Server.
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidDataException ex)
        {
            ModelState.AddModelError("posterFile", ex.Message);
        }
        catch (RequestFailedException)
        {
            ModelState.AddModelError(
                "posterFile",
                "Poster upload failed. Check that Azurite is running.");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MovieExists(movie.Id))
            {
                return NotFound();
            }

            throw;
        }

        movie.PosterUrl = existingMovie.PosterUrl;
        await PopulateMovieDropdownsAsync(movie);
        return View(movie);
    }



    // GET: MOVIES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies
            .Include(m => m.Genre)
            .Include(m => m.AudienceRating)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // POST: MOVIES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    private async Task PopulateMovieDropdownsAsync(Movie? movie = null)
    {
        // Retrieve the available genres from SQL Server.
        var genres = await _context.Genres
            .AsNoTracking()
            .OrderBy(g => g.Title)
            .ToListAsync();

        // Retrieve the available content ratings.
        var audienceRatings = await _context.AudienceRatings
            .AsNoTracking()
            .OrderBy(a => a.Title)
            .ToListAsync();

        // Populate the Genre dropdown.
        ViewData["GenreId"] = new SelectList(
            genres,
            "Id",
            "Title",
            movie?.GenreId
        );

        // Populate the Content Rating dropdown.
        ViewData["AudienceRatingId"] = new SelectList(
            audienceRatings,
            "Id",
            "Title",
            movie?.AudienceRatingId
        );
    }


    private bool MovieExists(int? id)
    {
        return _context.Movies.Any(e => e.Id == id);
    }
}

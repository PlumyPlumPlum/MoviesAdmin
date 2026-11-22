
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Data;

var builder = WebApplication.CreateBuilder(args);

// Register our API controllers.
builder.Services.AddControllers();

// Read the existing FoulBananas database connection.
var connectionString =
    builder.Configuration.GetConnectionString("MovieDatabase")
    ?? throw new InvalidOperationException(
        "MovieDatabase connection string is missing.");

// Register our database context using SQL Server.
builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Activate our controller endpoints.
app.MapControllers();

app.Run();

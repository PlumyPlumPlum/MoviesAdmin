using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Data;
using Azure.Storage.Blobs;
using MoviesAdmin.Services;

var builder = WebApplication.CreateBuilder(args); // Creates app builder/loads configuration, services, environment settings, etc.

// Add services to the container.
builder.Services.AddControllersWithViews(); // Registers ASP.NET MVC services so the application can use controllers and Razor views.

builder.Services.AddSingleton(
    new BlobServiceClient("UseDevelopmentStorage=true"));

builder.Services.AddScoped<PosterStorageService>();

var connectionString =
    builder.Configuration.GetConnectionString("MovieDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'MovieDatabase' was not found."); // Retrieves connection string "MovieDatabase" from the config (e.g., appsettings.json). Throws an exception when not found.

builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseSqlServer(connectionString)); // Registers the MovieDbContext with the dependency injection container/configures it to use SQL Server with the specified connection string.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
} // Checks if the app is running in a non-dev environment. If yes, configures an exception handler to redirect users to "/Home/Error" page in case of unhandled exceptions. Alsoenables HTTP Strict Transport Security (HSTS) to enforce secure connections.

app.UseHttpsRedirection(); // Redirects HTTP requests to HTTPS, ensuring secure communication.
app.UseRouting(); // Enables routing capabilities, allows the app to match incoming requests to route handlers (controllers and actions).

app.UseAuthorization(); // Enables authorization capabilities, allows the app to enforce access control based on user roles and policies.

app.MapStaticAssets(); // Custom extension method. Sets up the serving of static files (i.e* CSS, images) from the wwwroot folder. Allows the app to serve static assets directly to clients.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movies}/{action=Index}/{id?}") // Movies = default controller, Index = default action, id? = optional parameter for the action method.
    .WithStaticAssets(); // Sets up default route for the app, directs requests to the Movies controller and its Index action by default. Optional id parameter used for actions that require an identifier.

app.Run();

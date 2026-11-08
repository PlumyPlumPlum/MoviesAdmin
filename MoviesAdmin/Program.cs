using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Data;
using Azure.Storage.Blobs;
using MoviesAdmin.Services;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args); // Creates app builder/loads configuration, services, environment settings, etc.

// Add services to the container.
builder.Services.AddControllersWithViews(); // Registers ASP.NET MVC services so the application can use controllers and Razor views.

// Configure Auth0 authentication
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"]
        ?? throw new InvalidOperationException(
            "Auth0 Domain is missing.");

    options.ClientId = builder.Configuration["Auth0:ClientId"]
        ?? throw new InvalidOperationException(
            "Auth0 ClientId is missing.");
});

// Configure cookie authentication options
builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization(); // Registers authorization services, enabling the application to enforce access control based on user roles and policies.

builder.Services.AddSingleton(sp =>
{
    var connectionString =
        builder.Configuration["Storage:ConnectionString"]
        ?? throw new InvalidOperationException(
            "Storage connection string is missing.");

    return new BlobServiceClient(connectionString);
});

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
app.UseAuthentication(); // Enables authentication capabilities, allows the app to identify and verify users based on their credentials (e.g., Auth0).
app.UseAuthorization(); // Enables authorization capabilities, allows the app to enforce access control based on user roles and policies.

app.MapStaticAssets(); // Registers static assets such as CSS, JavaScript, and images.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

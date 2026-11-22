FoulBananas — MoviesAdmin & Web API
FoulBananas is a movie catalogue and review platform developed for Web Application Programming at NSCC. Across Sprints 1–3, the project evolved from an ASP.NET Core MVC administration application into a two-project solution with a separate, read-only ASP.NET Core Web API.

The administration application manages movies, genres, content ratings and critic reviews. The API exposes catalogue data as JSON for the public-facing React website planned for Sprint 4.
Solution Overview
Project	Purpose
MoviesAdmin	ASP.NET Core MVC administration application with Auth0 authentication, role-based access, movie management, poster uploads and critic reviews.
FoulBananasApi	Standalone ASP.NET Core Web API with controller-based endpoints for retrieving movies and published reviews.


Both applications use Entity Framework Core and LINQ to query the same SQL Server database. The API is read-only: movie and review management remains in MoviesAdmin.
Sprint 1 — Movie Administration
- Create, view, edit and delete movies, including delete confirmation.
- Store movie records in SQL Server through Entity Framework Core.
- Validate required movie information.
- Manage movie titles, directors, release dates, synopses, genres, content ratings, runtime and poster URLs.
- Sort movie listings by release date.
- Display runtime in hours and minutes.
- Display movie posters, with a fallback when no poster is available.
Sprint 2 — Authentication, Reviews & File Handling
- Integrated Auth0 login and logout with administrator and critic roles.
- Restricted administrative movie, genre and content-rating management to authorized users.
- Added a critic My Reviews area showing reviews belonging to the signed-in critic.
- Added critic review creation, editing, ratings and draft/published states.
- Enforced one review per critic per movie through application checks and a database unique index.
- Protected review ownership on the server and prevented critics from deleting reviews.
- Added Azure Blob Storage poster uploads, with local Azurite support for development.
- Validated poster size (up to 5 MB), permitted formats (JPG, PNG and WebP) and file signatures.
- Generated unique poster filenames to avoid collisions.
- Added a confirmation step when a movie is submitted without a poster, after other required fields pass validation.
- Applied FoulBananas branding and improved the administration interface.
Sprint 3 — User Persistence & Public Web API
- Added a UserSnapshots database table storing the authenticated user's name and email alongside a unique Auth0 user identifier.
- Implemented create-or-update synchronization of those user records after successful login.
- Created a separate FoulBananasApi project using ASP.NET Core Web API controllers.
- Reused the existing EF Core models, database context and LINQ-based movie queries.
- Added endpoints for the movie catalogue and individual movie details.
- Returned only published critic reviews in public movie responses, including each reviewer's display name.
- Added title search, genre filtering, pagination, published-review counts and average published critic ratings.
- Used AsNoTracking() and projections so public responses return only the fields needed by the website.
API Endpoints
Method	Route	Description
GET	/api/movies	Get a paginated catalogue of movies.
GET	/api/movies/{id}	Get a movie by ID, including its published critic reviews and reviewer names.


Optional catalogue query parameters:
Parameter	Description	Default
search	Filter by movie title.	No filter
genreId	Filter by genre ID.	No filter
page	Page number (minimum 1).	1
pageSize	Movies per page (clamped to 1–50).	20


Example requests:
GET /api/movies
GET /api/movies?search=Jurassic
GET /api/movies?genreId=2&page=1&pageSize=10
GET /api/movies/4
The catalogue response contains page, pageSize, totalCount, totalPages and a movies array. Each movie includes core catalogue information, genre, content rating, audience score, published-review count and average published critic rating.

The detail response additionally includes the movie synopsis and a reviews array. Each public review contains id, reviewerName, rating, description and publishedDate. Unpublished drafts and private Auth0 ownership identifiers are excluded. A missing movie ID returns HTTP 404.
Note: GET /api/movies is paginated; clients can request subsequent pages to retrieve the full catalogue.

Technology Stack
- Framework: .NET 10, ASP.NET Core MVC and ASP.NET Core Web API
- Language: C#
- Data access: Entity Framework Core 10, LINQ and SQL Server
- Authentication: Auth0 with role-based authorization in MoviesAdmin
- Storage: Azure Blob Storage; Azurite for local blob development
- Interface: Razor Views, Bootstrap, HTML, CSS and JavaScript
- Development tools: Visual Studio, Docker and DataGrip
- Cloud resources: Azure SQL Database and Azure storage/hosting resources
Project Structure
MoviesAdmin.slnx
README.md
MoviesAdmin/
├── Controllers/
│   ├── AccountController.cs
│   ├── MoviesController.cs
│   ├── CriticReviewsController.cs
│   ├── GenresController.cs
│   ├── AudienceRatingsController.cs
│   └── api/MoviesApiController.cs
├── Data/MovieDbContext.cs
├── Models/
├── Migrations/
├── Views/
├── wwwroot/
└── Program.cs
FoulBananasApi/
├── Controllers/MoviesApiController.cs
├── Data/MovieDbContext.cs
├── Models/
├── Program.cs
└── FoulBananasApi.csproj
The original MVC project retains its API controller; Sprint 3 adds the separately runnable Web API project. The API and MVC projects currently contain copies of the relevant EF Core data classes and point to the same database.
Running Locally
Prerequisites
- .NET 10 SDK and a Visual Studio version with .NET 10 support (or another compatible IDE)
- Access to a SQL Server database containing the MoviesAdmin schema
- Auth0 application settings for running MoviesAdmin authentication
- Azure Blob Storage or Azurite for poster uploads in MoviesAdmin
1. Clone and build
git clone https://github.com/PlumyPlumPlum/MoviesAdmin.git
cd MoviesAdmin
dotnet restore MoviesAdmin.slnx
dotnet build MoviesAdmin.slnx
2. Configure local secrets
Use Manage User Secrets separately for each project in Visual Studio. Do not commit passwords, database connection strings or storage keys.

MoviesAdmin user secrets:
{
  "ConnectionStrings": {
    "MovieDatabase": "<SQL_SERVER_CONNECTION_STRING>"
  },
  "Auth0": {
    "Domain": "<AUTH0_DOMAIN>",
    "ClientId": "<AUTH0_CLIENT_ID>"
  },
  "Storage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  }
}
For Azure Blob Storage instead of local Azurite, provide the appropriate storage connection string through a secure configuration source.

FoulBananasApi user secrets:
{
  "ConnectionStrings": {
    "MovieDatabase": "<SAME_SQL_SERVER_CONNECTION_STRING>"
  }
}
Both projects should connect to the same existing database. Database migrations are maintained in MoviesAdmin; do not generate a second set of migrations from FoulBananasApi.
3. Run the projects
Start either project from Visual Studio or use separate terminals:
dotnet run --project MoviesAdmin/MoviesAdmin.csproj
dotnet run --project FoulBananasApi/FoulBananasApi.csproj
Use the local URLs printed when each application starts. To test the API, open /api/movies and /api/movies/4 on the FoulBananasApi address.
Sprint 3 Verification
On November 22, 2026, local testing confirmed:
- The standalone FoulBananasApi application built and started successfully.
- GET /api/movies returned five movie records from Azure SQL.
- GET /api/movies/4 returned Jurassic Park with its published reviews and reviewer names.
- The Azure SQL UserSnapshots table held administrator and critic records with names and email addresses.
The existing MVC application has Azure deployment work from Sprint 2. The separate Sprint 3 API has been verified locally; a separate Azure deployment is an optional enhancement and is not claimed here as completed.
Security & Design Notes
- Administrative actions require authentication and the appropriate role.
- Public movie endpoints are read-only and do not expose unpublished critic drafts or Auth0 ownership IDs.
- Connection strings and secrets belong in User Secrets or protected deployment configuration, not source control.
- The future React application will consume these JSON endpoints; cross-origin access must be configured for the approved frontend origin when that application is introduced.
Course Context
Built for NSCC Web Application Programming, Sprints 1–3. Sprint 4 will focus on the public-facing React website that consumes the API.

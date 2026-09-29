# MoviesAdmin

MoviesAdmin is an ASP.NET Core MVC administration application for managing
the movie catalogue for the FoulBananas movie platform.

The project was developed as part of Sprint 1 for Web Application Programming
at NSCC.

## Sprint 1 Features

The application provides complete CRUD functionality for movie records:

- Create new movies
- View all movies
- View individual movie details
- Edit existing movies
- Delete movies with confirmation
- Sort movies by release date
- Persist movie data in SQL Server
- Validate submitted movie information
- Display movie runtime in hours and minutes
- Display optional movie poster images

## Movie Data

Each movie contains:

- Title
- Director
- Release date
- Genre
- Synopsis
- Rating
- Runtime in minutes
- Optional poster URL

## Technology Stack

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server 2022
- Docker
- Razor Views
- Bootstrap
- HTML/CSS

## Project Structure

```text
MoviesAdmin/
├── Controllers/
│   └── MoviesController.cs
├── Data/
│   └── MovieDbContext.cs
├── Models/
│   └── Movie.cs
├── Migrations/
├── Views/
│   ├── Movies/
│   └── Shared/
├── wwwroot/
├── Program.cs
└── appsettings.json

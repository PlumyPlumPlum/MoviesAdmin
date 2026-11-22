
using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Data;
using MoviesAdmin.Models;

namespace MoviesAdmin.Controllers
{
    public class AccountController : Controller
    {
        private readonly MovieDbContext _context;

        public AccountController(MovieDbContext context)
        {
            _context = context;
        }

        // Start Auth0 login and return to LoginComplete afterward.
        [AllowAnonymous]
        [HttpGet]
        public async Task Login(string returnUrl = "/")
        {
            var safeReturnUrl = Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : "/";

            var loginCompleteUrl = Url.Action(
                nameof(LoginComplete),
                "Account",
                new { returnUrl = safeReturnUrl })
                ?? "/Account/LoginComplete";

            var properties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(loginCompleteUrl)
                .Build();

            await HttpContext.ChallengeAsync(
                Auth0Constants.AuthenticationScheme,
                properties);
        }

        // Save new users or update existing users after login.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> LoginComplete(
            string returnUrl = "/")
        {
            var auth0UserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            var email =
                User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue("email");

            var name =
                User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue("name")
                ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(auth0UserId) ||
                string.IsNullOrWhiteSpace(email))
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Auth0 did not provide the required user ID and email.");
            }

            var displayName = string.IsNullOrWhiteSpace(name)
                ? email
                : name;

            var snapshot = await _context.UserSnapshots
                .SingleOrDefaultAsync(
                    u => u.Auth0UserId == auth0UserId);

            if (snapshot == null)
            {
                // First login: create a new database record.
                _context.UserSnapshots.Add(new UserSnapshot
                {
                    Auth0UserId = auth0UserId,
                    Name = displayName,
                    Email = email
                });
            }
            else
            {
                // Returning user: update existing information.
                snapshot.Name = displayName;
                snapshot.Email = email;
            }

            await _context.SaveChangesAsync();

            var safeReturnUrl = Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : "/";

            return LocalRedirect(safeReturnUrl);
        }

        // End the Auth0 session and remove the local cookie.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task Logout()
        {
            var properties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri("/")
                .Build();

            await HttpContext.SignOutAsync(
                Auth0Constants.AuthenticationScheme,
                properties);

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return View();
        }
    }
}


using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoviesAdmin.Controllers
{
    public class AccountController : Controller
    {
        // Redirect the user to Auth0 to sign in.
        [AllowAnonymous]
        [HttpGet]
        public async Task Login(string returnUrl = "/")
        {
            // Only allow redirects within our application.
            var safeReturnUrl = Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : "/";

            var properties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(safeReturnUrl)
                .Build();

            await HttpContext.ChallengeAsync(
                Auth0Constants.AuthenticationScheme,
                properties);
        }

        // End the Auth0 session and remove the local login cookie.
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
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

    }
}

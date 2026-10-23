using Microsoft.AspNetCore.Mvc;
using MoviesAdmin.Models;
using System.Diagnostics;

namespace MoviesAdmin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        } // This action method handles requests to the root URL ("/") or "/Home/Index". It returns the default view associated with this action, which is typically a Razor view file named "Index.cshtml" located in the "Views/Home" folder.

        public IActionResult Privacy()
        {
            return View();
        } // This action method handles requests to the "/Home/Privacy" URL. It returns the view associated with this action, which is typically a Razor view file named "Privacy.cshtml" located in the "Views/Home" folder.

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        } // This action method handles requests to the "/Home/Error" URL. It returns the view associated with this action, which is typically a Razor view file named "Error.cshtml" located in the "Views/Home" folder. The method also creates an instance of the ErrorViewModel class, which contains information about the current request, such as the RequestId. The ResponseCache attribute is applied to this method to prevent caching of the error response, ensuring that users always receive the most up-to-date error information.
    }
}

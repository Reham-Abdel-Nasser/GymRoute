using Microsoft.AspNetCore.Mvc;

namespace GymRoute.Presentation.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // 1. Views/Home/Index.cshtml
        // 2. Views/Shared/Index.cshtml
        return View();
    }
}

using System.Diagnostics;
using GymRoute.Presentation.Infrastructure.ExceptionHandling;
using GymRoute.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace GymRoute.Presentation.Controllers;

public class HomeController(
    IMemoryCache cache,
    IHostEnvironment environment) : Controller
{
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string? requestId)
    {
        var model = GlobalExceptionHandler.GetCachedError(cache, requestId)
            ?? new ErrorViewModel
            {
                RequestId = requestId ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = StatusCodes.Status500InternalServerError,
                Title = "Something went wrong",
                Message = "An unexpected error occurred. Please try again later.",
                ShowDetails = environment.IsDevelopment()
            };

        if (string.IsNullOrEmpty(model.RequestId))
            model.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        Response.StatusCode = model.StatusCode;
        return View("Error", model);
    }

    public IActionResult ThrowTest()
        => throw new InvalidOperationException("Demo exception from /Home/ThrowTest");

    public IActionResult ThrowNotFound()
        => throw new GymNotFoundException("The requested gym resource was not found.");
}

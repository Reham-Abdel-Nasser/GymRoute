using GymRoute.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymRoute.Presentation.Controllers;

public class PlansController(
    IPlanService planService,
    ILogger<PlansController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        logger.LogInformation("Plans list requested");
        var plans = await planService.GetActivePlansAsync();
        return View(plans);
    }

    public async Task<IActionResult> Details(int id)
    {
        logger.LogInformation("Plan details requested. PlanId={PlanId}", id);

        if (id <= 0)
            return NotFound();

        var plan = await planService.GetByIdAsync(id);

        if (plan is null)
            return RedirectToAction(nameof(Index));

        return View(plan);
    }
}
using GymRoute.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymRoute.Presentation.Controllers;

public class PlansController([FromKeyedServices("Plan")] IPlanRepository planRepo) : Controller
{
    private readonly IPlanRepository _planRepo = planRepo;

    public async Task<IActionResult> Index()
    {
        var plans = await _planRepo.GetAllAsync();
        return View(plans);
    }

    public async Task<IActionResult> Details(int id)
    {
        if(id <= 0)
        {
            return NotFound();
        }

        var plan = await _planRepo.GetByIdAsync(id);

        if(plan is null)
        {
            return RedirectToAction(nameof(Index));  // return 302, location
        }

        return View(plan);  // Views/Plans/Details.cshtml
    }
}

// OCP : Open for extensions but closed for modifications
// DIP : High level modules should not depends on low level modules, both depends "Abstractions"

// High level: Order Service
// Low level: InstaPay (astraction)
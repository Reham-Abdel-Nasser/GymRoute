using GymRoute.Presentation.Diagnostics.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace GymRoute.Presentation.Controllers;

public class DiLifetimeController(
    ITransientTrackedService transientFromCtor,
    IScopedTrackedService scopedFromCtor,
    ISingletonTrackedService singletonFromCtor,
    IScopedTrackedService scopedFromCtorAgain,
    LifetimeComparisonService comparisonService) : Controller
{
    public IActionResult Index()
    {
        var comparison = comparisonService.Compare();

        ViewBag.ComparisonServiceId = comparisonService.ServiceInstanceId;
        ViewBag.CtorTransientId = transientFromCtor.InstanceId;
        ViewBag.CtorScopedId = scopedFromCtor.InstanceId;
        ViewBag.CtorScopedAgainId = scopedFromCtorAgain.InstanceId;
        ViewBag.CtorSingletonId = singletonFromCtor.InstanceId;

        return View(comparison);
    }
}

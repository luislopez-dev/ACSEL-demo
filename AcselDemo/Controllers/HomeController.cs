using AcselDemo.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcselDemo.Controllers;

public class HomeController : Controller
{
    private readonly IPolizaService _polizaService;

    public HomeController(IPolizaService polizaService)
    {
        _polizaService = polizaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var polizas = await _polizaService.GetPagedAsync(page, pageSize, cancellationToken);
        return View(polizas);
    }

    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }
}

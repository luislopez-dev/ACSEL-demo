using AcselDemo.Dtos.Polizas;
using AcselDemo.Dtos.Siniestros;
using AcselDemo.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcselDemo.Controllers;

public class PolizasController : Controller
{
    private readonly IPolizaService _polizaService;
    private readonly ISiniestroService _siniestroService;

    public PolizasController(IPolizaService polizaService, ISiniestroService siniestroService)
    {
        _polizaService = polizaService;
        _siniestroService = siniestroService;
    }

    [HttpGet]
    public async Task<IActionResult> Emitir(CancellationToken cancellationToken = default)
    {
        ViewBag.Productos = await _polizaService.GetProductosAsync(cancellationToken);
        ViewBag.Terceros = await _polizaService.GetTercerosAsync(cancellationToken);

        return View(new EmitirPolizaRequestDto
        {
            IdProducto = 1,
            IdTercero = 1,
            FechaDesde = DateTime.Today,
            FechaHasta = DateTime.Today.AddYears(1),
            CantidadCuotas = 4,
            SumaAsegurada = 20_000m
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Emitir(EmitirPolizaRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Productos = await _polizaService.GetProductosAsync(cancellationToken);
            ViewBag.Terceros = await _polizaService.GetTercerosAsync(cancellationToken);
            return View(request);
        }

        var resultado = await _polizaService.EmitirAsync(request, cancellationToken);
        TempData["MensajeExito"] = $"Póliza {resultado.NumeroPoliza} emitida correctamente.";
        return RedirectToAction(nameof(Detalle), new { id = resultado.IdPoliza });
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id, CancellationToken cancellationToken = default)
    {
        var expediente = await _polizaService.GetByIdAsync(id, cancellationToken);
        if (expediente is null)
        {
            TempData["MensajeError"] = "La póliza solicitada no existe.";
            return RedirectToAction("Index", "Home");
        }

        return View(expediente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CobrarRecibo(int id, CobrarReciboRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["MensajeError"] = ObtenerPrimerErrorModelState();
            return RedirectToAction(nameof(Detalle), new { id });
        }

        await _polizaService.CobrarReciboAsync(request, cancellationToken);
        TempData["MensajeExito"] = "Recibo cobrado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarSiniestro(int id, RegistrarSiniestroRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["MensajeError"] = ObtenerPrimerErrorModelState();
            return RedirectToAction(nameof(Detalle), new { id });
        }

        request.IdPoliza = id;
        var res = await _siniestroService.RegistrarAsync(request, cancellationToken);
        TempData["MensajeExito"] = $"Siniestro {res.NumeroSiniestro} registrado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PagarSiniestro(int id, int idSiniestro, CancellationToken cancellationToken = default)
    {
        await _siniestroService.PagarAsync(idSiniestro, cancellationToken);
        TempData["MensajeExito"] = "Siniestro marcado como pagado.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    private string ObtenerPrimerErrorModelState() =>
        ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
        ?? "Verifique los datos ingresados.";
}

using AcselDemo.Exceptions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AcselDemo.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly ITempDataDictionaryFactory _tempDataFactory;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        ITempDataDictionaryFactory tempDataFactory)
    {
        _next = next;
        _logger = logger;
        _tempDataFactory = tempDataFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OracleBusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Regla de negocio Oracle ({ErrorCode}): {Message}", ex.OracleErrorCode, ex.Message);
            SetTempError(context, ex.Message);

            var referer = context.Request.Headers.Referer.ToString();
            if (Uri.TryCreate(referer, UriKind.Absolute, out var uri) &&
                string.Equals(uri.Host, context.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect(uri.PathAndQuery);
                return;
            }

            context.Response.Redirect("/Home/Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en el servidor al procesar {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Request.Path.StartsWithSegments("/Home/Error", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Error en el servidor.");
                return;
            }

            SetTempError(context, "Error en el servidor. Intente nuevamente.");
            context.Response.Redirect("/Home/Error");
        }
    }

    private void SetTempError(HttpContext context, string message)
    {
        var tempData = _tempDataFactory.GetTempData(context);
        tempData["MensajeError"] = message;
        tempData.Save();
    }
}

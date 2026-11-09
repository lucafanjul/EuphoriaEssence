using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Services;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Error en {Metodo} {Ruta}. Identificador: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        var conflicto =
            exception is DbUpdateConcurrencyException;

        var codigo = conflicto
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status500InternalServerError;

        var mensaje = conflicto
            ? "Los datos cambiaron mientras realizabas la operación. " +
              "Actualizá la información y volvé a intentar."
            : "No se pudo completar la operación. " +
              "Volvé a consultar la información antes de reintentar.";

        var problema = new ProblemDetails
        {
            Status = codigo,
            Title = conflicto
                ? "Conflicto al actualizar los datos."
                : "Error al procesar la solicitud.",

            Detail = mensaje,
            Instance = httpContext.Request.Path.Value
        };

        problema.Extensions["mensaje"] = mensaje;
        problema.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = codigo;

        await httpContext.Response.WriteAsJsonAsync(
            problema,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }
}
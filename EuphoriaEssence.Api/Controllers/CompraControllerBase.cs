using System.Data;
using System.Security.Claims;
using EuphoriaEssence.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

// Comparte la identificación del usuario y las transacciones.
// Esta clase no publica endpoints.
public abstract class CompraControllerBase : ControllerBase
{
    protected readonly EuphoriaEssenceDbContext Context;

    protected CompraControllerBase(
        EuphoriaEssenceDbContext context)
    {
        Context = context;
    }

    protected async Task<int?> UsuarioActualAsync(
        CancellationToken ct)
    {
        var identificador = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificador, out var idUsuario))
        {
            return null;
        }

        var existe = await Context.Usuarios
            .AsNoTracking()
            .AnyAsync(
                u => u.IdUsuario == idUsuario && u.Activo,
                ct);

        return existe ? idUsuario : null;
    }

    protected async Task<IActionResult> ModificarCompraAsync(
        Func<int, CancellationToken, Task<IActionResult>> accion,
        CancellationToken ct)
    {
        var idUsuario = await UsuarioActualAsync(ct);

        if (idUsuario is null)
        {
            return Unauthorized();
        }

        await using var transaccion = await Context.Database
            .BeginTransactionAsync(
                IsolationLevel.Serializable,
                ct);

        try
        {
            // Evita modificar simultáneamente el carrito del mismo usuario.
            var recurso =
                $"EuphoriaEssence:compra:{idUsuario.Value}";

            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                DECLARE @resultado int;

                EXEC @resultado = sys.sp_getapplock
                    @Resource = {recurso},
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = 10000;

                IF @resultado < 0
                    THROW 51001, 'No se pudo bloquear el carrito.', 1;
                """,
                ct);

            var resultado = await accion(
                idUsuario.Value,
                ct);

            var codigo =
                (resultado as IStatusCodeActionResult)?.StatusCode
                ?? 200;

            if (codigo >= 400)
            {
                await transaccion.RollbackAsync(ct);
            }
            else
            {
                await transaccion.CommitAsync(ct);
            }

            return resultado;
        }
        catch (Exception ex) when (
            ex is DbUpdateException ||
            ex is SqlException
            {
                Number: 1205 or 1222 or 51001
            })
        {
            // Al salir, Dispose revierte la transacción pendiente.
            Context.ChangeTracker.Clear();

            return Conflict(new
            {
                mensaje =
                    "No se pudo completar la operación. " +
                    "Consultá el carrito y volvé a intentar."
            });
        }
    }
}
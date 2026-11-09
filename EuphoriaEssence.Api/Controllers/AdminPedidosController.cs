using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[Authorize(Roles = "Administrador")]
[ApiController]
[Route("api/admin/pedidos")]
public class AdminPedidosController : CompraControllerBase
{
    private static readonly string[] Flujo =
    [
        "Aprobado",
        "Preparando pedido",
        "Despachado",
        "En camino",
        "Llegó a sucursal",
        "Entregado"
    ];

    public AdminPedidosController(
        EuphoriaEssenceDbContext context)
        : base(context)
    {
    }

    [HttpGet("estados")]
    public async Task<IActionResult> ObtenerEstados(
        CancellationToken ct)
    {
        if (await UsuarioActualAsync(ct) is null)
        {
            return Unauthorized();
        }

        var estados = await Context.EstadosPedido
            .AsNoTracking()
            .Select(e => new
            {
                e.IdEstadoPedido,
                e.NombreEstado
            })
            .ToListAsync(ct);

        var ordenados = estados.OrderBy(e =>
        {
            var orden = Array.FindIndex(
                Flujo,
                nombre => string.Equals(
                    nombre,
                    e.NombreEstado,
                    StringComparison.OrdinalIgnoreCase));

            return orden < 0 ? int.MaxValue : orden;
        });

        return Ok(ordenados);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        CancellationToken ct,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        if (await UsuarioActualAsync(ct) is null)
        {
            return Unauthorized();
        }

        var desplazamiento =
            ((long)pagina - 1) * tamanoPagina;

        if (pagina < 1 ||
            tamanoPagina < 1 ||
            tamanoPagina > 50 ||
            desplazamiento > int.MaxValue)
        {
            return BadRequest(new
            {
                mensaje =
                    "La página debe ser mayor a cero y " +
                    "el tamaño debe estar entre 1 y 50."
            });
        }

        var consulta = Context.Pedidos.AsNoTracking();

        var total = await consulta.CountAsync(ct);

        var items = await consulta
            .OrderByDescending(p => p.FechaPedido)
            .ThenByDescending(p => p.IdPedido)
            .Skip((int)desplazamiento)
            .Take(tamanoPagina)
            .Select(p => new
            {
                p.IdPedido,
                p.FechaPedido,
                p.IdUsuario,

                cliente =
                    p.IdUsuarioNavigation.Nombre + " " +
                    p.IdUsuarioNavigation.Apellido,

                p.DireccionEnvio,
                p.IdEstadoPedido,

                estado = p.IdEstadoPedidoNavigation.NombreEstado,

                total = p.DetallePedido
                    .Sum(d => (decimal?)(
                        d.PrecioUnitario * d.Cantidad)) ?? 0m
            })
            .ToListAsync(ct);

        return Ok(new
        {
            pagina,
            tamanoPagina,
            totalRegistros = total,

            totalPaginas = (int)Math.Ceiling(
                (double)total / tamanoPagina),

            items
        });
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> Obtener(
        int id,
        CancellationToken ct)
    {
        if (await UsuarioActualAsync(ct) is null)
        {
            return Unauthorized();
        }

        var pedido = await Context.Pedidos
            .AsNoTracking()
            .Where(p => p.IdPedido == id)
            .Select(p => new
            {
                p.IdPedido,
                p.FechaPedido,
                p.DireccionEnvio,
                p.IdEstadoPedido,

                estado = p.IdEstadoPedidoNavigation.NombreEstado,

                cliente = new
                {
                    p.IdUsuario,
                    p.IdUsuarioNavigation.Nombre,
                    p.IdUsuarioNavigation.Apellido,
                    p.IdUsuarioNavigation.Email,
                    p.IdUsuarioNavigation.Telefono
                },

                items = p.DetallePedido
                    .OrderBy(d => d.IdDetallePedido)
                    .Select(d => new
                    {
                        d.IdProducto,
                        d.IdProductoNavigation.Nombre,

                        marca = d.IdProductoNavigation
                            .IdMarcaNavigation.NombreMarca,

                        d.IdProductoNavigation.TamanoMl,
                        d.Cantidad,
                        d.PrecioUnitario,

                        subtotal =
                            d.PrecioUnitario * d.Cantidad
                    })
                    .ToList(),

                total = p.DetallePedido
                    .Sum(d => (decimal?)(
                        d.PrecioUnitario * d.Cantidad)) ?? 0m
            })
            .SingleOrDefaultAsync(ct);

        if (pedido is null)
        {
            return NotFound(new
            {
                mensaje = "El pedido no existe."
            });
        }

        return Ok(pedido);
    }

    [HttpPatch("{id:int:min(1)}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int id,
        [FromBody] CambiarEstadoPedidoDto dto,
        CancellationToken ct)
    {
        if (await UsuarioActualAsync(ct) is null)
        {
            return Unauthorized();
        }

        var pedido = await Context.Pedidos
            .AsNoTracking()
            .Where(p => p.IdPedido == id)
            .Select(p => new
            {
                p.IdEstadoPedido,
                nombre = p.IdEstadoPedidoNavigation.NombreEstado
            })
            .SingleOrDefaultAsync(ct);

        if (pedido is null)
        {
            return NotFound(new
            {
                mensaje = "El pedido no existe."
            });
        }

        var nuevoEstado = await Context.EstadosPedido
            .AsNoTracking()
            .SingleOrDefaultAsync(
                e => e.IdEstadoPedido == dto.IdEstadoPedido,
                ct);

        if (nuevoEstado is null)
        {
            return BadRequest(new
            {
                mensaje = "El estado no existe."
            });
        }

        var posicionActual = Array.FindIndex(
            Flujo,
            nombre => string.Equals(
                nombre,
                pedido.nombre,
                StringComparison.OrdinalIgnoreCase));

        var posicionNueva = Array.FindIndex(
            Flujo,
            nombre => string.Equals(
                nombre,
                nuevoEstado.NombreEstado,
                StringComparison.OrdinalIgnoreCase));

        if (posicionActual < 0 ||
            posicionNueva < 0 ||
            (posicionNueva != posicionActual &&
             posicionNueva != posicionActual + 1))
        {
            return BadRequest(new
            {
                mensaje =
                    "Solo se permite mantener el estado " +
                    "o avanzar al siguiente paso.",

                flujo = Flujo
            });
        }

        var modificados = await Context.Pedidos
            .Where(p =>
                p.IdPedido == id &&
                p.IdEstadoPedido == pedido.IdEstadoPedido)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    p => p.IdEstadoPedido,
                    dto.IdEstadoPedido),
                ct);

        if (modificados == 0)
        {
            return Conflict(new
            {
                mensaje =
                    "Otro administrador modificó el pedido. " +
                    "Actualizá la lista."
            });
        }

        return Ok(new
        {
            mensaje = "Estado actualizado correctamente.",
            idPedido = id,
            estado = nuevoEstado.NombreEstado
        });
    }
}
using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.DTOs;
using EuphoriaEssence.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[Authorize(Roles = "Cliente")]
[ApiController]
[Route("api/pedidos")]
public class PedidosController : CompraControllerBase
{
    public PedidosController(
        EuphoriaEssenceDbContext context)
        : base(context)
    {
    }

    [HttpPost]
    public Task<IActionResult> Crear(
        [FromBody] CrearPedidoDto dto,
        CancellationToken ct)
    {
        return ModificarCompraAsync(async (idUsuario, token) =>
        {
            var direccion = dto.DireccionEnvio.Trim();

            if (string.IsNullOrWhiteSpace(direccion))
            {
                return BadRequest(new
                {
                    mensaje = "Ingresá la dirección de envío."
                });
            }

            var carrito = await Context.Carritos
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    c => c.IdUsuario == idUsuario && c.Activo,
                    token);

            if (carrito is null)
            {
                return BadRequest(new
                {
                    mensaje = "El carrito está vacío."
                });
            }

            var items = await Context.DetalleCarrito
                .AsNoTracking()
                .Where(d => d.IdCarrito == carrito.IdCarrito)
                .OrderBy(d => d.IdProducto)
                .Select(d => new
                {
                    d.IdProducto,
                    d.Cantidad,
                    d.IdProductoNavigation.Nombre,
                    d.IdProductoNavigation.Precio,
                    d.IdProductoNavigation.Stock,

                    disponible =
                        d.IdProductoNavigation.Activo &&
                        d.IdProductoNavigation
                            .IdMarcaNavigation.Activo &&
                        d.IdProductoNavigation
                            .IdCategoriaNavigation.Activo
                })
                .ToListAsync(token);

            if (items.Count == 0)
            {
                return BadRequest(new
                {
                    mensaje = "El carrito está vacío."
                });
            }

            var invalidos = items
                .Where(i =>
                    !i.disponible ||
                    i.Cantidad < 1 ||
                    i.Cantidad > i.Stock ||
                    i.Precio <= 0)
                .Select(i => new
                {
                    i.IdProducto,
                    i.Nombre,
                    i.Stock
                })
                .ToList();

            if (invalidos.Count > 0)
            {
                return Conflict(new
                {
                    mensaje =
                        "Revisá los productos y el stock " +
                        "antes de confirmar.",

                    productos = invalidos
                });
            }

            var estado = await Context.EstadosPedido
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    e => e.NombreEstado == "Aprobado",
                    token);

            if (estado is null)
            {
                return Problem(
                    title:
                        "Falta configurar el estado inicial Aprobado.",
                    statusCode: 500);
            }

            var pedido = new Pedidos
            {
                IdUsuario = idUsuario,
                IdEstadoPedido = estado.IdEstadoPedido,
                FechaPedido = DateTime.UtcNow,
                DireccionEnvio = direccion
            };

            foreach (var item in items)
            {
                var modificados = await Context.Productos
                    .Where(p =>
                        p.IdProducto == item.IdProducto &&
                        p.Activo &&
                        p.IdMarcaNavigation.Activo &&
                        p.IdCategoriaNavigation.Activo &&
                        p.Stock >= item.Cantidad &&
                        p.Precio == item.Precio)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            p => p.Stock,
                            p => p.Stock - item.Cantidad),
                        token);

                if (modificados != 1)
                {
                    return Conflict(new
                    {
                        mensaje =
                            "Cambió el stock o el precio. " +
                            "Revisá el carrito antes de confirmar."
                    });
                }

                pedido.DetallePedido.Add(
                    new DetallePedido
                    {
                        IdProducto = item.IdProducto,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio
                    });
            }

            Context.Pedidos.Add(pedido);

            await Context.SaveChangesAsync(token);

            await Context.DetalleCarrito
                .Where(d => d.IdCarrito == carrito.IdCarrito)
                .ExecuteDeleteAsync(token);

            return CreatedAtAction(
                nameof(Obtener),
                new { id = pedido.IdPedido },
                new
                {
                    mensaje = "Pedido realizado con éxito.",
                    pedido.IdPedido,
                    estado = estado.NombreEstado,
                    pedido.FechaPedido,
                    pedido.DireccionEnvio,

                    total = items.Sum(
                        i => i.Precio * i.Cantidad)
                });
        }, ct);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        CancellationToken ct,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 12)
    {
        var idUsuario = await UsuarioActualAsync(ct);

        if (idUsuario is null)
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

        var consulta = Context.Pedidos
            .AsNoTracking()
            .Where(p => p.IdUsuario == idUsuario.Value);

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

                estado = p.IdEstadoPedidoNavigation.NombreEstado,

                p.DireccionEnvio,

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
        var idUsuario = await UsuarioActualAsync(ct);

        if (idUsuario is null)
        {
            return Unauthorized();
        }

        var pedido = await Context.Pedidos
            .AsNoTracking()
            .Where(p =>
                p.IdPedido == id &&
                p.IdUsuario == idUsuario.Value)
            .Select(p => new
            {
                p.IdPedido,
                p.FechaPedido,
                p.DireccionEnvio,

                estado = p.IdEstadoPedidoNavigation.NombreEstado,

                items = p.DetallePedido
                    .OrderBy(d => d.IdDetallePedido)
                    .Select(d => new
                    {
                        d.IdProducto,
                        d.IdProductoNavigation.Nombre,
                        d.IdProductoNavigation.TamanoMl,
                        d.Cantidad,
                        d.PrecioUnitario,

                        subtotal =
                            d.PrecioUnitario * d.Cantidad,

                        imagen = Context.ImagenesProducto
                            .Where(i =>
                                i.IdProducto == d.IdProducto)
                            .OrderByDescending(i => i.EsPrincipal)
                            .ThenBy(i => i.IdImagen)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
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
                mensaje =
                    "El pedido no existe o no pertenece a tu cuenta."
            });
        }

        return Ok(pedido);
    }
}
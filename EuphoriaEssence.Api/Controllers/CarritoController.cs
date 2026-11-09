using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.DTOs;
using EuphoriaEssence.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[Authorize(Roles = "Cliente")]
[ApiController]
[Route("api/carrito")]
public class CarritoController : CompraControllerBase
{
    public CarritoController(
        EuphoriaEssenceDbContext context)
        : base(context)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(
        CancellationToken ct)
    {
        var idUsuario = await UsuarioActualAsync(ct);

        if (idUsuario is null)
        {
            return Unauthorized();
        }

        var items = await Context.DetalleCarrito
            .AsNoTracking()
            .Where(d =>
                d.IdCarritoNavigation.IdUsuario == idUsuario.Value &&
                d.IdCarritoNavigation.Activo)
            .OrderBy(d => d.IdProducto)
            .Select(d => new
            {
                d.IdProducto,
                d.IdProductoNavigation.Nombre,

                marca = d.IdProductoNavigation
                    .IdMarcaNavigation.NombreMarca,

                d.IdProductoNavigation.TamanoMl,

                precioUnitario = d.IdProductoNavigation.Precio,

                d.Cantidad,

                subtotal =
                    d.IdProductoNavigation.Precio * d.Cantidad,

                stock = d.IdProductoNavigation.Stock,

                disponible =
                    d.IdProductoNavigation.Activo &&
                    d.IdProductoNavigation.IdMarcaNavigation.Activo &&
                    d.IdProductoNavigation.IdCategoriaNavigation.Activo &&
                    d.IdProductoNavigation.Stock >= d.Cantidad,

                imagen = Context.ImagenesProducto
                    .Where(i => i.IdProducto == d.IdProducto)
                    .OrderByDescending(i => i.EsPrincipal)
                    .ThenBy(i => i.IdImagen)
                    .Select(i => i.UrlImagen)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return Ok(new
        {
            items,

            totalProductos = items.Sum(
                i => (long)i.Cantidad),

            total = items.Sum(i => i.subtotal),

            puedeComprar =
                items.Count > 0 &&
                items.All(i => i.disponible)
        });
    }

    [HttpPost("items")]
    public Task<IActionResult> Agregar(
        [FromBody] AgregarCarritoDto dto,
        CancellationToken ct)
    {
        return ModificarCompraAsync(async (idUsuario, token) =>
        {
            var producto = await Context.Productos
                .AsNoTracking()
                .Where(p =>
                    p.IdProducto == dto.IdProducto &&
                    p.Activo &&
                    p.IdMarcaNavigation.Activo &&
                    p.IdCategoriaNavigation.Activo)
                .Select(p => new
                {
                    p.IdProducto,
                    p.Stock
                })
                .SingleOrDefaultAsync(token);

            if (producto is null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El perfume no está disponible en el catálogo."
                });
            }

            var carrito = await Context.Carritos
                .SingleOrDefaultAsync(
                    c => c.IdUsuario == idUsuario && c.Activo,
                    token);

            DetalleCarrito? item = null;

            if (carrito is not null)
            {
                item = await Context.DetalleCarrito
                    .SingleOrDefaultAsync(
                        d =>
                            d.IdCarrito == carrito.IdCarrito &&
                            d.IdProducto == dto.IdProducto,
                        token);
            }

            var cantidad =
                (long)(item?.Cantidad ?? 0) + dto.Cantidad;

            if (cantidad > producto.Stock)
            {
                return Conflict(new
                {
                    mensaje =
                        "No hay stock suficiente para esa cantidad.",

                    stock = producto.Stock
                });
            }

            if (carrito is null)
            {
                carrito = new Carritos
                {
                    IdUsuario = idUsuario,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                Context.Carritos.Add(carrito);

                await Context.SaveChangesAsync(token);
            }

            if (item is null)
            {
                Context.DetalleCarrito.Add(
                    new DetalleCarrito
                    {
                        IdCarrito = carrito.IdCarrito,
                        IdProducto = dto.IdProducto,
                        Cantidad = (int)cantidad
                    });
            }
            else
            {
                item.Cantidad = (int)cantidad;
            }

            await Context.SaveChangesAsync(token);

            return Ok(new
            {
                mensaje = "Perfume agregado al carrito.",
                dto.IdProducto,
                cantidad
            });
        }, ct);
    }

    [HttpPut("items/{idProducto:int:min(1)}")]
    public Task<IActionResult> CambiarCantidad(
        int idProducto,
        [FromBody] CambiarCantidadDto dto,
        CancellationToken ct)
    {
        return ModificarCompraAsync(async (idUsuario, token) =>
        {
            var item = await Context.DetalleCarrito
                .SingleOrDefaultAsync(
                    d =>
                        d.IdProducto == idProducto &&
                        d.IdCarritoNavigation.IdUsuario == idUsuario &&
                        d.IdCarritoNavigation.Activo,
                    token);

            if (item is null)
            {
                return NotFound(new
                {
                    mensaje = "El perfume no está en tu carrito."
                });
            }

            var producto = await Context.Productos
                .AsNoTracking()
                .Where(p =>
                    p.IdProducto == idProducto &&
                    p.Activo &&
                    p.IdMarcaNavigation.Activo &&
                    p.IdCategoriaNavigation.Activo)
                .Select(p => new
                {
                    p.Stock
                })
                .SingleOrDefaultAsync(token);

            if (producto is null ||
                dto.Cantidad > producto.Stock)
            {
                return Conflict(new
                {
                    mensaje =
                        "El perfume no está disponible en esa cantidad."
                });
            }

            item.Cantidad = dto.Cantidad;

            await Context.SaveChangesAsync(token);

            return Ok(new
            {
                mensaje = "Cantidad actualizada correctamente.",
                idProducto,
                dto.Cantidad
            });
        }, ct);
    }

    [HttpDelete("items/{idProducto:int:min(1)}")]
    public Task<IActionResult> Quitar(
        int idProducto,
        CancellationToken ct)
    {
        return ModificarCompraAsync(async (idUsuario, token) =>
        {
            var borrados = await Context.DetalleCarrito
                .Where(d =>
                    d.IdProducto == idProducto &&
                    d.IdCarritoNavigation.IdUsuario == idUsuario &&
                    d.IdCarritoNavigation.Activo)
                .ExecuteDeleteAsync(token);

            if (borrados == 0)
            {
                return NotFound(new
                {
                    mensaje = "El perfume no está en tu carrito."
                });
            }

            return Ok(new
            {
                mensaje = "Perfume eliminado del carrito."
            });
        }, ct);
    }

    [HttpDelete]
    public Task<IActionResult> Vaciar(
        CancellationToken ct)
    {
        return ModificarCompraAsync(async (idUsuario, token) =>
        {
            await Context.DetalleCarrito
                .Where(d =>
                    d.IdCarritoNavigation.IdUsuario == idUsuario &&
                    d.IdCarritoNavigation.Activo)
                .ExecuteDeleteAsync(token);

            return Ok(new
            {
                mensaje = "Carrito vaciado correctamente."
            });
        }, ct);
    }
}
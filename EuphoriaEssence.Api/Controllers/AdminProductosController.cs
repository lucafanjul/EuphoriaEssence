using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.DTOs;
using EuphoriaEssence.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[Authorize(Roles = "Administrador")]
[ApiController]
[Route("api/admin/productos")]
public class AdminProductosController : ControllerBase
{
    private readonly EuphoriaEssenceDbContext _context;

    public AdminProductosController(EuphoriaEssenceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        CancellationToken cancellationToken,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        if (pagina < 1 || tamanoPagina < 1 || tamanoPagina > 50)
        {
            return BadRequest(new
            {
                mensaje = "La página debe ser mayor a cero y el tamaño debe estar entre 1 y 50."
            });
        }

        var desplazamiento = ((long)pagina - 1) * tamanoPagina;

        if (desplazamiento > int.MaxValue)
        {
            return BadRequest(new
            {
                mensaje = "El número de página es demasiado grande."
            });
        }

        var consulta = _context.Productos.AsNoTracking();

        var total = await consulta.CountAsync(cancellationToken);

        var productos = await consulta
            .OrderBy(p => p.Nombre)
            .ThenBy(p => p.IdProducto)
            .Skip((int)desplazamiento)
            .Take(tamanoPagina)
            .Select(p => new
            {
                p.IdProducto,
                p.Nombre,
                p.Precio,
                p.Stock,
                p.TamanoMl,
                p.Concentracion,
                p.IdMarca,
                marca = p.IdMarcaNavigation.NombreMarca,
                p.IdCategoria,
                categoria = p.IdCategoriaNavigation.NombreCategoria,
                p.Activo
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            pagina,
            tamanoPagina,
            totalRegistros = total,
            totalPaginas = (int)Math.Ceiling(
                (double)total / tamanoPagina),
            items = productos
        });
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> Obtener(
        int id,
        CancellationToken cancellationToken)
    {
        var producto = await _context.Productos
            .AsNoTracking()
            .Where(p => p.IdProducto == id)
            .Select(p => new
            {
                p.IdProducto,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.TamanoMl,
                p.Concentracion,
                p.IdMarca,
                p.IdCategoria,
                p.Activo
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (producto is null)
        {
            return NotFound(new
            {
                mensaje = "El perfume no existe."
            });
        }

        return Ok(producto);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearProductoDto dto,
        CancellationToken cancellationToken)
    {
        var error = await ValidarProductoAsync(
            dto,
            cancellationToken);

        if (error is not null)
        {
            return BadRequest(new { mensaje = error });
        }

        var producto = new Productos
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = NormalizarTexto(dto.Descripcion),
            Precio = dto.Precio,
            Stock = dto.Stock,
            TamanoMl = dto.TamanoMl,
            Concentracion = dto.Concentracion.Trim(),
            IdMarca = dto.IdMarca,
            IdCategoria = dto.IdCategoria,
            Activo = true
        };

        _context.Productos.Add(producto);

        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(Obtener),
            new { id = producto.IdProducto },
            new
            {
                mensaje = "Perfume creado correctamente.",
                producto.IdProducto
            });
    }

    [HttpPut("{id:int:min(1)}")]
    public async Task<IActionResult> Editar(
        int id,
        [FromBody] GuardarProductoDto dto,
        CancellationToken cancellationToken)
    {
        var producto = await _context.Productos
            .SingleOrDefaultAsync(
                p => p.IdProducto == id,
                cancellationToken);

        if (producto is null)
        {
            return NotFound(new
            {
                mensaje = "El perfume no existe."
            });
        }

        var error = await ValidarProductoAsync(
            dto,
            cancellationToken);

        if (error is not null)
        {
            return BadRequest(new { mensaje = error });
        }

        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = NormalizarTexto(dto.Descripcion);
        producto.Precio = dto.Precio;
        producto.TamanoMl = dto.TamanoMl;
        producto.Concentracion = dto.Concentracion.Trim();
        producto.IdMarca = dto.IdMarca;
        producto.IdCategoria = dto.IdCategoria;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            mensaje = "Perfume actualizado correctamente.",
            producto.IdProducto
        });
    }

    [HttpPatch("{id:int:min(1)}/stock")]
    public async Task<IActionResult> ActualizarStock(
    int id,
    [FromBody] ActualizarStockDto dto,
    CancellationToken cancellationToken)
    {
        var modificados = await _context.Productos
            .Where(p => p.IdProducto == id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    p => p.Stock,
                    dto.Stock!.Value),
                cancellationToken);

        if (modificados == 0)
        {
            return NotFound(new
            {
                mensaje = "El perfume no existe."
            });
        }

        var producto = await _context.Productos
            .AsNoTracking()
            .Where(p => p.IdProducto == id)
            .Select(p => new
            {
                p.IdProducto,
                p.Stock,
                p.Activo,

                disponible =
                    p.Activo &&
                    p.IdMarcaNavigation.Activo &&
                    p.IdCategoriaNavigation.Activo &&
                    p.Stock > 0
            })
            .SingleAsync(cancellationToken);

        return Ok(new
        {
            mensaje = !producto.Activo
                ? "Stock actualizado. El perfume continúa retirado del catálogo."
                : producto.Stock == 0
                    ? "El perfume quedó sin stock."
                    : "Stock actualizado correctamente.",

            producto.IdProducto,
            producto.Stock,
            producto.Activo,
            producto.disponible
        });
    }
    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        var modificados = await _context.Productos
            .Where(p => p.IdProducto == id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    p => p.Activo,
                    false),
                cancellationToken);

        if (modificados == 0)
        {
            return NotFound(new
            {
                mensaje = "El perfume no existe."
            });
        }

        return Ok(new
        {
            mensaje = "Perfume eliminado del catálogo correctamente.",
            idProducto = id
        });
    }
    private async Task<string?> ValidarProductoAsync(
        GuardarProductoDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return "El nombre es obligatorio.";
        }

        if (string.IsNullOrWhiteSpace(dto.Concentracion))
        {
            return "La concentración es obligatoria.";
        }

        if (decimal.Round(dto.Precio, 2) != dto.Precio)
        {
            return "El precio admite como máximo dos decimales.";
        }

        var marcaValida = await _context.Marcas
            .AnyAsync(
                m => m.IdMarca == dto.IdMarca && m.Activo,
                cancellationToken);

        if (!marcaValida)
        {
            return "La marca no existe o está inactiva.";
        }

        var categoriaValida = await _context.Categorias
            .AnyAsync(
                c => c.IdCategoria == dto.IdCategoria && c.Activo,
                cancellationToken);

        if (!categoriaValida)
        {
            return "La categoría no existe o está inactiva.";
        }

        return null;
    }

    private static string? NormalizarTexto(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto)
            ? null
            : texto.Trim();
    }
}
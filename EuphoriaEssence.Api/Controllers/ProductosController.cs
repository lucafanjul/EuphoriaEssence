using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly EuphoriaEssenceDbContext _context;

    public ProductosController(EuphoriaEssenceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        CancellationToken cancellationToken,
        [FromQuery] string? buscar = null,
        [FromQuery] int? idMarca = null,
        [FromQuery] int? idCategoria = null,
        [FromQuery] bool soloConStock = false,
        [FromQuery] string ordenar = "nombre",
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 12)
    {
        if (pagina < 1 || tamanoPagina < 1 || tamanoPagina > 50)
        {
            return BadRequest(new
            {
                mensaje = "La página debe ser mayor a cero y el tamaño debe estar entre 1 y 50."
            });
        }

        if (idMarca.HasValue && idMarca.Value < 1 ||
            idCategoria.HasValue && idCategoria.Value < 1)
        {
            return BadRequest(new
            {
                mensaje = "La marca y la categoría deben tener identificadores válidos."
            });
        }

        ordenar = ordenar.Trim().ToLowerInvariant();

        if (ordenar != "nombre" &&
            ordenar != "precio_asc" &&
            ordenar != "precio_desc")
        {
            return BadRequest(new
            {
                mensaje = "Orden válido: nombre, precio_asc o precio_desc."
            });
        }

        var consulta = _context.Productos
            .AsNoTracking()
            .Where(p =>
                p.Activo &&
                p.IdMarcaNavigation.Activo &&
                p.IdCategoriaNavigation.Activo);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();

            consulta = consulta.Where(p =>
                p.Nombre.Contains(texto) ||
                p.IdMarcaNavigation.NombreMarca.Contains(texto));
        }

        if (idMarca.HasValue)
        {
            consulta = consulta.Where(p => p.IdMarca == idMarca.Value);
        }

        if (idCategoria.HasValue)
        {
            consulta = consulta.Where(
                p => p.IdCategoria == idCategoria.Value);
        }

        if (soloConStock)
        {
            consulta = consulta.Where(p => p.Stock > 0);
        }

        var total = await consulta.CountAsync(cancellationToken);

        var ordenada = ordenar switch
        {
            "precio_asc" => consulta
                .OrderBy(p => p.Precio)
                .ThenBy(p => p.IdProducto),

            "precio_desc" => consulta
                .OrderByDescending(p => p.Precio)
                .ThenBy(p => p.IdProducto),

            _ => consulta
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.IdProducto)
        };

        var desplazamiento = ((long)pagina - 1) * tamanoPagina;

        if (desplazamiento > int.MaxValue)
        {
            return BadRequest(new
            {
                mensaje = "El número de página es demasiado grande."
            });
        }

        var productos = await ordenada
            .Skip((int)desplazamiento)
            .Take(tamanoPagina)
            .Select(p => new ProductoResumenDto
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                Marca = p.IdMarcaNavigation.NombreMarca,
                Categoria = p.IdCategoriaNavigation.NombreCategoria,
                Precio = p.Precio,
                Stock = p.Stock,
                TamanoMl = p.TamanoMl,
                Concentracion = p.Concentracion,

                ImagenPrincipal = _context.ImagenesProducto
                    .Where(i => i.IdProducto == p.IdProducto)
                    .OrderByDescending(i => i.EsPrincipal)
                    .ThenBy(i => i.IdImagen)
                    .Select(i => i.UrlImagen)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return Ok(new RespuestaPaginadaDto<ProductoResumenDto>
        {
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = total,
            Items = productos
        });
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> ObtenerDetalle(
        int id,
        CancellationToken cancellationToken)
    {
        var producto = await _context.Productos
            .AsNoTracking()
            .Where(p =>
                p.IdProducto == id &&
                p.Activo &&
                p.IdMarcaNavigation.Activo &&
                p.IdCategoriaNavigation.Activo)
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
                marca = p.IdMarcaNavigation.NombreMarca,
                p.IdCategoria,
                categoria = p.IdCategoriaNavigation.NombreCategoria,
                disponible = p.Stock > 0
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (producto is null)
        {
            return NotFound(new
            {
                mensaje = "El perfume no existe o no está disponible en el catálogo."
            });
        }

        var imagenes = await _context.ImagenesProducto
            .AsNoTracking()
            .Where(i => i.IdProducto == id)
            .OrderByDescending(i => i.EsPrincipal)
            .ThenBy(i => i.IdImagen)
            .Select(i => new
            {
                i.IdImagen,
                i.UrlImagen,
                i.EsPrincipal
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            producto,
            imagenes
        });
    }
}
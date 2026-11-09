using EuphoriaEssence.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/catalogo")]
public class CatalogoController : ControllerBase
{
    private readonly EuphoriaEssenceDbContext _context;

    public CatalogoController(EuphoriaEssenceDbContext context)
    {
        _context = context;
    }

    [HttpGet("marcas")]
    public async Task<IActionResult> ObtenerMarcas(
        CancellationToken cancellationToken)
    {
        var marcas = await _context.Marcas
            .AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.NombreMarca)
            .Select(m => new
            {
                m.IdMarca,
                m.NombreMarca
            })
            .ToListAsync(cancellationToken);

        return Ok(marcas);
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> ObtenerCategorias(
        CancellationToken cancellationToken)
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.NombreCategoria)
            .Select(c => new
            {
                c.IdCategoria,
                c.NombreCategoria
            })
            .ToListAsync(cancellationToken);

        return Ok(categorias);
    }
}
using EuphoriaEssence.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[ApiController]
[Route("api/estado")]
public class EstadoController : ControllerBase
{
    private readonly EuphoriaEssenceDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public EstadoController(
        EuphoriaEssenceDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            aplicacion = "Euphoria Essence",
            estado = "API funcionando",
            fechaUtc = DateTime.UtcNow
        });
    }

    [HttpGet("base-datos")]
    public async Task<IActionResult> ComprobarBaseDatos(
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.IdRol)
            .Select(r => new
            {
                r.IdRol,
                r.NombreRol
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            estado = "Conexión con SQL Server correcta",
            baseDatos = _context.Database.GetDbConnection().Database,
            roles
        });
    }
}
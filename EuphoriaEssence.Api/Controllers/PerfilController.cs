using System.Security.Claims;
using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/perfil")]
public class PerfilController : ControllerBase
{
    private readonly EuphoriaEssenceDbContext _context;

    public PerfilController(EuphoriaEssenceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(
        CancellationToken cancellationToken)
    {
        var identificador = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificador, out var idUsuario))
        {
            return Unauthorized();
        }

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario && u.Activo)
            .Select(u => new
            {
                u.IdUsuario,
                u.Nombre,
                u.Apellido,
                u.Email,
                u.Telefono,
                u.Direccion,
                rol = u.IdRolNavigation.NombreRol
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (usuario is null)
        {
            return Unauthorized(new
            {
                mensaje = "La cuenta no está disponible."
            });
        }

        return Ok(usuario);
    }

    [HttpPut]
    public async Task<IActionResult> Editar(
        [FromBody] EditarPerfilDto dto,
        CancellationToken cancellationToken)
    {
        var identificador = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificador, out var idUsuario))
        {
            return Unauthorized();
        }

        var nombre = dto.Nombre.Trim();
        var apellido = dto.Apellido.Trim();

        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido))
        {
            return BadRequest(new
            {
                mensaje = "El nombre y el apellido son obligatorios."
            });
        }

        var usuario = await _context.Usuarios
            .SingleOrDefaultAsync(
                u => u.IdUsuario == idUsuario && u.Activo,
                cancellationToken);

        if (usuario is null)
        {
            return Unauthorized(new
            {
                mensaje = "La cuenta no está disponible."
            });
        }

        usuario.Nombre = nombre;
        usuario.Apellido = apellido;

        usuario.Telefono = string.IsNullOrWhiteSpace(dto.Telefono)
            ? null
            : dto.Telefono.Trim();

        usuario.Direccion = string.IsNullOrWhiteSpace(dto.Direccion)
            ? null
            : dto.Direccion.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            mensaje = "Perfil actualizado correctamente.",
            usuario = new
            {
                usuario.IdUsuario,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Email,
                usuario.Telefono,
                usuario.Direccion
            }
        });
    }
}
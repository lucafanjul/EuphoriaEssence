using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.DTOs;
using EuphoriaEssence.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly EuphoriaEssenceDbContext _context;
    private readonly IPasswordHasher<Usuarios> _passwordHasher;

    public AuthController(
        EuphoriaEssenceDbContext context,
        IPasswordHasher<Usuarios> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [AllowAnonymous]
    [HttpPost("registro")]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistroDto dto,
        CancellationToken cancellationToken)
    {
        var nombre = dto.Nombre.Trim();
        var apellido = dto.Apellido.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido))
        {
            return BadRequest(new
            {
                mensaje = "El nombre y el apellido son obligatorios."
            });
        }

        var emailExiste = await _context.Usuarios
            .AnyAsync(
                u => u.Email == email,
                cancellationToken);

        if (emailExiste)
        {
            return Conflict(new
            {
                mensaje = "Ya existe una cuenta con ese correo electrónico."
            });
        }

        var rolCliente = await _context.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                r => r.NombreRol == "Cliente",
                cancellationToken);

        if (rolCliente is null)
        {
            return Problem(
                title: "No se pudo completar el registro.",
                detail: "No está configurado el rol Cliente.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var usuario = new Usuarios
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono)
                ? null
                : dto.Telefono.Trim(),
            Direccion = string.IsNullOrWhiteSpace(dto.Direccion)
                ? null
                : dto.Direccion.Trim(),
            IdRol = rolCliente.IdRol,
            Activo = true
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(
            usuario,
            dto.Password);

        _context.Usuarios.Add(usuario);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException &&
                  (sqlException.Number == 2601 ||
                   sqlException.Number == 2627))
        {
            // La restricción UNIQUE también protege contra
            // dos registros simultáneos con el mismo correo.
            return Conflict(new
            {
                mensaje = "Ya existe una cuenta con ese correo electrónico."
            });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                mensaje = "Cuenta creada correctamente.",
                usuario = new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email,
                    rol = rolCliente.NombreRol
                }
            });
    }
}
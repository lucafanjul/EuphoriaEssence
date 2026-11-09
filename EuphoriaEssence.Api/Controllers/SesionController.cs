using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.DTOs;
using EuphoriaEssence.Api.Models;
using EuphoriaEssence.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class SesionController : ControllerBase
{
    private readonly EuphoriaEssenceDbContext _context;
    private readonly IPasswordHasher<Usuarios> _passwordHasher;
    private readonly JwtService _jwtService;

    public SesionController(
        EuphoriaEssenceDbContext context,
        IPasswordHasher<Usuarios> passwordHasher,
        JwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios
            .SingleOrDefaultAsync(
                u => u.Email == email,
                cancellationToken);

        if (usuario is null || !usuario.Activo)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            dto.Password);

        if (resultado == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        var rol = await _context.Roles
            .AsNoTracking()
            .Where(r => r.IdRol == usuario.IdRol)
            .Select(r => r.NombreRol)
            .SingleOrDefaultAsync(cancellationToken);

        if (rol is null)
        {
            return Problem(
                title: "La cuenta no tiene un rol válido.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        if (resultado ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.PasswordHash = _passwordHasher.HashPassword(
                usuario,
                dto.Password);

            await _context.SaveChangesAsync(cancellationToken);
        }

        var token = _jwtService.CrearToken(usuario, rol);

        return Ok(new
        {
            mensaje = "Inicio de sesión correcto.",
            token = token.Token,
            tipoToken = "Bearer",
            expiracionUtc = token.ExpiracionUtc,
            usuario = new
            {
                usuario.IdUsuario,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Email,
                rol
            }
        });
    }
}
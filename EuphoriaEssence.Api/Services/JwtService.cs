using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EuphoriaEssence.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace EuphoriaEssence.Api.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiracionUtc) CrearToken(
        Usuarios usuario,
        string rol)
    {
        var clave = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "No está configurada la clave JWT.");

        var ahora = DateTime.UtcNow;
        var expiracion = ahora.AddHours(1);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,
                usuario.IdUsuario.ToString()),

            new(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(ClaimTypes.NameIdentifier,
                usuario.IdUsuario.ToString()),

            new(ClaimTypes.Name,
                $"{usuario.Nombre} {usuario.Apellido}"),

            new(ClaimTypes.Role, rol)
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(clave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            notBefore: ahora,
            expires: expiracion,
            signingCredentials: credenciales);

        return (
            new JwtSecurityTokenHandler().WriteToken(token),
            expiracion);
    }
}
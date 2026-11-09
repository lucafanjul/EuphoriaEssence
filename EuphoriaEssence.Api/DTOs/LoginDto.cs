using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.Api.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Ingresá tu correo.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu contraseña.")]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}
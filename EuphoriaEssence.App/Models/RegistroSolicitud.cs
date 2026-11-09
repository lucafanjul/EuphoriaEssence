using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.App.Models;

public class RegistroSolicitud
{
    [Required(ErrorMessage = "Ingresá tu nombre.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu apellido.")]
    [StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá una contraseña.")]
    [StringLength(
        128,
        MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener entre 8 y 128 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "El teléfono admite hasta 30 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(200, ErrorMessage = "La dirección admite hasta 200 caracteres.")]
    public string? Direccion { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.Api.DTOs;

public class EditarPerfilDto
{
    [Required(ErrorMessage = "Ingresá tu nombre.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu apellido.")]
    [StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Telefono { get; set; }

    [StringLength(200)]
    public string? Direccion { get; set; }
}
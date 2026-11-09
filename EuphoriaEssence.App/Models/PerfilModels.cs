using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.App.Models;

public class PerfilRespuesta
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string Rol { get; set; } = string.Empty;
}

public class EditarPerfilSolicitud
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
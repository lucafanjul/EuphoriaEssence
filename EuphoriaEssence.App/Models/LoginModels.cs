using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.App.Models;

public class LoginSolicitud
{
    [Required(ErrorMessage = "Ingresá tu correo.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu contraseña.")]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRespuesta
{
    public string Mensaje { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string TipoToken { get; set; } = string.Empty;
    public DateTime ExpiracionUtc { get; set; }
    public UsuarioSesion Usuario { get; set; } = new();
}

public class UsuarioSesion
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
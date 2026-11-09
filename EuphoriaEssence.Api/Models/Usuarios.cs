using System;
using System.Collections.Generic;

namespace EuphoriaEssence.Api.Models;

public partial class Usuarios
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public int IdRol { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Carritos> Carritos
    { get; set; } = new List<Carritos>();

    public virtual Roles IdRolNavigation
    { get; set; } = null!;

    public virtual ICollection<Pedidos> Pedidos
    { get; set; } = new List<Pedidos>();
}
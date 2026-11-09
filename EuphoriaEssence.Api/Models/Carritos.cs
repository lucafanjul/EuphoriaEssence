using System;
using System.Collections.Generic;

namespace EuphoriaEssence.Api.Models;

public partial class Carritos
{
    public int IdCarrito { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<DetalleCarrito> DetalleCarrito { get; set; } = new List<DetalleCarrito>();

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}

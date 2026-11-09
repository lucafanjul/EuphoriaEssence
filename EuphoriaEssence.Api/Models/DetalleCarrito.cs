using System;
using System.Collections.Generic;

namespace EuphoriaEssence.Api.Models;

public partial class DetalleCarrito
{
    public int IdDetalleCarrito { get; set; }

    public int IdCarrito { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public virtual Carritos IdCarritoNavigation { get; set; } = null!;

    public virtual Productos IdProductoNavigation { get; set; } = null!;
}

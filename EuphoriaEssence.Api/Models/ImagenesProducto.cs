using System;
using System.Collections.Generic;

namespace EuphoriaEssence.Api.Models;

public partial class ImagenesProducto
{
    public int IdImagen { get; set; }

    public int IdProducto { get; set; }

    public string UrlImagen { get; set; } = null!;

    public bool EsPrincipal { get; set; }

    public virtual Productos IdProductoNavigation { get; set; } = null!;
}

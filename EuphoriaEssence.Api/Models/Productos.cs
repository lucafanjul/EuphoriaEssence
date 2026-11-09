using System;
using System.Collections.Generic;

namespace EuphoriaEssence.Api.Models;

public partial class Productos
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public int TamanoMl { get; set; }

    public string Concentracion { get; set; } = null!;

    public int IdMarca { get; set; }

    public int IdCategoria { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<DetalleCarrito> DetalleCarrito
    { get; set; } = new List<DetalleCarrito>();

    public virtual ICollection<DetallePedido> DetallePedido
    { get; set; } = new List<DetallePedido>();

    public virtual Categorias IdCategoriaNavigation
    { get; set; } = null!;

    public virtual Marcas IdMarcaNavigation
    { get; set; } = null!;

    public virtual ICollection<ImagenesProducto> ImagenesProducto
    { get; set; } = new List<ImagenesProducto>();
}
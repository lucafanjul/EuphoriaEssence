namespace EuphoriaEssence.App.Models;

public class PerfumeDetalleRespuesta
{
    public PerfumeDetalle Producto { get; set; } = new();
    public List<ImagenPerfume> Imagenes { get; set; } = new();
}

public class PerfumeDetalle
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int TamanoMl { get; set; }
    public string Concentracion { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public bool Disponible { get; set; }
}

public class ImagenPerfume
{
    public int IdImagen { get; set; }
    public string UrlImagen { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
}
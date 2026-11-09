namespace EuphoriaEssence.Api.DTOs;

public class ProductoResumenDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int TamanoMl { get; set; }
    public string Concentracion { get; set; } = string.Empty;
    public string? ImagenPrincipal { get; set; }

    public bool Disponible => Stock > 0;
}
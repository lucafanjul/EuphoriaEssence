namespace EuphoriaEssence.App.Models;

public class PaginaAdminProductos
{
    public int Pagina { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
    public List<AdminProducto> Items { get; set; } = new();
}

public class AdminProducto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int TamanoMl { get; set; }
    public string Concentracion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int IdMarca { get; set; }
    public int IdCategoria { get; set; }
    public string? ImagenPrincipal { get; set; }
}
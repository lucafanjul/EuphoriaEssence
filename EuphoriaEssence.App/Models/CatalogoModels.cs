namespace EuphoriaEssence.App.Models;

public class PerfumeResumen
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

public class PaginaPerfumes
{
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public List<PerfumeResumen> Items { get; set; } = new();
}
public class MarcaFiltro
{
    public int IdMarca { get; set; }
    public string NombreMarca { get; set; } = string.Empty;
}

public class CategoriaFiltro
{
    public int IdCategoria { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
}
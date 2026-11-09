namespace EuphoriaEssence.App.Models;

public class PaginaPedidos
{
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public List<PedidoResumen> Items { get; set; } = new();
}

public class PedidoResumen
{
    public int IdPedido { get; set; }
    public DateTime FechaPedido { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string DireccionEnvio { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class PedidoDetalleRespuesta : PedidoResumen
{
    public List<PedidoItem> Items { get; set; } = new();
}

public class PedidoItem
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int TamanoMl { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? Imagen { get; set; }
}
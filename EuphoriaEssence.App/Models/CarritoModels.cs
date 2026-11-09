namespace EuphoriaEssence.App.Models;

public class CarritoRespuesta
{
    public List<CarritoItem> Items { get; set; } = new();
    public long TotalProductos { get; set; }
    public decimal Total { get; set; }
    public bool PuedeComprar { get; set; }
}

public class CarritoItem
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public int TamanoMl { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    public int Stock { get; set; }
    public bool Disponible { get; set; }
    public string? Imagen { get; set; }
}

public class PedidoCreadoRespuesta
{
    public int IdPedido { get; set; }
}
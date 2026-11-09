namespace EuphoriaEssence.App.Models;

public class PaginaAdminPedidos
{
    public int Pagina { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
    public List<AdminPedidoResumen> Items { get; set; } = new();
}

public class AdminPedidoResumen
{
    public int IdPedido { get; set; }
    public DateTime FechaPedido { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string DireccionEnvio { get; set; } = string.Empty;
    public int IdEstadoPedido { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class EstadoPedidoOpcion
{
    public int IdEstadoPedido { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
}
public class AdminPedidoDetalleRespuesta
{
    public int IdPedido { get; set; }
    public DateTime FechaPedido { get; set; }
    public string DireccionEnvio { get; set; } = string.Empty;
    public int IdEstadoPedido { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }

    public AdminPedidoCliente Cliente { get; set; } = new();
    public List<AdminPedidoItem> Items { get; set; } = new();
}

public class AdminPedidoCliente
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
}

public class AdminPedidoItem
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public int TamanoMl { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
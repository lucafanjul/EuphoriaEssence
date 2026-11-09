using System;
using System.Collections.Generic;

namespace EuphoriaEssence.Api.Models;

public partial class Pedidos
{
    public int IdPedido { get; set; }

    public int IdUsuario { get; set; }

    public int IdEstadoPedido { get; set; }

    public DateTime FechaPedido { get; set; }

    public string DireccionEnvio { get; set; } = null!;

    public virtual ICollection<DetallePedido> DetallePedido { get; set; } = new List<DetallePedido>();

    public virtual EstadosPedido IdEstadoPedidoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace EuphoriaEssence.Api.Models;

public partial class EstadosPedido
{
    public int IdEstadoPedido { get; set; }

    public string NombreEstado { get; set; } = null!;

    public virtual ICollection<Pedidos> Pedidos { get; set; } = new List<Pedidos>();
}

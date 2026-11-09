using System;
using System.Collections.Generic;

namespace EuphoriaEssence.Api.Models;

public partial class Categorias
{
    public int IdCategoria { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Productos> Productos { get; set; } = new List<Productos>();
}

using System;
using System.Collections.Generic;

namespace EuphoriaEssence.Api.Models;

public partial class Marcas
{
    public int IdMarca { get; set; }

    public string NombreMarca { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Productos> Productos { get; set; } = new List<Productos>();
}

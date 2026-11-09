using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.Api.DTOs;

public class CrearProductoDto : GuardarProductoDto
{
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
}
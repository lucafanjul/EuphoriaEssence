using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.Api.DTOs;

public class ActualizarStockDto
{
    [Required(ErrorMessage = "Ingresá el stock.")]
    [Range(0, int.MaxValue)]
    public int? Stock { get; set; }
}
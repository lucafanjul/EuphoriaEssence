using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.Api.DTOs;

public class AgregarCarritoDto
{
    [Range(1, int.MaxValue)]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; } = 1;
}

public class CambiarCantidadDto
{
    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public class CrearPedidoDto
{
    [Required(ErrorMessage = "Ingresá la dirección de envío.")]
    [StringLength(200)]
    public string DireccionEnvio { get; set; } = string.Empty;
}

public class CambiarEstadoPedidoDto
{
    [Range(1, int.MaxValue)]
    public int IdEstadoPedido { get; set; }
}
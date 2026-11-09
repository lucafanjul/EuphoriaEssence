using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.Api.DTOs;

public class GuardarProductoDto
{
    [Required(ErrorMessage = "Ingresá el nombre.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(700)]
    public string? Descripcion { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "El precio debe ser mayor que cero y estar dentro del rango permitido.")]
    public decimal Precio { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "El tamaño debe ser mayor que cero.")]
    public int TamanoMl { get; set; }

    [Required(ErrorMessage = "Ingresá la concentración.")]
    [StringLength(50)]
    public string Concentracion { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int IdMarca { get; set; }

    [Range(1, int.MaxValue)]
    public int IdCategoria { get; set; }
}
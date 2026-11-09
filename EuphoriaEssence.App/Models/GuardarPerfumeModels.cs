using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.App.Models;

public class GuardarPerfumeSolicitud
{
    [Required(ErrorMessage = "Ingresá el nombre.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(700)]
    public string? Descripcion { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "Ingresá un precio mayor que cero.")]
    public decimal Precio { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Ingresá un tamaño mayor que cero.")]
    public int TamanoMl { get; set; }

    [Required(ErrorMessage = "Ingresá la concentración.")]
    [StringLength(50)]
    public string Concentracion { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una marca.")]
    public int IdMarca { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una categoría.")]
    public int IdCategoria { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
}

public class PerfumeAdminDetalle : GuardarPerfumeSolicitud
{
    public int IdProducto { get; set; }
    public bool Activo { get; set; }
}

public class PerfumeGuardadoRespuesta
{
    public int IdProducto { get; set; }
}
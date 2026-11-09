using System.ComponentModel.DataAnnotations;

namespace EuphoriaEssence.Api.DTOs;

public class SubirImagenDto
{
    [Required(ErrorMessage = "Seleccioná una imagen.")]
    public IFormFile Archivo { get; set; } = null!;
}
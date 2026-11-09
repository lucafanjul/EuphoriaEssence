using System.Data;
using EuphoriaEssence.Api.Data;
using EuphoriaEssence.Api.DTOs;
using EuphoriaEssence.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Controllers;

[Authorize(Roles = "Administrador")]
[ApiController]
[Route("api/admin/productos/{idProducto:int:min(1)}/imagenes")]
public class ImagenesController : ControllerBase
{
    private const long MaximoBytes = 5 * 1024 * 1024;

    private readonly EuphoriaEssenceDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ImagenesController(
        EuphoriaEssenceDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Subir(
        int idProducto,
        [FromForm] SubirImagenDto dto,
        CancellationToken cancellationToken)
    {
        var archivo = dto.Archivo;

        if (archivo.Length == 0 || archivo.Length > MaximoBytes)
        {
            return BadRequest(new
            {
                mensaje = "La imagen debe pesar entre 1 byte y 5 MB."
            });
        }

        var extension = Path.GetExtension(archivo.FileName)
            .ToLowerInvariant();

        var esPng = extension == ".png";
        var esJpeg = extension is ".jpg" or ".jpeg";

        if (!esPng && !esJpeg)
        {
            return BadRequest(new
            {
                mensaje = "Solo se permiten imágenes JPG o PNG."
            });
        }

        var tipoEsperado = esPng ? "image/png" : "image/jpeg";

        if (!string.Equals(
                archivo.ContentType,
                tipoEsperado,
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                mensaje = "El tipo de archivo no corresponde a una imagen JPG o PNG."
            });
        }

        var existeProducto = await _context.Productos
            .AnyAsync(
                p => p.IdProducto == idProducto,
                cancellationToken);

        if (!existeProducto)
        {
            return NotFound(new
            {
                mensaje = "El perfume no existe."
            });
        }

        // Comprobar la firma del archivo, además de su extensión.
        await using var entrada = archivo.OpenReadStream();

        var cabecera = new byte[8];
        var leidos = await entrada.ReadAtLeastAsync(
            cabecera.AsMemory(),
            cabecera.Length,
            throwOnEndOfStream: false,
            cancellationToken: cancellationToken);

        var firmaPng = leidos >= 8 &&
            cabecera[0] == 0x89 &&
            cabecera[1] == 0x50 &&
            cabecera[2] == 0x4E &&
            cabecera[3] == 0x47 &&
            cabecera[4] == 0x0D &&
            cabecera[5] == 0x0A &&
            cabecera[6] == 0x1A &&
            cabecera[7] == 0x0A;

        var firmaJpeg = leidos >= 3 &&
            cabecera[0] == 0xFF &&
            cabecera[1] == 0xD8 &&
            cabecera[2] == 0xFF;

        if ((esPng && !firmaPng) || (esJpeg && !firmaJpeg))
        {
            return BadRequest(new
            {
                mensaje = "El contenido del archivo no corresponde al formato indicado."
            });
        }

        var carpeta = Path.Combine(
            _environment.ContentRootPath,
            "wwwroot",
            "uploads");

        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
        var rutaFisica = Path.Combine(carpeta, nombreArchivo);
        var urlImagen = $"/uploads/{nombreArchivo}";

        try
        {
            await using (var salida = new FileStream(
                rutaFisica,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true))
            {
                // Guardar también los bytes leídos para validar la firma.
                await salida.WriteAsync(
                    cabecera.AsMemory(0, leidos),
                    cancellationToken);

                await entrada.CopyToAsync(
                    salida,
                    cancellationToken);
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var tienePrincipal = await _context.ImagenesProducto
                .AnyAsync(
                    i => i.IdProducto == idProducto && i.EsPrincipal,
                    cancellationToken);

            var imagen = new ImagenesProducto
            {
                IdProducto = idProducto,
                UrlImagen = urlImagen,
                EsPrincipal = !tienePrincipal
            };

            _context.ImagenesProducto.Add(imagen);

            await _context.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    mensaje = "Imagen cargada correctamente.",
                    imagen.IdImagen,
                    imagen.IdProducto,
                    imagen.UrlImagen,
                    imagen.EsPrincipal
                });
        }
        catch
        {
            // Si falla el registro, evitar dejar un archivo sin asociar.
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }

            throw;
        }
    }
}
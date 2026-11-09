using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EuphoriaEssence.App.Models;
using Microsoft.Maui.Storage;

namespace EuphoriaEssence.App.Services;

public class SesionService
{
    private const string ClaveSesion = "euphoria_sesion_v1";

    private readonly HttpClient _http;
    private LoginRespuesta? _sesion;

    public SesionService(HttpClient http)
    {
        _http = http;
    }

    public bool EstaLogueado
    {
        get
        {
            if (_sesion is null)
                return false;

            if (string.IsNullOrWhiteSpace(_sesion.Token) ||
                _sesion.ExpiracionUtc <= DateTime.UtcNow)
            {
                CerrarSesion();
                return false;
            }

            return true;
        }
    }

    public UsuarioSesion? Usuario =>
        EstaLogueado ? _sesion?.Usuario : null;

    public string RutaInicio =>
        Usuario?.Rol == "Administrador"
            ? "/admin/productos"
            : "/catalogo";

    public async Task RestaurarSesionAsync()
    {
        if (EstaLogueado)
            return;

        try
        {
            var contenido = await SecureStorage.Default
                .GetAsync(ClaveSesion);

            if (string.IsNullOrWhiteSpace(contenido))
                return;

            var datos = JsonSerializer
                .Deserialize<LoginRespuesta>(contenido);

            if (datos is null ||
                datos.Usuario is null ||
                string.IsNullOrWhiteSpace(datos.Token) ||
                datos.ExpiracionUtc <= DateTime.UtcNow)
            {
                CerrarSesion();
                return;
            }

            ActivarSesion(datos);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"No se pudo restaurar la sesión: {ex.GetType().Name}");

            CerrarSesion();
        }
    }

    public async Task<(bool Exito, string Mensaje)> IniciarSesionAsync(
        LoginSolicitud solicitud)
    {
        CerrarSesion();

        try
        {
            using var respuesta = await _http.PostAsJsonAsync(
                "api/auth/login",
                solicitud);

            if (!respuesta.IsSuccessStatusCode)
            {
                return (false, await LeerErrorAsync(respuesta));
            }

            var datos = await respuesta.Content
                .ReadFromJsonAsync<LoginRespuesta>();

            if (datos is null ||
                datos.Usuario is null ||
                string.IsNullOrWhiteSpace(datos.Token) ||
                datos.ExpiracionUtc <= DateTime.UtcNow)
            {
                return (
                    false,
                    "La API devolvió una sesión inválida.");
            }

            ActivarSesion(datos);

            try
            {
                await SecureStorage.Default.SetAsync(
                    ClaveSesion,
                    JsonSerializer.Serialize(datos));
            }
            catch (Exception ex)
            {
                // El usuario puede continuar con la sesión en memoria.
                Debug.WriteLine(
                    $"No se pudo conservar la sesión: {ex.GetType().Name}");
            }

            return (true, datos.Mensaje);
        }
        catch (HttpRequestException)
        {
            return (
                false,
                "No se pudo conectar con el servidor. Revisá tu conexión.");
        }
        catch (OperationCanceledException)
        {
            return (
                false,
                "El servidor tardó demasiado. Volvé a intentar.");
        }
        catch (JsonException)
        {
            return (
                false,
                "No se pudo interpretar la respuesta del servidor.");
        }
    }

    private void ActivarSesion(LoginRespuesta datos)
    {
        _sesion = datos;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", datos.Token);
    }

    public void CerrarSesion()
    {
        _sesion = null;
        _http.DefaultRequestHeaders.Authorization = null;

        try
        {
            SecureStorage.Default.Remove(ClaveSesion);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"No se pudo eliminar la sesión guardada: {ex.GetType().Name}");
        }
    }

    private static async Task<string> LeerErrorAsync(
        HttpResponseMessage respuesta)
    {
        try
        {
            using var documento = await JsonDocument.ParseAsync(
                await respuesta.Content.ReadAsStreamAsync());

            var raiz = documento.RootElement;

            if (raiz.ValueKind == JsonValueKind.Object)
            {
                if (raiz.TryGetProperty("mensaje", out var mensaje) &&
                    mensaje.ValueKind == JsonValueKind.String)
                {
                    return mensaje.GetString()
                        ?? "No se pudo iniciar sesión.";
                }

                if (raiz.TryGetProperty("errors", out var errores) &&
                    errores.ValueKind == JsonValueKind.Object)
                {
                    foreach (var campo in errores.EnumerateObject())
                    {
                        if (campo.Value.ValueKind != JsonValueKind.Array)
                            continue;

                        foreach (var error in campo.Value.EnumerateArray())
                        {
                            if (error.ValueKind == JsonValueKind.String)
                            {
                                return error.GetString()
                                    ?? "Revisá los datos ingresados.";
                            }
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
        }

        return respuesta.StatusCode ==
            System.Net.HttpStatusCode.Unauthorized
                ? "Correo o contraseña incorrectos."
                : $"No se pudo iniciar sesión. Código {(int)respuesta.StatusCode}.";
    }
}
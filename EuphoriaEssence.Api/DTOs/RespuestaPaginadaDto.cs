namespace EuphoriaEssence.Api.DTOs;

public class RespuestaPaginadaDto<T>
{
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }

    public int TotalPaginas =>
        (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);

    public List<T> Items { get; set; } = [];
}
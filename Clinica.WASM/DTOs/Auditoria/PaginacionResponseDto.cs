namespace Clinica.WASM.DTOs.Auditoria;

public class PaginacionResponseDto<T>
{
    public int Pagina { get; set; }
    public int CantidadPorPagina { get; set; }
    public int TotalRegistros { get; set; }
    public List<T> Datos { get; set; } = new();
}
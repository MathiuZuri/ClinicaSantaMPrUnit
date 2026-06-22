namespace Clinica.WASM.DTOs.Auditoria;

public class PaginacionRequestDto
{
    public int Pagina { get; set; } = 1;
    public int CantidadPorPagina { get; set; } = 10;
}
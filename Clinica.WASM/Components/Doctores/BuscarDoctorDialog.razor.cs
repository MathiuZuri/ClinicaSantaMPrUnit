using Clinica.WASM.Services.Api;
using Clinica.WASM.DTOs.Doctores;
using Clinica.WASM.DTOs.Common;
using Clinica.Domain.Enums;
using Clinica.WASM.DTOs.Auditoria;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using EstadoDoctor = Clinica.WASM.DTOs.Doctores.EstadoDoctor;

namespace Clinica.WASM.Components.Doctores;

public partial class BuscarDoctorDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private DoctorApiService DoctorApi { get; set; } = default!;

    private string NombreFiltro = string.Empty;
    private string EspecialidadFiltro = string.Empty;
    private EstadoDoctor? EstadoFiltro;
    private PaginacionResponseDto<DoctorResponseDto>? Resultado;
    private string MensajeVacio = string.Empty;

    private async Task BuscarAsync()
    {
        var request = new PaginacionRequestDto { Pagina = 1, CantidadPorPagina = 10 };
        Resultado = await DoctorApi.BuscarAsync(NombreFiltro, EspecialidadFiltro, EstadoFiltro, request);
        if (Resultado.Datos.Count == 0)
            MensajeVacio = "No se encontraron doctores con los filtros seleccionados.";
        else
            MensajeVacio = string.Empty;
    }

    private void Cerrar() => MudDialog.Close();
}
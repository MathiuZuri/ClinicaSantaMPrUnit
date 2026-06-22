using Clinica.WASM.Services.Api;
using Clinica.WASM.DTOs.Atenciones;
using Clinica.WASM.DTOs.Pacientes;
using Clinica.WASM.DTOs.Doctores;
using Clinica.WASM.DTOs.ServiciosClinicos;
using Clinica.Domain.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Components.Atenciones;

public partial class RegistrarAtencionDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private AtencionApiService AtencionApi { get; set; } = default!;
    [Inject] private PacienteApiService PacienteApi { get; set; } = default!;
    [Inject] private DoctorApiService DoctorApi { get; set; } = default!;
    [Inject] private ServicioClinicoApiService ServicioApi { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private MudForm? Form;
    private MudStepper? Stepper;
    private bool IsLoading;

    private RegistrarAtencionDto Model = new();
    private AnamnesisDto Anamnesis = new();
    private ImpresionDiagnosticaDto ImpresionDiagnostica = new();
    private List<ExamenFisicoDto> ExamenesFisicos = new();
    private List<TactoVaginalDto> TactosVaginales = new();
    private List<EcografiaObstetricaDto> Ecografias = new();

    private PacienteResponseDto? _pacienteSeleccionado;
    private PacienteResponseDto? PacienteSeleccionado
    {
        get => _pacienteSeleccionado;
        set
        {
            _pacienteSeleccionado = value;
            Model.HistorialClinicoId = value?.HistorialClinicoId ?? Guid.Empty;
        }
    }
    private DoctorResponseDto? DoctorSeleccionado;
    private ServicioClinicoResponseDto? ServicioSeleccionado;

    private async Task<IEnumerable<PacienteResponseDto>> BuscarPacientesAsync(string searchText, CancellationToken ct)
    {
        var todos = await PacienteApi.ObtenerTodosAsync();
        if (todos == null) return Array.Empty<PacienteResponseDto>();
        if (string.IsNullOrWhiteSpace(searchText)) return todos.Where(p => p != null).Take(20);
        var texto = searchText.ToLowerInvariant();
        return todos.Where(p => p != null && (p.Nombres + " " + p.Apellidos + " " + p.DNI + " " + p.CodigoPaciente).ToLowerInvariant().Contains(texto)).Take(20);
    }

    private async Task<IEnumerable<DoctorResponseDto>> BuscarDoctoresAsync(string searchText, CancellationToken ct)
    {
        var todos = await DoctorApi.ObtenerTodosAsync();
        if (todos == null) return Array.Empty<DoctorResponseDto>();
        if (string.IsNullOrWhiteSpace(searchText)) return todos.Where(d => d != null).Take(20);
        var texto = searchText.ToLowerInvariant();
        return todos.Where(d => d != null && (d.NombreCompleto + " " + d.Especialidad + " " + d.CMP).ToLowerInvariant().Contains(texto)).Take(20);
    }

    private async Task<IEnumerable<ServicioClinicoResponseDto>> BuscarServiciosAsync(string searchText, CancellationToken ct)
    {
        var todos = await ServicioApi.ObtenerTodosAsync();
        if (todos == null) return Array.Empty<ServicioClinicoResponseDto>();
        if (string.IsNullOrWhiteSpace(searchText)) return todos.Where(s => s != null).Take(20);
        var texto = searchText.ToLowerInvariant();
        return todos.Where(s => s != null && s.Nombre != null && s.Nombre.ToLowerInvariant().Contains(texto)).Take(20);
    }

    private void AgregarExamenFisico() => ExamenesFisicos.Add(new());
    private void AgregarTactoVaginal() => TactosVaginales.Add(new());
    private void AgregarEcografia() => Ecografias.Add(new());

    private void Cancelar() => MudDialog.Cancel();

    private async Task RegistrarAsync()
    {
        if (PacienteSeleccionado == null || DoctorSeleccionado == null || ServicioSeleccionado == null)
        {
            Snackbar.Add("Debe seleccionar paciente, doctor y servicio.", Severity.Warning);
            return;
        }

        // Validar costo
        if (Model.CostoFinal <= 0)
        {
            Snackbar.Add("El costo final debe ser mayor a 0.", Severity.Warning);
            return;
        }

        // Validar historial clínico
        if (Model.HistorialClinicoId == Guid.Empty)
        {
            Snackbar.Add("El paciente seleccionado no tiene un historial clínico asignado.", Severity.Error);
            return;
        }

        // Asignar IDs y submódulos
        Model.PacienteId = PacienteSeleccionado.Id;
        Model.DoctorId = DoctorSeleccionado.Id;
        Model.ServicioClinicoId = ServicioSeleccionado.Id;
        Model.Anamnesis = Anamnesis;
        Model.ExamenesFisicos = ExamenesFisicos.Any() ? ExamenesFisicos : null;
        Model.TactosVaginales = TactosVaginales.Any() ? TactosVaginales : null;
        Model.Ecografias = Ecografias.Any() ? Ecografias : null;
        Model.ImpresionDiagnostica = ImpresionDiagnostica;

        IsLoading = true;
        var (exitoso, mensaje, _) = await AtencionApi.RegistrarAsync(Model);
        IsLoading = false;

        if (exitoso)
            MudDialog.Close(DialogResult.Ok(true));
        else
            Snackbar.Add(mensaje, Severity.Error);
    }
    
    private async Task CargarHistorialAsync(PacienteResponseDto? paciente)
    {
        if (paciente == null) return;
        // Si el DTO ya contiene HistorialClinicoId, lo asignamos directamente
        if (paciente.HistorialClinicoId != Guid.Empty)
        {
            Model.HistorialClinicoId = paciente.HistorialClinicoId;
            return;
        }
        // Si no, obtenemos el paciente completo (por si acaso)
        var detalle = await PacienteApi.ObtenerPorIdAsync(paciente.Id);
        if (detalle?.HistorialClinicoId != null && detalle.HistorialClinicoId != Guid.Empty)
            Model.HistorialClinicoId = detalle.HistorialClinicoId;
        else
            Snackbar.Add("El paciente no tiene historial clínico asignado.", Severity.Warning);
    }
}
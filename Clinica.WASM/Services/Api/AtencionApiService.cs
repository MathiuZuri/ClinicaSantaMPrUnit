using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Atenciones;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Comprobantes;

namespace Clinica.WASM.Services.Api;

public class AtencionApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public AtencionApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<List<AtencionResponseDto>> ObtenerTodasAsync()
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<AtencionResponseDto>>>(ApiEndpoints.Atenciones);
            return response?.Data ?? new();
        }
        catch (Exception ex)
        {
            SetError($"Error al cargar atenciones: {ex.Message}");
            return new();
        }
        finally { SetLoading(false); }
    }

    public async Task<AtencionResponseDto?> ObtenerPorIdAsync(Guid id)
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<AtencionResponseDto>>($"{ApiEndpoints.Atenciones}/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener atención: {ex.Message}");
            return null;
        }
        finally { SetLoading(false); }
    }

    public async Task<(bool Exitoso, string Mensaje, Guid? Id)> RegistrarAsync(RegistrarAtencionDto dto)
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Atenciones, dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EmitirComprobanteResponseDto>();
                return (true, "Atención registrada correctamente.", result?.ComprobanteId);
            }
            var error = await _apiErrorService.ObtenerMensajeErrorAsync(response);
            return (false, error, null);
        }
        catch (Exception ex) { return (false, ex.Message, null); }
        finally { SetLoading(false); }
    }

    public async Task<(bool Exitoso, string Mensaje)> CerrarAsync(Guid id, CerrarAtencionDto dto)
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Atenciones}/{id}/cerrar", dto);
            if (response.IsSuccessStatusCode) return (true, "Atención cerrada correctamente.");
            var error = await _apiErrorService.ObtenerMensajeErrorAsync(response);
            return (false, error);
        }
        catch (Exception ex) { return (false, ex.Message); }
        finally { SetLoading(false); }
    }

    public async Task<(bool Exitoso, string Mensaje)> AnularAsync(Guid id, string motivo)
    {
        SetLoading(true);
        try
        {
            var dto = new AnularAtencionDto { Motivo = motivo };
            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Atenciones}/{id}/anular", dto);
            if (response.IsSuccessStatusCode) return (true, "Atención anulada correctamente.");
            var error = await _apiErrorService.ObtenerMensajeErrorAsync(response);
            return (false, error);
        }
        catch (Exception ex) { return (false, ex.Message); }
        finally { SetLoading(false); }
    }
    
    // Añadir este método dentro de tu clase AtencionApiService
    public async Task<List<AtencionResponseDto>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<AtencionResponseDto>>>($"{ApiEndpoints.Atenciones}/paciente/{pacienteId}");
            return response?.Data ?? new();
        }
        catch (Exception ex)
        {
            SetError($"Error al cargar atenciones del paciente: {ex.Message}");
            return new();
        }
        finally { SetLoading(false); }
    }

    private void SetLoading(bool isLoading) => IsLoading = isLoading;
    private void SetError(string? error) => ErrorMessage = error;
}
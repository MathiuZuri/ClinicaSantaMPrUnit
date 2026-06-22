using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Comprobantes;

namespace Clinica.WASM.Services.Api;

public class ComprobanteApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    // ==========================================================
    // ESTADOS PARA LA UI
    // ==========================================================
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public ComprobanteApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    // ==========================================================
    // PREVIEWS
    // ==========================================================

    public async Task<ComprobantePagoPreviewDto?> PreviewBoletaPagoAsync(Guid pagoId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/preview/boleta-pago/{pagoId}";
            var response = await _httpClient.GetFromJsonAsync<ComprobantePagoPreviewDto>(url);
            return response;
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener preview de boleta: {ex.Message}");
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<ComprobanteCitaPreviewDto?> PreviewConstanciaCitaAsync(Guid citaId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/preview/constancia-cita/{citaId}";
            return await _httpClient.GetFromJsonAsync<ComprobanteCitaPreviewDto>(url);
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener preview de constancia: {ex.Message}");
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<ComprobanteAtencionPreviewDto?> PreviewResumenAtencionAsync(Guid atencionId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/preview/resumen-atencion/{atencionId}";
            return await _httpClient.GetFromJsonAsync<ComprobanteAtencionPreviewDto>(url);
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener preview de resumen de atención: {ex.Message}");
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<ComprobanteEstadoCuentaPreviewDto?> PreviewEstadoCuentaPacienteAsync(Guid pacienteId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/preview/estado-cuenta/paciente/{pacienteId}";
            return await _httpClient.GetFromJsonAsync<ComprobanteEstadoCuentaPreviewDto>(url);
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener preview de estado de cuenta: {ex.Message}");
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ==========================================================
    // EMISIÓN
    // ==========================================================

    public async Task<(bool Exitoso, string Mensaje, Guid? ComprobanteId)> EmitirBoletaPagoAsync(EmitirComprobantePagoDto dto)
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoints.Comprobantes}/emitir/boleta-pago", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EmitirComprobanteResponseDto>();
                return (true, result?.Mensaje ?? "Boleta emitida.", result?.ComprobanteId);
            }

            var error = await _apiErrorService.ObtenerMensajeErrorAsync(response);
            return (false, error, null);
        }
        catch (Exception ex)
        {
            SetError($"Error al emitir boleta: {ex.Message}");
            return (false, ex.Message, null);
        }
        finally { SetLoading(false); }
    }

    public async Task<(bool Exitoso, string Mensaje, Guid? ComprobanteId)> EmitirConstanciaCitaAsync(EmitirComprobanteCitaDto dto)
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoints.Comprobantes}/emitir/constancia-cita", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EmitirComprobanteResponseDto>();
                return (true, result?.Mensaje ?? "Constancia emitida.", result?.ComprobanteId);
            }

            var error = await _apiErrorService.ObtenerMensajeErrorAsync(response);
            return (false, error, null);
        }
        catch (Exception ex)
        {
            SetError($"Error al emitir constancia: {ex.Message}");
            return (false, ex.Message, null);
        }
        finally { SetLoading(false); }
    }

    public async Task<(bool Exitoso, string Mensaje, Guid? ComprobanteId)> EmitirResumenAtencionAsync(EmitirComprobanteAtencionDto dto)
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoints.Comprobantes}/emitir/resumen-atencion", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EmitirComprobanteResponseDto>();
                return (true, result?.Mensaje ?? "Resumen emitido.", result?.ComprobanteId);
            }

            var error = await _apiErrorService.ObtenerMensajeErrorAsync(response);
            return (false, error, null);
        }
        catch (Exception ex)
        {
            SetError($"Error al emitir resumen de atención: {ex.Message}");
            return (false, ex.Message, null);
        }
        finally { SetLoading(false); }
    }


    public async Task<(bool Exitoso, string Mensaje, Guid? ComprobanteId)> EmitirEstadoCuentaAsync(EmitirComprobanteEstadoCuentaDto dto)
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoints.Comprobantes}/emitir/estado-cuenta", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EmitirComprobanteResponseDto>();
                return (true, result?.Mensaje ?? "Estado de cuenta emitido.", result?.ComprobanteId);
            }

            var error = await _apiErrorService.ObtenerMensajeErrorAsync(response);
            return (false, error, null);
        }
        catch (Exception ex)
        {
            SetError($"Error al emitir estado de cuenta: {ex.Message}");
            return (false, ex.Message, null);
        }
        finally { SetLoading(false); }
    }

    // ==========================================================
    // PDF (Descarga binaria)
    // ==========================================================

    public async Task<byte[]?> GenerarPdfBoletaPagoAsync(Guid comprobanteId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/{comprobanteId}/pdf/boleta-pago";
            return await _httpClient.GetByteArrayAsync(url);
        }
        catch (Exception ex)
        {
            SetError($"Error al descargar PDF de boleta: {ex.Message}");
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<byte[]?> GenerarPdfConstanciaCitaAsync(Guid comprobanteId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/{comprobanteId}/pdf/constancia-cita";
            return await _httpClient.GetByteArrayAsync(url);
        }
        catch (Exception ex)
        {
            SetError($"Error al descargar PDF de constancia: {ex.Message}");
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<byte[]?> GenerarPdfResumenAtencionAsync(Guid comprobanteId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/{comprobanteId}/pdf/resumen-atencion";
            return await _httpClient.GetByteArrayAsync(url);
        }
        catch (Exception ex)
        {
            SetError($"Error al descargar PDF de resumen de atención: {ex.Message}");
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<byte[]?> GenerarPdfEstadoCuentaAsync(Guid comprobanteId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/{comprobanteId}/pdf/estado-cuenta";
            return await _httpClient.GetByteArrayAsync(url);
        }
        catch (Exception ex)
        {
            SetError($"Error al descargar PDF de estado de cuenta: {ex.Message}");
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ==========================================================
    // CONSULTAS
    // ==========================================================

    public async Task<ComprobanteDto?> ObtenerPorIdAsync(Guid id)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/{id}";
            return await _httpClient.GetFromJsonAsync<ComprobanteDto>(url);
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener comprobante: {ex.Message}");
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<List<ComprobanteDto>> ObtenerPorPacienteAsync(Guid pacienteId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/paciente/{pacienteId}";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ComprobanteDto>>>(url);
            return response?.Data ?? new List<ComprobanteDto>();
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener comprobantes del paciente: {ex.Message}");
            return new List<ComprobanteDto>();
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<List<ComprobanteDto>> ObtenerPorPagoAsync(Guid pagoId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/pago/{pagoId}";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ComprobanteDto>>>(url);
            return response?.Data ?? new List<ComprobanteDto>();
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener comprobantes del pago: {ex.Message}");
            return new List<ComprobanteDto>();
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<List<ComprobanteDto>> ObtenerPorAtencionAsync(Guid atencionId)
    {
        SetLoading(true);
        try
        {
            var url = $"{ApiEndpoints.Comprobantes}/atencion/{atencionId}";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ComprobanteDto>>>(url);
            return response?.Data ?? new List<ComprobanteDto>();
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener comprobantes de la atención: {ex.Message}");
            return new List<ComprobanteDto>();
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ==========================================================
    // ANULACIÓN
    // ==========================================================

    public async Task<(bool Exitoso, string Mensaje)> AnularComprobanteAsync(Guid comprobanteId, string motivo)
    {
        SetLoading(true);
        try
        {
            var dto = new AnularComprobanteDto { Motivo = motivo };
            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Comprobantes}/{comprobanteId}/anular", dto);
            if (response.IsSuccessStatusCode)
                return (true, "Comprobante anulado correctamente.");

            var error = await _apiErrorService.ObtenerMensajeErrorAsync(response);
            return (false, error);
        }
        catch (Exception ex)
        {
            SetError($"Error al anular comprobante: {ex.Message}");
            return (false, ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ==========================================================
    // MÉTODOS PRIVADOS PARA MANEJO DE ESTADO
    // ==========================================================

    private void SetLoading(bool isLoading)
    {
        IsLoading = isLoading;
        // Nota: en Blazor, los componentes que usen este servicio deberán llamar a StateHasChanged()
        // o implementar INotifyPropertyChanged si se desea notificación automática.
    }

    private void SetError(string? errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public void ClearError() => SetError(null);
    
    public async Task<List<ComprobanteDto>> ObtenerTodosAsync()
    {
        SetLoading(true);
        try
        {
            var response = await _httpClient.GetAsync(ApiEndpoints.Comprobantes);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new HttpRequestException("No autorizado", null, System.Net.HttpStatusCode.Unauthorized);

            response.EnsureSuccessStatusCode();
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ComprobanteDto>>>();
            return apiResponse?.Data ?? new();
        }
        catch (HttpRequestException)
        {
            throw; // La página capturará este error y mostrará el mensaje adecuado
        }
        catch (Exception ex)
        {
            SetError($"Error al obtener comprobantes: {ex.Message}");
            return new();
        }
        finally
        {
            SetLoading(false);
        }
    }
}
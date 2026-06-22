using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Common;
using Clinica.WASM.DTOs.Finanzas;

namespace Clinica.WASM.Services.Api;

public class FinanzasApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public FinanzasApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<ResumenFinancieroMensualCompletoDto?> ObtenerResumenMensualCompletoAsync(int anio, int mes)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<ResumenFinancieroMensualCompletoDto>>($"{ApiEndpoints.FinanzasResumenMensualCompleto}?anio={anio}&mes={mes}");
        return respuesta?.Data;
    }

    public async Task<List<EstadoPagoAtencionDto>> ObtenerDeudasRealesAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<EstadoPagoAtencionDto>>>(ApiEndpoints.FinanzasDeudasReales);
        return respuesta?.Data ?? new();
    }

    public async Task<EstadoCuentaPacienteDto?> ObtenerEstadoCuentaPacienteAsync(Guid pacienteId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<EstadoCuentaPacienteDto>>($"{ApiEndpoints.FinanzasEstadoCuentaPaciente}/{pacienteId}/estado-cuenta");
        return respuesta?.Data;
    }

    public async Task<List<AjusteFinancieroDto>> ObtenerAjustesFinancierosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<AjusteFinancieroDto>>>(ApiEndpoints.FinanzasAjustes);
        return respuesta?.Data ?? new();
    }

    public async Task<List<AjusteFinancieroDto>> ObtenerAjustesPorPagoAsync(Guid pagoId)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<AjusteFinancieroDto>>>($"{ApiEndpoints.FinanzasAjustesPorPago}/{pagoId}/ajustes-financieros");
        return respuesta?.Data ?? new();
    }

    public async Task<(bool Exitoso, string Mensaje)> RegistrarAjusteFinancieroAsync(RegistrarAjusteFinancieroDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.FinanzasAjustesRegistrar, dto);
        if (response.IsSuccessStatusCode)
            return (true, "Ajuste registrado.");
        var msg = await _apiErrorService.ObtenerMensajeErrorAsync(response);
        return (false, msg);
    }
    
    // ==========================================
// NUEVOS MÉTODOS (Resumen anual, diario, libro, búsqueda)
// ==========================================
    public async Task<ResumenAnualFinanzasDto?> ObtenerResumenAnualAsync(int anio)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<ResumenAnualFinanzasDto>>($"{ApiEndpoints.FinanzasResumenAnual}?anio={anio}");
        return respuesta?.Data;
    }

    public async Task<ResumenDiarioFinanzasDto?> ObtenerResumenDiarioAsync(DateOnly fecha)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<ResumenDiarioFinanzasDto>>($"{ApiEndpoints.FinanzasResumenDiario}?fecha={fecha:yyyy-MM-dd}");
        return respuesta?.Data;
    }

    public async Task<List<PagoFinanzasDto>> ObtenerPagosPendientesAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<PagoFinanzasDto>>>(ApiEndpoints.FinanzasPagosPendientes);
        return respuesta?.Data ?? new();
    }

    public async Task<List<PagoFinanzasDto>> ObtenerPagosPagadosAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<PagoFinanzasDto>>>(ApiEndpoints.FinanzasPagosPagados);
        return respuesta?.Data ?? new();
    }

    public async Task<List<PagoFinanzasDto>> ObtenerPagosParcialesAsync()
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<PagoFinanzasDto>>>(ApiEndpoints.FinanzasPagosParciales);
        return respuesta?.Data ?? new();
    }

    public async Task<PagoFinanzasDto?> ObtenerPagoPorCodigoAsync(string codigo)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<PagoFinanzasDto>>($"{ApiEndpoints.FinanzasPagoCodigo}/{codigo}");
        return respuesta?.Data;
    }

    public async Task<List<PagoFinanzasDto>> ObtenerLibroDiarioAsync(DateOnly fecha)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<List<PagoFinanzasDto>>>($"{ApiEndpoints.FinanzasLibroDiario}?fecha={fecha:yyyy-MM-dd}");
        return respuesta?.Data ?? new();
    }
    
    public async Task<ResumenMensualFinanzasDto?> ObtenerResumenMensualAsync(int anio, int mes)
    {
        var respuesta = await _httpClient.GetFromJsonAsync<ApiResponse<ResumenMensualFinanzasDto>>(
            $"{ApiEndpoints.FinanzasResumenMensual}?anio={anio}&mes={mes}");
        return respuesta?.Data;
    }
}
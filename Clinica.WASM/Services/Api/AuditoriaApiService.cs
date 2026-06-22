using System.Net.Http.Json;
using Clinica.WASM.Constants;
using Clinica.WASM.DTOs.Auditoria;
using Clinica.WASM.DTOs.Common;
using Clinica.Domain.Enums;

namespace Clinica.WASM.Services.Api;

public class AuditoriaApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiErrorService _apiErrorService;

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public AuditoriaApiService(HttpClient httpClient, ApiErrorService apiErrorService)
    {
        _httpClient = httpClient;
        _apiErrorService = apiErrorService;
    }

    public async Task<PaginacionResponseDto<AuditoriaResponseDto>> ObtenerTodosAsync(
        PaginacionRequestDto request,
        TipoAccionAuditoria? tipoAccion = null,
        bool? soloConsultas = null)
    {
        SetLoading(true);
        ClearError();
        try
        {
            var url = $"{ApiEndpoints.Auditoria}?pagina={request.Pagina}&cantidadPorPagina={request.CantidadPorPagina}";
            if (tipoAccion.HasValue)
                url += $"&tipoAccion={tipoAccion}";
            if (soloConsultas.HasValue)
                url += $"&soloConsultas={soloConsultas.Value}";

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PaginacionResponseDto<AuditoriaResponseDto>>>(url);
            return response?.Data ?? new();
        }
        catch (Exception ex)
        {
            SetError($"Error al cargar auditoría: {ex.Message}");
            return new();
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<PaginacionResponseDto<AuditoriaResponseDto>> ObtenerPorUsuarioAsync(
        Guid usuarioId,
        PaginacionRequestDto request,
        TipoAccionAuditoria? tipoAccion = null,
        bool? soloConsultas = null)
    {
        SetLoading(true);
        ClearError();
        try
        {
            var url = $"{ApiEndpoints.Auditoria}/usuario/{usuarioId}?pagina={request.Pagina}&cantidadPorPagina={request.CantidadPorPagina}";
            if (tipoAccion.HasValue)
                url += $"&tipoAccion={tipoAccion}";
            if (soloConsultas.HasValue)
                url += $"&soloConsultas={soloConsultas.Value}";

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PaginacionResponseDto<AuditoriaResponseDto>>>(url);
            return response?.Data ?? new();
        }
        catch (Exception ex)
        {
            SetError($"Error al cargar auditoría: {ex.Message}");
            return new();
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool isLoading) => IsLoading = isLoading;
    private void SetError(string? error) => ErrorMessage = error;
    private void ClearError() => ErrorMessage = null;
}
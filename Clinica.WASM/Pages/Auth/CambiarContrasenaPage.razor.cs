using Clinica.WASM.DTOs.Auth;
using Clinica.WASM.Services.Api;
using Clinica.WASM.Services.Auth;
using Clinica.WASM.Themes;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Clinica.WASM.Pages.Auth;

public partial class CambiarContrasenaPage : ComponentBase
{
    [Inject] private AuthApiService AuthApi { get; set; } = default!;
    [Inject] private TokenStorageService TokenStorage { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected MudTheme Theme { get; } = new ClinicaTheme();
    protected bool IsDarkMode { get; set; }
    protected CambiarContrasenaDto Model { get; set; } = new();
    protected bool EstaProcesando { get; set; }
    protected string? MensajeError { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Verificar que el usuario esté autenticado
        var token = await TokenStorage.ObtenerTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            Navigation.NavigateTo("/login", replace: true);
        }
    }

    protected async Task CambiarAsync()
    {
        EstaProcesando = true;
        MensajeError = null;
        try
        {
            var (exitoso, mensaje) = await AuthApi.CambiarContrasenaAsync(Model);
            if (exitoso)
            {
                // Actualizar el token o la sesión si es necesario (el backend ya actualizó la BD)
                await TokenStorage.LimpiarSesionAsync(); // Forzar re-login después de cambio exitoso
                Navigation.NavigateTo("/login", replace: true);
            }
            else
            {
                MensajeError = mensaje;
            }
        }
        catch (Exception ex)
        {
            MensajeError = $"Error inesperado: {ex.Message}";
        }
        finally
        {
            EstaProcesando = false;
        }
    }
}
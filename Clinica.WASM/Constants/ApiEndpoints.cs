namespace Clinica.WASM.Constants;

public static class ApiEndpoints
{
    public const string AuthLogin = "api/auth/login";
    public const string Auth = "api/auth";

    public const string Pacientes = "api/pacientes";
    public const string Doctores = "api/doctores";
    public const string Horarios = "api/horarios";
    public const string Citas = "api/citas";
    public const string ServiciosClinicos = "api/serviciosclinicos";
    public const string Historiales = "api/historiales";
    public const string Usuarios = "api/usuarios";
    public const string UsuariosAsignarRol = "api/usuarios/asignar-rol";
    public const string Roles = "api/roles";
    public const string RolesAsignarPermisos = "api/roles/asignar-permisos";
    public const string PermisosEndpoint = "api/permisos";
    public const string Pagos = "api/pagos";
    public const string Auditoria = "api/auditoria";
    
    public const string FinanzasResumenMensualCompleto = "api/finanzas/resumen-financiero-mensual-completo";
    public const string FinanzasDeudasReales = "api/finanzas/deudas-reales";
    public const string FinanzasEstadoCuentaPaciente = "api/finanzas/paciente";
    public const string FinanzasAjustes = "api/finanzas/ajustes-financieros";
    public const string FinanzasAjustesPorPago = "api/finanzas/pago";
    public const string FinanzasAjustesRegistrar = "api/finanzas/ajustes-financieros";
    public const string Atenciones = "api/atenciones";
    public const string Comprobantes = "api/comprobantes";
    
    
    public const string FinanzasResumenAnual = "api/finanzas/resumen-anual";
    public const string FinanzasResumenDiario = "api/finanzas/resumen-diario";
    public const string FinanzasPagosPendientes = "api/finanzas/pagos-pendientes";
    public const string FinanzasPagosPagados = "api/finanzas/pagos-pagados";
    public const string FinanzasPagosParciales = "api/finanzas/pagos-parciales";
    public const string FinanzasPagoCodigo = "api/finanzas/pago/codigo";
    public const string FinanzasLibroDiario = "api/finanzas/libro-diario";
    public const string FinanzasResumenMensual = "api/finanzas/resumen-mensual";
}
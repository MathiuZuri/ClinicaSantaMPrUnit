import http from 'k6/http';
import { check, sleep } from 'k6';
import { login } from '../helpers/auth.js';

export const options = {
    vus: 1,
    duration: '15s', // un poco más largo por la cantidad de endpoints
};

const BASE_URL = __ENV.BASE_URL || 'https://localhost:7241';

export default function () {
    const token = login();
    if (!token) return;

    const params = {
        headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    };

    const today = new Date().toISOString().split('T')[0]; // yyyy-mm-dd
    const currentYear = new Date().getFullYear();
    const currentMonth = new Date().getMonth() + 1;

    // 1. Resumen diario
    const resDiario = http.get(`${BASE_URL}/api/finanzas/resumen-diario?fecha=${today}`, params);
    check(resDiario, { 'GET resumen diario status 200': (r) => r.status === 200 });

    // 2. Resumen mensual
    const resMensual = http.get(`${BASE_URL}/api/finanzas/resumen-mensual?anio=${currentYear}&mes=${currentMonth}`, params);
    check(resMensual, { 'GET resumen mensual status 200': (r) => r.status === 200 });

    // 3. Resumen anual
    const resAnual = http.get(`${BASE_URL}/api/finanzas/resumen-anual?anio=${currentYear}`, params);
    check(resAnual, { 'GET resumen anual status 200': (r) => r.status === 200 });

    // 4. Pagos pendientes
    const resPendientes = http.get(`${BASE_URL}/api/finanzas/pagos-pendientes`, params);
    check(resPendientes, { 'GET pagos pendientes status 200': (r) => r.status === 200 });

    // 5. Pagos pagados
    const resPagados = http.get(`${BASE_URL}/api/finanzas/pagos-pagados`, params);
    check(resPagados, { 'GET pagos pagados status 200': (r) => r.status === 200 });

    // 6. Pagos parciales
    const resParciales = http.get(`${BASE_URL}/api/finanzas/pagos-parciales`, params);
    check(resParciales, { 'GET pagos parciales status 200': (r) => r.status === 200 });

    // 7. Libro diario
    const resLibroDiario = http.get(`${BASE_URL}/api/finanzas/libro-diario?fecha=${today}`, params);
    check(resLibroDiario, { 'GET libro diario status 200': (r) => r.status === 200 });

    // 8. Resumen financiero mensual completo
    const resCompleto = http.get(`${BASE_URL}/api/finanzas/resumen-financiero-mensual-completo?anio=${currentYear}&mes=${currentMonth}`, params);
    check(resCompleto, { 'GET resumen financiero mensual completo status 200': (r) => r.status === 200 });

    // 9. Obtener un paciente para probar estado de cuenta y deudas
    const resPacientes = http.get(`${BASE_URL}/api/pacientes`, params);
    const pacientes = resPacientes.json()?.data || [];
    if (pacientes.length > 0) {
        const pacienteId = pacientes[0].id;

        // Estado de cuenta del paciente
        const resEstadoCuenta = http.get(`${BASE_URL}/api/finanzas/paciente/${pacienteId}/estado-cuenta`, params);
        check(resEstadoCuenta, { 'GET estado cuenta paciente status 200': (r) => r.status === 200 });

        // Deudas reales del paciente
        const resDeudasPaciente = http.get(`${BASE_URL}/api/finanzas/paciente/${pacienteId}/deudas-reales`, params);
        check(resDeudasPaciente, { 'GET deudas reales paciente status 200': (r) => r.status === 200 });
    }

    // 10. Deudas reales globales
    const resDeudasGlobales = http.get(`${BASE_URL}/api/finanzas/deudas-reales`, params);
    check(resDeudasGlobales, { 'GET deudas reales globales status 200': (r) => r.status === 200 });

    // 11. Ajustes financieros (lista)
    const resAjustes = http.get(`${BASE_URL}/api/finanzas/ajustes-financieros`, params);
    check(resAjustes, { 'GET ajustes financieros status 200': (r) => r.status === 200 });

    // 12. Obtener un pago existente para probar búsqueda por código y ajustes por pago
    const pagos = resPagados.json()?.data || [];
    if (pagos.length > 0) {
        const codigoPago = pagos[0].codigoPago;
        if (codigoPago) {
            const resPagoPorCodigo = http.get(`${BASE_URL}/api/finanzas/pago/codigo/${encodeURIComponent(codigoPago)}`, params);
            check(resPagoPorCodigo, { 'GET pago por código status 200': (r) => r.status === 200 });
        }

        const pagoId = pagos[0].pagoId;
        // Ajustes por pago
        const resAjustesPago = http.get(`${BASE_URL}/api/finanzas/pago/${pagoId}/ajustes-financieros`, params);
        check(resAjustesPago, { 'GET ajustes por pago status 200': (r) => r.status === 200 });

        // Crear un ajuste financiero (POST) asociado a ese pago
        const payloadAjuste = JSON.stringify({
            pagoId: pagoId,
            tipoAjuste: 1, // Descuento (asumiendo enum)
            montoAjuste: 5.00,
            motivo: 'Ajuste de prueba k6 smoke',
            observacion: 'Generado automáticamente'
        });
        const resCrearAjuste = http.post(`${BASE_URL}/api/finanzas/ajustes-financieros`, payloadAjuste, params);
        check(resCrearAjuste, { 'POST crear ajuste status 200': (r) => r.status === 200 });
    }

    sleep(1);
}
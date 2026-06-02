import http from 'k6/http';
import { check, sleep } from 'k6';
import { login } from '../helpers/auth.js';

export const options = {
    vus: 1,
    duration: '10s',
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

    // 1. Obtener pacientes para usar un ID válido
    const resPacientes = http.get(`${BASE_URL}/api/pacientes`, params);
    check(resPacientes, {
        'GET pacientes status 200': (r) => r.status === 200,
    });

    const pacientes = resPacientes.json()?.data || [];
    if (pacientes.length === 0) {
        console.log('No hay pacientes para probar pagos.');
        return;
    }

    const pacienteId = pacientes[0].id;

    // 2. Obtener servicios clínicos para usar un ID válido
    const resServicios = http.get(`${BASE_URL}/api/serviciosclinicos`, params);
    check(resServicios, {
        'GET servicios status 200': (r) => r.status === 200,
    });

    const servicios = resServicios.json()?.data || [];
    if (servicios.length === 0) {
        console.log('No hay servicios para probar pagos.');
        return;
    }

    const servicioId = servicios[0].id;

    // 3. Obtener pagos por paciente (puede estar vacío)
    const resPagosPaciente = http.get(`${BASE_URL}/api/pagos/paciente/${pacienteId}`, params);
    check(resPagosPaciente, {
        'GET pagos por paciente status 200': (r) => r.status === 200,
    });

    // 4. Registrar un nuevo pago (POST)
    const payloadPago = JSON.stringify({
        pacienteId: pacienteId,
        servicioClinicoId: servicioId,
        montoTotal: 100.00,
        montoPagado: 100.00,
        montoAdelanto: 0,
        metodoPago: 1, // Efectivo (asumiendo enum MetodoPago)
        observacion: 'Pago generado por k6 smoke test'
    });

    const resCrearPago = http.post(`${BASE_URL}/api/pagos`, payloadPago, params);
    check(resCrearPago, {
        'POST registrar pago status 200': (r) => r.status === 200,
    });

    const nuevoPagoId = resCrearPago.json()?.data?.id;

    // 5. Obtener pagos por cita (si hay citas en el sistema)
    const resCitas = http.get(`${BASE_URL}/api/citas`, params);
    const citas = resCitas.json()?.data || [];
    if (citas.length > 0) {
        const citaId = citas[0].id;
        const resPagosCita = http.get(`${BASE_URL}/api/pagos/cita/${citaId}`, params);
        check(resPagosCita, {
            'GET pagos por cita status 200': (r) => r.status === 200,
        });
    }

    // 6. Cambiar estado del pago recién creado (PUT .../estado)
    if (nuevoPagoId) {
        const payloadEstado = JSON.stringify({ estado: 3 }); // Pagado (asumiendo enum EstadoPago)
        const resCambioEstado = http.put(`${BASE_URL}/api/pagos/${nuevoPagoId}/estado`, payloadEstado, params);
        check(resCambioEstado, {
            'PUT cambiar estado pago status 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}
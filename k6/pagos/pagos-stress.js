import http from 'k6/http';
import { check, sleep } from 'k6';
import { login } from '../helpers/auth.js';

export const options = {
    stages: [
        { duration: '20s', target: 10 },
        { duration: '20s', target: 30 },
        { duration: '20s', target: 60 },
        { duration: '20s', target: 100 },
        { duration: '20s', target: 0 },
    ],
    thresholds: {
        http_req_failed: ['rate<0.10'],
        http_req_duration: ['p(95)<1500'],
    },
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

    // Lectura principal: pagos de un paciente aleatorio
    const resPacientes = http.get(`${BASE_URL}/api/pacientes`, params);
    check(resPacientes, {
        'GET pacientes status 200': (r) => r.status === 200,
    });

    const pacientes = resPacientes.json()?.data || [];
    if (pacientes.length > 0) {
        const paciente = pacientes[Math.floor(Math.random() * pacientes.length)];
        const resPagos = http.get(`${BASE_URL}/api/pagos/paciente/${paciente.id}`, params);
        check(resPagos, {
            'GET pagos por paciente status 200': (r) => r.status === 200,
        });
    }

    // Registro de un pago en un pequeño porcentaje de iteraciones
    if (Math.random() < 0.05) {
        const resServicios = http.get(`${BASE_URL}/api/serviciosclinicos`, params);
        const servicios = resServicios.json()?.data || [];
        if (pacientes.length > 0 && servicios.length > 0) {
            const paciente = pacientes[Math.floor(Math.random() * pacientes.length)];
            const servicio = servicios[Math.floor(Math.random() * servicios.length)];

            const payloadPago = JSON.stringify({
                pacienteId: paciente.id,
                servicioClinicoId: servicio.id,
                montoTotal: 50.00,
                montoPagado: 50.00,
                montoAdelanto: 0,
                metodoPago: 2, // Yape (ejemplo)
                observacion: 'Pago de estrés k6'
            });

            const resCrear = http.post(`${BASE_URL}/api/pagos`, payloadPago, params);
            check(resCrear, {
                'POST registrar pago stress status 200': (r) => r.status === 200,
            });
        }
    }

    sleep(1);
}
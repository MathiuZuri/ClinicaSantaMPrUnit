import http from 'k6/http';
import { check, sleep } from 'k6';
import { login } from '../helpers/auth.js';

export const options = {
    vus: 20,
    duration: '30s',
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<1000'],
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

    // Obtener lista de pacientes para elegir uno
    const resPacientes = http.get(`${BASE_URL}/api/pacientes`, params);
    check(resPacientes, {
        'GET pacientes status 200': (r) => r.status === 200,
    });

    const pacientes = resPacientes.json()?.data || [];
    if (pacientes.length > 0) {
        const pacienteId = pacientes[0].id;

        // Lectura principal: historial por paciente
        const resHistorial = http.get(`${BASE_URL}/api/historiales/paciente/${pacienteId}`, params);
        check(resHistorial, {
            'GET historial status 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}
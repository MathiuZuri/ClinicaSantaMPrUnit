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

    // Obtener pacientes
    const resPacientes = http.get(`${BASE_URL}/api/pacientes`, params);
    check(resPacientes, {
        'GET pacientes status 200': (r) => r.status === 200,
    });

    const pacientes = resPacientes.json()?.data || [];
    if (pacientes.length > 0) {
        // Elegir un paciente aleatorio
        const paciente = pacientes[Math.floor(Math.random() * pacientes.length)];

        // Golpear historial por paciente
        const resHistorial = http.get(`${BASE_URL}/api/historiales/paciente/${paciente.id}`, params);
        check(resHistorial, {
            'GET historial por paciente stress status 200': (r) => r.status === 200,
        });

        // Solo un porcentaje también obtiene los detalles
        if (Math.random() < 0.3) {
            const historial = resHistorial.json()?.data;
            if (historial && historial.id) {
                const resDetalles = http.get(`${BASE_URL}/api/historiales/${historial.id}/detalles`, params);
                check(resDetalles, {
                    'GET historial con detalles stress status 200': (r) => r.status === 200,
                });
            }
        }
    }

    sleep(1);
}
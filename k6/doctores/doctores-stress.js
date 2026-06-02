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

    // Golpeamos la lista de doctores y ocasionalmente añadimos uno nuevo (carga mixta)
    const resLista = http.get(`${BASE_URL}/api/doctores`, params);
    check(resLista, {
        'GET doctores status 200': (r) => r.status === 200,
    });

    // Solo un pequeño porcentaje de las iteraciones intentará crear un doctor
    if (Math.random() < 0.05) {
        const cmpAleatorio = `CMP${Math.floor(Math.random() * 900000) + 100000}`;
        const payloadCrear = JSON.stringify({
            cmp: cmpAleatorio,
            nombres: 'Stress',
            apellidos: 'K6',
            especialidad: 'Ginecología',
            celular: '987111222',
            correo: `stress.${cmpAleatorio}@test.com`,
            fechaInicioContrato: '2026-06-01T00:00:00Z'
        });

        const resCrear = http.post(`${BASE_URL}/api/doctores`, payloadCrear, params);
        check(resCrear, {
            'POST crear doctor stress status 201': (r) => r.status === 201,
        });
    }

    sleep(1);
}
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

    // Mezcla de endpoints de lectura para simular carga real
    const resTodos = http.get(`${BASE_URL}/api/serviciosclinicos`, params);
    check(resTodos, {
        'GET todos los servicios status 200': (r) => r.status === 200,
    });

    // Solo un pequeño porcentaje consulta también los activos
    if (Math.random() < 0.3) {
        const resActivos = http.get(`${BASE_URL}/api/serviciosclinicos/activos`, params);
        check(resActivos, {
            'GET servicios activos status 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}
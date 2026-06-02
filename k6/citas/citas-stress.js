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

    // En estrés, golpeamos tanto el listado como el detalle de una cita aleatoria
    const resTodas = http.get(`${BASE_URL}/api/citas`, params);
    check(resTodas, {
        'GET citas status 200': (r) => r.status === 200,
    });

    const body = resTodas.json();
    const citas = body?.data || [];
    if (citas.length > 0) {
        const aleatoria = citas[Math.floor(Math.random() * citas.length)];
        const resDetalle = http.get(`${BASE_URL}/api/citas/${aleatoria.id}`, params);
        check(resDetalle, {
            'GET cita aleatoria status 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}
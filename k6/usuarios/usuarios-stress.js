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

    // Lectura principal
    const resUsuarios = http.get(`${BASE_URL}/api/usuarios`, params);
    check(resUsuarios, {
        'GET usuarios status 200': (r) => r.status === 200,
    });

    // Creación ocasional (5% de las iteraciones)
    if (Math.random() < 0.05) {
        const username = `stress_${Date.now()}_${Math.random().toString(36).substr(2, 5)}`;
        const payloadCrear = JSON.stringify({
            nombres: 'Stress',
            apellidos: 'K6',
            userName: username,
            correo: `${username}@test.com`,
            password: 'Password123!'
        });
        const resCrear = http.post(`${BASE_URL}/api/usuarios`, payloadCrear, params);
        check(resCrear, {
            'POST crear usuario stress status 201': (r) => r.status === 201,
        });
    }

    sleep(1);
}
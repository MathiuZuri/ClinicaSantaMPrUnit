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

    // Único endpoint: obtener todos los permisos
    const res = http.get(`${BASE_URL}/api/permisos`, params);
    check(res, {
        'GET permisos status 200': (r) => r.status === 200,
        'permisos responde con datos': (r) => {
            const body = r.json();
            return body?.data && Array.isArray(body.data) && body.data.length > 0;
        },
    });

    sleep(1);
}
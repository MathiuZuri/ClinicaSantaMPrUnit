import http from 'k6/http';
import { check, sleep } from 'k6';

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

function login() {
    const payload = JSON.stringify({
        usuarioOCorreo: 'admin',
        password: 'admin123'
    });

    const params = {
        headers: {
            'Content-Type': 'application/json'
        },
    };

    const res = http.post(`${BASE_URL}/api/auth/login`, payload, params);

    if (res.status !== 200) {
        return null;
    }

    return res.json('data.token') || res.json('token');
}

export default function () {
    const token = login();

    if (!token) {
        check(null, {
            'login falló': () => false,
        });
        return;
    }

    const params = {
        headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    };

    const res = http.get(`${BASE_URL}/api/pacientes`, params);

    check(res, {
        'GET pacientes status 200': (r) => r.status === 200,
        'respuesta menor a 1500ms': (r) => r.timings.duration < 1500,
    });

    sleep(1);
}
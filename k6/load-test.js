import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 20,
    duration: '30s',
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<1000'],
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

    check(res, {
        'login correcto': (r) => r.status === 200,
    });

    return res.json('data.token') || res.json('token');
}

export default function () {
    const token = login();

    const params = {
        headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    };

    const pacientesRes = http.get(`${BASE_URL}/api/pacientes`, params);

    check(pacientesRes, {
        'GET pacientes 200': (r) => r.status === 200,
    });

    const citasRes = http.get(`${BASE_URL}/api/citas`, params);

    check(citasRes, {
        'GET citas 200': (r) => r.status === 200,
    });

    const serviciosRes = http.get(`${BASE_URL}/api/servicios-clinicos`, params);

    check(serviciosRes, {
        'GET servicios clinicos 200': (r) => r.status === 200,
    });

    sleep(1);
}
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 1,
    duration: '10s',
};

const BASE_URL = __ENV.BASE_URL || 'https://localhost:7241';

export default function () {
    const loginPayload = JSON.stringify({
        usuarioOCorreo: 'admin',
        password: 'admin123'
    });

    const loginParams = {
        headers: {
            'Content-Type': 'application/json'
        },
    };

    const loginRes = http.post(`${BASE_URL}/api/auth/login`, loginPayload, loginParams);

    check(loginRes, {
        'login status 200': (r) => r.status === 200,
        'login devuelve token': (r) => r.json('data.token') !== undefined || r.json('token') !== undefined,
    });

    let token = loginRes.json('data.token') || loginRes.json('token');

    const authParams = {
        headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    };

    const pacientesRes = http.get(`${BASE_URL}/api/pacientes`, authParams);

    check(pacientesRes, {
        'pacientes status 200': (r) => r.status === 200,
        'pacientes responde': (r) => r.body.length > 0,
    });

    sleep(1);
}
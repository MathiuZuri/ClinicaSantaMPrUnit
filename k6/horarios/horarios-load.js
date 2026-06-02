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

    // Lectura pesada: listado general de horarios
    const resHorarios = http.get(`${BASE_URL}/api/horarios`, params);
    check(resHorarios, {
        'GET horarios status 200': (r) => r.status === 200,
    });

    // Obtener un doctor aleatorio y consultar su matriz semanal (carga mixta)
    const resDoctores = http.get(`${BASE_URL}/api/doctores`, params);
    const doctores = resDoctores.json()?.data || [];
    if (doctores.length > 0) {
        const doctor = doctores[Math.floor(Math.random() * doctores.length)];
        const resMatriz = http.get(`${BASE_URL}/api/horarios/doctor/${doctor.id}/matriz`, params);
        check(resMatriz, {
            'GET matriz semanal status 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}
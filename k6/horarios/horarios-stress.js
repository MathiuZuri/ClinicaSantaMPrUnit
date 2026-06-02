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
    const resHorarios = http.get(`${BASE_URL}/api/horarios`, params);
    check(resHorarios, {
        'GET horarios status 200': (r) => r.status === 200,
    });

    // Creación ocasional (5% de las iteraciones) para simular escritura bajo estrés
    if (Math.random() < 0.05) {
        const resDoctores = http.get(`${BASE_URL}/api/doctores`, params);
        const doctores = resDoctores.json()?.data || [];
        if (doctores.length > 0) {
            const doctorId = doctores[0].id;
            const payloadHorario = JSON.stringify({
                doctorId: doctorId,
                diaSemana: 'Wednesday',
                horaInicio: '14:00',
                horaFin: '18:00',
                fechaInicioVigencia: '2026-06-01',
                fechaFinVigencia: null
            });
            const resCrear = http.post(`${BASE_URL}/api/horarios`, payloadHorario, params);
            check(resCrear, {
                'POST crear horario stress status 200': (r) => r.status === 200,
            });
        }
    }

    sleep(1);
}
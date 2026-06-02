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

    // Golpeamos lista de pacientes y, eventualmente, un POST de creación para simular carga mixta
    const resLista = http.get(`${BASE_URL}/api/pacientes`, params);
    check(resLista, {
        'GET pacientes status 200': (r) => r.status === 200,
    });

    // Solo un pequeño porcentaje de las iteraciones intentará crear un paciente
    if (Math.random() < 0.05) {
        const dniAleatorio = `98${Math.floor(Math.random() * 90000000) + 10000000}`;
        const payloadCrear = JSON.stringify({
            dni: dniAleatorio,
            nombres: 'Stress',
            apellidos: 'K6',
            fechaNacimiento: '2000-05-10T00:00:00Z',
            sexo: 'M',
            celular: '987111222',
            correo: `stress.${dniAleatorio}@test.com`,
            direccion: 'Av. Carga'
        });

        const resCrear = http.post(`${BASE_URL}/api/pacientes`, payloadCrear, params);
        check(resCrear, {
            'POST crear paciente stress status 201': (r) => r.status === 201,
        });
    }

    sleep(1);
}
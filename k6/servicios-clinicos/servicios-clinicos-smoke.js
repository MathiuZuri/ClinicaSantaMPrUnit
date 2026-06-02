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

    // 1. Obtener todos los servicios clínicos
    const resTodos = http.get(`${BASE_URL}/api/serviciosclinicos`, params);
    check(resTodos, {
        'GET todos los servicios status 200': (r) => r.status === 200,
    });

    // 2. Obtener servicios activos
    const resActivos = http.get(`${BASE_URL}/api/serviciosclinicos/activos`, params);
    check(resActivos, {
        'GET servicios activos status 200': (r) => r.status === 200,
    });

    // Si hay servicios, probar obtener por ID
    const body = resTodos.json();
    const servicios = body?.data || [];
    if (servicios.length > 0) {
        const primerServicio = servicios[0];
        const resPorId = http.get(`${BASE_URL}/api/serviciosclinicos/${primerServicio.id}`, params);
        check(resPorId, {
            'GET servicio por id status 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}
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

    // 1. Obtener todos los doctores
    const resTodos = http.get(`${BASE_URL}/api/doctores`, params);
    check(resTodos, {
        'GET todos los doctores status 200': (r) => r.status === 200,
    });

    // 2. Obtener doctores activos
    const resActivos = http.get(`${BASE_URL}/api/doctores/activos`, params);
    check(resActivos, {
        'GET doctores activos status 200': (r) => r.status === 200,
    });

    // Extraer el primer doctor (si existe) para pruebas adicionales
    const bodyTodos = resTodos.json();
    const doctores = bodyTodos?.data || [];
    if (doctores.length > 0) {
        const primerDoctor = doctores[0];

        // 3. Obtener doctor por ID
        const resPorId = http.get(`${BASE_URL}/api/doctores/${primerDoctor.id}`, params);
        check(resPorId, {
            'GET doctor por id status 200': (r) => r.status === 200,
        });
    }

    // 4. Crear un doctor nuevo (POST) con CMP aleatorio para no chocar
    const cmpAleatorio = `CMP${Math.floor(Math.random() * 900000) + 100000}`;
    const payloadCrear = JSON.stringify({
        cmp: cmpAleatorio,
        nombres: 'Test',
        apellidos: 'K6',
        especialidad: 'Obstetricia',
        celular: '987654321',
        correo: `doctor.${cmpAleatorio}@test.com`,
        fechaInicioContrato: '2026-01-15T00:00:00Z'
    });

    const resCrear = http.post(`${BASE_URL}/api/doctores`, payloadCrear, params);
    check(resCrear, {
        'POST crear doctor status 201': (r) => r.status === 201,
    });

    sleep(1);
}
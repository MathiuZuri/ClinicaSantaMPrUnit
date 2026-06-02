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

    // 1. Obtener un paciente de prueba (asumimos que el seeder ya creó al menos uno)
    const resPacientes = http.get(`${BASE_URL}/api/pacientes`, params);
    check(resPacientes, {
        'GET pacientes status 200': (r) => r.status === 200,
    });

    const pacientes = resPacientes.json()?.data || [];
    if (pacientes.length === 0) {
        console.log('No hay pacientes en la base de datos; se omite la prueba de historial.');
        return;
    }

    const pacienteId = pacientes[0].id;

    // 2. Obtener historial por paciente
    const resHistorial = http.get(`${BASE_URL}/api/historiales/paciente/${pacienteId}`, params);
    check(resHistorial, {
        'GET historial por paciente status 200': (r) => r.status === 200,
    });

    // 3. Obtener historial con detalles (usando el id del historial obtenido)
    const historial = resHistorial.json()?.data;
    if (historial && historial.id) {
        const resDetalles = http.get(`${BASE_URL}/api/historiales/${historial.id}/detalles`, params);
        check(resDetalles, {
            'GET historial con detalles status 200': (r) => r.status === 200,
        });
    } else {
        console.log('El paciente no tiene historial clínico asociado.');
    }

    sleep(1);
}
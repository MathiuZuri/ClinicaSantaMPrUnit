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

    // 1. Obtener todas las citas
    const resTodas = http.get(`${BASE_URL}/api/citas`, params);
    check(resTodas, {
        'GET todas las citas status 200': (r) => r.status === 200,
    });

    // Extraer el id de la primera cita (si existe)
    const body = resTodas.json();
    const citas = body?.data || [];
    if (citas.length > 0) {
        const primera = citas[0];

        // 2. Obtener cita por ID
        const resPorId = http.get(`${BASE_URL}/api/citas/${primera.id}`, params);
        check(resPorId, {
            'GET cita por id status 200': (r) => r.status === 200,
        });

        // 3. Obtener citas por paciente
        const resPorPaciente = http.get(`${BASE_URL}/api/citas/paciente/${primera.pacienteId}`, params);
        check(resPorPaciente, {
            'GET citas por paciente status 200': (r) => r.status === 200,
        });

        // 4. Obtener citas por doctor
        const resPorDoctor = http.get(`${BASE_URL}/api/citas/doctor/${primera.doctorId}`, params);
        check(resPorDoctor, {
            'GET citas por doctor status 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}
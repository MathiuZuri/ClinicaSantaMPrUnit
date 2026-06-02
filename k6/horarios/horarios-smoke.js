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

    // 1. Obtener todos los horarios
    const resTodos = http.get(`${BASE_URL}/api/horarios`, params);
    check(resTodos, {
        'GET todos los horarios status 200': (r) => r.status === 200,
    });

    // 2. Obtener doctores para disponer de un doctorId válido
    const resDoctores = http.get(`${BASE_URL}/api/doctores`, params);
    const doctores = resDoctores.json()?.data || [];
    if (doctores.length > 0) {
        const doctorId = doctores[0].id;

        // 3. Horarios por doctor
        const resPorDoctor = http.get(`${BASE_URL}/api/horarios/doctor/${doctorId}`, params);
        check(resPorDoctor, {
            'GET horarios por doctor status 200': (r) => r.status === 200,
        });

        // 4. Matriz semanal (sin fecha, toma hoy por defecto)
        const resMatriz = http.get(`${BASE_URL}/api/horarios/doctor/${doctorId}/matriz`, params);
        check(resMatriz, {
            'GET matriz semanal status 200': (r) => r.status === 200,
        });

        // 5. Crear un horario (POST)
        const payloadHorario = JSON.stringify({
            doctorId: doctorId,
            diaSemana: 'Monday',
            horaInicio: '08:00',
            horaFin: '12:00',
            fechaInicioVigencia: '2026-06-01',
            fechaFinVigencia: null
        });

        const resCrear = http.post(`${BASE_URL}/api/horarios`, payloadHorario, params);
        check(resCrear, {
            'POST crear horario status 200': (r) => r.status === 200,
        });

        // 6. Actualizar el horario recién creado (PUT)
        if (resCrear.status === 200) {
            const nuevoId = resCrear.json()?.data?.id;
            if (nuevoId) {
                const payloadActualizar = JSON.stringify({
                    diaSemana: 'Tuesday',
                    horaInicio: '09:00',
                    horaFin: '13:00',
                    fechaInicioVigencia: '2026-06-01',
                    fechaFinVigencia: null,
                    activo: true
                });
                const resActualizar = http.put(`${BASE_URL}/api/horarios/${nuevoId}`, payloadActualizar, params);
                check(resActualizar, {
                    'PUT actualizar horario status 200': (r) => r.status === 200,
                });
            }
        }
    }

    sleep(1);
}
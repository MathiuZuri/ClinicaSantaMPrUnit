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

    // 1. Obtener todos los pacientes
    const resTodos = http.get(`${BASE_URL}/api/pacientes`, params);
    check(resTodos, {
        'GET todos los pacientes status 200': (r) => r.status === 200,
    });

    // Extraer el primer paciente (si existe)
    const bodyTodos = resTodos.json();
    const pacientes = bodyTodos?.data || [];
    if (pacientes.length > 0) {
        const primerPaciente = pacientes[0];

        // 2. Obtener paciente por ID
        const resPorId = http.get(`${BASE_URL}/api/pacientes/${primerPaciente.id}`, params);
        check(resPorId, {
            'GET paciente por id status 200': (r) => r.status === 200,
        });

        // 3. Obtener paciente por DNI
        const resPorDni = http.get(`${BASE_URL}/api/pacientes/dni/${primerPaciente.dni}`, params);
        check(resPorDni, {
            'GET paciente por DNI status 200': (r) => r.status === 200,
        });
    }

    // 4. Crear un paciente nuevo (POST) con DNI aleatorio para no chocar
    const dniAleatorio = `99${Math.floor(Math.random() * 90000000) + 10000000}`;
    const payloadCrear = JSON.stringify({
        dni: dniAleatorio,
        nombres: 'Test',
        apellidos: 'K6',
        fechaNacimiento: '2000-01-15T00:00:00Z',
        sexo: 'F',
        celular: '999888777',
        correo: `test.${dniAleatorio}@test.com`,
        direccion: 'Calle K6'
    });

    const resCrear = http.post(`${BASE_URL}/api/pacientes`, payloadCrear, params);
    check(resCrear, {
        'POST crear paciente status 201': (r) => r.status === 201,
    });

    sleep(1);
}
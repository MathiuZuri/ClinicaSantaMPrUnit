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

    // 1. Obtener todos los usuarios
    const resUsuarios = http.get(`${BASE_URL}/api/usuarios`, params);
    check(resUsuarios, {
        'GET todos los usuarios status 200': (r) => r.status === 200,
    });

    const usuarios = resUsuarios.json()?.data || [];
    if (usuarios.length > 0) {
        // 2. Obtener usuario por ID (usamos el primero)
        const primerUsuario = usuarios[0];
        const resPorId = http.get(`${BASE_URL}/api/usuarios/${primerUsuario.id}`, params);
        check(resPorId, {
            'GET usuario por id status 200': (r) => r.status === 200,
        });
    }

    // 3. Crear un nuevo usuario (POST) con datos aleatorios
    const username = `test_${Date.now()}_${Math.random().toString(36).substr(2, 5)}`;
    const payloadCrear = JSON.stringify({
        nombres: 'Test',
        apellidos: 'K6',
        userName: username,
        correo: `${username}@test.com`,
        password: 'Password123!'
    });

    const resCrear = http.post(`${BASE_URL}/api/usuarios`, payloadCrear, params);
    check(resCrear, {
        'POST crear usuario status 201': (r) => r.status === 201,
    });

    const nuevoUsuarioId = resCrear.json()?.data?.id;
    if (nuevoUsuarioId) {
        // 4. Actualizar el usuario recién creado (PUT)
        const payloadActualizar = JSON.stringify({
            nombres: 'Actualizado',
            apellidos: 'K6',
            userName: username + '_edit',
            correo: `${username}_edit@test.com`
        });
        const resActualizar = http.put(`${BASE_URL}/api/usuarios/${nuevoUsuarioId}`, payloadActualizar, params);
        check(resActualizar, {
            'PUT actualizar usuario status 200': (r) => r.status === 200,
        });

        // 5. Cambiar estado del usuario (PUT .../estado)
        const payloadEstado = JSON.stringify({ estado: 2 }); // Inactivo
        const resEstado = http.put(`${BASE_URL}/api/usuarios/${nuevoUsuarioId}/estado`, payloadEstado, params);
        check(resEstado, {
            'PUT cambiar estado status 200': (r) => r.status === 200,
        });
    }

    // 6. Asignar rol a un usuario existente (POST /asignar-rol)
    const resRoles = http.get(`${BASE_URL}/api/roles`, params);
    const roles = resRoles.json()?.data || [];
    if (usuarios.length > 0 && roles.length > 0) {
        const payloadAsignar = JSON.stringify({
            usuarioId: usuarios[0].id,
            rolId: roles[0].id
        });
        const resAsignar = http.post(`${BASE_URL}/api/usuarios/asignar-rol`, payloadAsignar, params);
        check(resAsignar, {
            'POST asignar rol status 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}
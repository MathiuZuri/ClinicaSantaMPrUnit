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

    // 1. Obtener todos los roles
    const resRoles = http.get(`${BASE_URL}/api/roles`, params);
    check(resRoles, {
        'GET todos los roles status 200': (r) => r.status === 200,
    });

    const roles = resRoles.json()?.data || [];
    if (roles.length > 0) {
        // 2. Obtener rol por ID
        const primerRol = roles[0];
        const resPorId = http.get(`${BASE_URL}/api/roles/${primerRol.id}`, params);
        check(resPorId, {
            'GET rol por id status 200': (r) => r.status === 200,
        });
    }

    // 3. Crear un nuevo rol (POST)
    const nombreRol = `Rol_K6_${Date.now()}_${Math.random().toString(36).substr(2, 5)}`;
    const payloadCrear = JSON.stringify({
        nombre: nombreRol,
        descripcion: 'Rol creado por script de carga k6'
    });

    const resCrear = http.post(`${BASE_URL}/api/roles`, payloadCrear, params);
    check(resCrear, {
        'POST crear rol status 201': (r) => r.status === 201,
    });

    const nuevoRolId = resCrear.json()?.data?.id;
    if (nuevoRolId) {
        // 4. Actualizar el rol recién creado (PUT)
        const payloadActualizar = JSON.stringify({
            nombre: nombreRol + '_editado',
            descripcion: 'Rol actualizado por k6',
            activo: true
        });
        const resActualizar = http.put(`${BASE_URL}/api/roles/${nuevoRolId}`, payloadActualizar, params);
        check(resActualizar, {
            'PUT actualizar rol status 200': (r) => r.status === 200,
        });
    }

    // 5. Asignar permisos a un rol (POST /asignar-permisos)
    const resPermisos = http.get(`${BASE_URL}/api/permisos`, params);
    const permisos = resPermisos.json()?.data || [];
    if (roles.length > 0 && permisos.length > 0) {
        const payloadAsignar = JSON.stringify({
            rolId: roles[0].id,
            permisosIds: [permisos[0].id]
        });
        const resAsignar = http.post(`${BASE_URL}/api/roles/asignar-permisos`, payloadAsignar, params);
        check(resAsignar, {
            'POST asignar permisos status 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}
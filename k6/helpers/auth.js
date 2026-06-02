import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'https://localhost:7241';

export function login() {
    const payload = JSON.stringify({
        usuarioOCorreo: 'admin',
        password: 'admin123'
    });

    const params = {
        headers: {
            'Content-Type': 'application/json'
        },
    };

    const res = http.post(`${BASE_URL}/api/auth/login`, payload, params);

    const ok = check(res, {
        'login correcto': (r) => r.status === 200,
    });

    if (!ok) {
        console.error('Login falló con status: ' + res.status);
        return null;
    }

    // Intentar extraer el token de la estructura de ApiResponse
    const body = res.json();
    const token = body?.data?.token || body?.token;

    if (!token) {
        console.error('No se encontró token en la respuesta de login');
        return null;
    }

    return token;
}
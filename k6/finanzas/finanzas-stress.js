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

    const today = new Date().toISOString().split('T')[0];
    const currentYear = new Date().getFullYear();
    const currentMonth = new Date().getMonth() + 1;

    // Lecturas intensivas: resumen diario y pagos pendientes
    const resDiario = http.get(`${BASE_URL}/api/finanzas/resumen-diario?fecha=${today}`, params);
    check(resDiario, { 'GET resumen diario status 200': (r) => r.status === 200 });

    const resPendientes = http.get(`${BASE_URL}/api/finanzas/pagos-pendientes`, params);
    check(resPendientes, { 'GET pagos pendientes status 200': (r) => r.status === 200 });

    // Un pequeño porcentaje de las iteraciones también consulta el resumen mensual completo (más pesado)
    if (Math.random() < 0.2) {
        const resCompleto = http.get(`${BASE_URL}/api/finanzas/resumen-financiero-mensual-completo?anio=${currentYear}&mes=${currentMonth}`, params);
        check(resCompleto, { 'GET resumen mensual completo status 200': (r) => r.status === 200 });
    }

    sleep(1);
}
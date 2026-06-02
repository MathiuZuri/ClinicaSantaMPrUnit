import http from 'k6/http';
import { check, sleep } from 'k6';
import { login } from '../helpers/auth.js';

export const options = {
    vus: 20,
    duration: '30s',
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<1000'],
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

    // Simulamos las consultas más frecuentes: resumen diario, pagos pendientes y resumen mensual
    const responses = http.batch([
        ['GET', `${BASE_URL}/api/finanzas/resumen-diario?fecha=${today}`, null, params],
        ['GET', `${BASE_URL}/api/finanzas/pagos-pendientes`, null, params],
        ['GET', `${BASE_URL}/api/finanzas/resumen-mensual?anio=${currentYear}&mes=${currentMonth}`, null, params],
    ]);

    check(responses[0], { 'GET resumen diario status 200': (r) => r.status === 200 });
    check(responses[1], { 'GET pagos pendientes status 200': (r) => r.status === 200 });
    check(responses[2], { 'GET resumen mensual status 200': (r) => r.status === 200 });

    sleep(1);
}
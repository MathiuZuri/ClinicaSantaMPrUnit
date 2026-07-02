# 📊 Métricas y KPIs del Sistema — SYS Clínica Santa Mónica

Este documento consolida los principales indicadores de calidad, rendimiento y negocio que permiten evaluar la salud del sistema **SYS Clínica Santa Mónica** y su impacto en la operación de la clínica. Las métricas se dividen en tres categorías: **técnicas** (calidad del código, rendimiento), **operativas** (uso del sistema por el personal) y **de negocio** (impacto en la atención y la fidelización de pacientes). 

Estas métricas reflejan el estado real verificado por el equipo de desarrollo **UNIXCODE** y validadas formalmente en el cierre de auditoría del ciclo de vida del software (SDLC) finalizado con éxito en julio de 2026.

---

## 🎯 Propósito de las Métricas

La medición sistemática de indicadores es un pilar de la filosofía de **mejora continua** de la Clínica Santa Mónica. Estas métricas permiten:

- **Detectar tempranamente desviaciones** en el rendimiento del sistema o en la calidad del servicio.
- **Tomar decisiones basadas en datos** para priorizar inversiones en desarrollo, infraestructura o capacitación.
- **Validar el cumplimiento de los objetivos estratégicos** definidos en el plan de modernización digital.
- **Identificar oportunidades de optimización** en los procesos clínicos y administrativos.

---

## 📊 Métricas Técnicas (Calidad y Rendimiento del Software)

Estas métricas evalúan la robustez, seguridad y eficiencia del sistema desde la perspectiva de la ingeniería de software, alineadas con los estándares ISO/IEC 25010 y validadas en el entorno real de QA.

| Métrica | Descripción | Valor Objetivo | Estado Actual | Frecuencia de Medición |
| :--- | :--- | :--- | :--- | :--- |
| **Cobertura de Código** | Porcentaje de líneas de código ejecutadas por pruebas unitarias e integración (medido con Coverlet). | >80% | **85%** (backend), **72%** (frontend)<br>*(Módulo Finanzas corregido de 56.8% a >80% con NSubstitute)* | Por cada pull request |
| **Densidad de Bugs** | Número de bugs críticos por cada 1,000 líneas de código (identificados por SonarCloud). | <0.5 | **0** *(Calificación A por SonarCloud)* | Semanal |
| **Deuda Técnica** | Tiempo estimado para corregir todos los problemas de mantenibilidad (en horas). | <20 horas | **12 horas** *(Calificación A en mantenibilidad)* | Mensual |
| **Tiempo de Respuesta de la API** | Latencia promedio de los endpoints críticos (`/api/atenciones`, `/api/citas`) bajo carga normal y estrés. | <200 ms | **180 ms** *(Bajo carga normal)*<br>**284 ms** *(Bajo estrés extremo de 100 VUs en k6)* | Diario |
| **Tasa de Errores HTTP** | Porcentaje de peticiones que retornan códigos 5xx. | <0.5% | **0.2%** | Diario |
| **Tiempo de Inactividad (Uptime)** | Disponibilidad del sistema en la nube (Azure y Neon.tech). | >99.5% | **99.7%** | Mensual |
| **Velocidad de Despliegue** | Tiempo promedio desde el commit hasta el despliegue en producción (CI/CD). | <10 min | **8 min** | Por despliegue |
| **Cobertura de Pruebas de Integración** | Porcentaje de flujos críticos cubiertos por pruebas de integración (Testcontainers). | >70% | **75%** | Mensual |
| **Vulnerabilidades de Seguridad** | Número de vulnerabilidades de alta severidad detectadas por Snyk y SonarCloud. | 0 | **0** *(2 falsos positivos bloqueantes en blazor.boot.json fueron corregidos y excluidos)* | Semanal |

---

## 📈 Métricas Operativas (Uso y Adopción del Sistema)

Estas métricas reflejan cómo el personal de la clínica utiliza el sistema en su día a día, y permiten identificar necesidades de capacitación o mejoras en la experiencia de usuario.

| Métrica | Descripción | Valor Actual | Tendencia | Frecuencia de Medición |
| :--- | :--- | :--- | :--- | :--- |
| **Tasa de Adopción del Sistema** | Porcentaje del personal que utiliza activamente el sistema para sus tareas diarias. | 92% | **En aumento** (capacitación continua) | Mensual |
| **Tiempo Promedio de Registro de Paciente** | Tiempo que tarda una recepcionista en registrar un nuevo paciente en el sistema. | 2.5 min | **Reduciendo** (automatización) | Semanal |
| **Tiempo Promedio de Programación de Cita** | Tiempo desde que se solicita una cita hasta que queda registrada en el sistema. | 3 min | **Estable** | Semanal |
| **Número de Incidencias Reportadas** | Tickets de soporte técnico abiertos por el personal. | 12 por mes | **Descendiendo** | Mensual |
| **Tasa de Satisfacción del Personal** | Encuesta interna sobre la usabilidad y utilidad del sistema. | 4.2/5 | **Estable** | Trimestral |
| **Porcentaje de Citas Programadas Sin Conflictos** | Citas que no requieren reprogramación por sobreposición (validación `ExisteInterferenciaHorarioAsync`). | 98% | **Estable** | Mensual |
| **Tiempo de Capacitación Promedio** | Horas de entrenamiento necesarias para que un nuevo usuario alcance competencia básica en el sistema. | 4 horas | **Reduciendo** (mejora de UX) | Por nuevo usuario |

---

## 💰 Métricas de Negocio (Impacto en la Atención y la Fidelización)

Estas métricas vinculan el rendimiento del sistema con los resultados estratégicos de la clínica: retención de pacientes, eficiencia operativa y crecimiento de ingresos.

| Métrica | Descripción | Valor Actual | Tendencia | Frecuencia de Medición |
| :--- | :--- | :--- | :--- | :--- |
| **Tasa de Reconsulta de Pacientes** | Porcentaje de pacientes que regresan para una segunda consulta o control dentro del mismo año. | 65% | **Aumentando** (gracias a recordatorios y seguimiento) | Trimestral |
| **Tasa de Ausentismo** | Porcentaje de citas programadas que no se concretan (sin previo aviso). | 12% | **Descendiendo** (recordatorios automáticos de WhatsApp) | Mensual |
| **Tiempo de Espera Promedio en Recepción** | Tiempo desde la llegada de la paciente hasta el inicio de la consulta. | 8 min | **Reduciendo** (agenda optimizada) | Semanal |
| **Duración Promedio de Consulta** | Tiempo que el médico u obstetra dedica a cada paciente. | 20 min | **Estable** (calidad asistencial) | Mensual |
| **Porcentaje de Pacientes con Historial Digital** | Pacientes que tienen su historial clínico completamente migrado al sistema electrónico. | 60% (3,000 de 5,000) | **Acelerando** (meta: 100% al final del año) | Trimestral |
| **Ingresos por Servicio** | Distribución de los ingresos entre los distintos servicios (consultas, ecografías, partos, farmacia). | Consultas: 40%<br>Ecografías: 25%<br>Partos: 20%<br>Farmacia: 15% | **Estable** | Mensual |
| **Tasa de Fidelización** | Pacientes que han regresado para al menos un control posterior al parto. | 70% | **Aumentando** | Trimestral |
| **NPS (Net Promoter Score)** | Probabilidad de que una paciente recomiende la clínica a otras personas (escala 0-10). | 8.5 | **Mejorando** | Trimestral |
| **Porcentaje de Comprobantes Electrónicos Emitidos** | Cumplimiento de facturación electrónica conforme a SUNAT. | 100% (integración total) | **Estable** | Mensual |

---

## 📋 Cuadro de Mando Integral (Balanced Scorecard)

A continuación se presenta un resumen del cuadro de mando integral alineado con los objetivos estratégicos de la clínica. Las métricas se agrupan en cuatro perspectivas: **Financiera, Cliente, Procesos Internos y Aprendizaje/Crecimiento**.

| Perspectiva | Objetivo Estratégico | Indicador Clave | Valor Actual | Meta |
| :--- | :--- | :--- | :--- | :--- |
| **Financiera** | Incrementar la rentabilidad de la farmacia integrada | % de recetas dispensadas en la farmacia propia (vs. externas) | 45% | 60% |
| **Cliente** | Reducir el ausentismo y mejorar la experiencia | Tasa de ausentismo | 12% | <8% |
| **Cliente** | Aumentar la fidelización de pacientes postparto | Tasa de reconsulta postparto | 65% | 75% |
| **Procesos Internos** | Optimizar la ocupación de consultorios | Ocupación promedio de consultorios | **12.5%** *(Línea base inicial en auditoría; en incremento)* | 85% |
| **Procesos Internos** | Minimizar errores administrativos | Tasa de errores en registro de pacientes | 2% | <1% |
| **Aprendizaje/Crecimiento** | Acelerar la adopción digital del personal | % de personal con capacitación completa | 80% | 100% |
| **Aprendizaje/Crecimiento** | Reducir el tiempo de resolución de incidencias | Tiempo promedio de cierre de tickets de soporte | 4 horas | <2 horas |

---

## 🛠️ Herramientas de Medición y Recolección

Las métricas se obtienen a través de las siguientes herramientas y procesos:

| Herramienta | Métricas que Recolecta | Frecuencia de Recolección |
| :--- | :--- | :--- |
| **SonarCloud** | Cobertura de código, densidad de bugs, deuda técnica, vulnerabilidades e índices de confiabilidad. | Por cada build (GitHub Actions) |
| **Snyk** | Vulnerabilidades en dependencias de paquetes NuGet | Diario |
| **Azure Application Insights** | Tiempo de respuesta de API, tasa de errores HTTP, uptime | Tiempo real |
| **k6 (Grafana)** | Tiempo de respuesta bajo carga masiva y simulación de hasta 100 usuarios concurrentes (VUs). | Mensual (pruebas de carga) |
| **Base de datos PostgreSQL** | Métricas operativas: número de citas, pacientes registrados, comprobantes emitidos, ingresos | Diario (consultas SQL programadas) |
| **Encuestas internas (Google Forms)** | Satisfacción del personal, NPS | Trimestral |
| **Bitácora de incidencias (Jira)** | Número y tiempo de resolución de tickets de soporte y criterios de aceptación (DoD). | Semanal |

---

## 📝 Notas sobre la Interpretación de las Métricas

- **La cobertura de código del módulo de Finanzas:** En las primeras fases de auditoría, se detectó una cobertura insuficiente del 56.8% en las ramas de este módulo (Hallazgo HA-01). En respuesta, UNIXCODE implementó una batería intensiva de pruebas unitarias utilizando NSubstitute para cubrir todos los flujos condicionales de cálculos de deudas y saldos parciales, elevando con éxito el backend general a un sólido 85%.
- **Gestión de dependencias de mensajería (Evolution API):** Como acción correctiva para solucionar el riesgo de una caída del canal de WhatsApp, se diseñó e integró un servicio de fallback secundario automático mediante correo electrónico (SendGrid), el cual se activa de inmediato si la API principal falla tras 3 reintentos de comunicación con la paciente.
- **La tasa de ocupación operativa de consultorios (12.5%):** Representó un hallazgo comercial crítico en el Balanced Scorecard (HA-04). Como contramedida de negocio inmediata para rentabilizar la infraestructura de TI, el área administrativa lanzó el 01 de julio de 2026 su primera campaña publicitaria formal a través de Facebook Ads, orientada a la captación de nuevas pacientes para servicios prenatales.
- **La migración de los 5,000 registros históricos** es un proyecto prioritario. El avance del 60% implica que aún quedan 2,000 registros por digitalizar. El proceso se ejecuta de manera controlada con validaciones cruzadas por lotes y se estima su finalización en el tercer trimestre de 2026.

---

## 🚀 Proyección de Métricas para la Expansión a Clínica General

Con la expansión planificada hacia una clínica general, se espera que las siguientes métricas evolucionen:

| Métrica | Valor Actual (2026) | Proyección (2028) | Acción Requerida |
| :--- | :--- | :--- | :--- |
| Pacientes activos | 800/mes | 1,500/mes | Escalar infraestructura (VPS) y optimizar consultas SQL |
| Número de servicios | 8 | 15+ | Extender el catálogo de servicios en el sistema |
| Personal del sistema | 15 usuarios | 30+ usuarios | Revisar políticas de permisos y roles |
| Tiempo de respuesta de API | 180 ms | <250 ms (en pico) | Implementar cache (Redis) y balanceo de carga |
| Tasa de adopción del sistema | 92% | 95% | Mantener programa de capacitación continua |

---

## 💻 Verificación del Entorno de Aceptación (UAT)

Como parte de la validación final del sistema frente al docente revisor y la gerencia de la clínica, se encuentra desplegado un entorno en la nube para la constatación en vivo de los tiempos de respuesta y las transacciones de facturación automatizadas:

* **URL del Entorno de Demostración:** [https://salmon-bush-08c1e7510.7.azurestaticapps.net](https://salmon-bush-08c1e7510.7.azurestaticapps.net)
* **Credenciales de Acceso:** 
    * **Usuario:** `admin`
    * **Contraseña:** `admin123`
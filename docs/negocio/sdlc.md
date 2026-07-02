# ⚖️ Auditoría del Ciclo de Vida del Desarrollo (SDLC) — SYS Clínica Santa Mónica

Este componente documenta los hallazgos, metodologías de verificación, registros de campo y listas de comprobación aplicadas durante la auditoría formal de Gobierno de TI sobre el Ciclo de Vida del Desarrollo de Software (SDLC) de la plataforma informática **SYS Clínica Santa Mónica**[cite: 2]. Realizada sobre el sistema de información ginecobstétrico desarrollado por la startup **UNIXCODE**[cite: 2], esta auditoría se enmarca en el plan estratégico de modernización digital de la clínica para garantizar la seguridad, confiabilidad y sostenibilidad del sistema en el contexto operativo de Juliaca (Región Puno).

---

## 📋 Información y Alcance General del Proyecto de Auditoría

* **Código Oficial de Auditoría:** AUD-SDLC-SYS-CSM-2026-001[cite: 2].
* **Objetivo de Control:** Evaluar y certificar que cada una de las fases del ciclo de vida del software (Requerimientos, Diseño, Codificación, Testing, Despliegue y Mantenimiento) cumpla con los estándares internacionales de la ingeniería de software y los marcos normativos de la República del Perú. Se enfoca en las disposiciones del MINSA, SUSALUD, SUNAT, la Ley de Protección de Datos Personales (Ley N° 29733) y el DS N° 016-2013-SA.
* **Equipo Auditor y Dirección:** Presentado por el Equipo Auditor SDLC de la Escuela Profesional de Ingeniería de Sistemas de la Universidad Peruana Unión[cite: 2], bajo la supervisión y firma del Auditor Líder, el Ing. Ruben Roque Sucari[cite: 2].
* **Startup Desarrolladora Auditada:** UNIXCODE, integrado por los ingenieros Carlos Santiago Bustamante Carpio, Nahuel Rafael Supo Huahuacondori, Kevin Raphael Paricahua Sanchez y Jeimy Paul Ramos Coaquira[cite: 2].
* **Periodo de Evaluación:** Desde la notificación e inicio formal el 10 de abril de 2026 hasta el cierre de campo e informe final el 02 de julio de 2026[cite: 2].
* **Límites del Alcance:** Comprende la totalidad de los repositorios de código fuente backend (.NET 9) y frontend (Blazor WASM) alojados en GitHub, los esquemas de persistencia relacional en Neon.tech, los flujos operacionales en Juliaca, las métricas de SonarCloud, las pruebas de rendimiento en k6 y la integración con Evolution API[cite: 2].
* **Exclusiones Explícitas:** Se excluye de esta auditoría la evaluación física del hardware de los ecógrafos u otros dispositivos de diagnóstico médico, así como las pasarelas internas de los servidores de comunicación de Meta Corporation.

---

## 🎯 Justificación y Contexto Estratégico

La transición tecnológica desde un modelo vulnerable basado en registros físicos y hojas de cálculo (más de 5,000 historiales clínicos en proceso de migración activa) hacia el ecosistema digital de **SYS Clínica Santa Mónica** exige un control de gobernanza estricto. La auditoría se justifica ante la necesidad de:

1. **Blindar la plataforma contra fugas de información** de datos altamente sensibles de las pacientes (datos de filiación, ginecobstétricos y antecedentes), protegiendo a la clínica de sanciones por parte de SUSALUD o la Autoridad Nacional de Protección de Datos Personales (Ley N° 29733).
2. **Garantizar la continuidad operativa** ante la dependencia crítica de servicios externos (Meta/WhatsApp, VPS, conectividad a Internet), mitigando riesgos que podrían paralizar la atención en un entorno regional complejo.
3. **Validar la calidad e integridad del software** según las buenas prácticas internacionales (ISO/IEC 12207, ISO/IEC 25010 y CMMI-DEV)[cite: 2], asegurando que la arquitectura hexagonal pueda absorber la expansión planificada hacia una clínica general sin comprometer el núcleo[cite: 1].

!!! info "Contexto Regional de Juliaca"
    Juliaca enfrenta desafíos específicos: alta tasa de natalidad, saturación del sistema público de salud y una creciente demanda de servicios privados formalizados[cite: 1]. El sistema desarrollado por UNIXCODE debe operar en un entorno con conectividad variable y cortes de energía[cite: 2]. La auditoría considera estos factores al evaluar la resiliencia y la propuesta de valor del software.

---

## 🧩 Fases del Ciclo de Vida del Desarrollo (SDLC) Evaluadas

La auditoría ha revisado cada fase del ciclo de vida mediante el **Plan de Auditoría**, verificando la existencia de artefactos, la aplicación de buenas prácticas de agilidad (Sprints y Backlog) y la trazabilidad entre las etapas[cite: 2].

| Fase de Auditoría | Periodo de Ejecución | Actividades de Campo Realizadas | Cumplimiento |
| :--- | :--- | :--- | :--- |
| **Fase 1: Preparar y Planificar** | 10 de Abril de 2026 | Emisión de la Comunicación de Inicio formal al startup UNIXCODE[cite: 2]. Definición del alcance, criterios base (ISO 12207, ISO 25010, CMMI) y adaptación del Checklist del SDLC[cite: 2]. | **APROBADO** |
| **Fase 2: Describir el Proceso** | 23 Abr - 25 Jun de 2026 | Trabajo de campo exhaustivo[cite: 2]. Ejecución de entrevistas técnicas presenciales y virtuales con los analistas, arquitectos y desarrolladores backend/frontend[cite: 2]. | **APROBADO** |
| **Fase 3: Evaluar y Reportar** | 26 Jun - 01 Jul de 2026 | Contraste de evidencias versus criterios normativos[cite: 2]. Construcción de la Matriz de Hallazgos, Matriz de Riesgos y emisión del Informe Preliminar y Final[cite: 2]. | **CONFORME con observaciones** |
| **Fase 4: Seguimiento y Cierre** | 02 de Julio de 2026 | Verificación de la subsanación de observaciones a través del Plan de Acción Correctiva y firma del Acta Formal de Cierre[cite: 2]. | **COMPLETADO** |

---

## 🔍 Registro de Entrevistas de Campo de TI

Durante el trabajo de campo de la Fase 2, el equipo auditor ejecutó sesiones de control con los roles clave del proyecto[cite: 2]:

* **Sesión 23/04/2026 — Analista / Arquitecto de Software:** Se auditó la correcta implementación de la eliminación lógica para registros clínicos, la conformidad con la normativa de historias clínicas del MINSA (Ley N° 26842) y el aislamiento de la arquitectura hexagonal mediante diagramas de estado TO-BE[cite: 2].
* **Sesión 20/05/2026 — Backend Developer / Scrum Master:** Se examinó el pipeline de inyección de dependencias de .NET 9, el funcionamiento del servicio en segundo plano `Recordatorio Citas Background Service` y la estabilidad de las llamadas salientes hacia la Evolution API de WhatsApp[cite: 2]. Se constató el correcto uso de la metodología ágil, control de Sprints y priorización del Backlog[cite: 2].
* **Sesión 25/06/2026 — Validación Final del Equipo:** Reunión general con los representantes de la Clínica Santa Mónica, el equipo de UNIXCODE y el docente revisor[cite: 2]. Se evaluaron los paneles de SonarCloud, los reportes de xUnit y se obtuvo la aprobación de aceptación del cliente (Sesión grabada como evidencia EV-07)[cite: 2].

---

## 📊 Lista de Verificación Completa del SDLC (Checklist de Cumplimiento)

A continuación se detalla la matriz de evaluación aplicada sobre los procesos del sistema, cuya versión extendida de control se encuentra indexada formalmente en el documento maestro de diseño (págs. 189 a 203)[cite: 2]:

| Dimensión de Control | Criterio de Evaluación e Ingeniería | Estado | Evidencia de Soporte y Mitigación | Riesgo |
| :--- | :--- | :--- | :--- | :--- |
| **Gobernanza de Seguridad** | ¿El sistema restringe los accesos lógicos según las funciones del colaborador de la clínica? | **APROBADO** | Implementación estricta de Políticas de Autorización basadas en Permisos Atómicos (**Seguridad RBAC**) con tokens JWT de firma asimétrica en la API[cite: 1]. Cada acción está protegida por políticas específicas (ej. `ATENCION_REGISTRAR`, `FINANZAS_VER`)[cite: 1]. | **Bajo** |
| **Protección de Datos Privados** | ¿Se cumple con el marco regulatorio establecido por la Ley N° 29733 en el Perú? | **APROBADO** | Los datos de filiación y diagnósticos clínicos sensibles viajan encriptados y cuentan con trazas de auditoría forense inalterables[cite: 1]. El filtro de la API registra cada acceso, modificación y eliminación, almacenando valores anteriores y nuevos[cite: 1]. | **Medio** |
| **Integridad Fiscal y Tributaria** | ¿Existe consistencia en los registros contables y de facturación electrónica ante SUNAT? | **APROBADO** | Uso de campos configurados de tipo `jsonb` nativos en PostgreSQL, capturando copias inmutables del balance al momento del cobro (snapshots de comprobantes)[cite: 1]. La emisión de comprobantes electrónicos está integrada en el flujo de cierre de atención[cite: 1]. | **Medio** |
| **Gestión de Agenda Asistencial** | ¿El sistema impide la colisión o sobreposición de citas médicas presenciales en Juliaca? | **APROBADO** | Ejecución en el core del negocio del algoritmo de validación temporal indexada `ExisteInterferenciaHorarioAsync`, que evalúa intervalos horarios en la base de datos antes de confirmar una cita[cite: 1]. | **Bajo** |
| **Resiliencia Operativa** | ¿Existe un plan de contingencia ante cortes eléctricos o caídas locales de Internet? | **ADVERTENCIA** | El sistema es 100% cloud dependiente (Azure, Neon.tech)[cite: 1]. Se exige la instalación obligatoria de routers de respaldo móvil 4G/5G y UPS físicos en recepción[cite: 1]. Se recomienda tener un proveedor de hosting alternativo preconfigurado[cite: 1]. | **Alto** |
| **Aseguramiento de Calidad** | ¿El código es inspeccionado de forma automática ante la inyección de bugs o código sucio? | **APROBADO** | Integración nativa de Quality Gates automáticos en GitHub Actions conectados a SonarCloud (cobertura >80%, sin bugs críticos) y escaneos de dependencias con Snyk (vulnerabilidades de alta severidad bloquean el despliegue)[cite: 1]. | **Bajo** |
| **Continuidad de la Migración** | ¿Los 5,000 registros históricos se están migrando de manera controlada? | **EN PROCESO** | La migración de historiales desde Excel y papel se realiza bajo supervisión, con validación de datos y registro de auditoría[cite: 1]. El proceso está documentado y se estima su finalización en el tercer trimestre de 2026[cite: 1]. | **Medio** |
| **Independencia de Proveedores** | ¿Existen alternativas viables para los servicios críticos (WhatsApp, VPS)? | **ADVERTENCIA** | La dependencia de Meta (WhatsApp) es alta[cite: 1]. Se han identificado canales alternativos (SMS, correo, llamadas) y se recomienda mantenerlos operativos como plan de contingencia[cite: 1]. | **Alto** |

---

## 📈 Matriz de Hallazgos y Recomendaciones de Mejora

A través del trabajo de campo de los auditores, se consolidaron los siguientes hallazgos de control estructurados (HA-01 al HA-04)[cite: 2]:

### HA-01: Cobertura de Pruebas Insuficiente en Módulo Financiero
* **Descripción:** El módulo de Finanzas poseía una cobertura inicial de rama de solo 56.8% en sus primeras iteraciones[cite: 2].
* **Criterio / Norma:** CMMI-DEV (Verificación) e ISO/IEC 25010 (Fiabilidad)[cite: 2].
* **Recomendación del Auditor:** Diseñar nuevos casos de prueba unitarios para cubrir los flujos condicionales de cálculos de deudas y saldos parciales[cite: 2].
* **Estado de Cierre:** **CERRADO Y VALIDADO**. El ingeniero Kevin Paricahua desarrolló los tests unitarios complementarios utilizando NSubstitute antes del 28/06/2026, superando el umbral requerido del 80%[cite: 2].

### HA-02: Dependencia Externa Crítica sin Mecanismo de Fallback
* **Descripción:** El sistema depende críticamente de Evolution API para WhatsApp[cite: 2]. Si Meta cambia políticas o tarifas, no había un mecanismo automático de respaldo activo para alertar a las gestantes[cite: 2].
* **Criterio / Norma:** ISO/IEC 25010 (Disponibilidad) y 5 Fuerzas de Porter (Poder de Proveedores)[cite: 2].
* **Recomendación del Auditor:** Implementar una cola de mensajería con un canal secundario de comunicación (Email o SMS) si WhatsApp falla tras 3 reintentos[cite: 2].
* **Estado de Cierre:** **CERRADO**. El equipo de desarrollo backend implementó un servicio de failover acoplado a SendGrid para el despacho automático de correos de respaldo el 30/06/2026[cite: 2].

### HA-03: Falsos Positivos de Seguridad en Compilación del Frontend
* **Descripción:** SonarCloud detectó 2 vulnerabilidades de tipo Blocker erróneas dentro del archivo autogenerado `blazor.boot.json`[cite: 2].
* **Criterio / Norma:** ISO/IEC 25010 (Seguridad) y Buenas Prácticas del SDLC[cite: 2].
* **Recomendación del Auditor:** Configurar el archivo de propiedades de SonarCloud para excluir del análisis de secretos los archivos generados automáticamente por la compilación de Blazor WASM[cite: 2].
* **Estado de Cierre:** **CERRADO**. El área de DevOps reconfiguró el archivo `sonar-project.properties` excluyendo el subdirectorio de compilación el 28/06/2026[cite: 2].

### HA-04: Baja Ocupación Operativa de la Infraestructura TI
* **Descripción:** El Balanced Scorecard (BSC) inicial indicaba una ocupación de los consultorios físicos de solo el 12.5%, derivada de la nula actividad de la clínica en redes sociales, lo que ponía en riesgo el retorno de inversión del software[cite: 2].
* **Criterio / Norma:** Alineación Estratégica (McKinsey 7S) y Viabilidad Comercial[cite: 2].
* **Recomendación del Auditor:** Desplegar inmediatamente el plan de marketing digital establecido en el modelo de negocio para rentabilizar la infraestructura[cite: 2].
* **Estado de Cierre:** **CERRADO**. El área de administración lanzó la primera campaña publicitaria en Facebook Ads enfocada en servicios prenatales el 01/07/2026[cite: 2].

---

## ⚡ Matriz de Riesgos del Proyecto

Derivado de los hallazgos analizados en el trabajo de campo, se catalogaron los riesgos técnicos y financieros de la plataforma[cite: 2]:

| Código | Riesgo Identificado | Probabilidad | Impacto | Nivel de Riesgo |
| :--- | :--- | :--- | :--- | :--- |
| **RI-01** | Caída del servicio de mensajería (WhatsApp) afectando la tasa de retorno de pacientes gestantes a la clínica[cite: 2]. | Media[cite: 2] | Alto[cite: 2] | **ALTO**[cite: 2] |
| **RI-02** | Errores en el cálculo financiero o de comisiones médicas debido a código no cubierto por pruebas unitarias[cite: 2]. | Media[cite: 2] | Alto[cite: 2] | **ALTO**[cite: 2] |
| **RI-03** | Interrupción de la operación por caída del servidor VPS sin un plan de contingencia automatizado documentado[cite: 2]. | Baja[cite: 2] | Crítico[cite: 2] | **ALTO**[cite: 2] |
| **RI-04** | Insostenibilidad financiera del proyecto a futuro provocada por la baja ocupación de los consultorios físicos (12.5%)[cite: 2]. | Alta[cite: 2] | Crítico[cite: 2] | **CRÍTICO**[cite: 2] |

---

## 🔔 Advertencias de Seguridad y Factores Críticos de Éxito

!!! success "Factor Crítico de Éxito: Inmutabilidad Posterior al Cierre"
    Un hito de auditoría aprobado con honores es el comportamiento de bloqueo de la entidad `Atencion`. Una vez que el médico obstetra define la impresión diagnóstica y cierra el acto médico, la capa de API revoca de manera permanente los permisos de edición sobre ese registro. Cualquier intento posterior de alteración de datos clínicos levantará una alerta de seguridad severa, garantizando la fidelidad legal de la historia clínica electrónica ante fiscalizaciones sanitarias.

!!! warning "Advertencia de Mantenimiento Presupuestario: Sostenibilidad del Software"
    La auditoría alerta formalmente a la Gerencia General de la clínica que, si bien el desarrollo del primer año se ejecuta bajo un modelo sin costo de mano de obra por recursos de investigación internos de UNIXCODE[cite: 2], la plataforma entrará en fase de mantenimiento en el segundo año. Se debe presupuestar de forma obligatoria un fondo económico anual para horas de ingeniería especializada; de lo contrario, cambios imprevistos en las APIs externas de Meta (WhatsApp) o en los esquemas de la SUNAT dejarían el sistema inoperativo. Se recomienda asignar al menos el 10% del presupuesto operativo anual a la evolución y soporte del sistema[cite: 1].

---

## 🛠️ Registro de Evidencias Digitales Auditadas

La auditoría validó la existencia de los siguientes artefactos técnicos que respaldan la calidad del desarrollo de software[cite: 2]:

* **EV-01 — main.pdf (Requisitos y Arquitectura):** Documentación técnica completa, diagramas UML, casos de uso, flujos y arquitectura hexagonal. Incluye la lista de verificación extendida (Validado)[cite: 2].
* **EV-02 — Dante_2_corregido_final.pdf:** Documentación del modelo de negocio, PESTEL, 5 Fuerzas de Porter, BSC y McKinsey 7S (Validado)[cite: 2].
* **EV-03 — Repositorio GitHub:** Código fuente en C# (.NET 9), historial completo de commits y Pull Requests protegidos con reglas estrictas de fusión de ramas (Validado)[cite: 2].
* **EV-04 — Panel SonarCloud:** Métricas de calidad verificadas. Mantenibilidad: A, Confiabilidad: A, Bugs: 0, Vulnerabilidades: 2 falsos positivos resueltos (Evaluado)[cite: 2].
* **EV-05 — Scripts de k6 y Grafana:** Pruebas de estrés. Soporte verificado para 100 usuarios virtuales concurrentes con tiempos P95 óptimos (ej. 284ms en el módulo de Citas) (Validado)[cite: 2].
* **EV-07 — Grabación de Validación (25/06/2026):** Sesión de revisión final del sistema con el cliente y el docente revisor, almacenada de forma segura en Google Drive (Validado)[cite: 2].
* **EV-08 — Aplicación Web en Producción (Azure):** Sistema desplegado en la nube para demostración y pruebas de aceptación de usuario (UAT)[cite: 2]. Acceso disponible a través del enlace del entorno controlado[cite: 2]:
    * **URL Pública:** `https://salmon-bush-08c1e7510.7.azurestaticapps.net`[cite: 2]
    * **Credenciales de Validación:** Usuario: `admin` | Contraseña: `admin123`[cite: 2]

---

## 📝 Conclusiones y Acta Formal de Cierre de la Auditoría

El Ciclo de Vida del Desarrollo de Software de **SYS Clínica Santa Mónica** se declara **CONFORME** bajo la calificación de **ACEPTABLE CON OBSERVACIONES**, dictaminándose apto para operaciones clínicas integrales en entornos de producción en la nube[cite: 2].

### Acta Formal de Cierre de Auditoría SDLC
* **Código de Proyecto:** AUD-SDLC-SYS-CSM-2026-001[cite: 2]
* **Sistema Auditado:** SYS Clínica Santa Mónica[cite: 2]
* **Fecha de Cierre Oficial:** 02 de julio de 2026[cite: 2]

Por la presente, se certifica que la auditoría al Ciclo de Vida del Desarrollo de Software (SDLC) aplicada al equipo **UNIXCODE** ha concluido satisfactoriamente[cite: 2]. El equipo auditor verificó que todas las acciones prioritarias del Plan de Acción Correctiva han sido implementadas con éxito, destacándose la mejora sustancial en la cobertura de pruebas unitarias financieras, la corrección del análisis estático de código en SonarCloud y el estricto cumplimiento normativo y legal exigido por el sector salud peruano[cite: 2].

La validación final del sistema por parte del cliente y del docente revisor fue positiva, quedando respaldada por la grabación de la reunión virtual (EV-07), por la demostración en vivo del sistema desplegado en Azure (EV-08) y por las capturas de campo de la sesión de Google Meet que referenciaron el documento base de diseño `main.pdf`[cite: 2].

* **Auditor Líder:** *Ing. Ruben Roque Sucari*[cite: 2]
* **Representante de UNIXCODE:** *Kevin Raphael Paricahua Sanchez*[cite: 2]
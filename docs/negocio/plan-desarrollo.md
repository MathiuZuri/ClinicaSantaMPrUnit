# 📅 Plan de Desarrollo del Proyecto — SYS Clínica Santa Mónica

Este documento expone el marco operacional, metodológico y el cronograma de control que rigen el ciclo de vida del software de la institución en Juliaca. Su diseño asegura que la transición digital desde los formatos físicos y planillas dispersas hacia el sistema integrado se realice bajo estrictas salvaguardas de ingeniería, alineándose con los valores institucionales de **responsabilidad, ética profesional y calidad como estándar**.

---

## 🎯 Contexto Estratégico y Filosofía del Proyecto

El plan de desarrollo de **SYS Clínica Santa Mónica** no es un simple cronograma de tareas técnicas: es la hoja de ruta que materializa la **transformación digital** de la clínica, enmarcada en su misión de brindar atención materna integral, segura y digitalizada.

### Misión del Sistema
Proveer al personal de la Clínica Santa Mónica de una herramienta digital integral, segura y de alta disponibilidad que optimice el acto médico obstétrico y la recaudación de caja, erradicando la doble digitación y asegurando la inmutabilidad de los expedientes médicos.

### Visión de Futuro
Consolidarse como la base tecnológica de software médico de referencia en la región sur del Perú, con una arquitectura escalable capaz de mutar desde una especialización ginecobstétrica hacia el soporte de una clínica médica general.

### Filosofía que Guía el Desarrollo
Cada decisión técnica y cada funcionalidad implementada se evalúa bajo los siguientes principios:

- **Innovación con propósito:** La tecnología se adopta porque mejora tangiblemente la atención (reducción de tiempos de espera, seguimiento de pacientes, automatización de recordatorios).
- **Seguridad de la información médica como compromiso inquebrantable:** Los datos clínicos se protegen con encriptación, autenticación JWT y auditoría forense, cumpliendo la Ley N° 29733.
- **Transparencia administrativa:** La facturación electrónica y los reportes gerenciales se construyen con datos reales, no con estimaciones.
- **Mejora continua como cultura organizacional:** El sistema se ajusta permanentemente basado en indicadores de rendimiento y retroalimentación del personal.

---

## ⚙️ Metodología de Desarrollo Híbrida (CMMI + Scrum)

Para balancear la flexibilidad requerida ante la adaptación de procesos asistenciales y el rigor de calidad demandado por las normativas de **SUSALUD** y **MINSA**, el proyecto adopta un marco metodológico híbrido:

* **Scrum (Agilidad Operativa):** Gobierna el desarrollo diario a través de iteraciones cortas (Sprints de 2 semanas), reuniones diarias de alineamiento (*Daily Standups*) y entregas de software funcionales e incrementales. Esta agilidad permite adaptarse rápidamente a los cambios en los flujos de atención o en los requisitos normativos.
* **CMMI Nivel 3 (Rigor del Proceso):** Introduce la estandarización estricta de la documentación técnica, la gestión formal de requisitos lógicos, y procesos institucionalizados de verificación y validación cruzada antes de cada paso a producción. Esto asegura que cada componente (citas, historial, facturación) cumpla con los estándares de calidad exigidos por los entes reguladores.

---

## 👥 Organización del Equipo de Trabajo y Responsabilidades

El equipo se divide en un esquema claro de roles para garantizar la trazabilidad de los entregables y evitar zonas oscuras de responsabilidad técnica. Todos los miembros interactúan directamente con el sistema y contribuyen a la experiencia integral que distingue a la clínica.

### 1. Equipo de Desarrollo y QA
* **Líder de Arquitectura y Full-Stack Dev:** Responsable del diseño del núcleo atómico en el Dominio, la optimización de consultas en la Fluent API de EF Core y el consumo seguro del API de WhatsApp. Supervisa la integridad de la arquitectura hexagonal y la escalabilidad del sistema.
* **Ingeniero de Aseguramiento de la Calidad (QA):** Encargado del diseño y ejecución de la suite de pruebas unitarias (xUnit), simulacros de interfaz (NSubstitute) y la parametrización de las pruebas de estrés en k6. Garantiza que cada nueva funcionalidad supere los umbrales de cobertura (>80%) antes de ser desplegada.
* **Analista de Datos y Migración:** Rol encargado de velar por la integridad referencial durante la migración activa de las más de **5,000 historias clínicas históricas** desde papel y Excel hacia PostgreSQL. Diseña los scripts de validación y los puntos de control de calidad para cada lote migrado.

### 2. Roles de Gestión y Stakeholders
* **Product Owner (Administración de la Clínica):** Representa las necesidades del negocio, valida las pantallas de recaudación de caja, prioriza el *Product Backlog* y asegura la conformidad ante la SUNAT. Es el nexo entre el equipo técnico y la dirección estratégica.
* **Scrum Master:** Encargado de remover impedimentos logísticos y técnicos (como intermitencias de conectividad en Juliaca) y asegurar el flujo ágil del equipo. Facilita las ceremonias Scrum y protege al equipo de distracciones externas.
* **Personal de Recepción, Médicos y Obstetras:** Participan en las sesiones de validación de prototipos y en las pruebas de aceptación, proporcionando retroalimentación directa sobre la usabilidad y la adecuación a los flujos reales de trabajo.

---

## 🗓️ Planificación por Sprints e Hitos de Control

El desarrollo se organiza en Sprints de 2 semanas, con entregables funcionales al final de cada iteración. El cronograma considera tanto las prioridades del negocio como la disponibilidad de recursos.

### Cronograma General de Entregables

- [x] **Sprint 1: Núcleo y Arquitectura Base (Semanas 1-2)**
    - Configuración del ecosistema multinube (Azure + Neon PostgreSQL).
    - Inicialización de la solución bajo Arquitectura Hexagonal y compilación del Dominio.
    - Definición de las entidades atómicas (Paciente, Atencion, Pago, Comprobante) y sus relaciones.
    - Configuración inicial del `ApplicationDbContext` y las migraciones de EF Core.

- [x] **Sprint 2: Seguridad y Autenticación RBAC (Semanas 3-4)**
    - Implementación de controladores de cuentas y filtros de tokens perimetrales JWT.
    - Inyección del interceptor asíncrono de auditoría forense para cambios de datos.
    - Definición de la matriz de permisos (PACIENTE_VER, ATENCION_REGISTRAR, etc.) y su integración con las políticas de autorización.
    - Configuración de CORS para permitir el acceso desde el frontend Blazor WASM.

- [x] **Sprint 3: Módulos Clínicos Obstétricos (Semanas 5-6)**
    - Maquetación responsiva en MudBlazor del Stepper clínico (Anamnesis, Examen Físico, Ecografía, Tacto Vaginal, Impresión Diagnóstica).
    - Programación de las reglas matemáticas para el cálculo de la Edad Gestacional y FPP.
    - Implementación de la lógica de validación de la Fórmula Obstétrica (Gestaciones, Partos, Abortos, etc.).
    - Desarrollo del módulo de historial clínico electrónico con acceso rápido desde la consulta.

- [ ] **Sprint 4: Recaudación, Caja y SUNAT (Semanas 7-8)**
    - Construcción de pantallas de abonos, saldos pendientes y notas de ajuste financiero.
    - Configuración del motor QuestPDF para renderizado e inmutabilidad de boletas en formato JSONB dentro de PostgreSQL.
    - Integración del módulo de facturación electrónica con los requisitos de SUNAT.
    - Desarrollo del módulo de farmacia integrada (inventario, dispensación, facturación).

- [ ] **Sprint 5: Automatización de Mensajería y Cierre (Semanas 9-10)**
    - Sincronización de webhooks y sockets con Evolution API para recordatorios de citas por WhatsApp.
    - Ejecución masiva del plan de migración de las 5,000 carpetas médicas físicas.
    - Implementación del módulo de reportes gerenciales (ingresos, ocupación de consultorios, tasa de reconsulta).
    - Realización de pruebas de integración de extremo a extremo con Testcontainers y validación del pipeline CI/CD.

### Hitos Críticos del Proyecto

1. **M01 - Conformidad de Arquitectura Base:** Aprobación del aislamiento del Dominio y paso limpio por el Quality Gate local (cobertura >80%, sin bugs críticos).
2. **M02 - Certificación de Seguridad:** Cero vulnerabilidades críticas en el escaneo perimetral del middleware de autenticación (JWT, control de accesos, encriptación).
3. **M03 - Digitalización Completa:** Ingesta del 100% de los registros históricos validados en las tablas relacionales de PostgreSQL, con verificación de integridad referencial.
4. **M04 - Despliegue en Producción:** Sistema operativo en Azure con todos los módulos funcionales y los recordatorios automáticos activos.

---

## 📊 Representación del Cronograma (Gantt Analítico)

| Actividad / Hito | S1-S2 | S3-S4 | S5-S6 | S7-S8 | S9-S10 | Estado |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| Configuración de Infraestructura Cloud | █████ |       |       |       |       | Completado |
| Implementación RBAC y JWT |       | █████ |       |       |       | Completado |
| Desarrollo de Módulos Obstétricos |       |       | █████ |       |       | En Progreso |
| Integración Contable y QuestPDF |       |       |       | █████ |       | Planificado |
| Sincronización de WhatsApp API y Migración |       |       |       |       | █████ | Planificado |
| Pruebas de Integración y QA Final |       |       |       |       | █████ | Planificado |

---

## ⚡ Matriz de Riesgos Tecnológicos y Operativos

La gestión de riesgos es un componente crítico del plan de desarrollo, especialmente en un entorno con dependencias externas y desafíos de infraestructura local. A continuación se detallan los riesgos identificados, su impacto y las estrategias de mitigación.

| Código | Descripción del Riesgo | Impacto | Probabilidad | Estrategia de Mitigación en Ingeniería |
| :--- | :--- | :--- | :--- | :--- |
| **R-01** | Dependencia y cambios imprevistos en las políticas o tarifas de la API de WhatsApp de Meta. | **Alto** | Media | Mantener un desacoplamiento puro en la infraestructura. Tener listos disparadores automáticos alternativos hacia SMS o llamadas tradicionales. Monitorear trimestralmente la evolución de la API de Meta y mantener actualizado el plan de contingencia. |
| **R-02** | Cortes recurrentes de suministro eléctrico e inestabilidad de conectividad a Internet en Juliaca. | **Alto** | Alta | Aprovisionamiento de equipos UPS locales para apagados seguros de terminales. Configuración de caché temporal en memoria local dentro del cliente Blazor WASM para tolerar microcortes. Contratación de un segundo enlace de Internet de respaldo (fibra óptica + 4G/5G). |
| **R-03** | Errores de carga o pérdida de integridad durante la migración de las 5,000 historias clínicas físicas. | **Medio** | Alta | Establecer un pipeline de ingesta intermedio con scripts de validación de esquemas (Regex de DNI, nombres vacíos) antes de impactar PostgreSQL. Realizar migraciones por lotes con puntos de control de calidad y respaldos incrementales. |
| **R-04** | Curva de aprendizaje del personal en el uso del sistema digital, especialmente para quienes están habituados a procesos manuales. | **Medio** | Alta | Diseñar un programa de capacitación progresiva con materiales visuales y sesiones prácticas. Asignar un "embajador digital" por área para resolver dudas cotidianas. Realizar evaluaciones periódicas de adopción y ajustar el entrenamiento según los resultados. |
| **R-05** | Incremento de costos en servicios externos (VPS, Evolution API, energía eléctrica) que puedan presionar los márgenes operativos. | **Medio** | Media | Establecer un presupuesto anual de TI con un margen de contingencia del 20%. Negociar contratos a largo plazo con los proveedores de nube y buscar alternativas competitivas (DigitalOcean, Vultr) para reducir la dependencia. |
| **R-06** | Riesgos de ciberseguridad (ataques, fugas de datos, accesos no autorizados). | **Alto** | Media | Implementar autenticación JWT con rotación de claves, encriptación de datos sensibles, respaldos periódicos en ubicaciones separadas y auditoría continua de accesos. Realizar pruebas de penetración anuales. |

---

## 🔧 Consideraciones de Sostenibilidad y Costos

El plan de desarrollo reconoce que el proyecto atraviesa una **ventana de oportunidad** durante el primer año, donde el desarrollo se ejecuta sin costo de mano de obra (recursos internos). A partir del segundo año, la estructura de costos se modificará:

- **Desarrollo y soporte del sistema:** Cada modificación, nueva funcionalidad o ampliación (como las requeridas para la clínica general) tendrá un costo asociado. Se deberá establecer un presupuesto anual de evolución tecnológica basado en las prioridades estratégicas.
- **Marketing digital:** La inversión planificada en redes sociales y publicidad demandará un presupuesto mensual que actualmente no existe, pero que resulta indispensable para aumentar la visibilidad y la captación de pacientes.
- **Mantenimiento de equipos:** Con la expansión a clínica general, se sumarán nuevos equipos médicos y se ampliarán los contratos de mantenimiento.

Para garantizar la sostenibilidad, se recomienda:

- Asignar al menos el 10% del presupuesto operativo anual a la evolución y soporte del sistema.
- Priorizar funcionalidades con mayor retorno de inversión (automatización de seguimiento, reportes gerenciales, integración con laboratorios).
- Documentar exhaustivamente el código y los procesos para reducir la dependencia de un solo desarrollador.

---

## 📝 Notas sobre la Metodología y el Contexto Regional

- **Adaptación al contexto de Juliaca:** El plan considera las limitaciones de conectividad y la variabilidad del suministro eléctrico. Por ello, se prioriza el desarrollo de una aplicación web que pueda operar con conexiones intermitentes y se implementan estrategias de caché local.
- **Participación activa del personal:** Los médicos, obstetras y recepcionistas son consultados en cada sprint para validar la usabilidad y la adecuación a los flujos reales de trabajo. Esta retroalimentación directa acelera la adopción y reduce la resistencia al cambio.
- **Cumplimiento normativo:** Todos los entregables son revisados para garantizar el cumplimiento de las normas del MINSA, SUSALUD y la Ley de Protección de Datos Personales, asegurando que el sistema sea un activo confiable y legal para la clínica.

---
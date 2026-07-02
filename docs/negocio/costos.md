# 💰 Estructura Financiera y Sostenibilidad – SYS Clínica Santa Mónica

Esta página documenta la estructura de costos del sistema, la ventana de oportunidad actual (desarrollo sin costo de mano de obra durante el primer año) y la proyección de costos para la sostenibilidad a largo plazo, incluyendo la expansión hacia una clínica general.
---

## 💵 Composición de Costos Actuales

| Concepto | Descripción | Periodicidad | Impacto en la Operación |
|----------|-------------|--------------|--------------------------|
| **Hosting VPS** | Servidor virtual privado con Docker, Nginx, PostgreSQL | Mensual | **Crítico.** Sin VPS, el sistema no funciona. |
| **Dominio y SSL** | Dominio web y certificados HTTPS | Anual | Bajo. Necesario para seguridad e imagen profesional. |
| **Conectividad a Internet** | Banda ancha para recepción, consultorios y administración | Mensual | **Alto.** La operación es completamente dependiente de la conexión. Se recomienda un plan de respaldo (móvil o doble proveedor). |
| **Energía Eléctrica y UPS** | Consumo eléctrico y sistemas de respaldo | Mensual | Medio. El UPS es indispensable para apagados seguros. |
| **Mantenimiento de Equipos Médicos** | Ecógrafos, monitores fetales, camillas, etc. | Semestral/Anual | **Alto.** La disponibilidad de equipos determina la capacidad de atención. |
| **Insumos Médicos y de Farmacia** | Material descartable, medicamentos, reactivos | Mensual | **Alto.** Afecta la calidad de la atención y la rentabilidad de la farmacia. |
| **Personal Asistencial y Administrativo** | Médicos, obstetras, enfermeras, recepcionistas, administración | Mensual (planilla) | **Crítico.** Es el costo operativo más significativo y refleja la apuesta por la formalidad. |
| **Desarrollo de Software** | Construcción del sistema con ASP.NET Core, Blazor, PostgreSQL | **Sin costo de mano de obra durante el primer año** (recursos internos) | **Estratégico.** La gratuidad actual permite construir una base sólida sin presión financiera. |
| **Migración de Datos Históricos** | Digitalización de 5,000 registros desde Excel y papel | Concentrado en tiempo del personal y pasantes | Alto. Consume horas de trabajo pero no es un costo monetario directo elevado. |
| **Marketing Digital** | Publicaciones en Facebook, TikTok, publicidad paga | Actualmente casi nulo. Se planifica incorporarlo progresivamente. | Creciente. La ausencia de inversión limita la captación de nuevas pacientes. |
| **Licencias y Cumplimiento Normativo** | Autorizaciones MINSA, SUSALUD, SUNAT, colegiaturas | Anual / Según trámite | Medio. Obligatorios para la legalidad de la operación. |
| **Mobiliario y Adecuación Física** | Consultorios, sala de espera, recepción | Único (con mantenimiento menor) | Bajo una vez realizada la inversión inicial. |

---

## ⚠️ Ventana de Oportunidad: El Desarrollo "Gratuito" del Primer Año

Durante el **primer año**, el desarrollo y mantenimiento del sistema no representan un desembolso de mano de obra, ya que se realiza con recursos internos (el equipo de desarrollo no recibe remuneración adicional por esta tarea). Esto permite:

- **Construir el sistema sin presión financiera:** Se pueden dedicar ciclos de desarrollo a funcionalidades complejas (módulo de auditoría, integración con Evolution API, generación de PDF) sin preocuparse por el costo/hora.
- **Iterar rápidamente:** Los cambios y correcciones se implementan sin necesidad de aprobaciones presupuestales.
- **Capacitar al personal:** El equipo puede aprender y adaptarse al sistema sin la urgencia de justificar el retorno de inversión.

**Sin embargo, esta ventana es limitada.** A partir del **segundo año**, cada modificación, nueva funcionalidad o ampliación (como las requeridas para la clínica general) tendrá un costo asociado. Esto implica que se debe planificar un **presupuesto anual de evolución tecnológica** basado en las prioridades estratégicas.

---

## 📈 Proyección de Costos a Partir del Segundo Año

| Concepto | Situación Actual | Proyección (Año 2+) | Recomendación |
|----------|------------------|---------------------|---------------|
| **Desarrollo de Software** | Sin costo (recursos internos) | Costo por hora o proyecto (dependiendo de la complejidad) | Priorizar funcionalidades con mayor retorno: automatización de seguimiento, reportes gerenciales, integración con laboratorios. |
| **Soporte Técnico** | Interno | Posible necesidad de contratar soporte externo o dedicar más horas internas | Establecer un contrato de soporte con tiempos de respuesta garantizados. |
| **Marketing Digital** | Casi nulo | Presupuesto mensual para contenido y publicidad | Invertir progresivamente, midiendo el retorno en nuevos pacientes. |
| **Mantenimiento de Equipos** | Contratos puntuales | Ampliación de contratos al incorporar nuevos equipos (clínica general) | Negociar contratos anuales con proveedores locales. |
| **Capacitación** | Interna | Posible necesidad de formadores externos para nuevas especialidades | Documentar y estandarizar los procesos para reducir la dependencia de formadores externos. |

---

## 🚀 Expansión a Clínica General: Implicaciones Tecnológicas y Financieras

La clínica planea expandir sus servicios a **clínica general** en los próximos años. Esta expansión impactará tanto en la estructura de costos como en los requisitos del sistema:

### Nuevos Requisitos del Sistema

| Área | Requerimiento Nuevo | Impacto en el Desarrollo |
|------|---------------------|---------------------------|
| **Especialidades** | Medicina general, pediatría, etc. | Extender el modelo de entidades (ServicioClinico, Atencion) para soportar nuevas especialidades. |
| **Flujos de Ingresos** | Nuevos servicios y procedimientos | Adaptar el módulo de facturación y reportes financieros. |
| **Roles de Usuario** | Nuevos perfiles (pediatra, médico general) | Ampliar la matriz de permisos y políticas de autorización. |
| **Integraciones** | Laboratorios externos, seguros de salud | Desarrollar nuevos adaptadores de entrada/salida (API de terceros). |
| **Volumen de Datos** | Mayor número de pacientes y atenciones | Escalar la infraestructura (VPS) y optimizar las consultas SQL. |

### Costos Asociados a la Expansión

- **Desarrollo de nuevas funcionalidades:** Se estima que la expansión requerirá entre 3 y 6 meses de desarrollo (dependiendo de la complejidad de las integraciones). Esto tendrá un costo significativo a partir del segundo año.
- **Incremento de la infraestructura:** Mayor demanda de recursos (CPU, RAM, almacenamiento) en el VPS, lo que aumentará el costo mensual de hosting.
- **Capacitación del personal:** Nuevos médicos y especialistas necesitarán entrenamiento en el sistema.
- **Marketing y posicionamiento:** Campañas específicas para dar a conocer los nuevos servicios.

### Estrategia de Financiamiento

1. **Reinversión de utilidades:** La clínica destinará un porcentaje de los ingresos generados por los servicios actuales al desarrollo de la expansión.
2. **Presupuesto anual de TI:** Establecer una partida presupuestal dedicada a la evolución del sistema, revisada cada año.
3. **Priorización basada en ROI:** Las funcionalidades que generen mayor retorno (automatización de seguimiento, integración con laboratorios, reportes gerenciales) se desarrollarán primero.

---

## 📝 Notas sobre Sostenibilidad

- **La ventaja competitiva en costos:** El desarrollo del sistema no ha demandado el desembolso que representaría contratar un software comercial o una consultora externa. Esto permite ofrecer un servicio digitalizado, con farmacia integrada y facturación electrónica, a un costo operativo controlado.
- **Planificación temprana:** La clave para mantener esta ventaja será planificar con cuidado la transición al modelo de desarrollo con costo, priorizando aquellas funcionalidades que impacten directamente en la experiencia del paciente o en la eficiencia operativa.
- **Monitoreo continuo:** Los reportes gerenciales del sistema permitirán evaluar el retorno de inversión de cada nueva funcionalidad, ajustando el presupuesto según los resultados.

---

Este documento es la **guía de sostenibilidad financiera** del sistema. Ayuda a los responsables de la toma de decisiones a planificar el presupuesto de TI y a justificar las inversiones en nuevas funcionalidades.


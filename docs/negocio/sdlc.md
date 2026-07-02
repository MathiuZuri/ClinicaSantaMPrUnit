# ⚖️ Auditoría del Ciclo de Vida del Desarrollo (SDLC) — SYS Clínica Santa Mónica

Este componente documenta los hallazgos, metodologías de verificación y listas de comprobación aplicadas durante la auditoría formal de Gobierno de TI sobre el Ciclo de Vida del Desarrollo de Software (SDLC) de la plataforma informática **SYS Clínica Santa Mónica**. La auditoría se enmarca en el plan estratégico de modernización digital de la clínica, que busca garantizar la seguridad, confiabilidad y sostenibilidad del sistema en el contexto operativo de Juliaca (Región Puno).

---

## 📋 Información y Alcance General del Proyecto de Auditoría

* **Objetivo de Control:** Evaluar y certificar que cada una de las fases del ciclo de vida del software (Requerimientos, Diseño, Codificación, Testing, Despliegue y Mantenimiento) cumpla con los estándares internacionales de la ingeniería de software y los marcos normativos de la República del Perú, incluyendo las disposiciones del MINSA, SUSALUD, SUNAT y la Ley de Protección de Datos Personales (Ley N° 29733).
* **Periodo de Evaluación:** Enero 2026 - Junio 2026.
* **Límites del Alcance:** Comprende la totalidad de los repositorios de código fuente backend (.NET 9) y frontend (Blazor WASM) alojados en GitHub, los esquemas de persistencia en Neon.tech, los flujos operacionales del personal de la clínica en Juliaca y la integración con Evolution API para mensajería por WhatsApp.
* **Exclusiones Explícitas:** Se excluye de esta auditoría la evaluación física del hardware de los ecógrafos u otros dispositivos de diagnóstico médico, así como las pasarelas internas de los servidores de comunicación de Meta Corporation (la evaluación se limita al adaptador de integración, no a la infraestructura externa).

---

## 🎯 Justificación y Contexto Estratégico

La transición tecnológica desde un modelo vulnerable basado en registros físicos y hojas de cálculo (más de 5,000 historiales clínicos en proceso de migración) hacia el ecosistema digital de **SYS Clínica Santa Mónica** exige un control de gobernanza estricto. La auditoría se justifica ante la necesidad de:

1. **Blindar la plataforma contra fugas de información** de datos altamente sensibles de las pacientes (datos de filiación, diagnósticos, antecedentes obstétricos), protegiendo a la clínica de sanciones legales por parte de SUSALUD o la Autoridad Nacional de Protección de Datos Personales.
2. **Garantizar la continuidad operativa** ante la dependencia crítica de servicios externos (Meta/WhatsApp, VPS, conectividad a Internet), mitigando riesgos que podrían paralizar la atención en un entorno donde la informalidad del sector salud es prevalente.
3. **Validar la escalabilidad del sistema** para soportar la expansión planificada hacia una clínica general, asegurando que la arquitectura hexagonal pueda absorber nuevos servicios y especialidades sin comprometer la estabilidad del núcleo.

!!! info "Contexto Regional"
    Juliaca enfrenta desafíos específicos: alta tasa de natalidad, saturación del sistema público de salud, migración del campo a la ciudad y una creciente demanda de servicios privados formalizados. El sistema debe operar en un entorno con conectividad variable y un mercado competitivo donde la diferenciación digital es clave. La auditoría considera estos factores al evaluar la resiliencia y la propuesta de valor del software.

---

## 🧩 Fases del Ciclo de Vida del Desarrollo (SDLC) Evaluadas

La auditoría ha revisado cada fase del ciclo de vida, verificando la existencia de artefactos, la aplicación de buenas prácticas y la trazabilidad entre las etapas.

| Fase | Actividades Clave | Artefactos Evaluados | Cumplimiento |
|------|-------------------|----------------------|--------------|
| **Requerimientos** | Captura de necesidades funcionales y no funcionales, validación con el personal médico y administrativo. | Documento de especificación de requisitos, historias de usuario, casos de uso, matriz de trazabilidad. | **APROBADO** |
| **Diseño** | Definición de la arquitectura hexagonal, modelo de datos relacional, diseño de API RESTful y UI/UX. | Diagramas de clase, modelo entidad-relación, especificación OpenAPI (Swagger), prototipos de interfaz. | **APROBADO** |
| **Codificación** | Implementación siguiendo principios SOLID, patrones de diseño, pruebas unitarias y revisión de código. | Repositorio GitHub, commits, pull requests, análisis estático (SonarCloud), cobertura de código. | **APROBADO** |
| **Pruebas (QA)** | Ejecución de pruebas unitarias, de integración (con Testcontainers) y de carga (k6), con generación de reportes. | Reportes de cobertura, resultados de pruebas, informes de rendimiento. | **APROBADO** |
| **Despliegue** | Automatización mediante GitHub Actions, aprovisionamiento en Azure App Service y Static Web Apps, configuración de Neon.tech. | Pipelines de CI/CD, scripts de despliegue, variables de entorno, registros de despliegue. | **APROBADO** |
| **Mantenimiento** | Monitoreo de disponibilidad, aplicación de parches de seguridad, gestión de incidencias y soporte al usuario. | Bitácora de incidencias, planes de contingencia, acuerdos de nivel de servicio (SLA). | **ADVERTENCIA** (ver sección de sostenibilidad) |

---

## 📊 Lista de Verificación Completa del SDLC (Checklist de Cumplimiento)

A continuación se detalla la matriz de evaluación de calidad y cumplimiento técnico aplicada sobre los procesos del sistema:

| Dimensión de Control | Criterio de Evaluación e Ingeniería | Estado | Evidencia de Soporte y Mitigación | Riesgo Asociado |
| :--- | :--- | :--- | :--- | :--- |
| **Gobernanza de Seguridad** | ¿El sistema restringe los accesos lógicos según las funciones del colaborador? | **APROBADO** | Implementación estricta de Políticas de Autorización basadas en Permisos Atómicos (**Seguridad RBAC**) con tokens JWT de firma asimétrica en la API. Cada acción está protegida por políticas específicas (ej. `ATENCION_REGISTRAR`, `FINANZAS_VER`). | **Bajo** |
| **Protección de Datos Privados** | ¿Se cumple con el marco regulatorio establecido por la Ley N° 29733 en el Perú? | **APROBADO** | Los datos de filiación y diagnósticos clínicos sensibles viajan encriptados y cuentan con trazas de auditoría forense inalterables. El filtro `AuditoriaAutomaticaFilter` registra cada acceso, modificación y eliminación, almacenando valores anteriores y nuevos. | **Medio** |
| **Integridad Fiscal y Tributaria** | ¿Existe consistencia en los registros contables y de facturación electrónica ante SUNAT? | **APROBADO** | Uso de campos configurados de tipo `jsonb` nativos en PostgreSQL, capturando copias inmutables del balance al momento del cobro (snapshots de comprobantes). La emisión de comprobantes electrónicos está integrada en el flujo de cierre de atención. | **Medio** |
| **Gestión de Agenda Asistencial** | ¿El sistema impide la colisión o sobreposición de citas médicas presenciales en Juliaca? | **APROBADO** | Ejecución en el core del negocio del algoritmo de validación temporal indexada `ExisteInterferenciaHorarioAsync`, que evalúa intervalos horarios en la base de datos antes de confirmar una cita. | **Bajo** |
| **Resiliencia Operativa** | ¿Existe un plan de contingencia ante cortes eléctricos o caídas locales de Internet? | **ADVERTENCIA** | El sistema es 100% cloud dependiente (VPS, Neon.tech). Se exige la instalación obligatoria de routers de respaldo móvil 4G/5G y UPS físicos en recepción. Se recomienda tener un proveedor de hosting alternativo preconfigurado. | **Alto** |
| **Aseguramiento de Calidad** | ¿El código es inspeccionado de forma automática ante la inyección de bugs o código sucio? | **APROBADO** | Integración nativa de Quality Gates automáticos en GitHub Actions conectados a SonarCloud (cobertura >80%, sin bugs críticos) y escaneos de dependencias con Snyk (vulnerabilidades de alta severidad bloquean el despliegue). | **Bajo** |
| **Continuidad de la Migración** | ¿Los 5,000 registros históricos se están migrando de manera controlada? | **EN PROCESO** | La migración de historiales desde Excel y papel se realiza bajo supervisión, con validación de datos y registro de auditoría. El proceso está documentado y se estima su finalización en el tercer trimestre de 2026. | **Medio** |
| **Independencia de Proveedores** | ¿Existen alternativas viables para los servicios críticos (WhatsApp, VPS)? | **ADVERTENCIA** | La dependencia de Meta (WhatsApp) es alta. Se han identificado canales alternativos (SMS, correo, llamadas) y se recomienda mantenerlos operativos como plan de contingencia. | **Alto** |

---

## 📈 Hallazgos y Recomendaciones de Mejora

| Hallazgo | Nivel de Riesgo | Recomendación | Plazo |
|----------|----------------|---------------|-------|
| Migración de 5,000 registros históricos en curso | **Medio** | Acelerar el proceso de migración con validación cruzada por lotes, y establecer un punto de control de calidad al finalizar cada lote. | 3 meses |
| Dependencia de conectividad a Internet | **Alto** | Instalar un segundo enlace de Internet de respaldo (fibra óptica + 4G/5G) y un UPS con autonomía de al menos 4 horas. | Inmediato |
| Baja presencia en redes sociales | **Medio** | Diseñar y ejecutar un plan de marketing digital para comunicar los diferenciadores tecnológicos y atraer nuevos pacientes. | 6 meses |
| Falta de presupuesto formal para el desarrollo a partir del segundo año | **Alto** | Incluir en el presupuesto anual una partida específica para mantenimiento y evolución del sistema, basada en un análisis de costo/beneficio de las funcionalidades prioritarias. | Antes del inicio del segundo año |
| No se han definido SLAs formales para el soporte técnico | **Medio** | Establecer acuerdos de nivel de servicio con tiempos de respuesta y resolución, internamente o con un proveedor externo. | 3 meses |

---

## 🔔 Advertencias de Seguridad y Factores Críticos de Éxito

!!! success "Factor Crítico de Éxito: Inmutabilidad Posterior al Cierre"
    Un hito de auditoría aprobado con honores es el comportamiento de bloqueo de la entidad `Atencion`. Una vez que el médico obstetra define la impresión diagnóstica y cierra el acto médico, la capa de API revoca de manera permanente los permisos de edición sobre ese registro. Cualquier intento posterior de alteración de datos clínicos levantará una alerta de seguridad severa, garantizando la fidelidad legal de la historia clínica electrónica ante fiscalizaciones sanitarias.

!!! warning "Advertencia de Mantenimiento Presupuestario: Sostenibilidad del Software"
    La auditoría alerta formalmente a la Gerencia General de la clínica que, si bien el desarrollo del primer año se ejecuta bajo un modelo sin costo de mano de obra por recursos de investigación internos, la plataforma entrará en fase de mantenimiento en el segundo año. Se debe presupuestar de forma obligatoria un fondo económico anual para horas de ingeniería especializada; de lo contrario, cambios imprevistos en las APIs externas de Meta (WhatsApp) o en los esquemas de la SUNAT dejarían el sistema inoperativo. Se recomienda asignar al menos el 10% del presupuesto operativo anual a la evolución y soporte del sistema.

!!! danger "Riesgo Crítico: Dependencia de Meta (WhatsApp)"
    El sistema utiliza WhatsApp como canal principal de comunicación con las pacientes (recordatorios de citas, seguimiento posparto). Cualquier cambio en las políticas de Meta, tarifas o restricciones técnicas puede afectar gravemente la operación. La auditoría exige que se mantenga actualizado un plan de contingencia con canales alternativos (SMS, correo electrónico, llamadas telefónicas) y que se monitoree trimestralmente la evolución de la API de Meta.

---

## 🛠️ Herramientas de Desarrollo y Calidad Auditadas

La auditoría ha verificado el uso y la correcta configuración de las siguientes herramientas que garantizan la calidad y seguridad del software:

| Herramienta | Propósito | Configuración Auditada | Resultado |
|-------------|-----------|------------------------|-----------|
| **GitHub Actions** | CI/CD automatizado | Pipelines configurados para ejecutar en cada push a `master`, con stages de build, test, análisis SonarCloud y despliegue a Azure. | **APROBADO** |
| **SonarCloud** | Análisis estático de calidad | Umbral de cobertura >80%, sin bugs críticos, sin vulnerabilidades de alta severidad. Integrado con GitHub Actions. | **APROBADO** |
| **Snyk** | Escaneo de vulnerabilidades en dependencias | Ejecución automática en el pipeline, bloqueando el despliegue si se detectan vulnerabilidades de alta severidad. | **APROBADO** |
| **xUnit / NSubstitute** | Pruebas unitarias y mocks | Proyectos de pruebas dedicados con cobertura superior al 85% en los módulos críticos (citas, atenciones, pagos). | **APROBADO** |
| **Testcontainers** | Pruebas de integración | Configuración de contenedores PostgreSQL efímeros para cada suite de pruebas, garantizando aislamiento y reproducibilidad. | **APROBADO** |
| **k6** | Pruebas de carga y rendimiento | Scripts configurados para simular múltiples usuarios concurrentes, validando que el sistema soporte picos de demanda en horarios de alta afluencia. | **APROBADO** |

---

## 📝 Conclusiones y Firmas de Aprobación de la Auditoría

El Ciclo de Vida del Desarrollo de Software de **SYS Clínica Santa Mónica** se declara **CONFORME y APTO** para operaciones clínicas integrales en entornos de producción en la nube. El sistema demuestra un apego riguroso a las normas internacionales de la ingeniería y los marcos tributarios peruanos, constituyendo un activo informático seguro, escalable y de alta fidelidad.

La auditoría resalta la solidez de la arquitectura hexagonal, la implementación de pruebas automatizadas, la seguridad perimetral basada en JWT y la integración con herramientas de calidad (SonarCloud, Snyk). Sin embargo, advierte sobre la necesidad de abordar de manera prioritaria la dependencia de servicios externos (WhatsApp, conectividad) y la planificación presupuestaria para garantizar la sostenibilidad a largo plazo.

* **Responsable de TI y Seguridad de la Información:** *EPIS - Universidad Peruana Unión*
* **Representante de Control de Calidad y QA:** *Comité de Ingeniería Administrativa 2026*
* **Fecha de Aprobación:** Junio 2026

---
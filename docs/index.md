# 🏥 SYS Clínica Santa Mónica - Portal de Documentación Técnica

Bienvenido al centro neurálgico de documentación de **SYS Clínica Santa Mónica**[cite: 1]. Este portal condensa las especificaciones de ingeniería de software, decisiones de diseño de sistemas, diagramas de flujo transaccional y manuales operativos de la plataforma informática **SIGEC** (Sistema Integral de Gestión de Clínica)[cite: 1].

El software está concebido como una solución empresarial de uso estrictamente interno para la automatización integral de la institución[cite: 1]. No actúa como un portal de autogestión para pacientes, sino como la herramienta operativa unificada que orquesta las actividades del personal de Recepción, Caja, Finanzas, Farmacia, Obstetras y Médicos Especialistas[cite: 1].

!!! info "Paradigma de Diseño Estructural y Desacoplamiento"
    Toda la solución backend está desarrollada bajo los principios de **Diseño Guiado por el Dominio (DDD)** y acoplada de manera estricta mediante una **Arquitectura Hexagonal (Puertos y Adaptadores)** en .NET 9[cite: 1]. Este diseño garantiza el aislamiento absoluto de las reglas de negocio ginecobstétricas y de recaudación financiera frente a agentes externos, automatizando de forma robusta los flujos de trabajo e interrumpiendo la dependencia tradicional hacia registros manuales en papel o planillas de cálculo dispersas[cite: 1].

---

## 🎯 Contexto Estratégico e Institucional

El desarrollo de **SYS Clínica Santa Mónica** responde a un plan estratégico de modernización tecnológica diseñado para afrontar los desafíos operativos e infraestructurales específicos de la ciudad de Juliaca (Región Puno)[cite: 1]. La plataforma unifica los objetivos corporativos de la institución con los requisitos técnicos exigidos por el marco legal peruano[cite: 1].

=== "Misión del Sistema"
    Proveer al personal de la Clínica Santa Mónica de una herramienta digital integral, segura y de alta disponibilidad que optimice el acto médico obstétrico y la recaudación de caja[cite: 1]. El sistema erradica la doble digitación de datos, agiliza la atención al paciente y asegura la inmutabilidad de los expedientes médicos en el Altiplano peruano[cite: 1].

=== "Visión de Futuro"
    Consolidarse como la base tecnológica de software médico de referencia en la región sur del Perú, con una arquitectura escalable capaz de mutar de manera eficiente desde una especialización ginecobstétrica hacia el soporte de una clínica médica general de gran envergadura[cite: 1].

=== "Cumplimiento Regulatorio y Legal"
    El sistema incorpora en su diseño e infraestructura perimetral los lineamientos de las entidades fiscalizadoras del Perú:
    
    * **MINSA y SUSALUD:** Estructuración normalizada del expediente clínico y el partograma digital, resguardando la confidencialidad de la información médica[cite: 1].
    * **Ley N° 29733 (Protección de Datos Personales):** Cifrado de credenciales mediante algoritmos asimétricos y aislamiento de accesos basado en roles lógicos y permisos atómicos[cite: 1].
    * **SUNAT:** Persistencia inmutable de snapshots financieros en formato nativo JSONB dentro de la base de datos para la consistencia de auditorías fiscales y micro-emisión de comprobantes electrónicos[cite: 1].

---

## 📐 Matriz Resumen de la Arquitectura Hexagonal

La solución está segmentada de forma física en proyectos desacoplados que interactúan a través de abstracciones y contratos lógicos bien definidos:

| Capa Funcional | Responsabilidad Técnica en el Sistema | Tecnologías Clave Implementadas |
| :--- | :--- | :--- |
| **🟣 Dominio (Core)** | Corazón inmutable del negocio. Contiene las entidades puras, enumeraciones de estado y la declaración de los puertos de comunicación[cite: 1]. | .NET 9, DataAnnotations, LINQ Expressions[cite: 1] |
| **🟦 Aplicación** | Orquestador de los casos de uso. Gestiona el flujo de comandos, validaciones perimetrales y transformación de objetos mediante DTOs[cite: 1]. | .NET 9, Interfaces de Servicios, Patrón Mediator[cite: 1] |
| **⚙️ Infraestructura** | Adaptador de salida técnico. Implementa la persistencia de datos relacionales, el sembrado maestro y los motores de impresión[cite: 1]. | EF Core 9, PostgreSQL (Neon.tech), QuestPDF, BCrypt[cite: 1] |
| **🚀 API (Presentación)**| Adaptador de entrada perimetral. Expone los endpoints REST, gestiona la seguridad JWT y las conexiones por sockets[cite: 1]. | ASP.NET Core 9, JWT Bearer, Swagger OpenAPI, SignalR[cite: 1] |
| **🌐 Frontend (WASM)** | Interfaz de usuario SPA rica e interactiva. Renderiza componentes con la línea de diseño *Luxury Medical Style*[cite: 1]. | Blazor WebAssembly, MudBlazor Component Suite[cite: 1] |
| **🧪 Pruebas (QA)** | Suite de aseguramiento de la calidad. Valida de forma unitaria e integrada la robustez del código y los pipelines HTTP[cite: 1]. | xUnit, NSubstitute, FluentAssertions, Testcontainers[cite: 1] |

---

## 🗺️ Mapa Completo de la Documentación por Capas

Utilice los siguientes accesos directos para navegar de manera estructural a través de las especificaciones y manuales técnicos que componen el ecosistema de software:

### 🟣 Núcleo del Negocio y Arquitectura Base
* **[Visión General de la Arquitectura](arquitectura/general.md):** Mapa conceptual detallado sobre la estructura en capas de cebolla, fronteras de aislamiento del sistema, inversión de control (IoC) y flujo de dependencias lógicas de la solución[cite: 1].

### 📦 Componentes del Ecosistema de Software
* **[Capa de Dominio (Core)](arquitectura/dominio.md):** El corazón inmutable de la clínica. Contiene el diseño técnico de las entidades atómicas (`Paciente`, `Atencion`, `Pago`), la lógica del módulo de filiación obstétrica avanzada y el catálogo maestro de enumeraciones de estado[cite: 1].
* **[Capa de Infraestructura (Persistencia)](arquitectura/infraestructura.md):** El adaptador de salida tecnológico. Implementación detallada del `ApplicationDbContext`, estrategias de carga explícita (*Eager Loading*) optimizadas para Neon.tech, motores de renderizado PDF en memoria y el motor de datos `DataSeeder`[cite: 1].
* **[Capa de API (Presentación)](arquitectura/api.md):** El puerto de entrada perimetral del sistema. Documentación técnica de los controladores REST expuestos, middlewares globales para el manejo de excepciones, interceptores de auditoría forense y la pasarela de integración con WhatsApp[cite: 1].
* **[Capa de Frontend (Blazor WASM)](arquitectura/wasm.md):** Guía de desarrollo de la interfaz de usuario SPA. Explica la arquitectura de los componentes de MudBlazor, el sistema de layouts, el control de vistas basado en permisos (RBAC) y los interceptores de cabeceras HTTP[cite: 1].
* **[Capa de Pruebas (Unitarias e Integración)](arquitectura/pruebas.md):** La estrategia de Aseguramiento de la Calidad (QA). Detalla el diseño de pruebas unitarias con dobles de prueba y el despliegue automático de contenedores Docker efímeros para pruebas de integración de extremo a extremo[cite: 1].

### 🏢 Modelo de Negocio y Operaciones
* **[Análisis Estratégico](negocio/estrategia.md):** Documentación de las matrices de ingeniería administrativa que sustentan el software: Propuesta de Valor, PESTEL, 5 Fuerzas de Porter, FODA, Ansoff y las estrategias resultantes para el negocio[cite: 1].
* **[Estructura y Áreas](negocio/organizacion.md):** Descripción de las 5 áreas funcionales de la clínica (Médica, Administrativa, TI, Recepción, Financiera/Farmacia), los roles de los personajes clave y el organigrama institucional que interactúa con el sistema[cite: 1].
* **[Estructura Financiera y Sostenibilidad](negocio/costos.md):** Análisis detallado de la composición de costos (Hosting VPS, conectividad), la ventana de oportunidad del primer año (desarrollo sin costo de mano de obra) y la proyección de gastos para la expansión futura hacia una clínica general[cite: 1].

### 📅 Gestión de Proyectos, Control de Calidad y Gobernanza (PMO & QA)
* **[Plan de Desarrollo del Proyecto](negocio/plan-desarrollo.md):** El marco metodológico híbrido (CMMI + Scrum), organización de los ingenieros por Sprints, cronograma general, hitos de control del ciclo de vida y matriz de mitigación de riesgos operativos[cite: 1].
* **[Evaluación de Calidad y Métricas](negocio/metricas.md):** Indicadores analíticos de mantenibilidad e inmunidad bajo la norma ISO/IEC 25010 (SonarCloud), reporte de cobertura por Coverlet y los resultados de las pruebas de estrés en alta concurrencia por módulos ejecutadas con k6[cite: 1].
* **[Auditoría del Ciclo de Vida (SDLC)](negocio/sdlc.md):** Evaluación formal de gobierno de TI sobre las fases de desarrollo según exigencias regulatorias, presupuesto de control, factores críticos de éxito y la lista de verificación de cumplimiento normativo del SDLC[cite: 1].

### 🚀 Despliegue y Operaciones DevOps
* **[Despliegue en la Nube (Azure & Neon)](arquitectura/despliegue.md):** Manual técnico del pipeline automatizado en GitHub Actions. Configuración del build perimetral, integración continua con análisis estático de código en SonarCloud, escaneo de vulnerabilidades con Snyk y aprovisionamiento en Azure y Neon.tech[cite: 1].

---

## 🔍 Extractos Ejecutivos de los Componentes Principales

### 📅 Gestión Operacional del Proyecto (Capítulo 5)
El desarrollo del sistema se ejecuta bajo un marco híbrido que combina la adaptabilidad del marco ágil **Scrum** con la rigurosidad y documentación institucional exigida por **CMMI Nivel 3**[cite: 1]. Las iteraciones se organizan en Sprints estructurados de dos semanas con hitos claros de control que gobiernan la migración de las historias clínicas y resguardan la entrega continua frente a los riesgos latentes de conectividad de la zona[cite: 1].

### 📊 Control de Calidad e Ingeniería de Estrés (Capítulo 6)
El control estático de calidad en SonarCloud certifica una calificación de mantenibilidad **A** bajo el estándar **ISO/IEC 25010**, respaldada por una cobertura de pruebas automatizadas superior al **84%**[cite: 1]. Asimismo, la infraestructura del backend ha sido sometida a pruebas de carga extrema mediante **k6 (Grafana)** por cada módulo funcional, garantizando respuestas óptimas en percentiles críticos (p95 < 45ms) ante picos de concurrencia en salas de control y ventanillas de recaudación[cite: 1].

### ⚖️ Auditoría de Ciclo de Vida y Cumplimiento SDLC (Capítulo 7)
El proceso de auditoría formal de gobierno de TI examina el apego a las directivas nacionales y de salud (MINSA, SUSALUD, Ley N° 29733)[cite: 1]. A través de una lista de verificación detallada del SDLC, se garantiza que los flujos críticos (como el bloqueo por inmutabilidad post-cierre del acto médico y la encriptación transaccional) operen de forma idónea, alertando preventivamente sobre las necesidades presupuestarias de soporte a partir del segundo año[cite: 1].

---
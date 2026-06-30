# 🏥 Sistema de Gestión de Clínica - SIGEC

Bienvenido a la consola central de documentación técnica de la intranet de la **Clínica Santa Mónica**. Este portal condensa las especificaciones de diseño de software, diagramas de flujo de datos y manuales operativos del sistema de información ginecobstétrico.

!!! info "Paradigma de Diseño Estructural"
    Toda la solución backend está desarrollada bajo los principios de **Diseño Guiado por el Dominio (DDD)** y acoplada estrictamente mediante una **Arquitectura Hexagonal (Puertos y Adaptadores)** en .NET 9, garantizando el aislamiento total de las reglas de negocio frente a agentes externos.

---

## 📐 Resumen de la Arquitectura

El sistema se compone de cuatro capas principales, cada una con una responsabilidad clara dentro del hexágono:

| Capa | Responsabilidad | Tecnologías Clave |
|------|-----------------|-------------------|
| **Dominio** | Reglas de negocio, entidades y agregados puros | .NET 9, System.ComponentModel.DataAnnotations |
| **Aplicación** | Orquestación de casos de uso, puertos y DTOs | .NET 9, interfaces de servicios |
| **Infraestructura** | Adaptadores de persistencia y servicios externos | EF Core 9, PostgreSQL, QuestPDF, BCrypt |
| **Presentación (API)** | Exposición RESTful, autenticación y autorización | ASP.NET Core 9, JWT, Swagger, SignalR |
| **Frontend (WASM)** | Interfaz de usuario SPA | Blazor WebAssembly, MudBlazor |
| **Pruebas** | Unitarias e integración | xUnit, NSubstitute, Testcontainers |

Cada capa está completamente desacoplada, cumpliendo con los principios de la Arquitectura Hexagonal: el dominio no conoce la infraestructura, y la API solo orquesta, no implementa lógica de negocio.

---

## 🗺️ Mapa de la Documentación por Capas

Utiliza los siguientes accesos directos para navegar de manera estructural a través de cada uno de los hexágonos y componentes del ecosistema de software:

### 🟣 Núcleo y Arquitectura Base

* **[Visión General de la Arquitectura](arquitectura/general.md):** Mapa conceptual de la estructura en capas de cebolla, fronteras de aislamiento del sistema y flujo de dependencias de la solución en C#.

### 📦 Componentes de la Arquitectura Hexagonal

* **[Capa de Dominio (Core)](arquitectura/dominio.md):** El corazón inmutable de la clínica. Contiene el modelo de entidades atómicas (`Paciente`, `Atencion`, `Pago`), la lógica de filiación de pacientes y las reglas de negocio puras independientes de frameworks.

* **[Capa de Infraestructura (Persistencia)](arquitectura/infraestructura.md):** El adaptador de salida tecnológico. Implementación de repositorios de datos, configuración del mapeador tridimensional (EF Core 9) y la conexión segura a la nube de PostgreSQL.

* **[Capa de API (Presentación)](arquitectura/api.md):** El puerto de entrada del sistema. Documentación de los controladores expuestos, contratos de respuesta JSON, mecanismos de seguridad mediante tokens de autenticación JWT y configuraciones globales.

* **[Capa de Frontend (Blazor WASM)](arquitectura/wasm.md):** La interfaz de usuario SPA construida con Blazor WebAssembly, que consume la API y proporciona una experiencia de usuario rica y receptiva con el diseño "Luxury Medical Style".

* **[Capa de Pruebas](arquitectura/pruebas.md):** Estrategia de testing con pruebas unitarias (xUnit + NSubstitute) y de integración (Testcontainers + PostgreSQL), garantizando la calidad y robustez del sistema.

### 🚀 Despliegue y Mantenimiento DevOps

* **[Despliegue en la Nube (Azure & Neon)](arquitectura/despliegue.md):** Guía paso a paso para la puesta en producción del API en Azure App Service, la reescritura de rutas seguras contra errores 404 en el frontend y el aprovisionamiento de bases de datos serverless en Neon.tech.

---

## 🔍 Extractos Clave de las Capas Principales

### 🟦 Capa de API (Presentación)

La API RESTful sobre **ASP.NET Core 9** expone más de 15 controladores organizados por módulos: Autenticación, Pacientes, Doctores, Citas, Atenciones, Pagos, Comprobantes, Finanzas, Historial, Auditoría y WhatsApp. Todos los endpoints están protegidos con **JWT** y autorización basada en políticas de permisos. El sistema cuenta con un filtro automático de auditoría que registra cada operación, un middleware de manejo de excepciones y una integración en tiempo real con **SignalR** para el módulo de chat.

### 🟣 Capa de Dominio (Core)

El dominio contiene más de 20 entidades de negocio, incluyendo los agregados clínicos de la atención obstétrica (Anamnesis, Examen Físico, Tacto Vaginal, Ecografía Obstétrica e Impresión Diagnóstica). Las enumeraciones definen máquinas de estado para cada proceso, y los **puertos** (interfaces de repositorios y servicios) aseguran el aislamiento tecnológico. La entidad `Comprobante` almacena un snapshot JSON inmutable, garantizando la trazabilidad fiscal.

### ⚙️ Capa de Infraestructura

El `ApplicationDbContext` mapea todas las entidades a tablas PostgreSQL mediante configuraciones `IEntityTypeConfiguration`. Los repositorios implementan las interfaces del dominio y utilizan **EF Core 9** con estrategias de `Include` y `ThenInclude` para carga eficiente de relaciones. Se generan documentos PDF con **QuestPDF** (boletas, constancias, resúmenes, certificados, reportes financieros y resúmenes de parto). El `DataSeeder` inicializa la base de datos con roles, permisos y datos de prueba.

### 🧪 Capa de Pruebas

Se utilizan **xUnit** y **NSubstitute** para pruebas unitarias de controladores, servicios y entidades. Las pruebas de integración emplean **Testcontainers** para levantar una base de datos PostgreSQL real en un contenedor Docker, validando el pipeline HTTP completo. La cobertura de código se mide con **coverlet** y se integra con **SonarCloud**, manteniendo un umbral mínimo del 80%.

---

## ⚡ Comandos Rápidos del Entorno de Documentación

Si necesitas realizar modificaciones o agregar nuevos manuales Markdown a la carpeta `docs/`, recuerda utilizar estas instrucciones nativas desde tu consola de comandos local:

* `python -m mkdocs serve` — Levanta el servidor local con soporte *Live-Reload* en `http://127.0.0.1:8000/`.
* `python -m mkdocs build` — Compila y procesa todos los archivos estáticos listos para subirse a GitHub Pages.

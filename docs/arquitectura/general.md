# 🗺️ Visión General de la Arquitectura – SYS Clínica Santa Mónica

El sistema **SYS Clínica Santa Mónica** (anteriormente SIGEC) es la plataforma digital que materializa la transformación tecnológica de la **Clínica Santa Mónica** en Juliaca, una institución privada especializada en atención materna integral. Diseñado e implementado bajo los fundamentos de la **Arquitectura Hexagonal** (también conocida como patrón de *Puertos y Adaptadores*) y los principios del **Diseño Guiado por el Dominio (DDD)**, este sistema no es un simple software de gestión: es el reflejo de la **filosofía de innovación con propósito** que distingue a la clínica en el mercado de salud de la región Puno.

El objetivo principal de esta arquitectura es la **separación estricta de responsabilidades**, aislando por completo la lógica de negocio pura (ginecobstétrica, facturación, seguimiento de pacientes) de cualquier acoplamiento con frameworks, bases de datos, clientes web o servicios externos. Este diseño garantiza que la clínica pueda **evolucionar tecnológicamente sin comprometer sus reglas de negocio**, alineándose con sus valores de **responsabilidad, ética profesional y mejora continua**.

!!! info "Contexto Estratégico"
    La Clínica Santa Mónica apuesta por un modelo de atención materna integral, digitalizado y seguro. Su sistema propio reemplaza los registros en papel y las planillas Excel dispersas, integrando en una sola plataforma la gestión de pacientes, la agenda de citas, la historia clínica electrónica, la facturación conforme a SUNAT, los recordatorios automáticos por WhatsApp y la auditoría de cada acción. Esta transformación digital no es un fin en sí mismo, sino un medio para ofrecer **acompañamiento real** a las pacientes en cada etapa de su vida reproductiva, cumpliendo con los más altos estándares de protección de datos (Ley N° 29733) y normativas del MINSA y SUSALUD.

---

## 🧅 La Regla de Dependencia Dependiente (Dependency Inversion)

La arquitectura se visualiza como una serie de capas concéntricas (estilo cebolla) donde **las dependencias fluyen estrictamente hacia adentro**. Las capas externas conocen a las internas, pero las internas jamás tienen conocimiento, referencia directa o dependencia de las capas que las rodean. Este principio, conocido como **Inversión de Dependencias**, es el pilar que sostiene la Arquitectura Hexagonal y permite que el dominio sea **inmutable, testeable y evolucionable**.
```text
  ┌─────────────────────────────────────────────────────────────┐
  │                   ADAPTADORES DE ENTRADA                    │
  │          [Clinica.WASM] ➔ [Clinica.API (Controllers)]      │
  │      (Peticiones HTTP, SignalR, Webhooks de WhatsApp)      │
  └──────────────────────────────┬──────────────────────────────┘
                                 │ (Invocación)
                                 ▼
                    ┌──────────────────────────┐
                    │     PUERTOS DE ENTRADA   │
                    │   (Interfaces de Servicios de Aplicación)
                    └────────────┬─────────────┘
                                 │
                                 ▼
                  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
                  ┃   NÚCLEO DE DOMINIO (CORE)   ┃
                  ┃  Entidades, Enums, Reglas    ┃
                  ┃  (Agregados de Atención,    ┃
                  ┃   Paciente, Pago, Comprobante) ┃
                  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
                                 ▲
                                 │
                    ┌────────────┴─────────────┐
                    │     PUERTOS DE SALIDA    │
                     (Interfaces de Repositorio)
                    └────────────┬─────────────┘
                                 │ (Inversión de Control)
                                 ▼
  ┌─────────────────────────────────────────────────────────────┐
  │                    ADAPTADORES DE SALIDA                    │
  │     [Clinica.Infrastructure (EF Core 9, Neon, QuestPDF)]    │
  │     [Evolution API (WhatsApp), PDF Services, Seeders]       │
  └─────────────────────────────────────────────────────────────┘
```
### Beneficios de esta estructura para la clínica

- **Aislamiento de cambios:** Si la clínica decide cambiar de base de datos (por ejemplo, de Neon.tech a Azure SQL) o de proveedor de hosting (VPS a Azure), solo se afecta la capa de infraestructura. El dominio y los servicios de aplicación permanecen intactos.
- **Testeabilidad:** Cada capa puede ser probada de forma aislada. Las pruebas unitarias del dominio no requieren base de datos; las pruebas de integración utilizan Testcontainers para replicar el entorno real.
- **Mantenibilidad:** El código es más fácil de entender y modificar porque las responsabilidades están claramente separadas. Un nuevo desarrollador puede comprender el dominio sin necesidad de conocer los detalles de persistencia o de la interfaz de usuario.
- **Escalabilidad:** La arquitectura está preparada para la expansión planificada hacia una clínica general. Nuevos servicios y especialidades se integran sin reescribir el núcleo del sistema.

---

## 📐 Componentes de la Arquitectura Hexagonal

La solución se divide en cuatro capas principales, cada una con una responsabilidad bien definida dentro del hexágono:

| Capa | Proyecto | Responsabilidad | Tecnologías Clave |
|------|----------|-----------------|-------------------|
| **Dominio (Core)** | `Clinica.Domain` | Contiene las entidades de negocio (Paciente, Atencion, Pago, Comprobante, etc.), las enumeraciones que definen los estados del sistema, y las interfaces de los puertos (repositorios y servicios). No tiene dependencias externas. | .NET 9, System.ComponentModel.DataAnnotations |
| **Aplicación** | `Clinica.API` (Servicios) | Orquesta los casos de uso implementando los puertos de entrada. Aquí se definen los DTOs, se aplican las validaciones de negocio y se coordinan los repositorios. | .NET 9, interfaces de servicios |
| **Infraestructura** | `Clinica.Infrastructure` | Implementa los puertos de salida: repositorios con Entity Framework Core, generación de PDF con QuestPDF, envío de mensajes por WhatsApp con Evolution API, y seeders de datos iniciales. | EF Core 9, PostgreSQL, QuestPDF, BCrypt, Npgsql |
| **Presentación (API)** | `Clinica.API` (Controllers) | Expone los endpoints RESTful que consume el frontend. Gestiona la autenticación JWT, la autorización por permisos, los filtros de auditoría y el manejo de excepciones. | ASP.NET Core 9, JWT, Swagger, SignalR |
| **Frontend** | `Clinica.WASM` | Interfaz de usuario SPA en Blazor WebAssembly con diseño "Luxury Medical Style". Comunica con la API mediante servicios HTTP y SignalR. | Blazor WebAssembly, MudBlazor, SignalR |

---

## 🔗 Flujo de Dependencias y Comunicación

El flujo de comunicación en la arquitectura es unidireccional y sigue la regla de dependencia:

1. El frontend (Blazor WASM) envía peticiones HTTP al backend (API) o se conecta mediante SignalR para mensajería en tiempo real.
2. La API (controladores) recibe las peticiones, valida la autenticación y autorización, y llama a los servicios de aplicación (puertos de entrada) a través de interfaces definidas en el dominio.
3. Los servicios de aplicación orquestan la lógica de negocio, utilizando los repositorios (puertos de salida) también definidos como interfaces en el dominio.
4. La infraestructura implementa esos repositorios y servicios concretos, manejando la persistencia (EF Core), la generación de documentos (QuestPDF), y la comunicación con servicios externos (Evolution API, almacenamiento en la nube).
5. El dominio es completamente ignorante de todo lo anterior; solo conoce sus propias entidades, reglas y las interfaces que necesita.

Este flujo garantiza que el dominio sea el **centro inmutable** de la aplicación, y que cualquier cambio en la infraestructura o la presentación no afecte las reglas de negocio que hacen única a la Clínica Santa Mónica.

---

## 🧩 Justificación de la Arquitectura para la Clínica Santa Mónica

La elección de la Arquitectura Hexagonal no es técnica: es estratégica. La clínica se enfrenta a un entorno competitivo donde la **diferenciación** es clave. Su propuesta de valor se basa en:

- **Acompañamiento integral:** La paciente no recibe una consulta aislada, sino un seguimiento continuo desde la planificación familiar hasta el posparto. El sistema debe reflejar esa continuidad sin interrupciones.
- **Digitalización y automatización:** La agenda digital, los recordatorios automáticos por WhatsApp y la historia clínica electrónica reducen la carga administrativa y mejoran la experiencia de la paciente. La arquitectura debe soportar estas automatizaciones sin acoplamientos que limiten la evolución.
- **Cumplimiento normativo:** La clínica opera bajo el marco del MINSA, SUSALUD, SUNAT y la Ley de Protección de Datos Personales. La arquitectura debe facilitar la auditoría, la trazabilidad y la seguridad de la información.
- **Escalabilidad futura:** La clínica planea expandirse a clínica general. La arquitectura debe permitir la incorporación de nuevos servicios y especialidades sin reescribir el núcleo.

La Arquitectura Hexagonal, combinada con DDD, proporciona la **flexibilidad** necesaria para cumplir con todos estos requisitos, asegurando que la clínica pueda **innovar sin comprometer la estabilidad** y que los valores de **responsabilidad, ética y calidad** se reflejen en cada línea de código.

---

## 📦 Estructura de Proyectos y su Relación con los Puertos

El repositorio de la solución está organizado en proyectos que reflejan fielmente las capas de la arquitectura:

```text
Clinica.sln
├── Clinica.Domain/
│   ├── Entities/           # Entidades de negocio (Paciente, Atencion, Pago, etc.)
│   ├── Enums/              # Enumeraciones de estado (EstadoAtencion, EstadoPago, etc.)
│   ├── Interfaces/         # Puertos (repositorios y servicios)
│   ├── DTOs/               # Objetos de transferencia de datos por módulo
│   └── Validations/        # Validadores personalizados
├── Clinica.API/
│   ├── Controllers/        # Puertos de entrada REST
│   ├── Services/Imp/       # Implementación de servicios de aplicación
│   ├── Filters/            # Auditoría automática
│   ├── Middlewares/        # Manejo de excepciones y seguridad
│   ├── Hubs/               # SignalR para WhatsApp
│   ├── Authorization/      # Políticas de permisos
│   └── Models/             # ApiResponse y modelos de respuesta
├── Clinica.Infrastructure/
│   ├── Data/               # ApplicationDbContext y configuraciones de EF Core
│   ├── Repositories/       # Implementación de repositorios
│   ├── Documents/          # Servicios de generación de PDF
│   └── Migrations/         # Migraciones de base de datos
├── Clinica.WASM/
│   ├── Components/         # Componentes reutilizables Blazor
│   ├── Pages/              # Páginas de la aplicación
│   ├── Services/Api/       # Servicios HTTP para consumir la API
│   ├── Services/Auth/      # Gestión de autenticación y tokens
│   ├── Layout/             # MainLayout y EmptyLayout
│   ├── Themes/             # ClinicaTheme (paleta de colores)
│   └── wwwroot/            # Archivos estáticos, service worker
└── Tests/
    ├── Clinica.API.Tests/          # Pruebas unitarias (xUnit, NSubstitute)
    └── Clinica.API.IntegrationTests/# Pruebas de integración (Testcontainers)
```
# 🟦 Capa de API (Presentación)

La **Capa de API** constituye el **puerto de entrada** del sistema SYS Clínica Santa Mónica en la Arquitectura Hexagonal. Implementada como una API RESTful sobre **ASP.NET Core 9**, esta capa es responsable de recibir las solicitudes del cliente (Blazor WASM), autenticar y autorizar a los usuarios, orquestar los casos de uso a través de los servicios de aplicación, y retornar respuestas estructuradas. 

En el contexto estratégico de la **Clínica Santa Mónica** en Juliaca, la API actúa como el adaptador de entrada central que materializa la transformación digital de la institución. Su diseño erradica la dependencia tradicional de registros en papel y planillas Excel dispersas, unificando los flujos clínicos, contables y de comunicación en una plataforma única y auditable.

!!! info "Frontera Arquitectónica y Cumplimiento Legal"
    - **Dependencias PROHIBIDAS:** Acceso directo a Entity Framework, lógica de negocio embebida en los controladores, o cualquier operación directa de persistencia. La capa de API solo debe orquestar, no implementar reglas de negocio.
    - **Dependencias PERMITIDAS:** ASP.NET Core, Swagger/OpenAPI, librerías de autenticación JWT, y acceso directo a las interfaces de la capa de Aplicación e Infraestructura mediante inyección de dependencias.
    - **Marco Regulatorio Incorporado:** El pipeline de procesamiento de la API implementa de forma estricta las validaciones y restricciones exigidas por la legislación peruana: protección de datos clínicos sensibles (**Ley N° 29733**), estándares de facturación electrónica (**SUNAT**), y las normativas sanitarias vigentes del **MINSA** y **SUSALUD**.

---

## 🏛️ Arquitectura de la API

La API sigue una organización limpia orientada a desacoplar el protocolo de transporte HTTP de las reglas operativas del negocio. Cada componente responde a un valor compartido de la organización:

| Componente | Propósito Técnico | Relación con la Filosofía de la Clínica | Ubicación |
| :--- | :--- | :--- | :--- |
| **Controllers** | Puntos de entrada REST, manejan rutas, validan DTOs y delegan en servicios. | **Transparencia:** Eliminan intermediarios operacionales y zonas oscuras de información. | `Controllers/` |
| **Services** | Lógica de orquestación de casos de uso (Capa de Aplicación). | **Calidad de Servicio:** Garantizan la ejecución estandarizada de los flujos clínicos. | `Services/Imp/` |
| **Filters** | Atributos transversales para auditoría automática y trazas de auditoría de las acciones del personal. | **Responsabilidad:** Aseguran que cada acción del personal asistencial sea 100% auditable. | `Filters/` |
| **Middlewares** | Captura global de excepciones, manejo de códigos de estado HTTP y seguridad perimetral. | **Ética Profesional:** Protegen el sistema contra accesos no autorizados a datos de salud. | `Middlewares/` |
| **Hubs** | Tuberías de comunicación bidireccional en tiempo real mediante SignalR. | **Innovación con Propósito:** Soportan la sincronización instantánea del chat de mensajería. | `Hubs/` |
| **Helpers** | Utilidades criptográficas para tokens JWT y normalización de husos horarios locales. | **Seguridad Inquebrantable:** Encriptan las credenciales e identidades del personal. | `Helpers/` |
| **Authorization** | Evaluación dinámica de políticas basadas en los permisos del Colegio Médico/Obstetras. | **Legalidad y Formalidad:** Restringen el acceso a datos sensibles según el perfil colegiado. | `Authorization/` |
| **Configurations** | Mapeo de opciones fuertemente tipadas (Evolution API, credenciales SUNAT). | **Sostenibilidad:** Aíslan las credenciales técnicas de la lógica compilada. | `Configurations/` |
| **Models** | Modelos y DTOs de respuesta estandarizados (`ApiResponse<T>`). | **Orden y Previsibilidad:** Proveen un contrato JSON uniforme hacia el frontend. | `Models/` |

---

## 🔌 Controladores (Puertos de Entrada)

Los controladores exponen los endpoints REST que consume la interfaz de usuario. Es crucial destacar que el sistema SYS Clínica Santa Mónica es una **herramienta de gestión interna** operada exclusivamente por el personal de la clínica (recepcionistas, médicos, administradores y obstetras); **no es un portal de autogestión para pacientes**.

### Módulo de Autenticación y Seguridad
Gobierna el acceso restringido a la plataforma, asegurando el cumplimiento de la Ley N° 29733 al verificar mediante tokens JWT que el personal solo acceda a los datos estrictamente vinculados a su rol laboral.

| Controlador | Endpoints Base | Propósito Operativo e Institucional |
| :--- | :--- | :--- |
| **AuthController** | `api/auth` | Gestión del inicio de sesión seguro y flujo forzado de cambio de contraseña inicial (`DebeCambiarContrasena`). |
| **UsuariosController** | `api/usuarios` | Administración del personal, control de estado (`Activo`/`Inactivo`) y asignación de roles laborales. |
| **RolesController** | `api/roles` | Configuración de la matriz de privilegios y asignación de permisos atómicos por rol del sistema. |
| **PermisosController** | `api/permisos` | Exposición del catálogo completo de tokens de seguridad autorizados en la plataforma. |

### Módulo de Gestión Clínica y Agendas
Controla la administración demográfica de las pacientes y la optimización de los horarios médicos presenciales en Juliaca, combatiendo los tiempos muertos y las sobreposiciones de turnos.

| Controlador | Endpoints Base | Propósito Operativo e Institucional |
| :--- | :--- | :--- |
| **PacientesController** | `api/pacientes` | Registro digital con validación de DNI, actualización de fichas de contacto y carga del **Módulo de Filiación extendido**. |
| **DoctoresController** | `api/doctores` | Administración del staff médico, validación de colegiaturas vigentes y filtros avanzados de especialidades. |
| **ServiciosClinicosController** | `api/serviciosclinicos` | Catálogo de prestaciones médicas con parametrización de costos base y duraciones estimadas en minutos. |
| **HorariosController** | `api/horarios` | Modelado de la matriz semanal de disponibilidad médica para un control exacto de la agenda del consultorio. |

### Módulo de Citas y Atenciones Obstétricas
Materializa el valor de **Puntualidad y Respeto por el tiempo** de las pacientes. La API traduce las interacciones presenciales y las alertas asíncronas en un flujo obstétrico continuo.

| Controlador | Endpoints Base | Propósito Operativo e Institucional |
| :--- | :--- | :--- |
| **CitasController** | `api/citas` | Programación, reprogramación por motivos justificados y cancelaciones lógicas de citas médicas de control. |
| **AtencionesController** | `api/atenciones` | Registro, apertura y cierre definitivo del acto médico, impidiendo mutaciones de datos tras la consulta. |
| **AtencionesObstetricasController** | `api/atenciones/{id}/...` | Endpoints especializados para la evolución modular independiente: Anamnesis, Examen Físico, Tacto Vaginal y Ecografía. |

### Módulo Financiero, Farmacia y Comprobantes SUNAT
Garantiza la transparencia contable y la legalidad tributaria de la clínica. Coordina de manera transaccional la relación entre el acto médico, la dispensación en la farmacia integrada y la emisión del comprobante electrónico ante la SUNAT.

| Controlador | Endpoints Base | Propósito Operativo e Institucional |
| :--- | :--- | :--- |
| **PagosController** | `api/pagos` | Recaudación de caja, control de montos netos, adelantos preventivos de separación y deudas pendientes. |
| **ComprobantesController** | `api/comprobantes` | Emisión y previsualización de documentos electrónicos (Boletas, Facturas, Notas de Crédito) sincronizados con SUNAT. |
| **FinanzasController** | `api/finanzas` | Consolidación de estados de cuenta analíticos por paciente, deudas reales cruzadas y arqueos diarios de caja. |

### Módulo de Historial, Auditoría y Documentación PDF

| Controlador | Endpoints Base | Propósito Operativo e Institucional |
| :--- | :--- | :--- |
| **HistorialesController** | `api/historiales` | Consulta federada de la carpeta médica electrónica, soportando la migración activa de los **5,000 registros históricos**. |
| **AuditoriaController** | `api/auditoria` | Extracción de logs operacionales paginados. Muestra los estados `ValorAnterior` y `ValorNuevo` para auditoría forense. |
| **CertificadosController** | `api/certificados` | Generación de certificados de trabajo institucionales individuales o en bloque con códigos únicos de validación. |
| **HistoriaClinicaController** | `api/historiaclinica` | Renderizado binario de la Historia Clínica completa (Filiación, Antecedentes, Funciones Vitales y Evolución). |
| **ReportesFinancierosController** | `api/reportesfinancieros` | Emisión del Libro Diario de Ingresos y reportes de caja consolidados por método de pago. |
| **ResumenPartoController** | `api/resumenparto` | Exportación legal del Partograma horario y las métricas de evaluación del Recién Nacido (Escala Apgar). |

### Módulo de Integración con WhatsApp (Evolution API)
Este módulo representa la proactividad del sistema. La API interactúa con servidores externos de mensajería para mitigar la tasa de ausentismo en Juliaca, enviando recordatorios automáticos al canal habitual de la paciente sin saturar la carga de la recepción.

| Controlador | Endpoints Base | Propósito Operativo e Institucional |
| :--- | :--- | :--- |
| **ChatsController** | `api/whatsapp/chats` | Despacho de mensajería proactiva, alertas de inasistencia a controles críticos e historiales de conversación. |
| **WhatsAppController** | `api/whatsapp/device` | Orquestación de la infraestructura técnica para la extracción del código QR de vinculación del dispositivo de la clínica. |
| **WebhooksController** | `api/webhooks/evolution` | Puerto de escucha asíncrono para la captura de eventos de mensajería entrante y actualización de lectura en tiempo real. |

---

## 🛠️ Filtros de Extensión y Gestión Transversal

### Auditoría Automatizada por Atributos
Para evitar la duplicidad de código y asegurar la transparencia administrativa, la API implementa un filtro de acción personalizado llamado `[Auditoria]`. Este interceptor analiza las peticiones salientes exitosas y registra de forma asíncrona la actividad en la base de datos de PostgreSQL, capturando metadatos del entorno como direcciones IP y agentes de usuario.

---

## 💻 Ejemplo de Implementación Estándar: `AtencionesController`

El siguiente fragmento de código ilustra cómo los controladores de la API actúan como adaptadores estrictos, delegando la ejecución del caso de uso en la capa de aplicación, validando los permisos del Colegio Médico y gatillando la auditoría de operaciones conforme a los valores institucionales:

```csharp
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Clinica.Domain.Interfaces;
    using Clinica.Domain.DTOs.Atenciones;
    using Clinica.Domain.Enums;
    using Clinica.API.Authorization;
    using Clinica.API.Filters;
    using Clinica.API.Models;

    namespace Clinica.API.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    [Tags("Atenciones Médicas (Core)")]
    public class AtencionesController : ControllerBase
    {
        private readonly IAtencionService _atencionService;

        public AtencionesController(IAtencionService atencionService)
        {
            _atencionService = atencionService;
        }

        /// <summary>
        /// Recupera la colección completa de actos médicos registrados en el sistema.
        /// Exige privilegios de visualización clínica conforme a SUSALUD.
        /// </summary>
        [Authorize(Policy = PermisosPolicies.AtencionVer)]
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var atenciones = await _atencionService.ObtenerTodasAsync();
            return Ok(ApiResponse<object>.Ok(atenciones, "Atenciones obtenidas correctamente."));
        }

        /// <summary>
        /// Registra y apertura un nuevo acto médico obstétrico en Juliaca.
        /// Gatilla una auditoría de nivel Importante y protege la integridad del flujo clínico.
        /// </summary>
        [Authorize(Policy = PermisosPolicies.AtencionRegistrar)]
        [Auditoria("Atenciones", "Atencion", TipoAccionAuditoria.Creacion, NivelAuditoria.Importante)]
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegistrarAtencionDto dto)
        {
            // La validación del modelo DTO es automática gracias al atributo [ApiController]
            var id = await _atencionService.RegistrarAtencionAsync(dto);
            
            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id },
                ApiResponse<object>.Ok(new { Id = id }, "Atención registrada y aperturada correctamente.", 201)
            );
        }

        [Authorize(Policy = PermisosPolicies.AtencionVer)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObtenerPorId(Guid id)
        {
            var atencion = await _atencionService.ObtenerPorIdCompletoAsync(id);
            if (atencion == null)
                return NotFound(ApiResponse<object>.Error("El expediente de atención solicitado no existe.", 404));

            return Ok(ApiResponse<object>.Ok(atencion, "Expediente de atención recuperado correctamente."));
        }
    }
```
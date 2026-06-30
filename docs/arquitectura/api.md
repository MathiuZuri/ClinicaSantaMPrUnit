# 🟦 Capa de API (Presentación)

La **Capa de API** constituye el **puerto de entrada** del sistema SIGEC en la Arquitectura Hexagonal. Implementada como una API RESTful sobre **ASP.NET Core 9**, esta capa es responsable de recibir las solicitudes del cliente (Blazor WASM), autenticar y autorizar a los usuarios, orquestar los casos de uso a través de los servicios de aplicación, y retornar respuestas estructuradas. La API actúa como el adaptador de entrada que traduce las peticiones HTTP en llamadas a la lógica de negocio, manteniendo el dominio completamente aislado de los detalles de transporte y presentación.

!!! info "Frontera Arquitectónica"
    - **Dependencias PROHIBIDAS:** Acceso directo a Entity Framework, lógica de negocio en los controladores, o cualquier operación de persistencia. La capa de API solo debe orquestar, no implementar reglas de negocio.
    - **Dependencias PERMITIDAS:** ASP.NET Core, Swagger/OpenAPI, librerías de autenticación JWT, y las capas de Aplicación, Dominio e Infraestructura (a través de inyección de dependencias).
    - **Responsabilidad:** Validar solicitudes, autenticar/ autorizar, mapear DTOs, llamar a servicios de aplicación, y estructurar respuestas HTTP estandarizadas.

---

## 🏛️ Arquitectura de la API

La API sigue una arquitectura **limpia y por capas**, organizada en los siguientes componentes:

| Componente | Propósito | Ubicación |
|------------|-----------|-----------|
| **Controllers** | Puntos de entrada REST, manejan rutas y delegan en servicios | `Controllers/` |
| **Services** | Lógica de orquestación de casos de uso (Capa de Aplicación) | `Services/Imp/` |
| **Filters** | Atributos para auditoría automática y otras preocupaciones transversales | `Filters/` |
| **Middlewares** | Pipeline de procesamiento HTTP (manejo de excepciones, seguridad) | `Middlewares/` |
| **Hubs** | Comunicación en tiempo real (SignalR para WhatsApp) | `Hubs/` |
| **Helpers** | Utilidades (JWT, fechas) | `Helpers/` |
| **Authorization** | Políticas de permisos | `Authorization/` |
| **Configurations** | Configuraciones de la aplicación (WhatsApp, validación) | `Configurations/` |
| **Models** | DTOs de respuesta estandarizados | `Models/` |

---

## 🔌 Controladores (Puertos de Entrada)

Los controladores exponen los endpoints REST que el frontend consume. Cada controlador corresponde a un módulo funcional del sistema y utiliza servicios de aplicación para orquestar la lógica de negocio. Todos los controladores heredan de `ControllerBase` y utilizan los atributos `[ApiController]` y `[Route]`.

### Módulo de Autenticación y Seguridad

| Controlador | Endpoints Base | Propósito |
|-------------|---------------|-----------|
| **AuthController** | `api/auth` | Inicio de sesión, cambio de contraseña |
| **UsuariosController** | `api/usuarios` | CRUD de usuarios, asignación de roles, cambio de estado |
| **RolesController** | `api/roles` | CRUD de roles, asignación de permisos |
| **PermisosController** | `api/permisos` | Catálogo de permisos del sistema |

### Módulo de Gestión Clínica

| Controlador | Endpoints Base | Propósito |
|-------------|---------------|-----------|
| **PacientesController** | `api/pacientes` | CRUD de pacientes, actualización de contacto, cambio de estado |
| **DoctoresController** | `api/doctores` | CRUD de doctores, contratación, búsqueda avanzada |
| **ServiciosClinicosController** | `api/serviciosclinicos` | Catálogo de servicios clínicos |
| **HorariosController** | `api/horarios` | CRUD de horarios, matriz semanal de disponibilidad |

### Módulo de Citas y Atenciones

| Controlador | Endpoints Base | Propósito |
|-------------|---------------|-----------|
| **CitasController** | `api/citas` | CRUD de citas, reprogramación, cancelación |
| **AtencionesController** | `api/atenciones` | Registro, cierre, anulación de atenciones médicas |
| **AtencionesObstetricasController** | `api/atenciones/{id}/...` | Gestión de módulos clínicos (Anamnesis, Exámenes, Ecografías, etc.) |

### Módulo Financiero

| Controlador | Endpoints Base | Propósito |
|-------------|---------------|-----------|
| **PagosController** | `api/pagos` | Registro de pagos, cambio de estado, consultas |
| **ComprobantesController** | `api/comprobantes` | Emisión, previsualización, PDF, anulación de comprobantes |
| **FinanzasController** | `api/finanzas` | Resúmenes, deudas reales, estado de cuenta, ajustes |

### Módulo de Historial y Auditoría

| Controlador | Endpoints Base | Propósito |
|-------------|---------------|-----------|
| **HistorialesController** | `api/historiales` | Consulta de historiales clínicos |
| **AuditoriaController** | `api/auditoria` | Registros de auditoría con paginación y filtros |

### Módulo de WhatsApp (Evolution API)

| Controlador | Endpoints Base | Propósito |
|-------------|---------------|-----------|
| **ChatsController** | `api/chats` | Envío de mensajes, listado de conversaciones, historial |
| **WhatsAppController** | `api/whatsapp` | Obtención de QR para vinculación de dispositivos |
| **WebhooksController** | `api/webhooks` | Recepción de webhooks de Evolution API (mensajes entrantes) |

### Módulo de Documentos PDF

| Controlador | Endpoints Base | Propósito |
|-------------|---------------|-----------|
| **CertificadosController** | `api/certificados` | Generación de certificados de trabajo (individual y bloque) |
| **HistoriaClinicaController** | `api/historiaclinica` | Generación de historia clínica en PDF |
| **ReportesFinancierosController** | `api/reportesfinancieros` | Generación de reportes financieros diarios en PDF |
| **ResumenPartoController** | `api/resumenparto` | Generación de resumen de parto en PDF |

### Ejemplo de Controlador: `AtencionesController`

```csharp
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

    [Authorize(Policy = PermisosPolicies.AtencionVer)]
    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        var atenciones = await _atencionService.ObtenerTodasAsync();
        return Ok(ApiResponse<object>.Ok(atenciones, "Atenciones obtenidas correctamente."));
    }

    [Authorize(Policy = PermisosPolicies.AtencionRegistrar)]
    [Auditoria("Atenciones", "Atencion", TipoAccionAuditoria.Creacion, NivelAuditoria.Importante)]
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAtencionDto dto)
    {
        var id = await _atencionService.RegistrarAtencionAsync(dto);
        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id },
            ApiResponse<object>.Ok(new { Id = id }, "Atención registrada y aperturada correctamente.", 201)
        );
    }
}
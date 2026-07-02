# 🌐 Capa de Presentación (Clinica.WASM) – SYS Clínica Santa Mónica

La **Capa de Presentación** representa el **puerto de entrada** del usuario final al sistema **SYS Clínica Santa Mónica**, el sistema de gestión clínica desarrollado a medida para la Clínica Santa Mónica en Juliaca. Implementada como una **Single Page Application (SPA)** en **Blazor WebAssembly**, esta capa proporciona una interfaz de usuario rica, receptiva y con un diseño de alto contraste que refleja la identidad corporativa **"Luxury Medical Style"** de la clínica. La aplicación se comunica exclusivamente con la API a través de servicios HTTP que manejan la autenticación y autorización basada en JWT, materializando la **propuesta de valor** de atención materna digital, segura y cercana que distingue a la institución en el mercado de Juliaca.

!!! info "Frontera Arquitectónica"
    - **Dependencias PERMITIDAS:** Blazor WebAssembly, MudBlazor (librería de componentes UI), SignalR (comunicación en tiempo real), Microsoft.JSInterop (para interacción con JavaScript), y servicios HTTP.
    - **Dependencias PROHIBIDAS:** Acceso directo a bases de datos, Entity Framework, o cualquier capa de persistencia. Toda la comunicación con el backend se realiza exclusivamente a través de la API RESTful definida en la capa de presentación.
    - **Comunicación con el exterior:** Se realiza mediante peticiones HTTP a los endpoints de la API (con autenticación JWT) y mediante SignalR para la mensajería en tiempo real (WhatsApp), en línea con la **filosofía de innovación con propósito** de la clínica.

---

## 🏛️ Arquitectura del Frontend

La aplicación Blazor WASM sigue una arquitectura **limpia y modular**, organizada en las siguientes capas funcionales:

| Capa | Propósito | Ubicación |
|------|-----------|-----------|
| **Layout** | Define la estructura visual global (AppBar, Drawer) | `Layout/` |
| **Páginas** | Representan las vistas completas del sistema, accesibles por ruta | `Pages/` |
| **Componentes** | Unidades reutilizables de UI (tablas, formularios, diálogos) | `Components/` |
| **Servicios** | Lógica de negocio del cliente, comunicación con API, estado de autenticación | `Services/` |
| **Constantes** | Endpoints de API, rutas de navegación, códigos de permisos | `Constants/` |
| **DTOs** | Objetos de transferencia de datos, copias de los DTOs del dominio | `DTOs/` |
| **Temas** | Configuración de la paleta de colores y estilos de MudBlazor | `Themes/` |

Esta estructura refleja el **compromiso con la calidad y la organización** que la clínica pregona en sus valores, asegurando que el código sea mantenible y escalable para la expansión planificada hacia una clínica general.

---

## 📐 Sistema de Layout y Navegación

### MainLayout (Layout Principal)
El `MainLayout` es el contenedor visual principal de la aplicación. Integra:

- **AppBar:** Barra superior con el logo, título de la clínica, nombre del usuario autenticado y botón de cierre de sesión. Refuerza la **imagen de modernidad y profesionalismo** que la clínica proyecta.
- **Drawer:** Menú lateral de navegación con enlaces a los módulos principales del sistema (Dashboard, Pacientes, Citas, Atenciones, Doctores, Horarios, Pagos, Comprobantes, Finanzas, Historial Clínico, Auditoría, Usuarios, Roles, Permisos y Chat). Este menú se adapta a los permisos del usuario, reflejando el **control de acceso basado en roles** que garantiza la seguridad de la información clínica.
- **MainContent:** Área dinámica donde se renderizan las páginas según la ruta actual.

El `NavMenu` en el drawer utiliza el componente `RequirePermission` para ocultar/mostrar opciones según los permisos del usuario autenticado, alineándose con el **compromiso de la clínica con la protección de datos** (Ley N° 29733).

### EmptyLayout
Layout minimalista utilizado exclusivamente para las páginas de autenticación (Login y Cambiar Contraseña). Presenta un diseño de dos columnas con una sección de branding a la izquierda y el formulario a la derecha, comunicando **confianza y formalidad** desde el primer contacto digital.

### Sistema de Enrutamiento
La aplicación utiliza el enrutamiento integrado de Blazor con el atributo `@page`. Las rutas principales se definen como constantes en `AppRoutes.cs` para mantener la consistencia:

| Ruta | Página | Propósito |
|------|--------|-----------|
| `/` | Home | Redirige a Dashboard o Login según autenticación |
| `/login` | LoginPage | Pantalla de inicio de sesión |
| `/cambiar-contrasena` | CambiarContrasenaPage | Forzar cambio de contraseña en primer login |
| `/dashboard` | Dashboard | Panel de control con KPIs y accesos rápidos |
| `/panel/pacientes` | PacientesPage | Gestión de pacientes |
| `/panel/citas` | CitasPage | Agenda y programación de citas |
| `/atenciones` | AtencionesPage | Registro y seguimiento de atenciones médicas |
| `/doctores` | DoctoresPage | Gestión del personal médico |
| `/horarios` | HorariosPage | Control de horarios y turnos |
| `/panel/horarios/agenda` | AgendaMatriz | Vista semanal de ocupación de médicos |
| `/pagos` | PagosPage | Gestión de pagos y caja |
| `/comprobantes` | ComprobantesPage | Emisión y control de comprobantes |
| `/finanzas` | FinanzasPage | Reportes financieros y ajustes |
| `/historia-clinica` | HistorialClinicoPage | Consulta de expedientes clínicos |
| `/auditoria` | AuditoriaPage | Registro de auditoría del sistema |
| `/usuarios` | UsuariosPage | Gestión de cuentas de usuario |
| `/roles` | RolesPage | Gestión de roles y permisos |
| `/permisos` | PermisosPage | Catálogo de permisos del sistema |
| `/servicios-clinicos` | ServiciosClinicosPage | Catálogo de servicios médicos |
| `/panel/santamonica-chat` | PsicomedixChat | Módulo de chat por WhatsApp |

---

## 🧩 Componentes Principales por Módulo

La aplicación está construida con componentes reutilizables organizados por módulo funcional. Cada módulo contiene:
- **Tablas (`*Table.razor`):** Visualización de datos con filtros y paginación.
- **Formularios (`*Form.razor`):** Captura de datos para creación/edición.
- **Diálogos (`*Dialog.razor`):** Ventanas modales para operaciones específicas.

### Componentes de Autenticación
- `RequireAuth`: Componente envolvente que verifica autenticación y redirige a Login si es necesario.
- `RequirePermission`: Similar a `RequireAuth`, pero verifica que el usuario tenga un permiso específico antes de renderizar su contenido.
- `CambiarContrasenaDialog`: Diálogo para cambiar la contraseña desde el perfil.

### Módulo de Atenciones
- `RegistrarAtencionDialog`: Diálogo multi-step (Stepper) para registrar una nueva atención con todos los submódulos clínicos (Anamnesis, Exámenes Físicos, Tactos Vaginales, Ecografías, Impresión Diagnóstica). Este componente refleja el **acompañamiento integral** que la clínica ofrece a las pacientes en cada etapa reproductiva.
- `CerrarAtencionDialog`: Diálogo para cerrar una atención e ingresar la impresión diagnóstica final.
- `AnularAtencionDialog`: Diálogo para anular una atención (eliminación lógica).
- `DetalleAtencionDialog`: Visualización completa de una atención con todos sus módulos clínicos expandibles.

### Módulo de Citas
- `CitaForm`: Formulario para crear una nueva cita con autocompletado de pacientes, doctores y servicios.
- `CitasTable`: Tabla de citas con filtros, búsqueda y acciones (ver, reprogramar, cancelar).
- `CitaDetalleDialog`: Visualización detallada de una cita.
- `CitaReprogramarDialog`: Diálogo para cambiar fecha/hora de una cita.
- `CitaCancelarDialog`: Diálogo para cancelar una cita con justificación.

### Módulo de Comprobantes
- `EmitirComprobanteDialog`: Diálogo multi-step para emitir comprobantes (boletas, constancias, resúmenes, estados de cuenta) con previsualización, dando cumplimiento a las obligaciones de **facturación electrónica SUNAT**.
- `ComprobantesTable`: Tabla de comprobantes con descarga de PDF y anulación.
- `AnularComprobanteDialog`: Diálogo para anular un comprobante con motivo.

### Módulo de Finanzas
- `FinanzasResumenCaja`: Componente de resumen de caja con KPIs (tarjetas de métricas) y tabla de movimientos.
- `FinanzasDeudasReales`: Listado de deudas reales por atención.
- `AjustesFinancierosList`: Listado de ajustes financieros registrados.
- `RegistrarAjusteFinancieroDialog`: Diálogo para registrar un nuevo ajuste financiero.

### Módulo de Chat (WhatsApp)
- `ListaContactos`: Sidebar con la lista de conversaciones activas y estado de conexión de WhatsApp (QR/Conectado). Este componente es clave porque **WhatsApp es el canal principal de comunicación** con las pacientes, en línea con la estrategia de inclusión digital de la clínica.
- `VentanaConversacion`: Área central con el historial de mensajes y campo de entrada.
- `VincularWhatsAppDialog`: Diálogo que muestra el código QR para vincular un dispositivo.

### Componentes Compartidos (Shared)
- `EmptyState`: Componente para mostrar estados vacíos con icono y mensaje.
- `ErrorState`: Componente para mostrar errores con opción de reintento.
- `LoadingState`: Componente para estados de carga con spinner.
- `PageHeader`: Encabezado de página con título, subtítulo y acciones.

---

## 📦 Servicios y Gestión de Estado

### Servicios API (`Services/Api/`)

Cada servicio corresponde a un controlador del backend y proporciona métodos para interactuar con la API. Todos los servicios inyectan `HttpClient` con el `AuthHeaderHandler` para incluir automáticamente el token JWT en las peticiones.

| Servicio | Endpoints Base | Propósito |
|----------|---------------|-----------|
| `AuthApiService` | `api/auth` | Autenticación, inicio de sesión, cambio de contraseña |
| `PacienteApiService` | `api/pacientes` | CRUD de pacientes, actualización de contacto, cambio de estado |
| `CitaApiService` | `api/citas` | CRUD de citas, reprogramación, cancelación |
| `AtencionApiService` | `api/atenciones` | Registro, cierre, anulación de atenciones |
| `DoctorApiService` | `api/doctores` | CRUD de doctores, contratación, búsqueda avanzada |
| `HorarioDoctorApiService` | `api/horarios` | CRUD de horarios, matriz semanal de disponibilidad |
| `PagoApiService` | `api/pagos` | Registro de pagos, cambio de estado, consultas por paciente |
| `ComprobanteApiService` | `api/comprobantes` | Emisión, previsualización, PDF, anulación de comprobantes |
| `FinanzasApiService` | `api/finanzas` | Resúmenes, deudas reales, estado de cuenta, ajustes |
| `AuditoriaApiService` | `api/auditoria` | Consulta paginada de registros de auditoría |
| `HistorialClinicoApiService` | `api/historiales` | Consulta de historiales clínicos |
| `UsuarioApiService` | `api/usuarios` | CRUD de usuarios, asignación de roles, cambio de estado |
| `RolApiService` | `api/roles` | CRUD de roles, asignación de permisos |
| `PermisoApiService` | `api/permisos` | Consulta de catálogo de permisos |
| `ServicioClinicoApiService` | `api/serviciosclinicos` | Consulta de servicios clínicos |
| `ChatApiService` | `api/chats` | Obtención de chats, envío de mensajes, historial |
| `WhatsAppApiService` | `api/whatsapp` | Obtención de QR para vinculación de WhatsApp |

### Servicios de Autenticación y Estado

- **`TokenStorageService`:** Gestiona el almacenamiento de credenciales en `localStorage` (token, roles, permisos, datos del usuario).
- **`AuthStateService`:** Proporciona métodos para verificar autenticación, permisos y roles, manteniendo un caché en memoria.
- **`AuthHeaderHandler`:** Intercepta todas las peticiones HTTP para agregar el token JWT en el header `Authorization`. También maneja respuestas `401 Unauthorized` limpiando la sesión y redirigiendo al Login.
- **`AuthRedirectService`:** Servicio auxiliar para redirigir a la página de Login desde cualquier parte de la aplicación.

---

## 🔐 Mecanismos de Seguridad

### Autenticación (JWT)
El flujo de autenticación es el siguiente:

1. El usuario ingresa credenciales en `LoginPage`.
2. `AuthApiService.IniciarSesionAsync` envía las credenciales al endpoint `api/auth/login`.
3. El backend valida y retorna un token JWT junto con roles y permisos.
4. `TokenStorageService` almacena el token, roles y permisos en `localStorage`.
5. `AuthStateService` actualiza su caché con la información del usuario.
6. El usuario es redirigido a Dashboard o a CambiarContraseña si es requerido.

### Autorización (Permisos)
- **`RequirePermission`:** Componente que verifica si el usuario tiene un permiso específico antes de renderizar el contenido. Si no tiene el permiso, oculta el contenido.
- **`RequireAuth`:** Componente que verifica autenticación. Si no está autenticado, redirige a Login.
- **Políticas de permisos:** Los códigos de permisos se definen en `Permisos.cs` y coinciden con los definidos en el backend.

### Protección de Rutas
Las páginas principales utilizan `RequireAuth` y `RequirePermission` para proteger el acceso:


## 🎨 Tema y Estilos (ClinicaTheme)

El tema `ClinicaTheme` extiende `MudTheme` y define la identidad visual **"Luxury Medical Style"** con paletas para modo claro y oscuro, alineadas con la **imagen de modernidad y confianza** que la clínica proyecta.

### Paleta Claro (Modo Día)

| Color | Código | Uso |
|-------|--------|-----|
| **Primary** | `#2B4CDE` | Azul Clínico Institucional (botones principales, encabezados) |
| **Secondary** | `#D48A9C` | Rose Gold (acentos, detalles premium) |
| **Tertiary** | `#E5B324` | Oro/Ámbar (detalles decorativos, notificaciones) |
| **Background** | `#F4F7FB` | Fondo general con ligera tonalidad azul |
| **Surface** | `#FFFFFF` | Tarjetas, paneles, elementos elevados |
| **Divider** | `#CBD5E1` | Líneas de separación |

### Paleta Oscuro (Modo Noche)

| Color | Código | Uso |
|-------|--------|-----|
| **Primary** | `#849DFF` | Azul claro para modo oscuro |
| **Secondary** | `#EBB1C3` | Rose Gold adaptado para fondos oscuros |
| **Tertiary** | `#FCD34D` | Oro vibrante para modo noche |
| **Background** | `#090E17` | Fondo profundo Navy/Slate |
| **Surface** | `#161D2F` | Tarjetas con elevación sobre fondo oscuro |

### Tipografía

- **Fuente principal:** Inter (Google Fonts) como sistema tipográfico moderno y legible.
- **Tamaños:** Escala tipográfica definida con `H1` (2.25rem), `H2` (1.75rem), `H3` (1.25rem), `Body` (0.875rem).
- **Pesos:** Uso extensivo de `fw-700`, `fw-800` para enfatizar información clave.

### Sombras

El sistema de sombras utiliza una escala de 0 a 25 con incrementos visuales precisos, proporcionando profundidad tridimensional a tarjetas, diálogos y elementos interactivos.

---

## 🔄 Flujo de Ejecución Típico (Registro de una Atención)

A continuación se describe el flujo completo desde que el usuario inicia el registro de una atención hasta que la información persiste en el backend:

1. **Usuario:** Hace clic en "Registrar Atención" en la página `AtencionesPage`.
2. **UI:** Se abre `RegistrarAtencionDialog`, un diálogo multi-step que guía al usuario a través de:
   - **Paso 1:** Datos administrativos (paciente, doctor, servicio, costo).
   - **Paso 2:** Anamnesis (motivo, fórmula obstétrica, antecedentes).
   - **Paso 3:** Exámenes físicos (múltiples registros con LOTEP, AU, LCF, etc.).
   - **Paso 4:** Tactos vaginales (múltiples registros con dilatación, borramiento, etc.).
   - **Paso 5:** Ecografías (múltiples registros con biometría fetal).
   - **Paso 6:** Impresión diagnóstica (diagnóstico principal, indicaciones, próxima cita).
3. **Validación Frontend:** Cada paso valida los campos requeridos usando `DataAnnotationsValidator`.
4. **Envío:** Al hacer clic en "Registrar Atención", el diálogo construye un `RegistrarAtencionDto` con todos los datos.
5. **Servicio API:** `AtencionApiService.RegistrarAsync` envía el DTO mediante `POST` al endpoint `api/atenciones`.
6. **Backend:** El controlador recibe la solicitud, valida, y delega en el servicio de aplicación que persiste la entidad.
7. **Respuesta:** El backend retorna un `ApiResponse<Guid>` con el ID de la atención creada.
8. **UI:** El diálogo se cierra, se muestra un `Snackbar` de éxito, y la tabla de atenciones se recarga automáticamente.

Este flujo refleja el **acompañamiento integral** que la clínica ofrece a las pacientes, desde el primer contacto hasta el cierre de la atención, con total trazabilidad y seguridad de la información.

---

## 📝 Notas Adicionales sobre el Frontend

- **PWA (Progressive Web App):** La aplicación está configurada como PWA con soporte para modo offline y service worker (archivos `service-worker.js` y `service-worker.published.js`), garantizando **disponibilidad continua** incluso ante interrupciones de conectividad.

- **SignalR para Chat:** El módulo de chat utiliza SignalR para recibir mensajes en tiempo real, complementando las peticiones HTTP para enviar mensajes. La conexión se establece al cargar la página de chat, lo que permite una **comunicación proactiva** con las pacientes, en línea con el valor de **cercanía y acompañamiento**.

- **Descarga de PDFs:** Los comprobantes se descargan mediante peticiones HTTP que retornan arreglos de bytes, los cuales se convierten a base64 y se descargan usando JavaScript (`saveAsFile`), facilitando la **transparencia administrativa** y el cumplimiento de SUNAT.

- **MudBlazor:** La librería de componentes UI proporciona una amplia gama de controles (tablas, formularios, diálogos, tarjetas, steppers, etc.) que se adaptan al tema personalizado, permitiendo una **experiencia de usuario fluida y profesional**.

- **Estilos Responsivos:** Todas las páginas y componentes están diseñados con adaptabilidad móvil estricta. Las tablas colapsan verticalmente en dispositivos pequeños utilizando `DataLabel`, y los botones se expanden al 100% de ancho, reflejando el **compromiso con la accesibilidad** y la **inclusión digital** de la clínica.

- **Gestión de Errores:** Los servicios API capturan excepciones y las exponen a través de propiedades `ErrorMessage` e `IsLoading`, que los componentes consumen para mostrar estados de carga y error, asegurando que el equipo pueda **resolver incidencias de manera ágil**.

- **Autocompletados Asíncronos:** Los campos de búsqueda de pacientes, doctores y servicios utilizan `MudAutocomplete` con funciones de búsqueda asíncronas que filtran en memoria las listas cargadas, mejorando la **eficiencia operativa** del personal de recepción y médicos.

- **Snackbars y Diálogos:** Las notificaciones de éxito/error se muestran mediante `MudSnackbar`, y las operaciones críticas se realizan a través de diálogos modales (`MudDialog`), garantizando que el usuario reciba **retroalimentación clara** en cada acción.
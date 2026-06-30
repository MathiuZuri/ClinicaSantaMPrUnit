# 🟣 Capa de Dominio (Core)

La **Capa de Dominio** constituye el núcleo inmutable del sistema **SIGEC** para la **Clínica Santa Mónica**. Diseñada bajo los principios de **Arquitectura Hexagonal**, esta capa es completamente agnóstica a cualquier framework de infraestructura, persistencia o presentación. Su única responsabilidad es encapsular las **reglas de negocio**, las **entidades del dominio**, los **agregados clínicos** y las **enumeraciones** que rigen el comportamiento del sistema.

!!! info "Frontera Arquitectónica"
    - **Dependencias PROHIBIDAS:** Entity Framework, ASP.NET Core, cualquier librería de acceso a datos, JSON serialization, logging específico de infraestructura o sistemas de inyección de dependencias externos.
    - **Dependencias PERMITIDAS:** Solo librerías estándar de .NET (System, System.Collections.Generic, System.ComponentModel.DataAnnotations para anotaciones de validación) y los proyectos de referencia internos (ninguno, es la capa más interna).
    - **Comunicación con el exterior:** Se realiza exclusivamente a través de **puertos** (interfaces) definidos en esta misma capa, que serán implementados por las capas superiores (Aplicación e Infraestructura).

---

## 🏛️ Entidades de Dominio (Core Entities)

Las entidades representan los conceptos fundamentales del negocio. A continuación se resumen las más relevantes, agrupadas por su función dentro del ecosistema.

| Entidad | Propósito | Relaciones Principales |
|---------|-----------|------------------------|
| **Paciente** | Expediente personal de la gestante, con datos de filiación avanzada (lugar de nacimiento, ocupación, pareja, etc.) | 1:1 con `HistorialClinico`; 1:N con `Cita`, `Atencion`, `Pago`, `Comprobante` |
| **Doctor** | Médico especialista contratado, con su CMP, especialidad y vigencia de contrato | 1:N con `HorarioDoctor`, `Cita`, `Atencion` |
| **Usuario** | Cuenta de acceso al sistema, con roles y permisos | 1:N con `UsuarioRol`, `Auditoria`, comprobantes emitidos/anulados |
| **Rol / Permiso** | Control de acceso basado en roles (RBAC) | N:N a través de `UsuarioRol` y `RolPermiso` |
| **ServicioClinico** | Servicios médicos ofrecidos (consulta, ecografía, parto, etc.) con costo base y duración | 1:N con `Cita`, `Atencion`, `Pago` |
| **Cita** | Programación de una consulta o procedimiento | 1:1 con `Atencion` (si se atiende); 1:N con `Pago`, `Comprobante` |
| **Atencion** | Acto médico principal, agregador de módulos clínicos y financieros | 1:1 con `Anamnesis`, `ImpresionDiagnostica`; 1:N con `ExamenFisico`, `TactoVaginal`, `EcografiaObstetrica`, `Pago`, `Comprobante` |
| **Pago** | Transacción financiera que liquida total o parcialmente el costo de una cita/atención | 1:N con `Comprobante`, `AjusteFinanciero` |
| **Comprobante** | Documento emitido (boleta, constancia, resumen, estado de cuenta). Inmutable una vez emitido | 1:N con `ComprobanteDetalle` |
| **HistorialClinico** | Línea de tiempo de eventos del paciente | 1:N con `HistorialDetalle` |
| **Auditoria** | Registro de acciones significativas del sistema | N:1 con `Usuario` |
| **HorarioDoctor** | Franjas horarias de atención por día y vigencia | N:1 con `Doctor` |
| **AjusteFinanciero** | Correcciones sobre pagos (descuentos, recargos, etc.) | N:1 con `Pago` y `Atencion` (opcional) |
| **Chat / MensajeChat** | Comunicación por WhatsApp con pacientes | N:1 con `Paciente` (opcional) |
| **NotificacionCita** | Recordatorios automáticos de citas vía WhatsApp | N:1 con `Cita` y `Paciente` |

---

## 📊 Diccionario de Enumeraciones (Enums)

El comportamiento del sistema se rige por máquinas de estado definidas como enumeraciones puras. A continuación se listan las más importantes, agrupadas por ámbito.

| Enumeración | Valores | Uso |
|-------------|---------|-----|
| **EstadoAtencion** | `Abierta`, `Cerrada`, `Anulada`, `Eliminada` | Ciclo de vida de la atención médica |
| **EstadoCita** | `Pendiente`, `Confirmada`, `Reprogramada`, `Cancelada`, `Atendida`, `NoAsistio`, `EnProgreso`, `Eliminada` | Estado de la agenda y notificaciones |
| **EstadoPago** | `Pendiente`, `Parcial`, `Pagado`, `Anulado`, `Reembolsado`, `Eliminado` | Estado financiero del pago |
| **EstadoComprobante** | `Emitido`, `Anulado` | Validez del documento generado |
| **EstadoDoctor** | `Activo`, `Inactivo`, `ContratoVencido`, `Suspendido`, `Eliminado` | Situación laboral del médico |
| **EstadoPaciente** | `Activo`, `Inactivo`, `Fallecido`, `Bloqueado`, `Eliminado` | Estado del paciente en el sistema |
| **EstadoUsuario** | `Activo`, `Inactivo`, `Bloqueado`, `Eliminado` | Estado de la cuenta de acceso |
| **EstadoHistorialClinico** | `Activo`, `Cerrado`, `Archivado`, `Eliminado` | Estado del historial del paciente |
| **EstadoServicioClinico** | `Activo`, `Inactivo`, `Eliminado` | Disponibilidad del servicio |
| **MetodoPago** | `Efectivo`, `Yape`, `Plin`, `Transferencia`, `Tarjeta`, `Otro` | Clasificación para arqueos de caja |
| **TipoComprobante** | `BoletaPago`, `ConstanciaCita`, `ResumenAtencion`, `EstadoCuenta`, `HistoriaClinica`, `ReporteCajaDiario`, `ReporteFinancieroMensual`, `AjusteFinanciero` | Tipo de documento emitido |
| **TipoFormatoImpresion** | `A4`, `MediaHoja`, `Ticket80mm` | Formato de impresión |
| **TipoDocumentoComprobante** | `DNI`, `CarnetExtranjeria`, `RUC`, `Pasaporte`, `SinDocumento` | Documento de identidad del paciente |
| **TasaImpuesto** | `IGV_18`, `IGV_19`, `IGV_20`, `Exonerado` | Tasa aplicable al comprobante |
| **TipoAjusteFinanciero** | `Descuento`, `Recargo`, `Sobrepago`, `CorreccionMonto`, `AnulacionPago`, `Reembolso`, `PagoMenorAutorizado`, `PagoMayorAutorizado`, `ServicioAdicional`, `IndicacionMedica`, `ErrorAdministrativoCorregido`, `Otro` | Motivo del ajuste financiero |
| **NivelAuditoria** | `Normal`, `Importante`, `Critico` | Severidad del registro de auditoría |
| **TipoAccionAuditoria** | `Consulta`, `Creacion`, `Edicion`, `Eliminacion`, `Login`, `Asignacion`, `Error` | Tipo de acción registrada |
| **TipoMovimientoHistorial** | `RegistroUsuario`, `AperturaHistorial`, `CitaProgramada`, `CitaReprogramada`, `CitaCancelada`, `CitaAtendida`, `AtencionRegistrada`, `AtencionCerrada`, `PagoRegistrado`, `PagoParcial`, `PagoCompletado`, `SeguimientoRegistrado`, `ObservacionClinica`, `ActualizacionDatosPaciente`, `EliminacionLogica` | Eventos del historial clínico |

---

## 🩺 Módulos Clínicos de la Atención

La entidad `Atencion` actúa como agregador de submódulos clínicos especializados. Cada uno de estos submódulos se encuentra en el namespace `Clinica.Domain.Entities.ATENCIONES`.

=== "Anamnesis"
    Almacena el motivo de consulta y la **Fórmula Obstétrica**:
    - `MotivoConsulta` (string, obligatorio)
    - `Gestaciones`, `HijosVivos`, `Abortos`, `PartosPretermino`, `PartosATermino` (int)
    - `FechaUltimaRegla`, `FechaProbableParto` (DateTime? )
    - `EdadGestacional` (string, opcional)
    - `Alergias`, `EnfermedadesCronicas`, `CirugiasPrevias`, `AntecedentesAdicionales` (string? )

=== "Examen Físico"
    Registro evolutivo del estado materno-fetal:
    - `Lotep` (bool), `EstadoGeneral`, `EstadoHidratacion`, `EstadoNutricion` (string? )
    - `EscalaGlasgow` (int? )
    - `UteroGravido` (bool), `AlturaUterina` (int? )
    - `SituacionPosicionPresentacion`, `MovimientosFetales`, `TonoUterino`, `DinamicaUterina` (string? )
    - `LatidosCardiacosFetales` (int? )
    - `SangradoTv`, `PerdidaLiquidoAmniotico`, `TaponMucoso`, `FlujoVaginal` (bool)
    - `ColorLiquidoAmniotico`, `PunoPercusionLumbar`, `Edemas`, `ReflejosOsteotendinosos` (string? )

=== "Tacto Vaginal"
    Exploración pélvica para el seguimiento del trabajo de parto:
    - `Dilatacion` (int? cm)
    - `Borramiento` (int? %)
    - `AlturaPresentacion` (string? )
    - `MembranasOvulares`, `ColorLiquido`, `Pelvis`, `VariedadPresentacion` (string? )

=== "Ecografía Obstétrica"
    Biometría fetal y evaluación placentaria:
    - `DiametroBiparietal`, `CircunferenciaCefalica`, `CircunferenciaAbdominal`, `LongitudFemur` (int? mm)
    - `PesoFetalEstimado` (int? grs)
    - `IndiceLiquidoAmniotico` (decimal? )
    - `PlacentaLocalizacion`, `PlacentaGranum` (string? )
    - `CircularCordon` (bool)
    - `Conclusiones` (string? )

=== "Impresión Diagnóstica"
    Diagnóstico y plan de manejo:
    - `DiagnosticoPrincipal` (string, obligatorio)
    - `DiagnosticosSecundarios` (string? )
    - `IndicacionesReceta` (string, obligatorio)
    - `FechaProximaCita` (DateTime? )
    - `MotivoProximaCita` (string? )

---

## 🔌 Puertos (Interfaces)

La capa de dominio declara los **puertos** que serán implementados por las capas superiores. Se dividen en **puertos de salida** (repositorios) y **puertos de entrada** (servicios de aplicación). Todos ellos se encuentran en `Clinica.Domain.Interfaces`.

### Repositorios (Puertos de Salida)

Cada repositorio extiende `IGenericRepository<T>` que proporciona operaciones CRUD básicas (`GetAllAsync`, `GetByIdAsync`, `AddAsync`, `Update`, `Delete`, `SaveChangesAsync`). A continuación se detallan los repositorios con sus métodos adicionales específicos.

=== "Paciente"
    ```csharp
    public interface IPacienteRepository : IGenericRepository<Paciente>
    {
        Task<Paciente?> ObtenerPorDniAsync(string dni);
        Task<Paciente?> ObtenerConHistorialAsync(Guid pacienteId);
        Task<IEnumerable<Paciente>> ObtenerTodosConHistorialAsync();
    }
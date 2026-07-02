# 🟣 Capa de Dominio (Core) - SYS Clínica Santa Mónica

La **Capa de Dominio** constituye el corazón atómico e inmutable de **SYS Clínica Santa Mónica**. Diseñada bajo los fundamentos del **Diseño Guiado por el Dominio (DDD)** y completamente aislada dentro de la **Arquitectura Hexagonal**, esta capa es 100% agnóstica a cualquier framework de persistencia, motores de base de datos, protocolos de transporte HTTP o librerías de interfaz de usuario. 

Su única y más alta responsabilidad es salvaguardar el modelo conceptual del negocio ginecobstétrico y de recaudación financiera de la institución, abstrayendo las complejas realidades de la atención médica en la región de Juliaca (Puno) en reglas de software puras, coherentes y fuertemente tipadas.



!!! info "Frontera Arquitectónica e Inversión de Dependencias"
    - **Dependencias PROHIBIDAS:** Entity Framework Core, ASP.NET Core, Swagger, pasarelas de pago externas, JSON serialization, logging de terceros, o cualquier referencia a la capa de Aplicación o Infraestructura. Es el componente más interno de la solución.
    - **Dependencias PERMITIDAS:** Exclusivamente la librería estándar de .NET (`System`, `System.Collections.Generic`, `System.Linq`, `System.ComponentModel.DataAnnotations` únicamente para anotaciones nativas de metadatos de validación).
    - **Aislamiento Legal y Clínico:** Este núcleo encapsula las reglas críticas de la historia clínica electrónica y el consentimiento informado. Está protegido contra cambios tecnológicos externos, garantizando el cumplimiento ininterrumpido de la **Ley N° 29733 (Ley de Protección de Datos Personales en el Perú)** y los estándares técnicos de archivo clínico digital establecidos por el **MINSA**.

---

## 🏛️ Entidades de Dominio (Core Entities)

Las entidades de dominio modelan los conceptos tangibles e intangibles de la operación asistencial y administrativa de la clínica. A diferencia de un modelo plano de base de datos, estas estructuras imponen la integridad de los flujos del mundo real:

| Entidad | Propósito Operativo en la Clínica | Impacto en el Negocio / Cumplimiento | Relaciones Principales |
| :--- | :--- | :--- | :--- |
| **Paciente** | Expediente maestro demográfico e identitario de la gestante o paciente regular. | **Ley N° 29733:** Almacena el módulo de filiación extendido (ocupación, religión, datos de la pareja de soporte) exigido por el MINSA. | `1:1` HistorialClinico; `1:N` Cita, Atencion, Pago, Comprobante |
| **Doctor** | Registro del staff médico obstétrico colegiado autorizado para realizar actos médicos. | **Control SUSALUD:** Valida de forma estricta la vigencia de la colegiatura (CMP) y la especialidad antes de permitir firmas. | `1:N` HorarioDoctor, Cita, Atencion |
| **Atencion** | Agregador raíz (*Aggregate Root*) que consolida y fecha cronológicamente un acto médico obstétrico. | **Seguridad del Paciente:** Centraliza la anamnesis, exámenes físicos evolutivos y resoluciones diagnósticas. Bloqueado tras su cierre. | `1:1` Anamnesis, ImpresionDiagnostica; `1:N` ExamenFisico, TactoVaginal, EcografiaObstetrica, Pago |
| **Pago** | Entidad de control financiero encargada de auditar la caja de recaudación diaria. | **Auditoría Interna:** Controla deudas reales cruzando adelantos de separación, montos cobrados y saldos pendientes por liquidar. | `1:N` Comprobante, AjusteFinanciero |
| **Comprobante** | Documento tributario o clínico emitido de forma definitiva e inalterable. | **Normativa SUNAT:** Representa legalmente boletas de pago, constancias de asistencia, estados de cuenta o resúmenes de egreso. | `1:N` ComprobanteDetalle |
| **Cita** | Reserva formal de una franja de tiempo en la agenda médica de los consultorios físicos. | **Mitigación de Ausentismo:** Actúa como el disparador nativo para el motor asíncrono de recordatorios de WhatsApp. | `1:1` Atencion; `1:N` Pago, NotificacionCita |
| **HistorialClinico** | Carpeta médica federada de la paciente, organizada cronológicamente en una línea de tiempo. | **Migración Histórica:** Diseñado para absorber y estructurar las más de 5,000 fichas físicas heredadas del formato en papel. | `1:N` HistorialDetalle |
| **AjusteFinanciero** | Nota de corrección contable legal aplicada sobre transacciones de caja liquidadas. | **Prevención de Fraudes:** Registra y exige justificación detallada para descuentos, sobrepagos o errores administrativos de caja. | `N:1` Pago, Atencion |
| **Auditoria** | Registro forense no modificable de acciones significativas ejecutadas en la plataforma. | **Transparencia Total:** Captura instantáneas de estados (`ValorAnterior` y `ValorNuevo`) ante alteraciones de datos clínicos. | `N:1` Usuario |
| **Usuario** | Cuenta de credenciales y perfil de acceso digital de los colaboradores de la clínica. | **Seguridad RBAC:** Vincula al trabajador con sus correspondientes roles y tokens atómicos de autorización. | `1:N` UsuarioRol, Auditoria |
| **HorarioDoctor** | Parametrización semanal de los turnos de atención física de los médicos especialistas. | **Eficiencia Operativa:** Bloquea la sobreposición de turnos en salas de ecografía y consultorios obstétricos compartidos. | `N:1` Doctor |
| **NotificacionCita**| Bitácora de seguimiento de mensajes automáticos despachados hacia las pacientes. | **Integración Externa:** Monitorea el estado de entrega de las alertas generadas por la pasarela de Evolution API. | `N:1` Cita, Paciente |

---

## 📊 Diccionario de Enumeraciones (Enums)

El comportamiento lógico del software, el flujo de las transacciones financieras y las transiciones del estado de salud de las pacientes están gobernados de forma estricta por máquinas de estado modeladas mediante enumeraciones puras. Esto previene la inyección de estados corruptos o inválidos en los canales de la API:

### Gestión de Procesos Asistenciales

=== "EstadoAtencion"
    Controla el ciclo de vida del acto médico y la inmutabilidad de los registros clínicos:
    * `Abierta`: El médico u obstetra está digitando los hallazgos clínicos en el consultorio. Modificable.
    * `Cerrada`: Consulta finalizada. El registro se firma digitalmente y se bloquea para resguardar la validez legal.
    * `Anulada`: El acto médico se revoca por un criterio técnico justificado (ej: error en la apertura de la sesión).
    * `Eliminada`: Borrado lógico administrativo del registro del sistema.

=== "EstadoCita"
    Gobierna el flujo de la agenda de la clínica y las alertas automatizadas de WhatsApp:
    * `Pendiente`: Cita agendada por la recepción pero sin confirmación activa de asistencia.
    * `Confirmada`: La paciente ha ratificado su asistencia a través del canal interactivo de mensajería.
    * `EnProgreso`: La paciente ha pasado el triaje y se encuentra en sala de espera o atención actual.
    * `Atendida`: El acto médico de la cita ha sido completado y cerrado de forma exitosa.
    * `Reprogramada`: La reserva fue trasladada a una nueva fecha y hora por interferencia o solicitud de la paciente.
    * `Cancelada`: Cita revocada antes de la fecha asignada. Libera automáticamente el consultorio físico.
    * `NoAsistio`: La paciente faltó a su cita. Dispara una alerta de proactividad para agendamiento de control.
    * `Eliminada`: Retiro definitivo de la agenda por cancelación logística interna.

### Gestión de Procesos Financieros y Fiscales

=== "EstadoPago"
    Rige la auditoría contable y el balance de deudas cruzadas en la caja de recaudación:
    * `Pendiente`: El servicio clínico ha sido reservado u ordenado pero no cuenta con abonos monetarios.
    * `Parcial`: La paciente ha dejado un adelanto preventivo de separación o una cuota inicial (Saldo Pendiente activo).
    * `Pagado`: Transacción liquidada al 100%. El dinero ha ingresado formalmente a las arcas de la clínica.
    * `Anulado`: Operación de caja revocada antes del cierre diario por error de digitación o cancelación.
    * `Reembolso`: Devolución parcial o total de fondos justificada formalmente mediante un ajuste financiero.
    * `Eliminado`: Depuración lógica del registro contable.

=== "MetodoPago"
    Clasifica las vías de ingreso de capital para las conciliaciones bancarias y los arqueos de caja diarios:
    * `Efectivo`: Dinero físico recibido directamente en la ventanilla de recepción de la clínica.
    * `Yape`: Transferencia digital inmediata mediante código QR o número telefónico (Banca Móvil BCP).
    * `Plin`: Transferencia digital interbancaria inmediata (BBVA, Interbank, Scotiabank, BanBif).
    * `Transferencia`: Depósito directo en las cuentas corrientes institucionales de la clínica (operaciones mayores).
    * `Tarjeta`: Pago procesado en ventanilla mediante terminales POS de débito o crédito (Visa, Mastercard).
    * `Otro`: Métodos alternativos autorizados de manera excepcional por la gerencia.

=== "TipoComprobante"
    Determina la naturaleza legal del documento impreso o generado digitalmente por el sistema:
    * `BoletaPago`: Comprobante de venta oficial emitido al consumidor final bajo regulaciones de la SUNAT.
    * `ConstanciaCita`: Documento administrativo emitido como justificante laboral o de asistencia para la paciente.
    * `ResumenAtencion`: Epicrisis simplificada que recopila el motivo de consulta, diagnósticos e indicaciones de receta.
    * `EstadoCuenta`: Ficha financiera consolidada que expone los cobros, abonos realizados y saldos de deuda reales.
    * `HistoriaClinica`: Exportación legal en PDF de la ficha de filiación completa y evoluciones médicas.
    * `ReporteCajaDiario`: Documento de cuadre e ingresos netos consolidado por el cajero al final del turno.
    * `ReporteFinancieroMensual`: Balance general de recaudación e impuestos diseñado para la contabilidad externa.
    * `AjusteFinanciero`: Sustento formal de la emisión de notas de crédito o débito internas del sistema.

### Control de Seguridad y Auditoría

=== "NivelAuditoria"
    Establece la severidad de las acciones interceptadas por los filtros de seguridad de la API:
    * `Normal`: Operaciones cotidianas de lectura de datos o búsquedas predictivas en el sistema.
    * `Importante`: Acciones de escritura o modificación (creación de pacientes, apertura de atenciones, emisión de boletas).
    * `Critico`: Operaciones de alto impacto de seguridad o financiero (login fallido repetido, anulación de comprobantes, alteración de roles).

=== "TipoAccionAuditoria"
    Clasifica la naturaleza técnica de la transacción interceptada en el pipeline:
    * `Consulta` | `Creacion` | `Edicion` | `Eliminacion` | `Login` | `Asignacion` | `Error`

---

## 🩺 Módulos Clínicos Obstétricos de la Atención

La entidad **Atencion** actúa como un Agregador Raíz (*Aggregate Root*). Bajo las normativas del MINSA para la atención materno-infantil, los datos médicos no se estructuran de forma plana, sino que se segregan en submódulos especializados que capturan la evolución biométrica y física de la gestante:



=== "Anamnesis"
    Captura los antecedentes médicos estructurales de la paciente y calcula la **Fórmula Obstétrica** obligatoria para el triaje:
    
    * `MotivoConsulta` *(String, Obligatorio)*: Descripción detallada de los síntomas o el objetivo del control.
    * **Fórmula Gravídica:** `Gestaciones`, `PartosATermino`, `PartosPretermino`, `Abortos`, `HijosVivos` *(Int)*: Indicadores indispensables para la clasificación de riesgo obstétrico.
    * **Fechas de Control:** `FechaUltimaRegla` (FUR), `FechaProbableParto` (FPP) *(DateTime?)*.
    * `EdadGestacional` *(String)*: Descripción precisa calculada de las semanas de gestación (ej: "28.4 semanas").
    * `Alergias`, `EnfermedadesCronicas`, `CirugiasPrevias`, `AntecedentesAdicionales` *(String?)*: Alertas médicas críticas.

=== "Examen Físico"
    Monitoreo continuo de las funciones vitales maternas y los indicadores de desarrollo fetal:
    
    * `Lotep` *(Bool)*: Estado neurológico de la paciente (Lúcida, Orientada en Tiempo, Espacio y Persona).
    * `EstadoGeneral`, `EstadoHidratacion`, `EstadoNutricion` *(String?)*: Evaluaciones cualitativas médicas.
    * `EscalaGlasgow` *(Int?)*: Control neurológico cuantitativo en escenarios de emergencia (ej: preeclampsia severa).
    * `UteroGravido` *(Bool)* | `AlturaUterina` *(Int?)*: Medición física en centímetros del crecimiento uterino.
    * `LatidosCardiacosFetales` *(Int?)*: Frecuencia cardiaca del feto medida en latidos por minuto (lpm).
    * `SituacionPosicionPresentacion` *(String?)*: Orientación anatómica del feto (ej: Longitudinal / Izquierda / Cefálica).
    * `DinamicaUterina`, `TonoUterino`, `MovimientosFetales` *(String?)*: Monitoreo de contracciones y bienestar.
    * **Signos de Alerta Extremos:** `SangradoTv`, `PerdidaLiquidoAmniotico`, `TaponMucoso`, `FlujoVaginal` *(Bool)*: Interruptores de seguridad que disparan advertencias visuales de color rojo en la interfaz Blazor.
    * `ColorLiquidoAmniotico`, `PunoPercusionLumbar` (PPL), `Edemas`, `ReflejosOsteotendinosos` *(String?)*: Descarte de infecciones o síndromes hipertensivos del embarazo.

=== "Tacto Vaginal"
    Evaluación física cronológica ejecutada durante la fase de control de labor de parto:
    
    * `Dilatacion` *(Int?)*: Apertura del cuello uterino medida estrictamente en centímetros (rango 0 a 10 cm).
    * `Borramiento` *(Int?)*: Adelgazamiento del cuello cervical expresado en porcentaje (0% a 100%).
    * `AlturaPresentacion` *(String?)*: Ubicación de la cabeza fetal respecto a la pelvis materna (Estaciones de De Lee o Planos de Hodge).
    * `MembranasOvulares` *(String?)*: Clasificación clínica de las membranas (Integras o Rotas).
    * `ColorLiquido`, `Pelvis`, `VariedadPresentacion` *(String?)*: Evaluación de compatibilidad cefalópélvica y posicionamiento fetal (ej: OIA).

=== "Ecografía Obstétrica"
    Consolida los reportes e indicadores obtenidos mediante los ultrasonidos realizados en la clínica:
    
    * **Biometría Fetal (mm):** `DiametroBiparietal` (DBP), `CircunferenciaCefalica` (CC), `CircunferenciaAbdominal` (CA), `LongitudFemur` (LF) *(Int?)*.
    * `PesoFetalEstimado` *(Int?)*: Peso calculado del feto expresado en gramos.
    * `IndiceLiquidoAmniotico` *(Decimal?)*: Evaluación del volumen de líquido amniótico (Métrica ILA).
    * `PlacentaLocalizacion`, `PlacentaGranum` *(String?)*: Ubicación e índice de madurez placentaria (Escala de Grannum 0-III).
    * `CircularCordon` *(Bool)*: Indicador crítico de alerta si el cordón umbilical se encuentra rodeando el cuello del feto.
    * `Conclusiones` *(String?)*: Dictamen final diagnóstico del ecografista.

=== "Partograma"
    Herramienta gráfica cronológica integrada en el Dominio para el seguimiento del trabajo de parto en tiempo real. Modela de forma matricial las variaciones temporales combinadas de las funciones vitales de la madre y el descenso fetal:
    
    * `ControlVital`: Estructura interna encargada de capturar la Presión Arterial (PA), Pulso, Temperatura y Frecuencia Respiratoria en intervalos de tiempo fijos.
    * `RegistroPartograma`: Línea secuencial que indexa por horas la relación directa entre la curva de dilatación cervical, la estática de la presentación, la intensidad de la dinámica uterina y los latidos cardiacos fetales (LCF).

---

## 🔌 Puertos de la Arquitectura (Interfaces)

La capa de dominio declara los **puertos** de comunicación que definen las reglas de intercambio con el exterior. Se dividen rigurosamente en **puertos de salida** (interfaces de repositorio que implementará la Infraestructura) y **puertos de entrada** (interfaces de servicios que implementará la Aplicación).

### Repositorios Core (Puertos de Salida)

Todos los repositorios del sistema extienden la interfaz base `IGenericRepository<T>`, la cual encapsula las operaciones asíncronas CRUD tradicionales: `GetAllAsync()`, `GetByIdAsync(id)`, `AddAsync(entity)`, `Update(entity)`, `Delete(entity)` y `SaveChangesAsync()`.

A continuación se exponen las firmas de los puertos especializados encargados de forzar las reglas complejas de negocio:

```csharp
namespace Clinica.Domain.Interfaces;

public interface IPacienteRepository : IGenericRepository<Paciente>
{
    // Búsqueda directa por identidad nacional. Soporta la validación anti-duplicados en la recepción.
    Task<Paciente?> ObtenerPorDniAsync(string dni);
    
    // Recupera la paciente inyectando de forma atómica su Historial Clínico completo para la vista de la ficha médica.
    Task<Paciente?> ObtenerConHistorialAsync(Guid pacienteId);
    
    Task<IEnumerable<Paciente>> ObtenerTodosConHistorialAsync();
}

public interface IAtencionRepository : IGenericRepository<Atencion>
{
    // Carga el agregado completo de la atención médica, incluyendo la totalidad de sus submódulos clínicos obstétricos 
    // (Anamnesis, Exámenes Físicos, Tactos, Ecografías y Diagnósticos) en una única consulta indexada.
    Task<Atencion?> ObtenerDetalleCompletoAsync(Guid id);
}

public interface ICitaRepository : IGenericRepository<Cita>
{
    // Regla de Negocio Crítica: Evalúa si existe superposición de horarios para un médico específico en una fecha determinada.
    // Evita la sobreposición de turnos en las salas de ecografía de la clínica en Juliaca.
    Task<bool> ExisteInterferenciaHorarioAsync(Guid doctorId, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin, Guid? citaIdExcluir = null);
}

public interface IComprobanteRepository : IGenericRepository<Comprobante>
{
    // Recupera el último número correlativo emitido en la clínica para una serie específica (ej: B001). 
    // Indispensable para mantener la secuencia numérica exigida por la SUNAT.
    Task<int> ObtenerUltimoNumeroPorSerieAsync(string serie);
}

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    // Resuelve la autenticación del sistema cargando el grafo completo de Roles, Relaciones y Permisos atómicos del colaborador.
    Task<Usuario?> ObtenerPorUserNameAsync(string userName);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
}
```
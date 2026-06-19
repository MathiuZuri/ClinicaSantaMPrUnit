# language: es
Característica: Gestión de Citas Médicas
    Como usuario administrativo del sistema SIGEC
    Quiero programar, reprogramar y cancelar citas obstétricas
    Para mantener la agenda del Centro Materno Perinatal permanentemente actualizada

Antecedentes:
    Dado que el usuario ha iniciado sesión como administrador
    Y navega a la página de citas "/panel/citas"

Escenario: Programar una nueva cita exitosamente
    Cuando hace clic en el botón "Nueva cita"
    Y selecciona el paciente "Maria Lopez Quispe"
    Y selecciona el especialista "Carlos Ramirez"
    Y selecciona el servicio clínico "Consulta obstétrica"
    Y configura una fecha y hora disponibles sin conflictos
    Y escribe en el motivo clínico "Control prenatal de rutina."
    Y hace clic en el botón "Programar cita"
    Entonces el sistema muestra una alerta de éxito con el mensaje "Cita programada"
    Y la grilla de citas muestra una cita para el paciente "Maria Lopez Quispe" con estado "Pendiente"

Escenario: Validación de campos obligatorios al crear cita
    Cuando hace clic en el botón "Nueva cita"
    Y hace clic directamente en el botón "Programar cita" sin llenar los campos
    Entonces el formulario muestra errores de validación para los campos requeridos

Escenario: Reprogramar una cita existente
    Dado que se ha creado una cita de prueba para el paciente "Maria Lopez Quispe"
    Y se ha obtenido el código de la cita generada
    Cuando hace clic en el botón "Reprogramar Bloque" de la cita capturada
    Y configura la nueva fecha "16/06/2030"
    Y configura el nuevo horario de inicio "09:00" y fin "09:30"
    Y escribe el motivo de reprogramación "Ajuste de agenda operativa por emergencia en sala de partos."
    Y hace clic en el botón "Reprogramar"
    Entonces el sistema muestra una alerta de éxito con el mensaje "Cita reprogramada"
    Y la grilla de citas muestra la cita capturada con estado "Reprogramada" y el nuevo horario "09:00 - 09:30"

Escenario: Cancelar una cita vigente
    Dado que se ha creado una cita de prueba para el paciente "Maria Lopez Quispe"
    Y se ha obtenido el código de la cita generada
    Cuando hace clic en el botón "Anular Ticket" de la cita capturada
    Y escribe el motivo de cancelación "Paciente solicita desistir de la consulta por motivos laborales de fuerza mayor."
    Y hace clic en el botón "Confirmar cancelación"
    Entonces el sistema muestra una alerta de éxito con el mensaje "Cita cancelada"
    Y la grilla de citas muestra la cita capturada con estado "Cancelada" y sin acciones de reprogramación o anulación
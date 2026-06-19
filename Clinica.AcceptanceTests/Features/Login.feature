# language: es
Característica: Autenticación de Usuarios (Login)
Como usuario administrativo del sistema SIGEC
Quiero ingresar mis credenciales de acceso a la intranet
Para poder interactuar con los módulos protegidos de la clínica

Antecedentes:
    Dado que el usuario navega a la página de inicio de sesión

Escenario: Inicio de sesión exitoso con credenciales de administrador
    Cuando ingresa el usuario o correo "admin"
    Y digita la contraseña "admin123"
    Y hace clic en el botón principal "Ingresar a Intranet"
    Entonces el sistema debe redirigirlo automáticamente al panel principal "dashboard"

Escenario: Inicio de sesión fallido con credenciales incorrectas
    Cuando ingresa el usuario o correo "usuario_invalido"
    Y digita la contraseña "clave_incorrecta"
    Y hace clic en el botón principal "Ingresar a Intranet"
    Entonces el sistema debe mostrar un mensaje de alerta con el texto "Usuario o contraseña incorrectos."
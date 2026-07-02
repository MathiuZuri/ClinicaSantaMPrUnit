# 🧪 Capa de Pruebas (Unitarias e Integración)

La **Capa de Pruebas** es un pilar fundamental dentro del ecosistema de **SYS Clínica Santa Mónica**, el sistema de gestión clínica desarrollado a medida para la Clínica Santa Mónica en Juliaca. En un contexto donde la institución apuesta por la transformación digital para ofrecer una atención materna integral, segura y eficiente, la calidad del software no es un lujo sino una exigencia operativa. Cada línea de código que gestiona citas, historias clínicas, recordatorios por WhatsApp o facturación electrónica debe ser confiable, robusta y estar libre de errores que puedan comprometer la seguridad de los datos clínicos o la experiencia de las pacientes.

Por ello, el proyecto adopta una estrategia de testing en dos niveles: **pruebas unitarias** y **pruebas de integración**, organizadas en proyectos independientes que reflejan la arquitectura hexagonal del sistema. Este enfoque garantiza que cada componente —desde las reglas de negocio en el dominio hasta los controladores de la API— sea verificado de forma aislada o en interacción con sus dependencias reales, alineándose con los valores institucionales de **responsabilidad, ética profesional y mejora continua**.

!!! info "Frontera Arquitectónica"
    - **Pruebas Unitarias (`Clinica.API.Tests`):** Aíslan la lógica de negocio y orquestación, utilizando mocks y stubs para simular dependencias externas. No dependen de infraestructura real (base de datos, servicios externos).
    - **Pruebas de Integración (`Clinica.API.IntegrationTests`):** Validan la interacción entre componentes reales, utilizando una base de datos PostgreSQL en contenedor (Testcontainers) y el pipeline HTTP completo. Requieren autenticación y configuraciones reales.
    - **Cobertura de Código:** Ambos proyectos generan reportes de cobertura (Coverlet) que se integran con SonarQube para el análisis de calidad, en línea con el compromiso de la clínica con la transparencia y la mejora continua.

---

## 🏗️ Estructura de los Proyectos de Pruebas

### Clinica.API.Tests (Pruebas Unitarias)
Proyecto dedicado a las pruebas unitarias de la capa de API y de dominio. Su estructura refleja la organización del código fuente, aislando los controladores y servicios mediante la simulación dinámica de componentes. Estas pruebas se ejecutan en cada compilación, proporcionando retroalimentación inmediata a los desarrolladores y evitando la introducción de regresiones.

### Clinica.API.IntegrationTests (Pruebas de Integración)
Proyecto enfocado en validar el comportamiento del pipeline HTTP, el middleware de autenticación/autorización y la persistencia de datos relacionales frente a un entorno contenedorizado real. Se ejecutan con menor frecuencia (por ejemplo, en la pipeline de CI/CD) debido a su mayor duración, pero son esenciales para garantizar que los distintos módulos del sistema funcionen correctamente en conjunto, replicando el entorno de producción de la clínica.

---

## 🛠️ Matriz de Herramientas y Dependencias Técnicas

| Herramienta | Versión | Propósito Técnico |
| :--- | :--- | :--- |
| **xUnit** | 2.9.x | Framework de pruebas unitarias y de integración, ampliamente adoptado en el ecosistema .NET. |
| **NSubstitute** | 5.3.x | Creación de mocks y stubs dinámicos para pruebas unitarias, permitiendo simular dependencias externas sin complejidad. |
| **FluentAssertions** | 8.9.x | Sintaxis expresiva y legible para aserciones complejas, facilitando la lectura de los resultados de las pruebas. |
| **Microsoft.AspNetCore.Mvc.Testing** | 9.0.x | Hosting de la API en memoria para pruebas de integración de extremo a extremo, sin necesidad de un servidor real. |
| **Testcontainers.PostgreSql** | 4.8.x | Contenedor Docker efímero de PostgreSQL para entornos reales de pruebas, replicando fielmente la base de datos de producción. |
| **coverlet.collector** | 8.0.x | Recolección del porcentaje de cobertura de código para análisis estático, integrable con SonarQube. |
| **Microsoft.EntityFrameworkCore.InMemory** | 9.0.x | Base de datos virtualizada para escenarios unitarios controlados, utilizada en pruebas que no requieren un motor relacional completo. |

---

## 📋 Pruebas Unitarias (Clinica.API.Tests)

Las pruebas unitarias validan componentes individuales de forma aislada, reemplazando dependencias externas con mocks. Se organizan en tres categorías principales que cubren los aspectos críticos del sistema.

### 1. Pruebas de Controladores

Verifican que los controladores reciban solicitudes, validen autorización, llamen a los servicios correctos y retornen las respuestas HTTP adecuadas. Estas pruebas aseguran que la capa de presentación (API) actúe como un orquestador fiel, sin introducir lógica de negocio.

**Ejemplo: `AtencionesControllerTests`**

```csharp
[Fact]
public async Task ObtenerTodas_DebeRetornarOkConLista()
{
    // Arrange
    var lista = new List<AtencionResponseDto> { new() { Id = Guid.NewGuid() } };
    _atencionService.ObtenerTodasAsync().Returns(lista);

    // Act
    var resultado = await _controller.ObtenerTodas();

    // Assert
    var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
    var resp = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
    resp.Exitoso.Should().BeTrue();
    resp.Mensaje.Should().Be("Atenciones obtenidas correctamente.");
}

[Fact]
public async Task Registrar_Valido_RetornaCreated()
{
    // Arrange
    var dto = new RegistrarAtencionDto { /* ... */ };
    var nuevoId = Guid.NewGuid();
    _atencionService.RegistrarAtencionAsync(dto).Returns(nuevoId);

    // Act
    var resultado = await _controller.Registrar(dto);

    // Assert
    var created = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
    created.ActionName.Should().Be(nameof(AtencionesController.ObtenerPorId));
    created.RouteValues!["id"].Should().Be(nuevoId);
}
```
### 2. Pruebas de Servicios de Aplicación

Validan la lógica de orquestación, incluyendo validaciones de negocio, mapeos y persistencia a través de repositorios mockeados. Estas pruebas son cruciales porque los servicios contienen las reglas de negocio que materializan la propuesta de valor de la clínica (acompañamiento integral, registro de historias clínicas, gestión de citas, etc.).

**Ejemplo: `AtencionServiceTests`**

```csharp
[Fact]
public async Task RegistrarAtencionAsync_PacienteNoExiste_LanzaKeyNotFound()
{
    var dto = new RegistrarAtencionDto { /* ... */ };
    _pacienteRepo.GetByIdAsync(dto.PacienteId).Returns((Paciente?)null);

    Func<Task> act = () => _service.RegistrarAtencionAsync(dto);
    await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Paciente no encontrado.");
}

[Fact]
public async Task CerrarAtencionAsync_AtencionCerrada_LanzaInvalidOperation()
{
    var atencion = new Atencion { Id = id, Estado = EstadoAtencion.Cerrada };
    _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);

    Func<Task> act = () => _service.CerrarAtencionAsync(id, dto);
    await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("La atención ya está cerrada.");
}
```
### 3. Pruebas de Entidades de Dominio
Verifican el comportamiento de las entidades puras (constructores, propiedades, colecciones). Estas pruebas son las más ligeras y rápidas, y garantizan que el modelo de dominio —el corazón inmutable del sistema— se comporte según lo esperado.

**Ejemplo: `AuditoriaTests y AtencionTests`**

```csharp
[Fact]
public void ConstructorVacio_DebeInicializarValoresPorDefecto()
{
    var auditoria = new Auditoria();
    auditoria.Id.Should().NotBeEmpty();
    auditoria.TipoAccion.Should().Be(default);
    auditoria.FueExitoso.Should().BeTrue();
    auditoria.Nivel.Should().Be(NivelAuditoria.Normal);
    auditoria.EsConsulta.Should().BeFalse();
}

[Fact]
public void ConstructorVacio_DebeInicializarColecciones()
{
    var atencion = new Atencion();
    atencion.ExamenesFisicos.Should().NotBeNull().And.BeEmpty();
    atencion.TactosVaginales.Should().NotBeNull().And.BeEmpty();
    atencion.Ecografias.Should().NotBeNull().And.BeEmpty();
}
```

### 🔗 Pruebas de Integración (Clinica.API.IntegrationTests)
Las pruebas de integración validan el sistema en su conjunto, ejecutando la API real con una base de datos PostgreSQL en un contenedor Docker. Utilizan WebApplicationFactory para hospedar la API en memoria y Testcontainers para la base de datos, replicando el entorno de producción de la clínica.

## Configuración Base
PostgreSqlFixture: Inicia y detiene el contenedor PostgreSQL, y proporciona un contexto de base de datos para cada prueba. Esta configuración garantiza que las pruebas sean reproducibles y aisladas.

IntegrationTestBase: Clase base que:

Inicializa el cliente HTTP con autenticación.

Proporciona métodos para login como administrador.

Permite limpiar la base de datos entre pruebas.

TestDataSeeder: Utilidades para crear datos de prueba (pacientes, doctores, servicios, citas, atenciones, etc.), simulando el flujo real de la clínica.

**Ejemplo: `Pruebas del Módulo de Atenciones`**

```csharp
[Collection("IntegrationTests")]
public class AtencionesEndpointsTests : IntegrationTestBase
{
    public AtencionesEndpointsTests(PostgreSqlFixture postgreSqlFixture) : base(postgreSqlFixture) { }

    // Autenticación
    [Fact]
    public async Task Get_Atenciones_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var response = await Client.GetAsync("/api/atenciones");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Consulta exitosa
    [Fact]
    public async Task Get_Atenciones_ConAdmin_DeberiaRetornarOk()
    {
        await LoginAsAdminAsync();
        var response = await Client.GetAsync("/api/atenciones");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    // Creación exitosa
    [Fact]
    public async Task Post_Atenciones_Valida_DeberiaCrearAtencion()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseCita.Paciente.Id);

        var dto = new RegistrarAtencionDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HistorialClinicoId = historial.Id,
            CostoFinal = 150
        };

        var response = await Client.PostJsonAsync("/api/atenciones", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await JsonTestHelper.AssertSuccessAsync(response);
    }

    // Validación de errores
    [Fact]
    public async Task Post_Atenciones_SinHistorialClinico_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);

        var dto = new RegistrarAtencionDto
        {
            PacienteId = baseCita.Paciente.Id,
            DoctorId = baseCita.Doctor.Id,
            ServicioClinicoId = baseCita.Servicio.Id,
            HistorialClinicoId = null,
            CostoFinal = 100
        };

        var response = await Client.PostJsonAsync("/api/atenciones", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

### 📊 Reportes de Cobertura y Análisis de Calidad

Ambos proyectos generan reportes de cobertura utilizando `coverlet.collector`. Los archivos se almacenan en `TestResults/` y se integran con **SonarCloud** para el análisis de calidad, alineándose con la filosofía de **mejora continua** de la clínica. Los reportes incluyen:

- **Cobertura de líneas y ramas** – Mide el porcentaje de código ejecutado durante las pruebas.
- **Cobertura por clase y método** – Permite identificar qué componentes están mejor cubiertos y cuáles requieren atención.
- **Reportes HTML detallados** – Visualización interactiva que facilita la detección de áreas no cubiertas.


---

### 📝 Notas Adicionales

- **Aislamiento de Pruebas:** Cada prueba de integración se ejecuta en una transacción o se limpia después de la ejecución para evitar interferencias, garantizando que los datos de una prueba no afecten a otra. Esto asegura que los resultados sean reproducibles y confiables.

- **Velocidad:** Las pruebas unitarias son rápidas (~ms) mientras que las de integración tardan más (~segundos) debido al contenedor Docker. Por ello, se ejecutan en diferentes etapas del pipeline de CI/CD: las unitarias en cada compilación, y las de integración en fases previas al despliegue.

- **Ejecución en CI/CD:** Ambos conjuntos de pruebas se ejecutan en pipelines de Azure DevOps o GitHub Actions, generando reportes que se publican en SonarCloud. Esto cumple con el objetivo de la clínica de **transparencia y auditoría**, proporcionando evidencia objetiva de la calidad del software.

- **Manejo de Dependencias:** Se utilizan `Testcontainers` para evitar dependencias de infraestructura externa, garantizando que las pruebas sean reproducibles en cualquier entorno. Esto refuerza la **independencia tecnológica** que la clínica valora y permite ejecutar las pruebas de integración sin necesidad de una base de datos dedicada.

- **Cobertura Mínima:** El proyecto exige una cobertura mínima del **80%** para líneas y ramas, monitoreada por SonarCloud. Este umbral está en línea con el valor institucional de **calidad como estándar, no como aspiración**, y cualquier descenso por debajo de este nivel bloquea el despliegue a producción.

- **Pruebas de Integración vs Unitarias:** Las pruebas unitarias se ejecutan en cada compilación (feedback inmediato para el desarrollador), mientras que las de integración se ejecutan con menos frecuencia (por ejemplo, en la pipeline de CI/CD) debido a su mayor duración. Ambas son esenciales para mantener la robustez del sistema: las unitarias validan la lógica interna, y las de integración verifican que los componentes interactúen correctamente en un entorno realista.

!!! warning "Importante"
    La cobertura de código es una métrica orientativa, no un fin en sí mismo. El equipo prioriza la calidad de las aserciones y la relevancia de los escenarios probados sobre el simple porcentaje. Se fomenta la escritura de pruebas que cubran casos límite y flujos críticos (como la facturación electrónica y la auditoría de datos clínicos).
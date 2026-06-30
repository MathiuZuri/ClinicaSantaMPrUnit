# 🧪 Capa de Pruebas (Unitarias e Integración)

La **Capa de Pruebas** garantiza la calidad, estabilidad y correcto funcionamiento del sistema SIGEC a través de dos niveles de testing: **pruebas unitarias** y **pruebas de integración**. Ambas capas están organizadas en proyectos separados que reflejan la arquitectura hexagonal del sistema, asegurando que cada componente se pruebe de forma aislada o en conjunto con sus dependencias reales.

!!! info "Frontera Arquitectónica"
    * **Pruebas Unitarias (`Clinica.API.Tests`):** Aíslan la lógica de negocio y orquestación, utilizando mocks y stubs para simular dependencias externas. No dependen de infraestructura real (base de datos, servicios externos).
    * **Pruebas de Integración (`Clinica.API.IntegrationTests`):** Validan la interacción entre componentes reales, utilizando una base de datos PostgreSQL en contenedor (Testcontainers) y el pipeline HTTP completo. Requieren autenticación y configuraciones reales.
    * **Cobertura de Código:** Ambos proyectos generan reportes de cobertura (Coverlet) que se integran con SonarQube para el análisis de calidad.

---

## 🏗️ Estructura de los Proyectos de Pruebas

### Clinica.API.Tests (Pruebas Unitarias)
Proyecto dedicado a las pruebas unitarias de la capa de API y de dominio. Su estructura refleja la organización del código fuente, aislando los controladores y servicios mediante la simulación dinámica de componentes.

### Clinica.API.IntegrationTests (Pruebas de Integración)
Proyecto enfocado en validar el comportamiento del pipeline HTTP, el middleware de autenticación/autorización y la persistencia de datos relacionales frente a un entorno contenedorizado real.

---

## 🛠️ Herramientas y Dependencias

| Herramienta | Versión | Propósito Técnico |
| :--- | :--- | :--- |
| **xUnit** | 2.9.x | Framework de pruebas unitarias y de integración. |
| **NSubstitute** | 5.3.x | Creación de mocks y stubs dinámicos para pruebas unitarias. |
| **FluentAssertions** | 8.9.x | Sintaxis expresiva y legible para aserciones complejas. |
| **Microsoft.AspNetCore.Mvc.Testing** | 9.0.x | Hosting de la API en memoria para pruebas de integración de extremo a extremo. |
| **Testcontainers.PostgreSql** | 4.8.x | Contenedor Docker efímero de PostgreSQL para entornos reales de pruebas. |
| **coverlet.collector** | 8.0.x | Recolección del porcentaje de cobertura de código para análisis estático. |
| **Microsoft.EntityFrameworkCore.InMemory** | 9.0.x | Base de datos virtualizada para escenarios unitarios controlados. |

---

## 📋 Pruebas Unitarias (Clinica.API.Tests)

Las pruebas unitarias validan componentes individuales de forma aislada, reemplazando dependencias externas con mocks. Se organizan en tres categorías principales:

### 1. Pruebas de Controladores

Verifican que los controladores reciban solicitudes, validen autorización, llamen a los servicios correctos y retornen las respuestas HTTP adecuadas.

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
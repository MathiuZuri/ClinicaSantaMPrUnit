using Clinica.API.Services.Imp;
using Clinica.Domain.DTOs.Atenciones;
using Clinica.Domain.DTOs.Atenciones.Modulos;
using Clinica.Domain.Entities;
using Clinica.Domain.Entities.ATENCIONES;
using Clinica.Domain.Enums;
using Clinica.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Services;

public class AtencionServiceTests
{
    private readonly IAtencionRepository _atencionRepo;
    private readonly ICitaRepository _citaRepo;
    private readonly IPagoRepository _pagoRepo;
    private readonly IHistorialDetalleRepository _historialDetalleRepo;
    private readonly IUsuarioActualService _usuarioActual;
    private readonly IPacienteRepository _pacienteRepo;
    private readonly IServicioClinicoRepository _servicioRepo;
    private readonly AtencionService _service;

    public AtencionServiceTests()
    {
        _atencionRepo = Substitute.For<IAtencionRepository>();
        _citaRepo = Substitute.For<ICitaRepository>();
        _pagoRepo = Substitute.For<IPagoRepository>();
        _historialDetalleRepo = Substitute.For<IHistorialDetalleRepository>();
        _usuarioActual = Substitute.For<IUsuarioActualService>();
        _pacienteRepo = Substitute.For<IPacienteRepository>();
        _servicioRepo = Substitute.For<IServicioClinicoRepository>();

        _service = new AtencionService(
            _atencionRepo,
            _citaRepo,
            _pagoRepo,
            _historialDetalleRepo,
            _usuarioActual,
            _pacienteRepo,
            _servicioRepo);
    }

    [Fact]
    public async Task ObtenerTodasAsync_RetornaListaMapeada()
    {
        var atenciones = new List<Atencion>
        {
            new() { Id = Guid.NewGuid(), Paciente = new Paciente { Nombres = "A", Apellidos = "B" } }
        };
        _atencionRepo.ObtenerTodasConRelacionesAsync().Returns(atenciones);   // ✅ corregido

        var result = await _service.ObtenerTodasAsync();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_Existente_RetornaDto()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion { Id = id };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);

        var result = await _service.ObtenerPorIdAsync(id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_Inexistente_RetornaNull()
    {
        var id = Guid.NewGuid();
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns((Atencion?)null);

        var result = await _service.ObtenerPorIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegistrarAtencionAsync_PacienteNoExiste_LanzaKeyNotFound()
    {
        var dto = new RegistrarAtencionDto
        {
            PacienteId = Guid.NewGuid(),
            ServicioClinicoId = Guid.NewGuid(),
            HistorialClinicoId = Guid.NewGuid(),
            CostoFinal = 50
        };
        _pacienteRepo.GetByIdAsync(dto.PacienteId).Returns((Paciente?)null);

        Func<Task> act = () => _service.RegistrarAtencionAsync(dto);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Paciente no encontrado.");
    }

    [Fact]
    public async Task CerrarAtencionAsync_AtencionCerrada_LanzaInvalidOperation()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion { Id = id, Estado = EstadoAtencion.Cerrada };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);
        var dto = new CerrarAtencionDto { ImpresionDiagnostica = new() { DiagnosticoPrincipal = "X", IndicacionesReceta = "Y" } };

        Func<Task> act = () => _service.CerrarAtencionAsync(id, dto);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("La atención ya está cerrada.");
    }

    [Fact]
    public async Task AnularAtencionAsync_Cerrada_LanzaInvalidOperation()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion { Id = id, Estado = EstadoAtencion.Cerrada };
        _atencionRepo.GetByIdAsync(id).Returns(atencion);

        Func<Task> act = () => _service.AnularAtencionAsync(id, "motivo");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No se puede anular una atención cerrada.");
    }

    [Fact]
    public async Task AnularAtencionAsync_Abierta_CambiaEstadoYActualizaCita()
    {
        var id = Guid.NewGuid();
        var citaId = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = id,
            Estado = EstadoAtencion.Abierta,
            CitaId = citaId
        };
        var cita = new Cita { Id = citaId };
        _atencionRepo.GetByIdAsync(id).Returns(atencion);
        _citaRepo.GetByIdAsync(citaId).Returns(cita);

        await _service.AnularAtencionAsync(id, "motivo");

        atencion.Estado.Should().Be(EstadoAtencion.Anulada);
        cita.Estado.Should().Be(EstadoCita.Cancelada);
        _atencionRepo.Received().Update(atencion);
        await _atencionRepo.Received().SaveChangesAsync();
    }
    [Fact]
    public async Task ObtenerPorIdAsync_ExistenteConModulos_RetornaDtoCompleto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = id,
            Paciente = new Paciente { Nombres = "A", Apellidos = "B" },
            Doctor = new Doctor { Nombres = "C", Apellidos = "D", Especialidad = "Ginecología" },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" },
            Anamnesis = new Anamnesis
            {
                MotivoConsulta = "Dolor",
                Gestaciones = 2,
                HijosVivos = 1,
                Abortos = 0,
                PartosPretermino = 0,
                PartosATermino = 1
            },
            ExamenesFisicos = new List<ExamenFisico>
            {
                new ExamenFisico
                {
                    FechaHoraExamen = DateTime.UtcNow,
                    Lotep = true,
                    EstadoGeneral = "Bueno"
                }
            },
            TactosVaginales = new List<TactoVaginal>
            {
                new TactoVaginal
                {
                    FechaHora = DateTime.UtcNow,
                    Dilatacion = 3,
                    Borramiento = 50
                }
            },
            Ecografias = new List<EcografiaObstetrica>
            {
                new EcografiaObstetrica
                {
                    FechaHora = DateTime.UtcNow,
                    DiametroBiparietal = 90
                }
            },
            ImpresionDiagnostica = new ImpresionDiagnostica
            {
                DiagnosticoPrincipal = "Normal",
                IndicacionesReceta = "Reposo"
            }
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);

        // Act
        var result = await _service.ObtenerPorIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Anamnesis.Should().NotBeNull();
        result.Anamnesis!.MotivoConsulta.Should().Be("Dolor");
        result.ExamenesFisicos.Should().HaveCount(1);
        result.TactosVaginales.Should().HaveCount(1);
        result.Ecografias.Should().HaveCount(1);
        result.ImpresionDiagnostica.Should().NotBeNull();
    }
    
    [Fact]
    public async Task ObtenerPorIdAsync_PacienteNull_RetornaDtoConNombreVacio()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = id,
            Paciente = null!,
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor" },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" }
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);

        var result = await _service.ObtenerPorIdAsync(id);
        result!.PacienteNombre.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_DoctorNull_RetornaDoctorNombreVacio()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = id,
            Paciente = new Paciente { Nombres = "A", Apellidos = "B" },
            Doctor = null!,
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" }
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);

        var result = await _service.ObtenerPorIdAsync(id);
        result!.DoctorNombre.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ServicioClinicoNull_RetornaServicioNombreVacio()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = id,
            Paciente = new Paciente { Nombres = "A", Apellidos = "B" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor" },
            ServicioClinico = null!
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);

        var result = await _service.ObtenerPorIdAsync(id);
        result!.ServicioNombre.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_PagosNull_RetornaCostoCero()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = id,
            Paciente = new Paciente { Nombres = "A", Apellidos = "B" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor" },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" },
            Pagos = null!
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);

        var result = await _service.ObtenerPorIdAsync(id);
        result!.CostoFinal.Should().Be(0);
        result.MontoPagado.Should().Be(0);
        result.SaldoPendiente.Should().Be(0);
    }
    [Fact]
    public async Task RegistrarAtencionAsync_Valida_GeneraCodigosYRetornaId()
    {
        var usuarioId = Guid.NewGuid();
        var paciente = new Paciente { Id = Guid.NewGuid(), DNI = "12345678", Nombres = "A", Apellidos = "B" };
        var servicio = new ServicioClinico { Id = Guid.NewGuid(), CodigoServicio = "S1", Nombre = "Servicio" };
        var historialId = Guid.NewGuid();

        var dto = new RegistrarAtencionDto
        {
            PacienteId = paciente.Id,
            DoctorId = Guid.NewGuid(),
            ServicioClinicoId = servicio.Id,
            HistorialClinicoId = historialId,
            CostoFinal = 100,
            CitaId = Guid.NewGuid()
        };

        _usuarioActual.ObtenerUsuarioId().Returns(usuarioId);
        _pacienteRepo.GetByIdAsync(paciente.Id).Returns(paciente);
        _servicioRepo.GetByIdAsync(servicio.Id).Returns(servicio);
        _citaRepo.GetByIdAsync(dto.CitaId.Value).Returns(new Cita { Id = dto.CitaId.Value });

        var result = await _service.RegistrarAtencionAsync(dto);

        result.Should().NotBeEmpty();
        await _atencionRepo.Received().AddAsync(Arg.Any<Atencion>());
        await _pagoRepo.Received().AddAsync(Arg.Any<Pago>());
        await _historialDetalleRepo.Received().AddAsync(Arg.Any<HistorialDetalle>());
        await _atencionRepo.Received().SaveChangesAsync();
    }
    
    [Fact]
    public async Task CerrarAtencionAsync_ConObservacionesFinales_ActualizaDiagnostico()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = id,
            Estado = EstadoAtencion.Abierta,
            ImpresionDiagnostica = new ImpresionDiagnostica
            {
                Id = Guid.NewGuid(),
                DiagnosticoPrincipal = "DX previo",
                DiagnosticosSecundarios = null,
                IndicacionesReceta = "Receta previa"
            }
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);

        var dto = new CerrarAtencionDto
        {
            ImpresionDiagnostica = new ImpresionDiagnosticaDto
            {
                DiagnosticoPrincipal = "DX final",
                IndicacionesReceta = "Nueva receta"
            },
            ObservacionesFinales = "Observaciones adicionales"
        };

        await _service.CerrarAtencionAsync(id, dto);

        atencion.Estado.Should().Be(EstadoAtencion.Cerrada);
        atencion.ImpresionDiagnostica!.DiagnosticoPrincipal.Should().Be("DX final");
        atencion.ImpresionDiagnostica.DiagnosticosSecundarios.Should().Contain("OBSERVACIONES:");
        _atencionRepo.Received().Update(atencion);
        await _atencionRepo.Received().SaveChangesAsync();
    }
    [Fact]
    public async Task ObtenerPorIdAsync_DoctorConUsuarioNull_RetornaDoctorNombreVacio()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = id,
            Paciente = new Paciente { Nombres = "A", Apellidos = "B" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor", Usuario = null! },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" }
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);
        var result = await _service.ObtenerPorIdAsync(id);
        result!.DoctorNombre.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ColeccionesModulosNull_RetornaListasVacias()
    {
        var id = Guid.NewGuid();
        var atencion = new Atencion
        {
            Id = id,
            Paciente = new Paciente { Nombres = "A", Apellidos = "B" },
            Doctor = new Doctor { Nombres = "Doc", Apellidos = "Tor" },
            ServicioClinico = new ServicioClinico { Nombre = "Consulta" },
            ExamenesFisicos = null!,
            TactosVaginales = null!,
            Ecografias = null!
        };
        _atencionRepo.ObtenerDetalleCompletoAsync(id).Returns(atencion);
        var result = await _service.ObtenerPorIdAsync(id);
        result!.ExamenesFisicos.Should().BeEmpty();
        result.TactosVaginales.Should().BeEmpty();
        result.Ecografias.Should().BeEmpty();
    }
    
    
}
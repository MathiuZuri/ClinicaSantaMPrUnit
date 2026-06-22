using Clinica.API.Controllers;
using Clinica.API.Controllers.pdfControladores;
using Clinica.API.Services.Imp.ATENCIONES;
using Clinica.Domain.Interfaces;
using Clinica.Domain.Interfaces.ATENCIONES;
using Clinica.Domain.PDFsDto.Interfacespdf;
using NSubstitute;
using Xunit;

namespace Clinica.API.Tests.Test.Otros;

public class ControllersConstructorsTests
{
    [Fact]
    public void CrearControladoresPendientes_DebeInstanciarCorrectamente()
    {
        // AtencionesObstetricasController
        var _ = new AtencionesObstetricasController(
            Substitute.For<IAnamnesisService>(),
            Substitute.For<IExamenFisicoService>(),
            Substitute.For<ITactoVaginalService>(),
            Substitute.For<IEcografiaObstetricaService>(),
            Substitute.For<IImpresionDiagnosticaService>());

        // HistoriaClinicaController
        var __ = new HistoriaClinicaController(
            Substitute.For<IHistoriaClinicaPdfService>(),
            Substitute.For<IPacienteRepository>(),
            Substitute.For<IAtencionRepository>(),
            Substitute.For<IHistorialClinicoRepository>());

        // ReportesFinancierosController
        var ___ = new ReportesFinancierosController(
            Substitute.For<IReporteFinancieroPdfService>(),
            Substitute.For<IPagoRepository>());

        // ResumenPartoController
        var ____ = new ResumenPartoController(
            Substitute.For<IResumenPartoPdfService>(),
            Substitute.For<IAtencionRepository>(),
            Substitute.For<ITactoVaginalRepository>(),
            Substitute.For<IExamenFisicoRepository>());

        // Servicios de atención
        var anamnesis = new AnamnesisService(Substitute.For<IAnamnesisRepository>());
        var ecografia = new EcografiaObstetricaService(Substitute.For<IEcografiaObstetricaRepository>());
        var examen = new ExamenFisicoService(Substitute.For<IExamenFisicoRepository>());
        var diagnostico = new ImpresionDiagnosticaService(Substitute.For<IImpresionDiagnosticaRepository>());
        var tacto = new TactoVaginalService(Substitute.For<ITactoVaginalRepository>());

        // Verificación mínima para evitar advertencias de variables no usadas
        Assert.NotNull(anamnesis);
        Assert.NotNull(ecografia);
        Assert.NotNull(examen);
        Assert.NotNull(diagnostico);
        Assert.NotNull(tacto);
    }
}
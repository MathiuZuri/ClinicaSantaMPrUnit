using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Clinica.API.Controllers;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Comprobantes;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.IntegrationTests.PruebasInt.Comprobantes;

[Collection("IntegrationTests")]
public class ComprobantesEndpointsTests : IntegrationTestBase
{
    public ComprobantesEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    // ================================================================
    // AUTENTICACIÓN
    // ================================================================

    [Fact]
    public async Task Get_PreviewBoletaPago_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var response = await Client.GetAsync($"/api/comprobantes/preview/boleta-pago/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_EmitirBoletaPago_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var dto = new EmitirComprobantePagoDto { PagoId = Guid.NewGuid() };
        var response = await Client.PostJsonAsync("/api/comprobantes/emitir/boleta-pago", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Put_AnularComprobante_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();
        var request = new ComprobantesController.AnularComprobanteRequest { Motivo = "motivo" };
        var response = await Client.PutJsonAsync($"/api/comprobantes/{Guid.NewGuid()}/anular", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ================================================================
    // PREVIEWS
    // ================================================================

    [Fact]
    public async Task Get_PreviewBoletaPago_ConPagoExistente_DeberiaRetornarOk()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "10123456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, paciente.Id, servicio.Id, montoTotal: 100, montoPagado: 100);

        var response = await Client.GetAsync($"/api/comprobantes/preview/boleta-pago/{pago.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<ComprobantePagoPreviewDto>();
        preview!.PagoId.Should().Be(pago.Id);
    }

    [Fact]
    public async Task Get_PreviewConstanciaCita_ConCitaExistente_DeberiaRetornarOk()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var cita = await TestDataSeeder.CrearCitaAsync(db, baseCita.Paciente.Id, baseCita.Doctor.Id, baseCita.Servicio.Id, baseCita.Horario.Id);

        var response = await Client.GetAsync($"/api/comprobantes/preview/constancia-cita/{cita.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<ComprobanteCitaPreviewDto>();
        preview!.CitaId.Should().Be(cita.Id);
    }

    [Fact]
    public async Task Get_PreviewResumenAtencion_ConAtencionExistente_DeberiaRetornarOk()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseCita.Paciente.Id);
        var atencion = await TestDataSeeder.CrearAtencionAsync(db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            historialClinicoId: historial.Id);

        var response = await Client.GetAsync($"/api/comprobantes/preview/resumen-atencion/{atencion.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<ComprobanteAtencionPreviewDto>();
        preview!.AtencionId.Should().Be(atencion.Id);
    }

    [Fact]
    public async Task Get_PreviewEstadoCuentaPaciente_ConPacienteExistente_DeberiaRetornarOk()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "10223456");

        var response = await Client.GetAsync($"/api/comprobantes/preview/estado-cuenta/paciente/{paciente.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preview = await response.Content.ReadFromJsonAsync<ComprobanteEstadoCuentaPreviewDto>();
        preview!.PacienteId.Should().Be(paciente.Id);
    }

    // ================================================================
    // EMISIÓN
    // ================================================================

    [Fact]
    public async Task Post_EmitirBoletaPago_ConPagoExistente_DeberiaCrearComprobante()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "10323456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, paciente.Id, servicio.Id, montoTotal: 100, montoPagado: 100);

        var dto = new EmitirComprobantePagoDto { PagoId = pago.Id };
        var response = await Client.PostJsonAsync("/api/comprobantes/emitir/boleta-pago", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("mensaje").GetString().Should().Contain("Boleta de pago emitida");
        doc.RootElement.GetProperty("comprobanteId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Post_EmitirConstanciaCita_ConCitaExistente_DeberiaCrearComprobante()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var cita = await TestDataSeeder.CrearCitaAsync(db, baseCita.Paciente.Id, baseCita.Doctor.Id, baseCita.Servicio.Id, baseCita.Horario.Id);

        var dto = new EmitirComprobanteCitaDto { CitaId = cita.Id };
        var response = await Client.PostJsonAsync("/api/comprobantes/emitir/constancia-cita", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("mensaje").GetString().Should().Contain("Constancia de cita emitida");
    }

    [Fact]
    public async Task Post_EmitirResumenAtencion_ConAtencionExistente_DeberiaCrearComprobante()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseCita.Paciente.Id);
        var atencion = await TestDataSeeder.CrearAtencionAsync(db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            historialClinicoId: historial.Id);

        var dto = new EmitirComprobanteAtencionDto { AtencionId = atencion.Id };
        var response = await Client.PostJsonAsync("/api/comprobantes/emitir/resumen-atencion", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("mensaje").GetString().Should().Contain("Resumen de atención emitido");
    }

    [Fact]
    public async Task Post_EmitirEstadoCuenta_ConPacienteExistente_DeberiaCrearComprobante()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "10423456");

        var dto = new EmitirComprobanteEstadoCuentaDto { PacienteId = paciente.Id };
        var response = await Client.PostJsonAsync("/api/comprobantes/emitir/estado-cuenta", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("mensaje").GetString().Should().Contain("Estado de cuenta emitido");
    }

    // ================================================================
    // PDF
    // ================================================================

    [Fact]
    public async Task Get_PdfBoletaPago_ConComprobanteExistente_DeberiaRetornarPdf()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "10523456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, paciente.Id, servicio.Id, montoTotal: 100, montoPagado: 100);
        var dto = new EmitirComprobantePagoDto { PagoId = pago.Id };
        var emitResponse = await Client.PostJsonAsync("/api/comprobantes/emitir/boleta-pago", dto);
        emitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var emitDoc = await JsonDocument.ParseAsync(await emitResponse.Content.ReadAsStreamAsync());
        var comprobanteId = emitDoc.RootElement.GetProperty("comprobanteId").GetGuid();

        var response = await Client.GetAsync($"/api/comprobantes/{comprobanteId}/pdf/boleta-pago");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task Get_PdfConstanciaCita_ConComprobanteExistente_DeberiaRetornarPdf()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var cita = await TestDataSeeder.CrearCitaAsync(db, baseCita.Paciente.Id, baseCita.Doctor.Id, baseCita.Servicio.Id, baseCita.Horario.Id);
        var emitDto = new EmitirComprobanteCitaDto { CitaId = cita.Id };
        var emitResponse = await Client.PostJsonAsync("/api/comprobantes/emitir/constancia-cita", emitDto);
        emitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var emitDoc = await JsonDocument.ParseAsync(await emitResponse.Content.ReadAsStreamAsync());
        var comprobanteId = emitDoc.RootElement.GetProperty("comprobanteId").GetGuid();

        var response = await Client.GetAsync($"/api/comprobantes/{comprobanteId}/pdf/constancia-cita");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task Get_PdfResumenAtencion_ConComprobanteExistente_DeberiaRetornarPdf()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseCita.Paciente.Id);
        var atencion = await TestDataSeeder.CrearAtencionAsync(db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            historialClinicoId: historial.Id);
        var emitDto = new EmitirComprobanteAtencionDto { AtencionId = atencion.Id };
        var emitResponse = await Client.PostJsonAsync("/api/comprobantes/emitir/resumen-atencion", emitDto);
        emitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var emitDoc = await JsonDocument.ParseAsync(await emitResponse.Content.ReadAsStreamAsync());
        var comprobanteId = emitDoc.RootElement.GetProperty("comprobanteId").GetGuid();

        var response = await Client.GetAsync($"/api/comprobantes/{comprobanteId}/pdf/resumen-atencion");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task Get_PdfEstadoCuenta_ConComprobanteExistente_DeberiaRetornarPdf()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "10623456");
        var emitDto = new EmitirComprobanteEstadoCuentaDto { PacienteId = paciente.Id };
        var emitResponse = await Client.PostJsonAsync("/api/comprobantes/emitir/estado-cuenta", emitDto);
        emitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var emitDoc = await JsonDocument.ParseAsync(await emitResponse.Content.ReadAsStreamAsync());
        var comprobanteId = emitDoc.RootElement.GetProperty("comprobanteId").GetGuid();

        var response = await Client.GetAsync($"/api/comprobantes/{comprobanteId}/pdf/estado-cuenta");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
    }

    // ================================================================
    // CONSULTAS
    // ================================================================

    [Fact]
    public async Task Get_ObtenerPorId_Existente_DeberiaRetornarComprobante()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "10723456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, paciente.Id, servicio.Id, montoTotal: 100, montoPagado: 100);
        var emitResponse = await Client.PostJsonAsync("/api/comprobantes/emitir/boleta-pago", new EmitirComprobantePagoDto { PagoId = pago.Id });
        using var emitDoc = await JsonDocument.ParseAsync(await emitResponse.Content.ReadAsStreamAsync());
        var comprobanteId = emitDoc.RootElement.GetProperty("comprobanteId").GetGuid();

        var response = await Client.GetAsync($"/api/comprobantes/{comprobanteId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var comprobante = await response.Content.ReadFromJsonAsync<ComprobanteDto>();
        comprobante!.Id.Should().Be(comprobanteId);
    }

    [Fact]
    public async Task Get_ObtenerPorId_Inexistente_DeberiaRetornarNotFound()
    {
        await LoginAsAdminAsync();
        var response = await Client.GetAsync($"/api/comprobantes/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ObtenerPorPaciente_DeberiaRetornarComprobantes()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "10823456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, paciente.Id, servicio.Id, montoTotal: 100, montoPagado: 100);
        await Client.PostJsonAsync("/api/comprobantes/emitir/boleta-pago", new EmitirComprobantePagoDto { PagoId = pago.Id });

        var response = await Client.GetAsync($"/api/comprobantes/paciente/{paciente.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var comprobantes = await response.Content.ReadFromJsonAsync<List<ComprobanteDto>>();
        comprobantes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Get_ObtenerPorPago_DeberiaRetornarComprobantes()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "10923456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, paciente.Id, servicio.Id, montoTotal: 100, montoPagado: 100);
        await Client.PostJsonAsync("/api/comprobantes/emitir/boleta-pago", new EmitirComprobantePagoDto { PagoId = pago.Id });

        var response = await Client.GetAsync($"/api/comprobantes/pago/{pago.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var comprobantes = await response.Content.ReadFromJsonAsync<List<ComprobanteDto>>();
        comprobantes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Get_ObtenerPorAtencion_DeberiaRetornarComprobantes()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var baseCita = await TestDataSeeder.CrearBaseParaCitaAsync(db);
        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseCita.Paciente.Id);
        var atencion = await TestDataSeeder.CrearAtencionAsync(db,
            pacienteId: baseCita.Paciente.Id,
            doctorId: baseCita.Doctor.Id,
            servicioClinicoId: baseCita.Servicio.Id,
            historialClinicoId: historial.Id);
        await Client.PostJsonAsync("/api/comprobantes/emitir/resumen-atencion", new EmitirComprobanteAtencionDto { AtencionId = atencion.Id });

        var response = await Client.GetAsync($"/api/comprobantes/atencion/{atencion.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var comprobantes = await response.Content.ReadFromJsonAsync<List<ComprobanteDto>>();
        comprobantes.Should().NotBeNullOrEmpty();
    }

    // ================================================================
    // ANULACIÓN
    // ================================================================

    [Fact]
    public async Task Put_AnularComprobante_Valido_DeberiaAnular()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "11023456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, paciente.Id, servicio.Id, montoTotal: 100, montoPagado: 100);
        var emitResponse = await Client.PostJsonAsync("/api/comprobantes/emitir/boleta-pago", new EmitirComprobantePagoDto { PagoId = pago.Id });
        using var emitDoc = await JsonDocument.ParseAsync(await emitResponse.Content.ReadAsStreamAsync());
        var comprobanteId = emitDoc.RootElement.GetProperty("comprobanteId").GetGuid();

        var request = new ComprobantesController.AnularComprobanteRequest { Motivo = "Anulación de prueba" };
        var response = await Client.PutJsonAsync($"/api/comprobantes/{comprobanteId}/anular", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("mensaje").GetString().Should().Contain("Comprobante anulado correctamente");
    }

    [Fact]
    public async Task Put_AnularComprobante_YaAnulado_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "11123456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, paciente.Id, servicio.Id, montoTotal: 100, montoPagado: 100);
        var emitDto = new EmitirComprobantePagoDto { PagoId = pago.Id };
        var emitResponse = await Client.PostJsonAsync("/api/comprobantes/emitir/boleta-pago", emitDto);

        using var emitDoc = await JsonDocument.ParseAsync(await emitResponse.Content.ReadAsStreamAsync());
        var comprobanteId = emitDoc.RootElement.GetProperty("comprobanteId").GetGuid();

        var request = new ComprobantesController.AnularComprobanteRequest { Motivo = "Primera anulación" };
        await Client.PutJsonAsync($"/api/comprobantes/{comprobanteId}/anular", request);

        var response = await Client.PutJsonAsync($"/api/comprobantes/{comprobanteId}/anular", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Put_AnularComprobante_MotivoVacio_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();
        await using var db = CreateDbContext();
        var paciente = await TestDataSeeder.CrearPacienteAsync(db, dni: "11223456");
        var servicio = await db.ServiciosClinicos.FirstAsync();
        var pago = await TestDataSeeder.CrearPagoAsync(db, paciente.Id, servicio.Id, montoTotal: 100, montoPagado: 100);
        var emitResponse = await Client.PostJsonAsync("/api/comprobantes/emitir/boleta-pago", new EmitirComprobantePagoDto { PagoId = pago.Id });
        using var emitDoc = await JsonDocument.ParseAsync(await emitResponse.Content.ReadAsStreamAsync());
        var comprobanteId = emitDoc.RootElement.GetProperty("comprobanteId").GetGuid();

        var request = new ComprobantesController.AnularComprobanteRequest { Motivo = "   " };
        var response = await Client.PutJsonAsync($"/api/comprobantes/{comprobanteId}/anular", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
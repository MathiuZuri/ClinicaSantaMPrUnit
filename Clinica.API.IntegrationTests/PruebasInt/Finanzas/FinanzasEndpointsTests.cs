using System.Net;
using Clinica.API.IntegrationTests.Fixtures;
using Clinica.API.IntegrationTests.Helpers;
using Clinica.Domain.DTOs.Finanzas;
using Clinica.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clinica.API.IntegrationTests.PruebasInt.Finanzas;

[Collection("IntegrationTests")]
public class FinanzasEndpointsTests : IntegrationTestBase
{
    public FinanzasEndpointsTests(PostgreSqlFixture postgreSqlFixture)
        : base(postgreSqlFixture)
    {
    }

    [Fact]
    public async Task Get_Finanzas_ResumenDiario_SinToken_DeberiaRetornarUnauthorized()
    {
        ClearAuthorization();

        var fecha = DateOnly.FromDateTime(DateTime.UtcNow);

        var response = await Client.GetAsync($"/api/finanzas/resumen-diario?fecha={fecha:yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Finanzas_ResumenDiario_DeberiaCalcularTotalesDelDia()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var fecha = DateTime.UtcNow.Date.AddHours(10);
        var fechaConsulta = DateOnly.FromDateTime(fecha);

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado,
            metodoPago: MetodoPago.Efectivo,
            fechaPago: fecha
        );

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 200,
            montoPagado: 80,
            estado: EstadoPago.Parcial,
            metodoPago: MetodoPago.Yape,
            fechaPago: fecha.AddMinutes(10)
        );

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 500,
            montoPagado: 500,
            estado: EstadoPago.Anulado,
            metodoPago: MetodoPago.Efectivo,
            fechaPago: fecha.AddMinutes(20)
        );

        var response = await Client.GetAsync($"/api/finanzas/resumen-diario?fecha={fechaConsulta:yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var resumen = await response.ReadDataAsJsonAsync<ResumenDiarioFinanzasDto>();

        resumen.Should().NotBeNull();
        resumen!.Fecha.Should().Be(fechaConsulta);
        resumen.TotalIngresos.Should().Be(180);
        resumen.TotalPendiente.Should().Be(120);
        resumen.TotalDeuda.Should().Be(120);
        resumen.CantidadPagos.Should().Be(2);
        resumen.PagosCompletados.Should().Be(1);
        resumen.PagosParciales.Should().Be(1);
        resumen.PagosPendientes.Should().Be(1);
        resumen.Pagos.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_Finanzas_ResumenMensual_DeberiaCalcularTotalesDelMes()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var fecha = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 10, 9, 0, 0, DateTimeKind.Utc);

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado,
            fechaPago: fecha
        );

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 150,
            montoPagado: 50,
            estado: EstadoPago.Parcial,
            fechaPago: fecha.AddDays(1)
        );

        var response = await Client.GetAsync($"/api/finanzas/resumen-mensual?anio={fecha.Year}&mes={fecha.Month}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resumen = await response.ReadDataAsJsonAsync<ResumenMensualFinanzasDto>();

        resumen.Should().NotBeNull();
        resumen!.Anio.Should().Be(fecha.Year);
        resumen.Mes.Should().Be(fecha.Month);
        resumen.TotalIngresos.Should().Be(150);
        resumen.TotalPendiente.Should().Be(100);
        resumen.TotalDeuda.Should().Be(100);
        resumen.CantidadPagos.Should().Be(2);
        resumen.Dias.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Get_Finanzas_ResumenMensual_ConMesInvalido_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        var anio = DateTime.UtcNow.Year;

        var response = await Client.GetAsync($"/api/finanzas/resumen-mensual?anio={anio}&mes=13");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Get_Finanzas_ResumenAnual_DeberiaRetornarDoceMeses()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var fecha = new DateTime(DateTime.UtcNow.Year, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado,
            fechaPago: fecha
        );

        var response = await Client.GetAsync($"/api/finanzas/resumen-anual?anio={fecha.Year}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resumen = await response.ReadDataAsJsonAsync<ResumenAnualFinanzasDto>();

        resumen.Should().NotBeNull();
        resumen!.Anio.Should().Be(fecha.Year);
        resumen.TotalIngresos.Should().BeGreaterThanOrEqualTo(100);
        resumen.Meses.Should().HaveCount(12);
    }

    [Fact]
    public async Task Get_Finanzas_PagosPendientes_DeberiaRetornarPagosConSaldoPendiente()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var pagoParcial = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 200,
            montoPagado: 50,
            estado: EstadoPago.Parcial
        );

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado
        );

        var response = await Client.GetAsync("/api/finanzas/pagos-pendientes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagos = await response.ReadDataAsJsonAsync<List<PagoFinanzasDto>>();

        pagos.Should().NotBeNull();
        pagos.Should().Contain(x => x.PagoId == pagoParcial.Id);
        pagos!.Single(x => x.PagoId == pagoParcial.Id).SaldoPendiente.Should().Be(150);
    }

    [Fact]
    public async Task Get_Finanzas_PagosPagados_DeberiaRetornarSoloPagados()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var pagoPagado = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado
        );

        var response = await Client.GetAsync("/api/finanzas/pagos-pagados");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagos = await response.ReadDataAsJsonAsync<List<PagoFinanzasDto>>();

        pagos.Should().NotBeNull();
        pagos.Should().Contain(x => x.PagoId == pagoPagado.Id);
        pagos!.Single(x => x.PagoId == pagoPagado.Id).EstadoPago.Should().Be(nameof(EstadoPago.Pagado));
    }

    [Fact]
    public async Task Get_Finanzas_PagosParciales_DeberiaRetornarSoloParciales()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var pagoParcial = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 30,
            estado: EstadoPago.Parcial
        );

        var response = await Client.GetAsync("/api/finanzas/pagos-parciales");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagos = await response.ReadDataAsJsonAsync<List<PagoFinanzasDto>>();

        pagos.Should().NotBeNull();
        pagos.Should().Contain(x => x.PagoId == pagoParcial.Id);
        pagos!.Single(x => x.PagoId == pagoParcial.Id).EstadoPago.Should().Be(nameof(EstadoPago.Parcial));
    }

    [Fact]
    public async Task Get_Finanzas_PagoPorCodigo_Existente_DeberiaRetornarPago()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 80,
            montoPagado: 80,
            estado: EstadoPago.Pagado
        );

        var response = await Client.GetAsync($"/api/finanzas/pago/codigo/{pago.CodigoPago}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagoResponse = await response.ReadDataAsJsonAsync<PagoFinanzasDto>();

        pagoResponse.Should().NotBeNull();
        pagoResponse!.PagoId.Should().Be(pago.Id);
        pagoResponse.CodigoPago.Should().Be(pago.CodigoPago);
    }

    [Fact]
    public async Task Get_Finanzas_PagoPorCodigo_Inexistente_DeberiaRetornarNotFound()
    {
        await LoginAsAdminAsync();

        var response = await Client.GetAsync("/api/finanzas/pago/codigo/PAG-INEXISTENTE");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Get_Finanzas_EstadoCuentaPaciente_DeberiaCalcularEstadoRealPorAtencion()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseData.Paciente.Id);

        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            doctorId: baseData.Doctor.Id,
            servicioClinicoId: baseData.Servicio.Id,
            historialClinicoId: historial.Id,
            costoFinal: 100,
            montoPagado: 0
        );

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            atencionId: atencion.Id,
            montoTotal: 100,
            montoPagado: 40,
            estado: EstadoPago.Parcial
        );

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            atencionId: atencion.Id,
            montoTotal: 100,
            montoPagado: 20,
            estado: EstadoPago.Parcial
        );

        var response = await Client.GetAsync($"/api/finanzas/paciente/{baseData.Paciente.Id}/estado-cuenta");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var estadoCuenta = await response.ReadDataAsJsonAsync<EstadoCuentaPacienteDto>();

        estadoCuenta.Should().NotBeNull();
        estadoCuenta!.PacienteId.Should().Be(baseData.Paciente.Id);
        estadoCuenta.TotalFacturado.Should().Be(100);
        estadoCuenta.TotalPagado.Should().Be(60);
        estadoCuenta.TotalPendiente.Should().Be(40);
        estadoCuenta.CantidadPagos.Should().Be(2);
        estadoCuenta.PagosParciales.Should().Be(1);
        estadoCuenta.Detalles.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_Finanzas_DeudasReales_DeberiaRetornarAtencionesConDeuda()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseData.Paciente.Id);

        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            doctorId: baseData.Doctor.Id,
            servicioClinicoId: baseData.Servicio.Id,
            historialClinicoId: historial.Id,
            costoFinal: 200,
            montoPagado: 0
        );

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            atencionId: atencion.Id,
            montoTotal: 200,
            montoPagado: 70,
            estado: EstadoPago.Parcial
        );

        var response = await Client.GetAsync("/api/finanzas/deudas-reales");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var deudas = await response.ReadDataAsJsonAsync<List<EstadoPagoAtencionDto>>();

        deudas.Should().NotBeNull();
        deudas.Should().Contain(x =>
            x.AtencionId == atencion.Id &&
            x.TieneDeuda &&
            x.SaldoReal == 130);
    }

    [Fact]
    public async Task Get_Finanzas_DeudasRealesPaciente_DeberiaRetornarDeudasDelPaciente()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseData.Paciente.Id);

        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            doctorId: baseData.Doctor.Id,
            servicioClinicoId: baseData.Servicio.Id,
            historialClinicoId: historial.Id,
            costoFinal: 150,
            montoPagado: 0
        );

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            atencionId: atencion.Id,
            montoTotal: 150,
            montoPagado: 50,
            estado: EstadoPago.Parcial
        );

        var response = await Client.GetAsync($"/api/finanzas/paciente/{baseData.Paciente.Id}/deudas-reales");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var deudas = await response.ReadDataAsJsonAsync<List<EstadoPagoAtencionDto>>();

        deudas.Should().NotBeNull();
        deudas.Should().Contain(x =>
            x.AtencionId == atencion.Id &&
            x.SaldoReal == 100);
    }

    [Fact]
    public async Task Get_Finanzas_EstadoPagoAtencion_DeberiaRetornarEstadoParcial()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseData.Paciente.Id);

        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            doctorId: baseData.Doctor.Id,
            servicioClinicoId: baseData.Servicio.Id,
            historialClinicoId: historial.Id,
            costoFinal: 100,
            montoPagado: 0
        );

        await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            atencionId: atencion.Id,
            montoTotal: 100,
            montoPagado: 30,
            estado: EstadoPago.Parcial
        );

        var response = await Client.GetAsync($"/api/finanzas/atencion/{atencion.Id}/estado-pago");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var estado = await response.ReadDataAsJsonAsync<EstadoPagoAtencionDto>();

        estado.Should().NotBeNull();
        estado!.AtencionId.Should().Be(atencion.Id);
        estado.MontoTotal.Should().Be(100);
        estado.TotalPagado.Should().Be(30);
        estado.SaldoReal.Should().Be(70);
        estado.EstadoFinanciero.Should().Be("Parcial");
        estado.TieneDeuda.Should().BeTrue();
    }

    [Fact]
    public async Task Get_Finanzas_EstadoPagoAtencion_SinPagos_DeberiaRetornarNotFound()
    {
        await LoginAsAdminAsync();

        var response = await Client.GetAsync($"/api/finanzas/atencion/{Guid.NewGuid()}/estado-pago");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_Finanzas_LibroDiario_DeberiaRetornarMovimientosDelDia()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var fecha = DateTime.UtcNow.Date.AddHours(8);
        var fechaConsulta = DateOnly.FromDateTime(fecha);

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 90,
            montoPagado: 90,
            estado: EstadoPago.Pagado,
            fechaPago: fecha
        );

        var response = await Client.GetAsync($"/api/finanzas/libro-diario?fecha={fechaConsulta:yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var movimientos = await response.ReadDataAsJsonAsync<List<PagoFinanzasDto>>();

        movimientos.Should().NotBeNull();
        movimientos.Should().Contain(x => x.PagoId == pago.Id);
    }

    [Fact]
    public async Task Get_Finanzas_ResumenFinancieroMensualCompleto_DeberiaRetornarCajaRealYAjustes()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseData.Paciente.Id);

        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            doctorId: baseData.Doctor.Id,
            servicioClinicoId: baseData.Servicio.Id,
            historialClinicoId: historial.Id,
            costoFinal: 100,
            montoPagado: 0
        );

        var fecha = DateTime.UtcNow.Date.AddHours(9);

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            atencionId: atencion.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado,
            metodoPago: MetodoPago.Tarjeta,
            fechaPago: fecha
        );

        await TestDataSeeder.CrearAjusteFinancieroAsync(
            db,
            pagoId: pago.Id,
            pacienteId: baseData.Paciente.Id,
            atencionId: atencion.Id,
            tipoAjuste: TipoAjusteFinanciero.Descuento,
            montoAjuste: 10,
            fechaRegistro: fecha
        );

        var response = await Client.GetAsync(
            $"/api/finanzas/resumen-financiero-mensual-completo?anio={fecha.Year}&mes={fecha.Month}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resumen = await response.ReadDataAsJsonAsync<ResumenFinancieroMensualCompletoDto>();

        resumen.Should().NotBeNull();
        resumen!.Anio.Should().Be(fecha.Year);
        resumen.Mes.Should().Be(fecha.Month);
        resumen.ResumenCaja.TotalIngresos.Should().BeGreaterThanOrEqualTo(100);
        resumen.ResumenCaja.TotalTarjeta.Should().BeGreaterThanOrEqualTo(100);
        resumen.ResumenRealAtenciones.AtencionesPagadas.Should().BeGreaterThanOrEqualTo(1);
        resumen.AjustesFinancieros.Should().Contain(x => x.PagoId == pago.Id);
    }

    [Fact]
    public async Task Post_Finanzas_AjusteFinanciero_Valido_DeberiaRegistrarAjuste()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado
        );

        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = pago.Id,
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 15,
            Motivo = "Descuento autorizado por administración",
            Observacion = "Prueba de integración"
        };

        var response = await Client.PostJsonAsync("/api/finanzas/ajustes-financieros", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await JsonTestHelper.AssertSuccessAsync(response);

        var data = await JsonTestHelper.ReadDataAsync(response);
        var ajusteId = data.GetProperty("id").GetGuid();

        await using var dbVerificacion = CreateDbContext();

        var ajuste = await dbVerificacion.AjustesFinancieros
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == ajusteId);

        ajuste.Should().NotBeNull();
        ajuste!.PagoId.Should().Be(pago.Id);
        ajuste.PacienteId.Should().Be(baseData.Paciente.Id);
        ajuste.TipoAjuste.Should().Be(TipoAjusteFinanciero.Descuento);
        ajuste.MontoAjuste.Should().Be(15);
        ajuste.Motivo.Should().Be("Descuento autorizado por administración");
        ajuste.UsuarioRegistroId.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_Finanzas_AjusteFinanciero_Duplicado_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado
        );

        await TestDataSeeder.CrearAjusteFinancieroAsync(
            db,
            pagoId: pago.Id,
            pacienteId: baseData.Paciente.Id,
            tipoAjuste: TipoAjusteFinanciero.Descuento,
            montoAjuste: 15,
            motivo: "Duplicado Test"
        );

        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = pago.Id,
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 15,
            Motivo = "Duplicado Test",
            Observacion = "Debe fallar"
        };

        var response = await Client.PostJsonAsync("/api/finanzas/ajustes-financieros", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Finanzas_AjusteFinanciero_PagoInexistente_DeberiaRetornarNotFound()
    {
        await LoginAsAdminAsync();

        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.NewGuid(),
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 10,
            Motivo = "Pago inexistente",
            Observacion = "Debe retornar 404"
        };

        var response = await Client.PostJsonAsync("/api/finanzas/ajustes-financieros", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await JsonTestHelper.AssertErrorAsync(response);
    }

    [Fact]
    public async Task Post_Finanzas_AjusteFinanciero_MontoCero_DeberiaRetornarBadRequest()
    {
        await LoginAsAdminAsync();

        var dto = new RegistrarAjusteFinancieroDto
        {
            PagoId = Guid.NewGuid(),
            TipoAjuste = TipoAjusteFinanciero.Descuento,
            MontoAjuste = 0,
            Motivo = "Monto inválido"
        };

        var response = await Client.PostJsonAsync("/api/finanzas/ajustes-financieros", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_Finanzas_AjustesFinancieros_DeberiaRetornarAjustes()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado
        );

        var ajuste = await TestDataSeeder.CrearAjusteFinancieroAsync(
            db,
            pagoId: pago.Id,
            pacienteId: baseData.Paciente.Id,
            tipoAjuste: TipoAjusteFinanciero.Recargo,
            montoAjuste: 20,
            motivo: "Recargo de prueba"
        );

        var response = await Client.GetAsync("/api/finanzas/ajustes-financieros");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var ajustes = await response.ReadDataAsJsonAsync<List<AjusteFinancieroDto>>();

        ajustes.Should().NotBeNull();
        ajustes.Should().Contain(x =>
            x.Id == ajuste.Id &&
            x.PagoId == pago.Id &&
            x.TipoAjuste == nameof(TipoAjusteFinanciero.Recargo));
    }

    [Fact]
    public async Task Get_Finanzas_AjustesPorPago_DeberiaRetornarAjustesDelPago()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            montoTotal: 100,
            montoPagado: 100,
            estado: EstadoPago.Pagado
        );

        var ajuste = await TestDataSeeder.CrearAjusteFinancieroAsync(
            db,
            pagoId: pago.Id,
            pacienteId: baseData.Paciente.Id,
            montoAjuste: 12,
            motivo: "Ajuste por pago"
        );

        var response = await Client.GetAsync($"/api/finanzas/pago/{pago.Id}/ajustes-financieros");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var ajustes = await response.ReadDataAsJsonAsync<List<AjusteFinancieroDto>>();

        ajustes.Should().NotBeNull();
        ajustes.Should().Contain(x => x.Id == ajuste.Id && x.PagoId == pago.Id);
    }

    [Fact]
    public async Task Get_Finanzas_AjustesPorAtencion_DeberiaRetornarAjustesDeLaAtencion()
    {
        await LoginAsAdminAsync();

        await using var db = CreateDbContext();

        var baseData = await TestDataSeeder.CrearBasePacienteDoctorServicioAsync(db);

        var historial = await TestDataSeeder.CrearHistorialClinicoAsync(db, baseData.Paciente.Id);

        var atencion = await TestDataSeeder.CrearAtencionAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            doctorId: baseData.Doctor.Id,
            servicioClinicoId: baseData.Servicio.Id,
            historialClinicoId: historial.Id,
            costoFinal: 100,
            montoPagado: 0
        );

        var pago = await TestDataSeeder.CrearPagoAsync(
            db,
            pacienteId: baseData.Paciente.Id,
            servicioClinicoId: baseData.Servicio.Id,
            atencionId: atencion.Id,
            montoTotal: 100,
            montoPagado: 60,
            estado: EstadoPago.Parcial
        );

        var ajuste = await TestDataSeeder.CrearAjusteFinancieroAsync(
            db,
            pagoId: pago.Id,
            pacienteId: baseData.Paciente.Id,
            atencionId: atencion.Id,
            montoAjuste: 10,
            motivo: "Ajuste por atención"
        );

        var response = await Client.GetAsync($"/api/finanzas/atencion/{atencion.Id}/ajustes-financieros");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var ajustes = await response.ReadDataAsJsonAsync<List<AjusteFinancieroDto>>();

        ajustes.Should().NotBeNull();
        ajustes.Should().Contain(x =>
            x.Id == ajuste.Id &&
            x.AtencionId == atencion.Id);
    }
}
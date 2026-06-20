namespace Clinica.Domain.PDFsDto.Interfacespdf;

public interface IReporteFinancieroPdfService
{
    byte[] GeneratePdf(ReporteDiarioDto dto);
}
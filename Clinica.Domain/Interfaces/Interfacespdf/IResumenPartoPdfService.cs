namespace Clinica.Domain.PDFsDto.Interfacespdf;

public interface IResumenPartoPdfService
{
    byte[] GeneratePdf(ResumenPartoPdfDto dto);
}
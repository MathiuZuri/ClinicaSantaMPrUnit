namespace Clinica.Domain.PDFsDto.Interfacespdf;

public interface ICertificadoTrabajoPdfService
{
    byte[] GeneratePdf(CertificadoTrabajoDto dto);
}
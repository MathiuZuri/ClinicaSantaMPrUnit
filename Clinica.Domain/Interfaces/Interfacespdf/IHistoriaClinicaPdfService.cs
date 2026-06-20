namespace Clinica.Domain.PDFsDto.Interfacespdf;

public interface IHistoriaClinicaPdfService
{
    byte[] GeneratePdf(HistoriaClinicaPdfDto dto);
}
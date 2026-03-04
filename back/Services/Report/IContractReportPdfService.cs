using back.Models.DTOs;

namespace back.Services.Report;

public interface IContractReportPdfService
{
    byte[] GeneratePdf(ContractScannerReportRequest request);
}

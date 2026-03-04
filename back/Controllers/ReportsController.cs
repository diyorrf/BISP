using back.Models.DTOs;
using back.Services.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IContractReportPdfService _pdfService;

    public ReportsController(IContractReportPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    /// <summary>
    /// Generate and download contract scanner report as PDF.
    /// </summary>
    [HttpPost("contract-scanner")]
    [Produces("application/pdf", Type = typeof(FileResult))]
    public IActionResult DownloadContractScannerPdf([FromBody] ContractScannerReportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            return BadRequest("FileName is required.");

        var pdfBytes = _pdfService.GeneratePdf(request);
        var safeName = string.Join("_", request.FileName.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"LegalGuard_Report_{safeName}.pdf";
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            fileName += ".pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}

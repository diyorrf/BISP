using System.IO;
using back.Models.DTOs;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace back.Services.Report;

public class ContractReportPdfService : IContractReportPdfService
{
    public byte[] GeneratePdf(ContractScannerReportRequest request)
    {
        using var stream = new MemoryStream();
        using (var writer = new PdfWriter(stream))
        using (var pdf = new PdfDocument(writer))
        {
            var document = new iText.Layout.Document(pdf);
            var font = PdfFontFactory.CreateFont();
            var boldFont = PdfFontFactory.CreateFont();

            // Title
            document.Add(new Paragraph("LegalGuard - Contract Scanner Report")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetMarginBottom(4));
            document.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
                .SetFont(font)
                .SetFontSize(10)
                .SetFontColor(ColorConstants.GRAY)
                .SetMarginBottom(12));

            // File and risk
            document.Add(new Paragraph($"File: {request.FileName}")
                .SetFont(font)
                .SetFontSize(11)
                .SetMarginBottom(2));
            document.Add(new Paragraph($"Risk Level: {request.RiskLevel.ToUpperInvariant()}")
                .SetFont(boldFont)
                .SetFontSize(11)
                .SetMarginBottom(12));

            // Summary
            document.Add(new Paragraph("Summary")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginBottom(4));
            document.Add(new Paragraph(request.Summary)
                .SetFont(font)
                .SetFontSize(10)
                .SetMarginBottom(12));

            // Issues
            document.Add(new Paragraph("Issues")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginBottom(4));
            if (request.Issues.Count == 0)
            {
                document.Add(new Paragraph("None").SetFont(font).SetFontSize(10));
            }
            else
            {
                for (var i = 0; i < request.Issues.Count; i++)
                {
                    var issue = request.Issues[i];
                    var riskColor = issue.Risk.ToLowerInvariant() switch
                    {
                        "high" => ColorConstants.RED,
                        "medium" => new DeviceRgb(180, 83, 9),
                        _ => ColorConstants.GREEN
                    };
                    document.Add(new Paragraph($"{i + 1}. {issue.Clause} [{issue.Risk.ToUpperInvariant()}]")
                        .SetFont(boldFont)
                        .SetFontSize(10)
                        .SetFontColor(riskColor));
                    document.Add(new Paragraph(issue.Description).SetFont(font).SetFontSize(10).SetMarginLeft(12));
                    document.Add(new Paragraph($"Reference: {issue.Reference}")
                        .SetFont(font)
                        .SetFontSize(9)
                        .SetFontColor(ColorConstants.GRAY)
                        .SetMarginLeft(12)
                        .SetMarginBottom(6));
                }
            }
            document.Add(new Paragraph().SetMarginBottom(8));

            // Recommendations
            document.Add(new Paragraph("Recommendations")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginBottom(4));
            if (request.Recommendations.Count == 0)
            {
                document.Add(new Paragraph("None").SetFont(font).SetFontSize(10));
            }
            else
            {
                foreach (var rec in request.Recommendations)
                {
                    document.Add(new Paragraph($"• {rec}").SetFont(font).SetFontSize(10).SetMarginLeft(8));
                }
            }

            document.Close();
        }

        return stream.ToArray();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace back.Services.Parser
{
    public class DocumentParserService: IDocumentParserService
    {
        private readonly ILogger<DocumentParserService> _logger;

        private static readonly Dictionary<string, string[]> SupportedTypes = new()
        {
            { "text", new[] { "text/plain", "text/markdown" } },
            { "json", new[] { "application/json" } },
            { "pdf", new[] { "application/pdf" } },
            { "word", new[] { 
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
                "application/msword" // .doc
            }}
        };

        public DocumentParserService(ILogger<DocumentParserService> logger)
        {
            _logger = logger;
        }

        public bool IsSupported(string contentType)
        {
            return SupportedTypes.Values.Any(types => types.Contains(contentType));
        }

        public async Task<string> ExtractTextAsync(IFormFile file, CancellationToken ct = default)
        {
            var contentType = file.ContentType.ToLower();

            try
            {
                // Text files
                if (SupportedTypes["text"].Contains(contentType) || SupportedTypes["json"].Contains(contentType))
                {
                    return await ExtractTextFromTextFileAsync(file, ct);
                }

                // PDF files
                if (SupportedTypes["pdf"].Contains(contentType))
                {
                    return await ExtractTextFromPdfAsync(file, ct);
                }

                // Word files
                if (SupportedTypes["word"].Contains(contentType))
                {
                    return await ExtractTextFromWordAsync(file, ct);
                }

                throw new NotSupportedException($"File type '{contentType}' is not supported");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from file: {FileName}", file.FileName);
                throw;
            }
        }

        private async Task<string> ExtractTextFromTextFileAsync(IFormFile file, CancellationToken ct)
        {
            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            return await reader.ReadToEndAsync(ct);
        }

        private async Task<string> ExtractTextFromPdfAsync(IFormFile file, CancellationToken ct)
        {
            await using var stream = file.OpenReadStream();
            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);

            var text = new StringBuilder();
            
            for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
            {
                if (ct.IsCancellationRequested)
                    break;

                var strategy = new SimpleTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);
                text.AppendLine(pageText);
                text.AppendLine(); // Add separation between pages
            }

            return text.ToString();
        }

        private async Task<string> ExtractTextFromWordAsync(IFormFile file, CancellationToken ct)
        {
            await using var stream = file.OpenReadStream();
            using var wordDocument = WordprocessingDocument.Open(stream, false);

            var body = wordDocument.MainDocumentPart?.Document.Body;
            if (body == null)
                return string.Empty;

            var text = new StringBuilder();

            foreach (var paragraph in body.Elements<Paragraph>())
            {
                if (ct.IsCancellationRequested)
                    break;

                var paragraphText = paragraph.InnerText;
                if (!string.IsNullOrWhiteSpace(paragraphText))
                {
                    text.AppendLine(paragraphText);
                }
            }

            // Extract text from tables
            foreach (var table in body.Elements<Table>())
            {
                if (ct.IsCancellationRequested)
                    break;

                foreach (var row in table.Elements<TableRow>())
                {
                    var rowText = string.Join(" | ", row.Elements<TableCell>().Select(c => c.InnerText));
                    if (!string.IsNullOrWhiteSpace(rowText))
                    {
                        text.AppendLine(rowText);
                    }
                }
                text.AppendLine(); // Separation after table
            }

            return text.ToString();
        }
    }
}
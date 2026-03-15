using System.Text;
using System.Text.Json;
using back.Data.Entities;
using back.Data.Repos.Interfaces;

namespace back.Services.Regulatory;

public class LegalReferenceExtractionService : ILegalReferenceExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILegalReferenceRepository _legalReferenceRepository;
    private readonly ILogger<LegalReferenceExtractionService> _logger;

    public LegalReferenceExtractionService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILegalReferenceRepository legalReferenceRepository,
        ILogger<LegalReferenceExtractionService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key not configured");
        _model = configuration["OpenAI:ChatModel"] ?? "gpt-4o";
        _legalReferenceRepository = legalReferenceRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LegalReference>> ExtractReferencesAsync(
        Guid documentId, string documentContent, CancellationToken ct = default)
    {
        var prompt = BuildExtractionPrompt(documentContent);

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "You are a legal reference extraction engine. You analyze legal documents and extract all references to laws, statutes, codes, regulations, articles, and legal provisions. Return ONLY a JSON array." },
                new { role = "user", content = prompt }
            },
            temperature = 0.1,
            max_tokens = 4000,
            response_format = new { type = "json_object" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        try
        {
            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var jsonDoc = JsonDocument.Parse(responseContent);

            var content = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";

            var references = ParseExtractionResponse(documentId, content);
            _logger.LogInformation("Extracted {Count} legal references from document {DocumentId}", references.Count, documentId);

            if (references.Count > 0)
                await _legalReferenceRepository.AddRangeAsync(references, ct);

            return references;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract legal references from document {DocumentId}", documentId);
            return Array.Empty<LegalReference>();
        }
    }

    private static string BuildExtractionPrompt(string documentContent)
    {
        var truncated = documentContent.Length > 12000
            ? documentContent[..12000] + "\n...[truncated]"
            : documentContent;

        return $$"""
            Analyze the following legal document and extract ALL references to laws, statutes, codes, regulations, articles, and legal provisions.

            For each reference found, provide:
            - "title": The name of the law or code (e.g., "Civil Code of the Republic of Uzbekistan", "Labor Code", "Tax Code")
            - "articleOrSection": The specific article, section, or clause number (e.g., "Article 354", "Section 12", "Chapter 5")
            - "rawText": The exact text snippet from the document that contains the reference
            - "jurisdiction": The jurisdiction (e.g., "Uzbekistan", "European Union", "United States")

            Return a JSON object with a "references" key containing an array of objects.
            If no legal references are found, return {"references": []}.

            Document content:
            {{truncated}}
            """;
    }

    private static List<LegalReference> ParseExtractionResponse(Guid documentId, string jsonContent)
    {
        var references = new List<LegalReference>();

        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            JsonElement refsArray;

            if (doc.RootElement.TryGetProperty("references", out refsArray) && refsArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in refsArray.EnumerateArray())
                {
                    references.Add(new LegalReference
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        Title = item.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                        ArticleOrSection = item.TryGetProperty("articleOrSection", out var a) ? a.GetString() ?? "" : "",
                        RawText = item.TryGetProperty("rawText", out var r) ? r.GetString() ?? "" : "",
                        Jurisdiction = item.TryGetProperty("jurisdiction", out var j) ? j.GetString() ?? "" : "",
                        ExtractedAt = DateTime.UtcNow
                    });
                }
            }
        }
        catch (JsonException)
        {
            // Malformed AI response; return empty list
        }

        return references;
    }
}

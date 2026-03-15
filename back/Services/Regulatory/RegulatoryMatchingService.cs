using System.Text;
using System.Text.Json;
using back.Data.Entities;
using back.Data.Repos.Interfaces;

namespace back.Services.Regulatory;

public class RegulatoryMatchingService : IRegulatoryMatchingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILegalReferenceRepository _legalReferenceRepository;
    private readonly IRegulatoryAlertRepository _alertRepository;
    private readonly ILogger<RegulatoryMatchingService> _logger;

    public RegulatoryMatchingService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILegalReferenceRepository legalReferenceRepository,
        IRegulatoryAlertRepository alertRepository,
        ILogger<RegulatoryMatchingService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key not configured");
        _model = configuration["OpenAI:ChatModel"] ?? "gpt-4o";
        _legalReferenceRepository = legalReferenceRepository;
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task<int> MatchAndCreateAlertsAsync(RegulatoryUpdate update, CancellationToken ct = default)
    {
        var allReferences = (await _legalReferenceRepository.GetAllAsync(ct)).ToList();

        if (allReferences.Count == 0)
        {
            _logger.LogInformation("No legal references in database to match against update {UpdateId}", update.Id);
            return 0;
        }

        var matchedIds = await FindMatchesViaAI(update, allReferences, ct);
        var alertsCreated = 0;

        foreach (var refId in matchedIds)
        {
            var legalRef = allReferences.FirstOrDefault(r => r.Id == refId);
            if (legalRef?.Document == null) continue;
            if (legalRef.Document.UserId == null) continue;

            var alreadyExists = await _alertRepository.ExistsAsync(update.Id, refId, ct);
            if (alreadyExists) continue;

            var alert = new RegulatoryAlert
            {
                Id = Guid.NewGuid(),
                UserId = legalRef.Document.UserId.Value,
                DocumentId = legalRef.DocumentId,
                RegulatoryUpdateId = update.Id,
                LegalReferenceId = refId,
                IsRead = false,
                IsDismissed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _alertRepository.AddAsync(alert, ct);
            alertsCreated++;
        }

        _logger.LogInformation("Regulatory update {UpdateId} matched {Count} references, created {Alerts} alerts",
            update.Id, matchedIds.Count, alertsCreated);

        return alertsCreated;
    }

    private async Task<List<Guid>> FindMatchesViaAI(RegulatoryUpdate update, List<LegalReference> references, CancellationToken ct)
    {
        var refSummaries = references.Select((r, i) => new
        {
            index = i,
            id = r.Id.ToString(),
            title = r.Title,
            article = r.ArticleOrSection,
            jurisdiction = r.Jurisdiction
        });

        var effectiveDateStr = update.EffectiveDate?.ToString("yyyy-MM-dd") ?? "Not specified";
        var refJson = JsonSerializer.Serialize(refSummaries, new JsonSerializerOptions { WriteIndented = true });

        var prompt = $$"""
            A new regulatory change has been published:

            Title: {{update.Title}}
            Description: {{update.Description}}
            Law Identifier: {{update.LawIdentifier}}
            Effective Date: {{effectiveDateStr}}

            Below is a list of legal references extracted from user documents. Determine which of these references are AFFECTED by the regulatory change above. A reference is affected if the regulatory change modifies, amends, repeals, or is directly related to the law/article cited in the reference.

            Legal references:
            {{refJson}}

            Return a JSON object with an "affected_ids" key containing an array of the "id" strings of affected references.
            If none are affected, return {"affected_ids": []}.
            """;

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "You are a legal regulatory matching engine. You determine which legal references in documents are affected by a regulatory change. Be precise and only match references that are genuinely affected." },
                new { role = "user", content = prompt }
            },
            temperature = 0.1,
            max_tokens = 2000,
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

            return ParseMatchResponse(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI matching failed for regulatory update {UpdateId}", update.Id);
            return new List<Guid>();
        }
    }

    private static List<Guid> ParseMatchResponse(string jsonContent)
    {
        var ids = new List<Guid>();
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            if (doc.RootElement.TryGetProperty("affected_ids", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    if (Guid.TryParse(item.GetString(), out var id))
                        ids.Add(id);
                }
            }
        }
        catch (JsonException) { }

        return ids;
    }
}

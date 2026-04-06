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
    private readonly IDocumentTopicRepository _topicRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IRegulatoryAlertRepository _alertRepository;
    private readonly ILogger<RegulatoryMatchingService> _logger;

    public RegulatoryMatchingService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IDocumentTopicRepository topicRepository,
        IDocumentRepository documentRepository,
        IRegulatoryAlertRepository alertRepository,
        ILogger<RegulatoryMatchingService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key not configured");
        _model = configuration["OpenAI:ChatModel"] ?? "gpt-4o";
        _topicRepository = topicRepository;
        _documentRepository = documentRepository;
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task<int> MatchAndCreateAlertsAsync(RegulatoryUpdate update, CancellationToken ct = default)
    {
        var allTopics = (await _topicRepository.GetAllWithDocumentAsync(ct)).ToList();

        if (allTopics.Count == 0)
        {
            _logger.LogInformation("No document topics in database to match against update {UpdateId}", update.Id);
            return 0;
        }

        // Group topics by document
        var documentInfos = allTopics
            .Where(t => t.Document?.UserId != null)
            .GroupBy(t => t.DocumentId)
            .Select(g => new DocumentInfo
            {
                DocumentId = g.Key,
                UserId = g.First().Document!.UserId!.Value,
                FileName = g.First().Document!.FileName,
                Topics = g.Select(t => t.Topic).ToList()
            })
            .ToList();

        if (documentInfos.Count == 0)
        {
            _logger.LogInformation("No documents with topics and owners found for update {UpdateId}", update.Id);
            return 0;
        }

        var matches = await FindMatchesAndRisksViaAI(update, documentInfos, ct);
        var alertsCreated = 0;

        foreach (var match in matches)
        {
            if (!Guid.TryParse(match.DocumentId, out var docId)) continue;

            var alreadyExists = await _alertRepository.ExistsForDocumentAsync(update.Id, docId, ct);
            if (alreadyExists) continue;

            var docInfo = documentInfos.FirstOrDefault(d => d.DocumentId == docId);
            if (docInfo == null) continue;

            var alert = new RegulatoryAlert
            {
                Id = Guid.NewGuid(),
                UserId = docInfo.UserId,
                DocumentId = docId,
                RegulatoryUpdateId = update.Id,
                LegalReferenceId = null,
                RiskDescription = match.RiskDescription,
                IsRead = false,
                IsDismissed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _alertRepository.AddAsync(alert, ct);
            alertsCreated++;
        }

        _logger.LogInformation("Regulatory update {UpdateId} matched {Count} documents, created {Alerts} alerts",
            update.Id, matches.Count, alertsCreated);

        return alertsCreated;
    }

    private async Task<List<MatchResult>> FindMatchesAndRisksViaAI(
        RegulatoryUpdate update,
        List<DocumentInfo> documents,
        CancellationToken ct)
    {
        var docSummaries = documents.Select(d => new
        {
            id = d.DocumentId.ToString(),
            fileName = d.FileName,
            topics = d.Topics
        }).ToList();

        var effectiveDateStr = update.EffectiveDate?.ToString("yyyy-MM-dd") ?? "Not specified";
        var docJson = JsonSerializer.Serialize(docSummaries, new JsonSerializerOptions { WriteIndented = true });

        var lawContent = !string.IsNullOrEmpty(update.Content)
            ? $"\n\nFull text of the new law/regulation:\n{(update.Content.Length > 8000 ? update.Content[..8000] + "\n...[truncated]" : update.Content)}"
            : "";

        var prompt = $$"""
            A new law or regulatory change has been published:

            Title: {{update.Title}}
            Description: {{update.Description}}
            Law Identifier: {{update.LawIdentifier}}
            Effective Date: {{effectiveDateStr}}{{lawContent}}

            Below is a list of user documents with their extracted legal topics. Determine which documents are AFFECTED by this regulatory change based on their topics.

            For each affected document, provide a clear risk description explaining:
            - What specifically changed in the law
            - How it affects the document
            - What actions the user should take

            Documents:
            {{docJson}}

            Return a JSON object with a "matches" key containing an array of objects:
            [{"document_id": "guid", "risk_description": "Detailed explanation of the risk and recommended actions"}]
            If no documents are affected, return {"matches": []}.
            """;

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "You are a legal regulatory matching engine specializing in Uzbekistan law. You determine which documents are affected by regulatory changes based on their legal topics. Provide detailed, actionable risk descriptions in plain language." },
                new { role = "user", content = prompt }
            },
            temperature = 0.2,
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

            return ParseMatchResponse(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI matching failed for regulatory update {UpdateId}", update.Id);
            return new List<MatchResult>();
        }
    }

    private static List<MatchResult> ParseMatchResponse(string jsonContent)
    {
        var results = new List<MatchResult>();
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            if (doc.RootElement.TryGetProperty("matches", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    var docId = item.TryGetProperty("document_id", out var id) ? id.GetString() : null;
                    var risk = item.TryGetProperty("risk_description", out var r) ? r.GetString() : null;

                    if (!string.IsNullOrEmpty(docId))
                    {
                        results.Add(new MatchResult
                        {
                            DocumentId = docId,
                            RiskDescription = risk ?? "This document may be affected by the new regulatory change. Please review."
                        });
                    }
                }
            }
        }
        catch (JsonException) { }

        return results;
    }

    private class DocumentInfo
    {
        public Guid DocumentId { get; set; }
        public long UserId { get; set; }
        public string FileName { get; set; } = "";
        public List<string> Topics { get; set; } = new();
    }

    private class MatchResult
    {
        public string DocumentId { get; set; } = "";
        public string RiskDescription { get; set; } = "";
    }
}

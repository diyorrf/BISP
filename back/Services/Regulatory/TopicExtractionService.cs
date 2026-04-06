using System.Text;
using System.Text.Json;
using back.Data.Entities;
using back.Data.Repos.Interfaces;

namespace back.Services.Regulatory;

public class TopicExtractionService : ITopicExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly IDocumentTopicRepository _topicRepository;
    private readonly ILogger<TopicExtractionService> _logger;

    public TopicExtractionService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IDocumentTopicRepository topicRepository,
        ILogger<TopicExtractionService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key not configured");
        _model = configuration["OpenAI:ChatModel"] ?? "gpt-4o";
        _topicRepository = topicRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DocumentTopic>> ExtractTopicsAsync(
        Guid documentId, string documentContent, CancellationToken ct = default)
    {
        var truncated = documentContent.Length > 12000
            ? documentContent[..12000] + "\n...[truncated]"
            : documentContent;

        var prompt = $$"""
            Analyze the following legal document and extract the main legal TOPICS it covers.
            Topics should be short labels (1-4 words) representing the legal areas, industries, or regulatory domains the document deals with.

            Examples of good topics: "Labor Law", "Tax Compliance", "Data Protection", "Intellectual Property", "Employment Contract", "Corporate Governance", "Import/Export", "Construction Permits", "Real Estate", "Banking Regulations", "Consumer Rights", "Environmental Law".

            Return a JSON object with a "topics" key containing an array of topic strings.
            Return between 3 and 10 topics. If no clear topics are found, return {"topics": []}.

            Document content:
            {{truncated}}
            """;

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "You are a legal document classifier. You extract the main legal topics and regulatory areas from documents. Return concise, standardized topic labels." },
                new { role = "user", content = prompt }
            },
            temperature = 0.1,
            max_tokens = 1000,
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

            var topics = ParseTopics(documentId, content);
            _logger.LogInformation("Extracted {Count} topics from document {DocumentId}", topics.Count, documentId);

            if (topics.Count > 0)
                await _topicRepository.AddRangeAsync(topics, ct);

            return topics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract topics from document {DocumentId}", documentId);
            return Array.Empty<DocumentTopic>();
        }
    }

    private static List<DocumentTopic> ParseTopics(Guid documentId, string jsonContent)
    {
        var topics = new List<DocumentTopic>();

        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            if (doc.RootElement.TryGetProperty("topics", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    var topic = item.GetString()?.Trim();
                    if (!string.IsNullOrEmpty(topic))
                    {
                        topics.Add(new DocumentTopic
                        {
                            Id = Guid.NewGuid(),
                            DocumentId = documentId,
                            Topic = topic,
                            ExtractedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        catch (JsonException) { }

        return topics;
    }
}

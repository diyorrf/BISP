using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using back.Models.DTOs;

namespace back.Services.AI
{
    public class OpenAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger<OpenAIService> _logger;

        private const string SystemPrompt =
            "You are a legal assistant specializing in Uzbekistan law and business regulations. " +
            "You answer questions based on the provided document content. " +
            "Provide thorough, detailed explanations with specific references to relevant articles, clauses, and legal provisions. " +
            "When analyzing risks, explain the practical implications and cite the applicable law. " +
            "Maintain context across the conversation — if the user asks a follow-up, refer back to your previous answers. " +
            "IMPORTANT: Always respond in plain, human-readable text using markdown formatting (headings, bullet points, bold, etc.). " +
            "Never return raw JSON, code blocks containing JSON, or any structured data format. " +
            "Present all information as natural, well-formatted prose and lists that a non-technical user can easily read.";

        public OpenAIService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<OpenAIService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("OpenAI");
            _apiKey = configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI API key not configured");
            _model = configuration["OpenAI:ChatModel"] ?? "gpt-4o";
            _logger = logger;
        }

        public Task<string> GetAnswerAsync(string documentContent, string question, CancellationToken ct = default)
            => GetAnswerAsync(documentContent, question, null, ct);

        public async Task<string> GetAnswerAsync(string documentContent, string question, List<ChatMessageDto>? history, CancellationToken ct = default)
        {
            var messages = BuildMessages(documentContent, question, history);

            var requestBody = new
            {
                model = _model,
                messages,
                temperature = 0.4,
                max_tokens = 4000
            };

            int maxRetries = 3;
            int retryDelay = 2000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
                    {
                        Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                    };
                    request.Headers.Add("Authorization", $"Bearer {_apiKey}");

                    var response = await _httpClient.SendAsync(request, ct);

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        if (i < maxRetries - 1)
                        {
                            _logger.LogWarning("Rate limit hit, retrying in {Delay}ms (attempt {Attempt}/{Max})",
                                retryDelay, i + 1, maxRetries);
                            await Task.Delay(retryDelay, ct);
                            retryDelay *= 2;
                            continue;
                        }

                        _logger.LogError("Rate limit exceeded after {MaxRetries} retries", maxRetries);
                        throw new InvalidOperationException(
                            "OpenAI rate limit exceeded. Please check your API quota and billing at https://platform.openai.com/usage");
                    }

                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync(ct);
                    var jsonDoc = JsonDocument.Parse(responseContent);

                    return jsonDoc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "No answer generated";
                }
                catch (HttpRequestException ex) when (ex.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogError(ex, "Error calling OpenAI API");
                    throw;
                }
            }

            throw new InvalidOperationException("Failed to get response from OpenAI after multiple retries");
        }

        public IAsyncEnumerable<string> GetAnswerStreamAsync(string documentContent, string question, CancellationToken ct = default)
            => GetAnswerStreamAsync(documentContent, question, null, ct);

        public async IAsyncEnumerable<string> GetAnswerStreamAsync(
            string documentContent,
            string question,
            List<ChatMessageDto>? history,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var messages = BuildMessages(documentContent, question, history);

            var requestBody = new
            {
                model = _model,
                messages,
                temperature = 0.4,
                max_tokens = 4000,
                stream = true
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);

                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                    continue;

                var data = line.Substring(6);

                if (data == "[DONE]")
                    break;

                string? contentText = null;
                JsonDocument? jsonDoc = null;

                try
                {
                    jsonDoc = JsonDocument.Parse(data);
                    var delta = jsonDoc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("delta");

                    if (delta.TryGetProperty("content", out var content))
                    {
                        contentText = content.GetString();
                    }
                }
                catch (JsonException)
                {
                    // Skip malformed JSON chunks
                }
                finally
                {
                    jsonDoc?.Dispose();
                }

                if (!string.IsNullOrEmpty(contentText))
                {
                    yield return contentText;
                }
            }
        }

        private static List<object> BuildMessages(string documentContent, string question, List<ChatMessageDto>? history)
        {
            var messages = new List<object>
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = $"Here is the document I want to discuss:\n\n{documentContent}" },
                new { role = "assistant", content = "I've reviewed the document. What would you like to know about it?" }
            };

            if (history is { Count: > 0 })
            {
                foreach (var msg in history)
                {
                    var role = msg.Role?.ToLowerInvariant() switch
                    {
                        "user" => "user",
                        "assistant" => "assistant",
                        _ => (string?)null
                    };
                    if (role != null && !string.IsNullOrWhiteSpace(msg.Content))
                    {
                        messages.Add(new { role, content = msg.Content });
                    }
                }
            }

            messages.Add(new { role = "user", content = question });
            return messages;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace back.Services.AI
{
    public class OpenAIService: IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger<OpenAIService> _logger;

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

        public async Task<string> GetAnswerAsync(string documentContent, string question, CancellationToken ct = default)
        {
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that answers questions based on the provided document content. Be concise and accurate." },
                    new { role = "user", content = $"Document Content:\n{documentContent}\n\nQuestion: {question}" }
                },
                temperature = 0.7,
                max_tokens = 2000
            };

            int maxRetries = 3;
            int retryDelay = 2000; // Start with 2 seconds

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Create a NEW request for each attempt
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
                            retryDelay *= 2; // Exponential backoff
                            continue;
                        }
                        
                        _logger.LogError("Rate limit exceeded after {MaxRetries} retries. Check your OpenAI quota at https://platform.openai.com/usage", maxRetries);
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

        public async IAsyncEnumerable<string> GetAnswerStreamAsync(
            string documentContent, 
            string question, 
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that answers questions based on the provided document content. Be concise and accurate." },
                    new { role = "user", content = $"Document Content:\n{documentContent}\n\nQuestion: {question}" }
                },
                temperature = 0.7,
                max_tokens = 2000,
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
    }
}
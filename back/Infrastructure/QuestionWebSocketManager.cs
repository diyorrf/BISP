using System.Net.WebSockets;
using System.Text.Json;
using back.Extensions;
using back.Models;
using back.Models.DTOs;
using back.Services.Question;

namespace back.Infrastructure
{
    public class QuestionWebSocketManager
    {
        private readonly IQuestionService _questionService;
        private readonly WebSocketHandler _wsHandler;
        private readonly ILogger<QuestionWebSocketManager> _logger;

        public QuestionWebSocketManager(
            IQuestionService questionService,
            WebSocketHandler wsHandler,
            ILogger<QuestionWebSocketManager> logger)
        {
            _questionService = questionService;
            _wsHandler = wsHandler;
            _logger = logger;
        }

        public async Task HandleWebSocketAsync(WebSocket socket, Microsoft.AspNetCore.Http.HttpContext httpContext, CancellationToken ct = default)
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null)
            {
                await _wsHandler.SendMessageAsync(socket, new { error = "Unauthorized" }, ct);
                return;
            }

            _logger.LogInformation("WebSocket connection established for user {UserId}", userId);

            try
            {
                while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
                {
                    var message = await _wsHandler.ReceiveMessageAsync(socket, ct);
                    
                    if (message == null)
                        break;

                    try
                    {
                        var request = JsonSerializer.Deserialize<QuestionRequestDto>(message);
                        
                        if (request == null)
                        {
                            await _wsHandler.SendMessageAsync(socket, new { error = "Invalid request format" }, ct);
                            continue;
                        }

                        await ProcessStreamingQuestionAsync(socket, request, userId.Value, ct);
                    }
                    catch (JsonException)
                    {
                        await _wsHandler.SendMessageAsync(socket, new { error = "Invalid JSON" }, ct);
                    }
                    catch (InsufficientTokensException ex)
                    {
                        await _wsHandler.SendMessageAsync(socket, new { error = ex.Message, code = "TOKENS_EXHAUSTED" }, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing WebSocket message");
                        await _wsHandler.SendMessageAsync(socket, new { error = ex.Message }, ct);
                    }
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogWarning(ex, "WebSocket connection error");
            }
            finally
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Connection closed",
                        CancellationToken.None
                    );
                }
                _logger.LogInformation("WebSocket connection closed");
            }
        }

        private async Task ProcessStreamingQuestionAsync(
            WebSocket socket, 
            QuestionRequestDto request, 
            long userId,
            CancellationToken ct)
        {
            await foreach (var chunk in _questionService.AskQuestionStreamAsync(request, userId, ct))
            {
                await _wsHandler.SendMessageAsync(socket, chunk, ct);
                
                if (chunk.IsComplete)
                    break;
            }
        }
    }
}
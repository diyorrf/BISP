using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace back.Infrastructure
{
    public class WebSocketHandler
    {
        private readonly ILogger<WebSocketHandler> _logger;

        public WebSocketHandler(ILogger<WebSocketHandler> logger)
        {
            _logger = logger;
        }

        public async Task SendMessageAsync(WebSocket socket, object message, CancellationToken ct = default)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                ct
            );
        }

        public async Task<string?> ReceiveMessageAsync(WebSocket socket, CancellationToken ct = default)
        {
            var buffer = new byte[1024 * 4];
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", ct);
                return null;
            }

            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
    }
}
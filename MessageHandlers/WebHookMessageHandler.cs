using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BackgroundQueue.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BackgroundQueue.MessageHandlers
{
    public class WebHookMessageHandler : IMessageHandler
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public WebHookMessageHandler(HttpClient client, ILogger<WebHookMessageHandler> logger) {
            _client = client;
            _logger = logger;
        }

        public async Task ExecuteAsync(long messageId, JObject messageData, CancellationToken cancellation)
        {
            var webHookMessage = messageData.ToObject<WebHookMessage>();
            _logger.LogInformation("Sending {Data} to {EndPoint}", webHookMessage.Payload, webHookMessage.Endpoint);
            
            var response = await _client.PostAsync(
                webHookMessage.Endpoint, 
                new StringContent(webHookMessage.Payload.ToString(), System.Text.Encoding.UTF8, "application/json"),
                cancellation
            );

            _logger.LogInformation("Received response {Status}", response.StatusCode);
            response.EnsureSuccessStatusCode();
        }
    }
}

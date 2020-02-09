using System.Threading;
using System.Threading.Tasks;
using BackgroundQueue.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BackgroundQueue.MessageHandlers
{
    public class ConsoleMessageHandler : IMessageHandler
    {
        private readonly ILogger _logger;
        
        public ConsoleMessageHandler(ILogger<ConsoleMessageHandler> logger) {
            _logger = logger;
        }

        public Task ExecuteAsync(long messageId, JObject messageData, CancellationToken cancellation)
        {
            var consoleMessage = messageData.ToObject<ConsoleMessage>();

            System.Console.ForegroundColor = consoleMessage.Color;
            System.Console.WriteLine(consoleMessage.Message);
            System.Console.ResetColor();
            
            return Task.CompletedTask;
        }
    }
}

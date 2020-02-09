using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BackgroundQueue {
    public interface IMessageHandler {
        Task ExecuteAsync(long messageId, JObject messageData, CancellationToken cancellation);
    }
}
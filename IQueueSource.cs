using System;
using System.Threading.Tasks;

namespace BackgroundQueue {
    public interface IQueueSource {
        Task<MessageBatch> GetMessagesAsync(DateTime currentProcessingTime, int batchSize = 20);
        void RemoveMessage(Message message);
        void UpdateMessage(Message message);
    }
}
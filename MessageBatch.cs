using System;
using System.Collections.Generic;

namespace BackgroundQueue {
    public class MessageBatch : IDisposable {
        private readonly Action _releaseBatch;

        public MessageBatch(IReadOnlyCollection<Message> messages, Action releaseBatch) {
            _releaseBatch = releaseBatch;
            Messages = messages;
        }
        
        public IEnumerable<Message> Messages { get; }

        public void Dispose()
        {
            _releaseBatch();
        }
    }
}
using System;
using Newtonsoft.Json.Linq;

namespace BackgroundQueue.Messages {
    public class WebHookMessage {
        public const string MessageType = nameof(WebHookMessage);

        public Uri Endpoint { get; set; }

        public JObject Payload { get; set; }
    }
}
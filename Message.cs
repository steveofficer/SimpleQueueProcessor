using System;
using Newtonsoft.Json.Linq;

namespace BackgroundQueue
{
    public class Message {
        public string MessageType { get; set; }

        public long MessageId { get; set; }

        public int ErrorCount { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime NextRunTime { get; set; }

        public DateTime LastRunTime { get; set; }

        public JObject Data { get; set; }
    }
}
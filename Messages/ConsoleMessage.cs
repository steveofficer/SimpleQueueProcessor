namespace BackgroundQueue.Messages {
    public class ConsoleMessage {
        public const string MessageType = nameof(ConsoleMessage);

        public string Message { get; set; }

        public System.ConsoleColor Color { get; set; }
    }
}
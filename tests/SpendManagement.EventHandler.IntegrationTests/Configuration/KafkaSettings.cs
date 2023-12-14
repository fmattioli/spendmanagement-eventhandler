namespace SpendManagement.EventHandler.IntegrationTests.Configuration
{
    public sealed class KafkaSettings
    {
        public string Broker { get; set; } = null!;

        public string Environment { get; set; } = null!;

        public int MessageTimeoutMs { get; set; }

        public int InitializationDelay { get; set; }

        public KafkaBatchSettings? Batch { get; set; }
    }

    public class KafkaBatchSettings
    {
        public int WorkerCount { get; set; }

        public int BufferSize { get; set; }

        public int MessageTimeoutSec { get; set; }
    }
}

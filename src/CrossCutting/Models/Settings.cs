using CrossCutting.Models;

namespace Crosscutting.Models
{
    public interface ISettings
    {
        public TracingSettings? TracingSettings { get; }
        public KafkaSettings? KafkaSettings { get; }
        public SqlSettings? SqlSettings { get; }
        public MongoSettings? MongoSettings { get; }
    }

    public record Settings : ISettings
    {
        public TracingSettings? TracingSettings { get; set; }
        public KafkaSettings? KafkaSettings { get; set; }
        public SqlSettings? SqlSettings { get; set; }
        public MongoSettings? MongoSettings { get; set; }
    }
}

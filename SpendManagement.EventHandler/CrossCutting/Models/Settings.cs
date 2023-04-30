namespace Crosscutting.Models
{
    public interface ISettings
    {
        public KafkaSettings KafkaSettings { get; }
        public MongoSettings SqlSettings { get; }
    }

    public record Settings : ISettings
    {
        public KafkaSettings KafkaSettings { get; set; } = null!;
        public MongoSettings SqlSettings { get; set; } = null!;
    }
}

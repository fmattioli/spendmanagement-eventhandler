using Microsoft.Extensions.Configuration;

using SpendManagement.Domain.Integration.Tests.Configuration;

namespace SpendManagement.EventHandler.IntegrationTests.Configuration
{
    public static class TestSettings
    {
        static TestSettings()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile("testsettings.json", false, true)
               .Build();

            PollingSettings = config.GetSection("PollingSettings").Get<PollingSettings>();
            KafkaSettings = config.GetSection("KafkaSettings").Get<KafkaSettings>();
            MongoSettings = config.GetSection("MongoSettings").Get<MongoSettings>();
        }
        public static PollingSettings? PollingSettings { get; set; }
        public static KafkaSettings? KafkaSettings { get; }
        public static MongoSettings? MongoSettings { get; }
    }
}

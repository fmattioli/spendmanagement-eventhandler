namespace SpendManagement.EventHandler.IntegrationTests.Configuration
{
    public record MongoSettings
    {
        public string Database { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
    }
}

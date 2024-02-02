namespace SpendManagement.EventHandler.IntegrationTests.Fixtures
{
    [CollectionDefinition(nameof(SharedFixtureCollection))]
    public class SharedFixtureCollection :
        ICollectionFixture<SharedFixture>,
        ICollectionFixture<KafkaFixture>,
        ICollectionFixture<MongoDBFixture>,
        ICollectionFixture<SqlFixture>
    {
    }
}

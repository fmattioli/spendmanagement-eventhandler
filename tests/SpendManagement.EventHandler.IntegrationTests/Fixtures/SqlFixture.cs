using Dapper;
using Npgsql;
using SpendManagement.EventHandler.IntegrationTests.Configuration;

namespace SpendManagement.EventHandler.IntegrationTests.Fixtures
{
    public class SqlFixture : IAsyncLifetime
    {
        public static async Task<SpendManagementEvent> GetEventAsync(string eventId)
        {
            using var connection = new NpgsqlConnection(TestSettings.SqlSettings?.ConnectionString ?? throw new Exception("ConnectionString not provided."));
            var @event = await connection.QueryFirstOrDefaultAsync<SpendManagementEvent>("SELECT * FROM SpendManagementEvents WHERE RoutingKey = @id", new { id = eventId });
            return @event!;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using var connection = new NpgsqlConnection(TestSettings.SqlSettings?.ConnectionString);
            await connection.ExecuteAsync("DELETE FROM SpendManagementEvents");
        }
    }

    public class SpendManagementEvent(string routingKey, DateTime dataEvent, string nameEvent, string eventBody)
    {
        public string RoutingKey { get; set; } = routingKey;
        public DateTime DataEvent { get; set; } = dataEvent;
        public string NameEvent { get; set; } = nameEvent;
        public string EventBody { get; set; } = eventBody;
    }
}

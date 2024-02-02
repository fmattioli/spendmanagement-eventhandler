﻿using Dapper;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using System.Data.SqlClient;

namespace SpendManagement.EventHandler.IntegrationTests.Fixtures
{
    public class SqlFixture : IAsyncLifetime
    {
        private readonly List<string> routingKeys = [];
        
        public async Task<SpendManagementEvent> GetEventAsync(string eventId)
        {
            using var connection = new SqlConnection(TestSettings.SqlSettings?.ConnectionString);
            {
                var @event = await connection.QueryFirstOrDefaultAsync<SpendManagementEvent>("SELECT * FROM SpendManagementEvents WHERE RoutingKey = @id", new { id = eventId });
                routingKeys.Add(eventId);
                return @event!;
            }
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            if (routingKeys.Count != 0)
            {
                using var connection = new SqlConnection(TestSettings.SqlSettings?.ConnectionString);
                await connection.ExecuteAsync("DELETE FROM SpendManagementEvents WHERE RoutingKey in @ids", new { ids = routingKeys.Select(x => x) });
                await connection.ExecuteAsync("DELETE FROM SpendManagementCommands WHERE RoutingKey in @ids", new { ids = routingKeys.Select(x => x) });
            }
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

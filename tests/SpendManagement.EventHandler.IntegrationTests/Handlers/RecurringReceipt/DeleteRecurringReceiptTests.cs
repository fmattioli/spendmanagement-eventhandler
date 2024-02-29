using AutoFixture;
using FluentAssertions;
using Polly;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;
using SpendManagement.Contracts.V1.Events.RecurringReceiptEvents;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Fixtures;

namespace SpendManagement.EventHandler.IntegrationTests.Handlers.RecurringReceipt
{
    [Collection(nameof(SharedFixtureCollection))]
    public class DeleteRecurringReceiptTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture, SqlFixture sqlFixture)
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture = kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture = monboDBFixture;
        private readonly SqlFixture _sqlFixture = sqlFixture;

        [Fact]
        public async Task OnGivenAValidRecurringReceiptId_DeleteRecurringReceiptEventShouldBeProduced_AndShouldBeConsumedAndReceiptShouldBeDeletedOnDatabase()
        {
            //Arrange
            var receiptId = fixture.Create<Guid>();

            var recurringReceiptFixture = fixture
               .Build<Fixtures.RecurringReceipt>()
               .With(x => x.Id, receiptId)
               .Create();

            await _mongoDBFixture.InsertRecurringReceiptAsync(recurringReceiptFixture);

            var deleteReceiptEvent = fixture
                .Build<DeleteRecurringReceiptEvent>()
                .With(x => x.Id, receiptId)
                .With(x => x.RoutingKey, receiptId.ToString())
                .Create();

            // Act
            await this._kafkaFixture.ProduceEventAsync(deleteReceiptEvent);

            // Assert
            var spendManagementEvent = await Policy
               .HandleResult<SpendManagementEvent>(
                   p => p?.RoutingKey == null)
               .WaitAndRetryAsync(
                   TestSettings.PollingSettings!.RetryCount,
                   _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
               .ExecuteAsync(() => SqlFixture.GetEventAsync(receiptId.ToString()));

            spendManagementEvent
                .Should()
                .NotBeNull();
            
            spendManagementEvent.NameEvent
                .Should()
                .Be(nameof(DeleteRecurringReceiptEvent));

            spendManagementEvent.RoutingKey
                .Should()
                .Be(receiptId.ToString());

            spendManagementEvent.EventBody
                .Should()
                .NotBeNull();

            var receiptInserted = await Policy
                .HandleResult<Fixtures.RecurringReceipt>(
                    p => p?.Id != null)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _mongoDBFixture.FindRecurringReceiptAsync(recurringReceiptFixture.Id));

            receiptInserted
                .Should()
                .BeNull();
        }
    }
}

using AutoFixture;
using FluentAssertions;
using Polly;
using SpendManagement.Contracts.V1.Events.RecurringReceiptEvents;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Fixtures;

namespace SpendManagement.EventHandler.IntegrationTests.Handlers.RecurringReceipt
{
    [Collection(nameof(SharedFixtureCollection))]
    public class AddRecurringReceiptTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture, SqlFixture sqlFixture)
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture = kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture = monboDBFixture;
        private readonly SqlFixture _sqlFixture = sqlFixture;

        [Fact]
        public async Task OnGivenAValidRecurringReceipt_CreateRecurringReceiptEventShouldBeProduced_AndShouldBeConsumedAndInsertedOnDatabase()
        {
            //Arrange
            var receiptId = fixture.Create<Guid>();

            var recurringReceipt = fixture
                .Build< Contracts.Contracts.V1.Entities.RecurringReceipt> ()
                .With(x => x.Id, receiptId)
                .Create();

            _mongoDBFixture.AddRecurringReceiptToCleanUp(receiptId);

            var receiptItems = fixture
                .Build<Contracts.V1.Entities.ReceiptItem>()
                .CreateMany();

            var receiptCreateEvent = fixture
                .Build<CreateRecurringReceiptEvent>()
                .With(x => x.RoutingKey, receiptId.ToString())
                .With(x => x.RecurringReceipt, recurringReceipt)
                .Create();

            // Act
            await this._kafkaFixture.ProduceEventAsync(receiptCreateEvent);

            // Assert
            var spendManagementEvent = await Policy
               .HandleResult<SpendManagementEvent>(
                   p => p?.RoutingKey == null)
               .WaitAndRetryAsync(
                   TestSettings.PollingSettings!.RetryCount,
                   _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
               .ExecuteAsync(() => SqlFixture.GetEventAsync(receiptId.ToString()));

            spendManagementEvent.Should().NotBeNull();
            spendManagementEvent.NameEvent.Should().Be(nameof(CreateRecurringReceiptEvent));
            spendManagementEvent.RoutingKey.Should().Be(receiptId.ToString());
            spendManagementEvent.EventBody.Should().NotBeNull();

            var receiptInserted = await Policy
                .HandleResult<Fixtures.RecurringReceipt>(
                    p => p?.Id == null)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _mongoDBFixture.FindRecurringReceiptAsync(receiptId));

            receiptInserted
                .Should()
                .NotBeNull();

            recurringReceipt.EstablishmentName
                .Should()
                .Be(receiptInserted.EstablishmentName);

            recurringReceipt.Id
                .Should()
                .Be(receiptInserted.Id);
        }
    }
}

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
    public class UpdateRecurringReceiptTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture, SqlFixture sqlFixture)
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture = kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture = monboDBFixture;
        private readonly SqlFixture _sqlFixture = sqlFixture;

        [Fact]
        public async Task OnGivenAValidRecurringReceipt_UpdateRecurringReceiptEventShouldBeProduced_AndShouldBeConsumedAndReceiptShouldBeUpdateOnDatabase()
        {
            //Arrange
            var receiptId = fixture.Create<Guid>();
            var estabilshmentName = fixture.Create<string>();

            var recurringReceipt = fixture
              .Build<Fixtures.RecurringReceipt>()
              .With(x => x.Id, receiptId)
              .Create();

            await _mongoDBFixture.InsertRecurringReceiptAsync(recurringReceipt);

            var establishmentNameUpdated = fixture.Create<string>();

            var recurringReceiptUpdateEvent = fixture
               .Build<Contracts.Contracts.V1.Entities.RecurringReceipt>()
               .With(x => x.Id, receiptId)
               .With(x => x.EstablishmentName, establishmentNameUpdated)
               .Create();

            var updateReceiptEvent = fixture
                .Build<UpdateRecurringReceiptEvent>()
                .With(x => x.RecurringReceipt, recurringReceiptUpdateEvent)
                .With(x => x.RoutingKey, receiptId.ToString())
                .Create();

            // Act
            await this._kafkaFixture.ProduceEventAsync(updateReceiptEvent);

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
                .Be(nameof(UpdateRecurringReceiptEvent));

            spendManagementEvent.RoutingKey
                .Should()
                .Be(receiptId.ToString());

            spendManagementEvent.EventBody
                .Should()
                .NotBeNull();

            var receiptUpdated = await Policy
                .HandleResult<Fixtures.RecurringReceipt>(
                    p => p?.EstablishmentName != establishmentNameUpdated)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _mongoDBFixture.FindRecurringReceiptAsync(recurringReceipt.Id));

            receiptUpdated
                .Should()
                .NotBeNull();

            receiptUpdated.EstablishmentName
                .Should()
                .Be(establishmentNameUpdated);

            receiptUpdated?.CategoryId
                .Should()
                .NotBeEmpty();

            receiptUpdated?.DateInitialRecurrence
                .Should()
                .NotBe(DateTime.MinValue);

            receiptUpdated?.DateEndRecurrence
                .Should()
                .NotBe(DateTime.MinValue);
            
            receiptUpdated?.RecurrenceTotalPrice
                .Should()
                .NotBe(0);
        }
    }
}
using AutoFixture;
using FluentAssertions;
using Polly;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Fixtures;

namespace SpendManagement.EventHandler.IntegrationTests.Handlers.Receipt
{
    [Collection(nameof(SharedFixtureCollection))]
    public class AddReceiptEventHandlerTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture)
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture = kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture = monboDBFixture;

        [Fact]
        public async Task OnGivenAValidReceipt_CreateReceiptEventShouldBeProduced_AndShouldBeConsumedAndInsertedOnDatabase()
        {
            //Arrange
            var receiptId = fixture.Create<Guid>();

            var receipt = fixture
                .Build<Contracts.V1.Entities.Receipt>()
                .With(x => x.Id, receiptId)
                .Create();

            _mongoDBFixture.AddReceiptToCleanUp(receiptId);

            var receiptItems = fixture
                .Build<Contracts.V1.Entities.ReceiptItem>()
                .CreateMany();

            var receiptCreateEvent = fixture
                .Build<CreatedReceiptEvent>()
                .With(x => x.RoutingKey, receiptId.ToString())
                .With(x => x.Receipt, receipt)
                .With(x => x.ReceiptItem, receiptItems)
                .Create();

            // Act
            await this._kafkaFixture.ProduceEventAsync(receiptCreateEvent);

            // Assert
            var receiptInserted = await Policy
                .HandleResult<Fixtures.Receipt>(
                    p => p?.Id == null)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _mongoDBFixture.FindReceiptAsync(receiptId));

            receiptInserted.Should().NotBeNull();
            receipt.EstablishmentName.Should().Be(receiptInserted.EstablishmentName);
            receipt.Id.Should().Be(receiptInserted.Id);
            receiptItems.Count().Should().Be(receiptInserted?.ReceiptItems?.Count());
        }
    }
}

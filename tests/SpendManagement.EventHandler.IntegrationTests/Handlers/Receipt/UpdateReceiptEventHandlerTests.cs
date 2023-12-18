using AutoFixture;
using FluentAssertions;
using Polly;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Fixtures;

namespace SpendManagement.EventHandler.IntegrationTests.Handlers.Receipt
{
    [Collection(nameof(SharedFixtureCollection))]
    public class UpdateReceiptEventHandlerTests
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture;

        public UpdateReceiptEventHandlerTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture)
        {
            this._kafkaFixture = kafkaFixture;
            this._mongoDBFixture = monboDBFixture;
        }

        [Fact]
        public async Task OnGivenAValidReceipt_UpdateReceiptEventShouldBeProduced_AndShouldBeConsumedAndReceiptShouldBeUpdateOnDatabase()
        {
            //Arrange
            var receiptId = fixture.Create<Guid>();
            var estabilshmentName = fixture.Create<string>();

            var receiptItemsFixture = fixture
              .Build<ReceiptItem>()
              .CreateMany(3);

            var receiptFixture = fixture
               .Build<Fixtures.Receipt>()
               .With(x => x.Id, receiptId)
               .With(x => x.EstablishmentName, estabilshmentName)
               .With(x => x.ReceiptItems, receiptItemsFixture)
               .Create();

            await _mongoDBFixture.InsertReceiptAsync(receiptFixture);

            var establishmentNameUpdated = fixture.Create<string>();

            var receiptUpdateEvent = fixture
               .Build<Contracts.V1.Entities.Receipt>()
               .With(x => x.Id, receiptId)
               .With(x => x.EstablishmentName, establishmentNameUpdated)
               .Create();

            var receiptItems = fixture
                .Build<Contracts.V1.Entities.ReceiptItem>()
                .CreateMany(3);

            var updateReceiptEvent = fixture
                .Build<UpdateReceiptEvent>()
                .With(x => x.Receipt, receiptUpdateEvent)
                .With(x => x.ReceiptItems, receiptItems)
                .Create();

            // Act
            await this._kafkaFixture.ProduceEventAsync(updateReceiptEvent);

            // Assert
            var receiptUpdated = await Policy
                .HandleResult<Fixtures.Receipt>(
                    p => p?.EstablishmentName != establishmentNameUpdated)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _mongoDBFixture.FindReceiptAsync(receiptFixture.Id));

            receiptUpdated.Should().NotBeNull();
            receiptUpdated.EstablishmentName.Should().Be(establishmentNameUpdated);
            receiptUpdated?.ReceiptItems?.Count().Should().Be(receiptItemsFixture?.Count());
        }
    }
}

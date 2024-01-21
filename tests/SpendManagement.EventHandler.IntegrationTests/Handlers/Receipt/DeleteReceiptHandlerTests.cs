using AutoFixture;
using FluentAssertions;
using Polly;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Fixtures;

namespace SpendManagement.EventHandler.IntegrationTests.Handlers.Receipt
{
    [Collection(nameof(SharedFixtureCollection))]
    public class DeleteReceiptHandlerTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture)
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture = kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture = monboDBFixture;

        [Fact]
        public async Task OnGivenAValidReceiptId_DeleteReceiptEventShouldBeProduced_AndShouldBeConsumedAndReceiptShouldBeDeletedOnDatabase()
        {
            //Arrange
            var receiptId = fixture.Create<Guid>();

            var receiptFixture = fixture
               .Build<Fixtures.Receipt>()
               .With(x => x.Id, receiptId)
               .Create();

            await _mongoDBFixture.InsertReceiptAsync(receiptFixture);

            var deleteReceiptEvent = fixture
                .Build<DeleteReceiptEvent>()
                .With(x => x.Id, receiptFixture.Id)
                .Create();

            // Act
            await this._kafkaFixture.ProduceEventAsync(deleteReceiptEvent);

            // Assert
            var receiptInserted = await Policy
                .HandleResult<Fixtures.Receipt>(
                    p => p?.Id != null)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _mongoDBFixture.FindReceiptAsync(receiptFixture.Id));

            receiptInserted.Should().BeNull();
        }
    }
}

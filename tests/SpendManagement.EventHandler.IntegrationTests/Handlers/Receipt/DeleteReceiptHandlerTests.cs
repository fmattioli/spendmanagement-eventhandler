using AutoFixture;
using FluentAssertions;
using Polly;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Fixtures;

namespace SpendManagement.EventHandler.IntegrationTests.Handlers.Receipt
{
    [Collection(nameof(SharedFixtureCollection))]
    public class DeleteReceiptHandlerTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture, SqlFixture sqlFixture)
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture = kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture = monboDBFixture;
        private readonly SqlFixture _sqlFixture = sqlFixture;

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
            var spendManagementEvent = await Policy
               .HandleResult<SpendManagementEvent>(
                   p => p?.RoutingKey == null)
               .WaitAndRetryAsync(
                   TestSettings.PollingSettings!.RetryCount,
                   _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
               .ExecuteAsync(() => _sqlFixture.GetEventAsync(receiptId.ToString()));

            spendManagementEvent.Should().NotBeNull();
            spendManagementEvent.NameEvent.Should().Be(nameof(DeleteReceiptEvent));
            spendManagementEvent.RoutingKey.Should().Be(receiptId.ToString());
            spendManagementEvent.EventBody.Should().NotBeNull();

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

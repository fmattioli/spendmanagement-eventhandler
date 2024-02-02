using AutoFixture;
using FluentAssertions;
using Polly;
using SpendManagement.Contracts.V1.Commands.CategoryCommands;
using SpendManagement.Contracts.V1.Events.CategoryEvents;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Fixtures;

namespace SpendManagement.EventHandler.IntegrationTests.Handlers.Category
{
    [Collection(nameof(SharedFixtureCollection))]
    public class AddCategoryEventHandlerTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture, SqlFixture sqlFixture)
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture = kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture = monboDBFixture;
        private readonly SqlFixture _sqlFixture = sqlFixture;

        [Fact]
        public async Task OnGivenAValidCategory_CreateCategoryEventShouldBeProduced_AndShouldBeConsumedAndInsertedOnDatabase()
        {
            //Arrange
            var categoryId = fixture.Create<Guid>();

            _mongoDBFixture.AddCategoryToCleanUp(categoryId);

            var category = fixture
                .Build<Contracts.V1.Entities.Category>()
                .With(x => x.Id, categoryId)
                .With(x => x.CreatedDate, DateTime.UtcNow)
                .Create();

            var categoryEvent = fixture
                .Build<CreateCategoryEvent>()
                .With(x => x.RoutingKey, categoryId.ToString())
                .With(x => x.Category, category)
                .Create();

            // Act
            await this._kafkaFixture.ProduceEventAsync(categoryEvent);

            // Assert
            var categoryInserted = await Policy
                .HandleResult<Fixtures.Category>(
                    p => p?.Id == null)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _mongoDBFixture.FindCategoryAsync(categoryId));

            var spendManagementEvent = await Policy
                .HandleResult<SpendManagementEvent>(
                    p => p?.RoutingKey == null)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _sqlFixture.GetEventAsync(categoryId.ToString()));

            spendManagementEvent.Should().NotBeNull();
            spendManagementEvent.NameEvent.Should().Be(nameof(CreateCategoryEvent));
            spendManagementEvent.RoutingKey.Should().Be(categoryId.ToString());
            spendManagementEvent.EventBody.Should().NotBeNull();

            categoryInserted.Should().NotBeNull();
            category.Name.Should().Be(categoryInserted.Name);
            category.Id.Should().Be(categoryInserted.Id);
            category.CreatedDate.Should().BeSameDateAs(categoryInserted.CreatedDate);
        }
    }
}

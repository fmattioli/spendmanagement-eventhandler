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
    public class AddCategoryEventHandlerTests
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture;

        public AddCategoryEventHandlerTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture)
        {
            this._kafkaFixture = kafkaFixture;
            this._mongoDBFixture = monboDBFixture;
        }

        [Fact]
        public async Task OnGivenAValidCategory_CreateCategoryEventShouldBeProduced_AndShouldBeConsumedAndInsertedOnDatabase()
        {
            //Arrange
            var categoryId = fixture.Create<Guid>();

            var category = fixture
                .Build<Contracts.V1.Entities.Category>()
                .With(x => x.Id, categoryId)
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

            categoryInserted.Should().NotBeNull();
            category.Name.Should().Be(categoryInserted.Name);
            category.Id.Should().Be(categoryInserted.Id);
            category.CreatedDate.Should().BeSameDateAs(categoryInserted.CreatedDate);
        }
    }
}

using AutoFixture;
using FluentAssertions;
using Polly;
using SpendManagement.Contracts.V1.Events.CategoryEvents;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Fixtures;

namespace SpendManagement.EventHandler.IntegrationTests.Handlers.Category
{
    [Collection(nameof(SharedFixtureCollection))]
    public class UpdateCategoryEventHandlerTests
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture;

        public UpdateCategoryEventHandlerTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture)
        {
            this._kafkaFixture = kafkaFixture;
            this._mongoDBFixture = monboDBFixture;
        }

        [Fact]
        public async Task OnGivenAValidCategory_UpdateCategoryEventShouldBeProduced_AndShouldBeConsumedAndCategoryShouldBeUpdateOnDatabase()
        {
            //Arrange
            var categoryId = fixture.Create<Guid>();
            var categoryName = fixture.Create<string>();

            var categoryFixture = fixture
               .Build<Fixtures.Category>()
               .With(x => x.Id, categoryId)
               .With(x => x.Name, categoryName)
               .Create();

            await _mongoDBFixture.InsertCategoryAsync(categoryFixture);

            var categoryNameUpdated = fixture.Create<string>();

            var categoryUpdateEvent = fixture
               .Build<Contracts.V1.Entities.Category>()
               .With(x => x.Id, categoryId)
               .With(x => x.Name, categoryNameUpdated)
               .Create();

            var updateCategoryEvent = fixture
                .Build<UpdateCategoryEvent>()
                .With(x => x.Category, categoryUpdateEvent)
                .Create();

            // Act
            await this._kafkaFixture.ProduceEventAsync(updateCategoryEvent);

            // Assert
            var categoryUpdated = await Policy
                .HandleResult<Fixtures.Category>(
                    p => p?.Name != categoryNameUpdated)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _mongoDBFixture.FindCategoryAsync(categoryFixture.Id));

            categoryUpdated.Should().NotBeNull();
            categoryUpdated.Name.Should().Be(categoryNameUpdated);
        }
    }
}

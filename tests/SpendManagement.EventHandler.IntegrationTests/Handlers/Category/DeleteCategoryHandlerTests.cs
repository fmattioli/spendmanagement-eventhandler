using AutoFixture;

using FluentAssertions;

using Polly;

using SpendManagement.Contracts.V1.Events.CategoryEvents;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Fixtures;

namespace SpendManagement.EventHandler.IntegrationTests.Handlers.Category
{
    [Collection(nameof(SharedFixtureCollection))]
    public class DeleteCategoryHandlerTests(KafkaFixture kafkaFixture, MongoDBFixture monboDBFixture)
    {
        private readonly Fixture fixture = new();
        private readonly KafkaFixture _kafkaFixture = kafkaFixture;
        private readonly MongoDBFixture _mongoDBFixture = monboDBFixture;

        [Fact]
        public async Task OnGivenAValidCategoryId_DeleteCategoryEventShouldBeProduced_AndShouldBeConsumedAndCategoryShouldBeDeletedOnDatabase()
        {
            //Arrange
            var categoryId = fixture.Create<Guid>();
            var categoryFixture = fixture
               .Build<Fixtures.Category>()
               .With(x => x.Id, categoryId)
               .Create();

            await _mongoDBFixture.InsertCategoryAsync(categoryFixture);

            var deleteCategoryEvent = fixture
                .Build<DeleteCategoryEvent>()
                .With(x => x.Id, categoryFixture.Id)
                .Create();

            // Act
            await this._kafkaFixture.ProduceEventAsync(deleteCategoryEvent);

            // Assert
            var categoryInserted = await Policy
                .HandleResult<Fixtures.Category>(
                    p => p?.Id != null)
                .WaitAndRetryAsync(
                    TestSettings.PollingSettings!.RetryCount,
                    _ => TimeSpan.FromMilliseconds(TestSettings.PollingSettings.Delay))
                .ExecuteAsync(() => _mongoDBFixture.FindCategoryAsync(categoryFixture.Id));

            categoryInserted.Should().BeNull();
        }
    }
}

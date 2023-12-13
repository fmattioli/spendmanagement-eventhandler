using Application.Kafka.Handlers.Category;
using AutoFixture;
using Domain.Interfaces;
using KafkaFlow;
using Moq;
using SpendManagement.Contracts.V1.Events.CategoryEvents;

namespace SpendManagement.EventHandler.UnitTests.Handlers.Category
{
    public class CreateCategoryHandlerTests
    {
        private readonly CategoryEventHandler _categoryHandler;
        private readonly Mock<ICategoryRepository> _categoryRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();

        public CreateCategoryHandlerTests()
        {
            _categoryHandler = new CategoryEventHandler(_categoryRepository!.Object);
        }

        [Fact]
        public async Task Given_A_Valid_CreateCategoryEvent_CategoyShouldBeInsertedOnDatabase()
        {
            //Arrange
            var createCategoryEvent = _fixture.Create<CreateCategoryEvent>();

            _categoryRepository
                .Setup(x => x.AddOneAsync(It.IsAny<Domain.Entities.Category>()))
                .Returns(Task.CompletedTask);

            //Act
            await _categoryHandler.Handle(_messageContext.Object, createCategoryEvent);

            //Assert
            _categoryRepository
               .Verify(
                  x => x.AddOneAsync(It.IsAny<Domain.Entities.Category>()),
                   Times.Once);

            _categoryRepository.VerifyNoOtherCalls();
        }
    }
}

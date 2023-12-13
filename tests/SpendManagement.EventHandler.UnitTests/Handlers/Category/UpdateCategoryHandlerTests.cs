using Application.Kafka.Handlers.Category;
using AutoFixture;
using Domain.Interfaces;
using KafkaFlow;
using Moq;
using SpendManagement.Contracts.V1.Events.CategoryEvents;
using System.Linq.Expressions;

namespace SpendManagement.EventHandler.UnitTests.Handlers.Category
{
    public class UpdateCategoryHandlerTests
    {
        private readonly CategoryEventHandler _categoryHandler;
        private readonly Mock<ICategoryRepository> _categoryRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();

        public UpdateCategoryHandlerTests()
        {
            _categoryHandler = new CategoryEventHandler(_categoryRepository!.Object);
        }

        [Fact]
        public async Task Given_A_Valid_UpdateCategoryEvent_CategoyShouldBeUpdatedOnDatabase()
        {
            //Arrange
            var updateCategoryEvent = _fixture.Create<UpdateCategoryEvent>();

            _categoryRepository
                .Setup(x => x.ReplaceOneAsync(It.IsAny<Expression<Func<Domain.Entities.Category, bool>>>(),It.IsAny<Domain.Entities.Category>()))
                .Returns(Task.CompletedTask);

            //Act
            await _categoryHandler.Handle(_messageContext.Object, updateCategoryEvent);

            //Assert
            _categoryRepository
               .Verify(
                  x => x.ReplaceOneAsync(It.IsAny<Expression<Func<Domain.Entities.Category, bool>>>(), It.IsAny<Domain.Entities.Category>()),
                   Times.Once);

            _categoryRepository.VerifyNoOtherCalls();
        }
    }
}

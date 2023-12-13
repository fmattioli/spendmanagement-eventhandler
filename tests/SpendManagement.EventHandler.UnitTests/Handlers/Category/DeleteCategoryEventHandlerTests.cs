using Application.Kafka.Handlers.Category;
using AutoFixture;
using Domain.Interfaces;
using KafkaFlow;
using Moq;
using SpendManagement.Contracts.V1.Events.CategoryEvents;
using System.Linq.Expressions;

namespace SpendManagement.EventHandler.UnitTests.Handlers.Category
{
    public class DeleteCategoryEventHandlerTests
    {
        private readonly CategoryEventHandler _categoryHandler;
        private readonly Mock<ICategoryRepository> _categoryRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();

        public DeleteCategoryEventHandlerTests()
        {
            _categoryHandler = new CategoryEventHandler(_categoryRepository!.Object);
        }

        [Fact]
        public async Task Given_A_Valid_DeleteCategoryEvent_CategoyShouldBeDeletedOnDatabase()
        {
            //Arrange
            var createCategoryEvent = _fixture.Create<DeleteCategoryEvent>();

            _categoryRepository
                .Setup(x => x.DeleteAsync(It.IsAny<Expression<Func<Domain.Entities.Category, bool>>>()))
                .Returns(Task.CompletedTask);

            //Act
            await _categoryHandler.Handle(_messageContext.Object, createCategoryEvent);

            //Assert
            _categoryRepository
               .Verify(
                  x => x.DeleteAsync(It.IsAny<Expression<Func<Domain.Entities.Category, bool>>>()),
                   Times.Once);

            _categoryRepository.VerifyNoOtherCalls();
        }
    }
}

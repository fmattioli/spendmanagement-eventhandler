using Application.Kafka.Handlers.Category;
using AutoFixture;
using Data.Persistence.UnitOfWork;

using Domain.Entities;
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
        private readonly Mock<ISpendManagementEventRepository> _eventRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        public UpdateCategoryHandlerTests()
        {
            _categoryHandler = new CategoryEventHandler(_categoryRepository!.Object, _unitOfWork.Object);
        }

        [Fact]
        public async Task Given_A_Valid_UpdateCategoryEvent_CategoyShouldBeUpdatedOnDatabase()
        {
            //Arrange
            var updateCategoryEvent = _fixture.Create<UpdateCategoryEvent>();

            _categoryRepository
                .Setup(x => x.ReplaceOneAsync(It.IsAny<Expression<Func<Domain.Entities.Category, bool>>>(),It.IsAny<Domain.Entities.Category>()))
                .Returns(Task.CompletedTask);

            _eventRepository
                .Setup(x => x.Add(It.IsAny<SpendManagementEvent>()))
                .ReturnsAsync(_fixture.Create<Guid>());

            //Act
            await _categoryHandler.Handle(_messageContext.Object, updateCategoryEvent);

            //Assert
            _categoryRepository
               .Verify(
                  x => x.ReplaceOneAsync(It.IsAny<Expression<Func<Domain.Entities.Category, bool>>>(), It.IsAny<Domain.Entities.Category>()),
                   Times.Once);

            _eventRepository
               .Verify(
                   x => x.Add(It.IsAny<SpendManagementEvent>()),
                   Times.Once);

            _eventRepository.VerifyNoOtherCalls();
            _categoryRepository.VerifyNoOtherCalls();
        }
    }
}

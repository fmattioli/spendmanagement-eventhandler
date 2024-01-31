﻿using Application.Kafka.Handlers.Category;
using AutoFixture;
using Data.Persistence.UnitOfWork;
using Domain.Entities;
using Domain.Interfaces;
using KafkaFlow;
using Moq;
using Serilog;
using SpendManagement.Contracts.V1.Events.CategoryEvents;
using System.Data;
using System.Linq.Expressions;

namespace SpendManagement.EventHandler.UnitTests.Handlers.Category
{
    public class DeleteCategoryEventHandlerTests
    {
        private readonly CategoryEventHandler _categoryHandler;
        private readonly Mock<ICategoryRepository> _categoryRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();
        private readonly Mock<ISpendManagementEventRepository> _eventRepository = new();
        private readonly Mock<IDbTransaction> _dbTransactionObject = new();
        private readonly Mock<ILogger> _loggerObjetct = new();

        public DeleteCategoryEventHandlerTests()
        {
            var unitOfWork = new UnitOfWork(_dbTransactionObject.Object, _loggerObjetct.Object, _eventRepository.Object);
            _categoryHandler = new CategoryEventHandler(_categoryRepository!.Object, unitOfWork);
        }

        [Fact]
        public async Task Given_A_Valid_DeleteCategoryEvent_CategoyShouldBeDeletedOnDatabase()
        {
            //Arrange
            var createCategoryEvent = _fixture.Create<DeleteCategoryEvent>();

            _categoryRepository
                .Setup(x => x.DeleteAsync(It.IsAny<Expression<Func<Domain.Entities.Category, bool>>>()))
                .Returns(Task.CompletedTask);

            _eventRepository
                .Setup(x => x.Add(It.IsAny<SpendManagementEvent>()))
                .ReturnsAsync(_fixture.Create<Guid>());

            //Act
            await _categoryHandler.Handle(_messageContext.Object, createCategoryEvent);

            //Assert
            _categoryRepository
               .Verify(
                  x => x.DeleteAsync(It.IsAny<Expression<Func<Domain.Entities.Category, bool>>>()),
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

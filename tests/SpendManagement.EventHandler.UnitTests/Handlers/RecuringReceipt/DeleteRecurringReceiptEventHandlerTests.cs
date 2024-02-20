using Application.Kafka.Handlers.RecurringReceipt;
using AutoFixture;
using Data.Persistence.UnitOfWork;
using Domain.Entities;
using Domain.Interfaces;
using KafkaFlow;
using Moq;
using Serilog;
using SpendManagement.Contracts.V1.Events.RecurringReceiptEvents;
using System.Data;
using System.Linq.Expressions;

namespace SpendManagement.EventHandler.UnitTests.Handlers.RecuringReceipt
{
    public class DeleteRecurringReceiptEventHandlerTests
    {
        private readonly RecurringReceiptEventHandler _receiptEventHandler;
        private readonly Mock<IRecurringReceiptRepository> _recuringReceiptRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();
        private readonly Mock<ISpendManagementEventRepository> _eventRepository = new();
        private readonly Mock<IDbTransaction> _dbTransactionObject = new();
        private readonly Mock<ILogger> _loggerObjetct = new();
        public DeleteRecurringReceiptEventHandlerTests()
        {
            var unitOfWork = new UnitOfWork(_dbTransactionObject.Object, _loggerObjetct.Object, _eventRepository.Object);
            _receiptEventHandler = new RecurringReceiptEventHandler(_recuringReceiptRepository!.Object, unitOfWork);
        }

        [Fact]
        public async Task Given_A_Valid_DeleteReceiptEvent_ReceiptShouldBeInsertedOnDatabase()
        {
            //Arrange
            var deleteReceiptEvent = _fixture.Create<DeleteRecurringReceiptEvent>();

            _recuringReceiptRepository
                .Setup(x => x.AddOneAsync(It.IsAny<RecurringReceipt>()))
                .Returns(Task.CompletedTask);

            _eventRepository
               .Setup(x => x.Add(It.IsAny<SpendManagementEvent>()))
               .ReturnsAsync(_fixture.Create<Guid>());

            //Act
            await _receiptEventHandler.Handle(_messageContext.Object, deleteReceiptEvent);

            //Assert
            _recuringReceiptRepository
               .Verify(
                  x => x.DeleteAsync(It.IsAny<Expression<Func<RecurringReceipt, bool>>>()),
                   Times.Once);

            _eventRepository
               .Verify(
                   x => x.Add(It.IsAny<SpendManagementEvent>()),
                   Times.Once);

            _eventRepository.VerifyNoOtherCalls();
            _recuringReceiptRepository.VerifyNoOtherCalls();
        }
    }
}

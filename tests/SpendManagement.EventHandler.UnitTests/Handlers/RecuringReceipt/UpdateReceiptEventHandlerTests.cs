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
    public class UpdateReceiptEventHandlerTests
    {
        private readonly RecurringReceiptEventHandler _recurringReceiptEventHandler;
        private readonly Mock<IRecurringReceiptRepository> _recurringReceiptRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();
        private readonly Mock<IDbTransaction> _dbTransactionObject = new();
        private readonly Mock<ILogger> _loggerObjetct = new();
        private readonly Mock<ISpendManagementEventRepository> _eventRepository = new();

        public UpdateReceiptEventHandlerTests()
        {
            var unitOfWork = new UnitOfWork(_dbTransactionObject.Object, _loggerObjetct.Object, _eventRepository.Object);
            _recurringReceiptEventHandler = new RecurringReceiptEventHandler(_recurringReceiptRepository!.Object, unitOfWork);
        }

        [Fact]
        public async Task Given_A_Valid_UpdateReceiptEvent_ReceiptShouldBeInsertedOnDatabase()
        {
            //Arrange
            var updateReceiptEvent = _fixture.Create<UpdateRecurringReceiptEvent>();

            _recurringReceiptRepository
                .Setup(x => x.AddOneAsync(It.IsAny<RecurringReceipt>()))
                .Returns(Task.CompletedTask);

            _eventRepository
               .Setup(x => x.Add(It.IsAny<SpendManagementEvent>()))
               .ReturnsAsync(_fixture.Create<Guid>());

            //Act
            await _recurringReceiptEventHandler.Handle(_messageContext.Object, updateReceiptEvent);

            //Assert
            _recurringReceiptRepository
               .Verify(
                  x => x.ReplaceOneAsync(It.IsAny<Expression<Func<RecurringReceipt, bool>>>(), It.IsAny<RecurringReceipt>()),
                   Times.Once);

            _eventRepository
               .Verify(
                   x => x.Add(It.IsAny<SpendManagementEvent>()),
                   Times.Once);

            _eventRepository.VerifyNoOtherCalls();
            _recurringReceiptRepository.VerifyNoOtherCalls();
        }
    }
}

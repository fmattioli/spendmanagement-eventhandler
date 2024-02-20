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

namespace SpendManagement.EventHandler.UnitTests.Handlers.RecuringReceipt
{

    public class CreateRecurringReceiptEventHandlerTests
    {
        private readonly RecurringReceiptEventHandler _receiptEventHandler;
        private readonly Mock<IRecurringReceiptRepository> _receiptRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();
        private readonly Mock<ISpendManagementEventRepository> _eventRepository = new();
        private readonly Mock<IDbTransaction> _dbTransactionObject = new();
        private readonly Mock<ILogger> _loggerObjetct = new();

        public CreateRecurringReceiptEventHandlerTests()
        {
            var unitOfWork = new UnitOfWork(_dbTransactionObject.Object, _loggerObjetct.Object, _eventRepository.Object);
            _receiptEventHandler = new RecurringReceiptEventHandler(_receiptRepository!.Object, unitOfWork);
        }

        [Fact]
        public async Task Given_A_Valid_CreateReceiptEvent_ReceiptShouldBeInsertedOnDatabase()
        {
            //Arrange
            var createRecurringReceipt = _fixture.Create<CreateRecurringReceiptEvent>();

            _receiptRepository
                .Setup(x => x.AddOneAsync(It.IsAny<RecurringReceipt>()))
                .Returns(Task.CompletedTask);

            _eventRepository
                .Setup(x => x.Add(It.IsAny<SpendManagementEvent>()))
                .ReturnsAsync(_fixture.Create<Guid>());

            //Act
            await _receiptEventHandler.Handle(_messageContext.Object, createRecurringReceipt);

            //Assert
            _receiptRepository
               .Verify(
                  x => x.AddOneAsync(It.IsAny<RecurringReceipt>()),
                   Times.Once);

            _eventRepository
               .Verify(
                   x => x.Add(It.IsAny<SpendManagementEvent>()),
                   Times.Once);

            _eventRepository.VerifyNoOtherCalls();
            _receiptRepository.VerifyNoOtherCalls();
        }
    }
}

using Application.Kafka.Handlers.Receipt;
using AutoFixture;
using Data.Persistence.UnitOfWork;
using Domain.Entities;
using Domain.Interfaces;
using KafkaFlow;
using Moq;

using Serilog;

using SpendManagement.Contracts.V1.Events.ReceiptEvents;

using System.Data;

namespace SpendManagement.EventHandler.UnitTests.Handlers.Receipt
{
    public class CreateReceiptEventHandlerTests
    {
        private readonly ReceiptEventHandler _receiptEventHandler;
        private readonly Mock<IReceiptRepository> _receiptRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ISpendManagementEventRepository> _eventRepository = new();
        private readonly Mock<IDbTransaction> _dbTransactionObject = new();
        private readonly Mock<ILogger> _loggerObjetct = new();

        public CreateReceiptEventHandlerTests()
        {
            var unitOfWork = new UnitOfWork(_dbTransactionObject.Object, _loggerObjetct.Object, _eventRepository.Object);
            _receiptEventHandler = new ReceiptEventHandler(_receiptRepository!.Object, unitOfWork);
        }

        [Fact]
        public async Task Given_A_Valid_CreateReceiptEvent_ReceiptShouldBeInsertedOnDatabase()
        {
            //Arrange
            var createReceiptEvent = _fixture.Create<CreatedReceiptEvent>();

            _receiptRepository
                .Setup(x => x.AddOneAsync(It.IsAny<Domain.Entities.Receipt>()))
                .Returns(Task.CompletedTask);

            _eventRepository
                .Setup(x => x.Add(It.IsAny<SpendManagementEvent>()))
                .ReturnsAsync(_fixture.Create<Guid>());

            //Act
            await _receiptEventHandler.Handle(_messageContext.Object, createReceiptEvent);

            //Assert
            _receiptRepository
               .Verify(
                  x => x.AddOneAsync(It.IsAny<Domain.Entities.Receipt>()),
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

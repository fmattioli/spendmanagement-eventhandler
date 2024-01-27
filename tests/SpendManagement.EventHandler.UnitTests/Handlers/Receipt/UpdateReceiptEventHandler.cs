using Application.Kafka.Handlers.Receipt;
using AutoFixture;
using Data.Persistence.UnitOfWork;
using Domain.Entities;
using Domain.Interfaces;
using KafkaFlow;
using Moq;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;
using System.Linq.Expressions;

namespace SpendManagement.EventHandler.UnitTests.Handlers.Receipt
{
    public class UpdateReceiptEventHandler
    {
        private readonly ReceiptEventHandler _receiptEventHandler;
        private readonly Mock<IReceiptRepository> _receiptRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ISpendManagementEventRepository> _eventRepository = new();
        public UpdateReceiptEventHandler()
        {
            _receiptEventHandler = new ReceiptEventHandler(_receiptRepository!.Object, _unitOfWork.Object);
        }

        [Fact]
        public async Task Given_A_Valid_UpdateReceiptEvent_ReceiptShouldBeInsertedOnDatabase()
        {
            //Arrange
            var updateReceiptEvent = _fixture.Create<UpdateReceiptEvent>();

            _receiptRepository
                .Setup(x => x.AddOneAsync(It.IsAny<Domain.Entities.Receipt>()))
                .Returns(Task.CompletedTask);

            _eventRepository
               .Setup(x => x.Add(It.IsAny<SpendManagementEvent>()))
               .ReturnsAsync(_fixture.Create<Guid>());

            //Act
            await _receiptEventHandler.Handle(_messageContext.Object, updateReceiptEvent);

            //Assert
            _receiptRepository
               .Verify(
                  x => x.ReplaceOneAsync(It.IsAny<Expression<Func<Domain.Entities.Receipt, bool>>>(), It.IsAny<Domain.Entities.Receipt>()),
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

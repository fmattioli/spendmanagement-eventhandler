using Application.Kafka.Handlers.Receipt;
using AutoFixture;
using Domain.Interfaces;
using KafkaFlow;
using Moq;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;

namespace SpendManagement.EventHandler.UnitTests.Handlers.Receipt
{
    public class DeleteReceiptEventHandler
    {
        private readonly ReceiptEventHandler _receiptEventHandler;
        private readonly Mock<IReceiptRepository> _receiptRepository = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IMessageContext> _messageContext = new();

        public DeleteReceiptEventHandler()
        {
            _receiptEventHandler = new ReceiptEventHandler(_receiptRepository!.Object);
        }

        [Fact]
        public async Task Given_A_Valid_DeleteReceiptEvent_ReceiptShouldBeInsertedOnDatabase()
        {
            //Arrange
            var deleteReceiptEvent = _fixture.Create<DeleteReceiptEvent>();

            _receiptRepository
                .Setup(x => x.AddOneAsync(It.IsAny<Domain.Entities.Receipt>()))
                .Returns(Task.CompletedTask);

            //Act
            await _receiptEventHandler.Handle(_messageContext.Object, deleteReceiptEvent);

            //Assert
            _receiptRepository
               .Verify(
                  x => x.AddOneAsync(It.IsAny<Domain.Entities.Receipt>()),
                   Times.Once);

            _receiptRepository.VerifyNoOtherCalls();
        }
    }
}

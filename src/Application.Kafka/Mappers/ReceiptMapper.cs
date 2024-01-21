using Domain.Entities;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;

namespace Application.Kafka.Mappers
{
    public static class ReceiptMapper
    {
        public static Receipt ToDomain(this CreatedReceiptEvent receiptCreatedEvent)
        {
            var receiptItems = receiptCreatedEvent.ReceiptItem.Select(x => new Domain.ValueObjects.ReceiptItem
            {
                Id = x.Id,
                ItemName = x.ItemName,
                ItemPrice = x.ItemPrice,
                Observation = x.Observation,
                Quantity = x.Quantity,
                TotalPrice = x.TotalPrice,
                ItemDiscount = x.ItemDiscount
            });

            return new Receipt(
                receiptCreatedEvent.Receipt.Id,
                receiptCreatedEvent.Receipt.CategoryId,
                receiptCreatedEvent.Receipt.EstablishmentName,
                receiptCreatedEvent.Receipt.ReceiptDate,
                receiptItems,
                receiptCreatedEvent.Receipt.Discount,
                receiptCreatedEvent.Receipt.Total);
        }

        public static Receipt ToDomain(this UpdateReceiptEvent updateReceiptEvent)
        {
            var updatedReceiptItems = updateReceiptEvent.ReceiptItems.Select(x => new Domain.ValueObjects.ReceiptItem
            {
                Id = x.Id,
                ItemName = x.ItemName,
                ItemPrice = x.ItemPrice,
                Observation = x.Observation,
                Quantity = x.Quantity,
                TotalPrice = x.TotalPrice,
                ItemDiscount = x.ItemDiscount
            });

            return new Receipt(
                updateReceiptEvent.Receipt.Id,
                updateReceiptEvent.Receipt.CategoryId,
                updateReceiptEvent.Receipt.EstablishmentName,
                updateReceiptEvent.Receipt.ReceiptDate,
                updatedReceiptItems,
                updateReceiptEvent.Receipt.Discount,
                updateReceiptEvent.Receipt.Total);
        }
    }
}

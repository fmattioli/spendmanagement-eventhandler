using Domain.Entities;
using Newtonsoft.Json;
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

        public static SpendManagementEvent ToSpendManagementEvent(this CreatedReceiptEvent createReceiptEvent)
        {
            return new SpendManagementEvent(
                createReceiptEvent.RoutingKey,
                createReceiptEvent.EventCreatedDate,
                nameof(CreatedReceiptEvent),
                JsonConvert.SerializeObject(createReceiptEvent));
        }

        public static SpendManagementEvent ToSpendManagementEvent(this UpdateReceiptEvent updateReceiptEvent)
        {
            return new SpendManagementEvent(
                updateReceiptEvent.RoutingKey,
                updateReceiptEvent.EventCreatedDate,
                nameof(UpdateReceiptEvent),
                JsonConvert.SerializeObject(updateReceiptEvent));
        }

        public static SpendManagementEvent ToSpendManagementEvent(this DeleteReceiptEvent deleteReceiptEvent)
        {
            return new SpendManagementEvent(
                deleteReceiptEvent.RoutingKey,
                deleteReceiptEvent.EventCreatedDate,
                nameof(DeleteReceiptEvent),
                JsonConvert.SerializeObject(deleteReceiptEvent));
        }
    }
}

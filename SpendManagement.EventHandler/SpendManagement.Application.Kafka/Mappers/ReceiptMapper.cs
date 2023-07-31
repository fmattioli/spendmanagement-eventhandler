using Domain.Entities;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;

namespace Application.Kafka.Mappers
{
    public static class ReceiptMapper
    {
        public static Receipt ToDomain(this CreatedReceiptEvent createReceiptCommand)
        {
            return new Receipt(
                createReceiptCommand.Receipt.Id,
                createReceiptCommand.Receipt.EstablishmentName,
                createReceiptCommand.Receipt.ReceiptDate,
                createReceiptCommand.ReceiptItem.Select(x => new Domain.ValueObjects.ReceiptItem
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    ItemName = x.ItemName,
                    ItemPrice = x.ItemPrice,
                    Observation = x.Observation,
                    Quantity = x.Quantity,
                    TotalPrice = x.TotalPrice
                })
            );
        }
    }
}

using Domain.Entities;
using SpendManagement.Contracts.V1.Events;

namespace Application.Kafka.Mappers
{
    public static class ReceiptMapper
    {
        public static Receipt ToDomain(this CreatedReceiptEvent createReceiptCommand)
        {
            return new Receipt(
                createReceiptCommand.Id,
                createReceiptCommand.EstablishmentName,
                createReceiptCommand.ReceiptDate,
                createReceiptCommand.ReceiptItems.Select(x => new Domain.ValueObjects.ReceiptItem
                {
                    Id = x.Id,
                    ItemName = x.ItemName,
                    ItemPrice = x.ItemPrice,
                    Observation = x.Observation,
                    Quantity = x.Quantity
                })
            );
        }
    }
}

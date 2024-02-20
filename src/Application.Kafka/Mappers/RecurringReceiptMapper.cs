using Domain.Entities;
using Newtonsoft.Json;
using SpendManagement.Contracts.V1.Events.RecurringReceiptEvents;

namespace Application.Kafka.Mappers
{
    public static class RecurringReceiptMapper
    {
        public static RecurringReceipt ToDomain(this CreateRecurringReceiptEvent recurringReceiptCreatedEvent)
        {
            return new RecurringReceipt(
                recurringReceiptCreatedEvent.RecurringReceipt.Id,
                recurringReceiptCreatedEvent.RecurringReceipt.CategoryId,
                recurringReceiptCreatedEvent.RecurringReceipt.EstablishmentName,
                recurringReceiptCreatedEvent.RecurringReceipt.DateInitialRecurrence,
                recurringReceiptCreatedEvent.RecurringReceipt.DateEndRecurrence,
                recurringReceiptCreatedEvent.RecurringReceipt.RecurrenceTotalPrice,
                recurringReceiptCreatedEvent.RecurringReceipt.Observation);
        }

        public static RecurringReceipt ToDomain(this UpdateRecurringReceiptEvent updateRecurringReceiptEvent)
        {
            return new RecurringReceipt(
                updateRecurringReceiptEvent.RecurringReceipt.Id,
                updateRecurringReceiptEvent.RecurringReceipt.CategoryId,
                updateRecurringReceiptEvent.RecurringReceipt.EstablishmentName,
                updateRecurringReceiptEvent.RecurringReceipt.DateInitialRecurrence,
                updateRecurringReceiptEvent.RecurringReceipt.DateEndRecurrence,
                updateRecurringReceiptEvent.RecurringReceipt.RecurrenceTotalPrice,
                updateRecurringReceiptEvent.RecurringReceipt.Observation);
        }

        public static SpendManagementEvent ToSpendManagementEvent(this CreateRecurringReceiptEvent createRecurringReceiptEvent)
        {
            return new SpendManagementEvent(
                createRecurringReceiptEvent.RoutingKey,
                createRecurringReceiptEvent.EventCreatedDate,
                nameof(CreateRecurringReceiptEvent),
                JsonConvert.SerializeObject(createRecurringReceiptEvent));
        }

        public static SpendManagementEvent ToSpendManagementEvent(this UpdateRecurringReceiptEvent updateRecurringReceiptEvent)
        {
            return new SpendManagementEvent(
                updateRecurringReceiptEvent.RoutingKey,
                updateRecurringReceiptEvent.EventCreatedDate,
                nameof(UpdateRecurringReceiptEvent),
                JsonConvert.SerializeObject(updateRecurringReceiptEvent));
        }

        public static SpendManagementEvent ToSpendManagementEvent(this DeleteRecurringReceiptEvent deleteRecurringReceiptEvent)
        {
            return new SpendManagementEvent(
                deleteRecurringReceiptEvent.RoutingKey,
                deleteRecurringReceiptEvent.EventCreatedDate,
                nameof(DeleteRecurringReceiptEvent),
                JsonConvert.SerializeObject(deleteRecurringReceiptEvent));
        }
    }
}

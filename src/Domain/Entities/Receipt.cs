using Domain.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class Receipt(Guid id, Guid categoryId, string establishmentName, DateTime receiptDate, IEnumerable<ReceiptItem> receiptItems, decimal discount, decimal total)
    {
        [BsonId]
        public Guid Id { get; set; } = id;
        public Guid CategoryId { get; set; } = categoryId;
        public string EstablishmentName { get; set; } = establishmentName;
        public DateTime ReceiptDate { get; set; } = receiptDate;
        public IEnumerable<ReceiptItem> ReceiptItems { get; set; } = receiptItems;
        public decimal Discount { get; set; } = discount;
        public decimal Total { get; set; } = total;
    }
}

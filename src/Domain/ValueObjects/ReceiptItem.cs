namespace Domain.ValueObjects
{
    public class ReceiptItem
    {
        public Guid Id { get; set; }
        public string ItemName { get; set; } = null!;
        public short Quantity { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Observation { get; set; } = null!;
    }
}

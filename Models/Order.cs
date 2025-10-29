namespace OrderTracker.Models
{
    public class Order
    {
        public int Id { get; set; }               // PK, identity
        public string Product { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateOnly OrderDate { get; set; }
        public int Amount { get; set; }

        public List<OrderHistory> History { get; set; } = new();
    }

    public class OrderHistory
    {
        public int Id { get; set; }  // This is the PK

        public int OrderId { get; set; }  // FK to Order
        public Order Order { get; set; } = null!;

        public string Status { get; set; } = null!;
        public DateOnly ChangedAt { get; set; }
        public string ChangedBy { get; set; } = null!;
    }
}

namespace ERP.Domain.Entities
{
    public class Order : BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        // Sipariþ kalemleri
        public List<OrderItem> Items { get; set; } = new();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }

    

}

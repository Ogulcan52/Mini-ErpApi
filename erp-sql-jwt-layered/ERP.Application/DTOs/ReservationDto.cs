namespace ERP.Application.DTOs
{
    public class ReservationItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class ReservationReadDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public string StatusMessage { get; set; }
        public List<ReservationItemDto> Items { get; set; } = new();
    }

    public class CreateReservationRequestDto
    {
        
        public int CustomerId { get; set; }
        public Dictionary<string, int> ItemQuantities { get; set; } = new();
        public string Duration { get; set; } = "00:00:00"; 
    }

    public class UpdateReservationRequestDto
    {
        public Dictionary<string, int> ItemQuantities { get; set; } = new();
        public string Duration { get; set; } = "00:00:00";
        
    }
}
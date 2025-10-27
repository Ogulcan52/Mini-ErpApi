using System;
using System.Collections.Generic;

namespace ERP.Domain.Entities
{
    public enum ReservationStatus
    {
        Pending,
        Completed,
        Expired
    }

    public class Reservation : BaseEntity
    {
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpireAt { get; set; }

        // ✅ Yeni Status alanı, artık set edilebilir
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // ✅ Read-only DTO için kullanılacak property
        public string StatusMessage
        {
            get
            {
                if (Status == ReservationStatus.Completed)
                    return "Reservation Completed";

                if (DateTime.UtcNow > ExpireAt || Status == ReservationStatus.Expired)
                    return "Reservation Expired";

                return "Reservation Active"; // Pending
            }
        }

        public List<ReservationItem> Items { get; set; } = new();
    }

    public class ReservationItem : BaseEntity
    {
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
    }
}

namespace ERP.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Stock { get; set; }
        public int ReservedStock { get; set; } = 0;
        public bool IsActive { get; set; } = true;
      

    }
}

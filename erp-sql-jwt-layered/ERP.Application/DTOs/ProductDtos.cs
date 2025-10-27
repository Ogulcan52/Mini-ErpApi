namespace ERP.Application.DTOs
{
    public record ProductReadDto(int Id, string Name, string Sku, decimal UnitPrice, int Stock, bool IsActive, int ReservedStock);
    public record ProductCreateDto(string Name, string Sku, decimal UnitPrice, int Stock);
    public record ProductUpdateDto(string Name, decimal UnitPrice, int Stock, bool IsActive);
}

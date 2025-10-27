using System.Text.Json.Serialization;

namespace ERP.Application.DTOs
{
    public record OrderItemCreateDto(
        [property: JsonPropertyName("productName")] string ProductName,
        [property: JsonPropertyName("quantity")] int Quantity
    );

    public record OrderItemUpdateDto(
        [property: JsonPropertyName("CustomerId")] int CustomerId,
        [property: JsonPropertyName("items")] List<OrderItemCreateDto> Items
        );
   



    public record OrderReadDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("customerId")] int CustomerId,
        [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("items")] List<OrderItemReadDto> Items
    );

    public record OrderItemReadDto(
        [property: JsonPropertyName("productId")] int ProductId,
        [property: JsonPropertyName("productName")] string ProductName,
        [property: JsonPropertyName("quantity")] int Quantity,
        [property: JsonPropertyName("unitPrice")] decimal UnitPrice,
        [property: JsonPropertyName("lineTotal")] decimal LineTotal
    );

    public record OrderCreateDto(
        [property: JsonPropertyName("customerId")] int CustomerId,
        [property: JsonPropertyName("items")] List<OrderItemCreateDto> Items
       
    );
}

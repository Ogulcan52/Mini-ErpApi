using System.Dynamic;
using ERP.Application.DTOs;
using ERP.Domain.Entities.RequestFeatures;

namespace ERP.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderReadDto>> GetAllAsync();
        Task<OrderReadDto?> GetAsync(int id);
        Task<int> CreateAsync(OrderCreateDto dto);
        Task<bool>UpdateAsync(int orderId, OrderItemUpdateDto request);
        Task CompleteAsync(int id);
        Task DeleteAsync(int id);
    }
}

using System.Dynamic;
using ERP.Application.DTOs;
using ERP.Domain.Entities.RequestFeatures;

namespace ERP.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerReadDto>> GetAllAsync();
        Task<CustomerReadDto?> GetAsync(int id);
        Task<CustomerReadDto> CreateAsync(CustomerCreateDto dto);
        Task UpdateAsync(int id, CustomerUpdateDto dto);
        Task DeleteAsync(int id);
        Task<QueryResult<ExpandoObject>> GetCustomersAsync(QueryParams queryParams);

    }
}

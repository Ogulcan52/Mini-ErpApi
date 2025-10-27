using System.Dynamic;
using ERP.Application.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Entities.RequestFeatures;

namespace ERP.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductReadDto>> GetAllAsync();
        Task<ProductReadDto?> GetAsync(int id);
        Task<ProductReadDto> CreateAsync(ProductCreateDto dto);
        Task UpdateAsync(int id, ProductUpdateDto dto);
        Task DeleteAsync(int id);
        Task<QueryResult<ExpandoObject>> GetProductsAsync(QueryParams queryParams);
        




    }
}

using System.Dynamic;
using AutoMapper;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Application.Sepicifications;
using ERP.Domain.Entities;
using ERP.Domain.Entities.RequestFeatures;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDataShaper<ProductReadDto> _dataShaper;



        public ProductService(AppDbContext db, IMapper mapper,IDataShaper<ProductReadDto> dataShaper)
        {
            _db = db; _mapper = mapper;_dataShaper = dataShaper;    
        }

        public async Task<ProductReadDto> CreateAsync(ProductCreateDto dto)
        {
            var exists = await _db.Products.AnyAsync(p => p.Sku == dto.Sku);
            if (exists) throw new InvalidOperationException("SKU zaten mevcut.");
            var entity = _mapper.Map<Product>(dto);
            _db.Products.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<ProductReadDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _db.Products.FindAsync(id);
            if (e is null) throw new KeyNotFoundException();
            _db.Products.Remove(e);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductReadDto>> GetAllAsync()
        {
            var list = await _db.Products.AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<ProductReadDto>>(list);
        }

        public async Task<ProductReadDto?> GetAsync(int id)
        {
            var e = await _db.Products.FindAsync(id);
            return e is null ? null : _mapper.Map<ProductReadDto>(e);
        }


        public async Task<QueryResult<ExpandoObject>> GetProductsAsync(QueryParams queryParams)
        {
            // Specification oluþtur
            var spec = new DynamicSpecification<Product>(queryParams);
            var query = SpecificationEvaluator<Product>.GetQuery(_db.Products.AsQueryable(), spec);

            // Toplam kayýt sayýsýný al (pagination'dan önce)
            var totalRecords = await query.CountAsync();

            // Pagination uygula
            if (queryParams.PageNumber > 0 && queryParams.PageSize > 0)
            {
                query = query
                    .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize);
            }

            // Veriyi getir
            var products = await query.ToListAsync();

            // DTO'ya map et
            var dtos = _mapper.Map<List<ProductReadDto>>(products);

            // Data shaping uygula
            var shapedData = _dataShaper.ShapeData(
                dtos,
                queryParams.SelectFields != null ? string.Join(",", queryParams.SelectFields) : ""
            );

            // Sonuç döndür
            return new QueryResult<ExpandoObject>
            {
                Data = shapedData.ToList(),
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)(queryParams.PageSize > 0 ? queryParams.PageSize : totalRecords)),
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize
            };
        }

        public async Task UpdateAsync(int id, ProductUpdateDto dto)
        {
            var e = await _db.Products.FindAsync(id);
            if (e is null) throw new KeyNotFoundException();
            _mapper.Map(dto, e);
            e.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }



        


    }

}


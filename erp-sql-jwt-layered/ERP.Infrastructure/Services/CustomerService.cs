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
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDataShaper<CustomerReadDto> _dataShaper;

        public CustomerService(AppDbContext db, IMapper mapper, IDataShaper<CustomerReadDto> dataShaper)
        {
            _db = db; _mapper = mapper;_dataShaper = dataShaper;
        }

        public async Task<CustomerReadDto> CreateAsync(CustomerCreateDto dto)
        {
            var entity = _mapper.Map<Customer>(dto);
            _db.Customers.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<CustomerReadDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _db.Customers.FindAsync(id);
            if (e is null) throw new KeyNotFoundException();
            _db.Customers.Remove(e);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerReadDto>> GetAllAsync()
        {
            var list = await _db.Customers.AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<CustomerReadDto>>(list);
        }

        public async Task<CustomerReadDto?> GetAsync(int id)
        {
            var e = await _db.Customers.FindAsync(id);
            return e is null ? null : _mapper.Map<CustomerReadDto>(e);
        }

        public async Task<QueryResult<ExpandoObject>> GetCustomersAsync(QueryParams queryParams)
        {
            var spec = new DynamicSpecification<Customer>(queryParams);


            var query = SpecificationEvaluator<Customer>.GetQuery(_db.Customers.AsQueryable(), spec);

            var count = await query.CountAsync();

            var customers = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();
            if (queryParams.PageNumber > 0 && queryParams.PageSize > 0)
            {
                query = query
                    .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize);
            }

            customers = await query.ToListAsync();

            // DTO'ya map et
            var dtos = _mapper.Map<List<CustomerReadDto>>(customers);

            // Data shaping uygula
            var shapedData = _dataShaper.ShapeData(
                dtos,
                queryParams.SelectFields != null ? string.Join(",", queryParams.SelectFields) : ""
            );

            return new QueryResult<ExpandoObject>
            {
                Data = shapedData.ToList(),
                TotalRecords = count,
                TotalPages = (int)Math.Ceiling(count / (double)(queryParams.PageSize > 0 ? queryParams.PageSize : count)),
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize
            };
        }

        public async Task UpdateAsync(int id, CustomerUpdateDto dto)
        {
            var e = await _db.Customers.FindAsync(id);
            if (e is null) throw new KeyNotFoundException();
            _mapper.Map(dto, e);
            e.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        
    }
}

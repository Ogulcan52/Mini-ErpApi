using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ERP.Application.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Entities.RequestFeatures;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Specifications
{
    public class DynamicSpecification<T> : ISpecification<T> where T : class
    {
        public Expression<Func<T, bool>>? Criteria { get; private set; }
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public int? Skip { get; private set; }
        public int? Take { get; private set; }
        public bool IsPagingEnabled { get; private set; }

        public DynamicSpecification(QueryParams queryParams)
        {
            // Arama (Search)

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                if (typeof(T) == typeof(Product))
                {
                    var search = queryParams.Search;
                    Criteria = (Expression<Func<T, bool>>)(object)(Expression<Func<Product, bool>>)(p =>
                        p.Name.Contains(search)
                        || p.Sku.Contains(search) 
                        || p.UnitPrice.ToString().Contains(search)
                        || p.Stock.ToString().Contains(search)
                        || p.IsActive.ToString().Contains(search)
                        || p.ReservedStock.ToString().Contains(search));
                }
                else if (typeof(T) == typeof(Customer))
                {
                    var search = queryParams.Search;
                    Criteria = (Expression<Func<T, bool>>)(object)(Expression<Func<Customer, bool>>)(c =>
                        c.Name.Contains(search) || c.Email.Contains(search) || c.Phone.Contains(search));
                }
            }

            // Sıralama
            if (!string.IsNullOrEmpty(queryParams.SortBy))
            {
                if (queryParams.SortDesc)
                    OrderByDescending = x => EF.Property<object>(x, queryParams.SortBy);
                else
                    OrderBy = x => EF.Property<object>(x, queryParams.SortBy);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ERP.Application.Specifications
{
    public interface ISpecification<T> where T : class
    {
        // Filtreleme kriteri
        Expression<Func<T, bool>>? Criteria { get; }

        // Sıralama
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }

        // Include ilişkileri
        List<Expression<Func<T, object>>> Includes { get; }

        // Pagination
        int? Skip { get; }
        int? Take { get; }
        bool IsPagingEnabled { get; }
    }
}

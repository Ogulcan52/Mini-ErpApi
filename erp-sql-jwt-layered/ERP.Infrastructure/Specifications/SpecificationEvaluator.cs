using System.Linq;
using System.Linq.Expressions;
using ERP.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Specifications
{
    public static class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T>  inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;

            // Filtreleme (Criteria)
            if (spec.Criteria != null)
                query = query.Where(spec.Criteria);


            // OrderBy / OrderByDescending
            if (spec.OrderBy != null)
                query = query.OrderBy(spec.OrderBy);
            else if (spec.OrderByDescending != null)
                query = query.OrderByDescending(spec.OrderByDescending);

            // Include ilişkileri
            if (spec.Includes != null && spec.Includes.Any())
            {
                foreach (var include in spec.Includes)
                    query = query.Include(include);
            }

            // Pagination
            if (spec.IsPagingEnabled)
            {
                if (spec.Skip.HasValue)
                    query = query.Skip(spec.Skip.Value);

                if (spec.Take.HasValue)
                    query = query.Take(spec.Take.Value);
            }

            return query;
        }
    }
}

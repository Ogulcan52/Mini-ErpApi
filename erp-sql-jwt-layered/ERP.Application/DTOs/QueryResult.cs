using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.DTOs
{
    public class QueryResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}

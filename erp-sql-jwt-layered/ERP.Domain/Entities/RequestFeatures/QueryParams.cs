using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Domain.Entities.RequestFeatures
{
    public class QueryParams
    {

            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 0;
            public string? Search { get; set; }  // Arama için
            public string? SortBy { get; set; }  // Sıralama kolonu
            public bool SortDesc { get; set; } = false; // DESC için
            public string[]? SelectFields { get; set; } // Veri şekillendirme (projection)
        

    }
}

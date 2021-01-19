using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{ 
    public interface ISearchResult<ResultType> where ResultType : struct
    {
        ISearchFilter Filter { get; set; }
        IQueryable<ISearchResultItem<ResultType>> Items { get; set; }
    }
}

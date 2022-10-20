using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public interface ISearchResultItem<ResultType> where ResultType : struct
    {
        string Title { get; set; }
        string Desc { get; set; }
        string URL { get; set; }
        ResultType ItemType { get; set; }
        string ItemTypeName { get; set; }
    }
}

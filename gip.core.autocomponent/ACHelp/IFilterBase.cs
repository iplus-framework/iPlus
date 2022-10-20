using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{ 
    public interface IFilterBase
    {
        int PageIndex { get; set; }
        int PagesCount { get; set; }
        int PageSize { get; set; }

        int DomanID { get; set; }
        string FilterNo { get; set; }

        long ItemsCount { get; set; }

    }
}

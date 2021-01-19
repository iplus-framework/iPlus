using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public class SearchFilterBase : FilterBase, ISearchFilter
    {
        public string SearchPharse { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public interface ISearchFilter : IFilterBase
    {
        string SearchPharse { get; set; }
    }
}

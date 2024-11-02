// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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

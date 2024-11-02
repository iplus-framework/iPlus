// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SearchResultBase : ISearchResult<SearchTypeEnum>
    {
        [JsonProperty]
        public ISearchFilter Filter { get; set; }

        public IQueryable<ISearchResultItem<SearchTypeEnum>> Items { get; set; }

        public List<ISearchResultItem<SearchTypeEnum>> ExecuteQuery()
        {
            SearchFilterBase filterBase = Filter as SearchFilterBase;
            filterBase.CalculatePageCount(Items.Count());
            var qrt = Items;
            qrt = qrt.OrderBy(p => p.Title);
            if (filterBase.PageSize > 0)
                qrt = qrt.Skip((filterBase.PageIndex - 1) * filterBase.PageSize).Take(filterBase.PageSize);
            return qrt.ToList();
        }

        [JsonProperty]
        public List<ISearchResultItem<SearchTypeEnum>> DumpData { get; set; }
    }
}

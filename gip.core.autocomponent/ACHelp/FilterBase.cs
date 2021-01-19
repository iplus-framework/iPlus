using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [Serializable]
    public class FilterBase : IFilterBase
    {
        /// <summary>
        /// Suport CodeSnippets Delete shema (DeleteDate)
        /// </summary>
        public bool? Deleted { get; set; }

        /// <summary>
        /// If item can be from different domain and used in same database this is a basic filter for filtering by
        /// domain
        /// </summary>
        public int DomanID { get; set; }

        public string FilterNo { get; set; }

        public int PageIndex { get; set; }
        public int PagesCount { get; set; }
        public int PageSize { get; set; }
        public long ItemsCount { get; set; }
        public int Order { get; set; }

        // public OutputFormat OutputFormat { get; set; }

        public bool IsPartial { get; set; }

        public FilterBase()
        {
            PageIndex = 1;
            PageSize = 10;
        }

        public FilterBase(int? pageSize)
        {
            PageIndex = 1;
            PageSize = pageSize ?? 10;
        }


        /// <summary>
        /// CalculatePageCount
        /// </summary>
        /// <param name="count"></param>
        public void CalculatePageCount(long count)
        {
            ItemsCount = count;
            decimal pagesCount = ((decimal)count) / ((decimal)PageSize);
            PagesCount = (int)count / PageSize;
            if (PagesCount < pagesCount)
            {
                PagesCount++;
            }
        }
    }
}

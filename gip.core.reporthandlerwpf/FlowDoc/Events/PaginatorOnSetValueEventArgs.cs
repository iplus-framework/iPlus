using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    public class PaginatorOnSetValueEventArgs : PaginatorEventArgs
    {
        public PaginatorOnSetValueEventArgs(IInlinePropertyValue dv, object parentDataRow, object value, ReportPaginator paginator)
        {
            _FlowDocObj = dv;
            _ParentDataRow = parentDataRow;
            _Value = value;
            _Paginator = paginator;
        }

        private IInlinePropertyValue _FlowDocObj;
        public IInlinePropertyValue FlowDocObj
        {
            get
            {
                return _FlowDocObj;
            }
        }

        private object _ParentDataRow;
        public object ParentDataRow
        {
            get
            {
                return _ParentDataRow;
            }
        }

        private object _Value;
        public object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }
    }
}

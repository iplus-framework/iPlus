// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;

namespace gip.core.reporthandler.avui.Flowdoc
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

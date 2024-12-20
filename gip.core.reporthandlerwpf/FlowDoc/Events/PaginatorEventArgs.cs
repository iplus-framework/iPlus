// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    public abstract class PaginatorEventArgs : EventArgs
    {
        protected ReportPaginator _Paginator;
        public ReportPaginator Paginator
        {
            get
            {
                return _Paginator;
            }
        }

        private bool _Handled;
        public bool Handled
        {
            get
            {
                return _Handled;
            }
            set
            {
                _Handled = value;
            }
        }

    }
}

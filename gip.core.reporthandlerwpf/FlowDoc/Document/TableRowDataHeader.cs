// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    public class TableRowDataHeader : TableRow
    {
        public TableRowDataHeader()
        {
        }

        /// <summary>
        /// Repeat the table header on the following pages if the table is across multiple pages (Beta)
        /// </summary>
        public bool RepeatTableHeader
        {
            get { return (bool)GetValue(RepeatTableHeaderOnNewPageProperty); }
            set { SetValue(RepeatTableHeaderOnNewPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RepeatTableHeaderOnNewPage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RepeatTableHeaderOnNewPageProperty =
            DependencyProperty.Register("RepeatTableHeaderOnNewPage", typeof(bool), typeof(TableRowDataHeader), new PropertyMetadata(false));


    }
}

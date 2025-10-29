// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;

namespace gip.core.reporthandler.avui.Flowdoc
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

        // Using a StyledProperty as the backing store for RepeatTableHeaderOnNewPage. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<bool> RepeatTableHeaderOnNewPageProperty = 
            AvaloniaProperty.Register<TableRowDataHeader, bool>(nameof(RepeatTableHeader), false);
    }
}

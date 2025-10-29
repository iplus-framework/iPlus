// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Represents the report header.
    /// </summary>
    /// <sumamry xml:lang="de">
    /// Stellt den Berichtskopf dar.
    /// </sumamry>
    public class SectionReportHeader : Section
    {
        /// <summary>
        /// Gets or sets the page header height in percent
        /// </summary>
        public double PageHeaderHeight
        {
            get { return (double)GetValue(PageHeaderHeightProperty); }
            set { SetValue(PageHeaderHeightProperty, value); }
        }

        // Using a StyledProperty as the backing store for PageHeaderHeight. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<double> PageHeaderHeightProperty =
            AvaloniaProperty.Register<SectionReportHeader, double>(nameof(PageHeaderHeight), 2.0d);


        public bool ShowHeaderOnFirstPage
        {
            get { return (bool)GetValue(ShowHeaderOnFirstPageProperty); }
            set { SetValue(ShowHeaderOnFirstPageProperty, value); }
        }

        // Using a StyledProperty as the backing store for ShowHeaderOnFirstPage. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<bool> ShowHeaderOnFirstPageProperty =
            AvaloniaProperty.Register<SectionReportHeader, bool>(nameof(ShowHeaderOnFirstPage), true);


    }
}

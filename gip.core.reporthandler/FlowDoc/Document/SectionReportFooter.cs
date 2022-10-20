﻿using System;
using System.Windows;
using System.Windows.Documents;

namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Represents the report footer.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt den Berichtsfuß dar.
    /// </summary>
    public class SectionReportFooter : Section
    {
        /// <summary>
        /// Gets or sets the page footer height in percent
        /// </summary>
        public double PageFooterHeight
        {
            get { return (double)GetValue(PageFooterHeightProperty); }
            set { SetValue(PageFooterHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PageFooterHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageFooterHeightProperty =
            DependencyProperty.Register("PageFooterHeight", typeof(double), typeof(ReportProperties), new UIPropertyMetadata(2.0d));
    }
}

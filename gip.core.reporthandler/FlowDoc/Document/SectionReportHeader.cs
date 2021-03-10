﻿using System;
using System.Windows;
using System.Windows.Documents;

namespace gip.core.reporthandler.Flowdoc
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

        // Using a DependencyProperty as the backing store for PageHeaderHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageHeaderHeightProperty =
            DependencyProperty.Register("PageHeaderHeight", typeof(double), typeof(ReportProperties), new UIPropertyMetadata(2.0d));


        public bool ShowHeaderOnFirstPage
        {
            get { return (bool)GetValue(ShowHeaderOnFirstPageProperty); }
            set { SetValue(ShowHeaderOnFirstPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowHeaderOnFirstPage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowHeaderOnFirstPageProperty =
            DependencyProperty.Register("ShowHeaderOnFirstPage", typeof(bool), typeof(ReportProperties), new PropertyMetadata(true));


    }
}

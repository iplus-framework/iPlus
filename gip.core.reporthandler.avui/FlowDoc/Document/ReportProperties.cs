// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls.Documents;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Specifies properties for report
    /// </summary>
    public class ReportProperties : Section
    {
        /// <summary>
        /// Gets or sets the report name
        /// </summary>
        public string ReportName
        {
            get { return (string)GetValue(ReportNameProperty); }
            set { SetValue(ReportNameProperty, value); }
        }

        // Using a StyledProperty as the backing store for ReportName. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<string> ReportNameProperty = 
            AvaloniaProperty.Register<ReportProperties, string>(nameof(ReportName));

        /// <summary>
        /// Gets or sets the report title
        /// </summary>
        public string ReportTitle
        {
            get { return (string)GetValue(ReportTitleProperty); }
            set { SetValue(ReportTitleProperty, value); }
        }

        // Using a StyledProperty as the backing store for ReportTitle. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<string> ReportTitleProperty = 
            AvaloniaProperty.Register<ReportProperties, string>(nameof(ReportTitle));

        public virtual string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public static readonly AttachedProperty<string> StringFormatProperty = ReportDocument.StringFormatProperty.AddOwner<ReportProperties>();

        public virtual string CultureInfo
        {
            get { return (string)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        public static readonly AttachedProperty<string> CultureInfoProperty = ReportDocument.CultureInfoProperty.AddOwner<ReportProperties>();


        #region Printer-Settings
        public string AutoSelectPrinterName
        {
            get { return (string)GetValue(AutoSelectPrinterNameProperty); }
            set { SetValue(AutoSelectPrinterNameProperty, value); }
        }
        public static readonly StyledProperty<string> AutoSelectPrinterNameProperty = 
            AvaloniaProperty.Register<ReportProperties, string>(nameof(AutoSelectPrinterName));


        public string AutoSelectPageOrientation
        {
            get { return (string)GetValue(AutoSelectPageOrientationProperty); }
            set { SetValue(AutoSelectPageOrientationProperty, value); }
        }
        public static readonly StyledProperty<string> AutoSelectPageOrientationProperty = 
            AvaloniaProperty.Register<ReportProperties, string>(nameof(AutoSelectPageOrientation));


        public int AutoSelectTray
        {
            get { return (int)GetValue(AutoSelectTrayProperty); }
            set { SetValue(AutoSelectTrayProperty, value); }
        }
        public static readonly StyledProperty<int> AutoSelectTrayProperty = 
            AvaloniaProperty.Register<ReportProperties, int>(nameof(AutoSelectTray), -1);


        public string AutoPageMediaSize
        {
            get { return (string)GetValue(AutoPageMediaSizeProperty); }
            set { SetValue(AutoPageMediaSizeProperty, value); }
        }
        public static readonly StyledProperty<string> AutoPageMediaSizeProperty = 
            AvaloniaProperty.Register<ReportProperties, string>(nameof(AutoPageMediaSize));
        #endregion

    }
}

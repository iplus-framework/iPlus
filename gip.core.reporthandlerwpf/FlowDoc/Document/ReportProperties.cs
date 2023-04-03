using System.Windows;
using System.Windows.Documents;

namespace gip.core.reporthandlerwpf.Flowdoc
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

        // Using a DependencyProperty as the backing store for ReportName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReportNameProperty =
            DependencyProperty.Register("ReportName", typeof(string), typeof(ReportProperties), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the report title
        /// </summary>
        public string ReportTitle
        {
            get { return (string)GetValue(ReportTitleProperty); }
            set { SetValue(ReportTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReportTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReportTitleProperty =
            DependencyProperty.Register("ReportTitle", typeof(string), typeof(ReportProperties), new UIPropertyMetadata(null));

        public virtual string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public static readonly DependencyProperty StringFormatProperty = ReportDocument.StringFormatProperty.AddOwner(typeof(ReportProperties));

        public virtual string CultureInfo
        {
            get { return (string)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        public static readonly DependencyProperty CultureInfoProperty = ReportDocument.CultureInfoProperty.AddOwner(typeof(ReportProperties));


        #region Printer-Settings
        public string AutoSelectPrinterName
        {
            get { return (string)GetValue(AutoSelectPrinterNameProperty); }
            set { SetValue(AutoSelectPrinterNameProperty, value); }
        }
        public static readonly DependencyProperty AutoSelectPrinterNameProperty =
            DependencyProperty.Register("AutoSelectPrinterName", typeof(string), typeof(ReportProperties), new UIPropertyMetadata(null));


        public string AutoSelectPageOrientation
        {
            get { return (string)GetValue(AutoSelectPageOrientationProperty); }
            set { SetValue(AutoSelectPageOrientationProperty, value); }
        }
        public static readonly DependencyProperty AutoSelectPageOrientationProperty =
            DependencyProperty.Register("AutoSelectPageOrientation", typeof(string), typeof(ReportProperties), new UIPropertyMetadata(null));


        public int AutoSelectTray
        {
            get { return (int)GetValue(AutoSelectTrayProperty); }
            set { SetValue(AutoSelectTrayProperty, value); }
        }
        public static readonly DependencyProperty AutoSelectTrayProperty =
            DependencyProperty.Register("AutoSelectTray", typeof(int), typeof(ReportProperties), new UIPropertyMetadata((int )- 1));


        public string AutoPageMediaSize
        {
            get { return (string)GetValue(AutoPageMediaSizeProperty); }
            set { SetValue(AutoPageMediaSizeProperty, value); }
        }
        public static readonly DependencyProperty AutoPageMediaSizeProperty =
            DependencyProperty.Register("AutoPageMediaSize", typeof(string), typeof(ReportProperties), new UIPropertyMetadata(null));
        #endregion

    }
}

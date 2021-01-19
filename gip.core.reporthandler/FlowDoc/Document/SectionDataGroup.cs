using System.Windows;
using System.Windows.Documents;

namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Provides a section for grouped data
    /// </summary>
    /// <summary xml:lang="de">
    /// Bietet einen Abschnitt für gruppierte Daten.
    /// </summary>
    public class SectionDataGroup : Section
    {
        /// <summary>
        /// Gets or sets the data group name
        /// </summary>
        public string DataGroupName
        {
            get { return (string)GetValue(DataGroupProperty); }
            set { SetValue(DataGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataGroupProperty =
            DependencyProperty.Register("DataGroupName", typeof(string), typeof(SectionDataGroup), new UIPropertyMetadata(""));

        public virtual string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public static readonly DependencyProperty StringFormatProperty = ReportDocument.StringFormatProperty.AddOwner(typeof(SectionDataGroup));

        public virtual string CultureInfo
        {
            get { return (string)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        public static readonly DependencyProperty CultureInfoProperty = ReportDocument.CultureInfoProperty.AddOwner(typeof(SectionDataGroup));
    }
}

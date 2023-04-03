using System.Windows.Documents;
using System.Windows;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// Represents the row data for table in reports.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt die Zeilendaten für die Tabelle in Berichten dar.
    /// </summary>
    public class TableRowData : TableRowDataBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TableRowData()
        {
        }



        public bool Configuration
        {
            get { return (bool)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(bool), typeof(TableRowData), new PropertyMetadata(false));

        
    }
}

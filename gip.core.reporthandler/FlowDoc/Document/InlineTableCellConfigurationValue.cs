using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Contains a configurable report value is to be displayed on the report.
    /// </summary>
    /// <summary xml:lang="de">
    /// Enthält einen konfigurierbaren Berichtswert, der auf dem Bericht angezeigt werden soll.
    /// </summary>
    public class InlineTableCellConfigurationValue : InlineTableCellValue
    {

        public int ParameterNameIndex
        {
            get { return (int)GetValue(ParameterNameIndexProperty); }
            set { SetValue(ParameterNameIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParameterNameIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParameterNameIndexProperty =
            DependencyProperty.Register("ParameterNameIndex", typeof(int), typeof(InlineTableCellConfigurationValue));

    }
}

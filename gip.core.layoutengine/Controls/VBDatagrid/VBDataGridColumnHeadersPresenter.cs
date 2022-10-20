using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Used within the template of a <see cref="VBDataGrid"/> to specify the location in the control's visual tree where the column headers are to be added.
    /// </summary>
    /// <summary>
    /// Wird innerhalb der Vorlage eines <see cref="VBDataGrid"/> verwendet, um die Stelle im visuellen Baum des Controls anzugeben, an der die Spaltenüberschriften hinzugefügt werden sollen.
    /// </summary>
    public class VBDataGridColumnHeadersPresenter:DataGridColumnHeadersPresenter
    {
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new VBDataGridColumnHeader();
        }
    }
}

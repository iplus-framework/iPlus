using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace gip.core.layoutengine
{
    public class VBDataGridColumnHeadersPresenter2:DataGridColumnHeadersPresenter
    {
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new VBDataGridColumnHeader();
        }
    }
}

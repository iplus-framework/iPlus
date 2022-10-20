using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// General format for dialog result
    /// </summary>
    public class VBDialogResult
    {
        public IACObject ReturnValue { get; set; }

        public eMsgButton SelectedCommand { get; set; }
    }
}

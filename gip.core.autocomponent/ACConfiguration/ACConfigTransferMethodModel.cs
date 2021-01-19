using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACConfigTransferMethodModel'}de{'ACConfigTransferMethodModel'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    public class ACConfigTransferMethodModel
    {
        [ACPropertyInfo(1, "MethodSelection", "en{'Method'}de{'Method'}")]
        public ACClassMethod Method { get; set; }

        [ACPropertyInfo(1, "MethodSelection", "en{'Selected'}de{'Auswahl'}")]
        public bool Selected { get; set; }
    }
}

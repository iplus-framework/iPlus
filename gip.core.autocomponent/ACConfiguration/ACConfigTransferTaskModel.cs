using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Define task for transfer (clone) parameters from one wf-connected config place to another (ProdOrder, Partslist  etc)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACConfigTransferTaskModel'}de{'ACConfigTransferTaskModel'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    public class ACConfigTransferTaskModel
    {
        [ACPropertyInfo(1, "PreACUrl", "en{'Value'}de{'Wert'}")]
        public string PreConfigACUrl { get; set; }

        [ACPropertyInfo(2, "LocalACUrl", "en{'Value'}de{'Wert'}")]
        public string LocalConfigACUrl { get; set; }

        [ACPropertyInfo(3, "ExistOnTarget", "en{'Exist on target'}de{'Existiert bereits'}")]
        public bool ExistOnTarget { get; set; }

        [ACPropertyInfo(4, "Selected", "en{'Selected'}de{'Auswahl'}")]
        public bool Selected { get; set; }

        public Guid? ACClassMethodID { get; set; }
    }
}

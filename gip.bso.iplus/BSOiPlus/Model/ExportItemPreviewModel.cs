using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'ExportItemPreviewModel'}de{'ExportItemPreviewModel'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ExportItemPreviewModel
    {
        [ACPropertyInfo(1, "TypeName", "en{'Type'}de{'Type'}")]
        public string TypeName { get; set; }

        [ACPropertyInfo(2, Const.ACIdentifierPrefix, "en{'ACIdentifier'}de{'ACIdentifier'}")]
        public string ACIdentifier { get; set; }

        [ACPropertyInfo(3, Const.ACCaptionPrefix, "en{'ACCaption'}de{'ACCaption'}")]
        public string ACCaption { get; set; }

        [ACPropertyInfo(4, Const.EntityUpdateDate, "en{'Updated'}de{'Updated'}")]
        public DateTime UpdateDate { get; set; }
    }
}

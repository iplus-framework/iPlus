using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MediaItemPresentations'}de{'MediaItemPresentation'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class MediaItemPresentation
    {
        [ACPropertyInfo(999, "ThumbPath", "en{'ThumbPath'}de{'ThumbPath'}")]
        public string ThumbPath { get; set; }

        [ACPropertyInfo(999, "FilePath", "en{'FilePath'}de{'FilePath'}")]
        public string FilePath { get; set; }

        [ACPropertyInfo(999, "Name", "en{'Name'}de{'Name'}")]
        public string Name { get; set; }

        public bool HaveOwnThumb { get; set; }
    }
}

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

        #region path

        [ACPropertyInfo(999, "ThumbPath", "en{'ThumbPath'}de{'ThumbPath'}")]
        public string ThumbPath { get; set; }

        [ACPropertyInfo(999, "FilePath", "en{'FilePath'}de{'FilePath'}")]
        public string FilePath { get; set; }

        #endregion

        #region image - dump

        [ACPropertyInfo(999, "Thumb", "en{'ThumbPath'}de{'ThumbPath'}")]
        public byte[] Thumb { get; set; }

        [ACPropertyInfo(999, "File", "en{'FilePath'}de{'FilePath'}")]
        public byte[] File { get; set; }

        #endregion

        #region presentation

        [ACPropertyInfo(999, "Name", "en{'Name'}de{'Name'}")]
        public string Name { get; set; }

        [ACPropertyInfo(999, "IsDefault", "en{'Is default'}de{'Ist Standard'}")]
        public bool IsDefault { get; set; }

        [ACPropertyInfo(999, "IsGenerateThumb", "en{'Is default'}de{'Ist Standard'}")]
        public bool IsGenerateThumb { get; set; }

        #endregion


    }
}

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

        #region File Path

        [ACPropertyInfo(999, "ThumbPath", "en{'Thumb Image path'}de{'Standard-Daumenbildpfad'}")]
        public string ThumbPath { get; set; }

        [ACPropertyInfo(999, "FilePath", "en{'File path'}de{'Dateipfad'}")]
        public string FilePath { get; set; }

        #endregion

        #region Edit File Path

        [ACPropertyInfo(999, "ThumbPath", "en{'Thumb Image path'}de{'Standard-Daumenbildpfad'}")]
        public string EditThumbPath { get; set; }

        [ACPropertyInfo(999, "FilePath", "en{'File path'}de{'Dateipfad'}")]
        public string EditFilePath { get; set; }

        #endregion


        #region presentation

        [ACPropertyInfo(999, "Name", "en{'Name'}de{'Name'}")]
        public string Name { get; set; }

        [ACPropertyInfo(999, "IsDefault", "en{'Is default'}de{'Ist Standard'}")]
        public bool IsDefault { get; set; }

        [ACPropertyInfo(999, "IsGenerateThumb", "en{'Generate Thumb Image'}de{'Standard-Daumenbild generieren'}")]
        public bool IsGenerateThumb { get; set; }

        #endregion


    }
}

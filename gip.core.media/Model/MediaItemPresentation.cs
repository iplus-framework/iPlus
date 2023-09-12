using System.Runtime.CompilerServices;
using System;
using System.ComponentModel;
using System.IO;
using gip.core.datamodel;

namespace gip.core.media
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MediaItemPresentations'}de{'MediaItemPresentation'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class MediaItemPresentation : INotifyPropertyChanged
    {
        public MediaItemPresentation()
        {
            IsNew = true;
        }
        #region File Path

        [ACPropertyInfo(1, "ThumbPath", "en{'Thumb Image path'}de{'Standard-Daumenbildpfad'}")]
        public string ThumbPath { get; set; }

        [ACPropertyInfo(2, "FilePath", "en{'File path'}de{'Dateipfad'}")]
        public string FilePath { get; set; }

        #endregion

        #region Edit File Path

        [ACPropertyInfo(3, "ThumbPath", "en{'Thumb Image path'}de{'Standard-Daumenbildpfad'}")]
        public string EditThumbPath { get; set; }

        [ACPropertyInfo(4, "FilePath", "en{'File path'}de{'Dateipfad'}")]
        public string EditFilePath { get; set; }

        #region Temp File Path

        [ACPropertyInfo(5, "TempThumbPath", "en{'Thumb Image path'}de{'Standard-Daumenbildpfad'}")]
        public string TempThumbPath { get; set; }

        [ACPropertyInfo(6, "TempFilePath", "en{'File path'}de{'Dateipfad'}")]
        public string TempFilePath { get; set; }

        #endregion

        #endregion

        #region presentation

        [ACPropertyInfo(5, "Name", "en{'Name'}de{'Name'}")]
        public string Name { get; set; }

        [ACPropertyInfo(6, "IsDefault", "en{'Is default'}de{'Ist Standard'}")]
        public bool IsDefault { get; set; }

        [ACPropertyInfo(7, "IsGenerateThumb", "en{'Generate Thumb Image'}de{'Standard-Daumenbild generieren'}")]
        public bool IsGenerateThumb { get; set; }

        public bool IsNew { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MediaItemTypeEnum MediaType { get; set; }

        #endregion

        #region Methods

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}

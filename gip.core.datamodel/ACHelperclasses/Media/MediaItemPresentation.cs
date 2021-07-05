using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MediaItemPresentations'}de{'MediaItemPresentation'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class MediaItemPresentation
    {

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

        #endregion

        #region BitMap

        public ImageSource Image { get; set; }
        public ImageSource ImageThumb { get; set; }

        #endregion

        #region presentation

        [ACPropertyInfo(5, "Name", "en{'Name'}de{'Name'}")]
        public string Name { get; set; }

        [ACPropertyInfo(6, "IsDefault", "en{'Is default'}de{'Ist Standard'}")]
        public bool IsDefault { get; set; }

        [ACPropertyInfo(7, "IsGenerateThumb", "en{'Generate Thumb Image'}de{'Standard-Daumenbild generieren'}")]
        public bool IsGenerateThumb { get; set; }

        #endregion

        #region Methods
        public void LoadImage(bool isImage)
        {
            if (!string.IsNullOrEmpty(ThumbPath))
            {
                BitmapImage imageThumb = new BitmapImage();
                imageThumb.BeginInit();
                imageThumb.UriSource = new Uri(ThumbPath, UriKind.RelativeOrAbsolute);
                imageThumb.CacheOption = BitmapCacheOption.OnLoad;
                imageThumb.EndInit();
                ImageThumb = imageThumb;
            }
            if (isImage)
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(FilePath, UriKind.RelativeOrAbsolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                Image = image;
            }
        }
        #endregion


    }
}

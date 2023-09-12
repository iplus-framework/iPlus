using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Threading;

namespace gip.core.media
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'BSOFileDialog'}de{'BSOFileDialog'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class BSOFileDialog : ACBSO
    {

        #region c´tors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOFileDialog(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            
        }

        /// <summary>
        /// ACs the init.
        /// </summary>
        /// <param name="startChildMode">The start child mode.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

           

            return true;
        }

        #endregion

        #region Methods

        [ACMethodInfo("SetFilePath", "en{'...'}de{'...'}", 9999, false, false, true)]
        public string SetFilePath(string filePath, bool useExisting)
        {
            string fileName = null;
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = false;
                if(filePath != null)
                {
                    dialog.DefaultDirectory = Path.GetDirectoryName(filePath);
                }
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    if (!string.IsNullOrEmpty(dialog.FileName) && (!useExisting || File.Exists(dialog.FileName)))
                    {
                        fileName = dialog.FileName;
                    }
                }
            }
            return fileName;
        }

        #endregion
    }
}

using System;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Der VBBSOFolderDialog dient zur Auswahl eines Verzeichnisses
    /// 
    /// Der Aufruf erfolgt immer über Root.Messages:
    /// string folderPath = Messages.SelectFolderDialog(CurrentImportFolder);
    /// 
    /// Rückgabewert = !null signalisiert das der VBBSOFolderDialog mit "OK" bestätigt wurden.
    /// Rückgabewert = nulll signalisiert das der VBBSOFolderDialog mit "Cancel" geschlossen wurde. 
    /// 
    /// Nach dem schließen des Dialogs wird dieses Sub-BSO auch geschlossen 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Folderdialog'}de{'Verzeichnisdialog'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBBSOFolderDialog : ACBSO
    {
        #region c´tors
        public VBBSOFolderDialog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            //this._FolderPath = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region BSO->ACProperty
        string _FolderPath;
        /// <summary>
        /// Arbeitskopie Root ACQueryDefinition
        /// </summary>
        [ACPropertyCurrent(9999, "FolderPath", "en{'Folderpath'}de{'Verzeichnispfad'}")]
        public string CurrentFolderPath
        {
            get
            {
                return _FolderPath;
            }
            set
            {
                _FolderPath = value;
                OnPropertyChanged("CurrentFolderPath");
            }
        }
        #endregion

        #region BSO->ACMethod

        /// <summary>Zeigt den Dialog zum konfigurieren eine ACQueryDefinition an</summary>
        /// <param name="folderPath"></param>
        /// <returns>true wenn Dialog mit "OK" geschlossen wird</returns>
        [ACMethodCommand("FolderPath", "en{'Folderdialog'}de{'Verzeichnisdialog'}", 9999)]
        public string FolderDlg(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                CurrentFolderPath = Root.Environment.Rootpath + "Data";
            }
            else
            {
                CurrentFolderPath = folderPath;
            }
            ShowDialog(this, "FolderPathDlg");
            this.ParentACComponent.StopComponent(this);
            return CurrentFolderPath;
        }

        [ACMethodCommand("Folderdialog", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void OK()
        {
            CloseTopDialog();
        }

        [ACMethodCommand("Folderdialog", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void Cancel()
        {
            CurrentFolderPath = null;
            CloseTopDialog();
        }

        [ACMethodCommand("FolderPath", "en{'IPlusPath'}de{'IPlusPath'}", 1)]
        public void PathVariobatchData()
        {
            CurrentFolderPath = Root.Environment.Rootpath + "\\Data";
        }

        [ACMethodCommand("FolderPath", "en{'Desktop'}de{'Desktop'}", 2)]
        public void PathDesktop()
        {
            CurrentFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        }

        [ACMethodCommand("FolderPath", "en{'Programm Files'}de{'Programmdateien'}", 3)]
        public void PathProgramFiles()
        {
            CurrentFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
        }

        [ACMethodCommand("FolderPath", "en{'MyDocument'}de{'Dokumente'}", 4)]
        public void PathMyDocuments()
        {
            CurrentFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        }
        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"FolderDlg":
                    result = FolderDlg((String)acParameter[0]);
                    return true;
                case"OK":
                    OK();
                    return true;
                case"Cancel":
                    Cancel();
                    return true;
                case"PathVariobatchData":
                    PathVariobatchData();
                    return true;
                case"PathDesktop":
                    PathDesktop();
                    return true;
                case"PathProgramFiles":
                    PathProgramFiles();
                    return true;
                case"PathMyDocuments":
                    PathMyDocuments();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}

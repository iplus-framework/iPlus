using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'Resource selection'}de{'Ressourcenauswahl'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, false)]
    public class BSOiPlusResourceSelect : ACBSO
    {

        #region static local


        #endregion

        #region c´tors
        public BSOiPlusResourceSelect(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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

            List<SQLInstanceInfo> tmpList = SQLInstanceInfoList.ToList();
            if (tmpList.Any())
            {
                foreach (var item in tmpList)
                {
                    if (!string.IsNullOrEmpty(item.Password))
                        item.Password = ACCrypt.RijndaelEncrypt(item.Password, ResourcesSQL.Hash);
                }
                tmpList = tmpList.Where(item =>
                    !string.IsNullOrEmpty(item.ServerName) &&
                    !string.IsNullOrEmpty(item.Database) &&
                    !string.IsNullOrEmpty(item.Username) &&
                    !string.IsNullOrEmpty(item.Password)).ToList();
                SQLConnectionList = tmpList;
            }

            if (ACQueryDatabase != null)
                ACQueryDatabase.Dispose();

            return base.ACDeInit(deleteACClassTask);
        }

        private void LoadSelectedPaht(string path)
        {
            _SQLInstanceInfoList = new BindingList<SQLInstanceInfo>(LoadSQLInstanceInfoList());
            OnPropertyChanged("SQLInstanceInfoList");
            if(!string.IsNullOrEmpty(path) && path.Contains('|'))
            {
                SQLMode();
                Tuple<SQLInstanceInfo, string> selectedSQLItem = ResourcesSQL.DBUrlDecode(path);
                SelectedSQLInstanceInfo = selectedSQLItem.Item1;
                ACClass selectedACQueryDef = ACQueryDatabase.ACClass.FirstOrDefault(x => x.ACClass1_BasedOnACClass.ACIdentifier == ACQueryDefinition.ClassName && x.ACIdentifier == selectedSQLItem.Item2);
                if (!SQLACQueryDefList.Contains(selectedACQueryDef))
                {
                    SQLACQueryDefList.Add(selectedACQueryDef);
                    OnPropertyChanged("SQLACQueryDefList");
                }
                SelectedSQLACQueryDef = selectedACQueryDef;
                FilterACQueryName = selectedACQueryDef.ACIdentifier;
            }
            else
            {
                FolderMode();
                FolderPath = path;
            }
        }
        #endregion

        #region Working Mode

        private bool _IsFolderMode;
        [ACPropertyInfo(401, "IsFolderMode", "en{'ImportFolder'}de{'Importordner'}")]
        public bool IsFolderMode
        {
            get
            {
                return _IsFolderMode;
            }
            set
            {
                if (_IsFolderMode != value)
                {
                    _IsFolderMode = value;
                    OnPropertyChanged("IsFolderMode");
                    OnPropertyChanged("FormDesignName");
                }
            }
        }


        [ACPropertyInfo(402, "IsFolderMode", "en{'FormDesignName'}de{'FormDesignName'}")]
        public string FormDesignName
        {
            get
            {
                string designName = IsFolderMode ? "Folder" : "SQL";
                gip.core.datamodel.ACClassDesign acClassDesign = ACType.GetDesign(this, Global.ACUsages.DUMain, Global.ACKinds.DSDesignLayout, designName);
                string layoutXAML = null;
                if (acClassDesign != null && acClassDesign.ACIdentifier != "UnknowMainlayout")
                {
                    layoutXAML = acClassDesign.XMLDesign;
                }
                else
                {
                    layoutXAML = "<vb:VBDockPanel><vb:VBTextBox ACCaption=\"Unknown:\" Text=\"" + designName + "\"></vb:VBTextBox></vb:VBDockPanel>";
                }
                return layoutXAML;

            }
        }

        [ACMethodCommand("IsFolderMode", "en{'Folder'}de{'Datei'}", 401)]
        public void FolderMode()
        {
            IsFolderMode = true;
        }


        [ACMethodCommand("IsFolderMode", "en{'SQL'}de{'SQL'}", 402)]
        public void SQLMode()
        {
            IsFolderMode = false;
        }

        #endregion

        #region SQLInstanceInfo

        #region SQLInstanceInfo -> Select Item
        private SQLInstanceInfo _SelectedSQLInstanceInfo;
        /// <summary>
        /// Selected property for SQLInstanceInfo
        /// </summary>
        /// <value>The selected SQLInstanceInfo</value>
        [ACPropertySelected(403, "SQLInstanceInfo", "en{'TODO: SQLInstanceInfo'}de{'TODO: SQLInstanceInfo'}")]
        public SQLInstanceInfo SelectedSQLInstanceInfo
        {
            get
            {
                return _SelectedSQLInstanceInfo;
            }
            set
            {
                if (_SelectedSQLInstanceInfo != value)
                {
                    _SelectedSQLInstanceInfo = value;
                    SelectedSQLACQueryDef = null;
                    _SQLACQueryDefList = null;
                    FilterACQueryName = null;
                    FilterACQueryObjectTypeName = null;
                    if (ACQueryDatabase != null)
                        ACQueryDatabase.Dispose();
                    if (!string.IsNullOrEmpty(SelectedSQLInstanceInfo.ServerName) && !string.IsNullOrEmpty(SelectedSQLInstanceInfo.Username) && !string.IsNullOrEmpty(SelectedSQLInstanceInfo.Password))
                    {
                        try
                        {
                            ACQueryDatabase = ACObjectContextManager.FactoryContext(SelectedSQLInstanceInfo) as Database;
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            Messages.LogException("BSOiPlusResourceSelect", "SelectedSQLInstanceInfo", msg);
                        }
                    }
                    OnPropertyChanged("SelectedSQLInstanceInfo");
                    OnPropertyChanged("SQLACQueryDefList");
                }
            }
        }

        private BindingList<SQLInstanceInfo> _SQLInstanceInfoList;
        /// <summary>
        /// List property for SQLInstanceInfo
        /// </summary>
        /// <value>The SQLInstanceInfo list</value>
        [ACPropertyList(404, "SQLInstanceInfo")]
        public BindingList<SQLInstanceInfo> SQLInstanceInfoList
        {
            get
            {
                if (_SQLInstanceInfoList == null)
                    _SQLInstanceInfoList = new BindingList<SQLInstanceInfo>();
                return _SQLInstanceInfoList;
            }
        }

        private List<SQLInstanceInfo> LoadSQLInstanceInfoList()
        {
            List<SQLInstanceInfo> tmpList = SQLConnectionList.ToList();
            if (tmpList != null)
                foreach (var item in tmpList)
                {
                    item.Password = ACCrypt.RijndaelDecrypt(item.Password, ResourcesSQL.Hash);
                }
            return tmpList;
        }


        [ACPropertyConfig("en{'SQL Connection List'}de{'SQL Verbindungsliste'}")]
        public List<SQLInstanceInfo> SQLConnectionList
        {
            get
            {

                List<SQLInstanceInfo> result = new List<SQLInstanceInfo>();
                object storedResult = this[ResourcesSQL.SQLConnectionList];
                //object storedResult = GetConfiguration(Database as Database, "SQLConnectionList");
                if (storedResult != null && !string.IsNullOrEmpty(storedResult.ToString()))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<SQLInstanceInfo>));
                    using (TextReader reader = new StringReader(storedResult.ToString()))
                    {
                        result = (List<SQLInstanceInfo>)serializer.Deserialize(reader);
                    }
                }
                return result.OrderBy(x=>x.ServerName).ThenBy(x=>x.Database).ToList();
            }
            set
            {
                SerializeSQLConnectionList(value);
            }
        }

        public void SerializeSQLConnectionList(List<SQLInstanceInfo> list)
        {
            StringBuilder result = new StringBuilder();
            if (list != null && list.Any())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<SQLInstanceInfo>));
                using (var writer = XmlWriter.Create(result))
                {
                    serializer.Serialize(writer, list);
                }
            }
            this[ResourcesSQL.SQLConnectionList] = result;
            //SetConfiguration(Database as Database, ResourcesSQL.SQLConnectionList, result);
            Database.ACSaveChanges();

        }

        #endregion

        #region SQLInstanceInfo -> Methods

        [ACMethodInfo("SQLInstanceInfo", "en{'Add SQL connection'}de{'SQL Verbindung hinzufügen'}", 403)]
        public void CmdSQLInstanceInfoAdd()
        {
            SQLInstanceInfo item = new SQLInstanceInfo();
            SQLInstanceInfoList.Add(item);
            SelectedSQLInstanceInfo = item;
            OnPropertyChanged("SQLInstanceInfoList");
        }


        [ACMethodInfo("SQLInstanceInfo", "en{'Delete SQL connection'}de{'SQL Verbindung entfernen'}", 404)]
        public void CmdSQLInstanceInfoDelete()
        {
            SQLInstanceInfoList.Remove(SelectedSQLInstanceInfo);
            SelectedSQLInstanceInfo = SQLInstanceInfoList.FirstOrDefault();
            OnPropertyChanged("SQLInstanceInfoList");
        }

        public bool IsEnabledCmdSQLInstanceInfoDelete()
        {
            return SelectedSQLInstanceInfo != null;
        }

        #endregion

        #endregion

        #region SQLACQueryDef

        public Database ACQueryDatabase { get; set; }


        #region SQLACQueryDef -> SelectList
        private ACClass _SelectedSQLACQueryDef;
        /// <summary>
        /// Selected property for ACClass
        /// </summary>
        /// <value>The selected SQLACQueryDef</value>
        [ACPropertySelected(406, "SQLACQueryDef", "en{'TODO: SQLACQueryDef'}de{'TODO: SQLACQueryDef'}")]
        public ACClass SelectedSQLACQueryDef
        {
            get
            {
                return _SelectedSQLACQueryDef;
            }
            set
            {
                if (_SelectedSQLACQueryDef != value)
                {
                    _SelectedSQLACQueryDef = value;
                    OnPropertyChanged("SelectedSQLACQueryDef");
                }
            }
        }


        private List<ACClass> _SQLACQueryDefList;
        /// <summary>
        /// List property for ACClass
        /// </summary>
        /// <value>The SQLACQueryDef list</value>
        [ACPropertyList(407, "SQLACQueryDef")]
        public List<ACClass> SQLACQueryDefList
        {
            get
            {
                if (_SQLACQueryDefList == null)
                    _SQLACQueryDefList = new List<ACClass>();
                return _SQLACQueryDefList;
            }
        }

        private List<ACClass> LoadSQLACQueryDefList()
        {
            if (SelectedSQLInstanceInfo == null) return null;
            string acQueryDefClassName = FilterACQueryName ?? "";
            string typeName = FilterACQueryObjectTypeName ?? "";
            return ACQueryDatabase.ACClass
                    .Where(x => x.ACClass1_BasedOnACClass.ACIdentifier == ACQueryDefinition.ClassName)
                    .Where(x =>
                        (acQueryDefClassName == "" || x.ACIdentifier.Contains(acQueryDefClassName)) &&
                        (typeName == "" || x.ACClassProperty_ACClass.Any(pro => pro.ACIdentifier == Const.ACQueryRootObjectPrefix && pro.XMLValue.Contains(typeName)))
                        )
                    .OrderBy(x => x.ACIdentifier)
                    .ToList();
        }

        #endregion

        #region SQLACQueryDef -> Filter

        private string _FilterACQueryName;
        [ACPropertyInfo(408, "FilterACQuery", "en{'ACQuery'}de{'ACQuery'}")]
        public string FilterACQueryName
        {
            get
            {
                return _FilterACQueryName;
            }
            set
            {
                if (_FilterACQueryName != value)
                {
                    _FilterACQueryName = value;
                    OnPropertyChanged("FilterACQueryName");
                }
            }
        }


        private string _FilterACQueryObjectTypeName;
        [ACPropertyInfo(409, "FilterACQuery", "en{'Type'}de{'Type'}")]
        public string FilterACQueryObjectTypeName
        {
            get
            {
                return _FilterACQueryObjectTypeName;
            }
            set
            {
                if (_FilterACQueryObjectTypeName != value)
                {
                    _FilterACQueryObjectTypeName = value;
                    OnPropertyChanged("FilterACQueryObjectTypeName");
                }
            }
        }

        [ACMethodInfo("FilterACQuery", "en{'Search'}de{'Suche'}", 405)]
        public void CmdFilterACQuery()
        {
            if (BackgroundWorker.IsBusy) return;
            BackgroundWorker.RunWorkerAsync("DoSQLACQueryDefList");
            //ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledCmdFilterACQuery()
        {
            return !BackgroundWorker.IsBusy && ACQueryDatabase != null;
        }

        #endregion

        #region SQLACQueryDef -> methods

        [ACMethodInteraction("Query", "en{'Configuration'}de{'Konfiguration'}", 406, false)]
        public bool ShowACQueryDialog()
        {
            ACQueryDefinition acQueryDefinition = ACEntitySerializer.FactoryImportACQueryDefinition(ACQueryDatabase, SelectedSQLACQueryDef);
            return (bool)ACUrlCommand("VBBSOQueryDialog!QueryConfigDlg", new object[] { acQueryDefinition, true, true, true, true });
        }

        public bool IsEnabledShowACQueryDialog()
        {
            return SelectedSQLACQueryDef != null;
        }

        #endregion

        #endregion

        #region Folder selection

        private string _FolderPath;
        [ACPropertyInfo(405, "FolderPath", "en{'Import folder'}de{'Importverzeichnis'}")]
        public string FolderPath
        {
            get
            {
                return _FolderPath;
            }
            set
            {
                if (_FolderPath != value)
                {
                    _FolderPath = value;
                    OnPropertyChanged("FolderPath");
                }
            }
        }


        [ACMethodInfo("Import", "en{'...'}de{'...'}", 407, false, false, true)]
        public void ImportFolder()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                if (Directory.Exists(FolderPath))
                    dialog.InitialDirectory = FolderPath;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    if (Directory.Exists(dialog.FileName))
                    {
                        FolderPath = dialog.FileName;
                    }
                }
            }
        }

        #endregion

        #region Calling BSO outside

        /// <summary>Zeigt den Dialog zum konfigurieren eine ACQueryDefinition an</summary>
        /// <param name="path"></param>
        /// <returns>true wenn Dialog mit "OK" geschlossen wird</returns>
        [ACMethodCommand("ResourceDlg", "en{'Resource dialog'}de{'ResourceDialog'}", 408)]
        public string ResourceDlg(string path)
        {
            LoadSelectedPaht(path);
            ShowDialog(this, "ResourceDlg");
            this.ParentACComponent.StopComponent(this);
            if (IsFolderMode)
            {
                path = FolderPath;
            }
            else
            {
                if (SelectedSQLInstanceInfo != null && SelectedSQLACQueryDef != null)
                    path = ResourcesSQL.DBUrlEncode(SelectedSQLInstanceInfo, SelectedSQLACQueryDef);
            }
            return path;
        }

        [ACMethodCommand("ResourceDlg", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void OK()
        {
            CloseTopDialog();
        }

        [ACMethodCommand("Folderdialog", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void Cancel()
        {
            CloseTopDialog();
        }

        #endregion

        #region Backgroud worker

        /// <summary>
        /// 1. Dieser Eventhandler wird aufgerufen, wenn Hintergrundjob starten soll
        /// Dies wird ausgelöst durch den Aufruf der Methode RunWorkerAsync()
        /// Methode läuft im Hintergrundthread
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(sender, e);
            this.CurrentProgressInfo.ProgressInfoIsIndeterminate = true;
            
            string command = e.Argument.ToString();
            switch (command)
            {
                case "DoSQLACQueryDefList":
                    DoSQLACQueryDefList();
                    break;
            }
        }

        public override void BgWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            // base.BgWorkerCompleted(sender, e);
            this.CurrentProgressInfo.ProgressInfoIsIndeterminate = false;
            string command = worker.EventArgs.Argument.ToString();
            switch (command)
            {
                case "DoSQLACQueryDefList":
                    OnPropertyChanged("SQLACQueryDefList");
                    break;
            }
        }

        private void DoSQLACQueryDefList()
        {
            try
            {
                _SQLACQueryDefList = LoadSQLACQueryDefList();
            }
            catch (Exception ec)
            {
                Messages.Msg(
                        new Msg()
                        {
                            MessageLevel = eMsgLevel.Error,
                            Message = string.Format("Error fetching ACQueryDefinition list: {0}. Connection string: {1}", ec.Message, ACQueryDatabase.Connection.ConnectionString)
                        });
            }
        }

        #endregion


        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"FolderMode":
                    FolderMode();
                    return true;
                case"SQLMode":
                    SQLMode();
                    return true;
                case"CmdSQLInstanceInfoAdd":
                    CmdSQLInstanceInfoAdd();
                    return true;
                case"CmdSQLInstanceInfoDelete":
                    CmdSQLInstanceInfoDelete();
                    return true;
                case"IsEnabledCmdSQLInstanceInfoDelete":
                    result = IsEnabledCmdSQLInstanceInfoDelete();
                    return true;
                case"CmdFilterACQuery":
                    CmdFilterACQuery();
                    return true;
                case"IsEnabledCmdFilterACQuery":
                    result = IsEnabledCmdFilterACQuery();
                    return true;
                case"ShowACQueryDialog":
                    result = ShowACQueryDialog();
                    return true;
                case"IsEnabledShowACQueryDialog":
                    result = IsEnabledShowACQueryDialog();
                    return true;
                case"ImportFolder":
                    ImportFolder();
                    return true;
                case"ResourceDlg":
                    result = ResourceDlg((String)acParameter[0]);
                    return true;
                case"OK":
                    OK();
                    return true;
                case"Cancel":
                    Cancel();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }
}

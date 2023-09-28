using gip.core.autocomponent;
using gip.core.ControlScriptSync.file;
using gip.core.ControlScriptSync.sql;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'Import'}de{'Import'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, false, true)]
    public class BSOiPlusImport : ACBSO, IMsgObserver
    {

        #region consts

        public const string ImportSourcePath_IndexName = "CustomerImportSource";
        public const string BackgroundWorker_DoImport = "DoImport";
        public const string BackgroundWorker_DoInspectImport = "DoInspectImport";

        public const string Cmd_OnReportAddFsItem_CustomBreak = @"OnReportAddFsItem_CustomBreak";

        #endregion

        #region c´tors

        /// <summary>
        /// Initializes a new instance of the <see cref="BSOiPlusImport"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOiPlusImport(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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

            if (this[ImportSourcePath_IndexName] != null)
            {
                ImportSourcePath = this[ImportSourcePath_IndexName].ToString();
            }
            else
            {
                ImportSourcePath = Root.Environment.Datapath;
            }

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            Cleanup();

            bool done = base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return done;
        }

        private void Cleanup()
        {
            if (CurrentImportItemRoot != null)
                CurrentImportItemRoot.DeatachAll();
            CurrentImportItemRoot = null;
            CurrentImportItem = null;

            _ImportTypeFilterList = null;
            OnPropertyChanged("ImportTypeFilterList");

            var tmp = Database;
            _BSODatabase.ACUndoChanges(true);

            msgList = null;
            OnPropertyChanged("MsgList");
            CurrentMsg = null;
        }

        #endregion

        #region Messages/Progress/Database

        private Database _BSODatabase = null;
        /// <summary>
        /// Overriden: Returns a separate database context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_BSODatabase == null)
                    _BSODatabase = ACObjectContextManager.GetOrCreateContext<Database>(this.GetACUrl());
                return _BSODatabase;
            }
        }

        /// <summary>
        /// The _ current MSG
        /// </summary>
        Msg _CurrentMsg;
        /// <summary>
        /// Gets or sets the current MSG.
        /// </summary>
        /// <value>The current MSG.</value>
        [ACPropertyCurrent(405, "Message", "en{'Message'}de{'Meldung'}")]
        public Msg CurrentMsg
        {
            get
            {
                return _CurrentMsg;
            }
            set
            {
                _CurrentMsg = value;
                OnPropertyChanged("CurrentMsg");
            }
        }

        private ObservableCollection<Msg> msgList;
        /// <summary>
        /// Gets the MSG list.
        /// </summary>
        /// <value>The MSG list.</value>
        [ACPropertyList(406, "Message", "en{'Messagelist'}de{'Meldungsliste'}")]
        public ObservableCollection<Msg> MsgList
        {
            get
            {
                if (msgList == null)
                    msgList = new ObservableCollection<Msg>();
                return msgList;
            }
        }

        #endregion

        #region Properties / Filter

        /// <summary>
        /// The _ current import folder
        /// </summary>
        string _ImportSourcePath;
        /// <summary>
        /// Gets or sets the current import folder.
        /// </summary>
        /// <value>The current import folder.</value>
        [ACPropertyInfo(403, "ImportSourcePath", "en{'ImportSourcePath'}de{'ImportSourcePath'}")]
        public string ImportSourcePath
        {
            get
            {
                return _ImportSourcePath;
            }
            set
            {
                if (_ImportSourcePath != value)
                {
                    _ImportSourcePath = value;
                    this[ImportSourcePath_IndexName] = value;
                    OnPropertyChanged("ImportSourcePath");
                }
            }
        }

        private bool _CheckUpdateDate = true;
        [ACPropertyInfo(408, "CheckUpdateDate", "en{'No update if data in the database is newer'}de{'Keine Aktualisierung falls Daten in der Datenbank neuer sind'}")]

        public bool CheckUpdateDate
        {
            get
            {
                return _CheckUpdateDate;
            }
            set
            {
                if (_CheckUpdateDate != value)
                {
                    _CheckUpdateDate = value;
                    OnPropertyChanged("CheckUpdateDate");
                }
            }
        }

        #endregion

        #region ImportItem(Root)

        ACFSItem _CurrentImportItemRoot;
        ACFSItem _CurrentImportItem;

        public bool CurrentImportSuccess { get; set; }

        /// <summary>
        /// Gets or sets the current import project item root.
        /// </summary>
        /// <value>The current import project item root.</value>
        [ACPropertyCurrent(401, "ImportItemRoot")]
        public ACFSItem CurrentImportItemRoot
        {
            get
            {
                return _CurrentImportItemRoot;
            }
            set
            {
                _CurrentImportItemRoot = value;
                OnPropertyChanged("CurrentImportItemRoot");
            }

        }

        /// <summary>
        /// Gets or sets the current import project item.
        /// </summary>
        /// <value>The current import project item.</value>
        [ACPropertyCurrent(402, "ImportItem")]
        public ACFSItem CurrentImportItem
        {
            get
            {
                return _CurrentImportItem;
            }
            set
            {
                if (_CurrentImportItem != value)
                {
                    _CurrentImportItem = value;
                    OnPropertyChanged("CurrentImportItem");
                    OnPropertyChanged("CurrentImportItem\\ItemChangesList");
                    if (value != null)
                    {
                        if (loadItemXMLFileName != value.TrimACUrlFS())
                        {
                            string importXML = LoadItemXMLPreview(value);
                            CurrentImportFileXMLInitial = importXML;
                            CurrentImportFileXML = importXML;
                        }
                    }
                    else
                    {
                        CurrentImportFileXMLInitial = null;
                        CurrentImportFileXML = null;
                    }

                }
            }
        }

        #endregion

        #region XML preview

        private string CurrentImportFileXMLInitial { get; set; }

        /// <summary>
        /// The _ current import file XML
        /// </summary>
        string _CurrentImportFileXML;
        /// <summary>
        /// Gets or sets the current import file XML.
        /// </summary>
        /// <value>The current import file XML.</value>
        [ACPropertyCurrent(404, "", "en{'Filecontent'}de{'Dateiinhalt'}")]
        public string CurrentImportFileXML
        {
            get
            {
                return _CurrentImportFileXML;
            }
            set
            {
                _CurrentImportFileXML = value;
                isFileXMLChanged = CurrentImportFileXMLInitial != value;
                OnPropertyChanged("CurrentImportFileXML");
            }
        }

        private string loadItemXMLFileName;


        private string LoadItemXMLPreview(ACFSItem acFsItem)
        {
            string importXML = null;
            string filename = acFsItem.TrimACUrlFS();
            loadItemXMLFileName = filename;
            if (string.IsNullOrEmpty(filename)) return null;
            if (acFsItem.ResourceType == ResourceTypeEnum.IACObject || acFsItem.ResourceType == ResourceTypeEnum.List || acFsItem.ResourceType == ResourceTypeEnum.XML)
            {
                IResources resource = ResourceFactory.Factory(filename, this);
                importXML = resource.ReadText(filename);
            }
            return importXML;
        }
        #endregion

        #region Filter Import

        List<InstallImportTypeFilter> _ImportTypeFilterList;
        public List<InstallImportTypeFilter> ImportTypeFilterList
        {
            get
            {
                return _ImportTypeFilterList;
            }
        }

        private List<InstallImportTypeFilter> LoadImportTypeFilter()
        {
            if (
                    CurrentImportItemRoot == null ||
                    CurrentImportItemRoot.Container == null ||
                    !CurrentImportItemRoot.Container.Stack.Any()
                ) return null;

            List<InstallImportTypeFilter> filterItemList =
                CurrentImportItemRoot
                .Container
                .Stack
                .Where(x => x.ResourceType == ResourceTypeEnum.IACObject)
                .Select(x => x.ACObject.GetType())
                .Select(x => new { FullTypeName = x.FullName, TypeObject = x })
                .GroupBy(x => x.FullTypeName)
                .Select(x => new { TypeObject = x.FirstOrDefault().TypeObject })
                .Select(x =>
                    new InstallImportTypeFilter()
                    {
                        Type = x.TypeObject,
                        IsChecked = true
                    }).ToList();
            foreach (InstallImportTypeFilter filterItem in filterItemList)
            {
                string acCaptionTranslation = (filterItem.Type.GetCustomAttributes(typeof(ACClassInfo), false).FirstOrDefault() as ACClassInfo).ACCaptionTranslation;
                filterItem.ACCaption = Translator.GetTranslation(acCaptionTranslation);
                filterItem.OnImportTypeFilterChange -= filterItem_OnImportTypeFilterChange;
                filterItem.OnImportTypeFilterChange += filterItem_OnImportTypeFilterChange;
            }
            return filterItemList.OrderBy(x => x.ACCaption).ToList();
        }

        void filterItem_OnImportTypeFilterChange(InstallImportTypeFilter item)
        {
            if (CurrentImportItemRoot != null)
            {
                CurrentImportItemRoot.CallAction(ACFSItemOperations.Filter, item);
                CurrentImportItemRoot.CallAction(ACFSItemOperations.FilterParent, item);
            }
        }

        #endregion

        #region Method

        /// <summary>
        /// Imports this instance.
        /// </summary>
        [ACMethodInfo("Import", "en{'Analyse'}de{'Analysieren'}", 401, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void InspectImport()
        {
            if (!IsEnabledInspectImport()) return;
            Cleanup();
            BackgroundWorker.RunWorkerAsync(BackgroundWorker_DoInspectImport);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledInspectImport()
        {
            return
                !BackgroundWorker.IsBusy
                && !string.IsNullOrEmpty(ImportSourcePath);
        }

        /// <summary>
        /// Imports this instance.
        /// </summary>
        [ACMethodInfo("Import", "en{'Import'}de{'Import'}", 402, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void Import()
        {
            if (!IsEnabledImport()) return;
            msgList = null;

            doImportStartTime = DateTime.Now;
            strDoImportProgressMessage = string.Format("DoImport() Start: {0}", doImportStartTime.ToString("HH:mm:ss"));
#if DEBUG
            System.Diagnostics.Debug.WriteLine(strDoImportProgressMessage);
#endif
            SendMessage(new Msg() { MessageLevel = eMsgLevel.Info, Message = strDoImportProgressMessage });

            BackgroundWorker.RunWorkerAsync(BackgroundWorker_DoImport);
            ShowDialog(this, DesignNameProgressBar);
        }

        /// <summary>
        /// Determines whether [is enabled import].
        /// </summary>
        /// <returns><c>true</c> if [is enabled import]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledImport()
        {
            return CurrentImportItemRoot != null && !BackgroundWorker.IsBusy;
        }

        /// <summary>
        /// Imports the file.
        /// </summary>
        [ACMethodInfo("Import", "en{'Import File'}de{'Import Datei'}", 403, false, false, true)]
        public void ImportFile()
        {
            if (!IsEnabledImportFile()) return;
            try
            {
                IResources resource = ResourceFactory.Factory(CurrentImportItem.ACUrlFS, this);
                ACFSItem feakRoot = new ACFSItem(resource, new ACFSItemContainer(Database, false), null, "", ResourceTypeEnum.Folder);
                IACEntityObjectContext db = DoImportXML(resource, feakRoot, CurrentImportFileXML);
                db.ACSaveChanges();
            }
            catch (Exception ec)
            {
                SendMessage(new Msg()
                {
                    MessageLevel = eMsgLevel.Error,
                    Message = "Exception by XML deserialization! Message: " + ec.Message
                });
            }
        }

        /// <summary>
        /// Determines whether [is enabled import file].
        /// </summary>
        /// <returns><c>true</c> if [is enabled import file]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledImportFile()
        {
            return !string.IsNullOrEmpty(CurrentImportFileXML) && !BackgroundWorker.IsBusy;
        }

        /// <summary>
        /// Imports the save file.
        /// </summary>
        [ACMethodInfo("Import", "en{'Save File'}de{'Datei speichern'}", 404, false, false, true)]
        public void ImportSaveFile()
        {
            if (!IsEnabledImportSaveFile()) return;
            ACFSItem feakRoot = new ACFSItem(null, new ACFSItemContainer(Database, false), null, "", ResourceTypeEnum.Folder);
            try
            {
                IResources resource = ResourceFactory.Factory(CurrentImportItem.ACUrlFS, this);
                DoImportXML(resource, feakRoot, CurrentImportFileXML);
                if (feakRoot.Items == null || !feakRoot.Items.Any())
                {
                    Msg msgWrongSerialization = new Msg()
                    {
                        MessageLevel = eMsgLevel.Error,
                        Message = "XML deserialization problem! No element!"
                    };
                    SendMessage(msgWrongSerialization);
                }
                else
                {
                    List<ACFSItem> deserializedItems = feakRoot.Items.Select(x => x as ACFSItem).ToList();
                    CurrentImportItem.ACObject = deserializedItems.FirstOrDefault().ACObject;
                    ACFSItem xmlPreviewItem = CurrentImportItem;
                    if ((CurrentImportItem.ParentACObject as ACFSItem).ResourceType == ResourceTypeEnum.XML)
                    {
                        (CurrentImportItem.ParentACObject as ACFSItem).SetItems(deserializedItems.ToList());
                        xmlPreviewItem = (CurrentImportItem.ParentACObject as ACFSItem);
                    }
                    resource.WriteText(xmlPreviewItem.TrimACUrlFS(), CurrentImportFileXML);
                    CurrentImportFileXMLInitial = CurrentImportFileXML;
                    isFileXMLChanged = false;
                }
            }
            catch (Exception ec)
            {
                SendMessage(new Msg()
                {
                    MessageLevel = eMsgLevel.Error,
                    Message = "Exception by XML deserialization! Message: " + ec.Message
                });
            }
        }

        public bool IsEnabledImportSaveFile()
        {
            return !string.IsNullOrEmpty(CurrentImportFileXML)
                && isFileXMLChanged;
        }

        /// <summary>
        /// Imports the save file.
        /// </summary>
        [ACMethodInfo("Import", "en{'Undo'}de{'Änderung stornieren'}", 405, false, false, true)]
        public void ImportSaveFileUndo()
        {
            if (!IsEnabledImportSaveFileUndo()) return;
            CurrentImportFileXML = CurrentImportFileXMLInitial;
        }

        private bool isFileXMLChanged;
        public bool IsEnabledImportSaveFileUndo()
        {
            return IsEnabledImportSaveFile();
        }

        /// <summary>
        /// Imports the folder.
        /// </summary>
        [ACMethodInfo("Import", "en{'...'}de{'...'}", 406, false, false, true)]
        public void ImportSource()
        {
            if (!IsEnabledImportSource()) return;
            ImportSourcePath = (string)Root.Businessobjects.ACUrlCommand("BSOiPlusResourceSelect!ResourceDlg", new object[] { ImportSourcePath });
        }

        public bool IsEnabledImportSource()
        {
            return !BackgroundWorker.IsBusy;
        }

        [ACMethodInfo("", "en{'Expand'}de{'Expand'}", 407)]
        public void OnACItemExpand(ACFSItem item)
        {
            if (item.Items != null && item.Items.Any())
            {
                foreach (IACObject child in item.Items)
                {
                    (child as ACFSItem).IsVisible = true;
                    if ((child as ACFSItem).Items.Any())
                    {
                        ((child as ACFSItem).Items.FirstOrDefault() as ACFSItem).IsVisible = true;
                    }
                }
            }
        }

        #endregion

        #region Import DoMehtods

        #region Import DoMethods -> Inspect import


        public InspectImportResult DoInspectImport(ACBackgroundWorker worker, DoWorkEventArgs e)
        {
            InspectImportResult model = new InspectImportResult();
            if (!string.IsNullOrEmpty(ImportSourcePath))
            {
                try
                {
                    DateTime startTime = DateTime.Now;
                    int gipNrFiles = CalculateGipFileNr(ImportSourcePath);

                    IResources rootResources = ResourceFactory.Factory(ImportSourcePath, this);
                    worker.ProgressInfo.OnlyTotalProgress = true;
                    worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
                    worker.ProgressInfo.TotalProgress.ProgressRangeTo = gipNrFiles;
                    ACFSItemContainer rootContainer = new ACFSItemContainer(Database, true);
                    worker.ProgressInfo.AddSubTask(BackgroundWorker_DoInspectImport, 0, gipNrFiles);
                    worker.ProgressInfo.ReportProgress(BackgroundWorker_DoInspectImport, 0, string.Format("Importing {0} / {1} items...", 0, gipNrFiles));
                    int currentItem = 0;
                    rootContainer.OnReportAddFsItem += delegate (ACFSItemContainer cFSItemContainer, ACFSItem aCFSItem)
                    {
                        if (worker.CancellationPending == true)
                        {
                            e.Cancel = true;
                            throw new Exception("DoInspectImport() canceled... ") { Source = Cmd_OnReportAddFsItem_CustomBreak };
                        }

                        if (aCFSItem.ACObject != null)
                        {
                            currentItem++;
                            worker.ProgressInfo.ReportProgress(BackgroundWorker_DoInspectImport, currentItem, string.Format("Importing {0} / {1} items...[{2}]", currentItem, gipNrFiles, aCFSItem.ACObject.ACIdentifier));
                        }
                    };

                    ACFSItem rootACFSItem = rootResources.Dir(Database, rootContainer, ImportSourcePath, true);
                    rootContainer.Stack.Where(x => x.ResourceType == ResourceTypeEnum.IACObject).ToList().ForEach(x => x.IsVisible = false);
                    rootACFSItem.ShowFirst();

                    MsgWithDetails importMsgs = new MsgWithDetails();
                    rootACFSItem.CallAction(ACFSItemOperations.ProcessUpdateDate, importMsgs, "");


                    List<Msg> validationMessages = new List<Msg>();
                    rootACFSItem.CallAction(ACFSItemOperations.ReferenceValidation, validationMessages);
                    if (validationMessages.Count > 0)
                    {
                        foreach (Msg msg in validationMessages)
                            importMsgs.AddDetailMessage(msg);
                    }

                    model.ImportMessages = importMsgs;

                    model.RootACFSItem = rootACFSItem;
                }
                catch (Exception ec)
                {
                    if (ec.Source != Cmd_OnReportAddFsItem_CustomBreak)
                    {
                        SendMessage(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format(@"Error by DoInspectImport - analysing import soruce. Error: {0}", ec.Message) });
                    }
                }

            }
            model.Success = !MsgList.Any(c => c.MessageLevel >= eMsgLevel.Error) && model.ImportMessages.IsSucceded();
            return model;
        }

        private int CalculateGipFileNr(string path)
        {
            int nrFiles = 0;

            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                nrFiles = di.GetFiles("*.gip", SearchOption.AllDirectories).Count();

                try
                {
                    List<string> zipNames = di.GetFiles("*.zip").Select(c => c.FullName).ToList();
                    foreach (var zipName in zipNames)
                    {
                        using (ZipArchive zip = ZipFile.OpenRead(zipName))
                        {
                            int countZipGipEntries = zip.Entries.Where(c => c.FullName.EndsWith(".gip")).Count();
                            nrFiles = nrFiles + countZipGipEntries;
                        }
                    }
                }
                catch (Exception ec)
                {
                    Root.Messages.LogException(GetACUrl(), "CalculateGipFileNr", ec);
                }
            }

            return nrFiles;
        }

        #endregion


        DateTime doImportStartTime = DateTime.Now;
        string strDoImportProgressMessage = "";
        /// <summary>
        /// Does the import.
        /// </summary>
        public MsgWithDetails DoImport(ACBackgroundWorker worker)
        {
            MsgWithDetails importMsg = new MsgWithDetails();
            try
            {
                bool success = false;

                worker.ProgressInfo.ProgressInfoIsIndeterminate = true;
                worker.ProgressInfo.AddSubTask(BackgroundWorker_DoImport, 0, 0);
                worker.ProgressInfo.ReportProgress(BackgroundWorker_DoImport, null, "Begin saving changes ... ");
                CurrentImportItemRoot.CallAction(ACFSItemOperations.AttachOrDeattachToContext, CheckUpdateDate);
                List<IACEntityObjectContext> contextList = new List<IACEntityObjectContext>();

                // CurrentImportItemRoot.CallAction(ACFSItemOperations.RunSomeCheck, null);

                CurrentImportItemRoot.CollectContext(contextList);
                foreach (IACEntityObjectContext context in contextList)
                {
                    var a = context.ChangeTracker.Entries().Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added);
                    MsgWithDetails msg = context.ACSaveChanges(true, false, false);
                    if (msg != null)
                        foreach (var chMsg in msg.MsgDetails)
                            importMsg.AddDetailMessage(chMsg);
                    if (msg != null && !msg.IsSucceded())
                    {
                        context.ACUndoChanges();
                        success = false;
                    }
                }
                success = importMsg.IsSucceded();

                try
                {
                    if (success)
                    {
                        List<string> zipFileNames = new List<string>();
                        CurrentImportItemRoot.CallAction(ACFSItemOperations.CollectZipFileDates, zipFileNames);
                        if (zipFileNames.Any())
                        {
                            ConnectionSettings connectionSettings = new ConnectionSettings();
                            VBSQLCommand vBSQLCommand = new VBSQLCommand(connectionSettings.DefaultConnectionString);
                            foreach (string zipFileName in zipFileNames)
                            {
                                ScriptFileInfo scriptFileInfo = new ScriptFileInfo(zipFileName, CurrentImportItemRoot.Folderpath);
                                if (!vBSQLCommand.ExistVersion(scriptFileInfo.Version))
                                    vBSQLCommand.Insert(scriptFileInfo.Version, scriptFileInfo.Author);
                            }
                        }
                    }
                }
                catch (Exception errorUpdateCtrSyncTable)
                {
                    Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Info, Message = string.Format("Unable to update @ControlScriptSyncInfo table: {0}", errorUpdateCtrSyncTable.Message) };
                    importMsg.AddDetailMessage(errMsg);
                }
            }
            catch (Exception ec)
            {
                Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format("Error by saving import: {0}", ec.Message) };
                importMsg.AddDetailMessage(errMsg);
            }
            return importMsg;
        }

        public IACEntityObjectContext DoImportXML(IResources resources, ACFSItem rootFsItem, string xmlContent)
        {
            ACEntitySerializer serializer = new core.datamodel.ACEntitySerializer();
            // serializer.VBProgress = this.CurrentProgressInfo;
            using (TextReader textReader = new StringReader(xmlContent))
            {
                XElement xDoc = XElement.Load(textReader);
                IACEntityObjectContext cnt = serializer.DeserializeXMLData(resources, rootFsItem, xDoc);
                if (serializer.MsgList.Any())
                    serializer.MsgList.ForEach(x => SendMessage(x));
                return cnt;
            }
        }

        #endregion

        #region BackgroundWorker

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
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = e.Argument.ToString();
            switch (command)
            {
                case BackgroundWorker_DoInspectImport:
                    e.Result = DoInspectImport(worker, e);
                    break;
                case BackgroundWorker_DoImport:
                    e.Result = DoImport(worker);
                    break;
            }
        }

        public override void BgWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            base.BgWorkerCompleted(sender, e);
            CloseWindow(this, DesignNameProgressBar);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = worker.EventArgs.Argument.ToString();

            if (e.Cancelled)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Info, Message = string.Format(@"Operation {0} canceled by user!", command) });
            }
            else if (e.Error != null)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format(@"Error by doing {0}! Message:{1}", command, e.Error.Message) });
            }
            else
            {
                switch (command)
                {
                    case BackgroundWorker_DoInspectImport:
                        InspectImportResult model = e.Result as InspectImportResult;
                        model.EndTime = DateTime.Now;
                        DateTime doInspectImportEndTime = DateTime.Now;
                        TimeSpan duration = model.EndTime - model.StartTime;
                        int fetchedItems = 0;
                        if (model.RootACFSItem != null)
                            fetchedItems = model.RootACFSItem.Container.Stack.Where(x => x.ResourceType == ResourceTypeEnum.IACObject).Count();
                        string strDoInspectImportProgressMessage =
                            string.Format("DoInspectImport() End: {0}. Process duration: {1}, Items fetched: {2}",
                                doInspectImportEndTime.ToString("HH:mm:ss"),
                                string.Format("{0:hh\\:mm\\:ss}", duration), fetchedItems);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(strDoInspectImportProgressMessage);
#endif
                        SendMessage(new Msg() { MessageLevel = eMsgLevel.Info, Message = strDoInspectImportProgressMessage });

                        if (model.ImportMessages != null && model.ImportMessages.MsgDetails.Any())
                            foreach (var childMsg in model.ImportMessages.MsgDetails)
                                if (childMsg.MessageLevel == eMsgLevel.Error)
                                    SendMessage(childMsg);

                        CurrentImportItemRoot = model.RootACFSItem;
                        CurrentImportSuccess = model.Success;
                        _ImportTypeFilterList = LoadImportTypeFilter();
                        OnPropertyChanged("ImportTypeFilterList");

                        break;
                    case BackgroundWorker_DoImport:
                        MsgWithDetails importMsg = e.Result as MsgWithDetails;
                        DateTime doImportEndTime = DateTime.Now;
                        TimeSpan durationImport = doImportEndTime - doImportStartTime;
                        strDoImportProgressMessage =
                            string.Format("DoImport() End: {0}. Process duration: {1}.",
                                doImportEndTime.ToString("HH:mm:ss"),
                                string.Format("{0:hh\\:mm\\:ss}", durationImport));
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(strDoImportProgressMessage);
#endif
                        SendMessage(new Msg() { MessageLevel = eMsgLevel.Info, Message = strDoImportProgressMessage });
                        if (importMsg.MsgDetails.Any())
                            foreach (var msg in importMsg.MsgDetails)
                                SendMessage(msg);
                        break;
                }
            }

        }

        #endregion

        #region IMsgObserver

        public void SendMessage(Msg msg)
        {
            //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            //{
                MsgList.Add(msg);
            //});
            OnPropertyChanged(nameof(MsgList));
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "InspectImport":
                    InspectImport();
                    return true;
                case "Import":
                    Import();
                    return true;
                case "IsEnabledImport":
                    result = IsEnabledImport();
                    return true;
                case "ImportFile":
                    ImportFile();
                    return true;
                case "IsEnabledImportFile":
                    result = IsEnabledImportFile();
                    return true;
                case "ImportSaveFile":
                    ImportSaveFile();
                    return true;
                case "IsEnabledImportSaveFile":
                    result = IsEnabledImportSaveFile();
                    return true;
                case "ImportSaveFileUndo":
                    ImportSaveFileUndo();
                    return true;
                case "IsEnabledImportSaveFileUndo":
                    result = IsEnabledImportSaveFileUndo();
                    return true;
                case "ImportSource":
                    ImportSource();
                    return true;
                case "IsEnabledImportSource":
                    result = IsEnabledImportSource();
                    return true;
                case "OnACItemExpand":
                    OnACItemExpand((ACFSItem)acParameter[0]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

    public class InspectImportResult
    {
        public bool Success { get; set; }
        public ACFSItem RootACFSItem { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public MsgWithDetails ImportMessages { get; set; }
    }
}

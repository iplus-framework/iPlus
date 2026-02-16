// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static gip.core.datamodel.Global;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'Export'}de{'Export'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, false, true)]
    public class BSOiPlusExport : ACBSO
    {
        #region const

        public const string BackgroundWorker_Export = "Export";
        public const string BackgroundWorker_Package = "Package";

        private SynchronizationContext _MainSyncContext;


        #endregion

        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="BSOiPlusExport"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOiPlusExport(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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

            _ACEntitySerializer = new ACEntitySerializer();
            _QRYACProject = gip.core.datamodel.Database.Root.Queries.CreateQuery(Database as IACComponent, Const.QueryPrefix + ACProject.ClassName, null);

            CurrentExportACProject = ExportACProjectList.Where(c => c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Root).FirstOrDefault();

            if (this[CustomerDataPath_IndexName] != null && Directory.Exists(this[CustomerDataPath_IndexName].ToString()))
            {
                CurrentExportFolder = this[CustomerDataPath_IndexName].ToString();
            }
            else
            {
                CurrentExportFolder = Root.Environment.Datapath;
            }

            _ExportFromTime = DateTime.Now.Date;

            _ExportCommand = new ExportCommand();
            _ExportCommand.ExportErrorEvent += ExportCommand_ExportErrorEvent;
            _ExportCommand.ExportProgressEvent += ExportCommand_ExportProgressEvent;
            PackageExportUser = Root.Environment.User.Initials;

            _MainSyncContext = SynchronizationContext.Current;

            return true;
        }



        public async override Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            this._CurrentExportACProject = null;
            this._CurrentExportFolder = null;
            this._CurrentExportProjectItem = null;
            this._CurrentExportProjectItemRootChangeInfo = null;
            this._CurrentMsg = null;

            _ExportCommand.ExportErrorEvent -= ExportCommand_ExportErrorEvent;
            _ExportCommand.ExportProgressEvent -= ExportCommand_ExportProgressEvent;
            _ExportCommand = null;

            if (_ACProjectManager != null)
                _ACProjectManager.PropertyChanged -= _ACProjectManager_PropertyChanged;
            _ExportCommand = null;

            bool done = await base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }

            _MainSyncContext = null;

            return done;
        }

        #endregion

        #region consts

        public const string CustomerDataPath_IndexName = "CustomerDataPath";

        #endregion

        #region Messages/Progress / Database

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

        public void MsgListClear()
        {
            msgList = null;
        }
        #endregion

        #region Serializer - QRYACProject

        /// <summary>
        /// Gets the AC entity serializer.
        /// </summary>
        /// <value>The AC entity serializer.</value>
        public ACEntitySerializer ACEntitySerializer
        {
            get
            {
                return _ACEntitySerializer;
            }
        }

        /// <summary>
        /// The _ QRYAC project
        /// </summary>
        ACQueryDefinition _QRYACProject;
        /// <summary>
        /// Gets the QRYAC project.
        /// </summary>
        /// <value>The QRYAC project.</value>
        public ACQueryDefinition QRYACProject
        {
            get
            {
                return _QRYACProject;
            }
        }

        private ExportCommand _ExportCommand;
        public ExportCommand ExportCommand
        {
            get
            {
                return _ExportCommand;
            }
        }

        public void LoadExportCommandConfig()
        {
            ExportCommand.IsExportACClass = IsExportACClass;
            ExportCommand.IsExportACClassProperty = IsExportACClassProperty;
            ExportCommand.IsExportACClassMethod = IsExportACClassMethod;
            ExportCommand.IsExportACClassPropertyRelation = IsExportACClassPropertyRelation;
            ExportCommand.IsExportACClassConfig = IsExportACClassConfig;
            ExportCommand.IsExportACClassMessage = IsExportACClassMessage;
            ExportCommand.IsExportACClassText = IsExportACClassText;
            ExportCommand.IsExportACClassDesign = IsExportACClassDesign;
            ExportCommand.UseExportFromTime = UseExportFromTime;
            ExportCommand.ExportFromTime = ExportFromTime;
        }

        private void ExportCommand_ExportProgressEvent(int currentItem, string progressMessage)
        {
            BackgroundWorker.ProgressInfo.ReportProgress("DoExport", currentItem, progressMessage);
        }

        private void ExportCommand_ExportErrorEvent(ExportErrorEventArgs exportErrorEventArgs)
        {
            Msg errMessage = new Msg();
            string message = "";
            errMessage.MessageLevel = eMsgLevel.Error;
            switch (exportErrorEventArgs.ExportErrorType)
            {
                case ExportErrosEnum.MissingPath:
                    message = Root.Environment.TranslateMessage(this, "Error50208", exportErrorEventArgs.ACUrl, exportErrorEventArgs.Path);
                    break;
                case ExportErrosEnum.ErrorWriteFile:
                    message = Root.Environment.TranslateMessage(this, "Error50207", exportErrorEventArgs.ACUrl, exportErrorEventArgs.Exception);
                    break;
                default:
                    break;
            }
            errMessage.Message = message;

            SendMessage(errMessage);

        }

        #endregion

        #region Properties

        #region Properties -> Export folder

        /// <summary>
        /// The _ current export folder
        /// </summary>
        string _CurrentExportFolder;
        /// <summary>
        /// Gets or sets the current export folder.
        /// </summary>
        /// <value>The current export folder.</value>
        [ACPropertyCurrent(406, "ExportFolder", "en{'ExportFolder'}de{'Exportordner'}")]
        public string CurrentExportFolder
        {
            get
            {
                return _CurrentExportFolder;
            }
            set
            {
                if (_CurrentExportFolder != value)
                {
                    if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
                        this[CustomerDataPath_IndexName] = value;
                    _CurrentExportFolder = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Properties -> Filter
        /// <summary>
        /// The _ is export AC class
        /// </summary>
        bool _IsExportACClass = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is export AC class.
        /// </summary>
        /// <value><c>true</c> if this instance is export AC class; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(407, "", "en{'Export Classes'}de{'Export Klassen'}")]
        public bool IsExportACClass
        {
            get
            {
                return _IsExportACClass;
            }
            set
            {
                _IsExportACClass = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The _ is export AC class property
        /// </summary>
        bool _IsExportACClassProperty = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is export AC class property.
        /// </summary>
        /// <value><c>true</c> if this instance is export AC class property; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(408, "", "en{'Export Properties'}de{'Export Eigenschaften'}")]
        public bool IsExportACClassProperty
        {
            get
            {
                return _IsExportACClassProperty;
            }
            set
            {
                _IsExportACClassProperty = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The _ is export AC class method
        /// </summary>
        bool _IsExportACClassMethod = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is export AC class method.
        /// </summary>
        /// <value><c>true</c> if this instance is export AC class method; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(409, "", "en{'Export Methods'}de{'Export Methoden'}")]
        public bool IsExportACClassMethod
        {
            get
            {
                return _IsExportACClassMethod;
            }
            set
            {
                _IsExportACClassMethod = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The _ is export AC class design
        /// </summary>
        bool _IsExportACClassDesign = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is export AC class design.
        /// </summary>
        /// <value><c>true</c> if this instance is export AC class design; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(410, "", "en{'Export Designs'}de{'Export Designs'}")]
        public bool IsExportACClassDesign
        {
            get
            {
                return _IsExportACClassDesign;
            }
            set
            {
                _IsExportACClassDesign = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The _ is export AC class config
        /// </summary>
        bool _IsExportACClassConfig = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is export AC class config.
        /// </summary>
        /// <value><c>true</c> if this instance is export AC class config; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(411, "", "en{'Export Classconfig'}de{'Export Klassenkonfiguration'}")]
        public bool IsExportACClassConfig
        {
            get
            {
                return _IsExportACClassConfig;
            }
            set
            {
                _IsExportACClassConfig = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The _ is export AC class text
        /// </summary>
        bool _IsExportACClassText = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is export AC class text.
        /// </summary>
        /// <value><c>true</c> if this instance is export AC class text; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(412, "", "en{'Export Texts'}de{'Export Texte'}")]
        public bool IsExportACClassText
        {
            get
            {
                return _IsExportACClassText;
            }
            set
            {
                _IsExportACClassText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The _ is export AC class message
        /// </summary>
        bool _IsExportACClassMessage = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is export AC class message.
        /// </summary>
        /// <value><c>true</c> if this instance is export AC class message; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(413, "", "en{'Export Messages'}de{'Export Meldungen'}")]
        public bool IsExportACClassMessage
        {
            get
            {
                return _IsExportACClassMessage;
            }
            set
            {
                _IsExportACClassMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The _ is export AC class property relation
        /// </summary>
        bool _IsExportACClassPropertyRelation = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is export AC class property relation.
        /// </summary>
        /// <value><c>true</c> if this instance is export AC class property relation; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(414, "", "en{'Export Property Relation'}de{'Export Eigenschaftsbeziehung'}")]
        public bool IsExportACClassPropertyRelation
        {
            get
            {
                return _IsExportACClassPropertyRelation;
            }
            set
            {
                _IsExportACClassPropertyRelation = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The _ is export delete folder
        /// </summary>
        bool _IsExportDeleteFolder = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is export delete folder.
        /// </summary>
        /// <value><c>true</c> if this instance is export delete folder; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(415, "", "en{'Delete old directory'}de{'Altes Verzeichnis löschen'}")]
        public bool IsExportDeleteFolder
        {
            get
            {
                return _IsExportDeleteFolder;
            }
            set
            {
                _IsExportDeleteFolder = value;
                OnPropertyChanged();
            }
        }

        private DateTime _ExportFromTime;
        [ACPropertyCurrent(416, "", "en{'Export from time'}de{'Von Zeitpunkt exportieren'}")]
        public DateTime ExportFromTime
        {
            get
            {
                return _ExportFromTime;
            }
            set
            {
                _ExportFromTime = value;
            }
        }

        private bool _UseExportFromTime;
        [ACPropertyCurrent(417, "", "en{'Use export from time'}de{'Von Zeitpunkt exportieren'}")]
        public bool UseExportFromTime
        {
            get
            {
                return _UseExportFromTime;
            }
            set
            {
                _UseExportFromTime = value;
            }
        }

        /// <summary>
        /// The _ search class text
        /// </summary>
        string _SearchClassText = "";
        /// <summary>
        /// Gets or sets the search class text.
        /// </summary>
        /// <value>The search class text.</value>
        [ACPropertyInfo(418, "TreeConfig", "en{'Search Class'}de{'Suche Klasse'}")]
        public string SearchClassText
        {
            get
            {
                return _SearchClassText;
            }
            set
            {
                string inputString = value ?? "";
                if (_SearchClassText != inputString)
                {
                    _SearchClassText = inputString;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// The _ with caption
        /// </summary>
        bool _WithCaption = false;
        /// <summary>
        /// Gets or sets a value indicating whether [with caption].
        /// </summary>
        /// <value><c>true</c> if [with caption]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(419, "TreeConfig", "en{'With Caption'}de{'Mit Bezeichnung'}")]
        public bool WithCaption
        {
            get
            {
                return _WithCaption;
            }
            set
            {
                if (_WithCaption != value)
                {
                    _WithCaption = value;
                    OnPropertyChanged();
                    RefreshProjectTree();
                }
            }
        }

        /// <summary>
        /// The _ show group
        /// </summary>
        bool _ShowGroup = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show group].
        /// </summary>
        /// <value><c>true</c> if [show group]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(420, "TreeConfig", "en{'Grouped'}de{'Gruppiert'}")]
        public bool ShowGroup
        {
            get
            {
                return _ShowGroup;
            }
            set
            {
                _ShowGroup = value;
                OnPropertyChanged();
                RefreshProjectTree();
            }
        }



        private ACClassInfoWithItems.CheckHandler _ProjectTreeCheckHandler;
        protected ACClassInfoWithItems.CheckHandler ProjectTreeCheckHandler
        {
            get
            {
                if (_ProjectTreeCheckHandler == null)
                {
                    _ProjectTreeCheckHandler = new ACClassInfoWithItems.CheckHandler()
                    {
                        IsCheckboxVisible = true,
                        CheckedSetter = null,
                        CheckedGetter = null,
                    };
                }
                return _ProjectTreeCheckHandler;
            }
        }

        protected ACClassInfoWithItems.VisibilityFilters ProjectTreeVisibilityFilter
        {
            get
            {
                ACClassInfoWithItems.VisibilityFilters filter
                    = new ACClassInfoWithItems.VisibilityFilters()
                    {
                        SearchText = this.SearchClassText,
                        CustomFilter = ProcessForUseExportFromTime
                    };
                return filter;
            }
        }

        protected ACProjectManager.PresentationMode ProjectTreePresentationMode
        {
            get
            {
                ACProjectManager.PresentationMode mode
                    = new ACProjectManager.PresentationMode()
                    {
                        ShowCaptionInTree = WithCaption,
                        DisplayGroupedTree = ShowGroup,
                        DisplayTreeAsMenu = null
                    };
                return mode;
            }
        }


        #endregion

        #region Properties -> ACProject
        /// <summary>
        /// The _ current export AC project
        /// </summary>
        ACProject _CurrentExportACProject;
        /// <summary>
        /// Gets or sets the current export AC project.
        /// </summary>
        /// <value>The current export AC project.</value>
        [ACPropertyCurrent(401, "ExportACProject")]
        public ACProject CurrentExportACProject
        {
            get
            {
                return _CurrentExportACProject;
            }
            set
            {
                if (_CurrentExportACProject != value)
                {
                    _CurrentExportACProject = value;
                    OnPropertyChanged();
                    LoadExportTree();
                }
            }
        }

        /// <summary>
        /// Gets the export AC project list.
        /// </summary>
        /// <value>The export AC project list.</value>
        [ACPropertyList(402, "ExportACProject")]
        public IEnumerable<ACProject> ExportACProjectList
        {
            get
            {
                return Database.ContextIPlus.ACProject.OrderBy(c => c.ACProjectTypeIndex).ThenBy(c => c.ACProjectName).ToList();
            }
        }

        #endregion

        #region Properties -> Tree

        /// <summary>
        /// Gets the current export project item root.
        /// </summary>
        /// <value>The current export project item root.</value>
        [ACPropertyCurrent(403, "ProjectItemRoot")]
        public ACClassInfoWithItems CurrentExportProjectItemRoot
        {
            get
            {
                return ProjectManager.CurrentProjectItemRoot;
            }
        }

        /// <summary>
        /// The _ current export project item root change info
        /// </summary>
        ChangeInfo _CurrentExportProjectItemRootChangeInfo = null;
        /// <summary>
        /// Gets or sets the current export project item root change info.
        /// </summary>
        /// <value>The current export project item root change info.</value>
        [ACPropertyChangeInfo(404, "ProjectItem")]
        public ChangeInfo CurrentExportProjectItemRootChangeInfo
        {
            get
            {
                return _CurrentExportProjectItemRootChangeInfo;
            }
            set
            {
                _CurrentExportProjectItemRootChangeInfo = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The _ current export project item
        /// </summary>
        ACClassInfoWithItems _CurrentExportProjectItem = null;
        /// <summary>
        /// Gets or sets the current export project item.
        /// </summary>
        /// <value>The current export project item.</value>
        [ACPropertyCurrent(405, "ProjectItem")]
        public ACClassInfoWithItems CurrentExportProjectItem
        {
            get
            {
                return _CurrentExportProjectItem;
            }
            set
            {
                if (_CurrentExportProjectItem != value)
                {
                    _CurrentExportProjectItem = value;
                    _LoadedItemsList = LoadLoadedItemsList();
                    OnPropertyChanged(nameof(LoadedItemsList));
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        public async Task<bool> IsTreeFilterWithoutNameAndGroup()
        {
            if (ProjectTreePresentationMode.ShowCaptionInTree || ProjectTreePresentationMode.DisplayGroupedTree)
            {
                string msg = Translator.GetTranslation("en{'Options With Caption and Groupped should be off. Continue?'}de{'Optionen Mit Bezeichnung und Gruppiert sollte aus sein. Weiter?'}");
                MsgResult result = await Root.Messages.QuestionAsync(this, msg);
                if (result == MsgResult.Yes)
                {
                    _ShowGroup = false;
                    _WithCaption = false;
                    OnPropertyChanged(nameof(ShowGroup));
                    OnPropertyChanged(nameof(WithCaption));
                    RefreshProjectTree();
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Exports this instance.
        /// </summary>
        [ACMethodInfo("Export", "en{'Export'}de{'Export'}", 401, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public async void Export()
        {
            if (!IsEnabledExport()) return;
            if (BackgroundWorker.IsBusy) return;
            if (!await IsTreeFilterWithoutNameAndGroup()) return;

            ClearMessage();

            BackgroundWorker.RunWorkerAsync(BackgroundWorker_Export);
            await ShowDialogAsync(this, DesignNameProgressBar);
        }

        /// <summary>
        /// Determines whether [is enabled export].
        /// </summary>
        /// <returns><c>true</c> if [is enabled export]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledExport()
        {
            return CurrentExportProjectItemRoot != null && !BackgroundWorker.IsBusy;
        }

        /// <summary>
        /// Exports the folder.
        /// </summary>
        [ACMethodInfo("Export", "en{'...'}de{'...'}", 402, false, false, true)]
        public async Task ExportFolder()
        {
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            CurrentExportFolder = await mediaController.OpenFileDialog(true, CurrentExportFolder, true) ?? CurrentExportFolder;
        }

        public bool IsEnabledExportFolder()
        {
            return true;
        }

        [ACMethodInfo("LoadExportTree", "en{'Filter and load'}de{'Filtern und anzeigen'}", 9999, false, false, true)]
        public void LoadExportTree()
        {
            if (!IsEnabledLoadExportTree())
                return;
            ProjectManager.LoadACProject(CurrentExportACProject.ACProjectID, ProjectTreePresentationMode, ProjectTreeVisibilityFilter, ProjectTreeCheckHandler);
        }

        private void RefreshProjectTree(bool forceRebuildTree = false)
        {
            ProjectManager.RefreshProjectTree(ProjectTreePresentationMode, ProjectTreeVisibilityFilter, ProjectTreeCheckHandler, forceRebuildTree);
        }

        /// <summary>
        /// Filtering data for export by time
        /// </summary>
        /// <param name="classInfoItem"></param>
        private bool ProcessForUseExportFromTime(ACClassInfoWithItems classInfoItem)
        {
            return
                !UseExportFromTime
                || classInfoItem.ValueT == null
                ||
                    (
                        (IsExportACClass && classInfoItem.ValueT.UpdateDate >= ExportFromTime)
                        || (IsExportACClassProperty && classInfoItem.ValueT.ACClassProperty_ACClass.Any(x => x.UpdateDate >= ExportFromTime))
                        || (IsExportACClassMethod && classInfoItem.ValueT.ACClassMethod_ACClass.Any(x => x.UpdateDate >= ExportFromTime))
                        || (IsExportACClassDesign && classInfoItem.ValueT.ACClassDesign_ACClass.Any(x => x.UpdateDate >= ExportFromTime))
                        || (IsExportACClassText && classInfoItem.ValueT.ACClassText_ACClass.Any(x => x.UpdateDate >= ExportFromTime))
                        || (IsExportACClassMessage && classInfoItem.ValueT.ACClassMessage_ACClass.Any(x => x.UpdateDate >= ExportFromTime))
                        || (IsExportACClassConfig && classInfoItem.ValueT.ACClassConfig_ACClass.Any(x => x.UpdateDate >= ExportFromTime))
                        || (IsExportACClassPropertyRelation && classInfoItem.ValueT.ACClassPropertyRelation_TargetACClass.Any(x => x.UpdateDate >= ExportFromTime))
                      );
        }

        public bool IsEnabledLoadExportTree()
        {
            return !BackgroundWorker.IsBusy && CurrentExportACProject != null;
        }

        #endregion

        #region Preview items for load

        #region LoadedItems
        private ExportItemPreviewModel _SelectedLoadedItems;
        /// <summary>
        /// Selected property for ACFSItem
        /// </summary>
        /// <value>The selected LoadedItems</value>
        [ACPropertySelected(421, "LoadedItems", "en{'TODO: LoadedItems'}de{'TODO: LoadedItems'}")]
        public ExportItemPreviewModel SelectedLoadedItems
        {
            get
            {
                return _SelectedLoadedItems;
            }
            set
            {
                if (_SelectedLoadedItems != value)
                {
                    _SelectedLoadedItems = value;
                    OnPropertyChanged();
                }
            }
        }


        private List<ExportItemPreviewModel> _LoadedItemsList;
        /// <summary>
        /// List property for ACFSItem
        /// </summary>
        /// <value>The LoadedItems list</value>
        [ACPropertyList(422, "LoadedItems")]
        public List<ExportItemPreviewModel> LoadedItemsList
        {
            get
            {
                if (_LoadedItemsList == null)
                    _LoadedItemsList = LoadLoadedItemsList();
                return _LoadedItemsList;
            }
        }

        private List<ExportItemPreviewModel> LoadLoadedItemsList()
        {
            if (CurrentExportProjectItem == null || CurrentExportProjectItem.ValueT == null) return null;

            List<ExportItemPreviewModel> itemList = new List<ExportItemPreviewModel>();
            ACClass classItem = CurrentExportProjectItem.ValueT;
            ACQueryDefinition qryACClass = QRYACProject.ACUrlCommand(Const.QueryPrefix + ACClass.ClassName) as ACQueryDefinition;
            if (IsExportACClass)
            {
            }
            if (IsExportACClassProperty)
            {
                LoadClassSubItemPresentation(itemList, classItem, qryACClass, Const.QueryPrefix + ACClassProperty.ClassName);
            }
            if (IsExportACClassMethod)
            {
                LoadClassSubItemPresentation(itemList, classItem, qryACClass, Const.QueryPrefix + ACClassMethod.ClassName);
            }
            if (IsExportACClassDesign)
            {
                LoadClassSubItemPresentation(itemList, classItem, qryACClass, Const.QueryPrefix + ACClassDesign.ClassName);
            }
            if (IsExportACClassText)
            {
                LoadClassSubItemPresentation(itemList, classItem, qryACClass, Const.QueryPrefix + ACClassText.ClassName);
            }
            if (IsExportACClassMessage)
            {
                LoadClassSubItemPresentation(itemList, classItem, qryACClass, Const.QueryPrefix + ACClassMessage.ClassName);
            }
            if (IsExportACClassConfig)
            {
                LoadClassSubItemPresentation(itemList, classItem, qryACClass, Const.QueryPrefix + ACClassConfig.ClassName);
            }
            if (IsExportACClassPropertyRelation)
            {
                LoadClassSubItemPresentation(itemList, classItem, qryACClass, Const.QueryPrefix + ACClassPropertyRelation.ClassName);
            }
            return itemList;
        }

        private void LoadClassSubItemPresentation(List<ExportItemPreviewModel> itemList, ACClass classItem, ACQueryDefinition qryACClass, string subQueryName)
        {
            ACQueryDefinition qryACClassProperty = qryACClass.ACUrlCommand(subQueryName) as ACQueryDefinition;
            IEnumerable query = classItem.ACSelect(qryACClassProperty);
            if (query != null)
                foreach (var item in query)
                {
                    IACObject iACObject = item as IACObject;
                    bool forImport = true;
                    IUpdateInfo updateInfo = iACObject as IUpdateInfo;

                    string typeName = item.GetType().Name;
                    if (item.GetType().GetCustomAttributes(typeof(ACClassInfo), false).Any())
                    {
                        ACClassInfo acClassInfo = (ACClassInfo)item.GetType().GetCustomAttributes(typeof(ACClassInfo), false)[0];
                        typeName = Translator.GetTranslation(acClassInfo.ACCaptionTranslation);
                    }

                    if (UseExportFromTime)
                    {

                        forImport = updateInfo.UpdateDate >= ExportFromTime;
                    }
                    if (forImport)
                    {
                        itemList.Add(new ExportItemPreviewModel()
                        {
                            ACCaption = iACObject.ACCaption,
                            ACIdentifier = iACObject.ACIdentifier,
                            UpdateDate = updateInfo.UpdateDate,
                            TypeName = typeName
                        });
                    }
                }
        }

        #endregion

        #endregion

        #region Methods - > DoExport

        /// <summary>
        /// Does the export.
        /// </summary>
        private Msg DoExportPackage(ACBackgroundWorker worker, DoWorkEventArgs e, bool doPackage)
        {
            LoadExportCommandConfig();
            worker.ProgressInfo.OnlyTotalProgress = true;

            worker.ProgressInfo.TotalProgress.ProgressText = "Exporting ...";
            Msg errorMessage = null;
            try
            {
                if (IsExportDeleteFolder)
                {
                    try
                    {
                        if (Directory.Exists(Path.Combine(CurrentExportFolder, CurrentExportACProject.ACIdentifier)))
                            Directory.Delete(Path.Combine(CurrentExportFolder, CurrentExportACProject.ACIdentifier), true);
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                        if (ex.InnerException != null && ex.InnerException.Message != null)
                            msg += " Inner:" + ex.InnerException.Message;
                        errorMessage = new Msg() { Message = msg, MessageLevel = eMsgLevel.Error, ACIdentifier = ACIdentifier };
                        Messages.LogException(nameof(BSOiPlusExport), nameof(DoExportPackage) + "(10)", msg);
                        return errorMessage;
                    }
                }

                // Root-Ordner anlegen
                if (!CheckOrCreateDirectory(CurrentExportFolder))
                    return new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format(@"Missing folder: {0}", CurrentExportFolder) };

                int totalItems = 0;
                Action<ACClassInfoWithItems> action = delegate (ACClassInfoWithItems item)
                {
                    if (item.IsChecked)
                        totalItems++;
                };
                CurrentExportProjectItemRoot.CallOnAllItems(action);
                int currentItem = 0;
                worker.ProgressInfo.TotalProgress.ProgressRangeTo = totalItems;

                ACQueryDefinition qryACClass = QRYACProject.ACUrlCommand(Const.QueryPrefix + ACClass.ClassName) as ACQueryDefinition;

                if (!doPackage)
                    ExportCommand.DoExport(worker, e, ACEntitySerializer, QRYACProject, qryACClass, CurrentExportACProject, CurrentExportProjectItemRoot, CurrentExportFolder, currentItem, totalItems);
                else
                {
                    string subExportFolderName = ExportCommand.DoFolder(worker, e, ACEntitySerializer, QRYACProject, qryACClass, CurrentExportACProject, CurrentExportProjectItemRoot, CurrentExportFolder, currentItem, totalItems, PackageExportUser);
                    ExportCommand.DoPackage(CurrentExportFolder, subExportFolderName);
                }

            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;
                errorMessage = new Msg() { Message = msg, MessageLevel = eMsgLevel.Error, ACIdentifier = ACIdentifier };
                Messages.LogException(nameof(BSOiPlusExport), nameof(DoExportPackage) + "(20)", msg);
            }
            BackgroundWorker.ProgressInfo.TotalProgress.ProgressText = "";
            if (errorMessage != null)
                return errorMessage;
            return null;
        }

        #endregion

        #region PackageExportUser

        public static string PackageExportFolderTemplate = @"{datetime}_{user}";

        [ACPropertyInfo(423, "PackageExportUser", "en{'Developer'}de{'Entwickler'}")]
        public string PackageExportUser { get; set; }

        [ACMethodInfo("PackageDlg", "en{'Package'}de{'Paket'}", 403, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public async void PackageDlg()
        {
            if (!IsEnabledPackageDlg()) return;
            await ShowDialogAsync(this, "Package");
        }

        public bool IsEnabledPackageDlg()
        {
            return IsEnabledExport();
        }

        [ACMethodInfo("PackageDlgOk", Const.Ok, 404, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public async void PackageDlgOk()
        {
            if (!IsEnabledPackageDlgOk()) return;
            if (BackgroundWorker.IsBusy) return;
            CloseTopDialog();

            ClearMessage();

            BackgroundWorker.RunWorkerAsync(BackgroundWorker_Package);
            await ShowDialogAsync(this, DesignNameProgressBar);
        }

        public bool IsEnabledPackageDlgOk()
        {
            return Directory.Exists(CurrentExportFolder) && !string.IsNullOrEmpty(PackageExportUser) && !BackgroundWorker.IsBusy;
        }

        [ACMethodInfo("PackageDlgCancel", "en{'Cancel'}de{'Schließen'}", 405, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void PackageDlgCancel()
        {
            CloseTopDialog();
        }
        public bool IsEnabledPackageDlgCancel()
        {
            return true;
        }

        #endregion

        #region Messages

        Msg _CurrentMsg;
        /// <summary>
        /// Gets or sets the current MSG.
        /// </summary>
        /// <value>The current MSG.</value>
        [ACPropertyCurrent(424, "Message", "en{'Message'}de{'Meldung'}")]
        public Msg CurrentMsg
        {
            get
            {
                return _CurrentMsg;
            }
            set
            {
                _CurrentMsg = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Msg> msgList;
        /// <summary>
        /// Gets the MSG list.
        /// </summary>
        /// <value>The MSG list.</value>
        [ACPropertyList(425, "Message", "en{'Messagelist'}de{'Meldungsliste'}")]
        public ObservableCollection<Msg> MsgList
        {
            get
            {
                if (msgList == null)
                    msgList = new ObservableCollection<Msg>();
                return msgList;
            }
        }

        public void SendMessage(Msg msg)
        {
            _MainSyncContext?.Send((object state) =>
            {
                MsgList.Add(msg);
                OnPropertyChanged(nameof(MsgList));
            }, new object());
        }


        public void ClearMessage()
        {
            MsgList.Clear();
            OnPropertyChanged(nameof(MsgList));
        }

        #endregion

        #region Hilfsmethoden
        /// <summary>
        /// Checks the or create directory.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool CheckOrCreateDirectory(string folder)
        {
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException(nameof(BSOiPlusExport), nameof(CheckOrCreateDirectory) + "(10)", msg);

                return false;
            }
            return true;
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
                case BackgroundWorker_Export:
                    e.Result = DoExportPackage(worker, e, false);
                    break;
                case BackgroundWorker_Package:
                    e.Result = DoExportPackage(worker, e, true);
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
                Msg msg = null;
                if (e.Result != null)
                    msg = (Msg)e.Result;
                if (msg != null)
                    SendMessage(msg);
            }
        }

        public override void BgWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            base.BgWorkerProgressChanged(sender, e);
            if (e.UserState != null && e.UserState is Msg)
            {
                Msg msg = e.UserState as Msg;
                SendMessage(msg);
            }
        }

        #endregion

        #region private Property

        /// <summary>
        /// The _ AC project manager
        /// </summary>
        ACProjectManager _ACProjectManager;
        public ACProjectManager ProjectManager
        {
            get
            {
                if (_ACProjectManager != null)
                    return _ACProjectManager;
                _ACProjectManager = new ACProjectManager(Database.ContextIPlus, Root);
                _ACProjectManager.PropertyChanged += _ACProjectManager_PropertyChanged;
                return _ACProjectManager;
            }
        }

        private void _ACProjectManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ACProjectManager.CurrentProjectItemRootPropName)
                OnPropertyChanged(nameof(CurrentExportProjectItemRoot));
        }

        /// <summary>
        /// The _ AC entity serializer
        /// </summary>
        ACEntitySerializer _ACEntitySerializer;

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Export):
                    Export();
                    return true;
                case nameof(IsEnabledExport):
                    result = IsEnabledExport();
                    return true;
                case nameof(ExportFolder):
                    _ = ExportFolder();
                    return true;
                case nameof(IsEnabledExportFolder):
                    result = IsEnabledExportFolder();
                    return true;
                case nameof(LoadExportTree):
                    LoadExportTree();
                    return true;
                case nameof(IsEnabledLoadExportTree):
                    result = IsEnabledLoadExportTree();
                    return true;
                case nameof(PackageDlg) :
                    PackageDlg();
                    return true;
                case nameof(IsEnabledPackageDlg):
                    result = IsEnabledPackageDlg();
                    return true;
                case nameof(PackageDlgOk):
                    PackageDlgOk();
                    return true;
                case nameof(IsEnabledPackageDlgOk):
                    result = IsEnabledPackageDlgOk();
                    return true;
                case nameof(PackageDlgCancel):
                    PackageDlgCancel();
                    return true;
                case nameof(IsEnabledPackageDlgCancel):
                    result = IsEnabledPackageDlgCancel();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public override IEnumerable<string> GetPropsToObserveForIsEnabled(string acMethodName)
        {
            switch (acMethodName)
            {
                case nameof(Export):
                case nameof(IsEnabledExport):
                case nameof(PackageDlg):
                case nameof(IsEnabledPackageDlg):
                    return new string[] { nameof(CurrentExportProjectItemRoot) };
                case nameof(ExportFolder):
                case nameof(IsEnabledExportFolder):
                case nameof(PackageDlgCancel):
                case nameof(IsEnabledPackageDlgCancel):
                    return new string[] { nameof(InitState)};
                case nameof(LoadExportTree):
                case nameof(IsEnabledLoadExportTree):
                    return new string[] { nameof(CurrentExportACProject) };
                case nameof(PackageDlgOk):
                case nameof(IsEnabledPackageDlgOk):
                    return new string[] { nameof(CurrentExportFolder), nameof(PackageExportUser) };
            }
            return base.GetPropsToObserveForIsEnabled(acMethodName);
        }



        private bool IsPathValid(string path)
        {
            bool isValid = false;
            try
            {
                Path.GetFullPath(path);
                isValid = true;
            }
            catch (Exception)
            {

            }
            return isValid;
        }

        #endregion

    }
}
;
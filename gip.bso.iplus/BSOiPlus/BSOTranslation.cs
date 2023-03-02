using gip.core.autocomponent;
using gip.core.datamodel;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;

namespace gip.bso.iplus
{

    /// <summary>
    /// Handle translations from one place
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Translation'}de{'Translation'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACPackage.ClassName)]
    public class BSOTranslation : ACBSO
    {
        #region const
        public const string Const_ExportFileTemplate = @"Translation_{0}.json";
        public const string Const_CustomerDataPath = "CustomerDataPath";
        public const string Const_GoogleProjectID = "GoogleProjectID";
        public const int Const_GoogleAPIBytesLimit = 204800;
        // @aagincic: Google limit is 1024 but recive service exception: "grpc_message":"Text is too long.","grpc_status":3} - without any other explanation
        public const int Const_GoogleAPILengthLimit = 250;
        #endregion

        #region DI
        public TranslationServiceClient GoogleTranslationServiceClient { get; private set; }

        // public string GoogleProjectID { get; private set; }

        string _GoogleProjectID;
        /// <summary>
        /// Gets or sets the current export folder.
        /// </summary>
        /// <value>The current export folder.</value>
        [ACPropertyCurrent(606, "GoogleProjectID", "en{'GoogleProjectID'}de{'GoogleProjectID'}")]
        public string GoogleProjectID
        {
            get
            {
                return _GoogleProjectID;
            }
            set
            {
                if (_GoogleProjectID != value)
                {
                    this[Const_GoogleProjectID] = value;
                    _GoogleProjectID = value;
                    OnPropertyChanged("GoogleProjectID");
                }
            }
        }

        public bool GoogleAPIAvailable { get; private set; }
        #endregion

        #region c'tors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="acType"></param>
        /// <param name="content"></param>
        /// <param name="parentACObject"></param>
        /// <param name="parameter"></param>
        /// <param name="acIdentifier"></param>
        public BSOTranslation(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {

            GoogleAPIAvailable = StartGoogleApi();
            if (this[Const_CustomerDataPath] != null && Directory.Exists(this[Const_CustomerDataPath].ToString()))
            {
                CurrentExportFolder = this[Const_CustomerDataPath].ToString();
            }
            else
            {
                CurrentExportFolder = Root.Environment.Datapath;
            }

            return base.ACInit(startChildMode);
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }


        private bool StartGoogleApi()
        {
            bool startAPISuccess = false;
            try
            {
                if (this[Const_GoogleProjectID] != null)
                    GoogleProjectID = this[Const_GoogleProjectID].ToString();
                else
                    GoogleProjectID = "api-project-38160810658";
                GoogleTranslationServiceClient = TranslationServiceClient.Create();
                startAPISuccess = true;
            }
            catch (Exception ec)
            {
                Msg msg = new Msg() { MessageLevel = eMsgLevel.Exception, Message = "Error creating TranslationServiceClient! Message: " + ec.Message };
                SendMessage(msg);
            }
            return startAPISuccess;
        }

        #endregion

        #region Properties

        #region Properties -> ProjectManager
        ACProjectManager _ACProjectManager;
        public ACProjectManager ProjectManager
        {
            get
            {
                if (_ACProjectManager != null)
                    return _ACProjectManager;
                _ACProjectManager = new ACProjectManager(Database, Root);
                _ACProjectManager.PropertyChanged += _ACProjectManager_PropertyChanged;
                return _ACProjectManager;
            }
        }

        private void _ACProjectManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ACProjectManager.CurrentProjectItemRootPropName)
                OnPropertyChanged("CurrentProjectItemRoot");
        }

        #endregion

        #region Properties -> Tree

        /// <summary>
        /// Root-Item
        /// </summary>
        /// <value>The current project item root.</value>
        [ACPropertyCurrent(516, "ProjectItemRoot")]
        public ACClassInfoWithItems CurrentProjectItemRoot
        {
            get
            {
                return ProjectManager.CurrentProjectItemRoot;
            }
        }

        ACClassInfoWithItems _CurrentProjectItem = null;
        /// <summary>
        /// Selected Item
        /// </summary>
        /// <value>The current project item.</value>
        [ACPropertyCurrent(517, "ProjectItem")]
        public ACClassInfoWithItems CurrentProjectItem
        {
            get
            {
                return _CurrentProjectItem;
            }
            set
            {
                if (_CurrentProjectItem != value)
                {
                    _CurrentProjectItem = value;
                    if (value != null && value.ValueT != null)
                    {
                        FilterMandatoryClassID = value.ValueT.ACClassID;
                        FilterMandatoryClassACIdentifier = value.ACIdentifier;
                    }
                    else
                        RemoveMandatory();
                    OnPropertyChanged("CurrentProjectItem");
                    if (AutoGenerateByNavigation)
                        SearchBg(false);
                }
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
                        QueryRightsFromDB = true,
                        IsCheckboxVisible = true,
                        CheckedSetter = InfoItemIsCheckedSetter,
                        CheckedGetter = InfoItemIsCheckedGetter,
                        CheckIsEnabledGetter = InfoItemIsCheckEnabledGetter,
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
                        IncludeLibraryClasses = false
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
                        ShowCaptionInTree = this.WithCaption,
                        DisplayGroupedTree = this.ShowGroup,
                        DisplayTreeAsMenu = null
                    };
                return mode;
            }
        }


        /// <summary>
        /// The _ show group
        /// </summary>
        bool _ShowGroup = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show group].
        /// </summary>
        /// <value><c>true</c> if [show group]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(518, "TreeConfig", "en{'Grouped'}de{'Gruppiert'}")]
        public bool ShowGroup
        {
            get
            {
                return _ShowGroup;
            }
            set
            {
                _ShowGroup = value;
                OnPropertyChanged("ShowGroup");
                RefreshProjectTree(true);
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
        [ACPropertyInfo(519, "TreeConfig", "en{'Search Class'}de{'Suche Klasse'}")]
        public string SearchClassText
        {
            get
            {
                return _SearchClassText;
            }
            set
            {
                if (_SearchClassText != value)
                {
                    _SearchClassText = value;
                    OnPropertyChanged("SearchClassText");
                    RefreshProjectTree(true);
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
        [ACPropertyInfo(520, "TreeConfig", "en{'With Caption'}de{'Mit Bezeichnung'}")]
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
                    OnPropertyChanged("WithCaption");
                    RefreshProjectTree();
                }
            }
        }


        #endregion

        #region Properties -> ACProject
        /// <summary>
        /// The _ selected AC project
        /// </summary>
        ACProject _SelectedACProject;
        /// <summary>
        /// Gets or sets the selected AC project.
        /// </summary>
        /// <value>The selected AC project.</value>
        [ACPropertySelected(513, "ACProject")]
        public ACProject SelectedACProject
        {
            get
            {
                return _SelectedACProject;
            }
            set
            {
                _SelectedACProject = value;
                OnPropertyChanged("SelectedACProject");
            }
        }

        /// <summary>
        /// The _ current AC project
        /// </summary>
        ACProject _CurrentACProject;
        /// <summary>
        /// Gets or sets the current AC project.
        /// </summary>
        /// <value>The current AC project.</value>
        [ACPropertyCurrent(514, "ACProject", "en{'Project'}de{'Projekt'}")]
        public ACProject CurrentACProject
        {
            get
            {
                return _CurrentACProject;
            }
            set
            {
                if (value != null)
                {
                    if (_CurrentACProject != value)
                    {
                        ACClassNotHaveTranslation = null;
                        _CurrentACProject = value;

                        if (!BackgroundWorker.IsBusy && SelectedTargetLanguage != null)
                        {
                            BackgroundWorker.RunWorkerAsync(TranslationAutogenerateOption.GetACClassTranslationStatus);
                            ShowDialog(this, DesignNameProgressBar);
                        }
                        else
                            ProjectManager.LoadACProject(CurrentACProject, ProjectTreePresentationMode, ProjectTreeVisibilityFilter, ProjectTreeCheckHandler);
                    }
                }
                else
                {
                    _CurrentACProject = null;
                    ProjectManager.EliminateProjectTree();
                }
                OnPropertyChanged("CurrentACProject");
            }
        }

        /// <summary>
        /// Gets the AC project list.
        /// </summary>
        /// <value>The AC project list.</value>
        [ACPropertyList(515, "ACProject")]
        public IEnumerable<ACProject> ACProjectList
        {
            get
            {
                return Database.ContextIPlus.ACProject.OrderBy(c => c.ACProjectTypeIndex).ThenBy(c => c.ACProjectName).ToList();
            }
        }

        #endregion

        #region Properties -> Filter

        private Guid? _FilterMandatoryClassID;
        /// <summary>
        /// Doc  FilterMandatoryClassID
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "FilterMandatoryClassID", "en{'FilterMandatoryClassID'}de{'FilterMandatoryClassID'}")]
        public Guid? FilterMandatoryClassID
        {
            get
            {
                return _FilterMandatoryClassID;
            }
            set
            {
                if (_FilterMandatoryClassID != value)
                {
                    _FilterMandatoryClassID = value;
                    OnPropertyChanged("FilterMandatoryClassID");
                }
            }
        }


        private string _FilterMandatoryClassACIdentifier;
        /// <summary>
        /// Doc  FilterMandatoryClassACIdentifier
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "FilterMandatoryClassACIdentifier", "en{'Mandatory class'}de{'Verpflichtende Klasse'}")]
        public string FilterMandatoryClassACIdentifier
        {
            get
            {
                return _FilterMandatoryClassACIdentifier;
            }
            set
            {
                if (_FilterMandatoryClassACIdentifier != value)
                {
                    _FilterMandatoryClassACIdentifier = value;
                    OnPropertyChanged("FilterMandatoryClassACIdentifier");
                }
            }
        }


        private string _FilterNotHaveInTranslation;
        /// <summary>
        /// Doc  FilterNotHaveInTranslation
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "FilterNotHaveInTranslation", "en{'Translation not contains'}de{'Übersetzung enthält nicht'}")]
        public string FilterNotHaveInTranslation
        {
            get
            {
                return _FilterNotHaveInTranslation;
            }
            set
            {
                if (_FilterNotHaveInTranslation != value)
                {
                    if (string.IsNullOrEmpty(value))
                        _FilterNotHaveInTranslation = null;
                    else
                        _FilterNotHaveInTranslation = value;
                    OnPropertyChanged("FilterNotHaveInTranslation");
                }
            }
        }



        private bool? _FilterOnlyACClassTables;
        [ACPropertyInfo(405, "FilterOnlyACClassTables", "en{'Only ACClass tables'}de{'Nur ACClass Tabellen'}")]
        public bool? FilterOnlyACClassTables
        {
            get
            {
                return _FilterOnlyACClassTables;
            }
            set
            {
                if (_FilterOnlyACClassTables != value)
                {
                    _FilterOnlyACClassTables = value;
                    OnPropertyChanged("FilterOnlyACClassTables");
                }
            }
        }

        private bool? _FilterOnlyMDTables;
        [ACPropertyInfo(405, "FilterOnlyMDTables", "en{'Only MD Tables'}de{'Nur MD Tabellen'}")]
        public bool? FilterOnlyMDTables
        {
            get
            {
                return _FilterOnlyMDTables;
            }
            set
            {
                if (_FilterOnlyMDTables != value)
                {
                    _FilterOnlyMDTables = value;
                    OnPropertyChanged("FilterOnlyMDTables");
                }
            }
        }


        private string _FilterClassACIdentifier;
        [ACPropertyInfo(407, "FilterClassACIdentifier", "en{'Class ACIdentifier'}de{'Class ACIdentifier'}")]
        public string FilterClassACIdentifier
        {
            get
            {
                if (string.IsNullOrEmpty(_FilterClassACIdentifier))
                    _FilterClassACIdentifier = null;
                return _FilterClassACIdentifier;
            }
            set
            {
                if (_FilterClassACIdentifier != value)
                {
                    _FilterClassACIdentifier = value;
                    OnPropertyChanged("FilterClassACIdentifier");
                }
            }
        }

        private string _FilterACIdentifier;
        [ACPropertyInfo(410, "FilterACIdentifier", "en{'ACIdentifier'}de{'ACIdentifier'}")]
        public string FilterACIdentifier
        {
            get
            {
                if (string.IsNullOrEmpty(_FilterACIdentifier))
                    _FilterACIdentifier = null;
                return _FilterACIdentifier;
            }
            set
            {
                if (_FilterACIdentifier != value)
                {
                    _FilterACIdentifier = value;
                    OnPropertyChanged("FilterACIdentifier");
                }
            }
        }

        private string _FilterTranslation;
        [ACPropertyInfo(411, "FilterTranslation", "en{'Translation'}de{'Translation'}")]
        public string FilterTranslation
        {
            get
            {
                if (string.IsNullOrEmpty(_FilterTranslation))
                    _FilterTranslation = null;
                return _FilterTranslation;
            }
            set
            {
                if (_FilterTranslation != value)
                {
                    _FilterTranslation = value;
                    OnPropertyChanged("FilterTranslation");
                }
            }
        }

        #endregion

        #region Replace

        private bool _IsReplaceTranslationExact;
        /// <summary>
        /// Selected property for 
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "IsReplaceTranslationExact", "en{'Match complete text'}de{'Vollständigen Text abgleichen'}")]
        public bool IsReplaceTranslationExact
        {
            get
            {
                return _IsReplaceTranslationExact;
            }
            set
            {
                if (_IsReplaceTranslationExact != value)
                {
                    _IsReplaceTranslationExact = value;
                    OnPropertyChanged("IsReplaceTranslationExact");
                }
            }
        }

        #region IsReplaceSearchInSourceLanguage
        private bool _IsReplaceSearchInSourceLanguage;
        /// <summary>
        /// Selected property for 
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "IsReplaceSearchInSourceLanguage", "en{'Search text in source language'}de{'Text in Quellsprache suchen'}")]
        public bool IsReplaceSearchInSourceLanguage
        {
            get
            {
                return _IsReplaceSearchInSourceLanguage;
            }
            set
            {
                if (_IsReplaceSearchInSourceLanguage != value)
                {
                    _IsReplaceSearchInSourceLanguage = value;
                    OnPropertyChanged("IsReplaceSearchInSourceLanguage");
                }
            }
        }

        #endregion

        private string _ReplaceACIdentifier;
        /// <summary>
        /// Selected property for 
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "ReplaceACIdentifier", "en{'With ACIdentifier override'}de{'Mit ACIdentifier überschreibe'}")]
        public string ReplaceACIdentifier
        {
            get
            {
                return _ReplaceACIdentifier;
            }
            set
            {
                if (_ReplaceACIdentifier != value)
                {
                    _ReplaceACIdentifier = value;
                    if (!string.IsNullOrEmpty(_ReplaceACIdentifier))
                        FromText = null;
                    OnPropertyChanged("ReplaceACIdentifier");
                }
            }
        }

        private string _FromText;
        /// <summary>
        /// Selected property for 
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "FromText", "en{'From text'}de{'Aus Text'}")]
        public string FromText
        {
            get
            {
                return _FromText;
            }
            set
            {
                if (_FromText != value)
                {
                    _FromText = value;
                    _ReplaceACIdentifier = value;
                    if (!string.IsNullOrEmpty(_FromText))
                        ReplaceACIdentifier = null;
                    OnPropertyChanged("FromText");
                }
            }
        }

        private string _ToText;
        /// <summary>
        /// Selected property for 
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "ToText", "en{'with text replace'}de{'mit Text ersetzen'}")]
        public string ToText
        {
            get
            {
                return _ToText;
            }
            set
            {
                if (_ToText != value)
                {
                    _ToText = value;
                    OnPropertyChanged("ToText");
                }
            }
        }

        #endregion

        #region Properties -> Autogenerate

        private bool _AutoGenerateByNavigation = true;
        /// <summary>
        /// AUtogenerate for all in the list
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "AutoGenerateByNavigation", "en{'Activate translation mode'}de{'Übersetzungsmodus aktivieren'}")]
        public bool AutoGenerateByNavigation
        {
            get
            {
                return _AutoGenerateByNavigation;
            }
            set
            {
                if (_AutoGenerateByNavigation != value)
                {
                    _AutoGenerateByNavigation = value;
                    OnPropertyChanged("AutoGenerateByNavigation");
                }
            }
        }

        private string _AutoGeneratePrefix = "#";
        /// <summary>
        /// Doc  AutoGeneratePrefix
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "AutoGeneratePrefix", "en{'Prefix'}de{'Präfix'}")]
        public string AutoGeneratePrefix
        {
            get
            {
                return _AutoGeneratePrefix;
            }
            set
            {
                if (_AutoGeneratePrefix != value)
                {
                    _AutoGeneratePrefix = value;
                    OnPropertyChanged("AutoGeneratePrefix");
                }
            }
        }

        #region  Properties -> Autogenerate -> AutoGenerateOption

        public TranslationAutogenerateOption? SelectedAutoGenerateOptionEnumVal
        {
            get
            {
                if (SelectedAutoGenerateOption == null) return null;
                return (TranslationAutogenerateOption)((short)SelectedAutoGenerateOption.Value);
            }
        }


        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        public ACValueItemList GetAutoGenerateOptionList()
        {
            ACValueItemList aCValueItems = new ACValueItemList("AutoGenerateOptionList");
            aCValueItems.AddEntry((short)TranslationAutogenerateOption.GenerateEmptyTranslation, "en{'Generate empty translation'}de{'Leere Übersetzung generieren'}");
            aCValueItems.AddEntry((short)TranslationAutogenerateOption.GeneratePairFromSourceLanguage, "en{'Copy from english'}de{'Kopie aus dem Englischen'}");
            aCValueItems.AddEntry((short)TranslationAutogenerateOption.GeneratePairUsingGoogleApi, "en{'Use google translator'}de{'Benutze den Google Übersetzer'}");
            return aCValueItems;
        }


        private ACValueItemList _AutoGenerateOptionList = null;
        [ACPropertyList(618, "AutoGenerateOption", "en{'Period'}de{'Periode'}")]
        public ACValueItemList AutoGenerateOptionList
        {
            get
            {
                if (_AutoGenerateOptionList == null)
                    _AutoGenerateOptionList = GetAutoGenerateOptionList();
                return _AutoGenerateOptionList;
            }
        }

        private ACValueItem _SelectedAutoGenerateOption;
        /// <summary>
        /// Selected property for VBTranslationView
        /// </summary>
        /// <value>The selected Translations</value>
        [ACPropertySelected(401, "AutoGenerateOption", "en{'Select task'}de{'Aufgabe auswählen'}")]
        public ACValueItem SelectedAutoGenerateOption
        {
            get
            {
                return _SelectedAutoGenerateOption;
            }
            set
            {
                if (_SelectedAutoGenerateOption != value)
                {
                    _SelectedAutoGenerateOption = value;
                    OnPropertyChanged("SelectedAutoGenerateOption");
                }
            }
        }

        #endregion

        #endregion

        #region Properties -> Messages

        /// <summary>
        /// The _ current MSG
        /// </summary>
        Msg _CurrentMsg;
        /// <summary>
        /// Gets or sets the current MSG.
        /// </summary>
        /// <value>The current MSG.</value>
        [ACPropertyCurrent(9999, "Message", "en{'Message'}de{'Meldung'}")]
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
        [ACPropertyList(9999, "Message", "en{'Messagelist'}de{'Meldungsliste'}")]
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

        #region Properties -> TranslationView

        private VBTranslationView _SelectedTranslationView;
        /// <summary>
        /// Selected property for VBTranslationView
        /// </summary>
        /// <value>The selected Translations</value>
        [ACPropertySelected(401, "TranslationView", "en{'Translation preview'}de{'Übersetzungsvorschau'}")]
        public VBTranslationView SelectedTranslationView
        {
            get
            {
                return _SelectedTranslationView;
            }
            set
            {
                if (_SelectedTranslationView != value)
                {
                    _SelectedTranslationView = value;
                    OnPropertyChanged("SelectedTranslationView");
                    SetTranslationPairList();
                }
            }
        }

        private List<VBTranslationView> _TranslationViewList;
        /// <summary>
        /// List property for VBTranslationView
        /// </summary>
        /// <value>The Translations list</value>
        [ACPropertyList(402, "TranslationView")]
        public List<VBTranslationView> TranslationViewList
        {
            get
            {
                return _TranslationViewList;
            }
        }

        #endregion

        #region Properties -> TranslationView Selected index

        public int SelectedTranslationViewIndex
        {
            get
            {
                if (TranslationViewCount == 0 || SelectedTranslationView == null) return -1;
                return TranslationViewList.IndexOf(SelectedTranslationView);
            }
            set
            {
                if (TranslationViewCount > 0 && value > -1 && value < TranslationViewCount)
                {
                    SelectedTranslationView = TranslationViewList[value];
                }
            }
        }

        public int TranslationViewCount
        {
            get
            {
                if (TranslationViewList == null || !TranslationViewList.Any()) return 0;
                return TranslationViewList.Count();
            }
        }

        #endregion

        #region Properties -> SourceLanguage

        private VBLanguage _SelectedSourceLanguage;
        /// <summary>
        /// Selected property for VBLanguage
        /// </summary>
        /// <value>The selected VBLanguage</value>
        [ACPropertySelected(9999, "SourceLanguage", "en{'Source language'}de{'Quellsprache'}")]
        public VBLanguage SelectedSourceLanguage
        {
            get
            {
                return _SelectedSourceLanguage;
            }
            set
            {
                if (_SelectedSourceLanguage != value)
                {
                    _SelectedSourceLanguage = value;
                    OnPropertyChanged("SelectedSourceLanguage");
                }
            }
        }

        private List<VBLanguage> _SourceLanguageList;
        /// <summary>
        /// List property for VBLanguage
        /// </summary>
        /// <value>The VBLanguage list</value>
        [ACPropertyList(9999, "SourceLanguage")]
        public List<VBLanguage> SourceLanguageList
        {
            get
            {
                if (_SourceLanguageList == null)
                    _SourceLanguageList = LoadVBLanguageList();
                return _SourceLanguageList;
            }
        }

        private List<VBLanguage> LoadVBLanguageList()
        {
            List<VBLanguage> vBLanguages = new List<VBLanguage>();
            using (Database db = new core.datamodel.Database())
            {
                vBLanguages = db.VBLanguage.Where(c => c.IsTranslation).OrderBy(c => c.SortIndex).ToList();
            }
            return vBLanguages;
        }

        #endregion

        #region TargetLanguage

        private VBLanguage _SelectedTargetLanguage;
        /// <summary>
        /// Selected property for VBLanguage
        /// </summary>
        /// <value>The selected TargetLanguage</value>
        [ACPropertySelected(9999, "TargetLanguage", "en{'Target language'}de{'Zielsprache'}")]
        public VBLanguage SelectedTargetLanguage
        {
            get
            {
                return _SelectedTargetLanguage;
            }
            set
            {
                _SelectedTargetLanguage = value;
                OnPropertyChanged("SelectedTargetLanguage");
            }
        }


        private List<VBLanguage> _TargetLanguageList;
        /// <summary>
        /// List property for VBLanguage
        /// </summary>
        /// <value>The TargetLanguage list</value>
        [ACPropertyList(9999, "TargetLanguage")]
        public List<VBLanguage> TargetLanguageList
        {
            get
            {
                if (_TargetLanguageList == null)
                    _TargetLanguageList = LoadVBLanguageList();
                return _TargetLanguageList;
            }
        }
        #endregion

        #region Properties -> TranslationPair
        private TranslationPair _SelectedTranslationPair;
        /// <summary>
        /// Selected property for TranslationPair
        /// </summary>
        /// <value>The selected EditTranslation</value>
        [ACPropertySelected(403, "TranslationPair", "en{'TODO: EditTranslation'}de{'TODO: EditTranslation'}")]
        public TranslationPair SelectedTranslationPair
        {
            get
            {
                return _SelectedTranslationPair;
            }
            set
            {
                if (_SelectedTranslationPair != value)
                {
                    _SelectedTranslationPair = value;
                    OnPropertyChanged("SelectedTranslationPair");
                }
            }
        }


        private List<TranslationPair> _TranslationPairList;
        /// <summary>
        /// List property for TranslationPair
        /// </summary>
        /// <value>The EditTranslation list</value>
        [ACPropertyList(404, "TranslationPair")]
        public List<TranslationPair> TranslationPairList
        {
            get
            {
                return _TranslationPairList;
            }
        }

        private List<TranslationPair> LoadTranslationPairList()
        {
            if (SelectedTranslationView == null) return null;
            if (AutoGenerateByNavigation
                && SelectedTranslationView.EditTranslationList != null
                && SelectedTargetLanguage != null
                && SelectedSourceLanguage != null
                && SelectedAutoGenerateOption != null
                && !SelectedTranslationView.EditTranslationList.Any(c => c.LangCode == SelectedTargetLanguage.VBLanguageCode))
            {
                TranslationPair translationPair = null;
                TranslationPair sourcePair = SelectedTranslationView.EditTranslationList.FirstOrDefault(c => c.LangCode == SelectedSourceLanguage.VBLanguageCode);
                switch (SelectedAutoGenerateOptionEnumVal.Value)
                {
                    case TranslationAutogenerateOption.GenerateEmptyTranslation:
                        translationPair = new TranslationPair() { LangCode = SelectedTargetLanguage.VBLanguageCode, Translation = AutoGeneratePrefix };
                        break;
                    case TranslationAutogenerateOption.GeneratePairFromSourceLanguage:
                        if (sourcePair != null)
                            translationPair = new TranslationPair() { LangCode = SelectedTargetLanguage.VBLanguageCode, Translation = AutoGeneratePrefix + sourcePair.Translation };
                        break;
                    case TranslationAutogenerateOption.GeneratePairUsingGoogleApi:
                        if (sourcePair != null)
                        {
                            var translations = GetTranslationPairFromGoogleApi(SelectedSourceLanguage.VBLanguageCode, SelectedTargetLanguage.VBLanguageCode, new string[] { sourcePair.Translation });
                            if (translations.Any())
                                translationPair = translations.FirstOrDefault();
                        }
                        break;
                }
                if (translationPair != null)
                    SelectedTranslationView.EditTranslationList.Add(translationPair);
            }
            return SelectedTranslationView.EditTranslationList;
        }

        private void SetTranslationPairList()
        {
            _TranslationPairList = LoadTranslationPairList();
            OnPropertyChanged("TranslationPairList");
            if (_TranslationPairList != null)
            {
                string langCode = "en";
                if (SelectedTargetLanguage != null)
                    langCode = SelectedTargetLanguage.VBLanguageCode;
                SelectedTranslationPair = _TranslationPairList.FirstOrDefault(c => c.LangCode == langCode);
                if (SelectedTranslationPair == null)
                    SelectedTranslationPair = _TranslationPairList.FirstOrDefault();
            }
            else
                SelectedTranslationPair = null;
        }


        #endregion

        #region Properties -> TranslationPair SelectedIndex

        public int SelectedTranslationPairIndex
        {
            get
            {
                if (TranslationPairCount == 0 || SelectedTranslationPair == null) return -1;
                return TranslationPairList.IndexOf(SelectedTranslationPair);
            }
            set
            {
                if (TranslationPairCount > 0 && value > -1 && value < TranslationPairCount)
                {
                    SelectedTranslationPair = TranslationPairList[value];
                }
            }
        }

        public int TranslationPairCount
        {
            get
            {
                if (TranslationPairList == null || !TranslationPairList.Any()) return 0;
                return TranslationPairList.Count();
            }
        }

        #endregion

        #region Properties -> Import

        /// <summary>
        /// The _ current import folder
        /// </summary>
        string _ImportSourcePath;
        /// <summary>
        /// Gets or sets the current import folder.
        /// </summary>
        /// <value>The current import folder.</value>
        [ACPropertyInfo(403, "ImportSourcePath", "en{'Import File Path'}de{'Pfad der Importdatei'}")]
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
                    OnPropertyChanged("ImportSourcePath");
                }
            }
        }

        #endregion

        #region Properties -> Export


        private bool _ExportOnlyForSelectedTargetLanguage;
        /// <summary>
        /// Doc  ExportOnlyForSelectedTargetLanguage
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "ExportOnlyForSelectedTargetLanguage", "en{'Export only for selected target language'}de{'Exportiere Übersetzungen nur für Zielsprache'}")]
        public bool ExportOnlyForSelectedTargetLanguage
        {
            get
            {
                return _ExportOnlyForSelectedTargetLanguage;
            }
            set
            {
                if (_ExportOnlyForSelectedTargetLanguage != value)
                {
                    _ExportOnlyForSelectedTargetLanguage = value;
                    OnPropertyChanged("ExportOnlyForSelectedTargetLanguage");
                }
            }
        }


        /// <summary>
        /// The _ current export folder
        /// </summary>
        string _CurrentExportFolder;
        /// <summary>
        /// Gets or sets the current export folder.
        /// </summary>
        /// <value>The current export folder.</value>
        [ACPropertyCurrent(406, "ExportFolder", "en{'Export folder'}de{'Exportordner'}")]
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
                    if (value != null)
                    {
                        if (Directory.Exists(value))
                        {
                            if (string.IsNullOrEmpty(CurrentExportFileName))
                                CurrentExportFileName = string.Format(Const_ExportFileTemplate, DateTime.Now.ToString("yyyy-MM-dd_HH-mm"));
                            this[Const_CustomerDataPath] = value;
                            _CurrentExportFolder = value;
                        }
                    }
                    OnPropertyChanged("CurrentExportFolder");
                }
            }
        }

        private string _CurrentExportFileName;
        /// <summary>
        /// Doc  CurrentExportFileName
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "CurrentExportFileName", "en{'Export file name'}de{'Dateiname für den Export'}")]
        public string CurrentExportFileName
        {
            get
            {
                return _CurrentExportFileName;
            }
            set
            {
                if (_CurrentExportFileName != value)
                {
                    _CurrentExportFileName = value;
                    OnPropertyChanged("CurrentExportFileName");
                }
            }
        }

        private bool _IsJsonPrettyPrint;
        [ACPropertyInfo(610, "IsJsonPrettyPrint", "en{'Pretty print JSON'}de{'Hübscher Druck JSON'}")]
        public bool IsJsonPrettyPrint
        {
            get
            {
                return _IsJsonPrettyPrint;
            }
            set
            {
                if (_IsJsonPrettyPrint != value)
                {
                    _IsJsonPrettyPrint = value;
                    OnPropertyChanged("_IsJsonPrettyPrint");
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Methods -> Common

        [ACMethodCommand("Translation", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            BackgroundWorker.RunWorkerAsync(TranslationAutogenerateOption.SaveTranslation);
            ShowDialog(this, DesignNameProgressBar);
        }

        [ACMethodInfo("Translation", "en{'Search'}de{'Suche'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void Search()
        {
            if (!IsEnabledSearch())
                return;
            SearchBg(true);
        }

        public void SearchBg(bool withProgressBar)
        {
            if (!IsEnabledSearch())
                return;
            BackgroundWorker.RunWorkerAsync(TranslationAutogenerateOption.FetchTranslation);
            if (withProgressBar)
                ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledSearch()
        {

            return
                (FilterMandatoryClassID != null || (!(FilterOnlyACClassTables ?? true) && (FilterOnlyMDTables ?? false)))
                || !string.IsNullOrEmpty(FilterClassACIdentifier)
                || !string.IsNullOrEmpty(FilterACIdentifier)
                || !string.IsNullOrEmpty(FilterTranslation)
                || !string.IsNullOrEmpty(FilterNotHaveInTranslation);
        }

        [ACMethodInfo("RemoveMandatory", "en{'Remove'}de{'Entfernen'}", 500)]
        public void RemoveMandatory()
        {
            if (!IsEnabledRemoveMandatory())
                return;

            FilterMandatoryClassID = null;
            FilterMandatoryClassACIdentifier = null;
        }

        public bool IsEnabledRemoveMandatory()
        {
            return FilterMandatoryClassID != null;
        }


        #endregion

        #region Methods -> Navigate

        #region Methods -> Navigate -> Navigate TranlationViewList

        [ACMethodCommand("Translation", "en{'Backward (Ctrl + <-)'}de{'Rückwärts (Ctrl + <-)'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MoveBackward()
        {
            if (!IsEnabledMoveBackward())
                return;
            SelectedTranslationViewIndex--;
        }

        public bool IsEnabledMoveBackward() => TranslationViewCount > 0 && SelectedTranslationViewIndex > 0;


        [ACMethodCommand("Translation", "en{'Forward (Ctrl + ->)'}de{'Nach vorne (Ctrl + ->)'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MoveForward()
        {
            if (!IsEnabledMoveForward())
                return;
            SelectedTranslationViewIndex++;
        }

        public bool IsEnabledMoveForward() => TranslationViewCount > 0 && (SelectedTranslationViewIndex + 1) < TranslationViewCount;




        #endregion

        #region Methods -> Navigate -> Navigate TranslationPairList

        [ACMethodCommand("TranslationPair", "en{'Up (Ctrl + Up)'}de{'Oben (Ctrl + Oben)'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MoveUp()
        {
            if (!IsEnabledMoveUp())
                return;
            SelectedTranslationPairIndex--;
        }

        public bool IsEnabledMoveUp() => TranslationPairCount > 0 && SelectedTranslationPairIndex > 0;


        [ACMethodCommand("TranslationPair", "en{'Down (Ctrl + Down)'}de{'Unten (Ctrl + Unten)'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MoveDown()
        {
            if (!IsEnabledMoveDown())
                return;
            SelectedTranslationPairIndex++;
        }

        public bool IsEnabledMoveDown() => TranslationPairCount > 0 && (SelectedTranslationPairIndex + 1) < TranslationPairCount;


        #endregion

        [ACMethodInfo("", "en{'Key event'}de{'Tastatur Ereignis'}", 9999, false)]
        public void OnKeyEvent(KeyEventArgs e)
        {
            bool ctrlkeyPresent = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            Console.WriteLine(e.Key);
            if (ctrlkeyPresent && e.Key == Key.Left)
                MoveBackward();
            if (ctrlkeyPresent && e.Key == Key.Right)
                MoveForward();
            if (ctrlkeyPresent && e.Key == Key.Up)
                MoveUp();
            if (ctrlkeyPresent && e.Key == Key.Down)
                MoveDown();

        }


        #endregion

        #region Methods -> GenerateTranslation

        [ACMethodInfo("GenerateTranslation", "en{'Translate displayed list'}de{'Übersetze angezeigte Liste'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void GenerateTranslation()
        {
            if (!IsEnabledGenerateTranslation())
                return;
            TranslationAutogenerateOption selectedAutoGenerateOption = SelectedAutoGenerateOptionEnumVal.Value;
            BackgroundWorker.RunWorkerAsync(selectedAutoGenerateOption);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledGenerateTranslation()
        {
            return
                SelectedSourceLanguage != null
                && SelectedAutoGenerateOption != null
                && IsEnabledRemoveGeneratedTranslation()
                && SelectedTargetLanguage.VBLanguageCode != SelectedSourceLanguage.VBLanguageCode
                && IsEnabledGoogleApi();
        }

        public bool IsEnabledGoogleApi()
        {
            TranslationAutogenerateOption[] googleApis = new TranslationAutogenerateOption[] { TranslationAutogenerateOption.GeneratePairUsingGoogleApi, TranslationAutogenerateOption.GeneratePairUsingGoogleApiAll };
            return !googleApis.Contains(SelectedAutoGenerateOptionEnumVal.Value) || GoogleAPIAvailable;
        }

        [ACMethodInfo("RemoveGeneratedTranslation", "en{'Remove translations from selection'}de{'Entferne Übersetzungen in Auswahl'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void RemoveGeneratedTranslation()
        {
            if (!IsEnabledRemoveGeneratedTranslation())
                return;
            TranslationAutogenerateOption removeOption = TranslationAutogenerateOption.RemoveAutoGenerated;
            BackgroundWorker.RunWorkerAsync(removeOption);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledRemoveGeneratedTranslation()
        {
            return TranslationViewList != null
                && TranslationViewList.Any()
                && SelectedTargetLanguage != null;
        }

        #endregion

        #region Methods -> GenerateTranslationAll

        [ACMethodInfo("GenerateTranslationAll", "en{'Translate entire project'}de{'Übersetze ganzes Projekt'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void GenerateTranslationAll()
        {
            if (!IsEnabledGenerateTranslationAll())
                return;
            TranslationAutogenerateOption option = TranslationAutogenerateOption.GenerateEmptyTranslationAll;
            TranslationAutogenerateOption selectedAutoGenerateOption = SelectedAutoGenerateOptionEnumVal.Value;
            switch (selectedAutoGenerateOption)
            {
                case TranslationAutogenerateOption.GenerateEmptyTranslation:
                    option = TranslationAutogenerateOption.GenerateEmptyTranslationAll;
                    break;
                case TranslationAutogenerateOption.GeneratePairFromSourceLanguage:
                    option = TranslationAutogenerateOption.GeneratePairFromSourceLanguageAll;
                    break;
                case TranslationAutogenerateOption.GeneratePairUsingGoogleApi:
                    option = TranslationAutogenerateOption.GeneratePairUsingGoogleApiAll;
                    break;
            }

            BackgroundWorker.RunWorkerAsync(option);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledGenerateTranslationAll()
        {
            return
                SelectedTargetLanguage != null
                && SelectedAutoGenerateOption != null
                && IsEnabledGoogleApi();
        }

        [ACMethodInfo("RemoveGeneratedTranslationAll", "en{'Remove translations from the entire project'}de{'Entferne Übersetzungen aus gesamten Projekt'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void RemoveGeneratedTranslationAll()
        {
            if (!IsEnabledRemoveGeneratedTranslationAll())
                return;
            TranslationAutogenerateOption removeOption = TranslationAutogenerateOption.RemoveAutoGeneratedAll;
            BackgroundWorker.RunWorkerAsync(removeOption);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledRemoveGeneratedTranslationAll()
        {
            return SelectedTargetLanguage != null;
        }

        #endregion

        #region Methods -> Replace

        /// <summary>
        /// Method Replace
        /// </summary>
        [ACMethodInfo("Replace", "en{'Replace selection'}de{'Auswahl ersetzen'}", 9999, false, false, true)]
        public void Replace()
        {
            if (!IsEnabledReplace())
                return;
            BackgroundWorker.RunWorkerAsync(TranslationAutogenerateOption.Replace);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledReplace()
        {
            return
                IsEnabledReplaceAll()
                && TranslationViewList != null
                && TranslationViewList.Any();
        }


        /// <summary>
        /// Method ReplaceAll
        /// </summary>
        [ACMethodInfo("ReplaceAll", "en{'Replace in entire project'}de{'In gesamten Projekt ersetzen'}", 9999, false, false, true)]
        public void ReplaceAll()
        {
            if (!IsEnabledReplaceAll())
                return;
            BackgroundWorker.RunWorkerAsync(TranslationAutogenerateOption.ReplaceAll);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledReplaceAll()
        {
            return
                (!string.IsNullOrEmpty(FromText) || !string.IsNullOrEmpty(ReplaceACIdentifier))
                && SelectedSourceLanguage != null
                && SelectedTargetLanguage != null;
        }


        #endregion

        #region Methods -> Export

        /// <summary>
        /// Exports the folder.
        /// </summary>
        [ACMethodInfo("Export", "en{'...'}de{'...'}", 402, false, false, true)]
        public void ExportFolder()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                if (Directory.Exists(CurrentExportFolder))
                    dialog.InitialDirectory = CurrentExportFolder;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    if (Directory.Exists(dialog.FileName))
                    {
                        CurrentExportFolder = dialog.FileName;
                    }
                }
            }
        }

        [ACMethodInfo("ExportTranslations", "en{'Export selected only'}de{'Exportiere nur Auswahl'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void ExportTranslations()
        {
            if (!IsEnabledExportTranslations())
                return;
            TranslationAutogenerateOption selectedAutoGenerateOption = TranslationAutogenerateOption.Export;
            BackgroundWorker.RunWorkerAsync(selectedAutoGenerateOption);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledExportTranslations()
        {
            return
                TranslationViewList != null
                && TranslationViewList.Any()
                && IsExportFileAvailable()
                && (!ExportOnlyForSelectedTargetLanguage || SelectedTargetLanguage != null);
        }

        [ACMethodInfo("ExportTranslations", "en{'Export entire project'}de{'Exportiere gesamtes Projekt'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void ExportTranslationsAll()
        {
            if (!IsEnabledExportTranslationsAll())
                return;
            TranslationAutogenerateOption selectedAutoGenerateOption = TranslationAutogenerateOption.ExportAll;
            BackgroundWorker.RunWorkerAsync(selectedAutoGenerateOption);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledExportTranslationsAll()
        {
            return IsExportFileAvailable()
                && (!ExportOnlyForSelectedTargetLanguage || SelectedTargetLanguage != null);
        }

        private bool IsExportFileAvailable()
        {
            return
                !string.IsNullOrEmpty(CurrentExportFolder)
                && Directory.Exists(CurrentExportFolder)
                && !string.IsNullOrEmpty(CurrentExportFileName)
                && CurrentExportFileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        [ACMethodInfo("RefreshExportFileTime", "en{'New Filename'}de{'Neuer Dateiname'}", 611)]
        public void RefreshExportFileTime()
        {
            if (!IsEnabledRefreshExportFileTime())
                return;
            CurrentExportFileName = string.Format(Const_ExportFileTemplate, DateTime.Now.ToString("yyyy-MM-dd_HH-mm"));
        }

        public bool IsEnabledRefreshExportFileTime()
        {
            return !string.IsNullOrEmpty(CurrentExportFolder) && Directory.Exists(CurrentExportFolder);
        }

        #endregion

        #region Methods -> Import

        /// <summary>
        /// Imports the folder.
        /// </summary>
        [ACMethodInfo("Import", "en{'...'}de{'...'}", 406, false, false, true)]
        public void ImportSource()
        {
            if (!IsEnabledImportSource())
                return;
            using (var dialog = new CommonOpenFileDialog())
            {
                if (Directory.Exists(CurrentExportFolder))
                    dialog.InitialDirectory = CurrentExportFolder;
                dialog.IsFolderPicker = false;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    if (File.Exists(dialog.FileName))
                    {
                        ImportSourcePath = dialog.FileName;
                    }
                }
            }
        }

        public bool IsEnabledImportSource()
        {
            return !BackgroundWorker.IsBusy;
        }

        [ACMethodInfo("ImportTranslations", "en{'Import translations'}de{'Übersetzungen importieren'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void ImportTranslations()
        {
            if (!IsEnabledImportTranslations())
                return;
            TranslationAutogenerateOption selectedAutoGenerateOption = TranslationAutogenerateOption.Import;
            BackgroundWorker.RunWorkerAsync(selectedAutoGenerateOption);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledImportTranslations()
        {
            return
                !string.IsNullOrEmpty(ImportSourcePath)
                && File.Exists(ImportSourcePath);
        }

        #endregion

        #region Methods -> Messages


        public void ClearMessages()
        {
            MsgList.Clear();
        }

        public void SendMessage(Msg msg)
        {
            MsgList.Add(msg);
            OnPropertyChanged("MsgList");
        }

        #endregion

        #region Methods -> TranslationPair

        [ACMethodInfo("TranslationPair", "en{'Add'}de{'Neu'}", 999)]
        public void AddTranslationPair()
        {
            TranslationPair translationPair = new TranslationPair();
            translationPair.LangCode = SelectedTargetLanguage.VBLanguageCode;
            TranslationPairList.Add(translationPair);
            OnPropertyChanged("TranslationPairList");
            SelectedTranslationPair = translationPair;
        }

        public bool IsEnabledAddTranslationPair()
        {
            return
                SelectedTargetLanguage != null
                && SelectedTranslationView != null
                && !TranslationPairList.Any(x => x.LangCode == SelectedTargetLanguage.VBLanguageCode);
        }

        [ACMethodInfo("TranslationPair", "en{'Delete'}de{'Löschen'}", 999)]
        public void RemoveTranslationPair()
        {
            TranslationPairList.Remove(SelectedTranslationPair);
            SelectedTranslationPair = TranslationPairList.FirstOrDefault();
        }

        public bool IsEnabledRemoveTranslationPair()
        {
            return SelectedTranslationPair != null;
        }

        #endregion

        #region Methods -> ProjectTree

        public List<Guid> ACClassNotHaveTranslation { get; private set; }
        private void RefreshProjectTree(bool forceRebuildTree = false)
        {
            if (ProjectManager != null)
                ProjectManager.RefreshProjectTree(ProjectTreePresentationMode, ProjectTreeVisibilityFilter, ProjectTreeCheckHandler, forceRebuildTree);
        }

        private void InfoItemIsCheckedSetter(ACClassInfoWithItems infoItem, bool isChecked)
        {

        }


        private bool InfoItemIsCheckedGetter(ACClassInfoWithItems riInfoClass)
        {
            if (ACClassNotHaveTranslation == null || !ACClassNotHaveTranslation.Any() || riInfoClass.ValueT == null)
                return false;
            return !ACClassNotHaveTranslation.Contains(riInfoClass.ValueT.ACClassID);
        }

        private bool InfoItemIsCheckEnabledGetter(ACClassInfoWithItems riInfoClass)
        {
            return false;
        }
        #endregion

        #region Methods ->  BackgroundWorker

        #region Methods ->  BackgroundWorker -> Methods
        /// <summary>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            worker.ProgressInfo.OnlyTotalProgress = true;
            worker.ProgressInfo.TotalProgress.StartTime = DateTime.Now;
            TranslationAutogenerateOption command = (TranslationAutogenerateOption)e.Argument;
            switch (command)
            {
                case TranslationAutogenerateOption.FetchTranslation:
                    e.Result = DoFetchTranslation(worker, e, 0, 100);
                    break;
                case TranslationAutogenerateOption.SaveTranslation:
                    e.Result = DoSaveTranslation(worker, e, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.GenerateEmptyTranslation:
                    e.Result = DoGenerateEmptyTranslation(worker, e, SelectedTargetLanguage.VBLanguageCode, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.GeneratePairFromSourceLanguage:
                    e.Result = GeneratePairFromSourceLanguage(worker, e, SelectedSourceLanguage.VBLanguageCode, SelectedTargetLanguage.VBLanguageCode, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.GeneratePairUsingGoogleApi:
                    e.Result = DoGeneratePairUsingGoogleApi(worker, e, SelectedSourceLanguage.VBLanguageCode, SelectedTargetLanguage.VBLanguageCode, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.RemoveAutoGenerated:
                    e.Result = DoRemoveAutoGenerated(worker, e, SelectedTargetLanguage.VBLanguageCode, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.GenerateEmptyTranslationAll:
                    e.Result = DoGenerateEmptyTranslationAll(worker, e, SelectedTargetLanguage.VBLanguageCode);
                    break;
                case TranslationAutogenerateOption.GeneratePairFromSourceLanguageAll:
                    e.Result = GeneratePairFromSourceLanguageAll(worker, e, SelectedSourceLanguage.VBLanguageCode, SelectedTargetLanguage.VBLanguageCode);
                    break;
                case TranslationAutogenerateOption.GeneratePairUsingGoogleApiAll:
                    e.Result = DoGeneratePairUsingGoogleApiAll(worker, e, SelectedSourceLanguage.VBLanguageCode, SelectedTargetLanguage.VBLanguageCode);
                    break;
                case TranslationAutogenerateOption.RemoveAutoGeneratedAll:
                    e.Result = DoRemoveAutoGeneratedAll(worker, e, SelectedTargetLanguage.VBLanguageCode);
                    break;
                case TranslationAutogenerateOption.Export:
                    DoExport(worker, e, CurrentExportFolder, CurrentExportFileName, ExportOnlyForSelectedTargetLanguage, SelectedTargetLanguage?.VBLanguageCode, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.ExportAll:
                    DoExportAll(worker, e, CurrentExportFolder, CurrentExportFileName, ExportOnlyForSelectedTargetLanguage, SelectedTargetLanguage?.VBLanguageCode, 0, 100);
                    break;
                case TranslationAutogenerateOption.Import:
                    DoImport(worker, e, ImportSourcePath, 0, 100);
                    break;
                case TranslationAutogenerateOption.Replace:
                    e.Result = DoRename(worker, e, SelectedSourceLanguage.VBLanguageCode, SelectedTargetLanguage.VBLanguageCode,
                        FromText, ToText, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.ReplaceAll:
                    DoRenameAll(worker, e, SelectedSourceLanguage.VBLanguageCode, SelectedTargetLanguage.VBLanguageCode, FromText, ToText);
                    break;
                case TranslationAutogenerateOption.GetACClassTranslationStatus:
                    e.Result = DoGetACClassTranslationStatus(worker, e, SelectedTargetLanguage.VBLanguageCode, CurrentACProject.ACProjectID);
                    break;
            }
        }

        public override void BgWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            base.BgWorkerCompleted(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            worker.ProgressInfo.TotalProgress.EndTime = DateTime.Now;
            CloseWindow(this, DesignNameProgressBar);
            ClearMessages();
            TranslationAutogenerateOption command = (TranslationAutogenerateOption)worker.EventArgs.Argument;

            if (e.Cancelled)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Info, Message = string.Format(@"Operation {0} canceled by user!", command) });
            }
            if (e.Error != null)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format(@"Error by doing {0}! Message:{1}", command, e.Error.Message) });
            }
            else
            {
                if (command == TranslationAutogenerateOption.GetACClassTranslationStatus)
                {
                    ACClassNotHaveTranslation = e.Result as List<Guid>;
                    ProjectManager.LoadACProject(CurrentACProject, ProjectTreePresentationMode, ProjectTreeVisibilityFilter, ProjectTreeCheckHandler);

                }
                else if (command == TranslationAutogenerateOption.Replace)
                {
                    FromText = null;
                    ToText = null;
                    ReplaceACIdentifier = null;
                    SetListFromBGWorker(e);
                }
                else
                {
                    SetListFromBGWorker(e);
                }
            }
        }

        private void SetListFromBGWorker(RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<VBTranslationView> list = e.Result as List<VBTranslationView>;
                _TranslationViewList = list;
                OnPropertyChanged("TranslationViewList");
                _SelectedTranslationView = null;
                if (_TranslationViewList != null)
                    SelectedTranslationView = _TranslationViewList.FirstOrDefault();
                else
                    SelectedTranslationView = null;
            }
        }

        #endregion

        #region Methods -> BackgroundWorker -> DoWork -> Select & Save
        private List<VBTranslationView> DoFetchTranslation(ACBackgroundWorker worker, DoWorkEventArgs e, int rangeFrom, int rangeTo)
        {
            int halfRange = (rangeFrom + rangeTo) / 2;
            worker.ProgressInfo.TotalProgress.ProgressText = "Start fetching data... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom;

            List<VBTranslationView> list = new List<VBTranslationView>();

            Guid? acProjectID = null;
            if (CurrentACProject != null)
                acProjectID = CurrentACProject.ACProjectID;
            using (Database database = new core.datamodel.Database())
            {
                list = database.VBTranslationView.FromSql(FormattableStringFactory.Create("udpTranslation @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7",
                        acProjectID,
                        FilterMandatoryClassID,
                        FilterOnlyACClassTables,
                        FilterOnlyMDTables,
                        FilterClassACIdentifier,
                        FilterACIdentifier,
                        FilterTranslation,
                        FilterNotHaveInTranslation))
                   .ToList();
            }

            worker.ProgressInfo.TotalProgress.ProgressCurrent = halfRange;
            int itemsCount = list.Count();
            worker.ProgressInfo.TotalProgress.ProgressText = string.Format("Fetched {0} translations! prepare translation pairs...", itemsCount);
            if (list != null && list.Any())
                foreach (var translationItem in list)
                {
                    translationItem.SetTranslationList(TargetLanguageList);

                    int itemIndex = list.IndexOf(translationItem);
                    int addToProgress = (itemIndex / itemsCount) * halfRange;
                    worker.ProgressInfo.TotalProgress.ProgressCurrent = halfRange + addToProgress;
                }
            worker.ProgressInfo.TotalProgress.ProgressText = "Fetching translation finished!";
            return list;
        }

        private List<VBTranslationView> DoSaveTranslation(ACBackgroundWorker worker, DoWorkEventArgs e, List<VBTranslationView> list, int rangeFrom, int rangeTo, bool updateOnly = false)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start saving translation data... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom;

            int progressDiff = rangeTo - rangeFrom;
            int itemsCount = 0;
            if (list != null)
            {
                itemsCount = list.Count();
                foreach (var translationItem in list)
                {
                    translationItem.TranslationValue = string.Join("", translationItem.EditTranslationList.Where(c => !string.IsNullOrEmpty(c.Translation)).Select(c => c.GetTranslationTuple()));
                    object item = null;
                    if (translationItem.TableName.StartsWith("AC"))
                        item = GetACClassObjectItem(Database as Database, translationItem);
                    else
                        item = GetMDObjectItem(Database as Database, translationItem);

                    if (item != null)
                    {
                        if (updateOnly)
                        {
                            VBTranslationView tmpTranslationView = new VBTranslationView();
                            if (item is IACObjectEntityWithCheckTrans)
                                tmpTranslationView.TranslationValue = (item as IACObjectEntityWithCheckTrans).ACCaptionTranslation;
                            else if (item is IMDTrans)
                                tmpTranslationView.TranslationValue = (item as IMDTrans).MDNameTrans;
                            if (!string.IsNullOrEmpty(tmpTranslationView.TranslationValue))
                                tmpTranslationView.EditTranslationList = VBTranslationView.LoadEditTranslationList(SourceLanguageList, tmpTranslationView.TranslationValue);
                            else
                                tmpTranslationView.EditTranslationList = new List<TranslationPair>();

                            foreach (TranslationPair newTranslation in translationItem.EditTranslationList)
                            {
                                TranslationPair oldTranslation = tmpTranslationView.EditTranslationList.FirstOrDefault(c => c.LangCode == newTranslation.LangCode);
                                if (oldTranslation != null)
                                    oldTranslation.Translation = newTranslation.Translation;
                                else
                                    tmpTranslationView.EditTranslationList.Add(newTranslation);
                            }
                            tmpTranslationView.TranslationValue = string.Join("", tmpTranslationView.EditTranslationList.Where(c => !string.IsNullOrEmpty(c.Translation)).Select(c => c.GetTranslationTuple()));
                            translationItem.TranslationValue = tmpTranslationView.TranslationValue;
                        }

                        if (item is IACObjectEntityWithCheckTrans)
                            (item as IACObjectEntityWithCheckTrans).ACCaptionTranslation = translationItem.TranslationValue;
                        else
                            UpdateMDObject(Database as Database, SourceLanguageList, translationItem);
                    }

                    int itemIndex = list.IndexOf(translationItem);
                    int progressValue = (itemIndex / itemsCount) * progressDiff;
                    worker.ProgressInfo.TotalProgress.ProgressCurrent = progressValue;
                }
                var testResult = Database.ACSaveChanges();
            }
            return list;
        }

        private object GetACClassObjectItem(Database database, VBTranslationView translationItem)
        {
            object result = null;
            ACClass aCClass = database.ACClass.FirstOrDefault(c => c.ACURLCached == translationItem.MandatoryACURLCached);
            if (aCClass == null)
                aCClass = database.ACClass.FirstOrDefault(c => c.ACClassID == translationItem.MandatoryID);
            if (aCClass != null)
                switch (translationItem.TableName)
                {
                    case "ACClass":
                        result = aCClass;
                        break;
                    case "ACClassMessage":
                        ACClassMessage aCClassMessage = aCClass.ACClassMessage_ACClass.FirstOrDefault(c => c.ACIdentifier == translationItem.ACIdentifier);
                        result = aCClassMessage;
                        break;
                    case "ACClassMethod":
                        ACClassMethod aCClassMethod = aCClass.ACClassMethod_ACClass.FirstOrDefault(c => c.ACIdentifier == translationItem.ACIdentifier);
                        result = aCClassMethod;
                        break;
                    case "ACClassProperty":
                        ACClassProperty aCClassProperty = aCClass.ACClassProperty_ACClass.FirstOrDefault(c => c.ACIdentifier == translationItem.ACIdentifier);
                        result = aCClassProperty;
                        break;
                    case "ACClassText":
                        ACClassText aCClassText = aCClass.ACClassText_ACClass.FirstOrDefault(c => c.ACIdentifier == translationItem.ACIdentifier);
                        result = aCClassText;
                        break;
                    case "ACClassDesign":
                        ACClassDesign aCClassDesign = aCClass.ACClassDesign_ACClass.FirstOrDefault(c => c.ACIdentifier == translationItem.ACIdentifier);
                        result = aCClassDesign;
                        break;
                }

            return result;
        }

        private IMDTrans GetMDObjectItem(Database database, VBTranslationView translationItem)
        {
            MDTrans resultItem = null;
            string sql = "select MDKey, MDNameTrans from {0} where MDKey='{1}'";
            sql = string.Format(sql, translationItem.TableName, translationItem.ACIdentifier);

            //FormattableString sqlFormatted = FormattableStringFactory.Create(sql, translationItem.TableName, translationItem.ACIdentifier);
            //var result = database.Database.SqlQuery<MDTrans>(sqlFormatted);
            //if (result != null)
            //    resultItem = result.FirstOrDefault();

            string nameTrans = null;
            string mdKey = null;
            using (var command = database.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                database.Database.OpenConnection();
                using (var dbResult = command.ExecuteReader())
                {
                    foreach (var dbValues in dbResult)
                    {
                        IEnumerable values = dbValues.GetType().GetField("_values", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dbValues) as IEnumerable;
                        foreach (string value in values)
                        {
                            if (value.Contains("en"))
                                nameTrans = value;

                            else
                                mdKey = value;
                        }
                        if (mdKey != null && nameTrans != null)
                            resultItem = new MDTrans(nameTrans, mdKey);
                    }
                }
            }

            return resultItem;
        }

        private void UpdateMDObject(Database database, List<VBLanguage> vbLanguageList, VBTranslationView translationItem)
        {
            string sql = "update {0} set MDNameTrans='{1}' where MDKey='{2}'";
            string translation = translationItem.TranslationValue.Replace("'", "''");
            FormattableString sqlFormatted = FormattableStringFactory.Create(sql, translationItem.TableName, translation, translationItem.ACIdentifier);
            database.Database.ExecuteSql(sqlFormatted);
        }

        #endregion

        #region Methods -> BackgroundWorker -> DoWork -> Generate List

        private List<VBTranslationView> DoGenerateEmptyTranslation(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {

            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate empty translation... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom;

            int half = (rangeFrom - rangeTo) / 2;
            int itemsCount = list.Count();
            foreach (var item in list)
            {
                if (!item.EditTranslationList.Any(x => x.LangCode == targetLanguageCode))
                {
                    TranslationPair translationPair = new TranslationPair() { LangCode = targetLanguageCode, Translation = AutoGeneratePrefix };
                    item.EditTranslationList.Add(translationPair);
                }

                int itemIndex = list.IndexOf(item);
                int progressValue = (itemIndex / itemsCount) * half;
                worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom + progressValue;
            }
            list = DoSaveTranslation(worker, e, list, rangeFrom + half, rangeTo);
            return list;
        }

        private List<VBTranslationView> GeneratePairFromSourceLanguage(ACBackgroundWorker worker, DoWorkEventArgs e, string sourceLangaugeCode, string targetLanguageCode, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate translation from english... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom;

            int half = (rangeFrom - rangeTo) / 2;
            int itemsCount = list.Count();
            foreach (var item in list)
            {
                if (!item.EditTranslationList.Any(x => x.LangCode == targetLanguageCode))
                {
                    TranslationPair sourcePair = item.EditTranslationList.FirstOrDefault(c => c.LangCode == sourceLangaugeCode);
                    if (sourcePair != null)
                    {
                        TranslationPair translationPair = new TranslationPair() { LangCode = targetLanguageCode, Translation = AutoGeneratePrefix + sourcePair.Translation };
                        item.EditTranslationList.Add(translationPair);
                    }
                }


                int itemIndex = list.IndexOf(item);
                int progressValue = (itemIndex / itemsCount) * half;
                worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom + progressValue;
            }
            list = DoSaveTranslation(worker, e, list, rangeFrom + half, rangeTo);
            return list;
        }

        private List<VBTranslationView> DoGeneratePairUsingGoogleApi(ACBackgroundWorker worker, DoWorkEventArgs e, string sourceLanguageCode, string targetLanguageCode, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate translation from Google API... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom;

            List<VBTranslationView> itemsWithoutSource =
                list
                .Where(c =>
                            c.EditTranslationList == null
                            || !c.EditTranslationList.Any(x => x.LangCode == sourceLanguageCode)
                            || !c.EditTranslationList.Any(x => x.LangCode == sourceLanguageCode && !string.IsNullOrEmpty(x.Translation))
                       )
                .ToList();
            Guid[] itemsWithoutSourceIDs = itemsWithoutSource.Select(c => c.ID).ToArray();
            List<VBTranslationView> itemsWithSource = list.Where(c => !itemsWithoutSourceIDs.Contains(c.ID)).ToList();

            int half = (rangeFrom - rangeTo) / 2;

            string[] searchedTranslations = itemsWithSource.Select(c => c.EditTranslationList.FirstOrDefault(x => x.LangCode == sourceLanguageCode)).Select(c => c.Translation).ToArray();

            List<TranslationPair> translationPairs = new List<TranslationPair>();
            Dictionary<int, string[]> searchedTranslationPages = GetTranslationPagesForGoogleApi(searchedTranslations);
            foreach (KeyValuePair<int, string[]> page in searchedTranslationPages)
            {
                List<TranslationPair> tempTranslationPair = GetTranslationPairFromGoogleApi(sourceLanguageCode, targetLanguageCode, page.Value);
                translationPairs.AddRange(tempTranslationPair);
                Thread.Sleep(1000);
            }


            worker.ProgressInfo.TotalProgress.ProgressCurrent = half + rangeFrom;
            int count = searchedTranslations.Length;

            for (int i = 0; i < count; i++)
            {
                TranslationPair translationPair = translationPairs[i];
                itemsWithSource[i].EditTranslationList.Add(translationPair);
            }

            list = DoSaveTranslation(worker, e, list, half + rangeFrom, rangeTo);
            return list;
        }

        private List<VBTranslationView> DoRemoveAutoGenerated(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start remove generated translations... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom;

            int half = (rangeFrom - rangeTo) / 2;
            foreach (var item in list)
            {
                TranslationPair pair = item.EditTranslationList.FirstOrDefault(c => c.LangCode == targetLanguageCode);
                if (pair != null && pair.Translation.StartsWith(AutoGeneratePrefix))
                    item.EditTranslationList.Remove(pair);
            }
            list = DoSaveTranslation(worker, e, list, rangeFrom + half, rangeTo);
            return list;
        }

        private List<VBTranslationView> DoRename(ACBackgroundWorker worker, DoWorkEventArgs e, string sourceLanguageCode, string targetLanguageCode,
            string fromText, string toText, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {
            int half = (rangeTo - rangeFrom) / 2;
            worker.ProgressInfo.TotalProgress.ProgressText = string.Format("Start replace translations {0} => {1} for entire list...", fromText, toText);
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom;

            int itemsCount = list.Count;

            foreach (VBTranslationView item in list)
            {
                TranslationPair sourceTranslationPair = item.EditTranslationList.FirstOrDefault(c => c.LangCode == sourceLanguageCode);
                TranslationPair targetTranslationPair = item.EditTranslationList.FirstOrDefault(c => c.LangCode == targetLanguageCode);
                if (sourceTranslationPair != null)
                {
                    bool isMatch =
                   !string.IsNullOrEmpty(ReplaceACIdentifier)
                   && item.ACIdentifier == ReplaceACIdentifier;

                    if (!isMatch && !string.IsNullOrEmpty(fromText))
                    {
                        isMatch = IsReplaceSearchInSourceLanguage ?
                                (sourceTranslationPair != null
                                && (IsReplaceTranslationExact ? sourceTranslationPair.Translation == fromText : sourceTranslationPair.Translation.Contains(fromText)))
                                :
                                (targetTranslationPair != null
                                && (IsReplaceTranslationExact ? targetTranslationPair.Translation == fromText : targetTranslationPair.Translation.Contains(fromText)));
                    }

                    if (isMatch)
                    {
                        if (!string.IsNullOrEmpty(ReplaceACIdentifier) || IsReplaceTranslationExact || IsReplaceSearchInSourceLanguage)
                        {
                            if (targetTranslationPair == null)
                            {
                                targetTranslationPair = new TranslationPair() { LangCode = targetLanguageCode };
                                item.EditTranslationList.Add(targetTranslationPair);
                            }
                            targetTranslationPair.Translation = toText;
                        }
                        else
                            targetTranslationPair.Translation = targetTranslationPair.Translation.Replace(fromText, toText);
                    }
                }


                int itemIndex = list.IndexOf(item);
                int progressValue = (itemIndex / itemsCount) * half;
                worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom + progressValue;
            }

            return DoSaveTranslation(worker, e, list, half, rangeTo);
        }

        private void DoRenameAll(ACBackgroundWorker worker, DoWorkEventArgs e, string sourceLangaugeCode, string targetLanguageCode, string fromText, string toText)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = string.Format("Start replace translations {0} => {1} for entire list...", fromText, toText);
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 0;

            List<VBTranslationView> allTranslations = GetAllTranslations();
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            DoRename(worker, e, sourceLangaugeCode, targetLanguageCode, fromText, toText, allTranslations, 20, 80);
        }

        #endregion

        #region Methods -> BackgroundWorker -> DoWork -> Generate All

        private List<VBTranslationView> DoGenerateEmptyTranslationAll(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate emtpy translations for all items! Fetch all items...";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 0;

            List<VBTranslationView> allTranslations = GetAllTranslations();
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            DoGenerateEmptyTranslation(worker, e, targetLanguageCode, allTranslations, 20, 80);
            return DoFetchTranslation(worker, e, 80, 100); // Return only preselected by filter
        }

        private List<VBTranslationView> GeneratePairFromSourceLanguageAll(ACBackgroundWorker worker, DoWorkEventArgs e, string sourceLanguageCode, string targetLanguageCode)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate english copy of translations for all items! Fetch all items...";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 0;

            List<VBTranslationView> allTranslations = GetAllTranslations();
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            GeneratePairFromSourceLanguage(worker, e, sourceLanguageCode, targetLanguageCode, allTranslations, 20, 80);
            return DoFetchTranslation(worker, e, 80, 100); // Return only preselected by filter
        }

        private List<VBTranslationView> DoGeneratePairUsingGoogleApiAll(ACBackgroundWorker worker, DoWorkEventArgs e, string sourceLanguageCode, string targetLanguageCode)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate Google API translations for all items! Fetch all items ...";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 0;

            List<VBTranslationView> allTranslations = GetAllTranslations();
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            DoGeneratePairUsingGoogleApi(worker, e, sourceLanguageCode, targetLanguageCode, allTranslations, 20, 80);
            return DoFetchTranslation(worker, e, 80, 100); // Return only preselected by filter
        }

        private List<VBTranslationView> DoRemoveAutoGeneratedAll(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Remove autogenerated translations for all items! Fetch all items...";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 0;

            List<VBTranslationView> allTranslations = GetAllTranslations();
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            DoRemoveAutoGenerated(worker, e, targetLanguageCode, allTranslations, 20, 80);
            return DoFetchTranslation(worker, e, 80, 100); // Return only preselected by filter
        }

        #endregion

        #region Methods -> BackgroudWorker -> DoWork -> Export and import

        private void DoExport(ACBackgroundWorker worker, DoWorkEventArgs e, string currentExportFolder, string currentExportFileName, bool exportOnlyForSelectedTargetLanguage, string targetLanguageCode, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start export to file: " + currentExportFileName;
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom;

            string file = Path.Combine(currentExportFolder, currentExportFileName);
            List<VBTranslationView> localList = list.ToList();
            if (exportOnlyForSelectedTargetLanguage)
            {
                foreach (var item in localList)
                {
                    item.EditTranslationList.RemoveAll(x => x.LangCode != targetLanguageCode);
                }
            }

            var queryLocalList =
                localList
                .Select(c => new
                {
                    c.ACProjectName,
                    c.TableName,
                    c.MandatoryID,
                    c.MandatoryACIdentifier,
                    c.MandatoryACURLCached,
                    c.ID,
                    c.ACIdentifier,
                    c.UpdateName,
                    c.UpdateDate,
                    c.EditTranslationList
                });
            using (StreamWriter writer = new StreamWriter(file))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer ser = new JsonSerializer();
                if (IsJsonPrettyPrint)
                    ser.Formatting = Formatting.Indented;
                ser.Serialize(jsonWriter, queryLocalList);
                jsonWriter.Flush();
            }
        }

        private void DoExportAll(ACBackgroundWorker worker, DoWorkEventArgs e, string currentExportFolder, string currentExportFileName, bool exportOnlyForSelectedTargetLanguage, string targetLanguageCode, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start export all translations";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 0;

            string haveIntranslation = null;
            if (SelectedTargetLanguage != null && ExportOnlyForSelectedTargetLanguage)
                haveIntranslation = string.Format(@"{0}{{", SelectedTargetLanguage.VBLanguageCode);

            List<VBTranslationView> allTranslations = GetAllTranslations(haveIntranslation);
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            DoExport(worker, e, currentExportFolder, currentExportFileName, exportOnlyForSelectedTargetLanguage, targetLanguageCode, allTranslations, 20, rangeTo);
        }

        private void DoImport(ACBackgroundWorker worker, DoWorkEventArgs e, string importSourcePath, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start saving translation data... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom;

            int half = (rangeTo - rangeFrom) / 2;
            List<VBTranslationView> list = new List<VBTranslationView>();
            using (StreamReader streamReader = new StreamReader(importSourcePath))
            using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
            {
                JsonSerializer ser = new JsonSerializer();
                list = ser.Deserialize<List<VBTranslationView>>(jsonTextReader);
            }

            worker.ProgressInfo.TotalProgress.ProgressCurrent = half;
            DoSaveTranslation(worker, e, list, half, rangeTo, true);
        }

        #endregion

        #region Methods -> BackgroundWorkser -> DoWork -> DoGetACClassTranslationStatus

        private List<Guid> DoGetACClassTranslationStatus(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode, Guid aCProjectID)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Load project tree translation status...";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 0;

            List<Guid> classNotNaveTranslation = null;
            string notHaveInTranslation = targetLanguageCode + "{";
            List<VBTranslationView> list = GetAllTranslations(null, notHaveInTranslation);

            list =
                list
                .Where(c =>
                        c.TranslationValue != null
                        && !string.IsNullOrEmpty(c.TranslationValue.Trim())
                ).ToList();

            if (list != null)
                classNotNaveTranslation = list.Select(c => c.MandatoryID).Distinct().ToList();

            return classNotNaveTranslation;
        }

        #endregion

        #region Methods -> BackgroundWorker -> DoWork -> Common

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchedTranslations"></param>
        /// <returns></returns>
        private Dictionary<int, string[]> GetTranslationPagesForGoogleApi(string[] searchedTranslations)
        {
            Dictionary<int, string[]> pages = new Dictionary<int, string[]>();
            int tempCountBytes = 0;
            int translationCount = searchedTranslations.Count();
            List<string> pageItems = new List<string>();
            int currentPage = 0;
            int currentPageLength = 0;
            int currentStringLength = 0;

            for (int i = 0; i < translationCount; i++)
            {
                currentStringLength = searchedTranslations[i].Length * sizeof(Char);
                bool isLenthOwerflow =
                    ((tempCountBytes + currentStringLength) >= Const_GoogleAPIBytesLimit)
                    ||
                    (currentPageLength + 1 >= Const_GoogleAPILengthLimit);
                if (isLenthOwerflow)
                {
                    pages.Add(currentPage, pageItems.ToArray());
                    currentPage++;

                    tempCountBytes = 0;
                    currentPageLength = 0;
                    pageItems = new List<string>();
                }

                currentPageLength++;
                pageItems.Add(searchedTranslations[i]);
                tempCountBytes += currentStringLength;
            }

            if (pageItems.Any())
            {
                currentPage++;
                pages.Add(currentPage, pageItems.ToArray());
            }
            return pages;
        }

        /// <summary>Fetch translation from google api</summary>
        /// <param name="sourceLanguageCode"></param>
        /// <param name="targetLanguageCode"></param>
        /// <param name="pharses"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        private List<TranslationPair> GetTranslationPairFromGoogleApi(string sourceLanguageCode, string targetLanguageCode, string[] pharses)
        {
            List<TranslationPair> result = new List<TranslationPair>();
            TranslateTextRequest request = new TranslateTextRequest
            {
                SourceLanguageCode = GetGoogleLanguageCode(sourceLanguageCode),
                TargetLanguageCode = GetGoogleLanguageCode(targetLanguageCode),
                Parent = new ProjectName(GoogleProjectID).ToString()
            };
            request.Contents.Add(pharses);
            TranslateTextResponse response = GoogleTranslationServiceClient.TranslateText(request);
            if (response.Translations != null && response.Translations.Any())
            {
                foreach (Translation translation in response.Translations)
                {
                    string translatedText = translation.TranslatedText;
                    if (!string.IsNullOrEmpty(translatedText))
                        translatedText = translatedText[0].ToString().ToUpper() + translatedText.Substring(1, translatedText.Length - 1);
                    TranslationPair translationPair = new TranslationPair() { LangCode = targetLanguageCode, Translation = AutoGeneratePrefix + translatedText };
                    result.Add(translationPair);
                }
            }
            return result;
        }

        public string GetGoogleLanguageCode(string langCode)
        {
            string resultLangCode = null;
            if (langCode == "en")
                resultLangCode = "en-US";
            else if (langCode == "de")
                resultLangCode = "de-DE";
            else if (langCode == "hr")
                resultLangCode = "hr-HR";
            else
                throw new NotSupportedException(string.Format(@"Not prepared code for language: {0}", langCode));
            return resultLangCode;
        }

        private List<VBTranslationView> GetAllTranslations(string haveInTranslation = null, string notHaveInTranslation = null)
        {
            List<VBTranslationView> list = new List<VBTranslationView>();
            Guid? acProjectID = null;
            if (CurrentACProject != null)
                acProjectID = CurrentACProject.ACProjectID;

            bool? filterOnlyACClassTables = null;
            bool? filterOnlyMDTables = null;
            if (acProjectID != null)
            {
                filterOnlyACClassTables = true;
                filterOnlyMDTables = false;
            }
            else
            {
                filterOnlyACClassTables = FilterOnlyACClassTables;
                filterOnlyMDTables = FilterOnlyMDTables;
            }
            using (Database database = new core.datamodel.Database())
            {
                list = database.VBTranslationView.FromSql(FormattableStringFactory.Create("udpTranslation @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7",
                          acProjectID,
                          null,
                          filterOnlyACClassTables,
                          filterOnlyMDTables,
                          null,
                          null,
                          haveInTranslation,
                          notHaveInTranslation))
                      .ToList();
            }

            if (list != null && list.Any())
                foreach (var translationItem in list)
                    translationItem.SetTranslationList(TargetLanguageList);

            return list;
        }

        #endregion

        #endregion

        #endregion

        public enum TranslationAutogenerateOption
        {
            FetchTranslation,
            SaveTranslation,

            GenerateEmptyTranslation,
            GeneratePairFromSourceLanguage,
            GeneratePairUsingGoogleApi,

            GenerateEmptyTranslationAll,
            GeneratePairFromSourceLanguageAll,
            GeneratePairUsingGoogleApiAll,

            RemoveAutoGenerated,
            RemoveAutoGeneratedAll,

            Import,
            Export,
            ExportAll,

            Replace,
            ReplaceAll,
            GetACClassTranslationStatus
        }
    }
}

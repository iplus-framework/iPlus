using System.CodeDom;

namespace gip.core.datamodel
{
    /// <summary>
    /// Definition der grundsätzlichen Ansichtsarten, welche das Standardframework unterstützt
    /// </summary>
    public static class Const
    {
        public const string ACRootClassName = "ACRoot";
        public const string ACRootProjectName = "Root";
        public const string ACRootProjectNameTest = "RootTest";
        public const string Select = "en{'Select'}de{'Auswahl'}";
        public const string Ok = "en{'Ok'}de{'Ok'}";
        public const string Add = "en{'Add'}de{'Hinzufügen'}";
        public const string Cancel = "en{'Cancel'}de{'Abbrechen'}";
        public const string Remove = "en{'Remove'}de{'Entfernen'}";
        public const string Recalculate = "en{'Recalculate'}de{'Neu berechnen'}";

        //Datamodel Konstanten
        public const string EntityInsertName = "InsertName";
        public const string EntityUpdateName = "UpdateName";
        public const string EntityDeleteName = "DeleteName";
        public const string EntityInsertDate = "InsertDate";
        public const string EntityUpdateDate = "UpdateDate";
        public const string EntityDeleteDate = "DeleteDate";
        public const string EntityXMLConfig = "XMLConfig";

        public const string EntityTransInsertName = "en{'Created by'}de{'Angelegt von'}";
        public const string EntityTransUpdateName = "en{'Updated by'}de{'Aktualisiert von'}";
        public const string EntityTransDeleteName = "en{'Deleted by'}de{'Gelöscht von'}";
        public const string EntityTransInsertDate = "en{'Created on'}de{'Angelegt am'}";
        public const string EntityTransUpdateDate = "en{'Updated on'}de{'Aktualisiert am'}";
        public const string EntityTransDeleteDate = "en{'Deleted on'}de{'Gelöscht am'}";

        public const string IsDefault = "IsDefault";
        public const string EntityIsDefault = "en{'Default'}de{'Standard'}";
        public const string IsEnabled = "IsEnabled";
        public const string EntityIsEnabled = "en{'Enabled'}de{'Aktiv'}";
        public const string MDKey = "MDKey";
        public const string EntityKey = "en{'Key'}de{'Schlüssel'}";
        public const string MDNameTrans = "MDNameTrans";
        public const string EntityNameTrans = "en{'Names'}de{'Bezeichner'}";
        public const string SortIndex = "SortIndex";
        public const string EntitySortSequence = "en{'Sort Sequence'}de{'Sortierreihenfolge'}";

        public const string ACState = "ACState";
        public const string ACGroup = "ACGroup";
        public const string ACKindIndex = "ACKindIndex";
        public const string ACPropertyPrefix = "ACProperty";
        public const string IACPropertyPrefix = "IACProperty";
        public const string IACContainerTNetPrefix = "IACContainerTNet";
        public const string ACQueryTypePrefix = "QueryType";
        public const string ACQueryRootObjectPrefix = "RootObject";
        public const string ACUrlPrefix = "ACUrl";
        public const string ChildACUrlPrefix = "Child" + ACUrlPrefix;
        public const string ACIdentifierPrefix = "ACIdentifier";
        public const string ACCaptionPrefix = "ACCaption";
        public const string GlobalDatabase = "GlobalDatabase";
        public const string QueryPrefix = "QRY";
        public const string MN_NewACObject = "NewACObject";
        public const string MN_GetEnumList = "GetEnumList";

        public const string PreConfigACUrl = "en{'Parent WF URL'}de{'WF Eltern-URL'}";
        public const string LocalConfigACUrl = "en{'Property URL'}de{'Eigenschafts-URL'}";

        public const string PN_KeyACUrl = "KeyACUrl";
        public const string PN_LocalConfigACUrl = "LocalConfigACUrl";
        public const string PN_PreConfigACUrl = "PreConfigACUrl";
        public const string PN_ConfigACUrl = "ConfigACUrl";

        public const string KeyACUrl_BusinessobjectList = ".\\ACClassProperty(BussinessobjectList)";
        public const string LocalConfigACUrl_BusinessobjectList = "Bussinessobject";
        public const string KeyACUrl_NavigationqueryList = ".\\ACClassProperty(NavigationqueryList)";
        public const string LocalConfigACUrl_NavigationqueryList = "Navigationquery";
        public const string KeyACUrl_EnumACValueList = ".\\ACClassProperty(EnumACValueList)";
        public const string LocalConfigACUrl_EnumACValueList = "EnumACValue";

        public const string ACQueryExportFileType = ".gip";
        public const string InitState = "InitState";
        public const string ACUrlCmdMessage = "ACUrlCmdMessage";
        public const string IsEnabledPrefix = "IsEnabled";
        public const string AskUserPrefix = "AskUser";

        public const string ValueT = "ValueT";
        public const string Value = "Value";
        public const string SourceValue = "SourceValue";
        public const string TNameNullable = "Nullable`1";
        public const string TNameString = "String";
        public const string TNameDouble = "Double";
        public const string TNameFloat = "Float";
        public const string TNameSingle = "Single";
        public const string TNameDecimal = "Decimal";
        public const string TNameInt64 = "Int64";
        public const string TNameUInt64 ="UInt64";
        public const string TNameInt32 = "Int32";
        public const string TNameUInt32 = "UInt32";
        public const string TNameInt16 = "Int16";
        public const string TNameUInt16 = "UInt16";
        public const string TNameByte = "Byte";
        public const string TNameBoolean = "Boolean";
        public const string TNameDateTime = "DateTime";
        public const string TNameTimeSpan = "TimeSpan";

        public const string UnknownClass = "UnknownClass";
        public const string UnknownProperty = "UnknownProperty";
        public const string UnknownMethod = "UnknownMethod";
        public const string UnknownDesign = "UnknownDesign";
        public const string UnknownWorkflow = "UnknownWorkflow";

        /// <summary>
        /// Modus für ACObject
        /// </summary>
        public const string SMReadOnly = "SMReadOnly";                // Ansichtsmodus, bei BSOs, deren Daten nur anzuzeigen sind
        public const string SMNew = "SMNew";                          // Neuanlagemodus
        public const string SMEdit = "SMEdit";                        // Bearbeitenmodus
        public const string SMSearch = "SMSearch";                    // Suchmodus

        public const string TPWNodeStart = "Start";
        public const string TPWNodeEnd = "End";

        /// <summary>
        /// Befehle für das aktive Fenster
        /// </summary>
        public const string CmdNameNew = "New";
        public const string CmdNew = "!" + CmdNameNew;                        // Einfügen eines Hauptdatensatzes
        public const string CmdNameLoad = "Load";                      // Laden eines Hauptdatensatzes
        public const string CmdLoad = "!" + CmdNameLoad;                      // Laden eines Hauptdatensatzes
        public const string CmdNameUnLoad = "UnLoad";                      // Entladen eines Hauptdatensatzes
        public const string CmdUnLoad = "!" + CmdNameUnLoad;                      // Entladen eines Hauptdatensatzes
        public const string CmdNameDelete = "Delete";                  // Löschen des aktuellen Hauptdatensatzes
        public const string CmdDelete = "!" + CmdNameDelete;                  // Löschen des aktuellen Hauptdatensatzes
        public const string CmdNameRestore = "Restore";
        public const string CmdRestore = "!" + CmdNameRestore;          
        public const string CmdNameSave = "Save";                      // Speichern des Hauptdatensatzes
        public const string CmdSave = "!" + CmdNameSave;                      // Speichern des Hauptdatensatzes
        public const string CmdNameUndoSave = "UndoSave";                  // Kein Speichern des Hauptdatensatzes
        public const string CmdUndoSave = "!" + CmdNameUndoSave;                  // Kein Speichern des Hauptdatensatzes
        public const string CmdNameSearch = "Search";                  // Suchen mit Filter
        public const string CmdSearch = "!" + CmdNameSearch;                  // Suchen mit Filter
        public const string CmdNameFindAndReplace = "FindAndReplace";  // Suchen und Ersetzen
        public const string CmdFindAndReplace = "!" + CmdNameFindAndReplace;  // Suchen und Ersetzen

        public const string CmdNameNavigateFirst = "NavigateFirst";    // Laden der ersten Entity
        public const string CmdNavigateFirst = "!" + CmdNameNavigateFirst;    // Laden der ersten Entity
        public const string CmdNameNavigatePrev = "NavigatePrev";      // Laden der vorherigen Entity
        public const string CmdNavigatePrev = "!" + CmdNameNavigatePrev;      // Laden der vorherigen Entity
        public const string CmdNameNavigateNext = "NavigateNext";      // Laden der nächsten Entity
        public const string CmdNavigateNext = "!" + CmdNameNavigateNext;      // Laden der nächsten Entity
        public const string CmdNameNavigateLast = "NavigateLast";      // Laden der letzten Entity
        public const string CmdNavigateLast = "!" + CmdNameNavigateLast;      // Laden der letzten Entity

        public const string CmdNavigateFirstPrimary = "AccessPrimary" + CmdNavigateFirst;    // Laden der ersten EntityPrimary
        public const string CmdNavigatePrevPrimary = "AccessPrimary" + CmdNavigatePrev;      // Laden der vorherigen EntityPrimary
        public const string CmdNavigateNextPrimary = "AccessPrimary" + CmdNavigateNext;      // Laden der nächsten EntityPrimary
        public const string CmdNavigateLastPrimary = "AccessPrimary" + CmdNavigateLast;      // Laden der letzten EntityPrimary

        public const string CmdNameDesignModeOn = "DesignModeOn";              // Entwurfmodus beim VBCanvas einschalten
        public const string CmdDesignModeOn = "!" + CmdNameDesignModeOn;              // Entwurfmodus beim VBCanvas einschalten
        public const string CmdNameDesignModeOff = "DesignModeOff";            // Entwurfmodus beim VBCanvas ausschalten
        public const string CmdDesignModeOff = "!" + CmdNameDesignModeOff;            // Entwurfmodus beim VBCanvas ausschalten


        public const string CmdAssignShapeConfig = "!AssignShapeConfig";          
        public const string CmdPreSetShapeConfig = "!PreSetShapeConfig";          
        public const string CmdResetShapeConfig = "!ResetShapeConfig";

        public const string CmdNameQueryPrintDlg = "QueryPrintDlg";        // Drucken des Berichts (Dialog)
        public const string CmdQueryPrintDlg = "!" + CmdNameQueryPrintDlg;        // Drucken des Berichts (Dialog)
        public const string CmdNameQueryPreviewDlg = "QueryPreviewDlg";    // Vorschau des Berichts (Dialog)
        public const string CmdQueryPreviewDlg = "!" + CmdNameQueryPreviewDlg;    // Vorschau des Berichts (Dialog)
        public const string CmdNameQueryDesignDlg = "QueryDesignDlg";      // Design des Berichts (Dialog)
        public const string CmdQueryDesignDlg = "!" + CmdNameQueryDesignDlg;      // Design des Berichts (Dialog)
        public const string CmdNameReportPreview = "ReportPreview";    // Vorschau des Berichts
        public const string CmdReportPreview = "!" + CmdNameReportPreview;    // Vorschau des Berichts
        public const string CmdNameReportPrint = "ReportPrint";        // Drucken des Berichts
        public const string CmdReportPrint = "!" + CmdNameReportPrint;        // Drucken des Berichts
        public const string CmdNameReportDesign = "ReportDesign";      // Drucken des Berichts
        public const string CmdReportDesign = "!" + CmdNameReportDesign;      // Drucken des Berichts
        public const string CmdNameReportCancel = "ReportCancel";      // Abbrechen des Berichtsdialogs
        public const string CmdReportCancel = "!" + CmdNameReportCancel;      // Abbrechen des Berichtsdialogs

        public const string CmdNameReportNew = "ReportNew";            // Neuer Bericht
        public const string CmdReportNew = "!" + CmdNameReportNew;            // Neuer Bericht
        public const string CmdNameReportLoad = "ReportLoad";          // Laden eines Berichts
        public const string CmdReportLoad = "!" + CmdNameReportLoad;          // Laden eines Berichts
        public const string CmdNameReportDelete = "ReportDelete";      // Bericht löschen
        public const string CmdReportDelete = "!" + CmdNameReportDelete;      // Bericht löschen
        public const string CmdNameReportSave = "ReportSave";          // Speichern eines Berichts
        public const string CmdReportSave = "!" + CmdNameReportSave;          // Speichern eines Berichts

        public const string CmdCloseBSO = "!CloseBSO";                    // Aktives Fenster schliessen
        public const string CmdCloseBSOTab = "!CloseBSOTab";              // Aktives Fenster schliessen 

        public const string CmdShowACQueryDialog = "!ShowACQueryDialog";        // Suchdialog für Suchkriterien
        public const string CmdShowACQueryDialogPrimary = "AccessPrimary" + CmdShowACQueryDialog;        // Suchdialog für Suchkriterien

        // Navigation
        public const string CmdUpdateAllData = "!UpdateAllData";        // Daten komplett aktualisieren
        public const string CmdNewData = "!NewData";                    // Daten hinten anfügen
        public const string CmdInsertData = "!InsertData";              // Daten hinzufügen
        public const string CmdDeleteData = "!DeleteData";              // Daten löschen
        public const string CmdAddChildData = "!AddChildData";          // Kind-Daten hinten anfügen
        public const string CmdMoveData = "!MoveData";                  // Daten verschoben
        public const string CmdAddChildNoExpand = "!AddChildNoExpand";  // Add child data without tree view expand

        public const string CmdMoveDown = "!MoveDown";              // Daten nach unten verschieben
        public const string CmdMoveUp = "!MoveUp";                  // Daten nach oben verschieben
        public const string CmdMovedData = "!MovedData";            // Daten wurden visuell im Layout verschoben

        public const string CmdNameExport = "DataExportDialog";            // dataExport
        public const string CmdExport = "!" + CmdNameExport;            // dataExport

        public const string CmdNameKeyHandle = "KeyHandle";            // keyHand
        public const string CmdKeyHandle = "!" + CmdNameKeyHandle;            // dataExport

        // Visualisierung
        public const string CmdClassConfigurationOK = "!ClassConfigurationOK";
        public const string CmdClassConfigurationCancel = "!ClassConfigurationCancel";
        public const string CmdClassForeGround = "!ClassForeGround";
        public const string CmdClassBackGround = "!ClassBackGround";
        public const string CmdClassFore = "!ClassFore";
        public const string CmdClassBack = "!ClassBack";

        // Layoutausrichtung
        public const string CmdAlignLeft                = "!AlignLeft";
        public const string CmdAlignHorizontalCenters   = "!AlignHorizontalCenters";
        public const string CmdAlignRight               = "!AlignRight";
        public const string CmdAlignTop                 = "!AlignTop";
        public const string CmdAlignVerticalCenters     = "!AlignVerticalCenters";
        public const string CmdAlignBottom              = "!AlignBottom";
        public const string CmdDistributeHorizontal     = "!DistributeHorizontal";
        public const string CmdDistributeVertical       = "!DistributeVertical";

        /// <summary>
        /// Befehle für die gesamte Anwendung
        /// </summary>
//        public const string CmdCloseApplication = "!CloseApplication";  // Anwendung schliessen
        public const string CmdWPFCommand = "!WPFCommand"; // NUR INTERN VERWENDEN
        //public const string CmdCloseTopDialog = "!CloseTopDialog";  // Obersten Dialog schliessen
        public const string CmdClose = "!Close";
        public const string EventDeInit = "^ACDeInit";
        public const string CmdDeInit = "!ACDeInit";
        public const string CmdShowMsgBox = "!ShowMsgBox";
        public const string CmdStartBusinessobject = "!StartBusinessobject";
        public const string BusinessobjectsACUrl = "\\Businessobjects";
        public const string BSOiPlusStudio = "BSOiPlusStudio";

        //public const string CmdShowBusinessobjectDialog = "!ShowBusinessobjectDialog";
        //public const string CmdShowBusinessobjectDialogModeless = "!ShowBusinessobjectDialogModeless";
        //public const string CmdShowBusinessobjectDialogCancel = "!ShowBusinessobjectDialogCancel";

        public const string CmdNameCopy = "Copy";                      // Kopieren, ApplicationCommands.Copy
        public const string CmdCopy = "!" + CmdNameCopy;                      // Kopieren, ApplicationCommands.Copy
        public const string CmdNameCut = "Cut";                        // Ausschneiden, ApplicationCommands.Cut
        public const string CmdCut = "!" + CmdNameCut;                        // Ausschneiden, ApplicationCommands.Cut
        public const string CmdNamePaste = "Paste";                    // Einfügen, ApplicationCommands.Paste
        public const string CmdPaste = "!" + CmdNamePaste;                    // Einfügen, ApplicationCommands.Paste

        public const string CmdNameUndo = "Undo";                      // Rückgängig, ApplicationCommands.Undo
        public const string CmdUndo = "!" + CmdNameUndo;                      // Rückgängig, ApplicationCommands.Undo

        public const string CmdNameRedo = "Redo";                      // Wiederherstellen, ApplicationCommands.Redo
        public const string CmdRedo = "!" + CmdNameRedo;                      // Wiederherstellen, ApplicationCommands.Redo

        
        public const string CmdSelectAll = "!SelectAll";            // Alles auswählen
        public const string CmdClear = "!Clear";                    // Leeren

        public const string CmdTooltip = "Tooltip";
        public const string CmdFocusAndSelectAll = "FocusAndASelectAll";

        public const string CmdUpdateControlMode = "^CmdUpdateControlMode";
        public const string CmdInvalidateRequerySuggested = "^InvalidateRequerySuggested";
        public const string CmdHighlightVBControl = "^HighlightVBControl";
        public const string CmdHighlightContentACObject = "^HighlightContentACObject";
        public const string CmdInitSelectionManager = "^InitSelectionManager";
        public const string CmdShowHideVBContentInfo = "^ShowHideVBContentInfo";
        public const string CmdPrintScreenToImage = "^PrintScreenToImage";
        public const string CmdPrintScreenToIcon = "^CmdPrintScreenToIcon";
        public const string CmdPrintScreenToClipboard = "^CmdPrintScreenToClipboard";
        public const string CmdExportDesignToFile = "^CmdExportDesignToFile";
        public const string CmdFindGUI = "^CmdFindGUI";
        public const string CmdFindGUIResult = "^CmdFindGUIResult";
        public const string CmdOnCloseWindow = "^CmdOnCloseWindow";
        public const string CmdDesignerActive = "^PrintScreenToImage";
        public const string CmdUpdateVBContent = "^CmdUpdateVBContent";
		public const string CmdApplyConfig = "^ApplyConfig";
        public const string CmdFileOpen = "^CmdFileOpen";

        // Konstanten für Workflows
        public const string PWPointIn = "PWPointIn";
        public const string PWPointOut = "PWPointOut";
        public const string PWPointOut2 = "PWPointOut2";
        public const string ACClassIdentifierOfPWMethod = "PWMethod";
        public const string ACClassIdentifierOfPWNodeProcessWorkflow = "PWNodeProcessWorkflow";
        public const string VBPresenter_SelectedWFContext = "SelectedWFContext"; // VBPresenter.SelectedWFContext
        public const string VBPresenter_SelectedRootWFNode = "SelectedRootWFNode"; // VBPresenter.SelectedRootWFNode
        public const string SelectionManagerCDesign_ClassName = "VBBSOSelectionManager(CurrentDesign)";
        public const string VBVisualGroup_ClassName = "VBVisualGroup";
        public const string VBVisual_ClassName = "VBVisual";
        public const string VBDesign_ClassName = "VBDesign";
        public const string VBImage_ClassName = "VBImage";
        public const string VBBorder_ClassName = "VBBorder";
        public const string VBTranslationText_ClassName = "VBTranslationText";

        // Konstanten für PAEquipment und PAModule
        public const string PAPointMatIn1 = "PAPointMatIn1";
        public const string PAPointMatIn2 = "PAPointMatIn2";
        public const string PAPointMatOut1 = "PAPointMatOut1";
        public const string PAPointMatOut2 = "PAPointMatOut2";
        public const string PAPointMatOut3 = "PAPointMatOut3";
        public const string PAPointMatInOut1 = "PAPointMatInOut1";
        public const string PAPointMatInOut2 = "PAPointMatInOut2";

        // Parameter für den Start von ACComponent
        public const string ParamAutoLoad = "AutoLoad";
        public const string ParamAutoFilter = "AutoFilter";
        public const string ParamInheritedMember = "InheritedM";
        public const string ParamSeperateContext = "SeperateContext";
        public const string SkipSearchOnStart = "SkipSearchOnStart";

        public const string TaskSubscriptionPoint = "TaskSubscriptionPoint";
        public const string TaskInvocationPoint = "TaskInvocationPoint";
        public const string TaskCallback = "TaskCallback";

        public const string ContextIPlus = "ContextIPlus";
        public const string ContextDatabase = "Database";
        public const string ContextDatabaseIPlus = Const.ContextDatabase + "\\ContextIPlus";

        public const string StartupParamSimulation = "Simulation";
        public const string StartupParamUser = "User";
        public const string StartupParamRegisterACObjects = "RegisterACObjects";
        public const string StartupParamFullscreen = "Fullscreen";
        public const string StartupParamAutologin = "Autologin";

        /// <summary>
        /// Parameter for disabling Persistence of Property-Values. It's used for persistable Target-Properties 
        /// which are bound to Source-Properties from Data-Access-Components. This parameter is needed to prevent saving wrong values when e.g. a PLS is resetted 
        /// and the IPlus-Server starts and overrides the persisted Target-Properties with wrong values which comes form the PLC-Driver.
        /// </summary>
        public const string StartupParamPropPersistenceOff = "PropPersistenceOff";
        public const string StartupParamWCFOff = "WCFOff";

        public const string PackName_System = "System";
        public const string PackName_VarioSystem = "gip.VarioSystem";
        public const string PackName_VarioDevelopment = "gip.VarioDevelopment";
        public const string PackName_VarioAutomation = "gip.VarioAutomation";
        public const string PackName_VarioTest = "gip.VarioTest";
        public const string PackName_VarioMaterial = "gip.VarioMaterial";
        public const string PackName_VarioScheduling = "gip.VarioScheduling";
        public const string PackName_VarioCompany = "gip.VarioCompany";
        public const string PackName_VarioFacility = "gip.VarioFacility";
        public const string PackName_VarioLogistics = "gip.VarioLogistics";
        public const string PackName_VarioPurchase = "gip.VarioPurchase";
        public const string PackName_VarioSales = "gip.VarioSales";
        public const string PackName_VarioManufacturing = "gip.VarioManufacturing";
        public const string PackName_VarioLicense = "gip.VarioLicense";
        public const string PackName_TwinCAT = "gip.TwinCAT";

        public const string LicenseManager = "BSOiPlusLicenseManager";

        public const int EF_HResult_EntityException = unchecked((int)0x80131501); //-2146233087; // EntityException
        public const int EF_HResult_InvalidOperationException = unchecked((int)0x80131509); // -2146233079 InvalidOperationException
        public const int EF_HResult_SqlException = unchecked((int)0x80131904); // -2146232060 System.Data.SqlClient.SqlException

        public const string VBLanguage = "en{'Language'}de{'Sprache'}";

        public const string From = "en{'From'}de{'Von'}";
        public const string To = "en{'To'}de{'Bis'}";

        public const string Workflow = "en{'Workflow'}de{'Workflow'}";
        public const string ProcessModule = "en{'Process Module'}de{'Prozessmodul'}";
    }


    public enum MISort : short
    {
        Okay                    = 1010,
        Cancel                  = 1011,

        New						= 1030,
        Load					= 1040,
        Modify                  = 1041,
        UnLoad                  = 1050,
        Delete					= 1060,
        Restore                 = 1061,
        DeleteSoft              = 1062,
        DeleteHard              = 1063,
        Save = 1070,
        UndoSave				= 1080,
        Search					= 1090,
        SearchConfiguration     = 1092,
        NewSearch				= 1100,
        FindAndReplace			= 1120,

        UpdateAllData			= 1210,
        NewData					= 1220,
        InsertData				= 1230,
        DeleteData				= 1240,
        AddChildData			= 1250,
        MoveDown				= 1260,
        MoveUp					= 1270,
        MovedData				= 1280,

        QueryPreviewDlg	        = 1310,
        QueryPrintDlg			= 1320,
        QueryDesignDlg			= 1330,
        QueryPreview            = 1340,
        QueryPreviewSilent      = 1342,
        QueryPrint              = 1350,
        QueryPrintSilent        = 1352,
        QueryDesign			    = 1360,

        NavigateFirst			= 1410,
        NavigatePrev			= 1420,
        NavigateNext			= 1430,
        NavigateLast			= 1440,

        WizardPrev              = 1520,
        WizardNext              = 1530,

        Start                   = 1610,
        Hold                    = 1620,
        Restart                 = 1630,
        Pause                   = 1640,
        Resume                  = 1650,
        Stop                    = 1660,
        Abort                   = 1670,
        Reset                   = 1680,

        Copy					= 2010,
        Cut						= 2020,
        Paste					= 2030,

        Undo					= 2040,
        Redo					= 2050,

        SelectAll				= 2060,
        Clear					= 2070,

        BringForward            = 2110,
        BringToFront            = 2120,
        SendBackward            = 2130,
        SendToBack              = 2140,

        AlignLeft               = 2210,
        AlignHorizontalCenters  = 2220,
        AlignRight              = 2230,
        AlignTop                = 2240,
        AlignVerticalCenters    = 2250,
        AlignBottom             = 2260,
        DistributeHorizontal    = 2270,
        DistributeVertical      = 2280,

        DesignConfiguration		= 2310,


        AssignShapeConfig       = 2340,
        PreSetShapeConfig       = 2350,
        ResetShapeConfig        = 2360,

        ComponentExplorer       = 20009, 
        ControldialogOn         = 20010,

        IPlusStudio             = 20020,
        DiagnosticdialogOn      = 20030,
        PrintSelf               = 20031,
        DisplayACUrl            = 20040,

        DesignModeOn            = 20998,
        DesignModeOff           = 20999,

    }
}

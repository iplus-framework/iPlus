using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace gip.core.datamodel
{
    public partial class Global
    {
        #region Virtual

        public const string GetVirtualMethod = "GetVirtualMethod";
        public const string GetVirtualEventArgs = "GetVirtualEventArgs";

        #endregion

        #region Bindingflags
        public static BindingFlags bfBase1 = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        public static BindingFlags bfGetPropStatic = bfBase1 | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.GetProperty;
        public static BindingFlags bfSetPropStatic = bfBase1 | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.SetProperty;

        public static BindingFlags bfGetProp = bfBase1 | System.Reflection.BindingFlags.GetProperty;
        public static BindingFlags bfSetProp = bfBase1 | System.Reflection.BindingFlags.SetProperty;

        public static BindingFlags bfInvokeMethod = bfBase1 | System.Reflection.BindingFlags.InvokeMethod;

        public static BindingFlags bfInvokeMethodStatic = bfBase1 | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static;
        #endregion

        #region ACKind
        /// <summary>
        /// Enum for Meta-Description / categorization of a class, which should be known in the iPlus-Runtime
        /// and declared as a parameter in the ACClassInfo-Attribute-Class
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACKinds'}de{'ACKinds'}", Global.ACKinds.TACEnum)]
        public enum ACKinds : short                     
        {
            TACUndefined = 0000,

            /// <summary>
            /// Custom .NET-Class which is mostly derived from ACObject and has Properties and Methods which are not right-dependant
            /// </summary>
            TACClass = 0020,

            /// <summary>
            /// For classes which handles .NET-ApplicationCommands or are used for describing Menu-Trees
            /// </summary>
            TACCommand = 0021,

            /// <summary>
            /// For derivations of common WPF-Classes (System.Windows.UIElement) which are NOT DERIVATIONS of Frameworkelement
            /// </summary>
            TACUIControl = 0022,

            /// <summary>
            /// Derivations of System.Windows.FrameworkElement or System.Windows.Control which are used in the iPlus-UI-Designer 
            /// </summary>
            TACVBControl = 0040,

            /// <summary>
            /// Classes from .NET's System-Namespace which are the base types for the CLR (bool, short, long, double...)
            /// </summary>
            TACLRBaseTypes = 0050,

            /// <summary>
            /// Primitive custom .NET-Class which doesn't need right management and is mostly not derived fom ACObject.
            /// </summary>
            TACSimpleClass = 0060,

            /// <summary>
            /// Interface, which shold be known in the iPlus-Runtime
            /// </summary>
            TACInterface = 0070,

            /// <summary>
            /// For enumerations, which should be known in the iPlus-Runtime
            /// In most cases combine it with the ACSerializable-Attribute-Class
            /// </summary>
            TACEnum = 0080,

            /// <summary>
            /// Translations for enumarations
            /// </summary>
            TACEnumACValueList = 0081,

            /// <summary>
            /// .NET-Classes, which have a abstract-keyword in the class-declaration and 
            /// should be known in the iPlus-Runtime to be able to define Designs, Configurations, Translations etc. for it
            /// </summary>
            TACAbstractClass = 0090,

            /// <summary>
            /// Data-Access-Category: Components which communicate with other system over a network or serial interface: OPC, OPC-UA, Modbus, RS232, ODBC, SOAP, REST, MQTT...
            /// </summary>
            TACDAClass = 0100,

            /// <summary>
            /// Custom .NET-Class which is mostly derived from ACObject. Similar to TACClass but needs Right-Management for it's Properties and Methods instead.
            /// </summary>
            TACObject = 0110,


            ///////////////////////////////////////////////////////////////////////
            // Application 

            /// <summary>
            /// Derivations from PAModule which are Control-Modules according ISA-88
            /// </summary>
            TPAModule = 5200,

            /// <summary>
            /// Applicationmanger
            /// </summary>
            TACApplicationManager = 5202,

            /// <summary>
            /// Derivations from PAProcessModule which are Process-Modules according ISA-88
            /// </summary>
            TPAProcessModule = 5210,

            /// <summary>
            /// Derivations from PAProcessFunction which are Operations according ISA-88
            /// </summary>
            TPAProcessFunction = 5310,

            /// <summary>
            /// Components which doesn't have a physical respresentation ("Background" Software-Module)
            /// </summary>
            TPABGModule = 5410,

            /// <summary>
            /// Derivations from PARole. Used for stateless components which are referenced/share from different components or business-object
            /// to call common or "global" methods which have implemented a common businesslogic 
            /// </summary>
            TPARole = 5510,


            ///////////////////////////////////////////////////////////////////////
            // Root

            /// <summary>
            /// ONLY INTERNAL USAGE! Represents the Root-Class which is the most top Instance in the iPlus-Runtime
            /// </summary>
            TACRoot = 0300,

            /// <summary>
            /// ONLY INTERNAL USAGE! Represents the Businessobject-Manager
            /// </summary>
            TACBusinessobjects = 0400,

            /// <summary>
            /// Businessobject. Components which are Derivations of ACBSO
            /// </summary>
            TACBSO = 0401,

            /// <summary>
            /// Businessobject. Components which are Derivations of ACBSO and are used globally
            /// </summary>
            TACBSOGlobal = 0402,

            /// <summary>
            /// Businessobject used for reporting
            /// </summary>
            TACBSOReport = 0403,

            /// <summary>
            /// ONLY INTERNAL USAGE! Represents the Queries-Manager
            /// </summary>
            TACQueries = 0600,

            /// <summary>
            /// ONLY INTERNAL USAGE! Only for class ACQueryDefinition
            /// </summary>
            TACQRY = 0601,

            /// <summary>
            /// ONLY INTERNAL USAGE! Represents a Database-Context (ObjectContext)
            /// </summary>
            TACDBAManager = 0900,

            /// <summary>
            /// Entity-Class. USe it if you define a new Entity-Framework-Class for accessing a DB-Table.
            /// </summary>
            TACDBA = 0910,

            /// <summary>
            /// ONLY INTERNAL USAGE! Only for class Communications. Root-Object for the Network-Communication between iPlus-server and clients.
            /// </summary>
            TACCommunications = 1100,

            /// <summary>
            /// ONLY INTERNAL USAGE! Only for class WCFServiceManager. Root-Object for server-side network communication.
            /// </summary>
            TACWCFServiceManager = 1110,

            /// <summary>
            /// ONLY INTERNAL USAGE! Only for class WCFServiceChannel. Represents one Client-Networkchannel on server-side.
            /// </summary>
            TACWCFServiceChannel = 1111,

            /// <summary>
            /// ONLY INTERNAL USAGE! Only for class WCFClientManager. Root-Object for client-side network communication.
            /// </summary>
            TACWCFClientManager = 1120,

            /// <summary>
            /// ONLY INTERNAL USAGE! Only for class WCFClientChannel. Represents one Client-Networkchannel on client-side.
            /// </summary>
            TACWCFClientChannel = 1121,

            /// <summary>
            /// ONLY INTERNAL USAGE! Represents the Messages-Instance for Logging and opening Message-Dialogs on GUI
            /// </summary>
            TACMessages = 1200,

            /// <summary>
            /// ONLY INTERNAL USAGE! Represents the Environment-Instance
            /// </summary>
            TACEnvironment = 1300,

            /// <summary>
            /// ONLY INTERNAL USAGE! Only for class TACRuntimeDump to dump System-Performance-Logs
            /// </summary>
            TACRuntimeDump = 1301,

            /// <summary>
            /// ONLY INTERNAL USAGE! Only for class LocalServiceObjects
            /// </summary>
            TACLocalServiceObjects = 1400,


            ///////////////////////////////////////////////////////////////////////
            // Workflows

            /// <summary>
            /// Workflow-Class which is the root-Instance of a workflow. All derivations of PWMethod must be assigned to this category.
            /// </summary>
            TPWMethod = 6200,

            /// <summary>
            /// Workflow-Class which is a group-node. All derivations of PWGroup  must be assigned to this category.
            /// </summary>
            TPWGroup = 6210,

            /// <summary>
            /// ONLY INTERNAL USAGE! Only for PWBaseExecutable, PWBaseNodeProcess
            /// </summary>
            TPWNode = 6300,

            /// <summary>
            /// Workflow-Class which is a derivation of PWBaseExecutable or PWBaseNodeProcess and doesn't invoke a function on the physical model.
            /// TRuns independantly inside a workflow.
            /// </summary>
            TPWNodeStatic = 6310,

            /// <summary>
            /// Workflow-Class which is a derivation of PWNodeProcessMethod and invokes a function on the physical model.
            /// </summary>
            TPWNodeMethod = 6320,

            /// <summary>
            /// Workflow-Class which is a derivation of PWNodeProcessWorkflow and start a sub-workflow
            /// </summary>
            TPWNodeWorkflow = 6330,

            /// <summary>
            /// Only for class PWNodeStart
            /// </summary>
            TPWNodeStart = 6380,

            /// <summary>
            /// Only for class PWNodeEnd
            /// </summary>
            TPWNodeEnd = 6390,


            ///////////////////////////////////////////////////////////////////////
            // Properties

            /// <summary>
            /// Property which is defined hardcoded in a assembly
            /// </summary>
            PSProperty = 10010,

            /// <summary>
            /// Virtual Property. Defined via iPlus development environment.
            /// If a hardcoded poperty exists in a assembly with the same name this virtual property will be invoked instaed (Works like a overridden property)
            /// </summary>
            PSPropertyExt = 10020,


            ///////////////////////////////////////////////////////////////////////
            // Methods
            
            /// <summary>
            /// Method which is defined hardcoded in a assembly
            /// </summary>
            MSMethod = 11110,

            /// <summary>
            /// ONLY INTERNAL USAGE! Method which is defined hardcoded in a assembly and declares, that pre- and postscripts will be invoked for additioanal customized logic
            /// </summary>
            MSMethodPrePost = 11111,

            /// <summary>
            /// Persisted ACMethod with Parameters. It's presented like a attached Method on a Process-Module but is referenced to a conrete Process-Function.
            /// </summary>
            MSMethodFunction = 11112,

            /// <summary>
            /// Static Method which is defined hardcoded in a assembly and is invoked only on client-side
            /// </summary>
            MSMethodClient = 11113,

            /// <summary>
            /// ONLY INTERNAL USAGE! Virtual method. Defined via iPlus development environment.
            /// If a hardcoded method exists with the same name this virtual method will be invoked instaed (Works like a overridden method)
            /// </summary>
            MSMethodExt = 11120,

            /// <summary>
            /// ONLY INTERNAL USAGE! Works like a virtual method, but more scripts are invoked in series on server-side
            /// </summary>
            MSMethodExtTrigger = 11121,

            /// <summary>
            /// ONLY INTERNAL USAGE! Works like a virtual method, but more scripts are invoked in series on client-side
            /// </summary>
            MSMethodExtClient = 11122,

            /// <summary>
            /// ONLY INTERNAL USAGE! Workflow which acts as a virtual method which can be invoked.
            /// </summary>
            MSWorkflow = 11130,


            ///////////////////////////////////////////////////////////////////////
            // Designs

            /// <summary>
            /// ONLY INTERNAL USAGE! Design/XAML-Layouts for GUI
            /// </summary>
            DSDesignLayout = 12010,

            /// <summary>
            /// ONLY INTERNAL USAGE! Design/XAML-Layouts for Menus
            /// </summary>
            DSDesignMenu = 12020,

            /// <summary>
            /// ONLY INTERNAL USAGE! Design/XAML for Reports
            /// </summary>
            DSDesignReport = 12030,

            /// <summary>
            /// ONLY INTERNAL USAGE! Stored BMP/JPG/PNG-Image in database
            /// </summary>
            DSBitmapResource = 12040,
        }

        static ACValueItemList _ACKindList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList ACKindList
        {
            get
            {
                if (Global._ACKindList == null)
                {
                    Global._ACKindList = new ACValueItemList(Const.ACKindIndex);
                    Global._ACKindList.AddEntry((short)ACKinds.TACAbstractClass, "en{'Abstract class'}de{'Abstrakte Klasse'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACClass, "en{'Class without right-management'}de{'Klasse ohne Rechteverwaltung'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACUIControl, "en{'UI-Class'}de{'UI-Klasse'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACVBControl, "en{'UI-Control'}de{'UI-Steuerelement'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACLRBaseTypes, "en{'CLR-Type'}de{'CLR-Typ'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACSimpleClass, "en{'Primitive class'}de{'Primitive Klasse'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACInterface, "en{'Interface'}de{'Schnittstelle'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACEnum, "en{'Enumeration'}de{'Enumeration'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACEnumACValueList, "en{'Translation of Enumeration'}de{'Enumerationsübersetzung'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACCommand, "en{'Command'}de{'Befehl'}");

                    #region Root
                    Global._ACKindList.AddEntry((short)ACKinds.TACRoot, "en{'Root'}de{'Root'}");

                    Global._ACKindList.AddEntry((short)ACKinds.TACBusinessobjects, "en{'Businessobject-Manager'}de{'Geschäftsobjekt-Manager'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACBSO, "en{'Businessobject'}de{'Geschäftsobjekt'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACBSOGlobal, "en{'Businessobject global'}de{'Geschäftsobjekt global'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACBSOReport, "en{'BSO for Report'}de{'Geschäftsobjekt Bericht'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACDBAManager, "en{'Database'}de{'Datenbank'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACDBA, "en{'Entity/Table'}de{'Entität/Tabelle'}");

                    Global._ACKindList.AddEntry((short)ACKinds.TACEnvironment, "en{'Environment'}de{'Umgebung'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACLocalServiceObjects, "en{'Local Service Objects'}de{'Lokale Serviceobjekte'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACRuntimeDump, "en{'System-Dump'}de{'System-Dump'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACMessages, "en{'Messages'}de{'Meldungen'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACQueries, "en{'Queries'}de{'Abfragen'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACQRY, "en{'Query'}de{'Abfrage'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACCommunications, "en{'Network-Manager'}de{'Netzwerk-Manager'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACWCFServiceManager, "en{'Network-Service-Manager'}de{'Netzwerk-Service-Manager'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACWCFServiceChannel, "en{'Network-Servicechannel'}de{'Netzwerk Servicekanal'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACWCFClientManager, "en{'Network-Client-Manager'}de{'Netzwerk-Client-Manager'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACWCFClientChannel, "en{'Network-Clientchannel'}de{'Netzwerk Clientkanal''}");

                    Global._ACKindList.AddEntry((short)ACKinds.TPWMethod, "en{'Workflow-Root'}de{'Workflow-Wurzel'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPWGroup, "en{'Workflow-Group'}de{'Workflow-Gruppe'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPWNode, "en{'Workflow-Node'}de{''Workflow-Knoten'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPWNodeMethod, "en{'WF Function invoker'}de{'WF Funktionsaufrufer'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPWNodeWorkflow, "en{'WF Subworkflow starter'}de{'WF Unterworkflowstarter'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPWNodeStart, "en{'Workflow-Start-Node'}de{'Workflow-Startknoten'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPWNodeEnd, "en{'Workfow-End-Node'}de{'Workflow-Endeknoten'}");
                    #endregion

                    Global._ACKindList.AddEntry((short)ACKinds.TACDAClass, "en{'Communication class'}de{'Kommuinikationsklasse'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TACObject, "en{'Class with right-management'}de{'Klasse mit Rechteverwaltung'}");

                    Global._ACKindList.AddEntry((short)ACKinds.TACApplicationManager, "en{'Applicationmanager'}de{'Anwendungsmanager'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPAModule, "en{'Control-Module'}de{'Steuerungsmodul'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPAProcessModule, "en{'Process-Module'}de{'Prozessmodul'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPAProcessFunction, "en{'Function'}de{'Funktion'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPABGModule, "en{'Software-Module in Background)'}de{'Softwaremodul im Hintergrund)'}");
                    Global._ACKindList.AddEntry((short)ACKinds.TPARole, "en{'Role'}de{'Rolle'}");

                    Global._ACKindList.AddEntry((short)ACKinds.PSProperty, "en{'Property'}de{'Eigenschaft'}");
                    Global._ACKindList.AddEntry((short)ACKinds.PSPropertyExt, "en{'Virtual Property'}de{'Virtuelle Eigenschaft'}");

                    Global._ACKindList.AddEntry((short)ACKinds.MSMethod, "en{'Method'}de{'Methode'}");
                    Global._ACKindList.AddEntry((short)ACKinds.MSMethodPrePost, "en{'Pre/Post-Method'}de{'Pre/Post-Methode'}");
                    Global._ACKindList.AddEntry((short)ACKinds.MSMethodFunction, "en{'Attached virtual method'}de{'Angehängte virtuelle Methode'}");
                    Global._ACKindList.AddEntry((short)ACKinds.MSMethodClient, "en{'Method on Client-Side'}de{'Clientseitige Methode'}");


                    Global._ACKindList.AddEntry((short)ACKinds.MSMethodExt, "en{'Virtual method'}de{'Virtuelle methode'}");
                    Global._ACKindList.AddEntry((short)ACKinds.MSMethodExtTrigger, "en{'Server-side trigger method'}de{'Serverseitige Triggermethode'}");
                    Global._ACKindList.AddEntry((short)ACKinds.MSMethodExtClient, "en{'Client-side trigger method'}de{'Clientseitige Triggermethode'})");

                    Global._ACKindList.AddEntry((short)ACKinds.MSWorkflow, "en{'Workflow'}de{'Workflow'}");

                    Global._ACKindList.AddEntry((short)ACKinds.DSDesignLayout, "en{'UI-Design'}de{'UI-Design'}");
                    Global._ACKindList.AddEntry((short)ACKinds.DSDesignMenu, "en{'UI-Menu'}de{'UI-Menü'}");
                    Global._ACKindList.AddEntry((short)ACKinds.DSDesignReport, "en{'Report'}de{'Bericht'}");
                    Global._ACKindList.AddEntry((short)ACKinds.DSBitmapResource, "en{'Image'}de{'Bild'}");

                    Global._ACKindList.AddEntry((short)ACKinds.TACUndefined, "en{'Undefined'}de{'Undefiniert'}");
                }
                return Global._ACKindList;
            }
        }

        public class KindInfo
        {
            public string ACCaption { get; set; }
            public ACKinds RangeFrom { get; set; }
            public ACKinds RangeTo { get; set; }
        }

        static List<KindInfo> _KindInfoList;
        static public List<KindInfo> KindInfoList
        {
            get
            {
                if (_KindInfoList == null)
                {
                    _KindInfoList = new List<KindInfo>();
                    _KindInfoList.Add(new KindInfo { ACCaption = "VB Class", RangeFrom = ACKinds.TACClass, RangeTo = ACKinds.TACClass });
                    _KindInfoList.Add(new KindInfo { ACCaption = "VB Control", RangeFrom = ACKinds.TACUIControl	, RangeTo = ACKinds.TACVBControl	});

                    _KindInfoList.Add(new KindInfo { ACCaption = "VB Simple Type", RangeFrom = ACKinds.TACLRBaseTypes, RangeTo = ACKinds.TACLRBaseTypes });
                    _KindInfoList.Add(new KindInfo { ACCaption = "VB Simple Class", RangeFrom = ACKinds.TACSimpleClass, RangeTo = ACKinds.TACSimpleClass });
                    _KindInfoList.Add(new KindInfo { ACCaption = "VB Interface", RangeFrom = ACKinds.TACInterface, RangeTo = ACKinds.TACInterface });
                    _KindInfoList.Add(new KindInfo { ACCaption = "VB Enum", RangeFrom = ACKinds.TACEnum, RangeTo = ACKinds.TACEnumACValueList });
                    _KindInfoList.Add(new KindInfo { ACCaption = "VB Class (Abstract)", RangeFrom = ACKinds.TACAbstractClass, RangeTo = ACKinds.TACAbstractClass });
                    _KindInfoList.Add(new KindInfo { ACCaption = "AC Objects", RangeFrom = ACKinds.TACDAClass, RangeTo = ACKinds.TACObject });
                    _KindInfoList.Add(new KindInfo { ACCaption = "VB Query", RangeFrom = ACKinds.TACQueries, RangeTo = ACKinds.TACQRY });

                    _KindInfoList.Add(new KindInfo { ACCaption = "PA Module", RangeFrom = ACKinds.TPAModule, RangeTo = ACKinds.TACApplicationManager });
                    _KindInfoList.Add(new KindInfo { ACCaption = "PA Process Module", RangeFrom = ACKinds.TPAProcessModule		, RangeTo = ACKinds.TPAProcessModule});
                    _KindInfoList.Add(new KindInfo { ACCaption = "PA Process Function", RangeFrom = ACKinds.TPAProcessFunction, RangeTo = ACKinds.TPAProcessFunction });
                    _KindInfoList.Add(new KindInfo { ACCaption = "PA Background Module", RangeFrom = ACKinds.TPABGModule, RangeTo = ACKinds.TPABGModule });
                    _KindInfoList.Add(new KindInfo { ACCaption = "PA Role", RangeFrom = ACKinds.TPARole, RangeTo = ACKinds.TPARole });

                    _KindInfoList.Add(new KindInfo { ACCaption = "PW Program", RangeFrom = ACKinds.TPWMethod, RangeTo = ACKinds.TPWMethod });
                    _KindInfoList.Add(new KindInfo { ACCaption = "PW Workflownode", RangeFrom = ACKinds.TPWGroup, RangeTo = ACKinds.TPWNodeEnd });
                }
                return _KindInfoList;
            }
        }
        #endregion

        #region ACUsage
        /// <summary>
        /// Enum für das Feld ACUsageIndex
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'Design-Usage'}de{'Design-Verwendungszweck'}", Global.ACKinds.TACEnum)]
        public enum ACUsages : short
        {
            /// <summary>
            /// Not defined
            /// </summary>
            DUUndefined     = 0,

            /// <summary>
            /// Main view, which is loaded first from VBDesign when Businessobject is opened. Main views consist of many partial views (DULayout) 
            /// </summary>
            DUMain = 4200,

            /// <summary>
            /// Partial view, which is loaded when a VBDesign is presented an the VBContent points to this partail view (XAML-layout)
            /// </summary>
            DULayout        = 4210,

            /// <summary>
            /// XAML-Design for a Control (Presentation of ac ACComponent in a Visualisation).
            /// It's loaded by a VBVisual-Control
            /// </summary>
            DUControl       = 4220,

            /// <summary>
            /// XAML-Design which is loaded, when the Control-Dialog wil be opened
            /// </summary>
            DUControlDialog = 4230,

            /// <summary>
            /// XAML-Design for a Visualisation-Page which is selected in BSOVisualisationStudio
            /// </summary>
            DUVisualisation = 4250,

            /// <summary>
            /// Icon for Buttons, Menus or Trees
            /// </summary>
            DUIcon          = 4260,

            /// <summary>
            /// Bitmap
            /// </summary>
            DUBitmap        = 4270,

            /// <summary>
            /// Binary stream for custom designs, which are stored in another GUI-Language
            /// </summary>
            DUBinary        = 4280,

            /// <summary>
            /// Diagnostic dialog
            /// </summary>
            DUDiagnostic    = 4290,

            /// <summary>
            /// Main menu
            /// </summary>
            DUMainmenu      = 5280,

            /// <summary>
            /// List&Label Report: List of Master-Detail (Head + Lines)
            /// </summary>
            DULLReport        = 6300,

            /// <summary>
            /// List&Label Report: List of Masters (Only Head)
            /// </summary>
            DULLOverview = 6310,

            /// <summary>
            /// List&Label Report: List of Lines
            /// </summary>
            DULLList = 6320,

            /// <summary>
            /// List&Label Report: Label with Master-Data (Head)
            /// </summary>
            DULLLabel = 6330,

            /// <summary>
            /// List&Label Filecard
            /// </summary>
            DULLFilecard = 6340,

            /// <summary>
            /// Report as XAML and Flowdocument
            /// </summary>
            DUReport = 6400,
        }

        static ACValueItemList _ACUsageList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList ACUsageList
        {
            get
            {
                if (Global._ACUsageList == null)
                {
                    Global._ACUsageList = new ACValueItemList("ACUsageIndex");
                    // Der Originaltext ist immer in Englisch
                    Global._ACUsageList.AddEntry((short)ACUsages.DUMain, "en{'Main view'}de{'Hauptansicht'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DULayout, "en{'Partial view'}de{'Teilansicht'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DUControl, "en{'Control'}de{'Steuerelement'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DUControlDialog, "en{'Control dialog'}de{'Steuerungsdialog'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DUVisualisation, "en{'Visualisation'}de{'Visualisierung'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DUIcon, "en{'Icon'}de{'Ikone'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DUBitmap, "en{'Bitmap'}de{'Bitmap'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DUBinary, "en{'Binary Stream'}de{'Binärer Datenstrom'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DUDiagnostic, "en{'Diagnostic Dialog'}de{'Diagnosedialog'}");

                    Global._ACUsageList.AddEntry((short)ACUsages.DUMainmenu, "en{'Main menu'}de{'Hauptmenü'}");

                    Global._ACUsageList.AddEntry((short)ACUsages.DUReport, "en{'XAML Report'}de{'XAML Bericht'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DULLReport, "en{'L&L Report'}de{'L&L Bericht'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DULLOverview, "en{'L&L Overview'}de{'L&L Überblick'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DULLList, "en{'L&L List'}de{'L&L Liste'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DULLLabel, "en{'L&L Label'}de{'L&L Etikett'}");
                    Global._ACUsageList.AddEntry((short)ACUsages.DULLFilecard, "en{'L&L Filecard'}de{'L&L Dateikarte'}");
                }
                return Global._ACUsageList;
            }
        }
        #endregion

        #region ACProjectTypes
        /// <summary>
        /// Enum für das Feld MDClassTypeIndex
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACProjectTypes'}de{'ACProjectTypes'}", Global.ACKinds.TACEnum)]
        public enum ACProjectTypes : short
        {
            Root = 1,
            Application = 2,
            AppDefinition = 3,
            Service = 4,
            ClassLibrary = 100,
        }
       
        static ACValueItemList _ACProjectTypeList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList ACProjectTypeList
        {
            get
            {
                if (Global._ACProjectTypeList == null)
                {
                    Global._ACProjectTypeList = new ACValueItemList("ACProjectTypeIndex");
                    Global._ACProjectTypeList.AddEntry((short)ACProjectTypes.Root, "en{'Root-Project'}de{'Root-Projekt'}");
                    Global._ACProjectTypeList.AddEntry((short)ACProjectTypes.Application, "en{'Application-Project'}de{'Anwendungs-Projekt'}");
                    Global._ACProjectTypeList.AddEntry((short)ACProjectTypes.AppDefinition, "en{'Application-Definition-Project'}de{'Anwendungsdefinitions-Projekt'}");
                    Global._ACProjectTypeList.AddEntry((short)ACProjectTypes.ClassLibrary, "en{'Classlibrary-Project'}de{'Klassenbibliothek-Projekt'}");
                    Global._ACProjectTypeList.AddEntry((short)ACProjectTypes.Service, "en{'Service-Project'}de{'Service-Projekt'}");
                }
                return Global._ACProjectTypeList;
            }
        }
        #endregion

        #region ACStartTypes
        /// <summary>
        /// Enum für das Feld MDClassTypeIndex
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACStartTypes'}de{'ACStartTypes'}", Global.ACKinds.TACEnum)]
        public enum ACStartTypes : short
        {
            /// <summary>
            /// Not defined
            /// </summary>
            None = 0,

            /// <summary>
            /// ACComponent will be started during startup
            /// </summary>
            Automatic = 1,

            /// <summary>
            /// ACComponent starts as soon as it is accessed for the first time via ACUrl
            /// </summary>
            AutomaticOnDemand = 2,

            /// <summary>
            /// ACComponent must be started manually by calling the StartComponent-Method
            /// </summary>
            Manually = 3,

            /// <summary>
            /// ACComponent is disabled and can't be started
            /// </summary>
            Disabled = 4,
        }

        static ACValueItemList _ACStartTypeList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList ACStartTypeList
        {
            get
            {
                if (Global._ACStartTypeList == null)
                {
                    Global._ACStartTypeList = new ACValueItemList("ACStartTypeIndex");
                    Global._ACStartTypeList.AddEntry((short)ACStartTypes.None, "en{'Not defined'}de{'Nicht definiert'}");
                    Global._ACStartTypeList.AddEntry((short)ACStartTypes.Automatic, "en{'Automatic (on startup)'}de{'Automatisch (Hochfahren)'}");
                    Global._ACStartTypeList.AddEntry((short)ACStartTypes.AutomaticOnDemand, "en{'On Demand'}de{'Bei Bedarf'}");
                    Global._ACStartTypeList.AddEntry((short)ACStartTypes.Manually, "en{'Manually'}de{'Manuell'}");
                    Global._ACStartTypeList.AddEntry((short)ACStartTypes.Disabled, "en{'Disabled'}de{'Deaktiviert'}");
                }
                return Global._ACStartTypeList;
            }
        }

        #endregion

        #region ACTypeGroups
        /// <summary>
        /// Enum für das Feld MDClassTypeIndex
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACTypeGroups'}de{'ACTypeGroups'}", Global.ACKinds.TACEnum)]
        public enum ACTypeGroups : short
        {
            TGACProject = 1,
            TGWorkOrderMethod = 2,
            TGACBSO = 3,
            TGACBase = 4,
        }
        #endregion

        #region ACPropUsages
        /// <summary>
        /// Describes the Usage of a Property
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'Usage of a Property'}de{'Eigenschafts-Verwendungszweck'}", Global.ACKinds.TACEnum)]
        public enum ACPropUsages : short
        {
            /// <summary>
            /// Current-Property of a BSO. Belongs to a Property-Group which relates "Selected", "List" and "Access"-Properties.
            /// </summary>
            Current = 10,

            /// <summary>
            /// Selected-Property of a BSO. Belongs to a Property-Group which relates "Current", "List" and "Access"-Properties.
            /// </summary>
            Selected = 20,

            /// <summary>
            /// List-Property of a BSO. Belongs to a Property-Group which relates "Current", "Selected" and "Access"-Properties.
            /// </summary>
            List = 30,

            /// <summary>
            /// Stand-Alone Property which doesn't belong to a Property-Group
            /// </summary>
            Property = 40,

            /// <summary>
            /// Property that represents a physical (real) Point which can relate other physical (real) Points to describe a physical route
            /// </summary>
            ConnectionPoint = 60,

            /// <summary>
            /// Property that represents a logical Point which can relate other logical Points to describe a logical relation (e.g. OutOrder->OutOrderPos)
            /// </summary>
            RelationPoint = 61,

            /// <summary>
            /// Property that represents an network-capable event 
            /// </summary>
            EventPoint = 62,

            /// <summary>
            /// Property that represents an network-capable Event-Subscription
            /// </summary>
            EventPointSubscr = 63,

            /// <summary>
            /// Property that represents an network-capable asynchronous method
            /// </summary>
            AsyncMethodPoint = 64,

            /// <summary>
            /// Property that represents an network-capable subscription for asynchronous method
            /// </summary>
            AsyncMethodPointSubscr = 65,

            /// <summary>
            /// Property for describing the datatype which is stored in a ACClassConfig-Entry
            /// </summary>
            ConfigPointConfig = 67,

            /// <summary>
            /// Property for describing the datatype which is stored in a ACClassConfig-Entry as a List
            /// </summary>
            ConfigPointProperty = 68,

            /// <summary>
            /// Property which contains a List of IACPointReference
            /// </summary>
            ReferenceListPoint = 69,

            /// <summary>
            /// INTERNAL: Represents a Root-Object in a Tree
            /// </summary>
            ChangeInfo = 70,

            /// <summary>
            /// AccessPrimary-Property of a BSO. Belongs to a Property-Group which relates "Current", "Selected" and "List"-Properties.
            /// </summary>
            AccessPrimary = 80,

            /// <summary>
            /// Access-Property of a BSO. Belongs to a Property-Group which relates "Current", "Selected" and "List"-Properties.
            /// </summary>
            Access = 81,

            /// <summary>
            /// Property for storing simple Configuration-Values for a ACComponent-Instance via ACPropertyConfigValue
            /// </summary>
            Configuration = 90,
        }

        static ACValueItemList _ACPropUsageList = null;
        /// <summary>
        /// Describes the Usage of a Property as ACValueItemList
        /// </summary>
        static public ACValueItemList ACPropUsageList
        {
            get
            {
                if (Global._ACPropUsageList == null)
                {
                    Global._ACPropUsageList = new ACValueItemList("ACPropUsageIndex");
                    // Der Originaltext ist immer in Englisch
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.Current, "en{'Current'}de{'Aktuell'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.Selected, "en{'Selected'}de{'Selektiert'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.List, "en{'List'}de{'Liste'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.Property, "en{'Stand-Alone'}de{'Eigenständig'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.ConnectionPoint, "en{'Physical connection point'}de{'Physikalischer Verbindungspunkt'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.RelationPoint, "en{'Logical connection point'}de{'Logischer Verbindungspunkt'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.EventPoint, "en{'Event'}de{'Ereignis'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.EventPointSubscr, "en{'Event subscriber'}de{'Ereignis-Abonnement'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.AsyncMethodPoint, "en{'Asynchronous method'}de{'Asynchrone Methode'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.AsyncMethodPointSubscr, "en{'Subscription of asynchronous method'}de{'Abonnement einer asynchronen Methode'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.ConfigPointConfig, "en{'Configuration of association'}de{'Konfiguration von Beziehungen'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.ConfigPointProperty, "en{'Configuration-List'}de{'Konfigurationsliste'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.ReferenceListPoint, "en{'List of IACPointReference'}de{'Liste von IACPointReference'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.ChangeInfo, "en{'ChangeInfo (Internal)'}de{'ChangeInfo (Intern)'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.AccessPrimary, "en{'Result of a primary database query'}de{'Resultat einer primären Datenbankabfrage'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.Access, "en{'Result of a database query'}de{'Resultat einer Datenbankabfrage'}");
                    Global._ACPropUsageList.AddEntry((short)ACPropUsages.Configuration, "en{'Configuration value'}de{'Konfigurationswert'}");
                }
                return Global._ACPropUsageList;
            }
        }

        #endregion

        #region ACStateCommands
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACStateCommands'}de{'ACStateCommands'}", Global.ACKinds.TACEnum)]
        public enum ACStateCommands : short
        {
            Start = 10,
            Hold = 20,
            Restart = 30,
            Pause = 40,
            Resume = 50,
            Stop = 60,
            Abort = 70,
            Reset = 80
        }
        #endregion

        #region ACMethodResultState
        /// <summary>Enum for describing the state or result of an asynchronus or synchronous virtual method invocation (ACMethod) (e.g. Starting a ProcessFunction).</summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMethodResultState'}de{'ACMethodResultState'}", Global.ACKinds.TACEnum)]
        public enum ACMethodResultState : short
        {
            /// <summary>
            /// The incovation was sucessful and the process is running now.
            /// </summary>
            InProcess = 1,

            /// <summary>
            /// The asynchronus process has been sucessfully completed.
            /// </summary>
            Succeeded = 2,

            /// <summary>
            /// The invocation of the method failed. The problem should be solved first before you call the method again.
            /// </summary>
            Failed = 3,

            /// <summary>
            /// The method can't be executed. Maybe some parameters are wrong. Don't repeat the invocation again, because it will fail again.
            /// </summary>
            Notpossible = 4,

            /// <summary>
            /// The function or service is temporary not usable or something unexpected happend. Try to invoke the method again.
            /// </summary>
            FailedAndRepeat = 5,
        }
        #endregion

        #region ACTaskTypes
        /// <summary>
        /// Mit ACTaskTypes werden die Einträge in der Tabelle ACClassTask.ACTaskTypeIndex typisiert
        /// In Verbindung mit der Eigenschaft ACClassTask.IsDynamic und dem ACClassTask.ACStateIndex 
        /// wird beim entladen der ACComponent entschieden, ob der ACClassTask-Datensatz gelöscht wird. 
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACTaskTypes'}de{'ACTaskTypes'}", Global.ACKinds.TACEnum)]
        public enum ACTaskTypes : short
        {
            RootTask = 10,    // Task anhand einer ACRoot-ACComponent
                                    // Hier existieren nur ACClassTask, die IsDynamic=false sind und im 
                                    // Rahmen des BSOiPlusStudio angelegt und gelöscht werden.
                                    // Es werden grundsätzlich keine Informationen in ACClassTask u. ACClassTaskProperty gespeichert

            ModelTask = 20,         // Task anhand einer Model-ACComponent
                                    // Dies sind Model-Bezogene ACClassTask, welche in permanente (IsDynamic=false ,Definition im BSOiPlusStudio)
                                    // und temporäre (IsDynamic=true) Prozesse, welche vorübergehend von einer Model-ACComponent erzeugt werden, 
                                    // zu unterscheiden sind. 
            
            MethodTask = 30,        // Root-Task der sich auf eine ACMethod bezieht, je nach Implementierung
                                    // kann eine Assemblymthode, Scriptmethode oder Workflowmethode aufgerufen werden
                                    // Im Falle eines Workflows, ist dieser ACClassTask, der Content des Root-ACComponentWF
                                    // Auch hier kann zwischen permanenten und temporären Prozessen unterschieden werden.

            WorkflowTask = 40       // Dieser Typ befindet sich unterhalb eines "MethodTask" beim Fall eines Workflows 
        }
        #endregion

        #region ACProgramTypes
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACProgramTypes'}de{'ACProgramTypes'}", Global.ACKinds.TACEnum)]
        public enum ACProgramTypes : short
        {
            Precompiled = 10,    
            // Entsteht im Stammdatenbereich, wenn der "Programm-Test" bzw. Vorkompilierung ausgeführt wird

            CompiledTest = 20,   
            // Entsteht im Bewegungsdatenbereich, wenn anwendungsbezogene Entitäten (z.B. Produktionsaufträge, Warenannahmen, Lieferungen...)
            // mit einem ACMethod-Aufruf kombiniert werden und "Testlauf" bzw. eine "Compilierung" durchgeführt wird.
            // - wenn ein direkter Aufruf auf der statischen "Model-/Projekt"-Welt zu Testzwecken ausgeführt wird

            CompiledSimulation = 30,
            // Entsteht im Bewegungsdatenbereich, 
            // - wenn anwendungsbezogene Entitäten (Produktionsaufträge, Warenannahmen, Lieferungen...)
            // mit einem ACMethod-Aufruf kombiniert werden und anschliessend für einen Simulationslauf "compiliert" und ausgeführt wird.
            // - wenn ein direkter Aufruf auf der statischen "Model-/Projekt"-Welt zu Simulationszwecken ausgeführt wird

            CompiledExecutable = 40
            // Entsteht im Bewegungsdatenbereich, wenn anwendungsbezogene Entitäten (Produktionsaufträge, Warenannahmen, Lieferungen...)
            // mit einem ACMethod-Aufruf kombiniert werden und anschliessend "compiliert" und ausgeführt wird.
            // - wenn ein direkter Aufruf auf der statischen "Model-/Projekt"-Welt ausgeführt wird
        }
        #endregion

        #region ACStorableTypes
        /// <summary>
        /// Definition der Speicherung von ACComponent und Eigenschaften in die Tabellen ACClassTask und ACClassTaskValue
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACStorableTypes'}de{'ACStorableTypes'}", Global.ACKinds.TACEnum)]
        public enum ACStorableTypes : short
        {
            /// <summary>
            /// Members of a ACComponent will not be persisted, because no ACClassTask-Entry will be created in Database
            /// </summary>
            NotStorable = 10,

            /// <summary>
            /// The creation of a will ACClassTask-Entry only take place once the first storage request event occurs.
            /// </summary>
            Optional = 20,

            /// <summary>
            /// The creation of ACClassTask-Entry takes place as soon as the instance is instantiated for the first time.
            /// </summary>
            Required = 30,
        }


        static ACValueItemList _ACStorableTypeList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList ACStorableTypeList
        {
            get
            {
                if (Global._ACStorableTypeList == null)
                {
                    Global._ACStorableTypeList = new ACValueItemList("ACStorableTypeIndex");
                    Global._ACStorableTypeList.AddEntry((short)ACStorableTypes.NotStorable, "en{'Not Persistable'}de{'Nicht Speicherbar'}");
                    Global._ACStorableTypeList.AddEntry((short)ACStorableTypes.Optional, "en{'Optional'}de{'Optional'}");
                    Global._ACStorableTypeList.AddEntry((short)ACStorableTypes.Required, "en{'Persistable'}de{'Speicherbar'}");
                }
                return Global._ACStorableTypeList;
            }
        }

        #endregion

        #region ACMotionType
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMotionType'}de{'ACMotionType'}", Global.ACKinds.TACEnum)]
        public enum ACMotionType : short
        {
            Continuous = 0,     // Kontinuierlich 
            One = 2,            // Komplett
            Many = 3,           // Mehrfach
        }
        #endregion

        #region OperatingMode
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'OperatingMode'}de{'OperatingMode'}", Global.ACKinds.TACEnum)]
        public enum OperatingMode : short
        {
            Automatic = 0,
            Manual = 1,
            Maintenance = 2,
            Inactive = 9
        }
        #endregion

        #region VBDesignContainer
        /// <summary>
        /// Enum für das Feld ACDesignContainerType
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDesignContainer'}de{'VBDesignContainer'}", Global.ACKinds.TACEnum)]
        public enum VBDesignContainer : short
        {
            TabItem = 0, // Als Tabitem im Docking-Manager der aktuellen Docking-Manager-ebene
            DockableWindow = 1, // Als dock- und dragbares-bares Fenster des aktuellen Docking-Managers
            ModalDialog = 2, // Als modaler Dialog
        }

        static ACValueItemList _VBDesignContainerEnumList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList VBDesignContainerIndexList
        {
            get
            {
                if (Global._VBDesignContainerEnumList == null)
                {

                    Global._VBDesignContainerEnumList = new ACValueItemList("VBDesignContainerIndex");
                    // Der Originaltext ist immer in Englisch
                    Global._VBDesignContainerEnumList.AddEntry((short)VBDesignContainer.TabItem, "Tabitem");
                    Global._VBDesignContainerEnumList.AddEntry((short)VBDesignContainer.DockableWindow, "Dockable window");
                    Global._VBDesignContainerEnumList.AddEntry((short)VBDesignContainer.ModalDialog, "Modal dialog");
                }
                return Global._VBDesignContainerEnumList;
            }
        }
        #endregion

        #region VBDesignDockState
        /// <summary>
        /// Enum für das Feld DockState
        /// Bezieht sich auf Objekte, die ACDesignContainerType.DockableWindow sind
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDesignDockState'}de{'VBDesignDockState'}", Global.ACKinds.TACEnum)]
        public enum VBDesignDockState : short
        {
            AutoHideButton = 0, // Als Auto-Hide-Button
            Docked = 1,         // Als gedocktes Fenster
            Tabbed = 2,         // Als "getabbtes" Fesnter im Docking-Manager
            FloatingWindow = 3, // Als geöffnetes Fenster
        }

        static ACValueItemList _VBDesignDockStateEnumList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList VBDesignDockStateIndexList
        {
            get
            {
                if (Global._VBDesignDockStateEnumList == null)
                {

                    Global._VBDesignDockStateEnumList = new ACValueItemList("VBDesignDockStateIndex");
                    // Der Originaltext ist immer in Englisch
                    Global._VBDesignDockStateEnumList.AddEntry((short)VBDesignDockState.AutoHideButton, "Autohide-Button");
                    Global._VBDesignDockStateEnumList.AddEntry((short)VBDesignDockState.Docked, "Docked");
                    Global._VBDesignDockStateEnumList.AddEntry((short)VBDesignDockState.Tabbed, "Tabbed");
                    Global._VBDesignDockStateEnumList.AddEntry((short)VBDesignDockState.FloatingWindow, "FloatingWindow");
                }
                return Global._VBDesignDockStateEnumList;
            }
        }
        #endregion

        #region VBDesignDockPosition
        /// <summary>
        /// Enum für das Feld ACDesignDockOrientation
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDesignDockPosition'}de{'VBDesignDockPosition'}", Global.ACKinds.TACEnum)]
        public enum VBDesignDockPosition : short
        {
            // Zusammenfassung:
            //     Ein untergeordnetes Element, das auf der linken Seite des System.Windows.Controls.DockPanel
            //     positioniert wird.
            Left = 0,
            //
            // Zusammenfassung:
            //     Ein untergeordnetes Element, das am oberen Rand des System.Windows.Controls.DockPanel
            //     positioniert wird.
            Top = 1,
            //
            // Zusammenfassung:
            //     Ein untergeordnetes Element, das auf der rechten Seite des System.Windows.Controls.DockPanel
            //     positioniert wird.
            Right = 2,
            //
            // Zusammenfassung:
            //     Ein untergeordnetes Element, das am unteren Rand des System.Windows.Controls.DockPanel
            //     positioniert wird.
            Bottom = 3,
        }

        static ACValueItemList _VBDesignDockPositionEnumList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList VBDesignDockPositionIndexList
        {
            get
            {
                if (Global._VBDesignDockPositionEnumList == null)
                {

                    Global._VBDesignDockPositionEnumList = new ACValueItemList("VBDesignDockPositionIndex");
                    // Der Originaltext ist immer in Englisch
                    Global._VBDesignDockPositionEnumList.AddEntry((short)VBDesignDockPosition.Left, "Left");
                    Global._VBDesignDockPositionEnumList.AddEntry((short)VBDesignDockPosition.Top, "Top");
                    Global._VBDesignDockPositionEnumList.AddEntry((short)VBDesignDockPosition.Right, "Right");
                    Global._VBDesignDockPositionEnumList.AddEntry((short)VBDesignDockPosition.Bottom, "Bottom");
                }
                return Global._VBDesignDockPositionEnumList;
            }
        }
        #endregion

        #region CurrentOrList
        /// <summary>
        /// Enum für das Feld CurrentOrList
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'CurrentOrList'}de{'CurrentOrList'}", Global.ACKinds.TACEnum)]
        public enum CurrentOrList : short
        {
            Current = 1,
            List = 2
        }
        #endregion

        #region ConnectionType
        /// <summary>
        /// Usage of the connection
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ConnectionTypes'}de{'ConnectionTypes'}", Global.ACKinds.TACEnum)]
        public enum ConnectionTypes : short
        {
            /// <summary>
            /// Common Type when different Types must not be differentiated.
            /// E.g. in Material-Workflows only one ConnectionTypes makes sense.
            /// </summary>
            Connection = 10,

            /// <summary>
            /// Used in PAPoints for describing physical paths between components.
            /// ConnectionPhysical-Relations are defined in the Development-Environment in tab "Connections".
            /// </summary>
            ConnectionPhysical = 11,

            /// <summary>
            /// Describes the logical relationship between PAPoints and Properties to able to determine if a PAPoint is active when the state aof one or more Properties changes.
            /// PointState-Relations are created when Assemblies are anlayzed at startup by reading the ACPointStateInfo-Attribute-Class.
            /// </summary>
            PointState = 12,

            /// <summary>
            /// Relationship which is generated by program code.
            /// </summary>
            DynamicConnection = 20,

            /// <summary>
            /// Used for relations (Edges) in Workflows for AND-Logic.
            /// StartTrigger-Relations are created in the Workflow-Editor for ACClassWFEdge-entries.
            /// </summary>
            StartTrigger = 30,

            /// <summary>
            /// Used for relations (Edges) in Workflows for OR-Logic.
            /// StartTriggerDirect-Relations are created in the Workflow-Editor for ACClassWFEdge-entries.
            /// </summary>
            StartTriggerDirect = 31,

            /// <summary>
            /// A logical bridge is a relation between PAPoints of a Process-Module and a Process-function.
            /// LogicalBridge-Relations are defined in the Development-Environment in tab "Connections".
            /// </summary>
            LogicalBridge = 50,

            /// <summary>
            /// Relationship between a Source-Property and a Target-Property.
            /// Binding-Relations are defined in the Development-Environment in tab "Bindings"
            /// </summary>
            Binding = 100
        }
        #endregion

        #region Direction
        /// <summary>
        /// Richtung von Beziehungen
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'Directions'}de{'Directions'}", Global.ACKinds.TACEnum)]
        public enum Directions : short
        {
            None = 0,
            Forward = 1,            
            Backward = 2,     
            Both = Forward | Backward
        }
        #endregion

        #region ConfigurationType
        /// <summary>
        /// Enum für das Feld ConfigurationType
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ConfigurationType'}de{'ConfigurationType'}", Global.ACKinds.TACEnum)]
        public enum ConfigurationType : short
        {
            None = 0,

            FixValue = 10,
            Analog  = 20,
            Bool    = 30,
            Direct  = 40,

            Connector   = 100,

            Method      = 200,

            Property    = 300,
        }
        #endregion

        #region ConnectorOrientation
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ConnectorOrientation'}de{'ConnectorOrientation'}", Global.ACKinds.TACEnum)]
        public enum ConnectorOrientation : short
        {
            Hidden = 0,
            Left = 1,
            Top = 2,
            Right = 3,
            Bottom = 4
        }
        #endregion

        #region ParamOption
        /// <summary>
        /// Enum für das Feld ParamOptionIndex
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ParamOption'}de{'ParamOption'}", Global.ACKinds.TACEnum)]
        public enum ParamOption : short
        {
            NotRequired = 0,    // Nicht erforderlich
            Required = 1,       // Erforderlich
            Optional = 2,       // Optional
            Fix = 3             // Fester Wert, der nicht geändert werden kann
        }
        #endregion

        #region ControlModes
        /// <summary>Mode of the WPF-Control</summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ControlModes'}de{'ControlModes'}", Global.ACKinds.TACEnum)]
        public enum ControlModes : short
        {
            /// <summary>
            /// In Collapsed state (not visible)
            /// </summary>
            Collapsed = -3,
            
            /// <summary>
            /// In hidden state (not visible)
            /// </summary>
            Hidden = -2,
            
            /// <summary>
            /// Disabled or readonly state
            /// </summary>
            Disabled = -1,
            
            /// <summary>
            /// Enabled state (read and write)
            /// </summary>
            Enabled = 0,
            
            /// <summary>
            /// User must enter a value
            /// </summary>
            EnabledRequired = 1,

            /// <summary>
            /// User has entered a invalid value
            /// </summary>
            EnabledWrong = 2,
        }

        static ACValueItemList _ControlModeList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList ControlModeList
        {
            get
            {
                if (Global._ControlModeList == null)
                {

                    Global._ControlModeList = new ACValueItemList("ControlModeIndex");
                    // Der Originaltext ist immer in Englisch
                    Global._ControlModeList.AddEntry((short)ControlModes.Hidden, "Hidden");
                    Global._ControlModeList.AddEntry((short)ControlModes.Disabled, "Disabled");
                    Global._ControlModeList.AddEntry((short)ControlModes.Enabled, "Enabled");
                    Global._ControlModeList.AddEntry((short)ControlModes.EnabledRequired, "EnabledRequired");
                    Global._ControlModeList.AddEntry((short)ControlModes.EnabledWrong, "EnabledWrong");
                }
                return Global._ControlModeList;
            }
        }

        /// <summary>
        /// Struct for controlling the presentation mode/state of a WPF-Control
        /// </summary>
        public struct ControlModesInfo
        {
            /// <summary>Mode of the WPF-Control</summary>
            /// <value>Mode of the WPF-Control</value>
            public ControlModes Mode { get; set; }

            /// <summary>Informs a WPF-Control, that the bound value is null and should be highlighted</summary>
            /// <value>
            ///   <c>true</c> control must be highlighted because the value is null; otherwise, <c>false</c>.</value>
            public Boolean IsNull { get; set; }


            /// <summary>  Message that should appear in the WPF-Control to inform a user.</summary>
            /// <value>The message for presentation.</value>
            public Msg Message { get; set; }

            public static ControlModesInfo Collapsed
            {
                get
                {
                    return new ControlModesInfo() { Mode = ControlModes.Collapsed };
                }
            }

            public static ControlModesInfo Hidden
            {
                get
                {
                    return new ControlModesInfo() { Mode = ControlModes.Hidden };
                }
            }

            public static ControlModesInfo Disabled
            {
                get
                {
                    return new ControlModesInfo() { Mode = ControlModes.Disabled };
                }
            }

            public static ControlModesInfo Enabled
            {
                get
                {
                    return new ControlModesInfo() { Mode = ControlModes.Enabled };
                }
            }

            public static ControlModesInfo EnabledRequired
            {
                get
                {
                    return new ControlModesInfo() { Mode = ControlModes.EnabledRequired };
                }
            }

            public static ControlModesInfo EnabledWrong
            {
                get
                {
                    return new ControlModesInfo() { Mode = ControlModes.EnabledWrong };
                }
            }
        }
        #endregion

        #region Global.MsgResult
        //[ACSerializeableInfo]
        //[ACClassInfo(Const.PackName_VarioSystem, "en{'Global.MsgResult'}de{'Global.MsgResult'}", Global.ACKinds.TACEnum)]
        //public enum Global.MsgResult : short
        //{
        //    // Summary:
        //    //     The message box returns no result.
        //    None = 0,
        //    //
        //    // Summary:
        //    //     The result value of the message box is OK.
        //    OK = 1,
        //    //
        //    // Summary:
        //    //     The result value of the message box is Cancel.
        //    Cancel = 2,
        //    //
        //    // Summary:
        //    //     The result value of the message box is Yes.
        //    Yes = 3,
        //    //
        //    // Summary:
        //    //     The result value of the message box is No.
        //    No = 4,
        //}
        #endregion

        #region eLayoutOrientation
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'eLayoutOrientation'}de{'eLayoutOrientation'}", Global.ACKinds.TACEnum)]
        public enum eLayoutOrientation : short
        {
            HorizontalBottom = 1,
            VerticalLeft = 2
        }
        #endregion

        #region MaxRefreshRate
        /// <summary>
        /// Enum für das Feld MaxRefreshRateIndex
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'MaxRefreshRates'}de{'MaxRefreshRates'}", Global.ACKinds.TACEnum)]
        public enum MaxRefreshRates : short
        {
            Off = 0,
            EventDriven = 1,
            R100ms = 2,
            R200ms = 3,
            R500ms = 4,
            R1sec = 5,
            R2sec = 6,
            R5sec = 7,
            R10sec = 8,
            R20sec = 9,
            R1min = 10,
            R2min = 11,
            R5min = 12,
            R10min = 13,
            R20min = 14,
            Hourly = 15,
            Daily = 16,
            Weekly = 17,
            Monthly = 18,
            Yearly = 19,
        }
        #endregion

        #region eOperator        
        /// <summary>
        /// Logical connective AND / OR
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'Logical operator'}de{'Verknüpfungsoperatoren'}", Global.ACKinds.TACEnum)]
        public enum Operators : short
        {
            none = 0,

            /// <summary>
            /// OR-Operator
            /// </summary>
            or = 1,

            /// <summary>
            /// AND-Operator
            /// </summary>
            and = 2,
        }
        #endregion

        #region eLogicalOperator        
        /// <summary>
        /// Relational operators and logical operators (IS NOT NULL, IS NULL)
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'Relational- and logic operators'}de{'Vergleichs- und Logische operatoren:'}", Global.ACKinds.TACEnum)]
        public enum LogicalOperators : short
        {
            /// <summary>
            /// Undefined
            /// </summary>
            none = 0,

            /// <summary>
            /// Equal: =
            /// </summary>
            equal = 1,

            /// <summary>
            /// Not equal: !=
            /// </summary>
            notEqual = 2,

            /// <summary>
            /// Less than: <
            /// </summary>
            lessThan = 3,

            /// <summary>
            /// The greater than: >
            /// </summary>
            greaterThan = 4,

            /// <summary>
            /// Less than or equal: <=
            /// </summary>
            lessThanOrEqual = 5,

            /// <summary>
            /// Greater than or equal: >=
            /// </summary>
            greaterThanOrEqual = 6,

            /// <summary>
            /// String.StartsWith(x); SQL: LIKE 'x%'
            /// </summary>
            startsWith = 7,

            /// <summary>
            /// String.EndsWith(x); SQL: LIKE '%x'
            /// </summary>
            endsWith = 8,

            /// <summary>
            /// string.Contains(x); SQL: LIKE '%x%'
            /// </summary>
            contains = 9,

            /// <summary>
            /// The is null operator
            /// </summary>
            isNull = 10,

            /// <summary>
            /// The is not null operator
            /// </summary>
            isNotNull = 11,

            /// <summary>
            /// !string.Contains(x); SQL: NOT LIKE '%x%'
            /// </summary>
            notContains = 12,
        }
        #endregion

        #region eFilterType
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'FilterTypes'}de{'FilterTypes'}", Global.ACKinds.TACEnum)]
        public enum FilterTypes : short
        {
            /// <summary>
            /// Row for phrasing a search-condition
            /// </summary>
            filter = 0,

            /// <summary>
            /// The left paranthesis: (
            /// </summary>
            parenthesisOpen = 1,

            /// <summary>
            /// The right paranthesis: )
            /// </summary>
            parenthesisClose = 2
        }
        #endregion

        #region eSortDirection
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'SortDirections'}de{'SortDirections'}", Global.ACKinds.TACEnum)]
        public enum SortDirections : short
        {
            ascending = 1,
            descending = 2
        }
        #endregion

        #region DeleteAction
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'DeleteAction'}de{'DeleteAction'}", Global.ACKinds.TACEnum)]
        public enum DeleteAction
        {
            None = 0,           // Löschen nur manuell durch den Anwender oder spezielle Implementierung
            Cascade = 1,        // Löschen erfolgt durch Datenbank
            CascadeManual = 2,  // Löschen erfolgt durch Entityframework
        }
        #endregion
       
        #region CompositionTypes
        /// <summary>
        /// Enum für das Feld CompositionTypeIndex
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'CompositionTypes'}de{'CompositionTypes'}", Global.ACKinds.TACEnum)]
        public enum CompositionTypes : short
        {
            Query = 10,
            Navigationquery = 20,
            Bussinessobject = 30,
            Documentation = 40,
        }
        #endregion

        #region ElementActionType
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ElementActionType'}de{'ElementActionType'}", Global.ACKinds.TACEnum)]
        public enum ElementActionType : short
        {
            //
            // Summary:
            //     The drop target does not accept the data.
            None = 0,
            //
            // Summary:
            //     The data is copied to the drop target.
            Drop = 1,
            //
            // Summary:
            //     The data from the drag source is moved to the drop target.
            Move = 2,
            //
            // Summary:
            //     The data from the drag source is linked to the drop target.
            Link = 4,

            Line = 5,

            ContextMenu = 100,

            ACCommand = 200,

            AllowDesignMode = 300,
            DesignModeOn = 301,
            DesignModeOff = 302,
            TabItemActivated = 303,
            TabItemDeActivated = 304,
            TabItemLoaded = 305,
            TabItemUnloaded = 306,
            VBDesignLoaded = 307,
            VBDesignUnloaded = 308,
            VBDesignChanged = 309,
            Refresh = 310
        }
        #endregion

        #region MethodModes
        [ACClassInfo(Const.PackName_VarioSystem, "en{'MethodModes'}de{'MethodModes'}", Global.ACKinds.TACEnum)]
        public enum MethodModes : short
        {
            Methods,            // Alle Methoden
            Assemblymethods,    // Assemblymethoden
            Scriptmethods,      // Scriptmethoden
            Workflows,          // Workflowmethoden
            Submethod,          // Untermethoden, die in einem untergeordnetem TPAProcessFunction implementiert sind
            States              // Statusmethoden
        }
        #endregion

        #region PropertyModes
        [ACClassInfo(Const.PackName_VarioSystem, "en{'PropertyModes'}de{'PropertyModes'}", Global.ACKinds.TACEnum)]
        public enum PropertyModes : short
        {
            Properties,
            Connections,
            Bindings,
            Events,
            Relations,
            Joblists,
            Livevalues,
        }
        #endregion

        #region ConfigSaveModes        
        /// <summary>
        /// ConfigSaveModes controls under which key (LocalConfigACUrl) a ACQueryDefinition is stored in the ACClassConfig-Table.
        /// </summary>
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ConfigSaveModes'}de{'ConfigSaveModes'}", Global.ACKinds.TACEnum)]
        public enum ConfigSaveModes
        {
            /// <summary>
            /// LocalConfigACUrl is build with "_N_" and the passed name in configName-Parameter
            /// </summary>
            Configurationname,

            /// <summary>
            /// ACQueryDefinition should be stored for a special user: LocalConfigACUrl is build with "_U_" and the current username (Database.Root.Environment.User.VBUserName)
            /// </summary>
            User,

            /// <summary>
            /// ACQueryDefinition should be stored generally for all users and computer. LocalConfigACUrl is build with "_S" 
            /// </summary>
            Common,

            /// <summary>
            /// ACQueryDefinition should be stored for this computer. LocalConfigACUrl is build with "_C_" and the current computer name (Database.Root.Environment.ComputerName)
            /// </summary>
            Computer
        }
        #endregion

        #region TimelineItem

        public enum TimelineItemStatus : short
        {
            OK = 0,
            Duration = 1,
            Alarm = 2,
            ChildAlarm = 3
        }

        #endregion

        #region ContextMenuCategory
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ContextMenuCategory'}de{'ContextMenuCategory'}", Global.ACKinds.TACEnum)]
        public enum ContextMenuCategory : short
        {
            NoCategory = 0,
            ProcessCommands = 10,
            ProdPlanLog = 20,
            Utilities = 30, 
            Image = 40
        }

        static ACValueItemList _ContextMenuCategoryList = null;
        static Dictionary<ContextMenuCategory, string> IconCategory = new Dictionary<ContextMenuCategory, string>();
        
        /// <summary>
        /// List for context menu categories.
        /// If we use context menu categies with subcatergories(max depth is 2 levels), first we need define main category in _ContextMenuCategoryList and than define subcategory.
        /// Example:
        /// 1. _ContextMenuCategoryList..AddEntry((short)ContextMenuCategory.Utilities, "en{'Utilities'}de{'Utilities'}");
        /// 2. _ContextMenuCategoryList.Add(new ACValueItem("en{'Image'}de{'Image'}", (short)ContextMenuCategory.Image, null, _ContextMenuCategoryList.FirstOrDefault(c => (short)c.Value == (short)ContextMenuCategory.Utilities), 100));
        /// 
        /// </summary>
        public static ACValueItemList ContextMenuCategoryList
        {
            get
            {
                if (Global._ContextMenuCategoryList == null)
                {
                    _ContextMenuCategoryList = new ACValueItemList("ContextMenuCategoryIndex");
                    _ContextMenuCategoryList.Add(new ACValueItem("en{'Process-Commands'}de{'Prozessbefehle'}", (short)ContextMenuCategory.ProcessCommands, null, null, 220));
                    _ContextMenuCategoryList.Add(new ACValueItem("en{'Production, Planning & Logistics'}de{'Produktion, Planung & Logistik'}", (short)Global.ContextMenuCategory.ProdPlanLog, null, null, 250));
                    _ContextMenuCategoryList.Add(new ACValueItem("en{'Tools'}de{'Werkzeuge'}", (short)ContextMenuCategory.Utilities, null, null, 20008));
                    _ContextMenuCategoryList.Add(new ACValueItem("en{'Image'}de{'Bild'}", (short)ContextMenuCategory.Image, null, _ContextMenuCategoryList.FirstOrDefault(c => (short)c.Value == (short)ContextMenuCategory.Utilities), 1000));
                }
                return _ContextMenuCategoryList;
            }
        }

        public static string GetCategoryIconACUrl(ContextMenuCategory category)
        {
            if (IconCategory.ContainsKey(category))
            {
                return IconCategory[category];
            }
            else
            {
                string categoryName = Enum.GetName(typeof(ContextMenuCategory), category);
                ACClassDesign design = null;


                                using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                    design = Database.GlobalDatabase.ACClass.FirstOrDefault(c => c.ACIdentifier == "ContextMenuCategory").Designs.FirstOrDefault(x => x.ACIdentifier == "Icon" + categoryName);
                
                if (design != null && design.DesignBinary != null)
                {
                    string acurl = design.GetACUrl();
                    IconCategory.Add(category, acurl);
                    return acurl;
                }
                else
                    IconCategory.Add(category, null);
            }
            return null;
        }

#endregion

        #region VBTileType

        public enum VBTileType : short
        {
            Group = 0,
            Tile = 1
        }

        #endregion

        #region InterpolationMethod
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'Interpolation method'}de{'Interpolationsmethode'}", Global.ACKinds.TACEnum)]
        public enum InterpolationMethod : short
        {
            None = 0,
            MovingAverage = 1,
            WeightedForewardedAverage = 2,
            Median = 3
        }

        static ACValueItemList _InterpolationMethodList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        static public ACValueItemList InterpolationMethodList
        {
            get
            {
                if (Global._InterpolationMethodList == null)
                {
                    Global._InterpolationMethodList = new ACValueItemList("InterpolationMethodEnumIndex");
                    Global._InterpolationMethodList.AddEntry((short)Global.InterpolationMethod.None, "en{'None'}de{'Keine'}");
                    Global._InterpolationMethodList.AddEntry((short)Global.InterpolationMethod.MovingAverage, "en{'Moving Average'}de{'Gleitender Mittelwert'}");
                    Global._InterpolationMethodList.AddEntry((short)Global.InterpolationMethod.WeightedForewardedAverage, "en{'Foreword looking average with decay'}de{'Vorrausschauender Mittelwert mit abklingender Gewichtung'}");
                    Global._InterpolationMethodList.AddEntry((short)Global.InterpolationMethod.Median, "en{'Median'}de{'Medianwert'}");
                }
                return Global._InterpolationMethodList;
            }
        }
        #endregion

        #region TCatTreeIconState

        public enum ChangeTypeEnum : short
        {
            None = 0,
            Added = 10,
            Removed = 20,
            Changed = 30
        }

        #endregion

        #region MaintRuleIconState

        public enum ConfigIconState : short
        {
            NoConfig = 0,
            InheritedConfig = 10,
            Config = 20,
            ExclusionConfig = 30
        }

        #endregion

        #region PropertyLogRuleType

        public enum PropertyLogRuleType : short
        {
            ProjectHierarchy = 10,
            ProjectHierarchyOneself = 11,
            BasedOn = 20
        }

        static ACValueItemList _PropertyLogRuleTypeList = null;
        public static ACValueItemList PropertyLogRuleTypeList
        {
            get
            {
                if(_PropertyLogRuleTypeList == null)
                {
                    _PropertyLogRuleTypeList = new ACValueItemList("PropertyLogRuleType");
                    _PropertyLogRuleTypeList.Add(new ACValueItem("en{'Project hierarchy'}de{'Projekthierarchie'}", PropertyLogRuleType.ProjectHierarchy, null));
                    _PropertyLogRuleTypeList.Add(new ACValueItem("en{'Based on'}de{'Basierend auf'}", PropertyLogRuleType.BasedOn, null));
                }
                return _PropertyLogRuleTypeList;
            }
        }

        #endregion

        #region Graph actions
        public enum GraphAction
        {
            None,
            CleanUpGraph,
            InitVBControl,
            InitGraphSurface,
            AvailableRoutes,
            StartGraphProgress,
            RecalcEdgesRoute,
            Relayout
        }
        #endregion
    }


    public static class GlobalExtensionMethods
    {
        public static bool IsRefreshRateCycleElapsed(this TimeSpan span, Global.MaxRefreshRates refreshRate)
        {
            switch (refreshRate)
            {
                case Global.MaxRefreshRates.Off:
                    return false;
                case Global.MaxRefreshRates.EventDriven:
                    return true;
                case Global.MaxRefreshRates.R100ms:
                    if (span.TotalMilliseconds >= 100)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R200ms:
                    if (span.TotalMilliseconds >= 200)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R500ms:
                    if (span.TotalMilliseconds >= 500)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R1sec:
                    if (span.TotalSeconds >= 1)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R2sec:
                    if (span.TotalSeconds >= 2)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R5sec:
                    if (span.TotalSeconds >= 5)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R10sec:
                    if (span.TotalSeconds >= 10)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R20sec:
                    if (span.TotalSeconds >= 20)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R1min:
                    if (span.TotalMinutes >= 1)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R2min:
                    if (span.TotalMinutes >= 2)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R5min:
                    if (span.TotalMinutes >= 5)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R10min:
                    if (span.TotalMinutes >= 10)
                        return true;
                    return false;
                case Global.MaxRefreshRates.R20min:
                    if (span.TotalMinutes >= 20)
                        return true;
                    return false;
                case Global.MaxRefreshRates.Hourly:
                    if (span.TotalHours >= 1)
                        return true;
                    return false;
                case Global.MaxRefreshRates.Daily:
                    if (span.TotalDays >= 1)
                        return true;
                    return false;
                case Global.MaxRefreshRates.Weekly:
                    if (span.TotalDays >= 7)
                        return true;
                    return false;
                case Global.MaxRefreshRates.Monthly:
                    if (span.TotalDays >= 30)
                        return true;
                    return false;
                case Global.MaxRefreshRates.Yearly:
                    if (span.TotalDays >= 365)
                        return true;
                    return false;
                default:
                    return true;
            }
        }

        public static int MaxRefreshRatesGetMillisec(Global.MaxRefreshRates refreshRate)
        {
            switch (refreshRate)
            {
                case Global.MaxRefreshRates.Off:
                case Global.MaxRefreshRates.EventDriven:
                    return 0;
                case Global.MaxRefreshRates.R100ms:
                        return 100;
                case Global.MaxRefreshRates.R200ms:
                    return 200;
                case Global.MaxRefreshRates.R500ms:
                    return 500;
                case Global.MaxRefreshRates.R1sec:
                    return 1000;
                case Global.MaxRefreshRates.R2sec:
                    return 2000;
                case Global.MaxRefreshRates.R5sec:
                    return 5000;
                case Global.MaxRefreshRates.R10sec:
                    return 10000;
                case Global.MaxRefreshRates.R20sec:
                    return 20000;
                case Global.MaxRefreshRates.R1min:
                    return 60000;
                case Global.MaxRefreshRates.R2min:
                    return 120000;
                case Global.MaxRefreshRates.R5min:
                    return 300000;
                case Global.MaxRefreshRates.R10min:
                    return 600000;
                case Global.MaxRefreshRates.R20min:
                    return 1200000;
                case Global.MaxRefreshRates.Hourly:
                    return 3600000;
                case Global.MaxRefreshRates.Daily:
                case Global.MaxRefreshRates.Weekly:
                case Global.MaxRefreshRates.Monthly:
                case Global.MaxRefreshRates.Yearly:
                    return 86400000;
                default:
                    return 0;
            }
        }
    }
}

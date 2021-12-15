using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Printing settings'}de{'Druckeinstellungen'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class BSOPrint : ACBSO
    {
        #region const
        public const string ClassName = @"BSOPrint";
        public const string BGWorkerMehtod_LoadMachinesAndPrinters = "LoadMachinesAndPrinters";
        #endregion

        #region c'tors

        /// <summary>
        /// Creates a new instance of the BSOPropertyLogRules.
        /// </summary>
        /// <param name="acType">The acType parameter.</param>
        /// <param name="content">The content parameter.</param>
        /// <param name="parentACObject">The parentACObject parameter.</param>
        /// <param name="parameter">The parameters in the ACValueList.</param>
        /// <param name="acIdentifier">The acIdentifier parameter.</param>
        public BSOPrint(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            // 
        }

        /// <summary>
        /// Initializes this component.
        /// </summary>
        /// <param name="startChildMode">The start child mode parameter.</param>
        /// <returns>True if is initialization success, otherwise returns false.</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool baseInit = base.ACInit(startChildMode);

            _PrintManager = ACPrintManager.ACRefToServiceInstance(this);
            if (_PrintManager == null)
                throw new Exception("ACPrintManager not configured");

            LoadMachinesAndPrinters(Database as gip.core.datamodel.Database);

            return baseInit;
        }

        /// <summary>
        ///  Deinitializes this component.
        /// </summary>
        /// <param name="deleteACClassTask">The deleteACClassTask parameter.</param>
        /// <returns>True if is deinitialization success, otherwise returns false.</returns>
        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool done = base.ACDeInit(deleteACClassTask);
            if (_PrintManager != null)
                ACPrintManager.DetachACRefFromServiceInstance(this, _PrintManager);
            _PrintManager = null;
            return done;
        }

        #endregion

        #region Managers

        protected ACRef<ACComponent> _PrintManager = null;
        protected ACComponent PrintManager
        {
            get
            {
                if (_PrintManager == null)
                    return null;
                return _PrintManager.ValueT;
            }
        }

        #endregion

        #region Properties


        #region Properties -> Messages

        public void SendMessage(object result)
        {
            Msg msg = result as Msg;
            if (msg != null)
            {
                SendMessage(msg);
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
        [ACPropertyCurrent(528, "Message", "en{'Message'}de{'Meldung'}")]
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
        [ACPropertyList(529, "Message", "en{'Messagelist'}de{'Meldungsliste'}")]
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
            MsgList.Add(msg);
            OnPropertyChanged("MsgList");
        }

        public void ClearMessages()
        {
            MsgList.Clear();
            OnPropertyChanged("MsgList");
        }
        #endregion

        #region Properties -> Machines

        private ACItem _SelectedMachine;
        /// <summary>
        /// Selected property for ACComponent
        /// </summary>
        /// <value>The selected Machine</value>
        [ACPropertySelected(9999, "Machine", "en{'TODO: Machine'}de{'TODO: Machine'}")]
        public ACItem SelectedMachine
        {
            get
            {
                return _SelectedMachine;
            }
            set
            {
                if (_SelectedMachine != value)
                {
                    _SelectedMachine = value;
                    if (value != null)
                    {
                        LocationName = SelectedMachine.ACUrlComponent;
                    }
                    OnPropertyChanged("SelectedMachine");
                    OnSelectedMachineChanged(SelectedMachine);
                }
            }
        }

        public virtual void OnSelectedMachineChanged(ACItem machine)
        {

        }


        private List<ACItem> _MachineList;
        /// <summary>
        /// List property for ACComponent
        /// </summary>
        /// <value>The Machine list</value>
        [ACPropertyList(9999, "Machine")]
        public List<ACItem> MachineList
        {
            get
            {
                if (_MachineList == null)
                    _MachineList = new List<ACItem>();
                return _MachineList;
            }
        }

        #endregion

        #region LocationName
        private string _LocationName;
        /// <summary>
        /// Selected property for 
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "LocationName", "en{'Selected location'}de{'Ausgewählter Standort'}")]
        public string LocationName
        {
            get
            {
                return _LocationName;
            }
            set
            {
                if (_LocationName != value)
                {
                    _LocationName = value;
                    OnPropertyChanged("LocationName");
                }
            }
        }

        #endregion

        #region Properties -> ConfiguredPrinter

        private PrinterInfo _SelectedConfiguredPrinter;
        /// <summary>
        /// Selected property for PrinterInfo
        /// </summary>
        /// <value>The selected ConfiguredPrinter</value>
        [ACPropertySelected(9999, "ConfiguredPrinter", "en{'TODO: ConfiguredPrinter'}de{'TODO: ConfiguredPrinter'}")]
        public PrinterInfo SelectedConfiguredPrinter
        {
            get
            {
                return _SelectedConfiguredPrinter;
            }
            set
            {
                if (_SelectedConfiguredPrinter != value)
                {
                    _SelectedConfiguredPrinter = value;
                    OnPropertyChanged("SelectedConfiguredPrinter");
                }
            }
        }

        private List<PrinterInfo> _ConfiguredPrinterList;
        /// <summary>
        /// List property for PrinterInfo
        /// </summary>
        /// <value>The ConfiguredPrinters list</value>
        [ACPropertyList(9999, "ConfiguredPrinter")]
        public List<PrinterInfo> ConfiguredPrinterList
        {
            get
            {
                return _ConfiguredPrinterList;
            }
            set
            {
                _ConfiguredPrinterList = value;
                OnPropertyChanged("ConfiguredPrinterList");
            }
        }

        #endregion

        #region Properties -> Windows printers

        private PrinterInfo _SelectedWindowsPrinter;
        /// <summary>
        /// Selected property for PrinterInfo
        /// </summary>
        /// <value>The selected WindowsPrinter</value>
        [ACPropertySelected(9999, "WindowsPrinter", "en{'TODO: WindowsPrinter'}de{'TODO: WindowsPrinter'}")]
        public PrinterInfo SelectedWindowsPrinter
        {
            get
            {
                return _SelectedWindowsPrinter;
            }
            set
            {
                if (_SelectedWindowsPrinter != value)
                {
                    _SelectedWindowsPrinter = value;
                    if (value != null)
                    {
                        SelectedPrintServer = null;
                        PrinterName = value.PrinterName;
                    }
                    OnPropertyChanged("SelectedWindowsPrinter");
                }
            }
        }

        private List<PrinterInfo> _WindowsPrinterList;
        /// <summary>
        /// List property for PrinterInfo
        /// </summary>
        /// <value>The WindowsPrinter list</value>
        [ACPropertyList(9999, "WindowsPrinter")]
        public List<PrinterInfo> WindowsPrinterList
        {
            get
            {
                if (_WindowsPrinterList == null)
                    _WindowsPrinterList = LoadWindowsPrinterList();
                return _WindowsPrinterList;
            }
        }

        private List<PrinterInfo> LoadWindowsPrinterList()
        {
            string[] configuredPrinters = ConfiguredPrinterList.Select(c => c.PrinterName).ToArray();
            System.Drawing.Printing.PrinterSettings.StringCollection windowsPrinters = PrinterSettings.InstalledPrinters;
            List<string> windowsPrintersList = new List<string>();
            foreach (string printer in windowsPrinters)
            {
                windowsPrintersList.Add(printer);
            }
            return
                windowsPrintersList
                .Where(c => !configuredPrinters.Contains(c))
                .Select(c => new PrinterInfo() { PrinterName = c, Name = c })
                .ToList();
        }

        #endregion

        #region Properties ->  PrintServer

        private PrinterInfo _SelectedPrintServer;
        /// <summary>
        /// Selected property for PrinterInfo
        /// </summary>
        /// <value>The selected ESCPosPrinter</value>
        [ACPropertySelected(9999, "PrintServer", "en{'TODO: ESCPosPrinter'}de{'TODO: ESCPosPrinter'}")]
        public PrinterInfo SelectedPrintServer
        {
            get
            {
                return _SelectedPrintServer;
            }
            set
            {
                if (_SelectedPrintServer != value)
                {
                    _SelectedPrintServer = value;
                    if (value != null)
                    {
                        SelectedWindowsPrinter = null;
                        PrinterName = value.PrinterACUrl;
                    }
                    OnPropertyChanged("SelectedPrintServer");
                }
            }
        }


        private List<PrinterInfo> _PrintServerList;
        /// <summary>
        /// List property for PrinterInfo
        /// </summary>
        /// <value>The ESCPosPrinter list</value>
        [ACPropertyList(9999, "PrintServer")]
        public List<PrinterInfo> PrintServerList
        {
            get
            {
                if (_PrintServerList == null)
                    _PrintServerList = new List<PrinterInfo>();
                return _PrintServerList;
            }
        }

        #endregion

        #region Properties -> PrinterName


        #region PrinterName
        private string _PrinterName;
        /// <summary>
        /// Selected property for 
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "PrinterName", "en{'Selected printer'}de{'Ausgewählte Drucker'}")]
        public string PrinterName
        {
            get
            {
                return _PrinterName;
            }
            set
            {
                if (_PrinterName != value)
                {
                    _PrinterName = value;
                    OnPropertyChanged("PrinterName");
                }
            }
        }

        #endregion

        #endregion

        #region Properties => VBUser

        private core.datamodel.VBUser _SelectedVBUser;
        [ACPropertySelected(604, "VBUser", "en{'User'}de{'Benutzer'}")]
        public core.datamodel.VBUser SelectedVBUser
        {
            get => _SelectedVBUser;
            set
            {
                _SelectedVBUser = value;
                OnPropertyChanged("SelectedVBUser");
            }
        }

        [ACPropertyList(605, "VBUser")]
        public IEnumerable<core.datamodel.VBUser> VBUserList
        {
            get
            {
                return (Database as gip.core.datamodel.Database).VBUser/*.Where(c => !c.IsSuperuser)*/.ToArray();
            }
        }


        #endregion

        #endregion

        #region Methods

        [ACMethodInfo(BSOPrint.ClassName, "en{'Remove printer'}de{'Drucker entfernen'}", 9999)]

        public void RemovePrinter()
        {
            if (!IsEnabledRemovePrinter())
                return;

            ACClassConfig aCClassConfig = (Database as gip.core.datamodel.Database).ACClassConfig.FirstOrDefault(c => c.ACClassConfigID == SelectedConfiguredPrinter.ACClassConfigID);
            MsgWithDetails msg = aCClassConfig.DeleteACObject(Database, false);
            if (msg == null)
            {
                msg = Database.ACSaveChanges();
                if (msg == null)
                {
                    ConfiguredPrinterList.Remove(SelectedConfiguredPrinter);
                    SelectedConfiguredPrinter = ConfiguredPrinterList.FirstOrDefault();
                    OnPropertyChanged("ConfiguredPrinterList");
                }
            }
        }

        public bool IsEnabledRemovePrinter()
        {
            return SelectedConfiguredPrinter != null;
        }


        [ACMethodInfo(BSOPrint.ClassName, "en{'Add printer'}de{'Drucker hinzufügen'}", 9999)]
        public virtual void AddPrinter()
        {
            if (!IsEnabledAddPrinter())
                return;
            ACClass aCClass = (Database as gip.core.datamodel.Database).ACClass.FirstOrDefault(c => c.ACClassID == PrintManager.ComponentClass.ACClassID);
            ACClass propertyInfoType = (Database as gip.core.datamodel.Database).ACClass.FirstOrDefault(c => c.ACIdentifier == "PrinterInfo");

            ACClassConfig aCClassConfig = ACClassConfig.NewACObject(Database as gip.core.datamodel.Database, aCClass);
            aCClassConfig.ValueTypeACClassID = propertyInfoType.ACClassID;

            PrinterInfo newConfiguredPrinter = new PrinterInfo();
            newConfiguredPrinter.ACClassConfigID = aCClassConfig.ACClassConfigID;

            SetPrinterTarget(newConfiguredPrinter);

            if (SelectedWindowsPrinter != null)
                newConfiguredPrinter.PrinterName = SelectedWindowsPrinter.PrinterName;
            else if (SelectedPrintServer != null)
                newConfiguredPrinter.PrinterACUrl = SelectedPrintServer.PrinterACUrl;

            if (SelectedVBUser != null)
            {
                newConfiguredPrinter.VBUserID = SelectedVBUser.VBUserID;
            }

            aCClassConfig.KeyACUrl = ACPrintManager.Const_KeyACUrl_ConfiguredPrintersConfig;
            aCClassConfig.Value = newConfiguredPrinter;
            var test = aCClassConfig.ACProperties.Properties.FirstOrDefault().Value;
            (Database as gip.core.datamodel.Database).ACClassConfig.AddObject(aCClassConfig);
            MsgWithDetails msg = (Database as gip.core.datamodel.Database).ACSaveChanges();
            if (msg == null)
            {
                ConfiguredPrinterList.Add(newConfiguredPrinter);
                SelectedConfiguredPrinter = newConfiguredPrinter;
                OnPropertyChanged("ConfiguredPrinterList");
            }
        }

        public virtual void SetPrinterTarget(PrinterInfo printerInfo)
        {
            if (SelectedMachine != null)
                printerInfo.MachineACUrl = SelectedMachine.ACUrlComponent;
        }

        public virtual bool IsEnabledAddPrinter()
        {
            return
                   (SelectedMachine != null || SelectedVBUser != null)
                && (SelectedWindowsPrinter != null || SelectedPrintServer != null)
                && !ConfiguredPrinterList.Any(c =>
                    (
                        (SelectedWindowsPrinter != null && c.PrinterName == SelectedWindowsPrinter.PrinterName)
                        || (SelectedPrintServer != null && c.PrinterACUrl == SelectedPrintServer.PrinterACUrl)
                    )
                    && (SelectedMachine != null && SelectedMachine.ACUrl == c.MachineACUrl)
                );
        }

        public virtual void LoadConfiguredPrinters()
        {
            ConfiguredPrinterList = ACPrintManager.GetConfiguredPrinters(Database as gip.core.datamodel.Database, PrintManager.ComponentClass.ACClassID, true);
            foreach (PrinterInfo pInfo in ConfiguredPrinterList)
            {
                pInfo.Attach((Database as Database));
            }
        }

        #endregion


        private void LoadMachinesAndPrinters(Database database)
        {
            _PrintServerList = ACPrintManager.GetPrintServers(database);
            List<ACItem> machines = new List<ACItem>();

            ACClass acClassPAClassPhysicalBase = Database.ContextIPlus.GetACType(typeof(PAClassPhysicalBase));
            IQueryable<ACClass> queryClasses = ACClassManager.s_cQry_GetAvailableModulesAsACClass(Database.ContextIPlus, acClassPAClassPhysicalBase.ACIdentifier);
            if (queryClasses != null && queryClasses.Any())
            {
                ACClass[] aCClasses = queryClasses.ToArray();
                foreach (ACClass cClass in aCClasses)
                {
                    ACItem aCItem = new ACItem();
                    aCItem.ReadACClassData(cClass);
                    machines.Add(aCItem);
                }
            }
            _MachineList = machines;
            LoadConfiguredPrinters();
            OnPropertyChanged("MachineList");
        }

    }
}

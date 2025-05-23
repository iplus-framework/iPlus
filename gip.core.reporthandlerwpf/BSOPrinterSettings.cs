// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using gip.core.reporthandler;

namespace gip.core.reporthandlerwpf
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Printer settings'}de{'Drucker-Einstellungen'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class BSOPrinterSettings : ACBSO
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
        public BSOPrinterSettings(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
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
            {
                Messages.Error(this, "ACPrintManager not configured", true);
                //throw new Exception("ACPrintManager not configured");
            }

            LoadMachinesAndPrinters(Db);

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

            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }

            return done;
        }

        #endregion

        #region Managers

        protected ACRef<ACPrintManager> _PrintManager = null;
        protected ACPrintManager PrintManager
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

        #region Database

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

        public Database Db
        {
            get
            {
                return Database as Database;
            }
        }

        #endregion

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
                    else
                        LocationName = null;
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
        [ACPropertyInfo(999, "LocationName", "en{'Selected location (ACUrl)'}de{'Ausgewählter Standort (ACUrl)'}")]
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
            var printers = Root?.WPFServices?.VBMediaControllerService?.GetWindowsPrinters();
            if (printers != null)
                return ACPrintManager.GetPrinters(printers);
            return new List<PrinterInfo>();
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
                return Db.VBUser/*.Where(c => !c.IsSuperuser)*/.ToArray();
            }
        }


        #endregion

        #endregion

        #region Methods

        [ACMethodInfo(BSOPrinterSettings.ClassName, "en{'Remove printer'}de{'Drucker entfernen'}", 9999)]

        public void RemovePrinter()
        {
            if (!IsEnabledRemovePrinter())
                return;

            Msg msg = PrintManager.UnAssignPrinter(Db, SelectedConfiguredPrinter);
            if (msg == null)
            {
                ConfiguredPrinterList.Remove(SelectedConfiguredPrinter);
                SelectedConfiguredPrinter = ConfiguredPrinterList.FirstOrDefault();
                OnPropertyChanged("ConfiguredPrinterList");
            }
            
        }

        public bool IsEnabledRemovePrinter()
        {
            return SelectedConfiguredPrinter != null && PrintManager != null;
        }


        [ACMethodInfo(BSOPrinterSettings.ClassName, "en{'Add printer'}de{'Drucker hinzufügen'}", 9999)]
        public virtual void AddPrinter()
        {
            if (!IsEnabledAddPrinter() || PrintManager == null)
                return;

            PrinterInfo newConfiguredPrinter = new PrinterInfo();
            SetPrinterTarget(newConfiguredPrinter);

            if (SelectedWindowsPrinter != null)
                newConfiguredPrinter.PrinterName = SelectedWindowsPrinter.PrinterName;
            else if (SelectedPrintServer != null)
                newConfiguredPrinter.PrinterACUrl = SelectedPrintServer.PrinterACUrl;

            if (SelectedVBUser != null)
            {
                newConfiguredPrinter.VBUserID = SelectedVBUser.VBUserID;
                newConfiguredPrinter.Attach(Db);
            }

            Msg msg = PrintManager.AssignPrinter(Db, newConfiguredPrinter);
            if (msg == null)
            {
                ConfiguredPrinterList.Add(newConfiguredPrinter);
                SelectedConfiguredPrinter = newConfiguredPrinter;
                OnPropertyChanged("ConfiguredPrinterList");
            }
        }

        public virtual void SetPrinterTarget(PrinterInfo printerInfo)
        {
            if (!String.IsNullOrEmpty(LocationName))
                printerInfo.MachineACUrl = LocationName;
        }

        public virtual bool IsEnabledAddPrinter()
        {
            if (ConfiguredPrinterList == null)
                return false;

            return
                   (!String.IsNullOrEmpty(LocationName) || SelectedVBUser != null)
                && (SelectedWindowsPrinter != null || SelectedPrintServer != null)
                && !ConfiguredPrinterList.Any(c =>
                    (
                        (SelectedWindowsPrinter != null && c.PrinterName == SelectedWindowsPrinter.PrinterName)
                        || (SelectedPrintServer != null && c.PrinterACUrl == SelectedPrintServer.PrinterACUrl)
                    )
                    && (!String.IsNullOrEmpty(LocationName) && LocationName == c.MachineACUrl)
                );
        }

        public virtual void LoadConfiguredPrinters()
        {
            if (PrintManager == null)
                return;
            ConfiguredPrinterList = ACPrintManager.GetConfiguredPrinters(Db, PrintManager.ComponentClass.ACClassID, true);
            foreach (PrinterInfo pInfo in ConfiguredPrinterList)
            {
                pInfo.Attach(Db);
            }
        }

        [ACMethodInteraction("", "en{'Deselect user'}de{'Benutzer abwählen'}", 9999, true, "SelectedVBUser")]
        public void DeselectVBUser()
        {
            if (SelectedVBUser != null)
                SelectedVBUser = null;
        }

        public bool IsEnabledDeselectVBUser()
        {
            return SelectedVBUser != null;
        }


        private void LoadMachinesAndPrinters(Database database)
        {
            _PrintServerList = ACPrintManager.GetPrintServers(database);
            List<ACItem> machines = new List<ACItem>();

            ACClass acClassPAClassPhysicalBase = database.GetACType(typeof(PAProcessModule));
            IEnumerable<ACClass> queryClasses = ACClassManager.s_cQry_GetAvailableModulesAsACClass(database, acClassPAClassPhysicalBase.ACIdentifier);
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

        public override object Clone()
        {
            BSOPrinterSettings clone = base.Clone() as BSOPrinterSettings;
            clone.SelectedWindowsPrinter = this.SelectedWindowsPrinter;
            return clone;
        }
        #endregion

        #region Execute-Helper
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(AddPrinter):
                    AddPrinter();
                    return true;
                case nameof(IsEnabledAddPrinter):
                    result = IsEnabledAddPrinter();
                    return true;
                case nameof(RemovePrinter):
                    RemovePrinter();
                    return true;
                case nameof(IsEnabledRemovePrinter):
                    result = IsEnabledRemovePrinter();
                    return true;
                case nameof(DeselectVBUser):
                    DeselectVBUser();
                    return true;
                case nameof(IsEnabledDeselectVBUser):
                    result = IsEnabledDeselectVBUser();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }
}

﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioFacility, "en{'Printing settings'}de{'Druckeinstellungen'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
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
        public BSOPrint(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            // this is test
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

            // TODO SASA:
            //CurrentFacilityRoot = FacilityTree.LoadFacilityTree(DatabaseApp);
            CurrentFacility = CurrentFacilityRoot;

            BackgroundWorker.RunWorkerAsync(BGWorkerMehtod_LoadMachinesAndPrinters);
            ShowDialog(this, DesignNameProgressBar);

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

        protected WorkerResult BSOPrinterWorkerResult { get; set; }

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

        #region Properties -> FacilityTree

        ACFSItem _CurrentFacilityRoot;
        ACFSItem _CurrentFacility;


        /// <summary>
        /// Gets or sets the current import project item root.
        /// </summary>
        /// <value>The current import project item root.</value>
        [ACPropertyCurrent(9999, "FacilityRoot")]
        public ACFSItem CurrentFacilityRoot
        {
            get
            {
                return _CurrentFacilityRoot;
            }
            set
            {
                _CurrentFacilityRoot = value;
                OnPropertyChanged("CurrentFacilityRoot");
            }

        }

        /// <summary>
        /// Gets or sets the current import project item.
        /// </summary>
        /// <value>The current import project item.</value>
        [ACPropertyCurrent(9999, "Facility")]
        public ACFSItem CurrentFacility
        {
            get
            {
                return _CurrentFacility;
            }
            set
            {
                if (_CurrentFacility != value)
                {
                    if (_CurrentFacility != null && _CurrentFacility.ACObject != null)
                        (_CurrentFacility.ACObject as INotifyPropertyChanged).PropertyChanged -= _CurrentFacility_PropertyChanged;
                    _CurrentFacility = value;
                    if (_CurrentFacility != null && _CurrentFacility.ACObject != null)
                        (_CurrentFacility.ACObject as INotifyPropertyChanged).PropertyChanged += _CurrentFacility_PropertyChanged;
                    if (value != null && value.Value != null)
                    {
                        SelectedMachine = null;
                        // TODO: SASA
                        //LocationName = (value.Value as Facility).FacilityNo;
                    }
                    OnPropertyChanged("CurrentFacility");
                    OnPropertyChanged("SelectedFacility");
                }
            }
        }

        private void _CurrentFacility_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FacilityNo" || e.PropertyName == "FacilityName")
            {
                ACFSItem current = CurrentFacility;
                // TODO: SASA
                //current.ACCaption = FacilityTree.FacilityACCaption(CurrentFacility.ACObject as Facility);
                //CurrentFacilityRoot = FacilityTree.GetNewRootFacilityACFSItem(Database as gip.core.datamodel.Database, CurrentFacilityRoot.Items);
                CurrentFacility = current;
            }
        }

        // TODO: SASA
        //[ACPropertyInfo(9999, "SelectedFacility")]
        //public Facility SelectedFacility
        //{
        //    get
        //    {
        //        if (CurrentFacility != null && CurrentFacility.ACObject != null)
        //            return CurrentFacility.ACObject as Facility;
        //        return null;
        //    }
        //}

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
                        CurrentFacility = null;
                        LocationName = SelectedMachine.ACUrlComponent;
                    }
                    OnPropertyChanged("SelectedMachine");
                }
            }
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

        private ObservableCollection<PrinterInfo> _ConfiguredPrinterList;
        /// <summary>
        /// List property for PrinterInfo
        /// </summary>
        /// <value>The ConfiguredPrinters list</value>
        [ACPropertyList(9999, "ConfiguredPrinter")]
        public ObservableCollection<PrinterInfo> ConfiguredPrinterList
        {
            get
            {
                if (_ConfiguredPrinterList == null)
                {
                    // INvoke static method on ACPrintManager with passing context
                    //PrintManager.ComponentClass.ConfigurationEntries.Where(c => c.KeyACUrl == ACPrintManager.Const_KeyACUrl_ConfiguredPrintersConfig);
                    // TODO: SASA

                    //if (PrintManager.ConfiguredPrintersConfig != null && PrintManager.ConfiguredPrintersConfig.Any())
                    //{
                    //    PrinterInfo[] printerInfos = PrintManager.ConfiguredPrintersConfig.Select(c => c.Value as PrinterInfo).ToArray();
                    //    _ConfiguredPrinterList = new ObservableCollection<PrinterInfo>(printerInfos);
                    //}
                    // TODO Call !ReloadConfig on serivce object for refrehsing
                    _ConfiguredPrinterList.CollectionChanged += _ConfiguredPrinterList_CollectionChanged;
                }
                return _ConfiguredPrinterList;
            }
        }

        private void _ConfiguredPrinterList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                PrinterInfo newPrinterInfo = e.NewItems[0] as PrinterInfo;
                core.datamodel.ACClassConfig newClassConfig = core.datamodel.ACClassConfig.NewACObject(Database.ContextIPlus, PrintManager.ACType);
                newClassConfig.KeyACUrl = ACPrintManager.Const_KeyACUrl_ConfiguredPrintersConfig;
                newClassConfig.Value = newPrinterInfo;
                // TODO: SASA

                //PrintManager.ConfiguredPrintersConfig.Add(newClassConfig);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                PrinterInfo oldPrinterInfo = e.OldItems[0] as PrinterInfo;
                // TODO: SASA

                //core.datamodel.ACClassConfig existingConfig =
                //    PrintManager.ConfiguredPrintersConfig.Where(c => (c.Value as PrinterInfo) == oldPrinterInfo).FirstOrDefault();
                //PrintManager.ConfiguredPrintersConfig.Remove(existingConfig);
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


        #endregion

        #region Methods

        [ACMethodInfo(BSOPrint.ClassName, "en{'Remove printer'}de{'Drucker entfernen'}", 9999)]

        public void RemovePrinter()
        {
            if (!IsEnabledRemovePrinter())
                return;
            bool isWindowsPrinter = !string.IsNullOrEmpty(SelectedConfiguredPrinter.PrinterName);
            ConfiguredPrinterList.Remove(SelectedConfiguredPrinter);
            OnPropertyChanged("ConfiguredPrinterList");
        }

        public bool IsEnabledRemovePrinter()
        {
            return SelectedConfiguredPrinter != null;
        }

        [ACMethodInfo(BSOPrint.ClassName, "en{'Add printer'}de{'Drucker hinzufügen'}", 9999)]
        public void AddPrinter()
        {
            if (!IsEnabledAddPrinter())
                return;
            // TODO: SASA
            //if (SelectedWindowsPrinter != null)
            //{
            //    if (CurrentFacility != null)
            //        SelectedWindowsPrinter.FacilityNo = (CurrentFacility.Value as Facility).FacilityNo;
            //    else if (SelectedMachine != null)
            //        SelectedWindowsPrinter.MachineACUrl = SelectedMachine.ACUrlComponent;

            //    ConfiguredPrinterList.Add(SelectedWindowsPrinter);
            //    OnPropertyChanged("ConfiguredPrinterList");
            //}
            //else if (SelectedPrintServer != null)
            //{
            //    if (CurrentFacility != null)
            //        SelectedPrintServer.FacilityNo = (CurrentFacility.Value as Facility).FacilityNo;
            //    else if (SelectedMachine != null)
            //        SelectedPrintServer.MachineACUrl = SelectedMachine.ACUrlComponent;

            //    ConfiguredPrinterList.Add(SelectedPrintServer);
            //    OnPropertyChanged("PrintServerList");
            //}
        }

        public bool IsEnabledAddPrinter()
        {
            // TODO: SASA
            return false;
            //return
            //    ((CurrentFacility != null && CurrentFacility.Value != null) || SelectedMachine != null)
            //    && (SelectedWindowsPrinter != null || SelectedPrintServer != null)
            //    && !ConfiguredPrinterList.Any(c =>
            //        (
            //            (SelectedWindowsPrinter != null && c.PrinterName == SelectedWindowsPrinter.PrinterName)
            //            || (SelectedPrintServer != null && c.PrinterACUrl == SelectedPrintServer.PrinterACUrl)
            //        )
            //        && ((SelectedMachine != null && SelectedMachine.ACUrl == c.MachineACUrl) || (CurrentFacility != null && c.FacilityNo == (CurrentFacility.Value as Facility).FacilityNo))
            //    );
        }

        #endregion

        #region Background worker
        /// <summary>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = e.Argument.ToString();

            worker.ProgressInfo.OnlyTotalProgress = true;
            worker.ProgressInfo.AddSubTask(command, 0, 9);
            string message = Translator.GetTranslation("en{'Running {0}...'}de{'{0} läuft...'}");
            worker.ProgressInfo.ReportProgress(command, 0, string.Format(message, command));

            switch (command)
            {
                case BGWorkerMehtod_LoadMachinesAndPrinters:
                    e.Result = DoLoadMachinesAndPrinters(worker, Database.ContextIPlus);
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
            if (e.Error != null)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format(@"Error by doing {0}! Message:{1}", command, e.Error.Message) });
            }
            else
            {
                switch (command)
                {
                    case BGWorkerMehtod_LoadMachinesAndPrinters:
                        BSOPrinterWorkerResult = (WorkerResult)e.Result;
                        _PrintServerList = BSOPrinterWorkerResult.Printers;
                        _MachineList = BSOPrinterWorkerResult.Machines;
                        OnPropertyChanged("PrintServerList");
                        OnPropertyChanged("MachineList");
                        break;
                }
            }
        }

        private WorkerResult DoLoadMachinesAndPrinters(ACBackgroundWorker worker, Database database)
        {
            // TODO: SASA


            WorkerResult workerResult = new WorkerResult();
            //workerResult.Printers = PrintManager.GetPrintServers();
            //workerResult.Machines = new List<ACItem>();

            //gip.core.datamodel.ACClass acClassPAClassPhysicalBase = gip.core.datamodel.Database.GlobalDatabase.GetACType(typeof(PAClassPhysicalBase));
            //IQueryable<gip.core.datamodel.ACClass> queryClasses = FacilityManager.s_cQry_GetAvailableModulesAsACClass(Database.ContextIPlus, acClassPAClassPhysicalBase.ACIdentifier);
            //if (queryClasses != null && queryClasses.Any())
            //{
            //    gip.core.datamodel.ACClass[] aCClasses = queryClasses.ToArray();
            //    foreach (gip.core.datamodel.ACClass cClass in aCClasses)
            //    {
            //        ACItem aCItem = new ACItem();
            //        aCItem.ReadACClassData(cClass);
            //        workerResult.Machines.Add(aCItem);
            //    }
            //}
            return workerResult;
        }

        protected class WorkerResult
        {
            public List<PrinterInfo> Printers { get; set; }
            public List<ACItem> Machines { get; set; }
        }
        #endregion

    }
}

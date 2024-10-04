using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Timers;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Unter-BSO für VBBSOControlDialog
    /// Wird verwendet für PABase (Modelwelt) und Ableitungen
    /// 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Alarm query'}de{'Alarmabfrage'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    [ACClassConstructorInfo(
        new object[] 
        { 
            new object[] {Const.ACUrlPrefix, Global.ParamOption.Optional, typeof(String)}
        }
    )]
    public class VBBSOControlPA : ACBSO
    {
        #region c'tors
        public VBBSOControlPA(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            SearchFrom = DateTime.Now;
            SearchTo = DateTime.Now;
            _EventSubscr = new ACPointEventSubscr(this, "EventSubscr", 0);
            QueryAlarms = true;

            String acUrl = ParameterValue(Const.ACUrlPrefix) as String;

            if (!String.IsNullOrEmpty(acUrl))
            {
                IACComponent componentToQuery = ACUrlCommand(acUrl) as IACComponent;
                if (componentToQuery != null)
                {
                    _StaticComponentToQuery = new ACRef<IACComponent>(componentToQuery, this);
                }
            }
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += BGWorker_QueryServerComponent;
            _backgroundWorker.RunWorkerCompleted += BGWorker_QueryServerCompleted;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.RunWorkerAsync();
            return true;
        }
        BackgroundWorker _backgroundWorker = null;


        public override bool ACPostInit()
        {
            if (StaticComponentToQuery != null)
                CurrentACComponent = StaticComponentToQuery;
            else if (SelectionManager != null)
            {
                SelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
                if ((this.SelectionManager as VBBSOSelectionManager).ShowACObjectForSelection != null)
                {
                    CurrentACComponent = (this.SelectionManager as VBBSOSelectionManager).ShowACObjectForSelection;
                }
            }
            // TODO: Init-Query-Request
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_backgroundWorker != null)
            {
                _backgroundWorker.CancelAsync();
                _backgroundWorker.DoWork -= BGWorker_QueryServerComponent;
                _backgroundWorker.RunWorkerCompleted -= BGWorker_QueryServerCompleted;
            }
            _backgroundWorker = null;
            
            if (_SelectionManager != null)
            {
                _SelectionManager.Detach();
                _SelectionManager.ObjectDetaching -= _SelectionManager_ObjectDetaching;
                _SelectionManager.ObjectAttached -= _SelectionManager_ObjectAttached;
                _SelectionManager = null;
            }

            if (_CurrentACComponent != null)
            {
                EventSubscription.UnSubscribeAllEvents(_CurrentACComponent.ValueT as IACComponent);
                _CurrentACComponent.Detach();
                _CurrentACComponent = null;
            }

            if (_StaticComponentToQuery != null)
            {
                _StaticComponentToQuery.Detach();
                _StaticComponentToQuery = null;
            }
            ACSaveOrUndoChanges();
            this._ACMsgAlarmList = null;
            this._CurrentACComponent = null;
            this._CurrentACMsgAlarm = null;
            this._CurrentInPoint = null;
            this._CurrentMsgAlarmLog = null;
            this._CurrentOutPoint = null;
            this._EventSubscr = null;
            this._MsgAlarmLogList = null;
            this._QueryResult = null;
            this._SelectedACMsgAlarm = null;
            this._SelectedInPoint = null;
            this._SelectedInPointConfig = null;
            this._SelectedMsgAlarmLog = null;
            this._SelectedOutPoint = null;
            this._SelectedOutPointConfig = null;
            this._StaticComponentToQuery = null;
            this._Update = null;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Selection-Manager
        private ACRef<IACComponent> _StaticComponentToQuery = null;
        public IACComponent StaticComponentToQuery
        {
            get
            {
                if (_StaticComponentToQuery == null)
                    return null;
                return _StaticComponentToQuery.ValueT;
            }
        }

        private ACRef<VBBSOSelectionManager> _SelectionManager;
        public VBBSOSelectionManager SelectionManager
        {
            get
            {
                if (StaticComponentToQuery != null)
                    return null;
                if (_SelectionManager != null)
                    return _SelectionManager.ValueT;
                if (ParentACComponent != null)
                {
                    VBBSOSelectionManager subACComponent = ParentACComponent.GetChildComponent(SelectionManagerACName) as VBBSOSelectionManager;
                    if (subACComponent == null)
                    {
                        if (ParentACComponent is VBBSOSelectionDependentDialog)
                        {
                            subACComponent = (ParentACComponent as VBBSOSelectionDependentDialog).SelectionManager;
                        }
                        else
                            subACComponent = ParentACComponent.StartComponent(SelectionManagerACName, null, null) as VBBSOSelectionManager;
                    }
                    if (subACComponent != null)
                    {
                        _SelectionManager = new ACRef<VBBSOSelectionManager>(subACComponent, this);
                        _SelectionManager.ObjectDetaching += new EventHandler(_SelectionManager_ObjectDetaching);
                        _SelectionManager.ObjectAttached += new EventHandler(_SelectionManager_ObjectAttached);
                    }
                }
                if (_SelectionManager == null)
                    return null;
                return _SelectionManager.ValueT;
            }
        }

        void _SelectionManager_ObjectAttached(object sender, EventArgs e)
        {
            if (SelectionManager != null)
                SelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
        }

        void _SelectionManager_ObjectDetaching(object sender, EventArgs e)
        {
            if (SelectionManager != null)
                SelectionManager.PropertyChanged -= SelectionManager_PropertyChanged;
        }

        private string SelectionManagerACName
        {
            get
            {
                string acInstance = ACUrlHelper.ExtractInstanceName(this.ACIdentifier);
                if (String.IsNullOrEmpty(acInstance))
                    return "VBBSOSelectionManager";
                else
                    return "VBBSOSelectionManager(" + acInstance + ")";
            }
        }

        void SelectionManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowACObjectForSelection")
            {
                CurrentACComponent = SelectionManager.ShowACObjectForSelection;
            }
            //if (e.PropertyName == "SelectedVBControl")
            //{
            //    if ((this.SelectionManager as SelectionManager).SelectedVBControl != null)
            //        CurrentACComponent = (this.SelectionManager as SelectionManager).SelectedVBControl.ContextACObject;
            //    else
            //        CurrentACComponent = null;
            //}
        }
        #endregion

        #region BSO->ACProperty
        ACRef<IACObject> _CurrentACComponent;
        [ACPropertyInfo(9999)]
        public IACObject CurrentACComponent
        {
            get
            {
                if (_CurrentACComponent == null)
                    return null;
                return _CurrentACComponent.ValueT;
            }
            set
            {
                bool objectSwapped = true;
                if (_CurrentACComponent != null)
                {
                    if (_CurrentACComponent != value)
                    {
                        EventSubscription.UnSubscribeAllEvents(_CurrentACComponent.ValueT as IACComponent);
                        _CurrentACComponent.Detach();
                    }
                    else
                        objectSwapped = false;
                }
                if (value == null)
                    _CurrentACComponent = null;
                else
                    _CurrentACComponent = new ACRef<IACObject>(value, this);
                if (_CurrentACComponent != null)
                {
                    if (objectSwapped)
                    {
                        EventSubscription.SubscribeEvent(_CurrentACComponent.ValueT, "AlarmChangedEvent", EventCallback);
                        EventSubscription.SubscribeEvent(_CurrentACComponent.ValueT, "SubAlarmsChangedEvent", EventCallback);
                        StartNewQueryRequest(true);
                    }
                }
                if (objectSwapped && _LastDBSearch)
                {
                    ResetSearch();
                }
                OnPropertyChanged("CurrentACComponent");
            }
        }
        #endregion

        #region Event-Handling
        ACPointEventSubscr _EventSubscr;
        [ACPropertyEventPointSubscr(9999, false)]
        public ACPointEventSubscr EventSubscr
        {
            get
            {
                return _EventSubscr;
            }
        }

        [ACMethodInfo("Function", "en{'EventCallback'}de{'EventCallback'}", 9999)]
        public void EventCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                if (    (CurrentACComponent != null)
                     && (   (QueryAlarms && (sender.ACIdentifier == "AlarmChangedEvent"))
                         || (QuerySubAlarms && (sender.ACIdentifier == "SubAlarmsChangedEvent")))
                    )
                {
                    StartNewQueryRequest(false);
                }
            }
        }

        private DateTime _LastQueryTime = DateTime.MinValue;
        private bool IsRefreshCycleElapsed
        {
            get
            {
                if (_LastQueryTime == DateTime.MinValue)
                    return true;
                TimeSpan diff = DateTime.Now - _LastQueryTime;
                return diff.IsRefreshRateCycleElapsed(RequiredUpdateRate);
            }
        }

        private bool _DoNewQuery = false;
        private void StartNewQueryRequest(bool force)
        {
            if (force)
                _LastQueryTime = DateTime.MinValue;
            _DoNewQuery = true;
        }

        private bool _QueryDone = false;
        private List<Msg> _QueryResult = null;
        void BGWorker_QueryServerComponent(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(100);
            if (IsRefreshCycleElapsed && _DoNewQuery && CurrentACComponent != null)
            {
                _QueryResult = CurrentACComponent.ACUrlCommand("!GetAlarms", QueryAlarms, QuerySubAlarms, false) as List<Msg>;

                _LastQueryTime = DateTime.Now;
                _DoNewQuery = false;
                _QueryDone = true;
            }
        }

        void BGWorker_QueryServerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (_QueryDone)
                {
                    if (_QueryResult != null)
                    {
                        _ACMsgAlarmList.Clear();
                        foreach (var c in _QueryResult)
                        {
                            _ACMsgAlarmList.Add(c);
                        }
                    }
                    else
                    {
                        _ACMsgAlarmList.Clear();
                    }
                    _QueryDone = false;
                }
                if (_backgroundWorker != null)
                    _backgroundWorker.RunWorkerAsync();
            }
        }

        #endregion

        #region Properties
        #region Configuration
        [ACPropertyInfo(9999, DefaultValue = Global.MaxRefreshRates.R5sec)]
        public Global.MaxRefreshRates RequiredUpdateRate
        {
            get;
            set;
        }
        #endregion

        #region Filter
        bool _QueryAlarms = false;
        [ACPropertyInfo(9999, "", "en{'View/Query Alarms'}de{'Alarme anzeigen/abfragen'}")]
        public bool QueryAlarms
        {
            get
            {
                return _QueryAlarms;
            }
            set
            {
                _QueryAlarms = value;
                OnPropertyChanged("QueryAlarms");
                StartNewQueryRequest(true);
            }
        }

        bool _QuerySubAlarms = false;
        [ACPropertyInfo(9999, "", "en{'View/Query Sub-Alarms'}de{'Unteralarme anzeigen/abfragen'}")]
        public bool QuerySubAlarms
        {
            get
            {
                return _QuerySubAlarms;
            }
            set
            {
                _QuerySubAlarms = value;
                OnPropertyChanged("QuerySubAlarms");
                StartNewQueryRequest(true);
            }
        }
        #endregion

        #region AlarmListe
        Msg _CurrentACMsgAlarm;
        [ACPropertyCurrent(9999, "Msg")]
        public Msg CurrentACMsgAlarm
        {
            get
            {
                return _CurrentACMsgAlarm;
            }
            set
            {
                _CurrentACMsgAlarm = value;
                OnPropertyChanged("CurrentACMsgAlarm");
            }
        }

        private ObservableCollection<Msg> _ACMsgAlarmList = new ObservableCollection<Msg>();
        [ACPropertyList(9999, "Msg")]
        public ObservableCollection<Msg> ACMsgAlarmList
        {
            get
            {
                return _ACMsgAlarmList;
            }
        }


        Msg _SelectedACMsgAlarm;
        [ACPropertySelected(9999, "Msg")]
        public Msg SelectedACMsgAlarm
        {
            get
            {
                return _SelectedACMsgAlarm;
            }
            set
            {
                _SelectedACMsgAlarm = value;
                OnPropertyChanged("SelectedACMsgAlarm");
            }
        }

        [ACMethodInteraction("Msg", "en{'Acknowledge'}de{'Quittiere'}", (short)MISort.Delete, true, "CurrentACMsgAlarm", Global.ACKinds.MSMethodPrePost)]
        public void AcknowledgeCurrent()
        {
            if (!IsEnabledAcknowledgeCurrent())
                return;
            if (!PreExecute("AcknowledgeCurrent")) return;
            MsgList alarmListToAck = new MsgList();
            alarmListToAck.Add(CurrentACMsgAlarm);
            CurrentACComponent.ACUrlCommand("!AcknowledgeSubAlarms", alarmListToAck);
            PostExecute("AcknowledgeCurrent");
        }

        public bool IsEnabledAcknowledgeCurrent()
        {
            return (CurrentACComponent != null) && (CurrentACMsgAlarm != null);
        }

        [ACMethodInteraction("Msg", "en{'Acknowledge all'}de{'Quittiere alle'}", 9999, true, "", Global.ACKinds.MSMethodPrePost)]
        public void AcknowledgeAll()
        {
            if (!IsEnabledAcknowledgeAll())
                return;
            if (!PreExecute("AcknowledgeAll")) return;
            CurrentACComponent.ACUrlCommand("!AcknowledgeAllAlarms");
            PostExecute("AcknowledgeAll");
        }

        public bool IsEnabledAcknowledgeAll()
        {
            return (CurrentACComponent != null) && (ACMsgAlarmList.Any());
        }

        #endregion

        #region Archive

        DateTime _SearchFrom;
        [ACPropertyInfo(9999, "", "en{'Search from'}de{'Suche von'}")]
        public DateTime SearchFrom
        {
            get
            {
                return _SearchFrom;
            }
            set
            {
                _SearchFrom = value;
                OnPropertyChanged("SearchFrom");
            }
        }

        DateTime _SearchTo;
        [ACPropertyInfo(9999, "", "en{'Search to'}de{'Suche bis'}")]
        public DateTime SearchTo
        {
            get
            {
                return _SearchTo;
            }
            set
            {
                _SearchTo = value;
                OnPropertyChanged("SearchTo");
            }
        }


        MsgAlarmLog _CurrentMsgAlarmLog;
        [ACPropertyCurrent(9999, "MsgAlarmLog")]
        public MsgAlarmLog CurrentMsgAlarmLog
        {
            get
            {
                return _CurrentMsgAlarmLog;
            }
            set
            {
                if (_CurrentMsgAlarmLog != value)
                {
                    _CurrentMsgAlarmLog = value;
                    OnPropertyChanged("CurrentMsgAlarmLog");
                }
            }
        }

        private IEnumerable<MsgAlarmLog> _MsgAlarmLogList = new List<MsgAlarmLog>();
        [ACPropertyList(9999, "MsgAlarmLog")]
        public IEnumerable<MsgAlarmLog> MsgAlarmLogList
        {
            get
            {
                return _MsgAlarmLogList;
            }
        }

        MsgAlarmLog _SelectedMsgAlarmLog;
        [ACPropertySelected(9999, "MsgAlarmLog")]
        public MsgAlarmLog SelectedMsgAlarmLog
        {
            get
            {
                return _SelectedMsgAlarmLog;
            }
            set
            {
                _SelectedMsgAlarmLog = value;
                OnPropertyChanged("SelectedMsgAlarmLog");
            }
        }

        [ACMethodCommand("MsgAlarmLog", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            if (!IsEnabledSearch())
                return;

            string acUrl = string.Empty;
            Guid currentComponentClassID = Guid.Empty;

            if(CurrentACComponent != null)
            {
                ACComponent component = CurrentACComponent as ACComponent;
                if (component != null && component.ComponentClass != null)
                {
                    currentComponentClassID = component.ComponentClass.ACClassID;
                    acUrl = component.ACUrl;
                }
            }

            if (!this.QuerySubAlarms)
            {
                var query = Database.ContextIPlus.MsgAlarmLog.Where(c => c.TimeStampOccurred >= SearchFrom && c.TimeStampOccurred <= SearchTo && c.ACClass != null && 
                                                                         c.ACClassID == currentComponentClassID).AsQueryable();
                //TODO EFCore not implemented
                //(query as ObjectQuery).MergeOption = MergeOption.OverwriteChanges;
                _MsgAlarmLogList = query.ToList();
            }
            else
            {
                var query = Database.ContextIPlus.MsgAlarmLog.Where(c => c.TimeStampOccurred >= SearchFrom && c.TimeStampOccurred <= SearchTo).AsQueryable();

                //TODO EFCore not implemented
                //(query as ObjectQuery).MergeOption = MergeOption.OverwriteChanges;
                _MsgAlarmLogList = query.ToList().Where(c => (c.ACClass != null && c.ACClass.ACUrlComponent.StartsWith(acUrl)) || 
                                                             (c.ACProgramLog != null && c.ACProgramLog.ACUrl.StartsWith(acUrl)));
            }
            _LastDBSearch = true;
            OnPropertyChanged("MsgAlarmLogList");
        }

        public bool IsEnabledSearch()
        {
            return ((CurrentACComponent != null) && (SearchFrom != null) && (SearchTo != null));
        }

        private bool _LastDBSearch = false;
        private void ResetSearch()
        {
            _MsgAlarmLogList = new List<MsgAlarmLog>();
            _LastDBSearch = false;
            OnPropertyChanged("MsgAlarmLogList");
        }

        #endregion

        #endregion

        #region Design
        string _Update = "";
        public string CurrentLayout
        {
            get
            {
                if (!(ParentACComponent is VBBSOSelectionDependentDialog))
                    return "";

                string layoutXAML = "";

                ACClassDesign acClassDesign = CurrentACComponent.ACType.GetDesign(CurrentACComponent, Global.ACUsages.DUControlDialog, Global.ACKinds.DSDesignLayout);
                if (acClassDesign != null)
                    layoutXAML += acClassDesign.XMLDesign + _Update;

                // Sonst reagiert das Steuerelement nicht aufs PropertyChanged
                _Update = _Update == "" ? " " : "";
                return layoutXAML;
            }
        }
        #endregion

        #region Parameters-Config

        #region InPoints

        ObservableCollection<ACValue> tempConfigList;
        ACClass _CurrentInPoint = null;
        [ACPropertyList(9999, "Param", "en{'Sources'}de{'Quellen'}")]
        public IEnumerable<ACClass> InPointList
        {
            get 
            {
                IEnumerable<ACClass> _inPointList = null;
                if (CurrentACComponent == null)
                {
                    return null;
                }
                _CurrentInPoint = (CurrentACComponent.ACType as ACClass);

                using (ACMonitor.Lock(Database.QueryLock_1X000))
                {
                    _inPointList = _CurrentInPoint.ACClassPropertyRelation_TargetACClass
                        .Where(c => c.ConnectionTypeIndex == (short)Global.ConnectionTypes.ConnectionPhysical || c.ConnectionTypeIndex == (short)Global.ConnectionTypes.DynamicConnection)
                        .Join(Database.ContextIPlus.ACClass, x => x.SourceACClassID, y => y.ACClassID, (x, y) => new { y })
                        .Select(x => x.y)
                        .AsEnumerable()
                        .Where(c => c.ACKind == Global.ACKinds.TPAProcessModule)
                        .OrderBy(c => c.ACIdentifier);
                }
                return _inPointList;
            }
        }

        private ACClass _SelectedInPoint;
        [ACPropertySelected(9999, "Param", "en{'Source'}de{'Quelle'}")]
        public ACClass SelectedInPoint
        { 
            get
            {
                return _SelectedInPoint;
            }
            set 
            {
                if (SelectedInPoint != value)
                {
                    _SelectedInPoint = value;
                    OnPropertyChanged("InPointConfigList");
                    tempConfigList = null;
                }
            }
        }

        [ACPropertyList(9999, "ParamC")]
        public IEnumerable<ACValue> InPointConfigList
        {
            get
            {
                ACValueList ConfigList = null;
                if (SelectedInPoint != null)
                {

                    using (ACMonitor.Lock(Database.QueryLock_1X000))
                    {
                        var relation = Database.ContextIPlus.ACClassPropertyRelation.Where(c => c.SourceACClassID == SelectedInPoint.ACClassID && c.TargetACClassID == _CurrentInPoint.ACClassID).FirstOrDefault();
                        if (relation == null)
                            return ConfigList;
                        ACClassConfig acClassConfig = relation.ACClassConfig_ACClassPropertyRelation.FirstOrDefault();
                        if (acClassConfig != null)
                        {
                            ACMethod acMethodConfig = acClassConfig.Value as ACMethod;
                            if (acMethodConfig != null)
                                ConfigList = acMethodConfig.ParameterValueList;
                        }
                    }
                }
                return ConfigList;
            }
        }

        private ACValue _SelectedInPointConfig;
        [ACPropertySelected(9999, "ParamC")]
        public ACValue SelectedInPointConfig
        {
            get
            {
                return _SelectedInPointConfig;
            }
            set
            {
                if (_SelectedInPointConfig != value)
                {
                    _SelectedInPointConfig = value;
                }
                OnPropertyChanged("SelectedInPointConfig");
                OnPropertyChanged("CurrentInPointConfig");
            }
        }


        [ACPropertyCurrent(999, "ParamC", "en{'Value for mass update'}de{'Wert für massenaktualisierung'}")]
        public object CurrentInPointConfig
        {
            get 
            {
                if(_SelectedInPointConfig != null)
                    return _SelectedInPointConfig.Value;
                return null;
            }
            set 
            {
                _SelectedInPointConfig.Value = value;
                if (tempConfigList == null)
                {
                    tempConfigList = new ObservableCollection<datamodel.ACValue>();
                }
                if (tempConfigList.Any(c => c.ACIdentifier == _SelectedInPointConfig.ACIdentifier))
                    tempConfigList.Remove(_SelectedInPointConfig);
                tempConfigList.Add(_SelectedInPointConfig);
            }
        }

#endregion

#region OutPoints

        ObservableCollection<ACValue> tempConfigOutPointList;
        ACClass _CurrentOutPoint = null;
        [ACPropertyList(9999, "ParamO", "en{'Targets'}de{'Ziele'}")]
        public IEnumerable<ACClass> OutPointList
        {
            get
            {
                IEnumerable<ACClass> _OutPointList = null;
                if (CurrentACComponent == null)
                    return null;
                _CurrentOutPoint = (CurrentACComponent.ACType as ACClass);

                using (ACMonitor.Lock(Database.QueryLock_1X000))
                {
                    _OutPointList = _CurrentOutPoint.ACClassPropertyRelation_SourceACClass
                        .Where(c => c.ConnectionTypeIndex == (short)Global.ConnectionTypes.ConnectionPhysical || c.ConnectionTypeIndex == (short)Global.ConnectionTypes.DynamicConnection)
                        .Join(Database.ContextIPlus.ACClass, x => x.TargetACClassID, y => y.ACClassID, (x, y) => new { y })
                        .Select(x => x.y)
                        .AsEnumerable()
                        .Where(c => c.ACKind == Global.ACKinds.TPAProcessModule)
                        .OrderBy(c => c.ACIdentifier);
                }
                return _OutPointList;
            }
        }

        private ACClass _SelectedOutPoint;
        [ACPropertySelected(9999, "ParamO", "en{'Target'}de{'Ziel'}")]
        public ACClass SelectedOutPoint
        {
            get
            {
                return _SelectedOutPoint;
            }
            set
            {
                if (SelectedOutPoint != value)
                {
                    _SelectedOutPoint = value;
                    OnPropertyChanged("OutPointConfigList");
                    tempConfigOutPointList = null;
                }
            }
        }

        [ACPropertyList(9999, "ParamOC")]
        public IEnumerable<ACValue> OutPointConfigList
        {
            get
            {
                ACValueList ConfigList = null;
                if (SelectedOutPoint != null)
                {

                    using (ACMonitor.Lock(Database.QueryLock_1X000))
                    {
                        var relation = Database.ContextIPlus.ACClassPropertyRelation.Where(c => c.TargetACClassID == SelectedOutPoint.ACClassID && c.SourceACClassID == _CurrentOutPoint.ACClassID).FirstOrDefault();
                        if (relation == null)
                            return ConfigList;
                        ACClassConfig acClassConfig = relation.ACClassConfig_ACClassPropertyRelation.FirstOrDefault();
                        if (acClassConfig != null)
                        {

                            ACMethod acMethodConfig = acClassConfig.Value as ACMethod;
                            if (acMethodConfig != null)
                                ConfigList = acMethodConfig.ParameterValueList;
                        }
                    }
                }
                return ConfigList;
            }
        }

        private ACValue _SelectedOutPointConfig;
        [ACPropertySelected(9999, "ParamOC")]
        public ACValue SelectedOutPointConfig
        {
            get
            {
                return _SelectedOutPointConfig;
            }
            set
            {
                if (_SelectedOutPointConfig != value)
                {
                    _SelectedOutPointConfig = value;
                }
                OnPropertyChanged("SelectedOutPointConfig");
                OnPropertyChanged("CurrentOutPointConfig");
            }
        }

        [ACPropertyCurrent(999, "ParamOC", "en{'Value for mass update'}de{'Wert für massenaktualisierung'}")]
        public object CurrentOutPointConfig
        {
            get
            {
                if (_SelectedOutPointConfig != null)
                    return _SelectedOutPointConfig.Value;
                return null;
            }
            set
            {
                if (_SelectedOutPointConfig == null)
                    return;
                _SelectedOutPointConfig.Value = value;
                if (tempConfigOutPointList == null)
                {
                    tempConfigOutPointList = new ObservableCollection<datamodel.ACValue>();
                }
                if (tempConfigOutPointList.Any(c => c.ACIdentifier == _SelectedOutPointConfig.ACIdentifier))
                    tempConfigOutPointList.Remove(_SelectedOutPointConfig);
                tempConfigOutPointList.Add(_SelectedOutPointConfig);
            }
        }

#endregion

#region Methods

        [ACMethodInfo("ParamC", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void SaveConfig()
        {
            ACSaveChanges();
        }

        IEnumerable<object> queryACClassConfig;
        [ACMethodInfo("ParamC", "en{'Mass update'}de{'Massenaktualisierung'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MassUpdateInPoint()
        {
            queryACClassConfig = null;

            using (ACMonitor.Lock(Database.QueryLock_1X000))
            {
                queryACClassConfig = InPointList
                    .Join(Database.ContextIPlus.ACClassPropertyRelation.Where(y => y.TargetACClassID == ((ACClass)CurrentACComponent.ACType).ACClassID), c => c.ACClassID, x => x.SourceACClassID, (c, x) => new { x })
                    .Select(c => c.x).Join(Database.ContextIPlus.ACClassConfig, k => k.ACClassPropertyRelationID, n => n.ACClassPropertyRelationID, (k, n) => new { n })
                    .Select(k => k.n).Cast<ACClassConfig>().Select(u => u.Value);
            }
            if (queryACClassConfig != null && tempConfigList != null)
                MassUpdate(queryACClassConfig, tempConfigList);
        }

        public bool IsEnabledMassUpdateInPoint()
        {
            if (tempConfigList != null)
                return true;
            else
                return false;
        }

        [ACMethodInfo("ParamC", "en{'Mass update'}de{'Massenaktualisierung'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MassUpdateOutPoint()
        {
            queryACClassConfig = null;

            using (ACMonitor.Lock(Database.QueryLock_1X000))
            {
                queryACClassConfig = OutPointList
                    .Join(Database.ContextIPlus.ACClassPropertyRelation.Where(y => y.SourceACClassID == ((ACClass)CurrentACComponent.ACType).ACClassID), c => c.ACClassID, x => x.TargetACClassID, (c, x) => new { x })
                    .Select(c => c.x).Join(Database.ContextIPlus.ACClassConfig, k => k.ACClassPropertyRelationID, n => n.ACClassPropertyRelationID, (k, n) => new { n })
                    .Select(k => k.n).Cast<ACClassConfig>().Select(u => u.Value);
            }
            if (queryACClassConfig != null && tempConfigOutPointList != null)
                MassUpdate(queryACClassConfig, tempConfigOutPointList);
        }

        public bool IsEnabledMassUpdateOutPoint()
        {
            if (tempConfigOutPointList != null)
                return true;
            else
                return false;
        }

        public void MassUpdate(IEnumerable<object> queryACClassConfig, ObservableCollection<ACValue> tempList)
        {

            using (ACMonitor.Lock(Database.QueryLock_1X000))
            {
                if (queryACClassConfig != null)
                {
                    List<ACMethod> queryACMethod = queryACClassConfig.Cast<ACMethod>().ToList();
                    foreach (var updateValue in tempList)
                    {
                        foreach (var method in queryACMethod)
                        {
                            var changedParam = method.ParameterValueList.Where(c => c.ACIdentifier == updateValue.ACIdentifier);
                            foreach (var originalValue in changedParam)
                            {
                                if (originalValue.ACIdentifier == updateValue.ACIdentifier)
                                    originalValue.Value = updateValue.Value;
                            }
                        }
                    }
                }
            }
            OnSave();
            tempConfigList = null;
            tempConfigOutPointList = null;
        }
#endregion

#endregion

        #region ACProgramLog

        [ACMethodInfo("","en{'Show logs'}de{'Protokolle anzeigen'}",999)]
        public void ShowACProgramLog()
        {
            if (CurrentACComponent != null && SearchFrom < SearchTo)
            {
                short searchMode = 0;
                if ((SearchTo - SearchFrom).TotalDays >= 11)
                {
                    Global.MsgResult result = Messages.Question(this, "Warning50010", Global.MsgResult.No );
                    if (result == Global.MsgResult.No)
                        return;
                    // If you want to find the last entry, then click YES. If you want to search for the last entry from the current time period, press NO. With CANCEL, the search is carried out in exactly the specified period.
                    result = Messages.YesNoCancel(this, "Question50105", Global.MsgResult.No);
                    if (result == Global.MsgResult.Yes)
                        searchMode = 1;
                    if (result == Global.MsgResult.No)
                        searchMode = 2;
                }

                ACClass componentClass = (CurrentACComponent as ACComponent).ComponentClass;
                bool isProcessModule = componentClass.ACKind == Global.ACKinds.TPAProcessModule;

                PAShowDlgManagerBase service = PAShowDlgManagerBase.GetServiceInstance(this);
                if (service != null)
                {
                    ACValueList param = new ACValueList();
                    param.Add(new ACValue("ComponentACUrl", CurrentACComponent.GetACUrl()));
                    param.Add(new ACValue("SearchFrom", SearchFrom));
                    param.Add(new ACValue("SearchTo", SearchTo));
                    param.Add(new ACValue("SearchMode", searchMode));
                    param.Add(new ACValue("IsForProcessModule", isProcessModule));
                    service.ShowProgramLogViewer(this as IACComponent, param);
                }
            }

        }

        //public bool IsEnabledShowACProgramLog()
        //{
        //    return Database.ContextIPlus.ACProgramLog.Any(c => c.ACUrl.Contains(CurrentACComponent.ACIdentifier) && c.StartDate >= SearchFrom && c.EndDate <= SearchTo);
        //}

        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Search":
                    Search();
                    return true;
                case "SaveConfig":
                    SaveConfig();
                    return true;
                case "MassUpdateInPoint":
                    MassUpdateInPoint();
                    return true;
                case "MassUpdateOutPoint":
                    MassUpdateOutPoint();
                    return true;
                case "ShowACProgramLog":
                    ShowACProgramLog();
                    return true;
                case "AcknowledgeCurrent":
                    AcknowledgeCurrent();
                    return true;
                case "AcknowledgeAll":
                    AcknowledgeAll();
                    return true;
                case "EventCallback":
                    EventCallback(acParameter[0] as IACPointNetBase, acParameter[1] as ACEventArgs, acParameter[2] as IACObject);
                    return true;
                case Const.IsEnabledPrefix + "Search":
                    result = IsEnabledSearch();
                    return true;
                case Const.IsEnabledPrefix + "MassUpdateInPoint":
                    result = IsEnabledMassUpdateInPoint();
                    return true;
                case Const.IsEnabledPrefix + "MassUpdateOutPoint":
                    result = IsEnabledMassUpdateOutPoint();
                    return true;
                case Const.IsEnabledPrefix + "AcknowledgeCurrent":
                    result = IsEnabledAcknowledgeCurrent();
                    return true;
                case Const.IsEnabledPrefix + "AcknowledgeAll":
                    result = IsEnabledAcknowledgeAll();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion
    }
}

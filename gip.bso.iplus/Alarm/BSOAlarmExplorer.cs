// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Security.Claims;

namespace gip.bso.iplus
{
    public interface IACBSOAlarmPresenter : IACBSO
    {
        void SwitchToViewOnAlarm(Msg msgAlarm);
    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'Alarm explorer'}de{'Alarm explorer'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, false, true)]
    public class BSOAlarmExplorer : ACBSO
    {
        #region c'tors

        public BSOAlarmExplorer(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") : 
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _EventSubscr = new ACPointEventSubscr(this, "EventSubscr", 0);
            _CN_BSOProcessControl = new ACPropertyConfigValue<string>(this, "CN_BSOProcessControl", "");
            _CN_BSOVisualisation = new ACPropertyConfigValue<string>(this, "CN_BSOVisualisation", "");
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            if (SearchFrom == DateTime.MinValue)
                SearchFrom = DateTime.Now.AddDays(-7);

            if (SearchTo == DateTime.MinValue)
                SearchTo = DateTime.Now;

            InitializeBackgroundWorker();

            return true;
        }

        BackgroundWorker _backgroundWorker = null;

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_backgroundWorker != null)
            {
                _backgroundWorker.CancelAsync();
                _backgroundWorker.DoWork -= BGWorker_QueryServerComponent;
                _backgroundWorker.RunWorkerCompleted -= BGWorker_QueryServerCompleted;
            }
            _backgroundWorker = null;

            UnSubscribeEvents();

            ACSaveOrUndoChanges();
            this._ACMsgAlarmList = null;
            this._CurrentACMsgAlarm = null;
            this._EventSubscr = null;
            this._QueryResultCache = null;
            this._QueryResultNew = null;
            this._SelectedACMsgAlarm = null;
            this._AppManagerInvokers = null;
            this._MsgAlarmLogList = null;
            this._MessageLevelList = null;
            this._SelectedMessageLevel = null;
            this._SelectedMsgAlarmLog = null;
            this._CurrentACProject = null;
            this._ACProjectList = null;
            this._SelectedAlarmLogStatistic = null;
            this._AlarmLogStatisticList = null;
            bool done = base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return done;
        }

        #endregion

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

        #region Config
        private ACPropertyConfigValue<string> _CN_BSOProcessControl;
        [ACPropertyConfig("en{'Classname BSOProcessControl'}de{'Klassenname BSOProcessControl'}")]
        public string CN_BSOProcessControl
        {
            get
            {
                if (!String.IsNullOrEmpty(_CN_BSOProcessControl.ValueT))
                    return _CN_BSOProcessControl.ValueT;
                gip.core.datamodel.ACClass classOfBso = typeof(BSOProcessControl).GetACType() as gip.core.datamodel.ACClass;
                if (classOfBso != null)
                {
                    var derivation = gip.core.datamodel.Database.GlobalDatabase.ACClass
                                                .Where(c => c.BasedOnACClassID == classOfBso.ACClassID
                                                        && !String.IsNullOrEmpty(c.AssemblyQualifiedName)
                                                        && c.AssemblyQualifiedName != classOfBso.AssemblyQualifiedName).FirstOrDefault();
                    if (derivation != null)
                        _CN_BSOProcessControl.ValueT = derivation.ACIdentifier;
                    else
                        _CN_BSOProcessControl.ValueT = classOfBso.ACIdentifier;
                    return _CN_BSOProcessControl.ValueT;
                }
                return BSOProcessControl.BSOClassName;
            }
            set { _CN_BSOProcessControl.ValueT = value; }
        }

        private ACPropertyConfigValue<string> _CN_BSOVisualisation;
        [ACPropertyConfig("en{'Classname BSOVisualisation'}de{'Klassenname BSOVisualisation'}")]
        public string CN_BSOVisualisation
        {
            get
            {
                if (!String.IsNullOrEmpty(_CN_BSOVisualisation.ValueT))
                    return _CN_BSOVisualisation.ValueT;
                gip.core.datamodel.ACClass classOfBso = typeof(BSOVisualisationStudio).GetACType() as gip.core.datamodel.ACClass;
                if (classOfBso != null)
                {
                    var derivation = gip.core.datamodel.Database.GlobalDatabase.ACClass
                                                .Where(c => c.BasedOnACClassID == classOfBso.ACClassID
                                                        && !String.IsNullOrEmpty(c.AssemblyQualifiedName)
                                                        && c.AssemblyQualifiedName != classOfBso.AssemblyQualifiedName).FirstOrDefault();
                    if (derivation != null)
                        _CN_BSOVisualisation.ValueT = derivation.ACIdentifier;
                    else
                        _CN_BSOVisualisation.ValueT = classOfBso.ACIdentifier;
                    return _CN_BSOVisualisation.ValueT;
                }
                return BSOVisualisationStudio.BSOClassName;
            }
            set { _CN_BSOVisualisation.ValueT = value; }
        }

        #endregion

        #region Properties

        #region Properties => Live

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

        [ACPropertyInfo(9999, DefaultValue = Global.MaxRefreshRates.R5sec)]
        public Global.MaxRefreshRates RequiredUpdateRate
        {
            get;
            set;
        }

        #endregion

        #region Properties => Archive

        ACProject _CurrentACProject;
        [ACPropertyCurrent(9999, "ACProject", "en{'Search in project'}de{'Suche im Projekt'}")]
        public ACProject CurrentACProject
        {
            get
            {
                return _CurrentACProject;
            }
            set
            {
                _CurrentACProject = value;
                OnPropertyChanged("CurrentACProject");
            }
        }

        private ACProject[] _ACProjectList;
        [ACPropertyList(9999, "ACProject")]
        public IEnumerable<ACProject> ACProjectList
        {
            get
            {
                if(_ACProjectList == null)
                    _ACProjectList = Db.ACProject.Where(c => c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application || 
                                                        c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Service).ToArray();
                return _ACProjectList;
            }
        }

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

        string _SearchText;
        [ACPropertyInfo(9999, "", "en{'Search text'}de{'Suchtext'}")]
        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        private List<MsgAlarmLog> _MsgAlarmLogList = new List<MsgAlarmLog>();
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

        private ACValueItem _SelectedMessageLevel;
        [ACPropertySelected(9999,"MessageLevel", "en{'Message level'}de{'Nachrichtenebene'}")]
        public ACValueItem SelectedMessageLevel
        {
            get
            {
                return _SelectedMessageLevel;
            }
            set
            {
                _SelectedMessageLevel = value;
                OnPropertyChanged("SelectedMessageLevel");
            }
        }

        private ACValueItemList _MessageLevelList;
        [ACPropertyList(9999,"MessageLevel")]
        public ACValueItemList MessageLevelList
        {
            get
            {
                if(_MessageLevelList == null)
                {
                    _MessageLevelList = new ACValueItemList("eMsgLevel");
                    _MessageLevelList.Add(new ACValueItem("en{'Default'}de{'Standard'}", eMsgLevel.Default, null));
                    _MessageLevelList.Add(new ACValueItem("en{'Info'}de{'Info'}", eMsgLevel.Info, null));
                    _MessageLevelList.Add(new ACValueItem("en{'Warning'}de{'Warnung'}", eMsgLevel.Warning, null));
                    _MessageLevelList.Add(new ACValueItem("en{'Failure'}de{'Ausfall'}", eMsgLevel.Failure, null));
                    _MessageLevelList.Add(new ACValueItem("en{'Error'}de{'Fehler'}", eMsgLevel.Error, null));
                    _MessageLevelList.Add(new ACValueItem("en{'Exception'}de{'Ausnahme'}", eMsgLevel.Exception, null));
                }
                return _MessageLevelList;
            }
        }

        private string _AlarmSourceText;
        [ACPropertyInfo(9999, "", "en{'Alarm source text'}de{'Die Alarmquelle'}")]
        public string AlarmSourceText
        {
            get => _AlarmSourceText;
            set
            {
                _AlarmSourceText = value;
                OnPropertyChanged();
            }
        }


        #endregion

        #region Properties => ArchiveStatistic

        private MsgAlarmLogStatistic _SelectedAlarmLogStatistic;
        [ACPropertySelected(999, "AlarmLogStatistic")]
        public MsgAlarmLogStatistic SelectedAlarmLogStatistic
        {
            get => _SelectedAlarmLogStatistic;
            set
            {
                _SelectedAlarmLogStatistic = value;
                OnPropertyChanged("SelectedAlarmLogStatistic");
            }
        }

        private List<MsgAlarmLogStatistic> _AlarmLogStatisticList;
        [ACPropertyList(999,"AlarmLogStatistic")]
        public List<MsgAlarmLogStatistic> AlarmLogStatisticList
        {
            get => _AlarmLogStatisticList;
            set
            {
                _AlarmLogStatisticList = value;
                OnPropertyChanged("AlarmLogStatisticList");
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Methods => Live

        private void SubscribeEvents()
        {
            foreach (IACComponent appManager in Root.ACComponentChilds.Where(c => c is ApplicationManager || c is ApplicationManagerProxy))
            {
                EventSubscription.SubscribeEvent(appManager, "AlarmChangedEvent", EventCallback);
                EventSubscription.SubscribeEvent(appManager, "SubAlarmsChangedEvent", EventCallback);
            }
            StartNewQueryRequest(true);
        }

        private void UnSubscribeEvents()
        {
            foreach (IACComponent appManager in Root.ACComponentChilds.Where(c => c is ApplicationManager || c is ApplicationManagerProxy))
            {
                EventSubscription.UnSubscribeAllEvents(appManager);
            }
        }

        private void InitializeBackgroundWorker()
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += BGWorker_QueryServerComponent;
            _backgroundWorker.RunWorkerCompleted += BGWorker_QueryServerCompleted;
            _backgroundWorker.WorkerSupportsCancellation = true;
        }

        [ACMethodInfo("","",999)]
        public void ShowAlarmExplorer()
        {
            if (_backgroundWorker == null)
                InitializeBackgroundWorker();

            if (!_backgroundWorker.IsBusy)
                _backgroundWorker.RunWorkerAsync();

            SubscribeEvents();

            ShowDialog(this, "AlarmLive", "", true);

            _backgroundWorker.CancelAsync();
            UnSubscribeEvents();
        }


        [ACMethodInfo("", "en{'Acknowledge'}de{'Quittieren'}", 100)]
        public void AcknowledgeCurrent()
        {
            IACComponent component = Root.ACUrlCommand(CurrentACMsgAlarm.Source) as IACComponent;
            if (component != null)
            {
                MsgList alarmListToAck = new MsgList();
                var msgAlarm = CurrentACMsgAlarm;
                alarmListToAck.Add(msgAlarm);
                component.ACUrlCommand("!AcknowledgeSubAlarmsMsgList", alarmListToAck);
                using (ACMonitor.Lock(_60201_AppManagerInvokersLock))
                {
                    _ACMsgAlarmList.Remove(msgAlarm);
                }
            }
        }

        public bool IsEnabledAcknowledgeCurrent()
        {
            if (CurrentACMsgAlarm != null)
                return true;
            return false;
        }

        [ACMethodInfo("", "en{'Acknowledge All'}de{'Alle quittieren'}", 101)]
        public void AcknowledgeAll()
        {
            foreach (Msg msg in ACMsgAlarmList)
            {
                IACComponent component = Root.ACUrlCommand(msg.Source) as IACComponent;
                if (component != null)
                {
                    MsgList alarmListToAck = new MsgList();
                    alarmListToAck.Add(msg);
                    component.ACUrlCommand("!AcknowledgeSubAlarmsMsgList", alarmListToAck);
                }
            }
        }

        public bool IsEnabledAcknowledgeAll()
        {
            return true;
        }

        #endregion

        #region Methods => Archive

        [ACMethodInfo("", "", 999)]
        public void ShowAlarmArchiveExplorer()
        {
            if (SearchFrom == DateTime.MinValue)
                SearchFrom = DateTime.Now.AddDays(-7);

            if (SearchTo == DateTime.MinValue)
                SearchTo = DateTime.Now;

            ShowDialog(this, "AlarmArchive");

            SelectedMessageLevel = null;
            _MsgAlarmLogList = null;
            OnPropertyChanged("MsgAlarmLogList");
        }

        [ACMethodInfo("MsgAlarmLog", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            if (!IsEnabledSearch())
                return;

            IQueryable<MsgAlarmLog> query = Db.MsgAlarmLog.Where(c => c.TimeStampOccurred >= SearchFrom && c.TimeStampOccurred <= SearchTo).AsQueryable();

            if (CurrentACProject != null)
                query = query.Where(c => (c.ACClass != null && c.ACClass.ACProjectID == CurrentACProject.ACProjectID) ||
                                         (c.ACProgramLog != null && c.ACProgramLog.ACUrl.StartsWith("\\"+CurrentACProject.ACProjectName))).AsQueryable();
                //query = query.Where(c => c.Source.StartsWith("\\" + CurrentACProject.ACProjectName)).AsQueryable();

            if (SelectedMessageLevel != null)
                query = query.Where(c => c.MessageLevelIndex == (short)SelectedMessageLevel.Value).AsQueryable();

            if (!String.IsNullOrEmpty(SearchText))
                query = query.Where(c => c.Message.Contains(SearchText)).AsQueryable();

            if (!string.IsNullOrEmpty(AlarmSourceText))
                query = query.Where(c => (c.ACClass != null && c.ACClass.ACURLComponentCached == AlarmSourceText) ||
                                         (c.ACProgramLog != null && c.ACProgramLog.ACUrl == AlarmSourceText)).AsQueryable();

            //(query as ObjectQuery).MergeOption = MergeOption.OverwriteChanges;

            _MsgAlarmLogList = query.OrderByDescending(c => c.TimeStampOccurred).ToList();
            OnPropertyChanged("MsgAlarmLogList");
            BuildAlarmStatistic();
        }

        public bool IsEnabledSearch()
        {
            return SearchFrom > DateTime.MinValue && SearchTo > DateTime.MinValue;
        }

        private void BuildAlarmStatistic()
        {
            List<MsgAlarmLogStatistic> result = new List<MsgAlarmLogStatistic>();

            try
            {
                var groupedByTranslIDRowCol = _MsgAlarmLogList.GroupBy(c => new { c.TranslID, c.Column, c.Row }).ToArray();
                var emptyItem = groupedByTranslIDRowCol.FirstOrDefault(c => c.Key.Column == 0 && c.Key.Row == 0 && string.IsNullOrEmpty(c.Key.TranslID));

                foreach (var groupedItem in groupedByTranslIDRowCol)
                {
                    if (groupedItem == emptyItem)
                        continue;

                    MsgAlarmLogStatistic alarmStatistic = new MsgAlarmLogStatistic(groupedItem.ToArray());
                    result.Add(alarmStatistic);
                }

                if (emptyItem != null)
                {
                    var groupedBySource = emptyItem.GroupBy(c => c.Source);
                    foreach (var groupedItem in groupedBySource)
                    {
                        MsgAlarmLogStatistic alarmStatistic = new MsgAlarmLogStatistic(groupedItem.ToArray());
                        result.Add(alarmStatistic);
                    }
                }
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "BuildAlarmStatistic(10)", e);
            }
            AlarmLogStatisticList = result.OrderByDescending(c => c.AlarmsCount).ToList();
        }

        #endregion

        #region Methods => AlarmMessengerConfiguration

        [ACMethodInteraction("Msg", "en{'Set alarm to distribution'}de{'Alarm auf Verteiler setzen'}", 150,true, "CurrentACMsgAlarm")]
        public void SetAlarmMessengerConfig()
        {
            BSOAlarmMessengerConfig config = ACUrlCommand("?BSOAlarmMessengerConfig_Child") as BSOAlarmMessengerConfig;
            if (config == null)
                config = StartComponent("BSOAlarmMessengerConfig_Child", null, null) as BSOAlarmMessengerConfig;

            if (config != null)
                config.SetAlarmToDistribution(CurrentACMsgAlarm);
        }

        public bool IsEnabledSetAlarmMessengerConfig()
        {
            return CurrentACMsgAlarm != null && CurrentACMsgAlarm.ConfigIconState == Global.ConfigIconState.NoConfig;
        }

        [ACMethodInteraction("Msg", "en{'Unset alarm from distribution'}de{'Alarm vom Verteiler entfernen'}", 151, true, "CurrentACMsgAlarm")]
        public void UnsetAlarmMessengerConfig()
        {
            BSOAlarmMessengerConfig config = ACUrlCommand("?BSOAlarmMessengerConfig_Child") as BSOAlarmMessengerConfig;
            if (config == null)
                config = StartComponent("BSOAlarmMessengerConfig_Child", null, null) as BSOAlarmMessengerConfig;

            if (config != null)
                config.UnsetAlarmFromDistribution(CurrentACMsgAlarm);
        }

        public bool IsEnabledUnsetAlarmMessengerConfig()
        {
            return CurrentACMsgAlarm != null && CurrentACMsgAlarm.ConfigIconState != Global.ConfigIconState.NoConfig 
                                             && CurrentACMsgAlarm.ConfigIconState != Global.ConfigIconState.ExclusionConfig;
        }

        [ACMethodInteraction("Msg", "en{'Set exclusion rule'}de{'Ausnahmeregel setzen'}", 152, true, "CurrentACMsgAlarm")]
        public void SetExclusionRule()
        {
            BSOAlarmMessengerConfig config = ACUrlCommand("?BSOAlarmMessengerConfig_Child") as BSOAlarmMessengerConfig;
            if (config == null)
                config = StartComponent("BSOAlarmMessengerConfig_Child", null, null) as BSOAlarmMessengerConfig;

            if (config != null)
                config.SetExclusionRule(CurrentACMsgAlarm);
        }

        public bool IsEnabledSetExclusionRule()
        {
            return CurrentACMsgAlarm != null && CurrentACMsgAlarm.ConfigIconState == Global.ConfigIconState.InheritedConfig;
        }

        [ACMethodInteraction("Msg", "en{'Remove exclusion rule'}de{'Ausnahmeregel entfernen'}", 153, true, "CurrentACMsgAlarm")]
        public void RemoveExclusionRule()
        {
            BSOAlarmMessengerConfig config = ACUrlCommand("?BSOAlarmMessengerConfig_Child") as BSOAlarmMessengerConfig;
            if (config == null)
                config = StartComponent("BSOAlarmMessengerConfig_Child", null, null) as BSOAlarmMessengerConfig;

            if (config != null)
                config.RemoveExclusionRule(CurrentACMsgAlarm);
        }

        public bool IsEnabledRemoveExclusionRule()
        {
            return CurrentACMsgAlarm != null && CurrentACMsgAlarm.ConfigIconState == Global.ConfigIconState.ExclusionConfig;
        }

        #endregion

        #region Alarm-Navigation

        IACBSOAlarmPresenter _BSOAlarmPresenter = null;
        Msg _DelegatedMsgAlarm = null;
        FocusBSOResult? _DelegateFocus = null;

        [ACMethodInteraction("Msg", "en{'Navigate to visualisation'}de{'Navigiere zur Visualisierung'}", 110, true, "CurrentACMsgAlarm")]
        public void NavigateToVisualisation()
        {
            bool startNewBSO = false;
            if (_BSOAlarmPresenter != null)
                return;
            _DelegateFocus = null;
            _DelegatedMsgAlarm = null;

            IACBSOAlarmPresenter alarmPresenter = null;
            if (ACUrlHelper.IsUrlDynamicInstance(CurrentACMsgAlarm.Source))
            {
                alarmPresenter = Root.Businessobjects.FindChildComponents<BSOProcessControl>(c => c is BSOProcessControl, null, 1).FirstOrDefault();
                startNewBSO = alarmPresenter == null;
                if (startNewBSO)
                    this.Root.RootPageWPF.StartBusinessobject(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + CN_BSOProcessControl, null);
                alarmPresenter = Root.Businessobjects.FindChildComponents<BSOProcessControl>(c => c is BSOProcessControl, null, 1).FirstOrDefault();
            }
            else
            {
                alarmPresenter = Root.Businessobjects.FindChildComponents<BSOVisualisationStudio>(c => c is BSOVisualisationStudio, null, 1).FirstOrDefault();
                startNewBSO = alarmPresenter == null;
                if (startNewBSO)
                    this.Root.RootPageWPF.StartBusinessobject(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + CN_BSOVisualisation, null);
                alarmPresenter = Root.Businessobjects.FindChildComponents<BSOVisualisationStudio>(c => c is BSOVisualisationStudio, null, 1).FirstOrDefault();
            }
            if (alarmPresenter == null)
                return;
            if (startNewBSO)
            {
                _BSOAlarmPresenter = alarmPresenter;
                _DelegatedMsgAlarm = CurrentACMsgAlarm;
                // Wait until view is loaded
                _BSOAlarmPresenter.ACActionEvent += AlarmPresenter_ACActionEvent;
            }
            else
                SwitchToViewOnAlarm(alarmPresenter, CurrentACMsgAlarm);
        }

        public bool IsEnabledNavigateToVisualisation()
        {
            return CurrentACMsgAlarm != null;
        }

        private void AlarmPresenter_ACActionEvent(object sender, ACActionArgs e)
        {
            if (e.ElementAction == Global.ElementActionType.VBDesignLoaded)
            {
                if (_BSOAlarmPresenter != null)
                {
                    FocusBSOResult focusBSOResult = FocusBSOResult.NotFocusable;
                    try
                    {
                        _BSOAlarmPresenter.ACActionEvent -= AlarmPresenter_ACActionEvent;
                        // Invoked first time after new bso loaded
                        if (_DelegateFocus == null)
                            focusBSOResult = SwitchToViewOnAlarm(_BSOAlarmPresenter, _DelegatedMsgAlarm);
                        // Invoked second time after Tabitem with already running bso was switched and loaded to view
                        else
                        {
                            _DelegateFocus = null;
                            _BSOAlarmPresenter.SwitchToViewOnAlarm(_DelegatedMsgAlarm);
                        }
                    }
                    catch (Exception ex)
                    {
                        Messages.LogException(this.GetACUrl(), "AlarmPresenter_ACActionEvent", ex.Message);
                    }
                    finally
                    {
                        if (focusBSOResult != FocusBSOResult.SelectionSwitched)
                        {
                            _BSOAlarmPresenter = null;
                            _DelegatedMsgAlarm = null;
                        }
                    }
                }
            }
        }

        private FocusBSOResult SwitchToViewOnAlarm(IACBSOAlarmPresenter alarmPresenter, Msg currentACMsgAlarm)
        {
            FocusBSOResult focusResult = this.Root.RootPageWPF.FocusBSO(alarmPresenter);
            if (focusResult == FocusBSOResult.AlreadyFocused)
                alarmPresenter.SwitchToViewOnAlarm(CurrentACMsgAlarm);
            else if (focusResult == FocusBSOResult.SelectionSwitched)
            {
                _BSOAlarmPresenter = alarmPresenter;
                _DelegatedMsgAlarm = currentACMsgAlarm;
                _DelegateFocus = focusResult;
                // Wait until view is loaded
                _BSOAlarmPresenter.ACActionEvent += AlarmPresenter_ACActionEvent;
            }
            return focusResult;
        }

        #endregion

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
                if ((sender.ACIdentifier == "AlarmChangedEvent") || (sender.ACIdentifier == "SubAlarmsChangedEvent"))
                {
                    StartNewQueryRequest(false, sender.ParentACComponent);
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
        private void StartNewQueryRequest(bool force, IACComponent appManagerInvoker = null)
        {
            if (force)
            {
                _LastQueryTime = DateTime.MinValue;
                var childs = Root.ACComponentChilds.Where(c => c is ApplicationManager || c is ApplicationManagerProxy).ToList();

                using (ACMonitor.Lock(_60201_AppManagerInvokersLock))
                {
                    _AppManagerInvokers = childs;
                }
            }

            if (appManagerInvoker != null)
            {
                using (ACMonitor.Lock(_60201_AppManagerInvokersLock))
                {
                    if (_AppManagerInvokers != null && !_AppManagerInvokers.Contains(appManagerInvoker))
                        _AppManagerInvokers.Add(appManagerInvoker);
                }
            }

            _DoNewQuery = true;
        }

        private List<IACComponent> _AppManagerInvokers = new List<IACComponent>();
        private readonly ACMonitorObject _60201_AppManagerInvokersLock = new ACMonitorObject(60201);

        private bool _QueryDone = false;
        private Dictionary<IACComponent, List<Msg>> _QueryResultCache = new Dictionary<IACComponent, List<Msg>>();
        private Dictionary<IACComponent, List<Msg>> _QueryResultNew = new Dictionary<IACComponent, List<Msg>>();

        void BGWorker_QueryServerComponent(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(100);
            if (IsRefreshCycleElapsed && _DoNewQuery)
            {
                _QueryResultNew.Clear();

                IEnumerable<IACComponent> appManagers = null;

                using (ACMonitor.Lock(_60201_AppManagerInvokersLock))
                {
                    if (_AppManagerInvokers != null)
                    {
                        appManagers = _AppManagerInvokers.ToArray();
                        _AppManagerInvokers.Clear();
                    }
                    else
                        appManagers = Array.Empty<IACComponent>();
                }

                foreach (IACComponent appManager in appManagers.Where(c => c is ApplicationManager || c is ApplicationManagerProxy))
                {
                    if (_QueryResultNew != null && !_QueryResultNew.ContainsKey(appManager))
                        _QueryResultNew.Add(appManager, new List<Msg>());

                    List<Msg> alarms = appManager.ACUrlCommand("!GetAlarmsConfig", true, true, false) as List<Msg>;
                    if (alarms != null && alarms.Any())
                        _QueryResultNew[appManager] = alarms;
                }

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
                    using (ACMonitor.Lock(_60201_AppManagerInvokersLock))
                    {
                        foreach (var item in _QueryResultNew)
                        {
                            List<Msg> existingAlarms;
                            if (_QueryResultCache.TryGetValue(item.Key, out existingAlarms))
                            {
                                foreach (Msg alarm in existingAlarms)
                                {
                                    _ACMsgAlarmList.Remove(alarm);
                                }
                            }
                            else
                                _QueryResultCache.Add(item.Key, null);

                            _QueryResultCache[item.Key] = item.Value;
                            foreach (Msg alarm in item.Value.OrderBy(c => c.TimeStampOccurred))
                            {
                                _ACMsgAlarmList.Insert(0, alarm);
                            }
                        }
                    }

                    _QueryDone = false;
                }
                if (_backgroundWorker != null)
                    _backgroundWorker.RunWorkerAsync();
            }
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"ShowAlarmExplorer":
                    ShowAlarmExplorer();
                    return true;
                case"AcknowledgeCurrent":
                    AcknowledgeCurrent();
                    return true;
                case"IsEnabledAcknowledgeCurrent":
                    result = IsEnabledAcknowledgeCurrent();
                    return true;
                case"AcknowledgeAll":
                    AcknowledgeAll();
                    return true;
                case"IsEnabledAcknowledgeAll":
                    result = IsEnabledAcknowledgeAll();
                    return true;
                case "SetAlarmMessengerConfig":
                    SetAlarmMessengerConfig();
                    return true;
                case "IsEnabledSetAlarmMessengerConfig":
                    result = IsEnabledSetAlarmMessengerConfig();
                    return true;
                case "UnsetAlarmMessengerConfig":
                    UnsetAlarmMessengerConfig();
                    return true;
                case "IsEnabledUnsetAlarmMessengerConfig":
                    result = IsEnabledUnsetAlarmMessengerConfig();
                    return true;
                case "SetExclusionRule":
                    SetExclusionRule();
                    return true;
                case "IsEnabledSetExclusionRule":
                    result = IsEnabledSetExclusionRule();
                    return true;
                case "RemoveExclusionRule":
                    RemoveExclusionRule();
                    return true;
                case "IsEnabledRemoveExclusionRule":
                    result = IsEnabledRemoveExclusionRule();
                    return true;
                case "NavigateToVisualisation":
                    NavigateToVisualisation();
                    return true;
                case "IsEnabledNavigateToVisualisation":
                    result = IsEnabledNavigateToVisualisation();
                    return true;
                case "EventCallback":
                    EventCallback((IACPointNetBase)acParameter[0], (ACEventArgs)acParameter[1], (IACObject)acParameter[2]);
                    return true;
                case "ShowAlarmArchiveExplorer":
                    ShowAlarmArchiveExplorer();
                    return true;
                case "Search":
                    Search();
                    return true;
                case "IsEnabledSearch":
                    result = IsEnabledSearch();
                    return true;
                    
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}

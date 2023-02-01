using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomUI;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.processapplication;
using Microsoft.EntityFrameworkCore;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace gip.bso.iplus
{
    /// <summary>
    /// Presenter for the Equipment analysis (OEE)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Equipment Analysis (OEE)'}de{'Geräteanalyse (OEE)'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class VBBSOPropertyLogPresenter : ACBSO
    {
        #region c'tors

        /// <summary>
        /// Creates a new instance of Equipment analysis presenter.
        /// </summary>
        /// <param name="acType">The acType parameter.</param>
        /// <param name="content">The content parameter.</param>
        /// <param name="parentACObject">The parentACObject parameter.</param>
        /// <param name="parameter">The parameters in the ACValueList.</param>
        /// <param name="acIdentifier">The acIdentifier parameter.</param>
        public VBBSOPropertyLogPresenter(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        /// <summary>
        /// Initializes this component.
        /// </summary>
        /// <param name="startChildMode">The start child mode parameter.</param>
        /// <returns>True if is initialization success, otherwise returns false.</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            FromDate = DateTime.Now.AddDays(-1);
            ToDate = DateTime.Now;
            FillPresenterViewModeList();
            SelectedPresenterViewMode = PresenterViewModeList.FirstOrDefault();
            SelectedTimelineMode = TimelineModeList.FirstOrDefault();
            return base.ACInit(startChildMode);
        }

        /// <summary>
        ///  Deinitializes this component.
        /// </summary>
        /// <param name="deleteACClassTask">The deleteACClassTask parameter.</param>
        /// <returns>True if is deinitialization success, otherwise returns false.</returns>
        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            _TreeViewList = null;
            _SelectedProprertyLogAlarm = null;
            _UpdatedPropertyLogs = null;
            _AlarmsList = null;
            _OEERelevantProperty = null;
            _HiddenOEEState = null;
            _SelectedPresenterViewMode = null;
            _ACPropertyLogsRoot = null;
            _SelectedPropertyLogRoot = null;
            _ACPropertyLogs = null;
            _SelectedPropertyLog = null;
            _SelectedACProject = null;
            _CurrentComponentClass = null;
            _LastSelectedDateTime = null;
            _SelectedTimelineValue = null;
            _AvailableTimelineValues = null;
            _AlarmsSubAlarmsList = null;

            bool done = base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return done;
        }

        #endregion

        #region Properties

        #region Properties => Filter

        private ACProject _SelectedACProject;
        /// <summary>
        /// Gets or sets the currently selected ACProject.
        /// </summary>
        [ACPropertySelected(401, "ACProject", "en{'Project filter'}de{'Projektfilter'}")]
        public ACProject SelectedACProject
        {
            get => _SelectedACProject;
            set
            {
                _SelectedACProject = value;
                OnPropertyChanged("SelectedACProject");
            }
        }

        private List<ACProject> _ACProjectList;
        /// <summary>
        /// Gets the list of a available ApplicationProjects.
        /// </summary>
        [ACPropertyList(402, "ACProject")]
        public IEnumerable<ACProject> ACProjectsList
        {
            get
            {
                if (_ACProjectList != null)
                    return _ACProjectList;
                using (ACMonitor.Lock(Database.ContextIPlus.QueryLock_1X000))
                {
                    _ACProjectList = Database.ContextIPlus.ACProject.Where(c => c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application).ToList();
                }
                return _ACProjectList;
            }
        }

        private DateTime _FromDate;
        /// <summary>
        /// Gets or sets the FROM DateTime point for equipment analysis.
        /// </summary>
        [ACPropertyInfo(403)]
        public DateTime FromDate
        {
            get => _FromDate;
            set
            {
                _FromDate = value;
                OnPropertyChanged("FromDate");
            }
        }

        private DateTime _ToDate;
        /// <summary>
        /// Gets or sets the TO DateTime point for equipment analsis.
        /// </summary>
        [ACPropertyInfo(404)]
        public DateTime ToDate
        {
            get => _ToDate;
            set
            {
                _ToDate = value;
                OnPropertyChanged("ToDate");
            }
        }

        private ACClass _CurrentComponentClass;
        /// <summary>
        /// Gets or sets the CurrentComponentClass. Is used for access from Visualisation.
        /// </summary>
        public ACClass CurrentComponentClass
        {
            get => _CurrentComponentClass;
            set
            {
                _CurrentComponentClass = value;
                OnPropertyChanged("CurrentComponentClass");
                OnPropertyChanged("CurrentComponentACUrl");
            }
        }

        /// <summary>
        /// Gets the ACUrl of the current Component.
        /// </summary>
        [ACPropertyInfo(405, "", "en{'Component ACUrl'}de{'Komponente ACUrl'}")]
        public string CurrentComponentACUrl
        {
            get => CurrentComponentClass != null ? CurrentComponentClass.ACUrlComponent : "";
        }

        private string _SearchText;
        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        [ACPropertyInfo(406, "", "en{'Search'}de{'Suche'}")]
        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
            }
        }

        #endregion

        #region Properties => TimelineView

        private IEnumerable<ACPropertyLogModel> _ACPropertyLogsRoot;
        /// <summary>
        /// Gets or sets the ACPropertyLogsRoot. Represents the list for treeListView control.
        /// </summary>
        [ACPropertyList(407, "PropertyLogsRoot")]
        public IEnumerable<ACPropertyLogModel> ACPropertyLogsRoot
        {
            get
            {
                return _ACPropertyLogsRoot;
            }
            set
            {
                _ACPropertyLogsRoot = value;
                OnPropertyChanged("ACPropertyLogsRoot");
            }
        }

        private ACPropertyLogModel _SelectedPropertyLogRoot;
        /// <summary>
        /// Gets or sets the selected PropertyLog root. (Selected in treeListView control.)
        /// </summary>
        [ACPropertyCurrent(408, "PropertyLogsRoot")]
        public ACPropertyLogModel SelectedPropertyLogRoot
        {
            get
            {
                return _SelectedPropertyLogRoot;
            }
            set
            {
                _SelectedPropertyLogRoot = value;
                OnPropertyChanged("SelectedPropertyLogRoot");
            }
        }

        private ObservableCollection<ACPropertyLogModel> _ACPropertyLogs;
        /// <summary>
        /// Gets or sets the ACPropertyLogs. Represents the collection for timeline view control. Contains all timeline items(ACPropertyLogModel).
        /// </summary>
        [ACPropertyList(409, "PropertyLogs")]
        public ObservableCollection<ACPropertyLogModel> ACPropertyLogs
        {
            get
            {

                return _ACPropertyLogs;
            }
            set
            {
                _ACPropertyLogs = value;
                OnPropertyChanged("ACPropertyLogs");
            }
        }

        private ACPropertyLogModel _SelectedPropertyLog;
        /// <summary>
        /// Gets or sets the seelcted PropertyLog. (Selected in the timeline view control)
        /// </summary>
        [ACPropertyCurrent(410, "PropertyLogs")]
        public ACPropertyLogModel SelectedPropertyLog
        {
            get
            {
                return _SelectedPropertyLog;
            }
            set
            {
                _SelectedPropertyLog = value;
                if (_SelectedPropertyLog != null && (PresenterViewMode)SelectedPresenterViewMode.Value != PresenterViewMode.Grouped)
                {
                    var tempList = new ACValueItemList("");
                    tempList.AddRange(ACPropertyLogs.Where(c => c.DisplayOrder == _SelectedPropertyLog.DisplayOrder &&
                                                                c.PropertyLogModelType == ACPropertyLogModelType.PropertyLog)
                                                    .Select(x => x.ACCaption).Distinct().Select(k => new ACValueItem(k, k, null)));
                    AvailableTimelineValues = tempList;
                }
                else
                    AvailableTimelineValues = null;
                
                OnPropertyChanged("SelectedPropertyLog");
            }
        }


        #endregion

        #region Properties =>  View mode enum (Presentation mode)

        /// <summary>
        /// Represents the enumeration for presentation modes.
        /// </summary>
        public enum PresenterViewMode : short
        {
            Compact = 10,
            CompactFilter = 20,
            CompactOEEPAM = 30,
            CompactOEEAll = 40,
            Grouped = 45
        }

        private ACValueItem _SelectedPresenterViewMode;
        /// <summary>
        /// Gets or sets the selected presentation mode.
        /// </summary>
        [ACPropertySelected(411, "PresenterViewMode", "en{'Select presentation mode'}de{'Präsentationsmodus auswählen'}")]
        public ACValueItem SelectedPresenterViewMode
        {
            get => _SelectedPresenterViewMode;
            set
            {
                _SelectedPresenterViewMode = value;
                OnPropertyChanged("SelectedPresenterViewMode");
                IsOEEViewActive = false;
            }
        }

        private static ACValueItemList _PresenterViewModeList;
        /// <summary>
        /// Gets the list of presentation modes.
        /// </summary>
        [ACPropertyList(412, "PresenterViewMode")]
        public IEnumerable<ACValueItem> PresenterViewModeList
        {
            get
            {
                if (_IsCompactFilterAvailable)
                    return _PresenterViewModeList;
                else
                    return _PresenterViewModeList.Where(c => (short)c.Value != 20);
            }
        }

        #endregion

        #region Properties => Statistics

        private ACValueItem _SelectedTimelineMode;
        [ACPropertySelected(403, "TimelineMode")]
        public ACValueItem SelectedTimelineMode
        {
            get { return _SelectedTimelineMode; }
            set
            {
                if (_SelectedTimelineMode != value)
                {
                    _SelectedTimelineMode = value;
                    CalculatePropertyLogStatistics();
                }
                OnPropertyChanged("SelectedTimelineMode");
            }
        }

        private ACValueItemList _TimelineModeList;
        [ACPropertyList(404, "TimelineMode")]
        public ACValueItemList TimelineModeList
        {
            get
            {
                if (_TimelineModeList == null)
                {
                    _TimelineModeList = new ACValueItemList("TimelineModeList");
                    _TimelineModeList.Add(new ACValueItem("en{'Overall timeline'}de{'Gesamter Zeitstrahl'}", "1", null));
                    _TimelineModeList.Add(new ACValueItem("en{'Selected timeframe'}de{'Ausgewählter Zeitrahmen'}", "2", null));
                }
                return _TimelineModeList;
            }
        }

        private List<ACPropertyLogModel> _SelectedPropertyLogSum;
        [ACPropertySelected(450, "PropertyLogSum")]
        public List<ACPropertyLogModel> SelectedPropertyLogSum
        {
            get => _SelectedPropertyLogSum;
            set
            {
                _SelectedPropertyLogSum = value;
                OnPropertyChanged("SelectedPropertyLogSum");
            }
        }

        private List<ACPropertyLogModel> _PropertyLogSumList;
        [ACPropertyList(451, "PropertyLogSum")]
        public List<ACPropertyLogModel> PropertyLogSumList
        {
            get => _PropertyLogSumList;
            set
            {
                _PropertyLogSumList = value;
                OnPropertyChanged("PropertyLogSumList");
            }
        }

        private DateTime? _TimeFrameFrom;
        public DateTime? TimeFrameFrom
        {
            get => _TimeFrameFrom;
            set
            {
                _TimeFrameFrom = value;
                _NeedStatisticsRefresh = true;
            }
        }

        private DateTime? _TimeFrameTo;
        public DateTime? TimeFrameTo
        {
            get => _TimeFrameTo;
            set
            {
                _TimeFrameTo = value;
                _NeedStatisticsRefresh = true;
            }
        }

        #endregion

        #region Properties => Other

        /// <summary>
        /// Gets or sets the selected alarm.
        /// </summary>
        [ACPropertySelected(413, "Alarms")]
        public MsgAlarmLog SelectedAlarm
        {
            get;
            set;
        }

        private IEnumerable<MsgAlarmLog> _AlarmsList;
        /// <summary>
        /// Gets or sets the list of relevant alarms.
        /// </summary>
        [ACPropertyList(414, "Alarms")]
        public IEnumerable<MsgAlarmLog> AlarmsList
        {
            get => _AlarmsList;
            set
            {
                _AlarmsList = value;
                OnPropertyChanged("AlarmsList");
            }
        }

        [ACPropertySelected(415, "AlarmsSubAlarms")]
        public MsgAlarmLog SelectedAlarmSubAlarm
        {
            get;
            set;
        }

        private IEnumerable<MsgAlarmLog> _AlarmsSubAlarmsList;
        /// <summary>
        /// Gets or sets the list of relevant alarms.
        /// </summary>
        [ACPropertyList(416, "AlarmsSubAlarms")]
        public IEnumerable<MsgAlarmLog> AlarmsSubAlarmsList
        {
            get => _AlarmsSubAlarmsList;
            set
            {
                _AlarmsSubAlarmsList = value;
                OnPropertyChanged("AlarmsSubAlarmsList");
            }
        }

        private static ACClassProperty _OEERelevantProperty;
        /// <summary>
        /// Gets the property which is used for OEE presentation on a PA modules.
        /// </summary>
        public static ACClassProperty OEERelevantProperty
        {
            get
            {
                if (_OEERelevantProperty == null)
                {
                    using (Database db = new core.datamodel.Database())
                    {
                        _OEERelevantProperty = db.ACClassProperty.Include(c => c.ACClassPropertyRelation_TargetACClassProperty)
                                                                 .FirstOrDefault(c => c.ACIdentifier == GlobalProcApp.AvailabilityStatePropName);
                    }
                }
                return _OEERelevantProperty;
            }
        }

        private ACClassPropertyRelation _HiddenOEEState;
        /// <summary>
        /// Gets the rule for Hidden OEE State.
        /// </summary>
        private ACClassPropertyRelation HiddenOEEState
        {
            get
            {
                if (_HiddenOEEState == null)
                    _HiddenOEEState = OEERelevantProperty.ACClassPropertyRelation_TargetACClassProperty
                                                         .FirstOrDefault(c => c.ConnectionTypeIndex == (short)Global.ConnectionTypes.PointState &&
                                                                              c.LogicalOperationIndex == (short)Global.LogicalOperators.none);
                return _HiddenOEEState;
            }
        }

        private bool _IsOEEViewActive = false;
        /// <summary>
        /// Gets or sets is OEE presentation mode activated.
        /// </summary>
        public bool IsOEEViewActive
        {
            get => _IsOEEViewActive;
            set
            {
                _IsOEEViewActive = value;
                OnPropertyChanged("IsOEEViewActive");
            }
        }

        public ACValueItem _SelectedTimelineValue;
        [ACPropertySelected(417,"TimelineValue", "en{'Go to next value'}de{'Gehe zum nächsten Wert'}")]
        public ACValueItem SelectedTimelineValue
        {
            get => _SelectedTimelineValue;
            set
            {
                _SelectedTimelineValue = value;
                OnPropertyChanged("SelectedTimelineValue");
                _LastSelectedDateTime = new DateTime();
            }
        }

        private ACValueItemList _AvailableTimelineValues;
        [ACPropertyList(418,"TimelineValue")]
        public ACValueItemList AvailableTimelineValues
        {
            get => _AvailableTimelineValues;
            set
            {
                _AvailableTimelineValues = value;
                OnPropertyChanged("AvailableTimelineValues");
            }
        }

        #endregion

        #region Private
        //List for treeview
        private List<ACPropertyLogModel> _TreeViewList = new List<ACPropertyLogModel>();
        private int _DisplayOrder = 0;
        private bool _IsCompactFilterAvailable = false;
        private ACPropertyLogModel _SelectedProprertyLogAlarm = null;
        private enum OEEMode : byte { None = 0, PAM = 10, All = 20 }
        private List<ACPropertyLogModel> _UpdatedPropertyLogs;
        private DateTime? _LastSelectedDateTime = new DateTime();
        private bool _NeedStatisticsRefresh = false;
        private bool _IsStatisticsTabActivated = false;
        #endregion

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

        /// <summary>
        /// Gets the database instance. 
        /// </summary>
        public Database Db
        {
            get
            {
                return Database as Database;
            }
        }

        #endregion

        #region Methods

        private void CreateTimelineModel()
        {
            ACProject project = SelectedACProject;
            if (project == null && CurrentComponentClass != null)
                project = CurrentComponentClass.ACProject;

            _IsCompactFilterAvailable = false;
            if ((PresenterViewMode)SelectedPresenterViewMode.Value == PresenterViewMode.Compact)
            {
                ACPropertyLogs = new ObservableCollection<ACPropertyLogModel>(CreateTimelineModelCompact(FromDate, ToDate, project,
                                                                                                            CurrentComponentClass != null ? CurrentComponentClass.ACClassID : Guid.Empty));

                _TreeViewList = ACPropertyLogs.Where(c => c.PropertyLogModelType == ACPropertyLogModelType.Property && c.ParentACObject == null).ToList();
                RemoveTreeViewItems(_TreeViewList);

                ACPropertyLogsRoot = _TreeViewList;
                _IsCompactFilterAvailable = true;
            }
            else if ((PresenterViewMode)SelectedPresenterViewMode.Value == PresenterViewMode.Grouped)
            {
                ACPropertyLogs = new ObservableCollection<ACPropertyLogModel>(CreateTimelineModelGrouped(FromDate, ToDate, project,
                                                                                                        CurrentComponentClass != null ? CurrentComponentClass.ACClassID : Guid.Empty));
                _DisplayOrder = 0;
                UpdateDisplayOrder(_TreeViewList);
                RemoveTreeViewItems(_TreeViewList);
                ACPropertyLogsRoot = _TreeViewList;
            }
            else if ((PresenterViewMode)SelectedPresenterViewMode.Value == PresenterViewMode.CompactFilter)
            {
                DoCompactFilter();
                _IsCompactFilterAvailable = true;
            }
            else if ((PresenterViewMode)SelectedPresenterViewMode.Value == PresenterViewMode.CompactOEEAll)
            {
                ACPropertyLogs = new ObservableCollection<ACPropertyLogModel>(CreateTimelineModelCompact(FromDate, ToDate, project,
                                                                                                         CurrentComponentClass != null ? CurrentComponentClass.ACClassID : Guid.Empty, OEEMode.All));

                _TreeViewList = ACPropertyLogs.Where(c => c.PropertyLogModelType == ACPropertyLogModelType.Property && c.ParentACObject == null).ToList();
                RemoveTreeViewItems(_TreeViewList);
                ACPropertyLogModel.CalculateOEE(_TreeViewList, ACPropertyLogs);

                ACPropertyLogsRoot = _TreeViewList;
                IsOEEViewActive = true;
            }
            else if ((PresenterViewMode)SelectedPresenterViewMode.Value == PresenterViewMode.CompactOEEPAM)
            {
                ACPropertyLogs = new ObservableCollection<ACPropertyLogModel>(CreateTimelineModelCompact(FromDate, ToDate, project,
                                                                                                         CurrentComponentClass != null ? CurrentComponentClass.ACClassID : Guid.Empty, OEEMode.PAM));

                _TreeViewList = ACPropertyLogs.Where(c => c.PropertyLogModelType == ACPropertyLogModelType.Property && c.ParentACObject == null).ToList();
                RemoveTreeViewItems(_TreeViewList);
                ACPropertyLogModel.CalculateOEE(_TreeViewList, ACPropertyLogs);

                ACPropertyLogsRoot = _TreeViewList;
                IsOEEViewActive = true;
            }

            OnPropertyChanged("PresenterViewModeList");
        }

        #region Methods => Compact View

        private List<ACPropertyLogModel> CreateTimelineModelCompact(DateTime from, DateTime to, ACProject project = null, Guid? componentClassID = null, OEEMode oeeMode = OEEMode.None)
        {
            List<ACPropertyLogModel> propertyLogs = new List<ACPropertyLogModel>();
            _TreeViewList = new List<ACPropertyLogModel>();
            int displayOrder = 0;

            IEnumerable<ACPropertyLog> relevantLogs = DoFilter(Db, from, to, project, componentClassID);

            var groupedByClass = relevantLogs.GroupBy(c => c.ACClass);
            if (oeeMode == OEEMode.PAM)
                groupedByClass = groupedByClass.Where(c => typeof(IPAOEEProvider).IsAssignableFrom(c.Key.ObjectType));
            foreach (var groupedItem in groupedByClass.OrderBy(c => c.Key.ACUrlComponent))
            {
                ACPropertyLogModel logModelClass = new ACPropertyLogModel
                {
                    PropertyLogModelType = ACPropertyLogModelType.Property,
                    ACUrl = groupedItem.Key.ACUrlComponent,
                    _ACCaption = groupedItem.Key.ACUrlComponent,
                    DisplayOrder = displayOrder
                };
                propertyLogs.Add(logModelClass);

                if (oeeMode > OEEMode.None)
                {
                    var oeeRoot = FindOEERoot(groupedItem.Key.ACClass1_ParentACClass, propertyLogs);
                    if (oeeRoot != null)
                    {
                        oeeRoot.AddItem(logModelClass);
                        logModelClass.ParentACObject = oeeRoot;
                    }
                }

                var groupedByProp = groupedItem.GroupBy(c => c.ACClassProperty);

                if (groupedByProp.Count() > 1)
                {
                    IEnumerable<ACPropertyLogModel> compactModels = DetermineCompactModelView(groupedByProp, groupedItem.Key, Db, logModelClass);
                    IEnumerable<ACPropertyLogModel> hostModels = ACPropertyLogModel.SetupCompactModel(logModelClass, compactModels, groupedItem.Key, out displayOrder);
                    if (hostModels.Any())
                        propertyLogs.AddRange(hostModels);
                    propertyLogs.AddRange(compactModels);
                }
                else
                {
                    var groupedProp = groupedByProp.FirstOrDefault();
                    if (groupedProp != null)
                    {
                        var basicPropModels = DetermineBasicPropLogModels(groupedItem.Key, groupedProp.Key, groupedProp, Db);
                        if (!basicPropModels.Any())
                            continue;

                        foreach (var basicModel in basicPropModels)
                        {
                            string caption = groupedProp.Key.ObjectType.IsEnum || IsPropertyValueNumeric(groupedProp.Key.ObjectType) ? 
                                             basicModel.PropertyValue.ToString() : groupedProp.Key.ACCaption;
                            bool isOEEProp = groupedProp.Key.ACClassPropertyID == OEERelevantProperty.ACClassPropertyID;
                            if (isOEEProp)
                            {
                                logModelClass.IsOEERoot = true;
                                if (basicModel.PropertyValue.Equals(ACConvert.ChangeType(HiddenOEEState.XMLValue, groupedProp.Key.ObjectType, true, null)))
                                    continue;
                            }

                            ACPropertyLogModel propLog = CreateACPropertyLogModel(basicModel.StartDate, basicModel.EndDate, basicModel.PropertyValue, groupedProp.Key, 
                                                                                  groupedItem.Key, displayOrder, caption, false, logModelClass);
                            if (propLog == null)
                                continue;

                            logModelClass.AddItem(propLog);
                            propLog.ParentACObject = logModelClass;

                            var alarms = propLog.DeterminePropertyLogAlarms(groupedItem.Key);
                            if (alarms != null && alarms.Any())
                            {
                                propertyLogs.AddRange(alarms);
                                logModelClass.AddItems(alarms);
                            }

                            propertyLogs.Add(propLog);
                        }

                        logModelClass.SetStartMinDate(logModelClass.Items.Min(c => c.StartDate));
                        logModelClass.SetEndMaxDate(logModelClass.Items.Max(c => c.EndDate));
                    }
                }
                displayOrder++;
            }
            return propertyLogs;
        }

        private IEnumerable<ACPropertyLogModel> DetermineCompactModelView(IEnumerable<IGrouping<ACClassProperty, ACPropertyLog>> groupedPropertyLogs, ACClass currentComponent, Database db,
                                                                          ACPropertyLogModel parent)
        {
            List<Tuple<ACClassProperty, List<ACPropertyLogInfo>>> basicPropLogModels = new List<Tuple<ACClassProperty, List<ACPropertyLogInfo>>>();

            PropertyLogCompactModels comapactModels = new PropertyLogCompactModels();

            foreach (var groupedProp in groupedPropertyLogs)
            {
                var result = DetermineBasicPropLogModels(currentComponent, groupedProp.Key, groupedProp, db);
                basicPropLogModels.Add(new Tuple<ACClassProperty, List<ACPropertyLogInfo>>(groupedProp.Key, result));
            }

            foreach (var item in basicPropLogModels)
            {
                var points = item.Item1.ACClassPropertyRelation_TargetACClassProperty.Where(c => c.ConnectionTypeIndex == (short)Global.ConnectionTypes.PointState &&
                                                                                                 currentComponent.IsDerivedClassFrom(c.SourceACClass)).Select(p => p.SourceACClassProperty);

                foreach (var point in points)
                    comapactModels.CreateComapctModelByPoint(point, item);
            }

            return comapactModels.DeterminePropertyLogs(parent);
        }

        #endregion

        #region Methods => Grouped view

        private List<ACPropertyLogModel> CreateTimelineModelGrouped(DateTime from, DateTime to, ACProject project = null, Guid? componentClassID = null)
        {
            List<ACPropertyLogModel> propertyLogs = new List<ACPropertyLogModel>();
            _TreeViewList = new List<ACPropertyLogModel>();

            IEnumerable<ACPropertyLog> relevantLogs = DoFilter(Db, from, to, project, componentClassID);

            var groupedByClass = relevantLogs.GroupBy(c => c.ACClass);

            foreach (var groupedItem in groupedByClass)
            {
                ACPropertyLogModel logModelClass = CreateTreeViewModel(groupedItem.Key, groupedItem.Key, propertyLogs);

                var groupedByProp = groupedItem.GroupBy(c => c.ACClassProperty);

                foreach (var groupedProp in groupedByProp.OrderBy(c => c.Key.ACCaption))
                {
                    ACPropertyLogModel logModelProp = new ACPropertyLogModel
                    {
                        PropertyLogModelType = ACPropertyLogModelType.Property,
                        ParentACObject = logModelClass,
                        ACUrl = logModelClass.ACUrl,
                        _ACCaption = groupedProp.Key.ACCaption
                    };

                    propertyLogs.Add(logModelProp);
                    logModelClass.AddItem(logModelProp);

                    var basicPropLogModels = DetermineBasicPropLogModels(groupedItem.Key, groupedProp.Key, groupedProp, Db);
                    if (!basicPropLogModels.Any())
                        continue;

                    foreach (var basicPropLogModel in basicPropLogModels)
                    {
                        ACPropertyLogModel propLog = CreateACPropertyLogModel(basicPropLogModel.StartDate, basicPropLogModel.EndDate, basicPropLogModel.PropertyValue, 
                                                                              groupedProp.Key, groupedItem.Key, 0, groupedProp.Key.ObjectType != typeof(bool) ? basicPropLogModel.PropertyValue.ToString() : null);
                        if (propLog == null)
                            continue;

                        logModelProp.AddItem(propLog);
                        propLog.ParentACObject = logModelProp;

                        var alarms = propLog.DeterminePropertyLogAlarms(groupedItem.Key);
                        if (alarms != null && alarms.Any())
                        {
                            propertyLogs.AddRange(alarms);
                            logModelProp.AddItems(alarms);
                        }

                        propertyLogs.Add(propLog);
                    }

                    logModelProp.SetStartMinDate(logModelProp.Items.Min(c => c.StartDate));
                    logModelProp.SetEndMaxDate(logModelProp.Items.Max(c => c.EndDate));
                }
            }

            return propertyLogs;
        }

        private ACPropertyLogModel CreateTreeViewModel(ACClass currentACClass, ACClass acClass, List<ACPropertyLogModel> propertyLogs)
        {
            List<ACClass> hierarchyList = new List<ACClass>();
            string parentCaption = acClass.ACCaption;

            while (acClass.ACClass1_ParentACClass != null)
            {
                acClass = acClass.ACClass1_ParentACClass;
                hierarchyList.Add(acClass);
            }

            hierarchyList.Reverse();
            ACPropertyLogModel modelClass = null, parentModelClass = null;
            var currentList = _TreeViewList;

            foreach (ACClass parentClass in hierarchyList)
            {
                modelClass = currentList.FirstOrDefault(c => c.ACCaption == parentClass.ACIdentifier);
                if (modelClass == null)
                {
                    modelClass = new ACPropertyLogModel
                    {
                        PropertyLogModelType = ACPropertyLogModelType.Class,
                        ACUrl = parentClass.ACUrlComponent,
                        _ACCaption = parentClass.ACIdentifier
                    };
                    if (parentModelClass != null)
                    {
                        parentModelClass.AddItem(modelClass);
                        parentModelClass.Items.Sort((c, k) => c.ACCaption.CompareTo(k.ACCaption));
                    }
                    else if (hierarchyList.IndexOf(parentClass) == 0)
                        _TreeViewList.Add(modelClass);
                    propertyLogs.Add(modelClass);
                }
                currentList = modelClass.Items;
                parentModelClass = modelClass;
            }
            var result = modelClass.Items.FirstOrDefault(c => c.ACCaption == currentACClass.ACIdentifier);
            if (result == null)
            {
                ACPropertyLogModel logModelClass = new ACPropertyLogModel
                {
                    PropertyLogModelType = ACPropertyLogModelType.Class,
                    ACUrl = currentACClass.ACUrlComponent,
                    _ACCaption = currentACClass.ACIdentifier,
                };
                propertyLogs.Add(logModelClass);
                modelClass.AddItem(logModelClass);
                result = logModelClass;
            }
            return result;
        }

        private void UpdateDisplayOrder(List<ACPropertyLogModel> logsList)
        {
            foreach (ACPropertyLogModel logModel in logsList)
            {
                if (logModel.PropertyLogModelType >= ACPropertyLogModelType.PropertyLog)
                    continue;

                logModel.DisplayOrder = _DisplayOrder;

                if (logModel.PropertyLogModelType == ACPropertyLogModelType.Property)
                {
                    foreach (ACPropertyLogModel propLog in logModel.Items)
                        propLog.DisplayOrder = _DisplayOrder;
                }

                _DisplayOrder++;
                UpdateDisplayOrder(logModel.Items);
            }
        }

        #endregion

        #region Methods => ACMethods

        /// <summary>
        /// Builds the propertyLog models and shows it in a timeline control.
        /// </summary>
        [ACMethodInfo("", "en{'Show/Refesh'}de{'Anzeigen/neu laden'}", 401)]
        public void ShowLogsOnTimeline()
        {
            CreateTimelineModel();
            if(_IsStatisticsTabActivated)
            {
                CalculatePropertyLogStatistics();
                _NeedStatisticsRefresh = false;
            }
            else
            {
                _NeedStatisticsRefresh = true;
            }
        }

        /// <summary>
        /// Shows alarms in the dialog window for the timeline item.
        /// </summary>
        [ACMethodInteraction("", "en{'Show Alarms'}de{'Alarme anzeigen'}", 402, true, "SelectedPropertyLog")]
        public void ShowAlarms()
        {
            if (!IsEnabledShowAlarms())
                return;

            ShowDialog(this, "Alarms");
        }

        /// <summary>
        /// Checks is enabled show alarms command.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledShowAlarms()
        {
            return _SelectedProprertyLogAlarm != null && _SelectedProprertyLogAlarm.Alarms != null && _SelectedProprertyLogAlarm.Alarms.Any();
        }

        [ACMethodInteraction("", "en{'Show alarms/subalarms for this component'}de{'Zeigt Alarme/Subalarme für diese Komponente an.'}", 403, true, "SelectedPropertyLog")]
        public void ShowAllAlarms()
        {
            if (!IsEnabledShowAllAlarms())
                return;

            var query = Db.MsgAlarmLog.Where(c => c.TimeStampOccurred >= FromDate && c.TimeStampOccurred <= ToDate).AsQueryable();
            string acUrl = SelectedPropertyLog.ACUrl;

            //(query as ObjectQuery).MergeOption = MergeOption.OverwriteChanges;
            AlarmsSubAlarmsList = query.ToList().Where(c => (c.ACClass != null && c.ACClass.ACUrlComponent.StartsWith(acUrl)) ||
                                                         (c.ACProgramLog != null && c.ACProgramLog.ACUrl.StartsWith(acUrl)));

            ShowDialog(this, "AlarmsAll");
        }

        public bool IsEnabledShowAllAlarms()
        {
            return SelectedPropertyLog != null && (PresenterViewMode)SelectedPresenterViewMode.Value != PresenterViewMode.Grouped;
        }

        /// <summary>
        /// Opens the equipment analysis module from Visualisation for the component in parameter.
        /// </summary>
        /// <param name="componentClass">The component class parameter.</param>
        [ACMethodInfo("", "", 404)]
        public void ShowPropertyLogsDialog(ACClass componentClass)
        {
            CurrentComponentClass = componentClass.FromIPlusContext<ACClass>(Db);
            ShowDialog(this, "PropertyLogPresenterDialog");
            CurrentComponentClass = null;
        }

        /// <summary>
        /// Updates the display order for a propertyLog models. It is used for sort from treeListView control.
        /// </summary>
        /// <param name="treeViewItems"></param>
        [ACMethodInfo("", "", 405)]
        public void UpdateDisplayOrder(ICollectionView treeViewItems)
        {
            _UpdatedPropertyLogs = new List<ACPropertyLogModel>();
            int displayOrder = 0;
            UpdateDisplayOrderRecursive(treeViewItems.OfType<ACPropertyLogModel>(), ACPropertyLogs, ref displayOrder);
        }

        [ACMethodInfo("","en{'GO'}de{'GO'}",406)]
        public void GoToNextValue()
        {
            var nextPropLog = ACPropertyLogs.Where(c => c.PropertyLogModelType == ACPropertyLogModelType.PropertyLog &&
                                                        c.ACCaption == SelectedTimelineValue.Value.ToString() &&
                                                        c.DisplayOrder == SelectedPropertyLog.DisplayOrder &&
                                                        c.StartDate > _LastSelectedDateTime).OrderBy(x => x.StartDate).FirstOrDefault();

            if(nextPropLog == null)
                nextPropLog = ACPropertyLogs.Where(c => c.PropertyLogModelType == ACPropertyLogModelType.PropertyLog && c.ACCaption == SelectedTimelineValue.Value.ToString() &&
                                                        c.DisplayOrder == SelectedPropertyLog.DisplayOrder).OrderBy(x => x.StartDate).FirstOrDefault();

            if (nextPropLog != null)
            {
                _LastSelectedDateTime = nextPropLog.StartDate;
                BroadcastToVBControls("!ScrollToValue", "ACPropertyLogsRoot", nextPropLog.StartDate);
            }
        }

        public bool IsEnabledGoToNextValue()
        {
            return SelectedPropertyLog != null && SelectedTimelineValue != null;
        }

        #endregion

        #region Methods => Private

        private ACPropertyLogModel CreateACPropertyLogModel(DateTime? startDate, DateTime? endDate, object propertyValue, ACClassProperty acClassProperty, ACClass acClass, 
                                                           int displayOrder, string caption, bool skipRuleCheck = false, ACPropertyLogModel logModelClass = null)
        {
            IEnumerable<ACClassPropertyRelation> rules = acClassProperty.ACClassPropertyRelation_TargetACClassProperty.Where(c => acClass.IsDerivedClassFrom(c.SourceACClass));

            if (!skipRuleCheck)
            {
                if (rules == null)
                    return null;

                ACClassPropertyRelation inverseRule = rules.FirstOrDefault(c => c.LogicalOperationIndex == (short)Global.LogicalOperators.none);
                if (inverseRule != null && ACConvert.XMLToObject(acClassProperty.ObjectType, inverseRule.XMLValue, true, null).Equals(propertyValue))
                    return null;

                if (inverseRule == null && !rules.Any(c => ACConvert.XMLToObject(acClassProperty.ObjectType, c.XMLValue, true, null).Equals(propertyValue)))
                    return null;
            }

            if (logModelClass != null && rules != null && rules.Any())
                logModelClass._ACCaption = Translator.GetTranslation(rules.FirstOrDefault().GroupName);

            return new ACPropertyLogModel(startDate, endDate, propertyValue, ACPropertyLogModelType.PropertyLog, displayOrder, caption, acClassProperty.ObjectType, null,
                                          acClassProperty.ACIdentifier);
        }

        private List<ACPropertyLogInfo> DetermineBasicPropLogModels(ACClass acClass, ACClassProperty acClassProperty, IEnumerable<ACPropertyLog> propertyLogs, Database db)
        {
            List<ACPropertyLogInfo> resultList = new List<ACPropertyLogInfo>();

            var propLogs = propertyLogs.OrderBy(c => c.EventTime).ToArray();
            int propLogsCount = propLogs.Count();

            if (propLogsCount == 1)
            {
                ACPropertyLog currentLog = propLogs.FirstOrDefault();
                DateTime dateTime = currentLog.EventTime.AddMilliseconds(-2);
                ACPropertyLog prevLog = db.ACPropertyLog.Where(c => c.ACClassID == currentLog.ACClassID && c.ACClassPropertyID == currentLog.ACClassPropertyID 
                                                                                                        && c.EventTime < dateTime)
                                                        .OrderByDescending(x => x.EventTime)
                                                        .FirstOrDefault();

                if (prevLog != null)
                {
                    object logValue = ACConvert.XMLToObject(acClassProperty.ObjectType, prevLog.Value, true, db);
                    resultList.Add(new ACPropertyLogInfo(FromDate, currentLog.EventTime, logValue, "") { PropertyLog = prevLog });
                }
            }
            else
            {
                for (int i = 0; i < propLogsCount - 1; i++)
                {
                    ACPropertyLog propLog = propLogs[i];
                    ACPropertyLog propLogNext = propLogs[i + 1];

                    object logValue = ACConvert.XMLToObject(acClassProperty.ObjectType, propLog.Value, true, db);
                    resultList.Add(new ACPropertyLogInfo(propLog.EventTime, propLogNext.EventTime, logValue, "") { PropertyLog = propLog });
                }
            }

            ACPropertyLog lastLog = propLogs.LastOrDefault();
            object logValueLast = ACConvert.XMLToObject(acClassProperty.ObjectType, lastLog.Value, true, db);
            resultList.Add(new ACPropertyLogInfo(lastLog.EventTime, ToDate, logValueLast, "") { PropertyLog = lastLog });

            return resultList;
        }

        private void RemoveTreeViewItems(List<ACPropertyLogModel> items)
        {
            foreach (ACPropertyLogModel propModel in items)
            {
                if (propModel.PropertyLogModelType == ACPropertyLogModelType.Property)
                    propModel.Items.RemoveAll(c => c.PropertyLogModelType >= ACPropertyLogModelType.PropertyLog);
                RemoveTreeViewItems(propModel.Items);
            }
        }

        private void UpdateDisplayOrderRecursive(IEnumerable<ACPropertyLogModel> treeViewItems, IEnumerable<ACPropertyLogModel> timelineItems, ref int displayOrder)
        {
            foreach (var item in treeViewItems)
            {
                var timelineItemsToUpdate = timelineItems.Where(c => c.DisplayOrder == item.DisplayOrder && !_UpdatedPropertyLogs.Contains(c)).ToList();
                item.DisplayOrder = displayOrder;
                _UpdatedPropertyLogs.Add(item);
                foreach (var tlItem in timelineItemsToUpdate)
                {
                    tlItem.DisplayOrder = item.DisplayOrder;
                    _UpdatedPropertyLogs.Add(tlItem);
                }
                displayOrder++;
                UpdateDisplayOrderRecursive(item.Items, timelineItems, ref displayOrder);
            }
        }

        private IEnumerable<ACPropertyLog> DoFilter(Database db, DateTime from, DateTime to, ACProject project, Guid? componentClassID)
        {
            IEnumerable<ACPropertyLog> relevantLogs = db.ACPropertyLog.Where(c => c.EventTime > from && c.EventTime < to).AsQueryable();
            if (project != null)
                relevantLogs = relevantLogs.Where(c => c.ACClass.ACProjectID == project.ACProjectID);

            relevantLogs = relevantLogs.ToArray();

            if (componentClassID != null && componentClassID != Guid.Empty)
            {
                ACClass compClass = db.ACClass.FirstOrDefault(c => c.ACClassID == componentClassID.Value);
                if (compClass != null)
                    relevantLogs = relevantLogs.Where(c => c.ACClassID == componentClassID || c.ACClass?.ParentACClassID == componentClassID  // Show logs of child components
                                                                                           || c.ACClass?.ACClass1_ParentACClass?.ParentACClassID == componentClassID
                                                                                           || c.ACClass?.ACClass1_ParentACClass?.ACClass1_ParentACClass?.ParentACClassID == componentClassID);
            }

            if (!String.IsNullOrEmpty(SearchText))
            {
                string searchText = SearchText.ToLower();
                relevantLogs = relevantLogs.Where(c => c.ACClass.ACUrl.ToLower().Contains(searchText) || c.ACClassProperty.ACCaption.ToLower().Contains(searchText) ||
                                                       c.ACClassProperty.ACIdentifier.ToLower().Contains(searchText));
            }

            return relevantLogs;
        }

        private void DoCompactFilter()
        {
            var comparableTreeViewItems = ACPropertyLogsRoot.Where(c => c.IsComparable).OrderBy(x => x.DisplayOrder).ToList();
            var comparableTimelineItems = ACPropertyLogs.Where(c => comparableTreeViewItems.Any(x => x.DisplayOrder == c.DisplayOrder || x.Items.Any(k => k.DisplayOrder == c.DisplayOrder))).ToList();

            int displayOrder = 0;
            foreach (var compTreeItem in comparableTreeViewItems)
            {
                comparableTimelineItems.Where(c => c.DisplayOrder == compTreeItem.DisplayOrder).ToList().ForEach(c => c.DisplayOrder = displayOrder);
                compTreeItem.DisplayOrder = displayOrder;
                displayOrder++;

                foreach (var childItem in compTreeItem.Items)
                {
                    comparableTimelineItems.Where(c => c.DisplayOrder == childItem.DisplayOrder).ToList().ForEach(c => c.DisplayOrder = displayOrder);
                    childItem.DisplayOrder = displayOrder;
                    displayOrder++;
                }
            }

            ACPropertyLogs = new ObservableCollection<ACPropertyLogModel>(comparableTimelineItems);
            ACPropertyLogsRoot = comparableTreeViewItems;
        }

        private void FillPresenterViewModeList()
        {
            if (_PresenterViewModeList == null)
            {
                _PresenterViewModeList = new ACValueItemList("PresenterViewMode")
                {
                    new ACValueItem("en{'Compact'}de{'Kompakt'}", PresenterViewMode.Compact, null),
                    new ACValueItem("en{'Compact with filter'}de{'Kompakt mit Filter'}", PresenterViewMode.CompactFilter, null),
                    new ACValueItem("en{'Compact OEE PAProcess m.'}de{'Kompakt OEE PAProcess M.'}", PresenterViewMode.CompactOEEPAM, null),
                    new ACValueItem("en{'Compact OEE all'}de{'Kompakt OEE alle'}", PresenterViewMode.CompactOEEAll, null),
                    new ACValueItem("en{'Grouped'}de{'Gruppiert'}", PresenterViewMode.Grouped, null)
                };
            }
        }

        private ACPropertyLogModel FindOEERoot(ACClass parentComponent, IEnumerable<ACPropertyLogModel> propLogs)
        {
            if (parentComponent == null)
                return null;

            ACPropertyLogModel result = propLogs.FirstOrDefault(c => c.IsOEERoot && c.ACCaption == parentComponent.ACUrlComponent);
            if (result != null)
                return result;

            return FindOEERoot(parentComponent.ACClass1_ParentACClass, propLogs);
        }

        private static bool IsPropertyValueNumeric(Type propertyType)
        {
            switch (Type.GetTypeCode(propertyType))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Methods => Statistics

        public void CalculatePropertyLogStatistics()
        {
            if (   ACPropertyLogs == null
                || _SelectedTimelineMode == null)
                return;

            DateTime? from = null, to = null;
            if (_SelectedTimelineMode.Value as string == "2")
            {
                from = TimeFrameFrom;
                to = TimeFrameTo;
            }

            List<ACPropertyLogModel> items = new List<ACPropertyLogModel>();
            var groupedByDisplayOrder = ACPropertyLogs.GroupBy(c => c.DisplayOrder);
            foreach(var item in groupedByDisplayOrder)
            {
                ACPropertyLogModel model = item.FirstOrDefault(c => c.PropertyLogModelType == ACPropertyLogModelType.Property);
                if (model == null)
                    continue;
                if (string.IsNullOrEmpty(model.ACUrl) && model.ParentACObject  != null)
                {
                    var parent = model.ParentACObject as ACPropertyLogModel;
                    if (parent != null)
                        model.ACUrl = parent.ACUrl;
                }

                var result = ACPropertyLog.AggregateDurationOfPropertyValues(from, to, item);
                if (result == null || !result.Any())
                    continue;

                ACPropertyLogModel mainModel = new ACPropertyLogModel() { ACUrl = model.ACUrl, _ACCaption = model.ACCaption,
                                                                            PropertyLogModelType = model.PropertyLogModelType };
                mainModel.Items.AddRange(result.Where(x => string.IsNullOrEmpty(x.ACUrl))
                                                .Select(c => new ACPropertyLogModel() { ACUrl = c.ACUrl, _ACCaption = c.ACCaption, TotalDuration = c.Duration, 
                                                                                        ParentACObject = mainModel, PropertyLogModelType = ACPropertyLogModelType.PropertyLog }));

                mainModel.TotalDuration += TimeSpan.FromSeconds(mainModel.Items.Sum(c => c.TotalDuration.TotalSeconds));

                if (mainModel.Items.Any())
                    items.Add(mainModel);
            }

            PropertyLogSumList = items;
        }

        [ACMethodCommand("Query", "en{'Export'}de{'Export'}", (short)MISort.QueryDesignDlg, true)]
        public override void DataExportDialog()
        {
            base.DataExportDialog();
        }

        public override bool IsEnabledDataExportDialog()
        {
            return PropertyLogSumList != null && PropertyLogSumList.Any();
        }

        private void ExportStatisticsToExcel()
        {
            try
            {
                XLWorkbook workBook = new XLWorkbook();
                IXLWorksheet workSheet = workBook.Worksheets.Add("Statistics");

                var headerCell = workSheet.Cell("A2");
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                headerCell.Value = "ACUrl";
                SetBorder(headerCell, true, false);

                headerCell = workSheet.Cell("B2");
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                headerCell.Value = "ACCaption";
                SetBorder(headerCell, true, false);

                headerCell = workSheet.Cell("C2");
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                headerCell.Value = Translator.GetTranslation("en{'Duration'}de{'Dauer'}");
                SetBorder(headerCell, true, false);

                headerCell = workSheet.Cell("D2");
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                headerCell.Value = Translator.GetTranslation("en{'Duration %'}de{'Dauer %'}");
                SetBorder(headerCell, true, false);

                int currentRow = 3;

                foreach (var logItem in PropertyLogSumList)
                {
                    var cell = workSheet.Cell("A" + currentRow);
                    cell.Value = logItem.ACUrl;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.Linen;
                    SetBorder(cell, true, false);

                    cell = workSheet.Cell("B" + currentRow);
                    cell.Value = logItem.ACCaption;
                    cell.Style.Fill.BackgroundColor = XLColor.Linen;
                    SetBorder(cell, true, false);

                    cell = workSheet.Cell("C" + currentRow);
                    cell.Value = logItem.TotalDuration;
                    cell.Style.Fill.BackgroundColor = XLColor.Linen;
                    SetBorder(cell, true, false);

                    cell = workSheet.Cell("D" + currentRow);
                    cell.Value = Math.Round(logItem.DurationPercent,2) + " %";
                    cell.Style.Fill.BackgroundColor = XLColor.Linen;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    SetBorder(cell, true, false);

                    currentRow++;

                    var lastItem = logItem.Items.LastOrDefault();
                    foreach (var childItem in logItem.Items)
                    {
                        bool isLast = childItem == lastItem;

                        if (isLast)
                        {
                            cell = workSheet.Cell("A" + currentRow);
                            SetBorder(cell, false, true);
                        }

                        cell = workSheet.Cell("B" + currentRow);
                        cell.Value = childItem.ACCaption;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        if (isLast)
                            SetBorder(cell, false, true);

                        cell = workSheet.Cell("C" + currentRow);
                        cell.Value = childItem.TotalDuration;
                        if (isLast)
                            SetBorder(cell, false, true);
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;

                        cell = workSheet.Cell("D" + currentRow);
                        cell.Value = Math.Round(childItem.DurationPercent,2) + " %";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        if (isLast)
                            SetBorder(cell, false, true);
                        currentRow++;
                    }

                    currentRow++;
                }
                
                workSheet.Column("A").AdjustToContents();
                workSheet.Column("A").Width += 4;
                workSheet.Column("B").AdjustToContents();
                workSheet.Column("B").Width += 4;
                workSheet.Column("C").Width = 13;
                workSheet.Column("D").Width = 13;
                workBook.SaveAs(DataExportFilePath);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "VBStudioExport_TranslationExcel", e);
            }
        }

        private void SetBorder(IXLCell cell, bool top, bool bottom)
        {
            if (top)
            {
                cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.TopBorderColor = XLColor.Black;
            }

            if (bottom)
            {
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.BottomBorderColor = XLColor.Black;
            }
        }

        public override string DataExportGenerateFileName()
        {
            return string.Format("{0}\\Equipment analysis {1}.xlsx", Root.Environment.Datapath, DateTime.Now.ToString().Replace(".", "-").Replace(":", "-"));
        }

        #endregion

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public override void ACAction(ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.ContextMenu && actionArgs.DropObject != null)
            {
                _SelectedProprertyLogAlarm = actionArgs.DropObject.ContextACObject as ACPropertyLogModel;
                if (_SelectedProprertyLogAlarm != null && _SelectedProprertyLogAlarm.Alarms != null)
                    AlarmsList = _SelectedProprertyLogAlarm.Alarms.ToArray();
                else
                    AlarmsList = null;
            }
            else if (actionArgs.ElementAction == Global.ElementActionType.TabItemActivated)
            {
                if (actionArgs.DropObject.VBContent == "Statistics")
                {
                    _IsStatisticsTabActivated = true;
                    if (_NeedStatisticsRefresh)
                    {
                        CalculatePropertyLogStatistics();
                        _NeedStatisticsRefresh = false;
                    }
                }
                else
                {
                    _IsStatisticsTabActivated = false;
                }
            }
            base.ACAction(actionArgs);
        }

        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            var result = base.OnGetControlModes(vbControl);
            if (vbControl == null || string.IsNullOrEmpty(vbControl.VBContent))
                return result;

            switch (vbControl.VBContent)
            {
                case "CurrentComponentACUrl":
                    if (CurrentComponentClass != null)
                        result = Global.ControlModes.Enabled;
                    else
                        result = Global.ControlModes.Collapsed;
                    break;
                case "SelectedACProject":
                    if (CurrentComponentClass == null)
                        result = Global.ControlModes.Enabled;
                    else
                        result = Global.ControlModes.Collapsed;
                    break;
            }

            return result;
        }

        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = e.Argument.ToString();
            switch (command)
            {
                case "DataExport":
                    ExportStatisticsToExcel();
                    break;
            }
        }

        #endregion

        #region HandleExecuteACMethod

        /// <summary>
        /// Overrides the HandleExecuteACMethod.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="invocationMode">The invocationMode.</param>
        /// <param name="acMethodName">The acMethodName.</param>
        /// <param name="acClassMethod">The acClassMethod.</param>
        /// <param name="acParameter">The acParameter.</param>
        /// <returns>True if is method called successfully, otherwise returns false.</returns>
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "ShowLogsOnTimeline":
                    ShowLogsOnTimeline();
                    return true;
                case "ShowPropertyLogsDialog":
                    ShowPropertyLogsDialog(acParameter[0] as ACClass);
                    return true;
                case "ShowAlarms":
                    ShowAlarms();
                    return true;
                case "IsEnabledShowAlarms":
                    result = IsEnabledShowAlarms();
                    return true;
                case "UpdateDisplayOrder":
                    UpdateDisplayOrder(acParameter[0] as ICollectionView);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #region Helper model

        class PropertyLogCompactModels : List<PropertyLogCompactModel>
        {
            public PropertyLogCompactModels()
            {

            }

            public PropertyLogCompactModel CreateComapctModelByPoint(ACClassProperty point, Tuple<ACClassProperty, List<ACPropertyLogInfo>> basicPropLogModel)
            {
                var compactModel = this.FirstOrDefault(c => c.PAPointProperty == point);
                if (compactModel == null)
                {
                    compactModel = new PropertyLogCompactModel{PAPointProperty = point};
                    this.Add(compactModel);
                }
                compactModel.ListOfPropertyLogs.Add(basicPropLogModel);
                return compactModel;
            }

            public IEnumerable<ACPropertyLogModel> DeterminePropertyLogs(ACPropertyLogModel parent)
            {
                foreach (var compactModel in this)
                {
                    compactModel.DeterminePropertyLogs(parent);
                    //if (compactModel.PropertyLogs != null)
                    //{
                    //    int? maxDisplayGroupFactor = compactModel.MaxDisplayGroupFactor;
                    //    if (maxDisplayGroupFactor.HasValue)
                    //    {
                    //        foreach (var propLog in compactModel.PropertyLogs.Where(c => c.PropertyValue.Equals(true)))
                    //            propLog.DisplayOrder = (short)maxDisplayGroupFactor;
                    //    }
                    //}
                }

                return this.SelectMany(c => c.PropertyLogs).Where(c => c.PropertyType != typeof(bool) || c.PropertyValue.Equals(true));
            }
        }

        class PropertyLogCompactModel
        {
            public ACClassProperty PAPointProperty
            {
                get;
                set;
            }

            public List<Tuple<ACClassProperty, List<ACPropertyLogInfo>>> _ListOfPropertyLogs;
            public List<Tuple<ACClassProperty, List<ACPropertyLogInfo>>> ListOfPropertyLogs
            {
                get
                {
                    if (_ListOfPropertyLogs == null)
                        _ListOfPropertyLogs = new List<Tuple<ACClassProperty, List<ACPropertyLogInfo>>>();

                    return _ListOfPropertyLogs;
                }
            }

            public int? MaxDisplayGroupFactor
            {
                get
                {
                    int? displayGroupFactor = 0;
                    if (Rules != null)
                        displayGroupFactor = Rules.Max(c => c.DisplayGroup) / 10;
                    return displayGroupFactor;
                }
            }

            private IEnumerable<ACClassPropertyRelation> _Rules;
            public IEnumerable<ACClassPropertyRelation> Rules
            {
                get
                {
                    if (_Rules == null && PAPointProperty != null)
                        _Rules = PAPointProperty.ACClassPropertyRelation_SourceACClassProperty.ToArray();
                    return _Rules;
                }
            }

            public List<ACPropertyLogModel> PropertyLogs
            {
                get;
                set;
            }

            public void DeterminePropertyLogs(ACPropertyLogModel parent)
            {
                List<ACPropertyLogModel> propertyLogs = new List<ACPropertyLogModel>();
                var listOfPropLogsSorted = ListOfPropertyLogs.OrderByDescending(c => c.Item2.Count).ToList();
                var largestPropModel = listOfPropLogsSorted.FirstOrDefault();

                foreach (ACPropertyLogInfo mainPropLogModel in largestPropModel.Item2)
                {
                    List<Tuple<ACClassProperty, ACPropertyLogInfo>> listForCompare = new List<Tuple<ACClassProperty, ACPropertyLogInfo>>
                    {
                        new Tuple<ACClassProperty, ACPropertyLogInfo>(largestPropModel.Item1, mainPropLogModel)
                    };
                    foreach (var propModel in listOfPropLogsSorted.Skip(1))
                    {
                        var subPropLogModel = propModel.Item2.FirstOrDefault(c => c.StartDate < mainPropLogModel.EndDate && c.EndDate > mainPropLogModel.StartDate);
                        if(subPropLogModel != null)
                            listForCompare.Add(new Tuple<ACClassProperty, ACPropertyLogInfo>(propModel.Item1, subPropLogModel));
                    }
                    propertyLogs.Add(CreateCompactPropertyLogModel(listForCompare, parent));
                }
                PropertyLogs = propertyLogs;
            }

            private ACPropertyLogModel CreateCompactPropertyLogModel(List<Tuple<ACClassProperty, ACPropertyLogInfo>> items, ACPropertyLogModel parent)
            {
                List<Tuple<bool, Global.Operators>> results = new List<Tuple<bool, Global.Operators>>();
                DateTime? startDate = null, endDate = null;
                string caption = null;
                bool propValue = false;

                string groupName = Translator.GetTranslation(Rules.FirstOrDefault().GroupName);
                //short displayGroup = 0;

                if (items.Count == 1)
                {
                    var item = items.FirstOrDefault();
                    bool isOEEProp = false;

                    ACClassPropertyRelation rule = Rules.FirstOrDefault(c => c.TargetACClassProperty == item.Item1);
                    if (rule != null && rule.XMLValue != null)
                    {
                        if (rule.LogicalOperation == Global.Operators.none)
                        {
                            bool isVisible = !item.Item2.PropertyValue.Equals(ACConvert.XMLToObject(item.Item1.ObjectType, rule.XMLValue, true, null));
                            if (!isVisible)
                                return new ACPropertyLogModel(startDate, endDate, false, ACPropertyLogModelType.PropertyLog, 0, caption, typeof(bool), parent, "");
                        }
                        caption = item.Item1.ObjectType.IsEnum || IsPropertyValueNumeric(item.Item1.ObjectType) ?
                          item.Item2.PropertyValue.ToString() : null;

                        isOEEProp = item.Item1.ACClassPropertyID == OEERelevantProperty.ACClassPropertyID;
                        if (isOEEProp)
                            parent.IsOEERoot = true;

                        if (caption == null)
                        {
                            if (string.IsNullOrEmpty(rule.StateName))
                            {
                                caption = item.Item1.ACCaption;
                                if (item.Item1.ObjectType != typeof(bool))
                                    caption += ": " + item.Item2.PropertyValue;
                            }
                            else
                                caption = Translator.GetTranslation(rule.StateName);
                        }


                        if (!startDate.HasValue || startDate < item.Item2.StartDate)
                            startDate = item.Item2.StartDate;

                        if (!endDate.HasValue || endDate > item.Item2.EndDate)
                            endDate = item.Item2.EndDate;
                    }

                    return new ACPropertyLogModel(startDate, endDate, item.Item2.PropertyValue, ACPropertyLogModelType.PropertyLog, 0, caption, item.Item1.ObjectFullType, parent, groupName);
                }
                else
                { 
                    foreach (var item in items)
                    {
                        ACClassPropertyRelation rule = Rules.FirstOrDefault(c => c.TargetACClassProperty == item.Item1);
                        if (rule != null && rule.XMLValue != null)
                        {
                            bool pValue = false;

                            if (item.Item2 != null)
                            {
                                if (rule.LogicalOperation == Global.Operators.none)
                                {
                                    pValue = !item.Item2.PropertyValue.Equals(ACConvert.XMLToObject(item.Item1.ObjectType, rule.XMLValue, true, null));
                                }
                                else
                                {
                                    pValue = item.Item2.PropertyValue.Equals(ACConvert.XMLToObject(item.Item1.ObjectType, rule.XMLValue, true, null));
                                }
                            }

                            var result = new Tuple<bool, Global.Operators>(pValue, rule.LogicalOperation);

                            results.Add(result);
                            if (!result.Item1)
                                continue;

                            caption = item.Item1.ObjectType.IsEnum || IsPropertyValueNumeric(item.Item1.ObjectType) ?
                                                 item.Item2.PropertyValue.ToString() : null;

                            //bool isOEEProp = item.Item1.ACClassPropertyID == OEERelevantProperty.ACClassPropertyID;
                            //if (isOEEProp)
                            //{
                            //    parent.IsOEERoot =
                            //}

                            if (caption == null)
                            {
                                if (string.IsNullOrEmpty(rule.StateName))
                                {
                                    caption = item.Item1.ACCaption;
                                    if (item.Item1.ObjectType != typeof(bool))
                                        caption += ": " + item.Item2.PropertyValue;
                                }
                                else
                                    caption = Translator.GetTranslation(rule.StateName);
                            }

                            if (!startDate.HasValue || startDate < item.Item2.StartDate)
                                startDate = item.Item2.StartDate;

                            if (!endDate.HasValue || endDate > item.Item2.EndDate)
                                endDate = item.Item2.EndDate;
                        }
                    }

                    
                    if (results.All(c => c.Item1))
                        propValue = true;
                    else if (results.Any(c => c.Item1 && c.Item2 == Global.Operators.or))
                        propValue = true;

                     //items.Count == 1 ? items.FirstOrDefault().Item1.ACIdentifier : null;

                }

                

                return new ACPropertyLogModel(startDate, endDate, propValue, ACPropertyLogModelType.PropertyLog, 0, caption, typeof(bool), parent, groupName);
            }
        }

        #endregion
    }

    #region Model

    /// <summary>
    /// Represents the view model class for ACPropertyLog.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPropertyLogModel'}de{'ACPropertyLogModel'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, true)]
    public class ACPropertyLogModel : ACPropertyLogInfo
    {

        #region c'tors

        /// <summary>
        /// Creates a new instance of a ACPropertyLogModel.
        /// </summary>
        public ACPropertyLogModel() : base()
        {
        }

        /// <summary>
        /// Creates a new instance of a ACPropertyLogModel.
        /// </summary>
        /// <param name="start">The start date time for propertylog model.</param>
        /// <param name="end">The end date time for propertylog model.</param>
        /// <param name="propertyValue">The property value parameter.</param>
        /// <param name="modelType">The propertylog model type.</param>
        /// <param name="displayOrder">The display order paramter. It's used for display order in timline control.</param>
        /// <param name="acCaption">The acCaption parameter.</param>
        /// <param name="propertyType">The .NET type of a property.</param>
        /// <param name="parent">The parent object parameter.</param>
        public ACPropertyLogModel(DateTime? start, DateTime? end, object propertyValue, ACPropertyLogModelType modelType, int displayOrder = 0, string acCaption = null,
                                  Type propertyType = null, IACObject parent = null) : base(start, end, propertyValue, acCaption)
        {
            StartDate = start;
            EndDate = end;
            PropertyValue = propertyValue;
            PropertyLogModelType = modelType;
            DisplayOrder = displayOrder;
            _ACCaption = acCaption;
            PropertyType = propertyType;
            ParentACObject = parent;
        }

        /// <summary>Creates a new instance of a ACPropertyLogModel.</summary>
        /// <param name="start">The start date time for propertylog model.</param>
        /// <param name="end">The end date time for propertylog model.</param>
        /// <param name="propertyValue">The property value parameter.</param>
        /// <param name="modelType">The propertylog model type.</param>
        /// <param name="displayOrder">The display order paramter. It's used for display order in timline control.</param>
        /// <param name="acCaption">The acCaption parameter.</param>
        /// <param name="propertyType">The .NET type of a property.</param>
        /// <param name="parent">The parent object parameter.</param>
        /// <param name="groupName"></param>
        public ACPropertyLogModel(DateTime? start, DateTime? end, object propertyValue, ACPropertyLogModelType modelType, int displayOrder = 0, string acCaption = null,
                                  Type propertyType = null, IACObject parent = null, string groupName = null) : base(start, end, propertyValue, acCaption)
        {
            StartDate = start;
            EndDate = end;
            PropertyValue = propertyValue;
            PropertyLogModelType = modelType;
            DisplayOrder = displayOrder;
            _ACCaption = acCaption;
            PropertyType = propertyType;
            ParentACObject = parent;
            GroupName = groupName;
        }

        #endregion

        #region Properties

        private List<ACPropertyLogModel> _Items;
        /// <summary>
        /// Gets the child items.
        /// </summary>
        public List<ACPropertyLogModel> Items
        {
            get
            {
                if (_Items == null)
                    _Items = new List<ACPropertyLogModel>();
                return _Items;
            }
        }

        private int _DisplayOrder = 0;
        /// <summary>
        /// Gets or sets the display order. It's used for display order in the timeline control.
        /// </summary>
        [ACPropertyInfo(100)]
        public int DisplayOrder
        {
            get
            {
                return _DisplayOrder;
            }
            set
            {
                _DisplayOrder = value;
                OnPropertyChanged("DisplayOrder");
            }
        }

        /// <summary>
        /// Gets or sets the duration of all child items.
        /// </summary>
        [ACPropertyInfo(104, "", "en{'Total Duration'}de{'Gesamtdauer'}")]
        public TimeSpan TotalDuration
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the alarm duration of all child items.
        /// </summary>
        [ACPropertyInfo(105, "", "en{'Total Alarm Duration'}de{'Gesamte Alarmdauer'}")]
        public TimeSpan TotalAlarmDuration
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the duration of item in percent according parent item.
        /// </summary>
        [ACPropertyInfo(105, "", "en{'Duration %'}de{'Dauer %'}")]
        public double DurationPercent
        {
            get
            {
                ACPropertyLogModel parent = ParentACObject as ACPropertyLogModel;
                if (parent != null && parent.TotalDuration.TotalSeconds > 0 && TotalDuration.TotalSeconds > 0)
                {
                    return (TotalDuration.TotalSeconds / parent.TotalDuration.TotalSeconds) * 100;
                }
                else if(TotalDuration.TotalSeconds > 0)
                    return 100;

                return 0;
            }

        }

        /// <summary>
        /// Gets or sets the Overall Equipment Effectiveness.
        /// </summary>
        [ACPropertyInfo(106, "", "en{'OEE'}de{'OEE'}")]
        public double OEE
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets is item OEE root item.
        /// </summary>
        public bool IsOEERoot
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the PropertyType.
        /// </summary>
        public Type PropertyType
        {
            get;
            set;
        }

        public string GroupName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the list of alarms related for this propertylog model.
        /// </summary>
        public IEnumerable<MsgAlarmLog> Alarms
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets IsComparable. It's used for presentation mode: Compact with filter
        /// </summary>
        [ACPropertyInfo(108)]
        public bool IsComparable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of PropertyLogModel.
        /// </summary>
        public ACPropertyLogModelType PropertyLogModelType
        {
            get;
            set;
        }

        private List<Global.TimelineItemStatus> _Status = new List<Global.TimelineItemStatus>();

        /// <summary>
        /// Gets the status list, which is shown in a timeline item tooltip.
        /// </summary>
        public virtual List<Global.TimelineItemStatus> Status
        {
            get
            {
                _Status.Clear();

                if (Alarms != null && Alarms.Any())
                    _Status.Add(Global.TimelineItemStatus.Alarm);

                if (_Status.Any())
                    return _Status;
                else
                    _Status.Add(Global.TimelineItemStatus.OK);

                return _Status;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Setups the compact propertylog model.
        /// </summary>
        /// <param name="classModel">The classModel parameter.</param>
        /// <param name="compactModels">The compactModels parameter.</param>
        /// <param name="componentClass">The componentClass parameter.</param>
        /// <param name="displayOrder">The displayOrder parameter.</param>
        /// <returns>List of a Compact models.</returns>
        public static IEnumerable<ACPropertyLogModel> SetupCompactModel(ACPropertyLogModel classModel, IEnumerable<ACPropertyLogModel> compactModels, ACClass componentClass, out int displayOrder)
        {
            List<ACPropertyLogModel> result = new List<ACPropertyLogModel>();

            displayOrder = classModel.DisplayOrder;
            var groupedCM = compactModels.GroupBy(c => c.GroupName);
            short cmIndex = 0;

            foreach (var groupedModel in groupedCM)
            {
                string groupName = Translator.GetTranslation(groupedModel.Key);

                // add items to class
                if (cmIndex == 0)
                {
                    foreach (ACPropertyLogModel cm in groupedModel)
                    {
                        cm.DisplayOrder = classModel.DisplayOrder;
                        cm.ParentACObject = classModel;
                        var alarms = cm.DeterminePropertyLogAlarms(componentClass);
                        if (alarms != null && alarms.Any())
                        {
                            result.AddRange(alarms);
                            classModel.AddItems(alarms);
                        }
                    }
                    classModel.AddItems(groupedModel);
                    classModel.StartDate = groupedModel.Min(c => c.StartDate);
                    classModel.EndDate = groupedModel.Max(c => c.EndDate);
                    classModel.PropertyLogModelType = ACPropertyLogModelType.Property;
                    classModel.PropertyType = groupedModel.FirstOrDefault().PropertyType;
                    classModel._ACCaption = groupName;
                }
                else
                {
                    displayOrder++;

                    ACPropertyLogModel hostModel = new ACPropertyLogModel(groupedModel.Min(c => c.StartDate), groupedModel.Max(c => c.EndDate), null,
                                                                          ACPropertyLogModelType.Property, displayOrder, groupName, null, classModel);
                    result.Add(hostModel);
                    foreach (ACPropertyLogModel cm in groupedModel)
                    {
                        cm.DisplayOrder = displayOrder;
                        cm.ParentACObject = hostModel;
                        var alarms = cm.DeterminePropertyLogAlarms(componentClass);
                        if (alarms != null && alarms.Any())
                        {
                            result.AddRange(alarms);
                            classModel.AddItems(alarms);
                        }
                    }
                    hostModel.AddItems(groupedModel);
                    hostModel.PropertyType = groupedModel.FirstOrDefault().PropertyType;
                    classModel.AddItem(hostModel);
                }
                cmIndex++;
            }
            return result;
        }

        /// <summary>
        /// Sets the minimum start date time.
        /// </summary>
        /// <param name="dateTime">The date time to check.</param>
        public void SetStartMinDate(DateTime? dateTime)
        {
            if (StartDate == null || StartDate > dateTime)
                StartDate = dateTime;
        }

        /// <summary>
        /// Sets the maximum end date time.
        /// </summary>
        /// <param name="dateTime">The date time to check.</param>
        public void SetEndMaxDate(DateTime? dateTime)
        {
            if (EndDate == null || EndDate < dateTime)
                EndDate = dateTime;
        }

        /// <summary>
        /// Adds propertyLog model in the Items collection. Also calculates values for a TotalDuration and TotalAlarmDuration.
        /// </summary>
        /// <param name="propertyLogItem">The propertylog model to add.</param>
        public void AddItem(ACPropertyLogModel propertyLogItem)
        {
            if (PropertyLogModelType < ACPropertyLogModelType.PropertyLog)
            {
                if (propertyLogItem.PropertyLogModelType == ACPropertyLogModelType.PropertyLog)
                    TotalDuration += propertyLogItem.Duration;
                else if (propertyLogItem.PropertyLogModelType == ACPropertyLogModelType.PropertyLogAlarm)
                    TotalAlarmDuration += propertyLogItem.Duration;
            }

            Items.Add(propertyLogItem);
        }

        /// <summary>
        /// Adds a propertyLog models in the Items collection. Also calculates values for a TotalDuration and TotalAlarmDuration.
        /// </summary>
        /// <param name="propertyLogItems">The collection of a propertylog models to add.</param>
        public void AddItems(IEnumerable<ACPropertyLogModel> propertyLogItems)
        {
            if (PropertyLogModelType < ACPropertyLogModelType.PropertyLog)
            {
                TotalDuration += TimeSpan.FromSeconds(propertyLogItems.Where(p => p.PropertyLogModelType == ACPropertyLogModelType.PropertyLog).Sum(c => c.Duration.TotalSeconds));
                TotalAlarmDuration += TimeSpan.FromSeconds(propertyLogItems.Where(p => p.PropertyLogModelType == ACPropertyLogModelType.PropertyLogAlarm).Sum(c => c.Duration.TotalSeconds));
            }

            Items.AddRange(propertyLogItems);
        }

        /// <summary>
        /// Determines alarms related to property log model.
        /// </summary>
        /// <param name="componentClass">The component class parameter.</param>
        /// <returns>The list of property log models of type PropertyLogAlarm.</returns>
        public List<ACPropertyLogModel> DeterminePropertyLogAlarms(ACClass componentClass)
        {
            Alarms = componentClass.MsgAlarmLog_ACClass.Where(c => c.TimeStampOccurred >= StartDate && c.TimeStampOccurred <= EndDate).ToArray();
            if (!Alarms.Any())
                return null;

            List<ACPropertyLogModel> result = new List<ACPropertyLogModel>();

            foreach (MsgAlarmLog alarm in Alarms)
            {
                ACPropertyLogModel propLogAlarm = new ACPropertyLogModel(alarm.TimeStampOccurred, alarm.TimeStampAcknowledged, true, ACPropertyLogModelType.PropertyLogAlarm,
                                                                         DisplayOrder, "Alarm", null, this.ParentACObject);

                if ((alarm.TimeStampAcknowledged - alarm.TimeStampOccurred).TotalSeconds > 1)
                {
                    if (alarm.TimeStampAcknowledged > EndDate)
                        propLogAlarm.EndDate = EndDate;
                }
                else
                    propLogAlarm.EndDate = alarm.TimeStampOccurred.AddSeconds(0.1);

                propLogAlarm.Alarms = this.Alarms;
                result.Add(propLogAlarm);
            }

            return result;
        }

        /// <summary>
        /// Calculates the Overall Equipment Effectiveness.
        /// </summary>
        /// <param name="oeePropertyLogs"></param>
        /// <param name="timelinePropLogs"></param>
        public static void CalculateOEE(IEnumerable<ACPropertyLogModel> oeePropertyLogs, IEnumerable<ACPropertyLogModel> timelinePropLogs)
        {
            foreach (var item in oeePropertyLogs)
            {
                if (item.PropertyLogModelType > ACPropertyLogModelType.Property)
                    continue;

                CalculateOEE(item.Items, timelinePropLogs);

                if (item.PropertyType == typeof(GlobalProcApp.AvailabilityState))
                {
                    var scheduledTime = item.TotalDuration.TotalSeconds - timelinePropLogs.Where(c => c.PropertyLogModelType == ACPropertyLogModelType.PropertyLog && c.DisplayOrder == item.DisplayOrder &&
                                                                   (((GlobalProcApp.AvailabilityState)c.PropertyValue) == GlobalProcApp.AvailabilityState.Idle ||
                                                                    ((GlobalProcApp.AvailabilityState)c.PropertyValue) == GlobalProcApp.AvailabilityState.Maintenance ||
                                                                    ((GlobalProcApp.AvailabilityState)c.PropertyValue) == GlobalProcApp.AvailabilityState.Retooling ||
                                                                    ((GlobalProcApp.AvailabilityState)c.PropertyValue) == GlobalProcApp.AvailabilityState.ScheduledBreak)).Sum(x => x.Duration.TotalSeconds);

                    var unscheduledTime = timelinePropLogs.Where(c => c.PropertyLogModelType == ACPropertyLogModelType.PropertyLog && c.DisplayOrder == item.DisplayOrder &&
                                                     ((GlobalProcApp.AvailabilityState)c.PropertyValue) == GlobalProcApp.AvailabilityState.UnscheduledBreak).Sum(x => x.Duration.TotalSeconds);

                    var operatingTime = scheduledTime - unscheduledTime;

                    item.OEE = Math.Round((operatingTime / scheduledTime) * 100, 2);
                }
                else
                {
                    item.OEE = Math.Round(((item.TotalDuration - item.TotalAlarmDuration).TotalSeconds / item.TotalDuration.TotalSeconds) * 100, 2);
                }

            }
        }

        /// <summary>
        /// Overrides ToString method.
        /// </summary>
        /// <returns>ACCaption if is not null, otherwise returns empty string.</returns>
        public override string ToString()
        {
            return ACCaption ?? "";
        }

        #endregion
    }

    /// <summary>
    /// Represents the enumeration for a ACPropertyLogModel types.
    /// </summary>
    public enum ACPropertyLogModelType : short
    {
        Class = 10,
        Property = 20,
        PropertyLog = 30,
        PropertyLogAlarm = 40
    }

    #endregion
}

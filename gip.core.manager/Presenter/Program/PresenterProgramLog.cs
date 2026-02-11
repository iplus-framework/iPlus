using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.core.manager
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PresenterProgramLog'}de{'PresenterProgramLog'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACProgram.ClassName)]
    public class PresenterProgramLog : ACBSONav
    {
        #region c'tors
        public PresenterProgramLog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="") 
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            AccessPrimary.NavSearch(Db);
            _DisplayOrder = 0;
            wrapperList.Clear();
            if (CurrentACProgram != null)
                CreateProgramLogWrapper(CurrentACProgram.ACProgramLog_ACProgram.Where(c => c.ParentACProgramLogID == null));
            ProgramLogWrapperList = wrapperList;
            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            //ProgramLogWrapperList = null;
            //CurrentProgramLogWrapper = null;
            //ProgramLogWrapperRootList = null;
            return await base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties

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
                return Database.ContextIPlus as Database;
            }
        }

        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        /// <summary>
        /// The _ access primary
        /// </summary>
        ACAccessNav<ACProgram> _AccessPrimary;
        /// <summary>
        /// Gets the access primary.
        /// </summary>
        /// <value>The access primary.</value>
        [ACPropertyAccessPrimary(9999, "ACProgram")]
        public ACAccessNav<ACProgram> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessPrimary = navACQueryDefinition.NewAccessNav<ACProgram>(ACProgram.ClassName, this);
                }
                return _AccessPrimary;
            }
        }

        [ACPropertyCurrent(999,"ACProgram","en{'ACProgram'}de{'ACProgram'}")]
        public virtual ACProgram CurrentACProgram
        {
            get
            {
                return _AccessPrimary.Current;
            }
            set
            {
                _AccessPrimary.Current = value;
                _DisplayOrder = 0;
                wrapperListSearch.Clear();
                wrapperList.Clear();
                SearchText = "";
                OnPropertyChanged("SearchText");
                CurrentFilter = FilterItemsList.FirstOrDefault(c => (Global.TimelineItemStatus)c.Value == Global.TimelineItemStatus.OK);
                OnPropertyChanged("CurrentFilter");
                if (CurrentACProgram != null)
                    CreateProgramLogWrapper(CurrentACProgram.ACProgramLog_ACProgram.Where(c => c.ParentACProgramLogID == null));
                ProgramLogWrapperList = wrapperList;
                ProgramLogWrapperRootList = ProgramLogWrapperList.Where(c => c.ACProgramLog.ParentACProgramLogID == null);
                OnPropertyChanged("CurrentACProgram");
            }
        }

        [ACPropertyList(999,"ACProgram","en{'ACProgramList'}de{'ACProgramList'}")]
        public IList<ACProgram> ACProgramList
        {
            get 
            {
                return _AccessPrimary.NavList;
            }
        }

        private ObservableCollection<ProgramLogWrapper> _ProgramLogWrapperList = new ObservableCollection<ProgramLogWrapper>();

        private IEnumerable<ProgramLogWrapper> _ProgramLogWrapperRootList;
        [ACPropertyList(999, "ACProgramLogRoot", "en{'Program log root'}de{'Program log root'}")]
        public IEnumerable<ProgramLogWrapper> ProgramLogWrapperRootList
        {
            get 
            {
                return _ProgramLogWrapperRootList;
            }
            set
            {
                _ProgramLogWrapperRootList = value;
                OnPropertyChanged("ProgramLogWrapperRootList");
            }
        }

        private ProgramLogWrapper _ProgramLogWrapperRoot;
        [ACPropertyCurrent(999, "ACProgramLogRoot", "en{'Program log root'}de{'Program log root'}")]
        public ProgramLogWrapper ProgramLogWrapperRoot
        {
            get 
            {
                return _ProgramLogWrapperRoot;
            }
            set
            {
                _ProgramLogWrapperRoot = value;
            }
        }

        private ProgramLogWrapper _CurrentProgramLogWrapper;
        [ACPropertyCurrent(999,"ProgramLog","en{'Program log'}de{'Program log'}")]
        public ProgramLogWrapper CurrentProgramLogWrapper
        {
            get 
            {
                return _CurrentProgramLogWrapper;
            }
            set
            {
                if(_CurrentProgramLogWrapper != value)
                    _CurrentProgramLogWrapper = value;
            }
        }

        [ACPropertyList(999, "ProgramLog", "en{'Program logs'}de{'Program logs'}")]
        public ObservableCollection<ProgramLogWrapper> ProgramLogWrapperList
        {
            get
            {
                return _ProgramLogWrapperList;
            }
            set 
            {
                _ProgramLogWrapperList = value;
                OnPropertyChanged("ProgramLogWrapperList");
            }
        }

        private string _SearchText;
        [ACPropertyInfo(999, "", "en{'Search'}de{'Suche'}")]
        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                Search();
            }
        }
        
        private bool _IsRulerVisible;
        [ACPropertyInfo(999, "", "en{'Ruler'}de{'Ruler'}")]
        public bool IsRulerVisible
        {
            get { return _IsRulerVisible; }
            set 
            {
                _IsRulerVisible = value;
                OnPropertyChanged("IsRulerVisible");
            }
        }

        private DateTime _CurrentDateTime;
        [ACPropertyInfo(999, "", "en{'DateTime'}de{'DateTime'}")]
        public DateTime CurrentDateTime
        {
            get
            {
                return _CurrentDateTime;
            }
            set
            {
                _CurrentDateTime = value;
                OnPropertyChanged("CurrentDateTime");
            }
        }

        private ACValueItem _CurrentFilter;
        [ACPropertyCurrent(999,"ItemsFilter","en{'Filter'}de{'Filter'}")]
        public ACValueItem CurrentFilter
        {
            get 
            {
                if (_CurrentFilter == null)
                    _CurrentFilter = FilterItemsList.FirstOrDefault(c => (Global.TimelineItemStatus)c.Value == Global.TimelineItemStatus.OK);
                return _CurrentFilter;
            }
            set
            {
                _CurrentFilter = value;
                Filter();
            }
        }

        private ACValueItemList _FilterItemsList;
        [ACPropertyList(999,"ItemsFilter","en{'Filter list'}de{'Filter list'}")]
        public ACValueItemList FilterItemsList
        {
            get 
            {
                if (_FilterItemsList == null)
                {
                    _FilterItemsList = new ACValueItemList("Global.TimelineItemStatus");
                    _FilterItemsList.AddEntry(Global.TimelineItemStatus.OK, "en{'All'}de{'Alle'}");
                    _FilterItemsList.AddEntry(Global.TimelineItemStatus.Duration, "en{'Duration < 1\"'}de{'Duration < 1\"'}");
                    _FilterItemsList.AddEntry(Global.TimelineItemStatus.Alarm, "en{'Alarm'}de{'Alarm'}");
                    //_FilterItemsList.AddEntry(Global.TimelineItemStatus.DurationAlarm, "en{'Duration < 1\" & Alarm'}de{'Duration < 1\" & Alarm'}");
                }
                return _FilterItemsList;
            }
        }

        protected int _DisplayOrder = 0;
        protected bool _IsAnyItemFound = false;
        protected bool _IsAnyItemFoundFilter = false;
        protected bool _IsFromVBBSOControlPA = false;
        protected string _searchTextFromWF = null;

        protected ObservableCollection<ProgramLogWrapper> wrapperList = new ObservableCollection<ProgramLogWrapper>();
        protected ObservableCollection<ProgramLogWrapper> wrapperListSearch = new ObservableCollection<ProgramLogWrapper>();

        #endregion

        #region Methods

        private void Search()
        {
            if (string.IsNullOrEmpty(_SearchText))
            {
                if((Global.TimelineItemStatus)CurrentFilter.Value != Global.TimelineItemStatus.OK)
                {
                    Filter();
                }
                else 
                {
                    ProgramLogWrapperList = wrapperList;
                    if (!_IsFromVBBSOControlPA)
                        ProgramLogWrapperRootList = ProgramLogWrapperList.Where(c => c.ACProgramLog.ParentACProgramLogID == null);
                    else
                        ProgramLogWrapperRootList = wrapperList;
                    wrapperListSearch.Clear();
                }
            }
            else
            {
                foreach (var item in wrapperList)
                    item.IsShowInSearch = false;

                _DisplayOrder = 0;
                wrapperListSearch.Clear();
                CreateProgramLogWrapperSearch(CurrentACProgram.ACProgramLog_ACProgram.Where(c => c.ParentACProgramLogID == null));
                if (!_IsFromVBBSOControlPA)
                {
                    SearchLogs(wrapperList.Where(c => c.ACProgramLog.ParentACProgramLogID == null), true);
                    UpdateProgramLogWrappers(wrapperList.Where(c => c.IsShowInSearch && c.ACProgramLog.ParentACProgramLogID == null), true);
                    UpdateStatus(wrapperListSearch.Where(c => c.ACProgramLog.ParentACProgramLogID == null), wrapperListSearch);
                }
                else 
                {
                    SearchLogs(wrapperList, false);
                    UpdateProgramLogWrappers(wrapperList, false);
                }
                if (!_IsAnyItemFound)
                {
                    ProgramLogWrapperList = wrapperList;
                    wrapperListSearch.Clear();
                }
                ProgramLogWrapperList = wrapperListSearch;
                if (!_IsFromVBBSOControlPA)
                    ProgramLogWrapperRootList = ProgramLogWrapperList.Where(c => c.ACProgramLog.ParentACProgramLogID == null);
                else
                    ProgramLogWrapperRootList = wrapperListSearch;
                _IsAnyItemFound = false;
            }
        }

        private void Filter()
        {
            if ((Global.TimelineItemStatus)CurrentFilter.Value == Global.TimelineItemStatus.OK)
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    Search();
                }
                else
                {
                    ProgramLogWrapperList = wrapperList;
                    if (!_IsFromVBBSOControlPA)
                        ProgramLogWrapperRootList = ProgramLogWrapperList.Where(c => c.ACProgramLog.ParentACProgramLogID == null);
                    else
                    {
                        ProgramLogWrapperRootList = ProgramLogWrapperList;
                    }
                }
            }
            else
            {
                foreach (var item in wrapperList)
                    item.IsShowInFilter = false;
                _DisplayOrder = 0;
                wrapperListSearch.Clear();
                CreateProgramLogWrapperSearch(CurrentACProgram.ACProgramLog_ACProgram.Where(c => c.ParentACProgramLogID == null));

                if (!_IsFromVBBSOControlPA)
                {
                    FilterLogs(wrapperList.Where(c => c.ACProgramLog.ParentACProgramLogID == null),true);
                    UpdateProgramLogWrappers(wrapperList.Where(c => c.IsShowInFilter && c.ACProgramLog.ParentACProgramLogID == null), true);
                    UpdateStatus(wrapperListSearch.Where(c => c.ACProgramLog.ParentACProgramLogID == null), wrapperListSearch);
                }
                else 
                {
                    FilterLogs(wrapperList, false);
                    UpdateProgramLogWrappers(wrapperList,false);
                }

                if (!_IsAnyItemFoundFilter)
                {
                    ProgramLogWrapperList = wrapperList;
                    wrapperListSearch.Clear();
                }

                ProgramLogWrapperList = wrapperListSearch;
                if (!_IsFromVBBSOControlPA)
                    ProgramLogWrapperRootList = ProgramLogWrapperList.Where(c => c.ACProgramLog.ParentACProgramLogID == null);
                else
                    ProgramLogWrapperRootList = wrapperListSearch;
                _IsAnyItemFoundFilter = false;
            }
        }

        protected virtual void CreateProgramLogWrapper(IEnumerable<ACProgramLog> items)
        {
            foreach (ACProgramLog acprogramlog in items.OrderBy(c => c.StartDate))
            {
                ProgramLogWrapper tempWrapper = new ProgramLogWrapper(){ ACProgramLog = acprogramlog, DisplayOrder = _DisplayOrder++ };
                wrapperList.Add(tempWrapper);
                if (!_IsFromVBBSOControlPA)
                {
                    CreateProgramLogWrapper(acprogramlog.ACProgramLog_ParentACProgramLog);
                    if (acprogramlog.ParentACProgramLogID != null)
                    {
                        var tempLog = wrapperList.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == acprogramlog.ParentACProgramLogID);
                        tempLog.Items.Add(tempWrapper);
                        if (tempWrapper.Status.Contains(Global.TimelineItemStatus.Alarm) || tempWrapper.Status.Contains(Global.TimelineItemStatus.ChildAlarm))
                            tempLog.ChildAlarm = true;
                    }
                }
            }
        }

        protected virtual void CreateProgramLogWrapperSearch(IEnumerable<ACProgramLog> items)
        {
            if (!_IsFromVBBSOControlPA)
            {
                foreach (ACProgramLog acprogramlog in items.OrderBy(c => c.StartDate))
                {
                    if (wrapperList.Any(c => c.ACProgramLog == acprogramlog))
                    {
                        ProgramLogWrapper tempWrapper = new ProgramLogWrapper() { ACProgramLog = acprogramlog, DisplayOrder = _DisplayOrder++ };
                        wrapperListSearch.Add(tempWrapper);
                        CreateProgramLogWrapperSearch(acprogramlog.ACProgramLog_ParentACProgramLog);
                        if (acprogramlog.ParentACProgramLogID != null)
                        {
                            var tempLog = wrapperListSearch.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == acprogramlog.ParentACProgramLogID);
                            tempLog.Items.Add(tempWrapper);
                            if (tempWrapper.Status.Contains(Global.TimelineItemStatus.Alarm) || tempWrapper.Status.Contains(Global.TimelineItemStatus.ChildAlarm))
                                tempLog.ChildAlarm = true;
                        }
                    }
                }
            }
            else
                foreach(ProgramLogWrapper wrapper in wrapperList)
                    wrapperListSearch.Add(new ProgramLogWrapper() { ACProgramLog = wrapper.ACProgramLog, DisplayOrder = _DisplayOrder++ });
        }

        private void UpdateProgramLogWrappers(IEnumerable<ProgramLogWrapper> items, bool IsTreeStructure)
        {
            foreach (ProgramLogWrapper programLogWrapper in items)
            {
                if (IsTreeStructure)
                {
                    UpdateProgramLogWrappers(programLogWrapper.Items, IsTreeStructure);
                    if (!string.IsNullOrEmpty(SearchText) && !programLogWrapper.IsShowInSearch)
                    {
                        ProgramLogWrapper removeItem = wrapperListSearch.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ACProgramLogID);
                        wrapperListSearch.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ParentACProgramLogID).Items.Remove(removeItem);
                        wrapperListSearch.Remove(removeItem);
                    }
                    if ((Global.TimelineItemStatus)CurrentFilter.Value != Global.TimelineItemStatus.OK && !programLogWrapper.IsShowInFilter)
                    {
                        ProgramLogWrapper removeItem = wrapperListSearch.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ACProgramLogID);
                        if (removeItem != null)
                        {
                            wrapperListSearch.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ParentACProgramLogID).Items.Remove(removeItem);
                            wrapperListSearch.Remove(removeItem);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(SearchText) && !programLogWrapper.IsShowInSearch)
                    {
                        ProgramLogWrapper removeItem = wrapperListSearch.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ACProgramLogID);
                        wrapperListSearch.Remove(removeItem);
                    }
                    if ((Global.TimelineItemStatus)CurrentFilter.Value != Global.TimelineItemStatus.OK && !programLogWrapper.IsShowInFilter)
                    {
                        ProgramLogWrapper removeItem = wrapperListSearch.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ACProgramLogID);
                        wrapperListSearch.Remove(removeItem);
                    }
                }
                    
            }
        }

        private void UpdateProgramLogWrappersFromWF(IEnumerable<ProgramLogWrapper> items)
        {
            foreach (ProgramLogWrapper programLogWrapper in items)
            {
                UpdateProgramLogWrappersFromWF(programLogWrapper.Items);
                if (!string.IsNullOrEmpty(_searchTextFromWF) && !programLogWrapper.IsShowInSearch)
                {
                    ProgramLogWrapper removeItem = wrapperList.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ACProgramLogID);
                    wrapperList.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ParentACProgramLogID).Items.Remove(removeItem);
                    wrapperList.Remove(removeItem);
                }
                if ((Global.TimelineItemStatus)CurrentFilter.Value != Global.TimelineItemStatus.OK && !programLogWrapper.IsShowInFilter)
                {
                    ProgramLogWrapper removeItem = wrapperList.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ACProgramLogID);
                    if (removeItem != null)
                    {
                        wrapperList.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == programLogWrapper.ACProgramLog.ParentACProgramLogID).Items.Remove(removeItem);
                        wrapperList.Remove(removeItem);
                    }
                }
            }
        }

        private void SearchLogs(IEnumerable<ProgramLogWrapper> items, bool IsTreeStructure)
        {
            foreach (ProgramLogWrapper wrapper in items)
            {
                if (IsTreeStructure)
                    SearchLogs(wrapper.Items, IsTreeStructure);
                if ((Global.TimelineItemStatus)CurrentFilter.Value == Global.TimelineItemStatus.OK)
                    CheckSearch(wrapper, IsTreeStructure);
                else if (wrapper.IsShowInFilter)
                    CheckSearch(wrapper, IsTreeStructure);
            }
        }

        private void CheckSearch(ProgramLogWrapper wrapper, bool IsTreeStructure)
        {
            if (wrapper.ACProgramLog.ACUrl.ToLower().Contains(_SearchText.ToLower()))
            {
                wrapper.IsShowInSearch = true;
                _IsAnyItemFound = true;
            }
            if (IsTreeStructure && wrapper.IsShowInSearch && wrapper.ACProgramLog.ParentACProgramLogID != null)
                wrapperList.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == wrapper.ACProgramLog.ParentACProgramLogID).IsShowInSearch = wrapper.IsShowInSearch;
        }

        private void FilterLogs(IEnumerable<ProgramLogWrapper> items, bool IsTreeStructure)
        {
            foreach (ProgramLogWrapper wrapper in items)
            {   
                if(IsTreeStructure)
                    FilterLogs(wrapper.Items, IsTreeStructure);
                if (string.IsNullOrEmpty(SearchText))
                    CheckFilter(wrapper, IsTreeStructure);
                else if (wrapper.IsShowInSearch)
                    CheckFilter(wrapper, IsTreeStructure);
            }
        }

        private void CheckFilter(ProgramLogWrapper wrapper, bool IsTreeStructure)
        {
            if (wrapper.Status.Contains((Global.TimelineItemStatus)CurrentFilter.Value))
            {
                wrapper.IsShowInFilter = true;
                _IsAnyItemFoundFilter = true;
            }
            //else if ((Global.TimelineItemStatus)CurrentFilter.Value == Global.TimelineItemStatus.Duration && wrapper.Status.Contains(Global.TimelineItemStatus.DurationChildAlarm))
            //{
            //    wrapper.IsShowInFilter = true;
            //    _IsAnyItemFoundFilter = true;
            //}
            if (IsTreeStructure && wrapper.IsShowInFilter && wrapper.ACProgramLog.ParentACProgramLogID != null)
                wrapperList.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == wrapper.ACProgramLog.ParentACProgramLogID).IsShowInFilter = wrapper.IsShowInFilter;
        }

        private void SearchFromWF()
        {
            foreach (var item in wrapperListSearch)
                item.IsShowInSearch = false;
            _DisplayOrder = 0;
            wrapperListSearch.Clear();
            CreateProgramLogWrapperSearch(CurrentACProgram.ACProgramLog_ACProgram.Where(c => c.ParentACProgramLogID == null));
            SearchLogsFromWF(wrapperListSearch.Where(c => c.ACProgramLog.ParentACProgramLogID == null));
            UpdateProgramLogWrappersFromWF(wrapperListSearch.Where(c => c.IsShowInSearch && c.ACProgramLog.ParentACProgramLogID == null));
            UpdateStatus(wrapperList.Where(c => c.ACProgramLog.ParentACProgramLogID == null), wrapperList);
            if (!_IsAnyItemFound)
            {
                ProgramLogWrapperList = wrapperListSearch;
                wrapperList.Clear();
            }
            ProgramLogWrapperList = wrapperList;
            ProgramLogWrapperRootList = ProgramLogWrapperList.Where(c => c.ACProgramLog.ParentACProgramLogID == null);
            _IsAnyItemFound = false;
        }

        private void SearchLogsFromWF(IEnumerable<ProgramLogWrapper> items)
        {
            foreach (ProgramLogWrapper wrapper in items)
            {
                if (CheckSearchFromWF(wrapper))
                    MarkChildAsTrue(wrapper.Items);
                else
                    SearchLogsFromWF(wrapper.Items);
            }
        }

        private void UpdateStatus(IEnumerable<ProgramLogWrapper> items, ObservableCollection<ProgramLogWrapper> collectionForUpdate)
        {
            foreach (ProgramLogWrapper wrapper in items)
            {
                wrapper.ChildAlarm = false;
                UpdateStatus(wrapper.Items, collectionForUpdate);
                if ((Global.TimelineItemStatus)CurrentFilter.Value != Global.TimelineItemStatus.Duration &&
                    (wrapper.Status.Contains(Global.TimelineItemStatus.Alarm) || wrapper.Status.Contains(Global.TimelineItemStatus.ChildAlarm)) &&
                    wrapper.ACProgramLog.ParentACProgramLogID != null)
                {
                    collectionForUpdate.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == wrapper.ACProgramLog.ParentACProgramLogID).ChildAlarm = true;
                }
            }
        }

        private bool CheckSearchFromWF(ProgramLogWrapper wrapper)
        {
            bool result = false;
            if (wrapper.ACProgramLog.ACUrl.ToLower().EndsWith(_searchTextFromWF.ToLower()))
            {
                result = true;
                wrapper.IsShowInSearch = true;
                _IsAnyItemFound = true;
            }
            if (wrapper.IsShowInSearch && wrapper.ACProgramLog.ParentACProgramLogID != null)
                wrapperListSearch.FirstOrDefault(c => c.ACProgramLog.ACProgramLogID == wrapper.ACProgramLog.ParentACProgramLogID).IsShowInSearch = wrapper.IsShowInSearch;
            return result;
        }

        private void MarkChildAsTrue(IEnumerable<ProgramLogWrapper> items)
        {
            foreach (var item in items)
            {
                item.IsShowInSearch = true;
                MarkChildAsTrue(item.Items);
            }
        }

        [ACMethodInteraction("", "en{'Show Details'}de{'Details anzeigen'}", 900, true, "CurrentProgramLogWrapper")]
        public async Task ShowDetails()
        {
            CurrentACMethod = ACConvert.XMLToObject<ACMethod>(CurrentProgramLogWrapper.ACProgramLog.XMLConfig, true, Db);
            await ShowDialogAsync(this, "DetailsDialog");
        }

        [ACMethodInfo("", "en{'ShowACProgramLog'}de{'ShowACProgramLog'}",901,false)]
        public void ShowACProgramLog(ACValueList param)
        {
            ACValue isForProcessModule = param.FirstOrDefault(c => c.ACIdentifier == "IsForProcessModule");
            if (isForProcessModule == null || isForProcessModule.ParamAsBoolean)
            {
                if (param.Any(c => c.ObjectType == typeof(ACProgram)))
                {
                    ACProgram acprogram = param.FirstOrDefault(c => c.ObjectType == typeof(ACProgram)).Value as ACProgram;
                    if (param.FirstOrDefault(c => c.ACIdentifier == "WorkflowACUrl").Value != null)
                        ShowLogFromACProgram(acprogram, param.FirstOrDefault(c => c.ACIdentifier == "WorkflowACUrl").Value.ToString());
                    else
                        ShowLogFromACProgram(acprogram, null);
                }
                else
                {
                    string componentACUrl = param.FirstOrDefault(c => c.ACIdentifier == "ComponentACUrl").Value.ToString();
                    DateTime from = DateTime.Parse(param.FirstOrDefault(c => c.ACIdentifier == "SearchFrom").Value.ToString());
                    DateTime to = DateTime.Parse(param.FirstOrDefault(c => c.ACIdentifier == "SearchTo").Value.ToString());
                    short searchMode = (short)param.FirstOrDefault(c => c.ACIdentifier == "SearchMode")?.Value;
                    //param.Add(new ACValue("FindFirst", findFirst));
                    if (componentACUrl != null)
                        ShowLogFromVBBSOControlPA(componentACUrl, from, to, searchMode);
                }
            }
            else
            {
                string componentACUrl = param.FirstOrDefault(c => c.ACIdentifier == "ComponentACUrl").Value.ToString();
                DateTime from = DateTime.Parse(param.FirstOrDefault(c => c.ACIdentifier == "SearchFrom").Value.ToString());
                DateTime to = DateTime.Parse(param.FirstOrDefault(c => c.ACIdentifier == "SearchTo").Value.ToString());

                if (componentACUrl != null)
                    ShowLogFromVBBSOControlPAModule(componentACUrl, from, to);
            }
        }

        public async Task ShowLogFromACProgram(ACProgram currentACProgram, string wfACUrl)
        {
            CurrentACProgram = currentACProgram;
            if (wfACUrl != null)
            {
                _searchTextFromWF = wfACUrl;
                SearchFromWF();
            }

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                IEnumerable<ACPropertyLogSumOfProgram> sumOfProgram = gip.core.datamodel.ACPropertyLog.GetSummarizedDurationsOfProgram(Db, currentACProgram.ACProgramID, new string[] { "AvailabilityState" } );
                ACPropertyLogSumOfProgram lastEntry = sumOfProgram?.LastOrDefault();
                if (lastEntry != null)
                {
                    DateTime toTime = DateTime.Now;
                    var lastPropertyLog = lastEntry.Sum.LastOrDefault().PropertyLog;
                    if (lastPropertyLog != null)
                        toTime = lastPropertyLog.EventTime;
                    else
                    {
                        var lastProgramLog = Db.ACProgramLog.Where(c => c.ACProgramID == CurrentACProgram.ACProgramID && c.EndDate.HasValue).OrderByDescending(c => c.EndDate).FirstOrDefault();
                        if (lastProgramLog != null)
                            toTime = lastProgramLog.EndDate.Value;
                        else
                            toTime = CurrentACProgram.UpdateDate.AddDays(1);
                    }

                    IEnumerable<ACPropertyLogSum> sum2 = gip.core.datamodel.ACPropertyLog.AggregateDurationOfPropertyValues(Db, CurrentACProgram.InsertDate, toTime, lastEntry.ACClass.ACClassID, "AvailabilityState");
                }
            }
#endif

            await ShowDialogAsync(this, "PresenterProgramLogDialog");
            //ShowWindow(this,
            //    "PresenterProgramLogDialog", 
            //    true, 
            //    Global.VBDesignContainer.DockableWindow, 
            //    Global.VBDesignDockState.Tabbed, 
            //    Global.VBDesignDockPosition.Bottom, 
            //    Global.ControlModes.Enabled);

            if (this.ParentACComponent != null)
                await this.ParentACComponent.StopComponent(this);
        }

        public async Task ShowLogFromVBBSOControlPA(string componentACUrl, DateTime timeFrom, DateTime timeTo, short searchMode = 0)
        {
            _IsFromVBBSOControlPA = true;
            _DisplayOrder = 0;
            wrapperList.Clear();
            if (searchMode > 0)
            {
                ACProgramLog programLog = null;
                if (searchMode == 2)
                {
                    programLog = Db.ACProgramLog.Where(c => c.ACUrl.Contains(componentACUrl) && c.StartDate.HasValue && c.StartDate < timeFrom).OrderByDescending(c => c.StartDate).FirstOrDefault();
                    if (programLog != null)
                        timeFrom = programLog.StartDate.Value.AddSeconds(-1);
                }
                if (searchMode == 1 || (programLog == null && searchMode == 2))
                {
                    programLog = Db.ACProgramLog.Where(c => c.ACUrl.Contains(componentACUrl) && c.StartDate.HasValue).OrderByDescending(c => c.StartDate).FirstOrDefault();
                    if (programLog != null)
                        timeFrom = programLog.StartDate.Value.AddSeconds(-1);
                }

            }
            CreateProgramLogWrapper(Db.ACProgramLog.Where(c => c.ACUrl.Contains(componentACUrl) && c.StartDate >= timeFrom && c.EndDate < timeTo));
            ProgramLogWrapperList = wrapperList;
            ProgramLogWrapperRootList = ProgramLogWrapperList;
            await ShowDialogAsync(this, "PresenterProgramLogDialog");
            //ShowWindow(this,
            //    "PresenterProgramLogDialog",
            //    true,
            //    Global.VBDesignContainer.DockableWindow,
            //    Global.VBDesignDockState.Tabbed,
            //    Global.VBDesignDockPosition.Bottom,
            //    Global.ControlModes.Enabled);
            _IsFromVBBSOControlPA = false;
        }

        public async Task ShowLogFromVBBSOControlPAModule(string componentACUrl, DateTime timeFrom, DateTime timeTo)
        {
            _IsFromVBBSOControlPA = true;
            _DisplayOrder = 0;
            wrapperList.Clear();

            ACClass componentACClass = Db.ACClass.Where(c => c.ACURLComponentCached == componentACUrl).FirstOrDefault();
            if (componentACClass != null)
            {
                ACRoutingParameters routingParameters = new ACRoutingParameters()
                {
                    Direction = RouteDirections.Backwards,
                    IncludeAllocated = true,
                    IncludeReserved = true,
                    SelectionRuleID = PAProcessModule.SelRuleID_ProcessModuleWithFunction,
                    ResultMode = RouteResultMode.ShortRoute
                };

                List<ACClass> processModules = new List<ACClass>();

                RoutingResult rr = ACRoutingService.FindSuccessors(componentACClass, routingParameters);
                if (rr != null && rr.Routes != null && rr.Routes.Any())
                {
                    RouteItem[] routeItems = rr.Routes.Select(c => c.GetRouteSource()).ToArray();
                    foreach(RouteItem rItem in routeItems)
                    {
                        if (!rItem.IsAttached)
                            rItem.AttachTo(Db);
                    }

                    processModules.AddRange(routeItems.Select(c => c.Source));
                }

                routingParameters.Direction = RouteDirections.Forwards;
                rr = ACRoutingService.FindSuccessors(componentACClass, routingParameters);
                if (rr != null && rr.Routes != null && rr.Routes.Any())
                {
                    RouteItem[] routeItems = rr.Routes.Select(c => c.GetRouteTarget()).ToArray();
                    foreach (RouteItem rItem in routeItems)
                    {
                        if (!rItem.IsAttached)
                            rItem.AttachTo(Db);
                    }

                    processModules.AddRange(routeItems.Select(c => c.Target));
                }

                if (!processModules.Any())
                {
                    componentACClass = componentACClass.ACClass1_ParentACClass;

                    routingParameters.Direction = RouteDirections.Backwards;
                    rr = ACRoutingService.FindSuccessors(componentACClass, routingParameters);
                    if (rr != null && rr.Routes != null && rr.Routes.Any())
                    {
                        RouteItem[] routeItems = rr.Routes.Select(c => c.GetRouteSource()).ToArray();
                        foreach (RouteItem rItem in routeItems)
                        {
                            if (!rItem.IsAttached)
                                rItem.AttachTo(Db);
                        }

                        processModules.AddRange(routeItems.Select(c => c.Source));
                    }

                    routingParameters.Direction = RouteDirections.Forwards;
                    rr = ACRoutingService.FindSuccessors(componentACClass, routingParameters);
                    if (rr != null && rr.Routes != null && rr.Routes.Any())
                    {
                        RouteItem[] routeItems = rr.Routes.Select(c => c.GetRouteTarget()).ToArray();
                        foreach (RouteItem rItem in routeItems)
                        {
                            if (!rItem.IsAttached)
                                rItem.AttachTo(Db);
                        }

                        processModules.AddRange(routeItems.Select(c => c.Target));
                    }
                }

                if (processModules.Any())
                {
                    string[] processModulesACUrl = processModules.Select(c => c.ACUrlComponent).ToArray();

                    string compClassID = componentACClass.ACClassID.ToString();
                    CreateProgramLogWrapper(Db.ACProgramLog.Where(c => processModulesACUrl.Any(x => c.ACUrl.Contains(x) && c.StartDate >= timeFrom && c.EndDate < timeTo && c.XMLConfig.Contains(compClassID))));
                    ProgramLogWrapperList = wrapperList;
                    ProgramLogWrapperRootList = ProgramLogWrapperList;
                    await ShowDialogAsync(this, "PresenterProgramLogDialog", string.Format("{0} ({1})", componentACClass.ACCaption, componentACClass.ACUrlComponent));
                }
            }
            _IsFromVBBSOControlPA = false;
        }


        [ACMethodInfo("", "en{'Show complex value'}de{'Komplexen Wert anzeigen'}", 9999)]
        public void ShowComplexValue()
        {
            ACValue selParam = SelectedACMethodParam;
            if (selParam == null || selParam.Value == null)
                return;

            Route currentRoute = selParam.Value as Route;
            if (currentRoute == null)
                return;

            IACVBBSORouteSelector routeSelector = ACUrlCommand("VBBSORouteSelector_Child") as IACVBBSORouteSelector;
            if (routeSelector == null)
            {
                Messages.ErrorAsync(this, "Route selector is not installed");
                return;
            }

            //routeSelector.EditRoutesWithAttach(currentRoute, true, true, true);
            routeSelector.ShowRoute(currentRoute);

            ACComponent comp = routeSelector as ACComponent;
            if (comp != null)
                comp.Stop();
        }

        public bool IsEnabledShowComplexValue()
        {
            ACValue selParam = SelectedACMethodParam;

            if (selParam == null)
                return false;
            if (selParam.ObjectType == null)
                return false;
            if (selParam.ObjectType.IsValueType)
                return false;

            return true;
        }

        #endregion

        #region ACMethod
        private ACMethod _CurrentACMethod;
        [ACPropertyInfo(9999)]
        public ACMethod CurrentACMethod
        {
            get
            {
                return _CurrentACMethod;
            }
            set
            {
                _CurrentACMethod = value;
                OnPropertyChanged("CurrentACMethod");
                OnPropertyChanged("ACMethodParamList");
                OnPropertyChanged("ACMethodResultParamList");
            }
        }

        [ACPropertyList(9999, "ParamACMethod")]
        public IEnumerable<ACValue> ACMethodParamList
        {
            get
            {
                if (CurrentACMethod != null)
                    return CurrentACMethod.ParameterValueList;
                return null;
            }
        }

        private ACValue _SelectedACMethodParam;
        [ACPropertySelected(9999, "ParamACMethod")]
        public ACValue SelectedACMethodParam
        {
            get
            {
                return _SelectedACMethodParam;
            }
            set
            {
                _SelectedACMethodParam = value;
                OnPropertyChanged("SelectedACMethodParam");
            }
        }


        [ACPropertyList(9999, "ResultACMethod")]
        public IEnumerable<ACValue> ACMethodResultParamList
        {
            get
            {
                if (CurrentACMethod != null)
                    return CurrentACMethod.ResultValueList;
                return null;
            }
        }

        private ACValue _SelectedACMethodResultParam;
        [ACPropertySelected(9999, "ResultACMethod")]
        public ACValue SelectedACMethodResultParam
        {
            get
            {
                return _SelectedACMethodResultParam;
            }
            set
            {
                _SelectedACMethodResultParam = value;
                OnPropertyChanged("SelectedACMethodResultParam");
            }
        }
        #endregion

        #region Alarm

        private MsgAlarmLog _SelectedAlarmLog;
        [ACPropertySelected(999,"Alarms")]
        public MsgAlarmLog SelectedAlarmLog
        {
            get
            {
                return _SelectedAlarmLog;
            }
            set
            {
                _SelectedAlarmLog = value;
            }
        }

        [ACPropertyList(999,"Alarms")]
        public IEnumerable<MsgAlarmLog> AlarmList
        {
            get
            {
                if (   CurrentProgramLogWrapper != null 
                    && CurrentProgramLogWrapper.ACProgramLog != null)
                    return CurrentProgramLogWrapper.ACProgramLog.MsgAlarmLog_ACProgramLog.OrderBy(c => c.InsertDate);
                return null;
            }
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"ShowDetails":
                    ShowDetails();
                    return true;
                case"ShowACProgramLog":
                    ShowACProgramLog((ACValueList)acParameter[0]);
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}

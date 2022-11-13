using System;
using System.Collections.Generic;
using System.Linq;
using gip.core.autocomponent;
using gip.core.datamodel;
using System.Collections.ObjectModel;

namespace gip.bso.iplus
{
    /// <summary>
    /// Represents the configurator for alarm messenger.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Configurator for alarm messenger'}de{'Konfigurator für Alarm-Verteiler'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, false, true)]
    public class BSOAlarmMessengerConfig : ACBSO
    {
        #region ctor's   

        /// <summary>
        /// Creates a new instance of BSOAlarmMessengerConfig.
        /// </summary>
        /// <param name="acType"></param>
        /// <param name="content"></param>
        /// <param name="parentACObject"></param>
        /// <param name="parameter"></param>
        /// <param name="acIdentifier"></param>
        public BSOAlarmMessengerConfig(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
                               base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _DefaultAlarmMessengerACUrl = new ACPropertyConfigValue<string>(this, "DefaultAlarmMessengerACUrl", "");
        }

        /// <summary>
        /// Init's this instance.
        /// </summary>
        /// <param name="startChildMode">The start child mode parameter.</param>
        /// <returns>Returns true if initialization is successfull, otherwise false.</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            var result = base.ACInit(startChildMode);
            if (result)
            {
                if (ParentACComponent is VBBSOControlDialog)
                {
                    _controlDialog = ParentACComponent as VBBSOControlDialog;
                    _controlDialog.PropertyChanged += ControlDialog_PropertyChanged;

                    IACObject currentSelection = ParentACComponent.ACUrlCommand("CurrentSelection", null) as IACObject;
                    if (currentSelection != null)
                    {
                        ACClass sourceClass = null;
                        ResolveSourceClass(currentSelection as IACComponent, out sourceClass);
                        CurrentACClass = sourceClass;
                    }
                }

                if (!string.IsNullOrEmpty(DefaultAlarmMessengerACUrl))
                {
                    ACClass messenger = ACUrlCommand(DefaultAlarmMessengerACUrl) as ACClass;
                    if (messenger != null)
                        DefaultAlarmMessenger = new ACClassInfoWithItems() { ValueT = messenger };
                }
            }
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        /// <summary>
        /// Deinit's this instance.
        /// </summary>
        /// <param name="deleteACClassTask">The delete ACClassTask parameter.</param>
        /// <returns>Returns true if deinitialization is successfull, otherwise false.</returns>
        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_controlDialog != null)
                _controlDialog.PropertyChanged -= ControlDialog_PropertyChanged;
            _controlDialog = null;

            DetachAlarmMessengers();

            _alarmMessengerBase = null;
            _AlarmMessengers = null;
            _AlarmMsgLevelList = null;
            _AlarmSourceList = null;
            _appManagerType = null;
            _AssignedAlarmSourceList = null;
            _paNotifyStateID = null;
            _RootCache = null;
            _SearchTextMsgProp = null;
            _SelectedAlarmMsgLevel = null;
            _SelectedAlarmSource = null;
            _SelectedAssignedAlarmSource = null;
            _SelectedExclusionRule = null;
            _InitialAssignedAlarmSourceList = null;
            bool done = base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return done;
        }

        #endregion

        #region DB

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

        #region fields

        Type _ACBSOType = typeof(ACBSO);

        Msg _CurrentDialogMsg;

        Type _appManagerType = typeof(IAppManager);

        Type _alarmMessengerBase = typeof(PAAlarmMessengerBase);

        VBBSOControlDialog _controlDialog;

        ACClassInfoWithItems _RootCache;

        ACClassInfoWithItems DefaultAlarmMessenger;

        #endregion

        #region Properties

        #region Properties -> Available Assigned

        private VBEntityObject _SelectedAlarmSource;
        /// <summary>
        /// Gets or sets the selected alarm source(message or property).
        /// </summary>
        [ACPropertySelected(401, "AlarmSource")]
        public VBEntityObject SelectedAlarmSource
        {
            get
            {
                return _SelectedAlarmSource;
            }
            set
            {
                _SelectedAlarmSource = value;
                OnPropertyChanged("SelectedAlarmSource");
                if (CurrentACClass != null)
                    RefreshAlarmMsgLevelList();
            }
        }

        private List<VBEntityObject> _AlarmSourceList;
        /// <summary>
        /// Gets the list of alarm sources(messages and/or properties).
        /// </summary>
        [ACPropertyList(402, "AlarmSource")]
        public IEnumerable<VBEntityObject> AlarmSourceList
        {
            get
            {
                if (_AlarmSourceList == null)
                    RefreshRuleList();
                return _AlarmSourceList;
            }
        }

        private AlarmMessengerConfig _SelectedAssignedAlarmSource;
        /// <summary>
        /// Gets or sets the selected assigned alarm source.
        /// </summary>
        [ACPropertySelected(403, "AssignedAlarmSource")]
        public AlarmMessengerConfig SelectedAssignedAlarmSource
        {
            get
            {
                return _SelectedAssignedAlarmSource;
            }
            set
            {
                _SelectedAssignedAlarmSource = value;
                OnPropertyChanged("SelectedAssignedAlarmSource");
                _ExclusionRuleList = null;
                OnPropertyChanged("ExclusionRuleList");
            }
        }

        private ObservableCollection<AlarmMessengerConfig> _AssignedAlarmSourceList;
        private IEnumerable<AlarmMessengerConfig> _InitialAssignedAlarmSourceList;
        /// <summary>
        /// Gets the list of assigned alarm sources.
        /// </summary>
        [ACPropertyList(404, "AssignedAlarmSource")]
        public ObservableCollection<AlarmMessengerConfig> AssignedAlarmSourceList
        {
            get
            {
                if (_AssignedAlarmSourceList == null)
                {
                    _AssignedAlarmSourceList = new ObservableCollection<AlarmMessengerConfig>();
                    foreach(var messenger in ConfigurationTargetList)
                    {
                        if (messenger == null)
                            continue;
                        //messenger.ACClassConfig_ACClass.AutoLoad(this.Database);
                        //messenger.ACClassConfig_ACClass.AutoRefresh(this.Database);
                        //messenger.ClearCacheOfConfigurationEntries();
                        IEnumerable<IACConfig> configs = messenger.ConfigurationEntries.Where(c => c.LocalConfigACUrl != null 
                                                                                                && c.LocalConfigACUrl == PAAlarmMessengerBase.SaveConfigPropName);

                        foreach (IACConfig config in configs)
                        {
                            string acurlcomp = "";
                            var tempClass = ACUrlCommand(config.KeyACUrl) as ACClass;
                            if (tempClass != null)
                                acurlcomp = tempClass.ACUrlComponent;

                            if (tempClass != null && CurrentACClass != null)
                            {
                                if (!CurrentACClass.IsDerivedClassFrom(tempClass))
                                    continue;
                            }

                            _AssignedAlarmSourceList.Add(new AlarmMessengerConfig(config.KeyACUrl, config.Value.ToString(), acurlcomp, CheckExclusionRules(config.Value.ToString(), tempClass))
                                                        { MsgPropACCaption = GetAlarmSourceCaption(config.Value.ToString()), ConfigTargetACUrl = messenger.ACUrlComponent,
                                                          TargetACUrlComp = config.Expression});
                        }
                    }
                    _InitialAssignedAlarmSourceList = _AssignedAlarmSourceList.ToList();
                }

                return _AssignedAlarmSourceList;
            }
        }

        //private ACClass _SelectedConfigurationTarget;
        ///// <summary>
        ///// Gets or sets the selected configuration target(alarm messenger).
        ///// </summary>
        //[ACPropertySelected(999, "ConfigurationTarget", "en{'Configuration target'}de{'Konfigurationsziel'}")]
        //public ACClass SelectedConfigurationTarget
        //{
        //    get
        //    {
        //        return _SelectedConfigurationTarget;
        //    }
        //    set
        //    {
        //        _SelectedConfigurationTarget = value;
        //        OnPropertyChanged("SelectedConfigurationTarget");
        //        CheckConfiguredAlarmMessages();
        //    }
        //}

        private IEnumerable<ACClass> _ConfigurationTargetList;
        /// <summary>
        /// Gets the list of configuration targets(alarm messengers).
        /// </summary>
        [ACPropertyList(405, "ConfigurationTarget")]
        public IEnumerable<ACClass> ConfigurationTargetList
        {
            get
            {
                if (_ConfigurationTargetList == null)
                {
                    if (CurrentACClass != null)
                    {
                        if (_controlDialog != null && _controlDialog.CurrentSelection != null && _controlDialog.CurrentSelection is IACComponent)
                        {
                            var alarmMessenger = FindAlarmMessenger(_controlDialog.CurrentSelection as IACComponent, CurrentACClass);
                            if (alarmMessenger != null)
                                _ConfigurationTargetList = FindBasedOnConfigurationTargets(alarmMessenger);
                            else
                                _ConfigurationTargetList = Array.Empty<ACClass>();
                        }
                    }
                    else
                    {
                        if (AlarmMessengerBase != null)
                        {
                            List<ACClass> resultList = new List<ACClass>();
                            resultList.Add(AlarmMessengerBase);
                            FindConfigurationTargets(AlarmMessengerBase, resultList);
                            _ConfigurationTargetList = resultList;
                        }
                    }
                }
                return _ConfigurationTargetList;
            }
        }

        #endregion  

        #region Properties -> Filter

        private bool _ShowProperties = true;
        /// <summary>
        /// Determines is properties shown in the alarm source list.
        /// </summary>
        [ACPropertyInfo(450, "", "en{'Show properties'}de{'Eigenschaften anzeigen'}")]
        public bool ShowProperties
        {
            get
            {
                return _ShowProperties;
            }
            set
            {
                _ShowProperties = value;
                OnPropertyChanged("ShowProperties");
                RefreshRuleList();
            }
        }

        private bool _ShowMessages = true;
        /// <summary>
        /// Determines is messages shown in the alarm source list.
        /// </summary>
        [ACPropertyInfo(451, "", "en{'Show messages'}de{'Meldungen anzeigen'}")]
        public bool ShowMessages
        {
            get
            {
                return _ShowMessages;
            }
            set
            {
                _ShowMessages = value;
                OnPropertyChanged("ShowMessages");
                RefreshRuleList();
            }
        }

        private bool _ShowFromBaseClass = true;
        /// <summary>
        /// Determines is shown messages and/or properties from base classes.
        /// </summary>
        [ACPropertyInfo(452, "", "en{'Show from base classes'}de{'Aus Basisklassen anzeigen'}")]
        public bool ShowFromBaseClass
        {
            get
            {
                return _ShowFromBaseClass;
            }
            set
            {
                _ShowFromBaseClass = value;
                OnPropertyChanged("ShowFromBaseClass");
                RefreshRuleListFromComponent();
            }
        }

        public bool _ShowFromBSO = false;
        /// <summary>
        /// Determines is shown messager and/or properties from bussiness object classes.
        /// </summary>
        [ACPropertyInfo(453, "", "en{'Show messages/properties from BSO's'}de{'Meldungen/Eigenschaften von BSOs anzeigen'}")]
        public bool ShowFromBSO
        {
            get
            {
                return _ShowFromBSO;
            }
            set
            {
                _ShowFromBSO = value;
                OnPropertyChanged("ShowFromBSO");
                RefreshRuleList();
            }
        }

        public string _SearchTextMsgProp;
        /// <summary>
        /// Gets or sets the text for search properties or methods.
        /// </summary>
        [ACPropertyInfo(454, "", "en{'Search text'}de{'Text suchen'}")]
        public string SearchTextMsgProp
        {
            get
            {
                return _SearchTextMsgProp;
            }
            set
            {
                _SearchTextMsgProp = value;
                OnPropertyChanged("SearchTextMsgProp");
            }
        }

        private Guid? _paNotifyStateID;
        private Guid _PaNotifyStateID
        {
            get
            {
                if (_paNotifyStateID == null)
                {
                    var acClass = Db.ACClass.FirstOrDefault(c => c.ACIdentifier == typeof(PANotifyState).Name);
                    if (acClass != null)
                        _paNotifyStateID = acClass.ACClassID;
                }
                return _paNotifyStateID.HasValue ? _paNotifyStateID.Value : Guid.Empty;
            }
        }

        #endregion

        #region Properties -> HierarchyDialog

        private ACClassInfoWithItems _CurrentConfigLevelRoot;
        /// <summary>
        /// Gets the root item of configuration levels.
        /// </summary>
        [ACPropertyCurrent(406, "ProjectItemRoot")]
        public ACClassInfoWithItems CurrentConfigLevelRoot
        {
            get
            {
                return _CurrentConfigLevelRoot;
            }
        }

        private ACClassInfoWithItems _CurrentConfigLevel;
        /// <summary>
        /// Gets or sets the current configuration level.
        /// </summary>
        [ACPropertyCurrent(407, "ProjectItem")]
        public ACClassInfoWithItems CurrentConfigLevel
        {
            get
            {
                return _CurrentConfigLevel;
            }
            set
            {
                _CurrentConfigLevel = value;
                OnPropertyChanged("CurrentConfigLevel");
            }

        }

        /// <summary>
        /// Gets or sets the text for search in assign dialog.
        /// </summary>
        [ACPropertyInfo(408,"", "en{'Search text'}de{'Text suchen'}")]
        public string SearchTextDialog
        {
            get;
            set;
        }

        private ACClassInfoWithItems _CurrentConfigTargetRoot;
        /// <summary>
        /// Gets the root item of a current configuration target(alarm messenger).
        /// </summary>
        [ACPropertyCurrent(409,"")]
        public ACClassInfoWithItems CurrentConfigTargetRoot
        {
            get
            {
                return _CurrentConfigTargetRoot;
            }
        }

        private ACClassInfoWithItems _CurrentConfigTarget;
        /// <summary>
        /// Gets or sets the current configuration target(alarm messenger).
        /// </summary>
        [ACPropertyCurrent(410, "")]
        public ACClassInfoWithItems CurrentConfigTarget
        {
            get
            {
                return _CurrentConfigTarget;
            }
            set
            {
                _CurrentConfigTarget = value;
                OnPropertyChanged("CurrentConfigTarget");
            }
        }

        protected ACClassInfoWithItems.VisibilityFilters CLConfigLevelVisibilityFilter
        {
            get
            {
                return new ACClassInfoWithItems.VisibilityFilters()
                {
                    CustomFilter = string.IsNullOrEmpty(SearchTextDialog) ? null : new Func<ACClassInfoWithItems, bool>(c => c.ValueT != null &&  
                                                                                                                             c.ValueT.ACUrl.IndexOf(SearchTextDialog, StringComparison.CurrentCultureIgnoreCase) >= 0)
                };
            }
        }

        #endregion

        #region Properties => TargetComponents(IACAlarmReceiver)

        private ACClassInfoWithItems _CurrentConfigTargetCompRoot;
        /// <summary>
        /// Gets the root item of configuration levels.
        /// </summary>
        [ACPropertyCurrent(406, "ProjectItemRoot")]
        public ACClassInfoWithItems CurrentConfigTargetCompRoot
        {
            get
            {
                return _CurrentConfigTargetCompRoot;
            }
        }

        private ACClassInfoWithItems _CurrentConfigTargetComp;
        /// <summary>
        /// Gets or sets the current configuration level.
        /// </summary>
        [ACPropertyCurrent(407, "ProjectItem")]
        public ACClassInfoWithItems CurrentConfigTargetComp
        {
            get
            {
                return _CurrentConfigTargetComp;
            }
            set
            {
                _CurrentConfigTargetComp = value;
                OnPropertyChanged();
            }

        }

        

        #endregion

        private ACClass _CurrentACClass;
        /// <summary>
        /// Gets or sets the ACClass of selected component.
        /// </summary>
        [ACPropertyInfo(411)]
        public ACClass CurrentACClass
        {
            get
            {
                return _CurrentACClass;
            }
            set
            {
                _CurrentACClass = value;
                OnPropertyChanged("CurrentACClass");
            }
        }

        private ACClass _AlarmMessengerBase;
        private ACClass AlarmMessengerBase
        {
            get
            {
                if (_AlarmMessengerBase == null)
                {
                    _AlarmMessengerBase = Db.ACClass.FirstOrDefault(c => c.ACIdentifier == PAAlarmMessengerBase.ClassName);
                }

                return _AlarmMessengerBase;
            }
        }

        private bool _IsVisibleConfigTarget = true;
        /// <summary>
        /// Gets or sets is configuration targets (alarm messengers) visible or not.
        /// </summary>
        [ACPropertyInfo(412)]
        public bool IsVisibleConfigTarget
        {
            get
            {
                return _IsVisibleConfigTarget;
            }
            set
            {
                _IsVisibleConfigTarget = value;
                OnPropertyChanged("IsVisibleConfigTarget");
            }
        }

        private bool _IsVisibleConfigLevel = false;
        /// <summary>
        /// Gets or sets is configuration levels (components) visible or not.
        /// </summary>
        [ACPropertyInfo(413)]
        public bool IsVisibleConfigLevel
        {
            get
            {
                return _IsVisibleConfigLevel;
            }
            set
            {
                _IsVisibleConfigLevel = value;
                OnPropertyChanged("IsVisibleConfigLevel");
            }
        }

        private bool _IsVisibleConfigTargetComp;
        [ACPropertyInfo(414)]
        public bool IsVisibleConfigTargetComp
        {
            get => _IsVisibleConfigTargetComp;
            set
            {
                _IsVisibleConfigTargetComp = value;
                OnPropertyChanged();
            }

        }

        #region Properties -> Config level

        public ACClass _SelectedAlarmMsgLevel;
        /// <summary>
        /// Gets or sets the level of alarm messages.
        /// </summary>
        [ACPropertySelected(414, "AlarmMsgLevel", "en{'Select the configuration level'}de{'Konfigurationsebene wählen'}")]
        public ACClass SelectedAlarmMsgLevel
        {
            get
            {
                return _SelectedAlarmMsgLevel;
            }
            set
            {
                _SelectedAlarmMsgLevel = value;
                OnPropertyChanged("SelectedAlarmMsgLevel");
            }
        }

        private List<ACClass> _AlarmMsgLevelList;
        /// <summary>
        /// Gets the list of a levels for alarm message/property.
        /// </summary>
        [ACPropertyList(415, "AlarmMsgLevel")]
        public List<ACClass> AlarmMsgLevelList
        {
            get
            {
                return _AlarmMsgLevelList;
            }
        }

        #endregion

        #region Properties -> Exclusion

        private AlarmMessengerConfig _SelectedExclusionRule;
        /// <summary>
        /// Gets or sets the selected exclusion rule.
        /// </summary>
        [ACPropertySelected(416,"ExclusionRule", "en{'Selected exclusion rule'}de{'Selected exclusion rule'}")]
        public AlarmMessengerConfig SelectedExclusionRule
        {
            get
            {
                return _SelectedExclusionRule;
            }
            set
            {
                _SelectedExclusionRule = value;
                OnPropertyChanged("SelectedExclusionRule");
            }
        }

        private List<AlarmMessengerConfig> _ExclusionRuleList;
        /// <summary>
        /// Gets the list of exclusion rules.
        /// </summary>
        [ACPropertyList(417, "ExclusionRule")]
        public IEnumerable<AlarmMessengerConfig> ExclusionRuleList
        {
            get
            {
                if (_ExclusionRuleList == null)
                {
                    if (SelectedAssignedAlarmSource != null)
                    {
                        ACClass sourceComponent = ACUrlCommand(SelectedAssignedAlarmSource.SourceACUrl) as ACClass;
                        if (sourceComponent == null)
                            return null;

                        List<ACClass> ConfigTargets = ConfigurationTargetList.Where(c => c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application).ToList();
                        IEnumerable<ACClassConfig> excludedRules = ConfigTargets.Where(x => x.ConfigurationEntries != null && x.ConfigurationEntries.Any())
                                                                                .SelectMany(x => x.ConfigurationEntries)
                                                                                .Where(k =>    k.LocalConfigACUrl != null 
                                                                                            && k.LocalConfigACUrl == PAAlarmMessengerBase.SaveExclusionConfigPropName 
                                                                                            && k.Value != null 
                                                                                            && k.Value.ToString() == SelectedAssignedAlarmSource.MsgPropACIdentifier)
                                                                                .Select(c => c as ACClassConfig);

                        if (!excludedRules.Any())
                            return null;

                        List<AlarmMessengerConfig> tempList = new List<AlarmMessengerConfig>();

                        foreach (ACClassConfig config in excludedRules)
                        {
                            ACClass acClass = ACUrlCommand(config.KeyACUrl) as ACClass;
                            if (acClass != null && acClass.IsDerivedClassFrom(sourceComponent))
                                tempList.Add(new AlarmMessengerConfig(config.KeyACUrl, config.Value.ToString(), acClass.ACUrlComponent, false)
                                                                      { MsgPropACCaption = GetAlarmSourceCaption(config.Value.ToString()), SourceACUrlComp = acClass.ACUrlComponent,
                                                                        ConfigTargetACUrl = config.ACClass.ACUrlComponent});
                        }
                        _ExclusionRuleList = tempList;
                    }
                }
                return _ExclusionRuleList;
            }
        }

        #endregion

        private ACPropertyConfigValue<string> _DefaultAlarmMessengerACUrl;
        /// <summary>
        /// Gets or sets the ACUrl of default alarm messenger.
        /// </summary>
        [ACPropertyConfig("en{'ACUrl of default alarm messenger'}de{'ACUrl des Standard-Alarm-Messengers'}")]
        public string DefaultAlarmMessengerACUrl
        {
            get
            {
                return _DefaultAlarmMessengerACUrl.ValueT;
            }
            set
            {
                _DefaultAlarmMessengerACUrl.ValueT = value;
            }
        }

        #endregion

        #region Methods

        private void RefreshRuleList()
        {
            if (CurrentACClass != null)
                RefreshRuleListFromComponent();
            else
                RefreshRuleListFromExplorer();
        }

        private void RefreshRuleListFromComponent()
        {
            List<VBEntityObject> resultList = new List<VBEntityObject>();

            if (ShowFromBaseClass)
            {
                if (ShowMessages)
                    resultList.AddRange(CurrentACClass.Messages);
                if (ShowProperties)
                    resultList.AddRange(CurrentACClass.Properties.Where(c => c.ValueTypeACClassID == _PaNotifyStateID));
            }
            else
            {
                if (ShowMessages)
                    resultList.AddRange(CurrentACClass.ACClassMessage_ACClass);
                if (ShowProperties)
                    resultList.AddRange(CurrentACClass.ACClassProperty_ACClass.Where(c => c.ValueTypeACClassID == _PaNotifyStateID && !c.IsStatic));
            }

            if (!string.IsNullOrEmpty(SearchTextMsgProp))
                resultList = resultList.Where(c => c.ACIdentifier.StartsWith(SearchTextMsgProp) || c.ACCaption.StartsWith(SearchTextMsgProp)).ToList();

            _AlarmSourceList = resultList;
            OnPropertyChanged("AlarmSourceList");
        }

        private void RefreshRuleListFromExplorer()
        {
            List<VBEntityObject> resultList = new List<VBEntityObject>();

            if (ShowMessages)
            {
                var messages = Db.ACClassMessage.ToArray();
                if (!ShowFromBSO)
                    messages = messages.Where(c => !_ACBSOType.IsAssignableFrom(c.ACClass.ObjectFullType)).ToArray();

                resultList.AddRange(messages);
            }
            if (ShowProperties)
            {
                var properties = Db.ACClassProperty.Where(c => c.ValueTypeACClassID == _PaNotifyStateID).ToArray();
                if (!ShowFromBSO)
                    properties = properties.Where(c => !_ACBSOType.IsAssignableFrom(c.ObjectFullType)).ToArray();

                resultList.AddRange(properties);
            }

            if (!string.IsNullOrEmpty(SearchTextMsgProp))
                resultList = resultList.Where(c => c.ACIdentifier.ToLower().Contains(SearchTextMsgProp.ToLower()) || c.ACCaption.ToLower().Contains(SearchTextMsgProp.ToLower())).ToList();

            _AlarmSourceList = resultList;
            OnPropertyChanged("AlarmSourceList");
        }

        private void RefreshAlarmMsgLevelList()
        {
            ACClass currentACClass = CurrentACClass;

            List<ACClass> resultList = new List<ACClass>();

            if (SelectedAlarmSource is ACClassMessage)
            {
                ACClassMessage acClassMsg = SelectedAlarmSource as ACClassMessage;
                ACClass msgClass = acClassMsg.ACClass;

                while (true)
                {
                    if (currentACClass == null)
                        break;
                    resultList.Add(currentACClass);
                    if (currentACClass.ACClassID == msgClass.ACClassID)
                        break;
                    currentACClass = currentACClass.ACClass1_BasedOnACClass;
                }
            }
            else if (SelectedAlarmSource is ACClassProperty)
            {
                ACClassProperty acClassProp = SelectedAlarmSource as ACClassProperty;
                ACClass propClass = acClassProp.ACClass;

                while (true)
                {
                    if (currentACClass == null)
                        break;
                    resultList.Add(currentACClass);
                    if (currentACClass.ACClassID == propClass.ACClassID)
                        break;
                    currentACClass = currentACClass.ACClass1_BasedOnACClass;
                }
            }

            _AlarmMsgLevelList = resultList;
            OnPropertyChanged("AlarmMsgLevelList");
        }

        /// <summary>
        /// Searches the properties or messages according search text.
        /// </summary>
        [ACMethodInfo("", "en{'Search'}de{'Suche'}", 401)]
        public void SearchMsgProp()
        {
            RefreshRuleList();
        }

        /// <summary>
        /// Assigns the message or property.
        /// </summary>
        [ACMethodInfo("", "en{'Assign message or property'}de{'Nachricht oder Eigenschaft zuordnen'}", 402)]
        public void AssignMsgProp()
        {
            bool ctalLoaded = false;
            if (CurrentACClass != null)
            {
                if (this._controlDialog == null || this._controlDialog.CurrentSelection == null || !(this._controlDialog.CurrentSelection is IACComponent))
                    return;
                ctalLoaded = LoadConfigTargetAndLevel(this._controlDialog.CurrentSelection as IACComponent, CurrentACClass, AlarmMsgLevelList.ToArray());
            }
            if (!ctalLoaded)
            {
                SearchTextDialog = "";
                ACClass currentACClass = SelectedAlarmSource is ACClassMessage ? ((ACClassMessage)SelectedAlarmSource).ACClass : ((ACClassProperty)SelectedAlarmSource).ACClass;

                if (currentACClass != null)
                {
                    if (DefaultAlarmMessenger != null)
                    {
                        CurrentConfigLevel = DefaultAlarmMessenger;
                        IsVisibleConfigTarget = false;
                        IsVisibleConfigLevel = true;
                        IsVisibleConfigTargetComp = false;
                    }
                    else
                    {
                        CurrentConfigTarget = new ACClassInfoWithItems() { ValueT = AlarmMessengerBase };
                        IsVisibleConfigTarget = true;
                        IsVisibleConfigLevel = false;
                        IsVisibleConfigTargetComp = false;
                        _CurrentConfigTargetRoot = CurrentConfigTarget;
                        CreateHierarchyTree(CurrentConfigTarget, null);
                    }

                    CurrentConfigLevel = new ACClassInfoWithItems() { ValueT = currentACClass };
                    _CurrentConfigLevelRoot = CurrentConfigLevel;
                    CreateHierarchyTree(CurrentConfigLevel, nameof(ACClassInfoWithItems.IsChecked));
                    _RootCache = CurrentConfigLevel;


                    LoadTargetComponents();
                }
                else
                {
                    Messages.Info(this, "Can't find a source ACClass.", true);
                    return;
                }
            }
            ShowDialog(this, "ExplorerSelectLevelDialog");
        }

        /// <summary>
        /// Determines is enabled AssignMsgProp or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledAssignMsgProp()
        {
            return SelectedAlarmSource == null ? false : true;
        }

        /// <summary>
        /// Removes assigned message or property.
        /// </summary>
        [ACMethodInfo("", "en{'Remove message or property'}de{'Nachricht oder Eigenschaft entfernen'}", 403)]
        public void RemoveAssignedMsgProp()
        {
            if (SelectedAssignedAlarmSource.HasExclusionRules)
            {
                foreach(var exclusionRule in ExclusionRuleList.ToList())
                {
                    SelectedExclusionRule = exclusionRule;
                    RemoveExclusionRule();
                }
            }
            AssignedAlarmSourceList.Remove(SelectedAssignedAlarmSource);
        }

        /// <summary>
        /// Determines is enabled RemoveAssignedMsgProp or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledRemoveAssignedMsgProp()
        {
            return SelectedAssignedAlarmSource == null ? false : true;
        }

        private string GetAlarmSourceCaption(string msgPropACIdentifier)
        {
            VBEntityObject msgProp = null;

            if (_AlarmSourceList != null)
                msgProp = _AlarmSourceList.FirstOrDefault(c => c.ACIdentifier == msgPropACIdentifier);
            if (msgProp == null)
                msgProp = Db.ACClassMessage.FirstOrDefault(c => c.ACIdentifier == msgPropACIdentifier);
            if (msgProp == null)
                msgProp = Db.ACClassProperty.FirstOrDefault(c => c.ACIdentifier == msgPropACIdentifier);

            if (msgProp != null)
                return msgProp.ACCaption;
            return "";
        }

        private void FindConfigurationTargets(ACClass aCClass, List<ACClass> resultList, bool onlyFromAppProj = false)
        {
            foreach (var item in aCClass.ACClass_BasedOnACClass)
            {
                if (onlyFromAppProj)
                {
                    if (item.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application)
                        resultList.Add(item);
                }
                else
                {
                    resultList.Add(item);
                }
                FindConfigurationTargets(item, resultList);
            }
        }

        private IEnumerable<ACClass> FindBasedOnConfigurationTargets(ACClass aCClass)
        {
            List<ACClass> configTargetList = new List<ACClass>();
            while (true)
            {
                configTargetList.Add(aCClass);
                if (aCClass == null || aCClass.ACIdentifier == PAAlarmMessengerBase.ClassName)
                    break;
                aCClass = aCClass.ACClass1_BasedOnACClass;
            }
            return configTargetList;
        }

        private void CreateHierarchyTree(ACClassInfoWithItems info, string dataContextCheckBox)
        {
            foreach (var item in info.ValueT.ACClass_BasedOnACClass)
            {
                ACClassInfoWithItems infoItem = new ACClassInfoWithItems() { ValueT = item, DataContentCheckBox = dataContextCheckBox };
                info.Add(infoItem);
                CreateHierarchyTree(infoItem, dataContextCheckBox);
            }
        }

        /// <summary>
        /// Runs search over a hierarchy tree(configuration levels).
        /// </summary>
        [ACMethodInfo("", "en{'Search'}de{'Suchen'}", 404)]
        public void SearchHierarchyTree()
        {
            _RootCache.Filter = null;
            _RootCache.Filter = CLConfigLevelVisibilityFilter;
            var temp = new ACClassInfoWithItems() { ValueT = _RootCache.ValueT };
            BuildTree(temp, _RootCache);
            _CurrentConfigLevelRoot = temp;
            OnPropertyChanged("CurrentConfigLevelRoot");
        }

        private void BuildTree(ACClassInfoWithItems infoWithItems, ACClassInfoWithItems original)
        {
            foreach(var item in original.Items)
            {
                if ((item as ACClassInfoWithItems).SearchClassChildCount > 0)
                {
                    var newItem = new ACClassInfoWithItems() { ValueT = (item as ACClassInfoWithItems).ValueT };
                    infoWithItems.Add(newItem);
                    BuildTree(newItem, item as ACClassInfoWithItems);
                }
            }
        }

        /// <summary>
        /// Confirms a configuration target selection.
        /// </summary>
        [ACMethodInfo("", "en{'Select configuration target'}de{'Konfigurationsziel auswählen'}", 405)]
        public void SelectAlarmConfigTarget()
        {
            IsVisibleConfigTarget = false;
            IsVisibleConfigLevel = true;
            IsVisibleConfigTargetComp = false;
        }

        /// <summary>
        /// Determines is enabled select alarm configuration target or not.
        /// </summary>
        /// <returns>Returns true if is enabled select alarm configuration target, otherwise false.</returns>
        public bool IsEnabledSelectAlarmConfigTarget()
        {
            return CurrentConfigTarget != null;
        }
        [ACMethodInfo("", "en{'Select target components'}de{'Ziel-komponenten auswählen'}", 405)]
        public void SelectAlarmConfigTargetComponent()
        {
            IsVisibleConfigTarget = false;
            IsVisibleConfigLevel = false;
            IsVisibleConfigTargetComp = true;
        }

        public bool IsEnabledSelectAlarmConfigTargetComponent()
        {
            return true;
        }

        /// <summary>
        /// Assigns message or property in a dialog.
        /// </summary>
        [ACMethodInfo("", "en{'Assign message or property'}de{'Nachricht oder Eigenschaft zuordnen'}", 406)]
        public void AssignMsgPropDialog()
        {
            if (DefaultAlarmMessenger != null)
                CurrentConfigTarget = DefaultAlarmMessenger;

            if (CurrentConfigTarget != null /* && ValidateAssignMessageOrProperty(CurrentConfigTarget, CurrentConfigLevel, SelectedAlarmSource.ACIdentifier)*/)
            {
                List<ACClass> sourceComponents = GetCheckedComponents(CurrentConfigLevelRoot);
                if (sourceComponents == null || !sourceComponents.Any())
                {
                    //TODO message
                    Messages.Error(this, "Please select/check at least one source component for alarm!");
                    return;
                }

                foreach (ACClass sourceComp in sourceComponents)
                {
                    List<ACClass> targetComponents = GetCheckedComponents(CurrentConfigTargetCompRoot);

                    if (targetComponents != null && targetComponents.Any())
                    {
                        foreach (ACClass targetComp in targetComponents)
                        {
                            AlarmMessengerConfig config = new AlarmMessengerConfig(sourceComp.ACUrl, SelectedAlarmSource.ACIdentifier, sourceComp.ACUrlComponent, false)
                            {
                                MsgPropACCaption = SelectedAlarmSource.ACCaption,
                                ConfigTargetACUrl = CurrentConfigTarget.ValueT.ACUrlComponent,
                                TargetACUrlComp = targetComp.ACUrlComponent
                            };
                            AssignedAlarmSourceList.Add(config);
                        }
                    }
                    else
                    {

                        AlarmMessengerConfig config = new AlarmMessengerConfig(sourceComp.ACUrl, SelectedAlarmSource.ACIdentifier, sourceComp.ACUrlComponent, false)
                        { MsgPropACCaption = SelectedAlarmSource.ACCaption, ConfigTargetACUrl = CurrentConfigTarget.ValueT.ACUrlComponent };
                        AssignedAlarmSourceList.Add(config);
                    }
                }
            }

            CurrentConfigTarget = null;
            CloseTopDialog();
        }

        /// <summary>
        /// Determines is enabled assign message or property in dialog or not.
        /// </summary>
        /// <returns>Returns true if is enabled assign message or property in dialog, otherwise false.</returns>
        public bool IsEnabledAssignMsgPropDialog()
        {
            return CurrentConfigLevel != null || SelectedAlarmMsgLevel != null;
        }

        /// <summary>
        /// Saves the alarm distribution configuration.
        /// </summary>
        [ACMethodInfo("", "en{'Save configuration'}de{'Konfiguration speichern'}", 407)]
        public void SaveConfiguration()
        {
            bool isRebuildNeeded = false;

            foreach (ACClass configTarget in ConfigurationTargetList)
            {
                var currentConfigs = configTarget.ConfigurationEntries.Where(c =>  c.LocalConfigACUrl != null 
                                                                                && c.LocalConfigACUrl == PAAlarmMessengerBase.SaveConfigPropName).ToList();

                var preparedList = AssignedAlarmSourceList.Where(x => x.ConfigTargetACUrl == configTarget.ACUrlComponent)
                                                          .Select(c => new Tuple<string, string, string>(c.MsgPropACIdentifier, c.SourceACUrl, c.TargetACUrlComp));
                var preparedListConfig = currentConfigs.Select(c => new Tuple<string, string, string>(c.Value.ToString(), c.KeyACUrl, c.Expression));

                var newConfigs = preparedList.Except(preparedListConfig);
                var delConfigs = preparedListConfig.Except(preparedList);

                //if (!newConfigs.Any() && !delConfigs.Any())
                //    continue;

                //configTarget.ACConfigListCache_Initialize();

                foreach (var item in delConfigs)
                {
                    configTarget.RemoveACConfig(currentConfigs.FirstOrDefault(c => c.KeyACUrl == item.Item2 && c.Value.ToString() == item.Item1));
                }

                foreach (var item in newConfigs)
                {
                    var config = configTarget.NewACConfig(null, configTarget.Database.GetACType(typeof(string)));
                    config.LocalConfigACUrl = PAAlarmMessengerBase.SaveConfigPropName;
                    config.KeyACUrl = item.Item2;
                    config.Value = item.Item1;
                    config.Expression = item.Item3;
                }

                Msg msg = configTarget.Database.ACSaveChanges();
                if (msg != null)
                    Messages.Msg(msg);
                else
                    isRebuildNeeded = true;
            }

            if (isRebuildNeeded)
            {
                RebuildAlarmMessengerCache();
                _InitialAssignedAlarmSourceList = AssignedAlarmSourceList.ToList();
            }
        }

        public bool IsEnabledSaveConfiguration()
        {
            return _InitialAssignedAlarmSourceList != null && 
                  (AssignedAlarmSourceList.Except(_InitialAssignedAlarmSourceList).Any() || _InitialAssignedAlarmSourceList.Except(AssignedAlarmSourceList).Any());
        }

        private bool ResolveSource(Msg alarmMsg, out ACClass sourceACClass, out IACComponent sourceComp)
        {
            sourceACClass = null;
            sourceComp = null;
            if (alarmMsg.SourceComponent != null && alarmMsg.SourceComponent.ValueT != null)
                sourceComp = alarmMsg.SourceComponent.ValueT;
            else
                sourceComp = ACUrlCommand(alarmMsg.Source) as IACComponent;
            return ResolveSourceClass(sourceComp, out sourceACClass);
        }

        private bool ResolveSourceClass(IACComponent sourceComp, out ACClass sourceACClass)
        {
            sourceACClass = null;
            if (sourceComp != null)
            {
                if (sourceComp is IACComponentPWNode)
                {
                    if ((sourceComp as IACComponentPWNode).ContentACClassWF != null)
                        sourceACClass = (sourceComp as IACComponentPWNode).ContentACClassWF.PWACClass;
                }
                if (sourceACClass == null)
                    sourceACClass = sourceComp.ComponentClass;
            }
            if (sourceComp == null || sourceACClass == null)
            {
                Messages.Info(this, "Can't find a source component for the alarm message!", true);
                return false;
            }
            return true;
        }

        private bool ValidateAssignMessageOrProperty(ACClassInfoWithItems configTarget, ACClassInfoWithItems configLevel, string alarmSourceID)
        {
            IEnumerable<AlarmMessengerConfig> relevantConfigs = AssignedAlarmSourceList.Where(c => c.MsgPropACIdentifier == alarmSourceID);

            if (!relevantConfigs.Any())
                return true;

            AlarmMessengerConfig existConfig = relevantConfigs.FirstOrDefault(c => c.SourceACUrl == configLevel.ValueT.ACUrl && c.ConfigTargetACUrl == configTarget.ValueT.ACUrlComponent);
            if (existConfig != null)
            {
                Messages.Warning(this, "Warning50031", false, alarmSourceID, existConfig.SourceACUrlComp, existConfig.ConfigTargetACUrlComp);
                return false;
            }

            foreach(AlarmMessengerConfig config in relevantConfigs)
            {
                if (CheckBasedOnConfigTarget(configTarget.ValueT, config.ConfigTargetACUrl))
                {
                    if (CheckBasedOnConfigLevel(configLevel.ValueT, config.SourceACUrl))
                    {
                        if (Messages.Question(this, "Question50039", Global.MsgResult.No, false, alarmSourceID, config.SourceACUrlComp, config.ConfigTargetACUrlComp) ==
                            Global.MsgResult.Yes)
                            return true;
                        else
                            return false;
                    }
                }
            }

            return true;
        }

        private bool CheckBasedOnConfigTarget(ACClass currentConfigTarget, string configTargetACUrlComp)
        {
            while(true)
            {
                if (currentConfigTarget.ACUrlComponent == configTargetACUrlComp)
                    return true;

                if (currentConfigTarget.ACIdentifier == PAAlarmMessengerBase.ClassName)
                    return false;

                currentConfigTarget = currentConfigTarget.ACClass1_BasedOnACClass;
            }
        }

        private bool CheckBasedOnConfigLevel(ACClass currentConfigLevel, string configLevelACUrlComp)
        {
            while(true)
            {
                if (currentConfigLevel == null)
                    return false;

                if (currentConfigLevel.ACUrl == configLevelACUrlComp)
                    return true;

                currentConfigLevel = currentConfigLevel.ACClass1_BasedOnACClass;
            }
        }

        private ACClass FindAlarmMessenger(IACComponent sourceComp, ACClass sourceACClass)
        {
            IAppManager appManager = sourceComp.FindParentComponent<IAppManager>(c => c is IAppManager);
            if (appManager == null)
                return null;
            ACClass appMClass = appManager.ComponentClass;
            if (appMClass == null)
                return null;
            appMClass = appMClass.ACClass_ParentACClass.FirstOrDefault(c => _alarmMessengerBase.IsAssignableFrom(c.ObjectFullType));
            return appMClass;
        }

        private void ControlDialog_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentSelection" && _controlDialog.CurrentSelection != null)
            {
                ACClass sourceClass = null;
                bool resolved = ResolveSourceClass(_controlDialog.CurrentSelection as IACComponent, out sourceClass);
                CurrentACClass = sourceClass;

                RefreshRuleListFromComponent();

                if (resolved)
                {
                    var alarmMessenger = FindAlarmMessenger(_controlDialog.CurrentSelection as IACComponent, CurrentACClass);
                    _ConfigurationTargetList = FindBasedOnConfigurationTargets(alarmMessenger);
                }
            }
        }

        private bool CheckExclusionRules(string msgPropID, ACClass sourceComponent)
        {
            if (string.IsNullOrEmpty(msgPropID) || sourceComponent == null)
                return false;

            List<ACClass> ConfigTargets = ConfigurationTargetList.Where(c => c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application).ToList();
            IEnumerable<ACClassConfig> excludedRules = ConfigTargets.Where(x => x.ConfigurationEntries != null && x.ConfigurationEntries.Any())
                                                                              .SelectMany(x => x.ConfigurationEntries)
                                                                              .Where(k => k.LocalConfigACUrl != null 
                                                                                          && k.LocalConfigACUrl == PAAlarmMessengerBase.SaveExclusionConfigPropName 
                                                                                          && k.Value != null 
                                                                                          && k.Value.ToString() == msgPropID)
                                                                              .Select(c => c as ACClassConfig);

            if (excludedRules == null || !excludedRules.Any())
                return false;

            foreach(ACClassConfig config in excludedRules)
            {
                ACClass acClass = ACUrlCommand(config.KeyACUrl) as ACClass;
                if (acClass != null && acClass.IsDerivedClassFrom(sourceComponent))
                    return true;
            }

            return false;
        }

        private List<ACClass> GetCheckedComponents(ACClassInfoWithItems root)
        {
            if (CurrentConfigTargetComp == null)
                return null;

            List<ACClass> result = new List<ACClass>();

            GetCheckedComponentsRecursive(root, result);

            return result;
        }

        private void GetCheckedComponentsRecursive(ACClassInfoWithItems item, List<ACClass> resultList)
        {
            if (item == null)
                return;

            if (item.IsChecked)
                resultList.Add(item.ValueT);

            foreach (var childItem in item.ItemsT)
            {
                GetCheckedComponentsRecursive(childItem, resultList);
            }
        }

        /// <summary>
        /// Saves changes.
        /// </summary>
        [ACMethodCommand("", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            SaveConfiguration();
        }

        public bool IsEnabledSave()
        {
            return AssignedAlarmSourceList.Except(_InitialAssignedAlarmSourceList).Any() || _InitialAssignedAlarmSourceList.Except(AssignedAlarmSourceList).Any();
        }

        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return base.OnGetControlModes(vbControl);
            Global.ControlModes result = base.OnGetControlModes(vbControl);
            if (result < Global.ControlModes.Enabled)
                return result;
            switch (vbControl.VBContent)
            {
                case "ShowFromBaseClass":
                    if (CurrentACClass != null)
                        result = Global.ControlModes.Enabled;
                    else
                        result = Global.ControlModes.Hidden;
                    break;
                case "ShowFromBSO":
                    if (CurrentACClass == null)
                        result = Global.ControlModes.Enabled;
                    else
                        result = Global.ControlModes.Hidden;
                    break;
                case "IsVisibleConfigTarget":
                    if (IsVisibleConfigTarget)
                        result = Global.ControlModes.Enabled;
                    else
                        result = Global.ControlModes.Collapsed;
                    break;
                case "IsVisibleConfigLevel":
                    if (IsVisibleConfigLevel)
                        result = Global.ControlModes.Enabled;
                    else
                        result = Global.ControlModes.Collapsed;
                    break;
                case nameof(IsVisibleConfigTargetComp):
                    if (IsVisibleConfigTargetComp)
                        result = Global.ControlModes.Enabled;
                    else
                        result = Global.ControlModes.Collapsed;
                    break;

            }   
            return result;
        }

        private bool LoadConfigTargetAndLevel(IACComponent sourceComp, ACClass sourceACClass, ACClass[] configLevel)
        {
            if (sourceComp == null || sourceACClass == null)
                return false;

            if (DefaultAlarmMessenger == null)
            {
                IsVisibleConfigTarget = true;
                IsVisibleConfigLevel = false;

                ACClass currentAlarmMessenger = FindAlarmMessenger(sourceComp, sourceACClass);
                if (currentAlarmMessenger == null)
                    return false;
                var configTargets = FindBasedOnConfigurationTargets(currentAlarmMessenger).ToArray();
                configTargets.Reverse();

                CurrentConfigTarget = new ACClassInfoWithItems() { ValueT = configTargets[0] };
                ACClassInfoWithItems parentTargetInfo = CurrentConfigTarget;
                foreach (var item in configTargets)
                {
                    if (item.ACClassID == configTargets[0].ACClassID)
                        continue;
                    ACClassInfoWithItems info = new ACClassInfoWithItems() { ValueT = item };
                    parentTargetInfo.Add(info);
                    parentTargetInfo = info;
                }
                _CurrentConfigTargetRoot = CurrentConfigTarget;

            }
            else
            {
                IsVisibleConfigTarget = false;
                IsVisibleConfigLevel = true;
                CurrentConfigTarget = DefaultAlarmMessenger;
            }

            configLevel.Reverse();
            CurrentConfigLevel = new ACClassInfoWithItems() { ValueT = configLevel[0] };
            ACClassInfoWithItems parentInfo = CurrentConfigLevel;

            foreach (var item in configLevel)
            {
                if (item.ACClassID == configLevel[0].ACClassID)
                    continue;
                ACClassInfoWithItems info = new ACClassInfoWithItems() { ValueT = item };
                parentInfo.Add(info);
                parentInfo = info;
            }
            _CurrentConfigLevelRoot = CurrentConfigLevel;
            _RootCache = CurrentConfigLevel;
            return true;
        }

        private void LoadTargetComponents()
        {
            var componentTargets = Database.ContextIPlus.ACClass.Where(c => c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application)
                                                                        .ToArray()
                                                                        .Where(c => typeof(IACAttachedAlarmHandler).IsAssignableFrom(c.ObjectType));

            //var compTargets = componentTargets.Where(c => !componentTargets.Any(x => x.ACClassID == c.BasedOnACClassID)).ToArray();

            CurrentConfigTargetComp = new ACClassInfoWithItems("Root");

            foreach (var compTarget in componentTargets)
            {
                var temp = new ACClassInfoWithItems(compTarget) { DataContentCheckBox = nameof(ACClassInfoWithItems.IsChecked) };
                CurrentConfigTargetComp.Add(temp);
            }

            _CurrentConfigTargetCompRoot = CurrentConfigTargetComp;
        }

        #region Methods -> Explorer dialog

        /// <summary>
        /// Sets the alarm distribution configuration from BSOAlarmExplorer.
        /// </summary>
        /// <param name="alarmMsg">The alarmMsg parameter.</param>
        public void SetAlarmToDistribution(Msg alarmMsg)
        {
            _CurrentDialogMsg = alarmMsg;

            ACClass sourceACClass = null;
            IACComponent sourceComp = null;
            if (!ResolveSource(alarmMsg, out sourceACClass, out sourceComp))
                return;

            ACClass tempClass = sourceACClass;
            List<ACClass> msgPropSourceList = new List<ACClass>();
            msgPropSourceList.Add(tempClass);
            while (true)
            {
                if (tempClass.ACClassMessage_ACClass.Any(c => c.ACIdentifier == alarmMsg.TranslID)
                    || tempClass.ACClassProperty_ACClass.Any(x => x.ACIdentifier == alarmMsg.ACIdentifier && !x.IsStatic))
                    break;
                tempClass = tempClass.ACClass1_BasedOnACClass;
                if (tempClass == null)
                    break;
                msgPropSourceList.Add(tempClass);
            }

            LoadConfigTargetAndLevel(sourceComp, sourceACClass, msgPropSourceList.ToArray());

            LoadTargetComponents();

            ShowDialog(this, "AlarmExplorerDialog");
        }

        /// <summary>
        /// Unsets the alarm distribution configuration from BSOAlarmExplorer.
        /// </summary>
        /// <param name="alarmMsg"></param>
        public void UnsetAlarmFromDistribution(Msg alarmMsg)
        {
            ACClass sourceACClass = null;
            IACComponent sourceComp = null;
            if (!ResolveSource(alarmMsg, out sourceACClass, out sourceComp))
                return;

            bool _IsAnyConfigRemoved = false;
            ACClass alarmMessenger = FindAlarmMessenger(sourceComp, sourceACClass);
            string msgID = !string.IsNullOrEmpty(alarmMsg.TranslID) ? alarmMsg.TranslID : alarmMsg.ACIdentifier;
            if (sourceACClass != null && alarmMessenger != null)
            {
                while (true)
                {
                    IACConfig currentConfig = null;
                    ACClass configuredACClass = null;

                    if (alarmMsg.ConfigIconState == Global.ConfigIconState.Config)
                        currentConfig = alarmMessenger.ConfigurationEntries.Where(c => c.LocalConfigACUrl != null 
                                                                                    && c.LocalConfigACUrl == PAAlarmMessengerBase.SaveConfigPropName)
                                                                           .ToList()
                                                                            .FirstOrDefault(c =>   c.Value != null 
                                                                                                && c.Value.ToString() == msgID
                                                                                                && c.KeyACUrl != null
                                                                                                && c.KeyACUrl == sourceACClass.ACUrl);
                    else
                    {
                        currentConfig = alarmMessenger.ConfigurationEntries.Where(c => c.LocalConfigACUrl != null 
                                                                                    && c.LocalConfigACUrl == PAAlarmMessengerBase.SaveConfigPropName)
                                                                            .ToList()
                                                                            .FirstOrDefault(c =>   c.Value != null 
                                                                                                && c.Value.ToString() == msgID);

                        if (currentConfig != null)
                        {
                            configuredACClass = ACUrlCommand(currentConfig.KeyACUrl, null) as ACClass;
                            if (configuredACClass != null)
                            {
                                if (!sourceACClass.IsDerivedClassFrom(configuredACClass))
                                    currentConfig = null;
                            }
                        }
                    }

                    if (currentConfig == null)
                    {
                        alarmMessenger = alarmMessenger.ACClass1_BasedOnACClass;
                        if (alarmMessenger == null)
                            break;
                    }
                    else
                    {
                        //alarmMessenger.ACConfigListCache_Initialize();
                        alarmMessenger.RemoveACConfig(currentConfig);
                        var msg = alarmMessenger.Database.ACSaveChanges();
                        if (msg != null)
                            Messages.Msg(msg);
                        else 
                        {
                            _IsAnyConfigRemoved = true;
                            if (configuredACClass != null)
                                CheckAndRemoveExclusionRules(configuredACClass, msgID);
                        }

                    }
                }
                RebuildAlarmMessengerCache();
                if (!_IsAnyConfigRemoved)
                    Messages.Warning(this, "Warning50032");
            }
        }

        private void CheckAndRemoveExclusionRules(ACClass ruleSourceClass, string alarmMsgID)
        {
            foreach(var configTarget in ConfigurationTargetList)
            {
                //configTarget.ACClassConfig_ACClass.AutoRefresh();
                //configTarget.ACClassConfig_ACClass.AutoLoad();
                //configTarget.ClearCacheOfConfigurationEntries();

                var exclusionRules = configTarget.ConfigurationEntries.Where(c =>    c.LocalConfigACUrl != null 
                                                                                  && c.LocalConfigACUrl == PAAlarmMessengerBase.SaveExclusionConfigPropName 
                                                                                  && c.Value != null 
                                                                                  && c.Value.ToString() == alarmMsgID)
                                                                      .ToList();

                foreach(var exclusionRule in exclusionRules)
                {
                    ACClass aCClass = ACUrlCommand(exclusionRule.KeyACUrl) as ACClass;
                    if (aCClass != null && aCClass.IsDerivedClassFrom(ruleSourceClass))
                    {
                        //configTarget.ACConfigListCache_Initialize();
                        configTarget.RemoveACConfig(exclusionRule);
                        var msg = configTarget.Database.ACSaveChanges();
                        if (msg != null)
                            Messages.Msg(msg);
                    }
                }
            }

        }

        /// <summary>
        /// Saves configuration from alarm explorer dialog.
        /// </summary>
        [ACMethodInfo("", "en{'Save configuration'}de{'Konfiguration speichern'}", 410, true)]
        public void SaveConfigurationDialog()
        {
            string alarmSourceID = !string.IsNullOrEmpty(_CurrentDialogMsg.TranslID) ? _CurrentDialogMsg.TranslID : _CurrentDialogMsg.ACIdentifier;
            _AssignedAlarmSourceList = null;
            if (ValidateAssignMessageOrProperty(CurrentConfigTarget, CurrentConfigLevel, alarmSourceID))
            {
                var targetComponents = GetCheckedComponents(CurrentConfigTargetComp);
                if (targetComponents != null && targetComponents.Any())
                {
                    foreach (var targetComp in targetComponents)
                    {
                        IACConfig config = CurrentConfigTarget.ValueT.NewACConfig(null, CurrentConfigTarget.ValueT.Database.GetACType(typeof(string)));
                        config.LocalConfigACUrl = PAAlarmMessengerBase.SaveConfigPropName;
                        config.KeyACUrl = CurrentConfigLevel.ValueT.ACUrl;
                        config.Value = alarmSourceID;
                        config.Expression = targetComp.ACUrlComponent;
                    }
                }
                else
                {
                    IACConfig config = CurrentConfigTarget.ValueT.NewACConfig(null, CurrentConfigTarget.ValueT.Database.GetACType(typeof(string)));
                    config.LocalConfigACUrl = PAAlarmMessengerBase.SaveConfigPropName;
                    config.KeyACUrl = CurrentConfigLevel.ValueT.ACUrl;
                    config.Value = alarmSourceID;
                }
                Msg msg = CurrentConfigTarget.ValueT.Database.ACSaveChanges();
                if (msg == null)
                    RebuildAlarmMessengerCache();
                else
                {
                    Messages.Msg(msg);
                }
            }
            CloseTopDialog();
        }

        /// <summary>
        /// Determines is enabled SaveConfigurationDialog or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledSaveConfigurationDialog()
        {
            return CurrentConfigLevel != null && (CurrentConfigTarget != null || DefaultAlarmMessenger != null);
        }

        /// <summary>
        /// Closes the configuration dialog.
        /// </summary>
        [ACMethodInfo("", "en{'Cancel'}de{'Abbrechen'}", 411, true)]
        public void CloseConfigurationDialog()
        {
            CloseTopDialog();
        }

        /// <summary>
        /// Sets the exclusion rule on the source component in the message.
        /// </summary>
        /// <param name="alarmMsg">The alarm message.</param>
        public void SetExclusionRule(Msg alarmMsg)
        {
            ACClass sourceACClass = null;
            IACComponent sourceComp = null;
            if (!ResolveSource(alarmMsg, out sourceACClass, out sourceComp))
                return;

            ACClass messenger = FindAlarmMessenger(sourceComp, sourceACClass);
            if (messenger == null)
            {
                Messages.Error(this, "Error: Exclusion rule isn't setted. Can't find a alarm messenger ACClass!", true);
                return;
            }

            //messenger.ACClassConfig_ACClass.AutoLoad();
            //messenger.ACClassConfig_ACClass.AutoRefresh();
            //messenger.ClearCacheOfConfigurationEntries();

            string alarmMsgID = !string.IsNullOrEmpty(alarmMsg.TranslID) ? alarmMsg.TranslID : alarmMsg.ACIdentifier;
            if (messenger.ConfigurationEntries.Any(c => c.LocalConfigACUrl != null 
                                                    && c.LocalConfigACUrl == PAAlarmMessengerBase.SaveExclusionConfigPropName 
                                                    && c.KeyACUrl != null
                                                    && c.KeyACUrl == sourceACClass.ACUrl 
                                                    && c.Value != null
                                                    && c.Value.ToString() == alarmMsgID))
            {
                Messages.Warning(this, "Warning50033");
                return;
            }

            IACConfig config = messenger.NewACConfig(null, messenger.Database.GetACType(typeof(string)));
            config.LocalConfigACUrl = PAAlarmMessengerBase.SaveExclusionConfigPropName;
            config.KeyACUrl = sourceACClass.ACUrl;
            config.Value = alarmMsgID;

            Msg msg = messenger.Database.ACSaveChanges();
            if (msg == null)
                RebuildAlarmMessengerCache();
            else
                Messages.Msg(msg);
        }

        /// <summary>
        /// Removes the exclusion rule from the specific component.
        /// </summary>
        /// <param name="alarmMsg">The alarm message parameter.</param>
        public void RemoveExclusionRule(Msg alarmMsg)
        {
            ACClass sourceACClass = null;
            IACComponent sourceComp = null;
            if (!ResolveSource(alarmMsg, out sourceACClass, out sourceComp))
                return;

            ACClass messenger = FindAlarmMessenger(sourceComp, sourceACClass);
            if (messenger == null)
            {
                Messages.Error(this, "Error: Exclusion rule isn't removed. Can't find a alarm messenger ACClass!", true);
                return;
            }

            string alarmMsgID = !string.IsNullOrEmpty(alarmMsg.TranslID) ? alarmMsg.TranslID : alarmMsg.ACIdentifier;

            IACConfig config = messenger.ConfigurationEntries.Where(c => c.LocalConfigACUrl != null 
                                                                        && c.LocalConfigACUrl == PAAlarmMessengerBase.SaveExclusionConfigPropName).ToList()
                                                             .FirstOrDefault(x =>      x.Value != null 
                                                                                    && x.Value.ToString() == alarmMsgID 
                                                                                    && x.KeyACUrl == sourceACClass.ACUrl);

            if (config != null)
            {
                //messenger.ACConfigListCache_Initialize();
                messenger.RemoveACConfig(config);
                var msg = messenger.Database.ACSaveChanges();
                if (msg == null)
                    RebuildAlarmMessengerCache();
                else
                {
                    Messages.Msg(msg);
                }
            }
            else
            {
                Messages.Warning(this, "Warning50034");
            }
        }

        #endregion

        #region Methods -> Exclusion rule

        /// <summary>
        /// Removes a selected exclusion rule from the configuration.
        /// </summary>
        [ACMethodInfo("", "en{'Remove exclusion rule'}de{'Ausschlussregel entfernen'}", 413)]
        public void RemoveExclusionRule()
        {
            if (SelectedExclusionRule == null || SelectedExclusionRule.ConfigTargetACUrl == null)
                return;

            ACClass aCClass = ConfigurationTargetList.FirstOrDefault(c => c.ACUrlComponent == SelectedExclusionRule.ConfigTargetACUrl);
            if (aCClass == null)
                return;

            IACConfig config = aCClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl != null 
                                                                    && c.LocalConfigACUrl == PAAlarmMessengerBase.SaveExclusionConfigPropName).ToList()
                                                            .FirstOrDefault(x =>   x.KeyACUrl != null 
                                                                                && x.KeyACUrl == SelectedExclusionRule.SourceACUrl 
                                                                                && x.Value != null
                                                                                && x.Value.ToString() == SelectedExclusionRule.MsgPropACIdentifier);

            //aCClass.ACConfigListCache_Initialize();
            aCClass.RemoveACConfig(config);
            _ExclusionRuleList.Remove(SelectedExclusionRule);
            _ExclusionRuleList = _ExclusionRuleList.ToList();
            OnPropertyChanged("ExclusionRuleList");
            if (!_ExclusionRuleList.Any())
                SelectedAssignedAlarmSource.HasExclusionRules = false;
        }

        /// <summary>
        /// Determines is enabled RemoveTargetRule or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledRemoveExclusionRule()
        {
            return SelectedExclusionRule != null;  
        }

        #endregion  

        #endregion

        #region AlarmingMessenger

        private List<ACRef<ACComponent>> _AlarmMessengers;

        /// <summary>
        /// Gets the list of installed alarm messangers.
        /// </summary>
        public List<ACRef<ACComponent>> AlarmMessengers
        {
            get
            {
                return _AlarmMessengers;
            }
        }

        private void AttachToAlarmMessengers()
        {
            if (_AlarmMessengers != null)
                return;

            ACClass targetClass = Db.ACClass.FirstOrDefault(c => c.ACIdentifier == PAAlarmMessengerBase.ClassName);
            List<ACClass> messengers = new List<ACClass>();
            FindConfigurationTargets(targetClass, messengers, true);

            _AlarmMessengers = new List<ACRef<ACComponent>>();
            List<ACComponent> appManagers = this.Root.FindChildComponents<ACComponent>(c => c is ApplicationManagerProxy || c is ApplicationManager, null, 1);
            foreach (var appManager in appManagers)
            {
                Guid projectID = appManager.ComponentClass.ACProjectID;
                var appMessengers = messengers.Where(c => c.ACProjectID == projectID);

                foreach (var appMessenger in appMessengers)
                {
                    var alarmMessenger = appManager.ACUrlCommand("?"+appMessenger.ACIdentifier, null) as ACComponent;
                    if (alarmMessenger == null)
                        alarmMessenger = appManager.StartComponent(appMessenger.ACIdentifier, null, null, ACStartCompOptions.NoServerReqFromProxy | ACStartCompOptions.OnlyAutomatic) as ACComponent;

                    if (alarmMessenger != null)
                    {
                        ACRef<ACComponent> refToService = new ACRef<ACComponent>(alarmMessenger, this);
                        _AlarmMessengers.Add(refToService);
                    }
                }
            }
        }

        private void DetachAlarmMessengers()
        {
            if (_AlarmMessengers == null)
                return;
            _AlarmMessengers.ForEach(c => c.Detach());
            _AlarmMessengers = null;
        }

        private void RebuildAlarmMessengerCache()
        {
            if (_AlarmMessengers == null)
                AttachToAlarmMessengers();

            foreach (var messenger in AlarmMessengers)
                messenger.ValueT.ACUrlCommand("!RebuildConfigurationCache", null);
        }

        #endregion

        #region Execute helper handlers

        /// <summary>
        /// Handles a excution of ACMethods.
        /// </summary>
        /// <param name="result">The result parameter.</param>
        /// <param name="invocationMode">The inovation mode.</param>
        /// <param name="acMethodName">The name of ACMethod.</param>
        /// <param name="acClassMethod">The ACClassMethod parameter.</param>
        /// <param name="acParameter">The acParameter.</param>
        /// <returns></returns>
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "SearchMsgProp":
                    SearchMsgProp();
                    return true;
                case "AssignMsgProp":
                    AssignMsgProp();
                    return true;
                case "IsEnabledAssignMsgProp":
                    result = IsEnabledAssignMsgProp();
                    return true;
                case "RemoveAssignedMsgProp":
                    RemoveAssignedMsgProp();
                    return true;
                case "IsEnabledRemoveAssignedMsgProp":
                    result = IsEnabledRemoveAssignedMsgProp();
                    return true;
                case "SearchHierarchyTree":
                    SearchHierarchyTree();
                    return true;
                case "AssignMsgPropDialog":
                    AssignMsgPropDialog();
                    return true;
                case "IsEnabledAssignMsgPropDialog":
                    result = IsEnabledAssignMsgPropDialog();
                    return true;
                case "SaveConfiguration":
                    SaveConfiguration();
                    return true;
                case "IsEnabledSaveConfiguration":
                    result = IsEnabledSaveConfiguration();
                    return true;
                case "SaveConfigurationDialog":
                    SaveConfigurationDialog();
                    return true;
                case "IsEnabledSaveConfigurationDialog":
                    result = IsEnabledSaveConfigurationDialog();
                    return true;
                case "RemoveExclusionRule":
                    RemoveExclusionRule();
                    return true;
                case "IsEnabledRemoveTargetRule":
                    result = IsEnabledRemoveExclusionRule();
                    return true;
                case "CloseConfigurationDialog":
                    CloseConfigurationDialog();
                    return true;
                case "Save":
                    Save();
                    return true;
                case "IsEnabledSave":
                    result = IsEnabledSave();
                    return true;
                case "SelectAlarmConfigTarget":
                    SelectAlarmConfigTarget();
                    return true;
                case "IsEnabledSelectAlarmConfigTarget":
                    result = IsEnabledSelectAlarmConfigTarget();
                    return true;
            }

            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}

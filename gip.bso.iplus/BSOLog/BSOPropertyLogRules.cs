// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.bso.iplus
{
    /// <summary>
    /// Represents the module for configuration a PropertyLog rules.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Logging Rules for Properties'}de{'Protokollierungsregeln für Eigenschaften'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class BSOPropertyLogRules : ACBSO
    {
        #region c'tors

        /// <summary>
        /// Creates a new instance of the BSOPropertyLogRules.
        /// </summary>
        /// <param name="acType">The acType parameter.</param>
        /// <param name="content">The content parameter.</param>
        /// <param name="parentACObject">The parentACObject parameter.</param>
        /// <param name="parameter">The parameters in the ACValueList.</param>
        /// <param name="acIdentifier">The acIdentifier parameter.</param>
        public BSOPropertyLogRules(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
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
            _PropertyLogServiceName = new ACPropertyConfigValue<string>(this, "PropertyLogServiceName", "PropertyLogService");
            AttachToPropertyLogService();
            return base.ACInit(startChildMode);
        }

        /// <summary>
        ///  Deinitializes this component.
        /// </summary>
        /// <param name="deleteACClassTask">The deleteACClassTask parameter.</param>
        /// <returns>True if is deinitialization success, otherwise returns false.</returns>
        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (PropertyLogService != null)
                PropertyLogService.Detach();

            if (ComponentSelector != null)
                ComponentSelector.PropertyChanged -= _ComponentSelector_PropertyChanged;

            bool done = base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }

            _PropertyLogService = null;
            CurrentVisuACClass = null;
            _SelectedRuleType = null;
            _ComponentSelector = null;
            _ComponentTypeFilter = null;
            _ComponentCheckHandler = null;
            SelectedLoggableProperty = null;
            SelectAffectedACClass = null;
            _OneselfRules = null;

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

        #region Properties

        private ACValueItem _SelectedRuleType;
        /// <summary>
        /// Gets or sets the selected rule type.
        /// </summary>
        [ACPropertySelected(401, "RuleType", "en{'Log rule type'}de{'Protokollregeltyp'}")]
        public ACValueItem SelectedRuleType
        {
            get
            {
                return _SelectedRuleType;
            }
            set
            {
                _SelectedRuleType = value;
                OnPropertyChanged("SelectedRuleType");

                if (_SelectedRuleType != null && ComponentSelector != null)
                {
                    IsProjectSelectorEnabled = true;
                    if ((Global.PropertyLogRuleType)_SelectedRuleType.Value == Global.PropertyLogRuleType.BasedOn)
                    {
                        ComponentSelector.ProjectFilterTypes = _ProjectTypeFilterBasedOn;
                    }
                    else if ((Global.PropertyLogRuleType)_SelectedRuleType.Value == Global.PropertyLogRuleType.ProjectHierarchy)
                    {
                        ComponentSelector.ProjectFilterTypes = _ProjectTypeFilterHierarcy;
                    }
                }
                else
                    IsProjectSelectorEnabled = false;
            }
        }

        /// <summary>
        /// Gets the list of a available rule types.
        /// </summary>
        [ACPropertyList(402, "RuleType")]
        public ACValueItemList RuleTypeList
        {
            get
            {
                return Global.PropertyLogRuleTypeList;
            }
        }

        private ACRef<IACComponent> _PropertyLogService;
        /// <summary>
        /// Gets the ACRef to PropertyLogService.
        /// </summary>
        public ACRef<IACComponent> PropertyLogService
        {
            get
            {
                return _PropertyLogService;
            }
        }

        private ACPropertyConfigValue<string> _PropertyLogServiceName;
        /// <summary>
        /// Gets or sets the name of a PropertyLogService. 
        /// </summary>
        [ACPropertyConfig("en{'PropertyLogService name'}de{'PropertyLogService name'}")]
        public string PropertyLogServiceName
        {
            get
            {
                return _PropertyLogServiceName.ValueT;
            }
            set
            {
                _PropertyLogServiceName.ValueT = value;
                OnPropertyChanged("PropertyLogServiceName");
            }
        }

        /// <summary>
        /// Gets or sets the currently selected component's ACClass in the Visualisation.
        /// </summary>
        [ACPropertyInfo(403)]
        public IACObject CurrentVisuACClass
        {
            get;
            set;
        }

        private bool _IsProjectSelectorEnabled = false;
        /// <summary>
        /// Gets or sets the IsProjectSelectorEnabled. Determines is project selector enabled or disabled.
        /// </summary>
        [ACPropertyInfo(404)]
        public bool IsProjectSelectorEnabled
        {
            get => _IsProjectSelectorEnabled;
            set
            {
                _IsProjectSelectorEnabled = value;
                OnPropertyChanged("IsProjectSelectorEnabled");
            }
        }

        #region Properties => ComponentSelector

        private BSOComponentSelector _ComponentSelector;
        internal BSOComponentSelector ComponentSelector
        {
            get
            {
                if (_ComponentSelector == null)
                {
                    _ComponentSelector = FindChildComponents<BSOComponentSelector>(c => c is BSOComponentSelector).FirstOrDefault();
                    if (_ComponentSelector != null)
                    {
                        _ComponentSelector.ProjectCheckHandler = ComponentCheckHandler;
                        _ComponentSelector.PropertyChanged += _ComponentSelector_PropertyChanged;
                    }
                }
                return _ComponentSelector;
            }
        }

        private void _ComponentSelector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BSOComponentSelector.CurrentProjectItemPropName)
            {
                if (ComponentSelector != null && ComponentSelector.CurrentProjectItemCS != null && ComponentSelector.CurrentProjectItemCS.ValueT != null)
                {
                    if (ComponentSelector.CurrentACProject != null && (ComponentSelector.CurrentACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary ||
                                                                       ComponentSelector.CurrentACProject.ACProjectType == Global.ACProjectTypes.AppDefinition))
                        CurrentVisuACClass = ACUrlCommand(ComponentSelector.CurrentProjectItemCS.ValueT.ACUrl) as IACObject;
                    else
                        CurrentVisuACClass = ACUrlCommand(ComponentSelector.CurrentProjectItemCS.ValueT.ACUrlComponent) as IACObject;
                    OnPropertyChanged("CurrentVisuACClass");
                }
                else
                {
                    CurrentVisuACClass = null;
                    OnPropertyChanged("CurrentVisuACClass");
                }
                OnPropertyChanged("LoggableProperties");
                OnPropertyChanged("AffectedACClasses");
            }
            else if (e.PropertyName == BSOComponentSelector.CurrentACProjectPropName && ComponentSelector.CurrentACProject != null)
            {
                if ((Global.PropertyLogRuleType)SelectedRuleType.Value == Global.PropertyLogRuleType.ProjectHierarchy)
                {
                    var storedRules = GetStoredPropertyLogRules(Global.PropertyLogRuleType.ProjectHierarchy).ToList();
                    storedRules.AddRange(GetStoredPropertyLogRules(Global.PropertyLogRuleType.ProjectHierarchyOneself));
                    if (storedRules != null)
                    {
                        _OneselfRules = new List<Guid>();
                        MarkConfiguredRules(ComponentSelector.CurrentProjectItemRoot, storedRules);
                    }
                }

                else if ((Global.PropertyLogRuleType)SelectedRuleType.Value == Global.PropertyLogRuleType.BasedOn)
                {
                    var storedRules = GetStoredPropertyLogRules(Global.PropertyLogRuleType.BasedOn);
                    if (storedRules != null)
                        MarkConfiguredRules(ComponentSelector.CurrentProjectItemRoot, storedRules.ToArray());
                }
            }
            else if (e.PropertyName == "CurrentProjectItemRoot")
            {
                if (ComponentSelector.CurrentProjectItemRoot != null && ComponentSelector.CurrentProjectItemRoot.IsVisible)
                    ComponentSelector.CurrentProjectItemCS = ComponentSelector.CurrentProjectItemRoot;

                else
                    ComponentSelector.CurrentProjectItemCS = null;
            }
        }

        Global.ACProjectTypes[] _ProjectTypeFilterHierarcy = new Global.ACProjectTypes[] { Global.ACProjectTypes.Application, Global.ACProjectTypes.Service };
        Global.ACProjectTypes[] _ProjectTypeFilterBasedOn = new Global.ACProjectTypes[] { Global.ACProjectTypes.ClassLibrary, Global.ACProjectTypes.AppDefinition };

        private ACClassInfoWithItems.VisibilityFilters _ComponentTypeFilter;
        /// <summary>
        /// Gets the ComponentTypeFilter.
        /// </summary>
        [ACPropertyInfo(405)]
        public ACClassInfoWithItems.VisibilityFilters ComponentTypeFilter
        {
            get
            {
                if (_ComponentTypeFilter == null)
                {
                    _ComponentTypeFilter = new ACClassInfoWithItems.VisibilityFilters();
                    _ComponentTypeFilter.CustomFilter = new Func<ACClassInfoWithItems, bool>(x => IsComponentLoggable(x.ValueT));
                }
                return _ComponentTypeFilter;
            }
        }

        private ACClassInfoWithItems.CheckHandler _ComponentCheckHandler;
        /// <summary>
        /// Gets the ComponentCheckHandler.
        /// </summary>
        public ACClassInfoWithItems.CheckHandler ComponentCheckHandler
        {
            get
            {
                if (_ComponentCheckHandler == null)
                    _ComponentCheckHandler = new ACClassInfoWithItems.CheckHandler() { IsCheckboxVisible = true, CheckedSetter = CheckedSetter };
                return _ComponentCheckHandler;
            }
        }

        #endregion

        #region Properties => PropertyLogRuleInfo

        /// <summary>
        /// Gets or sets the selected loggable property.
        /// </summary>
        [ACPropertySelected(406, "LoggableProperty")]
        public ACClassProperty SelectedLoggableProperty
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a available loggable properties.
        /// </summary>
        [ACPropertyList(407, "LoggableProperty")]
        public IEnumerable<ACClassProperty> LoggableProperties
        {
            get
            {
                if (ComponentSelector != null && ComponentSelector.CurrentProjectItemCS != null && ComponentSelector.CurrentProjectItemCS.ValueT != null)
                    return ComponentSelector.CurrentProjectItemCS.ValueT.Properties.Where(c => c.ACClassPropertyRelation_TargetACClassProperty
                                                                                   .Any(x => x.ConnectionTypeIndex == (short)Global.ConnectionTypes.PointState)).ToArray();
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the selected affected ACClass.
        /// </summary>
        [ACPropertySelected(408, "AffectedACClass")]
        public ACClass SelectAffectedACClass
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a affected ACClasses.
        /// </summary>
        [ACPropertyList(409, "AffectedACClass")]
        public IEnumerable<ACClass> AffectedACClasses
        {
            get
            {
                if (SelectedRuleType == null)
                    return null;

                if ((Global.PropertyLogRuleType)SelectedRuleType.Value == Global.PropertyLogRuleType.ProjectHierarchy)
                    return GetAffectedACClassesHierarchy();

                else if ((Global.PropertyLogRuleType)SelectedRuleType.Value == Global.PropertyLogRuleType.BasedOn)
                    return GetAffectedACClassesBasedOn();

                return null;
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Applies a configured property log rules.
        /// </summary>
        [ACMethodInfo("", "en{'Apply rules'}de{'Regeln anwenden'}", 401, true)]
        public void ApplyRules()
        {
            if ((Global.PropertyLogRuleType)SelectedRuleType.Value == Global.PropertyLogRuleType.ProjectHierarchy)
                ApplyRulesHierarchy();

            else if ((Global.PropertyLogRuleType)SelectedRuleType.Value == Global.PropertyLogRuleType.BasedOn)
                ApplyRulesBasedOn();

            if (PropertyLogService != null)
                PropertyLogService.ValueT.ACUrlCommand("!RebuildRuleCache");
            else
                Messages.Error(this, "The PropertyLogService is not available! Run rule refresh manually on the property log service.");
        }

        /// <summary>
        /// Determines is ApplyRules enabled or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise returns false.</returns>
        public bool IsEnabledApplyRules()
        {
            return SelectedRuleType != null && ComponentSelector != null && ComponentSelector.CurrentACProject != null;
        }

        /// <summary>
        /// BSOProjectSelector - item checked setter.
        /// </summary>
        /// <param name="acClassInfoWithItems">The item on which is checkmark setted.</param>
        /// <param name="isChecked">Determines is checked or not.</param>
        public void CheckedSetter(ACClassInfoWithItems acClassInfoWithItems, bool isChecked)
        {
            RefreshIconState(acClassInfoWithItems, isChecked ? Global.ChangeTypeEnum.Changed : Global.ChangeTypeEnum.None);
        }

        #region Methods => Visualisation

        /// <summary>
        /// Turn on properties logging for given component.
        /// </summary>
        /// <param name="componentClass">The component ACClass.</param>
        [ACMethodInfo("", "", 402)]
        public void ComponentPropertiesLoggingOn(ACClass componentClass)
        {
            if (componentClass == null)
                return;

            using (Database db = new core.datamodel.Database())
            {
                var propLogs = db.ACPropertyLogRule.Where(c => c.RuleType == (short)Global.PropertyLogRuleType.ProjectHierarchy && c.ACClass.ACProjectID == componentClass.ACProjectID).ToArray();

                if (propLogs.Any(c => componentClass.ACUrlComponent.StartsWith(c.ACClass.ACUrlComponent, StringComparison.Ordinal)))
                {
                    //Question50042: Logging properties for this component is active by inherited rule in the project hierarchy. Do you still want activate properties logging on this component? 
                    if (Messages.Question(this, "Question50042", Global.MsgResult.No) == Global.MsgResult.No)
                        return;
                }

                ACPropertyLogRule propLogRule = ACPropertyLogRule.NewACObject(db, componentClass, Global.PropertyLogRuleType.ProjectHierarchyOneself);
                db.ACPropertyLogRule.Add(propLogRule);
                var msg = db.ACSaveChanges();

                if (msg != null)
                {
                    Messages.Msg(msg);
                }
                else
                {
                    if (PropertyLogService != null)
                    {
                        PropertyLogService.ValueT.ACUrlCommand("!RebuildRuleCache");
                        //Info50030:The rule for properties logging is succesfully activated.
                        Messages.Info(this, "Info50030");
                    }
                    else
                    {
                        Messages.Error(this, "The PropertyLogService is not available! Run rule refresh manually on the property log service.");
                    }
                }
            }
        }

        /// <summary>
        /// Determines is ComponentPropertiesLoggingOn enabled or disabled.
        /// </summary>
        /// <param name="componentClass">The component ACClass.</param>
        /// <returns>True if is enabled, oterwise returns false.</returns>
        public bool IsEnabledComponentPropertiesLoggingOn(ACClass componentClass)
        {
            if (componentClass == null || PropertyLogService == null)
                return false;

            using (Database db = new core.datamodel.Database())
            {
                return !db.ACPropertyLogRule.Any(c => c.ACClassID == componentClass.ACClassID);
            }
        }

        /// <summary>
        /// Turn off properties logging for given component.
        /// </summary>
        /// <param name="componentClass">The component ACClass.</param>
        [ACMethodInfo("", "", 403)]
        public void ComponentPropertiesLoggingOff(ACClass componentClass)
        {
            if (componentClass == null)
                return;

            using (Database db = new core.datamodel.Database())
            {
                ACPropertyLogRule propLogRule = db.ACPropertyLogRule.FirstOrDefault(c => c.ACClassID == componentClass.ACClassID);
                if (propLogRule != null)
                {
                    db.ACPropertyLogRule.Remove(propLogRule);
                    var msg = db.ACSaveChanges();
                    if (msg != null)
                    {
                        Messages.Msg(msg);
                    }
                    else
                    {
                        if (PropertyLogService != null)
                        {
                            PropertyLogService.ValueT.ACUrlCommand("!RebuildRuleCache");
                            //Info50030:The rule for properties logging is succesfully activated.
                            Messages.Info(this, "Info50030");
                        }
                        else
                        {
                            Messages.Error(this, "The PropertyLogService is not available! Run rule refresh manually on the property log service.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines is ComponentPropertiesLoggingOff enabled or disabled.
        /// </summary>
        /// <param name="componentClass">The component ACClass.</param>
        /// <returns>True if is enabled, oterwise returns false.</returns>
        public bool IsEnabledComponentPropertiesLoggingOff(ACClass componentClass)
        {
            if (componentClass == null || PropertyLogService == null)
                return false;

            using (Database db = new core.datamodel.Database())
            {
                return db.ACPropertyLogRule.Any(c => c.ACClassID == componentClass.ACClassID);
            }
        }

        #endregion

        #region Methods => Private

        /// <summary>
        /// Shows only components which are loggable(which have a properties with PointState)
        /// </summary>
        private bool IsComponentLoggable(ACClass acClass)
        {
            return acClass.Properties.Any(c => c.ACClassPropertyRelation_TargetACClassProperty.Any(x => x.ConnectionTypeIndex == (short)Global.ConnectionTypes.PointState));
        }

        private IEnumerable<ACPropertyLogRule> GetStoredPropertyLogRules(Global.PropertyLogRuleType ruleType)
        {
            if (ComponentSelector == null || ComponentSelector.CurrentACProject == null)
                return null;

            return Db.ACPropertyLogRule.Where(c => c.RuleType == (short)ruleType &&
                                                   c.ACClass.ACProject.ACProjectID == ComponentSelector.CurrentACProject.ACProjectID);
        }

        private List<Guid> _OneselfRules = new List<Guid>();

        private void MarkConfiguredRules(ACClassInfoWithItems infoClass, IEnumerable<ACPropertyLogRule> rules)
        {
            if (infoClass == null)
                return;

            if(infoClass.ValueT != null)
            {
                ACPropertyLogRule rule = rules.FirstOrDefault(c => c.ACClassID == infoClass.ValueT.ACClassID);
                if(rule != null)
                {
                    infoClass.IsChecked = true;
                    if (rule.RuleType == (short)Global.PropertyLogRuleType.ProjectHierarchyOneself)
                        _OneselfRules.Add(infoClass.ValueT.ACClassID);
                }
                else if((Global.PropertyLogRuleType)SelectedRuleType.Value == Global.PropertyLogRuleType.ProjectHierarchy && infoClass.ParentContainerT != null && infoClass.ParentContainerT.IsChecked &&
                        !_OneselfRules.Any(c => c == infoClass.ParentContainerT.ValueT.ACClassID))
                {
                    infoClass.IsChecked = true;
                }
            }

            foreach (ACClassInfoWithItems item in infoClass.VisibleItemsT)
                MarkConfiguredRules(item, rules);
        }

        private void RefreshIconState(ACClassInfoWithItems start, Global.ChangeTypeEnum changeType)
        {
            ACClassInfoWithItems parent = start.ParentContainerT;
            if (parent != null)
            {
                if (changeType == Global.ChangeTypeEnum.None && parent.ItemsT.Any(c => c != start && (c.IsChecked ||
                                                                                      (c.IconState != null && (Global.ChangeTypeEnum)c.IconState != Global.ChangeTypeEnum.None))))
                    return;

                parent.IconState = changeType;
                RefreshIconState(parent, changeType);
            }
        }

        #region Methods => Private => AffectedACClasses

        private List<ACClass> GetAffectedACClassesHierarchy()
        {
            if (ComponentSelector == null || ComponentSelector.CurrentProjectItemCS == null || ComponentSelector.CurrentProjectItemCS.ValueT == null)
                return null;

            List<ACClass> result = new List<ACClass>();
            GetAffectedACClassHierarchyRecursive(result, ComponentSelector.CurrentProjectItemCS);
            return result;
        }

        private void GetAffectedACClassHierarchyRecursive(List<ACClass> result, ACClassInfoWithItems acClassInfoWithItems)
        {
            if (IsComponentLoggable(acClassInfoWithItems.ValueT))
                result.Add(acClassInfoWithItems.ValueT);

            foreach (ACClassInfoWithItems classInfo in acClassInfoWithItems.ItemsT)
                GetAffectedACClassHierarchyRecursive(result, classInfo);
        }

        private List<ACClass> GetAffectedACClassesBasedOn()
        {
            if (ComponentSelector == null || ComponentSelector.CurrentProjectItemCS == null || ComponentSelector.CurrentProjectItemCS.ValueT == null)
                return null;

            List<ACClass> result = new List<ACClass>();
            GetAffectedACClassesBasedOnRecursive(result, ComponentSelector.CurrentProjectItemCS.ValueT);
            return result;

        }

        private void GetAffectedACClassesBasedOnRecursive(List<ACClass> result, ACClass acClass)
        {
            if ((acClass.ACProject.ACProjectType == Global.ACProjectTypes.Application || acClass.ACProject.ACProjectType == Global.ACProjectTypes.Service) &&
                IsComponentLoggable(acClass))
                result.Add(acClass);

            foreach (ACClass acClassDerived in acClass.ACClass_BasedOnACClass)
                GetAffectedACClassesBasedOnRecursive(result, acClassDerived);
        }

        #endregion

        private void ApplyRulesHierarchy()
        {
            List<Tuple<ACClass, Global.PropertyLogRuleType>> result = new List<Tuple<ACClass, Global.PropertyLogRuleType>>();
            GetRuleClassesFromProjectTree(result, ComponentSelector.CurrentProjectItemRoot);

            var storedRules = GetStoredPropertyLogRules(Global.PropertyLogRuleType.ProjectHierarchy).ToList();
            storedRules.AddRange(GetStoredPropertyLogRules(Global.PropertyLogRuleType.ProjectHierarchyOneself));

            if (storedRules != null)
            {
                foreach (var propLogRule in storedRules)
                    Db.ACPropertyLogRule.Remove(propLogRule);
            }

            foreach (var ruleClass in result)
            {
                ACPropertyLogRule rule = ACPropertyLogRule.NewACObject(Db, ruleClass.Item1);
                rule.RuleType = (short)ruleClass.Item2;
                Db.ACPropertyLogRule.Add(rule);
            }

            Db.ACSaveChanges();
        }

        private void GetRuleClassesFromProjectTree(List<Tuple<ACClass, Global.PropertyLogRuleType>> result, ACClassInfoWithItems infoItem, Global.PropertyLogRuleType ruleType = Global.PropertyLogRuleType.ProjectHierarchy)
        {
            if (infoItem.IsChecked)
            {
                if (ruleType == Global.PropertyLogRuleType.ProjectHierarchy)
                {
                    List<Tuple<ACClass, Global.PropertyLogRuleType>> resultOneselfList = new List<Tuple<ACClass, Global.PropertyLogRuleType>>();
                    DetermineProjectHierarchyCheckedRules(infoItem, resultOneselfList);
                    if (_IsProjectHierarchyOneself)
                        result.AddRange(resultOneselfList);
                    else
                        result.Add(new Tuple<ACClass, Global.PropertyLogRuleType>(infoItem.ValueT, Global.PropertyLogRuleType.ProjectHierarchy));

                    _IsProjectHierarchyOneself = false;
                    return;
                }
                else if (ruleType == Global.PropertyLogRuleType.BasedOn && infoItem.ValueT != null && IsComponentLoggable(infoItem.ValueT))
                    result.Add(new Tuple<ACClass, Global.PropertyLogRuleType>(infoItem.ValueT, Global.PropertyLogRuleType.BasedOn));
            }

            foreach (ACClassInfoWithItems child in infoItem.ItemsT)
                GetRuleClassesFromProjectTree(result, child, ruleType);
        }

        bool _IsProjectHierarchyOneself = false;

        private void DetermineProjectHierarchyCheckedRules(ACClassInfoWithItems infoItem, List<Tuple<ACClass, Global.PropertyLogRuleType>> result)
        {
            if (infoItem.IsVisible)
            {
                if (!infoItem.IsChecked)
                    _IsProjectHierarchyOneself = true;
                else
                    result.Add(new Tuple<ACClass, Global.PropertyLogRuleType>(infoItem.ValueT, Global.PropertyLogRuleType.ProjectHierarchyOneself));
            }

            foreach(ACClassInfoWithItems child in infoItem.ItemsT)
            {
                DetermineProjectHierarchyCheckedRules(child, result);
            }
        }

        private void ApplyRulesBasedOn()
        {
            List<Tuple<ACClass, Global.PropertyLogRuleType>> result = new List<Tuple<ACClass, Global.PropertyLogRuleType>>();
            GetRuleClassesFromProjectTree(result, ComponentSelector.CurrentProjectItemRoot, Global.PropertyLogRuleType.BasedOn);

            var storedRules = GetStoredPropertyLogRules(Global.PropertyLogRuleType.BasedOn);

            if (storedRules != null)
            {
                foreach (var propLogRule in storedRules.ToArray())
                    Db.ACPropertyLogRule.Remove(propLogRule);
            }

            foreach (var ruleClass in result)
            {
                ACPropertyLogRule rule = ACPropertyLogRule.NewACObject(Db, ruleClass.Item1, Global.PropertyLogRuleType.BasedOn);
                Db.ACPropertyLogRule.Add(rule);
            }

            var msg = Db.ACSaveChanges();
        }

        private void AttachToPropertyLogService()
        {
            IACComponent serviceAppManager = this.Root.FindChildComponents<ACComponent>(c => (c is ApplicationManagerProxy || c is ApplicationManager) &&
                                                                                                      c.ComponentClass.ACProject.ACProjectType == Global.ACProjectTypes.Service, null, 1)
                                                            .FirstOrDefault();

            if (serviceAppManager != null)
            {
                IACComponent propLogService = serviceAppManager.ACUrlCommand(PropertyLogServiceName) as IACComponent;
                if (propLogService != null)
                    _PropertyLogService = new ACRef<IACComponent>(propLogService, this);

            }
        }

        #endregion

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
                case "ApplyRules":
                    ApplyRules();
                    return true;
                case "IsEnabledApplyRules":
                    result = IsEnabledApplyRules();
                    return true;
                case "ComponentPropertiesLoggingOn":
                    ComponentPropertiesLoggingOn(acParameter[0] as ACClass);
                    return true;
                case Const.IsEnabledPrefix + "ComponentPropertiesLoggingOn":
                    result = IsEnabledComponentPropertiesLoggingOn(acParameter[0] as ACClass);
                    return true;
                case "ComponentPropertiesLoggingOff":
                    ComponentPropertiesLoggingOff(acParameter[0] as ACClass);
                    return true;
                case Const.IsEnabledPrefix + "ComponentPropertiesLoggingOff":
                    result = IsEnabledComponentPropertiesLoggingOff(acParameter[0] as ACClass);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}

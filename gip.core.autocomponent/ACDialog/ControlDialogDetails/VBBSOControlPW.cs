using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using gip.core.datamodel;
using System.Data.Objects.DataClasses;
using System.Collections.ObjectModel;
using System.Data.Objects;
using System.Data;
using System.Data.Metadata.Edm;


namespace gip.core.autocomponent
{
    /// <summary>
    /// Unter-BSO für VBBSOControlDialog
    /// Wird verwendet für PWBase (Workflowwelt) und Ableitungen
    /// 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBBSOControlPW'}de{'VBBSOControlPW'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, true)]
    public class VBBSOControlPW : ACBSO
    {
        #region c´tors

        public VBBSOControlPW(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }


        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            ParentACComponent.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ParentACComponent_PropertyChanged);
            _VarioConfigManager = ConfigManagerIPlus.ACRefToServiceInstance(this);
            CurrentPWInfo = ParentACComponent.ACUrlCommand("CurrentSelection") as IACComponentPWNode;
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            ParentACComponent.PropertyChanged -= ParentACComponent_PropertyChanged;

            // Saving procedure
            if (_CurrentRuleType != null)
                SaveSelectedValuesToRuleValues();

            if (IsMultiselect)
                ReplicationProcess();

            var multiSelect = VBControlMultiselect;
            if (multiSelect != null)
            {
                foreach (IACComponentPWNode item in multiSelect)
                {
                    if (item != null)
                        item.RefreshRuleStates();
                }
            }

            if (CurrentPWInfo != null)
                CurrentPWInfo.RefreshRuleStates();

            this._PWNodeMethodList = null;
            this._RuleTypeList = null;
            this._CurrentACClassWF = null;
            this._CurrentPWNodeMethod = null;
            this._CurrentPWInfo = null;
            this._CurrentRuleType = null;
            this._RuleObjectSelection = null;
            //this._Update = null;

            if (_VarioConfigManager != null)
                ConfigManagerIPlus.DetachACRefFromServiceInstance(this, _VarioConfigManager);
            _VarioConfigManager = null;

            return base.ACDeInit(deleteACClassTask);
        }


        #endregion

        #region Managers

        protected ACRef<ConfigManagerIPlus> _VarioConfigManager = null;
        public ConfigManagerIPlus VarioConfigManager
        {
            get
            {
                if (_VarioConfigManager == null)
                    return null;
                return _VarioConfigManager.ValueT;
            }
        }

        #endregion

        #region 1. PWNode/ACClassWF

        IACComponentPWNode _CurrentPWInfo;
        [ACPropertyInfo(9999)]
        public IACComponentPWNode CurrentPWInfo
        {
            get
            {
                return _CurrentPWInfo;
            }
            set
            {
                _CurrentPWInfo = value;
                if (CurrentPWInfo == null)
                {
                    CurrentACClassWF = null;
                }
                else
                {
                    CurrentACClassWF = CurrentPWInfo.ContentACClassWF;
                }
            }
        }

        [ACPropertyInfo(9999)]
        public List<IACComponentPWNode> VBControlMultiselect
        {
            get
            {
                var selDepDialog = ParentACComponent as VBBSOSelectionDependentDialog;
                if (selDepDialog == null || selDepDialog.VBControlMultiselect == null)
                    return null;
                return selDepDialog.VBControlMultiselect.Select(x => x as IACComponentPWNode).ToList();
            }
        }

        ACClassWF _CurrentACClassWF;
        [ACPropertyInfo(9999)]
        public ACClassWF CurrentACClassWF
        {
            get
            {
                return _CurrentACClassWF;
            }
            set
            {
                if (_IsChangeRuleTypeLocked)
                    return;
                if (_CurrentACClassWF != value)
                {
                    _PWNodeMethodList = new List<ACClassMethod>();

                    if (value != null)
                        SaveSelectedValuesToRuleValues();

                    _CurrentACClassWF = value;

                    // 1. Refresh methods-infos of Workflow-Node
                    if (_CurrentACClassWF != null)
                        _PWNodeMethodList = _CurrentACClassWF.PWACClass.MethodsCached.Where(c => c.ACGroup == Const.ACState).OrderBy(c => c.SortIndex).ToList();
                    OnPropertyChanged("PWNodeMethodList");
                    var firstMethod = _PWNodeMethodList.Where(c => c.ACIdentifier == ACStateConst.SMStarting).FirstOrDefault();
                    if (firstMethod == null)
                        firstMethod = _PWNodeMethodList.FirstOrDefault();
                    CurrentPWNodeMethod = firstMethod;
                    // RefreshPWNodeParamValueList(); Is done in CurrentPWNodeMethod

                    // 2. Refresh method-infos of PAFunction
                    OnPropertyChanged("CurrentPAFunctionMethod");
                    RefreshPAFunctionParamValueList();

                    // 3. Refresh Rulesdp
                    RefreshRules();

                    OnPropertyChanged("IsMachineConfigReadyStore");
                    LoadMachineList(_CurrentACClassWF);

                    OnPropertyChanged("CurrentACClassWF");
                }

            }
        }

        #endregion

        #region 1.1 PWNode-Method

        #region 1.1 PWNode-Method -> PWNode

        ACClassMethod _CurrentPWNodeMethod;
        [ACPropertyCurrent(9999, "PWNodeMethod", "en{'Workflow-Operation'}de{'Workflow-Operation'}")]
        public ACClassMethod CurrentPWNodeMethod
        {
            get
            {
                return _CurrentPWNodeMethod;
            }
            set
            {
                _CurrentPWNodeMethod = value;
                RefreshPWNodeParamValueList();
                OnPropertyChanged("PWNodeParamValueList");
                OnPropertyChanged("CurrentPWNodeMethod");
            }
        }

        List<ACClassMethod> _PWNodeMethodList;
        [ACPropertyList(9999, "PWNodeMethod")]
        public IEnumerable<ACClassMethod> PWNodeMethodList
        {
            get
            {
                return _PWNodeMethodList;
            }
        }

        public string CurrentPWNodeURL
        {
            get
            {
                return CurrentACClassWF.ConfigACUrl + @"\" + CurrentPWNodeMethod.ACIdentifier;
            }
        }

        public string CurrentPWNodePropertyURL
        {
            get
            {
                if (SelectedPWNodeParamValue == null)
                    return null;
                return CurrentPWNodeURL + @"\" + SelectedPWNodeParamValue.ACIdentifier;
            }
        }

        public bool PWNodeMethodEditorVisible
        {
            get
            {
                return SelectedPWNodeParamValue != null
                    && SelectedPWNodeParamValue.DefaultConfiguration != null
                    && SelectedPWNodeParamValue.DefaultConfiguration.ConfigStore != null
                    && CurrentPWInfo != null
                    && CurrentPWInfo.CurrentConfigStore != null
                    && SelectedPWNodeParamValue.DefaultConfiguration.ConfigStore.GetACUrl() == CurrentPWInfo.CurrentConfigStore.GetACUrl();
            }
        }

        #endregion

        #region PWNode & PAFunction Machine list common

        [ACPropertyInfo(9999)]
        public bool IsMachineConfigReadyStore
        {
            get
            {
                if (CurrentPWInfo == null || CurrentPWInfo.CurrentConfigStore == null) return false;
                return !(new List<string>() { "ACClassConfig", "ACProgramConfig" }).Contains(CurrentPWInfo.CurrentConfigStore.ACType.ACIdentifier);
            }
        }

        private void LoadMachineList(ACClassWF acClassWF)
        {
            List<ACClass> pwNodeMachineList = null;
            //List<ACClass> paFunctionMachineList = null;
            if (IsMachineConfigReadyStore)
            {
                // Group <-> Allowed_instances
                RuleTypeDefinition allowedInstancesRuleTypeDef = RulesCommand.ListOfRuleInfoPatterns.FirstOrDefault(p => p.RuleType == ACClassWFRuleTypes.Allowed_instances);
                // Method <-> Excluded_process_modules
                RuleTypeDefinition excludedProcessModulesRuleTypeDef = RulesCommand.ListOfRuleInfoPatterns.FirstOrDefault(p => p.RuleType == ACClassWFRuleTypes.Excluded_process_modules);

                // In case method
                if (excludedProcessModulesRuleTypeDef.RuleApplyedWFACKindTypes.Contains(acClassWF.PWACClass.ACKind))
                {
                    pwNodeMachineList = acClassWF.ACClassWF1_ParentACClassWF.RefPAACClass.DerivedClassesInProjects.ToList();
                    //paFunctionMachineList = RulesCommand.GetProcessModules(acClassWF, Database.ContextIPlus).ToList();
                }
                // In case PWGroup
                else if (allowedInstancesRuleTypeDef.RuleApplyedWFACKindTypes.Contains(acClassWF.PWACClass.ACKind))
                {
                    pwNodeMachineList = acClassWF.RefPAACClass.DerivedClassesInProjects.ToList();
                }
            }
            _PWNodeMachineList = pwNodeMachineList;
            // @aagincic: bypass to show group nodes for function to
            _PAFunctionMachineList = pwNodeMachineList;
            OnPropertyChanged("PWNodeMachineList");
            OnPropertyChanged("PAFunctionMachineList");
        }
        #endregion

        #region 1.1 PWNode-Method -> PWNodeParamValue

        #region 1.1 PWNode-Method -> PWNodeParamValue -> Properties

        private ACConfigParam _SelectedPWNodeParamValue;
        /// <summary>
        /// Selected property for ACValue
        /// </summary>
        /// <value>The selected PWNodeParamValue</value>
        [ACPropertySelected(9999, "PWNodeParamValue", "en{'TODO: PWNodeParamValue'}de{'TODO: PWNodeParamValue'}")]
        public ACConfigParam SelectedPWNodeParamValue
        {
            get
            {
                return _SelectedPWNodeParamValue;
            }
            set
            {
                if (_SelectedPWNodeParamValue != value)
                {
                    _SelectedPWNodeParamValue = value;
                    _AllHistoryPWNodeParamValueList = null;
                    OnPropertyChanged("SelectedPWNodeParamValue");
                    OnPropertyChanged("HistoryPWNodeParamValueList");
                    OnPropertyChanged("PWNodeMethodEditorVisible");
                    OnPropertyChanged("AllHistoryPWNodeParamValueList");
                }
            }
        }

        private List<ACConfigParam> _PWNodeParamValueList;
        /// <summary>
        /// List property for ACValue
        /// </summary>
        /// <value>The PWNodeParamValue list</value>
        [ACPropertyList(9999, "PWNodeParamValue")]
        public List<ACConfigParam> PWNodeParamValueList
        {
            get
            {
                return _PWNodeParamValueList;
            }
        }

        #endregion

        #region 1.1 PWNode-Method -> PWNodeParamValue -> Methods

        private void RefreshPWNodeParamValueList()
        {
            if (CurrentPWNodeMethod != null && CurrentPWNodeMethod.ACMethod != null)
            {
                _PWNodeParamValueList = VarioConfigManager.GetACConfigParamList(CurrentPWNodeMethod.ACMethod, CurrentPWInfo.MandatoryConfigStores, CurrentPWInfo.PreValueACUrl, CurrentPWNodeURL);
            }

            OnPropertyChanged("PWNodeParamValueList");
            OnPropertyChanged("HistoryPWNodeParamValueList");
        }

        #endregion

        #endregion

        #region 1.1 PWNode-Method -> PWNodeMachine

        private ACClass _SelectedPWNodeMachine;
        /// <summary>
        /// Selected property for ACClass
        /// </summary>
        /// <value>The selected PWNodeMachine</value>
        [ACPropertySelected(9999, "PWNodeMachine", "en{'Machine'}de{'Maschine'}")]
        public ACClass SelectedPWNodeMachine
        {
            get
            {
                return _SelectedPWNodeMachine;
            }
            set
            {
                if (_SelectedPWNodeMachine != value)
                {
                    _SelectedPWNodeMachine = value;
                    OnPropertyChanged("SelectedPWNodeMachine");
                }
            }
        }


        private List<ACClass> _PWNodeMachineList;
        /// <summary>
        /// List property for ACClass
        /// </summary>
        /// <value>The PWNodeMachine list</value>
        [ACPropertyList(9999, "PWNodeMachine")]
        public List<ACClass> PWNodeMachineList
        {
            get
            {
                return _PWNodeMachineList;
            }
        }

        private void AddPWNodeParamValueWithMachine()
        {
            var tmpParamList = PWNodeParamValueList.ToList();
            int currentIndex = tmpParamList.IndexOf(SelectedPWNodeParamValue);
            ACConfigParam additionalParam = ACConfigHelper.FactoryMachineParam(SelectedPWNodeParamValue, SelectedPWNodeMachine);
            tmpParamList.Insert(++currentIndex, additionalParam);
            _PWNodeParamValueList = tmpParamList;
            OnPropertyChanged("PWNodeParamValueList");
            SelectedPWNodeParamValue = additionalParam;
        }

        #endregion

        #region 1.1 PWNode-Method -> HistoryPWNodeParamValue

        private IACConfig _SelectedHistoryPWNodeParamValue;
        /// <summary>
        /// Selected property for IACConfig
        /// </summary>
        /// <value>The selected HistoryPWNodeParamValue</value>
        [ACPropertySelected(9999, "HistoryPWNodeParamValue", "en{'TODO: HistoryPWNodeParamValue'}de{'TODO: HistoryPWNodeParamValue'}")]
        public IACConfig SelectedHistoryPWNodeParamValue
        {
            get
            {
                return _SelectedHistoryPWNodeParamValue;
            }
            set
            {
                if (_SelectedHistoryPWNodeParamValue != value)
                {
                    _SelectedHistoryPWNodeParamValue = value;
                    OnPropertyChanged("SelectedHistoryPWNodeParamValue");
                }
            }
        }

        /// <summary>
        /// List property for IACConfig
        /// </summary>
        /// <value>The HistoryPWNodeParamValue list</value>
        [ACPropertyList(9999, "HistoryPWNodeParamValue")]
        public BindingList<IACConfig> HistoryPWNodeParamValueList
        {
            get
            {
                return SelectedPWNodeParamValue == null ? null : new BindingList<IACConfig>(SelectedPWNodeParamValue.ConfigurationList);
            }
        }

        #endregion

        #region 1.1 PWNode-Method -> Methods
        [ACMethodInteraction("PWNodeParamValue", "en{'Override parameter'}de{'Parameter überschreiben'}", (short)MISort.InsertData, true, "SelectedPWNodeParamValue")]
        public void CreatePWNodeValue()
        {
            if (!IsEnabledCreatePWNodeValue()) return;
            if (SelectedPWNodeMachine != null && (SelectedPWNodeParamValue.VBiACClassID == null
                || SelectedPWNodeParamValue.VBiACClassID != SelectedPWNodeMachine.ACClassID))
            {
                AddPWNodeParamValueWithMachine();
            }
            SelectedPWNodeParamValue.ACClassWF = CurrentACClassWF;
            Guid? vbiACClassID = SelectedPWNodeMachine != null ? SelectedPWNodeMachine.ACClassID : (Guid?)null;
            if (vbiACClassID == null && SelectedPWNodeParamValue.VBiACClassID != null)
                vbiACClassID = SelectedPWNodeParamValue.VBiACClassID;
            SelectedPWNodeParamValue.DefaultConfiguration =
                ConfigManagerIPlus
                .ACConfigFactory(
                    CurrentPWInfo.CurrentConfigStore,
                    SelectedPWNodeParamValue,
                    CurrentPWInfo.PreValueACUrl,
                    CurrentPWNodePropertyURL,
                    vbiACClassID);
            SelectedPWNodeParamValue.ConfigurationList.Insert(0, SelectedPWNodeParamValue.DefaultConfiguration);
            //CurrentPWInfo.CurrentConfigStore.ConfigurationEntries.Add(SelectedPWNodeParamValue.DefaultConfiguration);
            OnPropertyChanged("HistoryPWNodeParamValueList");
            OnPropertyChanged("SelectedPWNodeParamValue");
            OnPropertyChanged("PWNodeMethodEditorVisible");
        }

        public bool IsEnabledCreatePWNodeValue()
        {
            return
                CurrentPWInfo != null &&
                CurrentPWInfo.CurrentConfigStore != null &&
                !CurrentPWInfo.IsReadonly &&
                SelectedPWNodeParamValue != null &&
                (
                    SelectedPWNodeParamValue.DefaultConfiguration == null ||
                    SelectedPWNodeParamValue.DefaultConfiguration.ConfigStore.GetACUrl() != CurrentPWInfo.CurrentConfigStore.GetACUrl() ||
                    ((SelectedPWNodeParamValue.DefaultConfiguration.VBiACClassID == null) && (SelectedPWNodeMachine != null))
                );
        }

        [ACMethodInteraction("PWNodeParamValue", "en{'Remove overridden parameter'}de{'Überschriebenen Parameter entfernen'}", (short)MISort.Delete, true, "SelectedPWNodeParamValue")]
        public void DeletePWNodeParamValue()
        {
            if (!IsEnabledDeletePWNodeParamValue()) return;
            ACConfigParam configParam = SelectedPWNodeParamValue;
            ConfigParamDelete(configParam);
            RefreshPWNodeParamValueList();
            SelectedPWNodeParamValue = configParam;
        }

        public bool IsEnabledDeletePWNodeParamValue()
        {
            return
                CurrentPWInfo != null &&
                CurrentPWInfo.CurrentConfigStore != null &&
                !CurrentPWInfo.IsReadonly
                &&
                (
                    SelectedPWNodeParamValue != null &&
                    SelectedPWNodeParamValue.DefaultConfiguration != null &&
                    SelectedPWNodeParamValue.DefaultConfiguration.ConfigStore.GetACUrl() == CurrentPWInfo.CurrentConfigStore.GetACUrl()
                 );
        }

        #endregion

        #endregion

        #region 1.2 PAFunction-Method

        #region 1.2 PAFunction-Method -> PAFunction

        [ACPropertyCurrent(9999, "PAFunction")]
        public ACClassMethod CurrentPAFunctionMethod
        {
            get
            {
                if (_CurrentACClassWF == null)
                    return null;
                return _CurrentACClassWF.RefPAACClassMethod;
            }
        }

        public string CurrentPAFRulePropertyACUrl
        {
            get
            {
                if (CurrentPWInfo == null || CurrentRuleType == null) return null;
                return CurrentACClassWF.ConfigACUrl
                    //  + ((CurrentACClassWF != null && CurrentACClassWF.RefPAACClassMethod != null) ? (@"\" + CurrentACClassWF.RefPAACClassMethod.ACIdentifier) : "") 
                    + @"\Rules\"
                    + CurrentRuleType.RuleType.ToString();
            }
        }

        public string CurrentPAFunctionURL
        {
            get
            {
                return CurrentACClassWF.ConfigACUrl + @"\" + CurrentPAFunctionMethod.ACIdentifier;
            }
        }

        public string CurrentPAFunctionPropertyURL
        {
            get
            {
                return CurrentPAFunctionURL + @"\" + SelectedPAFunctionParamValue.ACIdentifier;
            }
        }

        public bool PAFunctionMethodEditorVisible
        {
            get
            {
                return
                    CurrentPWInfo != null
                    && !CurrentPWInfo.IsReadonly
                    && SelectedPAFunctionParamValue != null
                    && SelectedPAFunctionParamValue.DefaultConfiguration != null
                    && SelectedPAFunctionParamValue.DefaultConfiguration.ConfigStore != null
                    && SelectedPAFunctionParamValue.DefaultConfiguration.ConfigStore.GetACUrl() == CurrentPWInfo.CurrentConfigStore.GetACUrl();
            }
        }

        #endregion

        #region 1.2 PAFunction-Method -> PAFunctionParamValue

        #region 1.2 PAFunction-Method -> PAFunctionParamValue -> Properties

        private ACConfigParam _SelectedPAFunctionParamValue;
        /// <summary>
        /// Selected property for ACValue
        /// </summary>
        /// <value>The selected PAFunctionParamValue</value>
        [ACPropertySelected(9999, "PAFunctionParamValue", "en{'TODO: PAFunctionParamValue'}de{'TODO: PAFunctionParamValue'}")]
        public ACConfigParam SelectedPAFunctionParamValue
        {
            get
            {
                return _SelectedPAFunctionParamValue;
            }
            set
            {
                if (_SelectedPAFunctionParamValue != value)
                {
                    _SelectedPAFunctionParamValue = value;
                    _AllHistoryPAFunctionParamValueList = null;
                    OnPropertyChanged("SelectedPAFunctionParamValue");
                    OnPropertyChanged("HistoryPAFunctionParamValueList");
                    OnPropertyChanged("PAFunctionMethodEditorVisible");
                    OnPropertyChanged("AllHistoryPAFunctionParamValueList");
                }
            }
        }

        private List<ACConfigParam> _PAFunctionParamValueList;
        /// <summary>
        /// List property for ACValue
        /// </summary>
        /// <value>The PAFunctionParamValue list</value>
        [ACPropertyList(9999, "PAFunctionParamValue")]
        public List<ACConfigParam> PAFunctionParamValueList
        {
            get
            {
                return _PAFunctionParamValueList;
            }
        }

        #endregion

        #region 1.2 PAFunction-Method -> PAFunctionParamValue -> Methods

        private void RefreshPAFunctionParamValueList()
        {
            if (CurrentPAFunctionMethod != null)
            {
                _PAFunctionParamValueList = VarioConfigManager.GetACConfigParamList(CurrentPAFunctionMethod.ACMethod, CurrentPWInfo.MandatoryConfigStores, CurrentPWInfo.PreValueACUrl, CurrentPAFunctionURL);
            }

            OnPropertyChanged("PAFunctionParamValueList");
            OnPropertyChanged("HistoryPAFunctionParamValueList");
        }

        #endregion

        #endregion

        #region 1.2  PAFunction-Method -> PAFunctionMachine

        private ACClass _SelectedPAFunctionMachine;
        /// <summary>
        /// Selected property for ACClass
        /// </summary>
        /// <value>The selected PAFunctionMachine</value>
        [ACPropertySelected(9999, "PAFunctionMachine", "en{'Machine'}de{'Maschine'}")]
        public ACClass SelectedPAFunctionMachine
        {
            get
            {
                return _SelectedPAFunctionMachine;
            }
            set
            {
                if (_SelectedPAFunctionMachine != value)
                {
                    _SelectedPAFunctionMachine = value;
                    OnPropertyChanged("SelectedPAFunctionMachine");
                }
            }
        }

        private List<ACClass> _PAFunctionMachineList;
        /// <summary>
        /// List property for ACClass
        /// </summary>
        /// <value>The PAFunctionMachine list</value>
        [ACPropertyList(9999, "PAFunctionMachine")]
        public List<ACClass> PAFunctionMachineList
        {
            get
            {
                return _PAFunctionMachineList;
            }
        }

        private void AddPAFunctionParamValueWithMachine()
        {
            var tmpParamList = PAFunctionParamValueList.ToList();
            int currentIndex = tmpParamList.IndexOf(SelectedPAFunctionParamValue);
            ACConfigParam additionalParam = ACConfigHelper.FactoryMachineParam(SelectedPAFunctionParamValue, SelectedPAFunctionMachine);
            tmpParamList.Insert(++currentIndex, additionalParam);
            _PAFunctionParamValueList = tmpParamList;
            OnPropertyChanged("PAFunctionParamValueList");
            SelectedPAFunctionParamValue = additionalParam;
        }

        #endregion

        #region 1.2 PAFunction-Method -> HistoryPAFunctionParamValue

        private IACConfig _SelectedHistoryPAFunctionParamValue;
        /// <summary>
        /// Selected property for IACConfig
        /// </summary>
        /// <value>The selected HistoryPAFunctionParamValue</value>
        [ACPropertySelected(9999, "HistoryPAFunctionParamValue", "en{'TODO: HistoryPAFunctionParamValue'}de{'TODO: HistoryPAFunctionParamValue'}")]
        public IACConfig SelectedHistoryPAFunctionParamValue
        {
            get
            {
                return _SelectedHistoryPAFunctionParamValue;
            }
            set
            {
                if (_SelectedHistoryPAFunctionParamValue != value)
                {
                    _SelectedHistoryPAFunctionParamValue = value;
                    OnPropertyChanged("SelectedHistoryPAFunctionParamValue");
                }
            }
        }

        /// <summary>
        /// List property for IACConfig
        /// </summary>
        /// <value>The HistoryPAFunctionParamValue list</value>
        [ACPropertyList(9999, "HistoryPAFunctionParamValue")]
        public BindingList<IACConfig> HistoryPAFunctionParamValueList
        {
            get
            {

                return SelectedPAFunctionParamValue == null ? null : new BindingList<IACConfig>(SelectedPAFunctionParamValue.ConfigurationList);
            }
        }

        #endregion

        #region 1.2 PAFunction-Method -> Methods

        [ACMethodInteraction("PAFunctionParamValue", "en{'Override parameter'}de{'Parameter überschreiben'}", (short)MISort.InsertData, true, "SelectedPAFunctionParamValue")]
        public void CreatePAFunctionValue()
        {
            if (!IsEnabledCreatePAFunctionValue()) return;
            if (SelectedPAFunctionMachine != null && (SelectedPAFunctionParamValue.VBiACClassID == null
               || SelectedPAFunctionParamValue.VBiACClassID != SelectedPAFunctionMachine.ACClassID))
            {
                AddPAFunctionParamValueWithMachine();
            }
            SelectedPAFunctionParamValue.ACClassWF = CurrentACClassWF;
            Guid? vbiACClassID = SelectedPAFunctionMachine != null ? SelectedPAFunctionMachine.ACClassID : (Guid?)null;
            if (vbiACClassID == null && SelectedPAFunctionParamValue.VBiACClassID != null)
                vbiACClassID = SelectedPAFunctionParamValue.VBiACClassID;
            SelectedPAFunctionParamValue.DefaultConfiguration =
                ConfigManagerIPlus
                .ACConfigFactory(
                    CurrentPWInfo.CurrentConfigStore,
                    SelectedPAFunctionParamValue,
                    CurrentPWInfo.PreValueACUrl,
                    CurrentPAFunctionPropertyURL,
                    vbiACClassID);
            SelectedPAFunctionParamValue.ConfigurationList.Insert(0, SelectedPAFunctionParamValue.DefaultConfiguration);
            //CurrentPWInfo.CurrentConfigStore.ConfigurationEntries.Add(SelectedPAFunctionParamValue.DefaultConfiguration);
            OnPropertyChanged("SelectedPAFunctionParamValue");
            OnPropertyChanged("HistoryPAFunctionParamValueList");
            OnPropertyChanged("PAFunctionMethodEditorVisible");
        }

        public bool IsEnabledCreatePAFunctionValue()
        {
            return
                CurrentPWInfo != null &&
                CurrentPWInfo.CurrentConfigStore != null &&
                !CurrentPWInfo.IsReadonly &&
                SelectedPAFunctionParamValue != null &&
                (
                    SelectedPAFunctionParamValue.DefaultConfiguration == null ||
                    (SelectedPAFunctionParamValue.DefaultConfiguration.ConfigStore != null &&
                    SelectedPAFunctionParamValue.DefaultConfiguration.ConfigStore.GetACUrl() != CurrentPWInfo.CurrentConfigStore.GetACUrl()) ||
                    ((SelectedPAFunctionParamValue.DefaultConfiguration.VBiACClassID == null) && (SelectedPAFunctionMachine != null))
                );
        }

        [ACMethodInteraction("PAFunctionParamValue", "en{'Remove overridden parameter'}de{'Überschriebenen Parameter entfernen'}", (short)MISort.Delete, true, "SelectedPAFunctionParamValue")]
        public void DeletePAFunctionValue()
        {
            if (!IsEnabledDeletePAFunctionValue()) return;
            ACConfigParam configParam = SelectedPAFunctionParamValue;
            ConfigParamDelete(configParam);
            RefreshPAFunctionParamValueList();
            SelectedPAFunctionParamValue = configParam;
        }

        public bool IsEnabledDeletePAFunctionValue()
        {
            return
                CurrentPWInfo != null &&
                CurrentPWInfo.CurrentConfigStore != null &&
                !CurrentPWInfo.IsReadonly &&
                (
                    SelectedPAFunctionParamValue != null &&
                    SelectedPAFunctionParamValue.DefaultConfiguration != null &&
                    SelectedPAFunctionParamValue.DefaultConfiguration.ConfigStore != null &&
                    SelectedPAFunctionParamValue.DefaultConfiguration.ConfigStore.GetACUrl() == CurrentPWInfo.CurrentConfigStore.GetACUrl());
        }

        #endregion

        #endregion

        #region 1.4 ACClassWFRule

        #region - Properties

        private RuleInfo[] _RuleTypeList;
        [ACPropertyList(9999, "RuleType")]
        public RuleInfo[] RuleTypeList
        {
            get { return _RuleTypeList; }
        }


        private RuleInfo _CurrentRuleType;
        [ACPropertyCurrent(9999, "RuleType", "en{'Rule'}de{'Rule'}")]
        public RuleInfo CurrentRuleType
        {
            get
            {
                return _CurrentRuleType;
            }
            set
            {

                if (_CurrentRuleType != value)
                {
                    _CurrentRuleType = value;
                    OnPropertyChanged("CurrentRuleType");
                    OnPropertyChanged("IsMultiValueRuleType");
                    LoadRuleObjectSelectItem();
                    if (_CurrentRuleType != null && _CurrentRuleType.RuleType == ACClassWFRuleTypes.ActiveRoutes)
                        InitRoutingValues();
                    else
                    {
                        OnPropertyChanged("SelectedValues");
                        OnPropertyChanged("AvailableValues");
                    }
                }
            }
        }

        public bool Grouped
        {
            get
            {
                //if (CurrentPWInfo != null)
                //    return CurrentPWInfo.ACConfigStageList.Any();
                return false;
                //return ConfigStageProvider != null && ConfigStageProvider.ConfigStoreStages.Any(); 
            }
        }

        private List<RuleObjectSelectionModel> _RuleObjectSelection;
        public List<RuleObjectSelectionModel> RuleObjectSelection
        {
            get
            {
                if (_RuleObjectSelection == null)
                    _RuleObjectSelection = new List<RuleObjectSelectionModel>();
                return _RuleObjectSelection;
            }
        }

        public RuleObjectSelectionModel RuleObjectSelected
        {
            get
            {
                if (RuleObjectSelection == null || CurrentPAFRulePropertyACUrl == null || CurrentRuleType == null) return null;
                return RuleObjectSelection.FirstOrDefault(x =>
                    x.PAFRulePropertyACUrl == CurrentPAFRulePropertyACUrl &&
                    x.RuleInfo.ConfigStoreUrl == CurrentRuleType.ConfigStoreUrl &&
                    x.RuleInfo.RuleType == CurrentRuleType.RuleType);
            }
        }

        public List<object> AvailableValues
        {
            get
            {
                if (RuleObjectSelected == null) return null;
                return RuleObjectSelected.AvailableValues;
            }
        }

        public List<object> SelectedValues
        {
            get
            {
                if (RuleObjectSelected == null) return null;
                return RuleObjectSelected.SelectedValues;
            }
        }

        public void LoadRuleObjectSelectItem()
        {
            if (CurrentPAFRulePropertyACUrl == null || _CurrentRuleType == null) return;
            if (RuleObjectSelected == null)
            {
                var currentACClassWF = Database.ContextIPlus.ACClassWF.FirstOrDefault(x => x.ACClassWFID == _CurrentACClassWF.ACClassWFID);
                var ruleValueList = VarioConfigManager.GetDBStoredRuleValueList(CurrentPWInfo.MandatoryConfigStores, CurrentPWInfo.PreValueACUrl, CurrentPAFRulePropertyACUrl, CurrentRuleType.ConfigStoreUrl);
                ruleValueList = ruleValueList.Where(x => x.RuleType == CurrentRuleType.RuleType).ToList();
                RuleObjectSelectionModel ruleObjectSelectionModel = new RuleObjectSelectionModel()
                {
                    RuleInfo = CurrentRuleType,
                    ConfigStoreURL = CurrentRuleType.ConfigStoreUrl,
                    PreACUrl = CurrentPWInfo.PreValueACUrl,
                    PAFRulePropertyACUrl = CurrentPAFRulePropertyACUrl,
                    AvailableValues = RulesCommand.GetWFNodeRuleObjects(_CurrentRuleType.RuleType, currentACClassWF).ToList(),
                    SelectedValues = RulesCommand.ObjectListFromRuleValue(_CurrentACClassWF.Database, ruleValueList)
                };
                RuleObjectSelection.Add(ruleObjectSelectionModel);
            }
        }

        /// <summary>
        /// Saving selected rules
        /// </summary>
        private void SaveSelectedValuesToRuleValues()
        {
            {
                foreach (RuleObjectSelectionModel ruleSelectionModel in RuleObjectSelection.Where(x => !x.RuleInfo.IsReadOnly))
                {
                    List<RuleValue> newRuleValueList = new List<RuleValue>();
                    if (ruleSelectionModel.SelectedValues != null && ruleSelectionModel.SelectedValues.Any())
                    {
                        RuleValue ruleValue = RulesCommand.RuleValueFromObjectList(ruleSelectionModel.RuleInfo.RuleType, ruleSelectionModel.SelectedValues);
                        bool isFalseBreakpoint = ruleValue.RuleType ==
                            ACClassWFRuleTypes.Breakpoint &&
                            ruleValue.RuleObjectValue != null &&
                            ruleValue.RuleObjectValue.Any() &&
                            ruleValue.RuleObjectValue.First() is bool &&
                            !((bool)ruleValue.RuleObjectValue.First());
                        if (!isFalseBreakpoint)
                            newRuleValueList.Add(ruleValue);
                    }

                    List<RuleValue> oldRuleValueList = VarioConfigManager?.GetDBStoredRuleValueList(CurrentPWInfo.MandatoryConfigStores, ruleSelectionModel.PreACUrl, ruleSelectionModel.PAFRulePropertyACUrl, ruleSelectionModel.ConfigStoreURL);
                    if (!RulesCommand.IsRuleValueListSame(oldRuleValueList, newRuleValueList))
                        SetRuleValueList(newRuleValueList, ruleSelectionModel.PreACUrl, ruleSelectionModel.PAFRulePropertyACUrl);
                }
                if (CurrentPWInfo != null && !CurrentPWInfo.IsReadonly && CurrentPWInfo.ContentACClassWF != null)
                {
                    IACBSOConfigStoreSelection parentBSOConfigStore = FindParentComponent<IACBSOConfigStoreSelection>();
                    if (parentBSOConfigStore != null)
                    {
                        parentBSOConfigStore.AddVisitedMethods(CurrentPWInfo.ContentACClassWF.ACClassMethod);
                    }
                }
            }
        }

        public bool IsMultiValueRuleType
        {
            get { return _CurrentRuleType != null && _CurrentRuleType.RuleType != ACClassWFRuleTypes.Parallelization && _CurrentRuleType.RuleType != ACClassWFRuleTypes.Breakpoint; }
        }

        private List<object> _RoutesItemsList;
        public List<object> RoutesItemsList
        {
            get
            {
                if (_RoutesItemsList == null)
                    _RoutesItemsList = new List<object>();
                return _RoutesItemsList;
            }
        }

        #endregion

        #region - Methods

        private void SetRuleValueList(List<RuleValue> newRuleValueList, string preConfigACUrl, string localConfigACUrl)
        {
            if (CurrentPWInfo.CurrentConfigStore == null)
                return;
            IACConfig currentStoreConfigItem = ACConfigHelper.GetStoreConfiguration(CurrentPWInfo.CurrentConfigStore.ConfigurationEntries, preConfigACUrl, localConfigACUrl, false, null);

            // VarioConfigManager.GetConfiguration(new List<IACConfigStore>() { CurrentPWInfo.CurrentConfigStore }, preConfigACUrl, localConfigACUrl);
            if (!newRuleValueList.Any())
            {
                if (currentStoreConfigItem != null && (currentStoreConfigItem as EntityObject).EntityState != EntityState.Detached)
                {
                    //CurrentPWInfo.CurrentConfigStore.ConfigurationEntries.Remove(currentStoreConfigItem);
                    //(currentStoreConfigItem as VBEntityObject).DeleteACObject(Database, false);
                    CurrentPWInfo.CurrentConfigStore.RemoveACConfig(currentStoreConfigItem);
                }
            }
            else
            {
                if (currentStoreConfigItem == null)
                {
                    currentStoreConfigItem = CurrentPWInfo.CurrentConfigStore.NewACConfig(CurrentACClassWF);
                    //CurrentPWInfo.CurrentConfigStore.ConfigurationEntries.Add(currentStoreConfigItem);
                }
                currentStoreConfigItem.LocalConfigACUrl = localConfigACUrl;
                currentStoreConfigItem.PreConfigACUrl = preConfigACUrl;
                RulesCommand.WriteIACConfig(Database, currentStoreConfigItem, newRuleValueList);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshRules()
        {
            if (_CurrentACClassWF == null)
                _RuleTypeList = new RuleInfo[0];

            else
            {
                List<RuleInfo> items = new List<RuleInfo>();
                string groupRuleTitle = "Group Routing Rules";


                List<IACConfigStore> sources = VarioConfigManager.GetACConfigStores(CurrentPWInfo.MandatoryConfigStores).OrderByDescending(x => x.OverridingOrder).ToList();

                foreach (IACConfigStore source in sources)
                {
                    IEnumerable<RuleTypeDefinition> selectedRuleDefinitions =
                        RulesCommand
                        .ListOfRuleInfoPatterns
                        .Where(x => x.RuleApplyedWFACKindTypes.Contains(_CurrentACClassWF.PWACClass.ACKind));
                    if (selectedRuleDefinitions.Any())
                        foreach (RuleTypeDefinition itemDefinition in selectedRuleDefinitions)
                        {
                            RuleInfo ruleInfo = new RuleInfo(itemDefinition.RuleType, itemDefinition.Translation, source);
                            ruleInfo._IsReadOnly = CurrentPWInfo.IsReadonly
                                                    || CurrentPWInfo.CurrentConfigStore == null
                                                    || (ruleInfo.ConfigStoreUrl != CurrentPWInfo.CurrentConfigStore.GetACUrl() && ruleInfo.ConfigStoreName != groupRuleTitle);
                            items.Add(ruleInfo);
                        }
                }

                _RuleTypeList = items.ToArray();
            }

            OnPropertyChanged("RuleTypeList");

            if (_RuleTypeList.Length > 0)
                this.CurrentRuleType = _RuleTypeList[0];
            else
                this.CurrentRuleType = null;
        }

        #endregion

        #region -Routing

        private ACClassInfoWithItems _SelectedRuleValue;
        [ACPropertySelected(999, "RuleValue")]
        public ACClassInfoWithItems SelectedRuleValue
        {
            get { return _SelectedRuleValue; }
            set
            {
                _SelectedRuleValue = value;
                OnPropertyChanged("SelectedRuleValue");
            }
        }

        private List<ACClassInfoWithItems> _RuleValuesList;
        [ACPropertyList(999, "RuleValue")]
        public List<ACClassInfoWithItems> RuleValuesList
        {
            get { return _RuleValuesList; }
            set
            {
                _RuleValuesList = value;
                OnPropertyChanged("RuleValuesList");
            }
        }

        private void InitRoutingValues()
        {
            List<ACClassInfoWithItems> tempList = new List<ACClassInfoWithItems>();
            foreach (var rule in AvailableValues)
            {
                if (rule is Route)
                {
                    var route = rule as Route;
                    if (!route.IsAttached)
                        route.AttachTo(Database.ContextIPlus);
                    tempList.Add(new ACClassInfoWithItems() { Value = rule });
                }
            }
            RuleValuesList = tempList;
        }

        #region Routing dialog

        private bool _IsChangeRuleTypeLocked = false;

        [ACMethodInfo("", "en{'Add'}de{'Hinzufügen'}", 999, true)]
        public void AddRoute()
        {
            IACVBBSORouteSelector routeSelector = ParentACComponent.GetChildComponent("VBBSORouteSelector_Child") as IACVBBSORouteSelector;
            if (routeSelector == null)
            {
                Messages.Error(this, "Route selector is not installed on parent component!!!");
                return;
            }

            ACClassWF currentACClassWF = Database.ContextIPlus.ACClassWF.FirstOrDefault(x => x.ACClassWFID == _CurrentACClassWF.ACClassWFID);
            RouteDirections routeDirection;

            IEnumerable<ACClass> allowedInstances = CheckAllowedInstances(currentACClassWF);
            IEnumerable<ACClass> availableModules = RulesCommand.GetProcessModulesRouting(currentACClassWF, Database.ContextIPlus, allowedInstances, out routeDirection);

            IEnumerable<ACClass> startCompsACUrl;
            IEnumerable<ACClass> endCompsACUrl;

            if (routeDirection == RouteDirections.Forwards)
            {
                startCompsACUrl = !allowedInstances.Any() ? currentACClassWF.ParentACClass.DerivedClassesInProjects
                                                          : currentACClassWF.ParentACClass.DerivedClassesInProjects.Intersect(allowedInstances);
                endCompsACUrl = availableModules;
            }
            else
            {
                endCompsACUrl = !allowedInstances.Any() ? currentACClassWF.ParentACClass.DerivedClassesInProjects
                                                          : currentACClassWF.ParentACClass.DerivedClassesInProjects.Intersect(allowedInstances);
                startCompsACUrl = availableModules;
            }

            _IsChangeRuleTypeLocked = true;
            routeSelector.GetAvailableRoutes(startCompsACUrl, endCompsACUrl);

            if (routeSelector.RouteResult == null)
            {
                _IsChangeRuleTypeLocked = false;
                return;
            }

            Route mergedRoute = Route.MergeRoutes(routeSelector.RouteResult);

            if (RuleValuesList != null)
                RuleValuesList.Add(new ACClassInfoWithItems() { Value = mergedRoute });

            RuleValuesList = _RuleValuesList.ToList();
            RuleObjectSelected.AvailableValues = RuleValuesList.Select(x => x.Value).ToList();
            RuleObjectSelected.SelectedValues = RuleValuesList.Select(x => x.Value).ToList();
            _IsChangeRuleTypeLocked = false;
        }

        public bool IsEnabledAddRoute()
        {
            return CurrentRuleType != null && !CurrentRuleType.IsReadOnly && RuleObjectSelected != null;
        }

        [ACMethodInfo("", "en{'Delete'}de{'Löschen'}", 999, true)]
        public void DeleteRoute()
        {
            if (!IsEnabledDeleteRoute()) return;

            if (Messages.Question(this, "Question50036", Global.MsgResult.No) == Global.MsgResult.Yes)
            {
                if (RuleValuesList != null)
                    RuleValuesList.Remove(SelectedRuleValue);

                RuleValuesList = _RuleValuesList.ToList();
                RuleObjectSelected.AvailableValues = RuleValuesList.Select(x => x.Value).ToList();
                RuleObjectSelected.SelectedValues = RuleValuesList.Select(x => x.Value).ToList();
            }
        }

        public bool IsEnabledDeleteRoute()
        {
            return CurrentRuleType != null && !CurrentRuleType.IsReadOnly && SelectedRuleValue != null && (SelectedRuleValue.Value is Route) && RuleObjectSelected != null;
        }

        [ACMethodInfo("", "en{'View'}de{'Anzeigen'}", 999, true)]
        public void OpenRoute()
        {
            IACVBBSORouteSelector routeSelector = ParentACComponent.GetChildComponent("VBBSORouteSelector_Child") as IACVBBSORouteSelector;
            if (routeSelector == null)
            {
                Messages.Error(this, "Error50125");
                return;
            }

            if (!(SelectedRuleValue.Value is Route))
            {
                Messages.Error(this, "Error50126");
                return;
            }

            _IsChangeRuleTypeLocked = true;
            routeSelector.EditRoutes(SelectedRuleValue.Value as Route, CurrentRuleType.IsReadOnly, true, true);

            if (routeSelector.RouteResult == null)
            {
                _IsChangeRuleTypeLocked = false;
                return;
            }

            Route mergedRoute = Route.MergeRoutes(routeSelector.RouteResult);
            if (SelectedRuleValue.Value is Route && mergedRoute.SequenceEqual(SelectedRuleValue.Value as Route))
            {
                _IsChangeRuleTypeLocked = false;
                return;
            }

            if (RuleValuesList != null)
            {
                RuleValuesList.Remove(SelectedRuleValue);
                RuleValuesList.Add(new ACClassInfoWithItems() { Value = mergedRoute });
            }
            RuleValuesList = _RuleValuesList.ToList();

            RuleObjectSelected.AvailableValues = RuleValuesList.Select(x => x.Value).ToList();
            RuleObjectSelected.SelectedValues = RuleValuesList.Select(x => x.Value).ToList();
            _IsChangeRuleTypeLocked = false;
        }

        public bool IsEnabledOpenRoute()
        {
            return SelectedRuleValue != null && RuleObjectSelected != null;
        }

        public IEnumerable<ACClass> CheckAllowedInstances(ACClassWF currentACClassWF)
        {
            var configUrl = currentACClassWF.ACClassWF1_ParentACClassWF.ConfigACUrl + @"\Rules\" + ACClassWFRuleTypes.Allowed_instances.ToString();
            var rules = VarioConfigManager.GetRuleValueList(CurrentPWInfo.MandatoryConfigStores, CurrentPWInfo.PreValueACUrl, configUrl);

            return rules != null ? RulesCommand.ObjectListFromRuleValue(_CurrentACClassWF.Database, rules.Items).Cast<ACClass>() : new List<ACClass>();
        }

        #endregion

        #endregion

        #endregion

        #region Design

        public string CurrentApplicationContainerLayout
        {
            get
            {
                return LayoutHelper.VBDockPanelEmpty();
            }
        }

        public bool IsConfigurationMethodVisibleInCurrentContext
        {
            get
            {
                return CurrentACClassWF != null && CurrentACClassWF.RefPAACClassMethod != null;
            }
        }
        #endregion

        #region Callback mehtods, override

        void ParentACComponent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ParentACComponent is VBBSOSelectionDependentDialog)
            {
                IACObject currentSelection1 = ((VBBSOSelectionDependentDialog)ParentACComponent).CurrentSelection;
                if (currentSelection1 != null && currentSelection1 is IACComponentPWNode)
                {
                    CurrentPWInfo = currentSelection1 as IACComponentPWNode;
                    CurrentPWInfo.RefreshRuleStates();
                }
                else if (CurrentPWInfo != null)
                    CurrentPWInfo.RefreshRuleStates();
            }
        }

        protected override Msg OnPreSave()
        {
            return base.OnPreSave();
        }

        #endregion

        #region Saving

        [ACMethodCommand("Configuration", "en{'Save Controlproperties'}de{'Steuerelementeigenschaften speichern'}", (short)MISort.Save)]
        public void SaveSolProperties()
        {

            using (ACMonitor.Lock(Root.Database.ContextIPlus.QueryLock_1X000))
            {
                this.Root.Database.ACSaveChanges();
            }
        }

        #endregion

        #region Common methods

        private void ConfigParamDelete(ACConfigParam configParam)
        {
            configParam.ConfigurationList.Remove(configParam.DefaultConfiguration);
            //CurrentPWInfo.CurrentConfigStore.ConfigurationEntries.Remove(configParam.DefaultConfiguration);
            //(configParam.DefaultConfiguration as VBEntityObject).DeleteACObject(Database, false);
            CurrentPWInfo.CurrentConfigStore.RemoveACConfig(configParam.DefaultConfiguration);
            configParam.DefaultConfiguration = configParam.ConfigurationList.OrderByDescending(x => x.ConfigStore.OverridingOrder).FirstOrDefault();
        }

        #endregion

        #region Replication, Multiselect

        #region Replication -> Properties

        public bool IsMultiselect
        {
            get
            {
                var multiselect = VBControlMultiselect;
                if (multiselect == null || multiselect.Count < 2 || !multiselect.Any(x => x != CurrentPWInfo))
                    return false;
                return true;
            }
        }

        [ACPropertyInfo(9999)]
        public string ReplicationTitle
        {
            get
            {
                if (!IsMultiselect) return null;
                List<string> acIdentifiers = VBControlMultiselect.Where(x => x != CurrentPWInfo).Select(x => x.ContentACClassWF.ACIdentifier).ToList();
                return CurrentPWInfo.ContentACClassWF.ACCaption + @"," + string.Join(",", acIdentifiers);
            }
        }

        #endregion

        #region Replication -> Methods

        [ACMethodInfo("Replication", "en{'Apply to selected data objects'}de{'Bei ausgewählten Datenobjekten anwenden'}", 9999)]
        public void ReplicationNodeApplyToAll()
        {
            if (!IsEnabledReplicationNodeApplyToAll()) return;
            var value = SelectedPWNodeParamValue.DefaultConfiguration.Value;
            var selectedConfigList = AllHistoryPWNodeParamValueList.Where(p => p.Selected);
            foreach (ACConfigSelected selectedConfig in selectedConfigList)
            {
                selectedConfig.ACConfig.Value = value;
                selectedConfig.Selected = false;
            }
        }

        public bool IsEnabledReplicationNodeApplyToAll()
        {
            return
                // check null values
                CurrentPWInfo != null &&
                CurrentPWInfo.CurrentConfigStore != null &&
                SelectedPWNodeParamValue != null &&
                SelectedPWNodeParamValue.DefaultConfiguration != null &&
                // check if we are on right place where IACConfig is defined
                (SelectedPWNodeParamValue.DefaultConfiguration as EntityObject).EntityState == EntityState.Unchanged &&
                 SelectedPWNodeParamValue.DefaultConfiguration.ConfigStore.GetACUrl() == CurrentPWInfo.CurrentConfigStore.GetACUrl() &&
                 // check is there candidate for replication 
                 AllHistoryPWNodeParamValueList != null &&
                 AllHistoryPWNodeParamValueList.Any(p => p.Selected); ;
        }

        [ACMethodInfo("Replication", "en{'Apply to selected data objects'}de{'Bei ausgewählten Datenobjekten anwenden'}", 9999)]
        public void ReplicationFunctionApplyToAll()
        {
            if (!IsEnabledReplicationFunctionApplyToAll()) return;
            var value = SelectedPAFunctionParamValue.DefaultConfiguration.Value;
            var selectedConfigList = AllHistoryPAFunctionParamValueList.Where(p => p.Selected);
            foreach (ACConfigSelected selectedConfig in selectedConfigList)
            {
                selectedConfig.ACConfig.Value = value;
                selectedConfig.Selected = false;
            }
        }

        public bool IsEnabledReplicationFunctionApplyToAll()
        {
            return
                // all null values check
                CurrentPWInfo != null &&
                CurrentPWInfo.CurrentConfigStore != null &&
                SelectedPAFunctionParamValue != null &&
                SelectedPAFunctionParamValue.DefaultConfiguration != null &&
                // selected param should be allready saved into database and selection is on right setup place - where in this case PAF parameter is defined
                (SelectedPAFunctionParamValue.DefaultConfiguration as EntityObject).EntityState == EntityState.Unchanged &&
                 SelectedPAFunctionParamValue.DefaultConfiguration.ConfigStore.GetACUrl() == CurrentPWInfo.CurrentConfigStore.GetACUrl() &&
                 // check is any param selected for replication
                 AllHistoryPAFunctionParamValueList != null &&
                 AllHistoryPAFunctionParamValueList.Any(p => p.Selected);
        }

        #endregion

        #region Replication -> Hepler methods

        public void ReplicationProcess()
        {
            // Define interested cofig list
            if (CurrentPWInfo == null || CurrentPWInfo.CurrentConfigStore == null)
                return;

            List<IACConfig> updatedConfigItems = CurrentPWInfo.CurrentConfigStore.ConfigurationEntries.ToList()
                .Where(x =>
                (x as EntityObject).EntityState == System.Data.EntityState.Modified ||
                (x as EntityObject).EntityState == System.Data.EntityState.Deleted ||
                (x as EntityObject).EntityState == System.Data.EntityState.Added)
                .ToList();

            var multiSelect = VBControlMultiselect;
            if (multiSelect != null)
            {
                List<IACComponentPWNode> multiSelectedNodes = multiSelect.Where(x => x != CurrentPWInfo).ToList();
                if (updatedConfigItems != null && updatedConfigItems.Any() && multiSelectedNodes != null && multiSelectedNodes.Any())
                {
                    ACConfigReplicationCommand replicationCommand = new ACConfigReplicationCommand();
                    replicationCommand.ReplicationProcess(Database, CurrentPWInfo.CurrentConfigStore, updatedConfigItems, multiSelectedNodes);
                }
            }
        }

        #endregion

        #endregion

        #region Param Overrides Anywhere

        #region AllHistoryPWNodeParamValue

        string _FilterAllHyPWNodeParamValueMin;
        [ACPropertyInfo(9999, "AllHistoryPWNodeParamValue", "en{'Min'}de{'Min'}")]
        public string FilterAllHyPWNodeParamValueMin
        {
            get
            {
                return _FilterAllHyPWNodeParamValueMin;
            }
            set
            {
                _FilterAllHyPWNodeParamValueMin = value;
            }
        }

        string _FilterAllHyPWNodeParamValueMax;
        [ACPropertyInfo(9999, "AllHistoryPWNodeParamValue", "en{'Max'}de{'Max'}")]
        public string FilterAllHyPWNodeParamValueMax
        {
            get
            {
                return _FilterAllHyPWNodeParamValueMax;
            }
            set
            {
                _FilterAllHyPWNodeParamValueMax = value;
            }
        }

        //private ACConfigSelected _SelectedAllHistoryPWNodeParamValue;
        //[ACPropertySelected(9999, "AllHistoryPWNodeParamValue", "en{'TODO: AllHistoryPWNodeParamValue'}de{'TODO: AllHistoryPWNodeParamValue'}")]
        //public ACConfigSelected SelectedAllHistoryPWNodeParamValue
        //{
        //    get
        //    {
        //        return _SelectedAllHistoryPWNodeParamValue;
        //    }
        //}

        private List<ACConfigSelected> _AllHistoryPWNodeParamValueList;
        /// <summary>
        /// List property for ACValue
        /// </summary>
        /// <value>The AllHistoryPWNodeParamValueList list</value>
        [ACPropertyList(9999, "AllHistoryPWNodeParamValue")]
        public List<ACConfigSelected> AllHistoryPWNodeParamValueList
        {
            get
            {
                return _AllHistoryPWNodeParamValueList;
            }
        }

        [ACMethodInteraction("AllHistoryPWNodeParamValue", "en{'Search'}de{'Suchen'}", (short)MISort.Delete, true, "SelectedPWNodeParamValue")]
        public void SearchAllHistoryPWNodeParamValue()
        {
            if (SelectedPWNodeParamValue == null) 
                return;
            _AllHistoryPWNodeParamValueList = null;

            var allHistoryPWNodeParamValueList =
                VarioConfigManager
                .QueryAllCOnfigs(Database, CurrentPWInfo.CurrentConfigStore, CurrentPWInfo.PreValueACUrl, CurrentPWNodePropertyURL,
                SelectedPWNodeParamValue.VBiACClassID);
            allHistoryPWNodeParamValueList = FilterAllHistoryParamValue(allHistoryPWNodeParamValueList, FilterAllHyPWNodeParamValueMin, FilterAllHyPWNodeParamValueMax);
            if (allHistoryPWNodeParamValueList != null)
                _AllHistoryPWNodeParamValueList = allHistoryPWNodeParamValueList.Select(p => new ACConfigSelected() { ACConfig = p, Selected = false }).ToList();
            OnPropertyChanged("AllHistoryPWNodeParamValueList");
        }

        #endregion

        #region AllHistoryPAFunctionParamValue

        string _FilterAllHyPAFunctionParamValueMin;
        [ACPropertyInfo(9999, "AllHistoryPAFunctionParamValue", "en{'Min'}de{'Min'}")]
        public string FilterAllHyPAFunctionParamValueMin
        {
            get
            {
                return _FilterAllHyPAFunctionParamValueMin;
            }
            set
            {
                _FilterAllHyPAFunctionParamValueMin = value;
            }
        }

        string _FilterAllHyPAFunctionParamValueMax;
        [ACPropertyInfo(9999, "AllHistoryPAFunctionParamValue", "en{'Max'}de{'Max'}")]
        public string FilterAllHyPAFunctionParamValueMax
        {
            get
            {
                return _FilterAllHyPAFunctionParamValueMax;
            }
            set
            {
                _FilterAllHyPAFunctionParamValueMax = value;
            }
        }


        //private ACConfigSelected _SelectedAllHistoryPAFunctionParamValue;

        //[ACPropertySelected(9999, "AllHistoryPAFunctionParamValue", "en{'TODO: AllHistoryPAFunctionParamValue'}de{'TODO: AllHistoryPAFunctionParamValue'}")]
        //public ACConfigSelected SelectedAllHistoryPAFunctionParamValue
        //{
        //    get
        //    {
        //        return _SelectedAllHistoryPAFunctionParamValue;
        //    }
        //}

        private List<ACConfigSelected> _AllHistoryPAFunctionParamValueList;
        /// <summary>
        /// List property for ACValue
        /// </summary>
        /// <value>The AllHistoryPAFunctionParamValue list</value>
        [ACPropertyList(9999, "AllHistoryPAFunctionParamValue")]
        public List<ACConfigSelected> AllHistoryPAFunctionParamValueList
        {
            get
            {
                return _AllHistoryPAFunctionParamValueList;
            }
        }

        [ACMethodInteraction("AllHistoryPAFunctionParamValue", "en{'Search'}de{'Suchen'}", (short)MISort.Delete, true, "SelectedPAFunctionParamValue")]
        public void SearchAllHistoryPAFunctionParamValue()
        {
            if (SelectedPAFunctionParamValue == null) return;
            _AllHistoryPAFunctionParamValueList = null;
            var allHistoryPAFunctionParamValueList =
                VarioConfigManager
                .QueryAllCOnfigs(Database, CurrentPWInfo.CurrentConfigStore, CurrentPWInfo.PreValueACUrl, CurrentPAFunctionPropertyURL,
                SelectedPAFunctionParamValue.VBiACClassID).ToList();
            allHistoryPAFunctionParamValueList = FilterAllHistoryParamValue(allHistoryPAFunctionParamValueList, FilterAllHyPAFunctionParamValueMin, FilterAllHyPAFunctionParamValueMax);
            if (allHistoryPAFunctionParamValueList != null)
                _AllHistoryPAFunctionParamValueList = allHistoryPAFunctionParamValueList.Select(p => new ACConfigSelected() { ACConfig = p, Selected = false }).ToList();
            OnPropertyChanged("AllHistoryPAFunctionParamValueList");
        }
        #endregion

        public List<IACConfig> FilterAllHistoryParamValue(List<IACConfig> list, string minValue, string maxValue)
        {
            if (string.IsNullOrEmpty(minValue) && string.IsNullOrEmpty(maxValue)) 
                return list;
            List<IACConfig> processedList = new List<IACConfig>();
            double filterValue = 0;
            double itemValue = 0;
            bool? matching = null;
            foreach (var item in list)
            {
                if (item.ValueTypeACClass.ACKindIndex == (short)Global.ACKinds.TACLRBaseTypes && "Int32,UInt16,UInt32,Int64,Int16,Double,UInt64,Decimal".Contains(item.ValueTypeACClass.ACIdentifier))
                {
                    if (double.TryParse(item.Value.ToString(), out itemValue))
                    {
                        if (!string.IsNullOrEmpty(minValue))
                        {
                            if (double.TryParse(minValue, out filterValue))
                            {
                                matching = itemValue >= filterValue;
                            }
                        }

                        if (!string.IsNullOrEmpty(maxValue))
                        {
                            if (double.TryParse(maxValue, out filterValue))
                            {
                                matching = (matching ?? true) && itemValue < filterValue;
                            }
                        }
                    }
                }

                if (matching ?? false)
                    processedList.Add(item);
                matching = null;
            }
            return processedList;
        }

        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "CreatePWNodeValue":
                    CreatePWNodeValue();
                    return true;
                case "DeletePWNodeParamValue":
                    DeletePWNodeParamValue();
                    return true;
                case "CreatePAFunctionValue":
                    CreatePAFunctionValue();
                    return true;
                case "DeletePAFunctionValue":
                    DeletePAFunctionValue();
                    return true;
                case "SaveSolProperties":
                    SaveSolProperties();
                    return true;
                case "ReplicationNodeApplyToAll":
                    ReplicationNodeApplyToAll();
                    return true;
                case "ReplicationFunctionApplyToAll":
                    ReplicationFunctionApplyToAll();
                    return true;
                case "SearchAllHistoryPWNodeParamValue":
                    SearchAllHistoryPWNodeParamValue();
                    return true;
                case "SearchAllHistoryPAFunctionParamValue":
                    SearchAllHistoryPAFunctionParamValue();
                    return true;
                case "AddRoute":
                    AddRoute();
                    return true;
                case "DeleteRoute":
                    DeleteRoute();
                    return true;
                case "OpenRoute":
                    OpenRoute();
                    return true;
                case Const.IsEnabledPrefix + "CreatePWNodeValue":
                    result = IsEnabledCreatePWNodeValue();
                    return true;
                case Const.IsEnabledPrefix + "DeletePWNodeParamValue":
                    result = IsEnabledDeletePWNodeParamValue();
                    return true;
                case Const.IsEnabledPrefix + "CreatePAFunctionValue":
                    result = IsEnabledCreatePAFunctionValue();
                    return true;
                case Const.IsEnabledPrefix + "DeletePAFunctionValue":
                    result = IsEnabledDeletePAFunctionValue();
                    return true;
                case Const.IsEnabledPrefix + "ReplicationNodeApplyToAll":
                    result = IsEnabledReplicationNodeApplyToAll();
                    return true;
                case Const.IsEnabledPrefix + "ReplicationFunctionApplyToAll":
                    result = IsEnabledReplicationFunctionApplyToAll();
                    return true;
                case Const.IsEnabledPrefix + "AddRoute":
                    result = IsEnabledAddRoute();
                    return true;
                case Const.IsEnabledPrefix + "DeleteRoute":
                    result = IsEnabledDeleteRoute();
                    return true;
                case Const.IsEnabledPrefix + "OpenRoute":
                    result = IsEnabledOpenRoute();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }
}

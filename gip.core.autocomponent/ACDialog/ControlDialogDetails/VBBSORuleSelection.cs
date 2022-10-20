using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent.ACDialog.ControlDialogDetails
{

    /// <summary>
    /// Special dialog for selecting rules only ad-hoc
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBBSORuleSelection'}de{'VBBSORuleSelection'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, true)]
    public class VBBSORuleSelection : ACBSO
    {
        #region c´tors

        public VBBSORuleSelection(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

          
        }


        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {

            _VarioConfigManager = ConfigManagerIPlus.ACRefToServiceInstance(this);
            if (Msgs != null && Msgs.Any())
                Load();
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
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

        #region Input values

        public List<Msg> Msgs { get; set; }
        public List<ACClassWFRuleTypes> FilterRuleTypes { get; set; }

        public IACConfigStore CurrentConfigStore { get; set; }

        public IACConfigStoreSelection ConfigStoreSelection { get; set; }

        #endregion

        #region Main Model 

        private RuleSelectionModel _RuleForConfig;
        [ACPropertyInfo(9999)]
        public RuleSelectionModel RuleForConfig
        {
            get
            {
                return _RuleForConfig;
            }
            set
            {
                if (_RuleForConfig != value)
                {
                    _RuleForConfig = value;
                    OnPropertyChanged("RuleForConfig");
                }
            }
        }
        #endregion

        #region Rule types

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
                    if (_CurrentRuleType != null && RuleForConfig != null && RuleForConfig.Items != null && RuleForConfig.Items.Any())
                        SaveRuleValues();
                    _CurrentRuleType = value;
                    if (value != null)
                        LoadRuleValues(RuleForConfig.Items, value);
                    OnPropertyChanged("CurrentRuleType");
                }
            }
        }

        public VBDialogResult DialogResult { get; set; }
        #endregion

        #region Methods

        public void Load()
        {
            if (!IsEnabledLoad()) return;
            RuleForConfig = new RuleSelectionModel();
            RuleForConfig.Items = LoadWFNodes(Msgs);
            // TODO: place for define RuleSelectionItem -> PreConfigACUrl and RuleSelectionItem -> LocalConfigACUrl
            List<short> acKinds = RuleForConfig.Items.Select(c => c.CurrentACClassWF.PWACClass.ACKindIndex).ToList();
            LoadRuleTypes(acKinds, FilterRuleTypes);
        }


        public bool IsEnabledLoad()
        {
            return Msgs != null && Msgs.Any();
        }

        [ACMethodInfo("Dialog", "en{'Dialog Select Rules'}de{'Dialog Rule auswählen'}", 9999)]
        public VBDialogResult ShowDialogSelectRules(List<Msg> msgs, List<ACClassWFRuleTypes> filterRuleTypes, IACConfigStore configStore, string configStoreSelectionURL)
        {
            DialogResult = new VBDialogResult() { SelectedCommand = eMsgButton.Cancel };
            Msgs = msgs;
            FilterRuleTypes = filterRuleTypes;
            CurrentConfigStore = configStore;
            ConfigStoreSelection = Root.Businessobjects.ACUrlCommand(configStoreSelectionURL) as IACConfigStoreSelection;
            Load();
            ShowDialog(this, "DlgSelectRules");
            return DialogResult;
        }

        [ACMethodInfo("Dialog", "en{'Ok'}de{'Ok'}", 9999)]
        public void SelectResultDlgOK()
        {
            if (_CurrentRuleType != null && RuleForConfig != null && RuleForConfig.Items != null && RuleForConfig.Items.Any())
                SaveRuleValues();
            Database.ACSaveChanges();
            DialogResult.SelectedCommand = eMsgButton.OK;
            CloseTopDialog();
        }

        [ACMethodInfo("Dialog", "en{'Cancel'}de{'Abbrechen'}", 9999)]
        public void SelectResultDlgCancel()
        {
            Database.ACUndoChanges();
            DialogResult.SelectedCommand = eMsgButton.Cancel;
            CloseTopDialog();
        }

        #endregion

        #region private methods

        private List<RuleSelectionItem> LoadWFNodes(List<Msg> msgs)
        {
            List<RuleSelectionItem> ruleSselectionItemList = new List<RuleSelectionItem>();
            foreach (Msg msg in msgs)
            {
                Dictionary<int, ACClassWFSearchByURL> search = ACClassWFSearchByURL.Search(Database.ContextIPlus, msg.XMLConfig + "\\");
                if (search.Any(c => c.Value.URLCompleted))
                {
                    ACClassWFSearchByURL foundedItem = search.Where(c=>c.Value.URLCompleted).Select(c => c.Value).FirstOrDefault();
                    if(foundedItem != null)
                    {
                        ACClassWFSearchByURL nodeItem = foundedItem.GetItem(msg.XMLConfig);
                        if (nodeItem != null)
                        {
                            string preUrl = msg.XMLConfig.Replace(nodeItem.WFNode.ConfigACUrl, "");
                            if (!ruleSselectionItemList.Any(c => c.PreConfigACUrl == preUrl && c.LocalConfigACUrl == nodeItem.WFNode.ConfigACUrl))
                            {
                                RuleSelectionItem ruleSelectionItem = new RuleSelectionItem();
                                ruleSelectionItem.Title = msg.CustomerData["title"];
                                ruleSelectionItem.CurrentACClassWF = nodeItem.WFNode;
                                ruleSelectionItem.LocalConfigACUrl = nodeItem.WFNode.ConfigACUrl;
                                ruleSelectionItem.PreConfigACUrl = preUrl;
                                ruleSselectionItemList.Add(ruleSelectionItem);
                            }
                        }
                    }
                }
            }
            return ruleSselectionItemList;
        }

        private void LoadRuleTypes(List<short> acKinds, List<ACClassWFRuleTypes> filterRuleTypes)
        {
            List<RuleInfo> items = new List<RuleInfo>();
            //string groupRuleTitle = "Group Routing Rules";

            IEnumerable<RuleTypeDefinition> selectedRuleDefinitions =
                RulesCommand
                .ListOfRuleInfoPatterns
                .Where(c =>
                    (acKinds == null ||
                    !acKinds.Any() ||
                    c.RuleApplyedWFACKindTypes.Select(rt => (short)rt).Intersect(acKinds).Any())
                    &&
                    (
                        filterRuleTypes == null ||
                        !filterRuleTypes.Any() ||
                        filterRuleTypes.Contains(c.RuleType)
                    )
                )
                .ToList();
            _RuleTypeList = selectedRuleDefinitions.Select(c => new RuleInfo(c.RuleType, c.Translation, CurrentConfigStore, false) { }).ToArray();
            if (_RuleTypeList.Any())
                CurrentRuleType = _RuleTypeList[0];
        }

        private void LoadRuleValues(List<RuleSelectionItem> items, RuleInfo ruleInfo)
        {
            foreach (var item in items)
            {
                LoadRuleItemValues(item, ruleInfo);
            }
        }

        private void LoadRuleItemValues(RuleSelectionItem item, RuleInfo ruleInfo)
        {
            item._CurrentRuleType = ruleInfo;
            var currentPAFRulePropertyACUrl = item.LocalConfigACUrl
                    //  + ((CurrentACClassWF != null && CurrentACClassWF.RefPAACClassMethod != null) ? (@"\" + CurrentACClassWF.RefPAACClassMethod.ACIdentifier) : "") 
                    + @"\Rules\"
                    + ruleInfo.RuleType.ToString();
            var ruleValueList = VarioConfigManager.GetDBStoredRuleValueList(ConfigStoreSelection.MandatoryConfigStores, item.PreConfigACUrl, currentPAFRulePropertyACUrl, CurrentRuleType.ConfigStoreUrl);
            item.AvailableValues = RulesCommand.GetWFNodeRuleObjects(ruleInfo.RuleType, item.CurrentACClassWF).ToList();
            item.SelectedValues = RulesCommand.ObjectListFromRuleValue(item.CurrentACClassWF.Database, ruleValueList);
        }


        private void SaveRuleValues()
        {
            foreach (var item in RuleForConfig.Items)
            {
                string rulePropertyACUrl = item.CurrentACClassWF.ConfigACUrl
                    //  + ((CurrentACClassWF != null && CurrentACClassWF.RefPAACClassMethod != null) ? (@"\" + CurrentACClassWF.RefPAACClassMethod.ACIdentifier) : "") 
                    + @"\Rules\"
                    + CurrentRuleType.RuleType.ToString();
                List<RuleValue> newRuleValueList = new List<RuleValue>();
                if (item.SelectedValues != null && item.SelectedValues.Any())
                {
                    RuleValue ruleValue = RulesCommand.RuleValueFromObjectList(CurrentRuleType.RuleType, item.SelectedValues);
                    newRuleValueList.Add(ruleValue);
                }
                List<RuleValue> oldRuleValueList = VarioConfigManager.GetDBStoredRuleValueList(ConfigStoreSelection.MandatoryConfigStores, item.PreConfigACUrl, rulePropertyACUrl, CurrentRuleType.ConfigStoreUrl);
                if (!RulesCommand.IsRuleValueListSame(oldRuleValueList, newRuleValueList))
                {
                    IACConfig currentStoreConfigItem = ACConfigHelper.GetStoreConfiguration(CurrentConfigStore.ConfigurationEntries, item.PreConfigACUrl, rulePropertyACUrl, false, null);
                    if (!newRuleValueList.Any())
                    {
                        if (currentStoreConfigItem != null && (currentStoreConfigItem as EntityObject).EntityState != EntityState.Detached)
                        {
                            (currentStoreConfigItem as VBEntityObject).DeleteACObject(Database, false);
                        }
                    }
                    else
                    {
                        if (currentStoreConfigItem == null)
                        {
                            currentStoreConfigItem = CurrentConfigStore.NewACConfig(item.CurrentACClassWF);
                            //CurrentConfigStore.ConfigurationEntries.Add(currentStoreConfigItem);
                        }
                        currentStoreConfigItem.LocalConfigACUrl = rulePropertyACUrl;
                        currentStoreConfigItem.PreConfigACUrl = item.PreConfigACUrl;
                        RulesCommand.WriteIACConfig(Database, currentStoreConfigItem, newRuleValueList);
                    }
                }
            }
        }
        #endregion
    }
}

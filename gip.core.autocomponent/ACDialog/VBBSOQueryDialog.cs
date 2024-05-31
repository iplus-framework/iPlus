using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;


namespace gip.core.autocomponent
{
    /// <summary>
    /// Der VBBSOQueryDialog dient zum Konfigurieren einer beliebigen "ACQueryDefinition"
    /// 
    /// Der Aufruf erfolgt immer über die Methode "bool QueryConfigDlg(ACQueryDefinition acQueryDefinition)"
    /// Beispiel:
    /// 
    /// ACQueryDefinition acQueryDefinition == ...;
    /// bool ok = (bool)ACUrlCommand("VBBSOQueryDialog!QueryConfigDlg",  new object[] {acQueryDefinition});
    ///
    /// Rückgabewert = True signalisiert das im VBBSOQueryDialog vorgenommene Änderungen mit "OK" bestätigt wurden.
    /// Rückgabewert = False signalisiert das die acQueryDefinition das der Dialog mit "Cancel" geschlossen wurde. Die 
    /// "acQueryDefinition" ist dann unverändert
    /// 
    /// Nach dem schließen des Dialogs wird dieses Sub-BSO auch geschlossen 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Querydialog'}de{'Abfragedialog'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBBSOQueryDialog : ACBSO
    {
        #region c´tors
        public VBBSOQueryDialog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            Result = false;
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._ACAccessChangeValue = null;
            this._ACColumnChangeValue = null;
            this._ACQueryDefChangeValue = null;
            this._ChangeValuePropertyType = null;
            this._CurrentAvailableProperty = null;
            this._CurrentColumnItem = null;
            this._CurrentConfigSaveMode = null;
            this._CurrentConfigurationName = null;
            this._CurrentFilterItem = null;
            this._CurrentLoadConfiguration = null;
            this._CurrentQueryDefinition = null;
            this._CurrentQueryDefinitionOrigRoot = null;
            this._CurrentQueryDefinitionRoot = null;
            this._CurrentQueryInfo = null;
            this._CurrentSortItem = null;
            this._NewColumnValue = null;
            this._PropertyModeList = null;
            this._QueryInfoList = null;
            this._SelectedAvailableProperty = null;
            return base.ACDeInit(deleteACClassTask);
        }

        int _EditMode = 0;

        /// <summary>
        /// true = Änderungen werden übernommen
        /// false = Änderungen werden nicht übernommen
        /// </summary>
        bool Result
        {
            get;
            set;
        }

        /// <summary>
        /// Dürfen andere ACQueryDefinion ausgewählt werden
        /// </summary>
        bool WithQuerySelection
        {
            get;
            set;
        }

        bool ShowFilter
        {
            get;
            set;
        }

        bool ShowSort
        {
            get;
            set;
        }

        bool ShowColumns
        {
            get;
            set;
        }

        bool ShowStatements
        {
            get;
            set;
        }

        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return base.OnGetControlModes(vbControl);
            switch (vbControl.VBContent)
            {
                case "CurrentQueryInfo":
                    if (!WithQuerySelection || QueryInfoList.Count() < 2)
                        return Global.ControlModes.Disabled;
                    break;
                case "CurrentQueryDefinition\\SearchWord":
                    if (!ShowFilter)
                        return Global.ControlModes.Hidden;
                    break;
                case "CurrentConfigurationName":
                    switch ((Global.ConfigSaveModes)(short)CurrentConfigSaveMode.Value)
                    {
                        case Global.ConfigSaveModes.Configurationname:
                            return Global.ControlModes.Enabled;
                        case Global.ConfigSaveModes.User:
                        case Global.ConfigSaveModes.Common:
                        default:
                            return Global.ControlModes.Disabled;
                    }
            }
            return base.OnGetControlModes(vbControl);
        }

        #endregion

        #region Query Manipulation
        #region BSO->ACProperty
        #region ACQueryDefinition
        ACQueryDefinition _CurrentQueryDefinitionOrigRoot;
        /// <summary>
        /// Originales Root ACQueryDefinition
        /// </summary>
        [ACPropertyCurrent(9999, "QueryDefinitionOrig", "en{'Orig.Querydefinition'}de{'Orig.Abfragedefinition'}")]
        public ACQueryDefinition CurrentQueryDefinitionOrigRoot
        {
            get
            {
                return _CurrentQueryDefinitionOrigRoot;
            }
            set
            {
                _CurrentQueryDefinitionOrigRoot = value;
                OnPropertyChanged("CurrentQueryDefinitionOrigRoot");
            }
        }

        ACQueryDefinition _CurrentQueryDefinitionRoot;
        /// <summary>
        /// Arbeitskopie Root ACQueryDefinition
        /// </summary>
        [ACPropertyCurrent(9999, "QueryDefinitionRoot", "en{'Copy.Querydefinition'}de{'Kopie.Abfragedefinition'}")]
        public ACQueryDefinition CurrentQueryDefinitionRoot
        {
            get
            {
                return _CurrentQueryDefinitionRoot;
            }
            set
            {
                _CurrentQueryDefinitionRoot = value;
                OnPropertyChanged("CurrentQueryDefinitionRoot");
            }
        }

        ACQueryDefinition _CurrentQueryDefinition;
        [ACPropertyCurrent(9999, "QueryDefinition", "en{'Querydefinition'}de{'Abfragedefinition'}")]
        public ACQueryDefinition CurrentQueryDefinition
        {
            get
            {
                return _CurrentQueryDefinition;
            }
            set
            {
                _CurrentQueryDefinition = value;

                var query = QueryInfoList.Where(c => c.ACObject == value);
                if (query.Any())
                {
                    _CurrentQueryInfo = query.First();
                }
                OnPropertyChanged("CurrentQueryDefinition");
                OnPropertyChanged("FilterItemList");
                OnPropertyChanged("SortItemList");
                OnPropertyChanged("ColumnItemList");

                OnPropertyChanged("AvailablePropertyList");
            }
        }
        #endregion

        #region

        ACObjectItem _CurrentQueryInfo;
        [ACPropertyCurrent(9999, "QueryInfo")]
        public ACObjectItem CurrentQueryInfo
        {
            get
            {
                return _CurrentQueryInfo;
            }
            set
            {
                if (_CurrentQueryInfo != value)
                {
                    _CurrentQueryInfo = value;
                    if (_CurrentQueryInfo != null)
                    {
                        CurrentQueryDefinition = value.ACObject as ACQueryDefinition;
                    }
                    OnPropertyChanged("CurrentQueryInfo");
                }
            }
        }

        List<ACObjectItem> _QueryInfoList = null;
        [ACPropertyList(9999, "QueryInfo")]
        public IEnumerable<ACObjectItem> QueryInfoList
        {
            get
            {
                if (_QueryInfoList == null)
                {
                    _QueryInfoList = new List<ACObjectItem>();

                    ACObjectItem acObjectItem = new ACObjectItem(CurrentQueryDefinitionRoot, CurrentQueryDefinitionRoot.ACCaption);
                    _QueryInfoList.Add(acObjectItem);
                    FillQueryInfoList(acObjectItem, CurrentQueryDefinitionRoot, 1);
                }
                return _QueryInfoList;
            }
        }

        void FillQueryInfoList(ACObjectItem parentQueryInfo, ACQueryDefinition acQueryDefinition, int level)
        {
            foreach (var acQueryDefinitionChild in acQueryDefinition.ACQueryDefinitionChilds)
            {
                string acCaption = "";
                for (int i = 0; i < level; i++)
                {
                    acCaption += ">";
                }
                acCaption += acQueryDefinitionChild.ACCaption;
                ACObjectItem acObjectItem = new ACObjectItem(acQueryDefinitionChild, acCaption);
                _QueryInfoList.Add(acObjectItem);
                FillQueryInfoList(acObjectItem, acQueryDefinitionChild, level + 1);
            }
        }
        #endregion

        #region ACFilterItem
        ACFilterItem _CurrentFilterItem;
        [ACPropertyCurrent(9999, "FilterItem")]
        public ACFilterItem CurrentFilterItem
        {
            get
            {
                return _CurrentFilterItem;
            }
            set
            {
                if (_CurrentFilterItem != value)
                {
                    _CurrentFilterItem = value;
                    OnPropertyChanged("CurrentFilterItem");
                }
            }
        }

        [ACPropertyList(9999, "FilterItem")]
        public IList<ACFilterItem> FilterItemList
        {
            get
            {
                if (CurrentQueryDefinition == null)
                    return null;
                return CurrentQueryDefinition.ACFilterColumns;
            }
        }
        #endregion

        #region ACSortItem
        ACSortItem _CurrentSortItem;
        [ACPropertyCurrent(9999, "SortItem")]
        public ACSortItem CurrentSortItem
        {
            get
            {
                return _CurrentSortItem;
            }
            set
            {
                if (_CurrentSortItem != value)
                {
                    _CurrentSortItem = value;
                    OnPropertyChanged("CurrentSortItem");
                }
            }
        }

        [ACPropertyList(9999, "SortItem")]
        public IList<ACSortItem> SortItemList
        {
            get
            {
                if (CurrentQueryDefinition == null)
                    return null;
                return CurrentQueryDefinition.ACSortColumns;
            }
        }
        #endregion

        #region ACColumnItem
        ACColumnItem _CurrentColumnItem;
        [ACPropertyCurrent(9999, "ColumnItem")]
        public ACColumnItem CurrentColumnItem
        {
            get
            {
                return _CurrentColumnItem;
            }
            set
            {
                if (_CurrentColumnItem != value)
                {
                    _CurrentColumnItem = value;
                    OnPropertyChanged("CurrentColumnItem");
                }
            }
        }

        [ACPropertyList(9999, "ColumnItem")]
        public List<ACColumnItem> ColumnItemList
        {
            get
            {
                if (CurrentQueryDefinition == null)
                    return null;
                return CurrentQueryDefinition.ACColumns;
            }
        }
        #endregion

        #region AvailableProperty
        [ACPropertyList(9999, "AvailableProperty")]
        public IEnumerable<IACObject> AvailablePropertyList
        {
            get
            {
                return GetAvailableProperties();
            }
        }

        ACObjectItem _CurrentAvailableProperty = null;
        [ACPropertyCurrent(9999, "AvailableProperty")]
        public ACObjectItem CurrentAvailableProperty
        {
            get
            {
                return _CurrentAvailableProperty;
            }
            set
            {
                if (_CurrentAvailableProperty != value)
                {
                    _CurrentAvailableProperty = value;
                    OnPropertyChanged("CurrentAvailableProperty");
                }
            }
        }

        private int _CurrentAvailablePropertyClicked;
        [ACPropertyInfo(9999)]
        public int CurrentAvailablePropertyClicked
        {
            get
            {
                return _CurrentAvailablePropertyClicked;
            }
            set
            {
                _CurrentAvailablePropertyClicked = value;
            }
        }


        ACObjectItem _SelectedAvailableProperty;
        [ACPropertySelected(9999, "AvailableProperty")]
        public ACObjectItem SelectedAvailableProperty
        {
            get
            {
                return _SelectedAvailableProperty;
            }
            set
            {
                _SelectedAvailableProperty = value;
                OnPropertyChanged("SelectedAvailableProperty");
            }
        }
        #endregion

        #region Load/Save
        string _CurrentConfigurationName;
        [ACPropertyCurrent(9999, "ConfigurationName", "en{'Configurationname'}de{'Configurationname'}")]
        public string CurrentConfigurationName
        {
            get
            {
                return _CurrentConfigurationName;
            }
            set
            {
                _CurrentConfigurationName = value;
                OnPropertyChanged("CurrentConfigurationName");
            }
        }

        ACClassConfig _CurrentLoadConfiguration;
        [ACPropertyCurrent(9999, "LoadConfiguration", "en{'Configurationname'}de{'Configurationname'}")]
        public ACClassConfig CurrentLoadConfiguration
        {
            get
            {
                return _CurrentLoadConfiguration;
            }
            set
            {
                _CurrentLoadConfiguration = value;
                OnPropertyChanged("CurrentLoadConfiguration");
            }
        }

        [ACPropertyList(9999, "LoadConfiguration")]
        public IEnumerable<ACClassConfig> LoadConfigrationList
        {
            get
            {
                ACClass acClass = this.CurrentQueryDefinitionOrigRoot.TypeACClass as ACClass;

                string comment;
                string prefixS = CurrentQueryDefinitionOrigRoot.BuildLocalConfigACUrl(out comment, Global.ConfigSaveModes.Common);
                string prefixU = CurrentQueryDefinitionOrigRoot.BuildLocalConfigACUrl(out comment, Global.ConfigSaveModes.User);
                string prefixC = CurrentQueryDefinitionOrigRoot.BuildLocalConfigACUrl(out comment, Global.ConfigSaveModes.Computer, null, true);

                return acClass.ConfigurationEntries
                    .Where(c => c.LocalConfigACUrl == prefixS
                             || c.LocalConfigACUrl == prefixU 
                             || c.LocalConfigACUrl.StartsWith(prefixC))
                    .OrderBy(c => c.LocalConfigACUrl)
                    .Select(c => c as ACClassConfig);
            }
        }

        ACValueItem _CurrentConfigSaveMode;
        [ACPropertyCurrent(9999, "ConfigSaveMode", "en{'Configurationmode'}de{'Konfigurationsmodus'}", "", false)]
        public ACValueItem CurrentConfigSaveMode
        {
            get
            {
                return _CurrentConfigSaveMode;
            }
            set
            {
                if (_CurrentConfigSaveMode != value && value != null)
                {
                    _CurrentConfigSaveMode = value;

                    OnPropertyChanged("CurrentConfigSaveMode");
                }
            }
        }

        ACValueItemList _PropertyModeList = null;
        [ACPropertyList(9999, "ConfigSaveMode")]
        public IEnumerable<ACValueItem> ConfigSaveModesList
        {
            get
            {
                if (_PropertyModeList == null)
                {
                    _PropertyModeList = new ACValueItemList(typeof(Global.ConfigSaveModes).Name);
                    _PropertyModeList.AddEntry((short)Global.ConfigSaveModes.Configurationname, Global.ConfigSaveModes.Configurationname.ToString());
                    _PropertyModeList.AddEntry((short)Global.ConfigSaveModes.User, Global.ConfigSaveModes.User.ToString());
                    _PropertyModeList.AddEntry((short)Global.ConfigSaveModes.Common, Global.ConfigSaveModes.Common.ToString());
                    _PropertyModeList.AddEntry((short)Global.ConfigSaveModes.Computer, Global.ConfigSaveModes.Computer.ToString());
                }
                return _PropertyModeList;
            }
        }

        bool _WithFilter = false;
        [ACPropertyList(9999, "")]
        public bool WithFilter
        {
            get
            {
                return _WithFilter;
            }
            set
            {
                _WithFilter = value;
                OnPropertyChanged("WithFilter");
            }
        }


        bool _StoreConfig = false;
        [ACPropertyList(9999, "", "en{'Store configuration'}de{'Konfiguration speichern'}", "",true)]
        public bool StoreConfig
        {
            get
            {
                return _StoreConfig;
            }
            set
            {
                _StoreConfig = value;
                if (value == true)
                    _EditMode = 2;
                else
                    _EditMode = 0;
                if (CurrentConfigSaveMode == null)
                    CurrentConfigSaveMode = ConfigSaveModesList.Where(c => (short)c.Value == (short)Global.ConfigSaveModes.User).First();
                OnPropertyChanged("StoreConfig");
            }
        }
        #endregion
        #endregion

        #region BSO->ACMethod

        /// <summary>Zeigt den Dialog zum konfigurieren eine ACQueryDefinition an</summary>
        /// <param name="acQueryDefinition"></param>
        /// <param name="withQuerySelection">Dürfen andere ACQueryDefinition ausgewählt werden</param>
        /// <param name="showFilter"></param>
        /// <param name="showSort"></param>
        /// <param name="showColumns"></param>
        /// <returns>true wenn Dialog mit "OK" geschlossen wird</returns>
        [ACMethodCommand("Query", "en{'Config'}de{'Konfiguration'}", (short)MISort.QueryPrintDlg)]
        public bool QueryConfigDlg(ACQueryDefinition acQueryDefinition, bool withQuerySelection, bool showFilter, bool showSort, bool showColumns)
        {
            _EditMode = 0;
            // Die Editierung erfolgt immer auf der übergebenen acQueryDefinition
            // Die Konfiguration muss aber immer auf dem Root-ACQueryDefinition erfolgen
            ACQueryDefinition acQueryDefinitionRoot = acQueryDefinition;
            WithQuerySelection = withQuerySelection;
            ShowFilter = showFilter;
            ShowSort = showSort;
            ShowColumns = showColumns;
            ShowStatements = true;

            string acUrlQueryDefinition = "";
            while (acQueryDefinitionRoot.ParentACObject is ACQueryDefinition)
            {
                if (!string.IsNullOrEmpty(acUrlQueryDefinition))
                {
                    acUrlQueryDefinition += "\\";
                }
                acUrlQueryDefinition += acQueryDefinitionRoot.ACIdentifier;
                //acUrlQueryDefinition += acQueryDefinitionRoot.ChildACUrl;
                acQueryDefinitionRoot = acQueryDefinitionRoot.ParentACObject as ACQueryDefinition;
            }

            // Originale Root ACQueryDefinition 
            CurrentQueryDefinitionOrigRoot = acQueryDefinitionRoot;

            // Arbeitskopie Root ACQueryDefinition erzeugen
            CurrentQueryDefinitionRoot = CurrentQueryDefinitionOrigRoot.Clone() as ACQueryDefinition;
            //CurrentQueryDefinitionRoot = ACActivator.CreateInstance(acQueryDefinitionRoot.TypeACClass, acQueryDefinitionRoot.ACType, this, null) as ACQueryDefinition;

            // Arbeitskopie erzeugen
            if (string.IsNullOrEmpty(acUrlQueryDefinition))
            {
                // Zur bearbeitende ACQueryDefinition ist die Root-ACQueryDefinition
                CurrentQueryDefinition = CurrentQueryDefinitionRoot;    
            }
            else
            {
                // Zur bearbeitende ACQueryDefinition ist eine untergeordnete-ACQueryDefinition
                CurrentQueryDefinition = CurrentQueryDefinitionRoot.ACUrlCommand(acUrlQueryDefinition, null) as ACQueryDefinition;
                if (CurrentQueryDefinition == null && acQueryDefinition != null)
                {
                    CurrentQueryDefinition = acQueryDefinition.Clone() as ACQueryDefinition;
                    (CurrentQueryDefinitionRoot.ACObjectChilds as IList<IACObject>).Add(CurrentQueryDefinition);
                }
            }
            // Konfiguration von originaler ACQueryDefinition kopieren
            //CurrentQueryDefinitionRoot.ConfigXML = CurrentQueryDefinitionOrigRoot.ConfigXML;

            ShowDialog(this, "QueryConfigDlg");
            this.ParentACComponent.StopComponent(this);
            return Result;
        }

        [ACMethodCommand("Query", "en{'Load Configuration'}de{'Konfiguration laden'}", 9999)]
        public bool QueryLoadDlg(ACQueryDefinition acQueryDefinition)
        {
            CurrentQueryDefinitionOrigRoot = acQueryDefinition;
            if (!LoadConfigrationList.Any())
                return false;

            _EditMode = 1;
            CurrentLoadConfiguration = LoadConfigrationList.First();
            ShowDialog(this, "QueryLoadDlg");

            return Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="acQueryDefinition"></param>
        /// <returns>Name (PropertyACUrl) unter dem die Konfiguration gespeichert wurde.</returns>
        [ACMethodCommand("Query", "en{'Save Configuration'}de{'Konfiguration speichern'}", 9999)]
        public bool QuerySaveDlg(ACQueryDefinition acQueryDefinition)
        {
            CurrentQueryDefinitionOrigRoot = acQueryDefinition;

            // Arbeitskopie Root ACQueryDefinition erzeugen
            CurrentQueryDefinitionRoot = CurrentQueryDefinitionOrigRoot.Clone() as ACQueryDefinition;
            //CurrentQueryDefinitionRoot = ACActivator.CreateInstance(acQueryDefinition.TypeACClass, acQueryDefinition.ACType, this, null) as ACQueryDefinition;

            // Konfiguration von originaler ACQueryDefinition kopieren
            //CurrentQueryDefinitionRoot.ConfigXML = CurrentQueryDefinitionOrigRoot.ConfigXML;

            WithFilter = false;
            _EditMode = 2;
            CurrentConfigSaveMode = ConfigSaveModesList.First();
            ShowDialog(this, "QuerySaveDlg");
            return Result;
        }

        #region ACFilterItem
        [ACMethodInteraction("FilterItem", "en{'New Filter'}de{'Neuer Filter'}", (short)MISort.New, false, "CurrentFilterItem")]
        public void NewFilterItem()
        {
            CurrentFilterItem = new ACFilterItem();
            CurrentFilterItem.UsedInGlobalSearch = false;
            CurrentQueryDefinition.ACFilterColumns.Add(CurrentFilterItem);
            OnPropertyChanged("FilterItemList");
        }

        [ACMethodInteraction("FilterItem", "en{'Delete Filter'}de{'Filter löschen'}", (short)MISort.Delete, false, "CurrentFilterItem")]
        public void DeleteFilterItem()
        {
            CurrentQueryDefinition.ACFilterColumns.Remove(CurrentFilterItem);
            OnPropertyChanged("FilterItemList");
        }

        public bool IsEnabledDeleteFilterItem()
        {
            return CurrentFilterItem != null;
        }

        [ACMethodInteraction("FilterItem", "en{'Up'}de{'Auf'}", (short)MISort.MoveUp, false, "CurrentFilterItem")]
        public void UpFilterItem()
        {
            int index = FilterItemList.IndexOf(CurrentFilterItem);
            if (index > 0)
            {
                ACFilterItem acFilterItem = CurrentFilterItem;
                FilterItemList.RemoveAt(index);
                FilterItemList.Insert(index-1, acFilterItem);
                OnPropertyChanged("FilterItemList");
                CurrentFilterItem = acFilterItem;
            }
        }

        public bool IsEnabledUpFilterItem()
        {
            return CurrentFilterItem != null && FilterItemList.IndexOf(CurrentFilterItem) > 0;
        }

        [ACMethodInteraction("FilterItem", "en{'Down'}de{'Ab'}", (short)MISort.MoveDown, false, "CurrentFilterItem")]
        public void DownFilterItem()
        {
            int index = FilterItemList.IndexOf(CurrentFilterItem);
            if (FilterItemList.Count()-1 > index)
            {
                ACFilterItem acFilterItem = CurrentFilterItem;
                FilterItemList.RemoveAt(index);
                FilterItemList.Insert(index + 1, acFilterItem);
                OnPropertyChanged("FilterItemList");
                CurrentFilterItem = acFilterItem;
            }
        }

        public bool IsEnabledDownFilterItem()
        {
            return CurrentFilterItem != null && FilterItemList.Count()-1 > FilterItemList.IndexOf(CurrentFilterItem);
        }

        /// <summary>
        /// Einfügen eines FilterItems per DragAndDrop vom TreeView aus
        /// Wenn acFilterItem != null, dann vor diesem einfügen
        /// Wenn acFilterItem == null, dann am Ende einfügen
        /// </summary>
        /// <param name="dropObject"></param>
        /// <param name="acFilterItem"></param>
        public void InsertFilterItem(IACInteractiveObject dropObject, ACFilterItem acFilterItem = null)
        {
            if (!IsEnabledInsertFilterItem(dropObject, acFilterItem))
                return;
            ACObjectItem acObjectItem = dropObject.ACContentList.First() as ACObjectItem;
            string propertyName = acObjectItem.ACUrlRelative;

            bool likeOperator = false;
            IACType typeOfColumn = CurrentQueryDefinition.QueryType;
            object acSource = null;
            string acPath = "";
            Global.ControlModes acControlMode = Global.ControlModes.Disabled;
            if (typeOfColumn != null && typeOfColumn.ACUrlBinding(propertyName, ref typeOfColumn, ref acSource, ref acPath, ref acControlMode))
            {
                if (typeOfColumn != null && typeOfColumn.ObjectType != null)
                    likeOperator = typeOfColumn.ObjectType.IsAssignableFrom(typeof(string));
            }

            Global.LogicalOperators logicalOp = Global.LogicalOperators.contains;
            if (!likeOperator)
                logicalOp = Global.LogicalOperators.equal;

            ACFilterItem newACFilterItem = new ACFilterItem(Global.FilterTypes.filter, propertyName, logicalOp, Global.Operators.and, "", false);

            if (acFilterItem == null)
            {
                FilterItemList.Add(newACFilterItem);
            }
            else
            {
                int index = FilterItemList.IndexOf(acFilterItem);
                FilterItemList.Insert(index, newACFilterItem);
            }
            OnPropertyChanged("FilterItemList");
            CurrentFilterItem = newACFilterItem;
        }

        public bool IsEnabledInsertFilterItem(IACInteractiveObject dropObject, ACFilterItem acFilterItem = null)
        {
            ACObjectItem acObjectItem = dropObject.ACContentList.First() as ACObjectItem;
            return acObjectItem != null && !string.IsNullOrEmpty(acObjectItem.ACUrlRelative);
        }
        #endregion

        #region ACSortItem
        [ACMethodInteraction("SortItem", "en{'New Sorting'}de{'Neue Sortierung'}", (short)MISort.New, false, "CurrentSortItem")]
        public void NewSortItem()
        {
            CurrentSortItem = new ACSortItem();
            CurrentQueryDefinition.ACSortColumns.Add(CurrentSortItem);
            OnPropertyChanged("SortItemList");
        }

        [ACMethodInteraction("SortItem", "en{'Delete Sorting'}de{'Sortierung löschen'}", (short)MISort.Delete, false, "CurrentSortItem")]
        public void DeleteSortItem()
        {
            CurrentQueryDefinition.ACSortColumns.Remove(CurrentSortItem);
            OnPropertyChanged("SortItemList");
        }

        public bool IsEnabledDeleteSortItem()
        {
            return CurrentSortItem != null;
        }

        [ACMethodInteraction("SortItem", "en{'Up'}de{'Auf'}", (short)MISort.MoveUp, false, "CurrentSortItem")]
        public void UpSortItem()
        {
            int index = SortItemList.IndexOf(CurrentSortItem);
            if (index > 0)
            {
                ACSortItem acSortItem = CurrentSortItem;
                SortItemList.RemoveAt(index);
                SortItemList.Insert(index - 1, acSortItem);
                OnPropertyChanged("SortItemList");
                CurrentSortItem = acSortItem;
            }
        }

        public bool IsEnabledUpSortItem()
        {
            return CurrentSortItem != null && SortItemList.IndexOf(CurrentSortItem) > 0;
        }

        [ACMethodInteraction("SortItem", "en{'Down'}de{'Ab'}", (short)MISort.MoveDown, false, "CurrentSortItem")]
        public void DownSortItem()
        {
            int index = SortItemList.IndexOf(CurrentSortItem);
            if (SortItemList.Count() - 1 > index)
            {
                ACSortItem acSortItem = CurrentSortItem;
                SortItemList.RemoveAt(index);
                SortItemList.Insert(index + 1, acSortItem);
                OnPropertyChanged("SortItemList");
                CurrentSortItem = acSortItem;
            }
        }

        public bool IsEnabledDownSortItem()
        {
            return CurrentSortItem != null && SortItemList.Count() - 1 > SortItemList.IndexOf(CurrentSortItem);
        }

        /// <summary>Einfügen eines SortItems per DragAndDrop vom TreeView aus
        /// Wenn acSortItem != null, dann vor diesem einfügen
        /// Wenn acSortItem == null, dann am Ende einfügen</summary>
        /// <param name="dropObject"></param>
        /// <param name="acSortItem"></param>
        public void InsertSortItem(IACInteractiveObject dropObject, ACSortItem acSortItem = null)
        {
            if (!IsEnabledInsertSortItem(dropObject, acSortItem))
                return;
            ACObjectItem acObjectItem = dropObject.ACContentList.First() as ACObjectItem;
            string propertyName = acObjectItem.ACUrlRelative;

            ACSortItem newACSortItem = new ACSortItem(propertyName, Global.SortDirections.ascending, false);

            if (acSortItem == null)
            {
                SortItemList.Add(newACSortItem);
            }
            else
            {
                int index = SortItemList.IndexOf(acSortItem);
                SortItemList.Insert(index, newACSortItem);
            }
            OnPropertyChanged("SortItemList");
            CurrentSortItem = newACSortItem;
        }

        public bool IsEnabledInsertSortItem(IACInteractiveObject dropObject, ACSortItem acSortItem = null)
        {
            ACObjectItem acObjectItem = dropObject.ACContentList.First() as ACObjectItem;
            return acObjectItem != null && !string.IsNullOrEmpty(acObjectItem.ACUrlRelative);
        }
        #endregion

        #region ACColumnItem
        [ACMethodInteraction("ColumnItem", "en{'New Column'}de{'Neue Spalte'}", (short)MISort.New, false, "CurrentColumnItem")]
        public void NewColumnItem()
        {
            CurrentColumnItem = new ACColumnItem();
            CurrentQueryDefinition.ACColumns.Add(CurrentColumnItem);
            OnPropertyChanged("ColumnItemList");
        }

        [ACMethodInteraction("ColumnItem", "en{'Delete Column'}de{'Spalte löschen'}", (short)MISort.Delete, false, "CurrentColumnItem")]
        public void DeleteColumnItem()
        {
            CurrentQueryDefinition.ACColumns.Remove(CurrentColumnItem);
            OnPropertyChanged("ColumnItemList");
        }

        public bool IsEnabledDeleteColumnItem()
        {
            return CurrentColumnItem != null;
        }

        [ACMethodInteraction("ColumnItem", "en{'Up'}de{'Auf'}", (short)MISort.MoveUp, false, "CurrentColumnItem")]
        public void UpColumnItem()
        {
            int index = ColumnItemList.IndexOf(CurrentColumnItem);
            if (index > 0)
            {
                ACColumnItem acColumnItem = CurrentColumnItem;
                ColumnItemList.RemoveAt(index);
                ColumnItemList.Insert(index - 1, acColumnItem);
                OnPropertyChanged("ColumnItemList");
                CurrentColumnItem = acColumnItem;
            }
        }

        public bool IsEnabledUpColumnItem()
        {
            return CurrentColumnItem != null && ColumnItemList.IndexOf(CurrentColumnItem) > 0;
        }

        [ACMethodInteraction("ColumnItem", "en{'Down'}de{'Ab'}", (short)MISort.MoveDown, false, "CurrentColumnItem")]
        public void DownColumnItem()
        {
            int index = ColumnItemList.IndexOf(CurrentColumnItem);
            if (ColumnItemList.Count() - 1 > index)
            {
                ACColumnItem acColumnItem = CurrentColumnItem;
                ColumnItemList.RemoveAt(index);
                ColumnItemList.Insert(index + 1, acColumnItem);
                OnPropertyChanged("ColumnItemList");
                CurrentColumnItem = acColumnItem;
            }
        }

        public bool IsEnabledDownColumnItem()
        {
            return CurrentColumnItem != null && ColumnItemList.Count() - 1 > ColumnItemList.IndexOf(CurrentColumnItem);
        }

        /// <summary>Einfügen eines ColumnItems per DragAndDrop vom TreeView aus
        /// Wenn acColumnItem != null, dann vor diesem einfügen
        /// Wenn acColumnItem == null, dann am Ende einfügen</summary>
        /// <param name="dropObject"></param>
        /// <param name="acColumnItem"></param>
        public void InsertColumnItem(IACInteractiveObject dropObject, ACColumnItem acColumnItem = null)
        {
            if (!IsEnabledInsertColumnItem(dropObject, acColumnItem))
                return;
            ACObjectItem acObjectItem = dropObject.ACContentList.First() as ACObjectItem;
            string propertyName = acObjectItem.ACUrlRelative;

            ACColumnItem newACColumnItem = new ACColumnItem(propertyName);

            if (acColumnItem == null)
            {
                ColumnItemList.Add(newACColumnItem);
            }
            else
            {
                int index = ColumnItemList.IndexOf(acColumnItem);
                ColumnItemList.Insert(index, newACColumnItem);
            }
            OnPropertyChanged("ColumnItemList");
            CurrentColumnItem = newACColumnItem;
        }

        public bool IsEnabledInsertColumnItem(IACInteractiveObject dropObject, ACColumnItem acColumnItem = null)
        {
            ACObjectItem acObjectItem = dropObject.ACContentList.First() as ACObjectItem;
            return acObjectItem != null && !string.IsNullOrEmpty(acObjectItem.ACUrlRelative);
        }
        #endregion

        [ACMethodCommand("Querydialog", Const.Ok, (short)MISort.Okay)]
        public void OK()
        {
            switch (_EditMode)
            {
                case 0: // Konfigurieren
                    Result = true;
                    // Konfiguration von Arbeitskopie auf originale ACQueryDefinition kopieren
                    //CurrentQueryDefinitionOrigRoot.ConfigXML = CurrentQueryDefinitionRoot.ConfigXML;
                    CurrentQueryDefinitionOrigRoot.CopyFrom(CurrentQueryDefinitionRoot, true, true, true);
                    break;
                case 1: // Laden
                    if (CurrentLoadConfiguration == null)
                        return;
                    Result = true;
                    CurrentQueryDefinitionOrigRoot.LoadConfig(CurrentLoadConfiguration);
                    break;
                case 2: // Speichern
                    {
                        if (CurrentConfigSaveMode == null)
                            return;
                        Global.ConfigSaveModes configSaveMode = (Global.ConfigSaveModes)(short)CurrentConfigSaveMode.Value;
                        if (CurrentQueryDefinitionRoot != null)
                        {
                            //CurrentQueryDefinitionOrigRoot.ConfigXML = CurrentQueryDefinitionRoot.ConfigXML;
                            CurrentQueryDefinitionOrigRoot.CopyFrom(CurrentQueryDefinitionRoot, true, true, true);
                            Result = CurrentQueryDefinitionOrigRoot.SaveConfig(WithFilter, configSaveMode, configSaveMode == Global.ConfigSaveModes.Configurationname ? CurrentConfigurationName : "");
                        }
                    }
                    break;
            }
            CloseTopDialog();
        }

        public bool IsEnabledOK()
        {
            switch (_EditMode)
            {
                case 0: // Konfigurieren
                    return true;
                case 1: // Laden
                    return CurrentLoadConfiguration != null;
                case 2: // Speichern
                    if (CurrentConfigSaveMode == null)
                        return false;
                    switch ((Global.ConfigSaveModes)(short)CurrentConfigSaveMode.Value)
                    {
                        case Global.ConfigSaveModes.Configurationname:
                            return !string.IsNullOrEmpty(CurrentConfigurationName);
                        case Global.ConfigSaveModes.User:
                        case Global.ConfigSaveModes.Common:
                        default:
                            return true;
                    }
                default:
                    return false;
            }
        }

        [ACMethodCommand("Querydialog", Const.Cancel, (short)MISort.Cancel)]
        public void Cancel()
        {
            Result = false;
            CloseTopDialog();
        }

        [ACMethodInteraction("Querydialog", "en{'Delete Configuration'}de{'Löschen Configuration'}", (short)MISort.Delete, true, "CurrentLoadConfiguration")]
        public void DeleteConfig()
        {
            Database database = CurrentQueryDefinitionOrigRoot.TypeACClass.GetObjectContext<Database>();
            Msg msg = CurrentLoadConfiguration.DeleteACObject(database, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }
            database.ACSaveChanges();
            OnPropertyChanged("LoadConfigrationList");
        }

        public bool IsEnabledDeleteConfig()
        {
            return CurrentLoadConfiguration != null;
        }
        #endregion

        #region EventHandling
        // TODO: Remove WPF-Dependencies
#if EFCR
        [ACMethodInfo("", "en{'Key event'}de{'Tastatur Ereignis'}", 9999, false)]
        public void OnKeyEvent(KeyEventArgs e)
        {
            IVBContent control = e.Source as IVBContent;
            if (control != null && control.VBContent == "CurrentQueryDefinition\\SearchWord")
            {
                if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    OK();
                }
            }
        }
#else
        public void OnKeyEvent(object e)
        {
        }
#endif
        #endregion

        #region DragAndDrop
        /// <summary>
        /// Called from a WPF-Control inside it's ACAction-Method when a relevant interaction-event as occured (e.g. Drag-And-Drop).
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source.</param>
        public override void ACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.Drop && actionArgs.DropObject.VBContent == "CurrentAvailableProperty")
            {
                switch (targetVBDataObject.VBContent)
                {
                    case "CurrentFilterItem":
                        {
                            if (targetVBDataObject.ACContentList.Any())
                            {
                                ACFilterItem acFilterItem = targetVBDataObject.ACContentList.First() as ACFilterItem;
                                if (acFilterItem != null)
                                    InsertFilterItem(actionArgs.DropObject, acFilterItem);
                            }
                            else
                                InsertFilterItem(actionArgs.DropObject);
                        }
                        break;
                    case "CurrentSortItem":
                        {
                            if (targetVBDataObject.ACContentList.Any())
                            {
                                ACSortItem acSortItem = targetVBDataObject.ACContentList.First() as ACSortItem;
                                if (acSortItem != null)
                                    InsertSortItem(actionArgs.DropObject, acSortItem);
                            }
                            else
                                InsertSortItem(actionArgs.DropObject);
                        }
                        break;
                    case "CurrentColumnItem":
                        {
                            if (targetVBDataObject.ACContentList.Any())
                            {
                                ACColumnItem acColumnItem = targetVBDataObject.ACContentList.First() as ACColumnItem;
                                if (acColumnItem != null)
                                    InsertColumnItem(actionArgs.DropObject, acColumnItem);
                            }
                            else
                                InsertColumnItem(actionArgs.DropObject);
                        }
                        break;
                }
            }
            base.ACActionToTarget(targetVBDataObject, actionArgs);
        }

        /// <summary>
        /// Called from a WPF-Control when a relevant interaction-event as occured (e.g. Drag-And-Drop) and the related component should check if this interaction-event should be handled.
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public override bool IsEnabledACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.Move && actionArgs.DropObject.VBContent == "CurrentAvailableProperty") 
            {
                switch (targetVBDataObject.VBContent)
                {
                    case "CurrentFilterItem":
                        {
                            if (targetVBDataObject.ACContentList.Any())
                            {
                                ACFilterItem acFilterItem = targetVBDataObject.ACContentList.First() as ACFilterItem;
                                if (acFilterItem != null)
                                    return IsEnabledInsertFilterItem(actionArgs.DropObject, acFilterItem);
                            }
                            else
                                return IsEnabledInsertFilterItem(actionArgs.DropObject);
                        }
                        break;
                    case "CurrentSortItem":
                        {
                            if (targetVBDataObject.ACContentList.Any())
                            {
                                ACSortItem acSortItem = targetVBDataObject.ACContentList.First() as ACSortItem;
                                if (acSortItem != null)
                                    return IsEnabledInsertSortItem(actionArgs.DropObject, acSortItem);
                            }
                            else
                                return IsEnabledInsertSortItem(actionArgs.DropObject);
                        }
                        break;
                    case "CurrentColumnItem":
                        {
                            if (targetVBDataObject.ACContentList.Any())
                            {
                                ACColumnItem acColumnItem = targetVBDataObject.ACContentList.First() as ACColumnItem;
                                if (acColumnItem != null)
                                    return IsEnabledInsertColumnItem(actionArgs.DropObject, acColumnItem);
                            }
                            else
                                return IsEnabledInsertColumnItem(actionArgs.DropObject);
                        }
                        break;
                }
            }
            return base.IsEnabledACActionToTarget(targetVBDataObject, actionArgs);
        }
        #endregion

        #region Hilfmethoden
        public IEnumerable<IACObject> GetAvailableProperties()
        {
            List<ACObjectItem> propertyList = new List<ACObjectItem>();

            Database database = CurrentQueryDefinitionOrigRoot.TypeACClass.GetObjectContext<Database>();
            var acClass = database.ACClass.Where(c => c.AssemblyQualifiedName == CurrentQueryDefinition.QueryType.AssemblyQualifiedName).FirstOrDefault();
            if (acClass == null)
                return propertyList;
            foreach (ACClassProperty acClassProperty in acClass.Properties.Where(c=>c.SortIndex < 9999).OrderBy(c=>c.SortIndex))
            {
                if (acClassProperty.ACPropUsage == Global.ACPropUsages.Current ||
                    acClassProperty.ACPropUsage == Global.ACPropUsages.Selected ||
                    acClassProperty.ACPropUsage == Global.ACPropUsages.Property)
                {
                    string acUrlRelative = acClassProperty.ACIdentifier; 
                    ACObjectItem objectLayoutGroup = new ACObjectItem(acClassProperty, acClassProperty.ACCaption, acUrlRelative);
                    InsertACClassPropertySub(objectLayoutGroup, acClassProperty.ValueTypeACClass, acUrlRelative, 0);
                    propertyList.Add(objectLayoutGroup);
                }
            }           
            // ProcessIInsertInfo(acClass, propertyList);
            return propertyList;
        }

       

        private void InsertACClassPropertySub(ACObjectItem treeEntryRoot, ACClass acClass, string acUrlParent, int recursionDepth)
        {
            if (acClass == null)
                return;
            recursionDepth++;
            if (recursionDepth >= 2)
                return;
            foreach (ACClassProperty acClassProperty in acClass.Properties.Where(c => c.SortIndex < 9999).OrderBy(c => c.SortIndex))
            {
                ACObjectItem objectLayoutGroup = new ACObjectItem(acClassProperty, acClassProperty.ACCaption, acUrlParent + "\\" + acClassProperty.ACIdentifier);
                InsertACClassPropertySub(objectLayoutGroup, acClassProperty.ValueTypeACClass, acUrlParent + "\\" + acClassProperty.ACIdentifier, recursionDepth);
                treeEntryRoot.Add(objectLayoutGroup);
            }
        }

        //private void ProcessIInsertInfo(ACClass acClass, List<ACObjectItem> propertyList)
        //{
        //    if (acClass.Properties.Any(x => x.ACIdentifier == Const.EntityInsertDate))
        //    {                
        //        string acCaption = Root.Environment.TranslateMessage(this, "Info50018");
        //        Database database = CurrentQueryDefinitionOrigRoot.TypeACClass.GetObjectContext<Database>();
        //        ACClassProperty iinsertInfoProperty = database.ACClassProperty.Where(x => x.ACClassID == acClass.ParentACClassID && x.ACIdentifier == acClass.ACIdentifier).FirstOrDefault();
        //        ACObjectItem treeEntryRoot = new ACObjectItem(iinsertInfoProperty, acCaption, acClass.ACIdentifier + "\\" + iinsertInfoProperty.ACIdentifier);
        //        propertyList.Add(treeEntryRoot);
        //        List<string> properties = typeof(IInsertInfo).GetProperties().Select(x => x.Name).ToList();
        //        properties.AddRange(typeof(IUpdateInfo).GetProperties().Select(x => x.Name).ToList());
        //        foreach(string acIdentifier in properties)
        //        {
        //            ACClassProperty acClassProperty = acClass.Properties.FirstOrDefault(c => c.ACIdentifier == acIdentifier);
        //            if(acClassProperty != null)
        //            {
        //                ACObjectItem objectLayoutGroup = new ACObjectItem(acClassProperty, acClassProperty.ACCaption, acClassProperty.ACIdentifier);
        //                InsertACClassPropertySub(objectLayoutGroup, acClassProperty.ValueTypeACClass, acClassProperty.ACIdentifier, 1);
        //                treeEntryRoot.Add(objectLayoutGroup);
        //            }
        //        }
        //    }
        //}
        #endregion

        #region Layoutsteuerung
        public string CurrentLayout
        {
            get
            {
                string layoutXAML = LayoutHelper.DockingManagerBegin("tabControl1", "Grid.ColumnSpan=\"2\" Grid.Row=\"1\"");

                if (ShowFilter)
                    layoutXAML += LayoutHelper.DockingManagerAdd("*QueryConfigFilter", "QueryConfigFilter_0", "Filter");
                if(ShowSort)
                    layoutXAML += LayoutHelper.DockingManagerAdd("*QueryConfigSort", "QueryConfigSort_0", "Sorting");
                if(ShowColumns)
                    layoutXAML += LayoutHelper.DockingManagerAdd("*QueryConfigColumns", "QueryConfigColumns_0", "Columns");
                if (ShowStatements)
                    layoutXAML += LayoutHelper.DockingManagerAdd("*QueryStatements", "QueryStatemenets_0", "Columns");

                layoutXAML += LayoutHelper.DockingManagerEnd();

                return layoutXAML;
            }
        }
        #endregion
        #endregion

        #region Change Column Value
        public object _NewColumnValue;
        [ACPropertyInfo(100,"","en{'Change values in column'}de{'Ändere Werte in Spalte'}")]
        public object NewColumnValue
        {
            get
            {
                return _NewColumnValue;
            }
            set
            {
                _NewColumnValue = value;
                OnPropertyChanged("NewColumnValue");
            }
        }

        [ACPropertyInfo(101, "", "en{'Column name'}de{'Spaltenname'}")]
        public string ColumnName
        {
            get
            {
                if (_ACColumnChangeValue == null)
                    return "";
                if (_ChangeValuePropertyType != null)
                    return _ChangeValuePropertyType.ACCaption;
                return _ACColumnChangeValue.PropertyName;
            }
        }

        [ACPropertyInfo(102, "")]
        public string ChangeValueControlXAML
        {
            get
            {
                if (_ChangeValuePropertyType == null)
                    return "<TextBlock></TextBlock>";
                if (_ChangeValuePropertyType.ObjectType.IsAssignableFrom(typeof(DateTime)))
                {
                    return "<vb:VBDateTimePicker VBContent=\"NewColumnValue\" ShowCaption=\"false\"></vb:VBDateTimePicker>";
                }
                else if (_ChangeValuePropertyType.ObjectType.IsAssignableFrom(typeof(Boolean)))
                {
                    return "<vb:VBCheckBox VBContent=\"NewColumnValue\" ShowCaption=\"false\"></vb:VBCheckBox>";
                }
                return "<vb:VBTextBox VBContent=\"NewColumnValue\" ShowCaption=\"false\"></vb:VBTextBox>";
            }
        }

        private ACColumnItem _ACColumnChangeValue = null;
        private IAccess _ACAccessChangeValue = null;
        private ACQueryDefinition _ACQueryDefChangeValue = null;
        private ACClassProperty _ChangeValuePropertyType = null;

        [ACMethodCommand("Query", "en{'Change values in column'}de{'Ändere Werte in Spalte'}", 20)]
        public bool ChangeColumnValues(IAccess acAccess, ACColumnItem acColumn, ACQueryDefinition acQueryDef)
        {
            _ACColumnChangeValue = acColumn;
            _ACAccessChangeValue = acAccess;
            _ACQueryDefChangeValue = acQueryDef;
            if (_ACQueryDefChangeValue != null && _ACColumnChangeValue != null && _ACQueryDefChangeValue.QueryType != null)
                _ChangeValuePropertyType = _ACQueryDefChangeValue.QueryType.GetProperty(ColumnName);
            else
                _ChangeValuePropertyType = null;
            ShowDialog(this, "ChangeColumnValuesDlg");
            this.ParentACComponent.StopComponent(this);
            return Result;
        }

        [ACMethodCommand("Querydialog", Const.Ok, (short)MISort.Okay)]
        public void OKColumnValues()
        {
            if (!IsEnabledOKColumnValues())
            {
                CloseTopDialog();
                return;
            }
            if (_ACColumnChangeValue != null && _ACAccessChangeValue != null && _ChangeValuePropertyType != null)
            {
                Type typeToConvert = _ChangeValuePropertyType.ObjectFullType;
                if (typeToConvert != null)
                {
                    try
                    {
                        foreach (object row in _ACAccessChangeValue.NavObjectList)
                        {
                            if (row is IACObject)
                            {
                                Database database = CurrentQueryDefinitionOrigRoot.TypeACClass.GetObjectContext<Database>();
                                object convertedValue = ACConvert.ChangeType(NewColumnValue, typeToConvert, false, database);
                                (row as IACObject).ACUrlCommand(_ACColumnChangeValue.PropertyName, convertedValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException("VBBSOQueryDialog", "OKColumnValues", msg);
                    }
                }
            }
            CloseTopDialog();
        }

        public bool IsEnabledOKColumnValues()
        {
            if (_ACQueryDefChangeValue == null || _ACColumnChangeValue == null || _ACQueryDefChangeValue.QueryType == null || _ChangeValuePropertyType == null)
                return false;
            return true;
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"QueryConfigDlg":
                    result = QueryConfigDlg((ACQueryDefinition)acParameter[0], (Boolean)acParameter[1], (Boolean)acParameter[2], (Boolean)acParameter[3], (Boolean)acParameter[4]);
                    return true;
                case"QueryLoadDlg":
                    result = QueryLoadDlg((ACQueryDefinition)acParameter[0]);
                    return true;
                case"QuerySaveDlg":
                    result = QuerySaveDlg((ACQueryDefinition)acParameter[0]);
                    return true;
                case"NewFilterItem":
                    NewFilterItem();
                    return true;
                case"DeleteFilterItem":
                    DeleteFilterItem();
                    return true;
                case"IsEnabledDeleteFilterItem":
                    result = IsEnabledDeleteFilterItem();
                    return true;
                case"UpFilterItem":
                    UpFilterItem();
                    return true;
                case"IsEnabledUpFilterItem":
                    result = IsEnabledUpFilterItem();
                    return true;
                case"DownFilterItem":
                    DownFilterItem();
                    return true;
                case"IsEnabledDownFilterItem":
                    result = IsEnabledDownFilterItem();
                    return true;
                case"IsEnabledInsertFilterItem":
                    result = IsEnabledInsertFilterItem((IACInteractiveObject)acParameter[0], acParameter.Count() == 2 ? (ACFilterItem)acParameter[1] : null);
                    return true;
                case"NewSortItem":
                    NewSortItem();
                    return true;
                case"DeleteSortItem":
                    DeleteSortItem();
                    return true;
                case"IsEnabledDeleteSortItem":
                    result = IsEnabledDeleteSortItem();
                    return true;
                case"UpSortItem":
                    UpSortItem();
                    return true;
                case"IsEnabledUpSortItem":
                    result = IsEnabledUpSortItem();
                    return true;
                case"DownSortItem":
                    DownSortItem();
                    return true;
                case"IsEnabledDownSortItem":
                    result = IsEnabledDownSortItem();
                    return true;
                case"IsEnabledInsertSortItem":
                    result = IsEnabledInsertSortItem((IACInteractiveObject)acParameter[0], acParameter.Count() == 2 ? (ACSortItem)acParameter[1] : null);
                    return true;
                case"NewColumnItem":
                    NewColumnItem();
                    return true;
                case"DeleteColumnItem":
                    DeleteColumnItem();
                    return true;
                case"IsEnabledDeleteColumnItem":
                    result = IsEnabledDeleteColumnItem();
                    return true;
                case"UpColumnItem":
                    UpColumnItem();
                    return true;
                case"IsEnabledUpColumnItem":
                    result = IsEnabledUpColumnItem();
                    return true;
                case"DownColumnItem":
                    DownColumnItem();
                    return true;
                case"IsEnabledDownColumnItem":
                    result = IsEnabledDownColumnItem();
                    return true;
                case"IsEnabledInsertColumnItem":
                    result = IsEnabledInsertColumnItem((IACInteractiveObject)acParameter[0], acParameter.Count() == 2 ? (ACColumnItem)acParameter[1] : null);
                    return true;
                case"OK":
                    OK();
                    return true;
                case"IsEnabledOK":
                    result = IsEnabledOK();
                    return true;
                case"Cancel":
                    Cancel();
                    return true;
                case"DeleteConfig":
                    DeleteConfig();
                    return true;
                case"IsEnabledDeleteConfig":
                    result = IsEnabledDeleteConfig();
                    return true;
                case"OnKeyEvent":
                    //OnKeyEvent((KeyEventArgs)acParameter[0]);
                    OnKeyEvent(acParameter[0]);
                    return true;
                case"IsEnabledACActionToTarget":
                    result = IsEnabledACActionToTarget((IACInteractiveObject)acParameter[0], (ACActionArgs)acParameter[1]);
                    return true;
                case"ChangeColumnValues":
                    result = ChangeColumnValues((IAccess)acParameter[0], (ACColumnItem)acParameter[1], (ACQueryDefinition)acParameter[2]);
                    return true;
                case"OKColumnValues":
                    OKColumnValues();
                    return true;
                case"IsEnabledOKColumnValues":
                    result = IsEnabledOKColumnValues();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}

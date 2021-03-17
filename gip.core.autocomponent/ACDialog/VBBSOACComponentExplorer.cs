using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Components Explorer'}de{'Komponenten Explorer'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class VBBSOACComponentExplorer : VBBSOSelectionDependentDialog
    {
        #region c'tors

        public VBBSOACComponentExplorer(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_CurrentACComponent != null)
            {
                _CurrentACComponent.Detach();
                _CurrentACComponent = null;
            }

            _InstanceInfoList = null;
            _CurrentInstanceInfo = null;

            _CurrentACMember = null;
            _ACMemberLiveList = null;
            _PropertyValue = null;

            _CurrentInstanceInfo = null;
            _InstanceInfoList = null;

            _CurrentACPropertyLive = null;

            if (_AccessOnlineValue != null)
                _AccessOnlineValue.ACDeInit();
            _AccessOnlineValue = null;

            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties

        [ACPropertyInfo(999,"","en{'ACUrl'}de{'ACUrl'}")]
        public string ACUrlInfo
        { 
            get
            {
                if (_RootACComponent == null)
                    return "";
                return _RootACComponent.GetACUrl();
            }
            set
            {
            }
        }

        #region Search

        private string _ACClassSearch;
        [ACPropertyInfo(999, "", "en{'Search over class name'}de{'Suche nach Klassennamen'}")]
        public string ACClassSearch
        {
            get
            {
                return _ACClassSearch;
            }
            set
            {
                _ACClassSearch = value;
                OnPropertyChanged("ACClassSearch");
            }
        }

        private string _ACIdentifierSearch;
        [ACPropertyInfo(999, "", "en{'Search over instance name'}de{'Suche nach Instanzname'}")]
        public string ACIdentifierSearch
        {
            get
            {
                return _ACIdentifierSearch;
            }
            set
            {
                _ACIdentifierSearch = value;
                OnPropertyChanged("ACIdentifierSearch");
            }
        }

        private string _SearchDepth = "2";
        [ACPropertyInfo(999, "", "en{'Max. search depth'}de{'Max. Suchtiefe'}")]
        public string SearchDepth
        {
            get
            {
                return _SearchDepth;
            }
            set
            {
                _SearchDepth = value;
            }
        }

        private string _PropertyNameSearch;
        [ACPropertyInfo(999, "", "en{'Search over property name'}de{'Suche nach Eigenschaftsname'}")]
        public string PropertyNameSearch
        {
            get
            {
                return _PropertyNameSearch;
            }
            set
            {
                _PropertyNameSearch = value;
                OnPropertyChanged("PropertyNameSearch");
            }
        }

        private bool _WorkflowNodesShow = false;
        [ACPropertyInfo(999, "", "en{'Show workflow nodes'}de{'Workflowinstanzen anzeigen'}")]
        public bool WorkflowNodesShow
        {
            get
            {
                return _WorkflowNodesShow;
            }
            set
            {
                if (_WorkflowNodesShow != value)
                {
                    _WorkflowNodesShow = value;
                    _UpdateInstanceInfoList = true;
                    OnPropertyChanged("WorkflowNodesShow");
                }
            }
        }

        private bool SearchByProp
        {
            get
            {
                if (string.IsNullOrEmpty(PropertyNameSearch))
                    return false;
                return true;
            }
        }

        #endregion

        //private bool _ShowChilds;
        public override string CurrentLayout
        {
            get { return "ACComponentExplorer"; }
        }

        private ACComponent _RootACComponent;

        private ACRef<IACComponent> _CurrentACComponent;
        public IACComponent CurrentACComponent
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
                        _CurrentACComponent.Detach();
                    }
                    else
                        objectSwapped = false;
                }
                if (value == null)
                    _CurrentACComponent = null;
                else
                    _CurrentACComponent = new ACRef<IACComponent>(value, this);

                OnPropertyChanged("CurrentACComponent");
                if (objectSwapped)
                    OnPropertyChanged("ACPropertyLiveList");
            }
        }


        private ACChildInstanceInfo _CurrentInstanceInfo;
        [ACPropertyCurrent(999, "InstanceInfo", "en{'Instance informations'}de{'Instanzinformationen'}")]
        public ACChildInstanceInfo CurrentInstanceInfo
        { 
            get
            {
                return _CurrentInstanceInfo;
            }
            set
            {
                if (_CurrentInstanceInfo != value)
                {
                    _CurrentInstanceInfo = value;
                    if (CurrentInstanceInfo == null)
                        CurrentACComponent = null;
                    else
                        CurrentACComponent = ACUrlCommand(_CurrentInstanceInfo.ACUrlParent + "\\" + _CurrentInstanceInfo.ACIdentifier) as ACComponent;
                    OnPropertyChanged("CurrentInstanceInfo");
                }
            }
        }

        private IEnumerable<ACChildInstanceInfo> _InstanceInfoList;
        [ACPropertyList(999, "InstanceInfo", "en{'List with instance informations'}de{'Liste mit Instanzinformationen'}")]
        public IEnumerable<ACChildInstanceInfo> InstanceInfoList
        {
            get
            {
                return _InstanceInfoList;
            }
            set
            {
                _InstanceInfoList = value;
                OnPropertyChanged("InstanceInfoList");
            }
        }

        private IACMember _CurrentACPropertyLive;
        [ACPropertyCurrent(999, "ACProperty", "en{'Current property'}de{'Aktuelle Eigenschaft'}")]
        public IACMember CurrentACPropertyLive
        {
            get
            {
                return _CurrentACPropertyLive;
            }
            set
            {
                _CurrentACPropertyLive = value;
                OnPropertyChanged("CurrentACProperty");
            }
        }

        ACAccess<IACMember> _AccessOnlineValue;
        [ACPropertyAccess(9999, "ACProperty")]
        public ACAccess<IACMember> AccessOnlineValue
        {
            get
            {
                if (_AccessOnlineValue == null && ACType != null)
                {
                    ACQueryDefinition acQueryDefinition = Root.Queries.CreateQuery(null, Const.QueryPrefix + "ACMember", Const.QueryPrefix + "ACMember");
                    _AccessOnlineValue = acQueryDefinition.NewAccess<IACMember>("ACProperty", this);
                }
                return _AccessOnlineValue;
            }
        }

        //private IEnumerable<IACMember> _ACPropertyLiveList;
        [ACPropertyList(999, "ACProperty", "en{'Propertylist'}de{'Eigenschaftsliste'}")]
        public IEnumerable<IACMember> ACPropertyLiveList
        {
            get
            {
                if (CurrentACComponent == null)
                    return null;
                List<IACMember> copiedList = null;

                using (ACMonitor.Lock(CurrentACComponent.LockMemberList_20020))
                {
                    copiedList = CurrentACComponent.ACMemberList.ToList();
                }

                // Abfrage erst später auf Liste, damit es zu keinem Deadlock kommt.
                //return copiedList.AsParallel().Where(c => !string.IsNullOrEmpty(c.ACCaption)
                return copiedList.Where(c => !string.IsNullOrEmpty(c.ACCaption)
                                                                        && c.ACIdentifier != "ACDiagnoseInfo"
                                                                        && c.ACIdentifier != "ACDiagnoseXMLDoc"
                                                                        && c is IACPropertyNetBase
                                                                        && ((c.Value != null && (c.Value is IConvertible || c.Value.GetType().IsEnum || c.Value is IFormattable))
                                                                            || (c.Value == null && (typeof(IConvertible).IsAssignableFrom(c.ACType.ObjectType) || c.ACType.ObjectType.IsEnum || typeof(IFormattable).IsAssignableFrom(c.ACType.ObjectType)))
                                                                            )
                                                                        ).OrderBy(c => c.ACCaption).ToList();
            }
        }

        List<ACChildInstanceInfo> tempList = null;

        bool _UpdateInstanceInfoList;

#endregion

#region Methods

        public override void ShowSelectionDialog(IACInteractiveObject acElement)
        {
            //UpdateACComponentList();            
            ShowDialogForComponent(this);
        }

        [ACMethodInfo("","en{'Search'}de{'Suchen'}",999)]
        public void Search()
        {
            if(searchDepth != int.Parse(SearchDepth) || _UpdateInstanceInfoList)
            {
                searchDepth = int.Parse(SearchDepth);
                UpdateInstanceInfoList();
                _UpdateInstanceInfoList = false;
            }

            if (!string.IsNullOrEmpty(ACIdentifierSearch) && string.IsNullOrEmpty(ACClassSearch))
                InstanceInfoList = tempList.Where(c => c.ACIdentifier.ToLower().Contains(ACIdentifierSearch.ToLower()));

            else if (!string.IsNullOrEmpty(ACClassSearch) && string.IsNullOrEmpty(ACIdentifierSearch))
                InstanceInfoList = tempList.Where(c => ((ACClass)c.ACType.Value).InheritedASQN.ToLower().Contains(ACClassSearch.ToLower()));

            else if (!string.IsNullOrEmpty(ACClassSearch) && !string.IsNullOrEmpty(ACIdentifierSearch))
                InstanceInfoList = tempList.Where(c => c.ACIdentifier.ToLower().Contains(ACIdentifierSearch.ToLower())
                                                  && ((ACClass)c.ACType.Value).InheritedASQN.ToLower().Contains(ACClassSearch.ToLower()));
            else
                InstanceInfoList = tempList;
        }

        public bool IsEnabledSearch()
        {
            if (_RootACComponent == null || _RootACComponent.ConnectionState == ACObjectConnectionState.DisConnected)
                return false;
            return true;
        }

        public override void OnSelectionChanged()
        {
            InstanceInfoList = null;
            ACMemberLiveList = null;
            PropertyValue = null;
            _RootACComponent = SelectionManager.SelectedACObject as ACComponent;
            _UpdateInstanceInfoList = true;
            OnPropertyChanged("ACUrlInfo");
            base.OnSelectionChanged();
        }

        int searchDepth;

        private void UpdateInstanceInfoList(string searchPropertyName = "")
        {
            searchDepth = int.Parse(SearchDepth);
            var root = _RootACComponent.ParentACComponent?.GetChildInstanceInfo(1, new ChildInstanceInfoSearchParam() { ACIdentifier = _RootACComponent.ACIdentifier, 
                                                                                                                        ContainsPropertyName = searchPropertyName, WithWorkflows = WorkflowNodesShow });
            tempList = _RootACComponent.GetChildInstanceInfo(searchDepth, new ChildInstanceInfoSearchParam() { ReturnAsFlatList = true, ContainsPropertyName = searchPropertyName,
                                                                                                                WithWorkflows = WorkflowNodesShow}).ToList();
            if (root != null && root.Any())
                tempList.Insert(0, root.FirstOrDefault());
        }

#endregion

#region PropertySearch

        List<ACMemberWrapper> acMemberList = new List<ACMemberWrapper>();

        private ACMemberWrapper _CurrentACMember;
        [ACPropertyCurrent(999,"ACMemberWrapper")]
        public ACMemberWrapper CurrentACMember
        {
            get
            {
                return _CurrentACMember;
            }
            set
            {
                if (_CurrentACMember != value)
                {
                    _CurrentACMember = value;
                    if (_CurrentACMember != null)
                    {
                        ACComponent component = ACUrlCommand(_CurrentACMember.ACUrl) as ACComponent;
                        if (component != null)
                        {
                            PropertyValue = component.ACMemberList.FirstOrDefault(c => c.ACIdentifier == _CurrentACMember.ACMember.ACIdentifier);
                        }
                    }
                    OnPropertyChanged("CurrentACMember");
                }
            }
        }

        private IEnumerable<ACMemberWrapper> _ACMemberLiveList;
        [ACPropertyList(999,"ACMemberWrapper")]
        public IEnumerable<ACMemberWrapper> ACMemberLiveList
        {
            get
            {
                return _ACMemberLiveList;
            }
            set
            {
                _ACMemberLiveList = value;
                OnPropertyChanged("ACMemberLiveList");
            }
        }

        [ACMethodInfo("","en{'Search'}de{'Suchen'}",999)]
        public void SearchProperties()
        {
            if (string.IsNullOrEmpty(PropertyNameSearch))
                ACMemberLiveList = null;
            else
            {
                searchDepth = int.Parse(SearchDepth);
                UpdateInstanceInfoList(PropertyNameSearch);
                acMemberList.Clear();
                foreach (ACChildInstanceInfo instanceInfo in tempList)
                    if (instanceInfo.MemberValues != null && instanceInfo.MemberValues.Any())
                        foreach (ACValueWithCaption property in instanceInfo.MemberValues)
                            acMemberList.Add(new ACMemberWrapper() { ChildInstanceInfo = instanceInfo, ACMember = property});
                
                ACMemberLiveList = acMemberList.ToList();
            }
        }

        public bool IsEnabledSearchProperties()
        {
            if (_RootACComponent == null || _RootACComponent.ConnectionState == ACObjectConnectionState.DisConnected)
                return false;
            return true;
        }

        private IACMember _PropertyValue;
        [ACPropertyInfo(999,"","en{'Value'}de{'Wert'}")]
        public IACMember PropertyValue
        { 
            get
            {
                return _PropertyValue;
            }
            set
            {
                _PropertyValue = value;
                OnPropertyChanged("PropertyValue");
            }
        }

        [ACMethodInfo("","en{'Update'}de{'Aktualisieren'}",999)]
        public void UpdatePropertyValue()
        {
             CurrentACMember.ACMember.Value = PropertyValue.Value;
        }

        public bool IsEnabledUpdatePropertyValue()
        {
            if (CurrentACMember != null && CurrentACMember.ACMember != null && PropertyValue != null)
                return true;
            return false;
        }

        [ACMethodInfo("", "en{'Mass update'}de{'Massenaktualisierung'}", 999)]
        public void MassUpdatePropertyValue()
        { 
            foreach (ACMemberWrapper wrapper in ACMemberLiveList.Where(c => c.IsForUpdate))
            {
                IACComponent component = ACUrlCommand(wrapper.ACUrl) as IACComponent;
                if (component != null)
                {
                    component.ACMemberList.FirstOrDefault(c => c.ACIdentifier == wrapper.ACMember.ACIdentifier).Value = PropertyValue.Value;
                    wrapper.ACMember.Value = PropertyValue.Value;
                    wrapper.IsForUpdate = false;
                }
            }
        }

        public bool IsEnabledMassUpdatePropertyValue()
        {
            if (ACMemberLiveList == null)
                return false;
            Type type = null;
            foreach(ACMemberWrapper wrapper in ACMemberLiveList)
            {
                if (type != null && type != wrapper.ACMember.ObjectType)
                    return false;
                type = wrapper.ACMember.ObjectType;
            }
            return true;
        }

        [ACMethodInfo("", "en{'Select all'}de{'Alle auswählen'}", 999)]
        public void CheckAllProp()
        {
            foreach (ACMemberWrapper wrapper in ACMemberLiveList.Where(c => !c.IsForUpdate))
                wrapper.IsForUpdate = true;
        }

        public bool IsEnabledCheckAllProp()
        {
            if (ACMemberLiveList == null)
                return false;
            return true;
        }

        [ACMethodInfo("", "en{'Unselect all'}de{'Alle abwählen'}", 999)]
        public void UnCheckAllProp()
        {
            foreach (ACMemberWrapper wrapper in ACMemberLiveList.Where(c => c.IsForUpdate))
                wrapper.IsForUpdate = false;
        }

        public bool IsEnabledUnCheckAllProp()
        {
            if (ACMemberLiveList == null)
                return false;
            return true;
        }

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
                case "SearchProperties":
                    SearchProperties();
                    return true;
                case "UpdatePropertyValue":
                    UpdatePropertyValue();
                    return true;
                case "MassUpdatePropertyValue":
                    MassUpdatePropertyValue();
                    return true;
                case "CheckAllProp":
                    CheckAllProp();
                    return true;
                case "UnCheckAllProp":
                    UnCheckAllProp();
                    return true;
                case Const.IsEnabledPrefix + "Search":
                    result = IsEnabledSearch();
                    return true;
                case Const.IsEnabledPrefix + "SearchProperties":
                    result = IsEnabledSearchProperties();
                    return true;
                case Const.IsEnabledPrefix + "UpdatePropertyValue":
                    result = IsEnabledUpdatePropertyValue();
                    return true;
                case Const.IsEnabledPrefix + "MassUpdatePropertyValue":
                    result = IsEnabledMassUpdatePropertyValue();
                    return true;
                case Const.IsEnabledPrefix + "CheckAllProp":
                    result = IsEnabledCheckAllProp();
                    return true;
                case Const.IsEnabledPrefix + "UnCheckAllProp":
                    result = IsEnabledUnCheckAllProp();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
#endregion

    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMember wrapper'}de{'ACMember Wrapper'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACMemberWrapper : INotifyPropertyChanged
    {
        public ACMemberWrapper()
        {

        }

        [ACPropertyInfo(999,"","en{'Property'}de{'Eigenschaft'}")]
        public ACValueWithCaption ACMember
        {
            get;
            set;
        }

        [ACPropertyInfo(999, "", "en{'Name'}de{'Name'}")]
        public string ACMemberCaption
        { 
            get
            {
                return Translator.GetTranslation(ACMember.ACCaptionTranslation);
            }
        }

        [ACPropertyInfo(999,"","en{'Component'}de{'Component'}")]
        public ACChildInstanceInfo ChildInstanceInfo
        {
            get;
            set;
        }

        [ACPropertyInfo(999, "", "en{'ACUrl'}de{'ACUrl'}")]
        public string ACUrl
        {
            get
            {
                return ChildInstanceInfo.ACUrlParent+"\\"+ChildInstanceInfo.ACIdentifier;
            }
            set
            {

            }
        }

        private bool _IsForUpdate = false;
        [ACPropertyInfo(999,"","en{'Mass update'}de{'Massenaktualisierung'}")]
        public bool IsForUpdate
        {
            get
            {
                return _IsForUpdate;
            }
            set
            {
                _IsForUpdate = value;
                OnPropertyChanged("IsForUpdate");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

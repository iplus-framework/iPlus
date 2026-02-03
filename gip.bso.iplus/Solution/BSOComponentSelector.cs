using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Component selector'}de{'Komponentenauswahl'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class BSOComponentSelector : ACBSO
    {
        #region c'tors

        public BSOComponentSelector(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") : 
            base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool init = base.ACInit(startChildMode);
            if (ParentACComponent != null)
            {
                _VisibilityFilter = ParentACComponent.GetValue(ParentFilterPropertyName) as ACClassInfoWithItems.VisibilityFilters;

                List<Global.ACProjectTypes> projectTypes = ParentACComponent.GetValue(ParentProjectFilterPropertyName) as List<Global.ACProjectTypes>;
                if (projectTypes != null && projectTypes.Any())
                    _ProjectFilterTypes = projectTypes.ToArray();
                else
                    _ProjectFilterTypes = new Global.ACProjectTypes[2] { Global.ACProjectTypes.AppDefinition, Global.ACProjectTypes.Application };
            }
            return init;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if(_ACProjectManager != null)
                _ACProjectManager.PropertyChanged -= _ACProjectManager_PropertyChanged;
            _ACProjectManager = null;
            _CurrentACProject = null;
            _CurrentProjectItemCS = null;
            _CurrentProjectItemRootChangeInfo = null;
            bool done = await base.ACDeInit(deleteACClassTask);
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

        #region private members

        ACProjectManager _ACProjectManager;
        public ACProjectManager ProjectManager
        {
            get
            {
                if (_ACProjectManager != null)
                    return _ACProjectManager;
                _ACProjectManager = new ACProjectManager(Db, Root);
                _ACProjectManager.PropertyChanged += _ACProjectManager_PropertyChanged;
                return _ACProjectManager;
            }
        }

        private void _ACProjectManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ACProjectManager.CurrentProjectItemRootPropName)
                OnPropertyChanged("CurrentProjectItemRoot");
        }


        #endregion

        #region ACProject

        public const string CurrentACProjectPropName = "CurrentACProject";

        /// <summary>
        /// The _ current AC project
        /// </summary>
        gip.core.datamodel.ACProject _CurrentACProject;
        /// <summary>
        /// Gets or sets the current AC project.
        /// </summary>
        /// <value>The current AC project.</value>
        [ACPropertyCurrent(401, "ACProject")]
        public gip.core.datamodel.ACProject CurrentACProject
        {
            get
            {
                return _CurrentACProject;
            }
            set
            {
                if (value != null && VisibilityFilter != null)
                {
                    _CurrentACProject = ProjectManager.LoadACProject(value.ACProjectID, PresentationMode, VisibilityFilter, ProjectCheckHandler);

                    if (!IsEnabledLazyLoadOnProjectTree)
                        CheckIcons();
                }
                else
                {
                    _CurrentACProject = null;
                    ProjectManager.UnloadACProject();
                }

                OnPropertyChanged(CurrentACProjectPropName);
            }
        }

        /// <summary>
        /// Gets the AC project list.
        /// </summary>
        /// <value>The AC project list.</value>
        [ACPropertyList(402, "ACProject")]
        public IEnumerable<ACProject> ACProjectList
        {
            get
            {
                var projectlist = Db.ACProject
                    .ToArray()
                    .Where(c => _ProjectFilterTypes.Any(x => c.ACProjectTypeIndex == (short)x))
                    .OrderBy(c => c.ACIdentifier);
                return projectlist;
                //return Database.ContextIPlus.ACProject.Where(c => c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Appdefinitionproject
                //                                               || c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Applicationproject);
            }
        }

        /// <summary>
        /// Gets the current project item root.
        /// </summary>
        /// <value>The current project item root.</value>
        [ACPropertyCurrent(403, "ProjectItemRoot")]
        public ACClassInfoWithItems CurrentProjectItemRoot
        {
            get
            {
                return ProjectManager.CurrentProjectItemRoot;
            }
        }

        /// <summary>
        /// The _ current project item root change info
        /// </summary>
        ChangeInfo _CurrentProjectItemRootChangeInfo = null;
        /// <summary>
        /// Gets or sets the current project item root change info.
        /// </summary>
        /// <value>The current project item root change info.</value>
        [ACPropertyChangeInfo(404, "ProjectItem")]
        public ChangeInfo CurrentProjectItemRootChangeInfo
        {
            get
            {
                return _CurrentProjectItemRootChangeInfo;
            }
            set
            {
                _CurrentProjectItemRootChangeInfo = value;
                OnPropertyChanged("CurrentProjectItemRootChangeInfo");
            }
        }

        public const string CurrentProjectItemPropName = "CurrentProjectItemCS";

        /// <summary>
        /// The _ current project item
        /// </summary>
        ACClassInfoWithItems _CurrentProjectItemCS = null;
        /// <summary>
        /// Gets or sets the current project item.
        /// </summary>
        /// <value>The current project item.</value>
        [ACPropertyCurrent(405, "ProjectItem")]
        public ACClassInfoWithItems CurrentProjectItemCS
        {
            get
            {
                return _CurrentProjectItemCS;
            }
            set
            {
                if (_CurrentProjectItemCS != value)
                {
                    _CurrentProjectItemCS = value;
                    if (_CurrentProjectItemCS != null && _CurrentProjectItemCS.ValueT != null)
                        ParentACComponent.ACUrlCommand("!SetACClassInfoWithItems", _CurrentProjectItemCS);
                    OnPropertyChanged("CurrentProjectItemCS");
                }
            }
        }

        protected ACProjectManager.PresentationMode PresentationMode
        {
            get
            {
                return new ACProjectManager.PresentationMode()
                    {
                        ShowCaptionInTree = this.WithCaption,
                    };
            }
        }

        private ACClassInfoWithItems.VisibilityFilters _VisibilityFilter;
        public ACClassInfoWithItems.VisibilityFilters VisibilityFilter
        {
            get
            {
                ACClassInfoWithItems.VisibilityFilters visibilityFiter = _VisibilityFilter != null ? 
                    _VisibilityFilter.Clone() as ACClassInfoWithItems.VisibilityFilters 
                    : new ACClassInfoWithItems.VisibilityFilters();
                visibilityFiter.SearchText = SearchClassText;
                return visibilityFiter;
            }
        }

        private Global.ACProjectTypes[] _ProjectFilterTypes;
        public Global.ACProjectTypes[] ProjectFilterTypes
        {
            set
            {
                _ProjectFilterTypes = value;
                OnPropertyChanged("ACProjectList");
            }
        }

        public ACClassInfoWithItems.CheckHandler ProjectCheckHandler
        {
            get;
            set;
        }


        protected ACChildInstanceInfo _SelectedChildInstanceInfo;
        [ACPropertySelected(602, "ChildInstanceInfo")]
        public ACChildInstanceInfo SelectedChildInstanceInfo
        {
            get
            {
                return _SelectedChildInstanceInfo;
            }
            set
            {
                if (_SelectedChildInstanceInfo != value)
                {
                    _SelectedChildInstanceInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        protected ACChildInstanceInfo _UserSelectedChildInstanceInfo;

        private IEnumerable<ACChildInstanceInfo> _ChildInstanceInfoList;
        [ACPropertyList(603, "ChildInstanceInfo")]
        public IEnumerable<ACChildInstanceInfo> ChildInstanceInfoList
        {
            get
            {
                return _ChildInstanceInfoList;
            }
            set
            {
                _ChildInstanceInfoList = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region TreeItemIcon

        private void CheckIcons()
        {
            ParentACComponent.ACUrlCommand("!CheckIcons", CurrentProjectItemRoot, true);
        }

        #endregion

        #region Properties

        private string _ParentFilterPropertyName = "ComponentTypeFilter";
        [ACPropertyInfo(406)]
        public string ParentFilterPropertyName
        {
            get 
            {
                return _ParentFilterPropertyName;
            }
            set
            {
                _ParentFilterPropertyName = value;
                OnPropertyChanged("ParentFilterPropertyName");
            }
        }

        private string _ParentProjectFilterPropertyName = "ProjectTypeFilter";
        [ACPropertyInfo(407)]
        public string ParentProjectFilterPropertyName
        {
            get { return _ParentProjectFilterPropertyName; }
            set
            {
                _ParentProjectFilterPropertyName = value;
                OnPropertyChanged("ParentProjectFilterName");
            }
        }

        private bool _IsEnabledLazyLoadOnProjectTree = false;
        [ACPropertyInfo(408)]
        public bool IsEnabledLazyLoadOnProjectTree
        {
            get { return _IsEnabledLazyLoadOnProjectTree; }
            set
            {
                _IsEnabledLazyLoadOnProjectTree = value;
                OnPropertyChanged("IsEnabledLazyLoadOnProjectTree");
            }
        }

        /// <summary>
        /// The _ search class text
        /// </summary>
        string _SearchClassText = "";
        /// <summary>
        /// Gets or sets the search class text.
        /// </summary>
        /// <value>The search class text.</value>
        [ACPropertyInfo(409, "", "en{'Search Class'}de{'Suche Klasse'}")]
        public string SearchClassText
        {
            get
            {
                return _SearchClassText;
            }
            set
            {
                if (_SearchClassText != value)
                {
                    _SearchClassText = value;
                    OnPropertyChanged("SearchClassText");
                    RefreshProjectTree(true);
                }
            }
        }

        /// <summary>
        /// The _ with caption
        /// </summary>
        bool _WithCaption = false;
        /// <summary>
        /// Gets or sets a value indicating whether [with caption].
        /// </summary>
        /// <value><c>true</c> if [with caption]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(410, "TreeConfig", "en{'With Caption'}de{'Mit Bezeichnung'}")]
        public bool WithCaption
        {
            get
            {
                return _WithCaption;
            }
            set
            {
                _WithCaption = value;
                OnPropertyChanged("WithCaption");
                RefreshProjectTree();
            }
        }


        [ACPropertyInfo(411, DefaultValue = false)]
        public bool IsSelectButtonVisible
        {
            get;
            set;
        }

        #endregion

        #region Methods

        [ACMethodInfo("","en{'Show component selector'}de{'Show component selector'}",490)]
        public ACClass ShowComponentSelector(ACClassInfoWithItems.VisibilityFilters visibilityFilter, string filterProjectACIdentifer, string filterCompACIdentifier)
        {
            IsSelectButtonVisible = true;
            _VisibilityFilter = visibilityFilter;
            if (!String.IsNullOrEmpty(filterProjectACIdentifer))
            {
                var project = this.ACProjectList.Where(c => c.ACIdentifier == filterProjectACIdentifer).FirstOrDefault();
                if (project != null)
                    this.CurrentACProject = project;
            }
            if (!String.IsNullOrEmpty(filterCompACIdentifier))
            {
                SearchClassText = filterCompACIdentifier;
                SearchClass();
            }
            ShowDialog(this, "Mainlayout");
            IsSelectButtonVisible = false;
            if (CurrentProjectItemCS == null)
                return null;
            return CurrentProjectItemCS.ValueT;
        }

        [ACMethodInfo("","en{'Select component'}de{'Komponente übernehmen'}",401)]
        public void SelectComponent()
        {
            CloseTopDialog();
        }

        [ACMethodInfo("", "en{'Cancel'}de{'Abbruch'}", 402)]
        public void CancelSelectComponent()
        {
            CurrentProjectItemCS = null;
            CloseTopDialog();
        }

        [ACMethodCommand("TreeViewItemExpand", "en{'TVIExpand'}de{'TVIExpand'}", 491, false)]
        public void OnTreeViewItemExpand(ACClassInfoWithItems acClassInfoWithItems)
        {
            if (!IsEnabledLazyLoadOnProjectTree)
                return;

            if (acClassInfoWithItems.Items.Any())
            {
                ACClassInfoWithItems childInfo = acClassInfoWithItems.Items.FirstOrDefault() as ACClassInfoWithItems;
                if (childInfo != null && childInfo.ValueT == acClassInfoWithItems.ValueT)
                {
                    ProjectManager.ExpandProjectItem(acClassInfoWithItems);
                    ParentACComponent.ACUrlCommand("!CheckIcons", acClassInfoWithItems, true);
                }
            }
        }

        [ACMethodInfo("", "en{'Search'}de{'Suche'}", 403)]
        public void SearchClass()
        {
            RefreshProjectTree(true);
            OnPropertyChanged("CurrentProjectItemRoot");
        }

        private void RefreshProjectTree(bool forceRebuildTree = false)
        {
            ProjectManager.RefreshProjectTree(PresentationMode, VisibilityFilter, null, forceRebuildTree);
            CheckIcons();
        }

        public bool IsEnabledSearchClass()
        {
            if (!string.IsNullOrEmpty(SearchClassText) && CurrentACProject != null)
                return true;
            return false;
        }

        [ACMethodInfo("","en{'Refresh'}de{'Refresh'}",404,true)]
        public void RefreshItems()
        {
            CheckIcons();
        }


        [ACMethodInfo("", "en{'Show component selector'}de{'Show component selector'}", 490)]
        public ACChildInstanceInfo ShowChildInstanceInfo(IEnumerable<ACChildInstanceInfo> childInstanceInfoList)
        {
            _UserSelectedChildInstanceInfo = null;
            this.ChildInstanceInfoList = childInstanceInfoList;
            ShowDialog(this, "ACChildInstanceInfo");
            return _UserSelectedChildInstanceInfo;
        }

        [ACMethodInfo("", "en{'Use process module'}de{'Prozessmodul übernehmen'}", 491)]
        public void SelectChildInstanceInfo()
        {
            _UserSelectedChildInstanceInfo = SelectedChildInstanceInfo;
            CloseTopDialog();
        }

        [ACMethodInfo("", "en{'Cancel'}de{'Abbruch'}", 492)]
        public void CancelChildInstanceInfo()
        {
            _UserSelectedChildInstanceInfo = null;
            SelectedChildInstanceInfo = null;
            CloseTopDialog();
        }


        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            var cm = base.OnGetControlModes(vbControl);
            if (vbControl != null && !string.IsNullOrEmpty(vbControl.VBContent))
            {
                switch (vbControl.VBContent)
                {
                    case "IsSelectButtonVisible":
                        if (IsSelectButtonVisible)
                            cm = Global.ControlModes.Enabled;
                        else
                            cm = Global.ControlModes.Collapsed;
                        break;
                }
            }
            return cm;
        }
        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(ShowComponentSelector):
                    result = ShowComponentSelector((ACClassInfoWithItems.VisibilityFilters)acParameter[0], acParameter[1] as string, acParameter[2] as string);
                    return true;
                case nameof(SelectComponent):
                    SelectComponent();
                    return true;
                case nameof(CancelSelectComponent):
                    CancelSelectComponent();
                    return true;
                case nameof(OnTreeViewItemExpand):
                    OnTreeViewItemExpand((ACClassInfoWithItems)acParameter[0]);
                    return true;
                case nameof(SearchClass):
                    SearchClass();
                    return true;
                case nameof(IsEnabledSearchClass):
                    result = IsEnabledSearchClass();
                    return true;
                case nameof(RefreshItems):
                    RefreshItems();
                    return true;
                case nameof(ShowChildInstanceInfo):
                    result = ShowChildInstanceInfo(acParameter[0] as IEnumerable<ACChildInstanceInfo>);
                    return true;
                case nameof(SelectChildInstanceInfo):
                    SelectChildInstanceInfo();
                    return true;
                case nameof(CancelChildInstanceInfo):
                    CancelChildInstanceInfo();
                    return true;
            }
           return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}

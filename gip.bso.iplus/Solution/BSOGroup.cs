// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="BSOGroup.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOGroup
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Group Management'}de{'Gruppenverwaltung'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + VBGroup.ClassName)]
    public class BSOGroup : ACBSONav
    {
        #region c´tors
        /// <summary>
        /// The _ AC project manager
        /// </summary>
        ACProjectManager _ACProjectManager;
        /// <summary>
        /// Initializes a new instance of the <see cref="BSOGroup"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOGroup(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        /// <summary>
        /// ACs the init.
        /// </summary>
        /// <param name="startChildMode">The start child mode.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            _CurrentGroupMode = (ACObjectItem)GroupModeList.FirstOrDefault();
            Search();
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            //this._ChangedGroupRight = null;
            this._CurrentACProject = null;
            this._CurrentProjectItem = null;
            this._CurrentRightItemInfoClassDesign = null;
            this._CurrentRightItemInfoClassMethod = null;
            this._CurrentRightItemInfoClassProperty = null;
            //this._GroupRightList = null;
            this._RightItemInfoClassDesignList = null;
            this._RightItemInfoClassMethodList = null;
            this._RightItemInfoClassPropertyList = null;
            this._SelectedACProject = null;
            this._SelectedRightItemInfoClassDesign = null;
            this._SelectedRightItemInfoClassMethod = null;
            this._SelectedRightItemInfoClassProperty = null;
            bool done = base.ACDeInit(deleteACClassTask);
            if (_AccessPrimary != null)
            {
                _AccessPrimary.ACDeInit(false);
                _AccessPrimary = null;
            }
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return done;
        }

        #endregion

        #region BSO->ACProperty

        #region Access and db
        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        /// <summary>
        /// The _ access primary
        /// </summary>
        ACAccessNav<VBGroup> _AccessPrimary;
        /// <summary>
        /// Gets the access primary.
        /// </summary>
        /// <value>The access primary.</value>
        [ACPropertyAccessPrimary(9999, "VBGroup")]
        public ACAccessNav<VBGroup> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessPrimary = navACQueryDefinition.NewAccessNav<VBGroup>("VBGroup", this);
                }
                return _AccessPrimary;
            }
        }

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

        #region VBGroup
        /// <summary>
        /// Gets or sets the current group.
        /// </summary>
        /// <value>The current group.</value>
        [ACPropertyCurrent(501, "VBGroup")]
        public VBGroup CurrentGroup
        {
            get
            {
                if (AccessPrimary == null) return null; return AccessPrimary.Current;
            }
            set
            {
                if (CurrentGroup != value)
                {
                    AccessPrimary.Current = value;
                    OnPropertyChanged(nameof(CurrentGroup));
                    OnPropertyChanged(nameof(ACProjectList));
                    if (SelectedGroup != CurrentGroup)
                    {
                        SelectedGroup = CurrentGroup;
                    }
                    CurrentACProject = null;
                    LoadGroupUsers(Database as Database, CurrentGroup);
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected group.
        /// </summary>
        /// <value>The selected group.</value>
        [ACPropertySelected(502, "VBGroup")]
        public VBGroup SelectedGroup
        {
            get
            {
                if (AccessPrimary == null) return null; return AccessPrimary.Selected;
            }
            set
            {
                if (SelectedGroup != value)
                {
                    AccessPrimary.Selected = value;
                    OnPropertyChanged("SelectedGroup");
                }
            }
        }

        /// <summary>
        /// Gets or sets the group list.
        /// </summary>
        /// <value>The group list.</value>
        [ACPropertyList(503, "VBGroup")]
        public IEnumerable<VBGroup> GroupList
        {
            get
            {
                return AccessPrimary.NavList;
            }
        }
        #endregion

        #region Presentation Modes
        private ACObjectItem _CurrentGroupMode;
        [ACPropertyCurrent(521, "GroupMode", "en{'Structure'}de{'Struktur'}")]
        public ACObjectItem CurrentGroupMode
        {
            get
            {
                return _CurrentGroupMode;
            }
            set
            {
                _CurrentGroupMode = value;
                if (_CurrentGroupMode == null || _CurrentGroupMode.ACCaption == "Project")
                {
                    ProjectControlMode = Global.ControlModes.Enabled;
                    MenuControlMode = Global.ControlModes.Hidden;
                    CurrentACProject = null;
                    CurrentProjectItem = null;
                    CurrentMenu = null;
                }
                else
                {
                    MenuControlMode = Global.ControlModes.Enabled;
                    ProjectControlMode = Global.ControlModes.Hidden;
                    CurrentACProject = null;
                    CurrentProjectItem = null;
                    WithCaption = true;
                }
                OnPropertyChanged("CurrentGroupMode");
            }
        }

        private ACObjectItemList _GroupModeList;
        [ACPropertyList(522, "GroupMode")]
        public ACObjectItemList GroupModeList
        {
            get
            {
                if (_GroupModeList == null)
                {
                    _GroupModeList = new ACObjectItemList();
                    _GroupModeList.Add(new ACObjectItem("Project"));
                    _GroupModeList.Add(new ACObjectItem("Menu"));
                }
                return _GroupModeList;
            }
        }

        private Global.ControlModes _ProjectControlMode;
        [ACPropertyInfo(523)]
        public Global.ControlModes ProjectControlMode
        {
            get
            {
                return _ProjectControlMode;
            }
            set
            {
                _ProjectControlMode = value;
                OnPropertyChanged("ProjectControlMode");
            }
        }

        private Global.ControlModes _MenuControlMode = Global.ControlModes.Hidden;
        [ACPropertyInfo(524)]
        public Global.ControlModes MenuControlMode
        {
            get
            {
                return _MenuControlMode;
            }
            set
            {
                _MenuControlMode = value;
                OnPropertyChanged("MenuControlMode");
            }
        }

        private ACClassDesign _CurrentMenu;
        [ACPropertyCurrent(525, "BSOMenuOrganization", "en{'Menu'}de{'Menü'}")]
        public ACClassDesign CurrentMenu
        {
            get
            {
                return _CurrentMenu;
            }
            set
            {
                if (_CurrentMenu != value)
                {
                    _CurrentMenuRootItem = null;
                    _CurrentMenu = value;
                    if (CurrentACProject == null || (CurrentACProject != null && CurrentACProject.ACProjectType != Global.ACProjectTypes.Root))
                        CurrentACProject = ACProjectList.FirstOrDefault(c => c.ACProjectType == Global.ACProjectTypes.Root);
                    else
                        RefreshProjectTree(true);
                }
                OnPropertyChanged("CurrentMenu");
            }
        }

        private ACMenuItem _CurrentMenuRootItem = null;
        public ACMenuItem CurrentMenuRootItem
        {
            get
            {
                if (_CurrentMenuRootItem != null)
                    return _CurrentMenuRootItem;
                if (CurrentMenu == null)
                    return null;
                _CurrentMenuRootItem = CurrentMenu.MenuEntry;
                return _CurrentMenuRootItem;
            }
        }

        [ACPropertyList(526, "BSOMenuOrganization")]
        public IEnumerable<ACClassDesign> MenuList
        {
            get
            {
                return Db.ACClassDesign.Where(c => c.ACKindIndex == (short)Global.ACKinds.DSDesignMenu);
            }
        }
        #endregion

        #region ACProject
        /// <summary>
        /// The _ selected AC project
        /// </summary>
        ACProject _SelectedACProject;
        /// <summary>
        /// Gets or sets the selected AC project.
        /// </summary>
        /// <value>The selected AC project.</value>
        [ACPropertySelected(513, "ACProject")]
        public ACProject SelectedACProject
        {
            get
            {
                return _SelectedACProject;
            }
            set
            {
                _SelectedACProject = value;
                OnPropertyChanged("SelectedACProject");
            }
        }

        /// <summary>
        /// The _ current AC project
        /// </summary>
        ACProject _CurrentACProject;
        /// <summary>
        /// Gets or sets the current AC project.
        /// </summary>
        /// <value>The current AC project.</value>
        [ACPropertyCurrent(514, "ACProject", "en{'Project'}de{'Projekt'}")]
        public ACProject CurrentACProject
        {
            get
            {
                return _CurrentACProject;
            }
            set
            {
                if (value != null)
                {
                    if (_CurrentACProject != value)
                    {
                        _CurrentACProject = value;
                        ProjectManager.LoadACProject(CurrentACProject, ProjectTreePresentationMode, ProjectTreeVisibilityFilter, ProjectTreeCheckHandler);
                    }
                }
                else
                {
                    _CurrentACProject = null;
                    ProjectManager.EliminateProjectTree();
                }
                OnPropertyChanged("CurrentACProject");
            }
        }

        /// <summary>
        /// Gets the AC project list.
        /// </summary>
        /// <value>The AC project list.</value>
        [ACPropertyList(515, "ACProject")]
        public IEnumerable<ACProject> ACProjectList
        {
            get
            {
                return Db.ACProject.OrderBy(c => c.ACProjectTypeIndex).ThenBy(c => c.ACProjectName);
            }
        }

        #endregion

        #region Project Tree

        private bool _IsNeedRefreshAfterSave = false;

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


        /// <summary>
        /// Root-Item
        /// </summary>
        /// <value>The current project item root.</value>
        [ACPropertyCurrent(516, "ProjectItemRoot")]
        public ACClassInfoWithItems CurrentProjectItemRoot
        {
            get
            {
                return ProjectManager.CurrentProjectItemRoot;
            }
        }

        ACClassInfoWithItems _CurrentProjectItem = null;
        /// <summary>
        /// Selected Item
        /// </summary>
        /// <value>The current project item.</value>
        [ACPropertyCurrent(517, "ProjectItem")]
        public ACClassInfoWithItems CurrentProjectItem
        {
            get
            {
                return _CurrentProjectItem;
            }
            set
            {
                if (_CurrentProjectItem != value)
                {
                    _CurrentProjectItem = value;
                    RefreshMemberRightInfos();
                    OnPropertyChanged("CurrentProjectItem");
                }
            }
        }


        private ACClassInfoWithItems.CheckHandler _ProjectTreeCheckHandler;
        protected ACClassInfoWithItems.CheckHandler ProjectTreeCheckHandler
        {
            get
            {
                if (_ProjectTreeCheckHandler == null)
                {
                    _ProjectTreeCheckHandler = new ACClassInfoWithItems.CheckHandler()
                    {
                        QueryRightsFromDB = true,
                        IsCheckboxVisible = true,
                        CheckedSetter = InfoItemIsCheckedSetter,
                        CheckedGetter = InfoItemIsCheckedGetter,
                        CheckIsEnabledGetter = InfoItemIsCheckEnabledGetter,
                    };
                }
                return _ProjectTreeCheckHandler;
            }
        }

        protected ACClassInfoWithItems.VisibilityFilters ProjectTreeVisibilityFilter
        {
            get
            {
                ACClassInfoWithItems.VisibilityFilters filter
                    = new ACClassInfoWithItems.VisibilityFilters()
                    {
                        SearchText = this.SearchClassText,
                        IncludeLibraryClasses = CurrentACProject != null && CurrentACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary
                    };
                return filter;
            }
        }

        protected ACProjectManager.PresentationMode ProjectTreePresentationMode
        {
            get
            {
                ACProjectManager.PresentationMode mode
                    = new ACProjectManager.PresentationMode()
                    {
                        ShowCaptionInTree = this.WithCaption,
                        DisplayGroupedTree = this.ShowGroup,
                        DisplayTreeAsMenu = this.CurrentMenuRootItem
                    };
                return mode;
            }
        }


        /// <summary>
        /// The _ show group
        /// </summary>
        bool _ShowGroup = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show group].
        /// </summary>
        /// <value><c>true</c> if [show group]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(518, "TreeConfig", "en{'Grouped'}de{'Gruppiert'}")]
        public bool ShowGroup
        {
            get
            {
                return _ShowGroup;
            }
            set
            {
                _ShowGroup = value;
                OnPropertyChanged("ShowGroup");
                RefreshProjectTree(true);
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
        [ACPropertyInfo(519, "TreeConfig", "en{'Search Class'}de{'Suche Klasse'}")]
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
        [ACPropertyInfo(520, "TreeConfig", "en{'With Caption'}de{'Mit Bezeichnung'}")]
        public bool WithCaption
        {
            get
            {
                return _WithCaption;
            }
            set
            {
                if (_WithCaption != value)
                {
                    _WithCaption = value;
                    OnPropertyChanged("WithCaption");
                    RefreshProjectTree();
                }
            }
        }


        #endregion

        #region RightItemInfoClassProperty
        /// <summary>
        /// The _ selected right item info class property
        /// </summary>
        ACClassPropertyInfo _SelectedRightItemInfoClassProperty;
        /// <summary>
        /// Gets or sets the selected right item info class property.
        /// </summary>
        /// <value>The selected right item info class property.</value>
        [ACPropertySelected(504, "RightItemInfoClassProperty")]
        public ACClassPropertyInfo SelectedRightItemInfoClassProperty
        {
            get
            {
                return _SelectedRightItemInfoClassProperty;
            }
            set
            {
                _SelectedRightItemInfoClassProperty = value;
                OnPropertyChanged("SelectedRightItemInfoClassProperty");
            }
        }

        /// <summary>
        /// The _ current right item info class property
        /// </summary>
        ACClassPropertyInfo _CurrentRightItemInfoClassProperty;
        /// <summary>
        /// Gets or sets the current right item info class property.
        /// </summary>
        /// <value>The current right item info class property.</value>
        [ACPropertyCurrent(505, "RightItemInfoClassProperty")]
        public ACClassPropertyInfo CurrentRightItemInfoClassProperty
        {
            get
            {
                return _CurrentRightItemInfoClassProperty;
            }
            set
            {
                if (CurrentRightItemInfoClassProperty != null)
                    CurrentRightItemInfoClassProperty.PropertyChanged -= CurrentRightItemInfoClassProperty_PropertyChanged;

                _CurrentRightItemInfoClassProperty = value;
                OnPropertyChanged("CurrentRightItemInfoClassProperty");

                if (CurrentRightItemInfoClassProperty != null)
                    CurrentRightItemInfoClassProperty.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentRightItemInfoClassProperty_PropertyChanged);
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the CurrentRightItemInfoClassProperty control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void CurrentRightItemInfoClassProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (BackgroundWorker.IsBusy)
                return;

            ACClassPropertyInfo riInfoClassProperty = sender as ACClassPropertyInfo;
            if (riInfoClassProperty != null && e.PropertyName == "ControlMode")
            {
                List<VBGroupRight> rightsForClass = GetRightsForItem(CurrentProjectItem);
                if (rightsForClass != null)
                    UpdateRightsProperty(CurrentProjectItem, riInfoClassProperty, riInfoClassProperty.ControlMode, rightsForClass);
            }
        }

        /// <summary>
        /// The _ right item info class property list
        /// </summary>
        List<ACClassPropertyInfo> _RightItemInfoClassPropertyList = null;
        /// <summary>
        /// Gets the right item info class property list.
        /// </summary>
        /// <value>The right item info class property list.</value>
        [ACPropertyList(506, "RightItemInfoClassProperty")]
        public List<ACClassPropertyInfo> RightItemInfoClassPropertyList
        {
            get
            {
                return _RightItemInfoClassPropertyList;
            }
            set
            {
                _RightItemInfoClassPropertyList = value;
                OnPropertyChanged("RightItemInfoClassPropertyList");
            }
        }
        #endregion

        #region RightItemInfoClassMethod
        /// <summary>
        /// The _ selected right item info class method
        /// </summary>
        ACClassMethodInfo _SelectedRightItemInfoClassMethod;
        /// <summary>
        /// Gets or sets the selected right item info class method.
        /// </summary>
        /// <value>The selected right item info class method.</value>
        [ACPropertySelected(507, "RightItemInfoClassMethod")]
        public ACClassMethodInfo SelectedRightItemInfoClassMethod
        {
            get
            {
                return _SelectedRightItemInfoClassMethod;
            }
            set
            {
                _SelectedRightItemInfoClassMethod = value;
                OnPropertyChanged("SelectedRightItemInfoClassMethod");
            }
        }

        /// <summary>
        /// The _ current right item info class method
        /// </summary>
        ACClassMethodInfo _CurrentRightItemInfoClassMethod;
        /// <summary>
        /// Gets or sets the current right item info class method.
        /// </summary>
        /// <value>The current right item info class method.</value>
        [ACPropertyCurrent(508, "RightItemInfoClassMethod")]
        public ACClassMethodInfo CurrentRightItemInfoClassMethod
        {
            get
            {
                return _CurrentRightItemInfoClassMethod;
            }
            set
            {
                if (CurrentRightItemInfoClassMethod != null)
                    CurrentRightItemInfoClassMethod.PropertyChanged -= CurrentRightItemInfoClassMethod_PropertyChanged;

                _CurrentRightItemInfoClassMethod = value;
                OnPropertyChanged("CurrentRightItemInfoClassMethod");

                if (CurrentRightItemInfoClassMethod != null)
                    CurrentRightItemInfoClassMethod.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentRightItemInfoClassMethod_PropertyChanged);
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the CurrentRightItemInfoClassMethod control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void CurrentRightItemInfoClassMethod_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (BackgroundWorker.IsBusy)
                return;

            ACClassMethodInfo riInfoClassMethod = sender as ACClassMethodInfo;
            if (riInfoClassMethod != null && e.PropertyName == "ControlMode")
            {
                List<VBGroupRight> rightsForClass = GetRightsForItem(CurrentProjectItem);
                if (rightsForClass != null)
                    UpdateRightsMethod(CurrentProjectItem, riInfoClassMethod, riInfoClassMethod.ControlMode, rightsForClass);
            }
        }

        /// <summary>
        /// The _ right item info class method list
        /// </summary>
        List<ACClassMethodInfo> _RightItemInfoClassMethodList = null;
        /// <summary>
        /// Gets the right item info class method list.
        /// </summary>
        /// <value>The right item info class method list.</value>
        [ACPropertyList(509, "RightItemInfoClassMethod")]
        public List<ACClassMethodInfo> RightItemInfoClassMethodList
        {
            get
            {
                return _RightItemInfoClassMethodList;
            }
            set
            {
                _RightItemInfoClassMethodList = value;
                OnPropertyChanged("RightItemInfoClassMethodList");
            }
        }
        #endregion

        #region RightItemInfoClassDesign
        /// <summary>
        /// The _ selected right item info class design
        /// </summary>
        ACClassDesignInfo _SelectedRightItemInfoClassDesign;
        /// <summary>
        /// Gets or sets the selected right item info class design.
        /// </summary>
        /// <value>The selected right item info class design.</value>
        [ACPropertySelected(510, "RightItemInfoClassDesign")]
        public ACClassDesignInfo SelectedRightItemInfoClassDesign
        {
            get
            {
                return _SelectedRightItemInfoClassDesign;
            }
            set
            {
                _SelectedRightItemInfoClassDesign = value;
                OnPropertyChanged("SelectedRightItemInfoClassDesign");
            }
        }

        /// <summary>
        /// The _ current right item info class design
        /// </summary>
        ACClassDesignInfo _CurrentRightItemInfoClassDesign;
        /// <summary>
        /// Gets or sets the current right item info class design.
        /// </summary>
        /// <value>The current right item info class design.</value>
        [ACPropertyCurrent(511, "RightItemInfoClassDesign")]
        public ACClassDesignInfo CurrentRightItemInfoClassDesign
        {
            get
            {
                return _CurrentRightItemInfoClassDesign;
            }
            set
            {
                if (CurrentRightItemInfoClassDesign != null)
                    CurrentRightItemInfoClassDesign.PropertyChanged -= CurrentRightItemInfoClassDesign_PropertyChanged;

                _CurrentRightItemInfoClassDesign = value;
                OnPropertyChanged("CurrentRightItemInfoClassDesign");

                if (CurrentRightItemInfoClassDesign != null)
                    CurrentRightItemInfoClassDesign.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentRightItemInfoClassDesign_PropertyChanged);
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the CurrentRightItemInfoClassDesign control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void CurrentRightItemInfoClassDesign_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (BackgroundWorker.IsBusy)
                return;

            ACClassDesignInfo riInfoClassDesign = sender as ACClassDesignInfo;
            if (riInfoClassDesign != null && e.PropertyName == "ControlMode")
            {
                List<VBGroupRight> rightsForClass = GetRightsForItem(CurrentProjectItem);
                if (rightsForClass != null)
                    UpdateRightsDesign(CurrentProjectItem, riInfoClassDesign, riInfoClassDesign.ControlMode, rightsForClass);
            }
        }

        /// <summary>
        /// The _ right item info class design list
        /// </summary>
        List<ACClassDesignInfo> _RightItemInfoClassDesignList = null;
        /// <summary>
        /// Gets the right item info class design list.
        /// </summary>
        /// <value>The right item info class design list.</value>
        [ACPropertyList(512, "RightItemInfoClassDesign")]
        public List<ACClassDesignInfo> RightItemInfoClassDesignList
        {
            get
            {
                return _RightItemInfoClassDesignList;
            }
            set
            {
                _RightItemInfoClassDesignList = value;
                OnPropertyChanged("RightItemInfoClassDesignList");
            }
        }
        #endregion

        #region Group VBUser

        public const string GroupUser = "GroupUser";

        private VBUser _SelectedGroupUser;
        /// <summary>
        /// Selected property for VBUser
        /// </summary>
        /// <value>The selected GroupUser</value>
        [ACPropertySelected(9999, nameof(GroupUser), "en{'TODO: GroupUser'}de{'TODO: GroupUser'}")]
        public VBUser SelectedGroupUser
        {
            get
            {
                return _SelectedGroupUser;
            }
            set
            {
                if (_SelectedGroupUser != value)
                {
                    _SelectedGroupUser = value;
                    OnPropertyChanged(nameof(SelectedGroupUser));
                }
            }
        }

        private List<VBUser> _GroupUserList;
        /// <summary>
        /// List property for VBUser
        /// </summary>
        /// <value>The GroupUser list</value>
        [ACPropertyList(9999, nameof(GroupUser))]
        public List<VBUser> GroupUserList
        {
            get
            {
                return _GroupUserList;
            }
            set
            {
                _GroupUserList = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region OtherUser
        public const string OtherUser = "OtherUser";

        private VBUser _SelectedOtherUser;
        /// <summary>
        /// Selected property for VBUser
        /// </summary>
        /// <value>The selected OtherUser</value>
        [ACPropertySelected(9999, nameof(OtherUser), "en{'TODO: OtherUser'}de{'TODO: OtherUser'}")]
        public VBUser SelectedOtherUser
        {
            get
            {
                return _SelectedOtherUser;
            }
            set
            {
                if (_SelectedOtherUser != value)
                {
                    _SelectedOtherUser = value;
                    OnPropertyChanged(nameof(SelectedOtherUser));
                }
            }
        }


        private List<VBUser> _OtherUserList;
        /// <summary>
        /// List property for VBUser
        /// </summary>
        /// <value>The OtherUser list</value>
        [ACPropertyList(9999, nameof(OtherUser))]
        public List<VBUser> OtherUserList
        {
            get
            {
                return _OtherUserList;
            }
            set
            {
                _OtherUserList = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        #region BSO->ACMethod

        #region Ribbon-Commands
        /// <summary>
        /// Saves this instance.
        /// </summary>
        [ACMethodCommand("VBGroup", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            OnSave();
        }

        protected override Msg OnPreSave()
        {
            Msg result = base.OnPreSave();
            if (result != null)
                return result;
            return null;
        }

        protected override void OnPostSave()
        {
            if (_IsNeedRefreshAfterSave)
                RefreshProjectTree();
            _IsNeedRefreshAfterSave = false;
            base.OnPostSave();
        }

        /// <summary>
        /// Determines whether [is enabled save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledSave()
        {
            if (BackgroundWorker.IsBusy)
                return false;
            return OnIsEnabledSave();
        }

        /// <summary>
        /// Undoes the save.
        /// </summary>
        [ACMethodCommand("VBGroup", "en{'Undo'}de{'Nicht speichern'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
        public void UndoSave()
        {
            OnUndoSave();
            LoadGroupUsers(Database as Database, CurrentGroup);
        }

        /// <summary>
        /// Determines whether [is enabled undo save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled undo save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledUndoSave()
        {
            if (BackgroundWorker.IsBusy)
                return false;
            return OnIsEnabledUndoSave();
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        [ACMethodInteraction("VBGroup", "en{'Load'}de{'Laden'}", (short)MISort.Load, false, "SelectedGroup")]
        public void Load(bool requery = false)
        {
            if (!PreExecute("Load"))
                return;
            LoadEntity<VBGroup>(requery, () => SelectedGroup, () => CurrentGroup, c => CurrentGroup = c,
                        Db.VBGroup
                        .Where(c => c.VBGroupID == SelectedGroup.VBGroupID));
            PostExecute("Load");
        }

        /// <summary>
        /// Determines whether [is enabled load].
        /// </summary>
        /// <returns><c>true</c> if [is enabled load]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledLoad()
        {
            if (BackgroundWorker.IsBusy)
                return false;
            return SelectedGroup != null;
        }

        /// <summary>
        /// News this instance.
        /// </summary>
        [ACMethodInteraction("VBGroup", Const.New, (short)MISort.New, true, "SelectedGroup")]
        public void New()
        {
            CurrentGroup = VBGroup.NewACObject(Db, null);
            Db.VBGroup.AddObject(CurrentGroup);
            ACState = Const.SMNew;
            AccessPrimary.NavList.Add(CurrentGroup);
            OnPropertyChanged("GroupList");
            SelectedGroup = CurrentGroup;
        }

        /// <summary>
        /// Determines whether [is enabled new].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNew()
        {
            if (BackgroundWorker.IsBusy)
                return false;
            return true;
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        [ACMethodInteraction("VBGroup", Const.Delete, (short)MISort.Delete, true, "CurrentGroup")]
        public void Delete()
        {
            Msg msg = CurrentGroup.DeleteACObject(Db, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            if (AccessPrimary == null)
                return;
            AccessPrimary.NavList.Remove(CurrentGroup);
            SelectedGroup = AccessPrimary.NavList.FirstOrDefault();
            Load();
        }

        /// <summary>
        /// Determines whether [is enabled delete].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDelete()
        {
            if (BackgroundWorker.IsBusy)
                return false;
            return true;
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        [ACMethodCommand("VBGroup", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            AccessPrimary.NavSearch(Db);
            OnPropertyChanged("GroupList");
        }
        #endregion

        #region Right-Commands

        #region AssignAllRights
        public const string MN_AssignAllRights = "AssignAllRights";
        /// <summary>
        /// Assigns all rights.
        /// </summary>
        [ACMethodInteraction("ProjectItem", "en{'Assign all rights'}de{'Alle Rechte zuweisen'}", 100, false, "CurrentProjectItem")]
        public void AssignAllRights()
        {
            if (BackgroundWorker.IsBusy)
                return;
            BackgroundWorker.RunWorkerAsync(MN_AssignAllRights);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledAssignAllRights()
        {
            return !BackgroundWorker.IsBusy && CurrentProjectItem != null;
        }

        protected void AssignAllRightsAsync()
        {
            CurrentProgressInfo.TotalProgress.ProgressText = this.ComponentClass.GetMethod(MN_AssignAllRights).ACCaption;
            CurrentProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            CurrentProgressInfo.TotalProgress.ProgressRangeTo = CurrentProjectItem.SearchClassChildCount;
            CurrentProgressInfo.TotalProgress.ProgressCurrent = 0;

            UpdateRightsRecursive(CurrentProjectItem, Global.ControlModes.Enabled);
            CurrentProgressInfo.TotalProgress.ProgressText = "Saving Changes...";
            ACSaveChanges();
            RefreshProjectTree();
        }

        #endregion

        #region UnassignAllRights
        public const string MN_UnassignAllRights = "UnassignAllRights";

        /// <summary>
        /// Unassigns all rights.
        /// </summary>
        [ACMethodInteraction("ProjectItem", "en{'Remove all permissions'}de{'Alle Rechte aufheben'}", 102, false, "CurrentProjectItem")]
        public void UnassignAllRights()
        {
            if (BackgroundWorker.IsBusy)
                return;
            BackgroundWorker.RunWorkerAsync(MN_UnassignAllRights);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledUnassignAllRights()
        {
            return !BackgroundWorker.IsBusy && CurrentProjectItem != null;
        }

        protected void UnassignAllRightsAsync()
        {
            CurrentProgressInfo.TotalProgress.ProgressText = this.ComponentClass.GetMethod(MN_UnassignAllRights).ACCaption;
            CurrentProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            CurrentProgressInfo.TotalProgress.ProgressRangeTo = CurrentProjectItem.SearchClassChildCount;
            CurrentProgressInfo.TotalProgress.ProgressCurrent = 0;

            UpdateRightsRecursive(CurrentProjectItem, Global.ControlModes.Hidden);
            CurrentProgressInfo.TotalProgress.ProgressText = "Saving Changes...";
            ACSaveChanges();
            RefreshProjectTree();
        }
        #endregion

        #region AssignAllReadonlyRights
        public const string MN_AssignAllReadonlyRights = "AssignAllReadonlyRights";

        [ACMethodInteraction("ProjectItem", "en{'Assign readonly permission to all child elements'}de{'Untergeordnete Elemente nur mit Lesezugriff'}", 103, false, "CurrentProjectItem")]
        public void AssignAllReadonlyRights()
        {
            if (BackgroundWorker.IsBusy)
                return;
            BackgroundWorker.RunWorkerAsync(MN_AssignAllReadonlyRights);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledAssignAllReadonlyRights()
        {
            return !BackgroundWorker.IsBusy && CurrentProjectItem != null;
        }

        protected void AssignAllReadonlyRightsAsync()
        {
            CurrentProgressInfo.TotalProgress.ProgressText = this.ComponentClass.GetMethod(MN_AssignAllReadonlyRights).ACCaption;
            CurrentProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            CurrentProgressInfo.TotalProgress.ProgressRangeTo = CurrentProjectItem.SearchClassChildCount;
            CurrentProgressInfo.TotalProgress.ProgressCurrent = 0;

            UpdateRightsRecursive(CurrentProjectItem, Global.ControlModes.Disabled);
            CurrentProgressInfo.TotalProgress.ProgressText = "Saving Changes...";
            ACSaveChanges();
            RefreshProjectTree();
        }
        #endregion

        #region SelectedMethodRightAssignOnAllChilds
        public const string MN_SelectedMethodRightAssignOnAllChilds = "SelectedMethodRightAssignOnAllChilds";

        [ACMethodInteraction("", "en{'Apply permission on children'}de{'Berechtigung bei Unterobjekten setzen'}", 104, true, "SelectedRightItemInfoClassMethod")]
        public void SelectedMethodRightAssignOnAllChilds()
        {
            if (BackgroundWorker.IsBusy)
                return;
            BackgroundWorker.RunWorkerAsync(MN_SelectedMethodRightAssignOnAllChilds);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledSelectedMethodRightAssignOnAllChilds()
        {
            return !BackgroundWorker.IsBusy && CurrentProjectItem != null;
        }

        protected void SelectedMethodRightAssignOnAllChildsAsync()
        {
            CurrentProgressInfo.TotalProgress.ProgressText = this.ComponentClass.GetMethod(MN_SelectedMethodRightAssignOnAllChilds).ACCaption;
            CurrentProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            CurrentProgressInfo.TotalProgress.ProgressRangeTo = CurrentProjectItem.SearchClassChildCount;
            CurrentProgressInfo.TotalProgress.ProgressCurrent = 0;

            UpdateMethodRightsRecursive(CurrentProjectItem, SelectedRightItemInfoClassMethod.ControlMode, SelectedRightItemInfoClassMethod);
            _IsNeedRefreshAfterSave = true;
        }
        #endregion

        #region MN_SelectedPropertyRightAssignOnAllChilds
        public const string MN_SelectedPropertyRightAssignOnAllChilds = "SelectedPropertyRightAssignOnAllChilds";

        [ACMethodInteraction("", "en{'Apply permission on children'}de{'Berechtigung bei Unterobjekten setzen'}", 105, true, "SelectedRightItemInfoClassProperty")]
        public void SelectedPropertyRightAssignOnAllChilds()
        {
            if (BackgroundWorker.IsBusy)
                return;
            BackgroundWorker.RunWorkerAsync(MN_SelectedPropertyRightAssignOnAllChilds);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledSelectedPropertyRightAssignOnAllChilds()
        {
            return !BackgroundWorker.IsBusy && CurrentProjectItem != null;
        }

        protected void SelectedPropertyRightAssignOnAllChildsAsync()
        {
            CurrentProgressInfo.TotalProgress.ProgressText = this.ComponentClass.GetMethod(MN_SelectedPropertyRightAssignOnAllChilds).ACCaption;
            CurrentProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            CurrentProgressInfo.TotalProgress.ProgressRangeTo = CurrentProjectItem.SearchClassChildCount;
            CurrentProgressInfo.TotalProgress.ProgressCurrent = 0;

            UpdatePropertyRightsRecursive(CurrentProjectItem, SelectedRightItemInfoClassProperty.ControlMode, SelectedRightItemInfoClassProperty);
            _IsNeedRefreshAfterSave = true;
        }
        #endregion

        #region SelectedMethodRightAssignOnAllItemsInTree
        public const string MN_SelectedMethodRightAssignOnAllItemsInTree = "SelectedMethodRightAssignOnAllItemsInTree";

        [ACMethodInteraction("", "en{'Apply permission on all items'}de{'Berechtigung bei allen Objekten setzen'}", 106, true, "SelectedRightItemInfoClassMethod")]
        public void SelectedMethodRightAssignOnAllItemsInTree()
        {
            if (BackgroundWorker.IsBusy)
                return;
            BackgroundWorker.RunWorkerAsync(MN_SelectedMethodRightAssignOnAllItemsInTree);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledSelectedMethodRightAssignOnAllItemsInTree()
        {
            return !BackgroundWorker.IsBusy && CurrentProjectItemRoot != null;
        }

        protected void SelectedMethodRightAssignOnAllItemsInTreeAsync()
        {
            CurrentProgressInfo.TotalProgress.ProgressText = this.ComponentClass.GetMethod(MN_SelectedMethodRightAssignOnAllItemsInTree).ACCaption;
            CurrentProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            CurrentProgressInfo.TotalProgress.ProgressRangeTo = CurrentProjectItemRoot.SearchClassChildCount;
            CurrentProgressInfo.TotalProgress.ProgressCurrent = 0;

            UpdateMethodRightsRecursive(CurrentProjectItemRoot, SelectedRightItemInfoClassMethod.ControlMode, SelectedRightItemInfoClassMethod);
            _IsNeedRefreshAfterSave = true;
        }
        #endregion

        #region SelectedPropertyRightAssignOnAllItemsInTree
        public const string MN_SelectedPropertyRightAssignOnAllItemsInTree = "SelectedPropertyRightAssignOnAllItemsInTree";

        [ACMethodInteraction("", "en{'Apply permission on all items'}de{'Berechtigung bei allen Objekten setzen'}", 107, true, "SelectedRightItemInfoClassProperty")]
        public void SelectedPropertyRightAssignOnAllItemsInTree()
        {
            if (BackgroundWorker.IsBusy)
                return;
            BackgroundWorker.RunWorkerAsync(MN_SelectedPropertyRightAssignOnAllItemsInTree);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledSelectedPropertyRightAssignOnAllItemsInTree()
        {
            return !BackgroundWorker.IsBusy && CurrentProjectItemRoot != null;
        }

        protected void SelectedPropertyRightAssignOnAllItemsInTreeAsync()
        {
            CurrentProgressInfo.TotalProgress.ProgressText = this.ComponentClass.GetMethod(MN_SelectedPropertyRightAssignOnAllItemsInTree).ACCaption;
            CurrentProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            CurrentProgressInfo.TotalProgress.ProgressRangeTo = CurrentProjectItemRoot.SearchClassChildCount;
            CurrentProgressInfo.TotalProgress.ProgressCurrent = 0;

            UpdatePropertyRightsRecursive(CurrentProjectItemRoot, SelectedRightItemInfoClassProperty.ControlMode, SelectedRightItemInfoClassProperty);
            _IsNeedRefreshAfterSave = true;
        }
        #endregion

        #region (Un)AssignUser

        /// <summary>
        /// 
        /// </summary>
        [ACMethodInfo(nameof(AssignUser), "en{'>'}de{'>'}", 999)]
        public void AssignUser()
        {
            if (!IsEnabledAssignUser())
            {
                return;
            }

            VBUser user = SelectedOtherUser;

            VBUserGroup vBUserGroup = VBUserGroup.NewACObject(Database as Database, user);
            vBUserGroup.VBGroup = CurrentGroup;
            CurrentGroup.VBUserGroup_VBGroup.Add(vBUserGroup);

            GroupUserList.Add(user);
            OtherUserList.Remove(user);

            GroupUserList = GroupUserList.OrderBy(c => c.Initials).ToList();
            SelectedGroupUser = user;
            SelectedOtherUser = OtherUserList.FirstOrDefault();
        }

        public bool IsEnabledAssignUser()
        {
            return SelectedOtherUser != null;
        }

        /// <summary>
        /// 
        /// </summary>
        [ACMethodInfo(nameof(UnAssignUser), "en{'<'}de{'<'}", 999)]
        public void UnAssignUser()
        {
            if (!IsEnabledUnAssignUser())
            {
                return;
            }

            VBUser user = SelectedGroupUser;

            VBUserGroup vBUserGroup = CurrentGroup.VBUserGroup_VBGroup.Where(c=>c.VBUserID == user.VBUserID).FirstOrDefault();
            CurrentGroup.VBUserGroup_VBGroup.Remove(vBUserGroup);
            vBUserGroup.DeleteACObject(Database, false);

            OtherUserList.Add(user);
            GroupUserList.Remove(user);

            OtherUserList = OtherUserList.OrderBy(c => c.Initials).ToList();
            SelectedOtherUser = user;
            SelectedGroupUser = GroupUserList.FirstOrDefault();
        }

        public bool IsEnabledUnAssignUser()
        {
            return SelectedGroupUser != null;
        }

        #endregion

        #endregion

        #region Manipulation of Rights (Private)

        /// <summary>Updates the rights recursive.</summary>
        /// <param name="infoItem"></param>
        /// <param name="controlMode">The control mode.</param>
        private void UpdateRightsRecursive(ACClassInfoWithItems infoItem, Global.ControlModes controlMode)
        {
            if (BackgroundWorker.IsBusy)
                CurrentProgressInfo.TotalProgress.ProgressCurrent++;
            UpdateRightsOnClassItem(infoItem, controlMode, false);

            foreach (var infoChild in infoItem.VisibleItemsT)
            {
                UpdateRightsRecursive(infoChild, controlMode);
            }
        }
        private List<VBGroupRight> GetRightsForItem(ACClassInfoWithItems infoItem)
        {
            if (infoItem == null)
                return null;
            ACClass acClass = infoItem.ValueT;
            if (acClass == null)
                return null;
            return acClass.VBGroupRight_ACClass.ToArray().Where(c => c.VBGroup.VBGroupID == CurrentGroup.VBGroupID).ToList();
        }

        private void UpdatePropertyRightsRecursive(ACClassInfoWithItems infoItem, Global.ControlModes controlMode, ACClassPropertyInfo targetProperty)
        {
            if (BackgroundWorker.IsBusy)
                CurrentProgressInfo.TotalProgress.ProgressCurrent++;
            List<VBGroupRight> rightsForClass = GetRightsForItem(infoItem);
            if (rightsForClass != null)
            {
                ACClassPropertyInfo property = ProjectManager.GetRightItemInfoClassProperty(infoItem.ValueT, rightsForClass, targetProperty).FirstOrDefault();
                if (property != null)
                {
                    UpdateRightsProperty(infoItem, property, controlMode, rightsForClass);
                    if (controlMode >= Global.ControlModes.Enabled
                        && rightsForClass.Any()
                        && !rightsForClass.Where(c => !c.ACClassPropertyID.HasValue
                                                   && !c.ACClassMethodID.HasValue
                                                   && !c.ACClassDesignID.HasValue).Any())
                    {
                        UpdateRightsOnClassItem(infoItem, controlMode, true);
                    }
                }
            }

            foreach (var infoChild in infoItem.VisibleItemsT)
            {
                UpdatePropertyRightsRecursive(infoChild, controlMode, targetProperty);
            }
        }

        private void UpdateMethodRightsRecursive(ACClassInfoWithItems infoItem, Global.ControlModes controlMode, ACClassMethodInfo targetMethod)
        {
            if (BackgroundWorker.IsBusy)
                CurrentProgressInfo.TotalProgress.ProgressCurrent++;
            List<VBGroupRight> rightsForClass = GetRightsForItem(infoItem);
            if (rightsForClass != null)
            {
                ACClassMethodInfo method = ProjectManager.GetRightItemInfoClassMethod(infoItem.ValueT, rightsForClass, targetMethod).FirstOrDefault();
                if (method != null)
                {
                    UpdateRightsMethod(infoItem, method, controlMode, rightsForClass);
                    if (controlMode >= Global.ControlModes.Enabled
                        && rightsForClass.Any()
                        && !rightsForClass.Where(c => !c.ACClassPropertyID.HasValue
                                                   && !c.ACClassMethodID.HasValue
                                                   && !c.ACClassDesignID.HasValue).Any())
                    {
                        UpdateRightsOnClassItem(infoItem, controlMode, true);
                    }
                }
            }

            foreach (var infoChild in infoItem.VisibleItemsT)
            {
                UpdateMethodRightsRecursive(infoChild, controlMode, targetMethod);
            }
        }
        private void UpdateRightsOnClassItem(ACClassInfoWithItems infoItem, Global.ControlModes controlMode, bool applyOnlyOnClass = false)
        {
            List<VBGroupRight> rightsForClass = GetRightsForItem(infoItem);
            if (rightsForClass == null)
                return;
            VBGroupRight rightForItem = rightsForClass.Where(c => !c.ACClassPropertyID.HasValue
                                                                && !c.ACClassMethodID.HasValue
                                                                && !c.ACClassDesignID.HasValue)
                                                        .FirstOrDefault();
            if (controlMode >= Global.ControlModes.Disabled)
            {
                if (rightForItem == null)
                {
                    rightForItem = VBGroupRight.NewACObject(Db, CurrentGroup);
                    rightForItem.ACClass = infoItem.ValueT;
                    infoItem.ValueT.VBGroupRight_ACClass.Add(rightForItem);
                    rightsForClass.Add(rightForItem);
                }

                if (!applyOnlyOnClass)
                {
                    List<ACClassPropertyInfo> rightItemInfoClassPropertyList = ProjectManager.GetRightItemInfoClassProperty(infoItem.ValueT, rightsForClass);
                    foreach (var acClassPropertyInfo in rightItemInfoClassPropertyList)
                    {
                        UpdateRightsProperty(infoItem, acClassPropertyInfo, controlMode, rightsForClass);
                    }
                    List<ACClassMethodInfo> rightItemInfoClassMethodList = ProjectManager.GetRightItemInfoClassMethod(infoItem.ValueT, rightsForClass);
                    foreach (var acClassMethodInfo in rightItemInfoClassMethodList)
                    {
                        UpdateRightsMethod(infoItem, acClassMethodInfo, controlMode, rightsForClass);
                    }

                    List<ACClassDesignInfo> rightItemInfoClassDesignList = ProjectManager.GetRightItemInfoClassDesign(infoItem.ValueT, rightsForClass);
                    foreach (var acClassDesignInfo in rightItemInfoClassDesignList)
                    {
                        UpdateRightsDesign(infoItem, acClassDesignInfo, controlMode, rightsForClass);
                    }
                }
                infoItem.OnPropertyChanged(ACClassInfoWithItems.IsCheckedPropName);
            }
            else if (controlMode < Global.ControlModes.Disabled && rightsForClass.Any())
            {
                foreach (var removeRight in rightsForClass)
                {
                    removeRight.DeleteACObject(Db, false);
                    infoItem.ValueT.VBGroupRight_ACClass.Remove(removeRight);
                }
                infoItem.OnPropertyChanged(ACClassInfoWithItems.IsCheckedPropName);
            }
        }

        /// <summary>Updates the rights property.</summary>
        /// <param name="infoItem"></param>
        /// <param name="acClassPropertyInfo"></param>
        /// <param name="controlMode">The control mode.</param>
        /// <param name="rightsForClass"></param>
        private void UpdateRightsProperty(ACClassInfoWithItems infoItem, ACClassPropertyInfo acClassPropertyInfo, Global.ControlModes controlMode, List<VBGroupRight> rightsForClass)
        {
            VBGroupRight rightForItem = rightsForClass.Where(c => c.ACClassPropertyID == acClassPropertyInfo.ValueT.ACClassPropertyID).FirstOrDefault();
            if (controlMode >= Global.ControlModes.Disabled && rightForItem == null)
            {
                rightForItem = VBGroupRight.NewACObject(Db, CurrentGroup);
                rightForItem.ACClass = infoItem.ValueT;
                rightForItem.ACClassProperty = acClassPropertyInfo.ValueT;
                infoItem.ValueT.VBGroupRight_ACClass.Add(rightForItem);
                rightsForClass.Add(rightForItem);
                acClassPropertyInfo.ControlMode = controlMode;
            }
            else if (controlMode < Global.ControlModes.Disabled && rightForItem != null)
            {
                rightForItem.DeleteACObject(Db, false);
                infoItem.ValueT.VBGroupRight_ACClass.Remove(rightForItem);
                rightsForClass.Remove(rightForItem);
                acClassPropertyInfo.ControlMode = controlMode;
                rightForItem = null;
            }
            if (rightForItem != null)
                rightForItem.ControlMode = controlMode;
        }

        /// <summary>Updates the rights method.</summary>
        /// <param name="infoItem"></param>
        /// <param name="acClassMethodInfo"></param>
        /// <param name="controlMode">The control mode.</param>
        /// <param name="rightsForClass"></param>
        private void UpdateRightsMethod(ACClassInfoWithItems infoItem, ACClassMethodInfo acClassMethodInfo, Global.ControlModes controlMode, List<VBGroupRight> rightsForClass)
        {
            VBGroupRight rightForItem = rightsForClass.Where(c => c.ACClassMethodID == acClassMethodInfo.ValueT.ACClassMethodID).FirstOrDefault();
            if (controlMode >= Global.ControlModes.Disabled && rightForItem == null)
            {
                rightForItem = VBGroupRight.NewACObject(Db, CurrentGroup);
                rightForItem.ACClass = infoItem.ValueT;
                rightForItem.ACClassMethod = acClassMethodInfo.ValueT;
                infoItem.ValueT.VBGroupRight_ACClass.Add(rightForItem);
                rightsForClass.Add(rightForItem);
                acClassMethodInfo.ControlMode = controlMode;
            }
            else if (controlMode < Global.ControlModes.Disabled && rightForItem != null)
            {
                rightForItem.DeleteACObject(Db, false);
                infoItem.ValueT.VBGroupRight_ACClass.Remove(rightForItem);
                rightsForClass.Remove(rightForItem);
                acClassMethodInfo.ControlMode = controlMode;
                rightForItem = null;
            }
            if (rightForItem != null)
                rightForItem.ControlMode = controlMode;
        }

        /// <summary>Updates the rights design.</summary>
        /// <param name="infoItem"></param>
        /// <param name="acClassDesignInfo"></param>
        /// <param name="controlMode">The control mode.</param>
        /// <param name="rightsForClass"></param>
        private void UpdateRightsDesign(ACClassInfoWithItems infoItem, ACClassDesignInfo acClassDesignInfo, Global.ControlModes controlMode, List<VBGroupRight> rightsForClass)
        {
            VBGroupRight rightForItem = rightsForClass.Where(c => c.ACClassDesignID == acClassDesignInfo.ValueT.ACClassDesignID).FirstOrDefault();
            if (controlMode >= Global.ControlModes.Disabled && rightForItem == null)
            {
                rightForItem = VBGroupRight.NewACObject(Db, CurrentGroup);
                rightForItem.ACClass = infoItem.ValueT;
                rightForItem.ACClassDesign = acClassDesignInfo.ValueT;
                infoItem.ValueT.VBGroupRight_ACClass.Add(rightForItem);
                rightsForClass.Add(rightForItem);
                acClassDesignInfo.ControlMode = controlMode;
            }
            else if (controlMode < Global.ControlModes.Disabled && rightForItem != null)
            {
                rightForItem.DeleteACObject(Db, false);
                infoItem.ValueT.VBGroupRight_ACClass.Remove(rightForItem);
                rightsForClass.Remove(rightForItem);
                acClassDesignInfo.ControlMode = controlMode;
                rightForItem = null;
            }
            if (rightForItem != null)
                rightForItem.ControlMode = controlMode;
        }

        private void LoadGroupUsers(Database database, VBGroup vBGroup)
        {
            _GroupUserList = new List<VBUser>();

            if (vBGroup != null)
            {
                _GroupUserList =
                vBGroup
                .VBUserGroup_VBGroup
                .Select(c => c.VBUser)
                .OrderBy(c => c.Initials)
                .ToList();
            }

            Guid[] userIds = _GroupUserList.Select(c => c.VBUserID).ToArray();

            _OtherUserList =
                database
                .VBUser
                .Where(c => !userIds.Contains(c.VBUserID))
                .OrderBy(c => c.Initials)
                .ToList();

            OnPropertyChanged(nameof(GroupUserList));
            OnPropertyChanged(nameof(OtherUserList));
        }

        #endregion

        #region Tree-Handling

        private void InfoItemIsCheckedSetter(ACClassInfoWithItems infoItem, bool isChecked)
        {
            if (BackgroundWorker.IsBusy || infoItem == null)
                return;
            UpdateRightsOnClassItem(infoItem, isChecked ? Global.ControlModes.Enabled : Global.ControlModes.Hidden, true);
            RefreshMemberRightInfos();
        }

        private void RefreshMemberRightInfos()
        {
            if (_CurrentProjectItem != null)
            {
                List<VBGroupRight> rightsForClass = GetRightsForItem(_CurrentProjectItem);
                if (rightsForClass != null)
                {
                    RightItemInfoClassPropertyList = ProjectManager.GetRightItemInfoClassProperty(_CurrentProjectItem.ValueT, rightsForClass);
                    RightItemInfoClassMethodList = ProjectManager.GetRightItemInfoClassMethod(_CurrentProjectItem.ValueT, rightsForClass);
                    RightItemInfoClassDesignList = ProjectManager.GetRightItemInfoClassDesign(_CurrentProjectItem.ValueT, rightsForClass);
                }
                else
                {
                    RightItemInfoClassPropertyList = new List<ACClassPropertyInfo>();
                    RightItemInfoClassMethodList = new List<ACClassMethodInfo>();
                    RightItemInfoClassDesignList = new List<ACClassDesignInfo>();
                }
            }
            else
            {
                RightItemInfoClassPropertyList = new List<ACClassPropertyInfo>();
                RightItemInfoClassMethodList = new List<ACClassMethodInfo>();
                RightItemInfoClassDesignList = new List<ACClassDesignInfo>();
            }
        }

        private bool InfoItemIsCheckedGetter(ACClassInfoWithItems riInfoClass)
        {
            return riInfoClass.ValueT == null
                 || !riInfoClass.ValueT.IsRightmanagement
                 || riInfoClass.ValueT.VBGroupRight_ACClass.ToArray().Where(c => c.VBGroupID == CurrentGroup.VBGroupID
                                                                        && c.ACClassPropertyID == null
                                                                        && c.ACClassMethodID == null
                                                                        && c.ACClassDesignID == null).Any();
        }

        private bool InfoItemIsCheckEnabledGetter(ACClassInfoWithItems riInfoClass)
        {
            return riInfoClass.ValueT != null && riInfoClass.ValueT.IsRightmanagement;
        }

        [ACMethodCommand("TreeViewItemExpand", "en{'TVIExpand'}de{'TVIExpand'}", 590, false)]
        public void OnTreeViewItemExpand(ACClassInfoWithItems acClassInfoWithItems)
        {
            //_ACProjectManager.ExpandACProjectRightmanagmentItemsTree(acClassInfoWithItems, GroupRightList);
        }

        private void RefreshProjectTree(bool forceRebuildTree = false)
        {
            if (ProjectManager != null)
            {
                ProjectManager.RefreshProjectTree(ProjectTreePresentationMode, ProjectTreeVisibilityFilter, ProjectTreeCheckHandler, forceRebuildTree);
                RefreshMemberRightInfos();
            }
        }

        #endregion

        #region Clone

        [ACMethodInteraction(nameof(GroupClone), "en{'Clone'}de{'Duplizieren'}", (short)MISort.New, true, nameof(SelectedGroup), Global.ACKinds.MSMethodPrePost)]
        public void GroupClone()
        {
            if (!IsEnabledGroupClone())
            {
                return;
            }
            if (BackgroundWorker.IsBusy)
            {
                return;
            }
            BackgroundWorker.RunWorkerAsync(nameof(DoGroupClone));
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledGroupClone()
        {
            return CurrentGroup != null;
        }

        public VBGroup DoGroupClone(Database database, VBGroup group)
        {
            VBGroup clonedGroup = VBGroup.NewACObject(database, null);
            database.VBGroup.AddObject(clonedGroup);

            int count = database.VBGroup.Where(c => c.VBGroupName.StartsWith(group.VBGroupName)).Count();
            bool exist = true;
            string groupName = $"{group.VBGroupName}({count})";
            while (exist)
            {
                exist = database.VBGroup.Where(c => c.VBGroupName == groupName).Any();
                if (exist)
                {
                    count++;
                    groupName = $"{group.VBGroupName}({count})";
                }
            }

            clonedGroup.VBGroupName = groupName;
            clonedGroup.Description = group.Description;

            VBGroupRight[] groupRights = group.VBGroupRight_VBGroup.ToArray();
            foreach (VBGroupRight right in groupRights)
            {
                VBGroupRight clonedGroupRights = VBGroupRight.NewACObject(database, clonedGroup);
                clonedGroupRights.ACClass = right.ACClass;
                clonedGroupRights.ACClassProperty = right.ACClassProperty;
                clonedGroupRights.ACClassMethod = right.ACClassMethod;
                clonedGroupRights.ACClassDesign = right.ACClassDesign;
                clonedGroupRights.ControlModeIndex = right.ControlModeIndex;
                clonedGroup.VBGroupRight_VBGroup.Add(clonedGroupRights);
            }

            database.ACSaveChanges();
            return clonedGroup;
        }

        #endregion

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(AssignAllReadonlyRights):
                    AssignAllReadonlyRights();
                    return true;
                case nameof(AssignAllRights):
                    AssignAllRights();
                    return true;
                case nameof(Delete):
                    Delete();
                    return true;
                case nameof(GroupClone):
                    GroupClone();
                    return true;
                case nameof(IsEnabledAssignAllReadonlyRights):
                    result = IsEnabledAssignAllReadonlyRights();
                    return true;
                case nameof(IsEnabledAssignAllRights):
                    result = IsEnabledAssignAllRights();
                    return true;
                case nameof(IsEnabledDelete):
                    result = IsEnabledDelete();
                    return true;
                case nameof(IsEnabledGroupClone):
                    result = IsEnabledGroupClone();
                    return true;
                case nameof(IsEnabledLoad):
                    result = IsEnabledLoad();
                    return true;
                case nameof(IsEnabledNew):
                    result = IsEnabledNew();
                    return true;
                case nameof(IsEnabledSave):
                    result = IsEnabledSave();
                    return true;
                case nameof(IsEnabledSelectedMethodRightAssignOnAllChilds):
                    result = IsEnabledSelectedMethodRightAssignOnAllChilds();
                    return true;
                case nameof(IsEnabledSelectedMethodRightAssignOnAllItemsInTree):
                    result = IsEnabledSelectedMethodRightAssignOnAllItemsInTree();
                    return true;
                case nameof(IsEnabledSelectedPropertyRightAssignOnAllChilds):
                    result = IsEnabledSelectedPropertyRightAssignOnAllChilds();
                    return true;
                case nameof(IsEnabledSelectedPropertyRightAssignOnAllItemsInTree):
                    result = IsEnabledSelectedPropertyRightAssignOnAllItemsInTree();
                    return true;
                case nameof(IsEnabledUnassignAllRights):
                    result = IsEnabledUnassignAllRights();
                    return true;
                case nameof(IsEnabledUndoSave):
                    result = IsEnabledUndoSave();
                    return true;
                case nameof(Load):
                    Load(acParameter.Count() == 1 ? (System.Boolean)acParameter[0] : false);
                    return true;
                case nameof(New):
                    New();
                    return true;
                case nameof(OnTreeViewItemExpand):
                    OnTreeViewItemExpand((gip.core.datamodel.ACClassInfoWithItems)acParameter[0]);
                    return true;
                case nameof(Save):
                    Save();
                    return true;
                case nameof(Search):
                    Search();
                    return true;
                case nameof(SelectedMethodRightAssignOnAllChilds):
                    SelectedMethodRightAssignOnAllChilds();
                    return true;
                case nameof(SelectedMethodRightAssignOnAllItemsInTree):
                    SelectedMethodRightAssignOnAllItemsInTree();
                    return true;
                case nameof(SelectedPropertyRightAssignOnAllChilds):
                    SelectedPropertyRightAssignOnAllChilds();
                    return true;
                case nameof(SelectedPropertyRightAssignOnAllItemsInTree):
                    SelectedPropertyRightAssignOnAllItemsInTree();
                    return true;
                case nameof(UnassignAllRights):
                    UnassignAllRights();
                    return true;
                case nameof(UndoSave):
                    UndoSave();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #region BackgroundWorker

        /// <summary>
        /// 1. Dieser Eventhandler wird aufgerufen, wenn Hintergrundjob starten soll
        /// Dies wird ausgelöst durch den Aufruf der Methode RunWorkerAsync()
        /// Methode läuft im Hintergrundthread
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = e.Argument.ToString();
            switch (command)
            {
                case MN_AssignAllRights:
                    AssignAllRightsAsync();
                    break;
                case MN_UnassignAllRights:
                    UnassignAllRightsAsync();
                    break;
                case MN_AssignAllReadonlyRights:
                    AssignAllReadonlyRightsAsync();
                    break;
                case MN_SelectedMethodRightAssignOnAllChilds:
                    SelectedMethodRightAssignOnAllChildsAsync();
                    break;
                case MN_SelectedPropertyRightAssignOnAllChilds:
                    SelectedPropertyRightAssignOnAllChildsAsync();
                    break;
                case MN_SelectedMethodRightAssignOnAllItemsInTree:
                    SelectedMethodRightAssignOnAllItemsInTreeAsync();
                    break;
                case MN_SelectedPropertyRightAssignOnAllItemsInTree:
                    SelectedPropertyRightAssignOnAllItemsInTreeAsync();
                    break;
                case (nameof(DoGroupClone)):
                    e.Result = DoGroupClone(Db, CurrentGroup);
                    break;
            }
        }

        public override void BgWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            base.BgWorkerCompleted(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = worker.EventArgs.Argument.ToString();
            if (e.Cancelled)
            {
            }
            if (e.Error != null)
            {
            }
            else
            {
                switch (command)
                {
                    case (nameof(DoGroupClone)):
                        VBGroup clonedGroup = e.Result as VBGroup;
                        AccessPrimary.NavList.Insert(0, clonedGroup);
                        OnPropertyChanged(nameof(GroupList));
                        CurrentGroup = clonedGroup;
                        break;
                }
            }
        }

        #endregion
    }
}

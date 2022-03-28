// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="BSOUser.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.manager;
using System.Data.Objects;
using System.Text.RegularExpressions;

namespace gip.bso.iplus
{
    /*
     DblClick="!AssignGroup" VBContent="SelectedGroupID"
     DblClick="!UnassignGroup" VBContent="SelectedAssignedUserGroupID"
     */
    /// <summary>
    /// Class BSOUser
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'User Management'}de{'Benutzerverwaltung'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + VBUser.ClassName)]
    public class BSOUser : ACBSONav
    {
        #region private Members
        // "Atomare Klasse" für den Zugriff auf die Datenbank (stateless)
        /// <summary>
        /// The _ user manager
        /// </summary>
        ACUserManager _UserManager = null;
        #endregion

        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="BSOUser"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOUser(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            //DatabaseMode = DatabaseModes.OwnDB;

            _StartDelimiterString = new ACPropertyConfigValue<string>(this, "StartDelimiterString", "#");
            _EndDelimiterDesignString = new ACPropertyConfigValue<string>(this, "EndDelimiterDesignString", @"""");
            _EndDelimiterFavoritesString = new ACPropertyConfigValue<string>(this, "EndDelimiterFavoritesString", "<");

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
            _UserManager = new ACUserManager(Db, Root);

            _ = _StartDelimiterString.ValueT;
            _ = _EndDelimiterDesignString.ValueT;
            _ = _EndDelimiterFavoritesString.ValueT;

            Search();
            return true;
        }

        /// <summary>
        /// ACs the de init.
        /// </summary>
        /// <param name="deleteACClassTask">if set to <c>true</c> [delete AC class task].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            _CurrentACRoot = null;
            _SelectedGroup = null;
            _SelectedAssignedUserGroup = null;
            _SelectedACProject = null;
            _SelectedAssignedVBUserACProject = null;
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

        /// <summary>
        /// Gets a value indicating whether this instance is disposable.
        /// </summary>
        /// <value><c>true</c> if this instance is disposable; otherwise, <c>false</c>.</value>
        public override bool IsPoolable
        {
            get
            {
                return false;
            }
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

        #region Properties

        #region Properties -> AccessNav
        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        /// <summary>
        /// Dies sind die vom BSO verwendeten Datenentitäten.
        /// Für die Bereitstellung für einen Service, sind diese mittels AddEntityData
        /// zu registrieren
        /// </summary>
        ACAccessNav<VBUser> _AccessPrimary;
        /// <summary>
        /// Gets the access primary.
        /// </summary>
        /// <value>The access primary.</value>
        [ACPropertyAccessPrimary(590, "VBUser")]
        public ACAccessNav<VBUser> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessPrimary = navACQueryDefinition.NewAccessNav<VBUser>("VBUser", this);
                }
                return _AccessPrimary;
            }
        }

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        /// <value>The current user.</value>
        [ACPropertyCurrent(501, "VBUser")]
        public VBUser CurrentUser
        {
            get
            {
                if (AccessPrimary == null) return null; return AccessPrimary.Current;
            }
            set
            {
                if (AccessPrimary == null) return; AccessPrimary.Current = value;
                OnPropertyChanged("CurrentUser");
                if (CurrentUser != SelectedUser)
                    SelectedUser = CurrentUser;

                OnPropertyChanged("GroupList");
                OnPropertyChanged("UserGroupList");

                OnPropertyChanged("VBUserACProjectList");
                OnPropertyChanged("ACProjectList");
            }
        }

        /// <summary>
        /// Gets or sets the selected user.
        /// </summary>
        /// <value>The selected user.</value>
        [ACPropertySelected(502, "VBUser")]
        public VBUser SelectedUser
        {
            get
            {
                if (AccessPrimary == null) return null; return AccessPrimary.Selected;
            }
            set
            {
                if (AccessPrimary == null) return; AccessPrimary.Selected = value;
                if (CurrentUser != SelectedUser)
                    CurrentUser = SelectedUser;
                OnPropertyChanged("SelectedUser");
            }
        }

        /// <summary>
        /// Gets the user list.
        /// </summary>
        /// <value>The user list.</value>
        [ACPropertyList(503, "VBUser")]
        public IEnumerable<VBUser> UserList
        {
            get
            {
                return AccessPrimary.NavList;
            }
        }

        VBUserInstance _CurrentACRoot = null;
        [ACPropertyCurrent(504, "")]
        public VBUserInstance CurrentUserInstance
        {
            get
            {
                if (CurrentUser == null)
                {
                    _CurrentACRoot = null;
                }
                else
                {
                    if (_CurrentACRoot == null || _CurrentACRoot.VBUser != CurrentUser)
                        _CurrentACRoot = CurrentUser.VBUserInstance_VBUser.FirstOrDefault();
                }
                return _CurrentACRoot;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The _ selected group
        /// </summary>
        VBGroup _SelectedGroup = null;
        /// <summary>
        /// Gets or sets the selected group.
        /// </summary>
        /// <value>The selected group.</value>
        [ACPropertySelected(505, "VBGroup")]
        public VBGroup SelectedGroup
        {
            get
            {
                return _SelectedGroup;
            }
            set
            {
                _SelectedGroup = value;
                OnPropertyChanged("SelectedGroup");
            }
        }

        /// <summary>
        /// Gets the group list.
        /// </summary>
        /// <value>The group list.</value>
        [ACPropertyList(506, "VBGroup")]
        public IEnumerable<VBGroup> GroupList
        {
            get
            {
                if (CurrentUser == null)
                    return null;
                List<VBGroup> bsos = new List<VBGroup>();

                foreach (var group in Db.VBGroup)
                {
                    if (UserGroupList.Where(c => c.VBGroup.VBGroupID == group.VBGroupID).Any())
                        continue;

                    bsos.Add(group);
                }
                return bsos;

            }
        }
        /// <summary>
        /// The _ selected assigned user group
        /// </summary>
        VBUserGroup _SelectedAssignedUserGroup;
        /// <summary>
        /// Gets or sets the selected assigned user group.
        /// </summary>
        /// <value>The selected assigned user group.</value>
        [ACPropertySelected(507, "AssignedUserGroup")]
        public VBUserGroup SelectedAssignedUserGroup
        {
            get
            {
                return _SelectedAssignedUserGroup;
            }
            set
            {
                _SelectedAssignedUserGroup = value;
                OnPropertyChanged("SelectedAssignedUserGroup");
            }
        }

        /// <summary>
        /// Gets the user group list.
        /// </summary>
        /// <value>The user group list.</value>
        [ACPropertyList(508, "AssignedUserGroup")]
        public IEnumerable<VBUserGroup> UserGroupList
        {
            get
            {
                if (CurrentUser == null)
                    return null;
                return CurrentUser.VBUserGroup_VBUser.OrderBy(c => c.VBGroup.VBGroupName).Select(c => c);
            }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The _ selected AC project
        /// </summary>
        ACProject _SelectedACProject = null;
        /// <summary>
        /// Gets or sets the selected AC project.
        /// </summary>
        /// <value>The selected AC project.</value>
        [ACPropertySelected(509, "ACProject")]
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
        /// Gets the AC project list.
        /// </summary>
        /// <value>The AC project list.</value>
        [ACPropertyList(510, "ACProject")]
        public IEnumerable<ACProject> ACProjectList
        {
            get
            {
                if (CurrentUser == null)
                    return null;
                List<ACProject> bsos = new List<ACProject>();



                foreach (var ACProject in Db.ACProject.Where(c => c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application
                                                                                    || c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Service)
                                                                         .OrderBy(c => c.ACProjectName))
                {
                    if (VBUserACProjectList.Where(c => c.ACProject.ACProjectID == ACProject.ACProjectID).Any())
                        continue;

                    bsos.Add(ACProject);
                }
                return bsos;
            }
        }
        /// <summary>
        /// The _ selected assigned VB user AC project
        /// </summary>
        VBUserACProject _SelectedAssignedVBUserACProject;
        /// <summary>
        /// Gets or sets the selected assigned VB user AC project.
        /// </summary>
        /// <value>The selected assigned VB user AC project.</value>
        [ACPropertySelected(511, "VBUserACProject")]
        public VBUserACProject SelectedAssignedVBUserACProject
        {
            get
            {
                return _SelectedAssignedVBUserACProject;
            }
            set
            {
                _SelectedAssignedVBUserACProject = value;
                OnPropertyChanged("SelectedAssignedVBUserACProject");
            }
        }

        /// <summary>
        /// Gets the VB user AC project list.
        /// </summary>
        /// <value>The VB user AC project list.</value>
        [ACPropertyList(512, "VBUserACProject")]
        public IEnumerable<VBUserACProject> VBUserACProjectList
        {
            get
            {
                if (SelectedUser == null)
                    return null;
                return SelectedUser.VBUserACProject_VBUser.OrderBy(c => c.ACProject.ACProjectName).Select(c => c);
                /*if (CurrentUserInstance == null)
                    return null;
                return CurrentUserInstance.User.UserACProject_User.OrderBy(c => c.ACProject.ACProjectName).Select(c => c);*/
            }
        }
        ///////////////////////////////////////////////////////////////////////

        #endregion


        #region Properties -> User Design replaces
        private ACPropertyConfigValue<string> _StartDelimiterString;
        [ACPropertyConfig("en{'Initial sign of separation'}de{'Erste Zeichen der Trennung'}")]
        public string StartDelimiterString
        {
            get
            {
                return _StartDelimiterString.ValueT;
            }
            set
            {
                if (_StartDelimiterString.ValueT != value)
                {
                    _StartDelimiterString.ValueT = value;
                    OnPropertyChanged(nameof(StartDelimiterString));
                }
            }
        }

        private ACPropertyConfigValue<string> _EndDelimiterDesignString;
        [ACPropertyConfig("en{'Ending sign of separation (Design)'}de{'Das letzte Zeichen der Trennung (Design)'}")]
        public string EndDelimiterDesignString
        {
            get
            {
                return _EndDelimiterDesignString.ValueT;
            }
            set
            {
                if (_EndDelimiterDesignString.ValueT != value)
                {
                    _EndDelimiterDesignString.ValueT = value;
                    OnPropertyChanged(nameof(EndDelimiterDesignString));
                }
            }
        }

        private ACPropertyConfigValue<string> _EndDelimiterFavoritesString;
        [ACPropertyConfig("en{'Ending sign of separation (Favorite)'}de{'Das letzte Zeichen der Trennung (Favorite)'}")]
        public string EndDelimiterFavoritesString
        {
            get
            {
                return _EndDelimiterFavoritesString.ValueT;
            }
            set
            {
                if (_EndDelimiterFavoritesString.ValueT != value)
                {
                    _EndDelimiterFavoritesString.ValueT = value;
                    OnPropertyChanged(nameof(EndDelimiterFavoritesString));
                }
            }
        }

        /// <summary>
        /// Source Property: 
        /// </summary>
        private string _SourceBSO;
        [ACPropertySelected(999, "SourceBSO", "en{'Source BSO'}de{'Quelle BSO'}")]
        public string SourceBSO
        {
            get
            {
                return _SourceBSO;
            }
            set
            {
                if (_SourceBSO != value)
                {
                    _SourceBSO = value;
                    OnPropertyChanged("SourceBSO");
                }
            }
        }

        /// <summary>
        /// Source Property: 
        /// </summary>
        private string _TargetBSO;
        [ACPropertySelected(999, "TargetBSO", "en{'Target BSO'}de{'Ziel BSO'}")]
        public string TargetBSO
        {
            get
            {
                return _TargetBSO;
            }
            set
            {
                if (_TargetBSO != value)
                {
                    _TargetBSO = value;
                    OnPropertyChanged("TargetBSO");
                }
            }
        }


        #endregion

        #endregion

        #region Methods
        /// <summary>
        /// Diese Methoden sind nur innerhalb der Serverseite verfügbar und können nicht
        /// direkt über Services bereitgestellt werden.
        /// Für die Bereitstellung für einen Service, sind diese mittels AddCommand
        /// zu registrieren
        /// </summary>
        [ACMethodCommand("VBUser", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            OnSave();
        }

        /// <summary>
        /// Determines whether [is enabled save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledSave()
        {
            return OnIsEnabledSave();
        }

        /// <summary>
        /// Undoes the save.
        /// </summary>
        [ACMethodCommand("VBUser", "en{'Undo'}de{'Nicht speichern'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
        public void UndoSave()
        {
            OnUndoSave();
        }

        /// <summary>
        /// Determines whether [is enabled undo save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled undo save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledUndoSave()
        {
            return OnIsEnabledUndoSave();
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        [ACMethodInteraction("VBUser", "en{'Load'}de{'Laden'}", (short)MISort.Load, false, "SelectedUser")]
        public void Load(bool requery = false)
        {
            if (!PreExecute("Load"))
                return;
            LoadEntity<VBUser>(requery, () => SelectedUser, () => CurrentUser, c => CurrentUser = c,
                        Db.VBUser.Where(c => c.VBUserID == SelectedUser.VBUserID));
            PostExecute("Load");
        }

        /// <summary>
        /// Determines whether [is enabled load].
        /// </summary>
        /// <returns><c>true</c> if [is enabled load]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledLoad()
        {
            return SelectedUser != null;
        }

        /// <summary>
        /// News this instance.
        /// </summary>
        [ACMethodInteraction("VBUser", "en{'New'}de{'Neu'}", (short)MISort.New, true, "SelectedUser")]
        public void New()
        {
            CurrentUser = _UserManager.NewUser();
            AccessPrimary.NavList.Add(CurrentUser);
            OnPropertyChanged("UserList");
        }

        /// <summary>
        /// Determines whether [is enabled new].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNew()
        {
            return true;
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        [ACMethodInteraction("VBUser", "en{'Delete'}de{'Löschen'}", (short)MISort.Delete, true, "CurrentUser")]
        public void Delete()
        {
            Msg msg = CurrentUser.DeleteACObject(Db, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }
            if (ACSaveChanges())
            {
                Search();
                CurrentUser = UserList.First();
            }
        }

        /// <summary>
        /// Determines whether [is enabled delete].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDelete()
        {
            return true;
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        [ACMethodCommand("VBUser", "en{'Search'}de{'Suchen'}", (short)MISort.Search, true)]
        public void Search()
        {
            AccessPrimary.NavSearch(Db, MergeOption.OverwriteChanges);
            OnPropertyChanged("UserList");
        }

        /// <summary>
        /// Assigns the group.
        /// </summary>
        [ACMethodInteraction("VBGroup", "en{'>'}de{'>'}", 501, true, "SelectedGroup")]
        public void AssignGroup()
        {

            if (CurrentUser == null || SelectedGroup == null)
                return;
            if (CurrentUser.VBUserGroup_VBUser.Where(c => c.VBGroup.VBGroupID == SelectedGroup.VBGroupID).Any())
                return;
            VBUserGroup userGroup = VBUserGroup.NewACObject(Db, CurrentUser);
            userGroup.VBGroup = Db.VBGroup.Where(c => c.VBGroupID == SelectedGroup.VBGroupID).Select(c => c).First();
            Db.VBUserGroup.AddObject(userGroup);
            OnPropertyChanged("GroupList");
            OnPropertyChanged("UserGroupList");
        }

        /// <summary>
        /// Determines whether [is enabled assign group].
        /// </summary>
        /// <returns><c>true</c> if [is enabled assign group]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledAssignGroup()
        {
            return CurrentUser != null && SelectedGroup != null;
        }

        /// <summary>
        /// Unassigns the group.
        /// </summary>
        [ACMethodInteraction("AssignedUserGroup", "en{'<'}de{'<'}", 502, true, "SelectedAssignedUserGroup")]
        public void UnassignGroup()
        {
            if (CurrentUser == null || SelectedAssignedUserGroup == null)
                return;

            var userGroup = CurrentUser.VBUserGroup_VBUser.Where(c => c.VBUserGroupID == SelectedAssignedUserGroup.VBUserGroupID).First();
            Msg msg = userGroup.DeleteACObject(Db, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            OnPropertyChanged("GroupList");
            OnPropertyChanged("UserGroupList");
        }

        /// <summary>
        /// Determines whether [is enabled unassign group].
        /// </summary>
        /// <returns><c>true</c> if [is enabled unassign group]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledUnassignGroup()
        {
            return CurrentUser != null && SelectedAssignedUserGroup != null;
        }

        /// <summary>
        /// Assigns the AC project.
        /// </summary>
        [ACMethodInteraction("ACProject", "en{'>'}de{'>'}", 504, true, "SelectedACProject")]
        public void AssignACProject()
        {
            if (CurrentUserInstance == null || SelectedACProject == null)
                return;
            if (CurrentUserInstance.VBUser.VBUserACProject_VBUser.Where(c => c.ACProject.ACProjectID == SelectedACProject.ACProjectID).Any())
                return;
            // Norbert: Warum muss CurrentUserInstance übergeben weren, wenn userACProject gesetzt werden muss??
            VBUserACProject userACProject = VBUserACProject.NewVBUserACProject(Db, SelectedUser, Db.ACProject.Where(c => c.ACProjectID == SelectedACProject.ACProjectID).Select(c => c).First());
            //userACProject.ACProject = Database.ACProject.Where(c => c.ACProjectID == SelectedACProject.ACProjectID).Select(c => c).First();
            userACProject.VBUser = SelectedUser;
            Db.VBUserACProject.AddObject(userACProject);
            OnPropertyChanged("ACProjectList");
            OnPropertyChanged("VBUserACProjectList");
        }

        /// <summary>
        /// Determines whether [is enabled assign AC project].
        /// </summary>
        /// <returns><c>true</c> if [is enabled assign AC project]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledAssignACProject()
        {
            return CurrentUserInstance != null && SelectedACProject != null;
        }

        /// <summary>
        /// Unassigns the AC project.
        /// </summary>
        [ACMethodInteraction("VBUserACProject", "en{'<'}de{'<'}", 505, true, "SelectedAssignedVBUserACProject")]
        public void UnassignACProject()
        {
            if (CurrentUserInstance == null || SelectedAssignedVBUserACProject == null)
                return;

            var VBUserACProject = CurrentUserInstance.VBUser.VBUserACProject_VBUser.Where(c => c.VBUserACProjectID == SelectedAssignedVBUserACProject.VBUserACProjectID).First();
            Msg msg = VBUserACProject.DeleteACObject(Db, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            OnPropertyChanged("ACProjectList");
            OnPropertyChanged("VBUserACProjectList");
        }

        /// <summary>
        /// Determines whether [is enabled unassign AC project].
        /// </summary>
        /// <returns><c>true</c> if [is enabled unassign AC project]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledUnassignACProject()
        {
            return CurrentUser != null && SelectedAssignedVBUserACProject != null;
        }


        [ACMethodInteraction("UserClone", "en{'Clone'}de{'Klonen'}", (short)MISort.New, true, "SelectedUser", Global.ACKinds.MSMethodPrePost)]

        public void UserClone()
        {
            if (!IsEnabledUserClone())
                return;
            string secondaryKey = Root.NoManager.GetNewNo(_BSODatabase, typeof(VBUser), "VBUserNo", VBUser.FormatNewNo);
            VBUser clonedUser = VBUser.NewACObject(_BSODatabase, null, secondaryKey);

            string initials = "";
            string vbUserName = "";
            int nr = 0;
            bool allowed = false;
            while (!allowed)
            {
                initials = string.Format(SelectedUser.Initials + @"{0}", nr);
                vbUserName = string.Format(SelectedUser.VBUserName + @"{0}", nr);

                allowed = !_BSODatabase.VBUser.Where(c => c.VBUserName == vbUserName || c.Initials == initials).Any();
                nr++;
            }

            clonedUser.VBUserName = vbUserName;
            clonedUser.Initials = initials;
            clonedUser.Password = SelectedUser.Password;
            clonedUser.AllowChangePW = SelectedUser.AllowChangePW;
            clonedUser.MenuACClassDesign = SelectedUser.MenuACClassDesign;
            clonedUser.VBLanguage = SelectedUser.VBLanguage;
            clonedUser.IsSuperuser = SelectedUser.IsSuperuser;
            clonedUser.XMLConfig = SelectedUser.XMLConfig;

            foreach (var prevUserProject in SelectedUser.VBUserACProject_VBUser)
            {
                VBUserACProject vBUserACProject = VBUserACProject.NewACObject(_BSODatabase, clonedUser);
                vBUserACProject.ACProject = prevUserProject.ACProject;
                vBUserACProject.IsClient = prevUserProject.IsClient;
                vBUserACProject.IsServer = prevUserProject.IsServer;
            }


            foreach (var prevUserGroup in SelectedUser.VBUserGroup_VBUser)
            {
                VBUserGroup vBUserGroup = VBUserGroup.NewACObject(_BSODatabase, clonedUser);
                vBUserGroup.VBGroup = prevUserGroup.VBGroup;
            }

            foreach (var prevUserInstance in SelectedUser.VBUserInstance_VBUser)
            {
                VBUserInstance vBUserInstance = VBUserInstance.NewACObject(_BSODatabase, clonedUser);
                vBUserInstance.IsUserDefined = prevUserInstance.IsUserDefined;
                vBUserInstance.ServerIPV4 = prevUserInstance.ServerIPV4;
                vBUserInstance.ServerIPV6 = prevUserInstance.ServerIPV6;
                vBUserInstance.ServicePortObserverHTTP = prevUserInstance.ServicePortObserverHTTP;
                vBUserInstance.ServiceAppEnbledHTTP = prevUserInstance.ServiceAppEnbledHTTP;
                vBUserInstance.ServiceAppEnabledTCP = prevUserInstance.ServiceAppEnabledTCP;
                vBUserInstance.ServiceWorkflowEnabledHTTP = prevUserInstance.ServiceWorkflowEnabledHTTP;
                vBUserInstance.ServiceWorkflowEnabledTCP = prevUserInstance.ServiceWorkflowEnabledTCP;
                vBUserInstance.ServiceObserverEnabledTCP = prevUserInstance.ServiceObserverEnabledTCP;
                vBUserInstance.Hostname = prevUserInstance.Hostname;
                vBUserInstance.NameResolutionOn = prevUserInstance.NameResolutionOn;
                vBUserInstance.UseIPV6 = prevUserInstance.UseIPV6;
                vBUserInstance.UseTextEncoding = prevUserInstance.UseTextEncoding;
            }

            _BSODatabase.VBUser.AddObject(clonedUser);
            ACSaveChanges();
            OnPropertyChanged("UserList");
            SelectedUser = clonedUser;

        }

        /// <summary>
        /// Determines whether [is enabled unassign AC project].
        /// </summary>
        /// <returns><c>true</c> if [is enabled unassign AC project]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledUserClone()
        {
            return SelectedUser != null;
        }


        [ACMethodInfo("CleanUserDesign", "en{'Clean user designs'}de{'Klar Benutzerdesigns'}", 999)]

        public void CleanUserDesign()
        {
            if (!IsEnabledCleanUserDesign())
                return;
            VBUserACClassDesign[] userClassDesigns = CurrentUser.VBUserACClassDesign_VBUser.ToArray();
            foreach (VBUserACClassDesign userClassDesign in userClassDesigns)
            {
                userClassDesign.DeleteACObject(Database, false);
                CurrentUser.VBUserACClassDesign_VBUser.Remove(userClassDesign);
            }
        }

        public bool IsEnabledCleanUserDesign()
        {
            return CurrentUser != null;
        }

        #region Methdds -> User Design replaces

        /// <summary>
        /// Source Property: ReplaceBSO
        /// </summary>
        [ACMethodInfo("ReplaceBSO", "en{'Replace'}de{'Ersetzen'}", 999)]
        public void ReplaceBSO()
        {
            if (!IsEnabledReplaceBSO())
                return;
            int matchDesign = 0;
            VBUserACClassDesign[] designs = Database.ContextIPlus.VBUserACClassDesign.ToArray();
            foreach (VBUserACClassDesign dsg in designs)
            {
                string xml = dsg.XMLDesign;
                MatchCollection matches = Regex.Matches(xml, StartDelimiterString + SourceBSO + EndDelimiterDesignString);
                matchDesign += matches.Count;
                if (matches.Count > 0)
                    xml = xml.Replace(StartDelimiterString + SourceBSO + EndDelimiterDesignString, StartDelimiterString + TargetBSO + EndDelimiterDesignString);

                matches = Regex.Matches(xml, StartDelimiterString + SourceBSO + EndDelimiterFavoritesString);
                matchDesign += matches.Count;
                if (matches.Count > 0)
                    xml = xml.Replace(StartDelimiterString + SourceBSO + EndDelimiterFavoritesString, StartDelimiterString + TargetBSO + EndDelimiterFavoritesString);

                if (matchDesign > 0)
                    dsg.XMLDesign = xml;
            }
            if (matchDesign > 0)
            {
                ACSaveChanges();
                SourceBSO = null;
                TargetBSO = null;
            }
            Messages.Info(this, "Info50083", false, matchDesign);
        }

        public bool IsEnabledReplaceBSO()
        {
            return !string.IsNullOrEmpty(SourceBSO) && !string.IsNullOrEmpty(TargetBSO);
        }


        #endregion

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Save":
                    Save();
                    return true;
                case "IsEnabledSave":
                    result = IsEnabledSave();
                    return true;
                case "UndoSave":
                    UndoSave();
                    return true;
                case "IsEnabledUndoSave":
                    result = IsEnabledUndoSave();
                    return true;
                case "Load":
                    Load(acParameter.Count() == 1 ? (Boolean)acParameter[0] : false);
                    return true;
                case "IsEnabledLoad":
                    result = IsEnabledLoad();
                    return true;
                case "New":
                    New();
                    return true;
                case "IsEnabledNew":
                    result = IsEnabledNew();
                    return true;
                case "Delete":
                    Delete();
                    return true;
                case "IsEnabledDelete":
                    result = IsEnabledDelete();
                    return true;
                case "Search":
                    Search();
                    return true;
                case "AssignGroup":
                    AssignGroup();
                    return true;
                case "IsEnabledAssignGroup":
                    result = IsEnabledAssignGroup();
                    return true;
                case "UnassignGroup":
                    UnassignGroup();
                    return true;
                case "IsEnabledUnassignGroup":
                    result = IsEnabledUnassignGroup();
                    return true;
                case "AssignACProject":
                    AssignACProject();
                    return true;
                case "IsEnabledAssignACProject":
                    result = IsEnabledAssignACProject();
                    return true;
                case "UnassignACProject":
                    UnassignACProject();
                    return true;
                case "UserClone":
                    UserClone();
                    return true;
                case "IsEnabledUnassignACProject":
                    result = IsEnabledUnassignACProject();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }
}

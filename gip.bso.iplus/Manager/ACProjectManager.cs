// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-16-2013
// ***********************************************************************
// <copyright file="ACProjectManager.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.manager;
using System.Reflection;
using System.Data.Objects;
using System.Data;

namespace gip.bso.iplus
{
    public class ACProjectManager : ACManagerBase
    {
        #region c´tors
        public ACProjectManager(IACEntityObjectContext db, IRoot root)
            : base(db, root)
        {
        }
        #endregion


        #region Sub-Classes

        public class MoveInfo
        {
            /// <summary>
            /// The moved class
            /// </summary>
            public ACClass MovedClass;
            /// <summary>
            /// The source parent class
            /// </summary>
            public ACClass SourceParentClass;
            /// <summary>
            /// The target parent class
            /// </summary>
            public ACClass TargetParentClass;
        }

        public class PresentationMode : ICloneable
        {
            public bool ShowCaptionInTree { get; set; }
            public bool DisplayGroupedTree { get; set; }
            public bool ToolWindow { get; set; }
            public ACMenuItem DisplayTreeAsMenu { get; set; }

            public bool MustRebuildTree(PresentationMode newNode)
            {
                return newNode == null
                    || this.DisplayGroupedTree != newNode.DisplayGroupedTree
                    || this.DisplayTreeAsMenu != newNode.DisplayTreeAsMenu
                    || this.ToolWindow != newNode.ToolWindow;
            }

            public override bool Equals(object obj)
            {
                PresentationMode mode = obj as PresentationMode;
                if (mode == null)
                    return base.Equals(obj);
                return ShowCaptionInTree == mode.ShowCaptionInTree
                        && DisplayGroupedTree == mode.DisplayGroupedTree
                        && DisplayTreeAsMenu == mode.DisplayTreeAsMenu
                        && ToolWindow == mode.ToolWindow;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public object Clone()
            {
                return new PresentationMode()
                {
                    ShowCaptionInTree = this.ShowCaptionInTree,
                    DisplayGroupedTree = this.DisplayGroupedTree,
                    ToolWindow = this.ToolWindow,
                    DisplayTreeAsMenu = this.DisplayTreeAsMenu
                };
            }
        }
        #endregion


        #region Precompiled-Queries
        protected static readonly Func<Database, Guid, IQueryable<ACClass>> s_cQry_AllClassesFromProject =
        CompiledQuery.Compile<Database, Guid, IQueryable<ACClass>>(
            (ctx, projectID) => ctx.ACClass.Include("ACClass1_ParentACClass")
                                 .Include("ACClass_ParentACClass")
                                 .Include("ACClass1_BasedOnACClass")
                                 .Include("ACClass_BasedOnACClass")
                                 .Include("ACProject")
                                 .Where(c => c.ACProjectID == projectID)
        );

        protected static readonly Func<Database, Guid, IQueryable<ACClass>> s_cQry_AllClassesFromProjectWithRights =
        CompiledQuery.Compile<Database, Guid, IQueryable<ACClass>>(
            (ctx, projectID) => ctx.ACClass.Include("ACClass1_ParentACClass")
                                 .Include("ACClass_ParentACClass")
                                 .Include("ACClass1_BasedOnACClass")
                                 .Include("ACClass_BasedOnACClass")
                                 .Include("ACProject")
                                 .Include("VBGroupRight_ACClass")
                                 .Where(c => c.ACProjectID == projectID)
        );
        #endregion


        #region Properties

        #region Database
        public Database Database
        {
            get
            {
                // Für "normale" Anwendungen
                return this.ObjectContext as Database;
            }
        }
        #endregion


        #region Project Objects and tree
        /// <summary>
        /// Root-Project
        /// </summary>
        /// <value>The current AC project.</value>
        public ACProject CurrentACProject
        {
            get
            {
                return _CurrentACProject;
            }
            protected set
            {
                _CurrentACProject = value;
                OnPropertyChanged("CurrentACProject");
            }
        }

        public const string CurrentProjectItemRootPropName = "CurrentProjectItemRoot";
        /// <summary>
        /// Root-Object of the project tree for presentation
        /// </summary>
        public ACClassInfoWithItems CurrentProjectItemRoot
        {
            get
            {
                return _CurrentProjectItemRoot;
            }
            protected set
            {
                _CurrentProjectItemRoot = value;
                OnPropertyChanged(CurrentProjectItemRootPropName);
            }
        }

        /// <summary>
        /// Dictionary for fast Access to presentation-objects through ACClass-Instance
        /// </summary>
        public Dictionary<ACClass, ACClassInfoWithItems> MapClassToItem
        {
            get
            {
                return _MapClassToItem;
            }
            protected set
            {
                _MapClassToItem = value;
                OnPropertyChanged("MapClassToItem");
            }
        }


        public ACProject ACProjectClassLibrary
        {
            get
            {
                if (_ACProjectClassLibrary == null)
                    _ACProjectClassLibrary = Database.ACProject.Where(c => c.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary).First();
                return _ACProjectClassLibrary;
            }
        }

        #region private
        private ACProject _CurrentACProject = null;
        private ACProject _ACProjectClassLibrary = null;

        private Dictionary<ACClass, ACClassInfoWithItems> _MapClassToItemPrevious = null;
        private Dictionary<ACClass, ACClassInfoWithItems> _MapClassToItem = null;
        ACClassInfoWithItems _CurrentProjectItemRoot;

        private ACClassInfoWithItems.VisibilityFilters _CurrentVisibilityFilters = null;
        private ACClassInfoWithItems.CheckHandler _CurrentCheckHandler = null;
        private ACProjectManager.PresentationMode _CurrentPresentationMode = null;

        protected bool DisplayGroupedTree
        {
            get
            {
                if (_CurrentPresentationMode == null)
                    return false;
                return _CurrentPresentationMode.DisplayGroupedTree;
            }
        }

        protected ACMenuItem DisplayTreeAsMenu
        {
            get
            {
                if (_CurrentPresentationMode == null)
                    return null;
                return _CurrentPresentationMode.DisplayTreeAsMenu;
            }
        }
        #endregion

        #endregion


        #region Misc

        /// <summary>
        /// Gets the show AC class list.
        /// </summary>
        /// <returns>IEnumerable{ACClass}.</returns>
        public IEnumerable<ACClass> ShowACClassList
        {
            get
            {
                if (CurrentACProject == null)
                    return null;
                if (CurrentACProject.ACProjectType != Global.ACProjectTypes.Application
                    && CurrentACProject.ACProjectType != Global.ACProjectTypes.AppDefinition
                    && CurrentACProject.ACProjectType != Global.ACProjectTypes.Service)
                    return null;

                List<ACClass> showACClassList = new List<ACClass>();
                var query = CurrentACProject.ACClass_ACProject.
                                Where(c =>    c.ACKindIndex == (short)Global.ACKinds.TPAModule
                                           || c.ACKindIndex == (short)Global.ACKinds.TPAProcessModuleGroup
                                           || c.ACKindIndex == (short)Global.ACKinds.TPAProcessModule);
                foreach (var acClassP in query)
                {
                    var assemblyACClass = acClassP.BaseClassWithASQN;
                    if (showACClassList.Contains(assemblyACClass))
                        continue;
                    showACClassList.Add(assemblyACClass);
                }

                return showACClassList.OrderBy(c => c.ACIdentifier).ThenBy(c => c.ACCaption);
            }
        }

        #endregion

        #endregion


        #region Methods

        #region Manager->Modify->ACProject

        #region Public Project Loading/Adding/Removing
        /// <summary>
        /// Loads the Class-Library into the project tree
        /// </summary>
        /// <param name="presentationMode"></param>
        /// <param name="visibilityFilter"></param>
        /// <returns></returns>
        public ACProject LoadACProject_ClassLibrary(PresentationMode presentationMode, ACClassInfoWithItems.VisibilityFilters visibilityFilter)
        {
            return LoadACProject(Database.ACProject.Where(c => c.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary).FirstOrDefault(), presentationMode, visibilityFilter);
        }

        /// <summary>
        /// Loads the Application-Definition-Project for a given application project into the project tree
        /// </summary>
        /// <param name="acProjectApp"></param>
        /// <param name="presentationMode"></param>
        /// <param name="visibilityFilter"></param>
        /// <returns></returns>
        public ACProject LoadACProject_AppDefinition(ACProject acProjectApp, PresentationMode presentationMode, ACClassInfoWithItems.VisibilityFilters visibilityFilter)
        {
            return LoadACProject(acProjectApp.ACProject1_BasedOnACProject, presentationMode, visibilityFilter);
        }

        /// <summary>
        /// Loads a project into the project tree
        /// </summary>
        /// <param name="acProjectID"></param>
        /// <param name="presentationMode"></param>
        /// <param name="visibilityFilter"></param>
        /// <param name="checkHandler"></param>
        /// <returns></returns>
        public ACProject LoadACProject(Guid acProjectID, ACProjectManager.PresentationMode presentationMode = null, ACClassInfoWithItems.VisibilityFilters visibilityFilter = null, ACClassInfoWithItems.CheckHandler checkHandler = null)
        {
            return LoadACProject(Database.ACProject.Where(c => c.ACProjectID == acProjectID).FirstOrDefault(), presentationMode, visibilityFilter, checkHandler);
        }

        public void UnloadACProject()
        {
            CurrentACProject = null;
            CurrentProjectItemRoot = null;
        }

        /// <summary>
        /// Loads a project into the project tree
        /// </summary>
        /// <param name="acProject"></param>
        /// <param name="presentationMode"></param>
        /// <param name="visibilityFilter"></param>
        /// <param name="checkHandler"></param>
        /// <returns></returns>
        public ACProject LoadACProject(ACProject acProject, ACProjectManager.PresentationMode presentationMode = null, ACClassInfoWithItems.VisibilityFilters visibilityFilter = null, ACClassInfoWithItems.CheckHandler checkHandler = null)
        {
            if (acProject == null)
            {
                ResetLoadedProject();
                return null;
            }
            CurrentACProject = acProject;

            if (checkHandler != null && checkHandler.QueryRightsFromDB)
            {
                MapClassToItem = s_cQry_AllClassesFromProjectWithRights(this.Database, acProject.ACProjectID)
                                 .ToDictionary(g => g, g => new ACClassInfoWithItems(g));
            }
            else
                MapClassToItem = s_cQry_AllClassesFromProject(this.Database, acProject.ACProjectID)
                                 .ToDictionary(g => g, g => new ACClassInfoWithItems(g));

            RefreshProjectTree(presentationMode, visibilityFilter, checkHandler);
            return CurrentACProject;
        }

        /// <summary>
        /// Refrehshes the project tree. It detects automatically if a reload of the model is needed. Otherwise only the prenstation is refreshed
        /// </summary>
        /// <param name="presentationMode"></param>
        /// <param name="visibilityFilter"></param>
        /// <param name="checkHandler"></param>
        /// <param name="forceRebuildTree"></param>
        public void RefreshProjectTree(ACProjectManager.PresentationMode presentationMode = null, ACClassInfoWithItems.VisibilityFilters visibilityFilter = null, ACClassInfoWithItems.CheckHandler checkHandler = null, bool forceRebuildTree = false)
        {
            if (CurrentACProject == null)
            {
                EliminateProjectTree();
                return;
            }
            if (presentationMode == null)
                presentationMode = new PresentationMode();

            bool rebuildTreeModel = _CurrentPresentationMode == null
                                    || _CurrentPresentationMode.MustRebuildTree(presentationMode)
                                    || _MapClassToItemPrevious != MapClassToItem
                                    || forceRebuildTree;
            bool refreshPresentation = (_CurrentVisibilityFilters == null && visibilityFilter != null)
                                        || (_CurrentVisibilityFilters != null && visibilityFilter == null)
                                        || (_CurrentVisibilityFilters != null && visibilityFilter != null && !_CurrentVisibilityFilters.Equals(visibilityFilter))
                                        || (_CurrentCheckHandler == null && checkHandler != null)
                                        || (_CurrentCheckHandler != null && checkHandler == null)
                                        || (_CurrentCheckHandler != null && checkHandler != null && !_CurrentCheckHandler.Equals(checkHandler));
            bool captionChanged = _CurrentPresentationMode != null && _CurrentPresentationMode.ShowCaptionInTree != presentationMode.ShowCaptionInTree;

            _CurrentVisibilityFilters = visibilityFilter != null ? visibilityFilter.Clone() as ACClassInfoWithItems.VisibilityFilters : null;
            _CurrentCheckHandler = checkHandler != null ? checkHandler.Clone() as ACClassInfoWithItems.CheckHandler : null;
            _CurrentPresentationMode = presentationMode != null ? presentationMode.Clone() as PresentationMode : null;

            if (rebuildTreeModel || CurrentProjectItemRoot == null)
            {
                if (_MapClassToItemPrevious != MapClassToItem)
                    _CurrentProjectItemRoot = null;
                _MapClassToItemPrevious = MapClassToItem;
                if (CurrentProjectItemRoot != null)
                    CurrentProjectItemRoot.EliminateTree();

                // Only Classlibraryproject doesn't have a root class for this project, therfore generate a Dummy
                if (CurrentACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary)
                {
                    _CurrentProjectItemRoot = new ACClassInfoWithItems("Variolibrary", presentationMode.ShowCaptionInTree, visibilityFilter, checkHandler);
                    InitTreeChildsForLibrary(CurrentProjectItemRoot, CurrentACProject.ACClass_ACProject.Where(c => c.ParentACClassID == null));
                }
                // All other project have a root class
                else
                {
                    // Get root infoItem for root class
                    ACClassInfoWithItems root = null;
                    if (!MapClassToItem.TryGetValue(CurrentACProject.RootClass, out root))
                        return;
                    if (_CurrentProjectItemRoot == null)
                        _CurrentProjectItemRoot = root;
                    // Clone root-Object to refresh Tree, othwerwise if instance not changed, then PropertyChangedCallback is not invoked
                    else
                        _CurrentProjectItemRoot = root.Clone() as ACClassInfoWithItems;
                    _CurrentProjectItemRoot.SetPresentationOptions(presentationMode.ShowCaptionInTree, visibilityFilter, checkHandler);

                    if (DisplayTreeAsMenu != null)
                    {
                        var businessObjects = CurrentACProject.RootClass.Childs.FirstOrDefault(c => c.ACIdentifier == Businessobjects.ClassName);
                        InitTreeChildsForMenu(DisplayTreeAsMenu, CurrentProjectItemRoot, businessObjects);
                    }
                    else
                    {
                        InitTreeChilds(CurrentProjectItemRoot, 0);
                    }
                }
                _CurrentProjectItemRoot.ApplyFilterAndUpdateVisibilityInTree();
                OnPropertyChanged(CurrentProjectItemRootPropName);
            }
            else if (refreshPresentation || captionChanged)
            {
                if (refreshPresentation)
                {
                    CurrentProjectItemRoot.HandlerForCheckbox = checkHandler;
                    CurrentProjectItemRoot.Filter = visibilityFilter;
                }
                if (captionChanged)
                    CurrentProjectItemRoot.ShowCaption = presentationMode.ShowCaptionInTree;
            }
        }

        /// <summary>
        /// Removes all ojects from the project tree
        /// </summary>
        public void EliminateProjectTree()
        {
            _CurrentPresentationMode = null;
            _CurrentVisibilityFilters = null;
            _CurrentCheckHandler = null;
            if (CurrentProjectItemRoot != null)
            {
                CurrentProjectItemRoot.EliminateTree();
                CurrentProjectItemRoot = null;
            }
        }


        /// <summary>
        /// News the AC project.
        /// </summary>
        /// <returns>ACProject.</returns>
        public ACProject NewACProject()
        {
            string secondaryKey = ACRoot.SRoot.NoManager.GetNewNo(Database, typeof(ACProject), ACProject.NoColumnName, ACProject.FormatNewNo, null);
            ACProject acProject = ACProject.NewACObject(Database, null, secondaryKey);
            return acProject;
        }


        /// <summary>
        /// Inits the new AC project.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool InitNewACProject(ACProject acProject)
        {
            // Prüfen, ob Projektname schon vergeben ist
            var query1 = Database.ACProject.Where(c => c.ACProjectName == acProject.ACProjectName);
            if (query1.Any())
                return false;
            Database.ACProject.AddObject(acProject);
            if (acProject.ACProject1_BasedOnACProject != null)
            {
                var query = acProject.ACProject1_BasedOnACProject.ACClass_ACProject.Where(c => c.ACClass1_ParentACClass == null);
                acProject.IsProduction = acProject.ACProject1_BasedOnACProject.IsProduction;
                acProject.IsWorkflowEnabled = acProject.ACProject1_BasedOnACProject.IsWorkflowEnabled;

                switch (acProject.ACProjectType)
                {
                    case Global.ACProjectTypes.AppDefinition:
                        acProject.IsControlCenterEnabled = false;
                        acProject.IsVisualisationEnabled = false;
                        break;
                    case Global.ACProjectTypes.Application:
                    case Global.ACProjectTypes.Service:
                        acProject.IsControlCenterEnabled = true;
                        acProject.IsVisualisationEnabled = true;
                        break;
                }
                if (query.Any())
                {
                    GenerateChildAppClasses(acProject, null, query);

                    ACClass rootClass = acProject.RootClass;
                    rootClass.ACIdentifier = acProject.ACProjectName;
                    return true;
                }
            }
            else
            {
                acProject.IsEnabled = true;
                acProject.IsGlobal = false;
                acProject.IsWorkflowEnabled = true;
                acProject.IsProduction = true;
                switch (acProject.ACProjectType)
                {
                    case Global.ACProjectTypes.AppDefinition:
                        acProject.IsControlCenterEnabled = false;
                        acProject.IsVisualisationEnabled = false;
                        break;
                    case Global.ACProjectTypes.Application:
                    case Global.ACProjectTypes.Service:
                        acProject.IsControlCenterEnabled = true;
                        acProject.IsVisualisationEnabled = true;
                        break;
                }
            }

            ACClass acClassModelServer = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == "ApplicationManager" && c.ACKindIndex == (short)Global.ACKinds.TACApplicationManager).First();
            ACClass acClass = ACClass.NewACObject(Database, acProject);
            acClass.ACIdentifier = acProject.ACProjectName;
            //acClass.ACCaptionKey = acProject.ACProjectName;
            acClass.ACClass1_BasedOnACClass = acClassModelServer;
            acClass.ACStorableType = acClassModelServer.ACStorableType;
            if (acClass.ACStorableTypeIndex != (Int16)Global.ACStorableTypes.Required) // Anwendung muss immer persistierbar sein
                acClass.ACStorableType = Global.ACStorableTypes.Required;
            acClass.ACKind = Global.ACKinds.TACApplicationManager;
            acClass.ACPackage = acClassModelServer.ACPackage;
            acClass.ACClass1_PWACClass = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == PWGroup.PWClassName && c.ACKindIndex == (short)Global.ACKinds.TPWGroup).FirstOrDefault();

            Database.ACClass.AddObject(acClass);
            return true;
        }

        /// <summary>
        /// Damit ein ACProject gelöscht werden darf, darf keine darauf basierendes ACProject existieren
        /// und wenn es sich nicht um die Klassenbibliothek oder Root handelt
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <returns><c>true</c> if [is enabled delete AC project] [the specified ac project]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACProject(ACProject acProject) // "Löschen"
        {
            if (acProject.ACProjectType == Global.ACProjectTypes.Root ||
                acProject.ACProjectType == Global.ACProjectTypes.ClassLibrary)
                return false;
            try
            {
                return !Database.ACProject.Where(c => c.ACProject1_BasedOnACProject != null && c.ACProject1_BasedOnACProject.ACProjectID == acProject.ACProjectID).Any();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                this.Root().Messages.LogException("ACProjectManager", "IsEnabledDeleteACProject", msg);

                return false;
            }
        }

        #endregion


        #region Private Tree-Building Methods

        private void ResetLoadedProject()
        {
            CurrentACProject = null;
            MapClassToItem = null;
            EliminateProjectTree();
        }

        private void InitTreeChildsForMenu(ACMenuItem menuItem, ACClassInfoWithItems parentItem, ACClass bsoACClass)
        {
            ACClassInfoWithItems info = null;
            ACClass acClass = null;

            if (!(menuItem.ACIdentifier.Equals("-") || menuItem.ACIdentifier.Equals("<Empty>")))
            {
                string className = "";
                if (!String.IsNullOrEmpty(menuItem.ACUrl))
                {
                    if (menuItem.ACUrl.Contains('#'))
                        className = menuItem.ACUrl.Split('#').Last();
                    else if (menuItem.ACUrl.Contains('!'))
                    {
                        className = menuItem.ACUrl.Split('!').First();
                        if (className.Contains('\\'))
                            className = className.Split('\\').Last();
                    }
                }

                if (!String.IsNullOrEmpty(className))
                    acClass = bsoACClass.Childs.FirstOrDefault(c => c.ACIdentifier == className);
                if (acClass != null)
                {
                    if (MapClassToItem.TryGetValue(acClass, out info))
                    {
                        info = new ACClassInfoWithItems(acClass);
                    }
                }
                else
                    info = new ACClassInfoWithItems(menuItem.ACCaption);

                if (info != null)
                {
                    info.ACCaption = menuItem.ACCaption;
                    parentItem.Add(info);
                }
            }

            if (info != null && menuItem != null && menuItem.Items != null && menuItem.Items.Any())
            {
                foreach (var childMenuItem in menuItem.Items)
                {
                    if (acClass == null)
                        acClass = bsoACClass;
                    InitTreeChildsForMenu(childMenuItem, info, acClass);
                }
            }
        }

        private void InitTreeChildsForLibrary(ACClassInfoWithItems projectItemRoot, IEnumerable<ACClass> acClassChilds)
        {
            List<ACClass> acClassList = acClassChilds.OrderBy(c => c.ACKindIndex).ThenBy(c => c.ACIdentifier).ToList();

            foreach (Global.KindInfo kindInfo in Global.KindInfoList)
            {
                if (_CurrentPresentationMode != null && _CurrentPresentationMode.ToolWindow)
                {
                    switch (kindInfo.RangeFrom)
                    {
                        case Global.ACKinds.TPAModule:
                        case Global.ACKinds.TPAProcessModuleGroup:
                        case Global.ACKinds.TPAProcessModule:
                        case Global.ACKinds.TPAProcessFunction:
                        case Global.ACKinds.TPABGModule:
                        case Global.ACKinds.TPARole:
                        case Global.ACKinds.TACDAClass:
                            break;
                        default:
                            continue;
                    }
                }

                ACClassInfoWithItems mainGroup = new ACClassInfoWithItems(kindInfo.ACCaption);
                projectItemRoot.Add(mainGroup);

                IEnumerable<ACClass> query;
                if (DisplayGroupedTree)
                {
                    switch (kindInfo.RangeFrom)
                    {
                        case Global.ACKinds.TACInterface:
                            query = acClassList.Where(c => c.ACKind >= kindInfo.RangeFrom && c.ACKind <= kindInfo.RangeTo).OrderBy(c => c.ACGroup).ThenBy(c => c.ACIdentifier);
                            break;
                        case Global.ACKinds.TACAbstractClass:
                            query = acClassList.Where(c => c.ACKind != Global.ACKinds.TACInterface && c.IsAbstract).OrderBy(c => c.ACGroup).ThenBy(c => c.ACIdentifier);
                            break;
                        default:
                            query = acClassList.Where(c => c.ACKind >= kindInfo.RangeFrom && c.ACKind <= kindInfo.RangeTo && !c.IsAbstract).OrderBy(c => c.ACGroup).ThenBy(c => c.ACIdentifier);
                            break;
                    }
                }
                else
                {
                    switch (kindInfo.RangeFrom)
                    {
                        case Global.ACKinds.TACInterface:
                            query = acClassList.Where(c => c.ACKind >= kindInfo.RangeFrom && c.ACKind <= kindInfo.RangeTo).OrderBy(c => c.ACIdentifier);
                            break;
                        case Global.ACKinds.TACAbstractClass:
                            query = acClassList.Where(c => c.ACKind != Global.ACKinds.TACInterface && c.IsAbstract).OrderBy(c => c.ACIdentifier);
                            break;
                        default:
                            query = acClassList.Where(c => c.ACKind >= kindInfo.RangeFrom && c.ACKind <= kindInfo.RangeTo && !c.IsAbstract).OrderBy(c => c.ACIdentifier);
                            break;
                    }
                }
                ACClassInfoWithItems subGroup = mainGroup;
                foreach (var acClass in query)
                {
                    if (DisplayGroupedTree
                        && (subGroup == null || subGroup.ACCaption != acClass.ACGroup))
                    {
                        subGroup = new ACClassInfoWithItems(acClass.ACGroup);
                        mainGroup.Add(subGroup);
                    }

                    // Find class in dictionary
                    ACClassInfoWithItems projectItem = null;
                    if (!MapClassToItem.TryGetValue(acClass, out projectItem))
                        continue;
                    subGroup.Add(projectItem);

                    InitTreeChilds(projectItem, 0);
                }
            }
        }

        private void InitTreeChilds(ACClassInfoWithItems parentItem, int level)
        {
            if (parentItem.ValueT == null || !parentItem.ValueT.ACClass_ParentACClass.Any())
                return;

            IEnumerable<ACClass> query;
            if (level == 1 && DisplayGroupedTree)
                query = parentItem.ValueT.ACClass_ParentACClass.OrderBy(c => c.ACKindIndex).OrderBy(c => c.ACGroup).ThenBy(c => c.ACIdentifier);
            else
            {
                //query = parentItem.ValueT.ACClass_ParentACClass.OrderBy(c => c.ACKindIndex).ThenBy(c => c.ACIdentifier);
                if (_CurrentPresentationMode.ShowCaptionInTree)
                    query = parentItem.ValueT.ACClass_ParentACClass.OrderBy(c => c.ACCaption);
                else
                    query = parentItem.ValueT.ACClass_ParentACClass.OrderBy(c => c.ACIdentifier);
            }
            ACClassInfoWithItems subGroup = parentItem as ACClassInfoWithItems;

            foreach (var acProjectClass in query)
            {
                if (level == 1)
                {
                    if (DisplayGroupedTree && (subGroup == null || subGroup.ACCaption != acProjectClass.ACGroup))
                    {
                        subGroup = new ACClassInfoWithItems(acProjectClass.ACGroup);
                        parentItem.Add(subGroup);
                    }
                }

                ACClassInfoWithItems projectItem = null;
                if (!MapClassToItem.TryGetValue(acProjectClass, out projectItem))
                    continue;
                subGroup.Add(projectItem);
                InitTreeChilds(projectItem, level + 1);
            }
        }

        public void ExpandProjectItem(ACClassInfoWithItems acClassInfoWithItems)
        {
            if (_CurrentProjectItemRoot == null)
                return;
            acClassInfoWithItems.Remove(acClassInfoWithItems.Items.FirstOrDefault());
            ACClass acClass = CurrentACProject.RootClass.Childs.FirstOrDefault(c => c.ACIdentifier == acClassInfoWithItems.ACIdentifier);
            if (acClass == null)
            {
                ACClassInfoWithItems temp = acClassInfoWithItems as ACClassInfoWithItems;
                List<string> tempACIdentifier = new List<string>();
                do
                {
                    acClass = CurrentACProject.RootClass.Childs.FirstOrDefault(c => c.ACIdentifier == temp.ACIdentifier);
                    if (acClass == null)
                        tempACIdentifier.Add(temp.ACIdentifier);
                    temp = temp.ParentACObject as ACClassInfoWithItems;
                }
                while (acClass == null);
                foreach (string identifier in tempACIdentifier.Reverse<string>())
                    acClass = acClass.Childs.FirstOrDefault(c => c.ACIdentifier == identifier);
            }

            if (acClass != null)
                InitTreeChilds(acClassInfoWithItems, 999);

            _CurrentProjectItemRoot.ApplyFilterAndUpdateVisibilityInTree();
            OnPropertyChanged(CurrentProjectItemRootPropName);
        }

        #endregion


        #endregion

        #region Manager->Modify->ACProjectRightmanagement
        /// <summary>Gets the right item info class property.</summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="groupRightList">The group right list.</param>
        /// <param name="property"></param>
        /// <returns>List{ACClassPropertyInfo}.</returns>
        public List<ACClassPropertyInfo> GetRightItemInfoClassProperty(ACClass acClass, IEnumerable<VBGroupRight> groupRightList, ACClassPropertyInfo property = null)
        {
            List<ACClassPropertyInfo> rightItemInfoClassPropertyList = new List<ACClassPropertyInfo>();
            if (acClass == null)
                return rightItemInfoClassPropertyList;

            IEnumerable<ACClassProperty> query;
            if (property == null)
                query = acClass.Properties.Where(c => c.IsRightmanagement).OrderBy(c => c.ACIdentifier);
            else
                query = acClass.Properties.Where(c => c.IsRightmanagement && c.ACIdentifier == property.ACIdentifier).OrderBy(c => c.ACIdentifier);

            foreach (var acClassProperty in query)
            {
                VBGroupRight rightOfMember = groupRightList.Where(c => c.ACClassPropertyID == acClassProperty.ACClassPropertyID).FirstOrDefault();
                rightItemInfoClassPropertyList.Add(new ACClassPropertyInfo { ValueT = acClassProperty, ControlMode = rightOfMember != null ? rightOfMember.ControlMode : Global.ControlModes.Hidden });
            }

            return rightItemInfoClassPropertyList.OrderBy(c => c.ValueT.ACCaption).ToList();
        }

        /// <summary>Gets the right item info class method.</summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="groupRightList">The group right list.</param>
        /// <param name="method"></param>
        /// <returns>List{ACClassMethodInfo}.</returns>
        public List<ACClassMethodInfo> GetRightItemInfoClassMethod(ACClass acClass, IEnumerable<VBGroupRight> groupRightList, ACClassMethodInfo method = null)
        {
            List<ACClassMethodInfo> rightItemInfoClassMethodList = new List<ACClassMethodInfo>();
            if (acClass == null)
                return rightItemInfoClassMethodList;

            IEnumerable<ACClassMethod> query;
            if (method == null)
                query = acClass.Methods.Where(c => c.IsRightmanagement).OrderBy(c => c.ACIdentifier);
            else
                query = acClass.Methods.Where(c => c.IsRightmanagement && c.ACIdentifier == method.ACIdentifier).OrderBy(c => c.ACIdentifier);

            foreach (var acClassMethod in query)
            {
                VBGroupRight rightOfMember = groupRightList.Where(c => c.ACClassMethodID == acClassMethod.ACClassMethodID).FirstOrDefault();
                rightItemInfoClassMethodList.Add(new ACClassMethodInfo { ValueT = acClassMethod, ControlMode = rightOfMember != null ? rightOfMember.ControlMode : Global.ControlModes.Hidden });
            }
            return rightItemInfoClassMethodList.OrderBy(c => c.ValueT.ACCaption).ToList();
        }

        /// <summary>
        /// Gets the right item info class design.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="groupRightList">The group right list.</param>
        /// <returns>List{ACClassDesignInfo}.</returns>
        public List<ACClassDesignInfo> GetRightItemInfoClassDesign(ACClass acClass, IEnumerable<VBGroupRight> groupRightList)
        {
            List<ACClassDesignInfo> rightItemInfoClassDesignList = new List<ACClassDesignInfo>();
            if (acClass == null)
                return rightItemInfoClassDesignList;
            var query = acClass.Designs.Where(c => c.IsRightmanagement).OrderBy(c => c.ACIdentifier);
            foreach (var acClassDesign in query)
            {
                VBGroupRight rightOfMember = groupRightList.Where(c => c.ACClassDesignID == acClassDesign.ACClassDesignID).FirstOrDefault();
                rightItemInfoClassDesignList.Add(new ACClassDesignInfo { ValueT = acClassDesign, ControlMode = rightOfMember != null ? rightOfMember.ControlMode : Global.ControlModes.Hidden });
            }

            return rightItemInfoClassDesignList.OrderBy(c => c.ValueT.ACKindIndex).ThenBy(c => c.ValueT.ACCaption).ToList();
        }
        #endregion

        #region Manager->Modify->ACClass
        /// <summary>
        /// Inserts the project item.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="parentProjectItem">The parent project item.</param>
        /// <returns>ACClassInfoWithItems.</returns>
        public ACClassInfoWithItems InsertProjectItem(ACClass acClass, ACClassInfoWithItems parentProjectItem)
        {
            Database.ACClass.AddObject(acClass);
            ACClassInfoWithItems projectItem = null;
            if (this.MapClassToItem.TryGetValue(acClass, out projectItem))
                return projectItem;
            projectItem = new ACClassInfoWithItems { ValueT = acClass };
            parentProjectItem.Add(projectItem);
            this.MapClassToItem.Add(acClass, projectItem);

            return projectItem;
        }

        /// <summary>
        /// News the AC class.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="parentProjectItem">The parent project item.</param>
        /// <returns>ACClass.</returns>
        public ACClass NewACClass(ACProject acProject, ACClassInfoWithItems parentProjectItem)
        {
            ACClass acClass = ACClass.NewACObject(Database, acProject);
            acClass.ACClass1_BasedOnACClass = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == "ACGenericComponent" && c.ACKindIndex == (short)Global.ACKinds.TACClass).First();
            if (parentProjectItem.ValueT != null && parentProjectItem.ValueT.ACClass1_ParentACClass != null)
                acClass.ACClass1_ParentACClass = parentProjectItem.ValueT.ACClass1_ParentACClass as ACClass;

            return acClass;
        }

        /// <summary>
        /// Determines whether [is enabled new AC class] [the specified ac project].
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="parentProjectItem">The parent project item.</param>
        /// <returns><c>true</c> if [is enabled new AC class] [the specified ac project]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClass(ACProject acProject, ACClassInfoWithItems parentProjectItem)
        {
            if (acProject == null || parentProjectItem == null)
                return false;

            if (acProject.ACProjectType != Global.ACProjectTypes.ClassLibrary)
            {
                ACClass acClass1 = parentProjectItem.ValueT;
                if (acClass1 == null)
                    return false;
                if (acClass1.ACClass1_ParentACClass == null)
                    return false;
                switch (parentProjectItem.ValueT.ACKind)
                {
                    case Global.ACKinds.TACDBAManager:
                    case Global.ACKinds.TACDBA:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// News the child AC class.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="parentProjectItem">The parent project item.</param>
        /// <returns>ACClass.</returns>
        public ACClass NewChildACClass(ACProject acProject, ACClassInfoWithItems parentProjectItem)
        {
            ACClass parentClass = parentProjectItem.Value as ACClass;
            ACClass acClass = ACClass.NewACObject(Database, acProject);
            acClass.ACClass1_BasedOnACClass = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == "ACGenericComponent" && c.ACKindIndex == (short)Global.ACKinds.TACClass).First();
            acClass.ACClass1_ParentACClass = parentProjectItem.ValueT as ACClass;
            if (parentClass != null)
            {
                acClass.ACPackageID = parentClass.ACPackageID;
                acClass.ParentACClassID = parentClass.ACClassID;
            }
            return acClass;
        }

        /// <summary>
        /// Determines whether [is enabled new child AC class] [the specified ac project].
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="parentProjectItem">The parent project item.</param>
        /// <returns><c>true</c> if [is enabled new child AC class] [the specified ac project]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewChildACClass(ACProject acProject, ACClassInfoWithItems parentProjectItem)
        {
            if (acProject == null || parentProjectItem == null)
                return false;

            ACClass acClass1 = parentProjectItem.ValueT;
            if (acClass1 == null)
                return false;

            switch (acClass1.ACKind)
            {
                case Global.ACKinds.TACDBAManager:
                case Global.ACKinds.TACDBA:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// News the child AC class.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="parentProjectItem">The parent project item.</param>
        /// <returns>ACClass.</returns>
        public bool MoveUpInTree(ACProject acProject, ACClassInfoWithItems parentProjectItem)
        {
            ACClass thisClass = parentProjectItem.Value as ACClass;
            ACClass parentClass = thisClass;
            if (parentClass != null)
            {
                parentClass = parentClass.ACClass1_ParentACClass;
                if (parentClass != null)
                {
                    parentClass = parentClass.ACClass1_ParentACClass;
                    if (parentClass != null)
                    {
                        thisClass.ACClass1_ParentACClass = parentClass;
                        if (!String.IsNullOrEmpty(thisClass.ACURLComponentCached))
                            thisClass.RefreshChildrenACURLCache();
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is enabled new child AC class] [the specified ac project].
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="parentProjectItem">The parent project item.</param>
        /// <returns><c>true</c> if [is enabled new child AC class] [the specified ac project]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledMoveUpInTree(ACProject acProject, ACClassInfoWithItems parentProjectItem)
        {
            if (acProject == null || parentProjectItem == null)
                return false;

            ACClass acClass1 = parentProjectItem.ValueT;
            if (acClass1 == null || acClass1.ACClass1_ParentACClass == null)
                return false;
            return true;
        }

        /// <summary>
        /// Inits the AC class.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        public void InitACClass(ACClass acClass)
        {
            acClass.ACCaption = acClass.ACIdentifier;
            acClass.ACPackage = acClass.ACClass1_BasedOnACClass.ACPackage;
            acClass.ACKindIndex = acClass.ACClass1_BasedOnACClass.ACKindIndex;
            acClass.PWACClassID = acClass.ACClass1_BasedOnACClass.PWACClassID;
            acClass.ACStartTypeIndex = acClass.ACClass1_BasedOnACClass.ACStartTypeIndex;
            acClass.ACStorableTypeIndex = acClass.ACClass1_BasedOnACClass.ACStorableTypeIndex;
            acClass.IsRightmanagement = acClass.ACClass1_BasedOnACClass.IsRightmanagement;
        }

        /// <summary>
        /// Generates the child app classes.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="parentACClass">The parent AC class.</param>
        /// <param name="acClassTemplateList">The ac class template list.</param>
        private void GenerateChildAppClasses(ACProject acProject, ACClass parentACClass, IEnumerable<ACClass> acClassTemplateList)
        {
            foreach (var acClassTemplate in acClassTemplateList)
            {
                ACClass acClass = ACClass.NewACObjectWithBaseclass(Database, acProject, acClassTemplate);
                acClass.ACClass1_ParentACClass = parentACClass;
                if (parentACClass == null)
                {
                    switch (acClass.ACKind)
                    {
                        case Global.ACKinds.TPAModule:
                        case Global.ACKinds.TPAProcessModuleGroup:
                        case Global.ACKinds.TPAProcessModule:
                            acClass.ACIdentifier += "1";
                            break;
                        case Global.ACKinds.TPAProcessFunction:
                        case Global.ACKinds.TPABGModule:
                            break;
                    }
                }
                Database.ACClass.AddObject(acClass);
                GenerateChildAppClasses(acProject, acClass, acClassTemplate.Childs);
            }
        }

        /// <summary>
        /// Bases the AC class list.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="database">The database.</param>
        /// <returns>IEnumerable{ACClass}.</returns>
        public IEnumerable<ACClass> BaseACClassList(ACProject acProject, ACClass acClass, Database database)
        {
            // Aus sicherheitsgründen wird derzeit eine Liste erzeugt und sichergestellt, das 
            // der aktuelle Eintrag auch wirklich in der Liste vorhanden ist.
            List<ACClass> _BaseACClassList = new List<ACClass>();
            if (acClass == null)
                return _BaseACClassList;

            switch (acProject.ACProjectType)
            {
                case Global.ACProjectTypes.ClassLibrary:
                    _BaseACClassList = GetPublicACClassesLib(acProject, acClass, database).ToList();
                    break;
                case Global.ACProjectTypes.Root:
                    _BaseACClassList = GetPublicACClasses(acProject, acClass, database).ToList();
                    break;
                case Global.ACProjectTypes.AppDefinition:
                    _BaseACClassList = GetTPAppACClasses(acProject, acClass, database).ToList();
                    break;
                case Global.ACProjectTypes.Application:
                case Global.ACProjectTypes.Service:
                    {
                        if (acProject.ACProject1_BasedOnACProject != null)
                        {
                            if (acClass.ACClass1_ParentACClass != null &&
                                acClass.ACClass1_ParentACClass.ACClass1_BasedOnACClass != null &&
                                acClass.ACClass1_ParentACClass.ACClass1_BasedOnACClass.ACClass_ParentACClass != null)
                            {
                                _BaseACClassList = acClass.ACClass1_ParentACClass.ACClass1_BasedOnACClass.ACClass_ParentACClass.OrderBy(c => c.FullClassCaption).ToList();
                            }
                            else
                            {
                                if (acProject.ACProject1_BasedOnACProject != null)
                                {
                                    _BaseACClassList.Add(acProject.ACProject1_BasedOnACProject.RootClass);
                                }
                            }
                            foreach (var acClass1 in GetTPAppACClasses(acProject, acClass, database))
                            {
                                _BaseACClassList.Add(acClass1);
                            }
                        }
                        else
                        {
                            _BaseACClassList = GetTPAppACClasses(acProject, acClass, database).ToList();
                        }
                    }
                    break;
            }

            if (acClass.ACClass1_BasedOnACClass != null)
            {
                if (!_BaseACClassList.Contains(acClass.ACClass1_BasedOnACClass))
                {
                    _BaseACClassList.Add(acClass.ACClass1_BasedOnACClass);
                }
            }

            return _BaseACClassList.OrderBy(c => c.FullClassCaption);
        }

        /// <summary>
        /// Gets the TP app AC classes.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="database">The database.</param>
        /// <returns>IEnumerable{ACClass}.</returns>
        IEnumerable<ACClass> GetTPAppACClasses(ACProject acProject, ACClass acClass, Database database)
        {
            if (acClass == acProject.RootClass
                && acClass.ACKind == Global.ACKinds.TACApplicationManager
                && acClass.ACProject.ACProjectType == Global.ACProjectTypes.AppDefinition)
            {
                return Database.ACClass.Where(c => c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary
                                                && c.ACKindIndex == (short)Global.ACKinds.TACApplicationManager)
                                        .OrderBy(c => c.ACIdentifier);
            }
            else if (acClass == acProject.RootClass
                    && acClass.ACKind == Global.ACKinds.TACApplicationManager
                    && acClass.ACProject.ACProjectType == Global.ACProjectTypes.Application)
            {
                return Database.ACClass.Where(c => c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.AppDefinition
                                                    && c.ACKindIndex == (short)Global.ACKinds.TACApplicationManager)
                                        .OrderBy(c => c.ACIdentifier);
            }
            else
            {
                return Database.ACClass.Where(c => (c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary
                                                && (c.ACKindIndex == (short)Global.ACKinds.TPAModule
                                                     || c.ACKindIndex == (short)Global.ACKinds.TPAProcessModule
                                                     || c.ACKindIndex == (short)Global.ACKinds.TPAProcessModuleGroup
                                                     || c.ACKindIndex == (short)Global.ACKinds.TPAProcessFunction
                                                     || c.ACKindIndex == (short)Global.ACKinds.TPABGModule
                                                     || c.ACKindIndex == (short)Global.ACKinds.TACRuntimeDump
                                                     || c.ACKindIndex == (short)Global.ACKinds.TACDAClass))
                                            || (c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Root
                                                && (c.ACKindIndex == (short)Global.ACKinds.TACQRY
                                                     || c.ACKindIndex == (short)Global.ACKinds.TACRuntimeDump)))
                .OrderBy(c => c.ACIdentifier);
            }
        }

        /// <summary>
        /// Gets the public AC classes lib.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="database">The database.</param>
        /// <returns>IEnumerable{ACClass}.</returns>
        IEnumerable<ACClass> GetPublicACClassesLib(ACProject acProject, ACClass acClass, Database database)
        {
            return Database.ACClass.Where(c => c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary && c.ACClass1_ParentACClass == null).OrderBy(c => c.ACIdentifier);
        }

        /// <summary>
        /// Gets the public AC classes.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="database">The database.</param>
        /// <returns>IEnumerable{ACClass}.</returns>
        IEnumerable<ACClass> GetPublicACClasses(ACProject acProject, ACClass acClass, Database database)
        {
            if (acClass.ACClass1_ParentACClass != null &&
                acClass.ACClass1_ParentACClass.BaseClassWithASQN != null &&
                acClass.ACClass1_ParentACClass.BaseClassWithASQN.ACIdentifier == "VBBSOReportDialog")
            {
                return Database.ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.TACQRY &&
                    c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Root).OrderBy(c => c.ACIdentifier).ToList();
            }

            return Database.ACClass.Where(c => (c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary
                                                    && c.ACClass1_ParentACClass == null)
                                              || c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Root).OrderBy(c => c.ACIdentifier);
        }

        #region DragAndDrop
        /// <summary>
        /// Drops the AC class.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="action">The action.</param>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="targetVBDataObject">The target VB data object.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>ACClassInfoWithItems.</returns>
        public ACClassInfoWithItems DropACClass(ACProject acProject, Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            ACClass targetClassItem = targetVBDataObject.GetACValue(typeof(ACClass)) as ACClass;
            ACClass dropClassItem = dropObject.GetACValue(typeof(ACClass)) as ACClass;

            if (dropClassItem == null)
                return null;

            switch (action)
            {
                case Global.ElementActionType.Move: // Verschieben eines bestehenden Elements
                    return DropMoveACClass(acProject, dropObject, targetVBDataObject);
                case Global.ElementActionType.Drop: // Einfügen neues Element
                    {
                        ACClassInfoWithItems acClassInfoWithItems = targetVBDataObject.GetACValue(typeof(ACClassInfoWithItems)) as ACClassInfoWithItems;

                        if (acClassInfoWithItems == null)
                            return null;

                        ACClass acClass = NewChildACClass(acProject, acClassInfoWithItems);
                        acClass.ACCaptionTranslation = dropClassItem.ACCaptionTranslation;
                        ACClassInfoWithItems newProjectItem = InsertProjectItem(acClass, acClassInfoWithItems);
                        ACClass newACClass = newProjectItem.ValueT;

                        newACClass.ACClass1_BasedOnACClass = dropClassItem;
                        newACClass.ACStorableType = dropClassItem.ACStorableType;
                        newACClass.ACIdentifier = dropClassItem.ACIdentifier;
                        newACClass.ACStartType = dropClassItem.ACStartType;
                        newACClass.ACPackage = dropClassItem.ACPackage;
                        newACClass.IsRightmanagement = dropClassItem.IsRightmanagement;
                        StripAndCheckACClassName(newACClass, dropClassItem.ACProject.ACProjectType, true);
                        newProjectItem.ACCaption = newACClass.ACIdentifier;

                        switch (dropClassItem.ACProject.ACProjectType)
                        {
                            case Global.ACProjectTypes.ClassLibrary:
                                CopyChildClassesClassLibrary(acProject, dropClassItem, newProjectItem);
                                break;
                            case Global.ACProjectTypes.AppDefinition:
                                CopyChildClassesApplication(acProject, dropClassItem, newProjectItem);
                                break;
                        }
                        return newProjectItem;
                    }
            }
            return null;
        }

        /// <summary>
        /// Verschieben von Klassen in Anwendung und Anwendungsdefinition
        /// Im Projekttree kann mittels "Shift" und DragAndDrop verschoben werden.
        /// !!! ACHTUNG !!!
        /// Diese Funktion sollte nur nach gründlicher Überlegung durchgeführt werden,
        /// insbesondere wenn das System schon Live ist.
        /// Ausschlußkriterien:
        /// 1. Verschieben der Root-Klasse NICHT MÖGLICH
        /// 2. Verschieben einer Klasse zu einer untergeordneten Klasse NICHT MÖGLICH
        /// 3. Verschieben von Klassen innerhalb der Anwendung NICHT MÖGLICH, wenn deren Struktur
        /// bereits in der Anwendungsdefinition festgelegt wurde.
        /// Weitere Infos:
        /// 1. Wird in der Anwendungsdefinition eine Klasse verschoben, kommt beim Speichern die Meldung
        /// "Update Application from Appdefinition {0} ?"
        /// Beim bejahen, wird auch bei den Anwendungen die Klasse umgehängt.
        /// 2. Sollte beim umhängen in der Anwendung festgestellt werden, das es mehrere Ziel-Klassen
        /// gibt, dann wird die verschieben zur ersten Klassen in der Anwendung durchgeführt.
        /// Danach erscheint dann die Meldung:
        /// "Check Application, because their are more derived Classes from moved Class(es) "{0}" ?"
        /// Die Anwendungen sind dann manuell nachzuarbeiten, damit auch an den anderen Klassen
        /// die verschobene Klasse zugeordnet wird.
        /// 3. Sollten schon Workflows existieren, kann das Verschieben dazu führen, das ein Workflow
        /// nicht mehr funktioniert
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="targetVBDataObject">The target VB data object.</param>
        /// <returns>ACClassInfoWithItems.</returns>
        private ACClassInfoWithItems DropMoveACClass(ACProject acProject, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject)
        {
            Type t = typeof(System.Windows.Controls.TreeViewItem);
            PropertyInfo pi = t.GetProperty("ParentItemsControl", BindingFlags.NonPublic | BindingFlags.Instance);

            IACInteractiveObject parentObject = pi.GetValue(dropObject, null) as IACInteractiveObject;

            ACClass targetClassItem = targetVBDataObject.GetACValue(typeof(ACClass)) as ACClass;
            ACClass dropClassItem = dropObject.GetACValue(typeof(ACClass)) as ACClass;
            ACClass parentClassItem = parentObject.GetACValue(typeof(ACClass)) as ACClass;

            ACClassInfoWithItems dropItem = dropObject.ACContentList.First() as ACClassInfoWithItems;
            ACClassInfoWithItems targetItem = targetVBDataObject.ACContentList.First() as ACClassInfoWithItems;
            ACClassInfoWithItems parentItem = parentObject.ACContentList.First() as ACClassInfoWithItems;

            parentItem.Remove(dropItem);
            targetItem.Add(dropItem);

            dropClassItem.ACClass1_ParentACClass = targetClassItem;


            return dropObject.ACContentList.First() as ACClassInfoWithItems;
        }

        /// <summary>
        /// Strips the name of the and check AC class.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acProjectTypeSource">The ac project type source.</param>
        /// <param name="isDroppedClass">if set to <c>true</c> [is dropped class].</param>
        private void StripAndCheckACClassName(ACClass acClass, Global.ACProjectTypes acProjectTypeSource, bool isDroppedClass)
        {
            if (acClass.ACIdentifier.StartsWith("PAF") || acClass.ACIdentifier.StartsWith("PAM") || acClass.ACIdentifier.StartsWith("PAE"))
            {
                acClass.ACIdentifier = acClass.ACIdentifier.Substring(3);
            }

            // Unterinstanzen nicht umbenenen, da evtl. Logik im Basisimplementierung oder Binding im Design vorhanden sind
            if (!isDroppedClass)
                return;
            // Bei einer Anwendung wird über die Instanznummer die Eindeutigkeit definiert
            // 1. Einfügen von einer Anwendungsdefintion in eine Anwendung
            //if (acProjectTypeSource == Global.ACProjectTypes.ACAppDefinition)
            //{
            //    if (acClass.ACClass1_ParentACClass != null)
            //    {
            //        bool isUnique = true;
            //        int checkACClassInstanceNo = acClass.ACClassInstanceNo;
            //        do
            //        {
            //            isUnique = true;
            //            foreach (var acClassCheck in acClass.ACClass1_ParentACClass.ACEntityChilds)
            //            {
            //                if (acClassCheck == acClass)
            //                    continue;
            //                if (acClassCheck.ACIdentifier == acClass.ACIdentifier && acClassCheck.ACClassInstanceNo == checkACClassInstanceNo)
            //                    isUnique = false;
            //            }
            //            if (!isUnique)
            //            {
            //                checkACClassInstanceNo++;
            //            }
            //        } while (!isUnique);

            //        if (acClass.ACClassInstanceNo != checkACClassInstanceNo)
            //        {
            //            acClass.ACClassInstanceNo = checkACClassInstanceNo;
            //        }
            //    }
            //}
            // Bei einer Anwendungsdefinition wird über die Instanznummer die Eindeutigkeit definiert
            // 1. Einfügen von einer Klassenbibliothek in eine Anwendungsdefinition
            // 2. Einfügen von einer Klassenbibliothek in eine Anwendung
            if (acProjectTypeSource == Global.ACProjectTypes.ClassLibrary ||
                acProjectTypeSource == Global.ACProjectTypes.AppDefinition)
            {
                if (acClass.ACClass1_ParentACClass != null)
                {
                    bool isUnique = true;
                    int checkCount = 0;
                    string checkACClassName;
                    switch (acClass.ACKind)
                    {
                        case Global.ACKinds.TPAProcessModule:
                        case Global.ACKinds.TPAProcessModuleGroup:
                        case Global.ACKinds.TPAModule:
                            checkCount = 1;
                            checkACClassName = acClass.ACIdentifier + checkCount.ToString();
                            break;
                        case Global.ACKinds.TPAProcessFunction:
                        case Global.ACKinds.TPABGModule:
                        default:
                            checkACClassName = acClass.ACIdentifier;
                            break;
                    }

                    do
                    {
                        isUnique = true;
                        foreach (var acClassCheck in acClass.ACClass1_ParentACClass.Childs)
                        {
                            if (acClassCheck == acClass)
                                continue;
                            if (acClassCheck.ACIdentifier == checkACClassName)
                                isUnique = false;
                        }
                        if (!isUnique)
                        {
                            checkCount++;
                            checkACClassName = acClass.ACIdentifier + checkCount.ToString();
                        }
                    } while (!isUnique);

                    if (acClass.ACIdentifier != checkACClassName)
                    {
                        acClass.ACIdentifier = checkACClassName;
                        //acClass.ACCaptionKey = acClass.ACCaptionKey + "(" + checkCount.ToString() + ")";
                    }
                }
            }
        }
        /// <summary>
        /// Determines whether [is enabled drop AC class] [the specified ac project].
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="action">The action.</param>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="targetVBDataObject">The target VB data object.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if [is enabled drop AC class] [the specified ac project]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDropACClass(ACProject acProject, Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            ACClass targetClassItem = targetVBDataObject.GetACValue(typeof(ACClass)) as ACClass;
            ACClass dropClassItem = dropObject.GetACValue(typeof(ACClass)) as ACClass;

            if (dropClassItem == null)
                return false;

            switch (action)
            {
                case Global.ElementActionType.Move: // Verschieben eines bestehenden Elements
                    return IsEnabledDropMoveACClass(acProject, targetClassItem, dropClassItem);
                case Global.ElementActionType.Drop: // Einfügen neues Element
                    return IsEnabledDropCopyACClass(acProject, targetClassItem, dropClassItem);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether [is enabled drop copy AC class] [the specified ac project].
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="targetClassItem">The target class item.</param>
        /// <param name="dropClassItem">The drop class item.</param>
        /// <returns><c>true</c> if [is enabled drop copy AC class] [the specified ac project]; otherwise, <c>false</c>.</returns>
        private bool IsEnabledDropCopyACClass(ACProject acProject, ACClass targetClassItem, ACClass dropClassItem)
        {
            switch (dropClassItem.ACProject.ACProjectType)
            {
                case Global.ACProjectTypes.ClassLibrary:
                    if (dropClassItem.ACClass1_ParentACClass == null)
                    {
                        // Wenn kein Parent vorhanden, dann ist es eine freie Klasse die eingefügt werden darf
                        return true;
                    }
                    else
                    {
                        if ((targetClassItem != null) && (dropClassItem.ACClass1_ParentACClass == targetClassItem.ACClass1_BasedOnACClass))
                        {
                            // Wenn ein Parent vorhanden und und die Parentklassen gleich, dann darf eingefügt werden
                            if (dropClassItem.ACClass1_BasedOnACClass.ACKind == Global.ACKinds.TPAProcessFunction)
                            {
                                if (!targetClassItem.ACClass_ParentACClass.Where(c => c.ACClass1_BasedOnACClass == dropClassItem.ACClass1_BasedOnACClass).Any())
                                    return true; // Wenn kein gleichartiges Child schon vorhanden ist, dann ist einfügen möglich
                                else
                                    return false; // Wenn ein gleichartiges Child schon vorhanden ist, dann ist einfügen nicht möglich
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            // Wenn ein Parent vorhanden und und die Parentklassen nicht gleich, dann darf nicht eingefügt werden
                            return false;
                        }
                    }
                case Global.ACProjectTypes.AppDefinition:
                    if (dropClassItem.ACClass1_ParentACClass == null)
                        return false;
                    if (targetClassItem != null
                        && (targetClassItem.ACClass1_BasedOnACClass == dropClassItem.ACClass1_ParentACClass
                           || targetClassItem.ACClass1_BasedOnACClass == dropClassItem.ACClass1_ParentACClass.ACClass1_BasedOnACClass))
                        return true;
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether [is enabled drop move AC class] [the specified ac project].
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="targetClassItem">The target class item.</param>
        /// <param name="dropClassItem">The drop class item.</param>
        /// <returns><c>true</c> if [is enabled drop move AC class] [the specified ac project]; otherwise, <c>false</c>.</returns>
        private bool IsEnabledDropMoveACClass(ACProject acProject, ACClass targetClassItem, ACClass dropClassItem)
        {
            if (targetClassItem == null)
                return false;
            // Nur innerhalb eines Projekts verschieben
            if (acProject != targetClassItem.ACProject || acProject != dropClassItem.ACProject)
                return false;
            // Root-Klasse kann nicht verschoben werden
            if (dropClassItem == dropClassItem.ACProject.RootClass)
                return false;
            // Nicht auf sich selbst droppen
            if (targetClassItem == dropClassItem)
                return false;
            // Nicht wenn es schon an dem Parent hängt
            if (dropClassItem.ParentACObject == targetClassItem)
                return false;

            // Darf nicht unter einen seiner Childs gehängt werden
            if (!CheckCollision(dropClassItem, targetClassItem))
                return false;

            switch (acProject.ACProjectType)
            {
                case Global.ACProjectTypes.Application:
                case Global.ACProjectTypes.Service:
                    // Wenn Struktur schon in Anwendungsdefinition festgelegt, dann darf nicht verschoben werden
                    if (dropClassItem.ACClass1_BasedOnACClass.ACProject.ACProjectType == Global.ACProjectTypes.AppDefinition)
                        return false;
                    break;
                case Global.ACProjectTypes.AppDefinition:
                    // Wenn Struktur schon in Klassenbibliothek definiert, dann darf nicht verschoben werden
                    {
                        var bc = dropClassItem.ACClass1_BasedOnACClass; // Basisklasse in Klassenbibliothek
                        var pbc = dropClassItem.ACClass1_ParentACClass.ACClass1_BasedOnACClass; // Parent-Basisklasse in Klassenbibliothek
                        if (bc != null && pbc != null && bc.ACClass1_ParentACClass == pbc)
                        {
                            return false;
                        }
                    }
                    break;
                default:
                    // Nur bei Anwendungen und Anwendungsdefinitionen
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks the collision.
        /// </summary>
        /// <param name="dropClassItem">The drop class item.</param>
        /// <param name="targetClassItem">The target class item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private bool CheckCollision(ACClass dropClassItem, ACClass targetClassItem)
        {
            foreach (var item in dropClassItem.Childs)
            {
                if (item == targetClassItem)
                    return false;
                if (!CheckCollision(item, targetClassItem))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Kopieren der Child-Klassen als KOPIEN der Child-Klassen der Klassenbibliothek
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="sourceACClass">The source AC class.</param>
        /// <param name="targetProjectItem">The target project item.</param>
        public void CopyChildClassesClassLibrary(ACProject acProject, ACClass sourceACClass, ACClassInfoWithItems targetProjectItem)
        {
            if (sourceACClass.ACClass_ParentACClass != null && sourceACClass.ACClass_ParentACClass.Any())
            {
                foreach (var childACClass in sourceACClass.ACClass_ParentACClass)
                {
                    ACClass acClass = NewChildACClass(acProject, targetProjectItem);
                    ACClassInfoWithItems newProjectItem = InsertProjectItem(acClass, targetProjectItem);
                    ACClass newACClass = newProjectItem.ValueT;
                    ACClass parentACClass = targetProjectItem.ValueT;
                    newACClass.CopyAll(childACClass);
                    StripAndCheckACClassName(newACClass, childACClass.ACProject.ACProjectType, false);
                    newProjectItem.ACCaption = newACClass.ACIdentifier;

                    CopyChildClassesClassLibrary(acProject, childACClass, newProjectItem);
                }
            }

        }

        /// <summary>
        /// Kopieren der Child-Klassen als ABLEITUNDEN der Child-Klassen der Anwendungsdefinition
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="sourceACClass">The source AC class.</param>
        /// <param name="targetProjectItem">The target project item.</param>
        public void CopyChildClassesApplication(ACProject acProject, ACClass sourceACClass, ACClassInfoWithItems targetProjectItem)
        {
            if (sourceACClass.ACClass_ParentACClass != null && sourceACClass.ACClass_ParentACClass.Any())
            {
                foreach (var childACClass in sourceACClass.ACClass_ParentACClass)
                {
                    ACClass acClass = NewChildACClass(acProject, targetProjectItem);
                    ACClassInfoWithItems newProjectItem = InsertProjectItem(acClass, targetProjectItem);

                    ACClass newACClass = newProjectItem.ValueT;
                    newACClass.ACIdentifier = childACClass.ACIdentifier;
                    //newACClass.ACCaptionKey = childACClass.ACCaptionKey;
                    newACClass.ACClass1_BasedOnACClass = childACClass;
                    newACClass.ACStorableType = childACClass.ACStorableType;
                    newACClass.ACKind = childACClass.ACKind;
                    newACClass.ACStartType = childACClass.ACStartType;
                    newACClass.ACPackage = childACClass.ACPackage;
                    newACClass.ACCaptionTranslation = childACClass.ACCaptionTranslation;
                    newACClass.IsRightmanagement = childACClass.IsRightmanagement;
                    StripAndCheckACClassName(newACClass, childACClass.ACProject.ACProjectType, false);
                    newProjectItem.ACCaption = newACClass.ACIdentifier;

                    CopyChildClassesApplication(acProject, childACClass, newProjectItem);
                }
            }

        }
        #endregion

        #region Clone
        /// <summary>Root point for clone IPlus Studio Tree Item</summary>
        /// <param name="rootACClass"></param>
        /// <param name="userID"></param>
        /// <param name="model"></param>
        public void CloneClassTree(ACClass rootACClass, string userID, CloneDialogModel model)
        {
            Database.udpACClassClone(rootACClass.ACClassID, model.ACIdentifier,
                model.IsCloneACClassProperty,
                model.IsCloneACClassMethod,
                model.IsCloneACClassDesign,
                model.IsCloneACClassConfig,
                model.IsCloneACClassText,
                model.IsCloneACClassMessage,
                model.IsCloneACClassPropertyRelation,
                userID);

        }

        public string CloneGetACIdentifier(ACClass aCClass)
        {
            string name = aCClass.ACIdentifier;
            if (aCClass.ACClass1_ParentACClass != null)
            {
                List<string> sibilingsNames = aCClass.ACClass1_ParentACClass.Childs.Select(c => c.ACIdentifier).ToList();
                sibilingsNames.Remove(name);
                bool nameExist = true;
                int i = 0;
                while (nameExist)
                {
                    i++;
                    name = name + string.Format("{0}", i);
                    nameExist = sibilingsNames.Contains(name);
                }
            }
            else
            {
                name = name + "1";
            }
            return name;
        }

        #endregion

        #endregion

        #region Manager->Modify->ACClassProperty
        /// <summary>
        /// Determines whether [is enabled new AC class property] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new AC class property] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClassProperty(ACClass acClass) // "Eigenschaft löschen"
        {
            if (acClass == null || acClass.ACProject == null)
                return false;
            // Warum Norbert?
            //if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.Classlibraryproject && acClass.ACClass1_ParentACClass != null
            //    && acClass.ACKind != Global.ACKinds.TACClass)
            //    return false;
            return true;
        }

        /// <summary>
        /// Determines whether [is enabled delete AC class property] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassProperty">The ac class property.</param>
        /// <returns><c>true</c> if [is enabled delete AC class property] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACClassProperty(ACClass acClass, ACClassProperty acClassProperty) // "Eigenschaft löschen"
        {
            if (acClass == null || acClassProperty == null)
                return false;

            if (acClass != acClassProperty.ACClass)
                return false;

            if (acClassProperty.ACKind == Global.ACKinds.PSProperty
                && acClassProperty.BasedOnACClassPropertyID == acClassProperty.ACClassPropertyID)
                return false;

            return true;
        }


        /// <summary>
        /// Sets the default property AC class design.
        /// </summary>
        /// <param name="acClassProperty">The ac class property.</param>
        /// <param name="acClassDesign">The ac class design.</param>
        /// <param name="acKind">Kind of the ac.</param>
        /// <param name="acUsage">The ac usage.</param>
        public void SetDefaultPropertyACClassDesign(ACClassProperty acClassProperty, ACClassDesign acClassDesign, Global.ACKinds acKind, Global.ACUsages acUsage)
        {
            IACConfig acConfig = acClassProperty.GetConfigListOfType(acClassDesign.ACClass).Where(c => c.LocalConfigACUrl == "Design_" + acUsage.ToString()).FirstOrDefault();

            if (acConfig != null)
            {
                (acConfig as ACClassConfig).DeleteACObject(Database, false);
                acClassProperty.ClearCacheOfConfigurationEntries();
            }
            if (acClassDesign == null)
                return;
            if (acClassDesign == acClassProperty.GetDesign(acClassProperty, acUsage, acKind))
                return;
            acConfig = acClassProperty.NewACConfig(acClassDesign.ACClass, acClassProperty.Database.GetACType(typeof(ACComposition)), "Design_" + acUsage.ToString());

            ACComposition acComposition = new ACComposition();
            acComposition.SetComposition(acClassDesign);
            acConfig[Const.Value] = acComposition;
        }

        /// <summary>
        /// Aktualisiert das acClassProperty und erzeugt ggf. ein neues ACClassProperty
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassProperty">The ac class property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueOld">The value old.</param>
        /// <returns>ACClassProperty.</returns>
        public ACClassProperty UpdateACClassPropertyTemp(ACClass acClass, ACClassProperty acClassProperty, string propertyName, object value, object valueOld)
        {
            if (IsOverrideProperty(propertyName))
            {
                if (acClassProperty.ACClassID == acClass.ACClassID)
                {
                    // Wenn Property schon von dieser Ableitung (acClass) ist, dann nur Wert vom temp. Property übertragen
                    acClassProperty.ACUrlCommand(propertyName, value);
                    UpdateDerivedACClassProperty(acClass, acClassProperty, propertyName, value, valueOld);
                    return acClassProperty;
                }
                else
                {
                    ACClassProperty acClassPropertyNew = ACClassProperty.NewACClassProperty(Database, acClass, acClassProperty);
                    acClassPropertyNew.ACUrlCommand(propertyName, value);
                    Database.ACClassProperty.AddObject(acClassPropertyNew);
                    UpdateDerivedACClassProperty(acClassPropertyNew.ACClass, acClassPropertyNew, propertyName, value, valueOld);
                    return acClass.Properties.Where(c => c.ACIdentifier == acClassProperty.ACIdentifier).First();
                }
            }
            else
            {
                // Keine Änderung, wenn kein überschreibbares Property
                return acClassProperty;
            }
        }

        /// <summary>
        /// Updates the AC class property.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassProperty">The ac class property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>ACClassProperty.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public ACClassProperty UpdateACClassProperty(ACClass acClass, ACClassProperty acClassProperty, string propertyName)
        {
            // TODO: Ableitungen abgleichen
            if (propertyName == nameof(IInsertInfo.InsertName) || propertyName == nameof(IInsertInfo.InsertDate) || propertyName == nameof(IUpdateInfo.UpdateName) || propertyName == nameof(IUpdateInfo.UpdateDate))
                return acClassProperty;
            ACClassProperty basedOnACClassProperty = acClassProperty.ACClassProperty1_BasedOnACClassProperty;
            ACClass acTypeOfACClassProperty = Database.GlobalDatabase.GetACType(typeof(ACClassProperty)) as ACClass;
            IACType acType = acTypeOfACClassProperty.ReflectGetACTypeInfo(propertyName);

            if (acType != null && (acType is ACClassProperty) && (acType as ACClassProperty).IsInput)
            {
                // Falls dieses Property nicht das Property der Basisklasse ist, dannn Wert in Basisklasse eintragen
                if (basedOnACClassProperty != acClassProperty)
                {
                    basedOnACClassProperty.ACUrlCommand(propertyName, acClassProperty.ACUrlCommand(propertyName));
                }
                UpdateDerivedACClassProperty(basedOnACClassProperty.ACClass, basedOnACClassProperty, propertyName, acClassProperty.ACUrlCommand(propertyName), acClassProperty.ACUrlCommand(propertyName));
            }
            ACClassProperty newProperty = acClass.Properties.Where(c => c.ACIdentifier == acClassProperty.ACIdentifier).FirstOrDefault();
            return newProperty;
        }

        /// <summary>
        /// Updates the derived AC class property.
        /// </summary>
        /// <param name="acClass">In dieser Klasse ist das Property geändert wurden</param>
        /// <param name="acClassProperty">Dieses und alle ableitungeteten Properties sind abzugleichen</param>
        /// <param name="propertyName">Dieses Property hat sich geändert</param>
        /// <param name="value">The value.</param>
        /// <param name="valueOld">The value old.</param>
        /// <returns>Immer das für die acClass richtige Property wird zurückgegeben</returns>
        private void UpdateDerivedACClassProperty(ACClass acClass, ACClassProperty acClassProperty, string propertyName, object value, object valueOld)
        {
            // 1. Aktualisieren des Properties in der abgeleiteten Klasse
            if (acClass != acClassProperty.ACClass)
            {
                ACClass acTypeOfACClassProperty = Database.GlobalDatabase.GetACType(typeof(ACClassProperty)) as ACClass;
                IACType acType = acTypeOfACClassProperty.ReflectGetACTypeInfo(propertyName);
                if (acType == null)
                    return;
                if (!(acType is ACClassProperty))
                    return;
                if (!(acType as ACClassProperty).IsInput)
                    return;
                //if (acType != null && !acType.ACTypeProperty.IsInput)
                //    return;
                var query = acClass.ACClassProperty_ACClass.Where(c => c.ACIdentifier == acClassProperty.ACIdentifier);
                if (query.Any())
                {
                    var classACClassProperty = query.First();
                    if (!IsOverrideProperty(propertyName))
                    {
                        // Falls kein OverrideProperty, dann auf jeden Fall überschreiben
                        classACClassProperty.ACUrlCommand(propertyName, value);
                    }
                    else
                    {
                        // Falls Override und alter Wert identisch, dann überschreiben
                        // ansonsten ist wird der Wert in der Ableitung manuell überschrieben wurden
                        if (CompareObjects(classACClassProperty.ACUrlCommand(propertyName), valueOld))
                        {
                            classACClassProperty.ACUrlCommand(propertyName, value);
                        }
                        else
                        {
                            // Wenn nicht in dieser Ableitung der gleiche Wert, dann kann auch in den weiteren Ableitungen nicht vererbt werden
                            return;
                        }
                    }
                }
            }

            // 1. Alle direkten Ableitungen ermitteln

            if (acClass.ACClass_BasedOnACClass != null && acClass.ACClass_BasedOnACClass.Any())
            {
                List<ACClass> derivedACClassList = acClass.ACClass_BasedOnACClass.ToList();
                foreach (var derivedACClass in derivedACClassList)
                {
                    UpdateDerivedACClassProperty(derivedACClass, acClassProperty, propertyName, value, valueOld);
                }
            }
            return;
        }

        /// <summary>
        /// Compares the objects.
        /// </summary>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool CompareObjects(object value1, object value2)
        {
            if (value1 == null && value2 == null)
                return true;
            if (value1 is IComparable && value2 is IComparable)
            {
                return (value1 as IComparable).CompareTo(value2) == 0;
            }
            return value1 == value2;
        }

        /// <summary>
        /// Determines whether [is override property] [the specified property name].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><c>true</c> if [is override property] [the specified property name]; otherwise, <c>false</c>.</returns>
        bool IsOverrideProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "InputMask":
                case Const.Value:
                case "XMLValue":
                case "LogRefreshRateIndex":
                case "LogFilter":
                case "MinLength":
                case "MaxLength":
                case "MinValue":
                case "MaxValue":
                    return true;
                default:
                    return false;
            }
        }

        public class RemotePropValidResult
        {
            public RemotePropClass Root { get; set; }
            private Dictionary<ACClass, RemotePropClass> _RemotePropClassDict = new Dictionary<ACClass, RemotePropClass>();
            public Dictionary<ACClass, RemotePropClass> RemotePropClassDict { get { return _RemotePropClassDict; } }
            public DataTable ResultAsTable
            {
                get
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Class", typeof(string)); // 0
                    dt.Columns.Add("ClassMaxPropIDSaved", typeof(int)); // 1
                    dt.Columns.Add("ClassMaxPropIDSuggested", typeof(int)); // 2
                    dt.Columns.Add("ClassHasRedundantIDs", typeof(bool)); // 3
                    dt.Columns.Add("ClassHasGapsInSequence", typeof(bool)); // 4
                    dt.Columns.Add("ClassIsValid", typeof(bool)); // 5
                    dt.Columns.Add("HasNoProperties", typeof(bool)); // 6

                    dt.Columns.Add("Property", typeof(string)); // 7
                    dt.Columns.Add("PropIDSaved", typeof(int)); // 8
                    dt.Columns.Add("PropIDSuggested", typeof(int)); // 9
                    dt.Columns.Add("PropHasGapsInSequence", typeof(bool)); // 10
                    dt.Columns.Add("PropIsRedundantID", typeof(bool)); // 11
                    dt.Columns.Add("IsRedundantIDInHierarchy", typeof(bool)); // 12
                    dt.Columns.Add("PropIsValid", typeof(bool)); // 13
                    dt.Columns.Add("PropType", typeof(string)); // 14

                    AddToTable(dt, Root);

                    return dt;
                }
            }

            private void AddToTable(DataTable dt, RemotePropClass remotePropClass)
            {
                var row = dt.NewRow();
                row[0] = remotePropClass.AcClass.AssemblyQualifiedName;
                row[1] = remotePropClass.MaxPropIDSaved;
                row[2] = remotePropClass.MaxPropIDSuggested;
                row[3] = remotePropClass.SelfHasRedundantIDs;
                row[4] = remotePropClass.SelfHasGapsInSequence;
                row[5] = remotePropClass.SelfIsValid;
                row[6] = remotePropClass.Properties.Any();
                row[7] = "";
                row[8] = 0;
                row[9] = 0;
                row[10] = false;
                row[11] = false;
                row[12] = true;
                row[13] = true;
                row[14] = "";
                dt.Rows.Add(row);

                foreach (var property in remotePropClass.Properties)
                {
                    row = dt.NewRow();
                    row[0] = remotePropClass.AcClass.AssemblyQualifiedName;
                    row[1] = remotePropClass.MaxPropIDSaved;
                    row[2] = remotePropClass.MaxPropIDSuggested;
                    row[3] = remotePropClass.SelfHasRedundantIDs;
                    row[4] = remotePropClass.SelfHasGapsInSequence;
                    row[5] = remotePropClass.SelfIsValid;
                    row[6] = true;
                    row[7] = property.AcClassProperty.ACIdentifier;
                    row[8] = property.PropIDSaved;
                    row[9] = property.PropIDSuggested;
                    row[10] = property.HasGapsInSequence;
                    row[11] = property.IsRedundantID;
                    row[12] = property.IsRedundantIDInHierarchy;
                    row[13] = property.IsValid;
                    row[14] = property.AcClassProperty.ValueTypeACClass.ACIdentifier;
                    dt.Rows.Add(row);
                }

                foreach (var child in remotePropClass.Childs)
                {
                    AddToTable(dt, child);
                }
            }
        }

        public class RemotePropClass
        {
            public RemotePropClass ParentRemoteClass { get; set; }
            public ACClass AcClass { get; set; }
            private List<RemoteProp> _Properties = new List<RemoteProp>();
            public List<RemoteProp> Properties
            {
                get
                {
                    return _Properties;
                }
            }

            private List<RemotePropClass> _Childs = new List<RemotePropClass>();
            public List<RemotePropClass> Childs
            {
                get
                {
                    return _Childs;
                }
            }

            public int MaxPropIDSaved
            {
                get
                {
                    return Properties.Any() ? Properties.Max(c => c.PropIDSaved) : 0;
                }
            }

            public int MaxPropIDSuggested
            {
                get;set;
            }

            public bool SelfHasRedundantIDs
            {
                get
                {
                    return Properties.Any(c => c.IsRedundantID);
                }
            }

            public bool SelfHasRedundantIDInHierarchy
            {
                get
                {
                    return Properties.Any(c => c.IsRedundantIDInHierarchy);
                }
            }

            public bool SelfHasGapsInSequence
            {
                get
                {
                    return Properties.Any(c => c.HasGapsInSequence);
                }
            }


            public bool ChildsHasRedundantIDs
            {
                get;set;
            }

            public bool ChildsHasGapsInSequence
            {
                get; set;
            }

            public bool SelfIsValid
            {
                get
                {
                    return !SelfHasRedundantIDs && !SelfHasGapsInSequence && !SelfHasRedundantIDInHierarchy;
                }
            }

            public bool IsIDUsedInHierarchy(int remoteID)
            {
                if (Properties.Where(c => c.PropIDSaved == remoteID).Any())
                    return true;
                if (ParentRemoteClass == null)
                    return false;
                return ParentRemoteClass.IsIDUsedInHierarchy(remoteID);
            }
        }

        public class RemoteProp
        {
            public RemotePropClass RemotePropClass { get; set; }
            public ACClassProperty AcClassProperty { get; set; }
            public int PropIDSaved
            {
                get
                {
                    return AcClassProperty.RemotePropID;
                }
            }

            public int PropIDSuggested
            {
                get; set;
            }

            public bool HasGapsInSequence
            {
                get; set;
            }

            public bool IsRedundantID
            {
                get; set;
            }

            public bool IsRedundantIDInHierarchy
            {
                get; set;
            }

            public bool IsValid
            {
                get
                {
                    return !HasGapsInSequence && !IsRedundantID && !IsRedundantIDInHierarchy;
                }
            }

        }

        public RemotePropValidResult ValidateRemotePropHierarchy(Database db)
        {
            RemotePropValidResult remotePropValidResult = new RemotePropValidResult();
            IEnumerable<RemoteProp> remoteProps = db.ACClassProperty
                .Include(c => c.ACClass)
                .Include(c => c.ACClass.ACClass1_BasedOnACClass)
                .Where(c => c.RemotePropID > 0 && c.BasedOnACClassPropertyID == c.ACClassPropertyID)
                .OrderBy(c => c.ACClassID)
                .ThenBy(c => c.RemotePropID)
                .Select(c => new RemoteProp() { AcClassProperty = c })
                .ToArray();

            // 1. Build Model
            RemotePropClass prevRemoteCass = null;
            foreach (RemoteProp remoteProp in remoteProps)
            {
                RemotePropClass remoteClass = null;
                if (!remotePropValidResult.RemotePropClassDict.TryGetValue(remoteProp.AcClassProperty.ACClass, out remoteClass))
                {
                    remoteClass = new RemotePropClass() { AcClass = remoteProp.AcClassProperty.ACClass };
                    remotePropValidResult.RemotePropClassDict.Add(remoteProp.AcClassProperty.ACClass, remoteClass);
                }
                remoteProp.RemotePropClass = remoteClass;
                remoteClass.Properties.Add(remoteProp);

                // 1.1 Add Parent-Classes to top
                if (prevRemoteCass != remoteClass)
                {
                    RemotePropClass lastBaseClass = remoteClass;
                    prevRemoteCass = remoteClass;
                    ACClass baseACClass = prevRemoteCass.AcClass.ACClass1_BasedOnACClass;
                    while (baseACClass != null)
                    {
                        if (!remotePropValidResult.RemotePropClassDict.TryGetValue(baseACClass, out remoteClass))
                        {
                            remoteClass = new RemotePropClass() { AcClass = baseACClass };
                            remotePropValidResult.RemotePropClassDict.Add(baseACClass, remoteClass);
                        }
                        if (lastBaseClass.ParentRemoteClass == null)
                            lastBaseClass.ParentRemoteClass = remoteClass;
                        if (!remoteClass.Childs.Contains(lastBaseClass))
                            remoteClass.Childs.Add(lastBaseClass);
                        lastBaseClass = remoteClass;
                        baseACClass = baseACClass.ACClass1_BasedOnACClass;
                    }

                    if (remotePropValidResult.Root == null)
                        remotePropValidResult.Root = lastBaseClass;

                }
            }

            // 2. Validate Hierarchy
            ValidateRemotePropHierarchy(remotePropValidResult.Root);
            return remotePropValidResult;
        }

        private void ValidateRemotePropHierarchy(RemotePropClass classToValidate)
        {
            int lastSuggestedMax = classToValidate.ParentRemoteClass != null ? classToValidate.ParentRemoteClass.MaxPropIDSuggested : 0;
            RemoteProp prevRemoteProp = null;
            foreach (RemoteProp remoteProp in classToValidate.Properties.OrderBy(c => c.PropIDSaved).ToArray())
            {
                lastSuggestedMax++;
                remoteProp.PropIDSuggested = lastSuggestedMax;
                if (prevRemoteProp != null)
                {
                    if (prevRemoteProp.PropIDSaved == remoteProp.PropIDSaved)
                        remoteProp.IsRedundantID = true;
                    if (remoteProp.PropIDSaved != (prevRemoteProp.PropIDSaved + 1))
                        remoteProp.HasGapsInSequence = true;
                }
                remoteProp.IsRedundantIDInHierarchy = classToValidate.ParentRemoteClass.IsIDUsedInHierarchy(remoteProp.PropIDSaved);
            }
            classToValidate.MaxPropIDSuggested = lastSuggestedMax;
            bool selfHasGapsInSequence = classToValidate.SelfHasGapsInSequence;
            bool childsHasRedundantIDs = classToValidate.ChildsHasRedundantIDs;
            if (selfHasGapsInSequence || childsHasRedundantIDs)
            {
                RemotePropClass parentClass = classToValidate.ParentRemoteClass;
                while (parentClass != null)
                {
                    if (!parentClass.ChildsHasGapsInSequence && selfHasGapsInSequence)
                        parentClass.ChildsHasGapsInSequence = selfHasGapsInSequence;
                    if (!parentClass.ChildsHasRedundantIDs && childsHasRedundantIDs)
                        parentClass.ChildsHasRedundantIDs = childsHasRedundantIDs;
                    parentClass = parentClass.ParentRemoteClass;
                }
            }

            foreach (RemotePropClass child in classToValidate.Childs)
            {
                ValidateRemotePropHierarchy(child);
            }
        }


        #endregion

        #region Manager->Modify->ACClassMethod
        /// <summary>
        /// Determines whether [is enabled new work AC class method] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new work AC class method] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewWorkACClassMethod(ACClass acClass) // "Workflow einfügen"
        {
            if (acClass == null || acClass.ACProject == null)
                return false;
            if (!acClass.ACProject.IsWorkflowEnabled && acClass.ACProject.ACProjectType != Global.ACProjectTypes.ClassLibrary)
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            //if (acClass.MyPWACClass == null)
            //    return false;
            return true;
        }

        /// <summary>
        /// Determines whether [is enabled new script AC class method] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new script AC class method] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewScriptACClassMethod(ACClass acClass) // "Script einfügen"
        {
            if (acClass == null || acClass.ACProject == null)
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether [is enabled new script client AC class method] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new script client AC class method] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewScriptClientACClassMethod(ACClass acClass) // "Client Script einfügen"
        {
            if (acClass == null || acClass.ACProject == null)
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether [is enabled new pre AC class method] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <returns><c>true</c> if [is enabled new pre AC class method] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewPreACClassMethod(ACClass acClass, ACClassMethod acClassMethod) // "Pre-Methode einfügen"
        {
            if (acClass == null || acClassMethod == null)
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            return (acClassMethod.ACKind == Global.ACKinds.MSMethodPrePost);
        }

        /// <summary>
        /// Determines whether [is enabled new post AC class method] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <returns><c>true</c> if [is enabled new post AC class method] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewPostACClassMethod(ACClass acClass, ACClassMethod acClassMethod) // "Post-Methode einfügen"
        {
            if (acClass == null || acClassMethod == null)
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            return (acClassMethod.ACKind == Global.ACKinds.MSMethodPrePost);
        }

        /// <summary>
        /// Determines whether [is enabled new on set property AC class method] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassProperty">The ac class property.</param>
        /// <returns><c>true</c> if [is enabled new on set property AC class method] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewOnSetPropertyACClassMethod(ACClass acClass, ACClassProperty acClassProperty) // "OnSetProperty-Methode einfügen"
        {
            if (acClass == null || acClassProperty == null)
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            if (acClassProperty.ACPropUsage != Global.ACPropUsages.Property)
                return false;
            if (acClassProperty.IsBroadcast)
                return false;
            string methodName = ScriptTrigger.ScriptTriggers[(int)ScriptTrigger.Type.OnSetACProperty].GetMethodName(acClassProperty.ACIdentifier);
            if (acClass.Methods.Where(c => c.ACIdentifier == methodName).Any())
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether [is enabled new on set property net AC class method] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassProperty">The ac class property.</param>
        /// <returns><c>true</c> if [is enabled new on set property net AC class method] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewOnSetPropertyNetACClassMethod(ACClass acClass, ACClassProperty acClassProperty) // "OnSetPropertyNet-Methode einfügen"
        {
            if (acClass == null || acClassProperty == null)
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            if (acClassProperty.ACPropUsage != Global.ACPropUsages.Property)
                return false;
            if (!acClassProperty.IsBroadcast)
                return false;
            string methodName = ScriptTrigger.ScriptTriggers[(int)ScriptTrigger.Type.OnSetACPropertyNet].GetMethodName(acClassProperty.ACIdentifier);
            if (acClass.Methods.Where(c => c.ACIdentifier == methodName).Any())
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether [is enabled new on set point AC class method] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassProperty">The ac class property.</param>
        /// <returns><c>true</c> if [is enabled new on set point AC class method] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewOnSetPointACClassMethod(ACClass acClass, ACClassProperty acClassProperty) // "OnSetPoint-Methode einfügen"
        {
            if (acClass == null || acClassProperty == null)
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            if (acClassProperty.ACPropUsage != Global.ACPropUsages.ConnectionPoint ||
                    acClassProperty.ACPropUsage == Global.ACPropUsages.EventPoint ||
                    acClassProperty.ACPropUsage == Global.ACPropUsages.EventPointSubscr ||
                    acClassProperty.ACPropUsage == Global.ACPropUsages.AsyncMethodPoint)
                return false;
            string methodName = ScriptTrigger.ScriptTriggers[(int)ScriptTrigger.Type.OnSetACPoint].GetMethodName(acClassProperty.ACIdentifier);
            if (acClass.Methods.Where(c => c.ACIdentifier == methodName).Any())
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether [is enabled new on data row start] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new on data row start] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewOnDataRowStart(ACClass acClass) // "OnDataRowStart-Script einfügen"
        {
            if (acClass == null)
                return false;
            if (acClass.ACKind != Global.ACKinds.TACQRY)
                return false;
            string methodName = acClass.ACIdentifier + "OnDataRowStart";
            return !acClass.ACClassMethod_ACClass.Where(c => c.ACIdentifier == methodName).Any();
        }

        /// <summary>
        /// Determines whether [is enabled new on data row end] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new on data row end] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewOnDataRowEnd(ACClass acClass) // "OnDataRowEnd-Script einfügen"
        {
            if (acClass == null)
                return false;
            if (acClass.ACKind != Global.ACKinds.TACQRY)
                return false;
            string methodName = acClass.ACIdentifier + "OnDataRowEnd";
            return !acClass.ACClassMethod_ACClass.Where(c => c.ACIdentifier == methodName).Any();
        }

        /// <summary>
        /// Determines whether [is enabled delete AC class method] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <returns><c>true</c> if [is enabled delete AC class method] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACClassMethod(ACClass acClass, ACClassMethod acClassMethod) // "Methode löschen"
        {
            if (acClass == null || acClassMethod == null)
                return false;
            if (acClass != acClassMethod.ACClass)
                return false;
            return acClassMethod.ACKind != Global.ACKinds.MSMethod
                && acClassMethod.ACKind != Global.ACKinds.MSMethodPrePost
                && acClassMethod.ACKind != Global.ACKinds.MSMethodClient
                && (acClassMethod.ACKind != Global.ACKinds.MSMethodFunction
                    || (acClassMethod.ACKind == Global.ACKinds.MSMethodFunction && acClassMethod.AttachedFromACClassID.HasValue && acClassMethod.ParentACClassMethodID.HasValue));
        }

        /// <summary>
        /// Sets the default method AC class design.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <param name="acClassDesign">The ac class design.</param>
        /// <param name="acKind">Kind of the ac.</param>
        /// <param name="acUsage">The ac usage.</param>
        public static void SetDefaultMethodACClassDesign(Database database, ACClassMethod acClassMethod, ACClassDesign acClassDesign, Global.ACKinds acKind, Global.ACUsages acUsage)
        {
            var query1 = acClassMethod.GetConfigListOfType(acClassMethod.ACClass).Where(c => c.LocalConfigACUrl == "Design_" + acUsage.ToString());

            if (query1.Any())
            {
                (query1.First() as ACClassMethodConfig).DeleteACObject(database, false);
            }
            if (acClassDesign == null)
                return;
            if (acClassDesign == acClassMethod.GetDesign(acClassMethod, acUsage, acKind))
                return;
            IACConfig acConfig = acClassMethod.NewACConfig(null, acClassMethod.Database.GetACType(typeof(ACComposition)));

            acConfig.LocalConfigACUrl = "Design_" + acUsage.ToString();
            ACComposition acComposition = new ACComposition();
            acComposition.SetComposition(acClassDesign);
            acConfig[Const.Value] = acComposition;
        }
        #endregion

        #region Manager->Modify->ACClassDesign
        /// <summary>
        /// Determines whether [is enabled new AC class design] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new AC class design] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClassDesign(ACClass acClass)
        {
            if (acClass == null)
                return false;
            //if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.Classlibraryproject && acClass.ACClass1_ParentACClass != null)
            //return false;
            return true;
        }

        /// <summary>Gets the control mode is default AC class design.</summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassDesign">The ac class design.</param>
        /// <param name="msgDetails"></param>
        /// <returns>Global.ControlModes.</returns>
        public Global.ControlModes GetControlModeIsDefaultACClassDesign(ACClass acClass, ACClassDesign acClassDesign, MsgWithDetails msgDetails)
        {
            if (acClassDesign == null)
                return Global.ControlModes.Hidden;

            switch (acClassDesign.ACUsage)
            {
                case Global.ACUsages.DUVisualisation:
                case Global.ACUsages.DUMain:
                case Global.ACUsages.DUControl:
                case Global.ACUsages.DUControlDialog:
                case Global.ACUsages.DUDiagnostic:
                    {
                        bool isDefaultACClassDesign = IsDefaultACClassDesign(acClass, acClassDesign, msgDetails);

                        var query = acClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl != null && c.LocalConfigACUrl.StartsWith("Design_"));
                        bool exists = false;
                        foreach (var acConfig in query)
                        {
                            ACComposition acComposition = acConfig[Const.Value] as ACComposition;
                            if (acComposition.GetComposition(acClass.Database) is ACClassDesign)
                            {
                                if ((acComposition.GetComposition(acClass.Database) as ACClassDesign).ACClassDesignID == acClassDesign.ACClassDesignID)
                                {
                                    exists = true;
                                    break;
                                }
                            }
                        }

                        // Ist das DefaultDesign und existiert in der ACClassMapDesign, dann kann das Default auch entfernt werden
                        if (isDefaultACClassDesign && exists)
                        {
                            return Global.ControlModes.Enabled;
                        }
                        // Ist das DefaultDesign und existiert nicht in der ACClassMapDesign, dann kann das Default nicht entfernt werden
                        else if (isDefaultACClassDesign && !exists)
                        {
                            return Global.ControlModes.Disabled;
                        }
                        // Wenn nicht DefaultDesign, dann kann das Default gesetzt werden
                        else
                        {
                            return Global.ControlModes.Enabled;
                        }
                    }
                default:
                    return Global.ControlModes.Hidden;
            }
        }

        /// <summary>Determines whether [is default AC class design] [the specified ac class].</summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassDesign">The ac class design.</param>
        /// <param name="msgDetails"></param>
        /// <returns>
        ///   <c>true</c> if [is default AC class design] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsDefaultACClassDesign(ACClass acClass, ACClassDesign acClassDesign, MsgWithDetails msgDetails)
        {
            if (acClassDesign == null)
                return false;
            ACClassDesign defaultACClassDesign = acClass.GetDesign(acClass, acClassDesign.ACUsage, acClassDesign.ACKind, "", msgDetails);
            return defaultACClassDesign == acClassDesign;
        }

        /// <summary>
        /// Sets the default AC class design.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassDesign">The ac class design.</param>
        /// <param name="isDefaultACClassDesign">if set to <c>true</c> [is default AC class design].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool SetDefaultACClassDesign(ACClass acClass, ACClassDesign acClassDesign, bool isDefaultACClassDesign)
        {
            ACClassConfig currentDefaultDesign = acClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl == "Design_" + acClassDesign.ACUsage.ToString()).FirstOrDefault() as ACClassConfig;
            if (isDefaultACClassDesign)
            {
                if (currentDefaultDesign != null)
                    acClass.RemoveACConfig(currentDefaultDesign);

                if (!IsDefaultACClassDesign(acClass, acClassDesign, null))
                {
                    IACConfig acConfig = acClass.NewACConfig(null, acClass.Database.GetACType(typeof(ACComposition)));

                    ACComposition acComposition = new ACComposition();
                    acComposition.SetComposition(acClassDesign);
                    acConfig.LocalConfigACUrl = "Design_" + acClassDesign.ACUsage.ToString();
                    acConfig[Const.Value] = acComposition;
                }
            }
            else if (currentDefaultDesign != null)
                acClass.RemoveACConfig(currentDefaultDesign);
            return isDefaultACClassDesign;
        }
        #endregion

        #region Manager->Modify->ACClassMenu
        /// <summary>
        /// News the AC class menu.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns>ACClassDesign.</returns>
        public ACClassDesign NewACClassMenu(ACClass acClass)
        {
            string secondaryKey = ACRoot.SRoot.NoManager.GetNewNo(Database, typeof(ACClassDesign), ACClassDesign.NoColumnName, ACClassDesign.FormatNewNo, null);
            ACClassDesign acClassMenu = ACClassDesign.NewACObject(Database, acClass, secondaryKey);
            acClassMenu.ACKind = Global.ACKinds.DSDesignMenu;
            Database.ACClassDesign.AddObject(acClassMenu);
            return acClassMenu;
        }

        /// <summary>
        /// Determines whether [is enabled new AC class menu] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new AC class menu] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClassMenu(ACClass acClass)
        {
            if (acClass == null)
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether [is enabled delete AC class menu] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassMenu">The ac class menu.</param>
        /// <returns><c>true</c> if [is enabled delete AC class menu] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACClassMenu(ACClass acClass, ACClassDesign acClassMenu)
        {
            if (acClass == null || acClassMenu == null)
                return false;
            if (acClass != acClassMenu.ACClass)
                return false;
            // Hauptmenü darf nicht gelöscht werden
            if (acClass.ACIdentifier == ACRoot.ClassName && acClassMenu.ACKind == Global.ACKinds.DSDesignMenu)
                return false;
            return true;
        }
        #endregion

        #region Manager->Modify->ACClassSetting
        /// <summary>
        /// Determines whether [is enabled new AC class composition] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new AC class composition] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClassComposition(ACClass acClass) // "Konfiguration einfügen"
        {
            if ((acClass == null) || (acClass.ACProject == null))
                return false;
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary && acClass.ACClass1_ParentACClass != null)
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether [is enabled delete AC class composition] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="configSetting">The config setting.</param>
        /// <returns><c>true</c> if [is enabled delete AC class composition] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACClassComposition(ACClass acClass, ACClassConfig configSetting) // "Konfiguration löschen"
        {
            if (acClass == null || configSetting == null)
                return false;
            if (acClass != configSetting.ACClass)
                return false;
            return true;
        }
        #endregion

        #region Manager->Modify->ACClassPropertyRelation
        /// <summary>
        /// Determines whether [is enabled new AC class property relation] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns><c>true</c> if [is enabled new AC class property relation] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClassPropertyRelation(ACClass acClass)
        {
            if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary)
                return false;
            return true;
        }

        /// <summary>
        /// Removes the AC class property relation.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassPropertyRelation">The ac class property relation.</param>
        /// <returns>Msg.</returns>
        public Msg RemoveACClassPropertyRelation(ACClass acClass, ACClassPropertyRelation acClassPropertyRelation)
        {
            if (acClassPropertyRelation.ACClassConfig_ACClassPropertyRelation.Any())
                acClassPropertyRelation.ACClassConfig_ACClassPropertyRelation.ToList().ForEach(c => c.DeleteACObject(Database, false));
            return acClassPropertyRelation.DeleteACObject(Database, false);
        }

        /// <summary>
        /// Determines whether [is enabled remove AC class property relation] [the specified ac class].
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassPropertyRelation">The ac class property relation.</param>
        /// <returns><c>true</c> if [is enabled remove AC class property relation] [the specified ac class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledRemoveACClassPropertyRelation(ACClass acClass, ACClassPropertyRelation acClassPropertyRelation)
        {
            if (acClass != acClassPropertyRelation.SourceACClass && acClass != acClassPropertyRelation.TargetACClass)
                return false;

            return true;
        }
        #endregion

        #region Manager->Search->ACProject
        /// <summary>
        /// Gets the name of the AC project by model.
        /// </summary>
        /// <param name="acProjectName">Name of the ac project.</param>
        /// <returns>ACProject.</returns>
        public ACProject GetACProjectByModelName(string acProjectName)
        {
            return Database.ACProject
                    //.Include(c => c.ACProject_BasedOnACProject)
                    .Where(c => c.ACProjectName == acProjectName)
                    .FirstOrDefault();
        }
        #endregion

        #region Manager->Modify->ACVisual
        /// <summary>
        /// News the visualisation.
        /// </summary>
        /// <returns>ACClassDesign.</returns>
        public ACClassDesign NewVisualisation(ACClass parentACClass)
        {
            string secondaryKey = ACRoot.SRoot.NoManager.GetNewNo(Database, typeof(ACClassDesign), ACClassDesign.NoColumnName, ACClassDesign.FormatNewNo, null);
            ACClassDesign acClassDesign = ACClassDesign.NewACClassDesignVisualisation(Database, parentACClass, secondaryKey);
            Database.ACClassDesign.AddObject(acClassDesign);
            return acClassDesign;
        }

        #endregion

        #region Manager->Modify->Application
        /// <summary>
        /// Updates the applicationby app definition.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="acClassList">The ac class list.</param>
        /// <param name="moveInfoList">The move info list.</param>
        /// <returns>True = mit Warnung für Mehrfachinstanzen bei Anwendung</returns>
        public string UpdateApplicationbyAppDefinition(ACProject acProject, IEnumerable<ACClass> acClassList, IEnumerable<MoveInfo> moveInfoList)
        {
            if (!IsEnabledUpdateApplicationbyAppDefinition(acProject, acClassList, moveInfoList))
                return "";
            string info = "";
            foreach (var appACProject in acProject.ACProject_BasedOnACProject)
            {
                foreach (var acClass in acClassList)
                {
                    var query = appACProject.ACClass_ACProject.Where(c => c.ACClass1_BasedOnACClass == acClass.ACClass1_ParentACClass);
                    if (query.Any())
                    {
                        foreach (var appACClass in query.ToList())
                        {
                            if (appACClass.Childs.Where(c => c.ACIdentifier == acClass.ACIdentifier).Any())
                                continue;

                            InsertAppDefinitionClassRecursive(appACProject, acClass, appACClass);
                        }
                    }
                }
                // Verschobene ACClass
                foreach (var moveInfo in moveInfoList)
                {
                    // Klassen in Anwendung, die von der verschobenen Klasse abgeleitet sind
                    var query = appACProject.ACClass_ACProject.Where(c => c.ACClass1_BasedOnACClass == moveInfo.MovedClass);
                    foreach (var acClass in query)
                    {
                        // Zielklassen
                        var query2 = acClass.ACProject.ACClass_ACProject.Where(c => c.ACClass1_BasedOnACClass == moveInfo.TargetParentClass).OrderBy(c => c.ACIdentifier);
                        switch (query2.Count())
                        {
                            case 1: // Ist in Anwendung nur genau eine Klasse in der Ableitung vorhanden, kann das Element hierhin verschoben werden
                                acClass.ACClass1_ParentACClass = query2.First();
                                break;
                            case 0: // Ist in Anwendung nicht vorhanden
                                break;
                            default: // Ist mehrfach vorhanden, dann ans erste Kopieren
                                acClass.ACClass1_ParentACClass = query2.First();
                                foreach (var acClassParent in query2)
                                {
                                    if (!string.IsNullOrEmpty(info))
                                        info += ", ";
                                    info += acClassParent.ACIdentifier;
                                }
                                break;
                        }
                    }
                }
            }
            if (Database.IsChanged)
            {
                Database.ACSaveChanges();
            }
            return info;
        }

        /// <summary>
        /// Inserts the app definition class recursive.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="parentACClass">The parent AC class.</param>
        public void InsertAppDefinitionClassRecursive(ACProject acProject, ACClass acClass, ACClass parentACClass)
        {
            ACClass newACClass = ACClass.NewACObjectWithBaseclass(Database, acProject, acClass);
            newACClass.ACClass1_ParentACClass = parentACClass;
            Database.ACClass.AddObject(newACClass);

            if (acClass.Childs != null)
            {
                foreach (var subACClass in acClass.Childs)
                {
                    InsertAppDefinitionClassRecursive(acProject, subACClass, newACClass);
                }
            }
        }

        /// <summary>
        /// Determines whether [is enabled update applicationby app definition] [the specified ac project].
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        /// <param name="acClassList">The ac class list.</param>
        /// <param name="moveInfoList">The move info list.</param>
        /// <returns><c>true</c> if [is enabled update applicationby app definition] [the specified ac project]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledUpdateApplicationbyAppDefinition(ACProject acProject, IEnumerable<ACClass> acClassList, IEnumerable<MoveInfo> moveInfoList)
        {
            if (acProject.ACProjectType != Global.ACProjectTypes.AppDefinition)
                return false;
            foreach (var appACProject in acProject.ACProject_BasedOnACProject)
            {
                if (acClassList != null)
                {
                    // Hinzugefügte oder gelöschte ACClass
                    foreach (var acClass in acClassList)
                    {
                        var query = appACProject.ACClass_ACProject.Where(c => c.ACClass1_BasedOnACClass == acClass.ACClass1_ParentACClass);
                        if (query.Any())
                        {
                            return true;
                        }
                    }
                }
                if (moveInfoList != null)
                {
                    // Verschobene ACClass
                    foreach (var moveInfo in moveInfoList)
                    {
                        var query = appACProject.ACClass_ACProject.Where(c => c.ACClass1_BasedOnACClass == moveInfo.MovedClass);
                        if (query.Any())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region Manager->Modify->ACClassTask
        public void UpdateACClassTask(ACProject acProject)
        {
            if (acProject.ACProjectType != Global.ACProjectTypes.Application
                && acProject.ACProjectType != Global.ACProjectTypes.Root
                && acProject.ACProjectType != Global.ACProjectTypes.Service)
            {
                return;
            }

            Global.ACTaskTypes acTaskType = (acProject.ACProjectType == Global.ACProjectTypes.Application
                                                || acProject.ACProjectType == Global.ACProjectTypes.Service)
                                            ? Global.ACTaskTypes.ModelTask : Global.ACTaskTypes.RootTask;

            ACClass parentACClass = null;
            // Anwendungen werden immer unterhalb von Root gehängt
            if (acProject.ACProjectType == Global.ACProjectTypes.Application || acProject.ACProjectType == Global.ACProjectTypes.Service)
            {
                parentACClass = Database.ACClass.Where(c => c.ACIdentifier == ACRoot.ClassName && c.ACProject.ACProjectTypeIndex == (Int16)Global.ACProjectTypes.Root).First();
            }

            UpdateACClassTaskRekursiv(acProject.RootClass, parentACClass, acTaskType);
        }

        /// <summary>
        /// Updates the AC class task rekursiv.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="parentACClass">The parent AC class.</param>
        /// <param name="acTaskType">Type of the ac task.</param>
        public void UpdateACClassTaskRekursiv(ACClass acClass, ACClass parentACClass, Global.ACTaskTypes acTaskType)
        {
            if (acClass == null)
                return;
            // Bei Multiinstance werden die ACClassTask bei Bedarf automatisch erzeugt
            if (acClass.IsMultiInstanceInherited)
                return;

            if (parentACClass == null) // Nur Null wenn "Root"-Root
            {
                ACClassTask acClassTask = acClass.ACClassTask_TaskTypeACClass.Where(c => !c.IsTestmode).FirstOrDefault();
                if (acClassTask == null)
                {
                    acClassTask = ACClassTask.NewACObject(Database, null);
                    acClassTask.TaskTypeACClass = acClass;
                    acClassTask.ACTaskType = acTaskType;
                    acClassTask.IsDynamic = false;
                    acClassTask.ACIdentifier = acClass.ACIdentifier;
                    Database.ACClassTask.AddObject(acClassTask);
                }
                else
                {
                    if (acClassTask.ACIdentifier != acClass.ACIdentifier)
                        acClassTask.ACIdentifier = acClass.ACIdentifier;
                }

                ACClassTask acClassTaskTest = acClass.ACClassTask_TaskTypeACClass.Where(c => c.IsTestmode).FirstOrDefault();
                if (acClassTaskTest == null)
                {
                    acClassTaskTest = ACClassTask.NewACObject(Database, acClassTask);
                    acClassTaskTest.TaskTypeACClass = acClass;
                    acClassTaskTest.ACTaskType = acTaskType;
                    acClassTaskTest.IsDynamic = false;
                    acClassTaskTest.IsTestmode = true;
                    acClassTaskTest.ACIdentifier = Const.ACRootProjectNameTest;
                    Database.ACClassTask.AddObject(acClassTaskTest);
                }
                else
                {
                    if (acClassTaskTest.ACIdentifier != Const.ACRootProjectNameTest)
                        acClassTaskTest.ACIdentifier = Const.ACRootProjectNameTest;
                }
            }
            else
            {
                foreach (var acClassTaskParent in parentACClass.ACClassTask_TaskTypeACClass)
                {
                    ACClassTask acClassTaskTest = acClass.ACClassTask_TaskTypeACClass.Where(c => c.IsTestmode == acClassTaskParent.IsTestmode).FirstOrDefault();
                    if (acClassTaskTest == null)
                    {
                        acClassTaskTest = ACClassTask.NewACObject(Database, acClassTaskParent);
                        acClassTaskTest.TaskTypeACClass = acClass;
                        acClassTaskTest.ACTaskType = acTaskType;
                        acClassTaskTest.IsDynamic = false;
                        acClassTaskTest.IsTestmode = acClassTaskParent.IsTestmode;
                        acClassTaskTest.ACIdentifier = acClass.ACIdentifier;
                        Database.ACClassTask.AddObject(acClassTaskTest);
                    }
                    else
                    {
                        if (acClassTaskTest.ACIdentifier != acClass.ACIdentifier)
                            acClassTaskTest.ACIdentifier = acClass.ACIdentifier;
                    }
                }
            }

            var query2 = acClass.ACClass_ParentACClass.Where(c => !c.IsMultiInstanceInherited && c.ACStorableTypeIndex > (Int16)Global.ACStorableTypes.NotStorable);
            if (query2.Any())
            {
                foreach (var childACClass in query2)
                {
                    UpdateACClassTaskRekursiv(childACClass, acClass, acTaskType);
                }
            }
        }

        #endregion

        #region Manager->Modify->ProcessFunction
        // Aktualisieren der MSMethodFunction-Aufrufe bei der übergeordneten ACComponent
        /// <summary>
        /// Updates the MS method function.
        /// </summary>
        /// <param name="acProject">The ac project.</param>
        public MsgWithDetails UpdateMSMethodFunction(ACProject acProject)
        {
            MsgWithDetails msg = new MsgWithDetails();
            switch (acProject.ACProjectType)
            {
                case Global.ACProjectTypes.Application:
                case Global.ACProjectTypes.Service:
                case Global.ACProjectTypes.AppDefinition:
                    // Bei Anwendung und Anwendungsdeifition alle ACClass beim Root beginnend aktualisieren
                    UpdateMSMethodFunctionRekursiv(acProject.RootClass, msg);
                    break;
                case Global.ACProjectTypes.ClassLibrary:
                    {
                        // Bei Klassenbibliothek alle Root-ACClass aktualisieren
                        var query = acProject.ACClass_ACProject.Where(c => c.ParentACClassID == null);
                        foreach (var acClass in query)
                        {
                            UpdateMSMethodFunctionRekursiv(acClass, msg);
                        }
                    }
                    break;
            }
            if (!msg.MsgDetails.Any())
                return null;
            return msg;
        }

        private class MultipleFuncCounter4VMethod
        {
            public ACClassMethod Method { get; set; }
            public int Count { get; set; }
        }

        /// <summary>Updates the MS method function rekursiv.</summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="msg"></param>
        public void UpdateMSMethodFunctionRekursiv(ACClass acClass, MsgWithDetails msg)
        {
            //   DERIVATION - SZENARIOS
            //   -"PAFDosing"(Level1):
            //   -has a "Start" - Method(Level1) and
            // -a virtual Method "VDosing"(Level1) => ParentACClassMethodID points to "Start"-Method(Level1)
            //   
            //     - "PAProcessModule2"(Level2):
            //        - has a child "PAFDosing"(Level2), BasedOnACClassId points to PAFDosing(Level1)
            //            therefore PAProcessModule2 gets a new virtual attached Method:
            //            - "VDosing"(Level2) => ParentACClassMethodID points to "VDosing"(Level1)
            //                                => AttachedFromACClassID points to "PAFDosing"(Level2)
            //   
            //     - "PAProcessModule3" (Level3):
            //        - has a child "PAFDosing"(Level3), BasedOnACClassId points to "PAFDosing"(Level2)
            //            therefore PAProcessModule3 inhertis the virtual attached Method from "PAProcessModule2"(Level2):
            //            - "VDosing"(Level2) => ParentACClassMethodID points to "VDosing"(Level1)
            //                                => AttachedFromACClassID points to "PAFDosing"(Level2)
            //        - has a new child "PAFDosingB"(Level2), BasedOnACClassId points to PAFDosing(Level1)
            //            therefore PAProcessModule3 gets a new virtual attached Method WITH A PREFIX "PAFDosingB_":
            //            - "PAFDosingB_VDosing"(Level2) => ParentACClassMethodID points to "VDosing"(Level1)
            //                                           => AttachedFromACClassID points to "PAFDosingB"(Level2)
            //    select m.ACIdentifier, m.ACClassMethodID, m.ACClassID, m.ACKindIndex, c.ACIdentifier, c.ACURLCached, m.ParentACClassMethodID, mp.ACIdentifier, m.AttachedFromACClassID, ac.ACIdentifier from ACClassMethod m
            //    inner join ACClass c on c.ACClassID = m.ACClassID
            //    left join ACClass ac on ac.ACClassID = m.AttachedFromACClassID
            //    inner join ACClassMethod mp on mp.ACClassMethodID = m.ParentACClassMethodID
            //    where m.ACIdentifier = 'Dosing'
            //    order by c.ACURLCached desc;

            // If there are no Child-Classes for this ProcessModule do nothing
            if (!acClass.ACClass_ParentACClass.Any())
                return;

            // virtualMethodsOfChildFuncs contains tuples with the most derivated class and the virtual methods
            // e.g. "PAFDosing"(Level3) / "VDosing"(Level1)
            // e.g. "PAFDosingB"(Level2) / "VDosing"(Level1)
            List<Tuple<ACClass, ACClassMethod>> virtualMethodsOfChildFuncs = new List<Tuple<ACClass, ACClassMethod>>();
            List<MultipleFuncCounter4VMethod> vMethodsInMultipleFuncClasses = new List<MultipleFuncCounter4VMethod>();
            //List<ACClassMethod> virtualMethodsOfChilds = new List<ACClassMethod>();
            // Find all ACComponents which are TPAProcessFunction's an determine all VirtualACMethod's
            foreach (var childACClass in acClass.ACClass_ParentACClass)
            {
                if (childACClass.ACKind == Global.ACKinds.TPAProcessFunction)
                {
                    // Query contains all virtual methods that refer to a start-method
                    var queryVirtualMethods = childACClass.Methods.Where(c => c.PWACClassID.HasValue
                                                                            && c.ParentACClassMethodID.HasValue // ParentACClassMethodID points to "Start"-Method(Level1)
                                                                            && c.ACKind == Global.ACKinds.MSMethod);
                    foreach (var virtualMethod in queryVirtualMethods)
                    {
                        virtualMethodsOfChildFuncs.Add(new Tuple<ACClass, ACClassMethod>(childACClass, virtualMethod));
                        //virtualMethodsOfChilds.Add(virtualMethod);
                        MultipleFuncCounter4VMethod vMethodMupltipleEntry = vMethodsInMultipleFuncClasses.Where(c => c.Method == virtualMethod).FirstOrDefault();
                        if (vMethodMupltipleEntry == null)
                        {
                            vMethodMupltipleEntry = new MultipleFuncCounter4VMethod() { Method = virtualMethod, Count = 1 };
                            vMethodsInMultipleFuncClasses.Add(vMethodMupltipleEntry);
                        }
                        else
                            vMethodMupltipleEntry.Count++;
                    }
                }
                else
                {
                    UpdateMSMethodFunctionRekursiv(childACClass, msg);
                }
            }

            // currentlyAttachedMethods contains the attached methods: 
            // e.g. "VDosing"(Level2) and "PAFDosingB_VDosing"(Level2)
            List<ACClassMethod> currentlyAttachedMethods = acClass.Methods.Where(c => c.ACKind == Global.ACKinds.MSMethodFunction && c.ParentACClassMethodID.HasValue).ToList();
            //bool anyVirtualMethods = virtualMethodsOfChilds.Any();
            bool anyVirtualMethods = virtualMethodsOfChildFuncs.Any();
            // If this class doesn't have any virtual methods and attached methods, 
            // then its not a process module => ignore the following code and return.
            if (!anyVirtualMethods && !currentlyAttachedMethods.Any())
                return;

            // Compare attached Methods of ProcessModule with all existing virtual Methods of child ACComponents which are Function's
            // If it doesn't exist any more, delete attached Method if from this derivation-level
            foreach (var attachedMethod in currentlyAttachedMethods.ToArray())
            {
                if ((!anyVirtualMethods
                        //|| (!acClass.Methods.Where(c => c.ACClassMethodID == attachedMethod.ParentACClassMethodID).Any())
                        || (!attachedMethod.AttachedFromACClassID.HasValue && !virtualMethodsOfChildFuncs.Where(c => c.Item2.ACIdentifier == attachedMethod.ACIdentifier).Any())
                        || (attachedMethod.AttachedFromACClassID.HasValue && !virtualMethodsOfChildFuncs.Where(c => (c.Item1 == attachedMethod.AttachedFromACClass
                                                                                                                       || c.Item1.IsDerivedClassFrom(attachedMethod.AttachedFromACClass))
                                                                                                                    && c.Item2.ACIdentifier == attachedMethod.BasedOnACClassMethod.ACIdentifier).Any()
                            )
                        )
                    && attachedMethod.ACClass == acClass) // Check ist needed because attachedMethod is perhaps defined in the base class, then the method mustn't be deleted!
                {
                    attachedMethod.DeleteACObject(Database, true);
                    currentlyAttachedMethods.Remove(attachedMethod);
                }
            }

            // Check if AttachedFromACClassID is set properly, because older V4-Versions (prev. 09/2020) 
            // didn't support multiple equal Functions inside a Process-Module (e.g. Two times PAFDosing)
            foreach (var attachedMethod in currentlyAttachedMethods)
            {
                if (!attachedMethod.AttachedFromACClassID.HasValue)
                {
                    ACClass childFuncToAttach = attachedMethod.ACClass.ACClass_ParentACClass.Where(c => c.IsDerivedClassFrom(attachedMethod.BasedOnACClassMethod.ACClass)).FirstOrDefault();
                    if (childFuncToAttach != null)
                        attachedMethod.AttachedFromACClass = childFuncToAttach;
                }
            }

            if (anyVirtualMethods)
            {
                // Add new attached Methods for virtual Methods of child components
                // on this derivation level
                bool mustRepairFirst = false;
                //foreach (Tuple<ACClass, ACClassMethod> virtualMethodOfChildComp in virtualMethodsOfChildFuncs)
                //{
                //    if (!existsWithAttachedClassID)
                //    {
                //        ACClassMethod existingAttachedMethodWithoutAttachedClassID 
                //            = currentlyAttachedMethods.Where(c =>   !c.AttachedFromACClassID.HasValue 
                //                                                  && c.ParentACClassMethodID == virtualMethodOfChildComp.Item2.ACClassMethodID)
                //                                            .FirstOrDefault();
                //        // Bei älteren V4-Versionen wurde AttachedFromACClassID nicht gesetzt, jedoch die ParentACClassMethodID wurde richtig gesetzt
                //        // => korrigiere AttachedFromACClass
                //        if (existingAttachedMethodWithoutAttachedClassID != null)
                //        {
                //            // Falls noch eine zweite Funktion hinzugefügt worden ist vom selben typ, 
                //            // dann muss dies zuerst korrigiert werden, damit es keine Fehler gibt
                //            if (vMethodMupltipleEntry.Count > 1)
                //            {
                //                msg.MsgDetails.Add(new Msg()
                //                {
                //                    MessageLevel = eMsgLevel.Warning,
                //                    Message = String.Format("Can not add attached virtual Function with Name {0} from Function {1} to Class {1} because one already exists with the same name and without a reference to the attached function. " +
                //                    "Please remove your added Funticion and save the application tree again to repair it.",
                //                    virtualMethodOfChildComp.Item2.ACIdentifier, virtualMethodOfChildComp.Item1.ACIdentifier, acClass.ACIdentifier)
                //                });
                //                mustRepairFirst = true;
                //                continue;
                //            }
                //            else
                //            {
                //                existingAttachedMethodWithoutAttachedClassID.AttachedFromACClass = virtualMethodOfChildComp.Item1;
                //                continue;
                //            }
                //        }
                //        // Sonst wurde diese Methode noch nicht gefunden am Prozessmodul
                //        else
                //        {
                //            // Bei noch älteren Versionen wurde evtl. die ParentACClassMethodID nicht gesetzt, prüfe ob dies der Fall ist
                //            ACClassMethod existingAttachedMethodWithSameName = acClass.ACClassMethod_ACClass.ToArray()
                //                                                                .Where(c => c.ACIdentifier == virtualMethodOfChildComp.Item2.ACIdentifier)
                //                                                                .FirstOrDefault();
                //            // Korrektur falls Elternmethode (z.B. Start von Dosing) nicht gesetzt ist
                //            if (   existingAttachedMethodWithSameName != null
                //                && !existingAttachedMethodWithSameName.ParentACClassMethodID.HasValue 
                //                && existingAttachedMethodWithSameName.ACKind == Global.ACKinds.MSMethodFunction)
                //            {
                //                if (vMethodMupltipleEntry.Count > 1)
                //                {
                //                    msg.MsgDetails.Add(new Msg()
                //                    {
                //                        MessageLevel = eMsgLevel.Warning,
                //                        Message = String.Format("Can not add attached virtual Function with Name {0} from Function {1} to Class {1} because one already exists with the same name and without a reference to the attached function. " +
                //                        "Please remove your added Funticion and save the application tree again to repair it.",
                //                        virtualMethodOfChildComp.Item2.ACIdentifier, virtualMethodOfChildComp.Item1.ACIdentifier, acClass.ACIdentifier)
                //                    });
                //                    mustRepairFirst = true;
                //                    continue;
                //                }
                //                else
                //                {
                //                    existingAttachedMethodWithSameName.ACClassMethod1_ParentACClassMethod = virtualMethodOfChildComp.Item2;
                //                    existingAttachedMethodWithSameName.AttachedFromACClass = virtualMethodOfChildComp.Item1;
                //                    continue;
                //                }
                //            }
                //        }
                //    }
                //}

                if (mustRepairFirst)
                    return;

                foreach (Tuple<ACClass, ACClassMethod> virtualMethodOfChildComp in virtualMethodsOfChildFuncs)
                {
                    MultipleFuncCounter4VMethod vMethodMupltipleEntry
                        = vMethodsInMultipleFuncClasses.Where(c => c.Method == virtualMethodOfChildComp.Item2).FirstOrDefault();

                    ACClassMethod attachedMethodAtPM = null;
                    ACClass attachedChildClass4Method = virtualMethodOfChildComp.Item1;
                    while (attachedChildClass4Method != null && attachedMethodAtPM == null)
                    {
                        attachedMethodAtPM =
                                currentlyAttachedMethods.Where(c => c.AttachedFromACClassID == attachedChildClass4Method.ACClassID
                                                            && c.ParentACClassMethodID == virtualMethodOfChildComp.Item2.ACClassMethodID)
                                                            .FirstOrDefault();
                        if (attachedMethodAtPM != null)
                            break;
                        attachedChildClass4Method = attachedChildClass4Method.ACClass1_BasedOnACClass;
                    }

                    if (attachedMethodAtPM == null)
                    {
                        bool mustCombineMethodNameWithFunc
                            = currentlyAttachedMethods.Where(c => c.ACIdentifier == virtualMethodOfChildComp.Item2.ACIdentifier).Any()
                              || vMethodMupltipleEntry.Count > 1;
                        attachedMethodAtPM = ACClassMethod.NewAttachedFunctionMethod(Database, acClass, virtualMethodOfChildComp.Item2, virtualMethodOfChildComp.Item1, mustCombineMethodNameWithFunc);
                        if (attachedMethodAtPM != null)
                        {
                            currentlyAttachedMethods.Add(attachedMethodAtPM);
                            attachedChildClass4Method = null;
                        }
                    }
                }

                //foreach (ACClassMethod virtualMethodOfChildComp in virtualMethodsOfChilds)
                //{
                //    if (!currentlyAttachedMethods.Where(c => c.ParentACClassMethodID == virtualMethodOfChildComp.ACClassMethodID).Any())
                //    {
                //        ACClassMethod existingAttachedMethodWithSameName = acClass.ACClassMethod_ACClass.ToArray().Where(c => c.ACIdentifier == virtualMethodOfChildComp.ACIdentifier).FirstOrDefault();
                //        if (existingAttachedMethodWithSameName != null)
                //        {
                //            // Korrektur falls Elternmethode (z.B. Start von Dosing) nicht gesetzt ist
                //            if (!existingAttachedMethodWithSameName.ParentACClassMethodID.HasValue && existingAttachedMethodWithSameName.ACKind == Global.ACKinds.MSMethodFunction)
                //            {
                //                existingAttachedMethodWithSameName.ACClassMethod1_ParentACClassMethod = virtualMethodOfChildComp;
                //            }
                //            else
                //                msg.MsgDetails.Add(new Msg() { MessageLevel = eMsgLevel.Warning, Message = String.Format("Can not add attached virtual Function with Name {0} to Class {1} because one already exists with the same Name", virtualMethodOfChildComp.ACIdentifier, acClass.ACIdentifier) });
                //        }
                //        else
                //        {
                //            ACClassMethod newACClassMethod = ACClassMethod.NewAttachedFunctionMethod(Database, acClass, virtualMethodOfChildComp);
                //            if (newACClassMethod != null)
                //                newACClassMethod = null;
                //        }
                //    }
                //}
            }
        }
        #endregion

        #region Klassen-Point-Tree
        /// <summary>
        /// Gets the point tree.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns>IEnumerable{ACObjectItem}.</returns>
        public IEnumerable<ACObjectItem> GetPointTree(ACClass acClass)
        {
            List<ACObjectItem> pointTree = new List<ACObjectItem>();
            var query = acClass.Properties.Where(c => c.ACPropUsageIndex == 60 /*ConnectionPoint*/);
            foreach (var acClassProperty in query)
            {
                ACObjectItem treeEntry = new ACObjectItem(acClassProperty, acClassProperty.ACIdentifier);
                pointTree.Add(treeEntry);
            }
            return pointTree;
        }
        #endregion

        #region Property-Binding
        ///// <summary>
        ///// The _ current PB source project
        ///// </summary>
        //ACProject _CurrentPBSourceProject = null;
        ///// <summary>
        ///// Gets or sets the current PB source project.
        ///// </summary>
        ///// <value>The current PB source project.</value>
        //public ACProject CurrentPBSourceProject
        //{
        //    get
        //    {
        //        return _CurrentPBSourceProject;
        //    }
        //    set
        //    {
        //        if (_CurrentPBSourceProject != value)
        //        {
        //            _CurrentPBSourceProject = value;
        //            if (_CurrentPBSourceProject != null)
        //            {
        //                var prevItemRoot = _CurrentPBSourceItemRoot;
        //                _CurrentPBSourceItemRoot = new ACClassInfoWithItems { ValueT = _CurrentPBSourceProject.RootClass };
        //                ProjectItemFilter filter = new ProjectItemFilter() { SearchClassText = "" };
        //                InitProjectItems(prevItemRoot, CurrentPBSourceItemRoot, _CurrentPBSourceProject.RootClass.ACEntityChilds.OrderBy(c => c.ACIdentifier), 0, true, true, true, true, false, null, ProjectTreeType.Project, true, filter, ACProjectItemPreparedSearchFunctions.SearchACClassText);
        //            }
        //            else
        //            {
        //                _CurrentPBSourceItemRoot = null;
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// The _ current PB source item root
        ///// </summary>
        //ACClassInfoWithItems _CurrentPBSourceItemRoot = null;
        ///// <summary>
        ///// Gets the current PB source item root.
        ///// </summary>
        ///// <value>The current PB source item root.</value>
        //public ACClassInfoWithItems CurrentPBSourceItemRoot
        //{
        //    get
        //    {
        //        return _CurrentPBSourceItemRoot;
        //    }
        //}

        /// <summary>
        /// The _ PB source project list
        /// </summary>
        IEnumerable<ACProject> _PBSourceProjectList = null;
        /// <summary>
        /// Gets the PB source project list.
        /// </summary>
        /// <value>The PB source project list.</value>
        public IEnumerable<ACProject> PBSourceProjectList
        {
            get
            {
                if (_PBSourceProjectList == null)
                {
                    _PBSourceProjectList = Database.ACProject.Where(c => c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application
                                                                      || c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Service);
                }

                return _PBSourceProjectList;
            }
        }

        /// <summary>
        /// The _ current PB source AC class
        /// </summary>
        private ACClass _CurrentPBSourceACClass = null;
        /// <summary>
        /// Gets or sets the current PB source AC class.
        /// </summary>
        /// <value>The current PB source AC class.</value>
        public ACClass CurrentPBSourceACClass
        {
            get
            {
                return _CurrentPBSourceACClass;
            }
            set
            {
                _CurrentPBSourceACClass = value;
            }
        }

        /// <summary>
        /// Gets the PB source AC class property list.
        /// </summary>
        /// <param name="propertyMode">The property mode.</param>
        /// <returns>IEnumerable{ACClassProperty}.</returns>
        public IEnumerable<ACClassProperty> GetPBSourceACClassPropertyList(Global.PropertyModes propertyMode)
        {
            if (_CurrentPBSourceACClass == null)
                return new List<ACClassProperty>();
            switch (propertyMode)
            {
                //case PropertyModes.Configuration:
                //    return _CurrentPBSourceACClass.ACClassPropertyTopBaseList.Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConfigPointProperty);
                case Global.PropertyModes.Connections:
                    return _CurrentPBSourceACClass.BaseProperties.Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConnectionPoint);
                case Global.PropertyModes.Events:
                    return _CurrentPBSourceACClass.BaseProperties.Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.EventPoint || c.ACPropUsageIndex == (Int16)Global.ACPropUsages.EventPointSubscr);
                case Global.PropertyModes.Bindings:
                    return _CurrentPBSourceACClass.BaseProperties.Where(c => !c.IsProxyProperty && c.IsBroadcast);
                case Global.PropertyModes.Properties:
                case Global.PropertyModes.Relations:
                case Global.PropertyModes.Joblists:
                default:
                    return new List<ACClassProperty>();
            }
        }

        /// <summary>
        /// Drops the PBAC class property.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="targetVBDataObject">The target VB data object.</param>
        /// <param name="classOfTarget">The class of target.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="propertyMode">The property mode.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool DropPBACClassProperty(Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, ACClass classOfTarget, double x, double y, Global.PropertyModes propertyMode)
        {
            if (!IsEnabledDropPBACClassProperty(action, dropObject, targetVBDataObject, classOfTarget, x, y, propertyMode))
                return false;
            ACClassProperty targetProp = targetVBDataObject.GetACValue(typeof(ACClassProperty)) as ACClassProperty;
            ACClassProperty sourceProp = dropObject.GetACValue(typeof(ACClassProperty)) as ACClassProperty;

            switch (propertyMode)
            {
                case Global.PropertyModes.Bindings:
                    {
                        targetProp.MyCurrentAClassOfProperty = classOfTarget;
                        if (targetProp.ACClassPropertySource != null)
                        {
                            bool removeBinding = false;
                            if (targetProp.ACClassPropertySource == sourceProp)
                                removeBinding = true;
                            RemovePropertyBinding(targetProp, classOfTarget);
                            if (removeBinding)
                            {
                                return true;
                            }
                        }

                        // Bei Bit-Access-Typen, kann die hinzu-konfigurierte Source-Property(z.b. von Data-Access), den Datentype, der Target-Property annehmen
                        // damit keine ACPropertyNetTargetConverter<T, S>-Instanz benötigt wird:
                        if (sourceProp.ACKind == Global.ACKinds.PSPropertyExt)
                        {
                            if (targetProp.ValueTypeACClass.ObjectType != sourceProp.ValueTypeACClass.ObjectType)
                            {
                                Type typeBitAccess = typeof(IBitAccess);
                                if (typeBitAccess.IsAssignableFrom(sourceProp.ValueTypeACClass.ObjectType) && typeBitAccess.IsAssignableFrom(targetProp.ValueTypeACClass.ObjectType))
                                {
                                    string underlyingType1 = BitAccess<short>.GetUnderlyingTypeOfBitAccess(sourceProp.ValueTypeACClass.ObjectType);
                                    string underlyingType2 = BitAccess<short>.GetUnderlyingTypeOfBitAccess(targetProp.ValueTypeACClass.ObjectType);
                                    if (underlyingType1 == underlyingType2)
                                        sourceProp.ValueTypeACClass = targetProp.ValueTypeACClass;
                                }
                                else if (typeBitAccess.IsAssignableFrom(targetProp.ValueTypeACClass.ObjectType))
                                {
                                    string underlyingType1 = sourceProp.ValueTypeACClass.ObjectType.Name;
                                    string underlyingType2 = BitAccess<short>.GetUnderlyingTypeOfBitAccess(targetProp.ValueTypeACClass.ObjectType);
                                    if (underlyingType1 == underlyingType2)
                                        sourceProp.ValueTypeACClass = targetProp.ValueTypeACClass;
                                }
                            }
                        }

                        ACClassPropertyRelation binding = ACClassPropertyRelation.NewACClassPropertyRelation(this.Database, CurrentPBSourceACClass, sourceProp.TopBaseACClassProperty, classOfTarget, targetProp.TopBaseACClassProperty);
                        binding.ConnectionType = Global.ConnectionTypes.Binding;
                        targetProp.TopBaseACClassProperty.RaiseOnPropertyChanged("ACClassPropertyBindingToSource");
                        targetProp.TopBaseACClassProperty.RaiseOnPropertyChanged("ACClassPropertySource");
                        return true;
                    }
                case Global.PropertyModes.Connections:
                    ACClassPropertyRelation.NewACClassPropertyRelation(Database, CurrentPBSourceACClass, sourceProp.TopBaseACClassProperty, classOfTarget, targetProp.TopBaseACClassProperty);
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Determines whether [is enabled drop PBAC class property] [the specified action].
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="targetVBDataObject">The target VB data object.</param>
        /// <param name="classOfTarget">The class of target.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="propertyMode">The property mode.</param>
        /// <returns><c>true</c> if [is enabled drop PBAC class property] [the specified action]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDropPBACClassProperty(Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, ACClass classOfTarget, double x, double y, Global.PropertyModes propertyMode)
        {
            if ((dropObject == null) || (targetVBDataObject == null) || (classOfTarget == null) || (CurrentPBSourceACClass == null))
                return false;

            ACClassProperty targetProp = targetVBDataObject.GetACValue(typeof(ACClassProperty)) as ACClassProperty;
            ACClassProperty sourceProp = dropObject.GetACValue(typeof(ACClassProperty)) as ACClassProperty;
            if (targetProp == null || sourceProp == null)
                return false;

            switch (propertyMode)
            {
                case Global.PropertyModes.Bindings:
                    {
                        return ArePropertiesBindable(targetProp, sourceProp);
                    }
                case Global.PropertyModes.Connections:
                    if (sourceProp.ACPropUsage != Global.ACPropUsages.ConnectionPoint)
                        return false;
                    if (targetProp.ACPropUsage != Global.ACPropUsages.ConnectionPoint)
                        return false;
                    return true;
            }

            return false;
        }

        public static bool ArePropertiesBindable(ACClassProperty targetProp, ACClassProperty sourceProp)
        {
            if (!targetProp.IsProxyProperty || sourceProp.IsProxyProperty || !targetProp.IsBroadcast || !sourceProp.IsBroadcast)
                return false;
            if (targetProp.ValueTypeACClass != sourceProp.ValueTypeACClass)
            {
                if ((targetProp.ValueTypeACClass.ObjectType == null) || (sourceProp.ValueTypeACClass.ObjectType == null))
                    return false;
                return ACPropertyNetTargetConverter<object, object>.AreTypesCompatible(targetProp.ValueTypeACClass.ObjectType, sourceProp.ValueTypeACClass.ObjectType);
            }
            return true;
        }


        /// <summary>
        /// Removes the property binding.
        /// </summary>
        /// <param name="currentPBTargetACClassProperty">The current PB target AC class property.</param>
        /// <param name="currentACClass">The current AC class.</param>
        public void RemovePropertyBinding(ACClassProperty currentPBTargetACClassProperty, ACClass currentACClass)
        {
            if (!IsEnabledRemovePropertyBinding(currentPBTargetACClassProperty, currentACClass))
                return;
            if (currentPBTargetACClassProperty.ACClassPropertyBindingToSource != null)
            {
                currentPBTargetACClassProperty.ACClassPropertyBindingToSource.DeleteACObject(this.Database, false);
                currentPBTargetACClassProperty.RaiseOnPropertyChanged("ACClassPropertyBindingToSource");
                currentPBTargetACClassProperty.RaiseOnPropertyChanged("ACClassPropertySource");
            }
            if (currentPBTargetACClassProperty.TopBaseACClassProperty.ACClassPropertyBindingToSource != null)
            {
                currentPBTargetACClassProperty.TopBaseACClassProperty.ACClassPropertyBindingToSource.DeleteACObject(this.Database, false);
                currentPBTargetACClassProperty.TopBaseACClassProperty.RaiseOnPropertyChanged("ACClassPropertyBindingToSource");
                currentPBTargetACClassProperty.TopBaseACClassProperty.RaiseOnPropertyChanged("ACClassPropertySource");
            }
        }

        /// <summary>
        /// Determines whether [is enabled remove property binding] [the specified current PB target AC class property].
        /// </summary>
        /// <param name="currentPBTargetACClassProperty">The current PB target AC class property.</param>
        /// <param name="currentACClass">The current AC class.</param>
        /// <returns><c>true</c> if [is enabled remove property binding] [the specified current PB target AC class property]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledRemovePropertyBinding(ACClassProperty currentPBTargetACClassProperty, ACClass currentACClass)
        {
            if (currentPBTargetACClassProperty == null)
                return false;
            currentPBTargetACClassProperty.TopBaseACClassProperty.MyCurrentAClassOfProperty = currentACClass;
            if (currentPBTargetACClassProperty.TopBaseACClassProperty.ACClassPropertyBindingToSource == null && currentPBTargetACClassProperty.ACClassPropertyBindingToSource == null)
                return false;
            return true;
        }
        #endregion

        #endregion
    }

}

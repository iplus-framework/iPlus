// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-19-2013
// ***********************************************************************
// <copyright file="BSOiPlusStudio.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.manager;
using gip.core.autocomponent;
using gip.core.datamodel.ACContainer;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOiPlusStudio
    /// </summary>
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'Development Environment'}de{'Entwicklungsumgebung'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACProject.ClassName)]
    [ACClassConstructorInfo(
        new object[]
        {
            new object[] {"AutoFilter", Global.ParamOption.Optional, typeof(String) },
            new object[] {"AutoLoad", Global.ParamOption.Optional, typeof(String) }
        }
    )]
    public partial class BSOiPlusStudio : BSOiPlusBase, IVBFindAndReplaceDBSearch, IACComponentDesignManagerHost
    {
        #region private Property
        /// <summary>
        /// The _ AC project manager
        /// </summary>
        ACProjectManager _ACProjectManager;
        /// <summary>
        /// The _ PLC project manager
        /// </summary>
        ACProjectManager _PLCProjectManager = null; // Für ProjectLibraryClass (Klassenbibliothek)
        /// <summary>
        /// The _ PLAD project manager
        /// </summary>
        ACProjectManager _PLADProjectManager = null; // Für ProjectLibraryAppDefinition (Anwendungsdefinition)


        Type _IACObjectEntityType = typeof(IACObjectEntity);
        #endregion

        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="BSOiPlusStudio" /> class.
        /// </summary>
        /// <param name="typeACClass">The type AC class.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOiPlusStudio(ACClass typeACClass, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(typeACClass, content, parentACObject, parameter)
        {
        }

        /// <summary>
        /// The _ load AC class
        /// </summary>
        ACClass _LoadACClass = null;
        /// <summary>
        /// ACs the init.
        /// </summary>
        /// <param name="startChildMode">The start child mode.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (Root != null && !Root.Environment.License.MayUserDevelop)
            {
                Messages.Warning(this, "Warning50026");
                throw new Exception("No license for development");
            }

            if (!base.ACInit(startChildMode))
                return false;

            ACProject acProject = null;
            _LoadACClass = null;

            string autoLoad = ParameterValue("AutoLoad") as string;
            if (!string.IsNullOrEmpty(autoLoad))
            {
                _LoadACClass = ACUrlCommand(autoLoad) as ACClass;


                if (_LoadACClass != null)
                {
                    switch (_LoadACClass.ACKind)
                    {
                        case Global.ACKinds.TPAModule:
                            _ShowModule = true;
                            break;
                        case Global.ACKinds.TPAProcessModule:
                            _ShowProcessModule = true;
                            break;
                        case Global.ACKinds.TPAProcessFunction:
                            _ShowProcessFunction = true;
                            break;
                        case Global.ACKinds.TPABGModule:
                            _ShowBGModule = true;
                            break;
                    }
                    acProject = _LoadACClass.ACProject;

                    AccessPrimary.NavACQueryDefinition.SearchWord = acProject.ACProjectName;
                }
            }
            Search();
            if (acProject == null)
            {
                acProject = ACProjectList.Where(c => c.ACProjectTypeIndex == (short) Global.ACProjectTypes.Root).First();
            }

            SelectedACProject = acProject;

            Load();

            if (PropertyModeList != null && PropertyModeList.Any())
                CurrentPropertyMode = PropertyModeList.First();
            if (MethodModeList != null && MethodModeList.Any())
                CurrentMethodMode = MethodModeList.First();
            var query = PBSourceProjectList.Where(c => c.IsDataAccess);
            if (query.Any())
                CurrentPBSourceProject = query.First();
            return true;
        }

        /// <summary>
        /// ACs the post init.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACPostInit()
        {
            if (!base.ACPostInit())
                return false;

            return true;
        }

        /// <summary>
        /// ACs the de init.
        /// </summary>
        /// <param name="deleteACClassTask">if set to <c>true</c> [delete AC class task].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_CurrentACComponent != null)
            {
                _CurrentACComponent.Detach();
                _CurrentACComponent = null;
            }

            this._AccessACClass = null;
            this._AccessACClassDesign = null;
            this._AccessACClassMethod = null;
            this._AccessACClassProperty = null;
            this._AccessACTranslation = null;
            this._AccessOnlineValue = null;
            this._acClassDesignIDList = null;
            this._ACClassDesignList = null;
            this._acClassList = null;
            this._ACClassMethodControlList = null;
            this._ACClassMethodIconList = null;
            this._ACClassMethodList = null;
            this._ACClassPropertyControlList = null;
            this._ACClassPropertyIconList = null;
            this._ACClassPropertyList = null;
            this._ACClassToSwitch = null;
            this._CurrentACClass = null;
            this._CurrentACClassDesign = null;
            this._CurrentACClassMethod = null;
            this._CurrentACClassMethodControl = null;
            this._CurrentACClassMethodIcon = null;
            this._CurrentACClassProperty = null;
            this._CurrentACClassPropertyControl = null;
            this._CurrentACClassPropertyIcon = null;
            this._CurrentACClassPropertyTemp = null;
            this._CurrentACComponent = null;
            this._CurrentACProperty = null;
            this._CurrentACTranslation = null;
            this._CurrentCLProjectItem = null;
            this._CurrentConfigACClassProperty = null;
            this._CurrentConfigPointACClassProperty = null;
            this._CurrentFirstACTranslation = null;
            this._CurrentInvokingACCommand = null;
            this._CurrentLayouts = null;
            this._CurrentMenuEntry = null;
            this._CurrentMenuEntryChangeInfo = null;
            this._CurrentNewACClass = null;
            this._CurrentNewACClassDesign = null;
            this._CurrentNewACIdentifier = null;
            this._CurrentNewACProject = null;
            this._CurrentNewMessage = null;
            this._CurrentParameterValue = null;
            this._CurrentPBSourceACClass = null;
            this._CurrentPBSourceACClassProperty = null;
            this._CurrentPBSourceItem = null;
            this._CurrentPLADProjectItem = null;
            this._CurrentPointConfig = null;
            this._CurrentPointStateInfo = null;
            this._CurrentProjectItem = null;
            this._CurrentProjectItemRootChangeInfo = null;
            this._CurrentPropBindingToSource = null;
            this._CurrentPropertyMode = null;
            this._CurrentPropertyRelationFrom = null;
            this._CurrentPropertyRelationTo = null;
            this._CurrentSelectedMenuEntry = null;
            this._CurrentTranslation = null;
            this._CurrentVisualACClass = null;
            this._LoadACClass = null;
            this._MethodModeList = null;
            this._moveInfoList = null;
            this._NewACProjectAppDefinitionList = null;
            this._NewPrefix = null;
            this._PointStateInfoList = null;
            this._PropertyModeList = null;
            this._PropertyRelationFromList = null;
            this._PropertyRelationToList = null;
            this._RelationACClassList = null;
            this._SearchClassText = null;
            this._SearchPLADProjectItem = null;
            this._SearchPLADProjectItem = null;
            this._SelectedACClassDesign = null;
            this._SelectedACClassHierarchy = null;
            this._SelectedACClassMethod = null;
            this._SelectedOnlineValue = null;
            this._SelectedPointStateInfo = null;
            this._ShowACClass = null;
            this._TranslationList = null;
            var b = base.ACDeInit(deleteACClassTask);
            if (_AccessPrimary != null)
            {
                _AccessPrimary.ACDeInit(false);
                _AccessPrimary = null;
            }
            return b;
        }

        public override object Clone()
        {
            BSOiPlusStudio clone = base.Clone() as BSOiPlusStudio;
            clone.CurrentACClass = this.CurrentACClass;
            return clone;
        }

        public override object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            if (acUrl == Const.CmdPrintScreenToIcon)
            {
                byte[] result = (byte[])acParameter[0];
                if (result != null)
                    OnIconGenerated(result);
                return null;
            }
            else
                return base.ACUrlCommand(acUrl, acParameter);
        }
        #endregion

        #region BSO->ACDropData
        /// <summary>
        /// Called from a WPF-Control inside it's ACAction-Method when a relevant interaction-event as occured (e.g. Drag-And-Drop).
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source.</param>
        public override void ACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            if (targetVBDataObject is IVBContent)
            {
                switch (((IVBContent)targetVBDataObject).VBContent)
                {
                    case "CurrentProjectItem":
                        {
                            DropACClass(actionArgs.ElementAction, actionArgs.DropObject, targetVBDataObject, actionArgs.X, actionArgs.Y);
                            return;
                        }
                    case "CurrentACClassProperty":
                        {
                            DropPBACClassProperty(Global.ElementActionType.Drop, actionArgs.DropObject, targetVBDataObject, actionArgs.X, actionArgs.Y);
                            return;
                        }
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
            if (targetVBDataObject is IVBContent)
            {
                switch (((IVBContent)targetVBDataObject).VBContent)
                {
                    case "CurrentProjectItem":
                        return IsEnabledDropACClass(actionArgs.ElementAction, actionArgs.DropObject, targetVBDataObject, actionArgs.X, actionArgs.Y);
                    case "CurrentACClassProperty":
                        return IsEnabledDropPBACClassProperty(Global.ElementActionType.Drop, actionArgs.DropObject, targetVBDataObject, actionArgs.X, actionArgs.Y);
                }
            }
            return base.IsEnabledACActionToTarget(targetVBDataObject, actionArgs);
        }
        #endregion

        #region BSO->ACProperty

        #region New ACProject
        /// <summary>
        /// The _ current new AC project
        /// </summary>
        ACProject _CurrentNewACProject = null;
        /// <summary>
        /// Gets or sets the current new AC project.
        /// </summary>
        /// <value>The current new AC project.</value>
        [ACPropertyCurrent(9999, "NewACProject")]
        public ACProject CurrentNewACProject
        {
            get
            {
                return _CurrentNewACProject;
            }
            set
            {
                _CurrentNewACProject = value;
                OnPropertyChanged("CurrentNewACProject");
            }
        }

        /// <summary>
        /// Gets the new AC project type list.
        /// </summary>
        /// <value>The new AC project type list.</value>
        [ACPropertyList(9999, "NewACProjectType")]
        public IEnumerable<ACValueItem> NewACProjectTypeList
        {
            get
            {
                //Neue Projekte können nur vom Typ Anwendung oder Anwendungstyp sein
                //Global.ACProjectTypes.ACAppDefinition;
                //Global.ACProjectTypes.ACApplication
                return Database.ContextIPlus.ACProjectTypeList.Where(c => (Global.ACProjectTypes)c.Value == Global.ACProjectTypes.AppDefinition || (Global.ACProjectTypes)c.Value == Global.ACProjectTypes.Application);
            }
        }

        /// <summary>
        /// The _ new AC project app definition list
        /// </summary>
        List<ACProject> _NewACProjectAppDefinitionList = null;
        /// <summary>
        /// Gets the new AC project app definition list.
        /// </summary>
        /// <value>The new AC project app definition list.</value>
        [ACPropertyList(9999, "NewACProjectAppDefinition")]
        public IEnumerable<ACProject> NewACProjectAppDefinitionList
        {
            get
            {
                if (_NewACProjectAppDefinitionList == null)
                {
                    _NewACProjectAppDefinitionList = Database.ContextIPlus.ACProject
                                .Where(c => c.ACProjectTypeIndex == (short) Global.ACProjectTypes.AppDefinition)
                                .OrderBy(c => c.ACProjectName)
                                .ToList();
                }
                return _NewACProjectAppDefinitionList;
            }
        }

        #endregion

        #region 1. ACProject
        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        /// <summary>
        /// The _ access primary
        /// </summary>
        ACAccessNav<ACProject> _AccessPrimary;
        /// <summary>
        /// Gets the access primary.
        /// </summary>
        /// <value>The access primary.</value>
        [ACPropertyAccessPrimary(9999, "ACProject")]
        public ACAccessNav<ACProject> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessPrimary = navACQueryDefinition.NewAccessNav<ACProject>(ACProject.ClassName, this);
                }
                return _AccessPrimary;
            }
        }

        /// <summary>
        /// Gets or sets the current AC project.
        /// </summary>
        /// <value>The current AC project.</value>
        [ACPropertyCurrent(9999, "ACProject")]
        public override ACProject CurrentACProject
        {
            get
            {
                return AccessPrimary.Current;
            }
            set
            {

                if (AccessPrimary.Current != value)
                {
                    SetCurrentACProject(value);
                }
            }
        }

        private void SetCurrentACProject(ACProject value)
        {
            if (CurrentACProject != null)
            {
                CurrentACProject.PropertyChanged -= CurrentACProject_PropertyChanged;
            }
            IACObject windowClassLibrary = FindGui("", "", "*ProjectLibrary", this.ACIdentifier);
            if (windowClassLibrary != null)
                windowClassLibrary.ACUrlCommand(Const.CmdClose);

            AccessPrimary.Current = value;
            if (CurrentACProject != null)
            {
                CurrentACProject.PropertyChanged += new PropertyChangedEventHandler(CurrentACProject_PropertyChanged);
            }

            UpdateDefaultVisibilityFilters();
            ProjectManager.LoadACProject(CurrentACProject, ProjectTreePresentationMode, ProjectTreeVisibilityFilter);

            if (_LoadACClass != null)
            {
                ACClassInfoWithItems reqestedACClass = null;
                ProjectManager.MapClassToItem.TryGetValue(_LoadACClass, out reqestedACClass);
                if (reqestedACClass != null)
                    CurrentProjectItem = reqestedACClass;
                _LoadACClass = null;
                if (CurrentProjectItem == null)
                    CurrentProjectItem = CurrentProjectItemRoot;
            }
            else
            {
                CurrentProjectItem = CurrentProjectItemRoot;
            }
            OnPropertyChanged("CurrentACProject");
            _MethodModeList = null;
            OnPropertyChanged("MethodModeList");
            OnPropertyChanged("PWACClassList");
            OnPropertyChanged("ControlModeProjectFilter");

            RefreshPLADProjectTree();
        }

        /// <summary>
        /// Handles the PropertyChanged event of the CurrentACProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        void CurrentACProject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //switch (e.PropertyName)
            //{
            //    //case "CompanyAddressID":
            //    //    OnPropertyChanged("FactoryCompanyAddressDepartmentList");
            //    //    break;
            //}
        }

        /// <summary>
        /// Gets or sets the selected AC project.
        /// </summary>
        /// <value>The selected AC project.</value>
        [ACPropertySelected(9999, "ACProject")]
        public ACProject SelectedACProject
        {
            get
            {
                return AccessPrimary.Selected;
            }
            set
            {
                AccessPrimary.Selected = value;
                OnPropertyChanged("SelectedACProject");
            }
        }

        /// <summary>
        /// Gets or sets the AC project list.
        /// </summary>
        /// <value>The AC project list.</value>
        [ACPropertyList(9999, "ACProject", "en{'Application'}de{'Anwendung'}")]
        public override IEnumerable<ACProject> ACProjectList
        {
            get
            {
                return AccessPrimary.NavList;
            }
        }

        /// <summary>
        /// Gets the current project item root.
        /// </summary>
        /// <value>The current project item root.</value>
        [ACPropertyCurrent(9999, "ProjectItemRoot")]
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
        [ACPropertyChangeInfo(9999, "ProjectItem")]
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

        /// <summary>
        /// The _ current project item
        /// </summary>
        ACClassInfoWithItems _CurrentProjectItem = null;
        /// <summary>
        /// Gets or sets the current project item.
        /// </summary>
        /// <value>The current project item.</value>
        [ACPropertyCurrent(9999, "ProjectItem")]
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
                    if (_CurrentProjectItem != null && CurrentProjectItem.ValueT != null)
                    {
                        if (CurrentACClass != CurrentProjectItem.ValueT)
                        {
                            CurrentACClass = CurrentProjectItem.ValueT;
                        }
                        OnPropertyChanged("CurrentProjectItem");
                    }
                }
            }
        }
        #endregion

        #region 1.1 ACClass
        /// <summary>
        /// The _ access AC class
        /// </summary>
        ACAccess<ACClass> _AccessACClass;
        /// <summary>
        /// Gets the access AC class.
        /// </summary>
        /// <value>The access AC class.</value>
        [ACPropertyAccess(9999, "ACClass")]
        public override ACAccess<ACClass> AccessACClass
        {
            get
            {
                if (_AccessACClass == null && ACType != null)
                {
                    ACQueryDefinition acQueryDefinition = AccessPrimary.NavACQueryDefinition.ACUrlCommand(Const.QueryPrefix + ACClass.ClassName) as ACQueryDefinition;
                    _AccessACClass = acQueryDefinition.NewAccess<ACClass>(ACClass.ClassName, this);
                }
                return _AccessACClass;
            }
        }

        /// <summary>
        /// The _ current AC class
        /// </summary>
        ACClass _CurrentACClass = null;
        /// <summary>
        /// Gets or sets the current AC class.
        /// </summary>
        /// <value>The current AC class.</value>
        [ACPropertyCurrent(9999, "ACClass")]
        public override ACClass CurrentACClass
        {
            get
            {
                return _CurrentACClass;
            }
            set
            {
                if (_CurrentACClass != value)
                {
                    Global.ACKinds oldACType = Global.ACKinds.TACUndefined;

                    if (CurrentACClass != null)
                    {
                        oldACType = CurrentACClass.ACKind;
                        CurrentACClass.PropertyChanged -= CurrentACClass_PropertyChanged;
                    }

                    _CurrentACClass = null;

                    _CurrentACClass = value;
                    if (_CurrentACClass != null)
                        CurrentACClass.RefreshCachedMethods();

                    OnPropertyChanged("CurrentACClass");
                    if (CurrentACClass != null)
                    {
                        IACObject visualACClass = null;
                        string acUrlComponent = CurrentACClass.GetACUrlComponent();
                        if (!String.IsNullOrEmpty(acUrlComponent) && !acUrlComponent.StartsWith(Const.BusinessobjectsACUrl))
                        {
                            visualACClass = ACUrlCommand(acUrlComponent) as IACObject;
                        }
                        if (visualACClass == null)
                        {
                            string acUrlObject = CurrentACClass.GetACUrl(this.Database.ContextIPlus);
                            visualACClass = ACUrlCommand(acUrlObject) as IACObject;
                        }
                        CurrentVisualACClass = visualACClass;
                        CurrentACClass.PropertyChanged += new PropertyChangedEventHandler(CurrentACClass_PropertyChanged);

                        if (string.IsNullOrEmpty(CurrentACClass.ACURLCached))
                            CurrentACClass.ACURLCached = CurrentACClass.GetACUrl();

                        if (string.IsNullOrEmpty(CurrentACClass.ACURLComponentCached) && (CurrentACClass.ACProject.ACProjectType == Global.ACProjectTypes.Application || 
                                                                                         CurrentACClass.ACProject.ACProjectType == Global.ACProjectTypes.Service))
                            CurrentACClass.ACURLComponentCached = CurrentACClass.GetACUrlComponent();
                    }
                    else
                    {
                        CurrentVisualACClass = null;
                    }

                    //_ACClassCompositionList = null;
                    _ACClassMethodList = null;
                    _ACClassPropertyList = null;
                    _ACClassDesignList = null;
                    _RelationACClassList = null;
                    _CurrentACClassMethod = null;
                    if (ACClassDesignList != null && ACClassDesignList.Any())
                    {
                        var query = ACClassDesignList.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.DSDesignMenu);
                        if (query.Any())
                        {
                            SelectedACClassDesign = query.First();
                        }
                        else
                        {
                            SelectedACClassDesign = ACClassDesignList.First();
                        }
                    }
                    else
                    {
                        CurrentACClassDesign = null;
                    }
                    CurrentACClassProperty = null;
                    //CurrentACClassComposition = null;
                    CurrentMenuEntry = null;
                    CurrentPointConfig = null;
                    var currentACClass = _CurrentACClass;
                    _CurrentACClass = null;
                    //UpdateCurrentClassDesign(oldACType, Global.ACTypes.TACNone);
                    _CurrentACClass = currentACClass;
                    CurrentACClassMethod = null;

                    //UpdateCurrentClassDesign(Global.ACTypes.TACNone, _CurrentACClass == null ? Global.ACTypes.TACNone : _CurrentACClass.ACType);
                    UpdateCurrentClassDesign();
                    _PropertyModeList = null;
                    OnPropertyChanged("PropertyModeList");
                    OnPropertyChanged("CurrentPropertyMode");
                    OnPropertyChanged("PWACClassList");
                    OnPropertyChanged("PAACClassList");
                    OnPropertyChanged("VisualACClassDesignList");
                    OnPropertyChanged("ControlACClassDesignList");
                    OnPropertyChanged("ACClassMethodList");
                    OnPropertyChanged("ACClassPropertyList");
                    OnPropertyChanged("ACClassDesignList");
                    OnPropertyChanged("ACClassCompositionList");
                    OnPropertyChanged("RelationACClassList");
                    OnPropertyChanged("PointConfigList");
                    OnPropertyChanged("ACClassHierarchyList");
                    if (ACClassHierarchyList != null)
                        SelectedACClassHierarchy = ACClassHierarchyList.First();
                    OnPropertyChanged("PointTreeList");
                    OnPropertyChanged("OnlineValueList");
                    OnPropertyChanged("ConfigPointACClassPropertyList");
                    if (ConfigPointACClassPropertyList.Any())
                        CurrentConfigPointACClassProperty = ConfigPointACClassPropertyList.First();
                    else
                        CurrentConfigPointACClassProperty = null;
                    ShowTextAll = false;
                    ShowTextBase = false;
                    RefreshTranslation();
                }
            }
        }

        /// <summary>
        /// The _ current visual AC class
        /// </summary>
        IACObject _CurrentVisualACClass = null;
        /// <summary>
        /// Gets or sets the current visual AC class.
        /// </summary>
        /// <value>The current visual AC class.</value>
        [ACPropertyInfo(9999)]
        public IACObject CurrentVisualACClass
        {
            get
            {
                return _CurrentVisualACClass;
            }
            set
            {
                _CurrentVisualACClass = value;
                if (_DesignManagerACClassDesign != null)
                    _DesignManagerACClassDesign.CurrentDesignContext = value;
                OnPropertyChanged("CurrentVisualACClass");
            }
        }

        public IACObject CurrentDesignContext
        {
            get { return CurrentVisualACClass; }
        }

        /// <summary>
        /// Gets the current AC class VBV isual item.
        /// </summary>
        /// <value>The current AC class VBV isual item.</value>
        public IACInteractiveObject CurrentACClassVBVIsualItem
        {
            get
            {
                foreach (var client in ReferencePoint.ConnectionList)
                {
                    if (client is Control)
                    {
                        Control control = client as Control;
                        DependencyObject dObject = LogicalTreeHelper.FindLogicalNode(control, "VisualControl");
                        if (dObject != null)
                        {
                            return dObject as IACInteractiveObject;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The _ current new AC class
        /// </summary>
        ACClass _CurrentNewACClass;
        /// <summary>
        /// Gets or sets the current new AC class.
        /// </summary>
        /// <value>The current new AC class.</value>
        [ACPropertyInfo(9999)]
        public ACClass CurrentNewACClass
        {
            get
            {
                return _CurrentNewACClass;
            }
            set
            {
                _CurrentNewACClass = value;
                OnPropertyChanged("CurrentNewACClass");
            }
        }

        /// <summary>
        /// The _ current new AC class
        /// </summary>
        ACClass _ACClassToSwitch;
        /// <summary>
        /// Gets or sets the current new AC class.
        /// </summary>
        /// <value>The current new AC class.</value>
        [ACPropertyInfo(9999)]
        public ACClass ACClassToSwitch
        {
            get
            {
                return _ACClassToSwitch;
            }
            set
            {
                _ACClassToSwitch = value;
                OnPropertyChanged("ACClassToSwitch");
            }
        }

        private bool _NewSubclass = false;

        #endregion

        #region Hilfslisten
        /// <summary>
        /// Gets the PWAC class list.
        /// </summary>
        /// <value>The PWAC class list.</value>
        [ACPropertyList(9999, "PWACClass")]
        public IEnumerable<ACClass> PWACClassList
        {
            get
            {
                if (CurrentACClass == null)
                    return null;
                switch (CurrentACClass.ACKind)
                {
                    case Global.ACKinds.TACApplicationManager:
                        return Database.ContextIPlus.ACClass.Where(c =>    c.ACProject.ACProjectTypeIndex == (short) Global.ACProjectTypes.ClassLibrary
                                                                        && c.ACKindIndex == (short) Global.ACKinds.TPWGroup
                                                                        && !c.IsAbstract)
                                                            .OrderBy(c => c.ACIdentifier);
                    case Global.ACKinds.TPAProcessModule:
                    case Global.ACKinds.TPAProcessFunction:
                        return Database.ContextIPlus.ACClass.Where(c =>    c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary 
                                                                        && c.ACKindIndex == (short) Global.ACKinds.TPWMethod
                                                                        && !c.IsAbstract)
                                                            .OrderBy(c => c.ACIdentifier);
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the PAAC class list.
        /// </summary>
        /// <value>The PAAC class list.</value>
        [ACPropertyList(9999, "PAACClass")]
        public IEnumerable<ACClass> PAACClassList
        {
            get
            {
                if (CurrentACClass == null)
                    return null;
                if (CurrentACProject.ACProjectType != Global.ACProjectTypes.Application)
                    return null;
                if (CurrentACClass.ACClass1_BasedOnACClass == null)
                    return null;
                ACClass acClass = CurrentACClass.ACClass1_BasedOnACClass.BaseClassWithASQN;
                if (acClass == null)
                    return null;
                switch (CurrentACClass.ACKind)
                {
                    case Global.ACKinds.TACApplicationManager:
                    case Global.ACKinds.TPAProcessModule:
                    case Global.ACKinds.TPAProcessFunction:
                    case Global.ACKinds.TPAModule:
                    case Global.ACKinds.TPABGModule:
                        {
                            List<ACClass> acClassList = new List<ACClass>();
                            FillACClassList(acClass, acClassList);
                            return acClassList.OrderBy(c => c.ACIdentifier);
                        }
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Fills the AC class list.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassList">The ac class list.</param>
        private void FillACClassList(ACClass acClass, List<ACClass> acClassList)
        {
            acClassList.Add(acClass);
            foreach (var acClass1 in acClass.ACClass_BasedOnACClass.Where(c => c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary))
            {
                FillACClassList(acClass1, acClassList);
            }
        }


        /// <summary>
        /// Gets the base AC class list.
        /// </summary>
        /// <value>The base AC class list.</value>
        [ACPropertyList(9999, "BaseACClass")]
        public IEnumerable<ACClass> BaseACClassList
        {
            get
            {
                return ProjectManager.BaseACClassList(CurrentACProject, CurrentACClass, Database.ContextIPlus);
            }
        }
        #endregion

        #region Filter für Projekttree
        protected void UpdateDefaultVisibilityFilters()
        {
            try
            {
                _LockRefreshProjectTree = true;
                //if (CurrentACProject != null && CurrentACProject.ACProjectType == Global.ACProjectTypes.Applicationproject)
                //{
                //    ShowProcessModule = true;
                //    ShowProcessFunction = true;
                //    ShowModule = true;
                //    ShowBGModule = false;
                //    ShowACClass = null;
                //    SearchClassText = null;
                //}
                //else // if (CurrentACProject == null)
                //{
                    ShowProcessModule = false;
                    ShowProcessFunction = false;
                    ShowModule = false;
                    ShowBGModule = false;
                    ShowACClass = null;
                    SearchClassText = null;
                    SearchPLADProjectItem = null;
                    SearchPLADProjectItem = null;
                //}

                ShowGroup = !(CurrentACProject != null && (CurrentACProject.ACProjectType == Global.ACProjectTypes.Application || CurrentACProject.ACProjectType == Global.ACProjectTypes.AppDefinition));

            }
            finally
            {
                _LockRefreshProjectTree = false;
            }
        }

        protected ACClassInfoWithItems.VisibilityFilters ProjectTreeVisibilityFilter
        {
            get
            {
                ACClassInfoWithItems.VisibilityFilters filter
                    = new ACClassInfoWithItems.VisibilityFilters()
                    {
                        IncludeProcessModules = this.ShowProcessModule,
                        IncludeProcessFunctions = this.ShowProcessFunction,
                        IncludeModules = this.ShowModule,
                        IncludeBackgroundModules = this.ShowBGModule,
                        FilterACClass = this.ShowACClass,
                        SearchText = this.SearchClassText,
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
                        DisplayTreeAsMenu = null
                    };
                return mode;
            }
        }

        private bool _LockRefreshProjectTree = false;
        private void RefreshProjectTree(bool forceRebuildTree = false, bool onlyClassLibrary = false)
        {
            if (_LockRefreshProjectTree)
                return;
            if (!onlyClassLibrary)
                ProjectManager.RefreshProjectTree(ProjectTreePresentationMode, ProjectTreeVisibilityFilter, null, forceRebuildTree);
            if (_PLCProjectManager != null)
                _PLCProjectManager.RefreshProjectTree(CLProjectPresentationMode, CLProjectVisibilityFilter, null, forceRebuildTree);
        }

        public ACProjectManager ProjectManager
        {
            get
            {
                if (_ACProjectManager != null)
                    return _ACProjectManager;
                _ACProjectManager = new ACProjectManager(Database.ContextIPlus, Root);
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
        /// The _ show group
        /// </summary>
        bool _ShowGroup = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show group].
        /// </summary>
        /// <value><c>true</c> if [show group]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999, "TreeConfig", "en{'Grouped'}de{'Gruppiert'}")]
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
                RefreshProjectTree();
            }
        }

        /// <summary>
        /// The _ show process module
        /// </summary>
        bool _ShowProcessModule = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show process module].
        /// </summary>
        /// <value><c>true</c> if [show process module]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999, "TreeConfig", "en{'Process Module'}de{'Prozess Modul'}")]
        public bool ShowProcessModule
        {
            get
            {
                return _ShowProcessModule;
            }
            set
            {
                _ShowProcessModule = value;
                OnPropertyChanged("ShowProcessModule");
                RefreshProjectTree(true);
            }
        }

        /// <summary>
        /// The _ show process function
        /// </summary>
        bool _ShowProcessFunction = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show process function].
        /// </summary>
        /// <value><c>true</c> if [show process function]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999, "TreeConfig", "en{'Process Function'}de{'Prozess Funktion'}")]
        public bool ShowProcessFunction
        {
            get
            {
                return _ShowProcessFunction;
            }
            set
            {
                _ShowProcessFunction = value;
                OnPropertyChanged("ShowProcessFunction");
                RefreshProjectTree(true);
            }
        }

        /// <summary>
        /// The _ show module
        /// </summary>
        bool _ShowModule = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show module].
        /// </summary>
        /// <value><c>true</c> if [show module]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999, "TreeConfig", "en{'Module'}de{'Module'}")]
        public bool ShowModule
        {
            get
            {
                return _ShowModule;
            }
            set
            {
                _ShowModule = value;
                OnPropertyChanged("ShowModule");
                RefreshProjectTree(true);
            }
        }

        /// <summary>
        /// The _ show BG module
        /// </summary>
        bool _ShowBGModule = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show BG module].
        /// </summary>
        /// <value><c>true</c> if [show BG module]; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999, "TreeConfig", "en{'Background Module'}de{'Hintergr.Modul'}")]
        public bool ShowBGModule
        {
            get
            {
                return _ShowBGModule;
            }
            set
            {
                _ShowBGModule = value;
                OnPropertyChanged("ShowBGModule");
                RefreshProjectTree(true);
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
        [ACPropertyInfo(9999, "TreeConfig", "en{'With Caption'}de{'Mit Bezeichnung'}")]
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

        /// <summary>
        /// The _ show AC class
        /// </summary>
        ACClass _ShowACClass = null;
        /// <summary>
        /// Gets or sets the show AC class.
        /// </summary>
        /// <value>The show AC class.</value>
        [ACPropertyCurrent(9999, "ShowACClass")]
        public ACClass ShowACClass
        {
            get
            {
                return _ShowACClass;
            }
            set
            {
                _ShowACClass = value;
                OnPropertyChanged("ShowACClass");
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
        [ACPropertyInfo(9999, "TreeConfig", "en{'Search Class'}de{'Suche Klasse'}")]
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
        /// Gets the show AC class list.
        /// </summary>
        /// <value>The show AC class list.</value>
        [ACPropertyList(9999, "ShowACClass")]
        public IEnumerable<ACClass> ShowACClassList
        {
            get
            {
                return ProjectManager.ShowACClassList;
            }
        }
        #endregion

        #region ClassHierarchy
        /// <summary>
        /// The _ selected AC class hierarchy
        /// </summary>
        ACClass _SelectedACClassHierarchy;
        /// <summary>
        /// Gets or sets the selected AC class hierarchy.
        /// </summary>
        /// <value>The selected AC class hierarchy.</value>
        [ACPropertySelected(9999, "ACClassHierarchy")]
        public ACClass SelectedACClassHierarchy
        {
            get
            {
                return _SelectedACClassHierarchy;
            }
            set
            {
                _SelectedACClassHierarchy = value;
                OnPropertyChanged("SelectedACClassHierarchy");
            }
        }


        /// <summary>
        /// Gets the AC class hierarchy list.
        /// </summary>
        /// <value>The AC class hierarchy list.</value>
        [ACPropertyList(9999, "ACClassHierarchy")]
        public IEnumerable<ACClass> ACClassHierarchyList
        {
            get
            {
                if (CurrentACClass == null)
                    return null;
                return CurrentACClass.ClassHierarchyWithInterfaces;
            }
        }
        #endregion

        #region ProjectLibraryClass

        protected ACClassInfoWithItems.VisibilityFilters CLProjectVisibilityFilter
        {
            get
            {
                ACClassInfoWithItems.VisibilityFilters filter
                    = new ACClassInfoWithItems.VisibilityFilters()
                    {
                        SearchText = this.SearchCLProjectItem
                    };
                return filter;
            }
        }

        protected ACProjectManager.PresentationMode CLProjectPresentationMode
        {
            get
            {
                ACProjectManager.PresentationMode mode
                    = new ACProjectManager.PresentationMode()
                    {
                        ShowCaptionInTree = this.WithCaption,
                        ToolWindow = true
                    };
                return mode;
            }
        }

        protected ACProjectManager PLCProjectManager
        {
            get
            {
                if (_PLCProjectManager == null)
                {
                    _PLCProjectManager = new ACProjectManager(Database.ContextIPlus, Root);
                    _PLCProjectManager.PropertyChanged += _PLCProjectManager_PropertyChanged;
                    _PLCProjectManager.LoadACProject_ClassLibrary(CLProjectPresentationMode, CLProjectVisibilityFilter);
                }
                return _PLCProjectManager;
            }
        }

        private void _PLCProjectManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ACProjectManager.CurrentProjectItemRootPropName)
                OnPropertyChanged("CurrentCLProjectItemRoot");
        }


        /// <summary>
        /// Gets the current CL project item root.
        /// </summary>
        /// <value>The current CL project item root.</value>
        [ACPropertyCurrent(9999, "CLProjectItemRoot")]
        public IACObject CurrentCLProjectItemRoot
        {
            get
            {
                return PLCProjectManager.CurrentProjectItemRoot;
            }
        }

        /// <summary>
        /// The _ current CL project item
        /// </summary>
        ACClassInfoWithItems _CurrentCLProjectItem = null;
        /// <summary>
        /// Gets or sets the current CL project item.
        /// </summary>
        /// <value>The current CL project item.</value>
        [ACPropertyCurrent(9999, "CLProjectItem")]
        public ACClassInfoWithItems CurrentCLProjectItem
        {
            get
            {
                return _CurrentCLProjectItem;
            }
            set
            {
                if (_CurrentCLProjectItem != value)
                {
                    _CurrentCLProjectItem = value;
                    OnPropertyChanged("CurrentCLProjectItem");
                }
            }
        }

        /// <summary>
        /// The _ search class text
        /// </summary>
        string _SearchCLProjectItem = "";
        /// <summary>
        /// Gets or sets the search class text.
        /// </summary>
        /// <value>The search class text.</value>
        [ACPropertyInfo(9999, "CLProjectItem", "en{'Search Class'}de{'Suche Klasse'}")]
        public string SearchCLProjectItem
        {
            get
            {
                return _SearchCLProjectItem;
            }
            set
            {
                if (_SearchCLProjectItem != value)
                {
                    _SearchCLProjectItem = value;
                    OnPropertyChanged();
                    RefreshProjectTree(true, true);
                }
            }
        }
        #endregion

        #region ProjectLibraryAppDefinition

        protected ACClassInfoWithItems.VisibilityFilters ADProjectVisibilityFilter
        {
            get
            {
                ACClassInfoWithItems.VisibilityFilters filter
                    = new ACClassInfoWithItems.VisibilityFilters()
                    {
                        SearchText = this.SearchPLADProjectItem
                    };
                return filter;
            }
        }

        protected ACProjectManager.PresentationMode ADProjectPresentationMode
        {
            get
            {
                ACProjectManager.PresentationMode mode = new ACProjectManager.PresentationMode();
                return mode;
            }
        }


        protected ACProjectManager PLADProjectManager
        {
            get
            {
                if (_PLADProjectManager == null)
                {
                    _PLADProjectManager = new ACProjectManager(Database.ContextIPlus, Root);
                    _PLADProjectManager.PropertyChanged += _PLADProjectManager_PropertyChanged;
                }
                return _PLADProjectManager;
            }
        }

        private void _PLADProjectManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ACProjectManager.CurrentProjectItemRootPropName)
                OnPropertyChanged("CurrentPLADProjectItemRoot");
        }

        private void RefreshPLADProjectTree()
        {
            if (_PLADProjectManager != null)
                _PLADProjectManager.LoadACProject_AppDefinition(CurrentACProject, ADProjectPresentationMode, ADProjectVisibilityFilter);
        }

        /// <summary>
        /// Gets the current PLAD project item root.
        /// </summary>
        /// <value>The current PLAD project item root.</value>
        [ACPropertyCurrent(9999, "PLADProjectItemRoot")]
        public IACObject CurrentPLADProjectItemRoot
        {
            get
            {
                if (PLADProjectManager.CurrentACProject != this.CurrentACProject.ACProject1_BasedOnACProject)
                    RefreshPLADProjectTree();
                return PLADProjectManager.CurrentProjectItemRoot;
            }
        }

        /// <summary>
        /// The _ current PLAD project item
        /// </summary>
        ACClassInfoWithItems _CurrentPLADProjectItem = null;
        /// <summary>
        /// Gets or sets the current PLAD project item.
        /// </summary>
        /// <value>The current PLAD project item.</value>
        [ACPropertyCurrent(9999, "PLADProjectItem")]
        public ACClassInfoWithItems CurrentPLADProjectItem
        {
            get
            {
                return _CurrentPLADProjectItem;
            }
            set
            {
                if (_CurrentPLADProjectItem != value)
                {
                    _CurrentPLADProjectItem = value;
                    OnPropertyChanged("CurrentPLADProjectItem");
                }
            }
        }

        /// <summary>
        /// The _ search class text
        /// </summary>
        string _SearchPLADProjectItem = "";
        /// <summary>
        /// Gets or sets the search class text.
        /// </summary>
        /// <value>The search class text.</value>
        [ACPropertyInfo(9999, "CLProjectItem", "en{'Search Class'}de{'Suche Klasse'}")]
        public string SearchPLADProjectItem
        {
            get
            {
                return _SearchPLADProjectItem;
            }
            set
            {
                if (_SearchPLADProjectItem != value)
                {
                    _SearchPLADProjectItem = value;
                    OnPropertyChanged();
                    RefreshPLADProjectTree();
                }
            }
        }
        #endregion

        #region BSO->ACMethod
        #region 1. ACProject
        /// <summary>
        /// Saves this instance.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            if (Root.Environment.License.MayUserDevelop)
                OnSave();
            else
                Messages.Warning(this, "Warning50026");
        }

        IList<Guid> _acClassDesignIDList = null;
        IList<ACClass> _acClassList = null;
        List<ACProjectManager.MoveInfo> _moveInfoList = null;

        protected override Msg OnPreSave()
        {
            Msg result = base.OnPreSave();
            if (result != null)
                return result;

            ProjectManager.UpdateACClassTask(CurrentACProject);
            result = ProjectManager.UpdateMSMethodFunction(CurrentACProject);
            if (result != null)
            {
                return result;
                //Global.MsgResult userResult = Messages.Msg(msg, Global.MsgResult.Cancel, eMsgButton.OKCancel);
                //if (userResult == Global.MsgResult.Cancel)
                //    return
            }

            _acClassDesignIDList = Database.GetModifiedEntities<ACClassDesign>().Select(c => c.ACClassDesignID).ToList();
            _acClassList = Database.GetAddedEntities<ACClass>();
            foreach (var newACClass in _acClassList.ToArray())
            {
                if (newACClass.ACClass1_ParentACClass != null && newACClass.ACClass1_ParentACClass.EntityState == EntityState.Added)
                {
                    _acClassList.Remove(newACClass);
                }
            }

            _moveInfoList = new List<ACProjectManager.MoveInfo>();
            foreach (var stateEntry in Database.ContextIPlus.ChangeTracker.Entries<ACClass>().Where(c => c.Entity is ACClass).Select(c => c))
            {
                if (stateEntry.GetModifiedProperties().Where(c => c == "ParentACClassID").Any())
                {
                    Guid parentACClassID = (Guid)stateEntry.OriginalValues["ParentACClassID"];
                    ACProjectManager.MoveInfo moveInfo = new ACProjectManager.MoveInfo();
                    moveInfo.MovedClass = stateEntry.Entity as ACClass;
                    moveInfo.SourceParentClass = Database.ContextIPlus.ACClass.Where(c => c.ACClassID == parentACClassID).First();
                    moveInfo.TargetParentClass = moveInfo.MovedClass.ACClass1_ParentACClass;
                    _moveInfoList.Add(moveInfo);
                }
            }
            return null;
        }

        protected override void OnPostSave()
        {
            if (ProjectManager.IsEnabledUpdateApplicationbyAppDefinition(CurrentACProject, _acClassList, _moveInfoList))
            {
                if (Messages.Question(this, "Question00009", Global.MsgResult.Yes, false, CurrentACProject.ACProjectName) == Global.MsgResult.Yes)
                {
                    string info = ProjectManager.UpdateApplicationbyAppDefinition(CurrentACProject, _acClassList, _moveInfoList);
                    if (!string.IsNullOrEmpty(info))
                    {
                        Messages.Warning(this, "Warning00001", false, info);
                    }
                }
            }


            using (ACMonitor.Lock(Root.Database.ContextIPlus.QueryLock_1X000))
            {
                foreach (var acClassDesignID in _acClassDesignIDList)
                {
                    var acClassDesign1 = Root.Database.ContextIPlus.ACClassDesign.Where(c => c.ACClassDesignID == acClassDesignID).First();
                    Root.Database.ContextIPlus.Refresh(System.Data.Objects.RefreshMode.StoreWins, acClassDesign1);
                }
            }

            // Damir to Ivan ???
            //if (CurrentACClassDesign != null)
            //    CurrentACClassDesign.OnEntityPropertyChanged("IsDesignCompiled");

            base.OnPostSave();
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
        [ACMethodCommand("ACProject", "en{'undo'}de{'Nicht speichern'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
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
        [ACMethodInteraction("ACProject", "en{'Load'}de{'Laden'}", (short)MISort.Load, false, "SelectedACProject", Global.ACKinds.MSMethodPrePost)]
        public void Load(bool requery = false)
        {
            if (SelectedACProject != null && ACSaveOrUndoChanges())
            {
                if (!PreExecute("Load"))
                    return;
                CurrentACProject = SelectedACProject;
                ACState = Const.SMEdit;
                PostExecute("Load");
            }
        }

        /// <summary>
        /// Determines whether [is enabled load].
        /// </summary>
        /// <returns><c>true</c> if [is enabled load]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledLoad()
        {
            return SelectedACProject != null;
        }

        /// <summary>
        /// News this instance.
        /// </summary>
        [ACMethodInteraction("ACProject", "en{'New'}de{'Neu'}", (short)MISort.New, true, "SelectedACProject", Global.ACKinds.MSMethodPrePost)]
        public void New()
        {
            if (!PreExecute("New")) return;
            _NewACProjectAppDefinitionList = null;
            CurrentNewACProject = ProjectManager.NewACProject();

            ShowDialog(this, "ACProjectNew");

            PostExecute("New");
        }

        /// <summary>
        /// Determines whether [is enabled new].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNew()
        {
            return !Database.ContextIPlus.IsChanged;
        }

        /// <summary>
        /// News the AC project OK.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void NewACProjectOK()
        {
            CloseTopDialog();
            if (ProjectManager.InitNewACProject(CurrentNewACProject))
            {
                ACSaveChanges();
                Search();
                CurrentACProject = CurrentNewACProject;
            }
            else
            {
                CurrentNewACProject = null;
            }
        }

        /// <summary>
        /// Determines whether [is enabled new AC project OK].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new AC project OK]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACProjectOK()
        {
            if (CurrentNewACProject == null || string.IsNullOrEmpty(CurrentNewACProject.ACProjectName))
                return false;
            return true;
        }

        /// <summary>
        /// News the AC project cancel.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void NewACProjectCancel()
        {
            CloseTopDialog();
            CurrentNewACProject = null;
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        [ACMethodInteraction("ACProject", "en{'Delete'}de{'Löschen'}", (short)MISort.Delete, true, "CurrentACProject", Global.ACKinds.MSMethodPrePost)]
        public void Delete()
        {
            if (Messages.Question(this, "Question00002", Global.MsgResult.Yes, false, CurrentACProject.ACProjectName) == Global.MsgResult.Yes)
            {
                if (!PreExecute("Delete")) return;
                Msg msg = CurrentACProject.DeleteACObject(Database.ContextIPlus, true);
                if (msg != null)
                {
                    Messages.Msg(msg);
                    return;
                }
                ACSaveChanges();
                PostExecute("Delete");
                Search();
                SelectedACProject = ACProjectList.First();
                Load();
            }
        }

        /// <summary>
        /// Determines whether [is enabled delete].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDelete()
        {
            return CurrentACProject != null && ProjectManager.IsEnabledDeleteACProject(CurrentACProject);
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            AccessPrimary.NavSearch(Database.ContextIPlus);
            OnPropertyChanged("ACProjectList");
        }

        /// <summary>
        /// Shows the project property.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'Project'}de{'Projekt'}", 9999, false, Global.ACKinds.MSMethodPrePost)]
        public void ShowProjectProperty()
        {
            if (!PreExecute("ShowProjectProperty")) return;
            ShowDialog(this, "ACProjectProperty");

            PostExecute("ShowProjectProperty");
        }

        /// <summary>
        /// Determines whether [is enabled show project property].
        /// </summary>
        /// <returns><c>true</c> if [is enabled show project property]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledShowProjectProperty()
        {
            return CurrentACProject != null;
        }
        #endregion

        #region 1.1 ACClass

        /// <summary>
        /// News the AC class.
        /// </summary>
        [ACMethodInteraction("ACClass", "en{'New Class'}de{'Neue Klasse'}", (short)MISort.New, true, "CurrentProjectItem", Global.ACKinds.MSMethodPrePost)]
        public void NewACClass()
        {
            if (IsEnabledNewACClass())
            {
                if (!PreExecute("NewACClass")) return;
                _NewSubclass = false;
                CurrentNewACClass = ProjectManager.NewACClass(CurrentACProject, CurrentProjectItem);
                ShowDialog(this, "ACClassNew");

                PostExecute("NewACClass");
            }
        }

        /// <summary>
        /// Determines whether [is enabled new AC class].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new AC class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClass()
        {
            return ProjectManager.IsEnabledNewACClass(CurrentACProject, CurrentProjectItem);
        }

        /// <summary>
        /// News the child AC class.
        /// </summary>
        [ACMethodInteraction("ACClass", "en{'New Subclass'}de{'Neue untergeordnete Klasse'}", (short)MISort.New, true, "CurrentProjectItem", Global.ACKinds.MSMethodPrePost)]
        public void NewChildACClass()
        {
            if (IsEnabledNewChildACClass())
            {
                if (!PreExecute("NewChildACClass")) return;
                _NewSubclass = true;
                CurrentNewACClass = ProjectManager.NewChildACClass(CurrentACProject, CurrentProjectItem);
                ShowDialog(this, "ACClassNew");
                PostExecute("NewChildACClass");
            }
        }

        /// <summary>
        /// Determines whether [is enabled new child AC class].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new child AC class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewChildACClass()
        {
            return ProjectManager.IsEnabledNewChildACClass(CurrentACProject, CurrentProjectItem);
        }


        [ACMethodInteraction("ACClass", "en{'Switch baseclass'}de{'Basis-Klasse austauschen'}", (short)MISort.New, true, "CurrentProjectItem", Global.ACKinds.MSMethodPrePost)]
        public void SwitchACClass()
        {
            if (IsEnabledSwitchACClass())
            {
                if (!PreExecute("SwitchACClass"))
                    return;
                ACClassToSwitch = null;
                ShowDialog(this, "SwitchACClass");
                PostExecute("SwitchACClass");
            }
        }

        /// <summary>
        /// Determines whether [is enabled new child AC class].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new child AC class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledSwitchACClass()
        {
            return ProjectManager.IsEnabledNewChildACClass(CurrentACProject, CurrentProjectItem);
        }


        /// <summary>
        /// News the AC class OK.
        /// </summary>
        [ACMethodCommand("NewACClass", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void NewACClassOK()
        {
            CloseTopDialog();

            ProjectManager.InitACClass(CurrentNewACClass);
            ACClassInfoWithItems parentItem = CurrentProjectItem;
            if (!_NewSubclass && CurrentProjectItem.ParentACObject != null && CurrentProjectItem.ParentACObject is ACClassInfoWithItems)
                parentItem = CurrentProjectItem.ParentACObject as ACClassInfoWithItems;
            CurrentProjectItem = ProjectManager.InsertProjectItem(CurrentNewACClass, parentItem);
            CurrentProjectItemRootChangeInfo = new ChangeInfo(parentItem, CurrentProjectItem, Const.CmdAddChildData);
            OnPropertyChanged("CurrentProjectItemRoot");
        }

        /// <summary>
        /// Determines whether [is enabled new AC class OK].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new AC class OK]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClassOK()
        {
            if (CurrentNewACClass == null || string.IsNullOrEmpty(CurrentNewACClass.ACIdentifier) || CurrentNewACClass.ACClass1_BasedOnACClass == null)
                return false;
            return true;
        }

        /// <summary>
        /// News the AC class cancel.
        /// </summary>
        [ACMethodCommand("NewACClass", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void NewACClassCancel()
        {
            CloseTopDialog();
            CurrentNewACClass = null;
        }

        [ACMethodCommand("SwitchACClass", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void SwitchACClassOK()
        {
            CloseTopDialog();
            if (ACClassToSwitch != null)
            {
                if (CurrentProjectItem.ValueT.ObjectType != null 
                    && !CurrentProjectItem.ValueT.ObjectType.IsAssignableFrom(ACClassToSwitch.ObjectType))
                {
                    if (IsACClassReferenced(CurrentProjectItem.ValueT))
                    {
                        Messages.Warning(this, "Can't switch class because there are already references in the derived instances", false, null);
                        return;
                    }
                }
                CurrentProjectItem.ValueT.ACClass1_BasedOnACClass = ACClassToSwitch;
                CurrentProjectItem.ValueT.ACStorableType = ACClassToSwitch.ACStorableType;
                CurrentProjectItem.ValueT.ACKind = ACClassToSwitch.ACKind;
                CurrentProjectItem.ValueT.ACPackage = ACClassToSwitch.ACPackage;
                CurrentProjectItem.ValueT.IsRightmanagement = ACClassToSwitch.IsRightmanagement;
            }
            OnPropertyChanged("CurrentProjectItem");
            OnPropertyChanged("CurrentACClass");
            OnPropertyChanged("CurrentProjectItemRoot");
        }

        /// <summary>
        /// Determines whether [is enabled new AC class OK].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new AC class OK]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledSwitchACClassOK()
        {
            if (ACClassToSwitch == null || CurrentProjectItem.ValueT == null)
                return false;
            //if (CurrentProjectItem.ValueT.ObjectType.IsAssignableFrom(ACClassToSwitch.ObjectType))
            //    return true;
            return true;
        }

        public bool IsACClassReferenced(ACClass classToCheck)
        {
            if (classToCheck == null)
                return false;
            if (classToCheck.ACClassPropertyRelation_SourceACClass.Any() || classToCheck.ACClassPropertyRelation_TargetACClass.Any())
                return true;
            if (classToCheck.ACClassProperty_ACClass.Where(c => c.ACClassProperty1_ParentACClassProperty != null).Any())
                return true;
            if (classToCheck.ACClassMethod_ACClass.Any())
                return true;
            foreach (var derivation in classToCheck.ACClass_BasedOnACClass)
            {
                if (IsACClassReferenced(derivation))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// News the AC class cancel.
        /// </summary>
        [ACMethodCommand("SwitchACClass", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void SwitchACClassCancel()
        {
            CloseTopDialog();
            ACClassToSwitch = null;
        }

        #region 1.1 ACClass -> Delete

        /// <summary>
        /// Deletes the AC class.
        /// </summary>
        [ACMethodInteraction("ACClass", "en{'Delete Class'}de{'Klasse löschen'}", (short)MISort.Delete, true, "CurrentProjectItem", Global.ACKinds.MSMethodPrePost)]
        public void DeleteACClass()
        {
            if (CurrentACClass == null)
                return;
            if (CurrentACClass.CountDesignsRecursive() > 0)
            {
                if (Global.MsgResult.Yes != Messages.Question(this, "Question00003", Global.MsgResult.Yes, false, CurrentACClass.CountDesignsRecursive().ToString()))
                {
                    return;
                }
            }

            if (!IsEnabledDeleteACClass())
                return;
            if (!PreExecute("DeleteACClass")) return;
            ACClassInfoWithItems projectItem = CurrentProjectItem;

            Msg msg = CurrentProjectItem.ValueT.DeleteACClassRecursive(Database.ContextIPlus, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            PostExecute("DeleteACClass");
            CurrentProjectItemRootChangeInfo = new ChangeInfo(null, projectItem, Const.CmdDeleteData);
            CurrentACClass = CurrentACProject.ACClass_ACProject.Where(c => c.ParentACClassID == null).FirstOrDefault();
            OnPropertyChanged("CurrentACClass");
        }

        /// <summary>
        /// Determines whether [is enabled delete AC class].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete AC class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACClass()
        {
            if (CurrentACProject == null || CurrentProjectItem == null || CurrentProjectItem.ValueT == null /*|| !string.IsNullOrEmpty(CurrentProjectItem.ValueT.AssemblyQualifiedName)*/)
                return false;
            return true;
        }

        #endregion

        #region 1.1 ACClass -> Clone
        private CloneDialogModel _CloneData;
        [ACPropertyInfo(999)]
        public CloneDialogModel CloneData
        {
            get
            {
                if (_CloneData == null)
                    _CloneData = new CloneDialogModel();
                return _CloneData;
            }
            set
            {
                _CloneData = value;
                OnPropertyChanged("CloneData");
            }
        }


        /// <summary>
        /// Deletes the AC class.
        /// </summary>
        [ACMethodInteraction("ACClass", "en{'Clone Class'}de{'Klasse klonen'}", (short)MISort.New, true, "CurrentProjectItem", Global.ACKinds.MSMethodPrePost)]
        public void CloneACClass()
        {
            if (!IsEnabledCloneACClass())
                return;
            CloneData.ACIdentifier = ProjectManager.CloneGetACIdentifier(CurrentProjectItem.ValueT);
            ShowDialog(this, "CloneDialog");
        }

        /// <summary>
        /// Determines whether [is enabled delete AC class].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete AC class]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledCloneACClass()
        {
            if (CurrentACProject == null || CurrentProjectItem == null || CurrentProjectItem.ValueT == null || CurrentProjectItem.ParentACObject == null)
                return false;
            return true;
        }

        [ACMethodInfo("Clone", "en{'Ok'}de{'Ok'}", 9999, false)]
        public void CloneDialogOK()
        {
            if (!IsEnabledCloneDialogOK())
                return;
            CloseTopDialog();
            try
            {
                ProjectManager.CloneClassTree(CurrentProjectItem.ValueT, Root.CurrentInvokingUser.VBUserNo, CloneData);
            }
            catch (Exception e)
            {
                string message = e.Message;
                if (e.InnerException != null)
                    message += " " + e.InnerException.Message;
                Messages.Msg(new Msg(message, this, eMsgLevel.Exception, "BSOiPlusStudio", "CloneDialogOK", 1000));
            }
            var project = ProjectManager.LoadACProject(SelectedACProject, ProjectTreePresentationMode, ProjectTreeVisibilityFilter);
            SetCurrentACProject(project);
        }

        public bool IsEnabledCloneDialogOK()
        {
            return IsEnabledCloneACClass()
                &&
                !((ACClassInfoWithItems)CurrentProjectItem.ParentACObject).Items.Any(p => p.ACIdentifier == CloneData.ACIdentifier);
        }

        [ACMethodInfo("Clone", "en{'Cancel'}de{'Schließen'}", 9999, false)]
        public void CloneDialogCancel()
        {
            CloseTopDialog();
        }



        #endregion

        #region Start and Stop
        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <param name="vbContent">Content of the vb.</param>
        /// <param name="value">The value.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>Msg.</returns>
        [ACMethodInfo("ACClass", "en{'Validate input'}de{'Überprüfe Eingabe'}", 9999, false)]
        public Msg ValidateInput(string vbContent, object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new Msg() { MessageLevel = eMsgLevel.Info };
            }
            else
            {
                switch (vbContent)
                {
                    case "CurrentACClass\\ACIdentifier":
                    case "CurrentACClassProperty\\ACIdentifier":
                    case "CurrentACClassDesign\\ACIdentifier":
                    case "CurrentACClassMethod\\ACIdentifier":
                    case "CurrentNewACClassDesign\\ACIdentifier":
                    case "CurrentNewACClass\\ACIdentifier":
                    case "CurrentNewACIdentifier":
                        {
                            String strValue = value as String;
                            if (String.IsNullOrEmpty(strValue))
                                return new Msg() { MessageLevel = eMsgLevel.Info };
                            if (strValue.ContainsACUrlDelimiters() || strValue.Contains(" "))
                            {
                                Msg msg = new Msg { ACIdentifier = this.ACCaption, Message = Root.Environment.TranslateMessage(this, "Warning00002"), MessageLevel = eMsgLevel.Error };
                                return msg;
                            }
                            break;
                        }
                    case "IsDefaultACClassDesign":
                        {
                            MsgWithDetails msgWithDetails = new MsgWithDetails();
                            ProjectManager.GetControlModeIsDefaultACClassDesign(CurrentACClass, CurrentACClassDesign, msgWithDetails);
                            if (msgWithDetails.MsgDetails != null && msgWithDetails.MsgDetails.Any())
                            {
                                if (msgWithDetails.MessageLevel == eMsgLevel.Default)
                                    msgWithDetails.MessageLevel = msgWithDetails.MsgDetails.FirstOrDefault().MessageLevel;
                                return msgWithDetails;
                            }
                            break;
                        }
                }
            }
            return new Msg() { MessageLevel = eMsgLevel.Info };
        }

        /// <summary>
        /// Starts the BSO.
        /// </summary>
        [ACMethodInteraction("ACClass", "en{'Start Application'}de{'Anwendung starten'}", 9999, true, "CurrentACClass")]
        public void StartBSO()
        {
            this.Root.RootPageWPF.ACUrlCommand(Const.CmdStartBusinessobject, Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + CurrentACClass.ACIdentifier);
        }

        /// <summary>
        /// Determines whether [is enabled start BSO].
        /// </summary>
        /// <returns><c>true</c> if [is enabled start BSO]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledStartBSO()
        {
            if (CurrentACClass == null)
                return false;
            if (!(CurrentACClass.ParentACObject is ACClass))
                return false;
            if (((ACClass)CurrentACClass.ParentACObject).ACKind != Global.ACKinds.TACBusinessobjects)
                return false;

            if (CurrentACClass != null
                && CurrentACClass.Designs.Where(c => c.ACKindIndex == (short)Global.ACKinds.DSDesignLayout && c.ACUsageIndex == (int)Global.ACUsages.DUMain).Any()
                && !IsEnabledSave())
                return true;
            return false;
        }

        [ACMethodInteraction("ACClass", "en{'Start instance'}de{'Starte Instanz'}", (short)MISort.Start, true, "CurrentProjectItem")]
        public void StartInstance()
        {
            if (CurrentProjectItem == null)
                return;

            ACClassInfoWithItems parentItem = CurrentProjectItem.ParentContainer as ACClassInfoWithItems;
            if (parentItem == null)
                return;
            ACClass parentClass = parentItem.ValueT as ACClass;
            if (parentClass == null)
                return;
            string acUrlComponent = parentClass.GetACUrlComponent();
            if (String.IsNullOrEmpty(acUrlComponent))
                return;
            ACComponent currentInstance = ACUrlCommand("?" + acUrlComponent) as ACComponent;
            if (currentInstance != null)
            {
                ACComponentProxy proxy = currentInstance as ACComponentProxy;
                if (proxy != null)
                {
                    proxy.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + nameof(ACComponent.StartComponent), CurrentProjectItem.ACIdentifier, null, new object[] { }, ACStartCompOptions.Default);
                    ACClass runtimeType = gip.core.datamodel.Database.GlobalDatabase.GetACType(CurrentProjectItem.ValueT.ACClassID);
                    if (runtimeType != null)
                        proxy.StartComponent(CurrentProjectItem.ACIdentifier, null, null);
                }
                else
                    currentInstance.StartComponent(CurrentProjectItem.ACIdentifier, null, null);
            }
        }

        public bool IsEnabledStartInstance()
        {
            var currentInstance = CurrentACComponent;
            if (currentInstance != null)
            {
                ACComponentProxy proxy = currentInstance as ACComponentProxy;
                if (proxy != null)
                {
                    if (proxy.ConnectionState >= ACObjectConnectionState.Connected)
                        return false;
                }
                else
                {
                    if (currentInstance.InitState == ACInitState.Initialized)
                        return false;
                }
            }
            return true;
        }

        [ACMethodInteraction("ACClass", "en{'Stop instance'}de{'Stoppe Instanz'}", (short)MISort.Start, true, "CurrentProjectItem")]
        public void StopInstance()
        {
            var currentInstance = CurrentACComponent;
            if (currentInstance == null)
                return;
            ACComponentProxy proxy = currentInstance as ACComponentProxy;
            if (proxy != null)
            {
                if (proxy.ConnectionState >= ACObjectConnectionState.Connected)
                    proxy.InvokeACUrlCommand(proxy.GetACUrl() + ACUrlHelper.Delimiter_InvokeMethod + nameof(Stop), null);
            }
            else
            {
                if (currentInstance.InitState == ACInitState.Initialized)
                    currentInstance.Stop();
            }
        }

        public bool IsEnabledStopInstance()
        {
            var currentInstance = CurrentACComponent;
            if (currentInstance == null)
                return false;
            return true;
        }

        [ACMethodInteraction("ACClass", "en{'Reload instance'}de{'Instanz neu laden'}", (short)MISort.Start, true, "CurrentProjectItem")]
        public void ReloadInstance()
        {
            var currentInstance = CurrentACComponent;
            if (currentInstance == null)
                return;
            ACComponentProxy proxy = currentInstance as ACComponentProxy;
            if (proxy != null)
            {
                if (proxy.ConnectionState >= ACObjectConnectionState.Connected)
                {
                    proxy.InvokeACUrlCommand(proxy.GetACUrl() + ACUrlHelper.Delimiter_InvokeMethod + nameof(Reload), null);
                    proxy.Reload();
                }
            }
            else
            {
                if (currentInstance.InitState == ACInitState.Initialized)
                    currentInstance.Reload();
            }
        }

        public bool IsEnabledReloadInstance()
        {
            var currentInstance = CurrentACComponent;
            if (currentInstance == null)
                return false;
            return true;
        }
        #endregion

        #region DragAndDrop
        /// <summary>
        /// Drops the AC class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="targetVBDataObject">The target VB data object.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        [ACMethodInfo("ACClass", "en{'Drop Class'}de{'Drop Klasse'}", 9999, true, Global.ACKinds.MSMethodPrePost)]
        public void DropACClass(Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            if (!PreExecute("DropACClass")) return;

            ACClassInfoWithItems newItem = null;
            switch (action)
            {
                case Global.ElementActionType.Drop:
                    {
                        newItem = ProjectManager.DropACClass(CurrentACProject, action, dropObject, targetVBDataObject, x, y);
                        if (newItem != null)
                        {
                            CurrentProjectItem = newItem;
                            IACObject targetACObject = targetVBDataObject.GetFirstInnerValue() as IACObject;
                            CurrentProjectItemRootChangeInfo = new ChangeInfo(targetACObject, newItem, Const.CmdAddChildData);
                            OnPropertyChanged("CurrentProjectItemRoot");
                            PostExecute("DropACClass");
                        }
                    }
                    break;
                case Global.ElementActionType.Move:
                    {
                        newItem = ProjectManager.DropACClass(CurrentACProject, action, dropObject, targetVBDataObject, x, y);
                        if (newItem != null)
                        {
                            CurrentProjectItem = newItem;
                            IACObject targetACObject = targetVBDataObject.GetFirstInnerValue() as IACObject;
                            CurrentProjectItemRootChangeInfo = new ChangeInfo(targetACObject, newItem, Const.CmdMoveData);
                            OnPropertyChanged("CurrentProjectItemRoot");
                            PostExecute("DropACClass");
                        }
                    }

                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// Determines whether [is enabled drop AC class] [the specified action].
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="targetVBDataObject">The target VB data object.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if [is enabled drop AC class] [the specified action]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDropACClass(Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            return ProjectManager.IsEnabledDropACClass(CurrentACProject, action, dropObject, targetVBDataObject, x, y);
        }
        #endregion

        #endregion

        /// <summary>
        /// Modifies the AC class.
        /// </summary>
        [ACMethodInteraction("ACClass", "en{'Show in iPlus Studio'}de{'Show in iPlus Studio'}", (short)MISort.IPlusStudio, false, "SelectedACClassHierarchy")]
        public void ModifyACClass()
        {
            ACMethod acMethod = gip.core.datamodel.Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio);
            acMethod.ParameterValueList["AutoLoad"] = SelectedACClassHierarchy.GetACUrl();
            //ACUrlCommand(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio, acMethod.ParameterValueList);

            this.Root.RootPageWPF.ACUrlCommand(Const.CmdStartBusinessobject, Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio, acMethod.ParameterValueList);

        }

        #endregion

        #region ProjectLibrary
        /// <summary>
        /// Shows the class library.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'Library'}de{'Bibliothek'}", 9999, false)]
        public void ShowClassLibrary()
        {
            IACObject window = FindGui("", "", "*ProjectLibrary", this.ACIdentifier);
            if (window != null)
                return;

            ShowWindow(this, "ProjectLibrary", false,
                Global.VBDesignContainer.DockableWindow,
                Global.VBDesignDockState.Docked,
                Global.VBDesignDockPosition.Left,
                Global.ControlModes.Hidden,
                "TabMain");
        }

        /// <summary>
        /// Determines whether [is enabled show class library].
        /// </summary>
        /// <returns><c>true</c> if [is enabled show class library]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledShowClassLibrary()
        {
            return CurrentACProject != null && CurrentACProject.ACProjectType != Global.ACProjectTypes.Root;
        }
        #endregion

        #endregion

        #region Layoutsteuerung
        #region Layout Project
        /// <summary>
        /// Gets the current project library layout.
        /// </summary>
        /// <value>The current project library layout.</value>
        public string CurrentProjectLibraryLayout
        {
            get
            {
                string layoutXAML = LayoutHelper.DockingManagerBegin("tabControlLibrary", "Margin=\"5\"");
                switch (CurrentACProject.ACProjectType)
                {
                    case Global.ACProjectTypes.Application:
                    case Global.ACProjectTypes.Service:
                        if (CurrentACProject.ACProject1_BasedOnACProject != null)
                        {
                            layoutXAML += LayoutHelper.DockingManagerAdd("*ProjectLibraryAppDefinition", "ProjectLibraryAppDefinition_0");
                        }
                        layoutXAML += LayoutHelper.DockingManagerAdd("*ProjectLibraryClass", "ProjectLibraryClass_0");
                        break;
                    case Global.ACProjectTypes.AppDefinition:
                    case Global.ACProjectTypes.ClassLibrary:
                        layoutXAML += LayoutHelper.DockingManagerAdd("*ProjectLibraryClass", "ProjectLibraryClass_0");
                        break;
                }

                layoutXAML += LayoutHelper.DockingManagerEnd();
                return layoutXAML;
            }
        }
        #endregion

        #region Layout Maintabpages
        /// <summary>
        /// The _ current layouts
        /// </summary>
        List<string> _CurrentLayouts = null;
        /// <summary>
        /// Updates the current class design.
        /// </summary>
        public void UpdateCurrentClassDesign()
        {
            if (CurrentACClass == null)
                return;
            List<string> nextLayouts = new List<string>();
            switch (CurrentACClass.ACKind)
            {
                case Global.ACKinds.TACDBA:
                    nextLayouts.Add("TabOverview");
                    nextLayouts.Add("TabProperty");
                    nextLayouts.Add("TabDesign");
                    nextLayouts.Add("TabConfig");
                    break;
                case Global.ACKinds.TACUndefined:
                    break;
                default:
                    nextLayouts.Add("TabOverview");
                    if (AllowMethod())
                        nextLayouts.Add("TabMethod");
                    if (AllowProperty())
                        nextLayouts.Add("TabProperty");
                    nextLayouts.Add("TabDesign");
                    nextLayouts.Add("TabConfig");
                    break;
            }
            nextLayouts.Add("TabTranslation");
            bool update = _CurrentLayouts == null;
            if (!update)
            {
                if (nextLayouts.Count != _CurrentLayouts.Count())
                {
                    update = true;
                }
                else
                {
                    for (int i = 0; i < nextLayouts.Count(); i++)
                    {
                        if (nextLayouts[i] != _CurrentLayouts[i])
                        {
                            update = true;
                            break;
                        }
                    }
                }
            }

            if (update)
            {
                _CurrentLayouts = nextLayouts;
                OnPropertyChanged("CurrentClassDesign");

                OnPropertyChanged("IsTabOverviewVisible");
                OnPropertyChanged("IsTabPropertyVisible");
                OnPropertyChanged("IsTabMethodVisible");
                OnPropertyChanged("IsTabDesignVisible");
                OnPropertyChanged("IsTabConfigVisible");
                OnPropertyChanged("IsTabTranslationVisible");

            }
        }

        [ACPropertyInfo(9999)]
        public bool IsTabOverviewVisible
        {
            get
            {
                if (CurrentACClass == null)
                    return false;
                return CurrentACClass.ACKind != Global.ACKinds.TACUndefined;
            }
        }

        [ACPropertyInfo(9999)]
        public bool IsTabPropertyVisible
        {
            get
            {
                if (CurrentACClass == null)
                    return false;
                return CurrentACClass.ACKind != Global.ACKinds.TACUndefined
                       && AllowProperty();
            }
        }


        [ACPropertyInfo(9999)]
        public bool IsTabMethodVisible
        {
            get
            {
                if (CurrentACClass == null)
                    return false;
                return CurrentACClass.ACKind != Global.ACKinds.TACDBA
                       && CurrentACClass.ACKind != Global.ACKinds.TACUndefined
                       && AllowMethod();
            }
        }


        [ACPropertyInfo(9999)]
        public bool IsTabDesignVisible
        {
            get
            {
                if (CurrentACClass == null)
                    return false;
                return CurrentACClass.ACKind != Global.ACKinds.TACUndefined;
            }
        }


        [ACPropertyInfo(9999)]
        public bool IsTabConfigVisible
        {
            get
            {
                return IsTabDesignVisible;
            }
        }

        [ACPropertyInfo(9999)]
        public bool IsTabTranslationVisible
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Allows the property.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool AllowProperty()
        {
            if (CurrentACClass == null)
                return false;
            switch (CurrentACClass.ACKind)
            {
                case Global.ACKinds.TACClass: // 0020
                case Global.ACKinds.TACCommand: // 0021
                case Global.ACKinds.TACVBControl: // 0040
                case Global.ACKinds.TACSimpleClass: // 0060
                case Global.ACKinds.TACInterface: // 0070
                case Global.ACKinds.TACAbstractClass: // 0090
                case Global.ACKinds.TACDAClass: // 0100
                case Global.ACKinds.TACApplicationManager: // 5100
                case Global.ACKinds.TPAModule: // 5200
                case Global.ACKinds.TPAProcessModule: // 5210
                case Global.ACKinds.TPAProcessFunction: // 5310
                case Global.ACKinds.TPABGModule: // 5410
                case Global.ACKinds.TPARole: // 5510
                case Global.ACKinds.TACRoot: // 0300
                case Global.ACKinds.TACBusinessobjects: // 0400
                case Global.ACKinds.TACBSO: // 0401
                case Global.ACKinds.TACBSOGlobal: // 0402
                case Global.ACKinds.TACBSOReport: // 0403
                case Global.ACKinds.TACQueries: // 0600
                case Global.ACKinds.TACQRY: // 0601
                case Global.ACKinds.TACDBAManager: // 0900
                case Global.ACKinds.TACDBA: // 0910
                case Global.ACKinds.TACCommunications: // 1100
                case Global.ACKinds.TACWCFServiceManager: // 1110
                case Global.ACKinds.TACWCFServiceChannel: // 1111
                case Global.ACKinds.TACWCFClientManager: // 1120
                case Global.ACKinds.TACWCFClientChannel: // 1121
                case Global.ACKinds.TACMessages: // 1200
                case Global.ACKinds.TACEnvironment: // 1300
                case Global.ACKinds.TACRuntimeDump: // 1301
                case Global.ACKinds.TPWGroup: // 6200
                case Global.ACKinds.TPWMethod: // 6210
                case Global.ACKinds.TPWNode: // 6300
                case Global.ACKinds.TPWNodeStatic: // 6310
                case Global.ACKinds.TPWNodeMethod: // 6320
                case Global.ACKinds.TPWNodeWorkflow: // 6330
                case Global.ACKinds.TPWNodeStart: // 6380
                case Global.ACKinds.TPWNodeEnd: // 6390
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Allows the method.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool AllowMethod()
        {
            if (CurrentACClass == null)
                return false;
            switch (CurrentACClass.ACKind)
            {
                case Global.ACKinds.TACClass: // 0020
                case Global.ACKinds.TACCommand: // 0021
                case Global.ACKinds.TACUIControl: // 0022
                case Global.ACKinds.TACVBControl: // 0040
                case Global.ACKinds.TACInterface: // 0070
                case Global.ACKinds.TACAbstractClass: // 0090
                case Global.ACKinds.TACDAClass: // 0100
                case Global.ACKinds.TACApplicationManager: // 0200
                case Global.ACKinds.TPAModule: // 5200
                case Global.ACKinds.TPAProcessModule: // 5210
                case Global.ACKinds.TPAProcessFunction: // 5310
                case Global.ACKinds.TPABGModule: // 5410
                case Global.ACKinds.TPARole: // 5510
                case Global.ACKinds.TACRoot: // 0300
                case Global.ACKinds.TACBusinessobjects: // 0400
                case Global.ACKinds.TACBSO: // 0401
                case Global.ACKinds.TACBSOGlobal: // 0402
                case Global.ACKinds.TACBSOReport: // 0403
                case Global.ACKinds.TACQueries: // 0600
                case Global.ACKinds.TACQRY: // 0601
                case Global.ACKinds.TACDBAManager: // 0900
                case Global.ACKinds.TACCommunications: // 1100
                case Global.ACKinds.TACWCFServiceManager: // 1110
                case Global.ACKinds.TACWCFServiceChannel: // 1111
                case Global.ACKinds.TACWCFClientManager: // 1120
                case Global.ACKinds.TACWCFClientChannel: // 1121
                case Global.ACKinds.TACMessages: // 1200
                case Global.ACKinds.TACEnvironment: // 1300
                case Global.ACKinds.TACRuntimeDump: // 1301
                case Global.ACKinds.TPWGroup: // 6200
                case Global.ACKinds.TPWMethod: // 6210
                case Global.ACKinds.TPWNode: // 6300
                case Global.ACKinds.TPWNodeStatic: // 6310
                case Global.ACKinds.TPWNodeMethod: // 6320
                case Global.ACKinds.TPWNodeWorkflow: // 6330
                case Global.ACKinds.TPWNodeStart: // 6380
                case Global.ACKinds.TPWNodeEnd: // 6390
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the current class design.
        /// </summary>
        /// <value>The current class design.</value>
        public string CurrentClassDesign
        {
            get
            {
                if (CurrentACClass == null || CurrentACProject == null)
                    return null;

                string layoutXAML = LayoutHelper.DockingManagerBegin("tabControl1", "Margin=\"1,3,1,0\"");

                foreach (var acIdentifier in _CurrentLayouts)
                {
                    layoutXAML += LayoutHelper.DockingManagerAdd("*" + acIdentifier, acIdentifier + "_0");
                }
                layoutXAML += LayoutHelper.DockingManagerEnd();

                return layoutXAML;
            }
        }
        #endregion
        #endregion

        #region Layout und Propertychanged
        /// <summary>
        /// Handles the PropertyChanged event of the CurrentACClass control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        void CurrentACClass_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ACClass1_BasedOnACClass":
                    if (CurrentACClass.ACClass1_BasedOnACClass != null)
                    {
                        CurrentACClass.ACKind = CurrentACClass.ACClass1_BasedOnACClass.ACKind;
                    }
                    break;
                case "BasedOnACClassID":
                    if (CurrentACClass.ACClass1_BasedOnACClass != null)
                    {
                        if (CurrentACClass.ACClass1_BasedOnACClass.ACKind == Global.ACKinds.TPAProcessFunction)
                            CurrentACClass.ACIdentifier = CurrentACClass.ACClass1_BasedOnACClass.ACIdentifier;

                        OnPropertyChanged("VisualACClassDesignList");
                        OnPropertyChanged("ControlACClassDesignList");
                    }
                    break;
                case "ACIdentifier":
                    if (CurrentProjectItem != null && CurrentACClass != null)
                        CurrentProjectItem.ACCaption = CurrentACClass.ACIdentifier;
                    if(CurrentACClass != null && !string.IsNullOrEmpty(CurrentACClass.ACIdentifier))
                        CurrentACClass.RefreshChildrenACURLCache();
                    break;
            }
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
                case "CurrentOnlineValue":
                    if (CurrentACClassProperty == null || CurrentACComponent == null || CurrentACClassProperty.ACPropUsage == Global.ACPropUsages.ConfigPointProperty)
                        return Global.ControlModes.Hidden;
                    else
                    {
                        return CurrentACClassProperty.IsInput ? Global.ControlModes.Enabled : Global.ControlModes.Disabled;
                    }
                case "CurrentACClassProperty\\ConfigACClass":
                    if (CurrentACClassProperty == null)
                        return Global.ControlModes.Hidden;
                    else
                        return CurrentACClassProperty.ACPropUsage == Global.ACPropUsages.ConfigPointProperty ? Global.ControlModes.Disabled : Global.ControlModes.Hidden;
                case "CurrentACClassProperty\\ForceBroadcast":
                    if (CurrentACClassProperty == null)
                        return Global.ControlModes.Hidden;
                    else
                        return CurrentACClassProperty.ACPropUsage == Global.ACPropUsages.ConfigPointProperty ? Global.ControlModes.Hidden : Global.ControlModes.Enabled;
                case "ShowGroup":
                    if (CurrentACProject == null)
                        return Global.ControlModes.Hidden;
                    switch (CurrentACProject.ACProjectType)
                    {
                        case Global.ACProjectTypes.Root:
                        case Global.ACProjectTypes.ClassLibrary:
                        case Global.ACProjectTypes.Application:
                        case Global.ACProjectTypes.Service:
                            return Global.ControlModes.Enabled;
                        default:
                            return Global.ControlModes.Hidden;
                    }
                case "ControlModeProjectFilter":
                    if (CurrentACProject == null)
                        return Global.ControlModes.Collapsed;
                    else
                    {
                        return (CurrentACProject.ACProjectType == Global.ACProjectTypes.AppDefinition
                                    || CurrentACProject.ACProjectType == Global.ACProjectTypes.Application
                                    || CurrentACProject.ACProjectType == Global.ACProjectTypes.Service)
                               ? Global.ControlModes.Enabled : Global.ControlModes.Collapsed;
                    }
                case "ControlModePropertyModes":
                    return ControlModePropertyModes;
                case "ControlModePointConfig":
                    return ControlModePointConfig;
                case "ControlModePointProperty":
                    return ControlModePointProperty;
                case "ShowPointStateInfos":
                    return ShowPointStateInfos;
                case "ShowBindingInfos":
                    return ShowBindingInfos;
                case "CurrentACClass\\IsRightmanagement":
                    if (CurrentACClass == null || !CurrentACClass.AssemblyACClassInfo.IsRightmanagement)
                        return Global.ControlModes.Hidden;
                    break;
                case "CurrentACProject\\PAAppClassAssignmentACClass":
                    if (CurrentACProject == null ||
                        (CurrentACProject.ACProjectType != Global.ACProjectTypes.Application
                        && CurrentACProject.ACProjectType != Global.ACProjectTypes.AppDefinition
                        && CurrentACProject.ACProjectType != Global.ACProjectTypes.Service))
                        return Global.ControlModes.Disabled;
                    break;
                case "CurrentACClass\\ACClass1_PWMethodACClass":
                    if (CurrentACProject != null && CurrentACClass != null)
                    {
                        if (CurrentACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary &&
                            (CurrentACClass.ACKind == Global.ACKinds.TPWNodeMethod || CurrentACClass.ACKind == Global.ACKinds.TPWNodeWorkflow))
                        {
                            return Global.ControlModes.Enabled;
                        }
                        else
                        {
                            return Global.ControlModes.Hidden;
                        }
                    }
                    else
                    {
                        return Global.ControlModes.Hidden;
                    }
                case "CurrentACClass\\ACClass1_PWACClass":
                    if (CurrentACProject != null && CurrentACClass != null)
                    {
                        switch (CurrentACProject.ACProjectType)
                        {
                            case Global.ACProjectTypes.ClassLibrary:
                            case Global.ACProjectTypes.AppDefinition:
                                if (CurrentACClass.ACKind != Global.ACKinds.TPAProcessModule &&
                                    CurrentACClass.ACKind != Global.ACKinds.TACApplicationManager)
                                    return Global.ControlModes.Hidden;
                                break;
                            default:
                                return Global.ControlModes.Hidden;
                        }
                    }
                    else
                    {
                        return Global.ControlModes.Hidden;
                    }
                    break;
                case "CurrentACClass\\MyPWACClass\\ACIdentifier":
                    if (CurrentACProject != null && CurrentACClass != null)
                    {
                        switch (CurrentACProject.ACProjectType)
                        {
                            case Global.ACProjectTypes.ClassLibrary:
                            case Global.ACProjectTypes.AppDefinition:
                                if (CurrentACClass.ACKind != Global.ACKinds.TPAProcessModule &&
                                    CurrentACClass.ACKind != Global.ACKinds.TPAProcessFunction &&
                                    CurrentACClass.ACKind != Global.ACKinds.TACApplicationManager)
                                    return Global.ControlModes.Hidden;
                                break;
                            case Global.ACProjectTypes.Application:
                            case Global.ACProjectTypes.Service:
                                if (CurrentACClass.ACKind != Global.ACKinds.TPAProcessModule &&
                                    CurrentACClass.ACKind != Global.ACKinds.TPAProcessFunction &&
                                    CurrentACClass.ACKind != Global.ACKinds.TACApplicationManager)
                                    return Global.ControlModes.Hidden;
                                return Global.ControlModes.Disabled;
                            default:
                                return Global.ControlModes.Hidden;
                        }
                    }
                    else
                    {
                        return Global.ControlModes.Hidden;
                    }
                    break;
                case "CurrentACClass\\ACStartTypeIndex":
                case "CurrentACClass\\ACStorableTypeIndex":
                    if (CurrentACProject != null && CurrentACClass != null)
                    {
                        if (CurrentACClass.ACClass1_ParentACClass == null && CurrentACProject.ACProjectType == Global.ACProjectTypes.Root)
                            return Global.ControlModes.Hidden;

                        if (CurrentACClass.ObjectType != null && !typeof(ACComponent).IsAssignableFrom(CurrentACClass.ObjectType))
                            return Global.ControlModes.Hidden;
                        switch (CurrentACProject.ACProjectType)
                        {
                            case Global.ACProjectTypes.Application:
                            case Global.ACProjectTypes.Service:
                            case Global.ACProjectTypes.AppDefinition:
                                return Global.ControlModes.Enabled;
                            case Global.ACProjectTypes.Root:
                                if (CurrentACClass.ACClass1_ParentACClass.ACClass1_ParentACClass == null)
                                    return Global.ControlModes.Hidden;
                                break;
                        }
                        switch (CurrentACClass.ACKind)
                        {
                            case Global.ACKinds.TPWGroup:
                            case Global.ACKinds.TPWMethod:
                            case Global.ACKinds.TPWNode:
                            case Global.ACKinds.TPWNodeMethod:
                            case Global.ACKinds.TPWNodeWorkflow:
                            case Global.ACKinds.TPWNodeStart:
                            case Global.ACKinds.TPWNodeEnd:
                                return Global.ControlModes.Hidden;
                        }
                    }
                    else
                    {
                        return Global.ControlModes.Hidden;
                    }
                    break;
                case "CurrentACClass\\IsAbstract":
                    if (CurrentACClass != null && !string.IsNullOrEmpty(CurrentACClass.AssemblyQualifiedName) && CurrentACClass.IsAbstract)
                        return Global.ControlModes.Disabled;
                    else
                        return Global.ControlModes.Hidden;

                case "CurrentACClass\\IsMultiInstance":
                    if (CurrentACClass != null && CurrentACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary)
                    {
                        switch (CurrentACClass.ACKind)
                        {
                            case Global.ACKinds.TPWGroup:
                            case Global.ACKinds.TPWMethod:
                                return Global.ControlModes.Hidden;
                        }
                    }
                    if (CurrentACClass != null && (CurrentACClass.ObjectType != null || !typeof(ACComponent).IsAssignableFrom(CurrentACClass.ObjectType)))
                        return Global.ControlModes.Hidden;
                    // Root ist nie mehrfach Instanziierbar
                    if (CurrentACClass != null && CurrentACClass.ACClass1_ParentACClass == null)
                        return Global.ControlModes.Hidden;
                    if (CurrentACProject.ACProjectType == Global.ACProjectTypes.Root)
                    {
                        if (CurrentACClass != null && CurrentACClass.ACClass1_ParentACClass.ACClass1_ParentACClass == null)
                            return Global.ControlModes.Hidden;
                    }
                    return Global.ControlModes.Enabled;

                case "CurrentACClassProperty\\ACIdentifier":
                case "CurrentACClassProperty\\ACCaption":
                case "CurrentACClassProperty\\ACGroup":
                case "CurrentACClassProperty\\ACValueType":
                case "CurrentACClassProperty\\ACPropUsageInfo":
                case "CurrentACClassProperty\\IsInput":
                case "CurrentACClassProperty\\IsOutput":
                case "CurrentACClassProperty\\IsBroadcast":
                case "CurrentACClassProperty\\IsProxyProperty":
                case "CurrentACClassProperty\\ValueTypeACClass":
                    if (CurrentACClassProperty == null)
                        return Global.ControlModes.Disabled;
                    if (CurrentACClassProperty.ACKind == Global.ACKinds.PSProperty)
                        return Global.ControlModes.Disabled;
                    break;
                case "CurrentACClassDesign\\IsDefault":
                    if (CurrentACClass == null || CurrentACClassDesign == null)
                        return Global.ControlModes.Disabled;
                    if (CurrentACClass.ACIdentifier == ACRoot.ClassName && CurrentACClassDesign.ACKind == Global.ACKinds.DSDesignMenu)
                        return Global.ControlModes.Disabled;
                    break;
                case "CurrentACClassDesign\\ACMenuName":
                case "CurrentACClassDesign\\ACValueType":
                    if (CurrentACClass == null || CurrentACClassDesign == null)
                        return Global.ControlModes.Disabled;
                    if (CurrentACClass.ACIdentifier == ACRoot.ClassName && CurrentACClassDesign.ACKind == Global.ACKinds.DSDesignMenu)
                        return Global.ControlModes.Disabled;
                    break;
                case "IsDefaultACClassDesign":
                    return ProjectManager.GetControlModeIsDefaultACClassDesign(CurrentACClass, CurrentACClassDesign, null);
                case "!NewOnDataRowStart":
                case "!NewOnDataRowEnd":
                    if (CurrentACClass.ACKind == Global.ACKinds.TACQRY)
                        return Global.ControlModes.Enabled;
                    else
                        return Global.ControlModes.Hidden;
                case "!NewWorkACClassMethod":
                    if (ProjectManager.IsEnabledNewWorkACClassMethod(CurrentACClass))
                        return Global.ControlModes.Enabled;
                    else
                        return Global.ControlModes.Hidden;
                case "!NewScriptClientACClassMethod":
                    if (CurrentACClass.ACKind != Global.ACKinds.TACQRY)
                        return Global.ControlModes.Enabled;
                    else
                        return Global.ControlModes.Hidden;
                case "CurrentACClass\\ACIdentifier":
                case "CurrentACClass\\ACPackage":
                    if (CurrentACClass != null && !string.IsNullOrEmpty(CurrentACClass.AssemblyQualifiedName))
                        return Global.ControlModes.Disabled;
                    else
                        return Global.ControlModes.Enabled;
                case "CurrentPointConfig\\LocalConfigACUrl":
                case "CurrentPointConfig\\ValueTypeACClass":
                case "CurrentPointConfig\\Expression":
                case "CurrentPointConfig\\Comment":
                case "CurrentPointConfig\\ACCaption":
                    if (CurrentPointConfig == null)
                        return Global.ControlModes.Hidden;
                    else
                        return Global.ControlModes.Enabled;

                case "CurrentContextMenuCategory":
                    if (CurrentACClassMethod == null)
                        return Global.ControlModes.Disabled;
                    return (CurrentACClassMethod.IsInteraction) ? Global.ControlModes.Enabled : Global.ControlModes.Disabled;

                case "CurrentACClassProperty\\ChangeLogMax":
                    if (CurrentACClassProperty == null || CurrentACClass == null || !(_IACObjectEntityType.IsAssignableFrom(CurrentACClass.ValueTypeACClass.ObjectType)))
                        return Global.ControlModes.Disabled;
                    return Global.ControlModes.Enabled;

                case "CurrentACClass\\ChangeLogMax":
                    if (CurrentACClass == null || !(_IACObjectEntityType.IsAssignableFrom(CurrentACClass.ValueTypeACClass.ObjectType)))
                        return Global.ControlModes.Disabled;
                    return Global.ControlModes.Enabled;
                case "CurrentACClassMethod\\ExecuteByMouseDoubleClick":
                    if (CurrentACClassMethod != null && CurrentACClassMethod.IsStatic && CurrentACClassMethod.IsInteraction)
                        return Global.ControlModes.Enabled;
                    return Global.ControlModes.Disabled;
                case "CurrentACClassMethod\\IsStatic":
                    if (CurrentACClassMethod != null && (CurrentACClassMethod.ACKindIndex == (short)Global.ACKinds.MSMethodExt || 
                                                         CurrentACClassMethod.ACKindIndex == (short)Global.ACKinds.MSMethodExtClient))
                        return Global.ControlModes.Enabled;
                    return Global.ControlModes.Disabled;
            }

            return base.OnGetControlModes(vbControl);
        }

        /// <summary>
        /// The _ control mode project filter
        /// </summary>
        Global.ControlModes _ControlModeProjectFilter = Global.ControlModes.Collapsed;
        /// <summary>
        /// Gets or sets the control mode project filter.
        /// </summary>
        /// <value>The control mode project filter.</value>
        [ACPropertyInfo(9999)]
        public Global.ControlModes ControlModeProjectFilter
        {
            get
            {
                return _ControlModeProjectFilter;
            }
            set
            {
                _ControlModeProjectFilter = value;
                OnPropertyChanged("ControlModeProjectFilter");
            }
        }
        #endregion

        #region IVBFindAndReplaceDBSearch
        /// <summary>
        /// FARs the search in DB.
        /// </summary>
        /// <param name="bso">The bso.</param>
        /// <param name="wordToFind">The word to find.</param>
        /// <returns>IEnumerable{IACObjectEntityWithCheckTrans}.</returns>
        public IEnumerable<IACObjectEntityWithCheckTrans> FARSearchInDB(VBBSOFindAndReplace bso, string wordToFind)
        {
            if (bso == null)
                return new List<IACObjectEntityWithCheckTrans>();
            string findPattern = String.Format("%{0}%", wordToFind);
            if (bso.ACIdentifier.Contains("Script"))
            {
                var query = Database.ContextIPlus.ACClassMethod.Where(c => (SqlFunctions.PatIndex(findPattern, c.Sourcecode) > 0)
                                                                && c.ACClass.ACProjectID == CurrentACProject.ACProjectID);
                return query;
            }
            else if (bso.ACIdentifier.Contains("XML"))
            {
                var query = Database.ContextIPlus.ACClassDesign.Where(c => (SqlFunctions.PatIndex(findPattern, c.XMLDesign) > 0)
                                                                && c.ACClass.ACProjectID == CurrentACProject.ACProjectID);
                return query;
            }
            return new List<IACObjectEntityWithCheckTrans>();
        }

        /// <summary>
        /// Determines whether [is enabled FAR search in DB] [the specified bso].
        /// </summary>
        /// <param name="bso">The bso.</param>
        /// <returns><c>true</c> if [is enabled FAR search in DB] [the specified bso]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledFARSearchInDB(VBBSOFindAndReplace bso)
        {
            if (bso == null)
                return false;
            return true;
        }

        /// <summary>
        /// FARs the search item selected.
        /// </summary>
        /// <param name="bso">The bso.</param>
        /// <param name="item">The item.</param>
        public void FARSearchItemSelected(VBBSOFindAndReplace bso, IACObjectEntityWithCheckTrans item)
        {
            if ((bso == null) || (item == null))
                return;
            if (bso.ACIdentifier.Contains("Script") && item is ACClassMethod)
            {
                ACClassMethod acClassMethod = item as ACClassMethod;
                ACClassInfoWithItems classItem = null;
                ProjectManager.MapClassToItem.TryGetValue(acClassMethod.ACClass, out classItem);
                if (classItem != null)
                {
                    CurrentProjectItem = classItem;
                    CurrentProjectItemRootChangeInfo = new ChangeInfo(null, CurrentProjectItem, Const.CmdUpdateAllData);
                    CurrentACClassMethod = acClassMethod;
                }
            }
            else if (bso.ACIdentifier.Contains("XML") && item is ACClassDesign)
            {
                ACClassDesign acClassDesign = item as ACClassDesign;
                ACClassInfoWithItems classItem = null;
                ProjectManager.MapClassToItem.TryGetValue(acClassDesign.ACClass, out classItem);
                if (classItem != null)
                {
                    CurrentProjectItem = classItem;
                    CurrentProjectItemRootChangeInfo = new ChangeInfo(null, CurrentProjectItem, Const.CmdUpdateAllData);
                    CurrentACClassDesign = acClassDesign;
                }
            }
        }

        #endregion

        #region Data Export

        public override void DataExportDialog()
        {
            SelectedDataExportType = null;
            ShowDialog(this, "VBStudioExportDialog");
        }


        public override bool IsEnabledDataExportOk()
        {
            return CurrentProjectItemRoot != null && SelectedDataExportType != null;
        }

        #region DataExport -> DataExportType

        ACValueItem _SelectedDataExportType;
        [ACPropertySelected(9999, "DataExportType", "en{'Export type'}de{'Exporttyp'}")]
        public ACValueItem SelectedDataExportType
        {
            get
            {
                return _SelectedDataExportType;
            }
            set
            {
                if (_SelectedDataExportType != value)
                {
                    _SelectedDataExportType = value;
                    DataExportFilePath = null;
                    if (SelectedDataExportType != null)
                        DataExportFilePath = DataExportGenerateFileName();
                    OnPropertyChanged("SelectedDataExportType");
                }
            }
        }



        private ACValueItemList dataExportTypeList = null;
        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        [ACPropertyList(9999, "DataExportType")]
        public IEnumerable<ACValueItem> DataExportTypeList
        {
            get
            {
                if (dataExportTypeList == null)
                {
                    dataExportTypeList = new ACValueItemList("DataExportTypeList");
                    dataExportTypeList.AddEntry(1, "en{'Export current Project Tree'}de{'Exportieren Sie den aktuellen Projektbaum'}");
                    dataExportTypeList.AddEntry(2, "en{'Export current Project Tree with properties and methods'}de{'Exportieren Sie den aktuellen Projektbaum mit Methoden und Eingeschaften'}");
                    dataExportTypeList.AddEntry(3, "en{'Export class hierarchy'}de{'Klassenhierarchie exportieren'}");
                    dataExportTypeList.AddEntry(4, "en{'Export iPlus menu'}de{'iPlus Menu exportieren'}");
                    dataExportTypeList.AddEntry(5, "en{'Export current translation items to Excel'}de{'Aktuelle Übersetzungselemente nach Excel exportieren'}");
                }
                return dataExportTypeList;
            }
        }

        #endregion


        public override string DataExportGenerateFileName()
        {
            string name = "";
            switch ((int)SelectedDataExportType.Value)
            {
                case 1:
                    name = "ProjectTree";
                    if (CurrentProjectItemRoot != null)
                        name += "_" + CurrentProjectItemRoot.ACIdentifier;
                    break;
                case 2:
                    name = "ProjectTreeWithItems";
                    if (CurrentProjectItemRoot != null)
                        name += "_" + CurrentProjectItemRoot.ACIdentifier;
                    break;
                case 3:
                    name = "ClassHierarchy";
                    if (CurrentProjectItemRoot != null)
                        name += "_" + CurrentProjectItemRoot.ACIdentifier;
                    break;
                case 4:
                    name = "VarioabatchMenu";
                    if (SelectedACClassDesign.ACUsage == Global.ACUsages.DUMainmenu)
                        name = SelectedACClassDesign.ACIdentifier;
                    break;
                case 5:
                    return string.Format("{0}\\{1}_Translation_{2}.xlsx", Root.Environment.Datapath, CurrentProjectItem?.ACIdentifier, DateTime.Now.ToString("yyyy-MM-dd_HH-mm"));
            }
            return string.Format(Root.Environment.Datapath + @"\{0}_{1}.xml", name, DateTime.Now.ToString("yyyy-MM-dd_HH-mm"));
        }


        public override void BgWorkerDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = e.Argument.ToString();
            switch (command)
            {
                case "DataExport":
                    VBStudioExport();
                    break;
            }
        }


        #region VBStudioExport
        private void VBStudioExport()
        {
            if (SelectedDataExportType != null)
            {
                switch ((int)SelectedDataExportType.Value)
                {
                    case 1:
                        VBStudioExport_ExportProjectTree(null);
                        break;
                    case 2:
                        VBStudioExport_ExportProjectTree(VBStudioExport_HandlePropAndMehtods);
                        break;
                    case 3:
                        VBStudioExport_ClassHierarchy();
                        break;
                    case 4:
                        VBStudioExport_VariobatchMenu();
                        break;
                    case 5:
                        VBStudioExport_TranslationExcel();
                        break;
                }
            }
        }


        // Exporting elements
        private void VBStudioExport_ExportProjectTree(Action<ACClass, ACItem> handleCreationEvent)
        {
            ACItem presentation = new ACItem(CurrentProjectItemRoot, handleCreationEvent);
            using (FileStream fs = new FileStream(DataExportFilePath, FileMode.OpenOrCreate))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ACItem));
                serializer.Serialize(fs, presentation);
            }
        }


        private void VBStudioExport_ClassHierarchy()
        {
            ACItem rootItem = new ACItem() { ACIdentifier = "#root", ACTypeACIdentifier = "ACClass" };


            string iplusNamespace = "gip.core.datamodel";
            string basedACClassACIdentifier = @"VBEntityObject";

            List<ACClass> acClasses = 
                Database
                .ContextIPlus
                .ACClass
                .Where(c => !c.AssemblyQualifiedName.Contains(iplusNamespace) ||
                            (c.AssemblyQualifiedName.Contains(iplusNamespace) && !(c.BasedOnACClassID != null && c.ACClass1_BasedOnACClass.ACIdentifier == basedACClassACIdentifier))
                    )
                .OrderBy(c => c.ACIdentifier)
                .ToList();


            var tmpList = acClasses
                .Select(c =>
                    new
                    {
                        acClass = c,
                        acItem = VBStudioExport_FactoryACitem(c)
                    }
                ).ToList(); 
            tmpList.ForEach(c =>
                {
                    List<ACItem> childItem = tmpList.Select(p => p.acItem).Where(p => p.ParentID != null && p.ParentID == c.acItem.ID).OrderBy(p => p.ACIdentifier).ToList();
                    c.acItem.Items.AddRange(childItem);
                    VBStudioExport_HandlePropAndMehtods(c.acClass, c.acItem);
                });
            List<ACItem> rootACItems = tmpList.Select(p => p.acItem).Where(p => string.IsNullOrEmpty(p.ParentID)).OrderBy(p => p.ACIdentifier).ToList();
            rootItem.Items.AddRange(rootACItems);
            using (FileStream fs = new FileStream(DataExportFilePath, FileMode.OpenOrCreate))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ACItem));
                serializer.Serialize(fs, rootItem);
            }
        }


        private void VBStudioExport_VariobatchMenu()
        {
            ACMenuItemLight rootMenuItem = null;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ACMenuItemLight), @"http://schemas.datacontract.org/2004/07/gip.core.datamodel");
            ACClassDesign variobatchMenuDesginItem = null;
            if (SelectedACClassDesign.ACUsage == Global.ACUsages.DUMainmenu)
            {
                variobatchMenuDesginItem = SelectedACClassDesign;
            }
            else
            {
                variobatchMenuDesginItem = Database.ContextIPlus.ACClassDesign.FirstOrDefault(p => p.ACIdentifier == "IPlusMESMenu");
            }
            if (variobatchMenuDesginItem == null)
                return;
            using (StringReader tr = new StringReader(variobatchMenuDesginItem.XMLDesign))
            {
                rootMenuItem = (ACMenuItemLight)xmlSerializer.Deserialize(tr);

            }
            ACItem rootACItem = null;
            if (rootMenuItem != null)
            {
                rootACItem = VBStudioExport_GetACItemFromMenuItem(rootMenuItem, Database.ContextIPlus);
                xmlSerializer = new XmlSerializer(typeof(ACItem));
                using (FileStream fs = new FileStream(DataExportFilePath, FileMode.OpenOrCreate))
                {
                    xmlSerializer.Serialize(fs, rootACItem);
                }
            }
        }

        private void VBStudioExport_TranslationExcel()
        {
            if (CurrentProjectItem == null)
                return;

            try
            {

                List<Tuple<string, string, string>> messages = new List<Tuple<string, string, string>>();

                using (Database db = new core.datamodel.Database())
                {
                    ACClass currentACClass = CurrentACClass.FromIPlusContext<ACClass>(db);
                    var classMessages = currentACClass.ACClassMessage_ACClass.OrderBy(c => c.ACIdentifier).ToArray();

                    foreach (ACClassMessage classMessage in classMessages)
                    {
                        string enTrans = classMessage.GetTranslation("en");
                        string deTrans = classMessage.GetTranslation("de");
                        messages.Add(new Tuple<string, string, string>(classMessage.ACIdentifier, enTrans, deTrans));
                    }
                }

                XLWorkbook workBook = new XLWorkbook();
                IXLWorksheet workSheet = workBook.Worksheets.Add("Translation");

                var headerCell = workSheet.Cell("A1");
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = XLColor.Beige;
                headerCell.Value = "MessageID";
                headerCell = workSheet.Cell("A2");
                SetBorder(headerCell, false, true);
                workSheet.Range("A1:A2").Merge();

                headerCell = workSheet.Cell("B1");
                SetBorder(headerCell, true, false);
                headerCell.Value = "en";

                headerCell = workSheet.Cell("B2");
                SetBorder(headerCell, false, true);
                headerCell.Value = "de";

                int currentRow = 4;

                foreach (var message in messages)
                {
                    var cell = workSheet.Cell("A" + currentRow);
                    cell.Value = message.Item1;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.Beige;
                    SetBorder(cell, true, false);

                    cell = workSheet.Cell("A" + (currentRow + 1));
                    SetBorder(cell, false, true);

                    workSheet.Range(string.Format("A{0}:A{1}", currentRow, currentRow + 1)).Merge();

                    cell = workSheet.Cell("B" + currentRow);
                    cell.Value = message.Item2;
                    SetBorder(cell, true, false);
                    //cell.Style.Fill.BackgroundColor = XLColor.Beige;

                    cell = workSheet.Cell("B" + (currentRow + 1));
                    cell.Value = message.Item3;
                    SetBorder(cell, false, true);
                    //cell.Style.Fill.BackgroundColor = XLColor.Beige;

                    currentRow += 3;
                }
                workSheet.Column("A").Width = 12;
                workSheet.Column("B").AdjustToContents();
                workBook.SaveAs(DataExportFilePath);
            }
            catch(Exception e)
            {
                Messages.LogException(this.GetACUrl(), "VBStudioExport_TranslationExcel", e);
            }
        }

        private void SetBorder(IXLCell cell, bool top, bool bottom)
        {
            if (top)
            {
                cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.TopBorderColor = XLColor.Black;
            }

            if (bottom)
            {
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.BottomBorderColor = XLColor.Black;
            }
        }

        #endregion

        #endregion

        #region Export definitions - handle properties and mehtods

        private ACItem VBStudioExport_FactoryACitem(ACClass acClass)
        {
            ACItem item = new ACItem();
            item.ReadACClassData(acClass);
            if (acClass.BasedOnACClassID != null)
                item.ParentID = acClass.BasedOnACClassID.Value.ToString();
            return item;
        }

        private void VBStudioExport_HandlePropAndMehtods(ACClass acClass, ACItem acItem)
        {
            try
            {
                IEnumerable<ACClassProperty> propertyList = acClass.ACClassProperty_ACClass.OrderBy(p => p.ACIdentifier).ToList();
                IEnumerable<ACClassMethod> methodList = acClass.ACClassMethod_ACClass.OrderBy(p => p.ACIdentifier).ToList();

                if (propertyList != null && propertyList.Any())
                {
                    List<ACItem> propertyPresentationList = propertyList.Select(p => VBStudioExport_FactoryACItemForProperty(p)).ToList();
                    acItem.Items.AddRange(propertyPresentationList);
                }

                if (methodList != null && methodList.Any())
                {
                    List<ACItem> methodPresentationList = methodList.Select(p => VBStudioExport_FactoryACItemForMethod(p)).ToList();
                    acItem.Items.AddRange(methodPresentationList);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("BSOiPlusStudio", "VBStudioExport_HandlePropsAndMethods", msg);
            }
        }

        private ACItem VBStudioExport_FactoryACItemForProperty(ACClassProperty property)
        {
            ACItem item = new ACItem();
            item.ID = property.ACClassPropertyID.ToString();
            item.ACIdentifier = property.ACIdentifier;
            item.ACTypeACIdentifier = "ACClassProperty";
            item.ACCaptionTranslation = property.ACCaptionTranslation;
            try
            {
                item.ACUrl = property.GetACUrl();
                item.ACUrlComponent = property.GetACUrlComponent();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("BSOiPlusStudio", "VBStudioExport_FactoryACItemForProperty", msg);
            }
            return item;
        }

        private ACItem VBStudioExport_FactoryACItemForMethod(ACClassMethod method)
        {
            ACItem item = new ACItem();
            item.ID = method.ACClassMethodID.ToString();
            item.ACIdentifier = method.ACIdentifier;
            item.ACTypeACIdentifier = "ACClassMethod";
            item.ACCaptionTranslation = method.ACCaptionTranslation;
            try
            {
                item.ACUrl = method.GetACUrl();
                item.ACUrlComponent = method.GetACUrlComponent();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("BSOiPlusStudio", "VBStudioExport_FactoryACItemForMethod", msg);
            }
            return item;
        }

        private ACItem VBStudioExport_GetACItemFromMenuItem(ACMenuItemLight menuItem, Database db)
        {
            ACItem acItem = new ACItem();
            acItem.ACCaptionTranslation = menuItem.ACCaptionTranslation;
            acItem.ACUrl = menuItem.ACUrl;
            if (string.IsNullOrEmpty(menuItem.ACUrl))
            {
                acItem.ID = Guid.Empty.ToString();
                acItem.ACIdentifier = menuItem.ACCaption;
                acItem.ACIdentifier = acItem.ACIdentifier.TrimStart("&lt;".ToCharArray());
                acItem.ACIdentifier = acItem.ACIdentifier.TrimEnd("&gt;".ToCharArray());
                acItem.ACTypeACIdentifier = "VariobatchGroup";
            }
            else
            {
                string acIdentifier = menuItem.ACUrl.Substring(menuItem.ACUrl.IndexOf('#') + 1, menuItem.ACUrl.Length - 1 - menuItem.ACUrl.IndexOf('#'));
                ACClass acClassItem = db.ACClass.FirstOrDefault(p => p.ACIdentifier == acIdentifier);
                if (acClassItem != null)
                {
                    acItem.ID = acClassItem.ACClassID.ToString();
                    acItem.ACIdentifier = acIdentifier;
                    acItem.ACCaptionTranslation = acClassItem.ACCaptionTranslation;
                    acItem.IsAbstract = acClassItem.IsAbstract;
                    acItem.IsInterface = acClassItem.IsInterface;
                    acItem.Comment = acClassItem.Comment;
                    acItem.AssemblyQualifiedName = acClassItem.AssemblyQualifiedName;
                    acItem.ACUrl = acClassItem.ACUrl;
                    acItem.ACTypeACIdentifier = "ACClass";
                }
            }
            if (menuItem.Items != null && menuItem.Items.Any())
                foreach (ACMenuItemLight childMenuItem in menuItem.Items)
                {
                    ACItem childACItem = VBStudioExport_GetACItemFromMenuItem(childMenuItem, db);
                    acItem.Items.Add(childACItem);
                }
            return acItem;
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "IsEnabledFARSearchInDB":
                    result = IsEnabledFARSearchInDB((VBBSOFindAndReplace)acParameter[0]);
                    return true;
                case "IsEnabledDataExportOk":
                    result = IsEnabledDataExportOk();
                    return true;
                case "IsEnabledNewMessageWarning":
                    result = IsEnabledNewMessageWarning();
                    return true;
                case "NewMessageFailure":
                    NewMessageFailure();
                    return true;
                case "IsEnabledNewMessageFailure":
                    result = IsEnabledNewMessageFailure();
                    return true;
                case "NewMessageError":
                    NewMessageError();
                    return true;
                case "IsEnabledNewMessageError":
                    result = IsEnabledNewMessageError();
                    return true;
                case "NewMessageException":
                    NewMessageException();
                    return true;
                case "IsEnabledNewMessageException":
                    result = IsEnabledNewMessageException();
                    return true;
                case "NewMessageQuestion":
                    NewMessageQuestion();
                    return true;
                case "IsEnabledNewMessageQuestion":
                    result = IsEnabledNewMessageQuestion();
                    return true;
                case "NewMessageStatus":
                    NewMessageStatus();
                    return true;
                case "IsEnabledNewMessageStatus":
                    result = IsEnabledNewMessageStatus();
                    return true;
                case "NewOK":
                    NewOK();
                    return true;
                case "IsEnabledNewOK":
                    result = IsEnabledNewOK();
                    return true;
                case "NewCancel":
                    NewCancel();
                    return true;
                case "DeleteACTranslation":
                    DeleteACTranslation();
                    return true;
                case "IsEnabledDeleteACTranslation":
                    result = IsEnabledDeleteACTranslation();
                    return true;
                case "RefreshTranslation":
                    RefreshTranslation();
                    return true;
                case "IsEnabledACActionToTarget":
                    result = IsEnabledACActionToTarget((IACInteractiveObject)acParameter[0], (ACActionArgs)acParameter[1]);
                    return true;
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
                case "NewACProjectOK":
                    NewACProjectOK();
                    return true;
                case "IsEnabledNewACProjectOK":
                    result = IsEnabledNewACProjectOK();
                    return true;
                case "NewACProjectCancel":
                    NewACProjectCancel();
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
                case "ShowProjectProperty":
                    ShowProjectProperty();
                    return true;
                case "IsEnabledShowProjectProperty":
                    result = IsEnabledShowProjectProperty();
                    return true;
                case "NewACClass":
                    NewACClass();
                    return true;
                case "IsEnabledNewACClass":
                    result = IsEnabledNewACClass();
                    return true;
                case "NewChildACClass":
                    NewChildACClass();
                    return true;
                case "IsEnabledNewChildACClass":
                    result = IsEnabledNewChildACClass();
                    return true;
                case "SwitchACClass":
                    SwitchACClass();
                    return true;
                case "IsEnabledSwitchACClass":
                    result = IsEnabledSwitchACClass();
                    return true;
                case "NewACClassOK":
                    NewACClassOK();
                    return true;
                case "IsEnabledNewACClassOK":
                    result = IsEnabledNewACClassOK();
                    return true;
                case "NewACClassCancel":
                    NewACClassCancel();
                    return true;
                case "SwitchACClassOK":
                    SwitchACClassOK();
                    return true;
                case "IsEnabledSwitchACClassOK":
                    result = IsEnabledSwitchACClassOK();
                    return true;
                case "SwitchACClassCancel":
                    SwitchACClassCancel();
                    return true;
                case "DeleteACClass":
                    DeleteACClass();
                    return true;
                case "IsEnabledDeleteACClass":
                    result = IsEnabledDeleteACClass();
                    return true;
                case "ValidateInput":
                    result = ValidateInput((String)acParameter[0], (Object)acParameter[1], (System.Globalization.CultureInfo)acParameter[2]);
                    return true;
                case nameof(StartBSO):
                    StartBSO();
                    return true;
                case nameof(IsEnabledStartBSO):
                    result = IsEnabledStartBSO();
                    return true;
                case nameof(StartInstance):
                    StartInstance();
                    return true;
                case nameof(IsEnabledStartInstance):
                    result = IsEnabledStartInstance();
                    return true;
                case nameof(StopInstance):
                    StopInstance();
                    return true;
                case nameof(IsEnabledStopInstance):
                    result = IsEnabledStopInstance();
                    return true;
                case nameof(ReloadInstance):
                    ReloadInstance();
                    return true;
                case nameof(IsEnabledReloadInstance):
                    result = IsEnabledReloadInstance();
                    return true;
                case "DropACClass":
                    DropACClass((Global.ElementActionType)acParameter[0], (IACInteractiveObject)acParameter[1], (IACInteractiveObject)acParameter[2], (Double)acParameter[3], (Double)acParameter[4]);
                    return true;
                case "IsEnabledDropACClass":
                    result = IsEnabledDropACClass((Global.ElementActionType)acParameter[0], (IACInteractiveObject)acParameter[1], (IACInteractiveObject)acParameter[2], (Double)acParameter[3], (Double)acParameter[4]);
                    return true;
                case "ModifyACClass":
                    ModifyACClass();
                    return true;
                case "ShowClassLibrary":
                    ShowClassLibrary();
                    return true;
                case "IsEnabledShowClassLibrary":
                    result = IsEnabledShowClassLibrary();
                    return true;
                case "LoadACClassDesign":
                    LoadACClassDesign();
                    return true;
                case "IsEnabledLoadACClassDesign":
                    result = IsEnabledLoadACClassDesign();
                    return true;
                case "NewACClassDesign":
                    NewACClassDesign();
                    return true;
                case "IsEnabledNewACClassDesign":
                    result = IsEnabledNewACClassDesign();
                    return true;
                case "NewACClassDesignOK":
                    NewACClassDesignOK();
                    return true;
                case "IsEnabledNewACClassDesignOK":
                    result = IsEnabledNewACClassDesignOK();
                    return true;
                case "NewACClassDesignCancel":
                    NewACClassDesignCancel();
                    return true;
                case "DeleteACClassDesign":
                    DeleteACClassDesign();
                    return true;
                case "IsEnabledDeleteACClassDesign":
                    result = IsEnabledDeleteACClassDesign();
                    return true;
                case "DuplicateDesign":
                    DuplicateDesign();
                    return true;
                case "IsEnabledDuplicateDesign":
                    result = IsEnabledDuplicateDesign();
                    return true;
                case "OnToolWindowItemExpand":
                    OnToolWindowItemExpand((ACObjectItem)acParameter[0]);
                    return true;
                case "ImportBitmap":
                    ImportBitmap();
                    return true;
                case "IsEnabledImportBitmap":
                    result = IsEnabledImportBitmap();
                    return true;
                case nameof(GenerateIcon):
                    GenerateIcon();
                    return true;
                case nameof(IsEnabledGenerateIcon):
                    result = IsEnabledGenerateIcon();
                    return true;
                case "CompileACClassDesign":
                    CompileACClassDesign();
                    return true;
                case "IsEnabledCompileACClassDesign":
                    result = IsEnabledCompileACClassDesign();
                    return true;
                case "DeleteMenuEntry":
                    DeleteMenuEntry();
                    return true;
                case "IsEnabledDeleteMenuEntry":
                    result = IsEnabledDeleteMenuEntry();
                    return true;
                case "InsertMenuEntry":
                    InsertMenuEntry();
                    return true;
                case "IsEnabledInsertMenuEntry":
                    result = IsEnabledInsertMenuEntry();
                    return true;
                case "NewMenuEntry":
                    NewMenuEntry();
                    return true;
                case "IsEnabledNewMenuEntry":
                    result = IsEnabledNewMenuEntry();
                    return true;
                case "NewChildMenuEntry":
                    NewChildMenuEntry();
                    return true;
                case "IsEnabledNewChildMenuEntry":
                    result = IsEnabledNewChildMenuEntry();
                    return true;
                case "ACUrlEditor":
                    ACUrlEditor();
                    return true;
                case "IsEnabledACUrlEditor":
                    result = IsEnabledACUrlEditor();
                    return true;
                case "NewConfigACClassProperty":
                    NewConfigACClassProperty();
                    return true;
                case "IsEnabledNewConfigACClassProperty":
                    result = IsEnabledNewConfigACClassProperty();
                    return true;
                case "DeleteConfigACClassProperty":
                    DeleteConfigACClassProperty();
                    return true;
                case "IsEnabledDeleteConfigACClassProperty":
                    result = IsEnabledDeleteConfigACClassProperty();
                    return true;
                case "NewPointConfig":
                    NewPointConfig();
                    return true;
                case "IsEnabledNewPointConfig":
                    result = IsEnabledNewPointConfig();
                    return true;
                case "DeletePointConfig":
                    DeletePointConfig();
                    return true;
                case "IsEnabledDeletePointConfig":
                    result = IsEnabledDeletePointConfig();
                    return true;
                case "NewText":
                    NewText();
                    return true;
                case "IsEnabledNewText":
                    result = IsEnabledNewText();
                    return true;
                case "NewMessageInfo":
                    NewMessageInfo();
                    return true;
                case "IsEnabledNewMessageInfo":
                    result = IsEnabledNewMessageInfo();
                    return true;
                case "NewMessageWarning":
                    NewMessageWarning();
                    return true;
                case "UpdateOnlineValue":
                    UpdateOnlineValue();
                    return true;
                case "NewACClassProperty":
                    NewACClassProperty();
                    return true;
                case "IsEnabledNewACClassProperty":
                    result = IsEnabledNewACClassProperty();
                    return true;
                case "DeleteACClassProperty":
                    DeleteACClassProperty();
                    return true;
                case "IsEnabledDeleteACClassProperty":
                    result = IsEnabledDeleteACClassProperty();
                    return true;
                case "RemovePropertyRelationTo":
                    RemovePropertyRelationTo();
                    return true;
                case "IsEnabledRemovePropertyRelationTo":
                    result = IsEnabledRemovePropertyRelationTo();
                    return true;
                case "RemovePropertyRelationFrom":
                    RemovePropertyRelationFrom();
                    return true;
                case "IsEnabledRemovePropertyRelationFrom":
                    result = IsEnabledRemovePropertyRelationFrom();
                    return true;
                case "RemovePropertyBinding":
                    RemovePropertyBinding();
                    return true;
                case "IsEnabledRemovePropertyBinding":
                    result = IsEnabledRemovePropertyBinding();
                    return true;
                case "NewPointStateInfo":
                    NewPointStateInfo();
                    return true;
                case "IsEnabledNewPointStateInfo":
                    result = IsEnabledNewPointStateInfo();
                    return true;
                case "DeletePointStateInfo":
                    DeletePointStateInfo();
                    return true;
                case "IsEnabledDeletePointStateInfo":
                    result = IsEnabledDeletePointStateInfo();
                    return true;
                case "DropPBACClassProperty":
                    DropPBACClassProperty((Global.ElementActionType)acParameter[0], (IACInteractiveObject)acParameter[1], (IACInteractiveObject)acParameter[2], (Double)acParameter[3], (Double)acParameter[4]);
                    return true;
                case "IsEnabledDropPBACClassProperty":
                    result = IsEnabledDropPBACClassProperty((Global.ElementActionType)acParameter[0], (IACInteractiveObject)acParameter[1], (IACInteractiveObject)acParameter[2], (Double)acParameter[3], (Double)acParameter[4]);
                    return true;
                case "GetBindingSourceACUrl":
                    result = GetBindingSourceACUrl((Object)acParameter[0]);
                    return true;
                case "GetBindingSourceMultiplier":
                    result = GetBindingSourceMultiplier((Object)acParameter[0]);
                    return true;
                case "GetBindingSourceDivisor":
                    result = GetBindingSourceDivisor((Object)acParameter[0]);
                    return true;
                case "GetBindingSourceExprT":
                    result = GetBindingSourceExprT((Object)acParameter[0]);
                    return true;
                case "GetBindingSourceExprS":
                    result = GetBindingSourceExprS((Object)acParameter[0]);
                    return true;
                case "GetBindingTargetsACUrl":
                    result = GetBindingTargetsACUrl((Object)acParameter[0]);
                    return true;
                case "LoadACClassMethod":
                    LoadACClassMethod();
                    return true;
                case "IsEnabledLoadACClassMethod":
                    result = IsEnabledLoadACClassMethod();
                    return true;
                case "NewWorkACClassMethod":
                    NewWorkACClassMethod();
                    return true;
                case "IsEnabledNewWorkACClassMethod":
                    result = IsEnabledNewWorkACClassMethod();
                    return true;
                case "NewScriptACClassMethod":
                    NewScriptACClassMethod();
                    return true;
                case "IsEnabledNewScriptACClassMethod":
                    result = IsEnabledNewScriptACClassMethod();
                    return true;
                case "NewScriptClientACClassMethod":
                    NewScriptClientACClassMethod();
                    return true;
                case "IsEnabledNewScriptClientACClassMethod":
                    result = IsEnabledNewScriptClientACClassMethod();
                    return true;
                case "NewPreACClassMethod":
                    NewPreACClassMethod();
                    return true;
                case "IsEnabledNewPreACClassMethod":
                    result = IsEnabledNewPreACClassMethod();
                    return true;
                case "NewPostACClassMethod":
                    NewPostACClassMethod();
                    return true;
                case "IsEnabledNewPostACClassMethod":
                    result = IsEnabledNewPostACClassMethod();
                    return true;
                case "NewOnSetPropertyACClassMethod":
                    NewOnSetPropertyACClassMethod();
                    return true;
                case "IsEnabledNewOnSetPropertyACClassMethod":
                    result = IsEnabledNewOnSetPropertyACClassMethod();
                    return true;
                case "NewOnSetPropertyNetACClassMethod":
                    NewOnSetPropertyNetACClassMethod();
                    return true;
                case "IsEnabledNewOnSetPropertyNetACClassMethod":
                    result = IsEnabledNewOnSetPropertyNetACClassMethod();
                    return true;
                case "NewOnSetPointACClassMethod":
                    NewOnSetPointACClassMethod();
                    return true;
                case "IsEnabledNewOnSetPointACClassMethod":
                    result = IsEnabledNewOnSetPointACClassMethod();
                    return true;
                case "DeleteACClassMethod":
                    DeleteACClassMethod();
                    return true;
                case "IsEnabledDeleteACClassMethod":
                    result = IsEnabledDeleteACClassMethod();
                    return true;
                case "CompileACClassMethod":
                    CompileACClassMethod();
                    return true;
                case "IsEnabledCompileACClassMethod":
                    result = IsEnabledCompileACClassMethod();
                    return true;
                case "CloneACClass":
                    CloneACClass();
                    return true;
                case "IsEnabledCloneACClass":
                    result = IsEnabledCloneACClass();
                    return true;
                case "CloneDialogOK":
                    CloneDialogOK();
                    return true;
                case "IsEnabledCloneDialogOK":
                    result = IsEnabledCloneDialogOK();
                    return true;
                case "GenerateExecuteHandler":
                    GenerateExecuteHandler();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }


}

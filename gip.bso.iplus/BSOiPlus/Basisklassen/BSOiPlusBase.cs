// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-15-2012
// ***********************************************************************
// <copyright file="BSOiPlusBase.cs" company="gip mbh, Oftersheim, Germany">
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

namespace gip.bso.iplus
{
    /// <summary>
    /// Abstrakte Basisklasse für alle Workflows für ACClassMethod
    /// Hiervon gibt es zwei Ableitungen
    /// -BSOiPlusStudio
    /// -BSOiPlusWorkflow
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Workflow Base'}de{'Workflow Base'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, true)]
    public abstract class BSOiPlusBase : ACBSONav
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="BSOiPlusBase"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOiPlusBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._ACClassMethodControlList = null;
            this._ACClassMethodIconList = null;
            this._ACClassMethodList = null;
            this._ConfigACClassMethodList = null;
            this._CurrentACClassMethodControl = null;
            this._CurrentACClassMethodIcon = null;
            this._CurrentACMethod = null;
            this._CurrentACMethodResultValue = null;
            this._CurrentACMethodValue = null;
            this._CurrentConfigACClassMethod = null;
            this._CurrentInvokingACCommand = null;
            this._CurrentMethodMode = null;
            this._CurrentNewACClassMethod = null;
            this._MethodModeList = null;
            if (this._VBPresenterMethod != null)
            {
                (this._VBPresenterMethod as VBPresenterMethod).PropertyChanged -= BSOiPlusBase_PropertyChanged;
                this._VBPresenterMethod = null;
            }


            this._VBDesignerWorkflowMethod = null;
            bool done = base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            this._BSODatabase = null;
            return done;
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
                //case "CurrentACClassMethod\\ContinueByError":
                //    if (CurrentACClassMethod == null || CurrentACClassMethod.ACKind != Global.ACKinds.MSMethodPrePost)
                //        return Global.ControlModes.Hidden;
                //    break;
                case "CurrentACClassMethod\\IsSubMethod":
                    if (CurrentACClassMethod == null || CurrentACClassMethod.ACKind != Global.ACKinds.MSWorkflow)
                        return Global.ControlModes.Hidden;
                    break;
                case "CurrentACClassMethod\\ACIdentifier":
                case "CurrentACClassMethod\\ACKindInfo":
                case "CurrentACClassMethod\\PWACClass":
                case "CurrentACClassMethod\\IsAutoenabled":
                case "CurrentACClassMethod\\SortIndex":
                case "CurrentACClassMethod\\ValueTypeACClass":
                    if (CurrentACClassMethod == null)
                        return Global.ControlModes.Disabled;
                    switch (CurrentACClassMethod.ACKind)
                    {
                        case Global.ACKinds.MSMethod:
                        case Global.ACKinds.MSMethodClient:
                        case Global.ACKinds.MSMethodPrePost:
                            return Global.ControlModes.Disabled;
                        case Global.ACKinds.MSMethodExt:
                        case Global.ACKinds.MSMethodExtTrigger:
                        case Global.ACKinds.MSMethodExtClient:
                            return Global.ControlModes.Enabled;
                        case Global.ACKinds.MSWorkflow:
                        default:
                            return Global.ControlModes.Enabled;
                    }
                case "CurrentACClassMethodControl":
                case "CurrentACClassMethodIcon":
                    if (CurrentACClassMethod == null)
                        return Global.ControlModes.Disabled;
                    return (CurrentACClassMethod.IsCommand || CurrentACClassMethod.IsInteraction) ? Global.ControlModes.Enabled : Global.ControlModes.Disabled;
                case "CurrentACMethod\\ACIdentifier":
                case "CurrentACMethod\\ACState":
                    if (CurrentACClassMethod == null || CurrentACMethod == null)
                        return Global.ControlModes.Disabled;
                    return CurrentACClassMethod.ACKind == Global.ACKinds.MSMethod ? Global.ControlModes.Disabled : Global.ControlModes.Enabled;
            }
            return base.OnGetControlModes(vbControl);
        }
        #endregion

        #region BSO->ACMethod
        #region 1.1.2.3 ConfigACClassMethod
        /// <summary>
        /// News the config AC class method.
        /// </summary>
        [ACMethodInfo("ConfigACClassMethod", "en{'New virtual method'}de{'Neue virtuelle Methode'}", (short)MISort.New, true)]
        public void NewConfigACClassMethod()
        {
            if (!PreExecute("NewConfigACClassMethod")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassMethod acClassMethod = ACClassMethod.NewACObject(Database.ContextIPlus, CurrentACClass);
            acClassMethod.ACClassMethod1_ParentACClassMethod = CurrentACClassMethod;
            CurrentACClass.AddNewACClassMethod(acClassMethod);

            OnPropertyChanged("ConfigACClassMethodList");
            PostExecute("NewConfigACClassMethod");
            CurrentConfigACClassMethod = acClassMethod;
        }

        /// <summary>
        /// Determines whether [is enabled new config AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new config AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewConfigACClassMethod()
        {
            if (CurrentACClassMethod == null)
                return false;
            return CurrentACClassMethod.ACKind == Global.ACKinds.MSMethod || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodClient || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodPrePost || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExt;
        }

        /// <summary>
        /// Deletes the config AC class method.
        /// </summary>
        [ACMethodInfo("ConfigACClassMethod", "en{'Delete Configuration'}de{'Konfiguration löschen'}", (short)MISort.Delete, true)]
        public void DeleteConfigACClassMethod()
        {
            if (!PreExecute("DeleteConfigACClassMethod")) return;
            Msg msg = CurrentConfigACClassMethod.DeleteACObject(Database.ContextIPlus, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            OnPropertyChanged("ConfigACClassMethodList");
            PostExecute("DeleteConfigACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled delete config AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete config AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteConfigACClassMethod()
        {
            return CurrentConfigACClassMethod != null && CurrentConfigACClassMethod.ACClassMethod1_ParentACClassMethod != null && CurrentConfigACClassMethod.ACKind != Global.ACKinds.MSMethod;
        }
        #endregion

        #endregion

        #region BSO->ACProperty

        #region ACProject (abstract)

        /// <summary>
        /// Gets or sets the current AC project.
        /// </summary>
        /// <value>The current AC project.</value>
        public abstract ACProject CurrentACProject { get; set; }

        /// <summary>
        /// Gets or sets the AC project list.
        /// </summary>
        /// <value>The AC project list.</value>
        public abstract IEnumerable<ACProject> ACProjectList { get; }
        #endregion

        #region ACClass (abstract)
        /// <summary>
        /// Gets the access AC class.
        /// </summary>
        /// <value>The access AC class.</value>
        public abstract ACAccess<ACClass> AccessACClass { get; }

        /// <summary>
        /// Gets or sets the current AC class.
        /// </summary>
        /// <value>The current AC class.</value>
        public abstract ACClass CurrentACClass { get; set; }
        #endregion

        #region ACClassMethod (abstract)
        /// <summary>
        /// Gets the access AC class method.
        /// </summary>
        /// <value>The access AC class method.</value>
        public abstract ACAccess<ACClassMethod> AccessACClassMethod { get; }

        /// <summary>
        /// Gets or sets the current AC class method.
        /// </summary>
        /// <value>The current AC class method.</value>
        public abstract ACClassMethod CurrentACClassMethod { get; set; }

        /// <summary>
        /// Gets or sets the selected AC class method.
        /// </summary>
        /// <value>The selected AC class method.</value>
        public abstract ACClassMethod SelectedACClassMethod { get; set; }

        /// <summary>
        /// The _ AC class method list
        /// </summary>
        protected List<ACClassMethod> _ACClassMethodList = null;
        /// <summary>
        /// Gets the AC class method list.
        /// </summary>
        /// <value>The AC class method list.</value>
        public abstract IEnumerable<ACClassMethod> ACClassMethodList { get; }

        /// <summary>
        /// The _ current AC class method control
        /// </summary>
        protected ACClassDesign _CurrentACClassMethodControl;
        /// <summary>
        /// Gets or sets the current AC class method control.
        /// </summary>
        /// <value>The current AC class method control.</value>
        [ACPropertyCurrent(9999, "ACClassMethodControl", "en{'Control'}de{'Steuerelement'}")]
        public ACClassDesign CurrentACClassMethodControl
        {
            get
            {
                return _CurrentACClassMethodControl;
            }
            set
            {
                _CurrentACClassMethodControl = value;
                ACProjectManager.SetDefaultMethodACClassDesign(Database.ContextIPlus, CurrentACClassMethod, value, Global.ACKinds.DSDesignLayout, Global.ACUsages.DUControl);
                OnPropertyChanged("CurrentACClassMethodControl");
            }
        }

        /// <summary>
        /// The _ AC class method control list
        /// </summary>
        protected List<ACClassDesign> _ACClassMethodControlList = null;
        /// <summary>
        /// Gets the AC class method control list.
        /// </summary>
        /// <value>The AC class method control list.</value>
        [ACPropertyList(9999, "ACClassMethodControl")]
        public IEnumerable<ACClassDesign> ACClassMethodControlList
        {
            get
            {
                if (CurrentACClass == null || CurrentACClassMethod == null)
                    return null;
                if (_ACClassMethodControlList == null)
                {
                    var myACClassDesignList = CurrentACClassMethod.Designs;
                    if (myACClassDesignList != null)
                        _ACClassMethodControlList = myACClassDesignList.Where(c => c.ACUsageIndex == (int)Global.ACUsages.DUControl).ToList();
                }
                return _ACClassMethodControlList;
            }
        }

        /// <summary>
        /// The _ current AC class method icon
        /// </summary>
        protected ACClassDesign _CurrentACClassMethodIcon;
        /// <summary>
        /// Gets or sets the current AC class method icon.
        /// </summary>
        /// <value>The current AC class method icon.</value>
        [ACPropertyCurrent(9999, "ACClassMethodIcon", "en{'Icon'}de{'Symbol'}")]
        public ACClassDesign CurrentACClassMethodIcon
        {
            get
            {
                return _CurrentACClassMethodIcon;
            }
            set
            {
                _CurrentACClassMethodIcon = value;
                ACProjectManager.SetDefaultMethodACClassDesign(Database.ContextIPlus, CurrentACClassMethod, value, Global.ACKinds.DSBitmapResource, Global.ACUsages.DUIcon);
                OnPropertyChanged("CurrentACClassMethodIcon");
            }
        }

        /// <summary>
        /// The _ AC class method icon list
        /// </summary>
        protected List<ACClassDesign> _ACClassMethodIconList = null;
        /// <summary>
        /// Gets the AC class method icon list.
        /// </summary>
        /// <value>The AC class method icon list.</value>
        [ACPropertyList(9999, "ACClassMethodIcon")]
        public IEnumerable<ACClassDesign> ACClassMethodIconList
        {
            get
            {
                if (CurrentACClass == null || CurrentACClassMethod == null)
                    return null;
                if (_ACClassMethodIconList == null)
                {
                    var myACClassDesignList = CurrentACClass.Designs;
                    if (myACClassDesignList != null)
                        _ACClassMethodIconList = myACClassDesignList.Where(c => c.ACUsageIndex == (int)Global.ACUsages.DUIcon).ToList();
                    //else
                    //    _ACClassMethodIconList = new List<ACClassDesign>();
                    //var query = Root.Environment.ACType.MyACClassDesignList.Where(c => c.ACUsageIndex == (int)Global.ACUsages.DUIcon);
                    //foreach (var acClassDesign in query)
                    //{
                    //    _ACClassMethodIconList.Add(acClassDesign);
                    //}
                }
                return _ACClassMethodIconList;
            }
        }

        /// <summary>
        /// The _ current new AC class method
        /// </summary>
        ACClassMethod _CurrentNewACClassMethod;
        /// <summary>
        /// Gets or sets the current new AC class method.
        /// </summary>
        /// <value>The current new AC class method.</value>
        [ACPropertyCurrent(9999, "ACClassMethod")]
        public ACClassMethod CurrentNewACClassMethod
        {
            get
            {
                return _CurrentNewACClassMethod;
            }
            set
            {
                _CurrentNewACClassMethod = value;
                OnPropertyChanged("CurrentNewACClassMethod");
            }
        }

        ACClass _CurrentNewWFRootACClass;
        [ACPropertyCurrent(9999, "NewWFRootACClass", "en{'Instantiation class of root node'}de{'Zu instanziierende Klasse des Wurzelknotens'}")]
        public ACClass CurrentNewWFRootACClass
        {
            get
            {
                return _CurrentNewWFRootACClass;
            }
            set
            {
                _CurrentNewWFRootACClass = value;
                OnPropertyChanged("CurrentNewWFRootACClass");
            }
        }

        [ACPropertyList(9999, "NewWFRootACClass")]
        public IEnumerable<ACClass> NewMethodACClassList
        {
            get
            {
                return this.Database.ContextIPlus.WorkflowTypeMethodACClassList;
            }
        }


        ACClass _CurrentNewInvokingACClass;
        [ACPropertyCurrent(9999, "NewInvokingACClass", "en{'Class which invokes this subworkflow'}de{'Klasse die den Unterworkflow aufrufen soll'}")]
        public ACClass CurrentNewInvokingACClass
        {
            get
            {
                return _CurrentNewInvokingACClass;
            }
            set
            {
                _CurrentNewInvokingACClass = value;
                OnPropertyChanged("CurrentNewInvokingACClass");
            }
        }

        [ACPropertyList(9999, "NewInvokingACClass")]
        public IEnumerable<ACClass> NewInvokingACClassList
        {
            get
            {
                return this.Database.ContextIPlus.WorkflowInvokerACClassList;
            }
        }


        #endregion

        #region 1.1.1.1 ConfigACClassMethod
        /// <summary>
        /// The _ current config AC class method
        /// </summary>
        ACClassMethod _CurrentConfigACClassMethod;
        /// <summary>
        /// Gets or sets the current config AC class method.
        /// </summary>
        /// <value>The current config AC class method.</value>
        [ACPropertyCurrent(9999, "ConfigACClassMethod")]
        public ACClassMethod CurrentConfigACClassMethod
        {
            get
            {
                return _CurrentConfigACClassMethod;
            }
            set
            {
                if (_CurrentConfigACClassMethod != value)
                {
                    _CurrentConfigACClassMethod = value;
                    OnPropertyChanged("CurrentConfigACClassMethod");
                    if (_CurrentConfigACClassMethod == null)
                    {
                        _CurrentACMethod = null;
                    }
                    else
                    {
                        _CurrentACMethod = _CurrentConfigACClassMethod.ACMethod;
                    }
                    OnPropertyChanged("CurrentACMethod");
                    OnPropertyChanged("ACMethodValueList");
                    OnPropertyChanged("ACMethodResultValueList");
                }
            }
        }

        /// <summary>
        /// The _ config AC class method list
        /// </summary>
        List<ACClassMethod> _ConfigACClassMethodList = null;
        /// <summary>
        /// Gets the config AC class method list.
        /// </summary>
        /// <value>The config AC class method list.</value>
        [ACPropertyList(9999, "ConfigACClassMethod")]
        public IEnumerable<ACClassMethod> ConfigACClassMethodList
        {
            get
            {
                if (CurrentACClassMethod == null)
                    return null;
                if (CurrentACClassMethod.ACKind != Global.ACKinds.MSMethod &&
                    CurrentACClassMethod.ACKind != Global.ACKinds.MSMethodClient &&
                    CurrentACClassMethod.ACKind != Global.ACKinds.MSMethodPrePost &&
                    CurrentACClassMethod.ACKind != Global.ACKinds.MSMethodExt &&
                    CurrentACClassMethod.ACKind != Global.ACKinds.MSWorkflow)
                    return null;
                _ConfigACClassMethodList = new List<ACClassMethod>();
                _ConfigACClassMethodList.Add(CurrentACClassMethod);

                var query = CurrentACClass.ACClassMethod_ACClass.Where(c => c.ParentACClassMethodID == CurrentACClassMethod.ACClassMethodID).OrderBy(c => c.ACIdentifier);
                foreach (var acClassMethod in query)
                {
                    _ConfigACClassMethodList.Add(acClassMethod);
                }
                return _ConfigACClassMethodList;
            }
        }
        #endregion

        #region 1.1.1.1.1 ACMethod
        /// <summary>
        /// The _ method mode
        /// </summary>
        Global.MethodModes _MethodMode = Global.MethodModes.States;

        /// <summary>
        /// Gets or sets the current method mode enum.
        /// </summary>
        /// <value>The current method mode enum.</value>
        public Global.MethodModes CurrentMethodModeEnum
        {
            get
            {
                return _MethodMode;
            }
            set
            {
                if (_MethodMode != value)
                {
                    _MethodMode = value;
                    //switch (_MethodMode)
                    //{
                    //    case Global.MethodModes.Methods:
                    //        AccessACClassMethod.NavACQueryDefinition.ClearFilter();
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACGroup, Global.LogicalOperators.notEqual, Global.Operators.and, Const.ACState, false));
                    //        AccessACClassMethod.NavACQueryDefinition.ClearSort(true);
                    //        AccessACClassMethod.NavACQueryDefinition.ACSortColumns.Add(new ACSortItem(Const.ACIdentifierPrefix, Global.SortDirections.ascending, false));
                    //        break;
                    //    case Global.MethodModes.Assemblymethods:
                    //        AccessACClassMethod.NavACQueryDefinition.ClearFilter();
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACGroup, Global.LogicalOperators.notEqual, Global.Operators.and, Const.ACState, false));
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, ((Int16)Global.ACKinds.MSMethod).ToString(), false));
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, ((Int16)Global.ACKinds.MSMethodClient).ToString(), false));
                    //        AccessACClassMethod.NavACQueryDefinition.ClearSort(true);
                    //        AccessACClassMethod.NavACQueryDefinition.ACSortColumns.Add(new ACSortItem(Const.ACIdentifierPrefix, Global.SortDirections.ascending, false));
                    //        break;
                    //    case Global.MethodModes.Scriptmethods:
                    //        AccessACClassMethod.NavACQueryDefinition.ClearFilter();
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACGroup, Global.LogicalOperators.notEqual, Global.Operators.and, Const.ACState, false));
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.parenthesisOpen, null, Global.LogicalOperators.none, Global.Operators.and, null, false));
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, ((Int16)Global.ACKinds.MSMethodExt).ToString(), false));
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, ((Int16)Global.ACKinds.MSMethodExtClient).ToString(), false));
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, ((Int16)Global.ACKinds.MSMethodExtTrigger).ToString(), false));
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.parenthesisClose, null, Global.LogicalOperators.none, Global.Operators.and, null, false));
                    //        AccessACClassMethod.NavACQueryDefinition.ClearSort(true);
                    //        AccessACClassMethod.NavACQueryDefinition.ACSortColumns.Add(new ACSortItem(Const.ACIdentifierPrefix, Global.SortDirections.ascending, false));
                    //        break;
                    //    case Global.MethodModes.Workflows:
                    //        AccessACClassMethod.NavACQueryDefinition.ClearFilter();
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACGroup, Global.LogicalOperators.notEqual, Global.Operators.and, Const.ACState, false));
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, ((Int16)Global.ACKinds.MSWorkflow).ToString(), false));
                    //        AccessACClassMethod.NavACQueryDefinition.ClearSort(true);
                    //        AccessACClassMethod.NavACQueryDefinition.ACSortColumns.Add(new ACSortItem(Const.ACIdentifierPrefix, Global.SortDirections.ascending, false));
                    //        break;
                    //    case Global.MethodModes.Submethod:
                    //        AccessACClassMethod.NavACQueryDefinition.ClearFilter();
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACGroup, Global.LogicalOperators.notEqual, Global.Operators.and, Const.ACState, false));
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, ((Int16)Global.ACKinds.MSMethodFunction).ToString(), false));
                    //        AccessACClassMethod.NavACQueryDefinition.ClearSort(true);
                    //        AccessACClassMethod.NavACQueryDefinition.ACSortColumns.Add(new ACSortItem(Const.ACIdentifierPrefix, Global.SortDirections.ascending, false));
                    //        break;
                    //    case Global.MethodModes.States:
                    //        AccessACClassMethod.NavACQueryDefinition.ClearFilter();
                    //        AccessACClassMethod.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACGroup, Global.LogicalOperators.equal, Global.Operators.and, Const.ACState, false));
                    //        AccessACClassMethod.NavACQueryDefinition.ClearSort(true);
                    //        AccessACClassMethod.NavACQueryDefinition.ACSortColumns.Add(new ACSortItem("SortIndex", Global.SortDirections.ascending, false));
                    //        break;
                    //}

                    BroadcastToVBControls(Const.CmdUpdateControlMode, "CurrentACClassMethod");
                    BroadcastToVBControls(Const.CmdUpdateVBContent, "CurrentACClassMethod");
                    //OnPropertyChanged("ACQueryDefinition:CurrentACClassMethod");
                    OnPropertyChanged("CurrentMethodMode");
                    _ACClassMethodList = null;
                    OnPropertyChanged("ACClassMethodList");
                    OnPropertyChanged("CurrentMethodLayout");
                    OnPropertyChanged("IsMethodInfoVisible");
                    OnPropertyChanged("IsScriptEditorVisible");
                    OnPropertyChanged("IsWFEditorVisible");
                }
            }
        }

        /// <summary>
        /// The _ current method mode
        /// </summary>
        ACValueItem _CurrentMethodMode;
        /// <summary>
        /// Gets or sets the current method mode.
        /// </summary>
        /// <value>The current method mode.</value>
        [ACPropertyCurrent(9999, "MethodMode", "en{'MethodMode'}de{'Methodenmodus'}", "", false)]
        public ACValueItem CurrentMethodMode
        {
            get
            {
                return _CurrentMethodMode;
            }
            set
            {
                if (_CurrentMethodMode != value)
                {
                    _CurrentMethodMode = value;
                    if (value != null)
                    {
                        Int16 v = (Int16)value.Value;
                        CurrentMethodModeEnum = (Global.MethodModes)v;
                    }
                    OnPropertyChanged("CurrentMethodMode");
                }
            }
        }


        /// <summary>
        /// The _ method mode list
        /// </summary>
        protected ACValueItemList _MethodModeList = null;
        /// <summary>
        /// Gets the method mode list.
        /// </summary>
        /// <value>The method mode list.</value>
        [ACPropertyList(9999, "MethodMode")]
        public IEnumerable<ACValueItem> MethodModeList
        {
            get
            {
                if (_MethodModeList == null)
                {
                    _MethodModeList = new ACValueItemList(typeof(Global.MethodModes).Name);
                    _MethodModeList.AddEntry((short)Global.MethodModes.Methods, Global.MethodModes.Methods.ToString());
                    _MethodModeList.AddEntry((short)Global.MethodModes.Assemblymethods, Global.MethodModes.Assemblymethods.ToString());
                    _MethodModeList.AddEntry((short)Global.MethodModes.Scriptmethods, Global.MethodModes.Scriptmethods.ToString());

                    if (CurrentACProject.IsWorkflowEnabled)
                    {
                        _MethodModeList.AddEntry((short)Global.MethodModes.Workflows, Global.MethodModes.Workflows.ToString());
                    }
                    if (CurrentACProject.ACProjectType == Global.ACProjectTypes.AppDefinition 
                        || CurrentACProject.ACProjectType == Global.ACProjectTypes.Application 
                        || CurrentACProject.ACProjectType == Global.ACProjectTypes.Service)
                    {
                        _MethodModeList.AddEntry((short)Global.MethodModes.Submethod, Global.MethodModes.Submethod.ToString());
                    }
                    _MethodModeList.AddEntry((short)Global.MethodModes.States, Global.MethodModes.States.ToString());
                }
                return _MethodModeList;
            }
        }

        /// <summary>
        /// The _ current AC method
        /// </summary>
        ACMethod _CurrentACMethod = null;
        /// <summary>
        /// Gets the current AC method.
        /// </summary>
        /// <value>The current AC method.</value>
        [ACPropertyInfo(9999, "", "en{'Methodinfo'}de{'Methodeninfo'}")]
        public ACMethod CurrentACMethod
        {
            get
            {
                if (CurrentConfigACClassMethod == null)
                    return null;
                if (CurrentConfigACClassMethod.ACKind != Global.ACKinds.MSMethod &&
                    CurrentConfigACClassMethod.ACKind != Global.ACKinds.MSMethodClient &&
                    CurrentConfigACClassMethod.ACKind != Global.ACKinds.MSMethodPrePost &&
                    CurrentConfigACClassMethod.ACKind != Global.ACKinds.MSMethodExt &&
                    CurrentConfigACClassMethod.ACKind != Global.ACKinds.MSWorkflow)
                    return null;
                return CurrentConfigACClassMethod.ACMethod;
            }
        }

        /// <summary>
        /// The _ current AC method value
        /// </summary>
        ACValue _CurrentACMethodValue;
        /// <summary>
        /// Gets or sets the current AC method value.
        /// </summary>
        /// <value>The current AC method value.</value>
        [ACPropertyCurrent(9999, "ACMethodValue")]
        public ACValue CurrentACMethodValue
        {
            get
            {
                return _CurrentACMethodValue;
            }
            set
            {
                if (_CurrentACMethodValue != value)
                {
                    _CurrentACMethodValue = value;
                    OnPropertyChanged("CurrentACMethodValue");
                }
            }
        }

        /// <summary>
        /// Gets the AC method value list.
        /// </summary>
        /// <value>The AC method value list.</value>
        [ACPropertyList(9999, "ACMethodValue")]
        public IEnumerable<ACValue> ACMethodValueList
        {
            get
            {
                if (CurrentConfigACClassMethod == null || CurrentConfigACClassMethod.ACMethod == null)
                    return null;
                return CurrentConfigACClassMethod.ACMethod.ParameterValueList;
                //return CurrentConfigACClassMethod.ACMethod.ParameterValueList.Where(c => c is ACValue).Select(c => c as ACValue).ToList();
            }
        }

        /// <summary>
        /// The _ current AC method result value
        /// </summary>
        ACValue _CurrentACMethodResultValue;
        /// <summary>
        /// Gets or sets the current AC method result value.
        /// </summary>
        /// <value>The current AC method result value.</value>
        [ACPropertyCurrent(9999, "ACMethodResult")]
        public ACValue CurrentACMethodResultValue
        {
            get
            {
                return _CurrentACMethodResultValue;
            }
            set
            {
                if (_CurrentACMethodResultValue != value)
                {
                    _CurrentACMethodResultValue = value;
                    OnPropertyChanged("CurrentACMethodResultValue");
                }
            }
        }

        /// <summary>
        /// Gets the AC method result value list.
        /// </summary>
        /// <value>The AC method result value list.</value>
        [ACPropertyList(9999, "ACMethodResult")]
        public IEnumerable<ACValue> ACMethodResultValueList
        {
            get
            {
                if (CurrentConfigACClassMethod == null || CurrentConfigACClassMethod.ACMethod == null)
                    return null;
                return CurrentConfigACClassMethod.ACMethod.ResultValueList.Where(c => c is ACValue).Select(c => c as ACValue);
            }
        }

        /// <summary>
        /// News the work AC class method OK.
        /// </summary>
        [ACMethodCommand("NewWorkACClassMethod", Const.Ok, (short)MISort.Okay)]
        public virtual void NewWorkACClassMethodOK()
        {
            CloseTopDialog();

            if (!IsEnabledNewWorkACClassMethodOK())
                return;

            //acClassOfPWRootNode = Database.ContextIPlus.ACClass.Where(c => c.ACIdentifier == Const.ACClassIdentifierOfPWMethod).First();

            //if (CurrentNewACClassMethod.IsSubMethod)
            //    acClassOfInvoker = Database.ContextIPlus.ACClass.Where(c => c.ACIdentifier == Const.ACClassIdentifierOfPWNodeProcessWorkflow).First();
            //else
            //    acClassOfInvoker = acClassOfPWRootNode;

            CurrentNewACClassMethod.PWACClass = CurrentNewInvokingACClass;
            VBDesignerWorkflowMethod.DoInsertRoot(CurrentNewACClassMethod, CurrentNewWFRootACClass);

            CurrentNewACClassMethod.UpdateParamListFromACClassConstructor(CurrentNewWFRootACClass);

            CurrentACClass.AddNewACClassMethod(CurrentNewACClassMethod);
            Database.ContextIPlus.ACClassMethod.Add(CurrentNewACClassMethod);

            _ACClassMethodList = null;
            OnPropertyChanged("ACClassMethodList");

            SelectedACClassMethod = CurrentNewACClassMethod;
            CurrentACClassMethod = CurrentNewACClassMethod;

            CurrentNewACClassMethod = null;
        }

        /// <summary>
        /// Determines whether [is enabled new work AC class method OK].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new work AC class method OK]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewWorkACClassMethodOK()
        {
            return CurrentNewACClassMethod != null 
                && CurrentNewWFRootACClass != null 
                && CurrentNewInvokingACClass != null
                && VBDesignerWorkflowMethod != null;
        }

        /// <summary>
        /// News the work AC class method cancel.
        /// </summary>
        [ACMethodCommand("NewWorkACClassMethod", Const.Cancel, (short)MISort.Cancel)]
        public void NewWorkACClassMethodCancel()
        {
            CloseTopDialog();
            CurrentNewACClassMethod = null;
        }


        [ACMethodCommand("ACClassMethod", "en{'Change instantiation class of root node'}de{'Ändere zu instanziierende Klasse des Wurzelknotens'}", 130)]
        public void ChangeRootWFClass()
        {
            if (!IsEnabledChangeRootWFClass())
                return;
            ShowDialog(this, "ChangeRootWFClass");
        }

        public bool IsEnabledChangeRootWFClass()
        {
            return CurrentACClassMethod != null && CurrentACClassMethod.ACClassWF_ACClassMethod.Any();
        }

        [ACMethodCommand("ChangeRootWFClass", Const.Ok, (short)MISort.Okay)]
        public void ChangeRootWFClassOK()
        {
            CloseTopDialog();
            if (!IsEnabledChangeRootWFClassOK())
                return;
            ACClassWF rootNode = CurrentACClassMethod.ACClassWF_ACClassMethod.Where(c => !c.ParentACClassWFID.HasValue).FirstOrDefault();
            if (rootNode != null)
            {
                rootNode.PWACClass = this.CurrentNewWFRootACClass;
            }
        }


        public bool IsEnabledChangeRootWFClassOK()
        {
            return CurrentACClassMethod != null && CurrentACClassMethod.ACClassWF_ACClassMethod.Any() && this.CurrentNewWFRootACClass != null;
        }

        #endregion

        #endregion

        #region Layoutsteuerung
        /// <summary>
        /// Gets the current method layout.
        /// </summary>
        /// <value>The current method layout.</value>
        public string CurrentMethodLayout
        {
            get
            {
                if (CurrentACClassMethod == null)
                    return null;

                string layoutXAML = LayoutHelper.DockingManagerBegin("tabControl1", "Padding=\"0\"");

                //if (CurrentACClassMethod.ACKind == Global.ACKinds.MSMethod
                //    || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodClient
                //    || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodPrePost
                //    || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExt
                //    || CurrentACClassMethod.ACKind == Global.ACKinds.MSWorkflow)
                //{
                //    layoutXAML += LayoutHelper.DockingManagerAdd("*ACMethodConfig", "ACMethodConfig_0");
                //}

                switch (CurrentACClassMethod.ACKind)
                {
                    case Global.ACKinds.MSMethod:
                    case Global.ACKinds.MSMethodClient:
                    case Global.ACKinds.MSMethodPrePost:
                        layoutXAML += LayoutHelper.DockingManagerAdd("*ACMethodConfig", "ACMethodConfig_0");
                        break;
                    case Global.ACKinds.MSMethodExt:
                    case Global.ACKinds.MSMethodExtClient:
                        layoutXAML += LayoutHelper.DockingManagerAdd("*TabACMethodScript", "TabACMethodScript_0");
                        layoutXAML += LayoutHelper.DockingManagerAdd("*ACMethodConfig", "ACMethodConfig_0");
                        break;
                    case Global.ACKinds.MSMethodExtTrigger:
                        layoutXAML += LayoutHelper.DockingManagerAdd("*TabACMethodScript", "TabACMethodScript_0");
                        break;
                    case Global.ACKinds.MSWorkflow:
                        layoutXAML += LayoutHelper.DockingManagerAdd("*TabACMethodWorkflow", "TabACMethodWorkflow_0");
                        layoutXAML += LayoutHelper.DockingManagerAdd("*ACMethodConfig", "ACMethodConfig_0");
                        break;
                }

                layoutXAML += LayoutHelper.DockingManagerEnd();

                return layoutXAML;
            }
        }

        [ACPropertyInfo(9999)]
        public bool IsMethodInfoVisible
        {
            get
            {
                if (CurrentACClassMethod == null)
                    return false;
                return CurrentACClassMethod.ACKind == Global.ACKinds.MSMethod
                    || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodClient
                    || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodPrePost
                    || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExt
                    || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExtClient
                    || CurrentACClassMethod.ACKind == Global.ACKinds.MSWorkflow;
            }
        }

        [ACPropertyInfo(9999)]
        public bool IsScriptEditorVisible
        {
            get
            {
                if (CurrentACClassMethod == null)
                    return false;
                return CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExt
                        || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExtTrigger
                        || CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExtClient;
            }
        }

        [ACPropertyInfo(9999)]
        public bool IsWFEditorVisible
        {
            get
            {
                if (CurrentACClassMethod == null)
                    return false;
                return CurrentACClassMethod.ACKind == Global.ACKinds.MSWorkflow;
            }
        }
        #endregion

        #region Baustelle

        private ACComponent _VBPresenterMethod;
        public ACComponent VBPresenterMethod
        {
            get
            {
                if(_VBPresenterMethod == null)
                {
                     _VBPresenterMethod = this.ACUrlCommand("VBPresenterMethod(CurrentDesign)") as ACComponent;
                    (_VBPresenterMethod as VBPresenterMethod).PropertyChanged += BSOiPlusBase_PropertyChanged;
                }
                return _VBPresenterMethod;
            }
        }

        void BSOiPlusBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedACUrl")
            {
                OnPropertyChanged("SelectedACUrl");
            }
        }

        /// <summary>
        /// The _ VB designer workflow method
        /// </summary>
        VBDesignerWorkflowMethod _VBDesignerWorkflowMethod;
        /// <summary>
        /// Gets the VB designer workflow method.
        /// </summary>
        /// <value>The VB designer workflow method.</value>
        public VBDesignerWorkflowMethod VBDesignerWorkflowMethod
        {
            get
            {
                if (_VBDesignerWorkflowMethod == null)
                {
                    _VBDesignerWorkflowMethod = VBPresenterMethod.ACUrlCommand("VBDesignerWorkflowMethod(CurrentDesign)") as VBDesignerWorkflowMethod;
                }
                return _VBDesignerWorkflowMethod;
            }
        }
        #endregion

        #region VBDesignerWorkflowMethod Forward Up

        #region VBDesignerWorkflowMethod Forward Up -> DetailWorkflow
        [ACMethodInteraction("WF", "en{'Details'}de{'Details'}", 200, true)]
        public void ShowDetail()
        {
            if (!IsEnabledShowDetail())
                return;

            PWOfflineNodeMethod pwObjectNode = VBDesignerWorkflowMethod.CurrentContentACObject as PWOfflineNodeMethod;

            this.ParentACComponent.ACUrlCommand("!LoadDetail", pwObjectNode);
        }

        public bool IsEnabledShowDetail()
        {
            if (VBDesignerWorkflowMethod == null)
                return false;
            PWOfflineNodeMethod pwObjectNode = VBDesignerWorkflowMethod.CurrentContentACObject as PWOfflineNodeMethod;

            if (pwObjectNode == null)
                return false;
            if (pwObjectNode.ContentACClassWF.RefPAACClassMethod == null)
                return false;
            if (pwObjectNode.ContentACClassWF.RefPAACClassMethod.ACKind != Global.ACKinds.MSWorkflow)
                return false;

            return true;
        }
        #endregion


        #region VBDesignerWorkflowMethod Forward Up -> Hide Detail Workflow

        [ACMethodInteraction("Workflow", "en{'Back'}de{'Zurück'}", 9999, false)]
        public void HideDetail()
        {
            var parentRootWorkflowNode = (VBPresenterMethod as VBPresenterMethod).SelectedRootWFNode.ParentRootWFNode;
            if (parentRootWorkflowNode != null)
            {
                parentRootWorkflowNode.StopComponent((VBPresenterMethod as VBPresenterMethod).SelectedRootWFNode);
                (VBPresenterMethod as VBPresenterMethod).SelectedRootWFNode = parentRootWorkflowNode;
            }
            ParentACComponent.ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.Refresh));
        }

        public bool IsEnabledHideDetail()
        {
            if ((VBPresenterMethod as VBPresenterMethod).SelectedRootWFNode == null)
                return false;
            return (VBPresenterMethod as VBPresenterMethod).SelectedRootWFNode.ParentRootWFNode != null;
        }

        #endregion

        [ACPropertyInfo(9999)]
        public string SelectedACUrl
        {
            get
            {
                return (VBPresenterMethod as VBPresenterMethod).SelectedACUrl;
            }
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "NewConfigACClassMethod":
                    NewConfigACClassMethod();
                    return true;
                case "IsEnabledNewConfigACClassMethod":
                    result = IsEnabledNewConfigACClassMethod();
                    return true;
                case "DeleteConfigACClassMethod":
                    DeleteConfigACClassMethod();
                    return true;
                case "IsEnabledDeleteConfigACClassMethod":
                    result = IsEnabledDeleteConfigACClassMethod();
                    return true;
                case "NewWorkACClassMethodOK":
                    NewWorkACClassMethodOK();
                    return true;
                case "IsEnabledNewWorkACClassMethodOK":
                    result = IsEnabledNewWorkACClassMethodOK();
                    return true;
                case "NewWorkACClassMethodCancel":
                    NewWorkACClassMethodCancel();
                    return true;
                case "ChangeRootWFClass":
                    ChangeRootWFClass();
                    return true;
                case "IsEnabledChangeRootWFClass":
                    result = IsEnabledChangeRootWFClass();
                    return true;
                case "ChangeRootWFClassOK":
                    ChangeRootWFClassOK();
                    return true;
                case "IsEnabledChangeRootWFClassOK":
                    result = IsEnabledChangeRootWFClassOK();
                    return true;
                case "ShowDetail":
                    ShowDetail();
                    return true;
                case "IsEnabledShowDetail":
                    result = IsEnabledShowDetail();
                    return true;
                case "HideDetail":
                    HideDetail();
                    return true;
                case "IsEnabledHideDetail":
                    result = IsEnabledHideDetail();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}

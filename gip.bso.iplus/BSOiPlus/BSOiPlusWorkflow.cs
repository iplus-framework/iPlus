// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-09-2012
// ***********************************************************************
// <copyright file="BSOiPlusWorkflow.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.manager;
using System.Threading;
using System.ComponentModel;
using System.Data;

namespace gip.bso.iplus
{
    /// <summary>
    /// Bearbeitung von Workflows auf Anwendungsdefinitionsebene
    /// Hiermit arbeitet der Anwender um "Steuerrezepte" zu erstellen.
    /// Die entsprechenden Workflows können auch im Rahmen der BSOiPlusStudio bearbeitet werden.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Workflow'}de{'Workflow'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACProject.ClassName)]
    public class BSOiPlusWorkflow : BSOiPlusBase, IACBSOConfigStoreSelection
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="BSOiPlusBase" /> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOiPlusWorkflow(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._AccessACClass = null;
            var b = base.ACDeInit(deleteACClassTask);
            if (_AccessPrimary != null)
            {
                _AccessPrimary.ACDeInit(false);
                _AccessPrimary = null;
            }
            return b;
        }

        /// <summary>
        /// ACs the post init.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACPostInit()
        {
            if (!base.ACPostInit())
                return false;
            AccessPrimary.NavSearch(Database.ContextIPlus);

            if (ACProjectList.Any())
            {
                CurrentACProject = ACProjectList.First();
            }
            return true;
        }
        #endregion

        #region BSO->ACProperty
        #region ACProject
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
                    AccessPrimary.Current = value;

                    var query = CurrentACProject.ACClass_ACProject.Where(c => c.ParentACClassID == null);
                    CurrentACClass = query.First();
                    OnPropertyChanged("CurrentACProject");
                    //Search();
                }
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
                return AccessPrimary.NavList.Where(c => c.ACProjectTypeIndex == (Int16)Global.ACProjectTypes.AppDefinition && c.IsWorkflowEnabled).OrderBy(c => c.ACProjectName).ToList();
            }
        }
        #endregion

        #region ACClass
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
                    _CurrentACClass = value;
                    _ACClassMethodList = null;
                    _CurrentACClassMethod = null;

                    OnPropertyChanged("CurrentACClass");
                    OnPropertyChanged("ACClassMethodList");

                    if (ACClassMethodList != null && ACClassMethodList.Any())
                    {
                        CurrentACClassMethod = ACClassMethodList.First();
                    }
                    else
                    {
                        CurrentACClassMethod = null;
                    }
                }
            }
        }
        #endregion

        #region ACClassMethod
        /// <summary>
        /// The _ access AC class method
        /// </summary>
        ACAccess<ACClassMethod> _AccessACClassMethod;
        /// <summary>
        /// Gets the access AC class method.
        /// </summary>
        /// <value>The access AC class method.</value>
        [ACPropertyAccess(9999, "ACClassMethod")]
        public override ACAccess<ACClassMethod> AccessACClassMethod
        {
            get
            {
                if (_AccessACClassMethod == null && ACType != null)
                {
                    ACQueryDefinition acQueryDefinition = AccessACClass.NavACQueryDefinition.ACUrlCommand(Const.QueryPrefix + ACClassMethod.ClassName) as ACQueryDefinition;
                    _AccessACClassMethod = acQueryDefinition.NewAccess<ACClassMethod>(ACClassMethod.ClassName, this);
                }
                return _AccessACClassMethod;
            }
        }

        /// <summary>
        /// The _ current AC class method
        /// </summary>
        ACClassMethod _CurrentACClassMethod;
        /// <summary>
        /// Gets or sets the current AC class method.
        /// </summary>
        /// <value>The current AC class method.</value>
        [ACPropertyCurrent(9999, "ACClassMethod")]
        public override ACClassMethod CurrentACClassMethod
        {
            get
            {
                return _CurrentACClassMethod;
            }
            set
            {
                _CurrentACClassMethod = value;

                VBPresenterMethod vbPresenterMethod = this.ACUrlCommand("VBPresenterMethod(CurrentDesign)") as VBPresenterMethod;
                if (vbPresenterMethod == null)
                {
                    Messages.Error(this, "This user has no rights for viewing workflows. Assign rights for VBPresenterMethod in the group management!", true);
                    return;
                }
                vbPresenterMethod.Load(value);

                CurrentWorkflowLive = null;
                CurrentPWRootLive = null;

                _ACClassMethodControlList = null;
                _ACClassMethodIconList = null;
                if (value == null)
                {
                    _CurrentACClassMethodIcon = null;
                    _CurrentACClassMethodControl = null;
                }
                else
                {
                    _CurrentACClassMethodIcon = _CurrentACClassMethod.GetDesign(_CurrentACClassMethod, Global.ACUsages.DUIcon, Global.ACKinds.DSBitmapResource);
                    _CurrentACClassMethodControl = _CurrentACClassMethod.GetDesign(_CurrentACClassMethod, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                }


                OnPropertyChanged("CurrentACClassMethod");
                OnPropertyChanged("CurrentPWRoot");
                OnPropertyChanged("CurrentMethodLayout");
                OnPropertyChanged("IsMethodInfoVisible");
                OnPropertyChanged("IsScriptEditorVisible");
                OnPropertyChanged("IsWFEditorVisible");
                OnPropertyChanged("CurrentACClassMethodControl");
                OnPropertyChanged("CurrentACClassMethodIcon");
                OnPropertyChanged("ACClassMethodControlList");
                OnPropertyChanged("ACClassMethodIconList");
                OnPropertyChanged("ConfigACClassMethodList");
            }
        }

        /// <summary>
        /// The _ selected AC class method
        /// </summary>
        ACClassMethod _SelectedACClassMethod;
        /// <summary>
        /// Gets or sets the selected AC class method.
        /// </summary>
        /// <value>The selected AC class method.</value>
        [ACPropertySelected(9999, "ACClassMethod")]
        public override ACClassMethod SelectedACClassMethod
        {
            get
            {
                return _SelectedACClassMethod;
            }
            set
            {
                _SelectedACClassMethod = value;
                OnPropertyChanged("SelectedACClassMethod");
            }
        }

        /// <summary>
        /// Gets the AC class method list.
        /// </summary>
        /// <value>The AC class method list.</value>
        [ACPropertyList(9999, "ACClassMethod")]
        public override IEnumerable<ACClassMethod> ACClassMethodList
        {
            get
            {
                if (CurrentACClass == null)
                    return null;

                if (_ACClassMethodList == null)
                {
                    var query = CurrentACClass.ACClassMethod_ACClass.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.MSWorkflow);
                    if (query != null)
                        _ACClassMethodList = query.ToList();
                }
                return _ACClassMethodList;
            }
        }

        #endregion

        #region Live
        /// <summary>
        /// The _ current workflow live
        /// </summary>
        ACChildInstanceInfo _CurrentWorkflowLive = null;
        /// <summary>
        /// Gets or sets the current workflow live.
        /// </summary>
        /// <value>The current workflow live.</value>
        [ACPropertyCurrent(9999, "Workflow-Live")]
        public ACChildInstanceInfo CurrentWorkflowLive
        {
            get
            {
                return _CurrentWorkflowLive;
            }
            set
            {
                _CurrentWorkflowLive = value;
                OnPropertyChanged("CurrentWorkflowLive");

                //OnPropertyChanged("CurrentPWRoot");
            }
        }

        /// <summary>
        /// The _ selected workflow live
        /// </summary>
        ACChildInstanceInfo _SelectedWorkflowLive;
        /// <summary>
        /// Gets or sets the selected workflow live.
        /// </summary>
        /// <value>The selected workflow live.</value>
        [ACPropertySelected(9999, "Workflow-Live")]
        public ACChildInstanceInfo SelectedWorkflowLive
        {
            get
            {
                return _SelectedWorkflowLive;
            }
            set
            {
                _SelectedWorkflowLive = value;
                OnPropertyChanged("SelectedWorkflowLive");
            }
        }


        /// <summary>
        /// Gets the workflow live list.
        /// </summary>
        /// <value>The workflow live list.</value>
        [ACPropertyList(9999, "Workflow-Live")]
        public IEnumerable<ACChildInstanceInfo> WorkflowLiveList
        {
            get
            {
                if (CurrentACProject == null)
                    return null;
                List<ACChildInstanceInfo> workflowsLive = new List<ACChildInstanceInfo>();
                foreach (ACProject project in CurrentACProject.ACProject_BasedOnACProject)
                {
                    if (project.RootClass == null)
                        continue;
                    ACComponent pAppManager = ACUrlCommand("?\\" + project.RootClass.ACIdentifier, null) as ACComponent;
                    if (pAppManager != null)
                    {
                        IEnumerable<ACChildInstanceInfo> loadedChilds = pAppManager.GetChildInstanceInfo(1, true);
                        if (loadedChilds != null && loadedChilds.Any())
                        {
                            foreach (ACChildInstanceInfo child in loadedChilds)
                            {
                                if (child.Content.IsObjLoaded == true)
                                {
                                    ACClassWF acClassWF = null;
                                    if (child.Content.ValueT is ACClassWF)
                                        acClassWF = child.Content.ValueT as ACClassWF;
                                    else if (child.Content.ValueT is ACClassTask)
                                        acClassWF = (child.Content.ValueT as ACClassTask).ContentACClassWF;
                                    if (acClassWF != null)
                                    {
                                        if (acClassWF.ACClassMethod != null && acClassWF.ACClassMethodID == CurrentACClassMethod.ACClassMethodID)
                                            workflowsLive.Add(child);
                                    }
                                }
                                //workflowsLive.AddRange(loadedChilds);
                            }
                        }
                    }
                }
                return workflowsLive;
            }
        }

        #endregion

        #region Properties => Validate WF (DB == Presenter)

        private ACValueItem _SelectedDBItemToResolve;
        [ACPropertySelected(350, "ValidationItems")]
        public ACValueItem SelectedDBItemToResolve
        {
            get => _SelectedDBItemToResolve;
            set
            {
                _SelectedDBItemToResolve = value;
                OnPropertyChanged("SelectedDBItemToResolve");
            }
        }

        private List<ACValueItem> _DBItemsToResolve;
        [ACPropertyList(351, "ValidationItems")]
        public List<ACValueItem> DBItemsToResolve
        {
            get => _DBItemsToResolve;
            set
            {
                _DBItemsToResolve = value;
                OnPropertyChanged("DBItemsToResolve");
            }
        }

        #endregion

        #endregion

        #region BSO->ACMethod
        /// <summary>
        /// Saves this instance.
        /// </summary>
        [ACMethodCommand("ACClassMethod", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
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

        protected override Msg OnPreSave()
        {
            if (CurrentACClassMethod != null)
            {
                if (!IgnoreActiveWFValidation && ConfigManagerIPlus.IsActiveWorkflowChanged(this, CurrentACClassMethod, this.Database.ContextIPlus))
                {
                    Msg msg = new Msg { ACIdentifier = "BSOiPlusWorkflow", Message = Root.Environment.TranslateMessage(this, "Info50017", null), MessageLevel = eMsgLevel.Error };
                    return msg;
                }
                //if (!ConfigManagerIPlus.MustConfigBeReloadedOnServer(this, VisitedMethods, this.Database))
                    //this.VisitedMethods = null;
            }
            if (CurrentACClassMethod != null)
            {
                AddVisitedMethods(CurrentACClassMethod);
                //CurrentACClassMethod.UpdateDate = DateTime.Now;
            }
            return base.OnPreSave();
        }

        protected override void OnPostSave()
        {
            IgnoreActiveWFValidation = false;
            ConfigManagerIPlus.ReloadConfigOnServerIfChanged(this, VisitedMethods, this.Database, true);
            this.VisitedMethods = null;
            //ValidateWF(); //TODO Ivan: Consult with Damir
            base.OnPostSave();
        }

        /// <summary>
        /// Undoes the save.
        /// </summary>
        [ACMethodCommand("ACClassMethod", "en{'Undo'}de{'Nicht speichern'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
        public void UndoSave()
        {
            OnUndoSave();
        }

        protected override void OnPostUndoSave()
        {
            this.VisitedMethods = null;
            base.OnPostUndoSave();
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
        [ACMethodInteraction("ACClassMethod", "en{'Load'}de{'Laden'}", (short)MISort.Load, false, "SelectedACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void Load(bool requery = false)
        {
            if (!PreExecute("Load"))
                return;
            LoadEntity<ACClassMethod>(requery, () => SelectedACClassMethod, () => CurrentACClassMethod, c => CurrentACClassMethod = c,
                        Database.ContextIPlus.ACClassMethod
                        .Where(c => c.ACClassMethodID == SelectedACClassMethod.ACClassMethodID));
            if (CurrentACClassMethod != null && CurrentACClassMethod.ACClassWF_ACClassMethod_IsLoaded)
            {
                //CurrentACClassMethod.ACClassWF_ACClassMethod.AutoRefresh(Database.ContextIPlus);
                CurrentACClassMethod.ACClassWF_ACClassMethod.AutoLoad(CurrentACClassMethod.ACClassWF_ACClassMethodReference, CurrentACClassMethod);
            }
            PostExecute("Load");
        }

        /// <summary>
        /// Determines whether [is enabled load].
        /// </summary>
        /// <returns><c>true</c> if [is enabled load]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledLoad()
        {
            return SelectedACClassMethod != null;
        }

        /// <summary>
        /// News this instance.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'New'}de{'Neu'}", (short)MISort.New, true, "SelectedACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void New()
        {
            if (!PreExecute("New")) return;

            CurrentNewACClassMethod = ACClassMethod.NewWorkACClassMethod(Database.ContextIPlus, CurrentACClass);
            Database.ContextIPlus.ACClassMethod.Add(CurrentNewACClassMethod);

            ShowDialog(this, "WorkACClassMethodNew");
            PostExecute("New");
        }

        [ACMethodCommand("NewWorkACClassMethod", Const.Ok, (short)MISort.Okay)]
        public override void NewWorkACClassMethodOK()
        {
            CloseTopDialog();

            if (!IsEnabledNewWorkACClassMethodOK()) 
                return;

            CurrentNewACClassMethod.PWACClass = CurrentNewInvokingACClass;
            VBDesignerWorkflowMethod.DoInsertRoot(CurrentNewACClassMethod, CurrentNewWFRootACClass);

            CurrentNewACClassMethod.UpdateParamListFromACClassConstructor(CurrentNewWFRootACClass);

            CurrentACClass.AddNewACClassMethod(CurrentNewACClassMethod);
            Database.ContextIPlus.ACClassMethod.Add(CurrentNewACClassMethod);

            _ACClassMethodList = null;

            SelectedACClassMethod = CurrentNewACClassMethod;
            CurrentACClassMethod = CurrentNewACClassMethod;

            CurrentNewACClassMethod = null;
            ACSaveChanges();
            OnPropertyChanged("ACClassMethodList");
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
        /// Deletes this instance.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'Delete'}de{'Löschen'}", (short)MISort.Delete, true, "CurrentACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void Delete()
        {
            if (Messages.Question(this, "Question00008", Global.MsgResult.Yes, false, CurrentACClassMethod.ACIdentifier) == Global.MsgResult.Yes)
            {
                if (!PreExecute("Delete")) return;
                Msg msg = CurrentACClassMethod.DeleteACObject(Database.ContextIPlus, false);
                if (msg != null)
                {
                    Messages.Msg(msg);
                    return;
                }
                ACSaveChanges();
                ACClassMethod nextACClassMethod = ACClassMethodList.FirstOrDefault();
                CurrentACClassMethod = nextACClassMethod;
                SelectedACClassMethod = nextACClassMethod;
                PostExecute("Delete");
                _ACClassMethodList = null;
                OnPropertyChanged("ACClassMethodList");
            }
        }

        /// <summary>
        /// Determines whether [is enabled delete].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDelete()
        {
            return CurrentACClassMethod != null;
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            AccessPrimary.NavSearch(Database.ContextIPlus);
        }

        /// <summary>
        /// Shows the workflows live list.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'Workflow Live-List'}de{'Workflow Live-Liste'}", 9999, false, Global.ACKinds.MSMethodPrePost)]
        public void ShowWorkflowsLiveList()
        {
            if (!PreExecute("ShowWorkflowsLiveList")) return;
            ShowDialog(this, "LoadedWorkflowsLive");

            PostExecute("ShowWorkflowsLiveList");
        }

        /// <summary>
        /// Determines whether [is enabled show workflows live list].
        /// </summary>
        /// <returns><c>true</c> if [is enabled show workflows live list]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledShowWorkflowsLiveList()
        {
            return CurrentACClassMethod != null;
        }

        /// <summary>
        /// Shows the workflow live.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'Connect with Live-Workflow'}de{'Verbinde mit Live-Workflow'}", 9999, false, Global.ACKinds.MSMethodPrePost)]
        public void ShowWorkflowLive()
        {
            IACComponent wfInstance = null;
            if (CurrentWorkflowLive != null && CurrentWorkflowLive.ACType.IsObjLoaded)
            {
                ACComponent pAppManager = ACUrlCommand("?" + CurrentWorkflowLive.ACUrlParent, null) as ACComponent;
                if (pAppManager != null)
                {
                    if (pAppManager.IsProxy)
                    {
                        IEnumerable<ACChildInstanceInfo> childsWF = pAppManager.GetChildInstanceInfo(0, true, CurrentWorkflowLive.ACIdentifier);
                        if (childsWF != null && childsWF.Any())
                        {
                            ACChildInstanceInfo childWF = childsWF.First();
                            wfInstance = pAppManager.StartComponent(childWF, Global.ACStartTypes.Automatic, true);
                        }
                    }
                    else
                    {
                        var queryChild = pAppManager.ACComponentChilds.Where(c => c.ACIdentifier == CurrentWorkflowLive.ACIdentifier);
                        if (queryChild != null && queryChild.Any())
                        {
                            wfInstance = queryChild.First();
                        }
                    }
                }
            }
            if (wfInstance != null)
                CurrentPWRootLive = new ACRef<ACComponent>(wfInstance as ACComponent, this);
            else
                CurrentPWRootLive = null;
        }

        /// <summary>
        /// Starts the new workflow.
        /// </summary>
        [ACMethodInfo("", "en{'Start new Workflow'}de{'Starte neuen Workflow'}", 9999, false, false, true)]
        public void StartNewWorkflow()
        {
            if (!IsEnabledStartNewWorkflow())
                return;

            ACProject project = CurrentACProject.ACProject_BasedOnACProject.First();
            ACComponent pAppManager = ACUrlCommand("?\\" + project.RootClass.ACIdentifier, null) as ACComponent;
            if (pAppManager == null)
                return;

            ACMethod acMethod = pAppManager.NewACMethod(CurrentACClassMethod.ACIdentifier);
            if (acMethod == null)
                return;

            CurrentPWRootLive = null; // Neu laden, falls Server-Instanz, damit nicht recyclete Components die vom alten Workflow geladen sind angezeigt werden

            // Damir Test:
            string secondaryKey = Root.NoManager.GetNewNo(Database, typeof(ACProgram), ACProgram.NoColumnName, ACProgram.FormatNewNo, this);
            ACProgram program = ACProgram.NewACObject(this.Database.ContextIPlus, null, secondaryKey);
            program.ProgramACClassMethod = CurrentACClassMethod;
            program.WorkflowTypeACClass = CurrentACClassMethod.WorkflowTypeACClass;
            this.Database.ContextIPlus.ACProgram.Add(program);
            if (ACSaveChanges())
            {
                ACValue paramProgram = acMethod.ParameterValueList.GetACValue(ACProgram.ClassName);
                if (paramProgram == null)
                    acMethod.ParameterValueList.Add(new ACValue(ACProgram.ClassName, typeof(Guid), program.ACProgramID));
                else
                    paramProgram.Value = program.ACProgramID;

                IACPointAsyncRMI rmiInvocationPoint = pAppManager.GetPoint(Const.TaskInvocationPoint) as IACPointAsyncRMI;
                if (rmiInvocationPoint != null)
                    rmiInvocationPoint.AddTask(acMethod, this);
            }
        }

        /// <summary>
        /// Determines whether [is enabled start new workflow].
        /// </summary>
        /// <returns><c>true</c> if [is enabled start new workflow]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledStartNewWorkflow()
        {
            if ((CurrentACProject == null) || (CurrentACClassMethod == null))
                return false;
            if (CurrentACProject.ACProject_BasedOnACProject.Count <= 0)
                return false;
            return true;
        }


        /// <summary>
        /// Tests the workflow.
        /// </summary>
        [ACMethodInfo("", "en{'Test Workflow'}de{'Teste Workflow'}", 9999, false, false, true)]
        public void TestWorkflow()
        {
            if (!IsEnabledTestWorkflow())
                return;

            var variobatchTest = ACUrlCommand("\\" + Const.ACRootProjectNameTest) as ACRoot;
            if (variobatchTest == null)
                return;

            ACProject project = CurrentACProject.ACProject_BasedOnACProject.First();
            ACComponent pAppManager = variobatchTest.ACUrlCommand("?\\" + project.RootClass.ACIdentifier, null) as ACComponent;
            if (pAppManager == null)
                return;

            ACMethod acMethod = pAppManager.NewACMethod(CurrentACClassMethod.ACIdentifier);
            if (acMethod == null)
                return;
            // Norbert: Warum steht im ACNameIdentier "Workflow1116" ???
            if (acMethod.ACIdentifier != CurrentACClassMethod.ACIdentifier)
            {
                acMethod.ACIdentifier = CurrentACClassMethod.ACIdentifier;
            }

            _WorkflowTestCallbacks = 1;
            IACPointAsyncRMI rmiInvocationPoint = pAppManager.GetPoint(Const.TaskInvocationPoint) as IACPointAsyncRMI;
            if (rmiInvocationPoint != null)
                rmiInvocationPoint.AddTask(acMethod, this);
            if (_WorkflowTestCallbacks == 1)
            {
                // Workflow ist nicht vollständig ablauffähig
                Messages.Error(this, "Error00001");
            }
            _WorkflowTestCallbacks = 0;
        }

        /// <summary>
        /// Determines whether [is enabled test workflow].
        /// </summary>
        /// <returns><c>true</c> if [is enabled test workflow]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledTestWorkflow()
        {
            if ((CurrentACProject == null) || (CurrentACClassMethod == null))
                return false;
            if (CurrentACProject.ACProject_BasedOnACProject.Count <= 0)
                return false;
            return true;
        }

        [ACMethodInfo("", "en{'Duplicate'}de{'Duplizieren'}", 9999, false, false, true)]
        public void WFClone()
        {
            if (!IsEnabledWFClone())
                return;
            int nextWFNo = (this.Database as Database).ACClassMethod.Where(c => c.ACClassID == CurrentACClassMethod.ACClassID && c.ACIdentifier.StartsWith(CurrentACClassMethod.ACIdentifier)).Count() + 1;
            string newACIdentifier = Messages.InputBox("New Workflow-No.:", String.Format("{0}_{1}", CurrentACClassMethod.ACIdentifier, nextWFNo));
            if (String.IsNullOrWhiteSpace(newACIdentifier))
                return;
            var newWF = CurrentACClassMethod.WorkflowClone(newACIdentifier);
            CurrentACClass.ACClassMethod_ACClass.Add(newWF);
            Save();
            _ACClassMethodList = null;
            OnPropertyChanged("ACClassMethodList");
            CurrentACClassMethod = ACClassMethodList.FirstOrDefault(x => x.ACClassMethodID == newWF.ACClassMethodID);
            SelectedACClassMethod = CurrentACClassMethod;
        }

        public bool IsEnabledWFClone()
        {
            return CurrentACClassMethod != null;
        }


        #region IACComponentTaskSubscr
        /// <summary>
        /// The _ workflow test callbacks
        /// </summary>
        private short _WorkflowTestCallbacks = 0;
        /// <summary>
        /// Tasks the callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ACEventArgs"/> instance containing the event data.</param>
        /// <param name="wrapObject">The wrap object.</param>
        [ACMethodInfo("Function", "en{'TaskCallback'}de{'TaskCallback'}", 9999)]
        public override void TaskCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                IACTask taskEntry = wrapObject as IACTask;
                if (taskEntry.State == PointProcessingState.Deleted && taskEntry.InProcess)
                {
                    _WorkflowTestCallbacks++;
                }
                else if (taskEntry.State == PointProcessingState.Accepted && taskEntry.InProcess)
                {
                    gip.core.autocomponent.ACRoot.Dispatcher.Add(delegate
                    {
                        ACPointAsyncRMIWrap<ACComponent> rmiWrap = wrapObject as ACPointAsyncRMIWrap<ACComponent>;
                        if (rmiWrap.ExecutingInstance != null && rmiWrap.ExecutingInstance.IsObjLoaded)
                        {
                            if (CurrentPWRootLive == null)
                            {
                                if (rmiWrap.ExecutingInstance.ValueT.IsProxy)
                                {
                                    ACComponentProxy proxyInstance = rmiWrap.ExecutingInstance.ValueT as ACComponentProxy;
                                    if (proxyInstance != null)
                                        proxyInstance.ReloadChildsOverServerInstanceInfo(null);
                                    CurrentPWRootLive = new ACRef<ACComponent>(proxyInstance, this);
                                }
                                else
                                {
                                    CurrentPWRootLive = new ACRef<ACComponent>(rmiWrap.ExecutingInstance.ValueT as ACComponent, this);
                                }
                            }
                        }
                    });
                }
            }
        }
        #endregion

        /// <summary>
        /// The _ current PW root live
        /// </summary>
        ACRef<ACComponent> _CurrentPWRootLive = null;
        /// <summary>
        /// Gets or sets the current PW root live.
        /// </summary>
        /// <value>The current PW root live.</value>
        [ACPropertyCurrent(9999, "ProcessEntity")]
        public ACRef<ACComponent> CurrentPWRootLive
        {
            get
            {
                return _CurrentPWRootLive;
            }
            set
            {
                //if (CurrentPWRoot != null)
                //    CurrentPWRoot.ACDeInit();
                if (_CurrentPWRootLive != null)
                    _CurrentPWRootLive.Detach();
                _CurrentPWRootLive = value;
                //if (_CurrentPWRootLive != null && CurrentPWRoot != null)
                //{
                //    LoadPWRoot(CurrentProcessConfig.ACUrlCommand("ProgramACClassMethod") as ACClassMethod, null, _CurrentPWRootLive.Obj);
                //}
            }
        }

        /// <summary>
        /// The _ process entity
        /// </summary>
        IACConfigStore _ProcessEntity;
        /// <summary>
        /// Gets or sets the current process config.
        /// </summary>
        /// <value>The current process config.</value>
        [ACPropertyCurrent(9999, "ProcessEntity")]
        public IACConfigStore CurrentProcessConfig
        {
            get
            {
                return _ProcessEntity;
            }
            set
            {
                if (_ProcessEntity != value)
                {
                    _ProcessEntity = value;
                    OnPropertyChanged("CurrentProcessConfig");
                    if (_ProcessEntity != null)
                    {
                        //LoadPWRoot(_ProcessEntity.ACUrlCommand("ProgramACClassMethod") as ACClassMethod, null, null);
                    }
                }
            }
        }

        #region Methods => Validate WF (DB == Presenter)

        [ACMethodInteraction("", "", 350, true)]
        public void ValidateWF()
        {
            if (!IsEnabledValidateWF())
                return;

            List<string> presenterElements = null;

            Msg msg = (VBPresenterMethod as VBPresenterMethod)?.GetPresenterElements(out presenterElements);
            if (msg != null)
                Messages.Msg(msg);

            if(presenterElements == null)
                return;

            var nodes = CurrentACClassMethod.AllWFNodes?.ToList();
            var edges = CurrentACClassMethod.AllWFEdges?.ToList();

            foreach (string name in presenterElements)
            {
                var node = nodes?.FirstOrDefault(c => c.XName == name);
                if (node != null)
                {
                    nodes.Remove(node);
                }
                else
                {
                    var edge = edges?.FirstOrDefault(c => c.XName == name);
                    if (edge != null)
                        edges.Remove(edge);
                }
            }

            List<ACValueItem> itemsToResolve = new List<ACValueItem>();

            if (nodes.Any())
            {
                itemsToResolve.AddRange(nodes.Select(x => new ACValueItem(x.ACIdentifier + "  " + x.XName, x, null)));
            }

            if (edges.Any())
            {
                itemsToResolve.AddRange(edges.Select(x => new ACValueItem(x.XName + "  " + x.SourceACName + " --> " + x.TargetACName, x, null)));
            }

            if(itemsToResolve.Any())
            {
                DBItemsToResolve = itemsToResolve;
                ShowDialog(this, "ResolveDBItemsDialog");
            }
        }

        public bool IsEnabledValidateWF()
        {
            return CurrentACClassMethod != null;
        }

        [ACMethodInfo("", "en{'Delete database item'}de{'Datenbankelement löschen'}", 351)]
        public void DeleteDBItem()
        {
            if (!IsEnabledDeleteDBItem())
                return;

            if (Messages.Question(this, "Question50053", Global.MsgResult.No) != Global.MsgResult.Yes)
                return;

            IACWorkflowNode node = SelectedDBItemToResolve.Value as IACWorkflowNode;
            if(node != null)
            {
                var dbNode = CurrentACClassMethod.ACClassWF_ACClassMethod.FirstOrDefault(c => c.ACClassWFID == node.WFObjectID);
                if(dbNode != null)
                {
                    CurrentACClassMethod.ACClassWF_ACClassMethod.Remove(dbNode);
                    Database.ContextIPlus.ACClassWF.Remove(dbNode);
                    //Msg msg = Database.ACSaveChanges();
                    //if (msg != null)
                    //    Messages.Msg(msg);
                    //else
                    //{
                        var temp = _DBItemsToResolve;
                        temp.Remove(SelectedDBItemToResolve);
                        DBItemsToResolve = null;
                        DBItemsToResolve = temp;
                    //}
                }    
            }
            else
            {
                IACWorkflowEdge edge = SelectedDBItemToResolve.Value as IACWorkflowEdge;
                if(edge != null)
                {
                    var dbEdge = CurrentACClassMethod.ACClassWFEdge_ACClassMethod.FirstOrDefault(c => c.ACClassWFEdgeID == edge.WFObjectID);
                    if (dbEdge != null)
                    {
                        CurrentACClassMethod.ACClassWFEdge_ACClassMethod.Remove(dbEdge);
                        Database.ContextIPlus.ACClassWFEdge.Remove(dbEdge);
                        //Msg msg = Database.ACSaveChanges();
                        //if (msg != null)
                        //    Messages.Msg(msg);
                        //else
                        //{
                            var temp = _DBItemsToResolve;
                            temp.Remove(SelectedDBItemToResolve);
                            DBItemsToResolve = null;
                            DBItemsToResolve = temp;
                        //}
                    }
                }
            }
        }

        public bool IsEnabledDeleteDBItem()
        {
            return SelectedDBItemToResolve != null;
        }

        #endregion

        #endregion

        #region IACBSOConfigStoreSelection

        public List<IACConfigStore> MandatoryConfigStores
        {
            get
            {
                List<IACConfigStore> listOfSelectedStores = new List<IACConfigStore>();
                return listOfSelectedStores;
            }
        }

        public IACConfigStore CurrentConfigStore
        {
            get
            {
                return CurrentACClassMethod;
            }
        }

        public bool IsReadonly
        {
            get
            {
                return false;
            }
        }

        private bool _IgnoreActiveWFValidation = false;
        [ACPropertyInfo(200, "", "en{'Ignore active Workflow Validation'}de{'Ignoriere Überprüfung des aktiven Workflows'}")]
        public bool IgnoreActiveWFValidation
        {
            get
            {
                return _IgnoreActiveWFValidation;
            }
            set
            {
                if (value && !this.Root.Environment.User.IsSuperuser)
                    return;
                _IgnoreActiveWFValidation = value;
                OnPropertyChanged();
            }
        }

        private List<core.datamodel.ACClassMethod> _VisitedMethods;
        public List<core.datamodel.ACClassMethod> VisitedMethods
        {
            get
            {
                if (_VisitedMethods == null)
                    _VisitedMethods = new List<core.datamodel.ACClassMethod>();
                return _VisitedMethods;
            }
            set
            {
                _VisitedMethods = value;
                OnPropertyChanged("VisitedMethods");
            }
        }

        public void AddVisitedMethods(core.datamodel.ACClassMethod acClassMethod)
        {
            if (!VisitedMethods.Contains(acClassMethod))
                VisitedMethods.Add(acClassMethod);
        }

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
                case "ShowWorkflowsLiveList":
                    ShowWorkflowsLiveList();
                    return true;
                case "IsEnabledShowWorkflowsLiveList":
                    result = IsEnabledShowWorkflowsLiveList();
                    return true;
                case "ShowWorkflowLive":
                    ShowWorkflowLive();
                    return true;
                case "StartNewWorkflow":
                    StartNewWorkflow();
                    return true;
                case "IsEnabledStartNewWorkflow":
                    result = IsEnabledStartNewWorkflow();
                    return true;
                case "TestWorkflow":
                    TestWorkflow();
                    return true;
                case "IsEnabledTestWorkflow":
                    result = IsEnabledTestWorkflow();
                    return true;
                case "WFClone":
                    WFClone();
                    return true;
                case "IsEnabledWFClone":
                    result = IsEnabledWFClone();
                    return true;
                case "TaskCallback":
                    TaskCallback((IACPointNetBase)acParameter[0], (ACEventArgs)acParameter[1], (IACObject)acParameter[2]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }
}

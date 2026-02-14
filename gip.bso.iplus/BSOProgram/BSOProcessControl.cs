// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-15-2012
// ***********************************************************************
// <copyright file="BSOProcessControl.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.manager;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOProcessControl
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Processcontrol'}de{'Prozesssteuerung'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACProgram.ClassName)]
    public class BSOProcessControl : ACBSO, ITaskPreviewCall, IACBSOAlarmPresenter
    {
        public const string BSOClassName = "BSOProcessControl";

        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="BSOProcessControl"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOProcessControl(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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

            _CurrentProgramType = ProgramTypeList.First();

            _ShutdownEvent = new ManualResetEvent(false);
            _WorkCycleThread = new ACThread(RunWorkCycle);
            _WorkCycleThread.Start();

            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            this._CurrentACTask = null;
            this._CurrentApplicationManager = null;
            this._CurrentProgramType = null;
            this._ProgramTypeList = null;
            this._SelectedACTask = null;

            if (_WorkCycleThread != null)
            {
                if (_ShutdownEvent != null && _ShutdownEvent.SafeWaitHandle != null && !_ShutdownEvent.SafeWaitHandle.IsClosed)
                    _ShutdownEvent.Set();
                if (!_WorkCycleThread.Join(1000))
                    _WorkCycleThread.Abort();

                _WorkCycleThread = null;
                _ShutdownEvent = null;
            }

            bool done = await base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
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

        #endregion

        #region BSO->ACProperty

        protected ManualResetEvent _ShutdownEvent;
        private ACThread _WorkCycleThread;

        #region FilterMode
        public enum FilterMode
        {
            ByApplication,
            ByProgram
        }

        protected FilterMode _CurrentMode = FilterMode.ByApplication;
        protected FilterMode CurrentMode
        {
            get
            {
                return _CurrentMode;
            }
            set
            {
                _CurrentMode = value;
                OnPropertyChanged("CurrentMode");
            }
        }
        #endregion

        #region Filter by Program

        ACObjectItem _CurrentProgramType;
        [ACPropertyCurrent(404, "ProgramType", "en{'Programtype'}de{'Programmart'}")]
        public ACObjectItem CurrentProgramType
        {
            get
            {
                return _CurrentProgramType;
            }
            set
            {
                bool changed = _CurrentProgramType != value;
                _CurrentProgramType = value;
                OnPropertyChanged("CurrentProgramType");
                if (changed)
                {
                    CurrentMode = FilterMode.ByProgram;
                    _= Search();
                }
            }
        }

        List<ACObjectItem> _ProgramTypeList;
        [ACPropertyList(405, "ProgramType")]
        public IEnumerable<ACObjectItem> ProgramTypeList
        {
            get
            {
                if (_ProgramTypeList == null)
                {
                    _ProgramTypeList = new List<ACObjectItem>();
                    _ProgramTypeList.Add(new ACObjectItem(Root.Environment.TranslateText(this, "All Methods")));

                    var query = Database.ContextIPlus.WorkflowTypeMethodACClassList.OrderBy(c => c.SortIndex);
                    foreach (var pwMethod in query)
                    {
                        _ProgramTypeList.Add(new ACObjectItem(pwMethod, pwMethod.ACCaption));
                    }
                }
                return _ProgramTypeList;
            }
        }
        #endregion

        #region Filter by Application
        ACClass _CurrentApplicationManager;
        [ACPropertyCurrent(406, "ApplicationManager", "en{'Application'}de{'Anwendung'}")]
        public ACClass CurrentApplicationManager
        {
            get
            {
                return _CurrentApplicationManager;
            }
            set
            {
                if (_CurrentApplicationManager != value)
                {
                    _CurrentApplicationManager = value;
                    OnPropertyChanged("CurrentApplicationManager");
                    CurrentMode = FilterMode.ByApplication;
                    _= Search();
                }
            }
        }

        IEnumerable<ACClass> _ApplicationManagerList;
        [ACPropertyList(407, "ApplicationManager")]
        public IEnumerable<ACClass> ApplicationManagerList
        {
            get
            {
                _ApplicationManagerList = Database.ContextIPlus.ACClass
                    .Where(c => c.ACKindIndex == (short)Global.ACKinds.TACApplicationManager
                            && c.ACProject.IsWorkflowEnabled
                            && c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application
                            && c.ACClassTask_TaskTypeACClass.Where(d => !d.IsTestmode).Any())
                    .OrderBy(c => c.ACIdentifier)
                    .ToArray();
                return _ApplicationManagerList;
            }
        }
        #endregion

        #region Workflows
        /// <summary>
        /// The _ current AC task
        /// </summary>
        ACClassTask _CurrentACTask = null;
        /// <summary>
        /// Gets or sets the current AC task.
        /// </summary>
        /// <value>The current AC task.</value>
        [ACPropertyCurrent(401, "Workflow-Live")]
        public ACClassTask CurrentACTask
        {
            get
            {
                return _CurrentACTask;
            }
            set
            {
                lock (_RefreshLock)
                {
                    // If SelectedTask ist set from UI/DataGrid while Refreshing TaskList from Background-Thread, then return;
                    if (AutoRefresh && !_AutoRefreshActive && value == null)
                        return;
                }
                _CurrentACTask = value;
                OnPropertyChanged("CurrentACTask");
            }
        }

        /// <summary>
        /// The _ selected AC task
        /// </summary>
        ACClassTask _SelectedACTask;
        /// <summary>
        /// Gets or sets the selected AC task.
        /// </summary>
        /// <value>The selected AC task.</value>
        [ACPropertySelected(402, "Workflow-Live")]
        public ACClassTask SelectedACTask
        {
            get
            {
                return _SelectedACTask;
            }
            set
            {
                lock (_RefreshLock)
                {
                    // If SelectedTask ist set from UI/DataGrid while Refreshing TaskList from Background-Thread, then return;
                    if (AutoRefresh && !_AutoRefreshActive && value == null)
                        return;
                }
                if (_SelectedACTask != value)
                {
                    if (TaskPresenter != null)
                        TaskPresenter.Unload();
                }
                _SelectedACTask = value;
                OnPropertyChanged();
            }
        }


        protected IEnumerable<ACClassTask> _ACTaskList;
        /// <summary>
        /// Gets the AC task list.
        /// </summary>
        /// <value>The AC task list.</value>
        [ACPropertyList(403, "Workflow-Live")]
        public IEnumerable<ACClassTask> ACTaskList
        {
            get
            {
                return _ACTaskList;
            }
        }

        #endregion

        #region AutoRefresh
        private readonly object _RefreshLock = new object();
        private bool _AutoRefreshActive = false;
        bool _AutoRefresh = false;
        [ACPropertyInfo(100, "", "en{'Automatic refresh'}de{'Automatische Aktualisierung'}")]
        public bool AutoRefresh
        {
            get
            {
                return _AutoRefresh;
            }
            set
            {
                _AutoRefresh = value;
                OnPropertyChanged();
            }
        }

        public VBBSOSelectionManager VBBSOSelectionManager
        {
            get
            {
                return FindChildComponents<VBBSOSelectionManager>(c => c is VBBSOSelectionManager).FirstOrDefault();
            }
        }

        VBPresenterTask _presenter = null;
        bool _PresenterRightsChecked = false;
        public VBPresenterTask TaskPresenter
        {
            get
            {
                if (_presenter == null)
                {
                    _presenter = this.ACUrlCommand("VBPresenterTask(CurrentDesign)") as VBPresenterTask;
                    if (_presenter == null && !_PresenterRightsChecked)
                        Messages.ErrorAsync(this, "This user has no rights for viewing workflows. Assign rights for VBPresenterTask in the group management!", true);
                    _PresenterRightsChecked = true;
                }
                return _presenter;
            }
        }


        #endregion

        #endregion


        #region Methods

        #region Eventhandling
        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public override void ACAction(ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.TabItemActivated)
            {
                OnActivate(actionArgs.DropObject.VBContent);
            }
            else
                base.ACAction(actionArgs);
        }

        /// <summary>
        /// Called when [activate].
        /// </summary>
        /// <param name="page">The page.</param>
        [ACMethodInfo("Facility", "en{'Activate'}de{'Aktivieren'}", 490, true, Global.ACKinds.MSMethodPrePost)]
        public void OnActivate(string page)
        {
            if (!PreExecute("OnActivate"))
                return;
            if (page == "SearchByApplication")
            {
                _NeedSearch = true;
                CurrentMode = FilterMode.ByApplication;
                if (ApplicationManagerList != null && ApplicationManagerList.Any())
                    CurrentApplicationManager = ApplicationManagerList.FirstOrDefault();
                else
                    CurrentApplicationManager = null;
                if (_NeedSearch)
                    _= Search();
            }
            else if (page == "SearchByProgram")
            {
                _NeedSearch = true;
                CurrentProgramType = ProgramTypeList.FirstOrDefault();
                CurrentMode = FilterMode.ByProgram;
                if (_NeedSearch)
                    _= Search();
            }
            PostExecute("OnActivate");
        }
        #endregion

        #region Refresh Tasklist
        protected bool _NeedSearch = false;
        [ACMethodInfo("", "en{'List active workflows'}de{'Aktive Workflows auflisten'}", (short)MISort.Search)]
        public async Task Search()
        {
            await LoadACTaskList(CurrentMode, true);
        }

        protected virtual async Task<bool> LoadACTaskList(FilterMode filterMode, bool forceUpdateTaskList)
        {
            _NeedSearch = false;
            ACClassTask[] newTaskList = null;
            bool taskListChanged = true;
            if (filterMode == FilterMode.ByApplication)
            {
                if (CurrentApplicationManager == null)
                {
                    taskListChanged = _ACTaskList != null;
                    EmptyACTaskList();
                    return taskListChanged;
                }

                ACClassTask rootTaskAppManger = CurrentApplicationManager.ACClassTask_TaskTypeACClass.Where(c => !c.IsTestmode).FirstOrDefault();
                if (rootTaskAppManger == null)
                {
                    taskListChanged = _ACTaskList != null;
                    EmptyACTaskList();
                    return taskListChanged;
                }

                newTaskList = this.Database.ContextIPlus.ACClassTask
                    .Include(c => c.TaskTypeACClass)
                    .Include(c => c.ContentACClassWF)
                    .Include(c => c.ACProgram)
                    .Include("ACClassTask_ParentACClassTask")
                    .Where(c => c.ParentACClassTaskID.HasValue && c.ParentACClassTaskID == rootTaskAppManger.ACClassTaskID
                        && c.IsDynamic
                        && c.ACTaskTypeIndex == (short)Global.ACTaskTypes.WorkflowTask)
                    .OrderByDescending(c => c.InsertDate)
                    .ToArray();
            }
            else
            {
                if (CurrentProgramType == null)
                {
                    taskListChanged = _ACTaskList != null;
                    EmptyACTaskList();
                    return taskListChanged;
                }

                if (CurrentProgramType.ACObject is ACClass)
                {
                    ACClass pwACClass = _CurrentProgramType.ACObject as ACClass;
                    newTaskList = Database.ContextIPlus.ACClassTask
                        .Include(c => c.TaskTypeACClass)
                        .Include(c => c.ContentACClassWF)
                        .Include(c => c.ACProgram)
                        .Include("ACClassTask_ParentACClassTask")
                        .Where(c => c.IsDynamic
                                && c.ACProgramID.HasValue
                                && c.ACTaskTypeIndex == (short)Global.ACTaskTypes.WorkflowTask
                                && c.ParentACClassTaskID.HasValue && c.ACClassTask1_ParentACClassTask.TaskTypeACClass.ACKindIndex == (short)Global.ACKinds.TACApplicationManager
                                && c.ACClassTask1_ParentACClassTask.TaskTypeACClass.ACProject.IsWorkflowEnabled
                                && c.ContentACClassWFID.HasValue && c.ContentACClassWF.PWACClass.ACClassID == pwACClass.ACClassID)
                        .OrderBy(c => c.ACProgram.ProgramNo)
                        .ThenByDescending(c => c.InsertDate)
                        .ToArray();
                }
                else
                {
                    newTaskList = Database.ContextIPlus.ACClassTask
                        .Include(c => c.TaskTypeACClass)
                        .Include(c => c.ContentACClassWF)
                        .Include(c => c.ACProgram)
                        .Include("ACClassTask_ParentACClassTask")
                        .Where(c => c.IsDynamic
                                && c.ACProgramID.HasValue
                                && c.ACTaskTypeIndex == (short)Global.ACTaskTypes.WorkflowTask
                                && c.ParentACClassTaskID.HasValue && c.ACClassTask1_ParentACClassTask.TaskTypeACClass.ACKindIndex == (short)Global.ACKinds.TACApplicationManager
                                && c.ACClassTask1_ParentACClassTask.TaskTypeACClass.ACProject.IsWorkflowEnabled)
                        .OrderBy(c => c.ACProgram.ProgramNo)
                        .ThenByDescending(c => c.InsertDate)
                        .ToArray();
                }
            }

            if (!forceUpdateTaskList && _ACTaskList != null)
            {
                taskListChanged = newTaskList.Except(_ACTaskList).Any();
                if (taskListChanged)
                    _ACTaskList = newTaskList;
            }
            else
                _ACTaskList = newTaskList;

            if (taskListChanged)
            {
                OnPropertyChanged("ACTaskList");
                if (_ACTaskList != null)
                {
                    CurrentACTask = _ACTaskList.FirstOrDefault();
                    SelectedACTask = CurrentACTask;
                }
            }
            return taskListChanged;
        }

        protected virtual void EmptyACTaskList()
        {
            _ACTaskList = null;
            CurrentACTask = null;
            SelectedACTask = null;
            OnPropertyChanged("ACTaskList");
        }

        protected virtual bool SelectACTaskAndShowWF(string taskACIdentifier)
        {
            if (ACTaskList == null || !ACTaskList.Any())
                return false;
            var task = ACTaskList.Where(c => c.ACIdentifier == taskACIdentifier).FirstOrDefault();
            if (task == null)
                return false;
            SelectedACTask = task;
            ShowWorkflow();
            return true;
        }
        #endregion

        #region Active Workflows
        /// <summary>
        /// Loads the process workflow.
        /// </summary>
        [ACMethodInteraction("Workflow-Live", "en{'Show workflow'}de{'Workflow anzeigen'}", (short)MISort.Load, true, "SelectedACTask")]
        public void ShowWorkflow()
        {
            if (!IsEnabledShowWorkflow())
                return;
            if (TaskPresenter != null)
            {
                TaskPresenter.Load(SelectedACTask);
                if (CurrentACTask != SelectedACTask)
                    CurrentACTask = SelectedACTask;
            }
            OnPropertyChanged("CurrentLayout");
        }

        /// <summary>
        /// Determines whether [is enabled show process workflow].
        /// </summary>
        /// <returns><c>true</c> if [is enabled show process workflow]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledShowWorkflow()
        {
            return SelectedACTask != null;
        }

        [ACMethodInteraction("Workflow-Live", "en{'Delete inactive workflow'}de{'Inaktiven Workflow löschen'}", (short)MISort.Delete, true, "SelectedACTask")]
        public async Task DeleteWorkflow()
        {
            if (SelectedACTask == null)
                return;
            SelectedACTask.ACClassTask_ParentACClassTask.AutoRefresh(SelectedACTask.ACClassTask_ParentACClassTaskReference, SelectedACTask);
            if (!IsEnabledDeleteWorkflow())
                return;
            bool? loaded = await IsLoadedOnServer(SelectedACTask);
            if (!loaded.HasValue || loaded.Value)
                return;

            List<ACClassTask> deleteList = new List<ACClassTask>();
            var msg = DeleteACTasksOfWF(deleteList, SelectedACTask);
            if (msg != null)
            {
                await Messages.MsgAsync(msg);
                return;
            }
            if (!await OnSave())
                return;

            foreach (var acClassTask in deleteList)
            {
                msg = acClassTask.DeleteACObject(this.Database.ContextIPlus, true);
                if (msg != null)
                {
                    await Messages.MsgAsync(msg);
                    ACUndoChanges();
                    return;
                }
            }
            if (await OnSave())
                await Search();
        }

        protected virtual MsgWithDetails DeleteACTasksOfWF(List<ACClassTask> deleteList, ACClassTask acClassTask)
        {
            MsgWithDetails msg = null;
            if (acClassTask.ACClassTask_ParentACClassTask_IsLoaded)
            {
                //acClassTask.ACClassTask_ParentACClassTask.AutoRefresh(this.Database.ContextIPlus);
                acClassTask.ACClassTask_ParentACClassTask.AutoLoad(acClassTask.ACClassTask_ParentACClassTaskReference, acClassTask);
            }
            foreach (var childTask in acClassTask.ACClassTask_ParentACClassTask.ToArray())
            {
                msg = DeleteACTasksOfWF(deleteList, childTask);
                if (msg != null)
                    return msg;
            }
            OnDeleteACClassTask(acClassTask);
            deleteList.Add(acClassTask);
            return msg;
        }

        protected virtual void OnDeleteACClassTask(ACClassTask acClassTask)
        {
        }

        public bool IsEnabledDeleteWorkflow()
        {
            return SelectedACTask != null;// && !SelectedACTask.ACClassTask_ParentACClassTask.Any();
        }

        public async Task<bool?> IsLoadedOnServer(ACClassTask acClassTask)
        {
            if (acClassTask == null)
                return null;
            if (acClassTask.ACClassTask1_ParentACClassTask == null)
                return null;
            string acUrl = acClassTask.ACClassTask1_ParentACClassTask.TaskTypeACClass.GetACUrlComponent();
            if (String.IsNullOrEmpty(acUrl))
                return null;
            ACComponent appManager = ACUrlCommand(acUrl) as ACComponent;
            if (appManager == null)
                return null;
            if (appManager.ConnectionState == ACObjectConnectionState.DisConnected)
            {
                Msg childDeleteQuestion = new Msg() { MessageLevel = eMsgLevel.Question, Message = Root.Environment.TranslateMessage(this, "Question50025") };
                var result = await Messages.MsgAsync(childDeleteQuestion, Global.MsgResult.No, eMsgButton.YesNo);
                if (result == Global.MsgResult.No)
                    return null;
                return false;
            }
            var childsInfo = appManager.GetChildInstanceInfo(1, true);
            if (childsInfo == null || !childsInfo.Any())
                return false;
            var childInfo = childsInfo.Where(c => c.ACIdentifier == acClassTask.ACIdentifier).FirstOrDefault();
            if (childInfo != null)
            {
                Msg childDeleteQuestion = new Msg() { MessageLevel = eMsgLevel.Question, Message = Root.Environment.TranslateMessage(this, "Warning50009") };
                await Messages.MsgAsync(childDeleteQuestion, Global.MsgResult.OK, eMsgButton.OK);
                return true;
            }
            return false;
        }

        public async void SwitchToViewOnAlarm(Msg msgAlarm)
        {
            if (msgAlarm == null || msgAlarm.SourceComponent == null || VBBSOSelectionManager == null)
                return;

            string acUrlToFind = msgAlarm.Source;
            if (msgAlarm.SourceComponent != null && msgAlarm.SourceComponent.ValueT != null)
                acUrlToFind = msgAlarm.SourceComponent.ValueT.GetACUrl();
            if (!IsValidAlarmUrl(acUrlToFind))
                return;

            ACUrlHelper acUrlHelperAppManager = new ACUrlHelper(acUrlToFind);
            acUrlHelperAppManager = new ACUrlHelper(acUrlHelperAppManager.NextACUrl);
            ACUrlHelper acUrlHelperWFRoot = new ACUrlHelper(acUrlHelperAppManager.NextACUrl);

            if (CurrentApplicationManager == null || CurrentApplicationManager.GetACUrlComponent() != "\\" + acUrlHelperAppManager.ACUrlPart)
            {
                if (ApplicationManagerList == null || !ApplicationManagerList.Any())
                    return;
                var switchToAppManager = ApplicationManagerList.Where(c => c.GetACUrlComponent() == "\\" + acUrlHelperAppManager.ACUrlPart).FirstOrDefault();
                if (switchToAppManager == null)
                    return;
                CurrentApplicationManager = switchToAppManager;
                await LoadACTaskList(FilterMode.ByApplication, false);
            }

            if (SelectedACTask == null
                || SelectedACTask.ACIdentifier != acUrlHelperWFRoot.ACUrlPart)
            {
                await LoadACTaskList(FilterMode.ByApplication, false);
                if (TaskPresenter != null)
                    TaskPresenter.MsgForSwitchingView = msgAlarm;
                if (!SelectACTaskAndShowWF(acUrlHelperWFRoot.ACUrlPart))
                    return;
            }
            else if (TaskPresenter != null
                && (TaskPresenter.WFRootContext == null 
                || TaskPresenter.SelectedRootWFNode == null 
                || TaskPresenter.SelectedRootWFNode.ACIdentifier  != acUrlHelperWFRoot.ACUrlPart))
            {
                if (TaskPresenter != null)
                    TaskPresenter.MsgForSwitchingView = msgAlarm;
                if (!SelectACTaskAndShowWF(acUrlHelperWFRoot.ACUrlPart))
                    return;
            }
            else
            {
                if (TaskPresenter != null)
                {
                    var selManager = TaskPresenter.VBBSOSelectionManager as VBBSOSelectionManager;
                    if (selManager != null)
                        selManager.HighlightContentACObject(GetComponentToHighlight(msgAlarm), false);
                }
            }
        }

        public IACComponent GetComponentToHighlight(Msg msgAlarm)
        {
            if (msgAlarm.SourceComponent != null && msgAlarm.SourceComponent.ValueT != null)
                return msgAlarm.SourceComponent.ValueT;
            List<string> parents = ACUrlHelper.ResolveParents(msgAlarm.Source);
            if (!parents.Any())
                return null;
            parents.Reverse();
            foreach (var parent in parents)
            {
                if (parent == msgAlarm.Source)
                    continue;
                IACComponent parentComp = ACUrlCommand("?" + parent) as IACComponent;
                if (parentComp != null)
                    return parentComp;
            }
            return null;
        }

        public static bool IsValidAlarmUrl(string acUrlToFind)
        {
            var urlHelper = new ACUrlHelper(acUrlToFind);
            return urlHelper.UrlKey == ACUrlHelper.UrlKeys.Root
                && !String.IsNullOrEmpty(urlHelper.NextACUrl)
                && ACUrlHelper.IsUrlDynamicInstance(acUrlToFind);
        }

        #endregion

        #endregion

        #region Layoutsteuerung
        /// <summary>
        /// Gets the current layout.
        /// </summary>
        /// <value>The current layout.</value>
        public string CurrentLayout
        {
            get
            {
                string layoutXAML = "";
                if (CurrentACTask != null)
                {
                    layoutXAML += "<vb:VBDockPanel>";
                    layoutXAML += "<vb:VBDynamic VBContent=\"CurrentLayout\">";
                    layoutXAML += "<vb:VBInstanceInfo Key=\"VBPresenter\" ACIdentifier=\"VBPresenterTask(CurrentDesign)\"  SetAsDataContext=\"True\" SetAsBSOACComponet=\"True\" AutoStart=\"True\"/>";
                    layoutXAML += "</vb:VBDynamic>";
                    layoutXAML += "</vb:VBDockPanel>";
                }
                
                return layoutXAML;
            }
        }
        #endregion

        #region Cyclic

        private async void RunWorkCycle()
        {
            try
            {
                while (!_ShutdownEvent.WaitOne(5000, false))
                {
                    if (AutoRefresh)
                    {
                        //lock (_RefreshLock)
                        //{
                            try
                            {
                                _AutoRefreshActive = true;
                                bool taskListChanged = await LoadACTaskList(CurrentMode, false);
                                if (!taskListChanged && SelectedACTask != null)
                                {
                                    if (TaskPresenter != null && TaskPresenter.WFRootContext == null)
                                        taskListChanged = true;
                                }
                                if (taskListChanged)
                                    ShowWorkflow();
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                Messages.LogException("BSOProcessControl", "RunWorkCycle", msg);
                            }
                            finally
                            {
                                _AutoRefreshActive = false;
                            }
                        //}
                        //Search();
                        // TODO: Change only SelectedTask when there are new entries
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("BSOProcessControl", "RunWorkCycle", msg);
            }
        }
        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(OnActivate):
                    OnActivate((String)acParameter[0]);
                    return true;
                case nameof(Search):
                    _= Search();
                    return true;
                case nameof(ShowWorkflow):
                    ShowWorkflow();
                    return true;
                case nameof(IsEnabledShowWorkflow):
                    result = IsEnabledShowWorkflow();
                    return true;
                case nameof(DeleteWorkflow):
                    _= DeleteWorkflow();
                    return true;
                case nameof(IsEnabledDeleteWorkflow):
                    result = IsEnabledDeleteWorkflow();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #region ITaskPreviewCall
        public virtual void PreviewTask(core.datamodel.ACClass applicationManager, core.datamodel.ACClassTask task)
        {
            CurrentApplicationManager = applicationManager;
            SelectedACTask = _ACTaskList.FirstOrDefault(x => x.ACClassTaskID == task.ACClassTaskID);
            ShowWorkflow();
        }
        #endregion  

    }
}

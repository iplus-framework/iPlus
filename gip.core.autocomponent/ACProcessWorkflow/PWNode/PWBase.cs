// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.ComponentModel;
using System.Xml;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

namespace gip.core.autocomponent
{
    /// <summary>
    ///   <para>
    /// PWBase is the base-class for all workflow classes.
    /// She takes on the following tasks:
    /// </para>
    ///   <para>1. Initialization of a workflow instance based on the workflow description using the tables ACClassMethod, ACClassWF and ACClassWFEdge.The event points are also connected to one another during initialization.
    /// <br /> 2. Logging of the program flow in the ACProgramLog table.Whenever a workflow node is started, an entry is written to the ACProgramLog table.ACProgramLog contains information about the start and end time, the previous ProgramLog (ParentProgramLog) and the configuration and result parameters (ACMethod) with which the workflow node worked.The program flow can be viewed afterwards with the ProgramLog presenter.ACProgramLogs refer to an ACProgram that is generated when a workflow is started.
    /// <br />3. Search functions for workflow nodes within a workflow.
    /// </para>
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PABase" />
    /// <seealso cref="gip.core.datamodel.IACComponentPWNode" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWBase'}de{'PWBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.Optional, false, false)]
    public abstract class PWBase : PABase, IACComponentPWNode
    {
        #region c´tors
        static PWBase()
        {
            RegisterExecuteHandler(typeof(PWBase), HandleExecuteACMethod_PWBase);
        }

        public PWBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        protected override void Construct(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            base.Construct(acType, content, parentACObject, parameter, acIdentifier);
            PWContentTaskHelper.InitContent(acType, this, content, parameter);
        }

        /// <summary>
        /// This method is called inside the Construct-Method. Derivations can have influence to the naming of the instance by changing the acIdentifier-Parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        protected override void InitACIdentifier(ACValueList parameter, ref string acIdentifier)
        {
            if (ContentACClassWF != null)
                acIdentifier = ContentACClassWF.ACIdentifier;
        }

        public override bool ACPostInit()
        {
            if (!(this is PWProcessFunction))
            {
                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    var query = ACMemberList.Where(c => c is PWPointEventSubscr).Select(c => c as PWPointEventSubscr);
                    foreach (PWPointEventSubscr pwEventSubscr in query)
                    {
                        pwEventSubscr.SubscribeACClassWFEdgeEvents();
                    }
                }
            }
            return base.ACPostInit();
        }

        public override bool ACPreDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACPreDeInit(deleteACClassTask);
            if (!result)
                return result;

            if (!(this is PWProcessFunction))
            {
                if (deleteACClassTask)
                {
                    IEnumerable<IACMember> filteredMemberList = null;

                    using (ACMonitor.Lock(LockMemberList_20020))
                    {
                        filteredMemberList = ACMemberList.Where(c => c is PWPointEventSubscr).Select(c => c as PWPointEventSubscr).ToArray();
                    }
                    if (filteredMemberList != null && filteredMemberList.Any())
                    {
                        foreach (PWPointEventSubscr pwEventSubscr in filteredMemberList)
                        {
                            pwEventSubscr.UnSubscribeAllEvents();
                        }
                    }
                }
            }
            if (ProcessAlarm != null)
                ProcessAlarm.ValueT = PANotifyState.Off;
            if (HasRules != null)
                this.HasRules.ValueT = 0;
            UnSubscribeToProjectWorkCycle();
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool baseResult = base.ACDeInit(deleteACClassTask);
            if (!(this is PWProcessFunction))
            {
                if (baseResult)
                    PWContentTaskHelper.DeInitContent(this, deleteACClassTask);
            }
            _ContentACClassWF = null;

            SetCurrentProgramLog(null, deleteACClassTask);

            if (ACRef != null && ACRef.IsAttached)
                ACRef.Detach();

            return baseResult;
        }

        public override void Recycle(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            using (ACMonitor.Lock(this._20015_LockValue))
            {
                _ContentACClassWF = null;
                _CurrentProgramLog = null;
            }

            base.Recycle(content, parentACObject, parameter, acIdentifier);

            if (CurrentACState != ACStateEnum.SMIdle)
                CurrentACState = ACStateEnum.SMIdle;
        }

        private static bool? _PoolWFComponents;
        public static bool PoolWFComponents
        {
            get
            {
                if (!_PoolWFComponents.HasValue)
                    return true;
                return _PoolWFComponents.Value;
            }
            set
            {
                if (_PoolWFComponents.HasValue)
                    return;
                _PoolWFComponents = value;
            }
        }

        public override bool IsPoolable
        {
            get
            {
                return PoolWFComponents;
            }
        }

        #endregion


        #region Properties

        #region WF-References and ACClassWF
        
        public enum WFInstantiatonPhase
        {
            OldWF_Restored = 0,
            NewWF_Creating = 1,
            NewWF_TaskCreated = 2
        }

        private WFInstantiatonPhase _WFInitPhase = WFInstantiatonPhase.OldWF_Restored;
        public WFInstantiatonPhase WFInitPhase
        {
            get
            {
                return _WFInitPhase;
            }
        }

        
        /// <summary>The primary IACObject that IACComponent encapsulates.</summary>
        /// <value>Content of type IACObject</value>
        public override IACObject Content
        {
            get
            {
                return _Content;
            }
            set
            {
                if (_Content != value)
                {
                    if (value == null)
                        _ContentACClassWF = null;
                    else if (value is ACClassWF)
                    {
                        _ContentACClassWF = value as ACClassWF;
                        _WFInitPhase = WFInstantiatonPhase.NewWF_Creating;
                    }
                    else if (value is ACClassTask)
                    {
                        ACClassTask acClassTask = value as ACClassTask;
                        if (acClassTask.ContentACClassWF_IsLoaded)
                            _ContentACClassWF = acClassTask.ContentACClassWF;
                        if (_ContentACClassWF == null) // && (acClassTask.EntityState == System.Data.EntityState.Added || acClassTask.EntityState == System.Data.EntityState.Detached))
                            _ContentACClassWF = acClassTask.NewContentACClassWFForQueue;
                        if (_ContentACClassWF == null)
                            ACClassTaskQueue.TaskQueue.ProcessAction(() => { _ContentACClassWF = acClassTask.ContentACClassWF; });
                        if (_WFInitPhase == WFInstantiatonPhase.NewWF_Creating)
                            _WFInitPhase = WFInstantiatonPhase.NewWF_TaskCreated;
                    }
                }
                _Content = value;
                OnPropertyChanged("Content");
            }
        }


        ACClassWF _ContentACClassWF;
        /// <summary>Reference to the definition of this Workflownode.</summary>
        /// <value>Reference to the definition of this Workflownode.</value>
        public virtual ACClassWF ContentACClassWF
        {
            get
            {
                if (_ContentACClassWF == null && Content != null)
                {
                    if (Content is ACClassWF)
                        _ContentACClassWF = Content as ACClassWF;
                    else if (Content is ACClassTask)
                    {
                        ACClassTask acClassTask = Content as ACClassTask;
                        if (acClassTask.ContentACClassWF_IsLoaded)
                            _ContentACClassWF = acClassTask.ContentACClassWF;
                        if (_ContentACClassWF == null)// && (acClassTask.EntityState == System.Data.EntityState.Added || acClassTask.EntityState == System.Data.EntityState.Detached))
                            _ContentACClassWF = acClassTask.NewContentACClassWFForQueue;
                        if (_ContentACClassWF == null)
                            ACClassTaskQueue.TaskQueue.ProcessAction(() => { _ContentACClassWF = acClassTask.ContentACClassWF; });
                    }
                }
                return _ContentACClassWF;
            }
        }

        public ACClassMethod RefACClassMethodOfContentWF
        {
            get
            {
                var contentACClassWF = this.ContentACClassWF;
                if (contentACClassWF == null)
                    return null;
                ACClassMethod aCClassMethod = null;
                if (contentACClassWF.RefPAACClassMethod_IsLoaded)
                    aCClassMethod = contentACClassWF.RefPAACClassMethod;
                if (aCClassMethod == null)
                {
                    using (ACMonitor.Lock(this.ContextLockForACClassWF))
                    {
                        aCClassMethod = contentACClassWF.RefPAACClassMethod;
                    }
                }
                return aCClassMethod;
            }
        }

        public ACClass RefACClassOfContentWF
        {
            get
            {
                var contentACClassWF = this.ContentACClassWF;
                if (contentACClassWF == null)
                    return null;
                ACClass refACClassOfContentWF = null;
                if (contentACClassWF.RefPAACClass_IsLoaded)
                    refACClassOfContentWF = contentACClassWF.RefPAACClass;
                if (refACClassOfContentWF == null)
                {
                    using (ACMonitor.Lock(this.ContextLockForACClassWF))
                    {
                        refACClassOfContentWF = contentACClassWF.RefPAACClass;
                    }
                }
                return refACClassOfContentWF;
            }
        }

        public ACClassMethod ACClassMethodOfContentWF
        {
            get
            {
                var contentACClassWF = this.ContentACClassWF;
                if (contentACClassWF == null)
                    return null;
                ACClassMethod acClassMethodOfContentWF = null;
                if (contentACClassWF.ACClassMethod_IsLoaded)
                    acClassMethodOfContentWF = contentACClassWF.ACClassMethod;
                if (acClassMethodOfContentWF == null)
                {
                    using (ACMonitor.Lock(this.ContextLockForACClassWF))
                    {
                        acClassMethodOfContentWF = contentACClassWF.ACClassMethod;
                    }
                }
                return acClassMethodOfContentWF;
            }
        }

        public ACClass PWACClassOfContentWF
        {
            get
            {
                var contentACClassWF = this.ContentACClassWF;
                if (contentACClassWF == null)
                    return null;
                ACClass pwACClassOfContentW = null;
                if (contentACClassWF.PWACClass_IsLoaded)
                    pwACClassOfContentW = contentACClassWF.PWACClass;
                if (pwACClassOfContentW == null)
                {
                    using (ACMonitor.Lock(this.ContextLockForACClassWF))
                    {
                        pwACClassOfContentW = contentACClassWF.PWACClass;
                    }
                }
                return pwACClassOfContentW;
            }
        }


        /// <summary>
        /// Gets the Root-Workflownode.
        /// If this instance is a PWProcessFunction, than this-Pointer will be returned.
        /// (See also ParentRootWFNode)
        /// </summary>
        public PWProcessFunction RootPW
        {
            get
            {
                if (this is PWProcessFunction)
                    return this as PWProcessFunction;
                return ParentRootWFNode as PWProcessFunction;
            }
        }

        [ACPropertyInfo(9999)]
        public bool IsOnlineNode
        {
            get
            {
                return true;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public override string ACCaption
        {
            get
            {
                if (ContentACClassWF == null)
                {
                    if (ACType != null)
                        return ACType.ACCaption;
                    return this.ACIdentifier;
                }
                return ContentACClassWF.ACCaption;
            }
        }


        /// <summary>
        /// Gets the ParentACComponent as IACComponentPWGroup. 
        /// If this Instance is a PWProcessFunction, than null will be returned.
        /// </summary>
        public IACComponentPWGroup GroupPWComponent
        {
            get
            {
                return ParentACComponent as IACComponentPWGroup;
            }
        }
        
        #endregion


        #region ACProgram
        protected ACProgramLog _CurrentProgramLog;
        public override ACProgramLog CurrentProgramLog
        {
            get
            {
                return GetCurrentProgramLog(CurrentACState != ACStateEnum.SMIdle, IsNodeSkipping);
            }
        }

        public bool IsNodeSkipping
        {
            get
            {
                ACProgramLog currentProgramLog = null;
                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    currentProgramLog = _CurrentProgramLog;
                }
                return currentProgramLog == null && LastACState == ACStateEnum.SMStarting && CurrentACState == ACStateEnum.SMCompleted;
            }
        }

        protected override ACProgramLog GetCurrentProgramLog(bool attach, bool lookupOnlyInCache = false)
        {
            using (ACMonitor.Lock(this._20015_LockValue))
            {
                if (_CurrentProgramLog != null)
                    return _CurrentProgramLog;
            }

            ACProgramLog currentProgramLog = null;
            if (attach)
            {
                ACProgramLog parentProgramLog = ParentProgramLog;

                if (parentProgramLog != null)
                {
                    string acUrl = this.GetACUrl();
                    currentProgramLog = ACClassTaskQueue.TaskQueue.ProgramCache.GetCurrentProgramLog(parentProgramLog, acUrl, lookupOnlyInCache);
                    if (currentProgramLog == null
                        && CurrentACState > ACStateEnum.SMStarting
                        && LastACState > ACStateEnum.SMStarting) // If Node was skipped, because it never switched to Running, don't log error
                        Messages.LogError(this.GetACUrl(), String.Format("GetCurrentProgramLog(0)"), String.Format("ACProgramLogID {0} has no childs; CurrentACState {1}; LastACState {2}", parentProgramLog.ACProgramLogID, CurrentACState, LastACState));
                }
            }

            using (ACMonitor.Lock(this._20015_LockValue))
            {
                if (_CurrentProgramLog == null && currentProgramLog != null)
                    _CurrentProgramLog = currentProgramLog;
                return _CurrentProgramLog;
            }
        }

        protected override void SetCurrentProgramLog(ACProgramLog value, bool detach)
        {
            ACProgramLog programLog;

            using (ACMonitor.Lock(this._20015_LockValue))
            {
                programLog = _CurrentProgramLog;
            }


            if (detach && programLog != null && programLog != value)
            {
                ACClassTaskQueue.TaskQueue.ProgramCache.RemoveProgramLog(programLog);
            }

            if (value != null)
                ACClassTaskQueue.TaskQueue.ProgramCache.AddProgramLog(value, programLog == null);
            else if (detach && programLog == null)
            {
                PWProcessFunction pwFunction = ParentRootWFNode as PWProcessFunction;
                if (pwFunction != null)
                {
                    var acProgram = pwFunction.GetCurrentACProgram(false);
                    if (acProgram != null)
                    {
                        string acUrl = GetACUrl();
                        if (!String.IsNullOrEmpty(acUrl))
                            ACClassTaskQueue.TaskQueue.ProgramCache.RemoveProgramLog(acProgram, acUrl);
                    }
                }
            }

            using (ACMonitor.Lock(this._20015_LockValue))
            {
                _CurrentProgramLog = value;
            }
        }

        internal override string DumpReasonCurrentPLIsNull()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("CurrentACState: {0} @ {1}", this.CurrentACState, this.GetACUrl()));
            if (_CurrentProgramLog == null)
            {
                sb.AppendLine(String.Format("=> PWBase._CurrentProgramLog is null at {0}", this.GetACUrl()));
                ACProgramLog parentProgramLog = ParentProgramLog;
                if (parentProgramLog == null)
                {
                    sb.AppendLine(String.Format("=> ParentProgramLog is null at {0}", this.GetACUrl()));
                    PABase paBase = ParentACComponent as PABase;
                    if (paBase != null)
                    {
                        sb.Append(paBase.DumpReasonCurrentPLIsNull());
                    }
                }
                else
                    sb.AppendLine("ACClassTaskQueue.TaskQueue.ProgramCache.GetCurrentProgramLog not succeeded");
            }
            return sb.ToString();
        }

        /// <summary>
        /// NO-CHANGE-TRACKING!
        /// </summary>
        public override IEnumerable<ACProgramLog> PreviousProgramLogs
        {
            get
            {
                IEnumerable<ACProgramLog> previousLogs = new ACProgramLog[] { };
                ACProgramLog parentProgramLog = ParentProgramLog;
                if (parentProgramLog == null)
                    parentProgramLog = PreviousParentProgramLog; // NO-CHANGE-TRACKING!

                if (parentProgramLog != null)
                {
                    string acUrl = this.GetACUrl();
                    previousLogs = ACClassTaskQueue.TaskQueue.ProgramCache.GetPreviousLogsFromParentLog(parentProgramLog.ACProgramLogID, acUrl);
                }

                return previousLogs;
            }
        }


        public override ACProgramLog ParentProgramLog
        {
            get
            {
                PABase parentPABase = ParentACComponent as PABase;

                if (parentPABase == null)
                    return null;
                else
                    return parentPABase.CurrentProgramLog;
            }
        }

        /// <summary>
        /// NO-CHANGE-TRACKING!
        /// </summary>
        public override ACProgramLog PreviousParentProgramLog
        {
            get
            {
                PABase parentPABase = ParentACComponent as PABase;
                if (parentPABase == null)
                    return null;
                return parentPABase.PreviousProgramLogs.LastOrDefault();
            }
        }

        public PAProcessModule PreviousAccessedPM
        {
            get
            {
                var prevLogs = PreviousProgramLogs;
                if (prevLogs == null || !prevLogs.Any())
                    return null;
                ACProgramLog prevLog = prevLogs.LastOrDefault();
                if (prevLog == null || !prevLog.RefACClassID.HasValue)
                {
                    prevLog = prevLogs.Where(c => c.RefACClassID.HasValue).LastOrDefault();
                    if (prevLog == null)
                        return null;
                }
                return this.ApplicationManager.FindChildComponents<PAProcessModule>(c => c is PAProcessModule && c.ACType.ACTypeID == prevLog.RefACClassID.Value, c => c is PWBase, 3).FirstOrDefault();
            }
        }
        #endregion


        #region Network-Properties
        
        [ACPropertyBindingSource(210, "Error", "en{'Process alarm'}de{'Prozess-Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> ProcessAlarm { get; set; }
        public const string PropNameProcessAlarm = "ProcessAlarm";

        [ACPropertyBindingSource(211, "Error", "en{'Has Rules'}de{'Hat Regeln'}", "", false, false)]
        public IACContainerTNet<short> HasRules { get; set; }
        
        #endregion


        #region IACComponentPWNode

        /// <summary>
        /// Root-Workflownode of type PWProcessFunction
        /// </summary>
        /// <value>Root-Workflownode of type PWProcessFunction</value>
        public IACComponentPWNode ParentRootWFNode
        {
            get
            {
                return FindParentComponent<PWProcessFunction>(c => c is PWProcessFunction);
            }
        }

        /// <summary>
        /// Returns the Workflow-Context (Property ContentTask.ACProgram) for reading and saving the configuration-data of a workflow.
        /// </summary>
        /// <value>The Workflow-Context</value>
        public IACWorkflowContext WFContext
        {
            get
            {
                ACClassTask contentTask = ContentTask;
                if (contentTask == null)
                    return null;

                ACProgram acProgram = null;
                if (contentTask.ACProgram_IsLoaded)
                    acProgram = contentTask.ACProgram;
                if (acProgram == null)// && (contentTask.EntityState == System.Data.EntityState.Added || contentTask.EntityState == System.Data.EntityState.Detached))
                    acProgram = contentTask.NewACProgramForQueue;
                if (acProgram == null)
                    ACClassTaskQueue.TaskQueue.ProcessAction(() => { acProgram = contentTask.ACProgram; });
                if (acProgram != null)
                    return acProgram;

                ACClassMethod wfContext = null;
                ACClassWF contentACClassWF = ContentACClassWF;
                if (contentACClassWF == null)
                    return null;
                if (contentACClassWF.ACClassMethod_IsLoaded)
                    wfContext = contentACClassWF.ACClassMethod;
                if (wfContext == null)
                {
                    using (ACMonitor.Lock(this.ContextLockForACClassWF))
                    {
                        wfContext = ContentACClassWF.ACClassMethod;
                    }
                }
                return wfContext;
            }
        }

        /// <summary>
        /// XAML-Code for Presentation
        /// </summary>
        /// <value>
        /// XAML-Code for Presentation
        /// </value>
        public string XMLDesign
        {
            get
            {
                if (ContentACClassWF == null)
                    return null;
                using (ACMonitor.Lock(this.ContextLockForACClassWF))
                {
                    ContentACClassWF.ACClassMethod.AutoRefresh();
                }
                return ContentACClassWF.ACClassMethod.XMLDesign;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsWorkflowConsistent
        {
            get
            {
                if (ContentTask == null || WFInitPhase == WFInstantiatonPhase.NewWF_Creating)
                    return false;

                EntityState entityState = EntityState.Unchanged;
                entityState = ContentTask.EntityState;
                //ACClassTaskQueue.TaskQueue.ProcessAction(() => { entityState = ContentTask.EntityState; });

                // All entities must be saved to database first because when other entites from application-datamodel need references to the ACClassTask from its tables
                // e.g. task.ProdOrderPartslistPos_ACClassTask. Therefore entityState mus always be Unchanged or modified
                if (entityState != EntityState.Unchanged && entityState != EntityState.Modified)
                    return false;

                foreach (var child in ACComponentChilds)
                {
                    PWBase childPW = child as PWBase;
                    if (childPW != null && !childPW.IsWorkflowConsistent)
                        return false;
                }
                return true;
            }
        }

        #endregion


        #region IACConfigURL

        public virtual string ConfigACUrl
        {
            get
            {
                if (this.ParentACObject is IACConfigURL)
                {
                    return (ParentACObject as IACConfigURL).ConfigACUrl + "\\" + ContentACClassWF.ACIdentifier;
                }
                else
                {
                    return ContentACClassWF.ACIdentifier;
                }
            }
        }

        public virtual string PreValueACUrl
        {
            get
            {
                return (RootPW as IACConfigURL).PreValueACUrl;
            }
        }

        public string LocalConfigACUrl
        {
            get
            {
                return ACUrlHelper.BuildLocalConfigACUrl(this);
            }
        }


        public virtual void RefreshRuleStates()
        {
        }

        #endregion


        #region IACConfigMethodHierarchy

        /// <summary>
        /// Since IACComponentPWNode inherits from the IACConfigURL interface, the workflow instance itself knows from which "parent workflow" the "subworkflow", to which the workflow instance belongs, was called. 
        /// The "call stack" - that is the call sequence of workflows - provides the property ACConfigMethodHierarchy from this interface.
        /// </summary>
        public virtual List<ACClassMethod> ACConfigMethodHierarchy
        {
            get
            {
                return (RootPW as IACConfigMethodHierarchy).ACConfigMethodHierarchy;
            }
        }
        
        #endregion


        #region IACConfigStoreSelection

        public virtual List<IACConfigStore> MandatoryConfigStores
        {
            get
            {
                var rootPW = RootPW;
                if (rootPW == null)
                    return new List<IACConfigStore>();
                return rootPW.MandatoryConfigStores;
            }
        }

        public virtual bool ValidateExpectedConfigStores(bool autoReloadConfig = true)
        {
            var rootPW = RootPW;
            if (rootPW == null)
                return false;
            return rootPW.ValidateExpectedConfigStores(autoReloadConfig);
        }

        public virtual IACConfigStore CurrentConfigStore
        {
            get { return null; }
        }

        public virtual bool IsReadonly
        {
            get { return true; }
        }

        #endregion

        #endregion


        #region Methods

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case ACStateConst.TMReset:
                    Reset();
                    return true;
                case nameof(GetPAOrderInfo):
                    result = GetPAOrderInfo();
                    return true;
                case nameof(GetACConfigStoreInfo):
                    result = GetACConfigStoreInfo();
                    return true;
                case nameof(AreChildsOfTypeCompleted):
                    result = AreChildsOfTypeCompleted(acParameter[0] as ACRef<ACClass>);
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMReset:
                    result = IsEnabledReset();
                    return true;
                case nameof(GetConfigForACMethod):
                    if (acParameter.Count() > 2)
                    {
                        object[] dynParam = new object[acParameter.Count() - 2];
                        for (int i = 2; i < acParameter.Count(); i++)
                            dynParam[i - 2] = acParameter[i];

                        result = GetConfigForACMethod(acParameter[0] as ACMethod, (bool)acParameter[1], dynParam);
                    }
                    else
                        result = GetConfigForACMethod(acParameter[0] as ACMethod, (bool)acParameter[1]);
                    return true;
                case nameof(AfterConfigForACMethodIsSet):
                    if (acParameter.Count() > 2)
                    {
                        object[] dynParam = new object[acParameter.Count() - 2];
                        for (int i = 2; i < acParameter.Count(); i++)
                            dynParam[i - 2] = acParameter[i];

                        result = AfterConfigForACMethodIsSet(acParameter[0] as ACMethod, (bool)acParameter[1], dynParam);
                    }
                    else
                        result = AfterConfigForACMethodIsSet(acParameter[0] as ACMethod, (bool)acParameter[1]);
                    return true;
                case nameof(OnACMethodSended):
                    if (acParameter.Count() > 2)
                    {
                        object[] dynParam = new object[acParameter.Count() - 2];
                        for (int i = 2; i < acParameter.Count(); i++)
                            dynParam[i - 2] = acParameter[i];

                        OnACMethodSended(acParameter[0] as ACMethod, (bool)acParameter[1], dynParam);
                    }
                    else
                        OnACMethodSended(acParameter[0] as ACMethod, (bool)acParameter[1]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PWBase(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case Const.AskUserPrefix + "Reset":
                    result = AskUserReset(acComponent);
                    return true;
            }
            //return HandleExecuteACMethod_PWMethodVBBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
            return false;
        }
        #endregion


        #region State-Methods

        public override bool IsEnabledSubscribeToProjectWorkCycle()
        {
            bool isEnabled = base.IsEnabledSubscribeToProjectWorkCycle();
            if (isEnabled)
            {
                if (RootPW != null && RootPW.IsUnloadingWorkflow)
                    isEnabled = false;
            }
            return isEnabled;
        }

        [ACMethodInteraction("Process", "en{'Reset'}de{'Reset'}", 302, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Reset()
        {
            CurrentACState = ACStateEnum.SMIdle;
            //SetCurrentProgramLog(null, true);
            AcknowledgeAllAlarms();

            string user = "Service";
            if (Root.CurrentInvokingUser != null)
                user = Root.CurrentInvokingUser.VBUserName;
            Messages.LogDebug(this.GetACUrl(), "Reset()", user);
        }

        public virtual bool IsEnabledReset()
        {
            return true;
            //if (Root.CurrentInvokingUser != null && Root.CurrentInvokingUser.IsSuperuser)
            //    return true;
            //return false;
        }

        public static bool AskUserReset(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            return acComponent.Messages.Question(acComponent, "Question50018", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }


        [ACMethodInfo("Query", "en{'AreChildsOfTypeCompleted'}de{'AreChildsOfTypeCompleted'}", 9999)]
        public PWBaseChildsCompResult AreChildsOfTypeCompleted(ACRef<ACClass> acClassToFind)
        {
            PWBaseChildsCompResult result = new PWBaseChildsCompResult();
            if (acClassToFind == null)
                return result;
            if (!acClassToFind.IsObjLoaded)
            {
                acClassToFind.AttachTo(gip.core.datamodel.Database.GlobalDatabase);
            }
            if (!acClassToFind.IsObjLoaded)
                return result;

            var listOfChilds = FindChildComponents(acClassToFind.ValueT.ObjectType);
            if (listOfChilds != null && listOfChilds.Any())
            {
                result.ChildsFoundCount = listOfChilds.Count;
                foreach (IACComponent child in listOfChilds)
                {
                    PWBaseExecutable pwChild = child as PWBaseExecutable;
                    if (pwChild != null)
                    {
                        if ((pwChild.CurrentACState == ACStateEnum.SMIdle || pwChild.CurrentACState == ACStateEnum.SMBreakPoint) && pwChild.IterationCount.ValueT > 0)
                        {
                            result.ChildsCompletedCount++;
                        }
                        else if (pwChild.CurrentACState != ACStateEnum.SMIdle && pwChild.CurrentACState != ACStateEnum.SMBreakPoint)
                        {
                            result.ChildsRunning++;
                        }
                    }
                }
            }
            return result;
        }

        [ACMethodInfo("", "en{'Get current order informations'}de{'Informationen über aktuellen Auftrag'}", 9999)]
        public virtual PAOrderInfo GetPAOrderInfo()
        {
            PWBase pwParent = ParentACComponent as PWBase;
            if (pwParent != null)
                return pwParent.GetPAOrderInfo();
            return null;
        }

        #endregion


        #region ACMethod and Config-Store
        /// <summary>Fills Parameterlist in ACMethod with values from Config-Store-Hierarchy
        /// Derivations of PWClasses can manipulate the Paramterlist as well according to their individual logic</summary>
        /// <param name="paramMethod">ACMethod to fill</param>
        /// <param name="isForPAF">If its a acMethod which will be passed to the Start-Method of a PAPocessFunction else it's the local configuration for this PWNode</param>
        /// <param name="acParameter"></param>
        [ACMethodInfo("", "en{'GetConfigForACMethod'}de{'GetConfigForACMethod'}", 9999)]
        public virtual bool GetConfigForACMethod(ACMethod paramMethod, bool isForPAF, params Object[] acParameter)
        {
            string configACUrl = ConfigACUrl;
            configACUrl += "\\" + paramMethod.ACIdentifier;

            // 1. Get reference to local service instance of IACConfigProvider
            ConfigManagerIPlus serviceInstance = ConfigManagerIPlus.GetServiceInstance(this);
            if (serviceInstance == null)
            {
                Messages.LogError(this.GetACUrl(), String.Format("GetConfigForACMethod(0)"), "serviceInstance is null");
                return false;
            }

            PWGroup pwGroup = this as PWGroup;
            if (pwGroup == null)
                pwGroup = ParentACComponent as PWGroup;
            Guid? vbiACClassID = null;
            if (pwGroup != null && pwGroup.AccessedProcessModule != null)
                vbiACClassID = pwGroup.AccessedProcessModule.ACType.ACTypeID;

            // 2. Fill ParameterValueList of passed ACMethod with configured values according override direction of IACConfig-Stores (MandatoryConfigStores)
            HasRules.ValueT = serviceInstance.WriteConfigIntoACValue(paramMethod, MandatoryConfigStores, PreValueACUrl, configACUrl, vbiACClassID, true);
            return true;
        }


        [ACMethodInfo("", "en{'AfterConfigForACMethodIsSet'}de{'AfterConfigForACMethodIsSet'}", 9999)]
        public virtual bool AfterConfigForACMethodIsSet(ACMethod paramMethod, bool isForPAF, params Object[] acParameter)
        {
            return true;
        }


        [ACMethodInfo("", "en{'OnACMethodSended'}de{'OnACMethodSended'}", 9999)]
        public virtual void OnACMethodSended(ACMethod paramMethod, bool newTaskStarted, params Object[] acParameter)
        {
        }

        [ACMethodInfo("", "en{'GetACConfigStoreInfo'}de{'GetACConfigStoreInfo'}", 9999)]
        public List<ACConfigStoreInfo> GetACConfigStoreInfo()
        {
            var stores = MandatoryConfigStores;
            if (stores != null)
            {
                return stores
                        .Where(c => c is VBEntityObject)
                        .Select(c => new ACConfigStoreInfo((c as VBEntityObject).EntityKey, c.OverridingOrder, this.PreValueACUrl)).ToList();
            }
            return new List<ACConfigStoreInfo>();
        }
        #endregion


        #region Searching Nodes inside a Workflow

        /// <summary>
        /// Finds other predecessors from this inside a workflow
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="inSameGroup">Search is done only inside the same group where this node belongs to</param>
        /// <param name="selector">Search condition to find the appropriate nodes</param>
        /// <param name="deselector">Break condition for search. Can be null. Hint: If deselector equals the selector, then the first matching node will be returned.</param>
        /// <param name="maxRecursionDepth">If = 0, then unlimited search depth</param>
        /// <returns></returns>
        public List<TResult> FindPredecessors<TResult>(bool inSameGroup, Func<IACComponentPWNode, bool> selector, Func<IACComponentPWNode, bool> deselector = null,
                                                       int maxRecursionDepth = 0) where TResult : IACComponentPWNode
        {
            List<TResult> foundNodes = new List<TResult>();
            List<IACComponentPWNode> visitedNodes = new List<IACComponentPWNode>();
            FindPredecessorsIntern<TResult>(ref foundNodes, ref visitedNodes, inSameGroup ? this : null, null, selector, deselector, 0, maxRecursionDepth);
            return foundNodes;
        }

        private void FindPredecessorsIntern<TResult>(ref List<TResult> foundNodes, ref List<IACComponentPWNode> visitedNodes,
                                                PWBase startNodeForSearchInSameGroupOnly, PWBase[] previousNodes, Func<IACComponentPWNode, bool> selector, Func<IACComponentPWNode, bool> deselector,
                                                int currentRecursionDepth = 0, int maxRecursionDepth = 0) where TResult : IACComponentPWNode
        {
            if (visitedNodes.Contains(this))
                return;

            visitedNodes.Add(this);

            // Must be next recursion depth because starting point is not a predecessor itself
            if (currentRecursionDepth > 0)
            {
                if (selector(this))
                {
                    if (typeof(TResult).IsAssignableFrom(this.GetType()))
                        foundNodes.Add((TResult)(object)this);
                }

                if (deselector != null && deselector(this))
                    return;
            }

            currentRecursionDepth++;
            if (maxRecursionDepth > 0 && currentRecursionDepth > maxRecursionDepth)
                return;

            IPWNodeIn pwInNode = this as IPWNodeIn;
            if (pwInNode == null)
            {
                if (this is PWNodeStart)
                {
                    PWBase parentNode = this.ParentACComponent as PWBase;
                    if (parentNode != null)
                        parentNode.FindPredecessorsIntern<TResult>(ref foundNodes, ref visitedNodes, startNodeForSearchInSameGroupOnly, new PWBase[] { this }, selector, deselector, currentRecursionDepth, maxRecursionDepth);
                }
                return;
            }
            else if (pwInNode is PWGroup && previousNodes != null)
            {
                PWGroup pwGroup = pwInNode as PWGroup;
                if (pwGroup.PWPointOut.ConnectionList.Where(c => previousNodes.Contains(c.ValueT)).Any())
                {
                    pwInNode = pwGroup.PWNodeEnd;
                    if (pwInNode == null)
                        return;
                }
            }

            var sourceComps = pwInNode.PWPointIn.ConnectionList
                .Where(c => c.ValueT is PWBase
                            && (    (startNodeForSearchInSameGroupOnly == null)
                                 || (startNodeForSearchInSameGroupOnly != null && c.ValueT.ParentACComponent == startNodeForSearchInSameGroupOnly.ParentACComponent)))
              .Select(c => c.ValueT as PWBase).ToArray();
            if (!sourceComps.Any())
                return;

            foreach (PWBase pwBase in sourceComps)
            {
                pwBase.FindPredecessorsIntern<TResult>(ref foundNodes, ref visitedNodes, startNodeForSearchInSameGroupOnly, 
                                                    this != pwInNode ? new PWBase[] { pwInNode as PWBase, this } : new PWBase[] { this }, 
                                                    selector, deselector, currentRecursionDepth, maxRecursionDepth);
            }
        }


        /// <summary>
        /// Finds other predecessors from this inside a workflow
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="inSameGroup">Search is done only inside the same group where this node belongs to</param>
        /// <param name="selector">Search condition to find the appropriate nodes</param>
        /// <param name="deselector">Break condition for search. Can be null. Hint: If deselector equals the selector, then the first matching node will be returned.</param>
        /// <param name="maxRecursionDepth">If = 0, then unlimited search depth</param>
        /// <returns></returns>
        public List<TResult> FindSuccessors<TResult>(bool inSameGroup, Func<IACComponentPWNode, bool> selector, Func<IACComponentPWNode, bool> deselector = null,
                                                       int maxRecursionDepth = 0) where TResult : IACComponentPWNode
        {
            List<TResult> foundNodes = new List<TResult>();
            List<IACComponentPWNode> visitedNodes = new List<IACComponentPWNode>();
            FindSuccessorsIntern<TResult>(ref foundNodes, ref visitedNodes, inSameGroup ? this : null, null, selector, deselector, 0, maxRecursionDepth);
            return foundNodes;
        }


        private void FindSuccessorsIntern<TResult>(ref List<TResult> foundNodes, ref List<IACComponentPWNode> visitedNodes,
                                                IACComponentPWNode startNodeForSearchInSameGroupOnly, PWBase[] previousNodes, Func<IACComponentPWNode, bool> selector, Func<IACComponentPWNode, bool> deselector,
                                                int currentRecursionDepth = 0, int maxRecursionDepth = 0) where TResult : IACComponentPWNode
        {
            if (visitedNodes.Contains(this))
                return;

            visitedNodes.Add(this);

            // Must be next recursion depth because starting point is not a successor itself
            if (currentRecursionDepth > 0)
            {
                if (selector(this))
                {
                    if (typeof(TResult).IsAssignableFrom(this.GetType()))
                        foundNodes.Add((TResult)(object)this);
                }

                if (deselector != null && deselector(this))
                    return;
            }

            currentRecursionDepth++;
            if (maxRecursionDepth > 0 && currentRecursionDepth > maxRecursionDepth)
                return;

            IPWNodeOut pwOutNode = this as IPWNodeOut;
            if (pwOutNode == null)
            {
                if (this is PWNodeEnd)
                {
                    PWBase parentNode = this.ParentACComponent as PWBase;
                    if (parentNode != null)
                        parentNode.FindSuccessorsIntern<TResult>(ref foundNodes, ref visitedNodes, startNodeForSearchInSameGroupOnly, new PWBase[] { this }, selector, deselector, currentRecursionDepth, maxRecursionDepth);
                }
                return;
            }
            else if (pwOutNode is PWGroup && previousNodes != null)
            {
                PWGroup pwGroup = pwOutNode as PWGroup;
                if (pwGroup.PWPointIn.ConnectionList.Where(c => previousNodes.Contains(c.ValueT)).Any())
                {
                    pwOutNode = pwGroup.PWNodeStart;
                    if (pwOutNode == null)
                        return;
                }
            }

            var targetComps = pwOutNode.PWPointOut.ConnectionList
                            .Where(c => c.ValueT is IACComponentPWNode
                                        && (   (startNodeForSearchInSameGroupOnly == null) 
                                            || (startNodeForSearchInSameGroupOnly != null && c.ValueT.ParentACComponent == startNodeForSearchInSameGroupOnly.ParentACComponent)))
                          .Select(c => c.ValueT as IACComponentPWNode).ToArray();
            if (!targetComps.Any())
                return;

            foreach (IACComponentPWNode pwBase in targetComps)
            {
                (pwBase as PWBase).FindSuccessorsIntern<TResult>(ref foundNodes, ref visitedNodes, startNodeForSearchInSameGroupOnly, 
                                                                this != pwOutNode ? new PWBase[] { pwOutNode as PWBase, this } : new PWBase[] { this }, 
                                                                selector, deselector, currentRecursionDepth, maxRecursionDepth);
            }
        }
        #endregion


        #region Diagnostics and Dump
        
        public void ActivateProcessAlarmWithLog(Msg msg, bool autoAck = true)
        {
            if (IsAlarmActive(ProcessAlarm, msg.Message) == null)
                Messages.LogMessageMsg(msg);
            ActivateProcessAlarm(msg, autoAck);
        }

        public void ActivateProcessAlarm(Msg msg, bool autoAck = true)
        {
            OnNewAlarmOccurred(ProcessAlarm, msg, autoAck);
            ProcessAlarm.ValueT = PANotifyState.AlarmOrFault;
        }


        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList, ref DumpStats dumpStats)
        {
            base.DumpPropertyList(doc, xmlACPropertyList, ref dumpStats);

            XmlElement xmlIsRestart = xmlACPropertyList["WFInitPhase"];
            if (xmlIsRestart == null)
            {
                xmlIsRestart = doc.CreateElement("WFInitPhase");
                if (xmlIsRestart != null)
                    xmlIsRestart.InnerText = WFInitPhase.ToString();
                xmlACPropertyList.AppendChild(xmlIsRestart);
            }

            XmlElement wfInfos = xmlACPropertyList["ContentACClassWFInfo"];
            if (wfInfos == null && ContentACClassWF != null)
            {
                wfInfos = doc.CreateElement("ContentACClassWFInfo");
                if (wfInfos != null)
                {
                    if (ContentACClassWF != null)
                        wfInfos.InnerText = String.Format("ACClassWFID:{0}, PWACClassID:{1}, RefPAACClassID:{2}, RefPAACClassMethodID:{3}", ContentACClassWF.ACClassWFID, ContentACClassWF.PWACClassID, ContentACClassWF.RefPAACClassID, ContentACClassWF.RefPAACClassMethodID);
                    else
                        wfInfos.InnerText = "null";
                }
                xmlACPropertyList.AppendChild(wfInfos);
            }

            XmlElement xmlNode = xmlACPropertyList["CurrentProgramLog"];
            if (xmlNode == null)
            {
                xmlNode = doc.CreateElement("CurrentProgramLog");
                if (xmlNode != null)
                    xmlNode.InnerText = _CurrentProgramLog == null ? "null" : _CurrentProgramLog.ACProgramLogID.ToString();
                xmlACPropertyList.AppendChild(xmlNode);
            }

            xmlNode = xmlACPropertyList["MandatoryConfigStores"];
            if (xmlNode == null)
            {
                xmlNode = doc.CreateElement("MandatoryConfigStores");
                if (xmlNode != null)
                    xmlNode.InnerText = DumpMandatoryConfigStores();
                xmlACPropertyList.AppendChild(xmlNode);
            }
        }

        public string DumpMandatoryConfigStores()
        {
            StringBuilder sb = new StringBuilder();
            List<ACConfigStoreInfo> storeInfoList = GetACConfigStoreInfo();
            if (storeInfoList != null && storeInfoList.Any())
            {
                foreach (ACConfigStoreInfo storeInfo in storeInfoList)
                {
                    sb.AppendLine(storeInfo.ToString());
                }
            }
            return sb.ToString();
        }

        #endregion

        #endregion
    }

    [ACSerializeableInfo]
    [DataContract]
    public class PWBaseChildsCompResult
    {
        [DataMember]
        public int ChildsFoundCount { get; set; }
        [DataMember]
        public int ChildsCompletedCount { get; set; }
        [DataMember]
        public int ChildsRunning { get; set; }

        public bool AllCompleted
        {
            get
            {
                return ChildsFoundCount > 0
                    && (ChildsCompletedCount + ChildsRunning) >= ChildsFoundCount;
            }
        }
    }
}

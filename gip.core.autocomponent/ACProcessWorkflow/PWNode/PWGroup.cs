using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Xml;
using System.Diagnostics;

namespace gip.core.autocomponent
{

    public delegate void ProcessModuleChangedEventHandler(object sender, ProcessModuleChangedArgs e);

    public class ProcessModuleChangedArgs : EventArgs
    {
        public ProcessModuleChangedArgs(PAProcessModule module, bool removed)
        {
            Module = module;
            Removed = removed;
        }

        public PAProcessModule Module
        {
            get; private set;
        }

        public bool Removed
        {
            get; private set;
        }
    }

    /// <summary>
    ///   <para>
    /// PWGroup is on the one hand a derivative of PWBaseExecutable and on the other hand it implements the IACComponentPWGroup interface. </para>
    ///   <para>Like a PWProcessFunction, it is therefore able to have child workflow nodes. 
    ///   It contains a start and end node (PWNodeStart and PWNodeEnd) and should contain at least one node of the type PWNodeProcessMethod. 
    ///   This is because PWNodeProcessMethod-classes call PAProcessFunction's asynchronously in the physical model. 
    ///   These calls are only allowed, when the associated process module has been occupied by the PWGroup instance by setting the "PAProcessModule.Semaphore" service point with the "PWGroup.TrySemaphore" client point. 
    ///   The occupation of a process module takes place in the state "SMStarting" and will be, after all child workflow nodes have been processed and the end node has triggered, removed. 
    ///   The ACState of PWGroup will than be switched back from "SMRunning" back to the base state "SMIdle".
    /// </para>
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PWBaseExecutable" />
    /// <seealso cref="gip.core.datamodel.IACComponentPWGroup" />
    /// <seealso cref="gip.core.autocomponent.IACMyConfigCache" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWGroup'}de{'PWGroup'}", Global.ACKinds.TPWGroup, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWGroup : PWBaseExecutable, IACComponentPWGroup
    {
        public const string PWClassName = "PWGroup";

        #region c´tors

        static PWGroup()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();
            method.ParameterValueList.Add(new ACValue("IgnoreFIFO", typeof(bool), false, Global.ParamOption.Required));
            paramTranslation.Add("IgnoreFIFO", "en{'Ignore FIFO for Processmodule-Mapping'}de{'Ignoriere FIFO-Prinzip für Prozessmodul-Belegung'}");
            method.ParameterValueList.Add(new ACValue("RoutingCheck", typeof(bool), false, Global.ParamOption.Required));
            paramTranslation.Add("RoutingCheck", "en{'Only routeable modules from predecessor'}de{'Nur erreichbare Module vom Vorgänger'}");
            method.ParameterValueList.Add(new ACValue("WithoutPM", typeof(bool), false, Global.ParamOption.Required));
            paramTranslation.Add("WithoutPM", "en{'Ignore Processmodule-Mapping'}de{'Ohne Prozessmodul-Belegung'}");
            method.ParameterValueList.Add(new ACValue("OccupationByScan", typeof(bool), false, Global.ParamOption.Required));
            paramTranslation.Add("OccupationByScan", "en{'Processmodule-Mapping manually by user'}de{'Prozessmodulbelegung manuell vom Anwender'}");
            method.ParameterValueList.Add(new ACValue("Priority", typeof(ushort), 0, Global.ParamOption.Required));
            paramTranslation.Add("Priority", "en{'Priorization'}de{'Priorisierung'}");
            method.ParameterValueList.Add(new ACValue("FIFOCheckFirstPM", typeof(bool), false, Global.ParamOption.Required));
            paramTranslation.Add("FIFOCheckFirstPM", "en{'FIFO check only for WF-Groups which competes for the same process module'}de{'FIFO-Prüfung nur bei WF-Gruppen die das selbe Prozessmodul konkurrieren.'}");
            var wrapper = new ACMethodWrapper(method, "en{'Configuration'}de{'Konfiguration'}", typeof(PWGroup), paramTranslation, null);
            ACMethod.RegisterVirtualMethod(typeof(PWGroup), ACStateConst.SMStarting, wrapper);
            RegisterExecuteHandler(typeof(PWGroup), HandleExecuteACMethod_PWGroup);
        }

        public PWGroup(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _TrySemaphore = new ACPointClientACObject(this, "TrySemaphore", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            if (RootPW != null)
                PWContentTaskHelper.InitWFChilds(this, RootPW.WFDictionary);

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                _PossibleModuleList = null;
                _RoutableModuleList = null;
                _LastRuleValueList = null;
                _LastSelectedClasses = null;
                _Priority = null;
            }

            ClearMyConfiguration();
            bool result = base.ACDeInit(deleteACClassTask);

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _WaitsOnAvailableProcessModule = false;
            }

            if (deleteACClassTask)
            {
                if (TrySemaphore != null && TrySemaphore.ConnectionListCount > 0)
                    TrySemaphore.RemoveAll();
            }
            return result;
        }

        public override void Recycle(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                _PossibleModuleList = null;
                _RoutableModuleList = null;
                _LastRuleValueList = null;
                _LastSelectedClasses = null;
                _WaitsOnAvailableProcessModule = false;
                _Priority = null;
            }

            if (TrySemaphore != null && TrySemaphore.ConnectionListCount > 0)
                TrySemaphore.RemoveAll();
            base.Recycle(content, parentACObject, parameter, acIdentifier);
        }

        #endregion


        #region Points (Semaphore)
        protected ACPointClientACObject _TrySemaphore;
        [ACPropertyPoint(true, 0)]
        public ACPointClientACObject TrySemaphore
        {
            get
            {
                return _TrySemaphore;
            }
        }
        #endregion


        #region Properties

        #region Configuration

        public override void ClearMyConfiguration()
        {
            base.ClearMyConfiguration();
            using (ACMonitor.Lock(_20015_LockValue))
            {
                // Priority mustn't be reset! 
                // If _Priority was changed through SetPriority(PriorityMode mode) by a individual logic,
                // then the expected behaviour would not take place because the _Priority has been resetted when a change in the workflow-rules are done on client-side.
                //_Priority = null;
                _RoutableModuleList = null;
            }
        }


        /// <summary>
        /// Configuration-Property: 
        /// Ignores the priorization-logic which concurring PWGroups can occupy a processmodule.
        /// Default is FALSE!
        /// </summary>
        /// <value>
        ///   <c>true</c> if [ignore fifo]; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreFIFO
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("IgnoreFIFO");
                    if (acValue != null)
                    {
                        return acValue.ParamAsBoolean;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Configuration-Property: 
        /// Ignores all Processmodules that cannot be reached from a processmodule, that has been occupied by a preceding Worfklow-Group (Predecessor).
        /// Default is FALSE!
        /// </summary>
        /// <value>
        ///   <c>true</c> if [routing check]; otherwise, <c>false</c>.
        /// </value>
        public bool RoutingCheck
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("RoutingCheck");
                    if (acValue != null)
                    {
                        return acValue.ParamAsBoolean;
                    }
                }
                return false;
            }
        }


        /// <summary>
        /// Configuration-Property:
        /// Controls if this Worfklow-Group can be started without occupying a Processmodule.
        /// Default is FALSE!
        /// </summary>
        /// <value>
        ///   <c>true</c> if [without pm]; otherwise, <c>false</c>.
        /// </value>
        public bool WithoutPM
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("WithoutPM");
                    if (acValue != null)
                    {
                        return acValue.ParamAsBoolean;
                    }
                }
                return false;
            }
        }


        /// <summary>
        /// Configuration-Property:
        /// The occupation of a processmodule will be done by a scan-event.
        /// Therefore the corresponding Processmodules must have a PAFWorkTaskScanBase-Instance as a child. 
        /// The PAFWorkTaskScanBase instance calls the OccupyWithPModuleOnScan() method to put this workflow node in the running state.
        /// Default is FALSE!
        /// </summary>
        /// <value>
        ///   <c>true</c> if [occupation by scan]; otherwise, <c>false</c>.
        /// </value>
        public bool OccupationByScan
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("OccupationByScan");
                    if (acValue != null)
                    {
                        return acValue.ParamAsBoolean;
                    }
                }
                return false;
            }
        }

        public const ushort C_DefaultLowestPriority = 9999;
        /// <summary>
        /// Configuration-Property: 
        /// Sort order for priorizing. Value is between 0 and 9999.
        /// Default is 9999 if nothing is set.
        /// </summary>
        public ushort ConfiguredPriority
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("Priority");
                    if (acValue != null)
                    {
                        ushort priority = acValue.ParamAsUInt16;
                        if (priority == 0)
                            return C_DefaultLowestPriority;
                        return priority;
                    }
                }
                return 9999;
            }
        }

        protected ushort? _Priority;
        public ushort Priority
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_Priority.HasValue)
                        return _Priority.Value;
                }
                return ConfiguredPriority;
            }
            set
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _Priority = value;
                }
            }
        }

        public enum PriorityMode
        {
            ToConfigurationValue,
            ToLowestDefault,
            ToHighest,
            OccupyImmediately
        }

        public void SetPriority(PriorityMode mode)
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                if (mode == PriorityMode.ToConfigurationValue)
                    _Priority = null;
                else if (mode == PriorityMode.ToLowestDefault)
                    _Priority = C_DefaultLowestPriority;
                else if (mode == PriorityMode.ToHighest)
                    _Priority = 1;
                else //if (mode == PriorityMode.OccupyImmediately)
                    _Priority = 0;
            }
        }

        public bool FIFOCheckFirstPM
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("FIFOCheckFirstPM");
                    if (acValue != null)
                    {
                        return acValue.ParamAsBoolean;
                    }
                }
                return false;
            }
        }

        #endregion


        #region Occupation of Processmodules        
        /// <summary>
        /// Gets a value indicating wether this workflow-group should occupy a process module or not.
        /// If WithoutPM is set it returns false otherwise true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [needs a process module]; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsAProcessModule
        {
            get
            {
                if (ContentACClassWF == null || !ContentACClassWF.RefPAACClassID.HasValue)
                    return false;
                if (ACOperationMode == ACOperationModes.Test)
                    return false;
                if (IsMyConfigurationLoaded) // Don't remove it because of StackOverflow
                {
                    if (WithoutPM)
                        return false;
                }
                return true;
            }
        }

        private RuleValueList _LastRuleValueList = null;
        protected RuleValueList LastRuleValueList
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _LastRuleValueList;
                }
            }
            set
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _LastRuleValueList = value;
                }
            }
        }

        private IEnumerable<gip.core.datamodel.ACClass> _LastSelectedClasses = null;
        protected IEnumerable<gip.core.datamodel.ACClass> LastSelectedClasses
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_LastSelectedClasses == null)
                        return _LastSelectedClasses;
                    return _LastSelectedClasses.ToArray();
                }
            }
            set
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _LastSelectedClasses = value;
                }
            }
        }


        protected PAProcessModule[] _PossibleModuleList = null;
        /// <summary>
        /// Returns all processmodules that could be theoretically be occupied from this group. 
        /// It selects all processmodules that are a derivation of the class where "ContentACClassWF.RefPAACClass" points to.
        /// "ContentACClassWF.RefPAACClass" is a reference to a Class in the Application-Definition project that serves as the base class for the processmodules in the physical model(Application).
        /// </summary>
        /// <value>
        /// The possible module list.
        /// </value>
        public virtual PAProcessModule[] PossibleModuleList
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_PossibleModuleList != null)
                        return _PossibleModuleList;
                }

                PAProcessModule[] possibleModuleList;
                ACClass refPAACClass = RefACClassOfContentWF;
                if (refPAACClass == null)
                    return new PAProcessModule[0];

                Type processModuleType = refPAACClass.ObjectType;
                if (processModuleType == null)
                    return new PAProcessModule[0];

                var tempModules = ApplicationManager.ACCompTypeDict.GetComponentsOfType<PAProcessModule>(processModuleType);
                if (tempModules != null && tempModules.Any())
                    possibleModuleList = tempModules.Where(c => c.ComponentClass.IsDerivedClassFrom(refPAACClass)).ToArray();
                else
                    possibleModuleList = new PAProcessModule[0];


                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_PossibleModuleList == null)
                        _PossibleModuleList = possibleModuleList;
                    return _PossibleModuleList;
                }
            }
        }

        public void ResetCacheOfRoutableModuleList()
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                _RoutableModuleList = null;
            }
        }

        protected PAProcessModule[] _RoutableModuleList = null;
        /// <summary>
        /// Returns all processmodules that can be occupied from this group according to the routing rules. 
        /// It calls PossibleModuleList an removes all processmodules that are not in Global.OperatingMode.Automatic
        /// and excluded via Routing rules.
        /// </summary>
        public virtual PAProcessModule[] GetRoutableModuleList(bool forceRebuildCache = false)
        {
            try
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (forceRebuildCache)
                        _RoutableModuleList = null;
                    if (_RoutableModuleList != null)
                        return _RoutableModuleList;
                }

                if (!PossibleModuleList.Any())
                {
                    using (ACMonitor.Lock(_20015_LockValue))
                    {
                        _RoutableModuleList = new PAProcessModule[] { };
                        return _RoutableModuleList;
                    }
                }

                List<PAProcessModule> modulesInAutomaticMode = PossibleModuleList.Where(c => c.OperatingMode.ValueT == Global.OperatingMode.Automatic).ToList();

                // Apply rules
                if (modulesInAutomaticMode.Any())
                {
                    RuleValueList ruleValueList = null;
                    ConfigManagerIPlus serviceInstance = ConfigManagerIPlus.GetServiceInstance(this);
                    var mandantoryConfigStores = MandatoryConfigStores;
                    if (!ValidateExpectedConfigStores())
                        return new PAProcessModule[] { };
                    ruleValueList = serviceInstance.GetRuleValueList(mandantoryConfigStores, PreValueACUrl, ConfigACUrl + @"\Rules\" + ACClassWFRuleTypes.Allowed_instances.ToString());
                    if (ruleValueList != null)
                    {
                        modulesInAutomaticMode = OnApplyRoutingRules(ruleValueList, modulesInAutomaticMode);
                    }
                }

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_RoutableModuleList == null)
                    {
                        if (modulesInAutomaticMode != null && modulesInAutomaticMode.Any())
                            _RoutableModuleList = modulesInAutomaticMode.ToArray();
                        else
                            _RoutableModuleList = new PAProcessModule[] { };
                    }
                    return _RoutableModuleList;
                }
            }
            catch (Exception)
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_RoutableModuleList == null)
                        _RoutableModuleList = new PAProcessModule[] { };
                    return _RoutableModuleList;
                }
            }
        }


        /// <summary>
        /// ProcessModuleList removes all entries from PossibleModuleList that aren't in automatic mode and doesn't match the routing rules. 
        /// For applying the routing rules it reads the overridable configuration stores (MandatoryConfigStores) first. 
        /// Afterwards it removes all processmodules that can't be reached from preceding processmodules if the property "RoutingCheck" is set.
        /// </summary>
        /// <value>
        /// The process module list.
        /// </value>
        public virtual List<PAProcessModule> ProcessModuleList
        {
            get
            {
                if (ApplicationManager == null)
                    return null;

                if (!PossibleModuleList.Any())
                    return null;

                List<PAProcessModule> modulesInAutomaticMode = GetRoutableModuleList(true).ToList();

                if (!modulesInAutomaticMode.Any())
                {
                    using (ACMonitor.Lock(_20015_LockValue))
                    {
                        _LastRuleValueList = null;
                        _LastSelectedClasses = null;
                    }
                    return modulesInAutomaticMode;
                }
                else if (RoutingCheck)
                {
                    PAProcessModule apm = null;
                    PWGroup predecessorGroup = this.FindPredecessors<PWGroup>(false, c => c is PWGroup, null, 2).FirstOrDefault();
                    if (predecessorGroup != null)
                        apm = predecessorGroup.AccessedProcessModule != null ? predecessorGroup.AccessedProcessModule : predecessorGroup.PreviousAccessedPM;
                    if (apm == null && this.PWPointIn.ConnectionList.Any())
                    {
                        PWBaseInOut pwPredecessor = this.PWPointIn.ConnectionList.FirstOrDefault().ValueT as PWBaseInOut;
                        if (pwPredecessor != null && pwPredecessor.ParentPWGroup != null)
                            apm = pwPredecessor.ParentPWGroup.AccessedProcessModule != null ? pwPredecessor.ParentPWGroup.AccessedProcessModule : pwPredecessor.ParentPWGroup.PreviousAccessedPM;
                    }

                    if (apm != null && apm.ModuleDestinations.Any())
                    {
                        var filteredModules = modulesInAutomaticMode.Where(c => apm.ModuleDestinations.Contains(c.ComponentClass.ACClassID)).ToList();
                        if (filteredModules.Any())
                            modulesInAutomaticMode = filteredModules;
                        else
                            modulesInAutomaticMode = new List<PAProcessModule>();
                    }
                    else
                    {
                        // Error50151: No previous Workflowgroup found which is/was assigned to a process module
                        Msg msg = new Msg(this, eMsgLevel.Error, PWClassName, "ProcessModuleList", 1000, "Error50151");
                        OnNewAlarmOccurred(ProcessAlarm, msg, true);
                        return null;
                    }
                }
                return modulesInAutomaticMode;
            }
        }

        /// <summary>
        /// Returns a list of processmodules that is filtered by the passed rules.
        /// </summary>
        /// <param name="ruleValueList">List with routing rules</param>
        /// <param name="modulesInAutomaticMode">Unnfiltered modules list</param>
        /// <returns></returns>
        protected virtual List<PAProcessModule> OnApplyRoutingRules(RuleValueList ruleValueList, List<PAProcessModule> modulesInAutomaticMode)
        {
            IEnumerable<ACClass> selectedClasses = null;
            var lastRuleValueList = LastRuleValueList;
            var lastSelectedClasses = LastSelectedClasses;
            if (lastRuleValueList != null && lastSelectedClasses != null && lastRuleValueList.Equals(ruleValueList))
            {
                selectedClasses = lastSelectedClasses;
            }
            else
            {
                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                {
                    selectedClasses = ruleValueList.GetSelectedClasses(ACClassWFRuleTypes.Allowed_instances, gip.core.datamodel.Database.GlobalDatabase);
                    LastSelectedClasses = selectedClasses;
                }
            }
            if (selectedClasses != null && selectedClasses.Any())
            {
                var allowedComponents = selectedClasses.Select(c => c.GetACUrlComponent());
                modulesInAutomaticMode = modulesInAutomaticMode.Where(c => allowedComponents.Contains(c.GetACUrl())).ToList();
            }
            LastRuleValueList = ruleValueList;
            return modulesInAutomaticMode;
        }


        /// <summary>
        /// AvailableProcessModuleList removes all entries from ProcessModuleList that aren't occupied by other workflow-groups.
        /// </summary>
        /// <value>
        /// The available process module list.
        /// </value>
        protected virtual List<PAProcessModule> AvailableProcessModuleList
        {
            get
            {
                List<PAProcessModule> list = ProcessModuleList;
                if (list == null || list.Count <= 0)
                    return new List<PAProcessModule>();
                return list.Where(c => c.Semaphore.ConnectionListCount <= 0).ToList();
            }
        }

        protected virtual List<PAProcessModule> ScanableProcessModuleList
        {
            get
            {
                List<PAProcessModule> list = ProcessModuleList;
                if (list == null || list.Count <= 0)
                    return new List<PAProcessModule>();
                list = list.Where(c =>     c.OnGetSemaphoreCapacity() == 0 
                                        || c.Semaphore.ConnectionListCount <= 0 
                                        || c.Semaphore.ConnectionListCount < c.OnGetSemaphoreCapacity())
                           .ToList();
                List<ACComponent> occupiedModules = TrySemaphore.ConnectionList.Select(c => c.ValueT).ToList();
                list.RemoveAll(c => occupiedModules.Contains(c));
                return list;
            }
        }


        /// <summary>
        /// Returns the first element from AvailableProcessModuleList.
        /// </summary>
        /// <value>
        /// The first available process module.
        /// </value>
        protected virtual PAProcessModule FirstAvailableProcessModule
        {
            get
            {
                return AvailableProcessModuleList.FirstOrDefault();
            }
        }


        /// <summary>
        /// Undocumented. Declared for future features.
        /// </summary>
        /// <value>
        /// The process module for testmode.
        /// </value>
        public virtual PAProcessModule ProcessModuleForTestmode
        {
            get
            {
                List<PAProcessModule> list = ProcessModuleList;
                if (list.Count <= 0)
                    return null;
                return list.First();
            }
        }


        /// <summary>
        /// Returns the process module that has been occupied from this workflow-group.
        /// (Returns the first entry from TrySemaphore.ConnectionList)
        /// </summary>
        /// <value>
        /// The accessed process module.
        /// </value>
        public PAProcessModule AccessedProcessModule
        {
            get
            {
                if (!NeedsAProcessModule)
                    return null;
                ACPointNetWrapObject<ACComponent> semaphoreEntry = null;
                using (ACMonitor.Lock(TrySemaphore.LockConnectionList_20040))
                {
                    semaphoreEntry = TrySemaphore.ConnectionList.FirstOrDefault();
                }
                if (semaphoreEntry == null)
                    return null;
                PAProcessModule accessedProcessModule = GetProcessModuleFromEntry(semaphoreEntry);
                return accessedProcessModule;
            }
        }

        private Guid _LastSemaphoreReqIDError = Guid.Empty;
        private PAProcessModule GetProcessModuleFromEntry(ACPointNetWrapObject<ACComponent> semaphoreEntry)
        {
            PAProcessModule accessedProcessModule = semaphoreEntry.ValueT as PAProcessModule;
            if (accessedProcessModule != null)
            {
                ACPointNetWrapObject<ACComponent> entryInModule = accessedProcessModule.Semaphore.GetWrapObject(semaphoreEntry);
                if (entryInModule == null)
                {
                    bool logError = false;
                    using (ACMonitor.Lock(this._20015_LockValue))
                    {
                        logError = semaphoreEntry.RequestID != _LastSemaphoreReqIDError;
                        if (logError)
                            _LastSemaphoreReqIDError = semaphoreEntry.RequestID;
                    }
                    if (logError)
                        Messages.LogError(this.GetACUrl(), "AccessedProcessModule.get", String.Format("Process-Module {0} was resetted manually while it was occupied by this WF-Group!", accessedProcessModule.GetACUrl()));
                    return null;
                }
            }
            return accessedProcessModule;
        }


        /// <summary>
        /// Returns a list of all competing workflow group instances with the same ContentACClassWF.RefPAACClass sorted by the start date of their root workflow node (PWProcessFunction).
        /// ("ContentACClassWF.RefPAACClass" is a reference to a class in the Application Definition project that serves as the base class for the process modules in the physical model).
        /// </summary>
        /// <value>
        /// The priorized list.
        /// </value>
        public IEnumerable<PWGroup> PriorizedCompetingWFNodes
        {
            get
            {
                IEnumerable<PWGroup> allNodes = this.ApplicationManager.ACCompTypeDict.GetComponentsOfType<PWGroup>(true);
                if (allNodes == null || !allNodes.Any())
                    return allNodes;
                var queryWaitingNodes = allNodes.Where(c => c.IterationCount.ValueT <= 0
                                                            && c.InitState == ACInitState.Initialized
                                                            //&& c.AccessedProcessModule == null
                                                            && c.IsConsiderableInFIFOQueue
                                                            && (c.CurrentACState == ACStateEnum.SMIdle || c.CurrentACState == ACStateEnum.SMBreakPoint || c.CurrentACState == ACStateEnum.SMBreakPointStart || c.CurrentACState == ACStateEnum.SMStarting) // Noch nicht gemappt auf AccessedProcessModule
                                                            && (c.ContentACClassWF.RefPAACClassID == this.ContentACClassWF.RefPAACClassID)
                                                            && c.RootPW != null
                                                            && c.RootPW.TimeInfo.ValueT != null
                                                            && c.RootPW.TimeInfo.ValueT.ActualTimes != null
                                                            && c.RootPW.CurrentACProgram != null
                                                            && c.RootPW.CurrentACState != ACStateEnum.SMIdle)
                                                            .OrderBy(c => c.Priority)
                                                            .ThenBy(c => c.RootPW.TimeInfo.ValueT.ActualTimes.StartTime) // Sortierung nur nach Startdatum des Root-Knotens. Der Batch der zuerst gestartet wurde muss auch zuerst durchlaufen
                                                                                                                          // Falsch:
                                                                                                                          //.OrderBy(c => c.RootPW.CurrentACProgram.InsertDate) // 1. Sortierung nach gestarteten Programmen
                                                                                                                          //.ThenBy(c => c.RootPW.TimeInfo.ValueT.ActualTimes.StartTime) // 2. Sortierung nach Startdatum innerhalb eines Programmes
                                        .ToArray();
                return queryWaitingNodes;
            }
        }


        /// <summary>
        /// This property returns true if this workflow group has the highest priority to occupy a process module first. 
        /// To do this, all competing workflow group instances with the same ContentACClassWF.RefPAACClass are entered in a list sorted by the start date of their root workflow node (PWProcessFunction) 
        /// ("ContentACClassWF.RefPAACClass" is a reference to a class in the Application Definition project that serves as the base class for the process modules in the physical model).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has highest priority for mapping; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasHighestPriorityForMapping
        {
            get
            {
                return OnHasHighestPriorityForMapping(FirstAvailableProcessModule);
            }
        }

        protected virtual bool OnHasHighestPriorityForMapping(PAProcessModule firstModule)
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                if (_Priority.HasValue && _Priority.Value == 0)
                    return true;
            }
            if (IgnoreFIFO)
                return true;
            var queryWaitingNodes = PriorizedCompetingWFNodes;
            if (queryWaitingNodes != null && queryWaitingNodes.Any())
            {
                //PWGroup priorGroup = queryWaitingNodes.FirstOrDefault();
                //// Falls dieser Knoten nicht selbst die höchste Prio hat, dann prüfe ob es sich nur um paralleliserte Knoten aus dem selben Workflow handelt 
                //if (priorGroup != null && priorGroup != this)
                //{
                //    // Falls es andere Gruppe aus anderen Workflows gibt, dann warte
                //    if (queryWaitingNodes.Where(c => c.RootPW != this.RootPW).Any())
                //        return false;
                //}

                foreach (PWGroup priorGroup in queryWaitingNodes)
                {
                    if (priorGroup == null)
                        continue;
                    if (priorGroup == this)
                        return true;
                    if (priorGroup != this)
                    {
                        // Falls es andere Gruppen aus anderen Workflows gibt, dann warte
                        if (priorGroup.RootPW != this.RootPW)
                        {
                            if (firstModule != null 
                                && FIFOCheckFirstPM 
                                && priorGroup.FIFOCheckFirstPM)
                            {
                                if (priorGroup.FirstAvailableProcessModule != firstModule)
                                    continue;
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Gets a value indicating whether this instance is considerable in fifo queue.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is considerable in fifo queue; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsConsiderableInFIFOQueue
        {
            get
            {
                if (this.InitState != ACInitState.Initialized)
                    return false;
                if (WithoutPM)
                    return false;
                return !IgnoreFIFO;
            }
        }
        #endregion


        #region State-Dependant and Substate
        protected bool _WaitsOnAvailableProcessModule = false;

        [ACPropertyBindingSource(9999, "", "en{'Substate of a process object'}de{'Unterzustand eines Prozessobjekts'}", "", false, true)]
        public IACContainerTNet<uint> ACSubState { get; set; }

        public uint CurrentACSubState
        {
            get
            {
                if (ACSubState == null)
                    return 0;
                return ACSubState.ValueT;
            }
            set
            {
                ACSubState.ValueT = value;
            }
        }

        public virtual bool IsInSkippingMode
        {
            get
            {
                return false;
            }
        }

        public virtual bool MustRepeatGroupAtEnd
        {
            get
            {
                return false;
            }
        }

        public override bool MustBeInsidePWGroup
        {
            get
            {
                return false;
            }
        }

        #endregion


        #region Misc
        public PWNodeStart PWNodeStart
        {
            get
            {
                var query = this.ACComponentChilds.Where(c => c is PWNodeStart);
                if (query.Count() != 1)
                    return null;
                return query.First() as PWNodeStart;
            }
        }
        #endregion

        #endregion


        #region Methods

        #region ACState

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(GroupComplete):
                    GroupComplete();
                    return true;
                case nameof(SMRunning):
                    SMRunning();
                    return true;
                case nameof(SMCompleted):
                    SMCompleted();
                    return true;
                case nameof(ResetSubState):
                    ResetSubState();
                    return true;
                case nameof(ChangePriority):
                    ChangePriority((short)acParameter[0]);
                    return true;
                case nameof(OccupyWithPModuleOnScan):
                    result = OccupyWithPModuleOnScan(acParameter[0] as string);
                    return true;
                case nameof(IsEnabledGroupComplete):
                    result = IsEnabledGroupComplete();
                    return true;
                case nameof(IsEnabledResetSubState):
                    result = IsEnabledResetSubState();
                    return true;
                case nameof(GetScanableProcessModuleList):
                    result = GetScanableProcessModuleList();
                    return true;
                case nameof(CanScanAndOccupyProcessModules):
                    result = CanScanAndOccupyProcessModules();
                    return true;
                case nameof(ReleaseProcessModule):
                    result = ReleaseProcessModule(acParameter[0] as string);
                    return true;
                case nameof(GetReleaseableProcessModuleList):
                    result = GetReleaseableProcessModuleList();
                    return true;
                case nameof(CanReleaseProcessModules):
                    result = CanReleaseProcessModules();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PWGroup(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(SetPriorityClient):
                    SetPriorityClient(acComponent);
                    return true;
                case nameof(IsEnabledSetPriorityClient):
                    result = IsEnabledSetPriorityClient(acComponent);
                    return true;
                case nameof(ShowScanAndOccupyProcessModules):
                    ShowScanAndOccupyProcessModules(acComponent);
                    return true;
                case nameof(IsEnabledShowScanAndOccupyProcessModules):
                    result = IsEnabledShowScanAndOccupyProcessModules(acComponent);
                    return true;
                case nameof(ShowReleaseableProcessModules):
                    ShowReleaseableProcessModules(acComponent);
                    return true;
                case nameof(IsEnabledShowReleaseableProcessModules):
                    result = IsEnabledShowReleaseableProcessModules(acComponent);
                    return true;
            }
            return HandleExecuteACMethod_PWBaseExecutable(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        public override void SetBreakPoint()
        {
            if (IsEnabledSetBreakPoint())
            {
                if (CurrentACState == ACStateEnum.SMStarting)
                    CurrentACState = ACStateEnum.SMBreakPointStart;
                else
                    CurrentACState = ACStateEnum.SMBreakPoint;
            }
        }

        public override bool IsEnabledSetBreakPoint()
        {
            return CurrentACState == ACStateEnum.SMIdle || CurrentACState == ACStateEnum.SMStarting;
        }

        public override void Reset()
        {
            ClearMyConfiguration();
            PAProcessModule module = AccessedProcessModule;
            if (NeedsAProcessModule && module != null)
            {
                TrySemaphore.RemoveFromServicePoint(module, module.Semaphore);
                module.RefreshOrderInfo();
            }
            TrySemaphore.RemoveAll();
            ACSubState.ValueT = 0;

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _PossibleModuleList = null;
                _RoutableModuleList = null;
                _WaitsOnAvailableProcessModule = false;
                _LastRuleValueList = null;
                _LastSelectedClasses = null;
                _Priority = null;
            }

            // Damit nachfolge-Workflow gemappt werden kann wenn Resetted wird
            // Es soll stattdessn GroupComplete() aufgerufen werden
            //if (CurrentACState == PABaseState.SMRunning && IterationCount.ValueT <= 0)
            //    IterationCount.ValueT++;

            base.Reset();
        }


        public virtual void ResetWithRepeat()
        {
            PAProcessModule module = AccessedProcessModule;
            if (NeedsAProcessModule && module != null)
            {
                TrySemaphore.RemoveFromServicePoint(module, module.Semaphore);
                module.RefreshOrderInfo();
            }
            TrySemaphore.RemoveAll();
            ACSubState.ValueT = 0;

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _WaitsOnAvailableProcessModule = false;
                _Priority = null;
            }

            _ExecutingACMethod = null;
            AcknowledgeAllAlarms();
            CurrentACState = ACStateEnum.SMStarting;
        }

        /// <summary>
        ///   <para>
        /// Before the the first child in this group can be started this method does the following steps:
        /// </para>
        ///   <para>1. If the OccupationByScan-Property is set it unsubscribes from the cyclic callback and wait until a scan event happens to occupy a processmodule and switch to the Running-State.
        /// </para>
        ///   <para>2. If NeedsAProcessModule is TRUE (= a processmodule should be occupied) it reads the FirstAvailableProcessModule-Property and calls OnHandleAvailableProcessModule().
        /// Subclasses can override OnHandleAvailableProcessModule() to handle the occupation on their own. If OnHandleAvailableProcessModule() returns with true this method will return without doing anything.
        /// <br /> If NeedsAProcessModule is FALSE, the ACState ist switched to SMRunning without occupying a process module.
        /// </para>
        ///   <para>3. If FirstAvailableProcessModule is NULL, than SMStarting() subcribes to the work cycle until FirstAvailableProcessModule iis not null and this group is allowed to occupy the process module first (Property HasHighestPriorityForMapping).
        /// </para>
        ///   <para>4. The process module will be occcupied by calling TrySemaphore.AddToServicePoint().
        /// </para>
        ///   <para>5. The ACState is switched to SMRunning and the first child node "PWNodeStart" is started.
        /// </para>
        /// </summary>
        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public override void SMStarting()
        {
            //if (!PreExecute(PABaseState.SMStarting))
            //  return;

            if (!_WaitsOnAvailableProcessModule)
            {
                RecalcTimeInfo();
                if (CreateNewProgramLog(NewACMethodWithConfiguration()) <= CreateNewProgramLogResult.ErrorNoProgramFound)
                    return;
            }

            // Step 1: If the OccupationByScan-Property is set it unsubscribes from the cyclic callback and wait until a scane vent happens to occupy a processmodule and witch to the Running-State.
            if (OccupationByScan)
            {
                _WaitsOnAvailableProcessModule = true;
                UnSubscribeToProjectWorkCycle();
                return;
            }

            // Step 2: If NeedsAProcessModule is FALSE, the ACState ist switched to SMRunning without occupying a process module.
            if (NeedsAProcessModule)
            {
                // Beim Hochfahren warten, bis alle Semaphoren geladen sind erst dann auswerten
                if (!Root.Initialized)
                {
                    if (!_WaitsOnAvailableProcessModule)
                    {
                        _WaitsOnAvailableProcessModule = true;
                        SubscribeToProjectWorkCycle();
                    }
                    return;
                }
                // Step 2: If NeedsAProcessModule is TRUE (= a processmodule should be occupied) it reads the FirstAvailableProcessModule-Property and calls OnHandleAvailableProcessModule().
                // Subclasses can override OnHandleAvailableProcessModule() to handle the occupation on their own. If OnHandleAvailableProcessModule() returns with FALSE this method will return without doing anything.
                PAProcessModule module = FirstAvailableProcessModule;
                if (OnHandleAvailableProcessModule(module))
                    return;

                // Step 3. If FirstAvailableProcessModule is NULL, than SMStarting() subcribes to the work cycle until FirstAvailableProcessModule is not null and this group is allowed to occupy the process module first (Property HasHighestPriorityForMapping).
                if (module == null)
                {
                    if (!_WaitsOnAvailableProcessModule)
                    {
                        _WaitsOnAvailableProcessModule = true;
                        SubscribeToProjectWorkCycle();
                    }
                    return;
                }
                else
                {
                    if (!OnHasHighestPriorityForMapping(module))
                    {
                        if (!_WaitsOnAvailableProcessModule)
                        {
                            _WaitsOnAvailableProcessModule = true;
                            SubscribeToProjectWorkCycle();
                        }
                        return;
                    }

                    // Step 4: The process module will be occcupied by calling TrySemaphore.AddToServicePoint().       
                    ACPointNetWrapObject<ACComponent> lockObject = TrySemaphore.AddToServicePoint(module, module.Semaphore);
                    if (lockObject == null)
                    {
                        if (!_WaitsOnAvailableProcessModule)
                        {
                            _WaitsOnAvailableProcessModule = true;

                            //Error50168: Semaphore of {0} coudn't be aquired.
                            Msg msg = new Msg(this, eMsgLevel.Error, PWClassName, "SMStarting(1)", 1000, "Error50168", module.GetACUrl());

                            if (IsAlarmActive(ProcessAlarm, msg.Message) == null)
                                Messages.LogError(this.GetACUrl(), "SMStarting(1)", msg.Message);
                            OnNewAlarmOccurred(ProcessAlarm, msg, true);

                            SubscribeToProjectWorkCycle();
                        }
                        return;
                    }
                    else if (_WaitsOnAvailableProcessModule)
                    {
                        _WaitsOnAvailableProcessModule = false;
                        UnSubscribeToProjectWorkCycle();
                    }
                    if (lockObject != null)
                    {
                        module.LastOccupation.ValueT = DateTime.Now;
                        module.RefreshOrderInfo();
                        OnProcessModuleOccupied(module);
                    }

                }
            }

            if (IsACStateMethodConsistent(ACStateEnum.SMStarting) < ACStateCompare.WrongACStateMethod)
            {
                // Reset Configuration for Reloading parameters which are dependent of AccessedProcessModule
                ClearMyConfiguration();
                CurrentACState = ACStateEnum.SMRunning;
                //PostExecute(PABaseState.SMStarting);
            }

            PWNodeStart.Start();
        }


        [ACMethodState("en{'Running'}de{'Läuft'}", 30, true)]
        public virtual void SMRunning()
        {
            RootPW?.OnPWGroupRun(this);
        }


        /// <summary>
        /// Calls ReleaseAllAccessedModules() to removes this workflow-group from all occupied process modules.
        /// Afterwards it raises the PWPointOut-Event and calls Reset() to switch to the "Idle"-State.
        /// </summary>
        [ACMethodState("en{'Completed'}de{'Beendet'}", 40, true)]
        public virtual void SMCompleted()
        {
            UnSubscribeToProjectWorkCycle();

            PAProcessModule module = AccessedProcessModule;
            RecalcTimeInfo();
            FinishProgramLog(ExecutingACMethod, module != null ? module.ComponentClass.ACClassID : (Guid?)null);
            IterationCount.ValueT++;

            ReleaseAllAccessedModules();

            if (MustRepeatGroupAtEnd)
            {
                if (IsACStateMethodConsistent(ACStateEnum.SMCompleted) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
                {
                    ResetWithRepeat();
                }
            }
            else
            {
                ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs(Const.PWPointOut, VirtualEventArgs);
                eventArgs.GetACValue("TimeInfo").Value = RecalcTimeInfo();
                PWPointOut.Raise(eventArgs);

                if (IsACStateMethodConsistent(ACStateEnum.SMCompleted) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
                {
                    Reset();
                }
            }
        }

        public override void SMIdle()
        {
            RootPW?.OnPWGroupIdle(this);
            base.SMIdle();
            
        }

        /// <summary>
        /// Removes this workflow-group from all occupied process modules
        /// </summary>
        /// <returns></returns>
        protected bool ReleaseAllAccessedModules()
        {
            ACPointNetWrapObject<ACComponent>[] semaphoreEntries = null;
            using (ACMonitor.Lock(TrySemaphore.LockConnectionList_20040))
            {
                semaphoreEntries = TrySemaphore.ConnectionList.ToArray();
            }
            if (semaphoreEntries == null || !semaphoreEntries.Any())
                return true;
            foreach (var semaphoreEntry in semaphoreEntries)
            {
                PAProcessModule module = GetProcessModuleFromEntry(semaphoreEntry);
                if (!ReleaseProcessModule(module))
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Removes this workflow-group from the "Semaphore" service point of the passed process module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        public bool ReleaseProcessModule(PAProcessModule module)
        {
            if (module == null)
                return false;
            if (!TrySemaphore.RemoveFromServicePoint(module, module.Semaphore))
                return false;
            _ProcessModuleChanged?.Invoke(this, new ProcessModuleChangedArgs(module, true));
            module.RefreshOrderInfo();
            return true;
        }

        [ACMethodInfo("", "en{'Release module'}de{'Gebe Modul frei'}", 300)]
        public bool ReleaseProcessModule(string acUrlOfPM)
        {
            if (string.IsNullOrEmpty(acUrlOfPM))
                return false;
            PAProcessModule pm = ACUrlCommand(acUrlOfPM) as PAProcessModule;
            if (pm == null)
                return false;
            return ReleaseProcessModule(pm);
        }


        /// <summary>
        /// Subclasses can override OnHandleAvailableProcessModule() to handle the occupation on their own. 
        /// If OnHandleAvailableProcessModule() returns with FALSE the cyclic method SMStarting() will return without doing anything.
        /// </summary>
        /// <param name="processModule">The process module.</param>
        /// <returns></returns>
        protected virtual bool OnHandleAvailableProcessModule(PAProcessModule processModule)
        {
            return false;
        }

        protected virtual void OnProcessModuleOccupied(PAProcessModule processModule)
        {
            _ProcessModuleChanged?.Invoke(this, new ProcessModuleChangedArgs(processModule, false));
        }

        private event ProcessModuleChangedEventHandler _ProcessModuleChanged;
        public event ProcessModuleChangedEventHandler ProcessModuleChanged
        {
            add
            {
                _ProcessModuleChanged -= value;
                _ProcessModuleChanged += value;
            }
            remove
            {
                _ProcessModuleChanged -= value;
            }
        }

        /// <summary>
        /// Occupies the passed procesmodule if the Configuration-Property "OccupationByScan" was set to true.
        /// This happens when a PAFWorkTaskScanBase-Instance has received a scan-event.
        /// The PAFWorkTaskScanBase-Instance calls this OccupyWithPModuleOnScan()-Method.
        /// </summary>
        /// <param name="processModule">The process module.</param>
        /// <returns></returns>
        public bool OccupyWithPModuleOnScan(PAProcessModule processModule)
        {
            if (processModule == null
                || CurrentACState == ACStateEnum.SMIdle
                || !OccupationByScan)
                return false;

            var pmList = ProcessModuleList;
            if (pmList == null || !pmList.Any())
                return false;
            if (!ProcessModuleList.Contains(processModule))
                return false;
            if (processModule.OnGetSemaphoreCapacity() == 1 && processModule.Semaphore.ConnectionListCount > 0)
                return false;
            ACPointNetWrapObject<ACComponent> lockObject = TrySemaphore.AddToServicePoint(processModule, processModule.Semaphore);
            if (lockObject == null)
                return false;
            if (lockObject != null)
            {
                processModule.LastOccupation.ValueT = DateTime.Now;
                processModule.RefreshOrderInfo();
                OnProcessModuleOccupied(processModule);
            }

            if (IsACStateMethodConsistent(ACStateEnum.SMStarting) < ACStateCompare.WrongACStateMethod)
            {
                ClearMyConfiguration();
                CurrentACState = ACStateEnum.SMRunning;
                _WaitsOnAvailableProcessModule = false;
                PWNodeStart.Start();
            }
            return true;
        }


        /// <summary>
        /// Occupies the passed procesmodule (via ACURl) if the Configuration-Property "OccupationByScan" was set to true.
        /// This happens when a PAFWorkTaskScanBase-Instance has received a scan-event.
        /// The PAFWorkTaskScanBase-Instance calls this OccupyWithPModuleOnScan()-Method.
        /// </summary>
        /// <param name="acUrlOfPM">The ac URL of pm.</param>
        /// <returns></returns>
        [ACMethodInfo("", "en{'Occupy with module'}de{'Belege mit Modul'}", 300)]
        public bool OccupyWithPModuleOnScan(string acUrlOfPM)
        {
            if (string.IsNullOrEmpty(acUrlOfPM))
                return false;
            PAProcessModule pm = ACUrlCommand(acUrlOfPM) as PAProcessModule;
            if (pm == null)
                return false;
            return OccupyWithPModuleOnScan(pm);
        }

        #endregion


        #region User-Methods
        [ACMethodInteraction("", "en{'Reset substate'}de{'Unterstatus zurücksetzen'}", 299, true)]
        public virtual void ResetSubState()
        {
            if (!IsEnabledResetSubState())
                return;
            CurrentACSubState = 0;
        }

        public virtual bool IsEnabledResetSubState()
        {
            return CurrentACSubState != 0;
        }

        [ACMethodInfo("", "en{'Change Priority'}de{'Priorität ändern'}", 400)]
        public void ChangePriority(short priority)
        {
            if (priority < 0)
                SetPriority(PriorityMode.ToConfigurationValue);
            else if (priority == 0)
                SetPriority(PriorityMode.OccupyImmediately);
            else if (priority == 1)
                SetPriority(PriorityMode.ToHighest);
            else if (priority >= C_DefaultLowestPriority)
                SetPriority(PriorityMode.ToLowestDefault);
            else
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _Priority = Convert.ToUInt16(priority);
                }
            }
        }

        [ACMethodInteractionClient("", "en{'Change Priority'}de{'Priorität ändern'}", 298, true)]
        public static void SetPriorityClient(IACComponent acComponent)
        {
            if (acComponent == null)
                return;

            ACComponent _this = acComponent as ACComponent;

            string questionID = "Question50064";
            string header = acComponent.Root.Environment.TranslateMessage(acComponent, questionID);
            short priority = -1;
            string sPriority = acComponent.Messages.InputBox(header, "");
            if (String.IsNullOrWhiteSpace(sPriority))
                priority = -1;
            else
            {
                if (!short.TryParse(sPriority, out priority))
                    return;
                if (priority > C_DefaultLowestPriority)
                    priority = Convert.ToInt16(C_DefaultLowestPriority);
            }

            _this.ACUrlCommand("!ChangePriority", priority);
        }

        public static bool IsEnabledSetPriorityClient(IACComponent acComponent)
        {
            return true;
        }

        /// <summary>
        /// Completes this node by setting the CurrentACState-Property to ACStateEnum.SMCompleted.
        /// </summary>
        [ACMethodInteraction("Process", "en{'End Group'}de{'Beende Gruppe'}", (short)MISort.Stop, true)]
        public void GroupComplete()
        {
            CurrentACState = ACStateEnum.SMCompleted;
        }

        public bool IsEnabledGroupComplete()
        {
            if (CurrentACState == ACStateEnum.SMRunning)
                return true;
            return false;
        }

        [ACMethodInfo("", "en{'Scanable process modules'}de{'Scanbare Prozessmodule'}", 340)]
        public IEnumerable<ACChildInstanceInfo> GetScanableProcessModuleList()
        {
            return ScanableProcessModuleList.Select(c => new ACChildInstanceInfo(c)).ToList();
        }

        [ACMethodInfo("", "en{'CanScanAndOccupyProcessModules'}de{'CanScanAndOccupyProcessModules'}", 341)]
        public bool CanScanAndOccupyProcessModules()
        {
            return OccupationByScan && NeedsAProcessModule && ScanableProcessModuleList.Any();
        }

        [ACMethodInfo("", "en{'Releaseable process modules'}de{'Freigebbare Prozessmodule'}", 350)]
        public IEnumerable<ACChildInstanceInfo> GetReleaseableProcessModuleList()
        {
            return TrySemaphore.ConnectionList.Select(c => new ACChildInstanceInfo(c.ValueT)).ToList();
        }

        [ACMethodInfo("", "en{'CanReleaseProcessModules'}de{'CanReleaseProcessModules'}", 351)]
        public bool CanReleaseProcessModules()
        {
            return OccupationByScan && NeedsAProcessModule && TrySemaphore.ConnectionList.Any();
        }


        #endregion

        #region Client-Methods
        [ACMethodInteractionClient("", "en{'Show scanable process modules and occupy'}de{'Zeige scanbare Prozessmodule und belege'}", 342, true)]
        public static void ShowScanAndOccupyProcessModules(IACComponent acComponent)
        {
            ACComponent _this = acComponent as ACComponent;
            IEnumerable<ACChildInstanceInfo> childInstanceInfoList = _this.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + nameof(GetScanableProcessModuleList), null) as IEnumerable<ACChildInstanceInfo>;
            if (childInstanceInfoList == null)
                return;

            ACChildInstanceInfo selectedChildInstanceInfo = null;
            string bsoName = "BSOComponentSelector(Dialog)";
            IACBSO childBSO = acComponent.Root.Businessobjects.ACUrlCommand("?" + bsoName) as IACBSO;
            if (childBSO == null)
                childBSO = acComponent.Root.Businessobjects.StartComponent(bsoName, null, new object[] { }) as IACBSO;
            if (childBSO != null)
            {
                selectedChildInstanceInfo = childBSO.ACUrlCommand("!ShowChildInstanceInfo", childInstanceInfoList.OrderBy(c => c.ACIdentifier).ToList()) as ACChildInstanceInfo;
                childBSO.Stop();
            }
            if (selectedChildInstanceInfo == null)
                return;
            acComponent.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + nameof(OccupyWithPModuleOnScan), selectedChildInstanceInfo.ACUrlParent + ACUrlHelper.Delimiter_DirSeperator + selectedChildInstanceInfo.ACIdentifier);
        }

        public static bool IsEnabledShowScanAndOccupyProcessModules(IACComponent acComponent)
        {
            try
            {
                return (bool)acComponent.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + nameof(CanScanAndOccupyProcessModules), null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [ACMethodInteractionClient("", "en{'Show occupied process modules and release'}de{'Zeige belegte Prozessmodule und gebe frei'}", 352, true)]
        public static void ShowReleaseableProcessModules(IACComponent acComponent)
        {
            ACComponent _this = acComponent as ACComponent;
            IEnumerable<ACChildInstanceInfo> childInstanceInfoList = _this.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + nameof(GetReleaseableProcessModuleList), null) as IEnumerable<ACChildInstanceInfo>;
            if (childInstanceInfoList == null)
                return;

            ACChildInstanceInfo selectedChildInstanceInfo = null;
            string bsoName = "BSOComponentSelector(Dialog)";
            IACBSO childBSO = acComponent.Root.Businessobjects.ACUrlCommand("?" + bsoName) as IACBSO;
            if (childBSO == null)
                childBSO = acComponent.Root.Businessobjects.StartComponent(bsoName, null, new object[] { }) as IACBSO;
            if (childBSO != null)
            {
                selectedChildInstanceInfo = childBSO.ACUrlCommand("!ShowChildInstanceInfo", childInstanceInfoList.OrderBy(c => c.ACIdentifier).ToList()) as ACChildInstanceInfo;
                childBSO.Stop();
            }
            if (selectedChildInstanceInfo == null)
                return;
            acComponent.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + nameof(ReleaseProcessModule), selectedChildInstanceInfo.ACUrlParent + ACUrlHelper.Delimiter_DirSeperator + selectedChildInstanceInfo.ACIdentifier);
        }

        public static bool IsEnabledShowReleaseableProcessModules(IACComponent acComponent)
        {
            try
            {
                return (bool)acComponent.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + nameof(CanReleaseProcessModules), null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion


        #region Alarmhandling
        public override void RefreshHasAlarms(bool? childHasAlarms = null, bool resetIfNoUnackAlarms = false)
        {
            base.RefreshHasAlarms(childHasAlarms, resetIfNoUnackAlarms);
            var module = AccessedProcessModule;
            if (module != null)
            {
                module.RefreshHasAlarms(HasAlarms.ValueT ? true : childHasAlarms, resetIfNoUnackAlarms);
            }
        }

        protected override void OnSubAlarmChanged(ACEventArgs events, bool childHasAlarms)
        {
            base.OnSubAlarmChanged(events, childHasAlarms);
        }
        #endregion


        #region Helpermethods

        public TResult GetExecutingFunction<TResult>(Guid acMethodRequestID) where TResult : PAProcessFunction
        {
            if (acMethodRequestID == Guid.Empty)
                return default(TResult);
            PAProcessModule module = AccessedProcessModule;
            if (module == null)
                return default(TResult);
            return module.GetExecutingFunction<TResult>(acMethodRequestID);
        }

        #endregion


        #region Planning and Testing
        protected override TimeSpan GetPlannedDuration()
        {
            // Gruppe hat keine Wartezeit: Sobald die unteren Nodes durch Aufruf voin GroupComplete() durchgelaufen sind,
            // erfolgt kine Verzögerung des Rais-Events.
            //return base.GetPlannedDuration();
            return TimeSpan.Zero;
        }
        #endregion


        #region Diagnostics and Dump
        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            base.DumpPropertyList(doc, xmlACPropertyList);

            XmlElement xmlProperty = xmlACPropertyList["_WaitsOnAvailableProcessModule"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("_WaitsOnAvailableProcessModule");
                if (xmlProperty != null)
                    xmlProperty.InnerText = _WaitsOnAvailableProcessModule.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["HasHighestPriorityForMapping"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("HasHighestPriorityForMapping");
                if (xmlProperty != null)
                    xmlProperty.InnerText = HasHighestPriorityForMapping.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["IgnoreFIFO"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("IgnoreFIFO");
                if (xmlProperty != null)
                    xmlProperty.InnerText = IgnoreFIFO.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["RoutingCheck"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("RoutingCheck");
                if (xmlProperty != null)
                    xmlProperty.InnerText = RoutingCheck.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["IsConsiderableInFIFOQueue"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("IsConsiderableInFIFOQueue");
                if (xmlProperty != null)
                    xmlProperty.InnerText = IsConsiderableInFIFOQueue.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["WithoutPM"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("WithoutPM");
                if (xmlProperty != null)
                    xmlProperty.InnerText = WithoutPM.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["OccupationByScan"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("OccupationByScan");
                if (xmlProperty != null)
                    xmlProperty.InnerText = OccupationByScan.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["ConfiguredPriority"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("ConfiguredPriority");
                if (xmlProperty != null)
                    xmlProperty.InnerText = ConfiguredPriority.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["Priority"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("Priority");
                if (xmlProperty != null)
                    xmlProperty.InnerText = Priority.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["FIFOCheckFirstPM"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("FIFOCheckFirstPM");
                if (xmlProperty != null)
                    xmlProperty.InnerText = FIFOCheckFirstPM.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }
        }
        #endregion


        #endregion
    }
}

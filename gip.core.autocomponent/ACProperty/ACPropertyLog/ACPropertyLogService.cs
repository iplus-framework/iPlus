using gip.core.datamodel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPropertyLog Service'}de{'ACPropertyLog Service'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, true)]
    public class ACPropertyLogService : PARole
    {
        public const string ClassName = "ACPropertyLogService";

        #region c'tors

        public ACPropertyLogService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            ThreadPool.QueueUserWorkItem((object state) => RebuildPropertyLogRuleCache());

            using (Database db = new datamodel.Database())
            {
                _LoggableProperties = db.ACClassProperty.Where(c => c.ACClassPropertyRelation_TargetACClassProperty
                                                                     .Any(x => x.ConnectionTypeIndex == (short)Global.ConnectionTypes.PointState))
                                                        .Select(p => p.ACIdentifier).Distinct().ToArray();
            }

            if (this.ApplicationManager != null)
                this.ApplicationManager.ProjectWorkCycleR1min += ApplicationManager_ProjectWorkCycleR1min;
            (Root as ACRoot).OnSendPropertyValueEvent += OnPropertyValueChanged;

            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (this.ApplicationManager != null)
                this.ApplicationManager.ProjectWorkCycleR1min -= ApplicationManager_ProjectWorkCycleR1min;
            (Root as ACRoot).OnSendPropertyValueEvent -= OnPropertyValueChanged;

            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties

        private string[] _LoggableProperties;

        private string[] _HierachicalRulesACUrl;

        private Dictionary<Guid, Guid> _HierachicalOneselfRules;

        private ACClass[] _BasedOnRulesACClasses;

        private ACMonitorObject _LockObjectHierarchy = new ACMonitorObject(10000);

        private ACMonitorObject _LockObjectHierarchyOneself = new ACMonitorObject(10000);

        private ACMonitorObject _LockObjectBasedOn = new ACMonitorObject(10000);

        private DateTime? _RuleCacheRebuildAt;

        private bool _CacheRebuilded = false;

        #endregion

        #region Methods

        #region Methods => RebuildCache
        [ACMethodInfo("", "en{'Rebuild rule cache'}de{'Cache für den Neuaufbau von Regeln'}", 999,true)]
        public void RebuildRuleCache()
        {
            _RuleCacheRebuildAt = DateTime.Now.AddMinutes(3);
        }

        private void RebuildPropertyLogRuleCache()
        {
            _CacheRebuilded = false;
            using (Database db = new datamodel.Database())
            {
                using (ACMonitor.Lock(_LockObjectHierarchy))
                {
                    _HierachicalRulesACUrl = db.ACPropertyLogRule.Where(c => c.RuleType == (short)Global.PropertyLogRuleType.ProjectHierarchy && c.ACClass.ACURLComponentCached != null).ToArray()
                                                                .Select(x => x.ACClass.ACUrlComponent).ToArray();
                }

                using (ACMonitor.Lock(_LockObjectHierarchyOneself))
                {
                    _HierachicalOneselfRules = db.ACPropertyLogRule.Where(c => c.RuleType == (short)Global.PropertyLogRuleType.ProjectHierarchyOneself)
                                                                  .ToDictionary(c => c.ACClassID, c => c.ACClassID);
                }

                using (ACMonitor.Lock(_LockObjectBasedOn))
                {
                    _BasedOnRulesACClasses = db.ACPropertyLogRule.Include(i => i.ACClass.ACClass1_BasedOnACClass)
                                                                 .Where(c => c.RuleType == (short)Global.PropertyLogRuleType.BasedOn).Select(x => x.ACClass).ToArray();
                }
            }
            _CacheRebuilded = true;
        }

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(RebuildRuleCache):
                    RebuildRuleCache();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion



        protected virtual void OnPropertyValueChanged(object sender, ACPropertyNetSendEventArgs e)
        {
            if (   e.ForACComponent == null 
                || e.NetValueEventArgs.EventType != EventTypes.ValueChangedInSource 
                || !(     e.ForACComponent is PAClassPhysicalBase 
                      ||  e.ForACComponent is PAProcessFunction
                      ||  e.ForACComponent is PAProcessModule))
                return;

            if (!_LoggableProperties.Any(x => x == e.NetValueEventArgs.ACIdentifier))
                return;

            DateTime eventTime = DateTime.Now;
            ProcessLogProperty(e, eventTime);
        }

        private void ProcessLogProperty(ACPropertyNetSendEventArgs args, DateTime eventTime)
        {
            if (   !IsComponentAffectedHierahicallyOneself(args.ForACComponent.ComponentClass) 
                && !IsComponentAffectedHierahically(args.ForACComponent.ACUrl) 
                && !IsComponentAffectedBasedOn(args.ForACComponent.ComponentClass))
                return;

            ACProgramLog programLog = null;
            PABase paComp = args.ForACComponent as PABase;
            if (paComp != null)
                programLog = paComp.CurrentProgramLog;
            else
            {
                PAProcessModule pAProcessModule = args.ForACComponent as PAProcessModule;
                if (pAProcessModule == null)
                    pAProcessModule = args.ForACComponent.FindParentComponent<PAProcessModule>(c => c is PAProcessModule);
                programLog = pAProcessModule?.CurrentProgramLog;
            }
            Guid? acProgramLogID = null;
            if (programLog != null)
                acProgramLogID = programLog.ACProgramLogID;

            this.ApplicationManager.ApplicationQueue.Add(() => LogProperty(args.ForACComponent.ComponentClass.ACClassID, args.NetValueEventArgs.ACIdentifier,
                                                                           args.NetValueEventArgs.ChangedValue, eventTime, acProgramLogID));
        }

        private bool IsComponentAffectedHierahically(string acUrlComponent)
        {
            using (ACMonitor.Lock(_LockObjectHierarchy))
            {
                return _HierachicalRulesACUrl != null ? _HierachicalRulesACUrl.Any(c => acUrlComponent.StartsWith(c, StringComparison.Ordinal)) : false;
            }
        }

        private bool IsComponentAffectedHierahicallyOneself(ACClass componentClass)
        {
            using (ACMonitor.Lock(_LockObjectHierarchyOneself))
            {
                return _HierachicalOneselfRules != null ? _HierachicalOneselfRules.ContainsKey(componentClass.ACClassID) : false;
            }
        }

        private bool IsComponentAffectedBasedOn(ACClass componentClass)
        {
            using (ACMonitor.Lock(_LockObjectBasedOn))
            {
                return _BasedOnRulesACClasses != null ? _BasedOnRulesACClasses.Any(baseClass => componentClass.IsDerivedClassFrom(baseClass)) : false;
            }
        }

        private void LogProperty(Guid acClassID, string propACIdentifier, object value, DateTime eventTime, Guid? acProgramLogID = null)
        {
            try
            {
                using (Database db = new datamodel.Database())
                {
                    ACClass acClass = db.ACClass.FirstOrDefault(c => c.ACClassID == acClassID);
                    ACClassProperty acClassProperty = acClass.Properties.FirstOrDefault(c => c.ACIdentifier == propACIdentifier);

                    if (acClassProperty == null)
                    {
                        Messages.LogError(this.GetACUrl(), "LogProperty(10)", "ACClassProperty is null.");
                        return;
                    }

                    ACPropertyLog propertyLog = ACPropertyLog.NewACObject(db, acClass);
                    propertyLog.ACClassPropertyID = acClassProperty.ACClassPropertyID;
                    propertyLog.EventTime = eventTime;
                    propertyLog.Value = ACConvert.ChangeType(value, typeof(string), true, db) as string;
                    propertyLog.ACProgramLogID = acProgramLogID;

                    db.ACPropertyLog.Add(propertyLog);
                    var msg = db.ACSaveChanges();
                    if (msg != null)
                        Messages.LogError(this.GetACUrl(), "LogProperty(20)", msg.Message);
                }
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "LogProperty(30)", e);
            }
        }

        private void ApplicationManager_ProjectWorkCycleR1min(object sender, EventArgs e)
        {
            if (_RuleCacheRebuildAt == null || !_CacheRebuilded)
                return;

            if (DateTime.Now < _RuleCacheRebuildAt.Value)
                return;

            ThreadPool.QueueUserWorkItem((object state) => RebuildPropertyLogRuleCache());
        }

        #endregion
    }
}

using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace gip.core.autocomponent
{

    /// <summary>
    /// Baseclass for for Broadcasting alarms over different transfer protocols / media
    /// Basisklasse um Alarme über unterscheidliche Übertragunsprotokolle oder Medien zu versenden
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Alarm-Messenger'}de{'Alarm-Verteiler'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAAlarmMessengerBase : ACComponent
    {
        #region c'tors
        public PAAlarmMessengerBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _EventSubscr = new ACPointEventSubscr(this, "EventSubscr", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            return true;
        }

        public override bool ACPostInit()
        {
            RebuildRuleCache();

            this.ApplicationManager.ProjectWorkCycleR1min += ApplicationManager_ProjectWorkCycleR1min;
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (ApplicationManager != null)
            {
                this.ApplicationManager.ProjectWorkCycleR1min -= ApplicationManager_ProjectWorkCycleR1min;
                EventSubscr.UnSubscribeAllEvents(ApplicationManager);
            }
            return base.ACDeInit(deleteACClassTask);
        }

        public const string ClassName = "PAAlarmMessengerBase";

        public const string SaveConfigPropName = "AlarmMsgConfiguration";

        public const string SaveExclusionConfigPropName = "AlarmMsgExclusionConfiguration";

        #endregion

        #region Fields

        DateTime? _RebuildCacheAt;

        bool _IsCacheRebuilded;
        private object _IsCacheRebuildedLock = new object();

        private Dictionary<string, AlarmMessangerCacheRule[]> _InclusionRuleCache;
        private ACMonitorObject _InclusionRuleCacheLock = new ACMonitorObject(10000);

        private Dictionary<string, string[]> _ExclusionRuleCache;
        private ACMonitorObject _ExclusionRuleCacheLock = new ACMonitorObject(20000);

        #endregion  

        #region Properties

        public ApplicationManager ApplicationManager
        {
            get
            {
                return FindParentComponent<ApplicationManager>(c => c is ApplicationManager);
            }
        }

        [ACPropertyBindingSource(200, "", "en{'Distribution is on'}de{'Verteilung eingeschaltet'}", "", true, true, DefaultValue=false)]
        public IACContainerTNet<Boolean> RunDistribution { get; set; }

        [ACPropertyBindingSource(201, "", "en{'Distribute acknowledged alarms'}de{'Verteile quittierte Alarme'}", "", true, true, DefaultValue = false)]
        public IACContainerTNet<Boolean> DistributeAckAlarms { get; set; }

        [ACPropertyBindingSource(202, "", "en{'Configurable distribution rules activated'}de{'Konfigurierbare Verteilungsregeln aktiviert'}", "", true, true, DefaultValue = true)]
        public IACContainerTNet<Boolean> ConfigRulesEnabled { get; set; }

        public void OnSetRunDistribution(IACPropertyNetValueEvent valueEvent)
        {
            bool newValue = (valueEvent as ACPropertyValueEvent<bool>).Value;
            if (ApplicationManager != null)
            {
                if (newValue != RunDistribution.ValueT)
                {
                    if (newValue && !RunDistribution.ValueT)
                    {
                        EventSubscr.SubscribeEvent(ApplicationManager, "AlarmChangedEvent", EventCallback);
                        EventSubscr.SubscribeEvent(ApplicationManager, "SubAlarmsChangedEvent", EventCallback);
                    }
                    else if (!newValue && RunDistribution.ValueT)
                    {
                        EventSubscr.UnSubscribeAllEvents(ApplicationManager);
                    }
                }
            }
        }

        #endregion

        #region Methods
        [ACMethodInteraction("", "en{'Distribution on'}de{'Verteiler ein'}", 200, true)]
        public virtual void DistributionOn()
        {
            if (!IsEnabledDistributionOn())
                return;
            RunDistribution.ValueT = true;
        }

        public virtual bool IsEnabledDistributionOn()
        {
            if (RunDistribution.ValueT)
                return false;
            return true;
        }

        [ACMethodInteraction("", "en{'Distribution off'}de{'Verteiler aus'}", 201, true)]
        public virtual void DistributionOff()
        {
            if (!IsEnabledDistributionOff())
                return;
            RunDistribution.ValueT = false;
        }

        public virtual bool IsEnabledDistributionOff()
        {
            return !IsEnabledDistributionOn();
        }

        [ACMethodInfo("Function", "en{'FilterAlarm'}de{'FilterAlarm'}", 9999)]
        public virtual bool FilterAlarm(string propertyName, Msg alarm)
        {
            if (alarm.IsAcknowledged)
                return false;
            return true;
        }

        private void RebuildRuleCache()
        {
            lock (_IsCacheRebuildedLock)
            {
                _IsCacheRebuilded = false;
            }
            Dictionary<string, AlarmMessangerCacheRule[]> cache = new Dictionary<string, AlarmMessangerCacheRule[]>();
            Dictionary<string, string[]> cacheExclusion = new Dictionary<string, string[]>();

            try
            {
                ACClass acClass = this.ComponentClass;
                while (acClass != null)
                {
                    IEnumerable<ACClassConfig> configs = null;
                    using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
                    {
                        acClass.ACClassConfig_ACClass.AutoRefresh(acClass.Database);
                        acClass.ACClassConfig_ACClass.AutoLoad(acClass.Database);
                        configs = acClass.ACClassConfig_ACClass.Where(c => c.LocalConfigACUrl == SaveConfigPropName).ToList();
                    }
                    if (configs != null && configs.Any())
                    {
                        var groupedConfigs = configs.GroupBy(c => c.Value.ToString());

                        foreach (var config in groupedConfigs)
                        {
                            if (!cache.ContainsKey(config.Key))
                            {
                                var groupedBySource = config.Select(c => c).GroupBy(c => c.KeyACUrl);
                                
                                cache.Add(config.Key, config.Select(c => BuildAlarmRuleCacheFromConfig(c.KeyACUrl, groupedBySource.Where(x => x.Key == c.KeyACUrl)
                                                                                                                                  .SelectMany(x => x).Distinct())).ToArray());
                            }
                        }
                    }

                    if (acClass.ACIdentifier == ClassName)
                        break;

                    using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
                    {
                        acClass = acClass.ACClass1_BasedOnACClass;
                    }
                }

                acClass = ComponentClass as ACClass;
                if (acClass != null)
                {
                    IEnumerable<ACClassConfig> configsExclusion = null;
                    using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
                    {
                        acClass.ACClassConfig_ACClass.AutoRefresh(acClass.Database);
                        configsExclusion = acClass.ACClassConfig_ACClass.Where(c => c.LocalConfigACUrl == SaveExclusionConfigPropName && c.Value != null).ToArray();
                    }
                    if (configsExclusion != null && configsExclusion.Any())
                    {
                        var groupedConfigs = configsExclusion.GroupBy(c => c.Value.ToString());

                        foreach (var config in groupedConfigs)
                        {
                            cacheExclusion.Add(config.Key, config.Select(x => x.KeyACUrl).ToArray());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "RebuildAlarmMessengerConfigCache(10)", e.Message);
            }
            finally
            {
                lock (_IsCacheRebuildedLock)
                {
                    _IsCacheRebuilded = true;
                }
            }

            AlarmMessangerCacheRule[][] oldRuleCache = null;

            using (ACMonitor.Lock(_InclusionRuleCacheLock))
            {
                if (_InclusionRuleCache != null && _InclusionRuleCache.Any())
                    oldRuleCache = _InclusionRuleCache.Values.ToArray();
                _InclusionRuleCache = new Dictionary<string, AlarmMessangerCacheRule[]>(cache);
            }

            ReleaseRefFromCache(oldRuleCache);
            oldRuleCache = null;

            using (ACMonitor.Lock(_ExclusionRuleCacheLock))
            {
                _ExclusionRuleCache = new Dictionary<string, string[]>(cacheExclusion);
            }
        }

        private AlarmMessangerCacheRule BuildAlarmRuleCacheFromConfig(string sourceACUrl, IEnumerable<ACClassConfig> configs)
        {
            AlarmMessangerCacheRule rule = new AlarmMessangerCacheRule();
            rule.SourceComponent = ACUrlCommand(sourceACUrl, null) as ACClass;

            List<ACRef<ACComponent>> targetItems = new List<ACRef<ACComponent>>();

            foreach (ACClassConfig config in configs)
            {
                if (!string.IsNullOrEmpty(config.Expression))
                {
                    ACComponent comp = Root.ACUrlCommand(config.Expression) as ACComponent;
                    if (comp != null)
                    {
                        targetItems.Add(new ACRef<ACComponent>(comp, this));
                    }
                }
            }

            rule.TargetComponents = targetItems;

            return rule;
        }

        private void ReleaseRefFromCache(AlarmMessangerCacheRule[][] ruleCache)
        {
            if (ruleCache == null)
                return;

            foreach(AlarmMessangerCacheRule [] cacheArray in ruleCache)
            {
                foreach (AlarmMessangerCacheRule cache in cacheArray)
                {
                    if (cache.TargetComponents == null || !cache.TargetComponents.Any())
                        continue;

                    foreach (var refComp in cache.TargetComponents)
                    {
                        refComp.Detach();
                    }
                }
            }
        }

        public Global.ConfigIconState CheckAlarmMsgInConfig(Msg msg, out List<ACRef<ACComponent>> targetComponents)
        {
            bool isAnyConfigExist = false;
            targetComponents = null;

            using (ACMonitor.Lock(_InclusionRuleCacheLock))
            {
                    if (_InclusionRuleCache != null)
                    isAnyConfigExist = _InclusionRuleCache.Any();
            }

            if (!isAnyConfigExist)
            {

                using (ACMonitor.Lock(_ExclusionRuleCacheLock)) 
                {
                    if (_ExclusionRuleCache != null)
                        isAnyConfigExist = _ExclusionRuleCache.Any();
                }

                if (!isAnyConfigExist)
                    return Global.ConfigIconState.NoConfig;
            }

            ACClass acClass = null;
            string[] acUrlResult = Array.Empty<string>();
            if (!String.IsNullOrEmpty(msg.TranslID))
            {
                bool isExclusionValueExists = false;

                using (ACMonitor.Lock(_ExclusionRuleCacheLock))
                {
                    if (_ExclusionRuleCache != null)
                        isExclusionValueExists = _ExclusionRuleCache.TryGetValue(msg.TranslID, out acUrlResult);
                }

                if (isExclusionValueExists)
                {
                    acClass = null;
                    if (msg.SourceComponent != null)
                        acClass = msg.SourceComponent.ValueT.ComponentClass;
                    else
                    {
                        IACComponent sourceComp = ACUrlCommand(msg.Source) as IACComponent;
                        if (sourceComp != null && sourceComp.ComponentClass != null)
                            acClass = sourceComp.ComponentClass as ACClass;
                    }

                    if (acClass != null)
                    {
                        string acUrl = "";
                        using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
                        {
                            acUrl = acClass.ACUrl;
                        }
                        if (acUrlResult.Contains(acUrl))
                            return Global.ConfigIconState.ExclusionConfig;
                    }
                }
            }
            if (!String.IsNullOrEmpty(msg.ACIdentifier))
            {
                bool isExclusionValueExists = false;

                using (ACMonitor.Lock(_ExclusionRuleCacheLock))
                {
                    if (_ExclusionRuleCache != null)
                        isExclusionValueExists = _ExclusionRuleCache.TryGetValue(msg.ACIdentifier, out acUrlResult);
                }

                if (isExclusionValueExists)
                {
                    ACClass aCClass = null;
                    if (msg.SourceComponent != null)
                        aCClass = msg.SourceComponent.ValueT.ComponentClass;
                    else
                    {
                        IACComponent sourceComp = ACUrlCommand(msg.Source) as IACComponent;
                        if (sourceComp != null && sourceComp.ComponentClass != null)
                            aCClass = sourceComp.ComponentClass;
                    }

                    if (acClass != null)
                    {
                        string acUrl = "";
                        using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
                        {
                            acUrl = acClass.ACUrl;
                        }
                        if (acUrlResult.Contains(acUrl))
                            return Global.ConfigIconState.ExclusionConfig;
                    }
                }
            }

            acClass = null;
            AlarmMessangerCacheRule[] result = Array.Empty<AlarmMessangerCacheRule>();
            if (!String.IsNullOrEmpty(msg.TranslID))
            {
                bool isInclusionValueExists = false;

                using (ACMonitor.Lock(_InclusionRuleCacheLock))
                {
                    if (_InclusionRuleCache != null)
                        isInclusionValueExists = _InclusionRuleCache.TryGetValue(msg.TranslID, out result);
                }

                if (isInclusionValueExists)
                {
                    if (msg.SourceComponent != null)
                        acClass = msg.SourceComponent.ValueT.ComponentClass;
                    else
                    {
                        IACComponent sourceComp = ACUrlCommand(msg.Source) as IACComponent;
                        if (sourceComp != null && sourceComp.ComponentClass != null)
                            acClass = sourceComp.ComponentClass;
                    }

                    if (acClass != null)
                    {
                        if (result.Any(c => c.SourceComponent.ACClassID == acClass.ACClassID))
                        {
                            return Global.ConfigIconState.Config;
                        }

                        foreach (AlarmMessangerCacheRule resultItem in result)
                        {
                            try
                            {
                                if (acClass.IsDerivedClassFrom(resultItem.SourceComponent))
                                {
                                    return Global.ConfigIconState.InheritedConfig;
                                }
                            }
                            catch
                            {
                                return Global.ConfigIconState.NoConfig;
                            }
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(msg.ACIdentifier))
            {
                bool isInclusionValueExists = false;

                using (ACMonitor.Lock(_InclusionRuleCacheLock))
                {
                    if (_InclusionRuleCache != null)
                        isInclusionValueExists = _InclusionRuleCache.TryGetValue(msg.ACIdentifier, out result);
                }

                if (isInclusionValueExists)
                {
                    if(msg.SourceComponent != null)
                        acClass = msg.SourceComponent.ValueT.ComponentClass;
                    else
                    {
                        IACComponent sourceComp = ACUrlCommand(msg.Source) as IACComponent;
                        if (sourceComp != null && sourceComp.ComponentClass != null)
                            acClass = sourceComp.ComponentClass;
                    }

                    if (acClass != null)
                    {
                        var config = result.FirstOrDefault(c => c.SourceComponent.ACClassID == acClass.ACClassID);

                        if (config.SourceComponent != null)
                        {
                            targetComponents = config.TargetComponents;
                            return Global.ConfigIconState.Config;
                        }

                        foreach (AlarmMessangerCacheRule resultItem in result)
                        {
                            try
                            {
                                if (acClass.IsDerivedClassFrom(resultItem.SourceComponent))
                                {
                                    targetComponents = resultItem.TargetComponents;
                                    return Global.ConfigIconState.InheritedConfig;
                                }
                            }
                            catch
                            {
                                return Global.ConfigIconState.NoConfig;
                            }
                        }
                    }
                }
            }
            return Global.ConfigIconState.NoConfig; 
        }

        [ACMethodInfo("Function", "en{'DistributeAlarm'}de{'DistributeAlarm'}", 9999)]
        public abstract void DistributeAlarm(string propertyName, Msg alarm, List<ACRef<ACComponent>> targetComponents);

        private void ApplicationManager_ProjectWorkCycleR1min(object sender, EventArgs e)
        {
            bool isCacheRebuilded;
            lock(_IsCacheRebuildedLock)
            {
                isCacheRebuilded = _IsCacheRebuilded;
            }

            if (!_RebuildCacheAt.HasValue || !isCacheRebuilded)
                return;

            if (DateTime.Now < _RebuildCacheAt.Value)
                return;
            _RebuildCacheAt = null;
            ThreadPool.QueueUserWorkItem((object state) => RebuildRuleCache());
        }

        [ACMethodInteraction("", "en{'Rebuild configuration cache'}de{'Rebuild configuration cache'}", 999, true)]
        public void RebuildConfigurationCache()
        {
            _RebuildCacheAt = DateTime.Now.AddMinutes(1);
        }

        public bool IsEnabledRebuildConfigurationCache()
        {
            return !_RebuildCacheAt.HasValue;
        }

        #endregion

        #region Event-Handling
        ACPointEventSubscr _EventSubscr;
        [ACPropertyEventPointSubscr(9999, false)]
        public ACPointEventSubscr EventSubscr
        {
            get
            {
                return _EventSubscr;
            }
        }

        [ACMethodInfo("Function", "en{'EventCallback'}de{'EventCallback'}", 9999)]
        public void EventCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                if ((sender.ACIdentifier == "AlarmChangedEvent") || (sender.ACIdentifier == "SubAlarmsChangedEvent"))
                {
                    try
                    {
                        bool send = (bool)ACUrlCommand("!FilterAlarm", e["PropertyName"] as string, e[Const.Value] as Msg);
                        List<ACRef<ACComponent>> targetComponents = null;
                        if (send && ConfigRulesEnabled.ValueT)
                        {
                            Global.ConfigIconState configResult = CheckAlarmMsgInConfig(e[Const.Value] as Msg, out targetComponents);
                            send = configResult == Global.ConfigIconState.Config || configResult == Global.ConfigIconState.InheritedConfig;
                        }

                        if (send)
                            DistributeAlarm(e["PropertyName"] as string, e[Const.Value] as Msg, targetComponents);
                    }
                    catch (Exception ec)
                    {
                        Messages.LogException(this.GetACUrl(), "PAAlarmDistributor.EventCallback() ", "Exception in Script !FilterAlarm, Error:"+ec.Message);
                    }
                }
            }
        }
        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "DistributionOn":
                    DistributionOn();
                    return true;
                case "IsEnabledDistributionOn":
                    result = IsEnabledDistributionOn();
                    return true;
                case "DistributionOff":
                    DistributionOff();
                    return true;
                case "IsEnabledDistributionOff":
                    result = IsEnabledDistributionOff();
                    return true;
                case "FilterAlarm":
                    result = FilterAlarm(acParameter[0] as string, acParameter[1] as Msg);
                    return true;
                case "DistributeAlarm":
                    DistributeAlarm(acParameter[0] as string, acParameter[1] as Msg, acParameter[2] as List<ACRef<ACComponent>>);
                    return true;
                case "RebuildConfigurationCache":
                    RebuildConfigurationCache();
                    return true;
                case "IsEnabledRebuildConfigurationCache":
                    result = IsEnabledRebuildConfigurationCache();
                    return true;
                case "EventCallback":
                    EventCallback((gip.core.datamodel.IACPointNetBase)acParameter[0], (gip.core.datamodel.ACEventArgs)acParameter[1], (gip.core.datamodel.IACObject)acParameter[2]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }

    /// <summary>
    /// Represents the class for alarm messanger configuration.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'AlarmMessangerConfig'}de{'AlarmMessangerConfig'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class AlarmMessengerConfig : INotifyPropertyChanged
    {
        public AlarmMessengerConfig(string sourceACUrl, string msgPropACIdentifier, string sourceACUrlComp, bool hasExclusionRules)
        {
            SourceACUrl = sourceACUrl;
            MsgPropACIdentifier = msgPropACIdentifier;

            if (sourceACUrlComp.Split('\\').Count() == 2)
                SourceACUrlComp = "Variolibrary" + sourceACUrlComp;
            else
                SourceACUrlComp = sourceACUrlComp;

            HasExclusionRules = hasExclusionRules;
        }

        [ACPropertyInfo(999, "", "en{'Source ACUrl'}de{'Source ACUrl'}")]
        public string SourceACUrl
        {
            get;
            set;
        }

        [ACPropertyInfo(999, "", "en{'Source comp. url'}de{'Quell-Komp. url'}")]
        public string SourceACUrlComp
        {
            get;
            set;
        }

        [ACPropertyInfo(999, "", "en{'Target ACUrl'}de{'Target ACUrl'}")]
        public string TargetACUrl
        {
            get;
            set;
        }

        [ACPropertyInfo(999, "", "en{'Target comp. url'}de{'Ziel-Komp. url'}")]
        public string TargetACUrlComp
        {
            get;
            set;
        }

        [ACPropertyInfo(999,"","en{'ACIdentifier'}de{'ACIdentifier'}")]
        public string MsgPropACIdentifier
        {
            get;
            set;
        }

        [ACPropertyInfo(999, "", "en{'ACCaption'}de{'ACCaption'}")]
        public string MsgPropACCaption
        {
            get;
            set;
        }

        [ACPropertyInfo(999, "", "en{'Configuration target'}de{'Konfigurationsziel'}")]
        public string ConfigTargetACUrl
        {
            get;
            set;
        }

        [ACPropertyInfo(999, "", "en{'Configuration target url'}de{'Konfigurationsziel url'}")]
        public string ConfigTargetACUrlComp
        { 
            get
            {
                if (string.IsNullOrEmpty(ConfigTargetACUrl))
                    return SourceACUrl;

                if (ConfigTargetACUrl.Split('\\').Count() == 2)
                    return "Variolibrary" + ConfigTargetACUrl;

                return ConfigTargetACUrl;
            }
        }

        bool _HasExclusionRules;
        [ACPropertyInfo(999)]
        public bool HasExclusionRules
        {
            get
            {
                return _HasExclusionRules;
            }
            set
            {
                _HasExclusionRules = value;
                OnPropertyChanged("HasExclusionRules");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public struct AlarmMessangerCacheRule
    {
        public ACClass SourceComponent
        {
            get;
            set;
        }
            
        public List<ACRef<ACComponent>> TargetComponents
        {
            get;
            set;
        }
    }
}

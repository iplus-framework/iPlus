using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Property redirector'}de{'Eigenschaftsumleiter'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, true)]
    public class ACPropertyRedirector: PAClassAlarmingBase
    {
        public const string ClassName = "ACPropertyRedirector";

        #region c'tors

        public ACPropertyRedirector(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            StartRedirection();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            StopRedirection();
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Points
        [ACPropertyPointConfig(9999, "", typeof(PAPropRedirectConfig), "en{'Variables to export'}de{'Zu exportierende Variablen'}")]
        public List<ACClassConfig> PropertiesToRedirect
        {
            get
            {
                string keyACUrl = ".\\ACClassProperty(PropertiesToRedirect)";
                List<ACClassConfig> result = null;
                ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                {
                    try
                    {
                        ACTypeFromLiveContext.ACClassConfig_ACClass.Load(MergeOption.OverwriteChanges);
                        var query = ACTypeFromLiveContext.ACClassConfig_ACClass.Where(c => c.KeyACUrl == keyACUrl);
                        if (query.Any())
                            result = query.ToList();
                        else
                            result = new List<ACClassConfig>();
                    }
                    catch (Exception e)
                    {
                        Messages.LogException(this.GetACUrl(), "PropertiesToRedirect", e.Message);
                    }
                });
                return result;
            }
        }
        private int _CountConfigReads = 0;
        #endregion

        #region Internal Class
        internal class RedirectionHandler
        {
            public RedirectionHandler(IACPropertyNetServer sourceProperty, IACPropertyNetServer targetProperty)
            {
                if (sourceProperty == null || targetProperty == null)
                    throw new ArgumentNullException();
                SourceProperty = sourceProperty;
                TargetProperty = targetProperty;
                Subscribe();
            }

            public RedirectionHandler(IACPropertyNetServer sourceProperty, IACPropertyNetServer targetProperty, ACPropertyRedirector redirector, PAPropRedirectConfig redirectConfig)
            {
                RedirectConfig = redirectConfig;

                if (sourceProperty == null || targetProperty == null || redirector == null)
                    throw new ArgumentNullException();
                SourceProperty = sourceProperty;
                TargetProperty = targetProperty;
                PropertyRedirector = redirector;
                Subscribe();
            }

            internal IACPropertyNetServer SourceProperty { get; set; }
            internal IACPropertyNetServer TargetProperty { get; set; }

            internal PAPropRedirectConfig RedirectConfig { get; set; }

            internal ACPropertyRedirector PropertyRedirector { get; set; }

            internal void Subscribe()
            {
                if (_IsSubscribed)
                    return;
                SourceProperty.ValueUpdatedOnReceival += ConvertS2T;
                _IsSubscribed = true;
            }

            internal void UnSubscribe()
            {
                if (!_IsSubscribed)
                    return;
                SourceProperty.ValueUpdatedOnReceival -= ConvertS2T;
                _IsSubscribed = false;
            }

            bool _IsSubscribed = false;

            private void ConvertS2T(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase)
            {
                if (phase == ACPropertyChangedPhase.BeforeBroadcast)
                    return;
                try
                {
                    object sourceValue = SourceProperty.Value;

                    if (RedirectConfig != null && RedirectConfig.Interpolation != Global.InterpolationMethod.None)
                    {
                        TimeSpan[] fromIntervals = DetermineBackwardTimespans(RedirectConfig.InterpolationMaxTimeSpanBackwards);
                        int currentInterval = 0, maxInterval = fromIntervals.Length - 1;

                        DateTime now = DateTime.Now.AddSeconds(5);
                        DateTime to = now;
                        DateTime from = to - fromIntervals[currentInterval];

                        int valuesToTake = RedirectConfig.InterpolationRange;
                        if (valuesToTake > 0)
                        {
                            PropertyLogListInfo archive = SourceProperty.GetArchiveLog(from, to);
                            PropertyLogListInfo valuesToProcess = new PropertyLogListInfo(archive.RefreshRate, new List<PropertyLogItem>());
                            if (archive != null)
                            {
                                int counter = 0;
                                int counterMax = maxInterval > 0 ? maxInterval * 2 : 2;

                                while (true)
                                {
                                    if (archive.PropertyLogList.Any())
                                    {
                                        IEnumerable<PropertyLogItem> validatedItems = archive.PropertyLogList.ToArray();

                                        if (!string.IsNullOrEmpty(RedirectConfig.ValidationMethodName))
                                        {
                                            validatedItems = PropertyRedirector.ACUrlCommand("!" + RedirectConfig.ValidationMethodName, archive.PropertyLogList) as IEnumerable<PropertyLogItem>;
                                        }

                                        validatedItems = validatedItems.OrderByDescending(c => c.Time).ToArray();

                                        int alreadyAddedValues = valuesToProcess.PropertyLogList != null ? valuesToProcess.PropertyLogList.Count : 0;
                                        int neededValues = valuesToTake - alreadyAddedValues;
                                        if (validatedItems.Count() >= neededValues)
                                        {
                                            (valuesToProcess.PropertyLogList as List<PropertyLogItem>).AddRange(validatedItems.Take(neededValues));
                                            break;
                                        }
                                        else
                                        {
                                            (valuesToProcess.PropertyLogList as List<PropertyLogItem>).AddRange(validatedItems);
                                        }
                                    }

                                    if (currentInterval < maxInterval)
                                        currentInterval++;

                                    to = from.AddMilliseconds(-10);
                                    from = from - fromIntervals[currentInterval];

                                    if (RedirectConfig.InterpolationMaxTimeSpanBackwards.HasValue)
                                    { 
                                        if (now - from >= RedirectConfig.InterpolationMaxTimeSpanBackwards)
                                            break;
                                    }
                                    else
                                    {
                                        if (counter > counterMax)
                                            break;
                                    }

                                    archive = SourceProperty.GetArchiveLog(from, to);
                                    counter++;
                                }

                                if (valuesToProcess.PropertyLogList != null && valuesToProcess.PropertyLogList.Count >= valuesToTake)
                                {
                                    valuesToProcess.SetInterpolationParams(RedirectConfig.Interpolation, RedirectConfig.InterpolationRange, RedirectConfig.InterpolationDecay);
                                    object sourceVal = valuesToProcess.PropertyLogList?.LastOrDefault()?.Value;
                                    if (sourceVal != null)
                                        sourceValue = sourceVal;
                                }
                                else if(RedirectConfig.InterpolationLackOfDataReturnValue.HasValue)
                                {
                                    sourceValue = RedirectConfig.InterpolationLackOfDataReturnValue.Value;
                                }
                            }
                        }
                    }

                    TargetProperty.Value = ACPropertyNetTargetConverter<object, object>.ConvertS2T(sourceValue, SourceProperty.PropertyType, TargetProperty.PropertyType);
                }
                catch (Exception ec)
                {
                    TargetProperty.ParentACComponent.Messages.LogException(TargetProperty.ParentACComponent.GetACUrl(), "RedirectionHandler.ConvertS2T()", ec);
                }
            }

            private TimeSpan[] DetermineBackwardTimespans(TimeSpan? maxTimeSpanBackwards)
            {
                TimeSpan[] result;

                if (maxTimeSpanBackwards.HasValue)
                {
                    double totalMinutes = maxTimeSpanBackwards.Value.TotalMinutes;

                    if(totalMinutes > 310)
                        result = new TimeSpan[] { TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(40), TimeSpan.FromMinutes(80), TimeSpan.FromMinutes(160), TimeSpan.FromMinutes(totalMinutes - 310) };
                    else if (totalMinutes > 150)
                        result = new TimeSpan[] { TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(40), TimeSpan.FromMinutes(80), TimeSpan.FromMinutes(totalMinutes - 150) };
                    else if (totalMinutes > 70)
                        result = new TimeSpan[] { TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(40), TimeSpan.FromMinutes(totalMinutes - 70) };
                    else if (totalMinutes > 30)
                        result = new TimeSpan[] { TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(totalMinutes - 30) };
                    else if (totalMinutes > 10)
                        result = new TimeSpan[] { TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(totalMinutes - 10) };
                    else
                        result = new TimeSpan[] { maxTimeSpanBackwards.Value };
                }
                else
                {
                    result = new TimeSpan[] { TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(40), TimeSpan.FromMinutes(80), TimeSpan.FromMinutes(160) };
                }

                return result;
            }
        }
        #endregion

        #region Properties
        private List<RedirectionHandler> _RedirectionHandlers = new List<RedirectionHandler>();

        [ACPropertyBindingSource]
        public IACContainerTNet<bool> IsActive { get; set; }

        #endregion

        #region Methods

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "StartRedirection":
                    StartRedirection();
                    return true;
                case "StopRedirection":
                    StopRedirection();
                    return true;
                case Const.IsEnabledPrefix + "StartRedirection":
                    result = IsEnabledStartRedirection();
                    return true;
                case Const.IsEnabledPrefix + "StopRedirection":
                    result = IsEnabledStopRedirection();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        [ACMethodInteraction("Run", "en{'Activate redirection'}de{'Umleitung aktivieren'}", 200, true)]
        public void StartRedirection()
        {
            if (!IsEnabledStartRedirection())
                return;

            var propertyQuery = PropertiesToRedirect;
            if (propertyQuery == null || !propertyQuery.Any())
                return;
            if (_CountConfigReads > 0)
                propertyQuery.ForEach(c => c.ACProperties.Refresh());
            _CountConfigReads++;
            IEnumerable<IACConfig> redirectList = propertyQuery.Where(c => c.Value != null
                                                && (c.Value is PAPropRedirectConfig)
                                                && (c.Value as PAPropRedirectConfig).ExportOff == false
                                                && !String.IsNullOrEmpty(c.LocalConfigACUrl));
            //.Select(c => c.Value as PAFilePropertyLogConfig);
            if (redirectList == null || !redirectList.Any())
                return;

            foreach (IACConfig acConfig in redirectList)
            {
                PAPropRedirectConfig redirectConfig = acConfig.Value as PAPropRedirectConfig;
                if (redirectConfig == null)
                    continue;
                if (String.IsNullOrEmpty(redirectConfig.TargetUrl))
                    continue;

                IACPropertyNetServer sourceProperty = ResolveProperty(acConfig.LocalConfigACUrl);
                if (sourceProperty == null)
                    continue;
                IACPropertyNetServer targetProperty = ResolveProperty(redirectConfig.TargetUrl);
                if (targetProperty == null)
                    continue;

                if (!ACPropertyNetTargetConverter<object, object>.AreTypesCompatible(targetProperty.PropertyType, sourceProperty.PropertyType))
                {
                    Messages.LogError(this.GetACUrl(), "InitProps4Redirection()", String.Format("Type of {0} is not compatible with {1}", acConfig.LocalConfigACUrl, redirectConfig.TargetUrl));
                    continue;
                }

                PAPropRedirectConfig propRedirectConfig = acConfig.Value as PAPropRedirectConfig;
                if (propRedirectConfig != null)
                {
                    _RedirectionHandlers.Add(new RedirectionHandler(sourceProperty, targetProperty, this, propRedirectConfig));
                }
                else
                    _RedirectionHandlers.Add(new RedirectionHandler(sourceProperty, targetProperty));
            }

            IsActive.ValueT = _RedirectionHandlers.Any();
        }

        public virtual bool IsEnabledStartRedirection()
        {
            return !_RedirectionHandlers.Any();
        }

        [ACMethodInteraction("Run", "en{'Stop redirection'}de{'Umleitung beenden'}", 201, true)]
        public void StopRedirection()
        {
            if (!IsEnabledStopRedirection())
                return;
            _RedirectionHandlers.ForEach(c => c.UnSubscribe());
            _RedirectionHandlers = new List<RedirectionHandler>();
            IsActive.ValueT = false;
        }

        public virtual bool IsEnabledStopRedirection()
        {
            return _RedirectionHandlers != null && _RedirectionHandlers.Any();
        }

        protected IACPropertyNetServer ResolveProperty(string acUrl)
        {
            int nPos = acUrl.LastIndexOf('\\');
            if (nPos < 2 || (nPos >= acUrl.Length - 1))
            {
                Messages.LogError(this.GetACUrl(), "ResolveProperty()", String.Format("acUrl {0} is empty", acUrl));
                return null;
            }

            acUrl = acUrl.Trim();
            string acUrlComponent = acUrl.Substring(0, nPos);
            string propertyName = acUrl.Substring(nPos + 1);
            ACComponent acComponent = ACUrlCommand("?" + acUrlComponent) as ACComponent;
            if (acComponent == null)
            {
                Messages.LogError(this.GetACUrl(), "ResolveProperty()", String.Format("Component {0} not found or no access rights", acUrl));
                return null;
            }

            IACPropertyNetServer resolvedProperty = acComponent.GetPropertyNet(propertyName) as IACPropertyNetServer;
            if (resolvedProperty == null)
            {
                Messages.LogError(this.GetACUrl(), "ResolveProperty()", String.Format("Property {0} in {1} not found or no access rights", propertyName, acUrl));
                return null;
            }

            return resolvedProperty;
        }

        #endregion
    }
}

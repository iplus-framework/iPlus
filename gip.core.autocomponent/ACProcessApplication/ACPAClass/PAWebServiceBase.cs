using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using gip.core.datamodel;
using CoreWCF;
using CoreWCF.Dispatcher;
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Baseclass for webservices
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAClassPhysicalBase'}de{'PAClassPhysicalBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAWebServiceBase : PAClassAlarmingBase
    {
        #region c'tors
        public PAWebServiceBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ServicePort = new ACPropertyConfigValue<int>(this, "ServicePort", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            return result;
        }

        public override bool ACPostInit()
        {
            var temp = ServicePort;
            StartService();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            StopService();

            ACCompTypeDictionary serviceTypeDict = null;
            using (ACMonitor.Lock(_S_20015_LockValue))
            {
                serviceTypeDict = _ServiceTypeDict;
            }
            if (serviceTypeDict != null)
                serviceTypeDict.DetachAll();
            using (ACMonitor.Lock(_S_20015_LockValue))
            {
                _ServiceTypeDict = null;
            }

            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }
        #endregion

        #region Properties, Range 200

        public const int HTTP_PORT = 8088;
        public const int HTTPS_PORT = 8443;
        public const int NETTCP_PORT = 8089;

        SyncQueueEvents _syncHostStart;
        ACThread _ACHostStartThread = null;

        private IWebHost _SvcHost = null;
        public IWebHost SvcHost
        {
            get
            {
                return _SvcHost;
            }
        }

        /// <summary>
        /// Type of the Service-Class that implements ServiceInterfaceType
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public abstract Type ServiceType
        {
            get;
        }

        /// <summary>
        /// Type of the Service-Interface that ServiceType must implement
        /// </summary>
        /// <value>
        /// The type of the service interface.
        /// </value>
        public abstract Type ServiceInterfaceType
        {
            get;
        }

        public virtual object GetWebServiceInstance()
        {
            return new object();
        }

        [ACPropertyBindingSource(203, "Error", "en{'Watching Alarm'}de{'Überwachungs Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsServiceAlarm { get; set; }

        [ACPropertyBindingSource(204, "Error", "en{'Error-text'}de{'Fehlertext'}", "", true, false)]
        public IACContainerTNet<String> ErrorText { get; set; }

        [ACPropertyBindingSource(205, "Error", "en{'Communication state'}de{'Kommunikationsstatus'}", "", false, false)]
        public IACContainerTNet<int> CommState { get; set; }

        private ACPropertyConfigValue<int> _ServicePort;
        [ACPropertyConfig("en{'Service Url'}de{'Service Url'}")]
        public int ServicePort
        {
            get => _ServicePort.ValueT;
            set => _ServicePort.ValueT = value;
        }

        #endregion

        #region Methods, Range 200

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "StartService":
                    StartService();
                    return true;
                case "IsEnabledStartService":
                    result = IsEnabledStartService();
                    return true;
                case "StopService":
                    StopService();
                    return true;
                case "IsEnabledStopService":
                    result = IsEnabledStopService();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #region abstract

        /// <summary>
        /// Override this method and create the ServiceHost.
        /// Afterwards the events will be automatically subscribed:
        /// _SvcHost.Opened += _SvcHost_Opened;
        /// _SvcHost.Faulted += _SvcHost_Faulted;
        /// _SvcHost.UnknownMessageReceived += _SvcHost_UnknownMessageReceived;
        /// </summary>
        /// <returns></returns>
        public IWebHost CreateService()
        {
            _SvcHost = WebHost.CreateDefaultBuilder()
                .UseKestrel(options =>
                {
                    options.ListenAnyIP(HTTP_PORT);
                    options.ListenAnyIP(HTTPS_PORT, listenOptions =>
                    {
                        listenOptions.UseHttps();
                        if (Debugger.IsAttached)
                        {
                            listenOptions.UseConnectionLogging();
                        }
                    });
                })

                .ConfigureServices(services =>
                {
                    services.AddServiceModelServices()
                   .AddServiceModelMetadata()
                   .AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
                })
                .Configure(app =>
                {
                    app.UseServiceModel(builder =>
                    {
                        builder.AddServiceEndpoint<WCFService, IWCFService>(new NetTcpBinding(), );
                    });
                })
                .Build();

            return _SvcHost;
        }

        #endregion


        public static readonly ACMonitorObject _S_20015_LockValue = new ACMonitorObject(20015);
        private static ACCompTypeDictionary _ServiceTypeDict = new ACCompTypeDictionary();
        public static ACCompTypeDictionary ServiceTypeDict
        {
            get
            {
                using (ACMonitor.Lock(_S_20015_LockValue))
                {
                    if (_ServiceTypeDict == null)
                        _ServiceTypeDict = new ACCompTypeDictionary();
                    return _ServiceTypeDict;
                }
            }
        }

        public static TResult FindPAWebService<TResult>(bool findInheritedType = true) where TResult : PAWebServiceBase
        {
            ACRoot root = gip.core.datamodel.Database.Root as ACRoot;
            if (root == null)
                return null;
            var webServices = ServiceTypeDict.GetComponentsOfType<TResult>();
            if (webServices != null && webServices.Any())
                return webServices.FirstOrDefault();
            var appManagers = root.FindChildComponents<ApplicationManager>(c => c is ApplicationManager, null, 1);
            if (appManagers == null)
                return null;
            foreach (var appManager in appManagers)
            {
                webServices = appManager.ACCompTypeDict.GetComponentsOfType<TResult>(findInheritedType);
                if (webServices != null && webServices.Any())
                {
                    TResult firstWebService = webServices.FirstOrDefault();
                    ServiceTypeDict.AddComponent(firstWebService);
                    return firstWebService;
                }
            }
            return null;
        }

        [ACMethodInteraction("Watching", "en{'Start Webservice'}de{'Starte Webdienst'}", 200, true)]
        public virtual void StartService()
        {
            StopService();
            if (!IsEnabledStartService())
                return;

            try
            {
                _SvcHost = CreateService();
                if (_SvcHost != null)
                {
                    _SvcHost.Opened += _SvcHost_Opened;
                    _SvcHost.Faulted += _SvcHost_Faulted;
                    _SvcHost.UnknownMessageReceived += _SvcHost_UnknownMessageReceived;

                    if (_SvcHost.State != CommunicationState.Opened)
                    {
                        _ACHostStartThread = new ACThread(StartHost);
                        _ACHostStartThread.Name = "ACUrl:" + this.GetACUrl() + ";StartHost();";
                        _ACHostStartThread.Start();
                    }
                }
            }
            catch (Exception e)
            {
                Messages.LogException(GetACUrl(), "StartService()", e.Message);
                if (e.InnerException != null)
                    Messages.LogException(GetACUrl(), "StartService()", e.InnerException.Message);
                StopService();
            }
        }

        public void StartHost()
        {
            _SvcHost.Start();
        }

        protected void _SvcHost_Opened(object sender, EventArgs e)
        {
            this.CommState.ValueT = (int)_SvcHost.State;
        }

        protected void _SvcHost_UnknownMessageReceived(object sender, System.ServiceModel.UnknownMessageReceivedEventArgs e)
        {
            this.CommState.ValueT = (int)_SvcHost.State;
            if (e != null)
            {
                IsServiceAlarm.ValueT = PANotifyState.AlarmOrFault;
                Messages.LogException(this.GetACUrl(), "_SvcHost_UnknownMessageReceived", e.Message.ToString());
                OnNewAlarmOccurred(IsServiceAlarm, "_SvcHost_UnknownMessageReceived", true);
            }
        }

        protected void _SvcHost_Faulted(object sender, EventArgs e)
        {
            this.CommState.ValueT = (int)_SvcHost.State;
            if (e != null)
            {
                IsServiceAlarm.ValueT = PANotifyState.AlarmOrFault;
                Messages.LogException(this.GetACUrl(), "_SvcHost_Faulted", "_SvcHost_Faulted");
                OnNewAlarmOccurred(IsServiceAlarm, "_SvcHost_Faulted", true);
            }
        }

        public void AddWebServiceAlarm(string message)
        {
            IsServiceAlarm.ValueT = PANotifyState.AlarmOrFault;
            Messages.LogException(this.GetACUrl(), "AddWebServiceAlarm", message);
            OnNewAlarmOccurred(IsServiceAlarm, message, true);
        }

        public bool IsEnabledStartService()
        {
            return _SvcHost == null;
        }

        [ACMethodInteraction("Watching", "en{'Stop Webservice'}de{'Stoppe Webdienst'}", 200, true)]
        public void StopService()
        {
            if (!IsEnabledStopService())
                return;

            try
            {
                if (_SvcHost != null)
                {
                    _SvcHost.Opened -= _SvcHost_Opened;
                    _SvcHost.Faulted -= _SvcHost_Faulted;
                    _SvcHost.UnknownMessageReceived -= _SvcHost_UnknownMessageReceived;
                }
                if (_SvcHost.State == CommunicationState.Opened)
                {
                    _SvcHost.Close();
                }
                _SvcHost = null;
            }
            catch (Exception e)
            {
                Messages.LogException(GetACUrl(), "StopService()", e.Message);
                if (e.InnerException != null)
                    Messages.LogException(GetACUrl(), "StopService()", e.InnerException.Message);
            }
        }

        public bool IsEnabledStopService()
        {
            return _SvcHost != null;
        }

        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (IsServiceAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                IsServiceAlarm.ValueT = PANotifyState.Off;
                OnAlarmDisappeared(IsServiceAlarm);
            }
            base.AcknowledgeAlarms();
        }

        public virtual PerformanceEvent OnMethodCalled(string methodName, int id = 100)
        {
            var vbDump = Root.VBDump;
            if (vbDump == null)
                return null;
            return vbDump.PerfLoggerStart(this.GetACUrl() + "!" + methodName, id);
        }

        public virtual void OnMethodReturned(PerformanceEvent perfEvent, string methodName, int id = 100)
        {
            if (perfEvent == null)
                return;
            var vbDump = Root.VBDump;
            if (vbDump == null)
                return;
            vbDump.PerfLoggerStop(this.GetACUrl() + "!" + methodName, id, perfEvent);
        }

        #endregion

    }

    public class PAWebServiceBaseErrorHandler : IErrorHandler
    {
        string _ServiceACUrl = null;
        public PAWebServiceBaseErrorHandler()
        {
        }

        public PAWebServiceBaseErrorHandler(string serviceACUrl)
        {
            _ServiceACUrl = serviceACUrl;
        }

        public bool HandleError(Exception error)
        {
            ReportServiceAlarm(error.Message);
            return false;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            ReportServiceAlarm(error.Message);
        }

        public void ReportServiceAlarm(string message)
        {
            if (!String.IsNullOrEmpty(_ServiceACUrl))
            {
                PAWebServiceBase wsHost = ACRoot.SRoot.ACUrlCommand(_ServiceACUrl) as PAWebServiceBase;
                if (wsHost != null)
                {
                    wsHost.AddWebServiceAlarm(message);
                }
            }
        }
    }

    public class PAWebServiceBaseErrorBehavior : IEndpointBehavior
    {
        string _ServiceACUrl = null;
        public PAWebServiceBaseErrorBehavior()
        {
        }

        public PAWebServiceBaseErrorBehavior(string serviceACUrl)
        {
            _ServiceACUrl = serviceACUrl;
        }

        public void ApplyDispatchBehavior(CoreWCF.Description.ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new PAWebServiceBaseErrorHandler(_ServiceACUrl));
        }

        public void AddBindingParameters(CoreWCF.Description.ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(CoreWCF.Description.ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void Validate(CoreWCF.Description.ServiceEndpoint endpoint)
        {
        }
    }

}

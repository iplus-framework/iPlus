using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Runtime.Remoting.Contexts;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace gip.core.webservices
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Json Host iPlus'}de{'Json Host iPlus'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    public partial class PAJsonServiceHost : PAWebServiceBase
    {
        #region c´tors
        public PAJsonServiceHost(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _UseCustomHttpListener = new ACPropertyConfigValue<bool>(this, nameof(UseCustomHttpListener), false);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (UseCustomHttpListener)
                InitPAJsonServiceHostListener();
            return base.ACInit(startChildMode);
        }
        #endregion

        #region Config
        private ACPropertyConfigValue<bool> _UseCustomHttpListener;
        [ACPropertyConfig("en{'Use Custom http listener'}de{'Verwende eigenen http listener'}")]
        public bool UseCustomHttpListener
        {
            get => _UseCustomHttpListener.ValueT;
            set => _UseCustomHttpListener.ValueT = value;
        }
        #endregion


        #region Implementation

        public override ServiceHost CreateService()
        {
            if (UseCustomHttpListener)
                return CreateHttpListenerService();
            else
                return CreateWCFHttpService();
        }

        protected ServiceHost CreateWCFHttpService()
        {
            int servicePort = ServicePort;
            if (servicePort <= 0)
            {
                servicePort = 8730;
                ServicePort = servicePort;
            }

            // Plattform-spezifische URI
            string strUri;
            bool isWine = !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("WINE")) ||
                           System.Environment.OSVersion.Platform == PlatformID.Unix;

            if (isWine)
            {
                // Für Wine: localhost verwenden
                strUri = String.Format("http://localhost:{0}/", servicePort);
            }
            else
            {
                // Für Windows: Standard-Wildcard
                //strUri = String.Format("http://+:{0}/", servicePort);
                strUri = String.Format("http://localhost:{0}/", servicePort);
            }

            Uri uri = new Uri(strUri);
            ServiceHost serviceHost = new ServiceHost(ServiceType, uri);
            serviceHost.Authorization.ServiceAuthorizationManager = new WSRestAuthorizationManager();

            WebHttpBinding httpBinding = new WebHttpBinding()
            {
                HostNameComparisonMode = HostNameComparisonMode.WeakWildcard,
                ContentTypeMapper = GetContentTypeMapper(),
                AllowCookies = true,
                BypassProxyOnLocal = true,
                UseDefaultWebProxy = false,
                Security = new WebHttpSecurity
                {
                    Mode = WebHttpSecurityMode.None // Falls Transport-Security Probleme macht
                }
            };
            httpBinding.MaxReceivedMessageSize = int.MaxValue;
            httpBinding.ReaderQuotas.MaxStringContentLength = 1000000;
            httpBinding.MaxBufferSize = int.MaxValue;
            httpBinding.MaxBufferPoolSize = int.MaxValue;

            ServiceEndpoint serviceEndpoint = serviceHost.AddServiceEndpoint(ServiceInterfaceType, httpBinding, "");
            if (serviceEndpoint != null)
            {
                serviceEndpoint.EndpointBehaviors.Add(new PAWebServiceBaseErrorBehavior(this.GetACUrl()));
                // Explizit WebHttpBehavior hinzufügen (WebServiceHost macht das automatisch)
                serviceEndpoint.EndpointBehaviors.Add(new WebHttpBehavior());
            }

            ServiceMetadataBehavior metad = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metad == null)
            {
                metad = new ServiceMetadataBehavior();
                serviceHost.Description.Behaviors.Add(metad);
            }
            metad.HttpGetEnabled = true;

            foreach (ServiceEndpoint endpoint in serviceHost.Description.Endpoints)
            {
                foreach (OperationDescription opDescr in endpoint.Contract.Operations)
                {
                    OnAddKnownTypesToOperationContract(endpoint, opDescr);
                }
            }

            return serviceHost;
        }

        public virtual WebContentTypeMapper GetContentTypeMapper()
        {
            return new WSJsonServiceContentTypeMapper();
        }

        protected virtual void OnAddKnownTypesToOperationContract(ServiceEndpoint endpoint, OperationDescription opDescr)
        {
            //var knownTypes = ACKnownTypes.GetKnownType();
            //foreach (var knownType in knownTypes)
            //{
            //    opDescr.KnownTypes.Add(knownType);
            //}
            //foreach (IOperationBehavior behavior in opDescr.Behaviors)
            //{
            //    if (behavior is DataContractSerializerOperationBehavior)
            //    {
            //        DataContractSerializerOperationBehavior dataContractBeh = behavior as DataContractSerializerOperationBehavior;
            //        //dataContractBeh.MaxItemsInObjectGraph = WCFServiceManager.MaxItemsInObjectGraph;
            //        dataContractBeh.DataContractResolver = ACConvert.MyDataContractResolver;
            //    }
            //}
        }

        public override Type ServiceType
        {
            get
            {
                return typeof(CoreWebService);
            }
        }

        public override Type ServiceInterfaceType
        {
            get
            {
                return typeof(ICoreWebService);
            }
        }

        private CoreWebService _serviceInstance;
        public override object GetWebServiceInstance()
        {
            if (_serviceInstance == null)
                _serviceInstance = new CoreWebService();
            return _serviceInstance;
        }

        private ConcurrentDictionary<Guid, VBUserRights> _Sessions = new ConcurrentDictionary<Guid, VBUserRights>();

        public void AddSession(VBUserRights vbUserRights)
        {
            if (vbUserRights.SessionID.HasValue)
                _Sessions.TryAdd(vbUserRights.SessionID.Value, vbUserRights);
        }

        public bool RemoveSession(Guid guid)
        {
            VBUserRights vBUserRights;
            return _Sessions.TryRemove(guid, out vBUserRights);
        }

        public VBUserRights GetRightsForSession(Guid guid)
        {
            VBUserRights vBUserRights;
            _Sessions.TryGetValue(guid, out vBUserRights);
            return vBUserRights;
        }

        #endregion

    }

    public class WSJsonServiceContentTypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            if (contentType == "text/javascript"
                || contentType == "text/plain"
                || contentType == "application/x-www-form-urlencoded")
            {
                return WebContentFormat.Json;
            }
            else
            {
                return WebContentFormat.Default;
            }
        }
    }
}

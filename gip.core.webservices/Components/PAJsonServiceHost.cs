using gip.core.autocomponent;
using gip.core.datamodel;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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
            _UseHTTPS = new ACPropertyConfigValue<bool>(this, nameof(UseHTTPS), false);
            _JsonTrashFolder = new ACPropertyConfigValue<string>(this, nameof(JsonTrashFolder), "");
            _JsonArchiveDirectory = new ACPropertyConfigValue<string>(this, nameof(JsonArchiveDirectory), "");
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _ = UseCustomHttpListener;
            _ = UseHTTPS;
            _ = JsonTrashFolder;
            _ = JsonArchiveDirectory;

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

        private ACPropertyConfigValue<bool> _UseHTTPS;
        [ACPropertyConfig("en{'Use HTTPS'}de{'Verwende HTTPS'}")]
        public bool UseHTTPS
        {
            get => _UseHTTPS.ValueT;
            set => _UseHTTPS.ValueT = value;
        }


        private ACPropertyConfigValue<string> _JsonTrashFolder;
        /// <summary>
        /// 
        /// </summary>
        [ACPropertyConfig("en{'Json Trash Folder'}de{'Json-Papierkorbordner'}")]
        public string JsonTrashFolder
        {
            get
            {
                return _JsonTrashFolder.ValueT;
            }
            set
            {
                _JsonTrashFolder.ValueT = value;
            }
        }

        private ACPropertyConfigValue<string> _JsonArchiveDirectory;
        /// <summary>
        /// 
        /// </summary>
        [ACPropertyConfig("en{'Json Archive Folder'}de{'Json-Arhivordner'}")]
        public string JsonArchiveDirectory
        {
            get
            {
                return _JsonArchiveDirectory.ValueT;
            }
            set
            {
                _JsonArchiveDirectory.ValueT = value;
            }
        }

        #endregion

        #region Properties
        [ACPropertyBindingSource(210, "Error", "en{'Import Alarm'}de{'Import Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsImportAlarm { get; set; }
        #endregion

        #region Implementation

        public override ServiceHost CreateService()
        {
            IOAuthTokenValidator oAuthTokenValidator = null;
            var searchOAuth = FindChildComponents<IOAuthTokenValidator>();
            if (searchOAuth != null)
            {
                oAuthTokenValidator = searchOAuth.FirstOrDefault();
            }

            if (oAuthTokenValidator != null && oAuthTokenValidator.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return CreateWCFHttpServiceOAuth(oAuthTokenValidator);
            }
            else
            {
                if (UseCustomHttpListener)
                    return CreateHttpListenerService();
                else
                    return CreateWCFHttpService();
            }
        }

        protected ServiceHost CreateWCFHttpService()
        {
            int servicePort = ServicePort;
            if (servicePort <= 0)
            {
                servicePort = 8730;
                ServicePort = servicePort;
            }

            string scheme = UseHTTPS ? "https" : "http";
            string strUri = String.Format("{0}://{1}:{2}/", scheme, this.Root.Environment.UserInstance.Hostname, servicePort);
            Uri uri = new Uri(strUri);
            WebServiceHost serviceHost = new WebServiceHost(ServiceType, uri);
            serviceHost.Authorization.ServiceAuthorizationManager = new WSRestAuthorizationManager();
            WebHttpBinding httpBinding = new WebHttpBinding()
            {
                ContentTypeMapper = GetContentTypeMapper(),
                AllowCookies = true
            };
            httpBinding.Security.Mode = UseHTTPS ? WebHttpSecurityMode.Transport : WebHttpSecurityMode.None;
            httpBinding.MaxReceivedMessageSize = int.MaxValue;
            httpBinding.ReaderQuotas.MaxStringContentLength = 1000000;
            httpBinding.MaxBufferSize = int.MaxValue;
            //httpBinding.MaxReceivedMessageSize = WCFServiceManager.MaxBufferSize;
            httpBinding.MaxBufferPoolSize = int.MaxValue;

            ServiceEndpoint serviceEndpoint = serviceHost.AddServiceEndpoint(ServiceInterfaceType, httpBinding, "");
            if (serviceEndpoint != null)
                serviceEndpoint.EndpointBehaviors.Add(new PAWebServiceBaseErrorBehavior(this.GetACUrl()));

            ServiceMetadataBehavior metad = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metad == null)
            {
                metad = new ServiceMetadataBehavior();
                serviceHost.Description.Behaviors.Add(metad);
            }
            metad.HttpGetEnabled = !UseHTTPS;
            metad.HttpsGetEnabled = UseHTTPS;

            foreach (ServiceEndpoint endpoint in serviceHost.Description.Endpoints)
            {
                foreach (OperationDescription opDescr in endpoint.Contract.Operations)
                {
                    OnAddKnownTypesToOperationContract(endpoint, opDescr);
                }
            }

            return serviceHost;
        }

        protected ServiceHost CreateWCFHttpServiceOAuth(IOAuthTokenValidator oAuthTokenValidator)
        {
            int servicePort = ServicePort;
            if (servicePort <= 0)
            {
                servicePort = 8730;
                ServicePort = servicePort;
            }

            string scheme = UseHTTPS ? "https" : "http";
            string strUri = String.Format("{0}://{1}:{2}/", scheme, this.Root.Environment.UserInstance.Hostname, servicePort);
            Uri uri = new Uri(strUri);
            WebServiceHost serviceHost = new WebServiceHost(ServiceType, uri);
            serviceHost.Authorization.ServiceAuthorizationManager = new OAuthBearerAuthorizationManager(oAuthTokenValidator.ValidatePrincipalFromBearerToken);
            serviceHost.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.None;

            WebHttpBinding httpBinding = new WebHttpBinding()
            {
                ContentTypeMapper = GetContentTypeMapper(),
                AllowCookies = false
            };
            httpBinding.Security.Mode = UseHTTPS ? WebHttpSecurityMode.Transport : WebHttpSecurityMode.None;
            httpBinding.MaxReceivedMessageSize = int.MaxValue;
            httpBinding.ReaderQuotas.MaxStringContentLength = 1000000;
            httpBinding.MaxBufferSize = int.MaxValue;
            httpBinding.MaxBufferPoolSize = int.MaxValue;

            ServiceEndpoint serviceEndpoint = serviceHost.AddServiceEndpoint(ServiceInterfaceType, httpBinding, "");
            if (serviceEndpoint != null)
            {
                serviceEndpoint.EndpointBehaviors.Add(new PAWebServiceBaseErrorBehavior(this.GetACUrl()));

                WebHttpBehavior webHttpBehavior = serviceEndpoint.EndpointBehaviors.OfType<WebHttpBehavior>().FirstOrDefault();
                if (webHttpBehavior == null)
                    serviceEndpoint.EndpointBehaviors.Add(new WebHttpBehavior());
            }

            ServiceMetadataBehavior metad = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metad == null)
            {
                metad = new ServiceMetadataBehavior();
                serviceHost.Description.Behaviors.Add(metad);
            }
            metad.HttpGetEnabled = !UseHTTPS;
            metad.HttpsGetEnabled = UseHTTPS;

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

        #region Log all / incorrect JSON.s

        public virtual string GetJsonFolder(Type type, string rootFolder)
        {
            string folder = null;

            if (Directory.Exists(rootFolder))
            {
                folder = Path.Combine(rootFolder, type.Name);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }


                folder = Path.Combine(folder, DateTime.Now.Year.ToString("00"));
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                folder = Path.Combine(folder, DateTime.Now.Month.ToString("00"));
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                folder = Path.Combine(folder, DateTime.Now.Day.ToString("00"));
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            else
            {
                // Error50769
                // PAJsonServiceHost
                // JsonTrashFolder is not defined or not exist! JsonTrashFolder value: {0}
                // JsonTrashFolder ist nicht definiert oder existiert nicht! JsonTrashFolder-Wert: {0}
                Msg msg = new Msg(this, eMsgLevel.Error, nameof(PAJsonServiceHost), nameof(GetJsonFolder), 130, "Error50769", JsonTrashFolder);
                IsImportAlarm.ValueT = PANotifyState.AlarmOrFault;
                ErrorText.ValueT = msg.Message;
                Messages.LogMessageMsg(msg);
                OnNewAlarmOccurred(IsImportAlarm, msg, true);
            }
            return folder;
        }

        public virtual string GetJsonFileName(Type type)
        {
            return $"{type.Name}_{DateTime.Now:yyyy-MM_dd-HH-mm-ss-fff}.json";
        }

        public virtual string WriteJsonToFolder(Type type, string rootFolder, string json)
        {
            string filePath = null;
            try
            {
                string folder = GetJsonFolder(type, rootFolder);
                if (!string.IsNullOrEmpty(folder))
                {
                    string fileName = GetJsonFileName(type);
                    filePath = Path.Combine(folder, fileName);
                    File.WriteAllText(filePath, json);
                }
            }
            catch (Exception ex)
            {
                // Error50770
                // PAJsonServiceHost
                // Error write json trash log! Message: {0}
                // Fehler beim Schreiben des JSON-Papierkorbprotokolls! Meldung: {0}
                Msg msg = new Msg(this, eMsgLevel.Error, nameof(PAJsonServiceHost), nameof(WriteJsonToFolder), 162, "Error50770", ex.Message);
                IsImportAlarm.ValueT = PANotifyState.AlarmOrFault;
                ErrorText.ValueT = msg.Message;
                Messages.LogMessageMsg(msg);
                OnNewAlarmOccurred(IsImportAlarm, msg, true);
            }
            return filePath;
        }

        public virtual (bool success, Msg msg, T item) DeserializeJson<T>(Type declaringType, string json)
        {
            try
            {
                if(Directory.Exists(JsonArchiveDirectory))
                {
                    WriteJsonToFolder(declaringType, JsonArchiveDirectory, json);
                }
                T item = JsonConvert.DeserializeObject<T>(json);
                return (true, null, item);
            }
            catch (Exception ex)
            {
                string errMsg = "";
                Exception tmpEx = ex;
                while (tmpEx != null)
                {
                    errMsg += tmpEx.Message + System.Environment.NewLine;
                    tmpEx = tmpEx.InnerException;
                }
                // Error50771
                // PAJsonServiceHost
                // Error deserializing json of type {0}! Error: {1}.
                // Fehler beim Deserialisieren von JSON vom Typ {0}! Fehler: {1}.
                Msg msg = new Msg(this, eMsgLevel.Error, nameof(PAJsonServiceHost), nameof(DeserializeJson), 190, "Error50771", typeof(T), errMsg);
                if(Directory.Exists(JsonTrashFolder))
                {
                    WriteJsonToFolder(declaringType, JsonTrashFolder, json);
                }
                return (false, msg, default);
            }
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

using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua.Configuration;
using Opc.Ua;
using Opc.Ua.Client;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPCUAClientACSession'}de{'OPCUAClientACSession'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class OPCUAClientACSession : OPCClientACSession
    {
        #region c'tors

        public const string ClassName = "OPCUAClientACSession";

        public OPCUAClientACSession(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool success = base.ACInit(startChildMode);

            if (success)
                switch(UserAuthenticationMode)
                {
                    case OPCUserAuthenticationMode.Anonymous:
                        IsAnonymousAuthMode = true;
                        break;

                    case OPCUserAuthenticationMode.Username:
                        IsUsernameAuthMode = true;
                        break;

                    case OPCUserAuthenticationMode.Certificate:
                        IsCertificateAuthMode = true;
                        break;

                }

            return success;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();
            if(result)
            {
                if (AppInstance != null && AppInstance.ApplicationConfiguration != null)
                    AppInstance.ApplicationConfiguration.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            }
            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if(AppInstance != null && AppInstance.ApplicationConfiguration != null && AppInstance.ApplicationConfiguration.CertificateValidator != null)
            {
                AppInstance.ApplicationConfiguration.CertificateValidator.CertificateValidation -= CertificateValidator_CertificateValidation;
            }

            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties

        private Session _UASession;
        public Session UASession
        {
            get
            {
                return _UASession;
            }
        }

        public OPCUAClientACService ParentOPCUAClientACService
        {
            get
            {
                if (ParentACComponent == null)
                    return null;
                return ParentACComponent as OPCUAClientACService;
            }
        }

        public ApplicationInstance AppInstance
        {
            get
            {
                if (ParentOPCUAClientACService == null)
                    return null;
                return ParentOPCUAClientACService.AppInstance;
            }
        }

        private string _EndpointURL;
        /// <summary>
        /// Gets or sets the Endpoint URL. Example: opc.tcp://localhost:51210/UA/SampleServer
        /// </summary>
        [ACPropertyInfo(410, "", "en{'Server Endpoint URL'}de{'Sever Endpoint URL'}")]
        public string EndpointURL
        {
            get
            {
                return _EndpointURL;
            }
            set
            {
                _EndpointURL = value;
            }
        }

        #region Properties => Authentication

        private OPCUserAuthenticationMode _UserAuthenticationMode;
        [ACPropertyInfo(999, "", "en{'User authentication mode'}de{'Benutzerauthentifizierungsmodus'}")]
        public OPCUserAuthenticationMode UserAuthenticationMode
        {
            get
            {
                return _UserAuthenticationMode;
            }
            set
            {
                _UserAuthenticationMode = value;
            }
        }

        private bool _IsAnonymousAuthMode;
        [ACPropertyInfo(999, "", "en{'Anonymous'}de{'Anonym'}")]
        public bool IsAnonymousAuthMode
        {
            get => _IsAnonymousAuthMode;
            set
            {
                _IsAnonymousAuthMode = value;
                if (_IsAnonymousAuthMode)
                    UserAuthenticationMode = OPCUserAuthenticationMode.Anonymous;
                OnPropertyChanged("IsAnonymousAuthMode");
            }
        }

        private bool _IsUsernameAuthMode;
        [ACPropertyInfo(999, "", "en{'Username'}de{'Benutzername'}")]
        public bool IsUsernameAuthMode
        {
            get => _IsUsernameAuthMode;
            set
            {
                _IsUsernameAuthMode = value;
                if (_IsUsernameAuthMode)
                    UserAuthenticationMode = OPCUserAuthenticationMode.Username;
                OnPropertyChanged("IsUsernameAuthMode");
            }
        }

        private bool _IsCertificateAuthMode;
        [ACPropertyInfo(999, "", "en{'Certificate'}de{'Zertifikat'}")]
        public bool IsCertificateAuthMode
        {
            get => _IsCertificateAuthMode;
            set
            {
                _IsCertificateAuthMode = value;
                if (_IsCertificateAuthMode)
                    UserAuthenticationMode = OPCUserAuthenticationMode.Certificate;
                OnPropertyChanged("IsCertificateAuthMode");
            }
        }

        private string _Username;
        [ACPropertyInfo(999, "", "en{'Username'}de{'Benutzername'}")]
        public string Username
        {
            get
            {
                return _Username;
            }
            set
            {
                _Username = value;
            }
        }

        private string _CertificateTokenPath;
        [ACPropertyInfo(999, "", "en{'Path of the certificate token'}de{'Pfad des Zertifikat-Tokens'}")]
        public string CertificateTokenPath
        {
            get
            {
                return _CertificateTokenPath;
            }
            set
            {
                _CertificateTokenPath = value;
            }
        }

        private string _Password;
        [ACPropertyInfo(999, "", "en{'Password'}de{'Passwort'}")]
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
            }
        }

        # endregion

        public int _ReconnectPeriod = 2000;
        [ACPropertyInfo(411, "", "en{'Reconnect period'}de{'Wiedereinschaltzeitraum'}", IsPersistable = true)]
        public int ReconnectPeriod
        {
            get
            {
                return _ReconnectPeriod;
            }
            set
            {
                _ReconnectPeriod = value;
                OnPropertyChanged("ReconnectPeriod");
            }
        }

        [ACPropertyBindingSource(412, "SessionAlarm", "en{'OPCUA Session Alarm'}de{'OPCUA Session Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsUASessionAlarm { get; set; }

        [ACPropertyInfo(true, 413, "", "en{'Accept untrusted certificates'}de{'Accept untrusted certificates'}")]
        public bool AcceptUntrustedCertificates
        {
            get;
            set;
        }

        #region Private fields

        private ACThread _ReconnectionThread = null;

        private ManualResetEvent _SyncReconnectionThread = null;

        private bool _StopReconnectInitiated = false;

        private ACMonitorObject _30100_ReconnectActiveLock = new ACMonitorObject(30100);

        private SessionReconnectHandler _ReconnectHandler;

        private bool _IsSettingsChanged = false;

        #endregion

        #endregion

        #region Methods

        #region Methods => Init/Deinit

        public override bool IsEnabledInitSession()
        {
            if (UASession == null)
                return true;
            if (!string.IsNullOrEmpty(EndpointURL))
                return true;
            return false;
        }

        public override bool InitSession()
        {
            if (UASession != null && !UASession.Disposed && UASession.MessageContext != null && !_IsSettingsChanged)
                return true;
            //else
            //    DeInitSession();

            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), ClassName + ".InitSession()", "Start InitSession");

            if (String.IsNullOrEmpty(EndpointURL))
                return false;

            if (AppInstance == null)
                return false;

            try
            {
                EndpointDescription selectedEndpoint = CoreClientUtils.SelectEndpoint(EndpointURL, ParentOPCUAClientACService.HasAppCertificate, 15000);
                EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(AppInstance.ApplicationConfiguration);
                ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

                if (_ReconnectTries <= 0)
                    Messages.LogDebug(this.GetACUrl(), ClassName + ".InitSession()", "New Session");
                _UASession = CreateSession(AppInstance.ApplicationConfiguration, endpoint, false, false);
            }
            catch (Exception e)
            {
                _UASession = null;
                if (_ReconnectTries <= 0)
                    Messages.LogException(this.GetACUrl(), ".InitSession()", e);
                return false;
            }

            _UASession.KeepAlive += UASession_KeepAlive;

            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), ClassName + ".InitSession()", "Init Subscriptions");

            foreach (IACObject child in this.ACComponentChilds)
            {
                if (child is OPCClientACSubscr)
                {
                    OPCClientACSubscr acSubscription = child as OPCClientACSubscr;
                    acSubscription.InitSubscription();
                }
            }

            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), ClassName + ".InitSession()", "Init Completed");
            return true;
        }

        //Deinit
        public override bool IsEnabledDeInitSession()
        {
            if (UASession != null)
                return true;
            return false;
        }

        public override bool DeInitSession()
        {
            InitiateStopReconnection();

            if (UASession == null)
                return true;

            UASession.KeepAlive -= UASession_KeepAlive;

            DisConnect();

            foreach (IACComponent child in this.ACComponentChilds)
            {
                if (child is OPCClientACSubscr)
                {
                    OPCClientACSubscr acSubscription = child as OPCClientACSubscr;
                    acSubscription.DeInitSubscription();
                }
            }

            UASession.Dispose();
            _UASession = null;

            using (ACMonitor.Lock(_30100_ReconnectActiveLock))
            {
                if (_ReconnectHandler != null)
                    _ReconnectHandler.Dispose();
                _ReconnectHandler = null;
            }

            return true;
        }

        #endregion

        #region Methods => Connect/Disconnect

        public override bool IsEnabledConnect()
        {
            if (!base.IsEnabledConnect())
                return false;

            if (UASession != null && IsConnected.ValueT)
                return false;

            return true;
        }

        private int _ReconnectTries = 0;
        public override bool Connect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return true;

            if (!InitSession())
                return false;

            if (!IsEnabledConnect())
            {
                _ReconnectTries++;
                return false;
            }

            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), ClassName + ".Connect()", "Start Connect");

            try
            {
                UserIdentity userIdentity;

                switch (UserAuthenticationMode)
                {
                    default:
                    case OPCUserAuthenticationMode.Anonymous:
                        userIdentity = new UserIdentity(new AnonymousIdentityToken());
                        break;
                    case OPCUserAuthenticationMode.Username:
                        userIdentity = new UserIdentity(Username, Password);
                        break;
                    case OPCUserAuthenticationMode.Certificate:
                        userIdentity = LoadCertificate();
                        break;
                }

                UASession.Open(ACIdentifier, userIdentity);
            }
            catch (ServiceResultException exc)
            {
                //BadIdentityTokenRejected         BadUserAccessDenied
                if (IsAlarmActive(IsUASessionAlarm, exc.Message) == null && (exc.StatusCode == 2149646336 || exc.StatusCode == 2149515264))
                    AddAlarm(new Msg(exc.Message, this, eMsgLevel.Error, ClassName, "Connect()", 377));

                if (_ReconnectTries <= 0)
                    Messages.LogException(this.GetACUrl(), ClassName + ".Connect(10)", exc);
                return false;
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), ClassName + ".Connect(20)", e);
                return false;
            }

            foreach (OPCClientACSubscr subscr in ACComponentChilds)
                subscr.Connect();

            _ReconnectTries = 0;
            Messages.LogDebug(this.GetACUrl(), ClassName + ".Connect()", "Connected");
            IsConnected.ValueT = true;
            _IsSettingsChanged = false;

            return true;
        }

        //Disconnect
        public override bool IsEnabledDisConnect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return true;

            if (UASession == null || !IsConnected.ValueT)
                return false;

            return true;
        }

        public override bool DisConnect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return true;
            if (!IsEnabledDisConnect())
                return false;

            _UASession.KeepAlive -= UASession_KeepAlive;
            UASession.Close();
            IsConnected.ValueT = false;
            return true;
        }

        #endregion

        #region Methods => Reconnect

        private void UASession_KeepAlive(Session session, KeepAliveEventArgs e)
        {
            if (e.Status != null)
            {
                if (ServiceResult.IsNotGood(e.Status))
                { 
                    IsConnected.ValueT = false;

                    using (ACMonitor.Lock(_30100_ReconnectActiveLock))
                    {
                        if (_ReconnectHandler == null && AutoReconnect)
                        {
                            _ReconnectHandler = new SessionReconnectHandler();
                            _ReconnectHandler.BeginReconnect(_UASession, ReconnectPeriod, Client_ReconnectComplete);
                        }
                    }
                }

                bool isGood = ServiceResult.IsGood(e.Status);
                if (!isGood)
                {
                    Messages.LogInfo(this.GetACUrl(), "OPCUA KeepAlive status:", e.Status.ToLongString());
                }
            }
        }

        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            SessionReconnectHandler reconnectHandler = null;
            using (ACMonitor.Lock(_30100_ReconnectActiveLock))
            {
                reconnectHandler = _ReconnectHandler;
            }

            if (!Object.ReferenceEquals(sender, reconnectHandler))
                return;

            _UASession = reconnectHandler.Session;

            try
            {
                foreach (var subs in _UASession.Subscriptions)
                {
                    subs.Delete(true);
                    subs.Create();
                }
            }
            catch(Exception exc)
            {
                if (_ReconnectTries <= 0)
                    Messages.LogException(this.GetACUrl(), "Client_ReconnectComplete(10)", exc);
                AddAlarm(new Msg(exc.Message, this, eMsgLevel.Error, ClassName, "Client_ReconnectComplete(10)", 498));
            }

            using (ACMonitor.Lock(_30100_ReconnectActiveLock))
            {
                _ReconnectHandler.Dispose();
                _ReconnectHandler = null;
            }
            _ReconnectTries = 0;
            IsConnected.ValueT = true;
        }

        protected override void StartReconnection()
        {
            if (ACOperationMode == ACOperationModes.Live && !this.IsConnected.ValueT && _ReconnectionThread == null)
            {
                _SyncReconnectionThread = new ManualResetEvent(false);
                _ReconnectionThread = new ACThread(Reconnect);
                _ReconnectionThread.Name = "ACUrl:" + this.GetACUrl() + ";Reconnect();";
                _ReconnectionThread.Start();
            }
        }

        private void Reconnect()
        {
            while (!_SyncReconnectionThread.WaitOne(2000, false))
            {
                if (IsConnected.ValueT)
                {
                    InitiateStopReconnection();
                    continue;
                }

                Connect();

                if (IsConnected.ValueT)
                    InitiateStopReconnection();
            }
        }

        private void InitiateStopReconnection()
        {
            if (_StopReconnectInitiated)
                return;
            _StopReconnectInitiated = true;
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                try
                {
                    StopReconnection();
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "InitiateStopReconnection(0)", ex.Message);
                }
            });
        }

        private void StopReconnection()
        {
            if (_ReconnectionThread != null)
            {
                if (_SyncReconnectionThread != null && _SyncReconnectionThread.SafeWaitHandle != null && !_SyncReconnectionThread.SafeWaitHandle.IsClosed)
                    _SyncReconnectionThread.Set();
                if (!_ReconnectionThread.Join(10000))
                    _ReconnectionThread.Abort();
                _ReconnectionThread = null;
            }
            _StopReconnectInitiated = false;
        }

        private void SubscribeToEvents(Session session)
        {
            if (session == null)
                return;



        }

        private void UnSubscribeFromEvents(Session session)
        {
            if (session == null)
                return;

            session.Notification -= Session_Notification;
        }

        private void Session_Notification(Session session, NotificationEventArgs e)
        {
            
        }

        #endregion

        private UserIdentity LoadCertificate()
        {
            try
            {
                X509Certificate2 certificate = new X509Certificate2(CertificateTokenPath, Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                return new UserIdentity(certificate);
            }
            catch (Exception e)
            {
                AddAlarm(new Msg("Certificate loading error: " + e.Message, this, eMsgLevel.Error, ClassName, "LoadCertificate", 520));
                Messages.LogException(ClassName, "LoadCertificate(10)", e);
                return new UserIdentity(new AnonymousIdentityToken());
            }
        }

        internal void AddAlarm(Msg message)
        {
            IsUASessionAlarm.ValueT = PANotifyState.AlarmOrFault;
            OnNewAlarmOccurred(IsUASessionAlarm, message);
        }

        private void CertificateValidator_CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            if (AcceptUntrustedCertificates)
                e.Accept = true;
            else
                e.Accept = false;
        }

        #region Methods => Static

        public static OPCUAClientSession CreateSession(ApplicationConfiguration configuration, ConfiguredEndpoint endpoint, bool updateBeforeConnect, bool checkDomain)
        {
            endpoint.UpdateBeforeConnect = updateBeforeConnect;

            EndpointDescription endpointDescription = endpoint.Description;

            // create the endpoint configuration (use the application configuration to provide default values).
            EndpointConfiguration endpointConfiguration = endpoint.Configuration;

            if (endpointConfiguration == null)
            {
                endpoint.Configuration = endpointConfiguration = EndpointConfiguration.Create(configuration);
            }

            // create message context.
            ServiceMessageContext messageContext = configuration.CreateMessageContext();

            // update endpoint description using the discovery endpoint.
            if (endpoint.UpdateBeforeConnect)
            {
                endpoint.UpdateFromServer();

                endpointDescription = endpoint.Description;
                endpointConfiguration = endpoint.Configuration;
            }

            // checks the domains in the certificate.
            if (checkDomain && endpoint.Description.ServerCertificate != null && endpoint.Description.ServerCertificate.Length > 0)
            {
                CheckCertificateDomain(endpoint);
            }

            X509Certificate2 clientCertificate = null;
            X509Certificate2Collection clientCertificateChain = null;

            if (endpointDescription.SecurityPolicyUri != SecurityPolicies.None)
            {
                if (configuration.SecurityConfiguration.ApplicationCertificate == null)
                {
                    throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "ApplicationCertificate must be specified.");
                }

                clientCertificate = configuration.SecurityConfiguration.ApplicationCertificate.Find(true).Result;

                if (clientCertificate == null)
                {
                    throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "ApplicationCertificate cannot be found.");
                }

                // load certificate chain.
                if (configuration.SecurityConfiguration.SendCertificateChain)
                {
                    clientCertificateChain = new X509Certificate2Collection(clientCertificate);
                    List<CertificateIdentifier> issuers = new List<CertificateIdentifier>();
                    configuration.CertificateValidator.GetIssuers(clientCertificate, issuers);

                    for (int i = 0; i < issuers.Count; i++)
                    {
                        clientCertificateChain.Add(issuers[i].Certificate);
                    }
                }
            }

            //if (configuration.CertificateValidator != null)
            //{
            //    configuration.CertificateValidator.CertificateValidation -= CertificateValidator_CertificateValidation;
            //    configuration.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            //}

            // initialize the channel which will be created with the server.
            ITransportChannel channel = SessionChannel.Create(
                 configuration,
                 endpointDescription,
                 endpointConfiguration,
                 clientCertificate,
                 clientCertificateChain,
                 messageContext);

            return new OPCUAClientSession(channel, configuration, endpoint, null);
        }



        private static void CheckCertificateDomain(ConfiguredEndpoint endpoint)
        {
            bool domainFound = false;

            X509Certificate2 serverCertificate = new X509Certificate2(endpoint.Description.ServerCertificate);

            // check the certificate domains.
            IList<string> domains = X509Utils.GetDomainsFromCertficate(serverCertificate);

            if (domains != null)
            {
                string hostname;
                string dnsHostName = hostname = endpoint.EndpointUrl.DnsSafeHost;
                bool isLocalHost = false;
                if (endpoint.EndpointUrl.HostNameType == UriHostNameType.Dns)
                {
                    if (dnsHostName.ToLowerInvariant() == "localhost")
                    {
                        isLocalHost = true;
                    }
                    else
                    {   // strip domain names from hostname
                        hostname = dnsHostName.Split('.')[0];
                    }
                }
                else
                {   // dnsHostname is a IPv4 or IPv6 address
                    // normalize ip addresses, cert parser returns normalized addresses
                    hostname = Utils.NormalizedIPAddress(dnsHostName);
                    if (hostname == "127.0.0.1" || hostname == "::1")
                    {
                        isLocalHost = true;
                    }
                }

                if (isLocalHost)
                {
                    dnsHostName = Utils.GetFullQualifiedDomainName();
                    hostname = Utils.GetHostName();
                }

                for (int ii = 0; ii < domains.Count; ii++)
                {
                    if (String.Compare(hostname, domains[ii], StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(dnsHostName, domains[ii], StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        domainFound = true;
                        break;
                    }
                }
            }

            if (!domainFound)
            {
                string message = Utils.Format(
                    "The domain '{0}' is not listed in the server certificate.",
                    endpoint.EndpointUrl.DnsSafeHost);
                throw new ServiceResultException(StatusCodes.BadCertificateHostNameInvalid, message);
            }
        }

        #endregion

        #endregion
    }
}

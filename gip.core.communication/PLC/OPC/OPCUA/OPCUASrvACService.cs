using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Configuration;
using gip.core.autocomponent;
using System.IO;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'iplus OPC UA Server'}de{'iplus OPC UA Server'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class OPCUASrvACService : OPCClientACService
    {
        #region c'tors

        public const string ClassName = "OPCUASrvACService";
        public const string Namespace_UA = "http://iplus-framework.com/UA/";
        public const string Namespace_UA_App = "http://iplus-framework.com/UA/Root";

        public OPCUASrvACService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            return true;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();
            _OPCUASyncQueque = new SyncQueueEvents();
            _OPCUAThread = new ACThread(ManageLifeOfOPCUAServer);
            _OPCUAThread.Name = "ACUrl:" + this.GetACUrl() + ";ManageLifeOfOPCUAServer();";
            _OPCUAThread.Start();
            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if(_OPCUAThread != null)
            {
                _OPCUASyncQueque.TerminateThread();
                _OPCUAThread.Join();
                _OPCUAThread = null;
            }

            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties

        SyncQueueEvents _OPCUASyncQueque;
        ACThread _OPCUAThread = null;
        private bool _AutoAccept = false;

        private ApplicationInstance _AppInstance = null;
        public ApplicationInstance AppInstance
        {
            get
            {
                return _AppInstance;
            }
        }

        private ApplicationConfiguration _AppConfiguration = null;
        public ApplicationConfiguration AppConfiguration
        {
            get
            {
                return _AppConfiguration;
            }
        }

        private OPCUASrvServer _OPCUASrvServer = null;

        //example => config file name: OPCUAServer.Config.xml
        //ServerAppConfigPath: C:\Temp\OPCUAClient
        private string _ServerAppConfigPath;
        [ACPropertyInfo(999, "", "en{'Path of server application configuration'}de{'Pfad zur Konfiguration der Server-Anwendung'}", "", true)]
        public string ServerAppConfigPath
        {
            get
            {
                return _ServerAppConfigPath;
            }
            set
            {
                _ServerAppConfigPath = value;
            }
        }

        public string MyServerAppConfigPath
        {
            get
            {
                string configPath = ServerAppConfigPath;
                if (String.IsNullOrEmpty(ServerAppConfigPath))
                    configPath = Path.Combine(Root.Environment.Rootpath, "OPCUAServer");
                configPath = configPath.EndsWith(".Config.xml") ? configPath.Replace(".Config.xml", "") : configPath;
                return configPath;
            }
        }

        private bool _UseCertificate;
        [ACPropertyInfo(999,"", "en{'Use certificate authentication'}de{'Zertifikatauthentifizierung verwenden'}", "", false)]
        public bool UseCertificate
        {
            get
            {
                return _UseCertificate;
            }
            set
            {
                _UseCertificate = value;
            }
        }

        private bool _HasAppCertificate = false;
        public bool HasAppCertificate
        {
            get
            {
                return _HasAppCertificate;
            }
        }

        #endregion

        #region Methods

        private void ManageLifeOfOPCUAServer()
        {
            InitOPCUAApp();
            _OPCUASyncQueque.ExitThreadEvent.WaitOne();
            DeInitOPCUAApp();
            _OPCUASyncQueque.TerminateThread();
        }

        private void InitOPCUAApp()
        {
            _AppInstance = new ApplicationInstance()
            {
                ApplicationName = "iplus OPC UA Server",
                ApplicationType = Opc.Ua.ApplicationType.Server,
                ConfigSectionName = MyServerAppConfigPath
            };

            try
            {
                Task<ApplicationConfiguration> taskConfig = _AppInstance.LoadApplicationConfiguration(true);
                if (taskConfig != null)
                    _AppConfiguration = taskConfig.Result;
                else
                    Messages.LogError(this.GetACUrl(), ClassName, "Load application configuration error!!!");
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "InitOPCUAApp(10)", e);
            }

            if (_AppConfiguration == null)
            {
                return;
            }

            if (!UseCertificate)
                _AutoAccept = true;
            //{
                Task<bool> taskCert = _AppInstance.CheckApplicationInstanceCertificate(true, 0);
                if (taskCert != null)
                {
                    _HasAppCertificate = taskCert.Result;
                    if (HasAppCertificate)
                    {
                        AppConfiguration.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(AppConfiguration.SecurityConfiguration.ApplicationCertificate.Certificate);
                        if (AppConfiguration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                            _AutoAccept = true;

                        AppConfiguration.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
                    }
                    else
                        Messages.LogError(this.GetACUrl(), ClassName, "Application certificate is missing!!!");

                }
                else
                    Messages.LogError(this.GetACUrl(), ClassName, "Check application instance certificate error!!!");
            //}
            //else
            //    _AutoAccept = true;

            // start the server.
            _OPCUASrvServer = new OPCUASrvServer(this);
            _AppInstance.Start(_OPCUASrvServer).Wait();

            //if (ReverseConnectUrl != null)
            //{
            //    Server.AddReverseConnection(ReverseConnectUrl);
            //}

            //var reverseConnections = _OPCUASrvServer.GetReverseConnections();
            //if (reverseConnections?.Count > 0)
            //{
            //    // print reverse connect info
            //    Console.WriteLine("Reverse Connect Clients:");
            //    foreach (var connection in reverseConnections)
            //    {
            //        Console.WriteLine(connection.Key);
            //    }
            //}

            // print endpoint info
            //Console.WriteLine("Server Endpoints:");
            //var endpoints = Server.GetEndpoints().Select(e => e.EndpointUrl).Distinct();
            //foreach (var endpoint in endpoints)
            //{
            //    Console.WriteLine(endpoint);
            //}

            //// start the status thread
            //Status = Task.Run(new Action(StatusThread));

            //// print notification on session events
            //Server.CurrentInstance.SessionManager.SessionActivated += EventStatus;
            //Server.CurrentInstance.SessionManager.SessionClosing += EventStatus;
            //Server.CurrentInstance.SessionManager.SessionCreated += EventStatus;

        }

        private void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = _AutoAccept;
                if (_AutoAccept)
                    Messages.LogInfo(this.GetACUrl(), ClassName, string.Format("The certificate {0} is untrusted, but it is accepted.", e.Certificate.Subject));

                else
                    Messages.LogInfo(this.GetACUrl(), ClassName, string.Format("The certificate {0} is untrusted and it is rejected.", e.Certificate.Subject));
            }
        }

        private void DeInitOPCUAApp()
        {
            if (_OPCUASrvServer != null)
                _OPCUASrvServer.ACDeInit();
            _OPCUASrvServer = null;
            _AppInstance = null;
        }

        #endregion
    }
}

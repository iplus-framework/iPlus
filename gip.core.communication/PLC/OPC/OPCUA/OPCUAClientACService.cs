using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPCUAClientACService'}de{'OPCUAClientACService'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class OPCUAClientACService : OPCClientACService
    {
        #region c'tors

        public const string ClassName = "OPCUAClientACService";

        public OPCUAClientACService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            _OPCUASyncQueque = new SyncQueueEvents();
            _OPCUAThread = new ACThread(ManageLifeOfOPCUAClient);
            _OPCUAThread.Name = "ACUrl:" + this.GetACUrl() + ";ManageLifeOfOPCUAClient();";
            _OPCUAThread.Start();

            return true;
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

        //example => config file name: OPCUAClient.Config.xml
        //ClientApplicationConfigurationPath: C:\Temp\OPCUAClient
        private string _ClientApplicationConfigurationPath;
        [ACPropertyInfo(999, "", "en{'Path of client application configuration'}de{'Pfad der Konfiguration der Client-Anwendung'}", "", true)]
        public string ClientApplicationConfigrationPath
        {
            get
            {
                return _ClientApplicationConfigurationPath;
            }
            set
            {
                _ClientApplicationConfigurationPath = value;
            }
        }

        private bool _UseCertificate;
        [ACPropertyInfo(999,"", "en{'Use certificate authentication'}de{'Zertifikatauthentifizierung verwenden'}", "",true)]
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

        private void ManageLifeOfOPCUAClient()
        {
            InitOPCUAApp();
            _OPCUASyncQueque.ExitThreadEvent.WaitOne();
            DeInitOPCUAApp();
            _OPCUASyncQueque.TerminateThread();
        }

        private void InitOPCUAApp()
        {
            string configPath = ClientApplicationConfigrationPath != null 
                             && ClientApplicationConfigrationPath.EndsWith(".Config.xml") ? ClientApplicationConfigrationPath.Replace(".Config.xml", "") 
                                                                                          : ClientApplicationConfigrationPath;

            _AppInstance = new ApplicationInstance()
            {
                ApplicationName = this.ACIdentifier,
                ApplicationType = Opc.Ua.ApplicationType.Client,
                ConfigSectionName = configPath
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

            if(_AppConfiguration == null)
            {
                return;
            }

            if (UseCertificate)
            {
                Task<bool> taskCert = _AppInstance.CheckApplicationInstanceCertificates(true);
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
            }
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
            _AppInstance = null;
        }

        #endregion
    }

    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPCUserAuthenticationMode'}de{'OPCUserAuthenticationMode'}", Global.ACKinds.TACEnum)]
    public enum OPCUserAuthenticationMode : short
    {
        Anonymous = 0,
        Username = 10,
        Certificate = 20
    }
}

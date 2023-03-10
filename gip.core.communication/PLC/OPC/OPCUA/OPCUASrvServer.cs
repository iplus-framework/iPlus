using System;
using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Server;
using System.Security.Cryptography.X509Certificates;
using gip.core.datamodel;
using System.Linq;
using System.Collections.Concurrent;
using gip.core.autocomponent;

namespace gip.core.communication
{
    /// <summary>
    /// A class which implements an instance of a UA server.
    /// </summary>
    public class OPCUASrvServer : ReverseConnectServer
    {
        #region c'tors
        public OPCUASrvServer(OPCUASrvACService acService)
        {
            _ACService = acService;
        }

        public void ACDeInit()
        {
            Stop();
            using (ACMonitor.Lock(_30210_LockValue))
            {
                _ACService = null;
            }
        }
        #endregion

        #region Properties
        public readonly ACMonitorObject _30210_LockValue = new ACMonitorObject(30210);
        private OPCUASrvACService _ACService = null;
        public OPCUASrvACService ACService
        {
            get
            {
                using (ACMonitor.Lock(_30210_LockValue))
                {
                    return _ACService;
                }
            }
        }

        #endregion


        #region Overridden Methods
        /// <summary>
        /// Initializes the server before it starts up.
        /// </summary>
        /// <remarks>
        /// This method is called before any startup processing occurs. The sub-class may update the 
        /// configuration object or do any other application specific startup tasks.
        /// </remarks>
        protected override void OnServerStarting(ApplicationConfiguration configuration)
        {
            //Utils.Trace("The server is starting.");

            base.OnServerStarting(configuration);

            // it is up to the application to decide how to validate user identity tokens.
            // this function creates validator for X509 identity tokens.
            CreateUserIdentityValidators(configuration);
        }

        /// <summary>
        /// Called after the server has been started.
        /// </summary>
        protected override void OnServerStarted(IServerInternal server)
        {
            base.OnServerStarted(server);

            // request notifications when the user identity is changed. all valid users are accepted by default.
            server.SessionManager.ImpersonateUser += new ImpersonateEventHandler(SessionManager_ImpersonateUser);
        }

        /// <summary>
        /// Cleans up before the server shuts down.
        /// </summary>
        /// <remarks>
        /// This method is called before any shutdown processing occurs.
        /// </remarks>
        protected override void OnServerStopping()
        {
            //Debug.WriteLine("The Server is stopping.");

            base.OnServerStopping();

        }

        /// <summary>
        /// Creates the node managers for the server.
        /// </summary>
        /// <remarks>
        /// This method allows the sub-class create any additional node managers which it uses. The SDK
        /// always creates a CoreNodeManager which handles the built-in nodes defined by the specification.
        /// Any additional NodeManagers are expected to handle application specific nodes.
        /// 
        /// Applications with small address spaces do not need to create their own NodeManagers and can add any
        /// application specific nodes to the CoreNodeManager. Applications should use custom NodeManagers when
        /// the structure of the address space is stored in another system or when the address space is too large
        /// to keep in memory.
        /// </remarks>
        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            //Debug.WriteLine("Creating the Node Managers.");

            List<INodeManager> nodeManagers = new List<INodeManager>();

            // create the custom node managers.
            nodeManagers.Add(new OPCUANodeManager(server, configuration, this));            
           
            // create master node manager.
            return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
        }

        /// <summary>
        /// Loads the non-configurable properties for the application.
        /// </summary>
        /// <remarks>
        /// These properties are exposed by the server but cannot be changed by administrators.
        /// </remarks>
        protected override ServerProperties LoadServerProperties()
        {
            ServerProperties properties = new ServerProperties();

            properties.ManufacturerName = "gipsoft";
            properties.ProductName = "iplus OPC UA Server";
            properties.ProductUri = "https://iplus-framework.com/";
            properties.SoftwareVersion = Utils.GetAssemblySoftwareVersion();
            properties.BuildNumber = Utils.GetAssemblyBuildNumber();
            properties.BuildDate = Utils.GetAssemblyTimestamp();

            // TBD - All applications have software certificates that need to added to the properties.

            // for (int ii = 0; ii < certificates.Count; ii++)
            // {
            //    properties.SoftwareCertificates.Add(certificates[ii]);
            // }

            return properties;
        }

        /// <summary>
        /// Initializes the address space after the NodeManagers have started.
        /// </summary>
        /// <remarks>
        /// This method can be used to create any initialization that requires access to node managers.
        /// </remarks>
        protected override void OnNodeManagerStarted(IServerInternal server)
        {
            //Debug.WriteLine("The NodeManagers have started.");

            // allow base class processing to happen first.
            base.OnNodeManagerStarted(server);
        }

        //#if USER_AUTHENTICATION
        //        /// <summary>
        //        /// Creates the resource manager for the server.
        //        /// </summary>
        //        protected override ResourceManager CreateResourceManager(IServerInternal server, ApplicationConfiguration configuration)
        //        {
        //            ResourceManager resourceManager = new ResourceManager(server, configuration);

        //            // add some localized strings to the resource manager to demonstrate that localization occurs.
        //            resourceManager.Add("InvalidPassword", "de-DE", "Das Passwort ist nicht gültig für Konto '{0}'.");
        //            resourceManager.Add("InvalidPassword", "es-ES", "La contraseña no es válida para la cuenta de '{0}'.");

        //            resourceManager.Add("UnexpectedUserTokenError", "fr-FR", "Une erreur inattendue s'est produite lors de la validation utilisateur.");
        //            resourceManager.Add("UnexpectedUserTokenError", "de-DE", "Ein unerwarteter Fehler ist aufgetreten während des Anwenders.");

        //            return resourceManager;
        //        }
        //#endif

        #region User Validation Functions
        /// <summary>
        /// Creates the objects used to validate the user identity tokens supported by the server.
        /// </summary>
        private void CreateUserIdentityValidators(ApplicationConfiguration configuration)
        {
            for (int ii = 0; ii < configuration.ServerConfiguration.UserTokenPolicies.Count; ii++)
            {
                UserTokenPolicy policy = configuration.ServerConfiguration.UserTokenPolicies[ii];

                // create a validator for a certificate token policy.
                if (policy.TokenType == UserTokenType.Certificate)
                {
                    // check if user certificate trust lists are specified in configuration.
                    if (configuration.SecurityConfiguration.TrustedUserCertificates != null &&
                        configuration.SecurityConfiguration.UserIssuerCertificates != null)
                    {
                        CertificateValidator certificateValidator = new CertificateValidator();
                        certificateValidator.Update(configuration.SecurityConfiguration).Wait();
                        certificateValidator.Update(configuration.SecurityConfiguration.UserIssuerCertificates,
                            configuration.SecurityConfiguration.TrustedUserCertificates,
                            configuration.SecurityConfiguration.RejectedCertificateStore);

                        // set custom validator for user certificates.
                        m_certificateValidator = certificateValidator.GetChannelValidator();
                    }
                }
            }
        }

        /// <summary>
        /// Called when a client tries to change its user identity.
        /// </summary>
        private void SessionManager_ImpersonateUser(Session session, ImpersonateEventArgs args)
        {
            // check for a WSS token.
            IssuedIdentityToken wssToken = args.NewIdentity as IssuedIdentityToken;

            // check for a user name token.
            UserNameIdentityToken userNameToken = args.NewIdentity as UserNameIdentityToken;

            if (userNameToken != null)
            {
                VBUser vbUser = VerifyPassword(userNameToken.UserName, userNameToken.DecryptedPassword);
                args.Identity = new OPCUAUserIdentity(userNameToken, vbUser);
                ACService.Messages.LogDebug(ACService.GetACUrl(), "SessionManager_ImpersonateUser(10)", String.Format("User {0} accepted for OPC-UA",  vbUser.VBUserName));
                //Utils.Trace("UserName Token Accepted: {0}", args.Identity.DisplayName);
                return;
            }

            // check for x509 user token.
            X509IdentityToken x509Token = args.NewIdentity as X509IdentityToken;
            if (x509Token != null)
            {
                VerifyCertificate(x509Token.Certificate);
                args.Identity = new OPCUAUserIdentity(x509Token, ACService.Root.Environment.User);
                ACService.Messages.LogDebug(ACService.GetACUrl(), "SessionManager_ImpersonateUser(20)", String.Format("X509 Token Accepted: {0}", args.Identity.DisplayName));
                return;
            }

            // construct translation object with default text.
            TranslationInfo info = new TranslationInfo(
                "IvalidAuthentication",
                "en-US",
                "Authentication needed");
            ACService.Messages.LogDebug(ACService.GetACUrl(), "SessionManager_ImpersonateUser(30)", info.Text);

            // create an exception with a vendor defined sub-code.
            throw new ServiceResultException(new ServiceResult(
                StatusCodes.BadIdentityTokenRejected,
                "IvalidAuthentication",
                OPCUASrvACService.Namespace_UA_App,
                new LocalizedText(info)));
        }

        /// <summary>
        /// Validates the password for a username token.
        /// </summary>
        private VBUser VerifyPassword(string userName, string password)
        {
            if (String.IsNullOrEmpty(password) || String.IsNullOrEmpty(userName))
            {
                // construct translation object with default text.
                TranslationInfo info = new TranslationInfo(
                    "InvalidPassword",
                    "en-US",
                    "Specified password is not valid for user '{0}'.",
                    userName);
                ACService.Messages.LogDebug(ACService.GetACUrl(), "VerifyPassword", String.Format("User {0} denied for OPC-UA", info.Text));

                // create an exception with a vendor defined sub-code.
                throw new ServiceResultException(new ServiceResult(
                    StatusCodes.BadIdentityTokenRejected,
                    "InvalidPassword",
                    OPCUASrvACService.Namespace_UA_App,
                    new LocalizedText(info)));
            }

            ACStartUpRoot startUpRoot = new ACStartUpRoot(null);
            string errorMessage = "";
            VBUser vbUser = startUpRoot.CheckLogin(userName, password, ref errorMessage);
            if (vbUser == null)
            {
                TranslationInfo info = new TranslationInfo(
                    "InvalidLogin",
                    "en-US",
                    errorMessage);
                ACService.Messages.LogDebug(ACService.GetACUrl(), "VerifyPassword", String.Format("User {0} denied for OPC-UA", info.Text));

                throw new ServiceResultException(new ServiceResult(
                    StatusCodes.BadUserAccessDenied,
                    "InvalidLogin",
                    OPCUASrvACService.Namespace_UA_App,
                    new LocalizedText(info)));
            }
            AddLoggedOnUser(vbUser);
            return vbUser;
        }

        private List<VBUser> _LoggedOnUsers = new List<VBUser>();
        public VBUser GetLoggedOnUser(string user)
        {
            using (ACMonitor.Lock(_30210_LockValue))
            {
                return _LoggedOnUsers.Where(c => c.VBUserName == user).FirstOrDefault();
            }
        }

        private void AddLoggedOnUser(VBUser vbUser)
        {
            using (ACMonitor.Lock(_30210_LockValue))
            {
                if (!_LoggedOnUsers.Contains(vbUser))
                    _LoggedOnUsers.Add(vbUser);
            }
        }

        /// <summary>
        /// Verifies that a certificate user token is trusted.
        /// </summary>
        private void VerifyCertificate(X509Certificate2 certificate)
        {
            try
            {
                if (m_certificateValidator != null)
                {
                    m_certificateValidator.Validate(certificate);
                }
                else
                {
                    CertificateValidator.Validate(certificate);
                }
            }
            catch (Exception e)
            {
                TranslationInfo info;
                StatusCode result = StatusCodes.BadIdentityTokenRejected;
                ServiceResultException se = e as ServiceResultException;
                if (se != null && se.StatusCode == StatusCodes.BadCertificateUseNotAllowed)
                {
                    info = new TranslationInfo(
                        "InvalidCertificate",
                        "en-US",
                        "'{0}' is an invalid user certificate.",
                        certificate.Subject);

                    result = StatusCodes.BadIdentityTokenInvalid;
                }
                else
                {
                    // construct translation object with default text.
                    info = new TranslationInfo(
                        "UntrustedCertificate",
                        "en-US",
                        "'{0}' is not a trusted user certificate.",
                        certificate.Subject);
                }

                // create an exception with a vendor defined sub-code.
                throw new ServiceResultException(new ServiceResult(
                    result,
                    info.Key,
                    OPCUASrvACService.Namespace_UA_App,
                    new LocalizedText(info)));
            }
        }
        #endregion

        #region Private Fields
        private ICertificateValidator m_certificateValidator;
        #endregion 

        #endregion
    }
}

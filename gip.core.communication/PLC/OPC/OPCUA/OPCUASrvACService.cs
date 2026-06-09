using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Configuration;
using gip.core.autocomponent;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;

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

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if(_OPCUAThread != null)
            {
                _OPCUASyncQueque.TerminateThread();
                _OPCUAThread.Join();
                _OPCUAThread = null;
            }

            return await base.ACDeInit(deleteACClassTask);
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
            _AppInstance = new ApplicationInstance((ITelemetryContext)null)
            {
                ApplicationName = "iplus OPC UA Server",
                ApplicationType = Opc.Ua.ApplicationType.Server,
                ConfigSectionName = MyServerAppConfigPath
            };

            try
            {
                LogCertificatePatchRuntimeMarker();
                _AppConfiguration = _AppInstance.LoadApplicationConfigurationAsync(true, default).GetAwaiter().GetResult();
                if (_AppConfiguration == null)
                {
                    Messages.LogError(this.GetACUrl(), ClassName, "UA-Server Configuration file not found!");
                    return;
                }

                if (_AppConfiguration == null)
                    return;

                EnsureDirectoryStorePreference(_AppConfiguration);
                NormalizeApplicationCertificateStorePaths(_AppConfiguration);
                EnsureSupportedPrivateKeyFormats(_AppConfiguration);
                NormalizeApplicationCertificateSubjectNames(_AppConfiguration);
                PreferPemPrivateKeysForApplicationStore(_AppConfiguration);
                PinMostRecentApplicationCertificate(_AppConfiguration);
                AttachPinnedCertificatePrivateKey(_AppConfiguration);
                LogApplicationCertificateDiagnostics(_AppConfiguration, "after-pin", -1);

                bool hasAppCertificate = false;
                Exception certificateValidationException = null;
                const int maxCertificateCheckAttempts = 4;
                for (int attempt = 0; attempt < maxCertificateCheckAttempts; attempt++)
                {
                    LogApplicationCertificateDiagnostics(_AppConfiguration, "before-check", attempt);

                    try
                    {
                        hasAppCertificate = _AppInstance.CheckApplicationInstanceCertificatesAsync(true, null, default).GetAwaiter().GetResult();
                        Messages.LogInfo(this.GetACUrl(), ClassName,
                            $"CheckApplicationInstanceCertificates attempt={attempt + 1}/{maxCertificateCheckAttempts} result={hasAppCertificate}");
                        if (hasAppCertificate)
                        {
                            LogApplicationCertificateDiagnostics(_AppConfiguration, "check-success", attempt);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        certificateValidationException = ex;
                        Messages.LogException(this.GetACUrl(), $"CheckApplicationInstanceCertificatesAttempt({attempt + 1})", ex);
                    }

                    // Newly created certs may be materialized as PFX in this pass.
                    // Convert and retry so the UA stack can load PEM on Wine.
                    if (attempt < maxCertificateCheckAttempts - 1)
                    {
                        EnsureDirectoryStorePreference(_AppConfiguration);
                        NormalizeApplicationCertificateStorePaths(_AppConfiguration);
                        EnsureSupportedPrivateKeyFormats(_AppConfiguration);
                        PreferPemPrivateKeysForApplicationStore(_AppConfiguration);
                        PinMostRecentApplicationCertificate(_AppConfiguration);
                        AttachPinnedCertificatePrivateKey(_AppConfiguration);
                        LogApplicationCertificateDiagnostics(_AppConfiguration, "after-repair", attempt);
                    }
                }

                _HasAppCertificate = hasAppCertificate;
                if (!HasAppCertificate)
                {
                    if (certificateValidationException != null)
                        Messages.LogException(this.GetACUrl(), "CheckApplicationInstanceCertificates(10)", certificateValidationException);

                    Messages.LogError(this.GetACUrl(), ClassName, "Application instance certificate invalid!");
                    return;
                }

                if (!_AppConfiguration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                    _AppConfiguration.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);

                if (!UseCertificate)
                    _AutoAccept = true;

                // start the server.
                _OPCUASrvServer = new OPCUASrvServer(this);
                _AppInstance.StartAsync(_OPCUASrvServer).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "InitOPCUAApp(10)", e);
                return;
            }

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

        private void LogCertificatePatchRuntimeMarker()
        {
            try
            {
                string assemblyPath = typeof(OPCUASrvACService).Assembly.Location;
                string assemblyLastWriteUtc = File.Exists(assemblyPath)
                    ? File.GetLastWriteTimeUtc(assemblyPath).ToString("O")
                    : "<missing>";

                Messages.LogInfo(this.GetACUrl(), ClassName,
                    $"OPCUA_CERT_PATCH_MARKER=2026-06-09c assemblyPath={assemblyPath} assemblyLastWriteUtc={assemblyLastWriteUtc}");
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "LogCertificatePatchRuntimeMarker(10)", ex);
            }
        }

        private void EnsureDirectoryStorePreference(ApplicationConfiguration configuration)
        {
            if (configuration?.SecurityConfiguration?.ApplicationCertificates == null)
                return;

            const string preferredStorePath = "%LocalApplicationData%\\OPC Foundation\\pki\\own";
            string resolvedPreferredStorePath = Utils.ReplaceSpecialFolderNames(preferredStorePath);
            string preferredCertDirectory = string.IsNullOrWhiteSpace(resolvedPreferredStorePath)
                ? string.Empty
                : Path.Combine(resolvedPreferredStorePath, "certs");
            string preferredPrivateDirectory = string.IsNullOrWhiteSpace(resolvedPreferredStorePath)
                ? string.Empty
                : Path.Combine(resolvedPreferredStorePath, "private");

            bool hasPreferredDirectoryStore = Directory.Exists(preferredCertDirectory)
                && Directory.Exists(preferredPrivateDirectory);

            if (!hasPreferredDirectoryStore)
                return;

            foreach (var certId in configuration.SecurityConfiguration.ApplicationCertificates)
            {
                if (certId == null)
                    continue;

                bool isCurrentUserMy = string.Equals(certId.StoreType, CertificateStoreType.X509Store, StringComparison.OrdinalIgnoreCase)
                    && string.Equals((certId.StorePath ?? string.Empty).Replace('/', '\\'), "CurrentUser\\My", StringComparison.OrdinalIgnoreCase);

                if (!isCurrentUserMy)
                    continue;

                string previousStoreType = certId.StoreType;
                string previousStorePath = certId.StorePath;

                certId.StoreType = CertificateStoreType.Directory;
                certId.StorePath = preferredStorePath;
                ForceThumbprintOnlyLookup(certId);

                Messages.LogInfo(this.GetACUrl(), ClassName,
                    $"Restored OPC UA application certificate store preference: storeType {previousStoreType}->{certId.StoreType}; storePath {previousStorePath}->{certId.StorePath}");
            }
        }

        private void NormalizeApplicationCertificateStorePaths(ApplicationConfiguration configuration)
        {
            if (configuration?.SecurityConfiguration?.ApplicationCertificates == null)
                return;

            foreach (var certId in configuration.SecurityConfiguration.ApplicationCertificates)
            {
                if (certId == null)
                    continue;

                if (!string.Equals(certId.StoreType, CertificateStoreType.Directory, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.IsNullOrWhiteSpace(certId.StorePath))
                    continue;

                string normalizedStorePath = certId.StorePath.Replace('/', '\\');
                if (!string.Equals(normalizedStorePath, certId.StorePath, StringComparison.Ordinal))
                {
                    string previousStorePath = certId.StorePath;
                    certId.StorePath = normalizedStorePath;
                    Messages.LogInfo(this.GetACUrl(), ClassName,
                        $"Normalized OPC UA certificate store path separators: {previousStorePath} -> {certId.StorePath}");
                }
            }
        }

        private void EnsureSupportedPrivateKeyFormats(ApplicationConfiguration configuration)
        {
            if (configuration == null)
                return;

            TryEnsureSupportedPrivateKeyFormats(configuration.ServerConfiguration, "ServerConfiguration");
            TryEnsureSupportedPrivateKeyFormats(configuration.SecurityConfiguration, "SecurityConfiguration");
        }

        private void TryEnsureSupportedPrivateKeyFormats(object configurationSection, string sectionName)
        {
            if (configurationSection == null)
                return;

            try
            {
                PropertyInfo property = configurationSection.GetType().GetProperty("SupportedPrivateKeyFormats");
                if (property == null || !property.CanRead)
                    return;

                object listObject = property.GetValue(configurationSection);
                IList list = listObject as IList;

                if (list == null && property.CanWrite)
                {
                    var ctor = property.PropertyType.GetConstructor(Type.EmptyTypes);
                    if (ctor != null)
                    {
                        listObject = Activator.CreateInstance(property.PropertyType);
                        property.SetValue(configurationSection, listObject);
                        list = listObject as IList;
                    }
                }

                if (list == null)
                    return;

                bool addedPem = EnsureListContainsIgnoreCase(list, "PEM");
                bool addedPfx = EnsureListContainsIgnoreCase(list, "PFX");

                if (addedPem || addedPfx)
                {
                    Messages.LogInfo(this.GetACUrl(), ClassName,
                        $"Ensured SupportedPrivateKeyFormats contains PEM/PFX in {sectionName}.");
                }
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), $"TryEnsureSupportedPrivateKeyFormats({sectionName})", ex);
            }
        }

        private static bool EnsureListContainsIgnoreCase(IList list, string value)
        {
            if (list == null || string.IsNullOrWhiteSpace(value))
                return false;

            foreach (var item in list)
            {
                if (string.Equals(item?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            list.Add(value);
            return true;
        }

        private static string FindCertificateFileByThumbprint(string directory, string extension, string thumbprint)
        {
            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(extension) || string.IsNullOrWhiteSpace(thumbprint))
                return null;

            if (!Directory.Exists(directory))
                return null;

            string marker = "[" + thumbprint + "]";
            return Directory.GetFiles(directory, "*" + extension)
                .Where(path => Path.GetFileName(path).IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }

        private void LogApplicationCertificateDiagnostics(ApplicationConfiguration configuration, string phase, int attempt)
        {
            if (configuration?.SecurityConfiguration?.ApplicationCertificates == null)
                return;

            int certIndex = 0;
            foreach (var certId in configuration.SecurityConfiguration.ApplicationCertificates)
            {
                certIndex++;

                if (certId == null)
                {
                    Messages.LogInfo(this.GetACUrl(), ClassName,
                        $"CertDiag phase={phase} attempt={attempt + 1} certIndex={certIndex} certId=null");
                    continue;
                }

                try
                {
                    string resolvedStorePath = string.IsNullOrWhiteSpace(certId.StorePath)
                        ? string.Empty
                        : Utils.ReplaceSpecialFolderNames(certId.StorePath);

                    string certDir = string.IsNullOrWhiteSpace(resolvedStorePath)
                        ? string.Empty
                        : Path.Combine(resolvedStorePath, "certs");
                    string privateDir = string.IsNullOrWhiteSpace(resolvedStorePath)
                        ? string.Empty
                        : Path.Combine(resolvedStorePath, "private");

                    string thumbprint = certId.Thumbprint ?? string.Empty;
                    string matchedDer = string.IsNullOrWhiteSpace(thumbprint)
                        ? null
                        : FindCertificateFileByThumbprint(certDir, ".der", thumbprint);
                    string matchedPem = string.IsNullOrWhiteSpace(thumbprint)
                        ? null
                        : FindCertificateFileByThumbprint(privateDir, ".pem", thumbprint);
                    string matchedPfx = string.IsNullOrWhiteSpace(thumbprint)
                        ? null
                        : FindCertificateFileByThumbprint(privateDir, ".pfx", thumbprint);
                    string matchedPfxBak = string.IsNullOrWhiteSpace(thumbprint)
                        ? null
                        : FindCertificateFileByThumbprint(privateDir, ".pfx.bak", thumbprint);

                    int privatePemCount = Directory.Exists(privateDir) ? Directory.GetFiles(privateDir, "*.pem").Length : 0;
                    int privatePfxCount = Directory.Exists(privateDir) ? Directory.GetFiles(privateDir, "*.pfx").Length : 0;
                    int privatePfxBakCount = Directory.Exists(privateDir) ? Directory.GetFiles(privateDir, "*.pfx.bak").Length : 0;

                    string normalizedSubject = string.IsNullOrWhiteSpace(certId.SubjectName) ? "<null>" : certId.SubjectName;
                    string normalizedThumbprint = string.IsNullOrWhiteSpace(thumbprint) ? "<null>" : thumbprint;

                    Messages.LogInfo(this.GetACUrl(), ClassName,
                        $"CertDiag phase={phase} attempt={attempt + 1} certIndex={certIndex} storeType={certId.StoreType} storePath={certId.StorePath} resolvedStorePath={resolvedStorePath} thumbprint={normalizedThumbprint} subject={normalizedSubject}");
                    Messages.LogInfo(this.GetACUrl(), ClassName,
                        $"CertDiagFiles phase={phase} attempt={attempt + 1} certIndex={certIndex} der={(matchedDer ?? "<none>")} pem={(matchedPem ?? "<none>")} pfx={(matchedPfx ?? "<none>")} pfxBak={(matchedPfxBak ?? "<none>")} privateCounts pem={privatePemCount} pfx={privatePfxCount} pfxBak={privatePfxBakCount}");
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "LogApplicationCertificateDiagnostics(10)", ex);
                }
            }
        }

        private void NormalizeApplicationCertificateSubjectNames(ApplicationConfiguration configuration)
        {
            if (configuration?.SecurityConfiguration?.ApplicationCertificates == null)
                return;

            foreach (var certId in configuration.SecurityConfiguration.ApplicationCertificates)
            {
                if (certId == null)
                    continue;

                if (string.IsNullOrWhiteSpace(certId.SubjectName))
                    continue;

                try
                {
                    string normalizedSubject = certId.SubjectName
                        .Replace(", S=", ", ST=", StringComparison.OrdinalIgnoreCase)
                        .Replace("/S=", "/ST=", StringComparison.OrdinalIgnoreCase);

                    if (normalizedSubject.StartsWith("S=", StringComparison.OrdinalIgnoreCase))
                    {
                        normalizedSubject = "ST=" + normalizedSubject.Substring(2);
                    }

                    if (!string.Equals(normalizedSubject, certId.SubjectName, StringComparison.Ordinal))
                    {
                        certId.SubjectName = normalizedSubject;
                        Messages.LogInfo(this.GetACUrl(), ClassName, $"Normalized OPC UA certificate subject for lookup: {normalizedSubject}");
                    }
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "NormalizeApplicationCertificateSubjectNames(10)", ex);
                }
            }
        }

        private void PreferPemPrivateKeysForApplicationStore(ApplicationConfiguration configuration)
        {
            if (configuration?.SecurityConfiguration?.ApplicationCertificates == null)
                return;

            foreach (var certId in configuration.SecurityConfiguration.ApplicationCertificates)
            {
                if (certId == null)
                    continue;

                if (!string.Equals(certId.StoreType, CertificateStoreType.Directory, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.IsNullOrWhiteSpace(certId.StorePath))
                    continue;

                try
                {
                    string storePath = Utils.ReplaceSpecialFolderNames(certId.StorePath);
                    if (string.IsNullOrWhiteSpace(storePath))
                        continue;

                    string privateKeyDirectory = Path.Combine(storePath, "private");
                    if (!Directory.Exists(privateKeyDirectory))
                        continue;

                    var pfxCandidates = Directory.GetFiles(privateKeyDirectory, "*.pfx")
                        .Concat(Directory.GetFiles(privateKeyDirectory, "*.pfx.bak"))
                        .OrderByDescending(File.GetLastWriteTimeUtc)
                        .ToList();

                    foreach (string pfxFilePath in pfxCandidates)
                    {
                        bool isBackupPfx = pfxFilePath.EndsWith(".pfx.bak", StringComparison.OrdinalIgnoreCase);
                        string sourcePfxPath = isBackupPfx ? pfxFilePath.Substring(0, pfxFilePath.Length - 4) : pfxFilePath;
                        string pemFilePath = Path.ChangeExtension(sourcePfxPath, ".pem");

                        if (isBackupPfx && File.Exists(pemFilePath))
                            continue;

                        if (!TryConvertPfxToPem(pfxFilePath, pemFilePath))
                            continue;

                        if (!isBackupPfx)
                        {
                            string backupFilePath = pfxFilePath + ".bak";
                            if (File.Exists(backupFilePath))
                                File.Delete(backupFilePath);

                            File.Move(pfxFilePath, backupFilePath);
                            Messages.LogInfo(this.GetACUrl(), ClassName, $"Renamed OPC UA private key file to PEM-preferred mode: {backupFilePath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "PreferPemPrivateKeysForApplicationStore(10)", ex);
                }
            }
        }

        private void PinMostRecentApplicationCertificate(ApplicationConfiguration configuration)
        {
            if (configuration?.SecurityConfiguration?.ApplicationCertificates == null)
                return;

            foreach (var certId in configuration.SecurityConfiguration.ApplicationCertificates)
            {
                if (certId == null)
                    continue;

                if (!string.Equals(certId.StoreType, CertificateStoreType.Directory, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.IsNullOrWhiteSpace(certId.StorePath))
                    continue;

                try
                {
                    string storePath = Utils.ReplaceSpecialFolderNames(certId.StorePath);
                    if (string.IsNullOrWhiteSpace(storePath))
                        continue;

                    string certDirectory = Path.Combine(storePath, "certs");
                    string privateKeyDirectory = Path.Combine(storePath, "private");
                    if (!Directory.Exists(certDirectory) || !Directory.Exists(privateKeyDirectory))
                        continue;

                    string selectedCertPath = null;
                    string selectedCertThumbprint = null;
                    string selectedCertSubject = null;

                    var certificateCandidates = Directory.GetFiles(certDirectory, "*.der")
                        .OrderByDescending(File.GetLastWriteTimeUtc)
                        .ToList();

                    foreach (string certPath in certificateCandidates)
                    {
                        string certFileNameWithoutExtension = Path.GetFileNameWithoutExtension(certPath);
                        if (string.IsNullOrWhiteSpace(certFileNameWithoutExtension))
                            continue;

                        string pemPath = Path.Combine(privateKeyDirectory, certFileNameWithoutExtension + ".pem");
                        if (!File.Exists(pemPath))
                            continue;

                        if (!TryValidatePemPrivateKeyForCertificate(certPath, pemPath, out string validationError))
                        {
                            Messages.LogInfo(this.GetACUrl(), ClassName,
                                $"Skipping OPC UA certificate candidate (unloadable private key): {Path.GetFileName(certPath)} ({validationError})");
                            continue;
                        }

                        using var certificate = X509CertificateLoader.LoadCertificateFromFile(certPath);
                        selectedCertPath = certPath;
                        selectedCertThumbprint = certificate.Thumbprint;
                        selectedCertSubject = certificate.Subject;
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(selectedCertPath) || string.IsNullOrWhiteSpace(selectedCertThumbprint))
                        continue;

                    certId.Thumbprint = selectedCertThumbprint;
                    // Keep subject empty so UA lookup uses thumbprint only.
                    // DirectoryCertificateStore still applies subject filtering if present,
                    // and subject serialization differences (S vs ST, ordering) can reject
                    // an otherwise correct thumbprint match.
                    certId.SubjectName = string.Empty;

                    Messages.LogInfo(this.GetACUrl(), ClassName,
                        $"Pinned OPC UA application certificate: thumbprint={certId.Thumbprint}, cert={Path.GetFileName(selectedCertPath)}, subject={selectedCertSubject}");
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "PinMostRecentApplicationCertificate(10)", ex);
                }
            }
        }

        private void AttachPinnedCertificatePrivateKey(ApplicationConfiguration configuration)
        {
            if (configuration?.SecurityConfiguration?.ApplicationCertificates == null)
                return;

            foreach (var certId in configuration.SecurityConfiguration.ApplicationCertificates)
            {
                if (certId == null)
                    continue;

                if (!string.Equals(certId.StoreType, CertificateStoreType.Directory, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.IsNullOrWhiteSpace(certId.StorePath) || string.IsNullOrWhiteSpace(certId.Thumbprint))
                    continue;

                try
                {
                    string storePath = Utils.ReplaceSpecialFolderNames(certId.StorePath);
                    if (string.IsNullOrWhiteSpace(storePath))
                        continue;

                    string certDirectory = Path.Combine(storePath, "certs");
                    string privateKeyDirectory = Path.Combine(storePath, "private");
                    if (!Directory.Exists(certDirectory) || !Directory.Exists(privateKeyDirectory))
                        continue;

                    string thumbprint = certId.Thumbprint;
                    string derPath = FindCertificateFileByThumbprint(certDirectory, ".der", thumbprint);
                    string pemPath = FindCertificateFileByThumbprint(privateKeyDirectory, ".pem", thumbprint);
                    if (string.IsNullOrWhiteSpace(derPath) || string.IsNullOrWhiteSpace(pemPath))
                        continue;

                    using var publicCertificate = X509CertificateLoader.LoadCertificateFromFile(derPath);
                    string pemPrivateKeyText = File.ReadAllText(pemPath);

                    X509Certificate2 certificateWithPrivateKey = null;
                    using RSA rsaPublic = publicCertificate.GetRSAPublicKey();
                    if (rsaPublic != null)
                    {
                        using var rsaPrivate = RSA.Create();
                        rsaPrivate.ImportFromPem(pemPrivateKeyText);
                        certificateWithPrivateKey = publicCertificate.CopyWithPrivateKey(rsaPrivate);
                    }
                    else
                    {
                        using ECDsa ecdsaPublic = publicCertificate.GetECDsaPublicKey();
                        if (ecdsaPublic != null)
                        {
                            using var ecdsaPrivate = ECDsa.Create();
                            ecdsaPrivate.ImportFromPem(pemPrivateKeyText);
                            certificateWithPrivateKey = publicCertificate.CopyWithPrivateKey(ecdsaPrivate);
                        }
                    }

                    if (certificateWithPrivateKey == null || !certificateWithPrivateKey.HasPrivateKey)
                    {
                        Messages.LogInfo(this.GetACUrl(), ClassName,
                            $"AttachPinnedCertificatePrivateKey skipped: unable to create private-key certificate for thumbprint={thumbprint}");
                        continue;
                    }

                    bool attached = TrySetIdentifierCertificate(certId, certificateWithPrivateKey, out string attachInfo);
                    if (attached)
                    {
                        certId.Thumbprint = certificateWithPrivateKey.Thumbprint;
                        ForceThumbprintOnlyLookup(certId);
                        EnsurePfxFallback(privateKeyDirectory, derPath, certificateWithPrivateKey);

                        Messages.LogInfo(this.GetACUrl(), ClassName,
                            $"Attached private-key certificate to identifier for thumbprint={certId.Thumbprint}; {attachInfo}; storeType={certId.StoreType}; storePath={certId.StorePath}");
                    }
                    else
                    {
                        Messages.LogInfo(this.GetACUrl(), ClassName,
                            $"AttachPinnedCertificatePrivateKey could not attach certificate object for thumbprint={thumbprint}; {attachInfo}");
                        certificateWithPrivateKey.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "AttachPinnedCertificatePrivateKey(10)", ex);
                }
            }
        }

        private void EnsurePfxFallback(string privateKeyDirectory, string derPath, X509Certificate2 certificateWithPrivateKey)
        {
            if (string.IsNullOrWhiteSpace(privateKeyDirectory) || string.IsNullOrWhiteSpace(derPath) || certificateWithPrivateKey == null)
                return;

            try
            {
                if (!Directory.Exists(privateKeyDirectory))
                    return;

                string baseName = Path.GetFileNameWithoutExtension(derPath);
                if (string.IsNullOrWhiteSpace(baseName))
                    return;

                string pfxPath = Path.Combine(privateKeyDirectory, baseName + ".pfx");
                byte[] pfxContent = certificateWithPrivateKey.Export(X509ContentType.Pkcs12, string.Empty);
                File.WriteAllBytes(pfxPath, pfxContent);

                Messages.LogInfo(this.GetACUrl(), ClassName,
                    $"Wrote OPC UA private key fallback PFX for thumbprint={certificateWithPrivateKey.Thumbprint}: {pfxPath}");
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "EnsurePfxFallback(10)", ex);
            }
        }

        private static void ForceThumbprintOnlyLookup(CertificateIdentifier certId)
        {
            if (certId == null)
                return;

            try
            {
                // Preferred path: clear public property.
                certId.SubjectName = null;
                certId.SubjectName = string.Empty;

                // Defensive path: some UA versions cache the value in backing/private fields.
                var certType = certId.GetType();
                const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

                var candidateFieldNames = new[]
                {
                    "m_subjectName",
                    "_subjectName",
                    "<SubjectName>k__BackingField"
                };

                foreach (var fieldName in candidateFieldNames)
                {
                    var field = certType.GetField(fieldName, instanceFlags);
                    if (field != null && field.FieldType == typeof(string))
                    {
                        field.SetValue(certId, string.Empty);
                    }
                }
            }
            catch
            {
                // Best-effort only: leave as-is if reflection is blocked.
            }
        }

        private static bool TrySetIdentifierCertificate(CertificateIdentifier certId, X509Certificate2 certificateWithPrivateKey, out string attachInfo)
        {
            attachInfo = "certificate property not found";
            if (certId == null || certificateWithPrivateKey == null)
                return false;

            PropertyInfo certificateProperty = certId.GetType().GetProperty("Certificate");
            if (certificateProperty == null || !certificateProperty.CanWrite)
            {
                attachInfo = "identifier has no writable Certificate property";
                return false;
            }

            if (!certificateProperty.PropertyType.IsAssignableFrom(certificateWithPrivateKey.GetType()))
            {
                attachInfo = $"certificate property type mismatch: {certificateProperty.PropertyType.FullName}";
                return false;
            }

            certificateProperty.SetValue(certId, certificateWithPrivateKey);
            attachInfo = $"assigned {certificateWithPrivateKey.GetType().FullName} to {certificateProperty.PropertyType.FullName}";
            return true;
        }

        private static bool TryValidatePemPrivateKeyForCertificate(string certificatePath, string pemPrivateKeyPath, out string validationError)
        {
            validationError = null;
            try
            {
                using var certificate = X509CertificateLoader.LoadCertificateFromFile(certificatePath);
                string pemPrivateKeyText = File.ReadAllText(pemPrivateKeyPath);

                using RSA rsaPublic = certificate.GetRSAPublicKey();
                if (rsaPublic != null)
                {
                    using var rsaPrivate = RSA.Create();
                    rsaPrivate.ImportFromPem(pemPrivateKeyText);
                    using var certificateWithPrivateKey = certificate.CopyWithPrivateKey(rsaPrivate);
                    return certificateWithPrivateKey.HasPrivateKey;
                }

                using ECDsa ecdsaPublic = certificate.GetECDsaPublicKey();
                if (ecdsaPublic != null)
                {
                    using var ecdsaPrivate = ECDsa.Create();
                    ecdsaPrivate.ImportFromPem(pemPrivateKeyText);
                    using var certificateWithPrivateKey = certificate.CopyWithPrivateKey(ecdsaPrivate);
                    return certificateWithPrivateKey.HasPrivateKey;
                }

                validationError = "Unsupported certificate public key algorithm.";
                return false;
            }
            catch (Exception ex)
            {
                validationError = ex.Message;
                return false;
            }
        }

        private bool TryConvertPfxToPem(string pfxFilePath, string pemFilePath)
        {
            try
            {
                var pkcs12Store = new Pkcs12StoreBuilder().Build();
                using (var pfxStream = File.OpenRead(pfxFilePath))
                {
                    // OPC UA generated own-store PFX files use an empty password by default.
                    pkcs12Store.Load(pfxStream, Array.Empty<char>());
                }

                string keyAlias = null;
                foreach (string alias in pkcs12Store.Aliases)
                {
                    if (pkcs12Store.IsKeyEntry(alias))
                    {
                        keyAlias = alias;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(keyAlias))
                {
                    Messages.LogError(this.GetACUrl(), ClassName, $"No private key entry found in PFX file: {pfxFilePath}");
                    return false;
                }

                var keyEntry = pkcs12Store.GetKey(keyAlias);
                if (keyEntry == null)
                {
                    Messages.LogError(this.GetACUrl(), ClassName, $"Failed to read private key entry from PFX file: {pfxFilePath}");
                    return false;
                }

                using (var writer = new StringWriter())
                {
                    var pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(writer);
                    // Write key in native PEM form (e.g. RSA PRIVATE KEY / EC PRIVATE KEY)
                    // for best compatibility with UA PEMReader key-type parsing.
                    pemWriter.WriteObject(keyEntry.Key);
                    pemWriter.Writer.Flush();
                    File.WriteAllText(pemFilePath, writer.ToString());
                }

                Messages.LogInfo(this.GetACUrl(), ClassName, $"Converted OPC UA private key file to PEM: {pemFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "TryConvertPfxToPem(10)", ex);
                return false;
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
            if (_OPCUASrvServer != null)
                _OPCUASrvServer.ACDeInit();
            _OPCUASrvServer = null;
            _AppInstance = null;
        }

        #endregion
    }
}

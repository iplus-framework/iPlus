// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Collections;
using System.Reflection;
using gip.core.licensing;

namespace gip.core.datamodel.Licensing
{
    /// <summary>
    /// License engine of the iPlus runtime.
    /// </summary>
    /// <remarks>
    /// V5 is purely software based: licenses are offline signed <c>*.gip</c> files
    /// that are validated with asymmetric cryptography (see
    /// <see cref="License.FileLicensing"/>). The V4 USB dongle support
    /// (TDi Matrix32/Matrix64) has been removed — it required a native Windows
    /// driver that does not exist on Linux/macOS and aborted startup with a
    /// <c>DllNotFoundException</c>.
    /// </remarks>

    public sealed partial class License : INotifyPropertyChanged
    {
        #region c'tors
        public License(VBUser vbUser, Database database)
        {
            //if (_SingleInstance)
            //    throw new SystemException("Only one License-Instance can be created!");
            //_SingleInstance = true;
            _VBUser = vbUser;
            _Database = database;
            Initialize(_Database);
        }


        private void Initialize(Database db)
        {
            NeedRestart = false;
            // Load signed license files (folder + activated database rows) and
            // derive the license type from them.
            ReloadFileLicenses();
        }
        #endregion


        #region Private Fields
        //private static bool _SingleInstance = false;

        private string _SystemComm = "";

        private VBUser _VBUser;


//#if (!DEBUG)
//        private TimeSpan _TrialTime = new TimeSpan(0, 45, 0);

//        private TimeSpan _PackageRemainingTime = new TimeSpan(48, 0, 0);

//        private TimeSpan _CyclicDongleReadInterval = new TimeSpan(0, 0, 30);

//        private DateTime? _NextCyclicDongleRead = null;

//        private DateTime _StartDT;
//#endif

        /// <summary>
        /// Key which indentifies the Development-License in the VBSystem-Table (Its for companies which develop their own packages an can issue licesnse for it)
        /// </summary>
        private const string SN_PackIssuer = "VBD";

        /// <summary>
        /// Key which indentifies Package-Licenses in the VBSystem-Table
        /// </summary>
        private const string SN_PackLicense = "VBP";

        #endregion


        #region Properties
        private Database _Database;

        private string _DatabaseName;
        internal string DatabaseName
        {
            get
            {
                if (_DatabaseName == null)
                    _DatabaseName = ((DbConnection)_Database.Connection).Database;
                return _DatabaseName;
            }
        }

        private Guid? _VarioSystemID;
        private Guid? VarioSystemID
        {
            get
            {
                if (!_VarioSystemID.HasValue)
                {

                    using (ACMonitor.Lock(_Database.QueryLock_1X000))
                    {
                        var package = _Database.ACPackage.FirstOrDefault(c => c.ACPackageName == Const.PackName_VarioSystem);
                        if (package != null)
                        {
                            _VarioSystemID = package.ACPackageID;
                        }
                    }
                }
                return _VarioSystemID;
            }
        }

        private Guid? _SystemID;
        private Guid? SystemID
        {
            get
            {
                if (!_SystemID.HasValue)
                {

                    using (ACMonitor.Lock(_Database.QueryLock_1X000))
                    {
                        var package = _Database.ACPackage.FirstOrDefault(c => c.ACPackageName == Const.PackName_System);
                        if (package != null)
                        {
                            _SystemID = package.ACPackageID;
                        }
                    }
                }
                return _SystemID;
            }
        }

        private object _LPLock = new object();
        private List<ACPackage> _LicensedPackages;
        public IEnumerable<ACPackage> LicensedPackages
        {
            get
            {
                lock (_LPLock)
                {
                    if (_LicensedPackages == null)
                        return null;
                    return _LicensedPackages.AsReadOnly();
                }
            }
        }

        public List<VBSystem> SystemList
        {
            get
            {
                using (ACMonitor.Lock(_Database.QueryLock_1X000))
                {
                    return _Database.VBSystem.Where(c => c.SystemName == License.SN_PackLicense).ToList();
                }
            }
        }

        public LicenseType CurrentLicenseType
        {
            get
            {
                // Signed license files are the only source of the license type in V5.
                if (IsDeveloperWithIssuingRightsByFile)
                    return LicenseType.Developer_Issuer;
                if (IsDeveloperByFile)
                    return LicenseType.Developer_EndUser;
                if (_IsRemoteLoginActive)
                    return LicenseType.RemoteDeveloper;

                // V5 has no trial: without a license the installation runs the open
                // (GPL-3) edition, which is fully usable and not time limited.
                return LicenseType.User;
            }
        }

        /// <summary>
        /// Returns if Trial. (Doesn't read from dongle - uses the dongle cache.)
        /// </summary>
        public bool IsTrial
        {
            get
            {
                return CurrentLicenseType == LicenseType.Trial;
            }
        }

        /// <summary>
        /// <c>true</c> when the installation runs under the open (GPL-3) edition,
        /// i.e. no framework runtime license is present. The system is fully usable;
        /// only commercial-only packages are blocked.
        /// </summary>
        public bool IsOpenEdition
        {
            get
            {
                return !HasFrameworkRuntimeLicense;
            }
        }

        /// <summary>
        /// Returns if has development rights. (Doesn't read from dongle - uses the dongle cache.)
        /// </summary>
        public bool IsDeveloper
        {
            get
            {
                return CurrentLicenseType > LicenseType.Trial && CurrentLicenseType < LicenseType.RemoteDeveloper;
            }
        }

        /// <summary>
        /// Returns if logged in over temporary activation key. (Doesn't read from dongle - uses the dongle cache.)
        /// </summary>
        public bool IsRemoteDeveloper
        {
            get
            {
                return CurrentLicenseType == LicenseType.RemoteDeveloper;
            }
        }

        /// <summary>
        /// Determines if the installation has the right to issue product licenses
        /// for its own packages (DeveloperIssuer license file).
        /// </summary>
        public bool IsDeveloperWithIssuingRights
        {
            get
            {
                return IsDeveloperWithIssuingRightsByFile;
            }
        }

        public bool IsTrialTimeExpired
        {
            get
            {
                // V5 has no trial. Kept for API compatibility: the "trial" is over
                // once the block grace period has elapsed without a valid license.
                return IsLicenseBlockDue;
            }
        }

        public bool IsPackageTimeExpired
        {
            get
            {
                // Package usage is blocked when the grace period has elapsed and at
                // least one package is not covered by a license.
                return IsLicenseBlockDue;
            }
        }

        /// <summary>
        /// Attention: This Property reads the dongle cyclic!
        /// </summary>
        public bool MayUserDevelop
        {
            get
            {
                // In V5 there is no developer dongle any more: everybody may develop
                // and sell solutions. Only issuing rights (creating product licenses
                // for customers) require a DeveloperIssuer license file.
                return true;
            }
        }

        /// <summary>
        /// <c>true</c> when a license is installed for this installation: a
        /// framework runtime license or a developer license file.
        /// </summary>
        /// <remarks>
        /// The open (GPL-3) edition is fully usable but has no license installed, so
        /// this returns <c>false</c> and the activation BSO offers the
        /// runtime-license installation.
        /// </remarks>
        public bool IsLicenseValid
        {
            get
            {
                return HasFrameworkRuntimeLicense || IsDeveloperByFile;
            }
        }

        public string LicensedTo
        {
            get
            {
                // Prefer the signed developer license file (V5 path).
                LicensePayload issuer = FileLicenses.FirstOrDefault(p => p.Kind == LicenseKind.DeveloperIssuer);
                if (issuer != null && !string.IsNullOrEmpty(issuer.CustomerName))
                    return issuer.CustomerName;

                string temp = "";
                VBSystem sys = null;
                using (ACMonitor.Lock(_Database.QueryLock_1X000))
                {
                    sys = _Database.VBSystem.FirstOrDefault(c => c.SystemName == License.SN_PackIssuer);
                }
                if (sys == null)
                {
                    using (ACMonitor.Lock(_Database.QueryLock_1X000))
                    {
                        sys = _Database.VBSystem.FirstOrDefault(c => c.SystemName == License.SN_PackLicense);
                    }
                }

                if (sys != null)
                    temp = sys.CustomerName;
                return temp;
            }
        }

        public string LicensedToTitle
        {
            get
            {
                if (string.IsNullOrEmpty(LicensedTo))
                    return "";
                return string.Format(" - {0} ", LicensedTo);
            }
        }

        private bool _NeedRestart;
        public bool NeedRestart
        {
            get
            {
                return _NeedRestart;
            }
            private set
            {
                _NeedRestart = value;
                OnPropertyChanged("NeedRestart");
            }
        }

#endregion


#region Methods


#region Has package license checks

        /// <summary>
        /// Returns <c>true</c> when the given package may run in the current
        /// installation.
        /// </summary>
        /// <remarks>
        /// Three situations exist:
        /// <list type="bullet">
        /// <item><b>Developer installation</b> (a <c>DeveloperIssuer</c> or
        ///       <c>DeveloperEndUser</c> license file): everything may run, so the
        ///       developer can build and test both GPL and commercial packages.</item>
        /// <item><b>Open edition</b> (no framework runtime license): every package
        ///       may run except commercial-only ones. A product license alone is
        ///       <b>not</b> effective — the end customer must also hold a framework
        ///       runtime license.</item>
        /// <item><b>Commercial edition</b> (framework runtime license present): the
        ///       runtime license covers all dual-licensed and permissive packages;
        ///       commercial-only packages additionally need a product license; pure
        ///       copyleft (GPL-3) packages are blocked, because a third party cannot
        ///       relicense GPL code and mixing it with commercially licensed iPlus
        ///       would violate the GPL.</item>
        /// </list>
        /// Blocked packages do not fail immediately: the grace period gives the end
        /// user time to resolve the license issue (30 min warning, then
        /// <c>ACUrlCommand</c> blocked after 1–2 days — V4 parity).
        /// </remarks>
        public bool IsPackageLicensed(ACPackage acPackage)
        {
            if (acPackage == null)
                return false;

            if (acPackage.ACPackageName == Const.PackName_VarioLicense
                || acPackage.ACPackageName == Const.PackName_System
                || acPackage.ACPackageName == Const.PackName_VarioSystem)
                return true;

            bool allowed = IsPackageAllowedByLicenseExpression(acPackage);

            if (allowed)
                return true;

            StartLicenseGracePeriod();
            return !IsLicenseBlockDue;
        }

        /// <summary>
        /// Returns the license state of a component's package.
        /// <see cref="ComponentLicense.False"/> makes
        /// <c>ACComponent.ACUrlCommand</c> return <c>null</c>, so the component
        /// stops working — this is the server-side block after the grace period.
        /// </summary>
        public ComponentLicense IsComponentLicensed(Guid packageID)
        {
            if ((VarioSystemID.HasValue && VarioSystemID == packageID)
                || (SystemID.HasValue && SystemID == packageID))
                return ComponentLicense.True;

            bool cached;
            lock (_LPLock)
            {
                if (_ComponentLicenseCache.TryGetValue(packageID, out cached))
                    return cached ? ComponentLicense.True : ComponentLicense.False;
            }

            ACPackage acPackage;
            using (ACMonitor.Lock(_Database.QueryLock_1X000))
            {
                acPackage = _Database.ACPackage.FirstOrDefault(c => c.ACPackageID == packageID);
            }

            if (acPackage == null)
                return ComponentLicense.TimeCheck;

            bool licensed = IsPackageLicensed(acPackage);

            // Do not cache a "licensed" result while the grace period is running:
            // the package is only temporarily allowed and must flip to False once
            // the block grace period has elapsed.
            if (licensed && LicenseGraceStartUtc.HasValue)
                return ComponentLicense.True;

            lock (_LPLock)
            {
                _ComponentLicenseCache[packageID] = licensed;
            }

            return licensed ? ComponentLicense.True : ComponentLicense.False;
        }

        private readonly Dictionary<Guid, bool> _ComponentLicenseCache = new Dictionary<Guid, bool>();

        /// <summary>
        /// Clears the per-package license cache. Called when license files are
        /// reloaded or activated.
        /// </summary>
        internal void InvalidateComponentLicenseCache()
        {
            lock (_LPLock)
            {
                _ComponentLicenseCache.Clear();
            }
        }

#endregion


#region Package reading
        /// <summary>
        /// Reads all package-licenses and fills _AvailablePackages-Member
        /// It iterates all VBSystem-Entries (= customers who sells their own packages) and verifies the license
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private void VerifyAndReadAllPackages(Database db)
        {
            lock (_LPLock)
            {
                _LicensedPackages = new List<ACPackage>();
            }
            OnPropertyChanged("LicensedPackages");
            List<VBSystem> vbSystemList = null;

            using (ACMonitor.Lock(db.QueryLock_1X000))
            {
                vbSystemList = db.VBSystem.Where(c => c.SystemName == License.SN_PackLicense).ToList();
            }

            foreach (VBSystem vbSys in vbSystemList)
            {
                VerifyAndReadSystemPackages(vbSys, db);
            }
            ReportLicenseTypeChanged();
        }

        /// <summary>
        /// Verifies the license for the passed VBSystem (= customer specific products)
        /// </summary>
        /// <param name="vbSys"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private bool VerifyAndReadSystemPackages(VBSystem vbSys, Database db)
        {
            if (!VerifyContent(vbSys))
                return false;

            if (!CheckDBIntegrity(vbSys, db))
                return false;

            List<ACPackage> _availablePackages;
            if (!ReadSystemPackages(vbSys, db, out _availablePackages))
                return false;
            lock (_LPLock)
            {
                if (_LicensedPackages != null)
                    _LicensedPackages.AddRange(_availablePackages);
            }
            OnPropertyChanged("LicensedPackages");
            return true;
        }

        private bool VerifyContent(VBSystem vbSys)
        {
            bool result = CSP.Verify(vbSys.GetChecksum(), vbSys.SystemCommon, vbSys.SystemCommonPublic);
            if (!result)
                return false;
            if (vbSys.SystemCommon1 != null)
                return CSP.Verify(vbSys.GetChecksumSigned(), vbSys.SystemCommon1, _SystemComm);
            return result;
        }

        /// <summary>
        /// Reads the packages for the passed VBSYstem
        /// </summary>
        /// <param name="vbSys"></param>
        /// <param name="db"></param>
        /// <param name="packages"></param>
        /// <returns></returns>
        private bool ReadSystemPackages(VBSystem vbSys, Database db, out List<ACPackage> packages)
        {
            packages = null;

            if (!CSP.Verify(vbSys.SystemInternal, vbSys.SystemInternal1, vbSys.SystemInternal2))
                return false;

            packages = new List<ACPackage>();

            string[] packs = Encoding.UTF8.GetString(vbSys.SystemInternal).Split(',');
            foreach (string pack in packs)
            {
                ACPackage acPack = null;

                using (ACMonitor.Lock(db.QueryLock_1X000))
                {
                    acPack = db.ACPackage.FirstOrDefault(c => c.ACPackageName.ToLower() == pack.ToLower().Trim());
                }
                if (acPack == null)
                    continue;
                packages.Add(acPack);
            }

            if (_IsRemoteLoginActive)
            {
                ACPackage acPack2 = null;

                using (ACMonitor.Lock(db.QueryLock_1X000))
                {
                    acPack2 = db.ACPackage.FirstOrDefault(c => c.ACPackageName == Const.PackName_VarioDevelopment);
                }
                if (acPack2 != null)
                    packages.Add(acPack2);
            }

            return true;
        }

        private bool CheckDBIntegrity(VBSystem vbSys, Database db)
        {
            if (CSP.Verify(Encoding.UTF8.GetBytes(GetDBCode(db)), vbSys.SystemInternal3, vbSys.SystemInternal2))
                return true;
            return false;
        }

        public string GetDBCode(Database db)
        {
            string dbName = ((DbConnection)db.Connection).Database;
            if (dbName == null)
                return "";
            string query = $"SELECT d.create_date, d.service_broker_guid, r.database_guid FROM sys.databases d inner join sys.database_recovery_status r on d.database_id = r.database_id WHERE d.name = '-dbName-'";
            query = query.Replace("-dbName-", dbName);
            DBInfo dbInfo = null;
            using (ACMonitor.Lock(db.QueryLock_1X000))
            {
                //var result = db.ExecuteStoreQuery<DBInfo>(query);
                //var result = db.Database.SqlQuery<DBInfo>(query);
                //dbInfo = result.FirstOrDefault();

                DateTime time = DateTime.Now;
                Guid dbGuid = Guid.Empty;
                Guid serviceBrokerGuid = Guid.Empty;
                int counter = 0;
                using (var command = db.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    db.Database.OpenConnection();
                    using (var dbResult = command.ExecuteReader())
                    {
                        foreach (var dbValues in dbResult)
                        {
                            IEnumerable values = dbValues.GetType().GetField("_values", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dbValues) as IEnumerable;
                            foreach (var value in values)
                            {
                                counter++;
                                if (value.ToString().Contains(":"))
                                    time = (DateTime)value;

                                if (counter == 2)
                                    dbGuid = (Guid)value;

                                if (counter == 3)
                                    serviceBrokerGuid = (Guid)value;
                            }

                            if (!dbGuid.Equals(serviceBrokerGuid) && time != DateTime.Now)
                                dbInfo = new DBInfo(time, serviceBrokerGuid, dbGuid);
                        }
                    }
                }
            }
            if (dbInfo == null)
                return "";
            return dbInfo.GetCheckSumCode();
        }
#endregion


#region License Activation & Generation

        /// <summary>
        /// Indentifies a iPlus-Installation on a target machine
        /// It depends on the unique ID of the MSSQL-Server instance
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public bool GetUniqueUserInfo(out Tuple<string, string, string> userInfo)
        {
            userInfo = null;

            if (_Database == null)
                return false;

            DbConnection ec = _Database.Connection as DbConnection;
            if (ec == null || ec == null || string.IsNullOrEmpty(ec.Database) || string.IsNullOrEmpty(ec.DataSource))
                return false;

            userInfo = new Tuple<string, string, string>(ec.Database, ec.DataSource, GetDBCode(_Database));
            return true;
        }

        /// <summary>
        /// CONSUMER-Method
        /// Activates a License on the target system
        /// The license will be saved in the database
        /// </summary>
        /// <param name="license"></param>
        /// <param name="customerName"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool ActivateLicense(string license, string customerName, Database db)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(VBSystem));

            VBSystem sysNew = null;
            bool isValid = false;
            try
            {
                sysNew = serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(license))) as VBSystem;
                isValid = CheckActivateLicense(sysNew, customerName, db);

                if (!isValid)
                    return false;
                NeedRestart = true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("License", "ActivateLicense", msg);

                return false;
            }

            VBSystem oldSys = db.VBSystem.FirstOrDefault(c => c.SystemName == License.SN_PackIssuer);
            if (oldSys != null && sysNew.SystemName == License.SN_PackIssuer)
            {
                db.Remove(oldSys);
                db.ACSaveChanges();
            }
            db.VBSystem.Add(sysNew);
            db.ACSaveChanges();

            return true;
        }

        private bool CheckActivateLicense(VBSystem sys, string customerName, Database db)
        {
            if (sys.SystemName == License.SN_PackIssuer)
            {
                // V4 developer licenses were bound to a TDi Matrix dongle. The dongle
                // support has been removed in V5, so such licenses can no longer be
                // validated. Developers use a signed DeveloperIssuer *.gip file instead
                // (see ActivateLicenseFile).
                return false;
            }
            else if (sys.SystemName == License.SN_PackLicense)
            {
                return VerifyAndReadSystemPackages(sys, db);
            }
            return false;
        }


        /// <summary>
        /// PROVIDER-Method
        /// Generates a license-file.
        /// It's only possible for Developer with License-Issuing rights
        /// </summary>
        /// <param name="licenseDir"></param>
        /// <param name="uniqueUserCode"></param>
        /// <param name="license"></param>
        /// <returns></returns>
        public bool GenerateLicenseFile(string licenseDir, string uniqueUserCode, ref VBLicense license)
        {
//#if (!DEBUG)
//            if (!IsDeveloperWithIssuingRights)
//                return false;
//#endif
            VBSystem sysD = null;

            using (ACMonitor.Lock(_Database.QueryLock_1X000))
            {
                sysD = _Database.VBSystem.FirstOrDefault(c => c.SystemName == License.SN_PackIssuer);
            }
            if (sysD == null)
                return false;

            VBSystem sysLicense = new VBSystem();
            sysLicense.VBSystemID = Guid.NewGuid();
            sysLicense.SystemInternal = license.PackageSystem;
            sysLicense.Company = sysD.CustomerName;
            sysLicense.CustomerName = license.CustomerName;
            sysLicense.SystemName = License.SN_PackLicense;
            sysLicense.ProjectNo = license.ProjectNo;

            SignInfo info = CSP.SignData(license.PackageSystem);
            sysLicense.SystemInternal1 = info.Signature;
            sysLicense.SystemInternal2 = info.PublicKey;
            license.PackageSystem1 = info.PrivateKey;
            license.SystemDB = uniqueUserCode;
            license.SystemDS = "";

            SignInfo infoDB = CSP.SignData(Encoding.UTF8.GetBytes(uniqueUserCode), info.PrivateKey);
            sysLicense.SystemInternal3 = infoDB.Signature;

            SignInfo infoRM = CSP.SignDataDSA(new byte[1]);
            sysLicense.SystemRemote = infoRM.PublicKey;
            license.SystemRemote = infoRM.PrivateKey;

            SignInfo infoAll = CSP.SignData(sysLicense.GetChecksum(), sysD.SystemPrivate);
            sysLicense.SystemCommon = infoAll.Signature;
            sysLicense.SystemCommonPublic = infoAll.PublicKey;
            license.SystemCommon = infoAll.Signature;

            DataContractSerializer serializer = new DataContractSerializer(typeof(VBSystem));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, sysLicense);
                string xmlLicense = Encoding.UTF8.GetString(ms.ToArray());
                File.WriteAllText(string.Format(@"{0}\{1}-license.gip", licenseDir, license.CustomerName), xmlLicense);
            }

            return true;
        }

        public bool IsEnabledGenerateLicenseFile()
        {
            return IsDeveloperWithIssuingRights;
        }


        public IEnumerable<string> GetAvailablePackages()
        {
            if (!IsDeveloperWithIssuingRights)
                return null;

            if (string.IsNullOrEmpty(LicensedTo))
                return null;

            using (ACMonitor.Lock(_Database.QueryLock_1X000))
            {
                return _Database.ACPackage.Where(c => c.ACPackageName != Const.PackName_VarioDevelopment).ToArray()
                               .Where(x => x.ACPackageName.Split('.').FirstOrDefault() == LicensedTo).Select(x => x.ACPackageName);
            }
        }

#endregion


#region RemoteLogin

        private static bool _IsRemoteLoginActive = false;

        /// <summary>
        /// CONSUMER-Method
        /// Creates a request code for login
        /// </summary>
        /// <returns></returns>
        public static string GenerateRemoteLoginCode()
        {
            Random rnd = new Random();
            byte[] rndKey = new byte[9];
            rnd.NextBytes(rndKey);
            //_RandomKey = rndKey.ToByteStringKey();
            return rndKey.ToByteStringKey();
        }

        /// <summary>
        /// CONSUMER-Method
        /// Checks if the provided login/activation key is valid.
        /// The user to gets temporary development rights during one iPlus-Session
        /// </summary>
        /// <param name="remoteKey"></param>
        /// <param name="randomKey"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool VerifyRemoteLogin(string remoteKey, string randomKey, Database db)
        {
            foreach (var sys in db.VBSystem)
            {
                if (CSP.VerifyDSA(Encoding.UTF8.GetBytes(randomKey), ByteExtension.FromByteStringKey(remoteKey), sys.SystemRemote))
                {
                    _IsRemoteLoginActive = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// PROVIDER-Method
        /// Creates a temporary activation key to enable the user to have development rights during one iPlus-Session
        /// Requires a DeveloperIssuer license file (see <see cref="IsDeveloperWithIssuingRights"/>).
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="vbSys"></param>
        /// <returns></returns>
        public string GenerateRemoteLoginKey(string userCode, VBLicense vbSys)
        {
            if (!IsEnabledGenerateRemoteUserKey())
                return null;
            string remoteLoginKey = "";
            using (var dsa = new DSACryptoServiceProvider())
            {
                try
                {
                    dsa.FromXmlString(vbSys.SystemRemote);
                    var signature = dsa.SignData(Encoding.UTF8.GetBytes(userCode));
                    remoteLoginKey = signature.ToByteStringKey();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("License", "GenerateRemoteLoginKey", msg);

                    return remoteLoginKey;
                }
            }
            return remoteLoginKey;
        }

        public bool IsEnabledGenerateRemoteUserKey()
        {
            bool isEnabled = false;
//#if DEBUG
            isEnabled = true;
//#else
//            isEnabled = CurrentLicenseType == LicenseType.Developer_EndUser || CurrentLicenseType == LicenseType.Developer_Issuer;
//#endif
            return isEnabled;
        }

#endregion


#endregion


#region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ReportLicenseTypeChanged()
        {
            OnPropertyChanged("CurrentLicenseType");
            OnPropertyChanged("IsTrial");
            OnPropertyChanged("IsOpenEdition");
            OnPropertyChanged("IsDeveloper");
            OnPropertyChanged("IsRemoteDeveloper");
            OnPropertyChanged("MayUserDevelop");
            OnPropertyChanged("IsLicenseValid");
            OnPropertyChanged("LicensedTo");
            OnPropertyChanged("LicensedToTitle");
        }

#endregion

    }

    public enum LicenseType : byte
    {
        /// <summary>
        /// Trial
        /// </summary>
        Trial = 0,

        /// <summary>
        /// Its a End-User with development rights (Access to the iPlus Development Environment)
        /// </summary>
        Developer_EndUser = 10,

        /// <summary>
        /// Its a developer (customer) who is able to create his own product licenses
        /// </summary>
        Developer_Issuer = 20,

        /// <summary>
        /// End-User who has entered a temporary key in the login-window to have access to the development Environment
        /// </summary>
        RemoteDeveloper = 30,

        /// <summary>
        /// Normal end-user with a regular runtime license
        /// </summary>
        User = 40
    }

    public enum ComponentLicense
    {
        TimeCheck = 0,
        True = 10,
        False = 20
    }
}

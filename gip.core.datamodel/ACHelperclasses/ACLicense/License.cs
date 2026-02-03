// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MXAPI;
using System.Security.Cryptography;
using System.IO;
using System.Timers;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Data;
using System.CodeDom;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Collections;
using System.Reflection;

namespace gip.core.datamodel.Licensing
{
    /// <summary>
    /// --- Vorgehensweise bei Installation beim Kunden: ---
    /// Matrix32.dll, Matrix64.dll ins Installationsverzeichnis kopieren (auch wenn kein Dongle verwendet wird)
    /// --- Falls Dongle vorort verwendet wird: ---
    /// matrix.ini konfigurieren, falls Dongle eingesteckt werden soll vorort
    /// USB-Treiber für Dongle installieren (inf_inst.exe)
    /// </summary>

    public sealed class License : INotifyPropertyChanged
    {
        #region c'tors
        public License(VBUser vbUser, Database database)
        {
            //if (_SingleInstance)
            //    throw new SystemException("Only one License-Instance can be created!");
            _SingleInstance = true;
            _VBUser = vbUser;
            _Database = database;
            Initialize(_Database);
        }


        private void Initialize(Database db)
        {
            NeedRestart = false;
//#if DEBUG
            return;
//#else
//            _StartDT = DateTime.Now;
//            VerifyAndReadAllPackages(db);
//            ReadLicenseTypeFromDongle();
//#endif
        }
        #endregion


        #region Private Fields
        private static bool _SingleInstance = false;

        private string _SystemComm = "";

        private VBUser _VBUser;


//#if (!DEBUG)
//        private TimeSpan _TrialTime = new TimeSpan(0, 45, 0);

//        private TimeSpan _PackageRemainingTime = new TimeSpan(48, 0, 0);

//        private TimeSpan _CyclicDongleReadInterval = new TimeSpan(0, 0, 30);

//        private DateTime? _NextCyclicDongleRead = null;

//        private DateTime _StartDT;
//#endif

        private const int _UserCode = 4228216;
        private const short _Port = 85;
        private int[] _DongleData;
        private bool? _DongleConnected;
        private object _DongleReadLock = new object();
        /// <summary>
        /// Determines if the connected Dongle 
        /// is a Developer-Dongle who can issue product licenses (_DongleData is not empty) 
        /// or is it a EndUser-Dongle that enables the user to use the Development-Environment
        /// </summary>
        private LicenseType _DongleType = LicenseType.Trial;



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
                if (!SystemID.HasValue)
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
                //if (_DongleType == LicenseType.Developer_Issuer || _DongleType == LicenseType.Developer_EndUser)
                //    return _DongleType;
                //else if (_IsRemoteLoginActive)
                //    return LicenseType.RemoteDeveloper;
                //lock (_LPLock)
                //{
                //    if (_LicensedPackages != null && _LicensedPackages.Any())
                //        return LicenseType.User;
                //}
                //return LicenseType.Trial;
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
        /// Reads the Dongle an determines if Developer has issuing rights
        /// </summary>
        public bool IsDeveloperWithIssuingRights
        {
            get
            {
                ReadLicenseTypeFromDongle();
                return CurrentLicenseType == LicenseType.Developer_Issuer;
            }
        }

        public bool IsTrialTimeExpired
        {
            get
            {
//#if DEBUG
                return false;
//#else
//                return _TrialTime < DateTime.Now - _StartDT;
//#endif
            }
        }

        public bool IsPackageTimeExpired
        {
            get
            {
//#if DEBUG
                return false;
//#else
//                return _PackageRemainingTime < DateTime.Now - _StartDT;
//#endif
            }
        }

        /// <summary>
        /// Attention: This Property reads the dongle cyclic!
        /// </summary>
        public bool MayUserDevelop
        {
            get
            {
//#if DEBUG
                return true;
//#else
//                ReadLicenseTypeFromDongleCyclic();
//                return CurrentLicenseType != LicenseType.User;
//#endif
            }
        }

        public bool IsLicenseValid
        {
            get
            {
                bool hasVBSystemEntries = false;

                using (ACMonitor.Lock(_Database.QueryLock_1X000))
                {
                    hasVBSystemEntries = _Database.VBSystem.Any();
                }
                return CurrentLicenseType > LicenseType.Trial && hasVBSystemEntries;
            }
        }

        public string LicensedTo
        {
            get
            {
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
        public bool IsPackageLicensed(ACPackage acPackage)
        {
//#if DEBUG
            return true;
//#else
//            if (acPackage.ACPackageName == Const.PackName_VarioLicense || acPackage.ACPackageName == Const.PackName_System || acPackage.ACPackageName == Const.PackName_VarioSystem)
//                return true;

//            lock (_LPLock)
//            {
//                if (_LicensedPackages == null || acPackage.ACPackageName == Const.PackName_VarioDevelopment)
//                    return false;

//                if (_LicensedPackages.Any(c => c.ACPackageName == acPackage.ACPackageName))
//                    return true;
//            }
//            return false;
//#endif
        }

        public ComponentLicense IsComponentLicensed(Guid packageID)
        {
//#if DEBUG
            return ComponentLicense.True;
//#else
//            if ((VarioSystemID.HasValue && VarioSystemID == packageID) || (_SystemID.HasValue && _SystemID == packageID))
//                return ComponentLicense.True;

//            lock (_LPLock)
//            {
//                if (_LicensedPackages != null && _LicensedPackages.Any(c => c.ACPackageID == packageID))
//                    return ComponentLicense.True;
//            }

//            if (!IsPackageTimeExpired)
//                return ComponentLicense.TimeCheck;

//            return ComponentLicense.False;
//#endif
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

            if (sysNew.SystemName == License.SN_PackIssuer)
            {
                ReadLicenseTypeFromDongle();
                //_CurrentLicenseType = LicenseType.Developer_Issuer;
            }

            return true;
        }

        private bool CheckActivateLicense(VBSystem sys, string customerName, Database db)
        {
            if (sys.SystemName == License.SN_PackIssuer)
            {
                if (db.VBSystem.Any() && db.VBSystem.FirstOrDefault().CustomerName == sys.CustomerName)
                    return DoesDongleContainIssuingLicense(sys);

                else if (sys.CustomerName == customerName)
                    return DoesDongleContainIssuingLicense(sys);

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
//#if DEBUG
            return true;
//#else
//            return CurrentLicenseType == LicenseType.Developer_Issuer;
//#endif
        }


        public IEnumerable<string> GetAvailablePackages()
        {
            ReadLicenseTypeFromDongle();
            if (_DongleType != LicenseType.Developer_Issuer)
                return null;

            if (CurrentLicenseType != LicenseType.Developer_Issuer || string.IsNullOrEmpty(LicensedTo))
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
        /// This method could only be used if a Dongle is connected
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


#region Dongle-Operations

        /// <summary>
        /// Method reads the dongle one time and writes the content to the _DongleData (Cache)
        /// </summary>
        /// <param name="forceRead">Resets the _DongleData-Field (cache) and reads again.</param>
        /// <returns>true if dongle connected via usb</returns>
        private bool HasConnectedDongleAndRead(bool forceRead = false)
        {
            lock (_DongleReadLock)
            {
                if (forceRead)
                    ResetDongleData();
                if (_DongleConnected.HasValue)
                    return _DongleConnected.Value;
                ReadDongleData();
                if (!_DongleConnected.HasValue)
                    return false;
                return _DongleConnected.Value;
            }
        }

        /// <summary>
        /// Evaluates the _DongleData. If cached _DongleData is not set, then it reads the dongle one time
        /// If the dongle contains encrypted data it veryfies if contains a valid license for a developer
        /// (Customers can create his own licenses for their own packages)
        /// </summary>
        /// <param name="sys"></param>
        /// <param name="forceRead">Resets the _DongleData-Field (cache) and reads again. Otherwise it uses the cached _DongleData-Field</param>
        /// <returns>true, if the dongle contains a valid developer license</returns>
        private bool DoesDongleContainIssuingLicense(VBSystem sys = null, bool forceRead = false)
        {
            int[] dongleData = _DongleData;
            lock (_DongleReadLock)
            {
                if (forceRead)
                    ResetDongleData();
                if (_DongleConnected.HasValue && _DongleData != null)
                    dongleData = _DongleData;
                else
                    ReadDongleData();
                if (!_DongleConnected.HasValue || !_DongleConnected.Value)
                    return false;
            }

            if (!EvaluateIssuingLicenseFromDongleData(dongleData, sys))
                return false;

            return true;
        }

        /// <summary>
        /// Reads the dongle, Reevaluates the Dongle-Data and updates the _DongleType-Field.
        /// </summary>
        private void ReadLicenseTypeFromDongle()
        {
            ResetDongleData();
            LicenseType dongleType = _DongleType;
            _DongleType = LicenseType.Trial;
            if (HasConnectedDongleAndRead())
            {
                _DongleType = LicenseType.Developer_EndUser;
                if (DoesDongleContainIssuingLicense())
                    _DongleType = LicenseType.Developer_Issuer;
            }
            if (dongleType != _DongleType)
                ReportLicenseTypeChanged();
        }

        private void ReadLicenseTypeFromDongleCyclic()
        {
//#if DEBUG
            return;
//#else
//            if (!_NextCyclicDongleRead.HasValue || DateTime.Now > _NextCyclicDongleRead.Value)
//            {
//                ReadLicenseTypeFromDongle();
//                _NextCyclicDongleRead = DateTime.Now + _CyclicDongleReadInterval;
//            }
//#endif
        }

        private void ReadDongleData()
        {
            lock (_DongleReadLock)
            {
                if (_DongleConnected.HasValue)
                    return;
                _DongleData = null;
                if (Environment.Is64BitProcess)
                    _DongleConnected = ReadDongleData64(out _DongleData);
                else
                    _DongleConnected = ReadDongleData32(out _DongleData);
            }
        }

        private void ResetDongleData()
        {
            lock (_DongleReadLock)
            {
                _DongleData = null;
                _DongleConnected = null;
            }
        }

        private bool ReadDongleData32(out int[] dataOut)
        {
            dataOut = new int[15];
            if (Matrix.Dongle_Count(_Port) != 1)
                return false;
            short dng_Nummer = Matrix.Dongle_Count(_Port);
            if (Matrix.Dongle_ReadData(_UserCode, ref dataOut[0], 15, dng_Nummer, _Port) < 0)
                return false;
            return true;
        }

        private bool ReadDongleData64(out int[] dataOut)
        {
            dataOut = new int[15];
            if (Matrix.Dongle_Count64(_Port) != 1)
                return false;
            short dng_Nummer = Matrix.Dongle_Count64(_Port);
            if (Matrix.Dongle_ReadData64(_UserCode, ref dataOut[0], 15, dng_Nummer, _Port) < 0)
                return false;
            return true;
        }
#endregion


#region Encryption and decoding Dongle data
        private bool EvaluateIssuingLicenseFromDongleData(int[] dongleData, VBSystem sys = null)
        {
            if (sys == null)
            {
                using (ACMonitor.Lock(_Database.QueryLock_1X000))
                {
                    sys = _Database.VBSystem.FirstOrDefault(c => c.SystemName == License.SN_PackIssuer);
                }
            }

            if (sys == null)
                return false;

            if (!CSP.Verify(sys.GetChecksum(), sys.SystemCommon, _SystemComm))
                return false;

            byte[] code;
            using (SHA256 sha = SHA256.Create())
            {
                code = sha.ComputeHash(Encoding.UTF8.GetBytes(UnpackLicense(dongleData).ToByteStringKey()));
            }

            if (sys.CustomerName != DecryptString(Encoding.UTF8.GetString(sys.SystemInternal), code))
                return false;
            return true;
        }

        private byte[] UnpackLicense(int[] license)
        {
            byte[] result = new byte[60];
            for (int i = 0; i < 15; i++)
            {
                Array.Copy(BitConverter.GetBytes(license[i]), 0, result, i * 4, 4);
            }
            return result;
        }

        private static string DecryptString(string cipherText, byte[] pass)
        {
            Aes aesCipher = Aes.Create();
            aesCipher.Padding = PaddingMode.PKCS7;
            aesCipher.Key = pass;
            aesCipher.IV = pass.Take(aesCipher.BlockSize / 8).ToArray();

            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform aesDecryptor = aesCipher.CreateDecryptor();
            string plainText = String.Empty;
            using (VBCryptoStream cryptoStream = new VBCryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write))
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                try
                {
                    cryptoStream.FlushFinalBlock();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("License", "DecryptString", msg);
                }
                byte[] plainBytes = memoryStream.ToArray();
                plainText = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);
                memoryStream.Close();
            }
            return plainText;
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

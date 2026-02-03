// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.datamodel.Licensing;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Data.Common;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Manager für Umgebungseigenschaften, Icons, Bitmaps, Übersetzung und globale C#-Scripte
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Environment-Manager'}de{'Umgebungs-Manager'}", Global.ACKinds.TACEnvironment, Global.ACStorableTypes.Required, false, false)]
    public class Environment : ACComponent, IEnvironment
    {

        #region c´tors
        public Environment(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _Rootpath = AppDomain.CurrentDomain.BaseDirectory;
            Translator.DefaultVBLanguageCode = VBLanguage.DefaultVBLanguage(Database.ContextIPlus).VBLanguageCode;
            _AccessDefaultTakeCount = new ACPropertyConfigValue<int>(this, "AccessDefaultTakeCount", ACQueryDefinition.C_DefaultTakeCount);
            if (_AccessDefaultTakeCount.ValueT == 0)
                _AccessDefaultTakeCount.ValueT = ACQueryDefinition.C_DefaultTakeCount;
            _UseDynLINQ = new ACPropertyConfigValue<bool>(this, "UseDynLINQ", false);
            _Datapath = new ACPropertyConfigValue<string>(this, "Datapath", "");
            _MaxDBConnectionCount = new ACPropertyConfigValue<int>(this, "MaxDBConnectionCount", 0);
            _MaxWCFConnectionCount = new ACPropertyConfigValue<int>(this, "MaxWCFConnectionCount", 0);
            _MaxWinSessionCount = new ACPropertyConfigValue<int>(this, "MaxWinSessionCount", 0);
            _MaxLicensedWinSessions = new ACPropertyConfigValue<int>(this, "MaxLicensedWinSessions", 0);
            _MaxLicensedWCFConnections = new ACPropertyConfigValue<int>(this, "MaxLicensedWCFConnections", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            // Reset Caache because RegisterAndUpdateACObjects() maybe will _MaxDBConnectionCount on another context and then there are duplicate entries.
            this.ACTypeFromLiveContext.ClearCacheOfConfigurationEntries();
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion


        #region Icon/Bitmap
        [ACMethodInfo("Icon", "en{'Get Icon'}de{'Icon holen'}", 9999)]
        public ACClassDesign GetIcon(string acNameIdentifier)
        {
            ACClass acClass = ACType as ACClass;
            if (acClass == null)
                return null;


            using (ACMonitor.Lock(acClass.Database.QueryLock_1X000)) // ACType ist immer vom globalen Datenbankkontext
            {
                return acClass.ACClassDesign_ACClass.Where(c => c.ACIdentifier == "Icon" + acNameIdentifier && c.ACUsageIndex == (Int16)Global.ACUsages.DUIcon).FirstOrDefault();
            }
        }

        [ACMethodInfo("Bitmap", "en{'Get Bitmap'}de{'Bitmap holen'}", 9999)]
        public ACClassDesign GetBitmap(string acNameIdentifier)
        {
            ACClass acClass = ACType as ACClass;
            if (acClass == null)
                return null;


            using (ACMonitor.Lock(acClass.Database.QueryLock_1X000)) // ACType ist immer vom globalen Datenbankkontext
            {
                return acClass.ACClassDesign_ACClass.Where(c => c.ACIdentifier == "Bitmap" + acNameIdentifier && c.ACUsageIndex == (Int16)Global.ACUsages.DUBitmap).FirstOrDefault();
            }
        }
        #endregion


        #region Translation
        [ACMethodInfo("Translation", "en{'Translation'}de{'Übersetzung'}", 9999)]
        public string TranslateText(IACObject acObject, string acIdentifier)
        {
            return TranslateTextLC(acObject, acIdentifier, Root.Environment.VBLanguageCode);
        }

        [ACMethodInfo("Translation", "en{'Translation'}de{'Übersetzung'}", 9999)]
        public string TranslateText(IACObject acObject, string acIdentifier, params object[] parameter)
        {
            return string.Format(TranslateTextLC(acObject, acIdentifier, Root.Environment.VBLanguageCode), parameter);
        }

        [ACMethodInfo("Translation", "en{'Translation in Language'}de{'Übersetzung in Sprache'}", 9999)]
        public string TranslateTextLC(IACObject acObject, string acIdentifier, string VBLanguageCode)
        {
            if (string.IsNullOrEmpty(acIdentifier))
                return "";
            ACClass acClass = null;
            if (acObject is ACClass)
                acClass = acObject as ACClass;
            else
                acClass = acObject.ACType as ACClass;
            return TranslateTextACClass(acClass, acIdentifier, VBLanguageCode);
        }

        public string TranslateTextACClass(ACClass acClass, string acIdentifier, string VBLanguageCode)
        {
            if (acClass == null)
                return "";
            ACClassText acClassText = acClass.GetText(acIdentifier);
            if (acClassText == null)
            {
                var context = acClass.Database;
                if (context != null)
                {

                    using (ACMonitor.Lock(context.QueryLock_1X000))
                    {
                        acClassText = ACClassText.NewACObject(context, acClass);
                        acClassText.ACIdentifier = acIdentifier;
                        acClassText.ACCaption = "en:" + acIdentifier;
                        acClassText.ACCaption = "de:" + acIdentifier;
                        context.ACClassText.Add(acClassText);
                        context.ACSaveChanges();
                    }
                }
                return acIdentifier;
            }
            return acClassText.GetTranslation(VBLanguageCode);
        }


        [ACMethodInfo("Translation", "en{'Translation Message'}de{'Übersetung Meldung'}", 9999)]
        public string TranslateMessage(IACObject acObject, string acIdentifier, params object[] parameter)
        {
            return TranslateMessageLC(acObject, acIdentifier, Root.Environment.VBLanguageCode, parameter);
        }

        [ACMethodInfo("Translation", "en{'Translation Message in Language'}de{'Übersetung Meldung in Sprache'}", 9999)]
        public string TranslateMessageLC(IACObject acObject, string acIdentifier, string VBLanguageCode, params object[] parameter)
        {
            if (acObject == null || acObject.ACType == null)
                return "";
            ACClass typeAsACClass = acObject.ACType as ACClass;
            if (typeAsACClass == null)
                return "";
            ACClassMessage acClassMessage = typeAsACClass.GetMessage(acIdentifier);
            if (acClassMessage == null)
            {
                return string.Format("*No Translation* {0}", acIdentifier);
            }
            var result2 = acClassMessage.GetTranslation(VBLanguageCode);
            string result = result2;
            if (parameter != null && parameter.Any())
            {
                try
                {
                    result = string.Format(result2, parameter);
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "Environment.TranslateMessageLC()", "TranslId: " + acIdentifier + ", Text: " + result2 + ", " + e.Message);
                }
            }
            return result;
        }
        #endregion


        #region Environment

        public static License _Licence = null;
        public License License
        {
            get
            {
                return Environment._Licence;
            }
        }


        private string _Rootpath;
        /// <summary>
        /// Location of the current assembly (AppDomain.CurrentDomain.BaseDirectory)
        /// </summary>
        /// <value>The rootpath.</value>
        [ACPropertyInfo(9999, "Environment")]
        public string Rootpath
        {
            get
            {
                return _Rootpath;
            }
        }


        private ACPropertyConfigValue<string> _Datapath;
        /// <summary>
        /// Defaultpath for exporting/importing of configuration data
        /// </summary>
        /// <value>The datapath.</value>
        [ACPropertyConfig("Datapath")]
        public string Datapath
        {
            get
            {
                if (string.IsNullOrEmpty(_Datapath.ValueT))
                    _Datapath.ValueT = _Rootpath.Substring(0, 3) + "VarioData";
                return _Datapath.ValueT;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
                {
                    _Datapath.ValueT = value;
                }
            }
        }


        /// <summary>
        /// Gets the name of the computer.
        /// </summary>
        /// <value>The name of the computer.</value>
        public string ComputerName
        {
            get
            {
                return System.Net.Dns.GetHostName();
            }
        }


        private int? _SessionID;
        /// <summary>
        /// Windows Session-ID
        /// </summary>
        public int? SessionID
        {
            get
            {
                if (!_SessionID.HasValue)
                {
                    Process thisProcess = Process.GetCurrentProcess();
                    if (thisProcess != null)
                        _SessionID = thisProcess.SessionId;
                }
                return _SessionID;
            }
        }

        #endregion


        #region User Login/Logout and Rights

        private VBUser _VBUser;
        /// <summary>
        /// Current logged in user
        /// </summary>
        [ACPropertyInfo(9999, "Environment")]
        public VBUser User
        {
            get
            {
                return _VBUser;
            }
        }

        internal void LoginUser(VBUser value)
        {
            _VBUser = value;
            if (_VBUser != null)
            {
                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                {
                    _UserInstance = _VBUser.VBUserInstance_VBUser.FirstOrDefault();
                }
                if (_UserInstance != null)
                {
                    int? sessionid = SessionID;
                    if (sessionid.HasValue)
                    {
                        _UserInstance.LogIn(ComputerName, sessionid.Value);
                        gip.core.datamodel.Database.GlobalDatabase.ACSaveChanges();
                    }
                }
            }
            ResetLanguage();        
        }

        internal void LogoutUser()
        {
            int? sessionid = SessionID;
            if (_UserInstance != null && sessionid.HasValue)
            {
                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                {
                    _UserInstance.AutoRefresh(gip.core.datamodel.Database.GlobalDatabase);
                }
                _UserInstance.ReloadSessions();
                _UserInstance.LogOut(ComputerName, sessionid.Value);
                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                {
                    gip.core.datamodel.Database.GlobalDatabase.ACSaveChanges();
                }
            }
        }


        string _VBLanguageCode;
        /// <summary>
        /// Current language I18N-Code
        /// </summary>
        /// <value>Current language I18N-Code</value>
        [ACPropertyInfo(9999, "Environment")]
        public string VBLanguageCode
        {
            get
            {
                return _VBLanguageCode;
            }
        }

        string _OverrideLanguage;
        public void SetLanguageCode(string code)
        {
            _OverrideLanguage = code;
            ResetLanguage();
        }

        private void ResetLanguage()
        {
            if (!String.IsNullOrEmpty(_OverrideLanguage))
                _VBLanguageCode = _OverrideLanguage;
            else if (_VBUser != null && _VBUser.VBLanguage != null)
                _VBLanguageCode = _VBUser.VBLanguage.VBLanguageCode;
            else
                _VBLanguageCode = "en";
            Translator.VBLanguageCode = _VBLanguageCode;
        }

        VBUserInstance _UserInstance = null;
        /// <summary>
        /// Current logged in user
        /// </summary>
        /// <value>The user instance.</value>
        [ACPropertyInfo(9999, "Environment")]
        public VBUserInstance UserInstance
        {
            get
            {
                return _UserInstance;
            }
        }


        /// <summary>
        /// Gets the manager for Rights for the current user
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns>ClassRightManager.</returns>
        public ClassRightManager GetClassRightManager(ACClass acClass)
        {
            return new ClassRightManager(acClass, User);
        }

        #endregion


        #region Database

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>The name of the database.</value>
        [ACPropertyInfo(9999, "Environment")]
        public string DatabaseName
        {
            get
            {
                //return (((System.Data.EntityClient.EntityConnection)(((System.Data.Objects.ObjectContext)(gip.core.datamodel.Database.GlobalDatabase)).Connection)).StoreConnection).Database;
                return (((DbConnection)(((DbContext)(gip.core.datamodel.Database.GlobalDatabase)).Database.GetDbConnection()))).Database;
            }
        }


        /// <summary>
        /// Gets the name of the data source (SQL-Server-Name)
        /// </summary>
        /// <value>The name of the data source.</value>
        [ACPropertyInfo(9999, "Environment")]
        public string DataSourceName
        {
            get
            {
                return gip.core.datamodel.Database.GlobalDatabase.Connection.DataSource;
            }
        }


        /// <summary>
        /// Informations from Connection-String
        /// </summary>
        public SqlConnectionStringBuilder SQLConnectionInfo
        {
            get
            {
                DbConnection entityConnection = gip.core.datamodel.Database.GlobalDatabase.Connection as DbConnection;
                if (entityConnection != null)
                {
                    return new SqlConnectionStringBuilder(entityConnection.ConnectionString);
                }
                return null;
            }
        }


        private ACPropertyConfigValue<Int32> _AccessDefaultTakeCount;
        /// <summary>
        /// Maximum of records that should be returned via ACAccess
        /// </summary>
        [ACPropertyConfig("en{'Default TakeCount for Access'}de{'Defaultlimit Datensätze'}", DefaultValue = (int)500)]
        public Int32 AccessDefaultTakeCount
        {
            get
            {
                return _AccessDefaultTakeCount.ValueT;
            }
            set
            {
                _AccessDefaultTakeCount.ValueT = value;
            }
        }

        private ACPropertyConfigValue<bool> _UseDynLINQ;
        /// <summary>
        /// Maximum of records that should be returned via ACAccess
        /// </summary>
        [ACPropertyConfig("en{'Use Dynamic LINQ for Queries'}de{'Verwende Dynamic LINQ für Abfragen'}", DefaultValue = (bool)false)]
        public bool UseDynLINQ
        {
            get
            {
                return _UseDynLINQ.ValueT;
            }
            set
            {
                _UseDynLINQ.ValueT = value;
            }
        }

        #endregion


        #region Connection Statistics
        internal void RecalcConnectionStatistics()
        {
            _MaxDBConnectionCount.IsCachedValueSet = false;
            int maxCount = MaxDBConnectionCount;
            int? currentCount = CurrentDBConnectionCount;
            if (currentCount.HasValue && currentCount.Value > maxCount)
                MaxDBConnectionCount = currentCount.Value;

            int totalSessionCount = CurrentWinSessionCount;
            _MaxWinSessionCount.IsCachedValueSet = false;
            if (totalSessionCount > MaxWinSessionCount)
                MaxWinSessionCount = totalSessionCount;
        }


        private ACPropertyConfigValue<int> _MaxDBConnectionCount;
        /// <summary>
        /// Largest number of concurrent database connections
        /// </summary>
        [ACPropertyConfig("en{'Largest number of concurrent database connections'}de{'Größte Anzahl gleichzeitiger Datenbankverbindungen'}")]
        public int MaxDBConnectionCount
        {
            get
            {
                return _MaxDBConnectionCount.ValueT;
            }
            internal set
            {
                _MaxDBConnectionCount.ValueT = value;
            }
        }


        private const string C_SQL_SysProcProgram = "SELECT COUNT(DISTINCT hostprocess) FROM sys.sysprocesses where program_name = {0} and DB_NAME(dbid) = {1}";
        private const string C_SQL_SysProc = "SELECT COUNT(DISTINCT hostprocess) FROM sys.sysprocesses where DB_NAME(dbid) = {0}";
        /// <summary>
        /// Current number of concurrent database connections
        /// This works only if loggen on sql-user has administrative sql-rights for querying sys.sysprocesses
        /// Otherwise it returns null or only one connection
        /// </summary>
        [ACPropertyInfo(500, "", "en{'Current number of database connections'}de{'Aktuelle Anzahl von Datenbankverbindungen'}")]
        public int? CurrentDBConnectionCount
        {
            get
            {
                try
                {
                    var sqlConnectionInfo = SQLConnectionInfo;
                    if (sqlConnectionInfo == null)
                        return null;
                    using (ACMonitor.Lock(Database.ContextIPlus.QueryLock_1X000))
                    {
                        FormattableString cntQueryStringSysProcProgram = FormattableStringFactory.Create(C_SQL_SysProcProgram, sqlConnectionInfo.ApplicationName, sqlConnectionInfo.InitialCatalog);
                        var cntQuery = Database.ContextIPlus.Database.SqlQuery<int>(cntQueryStringSysProcProgram);
                        int countAppConnections = cntQuery.AsEnumerable().FirstOrDefault();
                        FormattableString cntQueryStringSysProc = FormattableStringFactory.Create(C_SQL_SysProc, sqlConnectionInfo.InitialCatalog);
                        cntQuery = Database.ContextIPlus.Database.SqlQuery<int>(cntQueryStringSysProc);
                        int countDBConnections = cntQuery.AsEnumerable().FirstOrDefault();
                        if (countAppConnections != countDBConnections)
                            Messages.LogDebug(this.GetACUrl(), "GetDBConnectionCount(10)", String.Format("countAppConnections {0} and countDBConnections {1} are different! Maybe someone has opened a SQL-Connection from another program than iPlus or the Application-Name in the Connectionstrings are different in some clients!", countAppConnections, countDBConnections));
                        return countDBConnections;
                    }
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "GetDBConnectionCount(20)", e);
                }
                return null;
            }
        }


        internal void RefreshWCFConnectionStat(int currentCount)
        {
            //_MaxWCFConnectionCount.IsCachedValueSet = false;
            int maxCount = MaxWCFConnectionCount;
            if (currentCount > maxCount)
                MaxWCFConnectionCount = currentCount;
        }


        private ACPropertyConfigValue<int> _MaxWCFConnectionCount;
        /// <summary>
        /// Largest number of concurrent network connections
        /// </summary>
        [ACPropertyConfig("en{'Largest number of concurrent network connections'}de{'Größte Anzahl gleichzeitiger Netzwerkverbindungen'}")]
        public int MaxWCFConnectionCount
        {
            get
            {
                return _MaxWCFConnectionCount.ValueT;
            }
            internal set
            {
                _MaxWCFConnectionCount.ValueT = value;
            }
        }


        private ACPropertyConfigValue<int> _MaxWinSessionCount;
        /// <summary>
        /// Largest number of concurrent windows sessions
        /// </summary>
        [ACPropertyConfig("en{'Largest number of concurrent windows sessions'}de{'Größte Anzahl gleichzeitiger Windows-Sitzungen'}")]
        public int MaxWinSessionCount
        {
            get
            {
                return _MaxWinSessionCount.ValueT;
            }
            internal set
            {
                _MaxWinSessionCount.ValueT = value;
            }
        }


        /// <summary>
        /// Current number of concurrent windows sessions
        /// </summary>
        [ACPropertyInfo(502, "", "en{'Current number of concurrent windows sessions'}de{'Aktuelle Anzahl von Windows-Sitzungen'}")]
        public int CurrentWinSessionCount
        {
            get
            {
                using (Database db = new datamodel.Database())
                {
                    return db.VBUserInstance.Sum(c => c.SessionCount);
                }
            }
        }


        private ACPropertyConfigValue<int> _MaxLicensedWinSessions;
        /// <summary>
        /// Maximum licensed windows sessions
        /// </summary>
        [ACPropertyConfig("en{'Maximum number of licensed windows sessions'}de{'Maximale lizenzierte Windows-Sitzungen'}")]
        public int MaxLicensedWinSessions
        {
            get
            {
                return _MaxLicensedWinSessions.ValueT;
            }
            internal set
            {
                _MaxLicensedWinSessions.ValueT = value;
            }
        }


        /// <summary>
        /// Returns true if MaxLicensedWinSessions are exceeded
        /// </summary>
        [ACPropertyInfo(503, "", "en{'Maximum number of windows sessions is exceeded'}de{'Maximale Windows-Sitzungen sind überschritten'}")]
        public bool IsMaxWinSessionsExceeded
        {
            get
            {
                if (MaxLicensedWinSessions <= 0)
                    return false;
                return CurrentWinSessionCount > MaxLicensedWinSessions;
            }
        }

        private ACPropertyConfigValue<int> _MaxLicensedWCFConnections;
        /// <summary>
        /// Maximum licensed network connections
        /// </summary>
        [ACPropertyConfig("en{'Maximum number of licensed network connections'}de{'Maximale lizenzierte Netzwerk-Verbindungen'}")]
        public int MaxLicensedWCFConnections
        {
            get
            {
                return _MaxLicensedWCFConnections.ValueT;
            }
            internal set
            {
                _MaxLicensedWCFConnections.ValueT = value;
            }
        }


        /// <summary>
        /// Returns true if MaxLicensedWinSessions are exceeded
        /// </summary>
        internal bool IsMaxWCFConnectionsExceeded(int currentCount)
        {
                if (MaxLicensedWCFConnections <= 0)
                    return false;
                return currentCount > MaxLicensedWCFConnections;
        }

        #endregion

    }
}

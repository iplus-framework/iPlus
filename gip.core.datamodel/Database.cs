using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using System.Transactions;
using System.Data;
using System.Configuration;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace gip.core.datamodel
{
    public static class EntityObjectExtension
    {
        public static TEntityIPlus FromIPlusContext<TEntityIPlus>(this VBEntityObject entityApp, Database dbIPlus = null) where TEntityIPlus : VBEntityObject
        {
            if (entityApp == null)
                return default(TEntityIPlus);

            object obj = null;
            if (dbIPlus == null)
                dbIPlus = entityApp.GetObjectContext().ContextIPlus;

            Type typeOfTargetContext = dbIPlus.GetType();
            string fullName = typeOfTargetContext.Namespace + "." + entityApp.EntityKey.EntitySetName;
            Type typeOfTargetEntity = typeOfTargetContext.Assembly.GetType(fullName);
            if (typeOfTargetEntity == null)
                throw new ArgumentException(String.Format("Type {0} not found in assembly {1}", fullName, typeOfTargetContext.Assembly.ToString()));

            EntityKey key = new EntityKey(typeOfTargetEntity, entityApp.EntityKey.EntityKeyValues);
            using (ACMonitor.Lock(dbIPlus.QueryLock_1X000))
            {
                if (!dbIPlus.TryGetObjectByKey(key, out obj))
                    return default(TEntityIPlus);
            }
            return (TEntityIPlus)obj;
        }

        public static DbContextOptions<T> DbContextOptions<T>(string connectionString) where T : DbContext
        {
            var dbOptions = new DbContextOptionsBuilder<T>()
                    .UseSqlServer(connectionString).Options;
            return dbOptions;
        }

        public static DbContextOptions<T> DbContextOptions<T>(DbConnection connection) where T : DbContext
        {
            var dbOptions = new DbContextOptionsBuilder<T>()
                    .UseSqlServer(connection).Options;
            return dbOptions;
        }
    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'Database'}de{'Datenbank'}", Global.ACKinds.TACDBAManager, Global.ACStorableTypes.NotStorable, false, false)]
    public partial class Database : iPlusV4Context, IACEntityObjectContext
    {
        public const string ClassName = "Database";
        /// <summary> 
        /// Diese Managerklasse wird einmal Statisch zur Verfügung gestellt, 
        /// damit alle Instanzen von Database damit arbeiten können.
        /// </summary>
#region c'tors
        static Database()
        {
            // Keine Ständige Offene Verbindung bei Globaler Datenbank, wegen Transaktionen
            // Problem tritt auf, wenn z.B. Neue Enitäten in einer eigenen Transaktion angelegt werden
            // Um die Defaultwerte zu setzen erfolgen auch Abfragen auf dem Globalen DAtenbankkontext innerhalb dieser TRansaktion
            // Falls die Verbindung manuell geöffnet wurde, dann kommte eine Exception, dass die EntlistTransaction-Aktion nicht durchgeführt werden kann
            // Um dies zu verhindern gibt es zwei möglichkeiten
            // 1. Datenbankverbindung (Öffnen/Schliessen) wird automatisch durch den ObjectContext verwaltet
            // 2. Jede Abfrage auf dem Globalen Context muss mit einer inneren Transaktion 
            //    "using (TransactionScope innerScope = new TransactionScope(TransactionScopeOption.Suppress)) { }"
            // ausgeführt wird, die mit dem Schalter "TransactionScopeOption.Suppress" die umgebende Transaktion ausschliesst

            _GlobalDatabase = ACObjectContextManager.GetOrCreateContext<Database>(Const.GlobalDatabase);

            ACClass acClassQRYACQueryDefinition = _GlobalDatabase.ACClass.Where(c => c.ACIdentifier == Const.QueryPrefix + ACQueryDefinition.ClassName).First();

            _QRYACQueryDefinition = ACQueryDefinition.CreateRootQueryDefinition(acClassQRYACQueryDefinition);

            _GlobalDatabase.InitTypeDictFromSystemNamespace();
        }

        public Database()
            : base(EntityObjectExtension.DbContextOptions<iPlusV4Context>(ConnectionString))
        {
            _ObjectContextHelper = new ACObjectContextHelper(this);
        }

        void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            throw new NotImplementedException();
        }

        public Database(string connectionString)
            : base(EntityObjectExtension.DbContextOptions<iPlusV4Context>(connectionString))
        {
            _ObjectContextHelper = new ACObjectContextHelper(this);
        }

        public Database(bool createSeparateConnection)
            : this(new SqlConnection(ConnectionString))
        {
        }

        public Database(DbConnection connection)
            : base(EntityObjectExtension.DbContextOptions<iPlusV4Context>(connection))
        {
            _SeparateConnection = connection;
            _ObjectContextHelper = new ACObjectContextHelper(this);
        }

        public override void Dispose()
        {
            if (_ObjectContextHelper != null)
                _ObjectContextHelper.Dispose();
            _ObjectContextHelper = null;
            base.Dispose();
            if (SeparateConnection != null)
                SeparateConnection.Dispose();
            _SeparateConnection = null;
        }

#endregion

#region Properties

#region Private
        private ACObjectContextHelper _ObjectContextHelper;
        static string _Initials = null;
        static Database _GlobalDatabase = null;
        static internal ACQueryDefinition _QRYACQueryDefinition = null;
#endregion

#region Public Static
        [NotMapped]
        public static string ConnectionString
        {
            get
            {
                if (CommandLineHelper.ConfigCurrentDir != null && CommandLineHelper.ConfigCurrentDir.ConnectionStrings != null)
                {
                    try
                    {
                        ConnectionStringSettings setting = CommandLineHelper.ConfigCurrentDir.ConnectionStrings.ConnectionStrings["iPlusV4_Entities"];
                        return setting.ConnectionString;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null)
                            gip.core.datamodel.Database.Root.Messages.LogException("Database", "ConnectionString", msg);
                    }
                }

                return ConfigurationManager.ConnectionStrings["iPlusV4_Entities"].ConnectionString;
            }
        }

        /// <summary>
        /// Method for changing Connection-String to generate own connectionpool
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static string ModifiedConnectionString(string appName)
        {
            var connString = ConfigurationManager.ConnectionStrings["iPlusV4_Entities"].ConnectionString.Replace("iPlus_db", appName);
            return connString;
        }


        [ACPropertyInfo(9999)]
        [NotMapped]
        public Database ContextIPlus
        {
            get
            {
                return this;
            }
        }

        [NotMapped]
        public bool IsSeparateIPlusContext
        {
            get
            {
                return false;
            }
        }

        DbConnection _SeparateConnection;
        public DbConnection SeparateConnection
        {
            get
            {
                return _SeparateConnection;
            }
        }

        private string _UserName;
        [NotMapped]
        public string UserName
        {
            get
            {
                if (!String.IsNullOrEmpty(_UserName))
                    return _UserName;
                if (Database.Root == null 
                    || !gip.core.datamodel.Database.Root.Initialized
                    || gip.core.datamodel.Database.Root.Environment == null
                    || gip.core.datamodel.Database.Root.Environment.User == null)
                    return "Init";
                _UserName = gip.core.datamodel.Database.Root.Environment.User.Initials;
                return _UserName;
            }
            set
            {
                _UserName = value;
            }
        }

        /// <summary>
        /// 1. Globale Datenbank (Nur eine Instanz)
        /// ---------------------------------------
        /// static Database Database.GlobalDatabase
        /// 
        /// Diese dient dazu um Informationen von der Definitions/-Typenwelt zu erhalten 
        /// (Infos über ACLass. ACClassProperty...)
        /// Alle ACComponents, ACProperties, ACPoints.. werden über diese Typen instanziiert und alle Referenzen zu den IACTypes
        /// sind von diesem Datenbankkontext. Werden neue ACComponents instanziert, dann ist immer der PArameter ACClass acType im Konstruktor
        /// der Komponente von diesem Datenbankkontext. 
        /// 
        /// Verweise von anderen Instanzen zur GlobalDatabase:
        /// Die Property ACRoot.Database ist ein Verweis zu Database.GlobalDatabase und hat keinen eigenen Kontext
        /// </summary>
        [NotMapped]
        public static Database GlobalDatabase
        {
            get
            {
                return _GlobalDatabase;
            }
        }

        [NotMapped]
        public static ACQueryDefinition QRYACQueryDefinition
        {
            get
            {
                return _QRYACQueryDefinition;
            }
        }

        internal static void SetRoot(IRoot root)
        {
            if (_Root == null)
                _Root = root;
        }
        private static IRoot _Root;
        [NotMapped]
        public static IRoot Root
        {
            get
            {
                return _Root;
            }
        }

        [NotMapped]
        public static string Initials
        {
            get
            {
                if (_Initials == null)
                {
                    if (gip.core.datamodel.Database.Root == null || !gip.core.datamodel.Database.Root.Initialized)
                        return "Init";
                    _Initials = Root.Environment.User.Initials;
                }
                return _Initials;
            }
        }

        private static DerivationCache _derivationCache = new DerivationCache();

#endregion

#region Public
        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// </summary>
        /// <returns></returns>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public bool IsChanged
        {
            get
            {
                return _ObjectContextHelper.IsChanged;
            }
        }

        [NotMapped]
        public MergeOption RecommendedMergeOption
        {
            get
            {
                return MergeOption.AppendOnly;
                //return IsChanged ? MergeOption.AppendOnly : MergeOption.OverwriteChanges;
            }
        }

        public event ACChangesEventHandler ACChangesExecuted;

#endregion

#region IACUrl Member
        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [NotMapped]
        public IACObject ParentACObject
        {
            get
            {
                return Database.Root();
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public string ACCaption
        {
            get
            {
                return ACIdentifier;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public string ACIdentifier
        {
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        [NotMapped]
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return this.ReflectGetACContentList();
            }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [NotMapped]
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }
        #endregion

        #endregion

        #region Methods

        #region public

        public override EntityEntry Attach(object entity)
        {
            VBEntityObject vbobj = entity as VBEntityObject;
            vbobj.Context = this;
            return base.Attach(entity);
        }

        public override EntityEntry<TEntity> Attach<TEntity>(TEntity entity)
        {
            VBEntityObject vbobj = entity as VBEntityObject;
            vbobj.Context = this;
            return base.Attach(entity);
        }

        public override void AttachRange(IEnumerable<object> entities)
        {
            foreach (var entity in entities)
            {
                VBEntityObject vbobj = entity as VBEntityObject;
                vbobj.Context = this;
            }
            base.AttachRange(entities);
        }

        public override void AttachRange(params object[] entities)
        {
            foreach (var entity in entities)
            {
                VBEntityObject vbobj = entity as VBEntityObject;
                vbobj.Context = this;
            }
            base.AttachRange(entities);
        }

        public void Refresh(RefreshMode refreshMode, object entity)
        {
            _ObjectContextHelper.Refresh(refreshMode, entity);
        }

        /// <summary>
        /// UNSAFE. Use QueryLock_1X000 outside
        /// </summary>
        /// <returns></returns>
        [ACMethodInfo("", "", 9999)]
        public MsgWithDetails ACSaveChanges(bool autoSaveContextIPlus = true, bool acceptAllChangesOnSuccess = true, bool validationOff = false, bool writeUpdateInfo = true)
        {
            MsgWithDetails result = _ObjectContextHelper.ACSaveChanges(autoSaveContextIPlus, acceptAllChangesOnSuccess, validationOff, writeUpdateInfo);
            if (result == null)
            {
                if (ACChangesExecuted != null)
                    ACChangesExecuted.Invoke(this, new ACChangesEventArgs(ACChangesEventArgs.ACChangesType.ACSaveChanges, true));
            }
            else
            {
                if (ACChangesExecuted != null)
                    ACChangesExecuted.Invoke(this, new ACChangesEventArgs(ACChangesEventArgs.ACChangesType.ACSaveChanges, false));
            }
            return result;
        }

        /// <summary>
        /// Invokes ACSaveChanges. If a transaction error occurs ACSaveChanges is called again.
        /// If parameter retries ist not set, then ACObjectContextHelper.C_NumberOfRetriesOnTransError is used to limit the Retry-Loop.
        /// </summary>
        [ACMethodInfo("", "", 9999)]
        public MsgWithDetails ACSaveChangesWithRetry(ushort? retries = null, bool autoSaveContextIPlus = true, bool acceptAllChangesOnSuccess = true, bool validationOff = false, bool writeUpdateInfo = true)
        {
            MsgWithDetails result = _ObjectContextHelper.ACSaveChangesWithRetry(retries, autoSaveContextIPlus, acceptAllChangesOnSuccess, validationOff, writeUpdateInfo);
            if (result == null)
            {
                if (ACChangesExecuted != null)
                    ACChangesExecuted.Invoke(this, new ACChangesEventArgs(ACChangesEventArgs.ACChangesType.ACSaveChanges, true));
            }
            else
            {
                if (ACChangesExecuted != null)
                    ACChangesExecuted.Invoke(this, new ACChangesEventArgs(ACChangesEventArgs.ACChangesType.ACSaveChanges, false));
            }
            return result;
        }

        /// <summary>
        /// UNSAFE. Use QueryLock_1X000 outside
        /// </summary>
        /// <returns></returns>
        [ACMethodInfo("", "", 9999)]
        public MsgWithDetails ACUndoChanges(bool autoUndoContextIPlus = true)
        {
            MsgWithDetails result = _ObjectContextHelper.ACUndoChanges(autoUndoContextIPlus);
            if (result == null)
            {
                if (ACChangesExecuted != null)
                    ACChangesExecuted.Invoke(this, new ACChangesEventArgs(ACChangesEventArgs.ACChangesType.ACUndoChanges, true));
            }
            else
            {
                if (ACChangesExecuted != null)
                    ACChangesExecuted.Invoke(this, new ACChangesEventArgs(ACChangesEventArgs.ACChangesType.ACUndoChanges, false));
            }
            return result;
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// </summary>
        /// <returns></returns>
        public bool HasModifiedObjectStateEntries()
        {
            return _ObjectContextHelper.HasModifiedObjectStateEntries();
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// </summary>
        /// <returns></returns>
        public bool HasAddedEntities<T>() where T : class
        {
            return _ObjectContextHelper.HasAddedEntities<T>();
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Added-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetAddedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return _ObjectContextHelper.GetChangedEntities<T>(EntityState.Added, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Modified-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetModifiedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return _ObjectContextHelper.GetChangedEntities<T>(EntityState.Modified, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Deleted-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetDeletedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return _ObjectContextHelper.GetChangedEntities<T>(EntityState.Deleted, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Detached-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetDetachedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return _ObjectContextHelper.GetChangedEntities<T>(EntityState.Detached, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Unchanged-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetUnchangedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return _ObjectContextHelper.GetChangedEntities<T>(EntityState.Unchanged, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns EntityState.Modified | EntityState.Added | EntityState.Deleted
        /// </summary>
        /// <returns></returns>
        public IList<T> GetChangedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return _ObjectContextHelper.GetChangedEntities<T>(EntityState.Modified | EntityState.Added | EntityState.Deleted, selector);
        }

        /// <summary>
        /// UNSAFE. Use QueryLock_1X000 outside
        /// </summary>
        /// <returns></returns>
        public IList<Msg> CheckChangedEntities()
        {
            return _ObjectContextHelper.CheckChangedEntities();
        }

        /// <summary>
        /// Refreshes the VBEntityObject if not in modified state. Else it leaves it untouched.
        /// </summary>
        /// <param name="entityObject"></param>
        /// <param name="refreshMode"></param>
        public void AutoRefresh(VBEntityObject entityObject)
        {
            _ObjectContextHelper.AutoRefresh(entityObject);
        }

        /// <summary>
        /// Refreshes all EntityObjects in the EntityCollection if not in modified state. Else it leaves it untouched.
        /// Attention: This method will only refresh the entities with entity keys that are tracked by the ObjectContext. 
        /// If changes are made in background on the database you shoud use the method AutoLoad, to retrieve new Entries from the Database!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityCollection"></param>
        /// <param name="refreshMode"></param>
        public void AutoRefresh<T>(ICollection<T> entityCollection, CollectionEntry entry) where T : class
        {
            _ObjectContextHelper.AutoRefresh<T>(entityCollection, entry);
        }

        /// <summary>
        /// Queries the Database an refreshes the collection if not in modified state. MergeOption.OverwriteChanges
        /// Els if in modified state, then colletion is only refreshed with MergeOption.AppendOnly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityCollection"></param>
        public void AutoLoad<T>(ICollection<T> entityCollection, CollectionEntry entry) where T : class
        {
            _ObjectContextHelper.AutoLoad<T>(entityCollection, entry);
        }

        public void ParseException(MsgWithDetails msg, Exception e)
        {
            _ObjectContextHelper.ParseException(msg, e);
        }

        public bool? IsDerived(Guid baseClassID, Guid derivedClassID)
        {
            return _derivationCache.IsDerived(baseClassID, derivedClassID);
        }

        public void RegisterDerivedClass(Guid baseClassID, Guid derivedClassID, bool isDerived)
        {
            _derivationCache.RegisterDerivedClass(baseClassID, derivedClassID, isDerived);
        }

#region IACUrl Member

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            if (_ObjectContextHelper == null)
                return null;
            return _ObjectContextHelper.ACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return true;
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return _ObjectContextHelper.ACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("", "", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        public void FullDetach(VBEntityObject obj)
        {
            Detach(obj);
            //throw new NotImplementedException();
            // General Problem of ObjectContext-MAnager
            // When a object should be detached, then the object which have a relational relationship will not be detached
            // The Information about the relation are stored in the internal Member _danglingForeignKeys of the ObjectContextManager
            // This entries will never be deleted - so the memory increases for long term open contexts
            // See under: http://referencesource.microsoft.com/#System.Data.Entity/System/Data/Objects/ObjectStateManager.cs
            // The following code is a first attempt to empty this cache:
            /*if (this.ObjectStateManager == null)
                return;
            ObjectStateEntry entry = null;
            if (!this.ObjectStateManager.TryGetObjectStateEntry(obj.EntityKey, out entry))
                return;
            try
            {
                this.Detach(obj);
                Type tOSM = this.ObjectStateManager.GetType();
                //RemoveForeignKeyFromIndex(EntityKey foreignKey)
                //MethodInfo mi = tOSM.GetMethod("FixupKey", BindingFlags.Instance | BindingFlags.NonPublic);
                //mi.Invoke(this.ObjectStateManager, new object[] { entry });
                MethodInfo mi = tOSM.GetMethod("RemoveForeignKeyFromIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(this.ObjectStateManager, new object[] { obj.EntityKey });
            }
            ctch
            {
            }*/
        }
#endregion

#endregion

#region Critical Section
        public void EnterCS()
        {
            _ObjectContextHelper.EnterCS();
        }

        public void EnterCS(bool DeactivateEntityCheck)
        {
            _ObjectContextHelper.EnterCS(DeactivateEntityCheck);
        }

        public void LeaveCS()
        {
            _ObjectContextHelper.LeaveCS();
        }

        private ACMonitorObject _10000_QueryLock = new ACMonitorObject(10000);
        [NotMapped]
        public ACMonitorObject QueryLock_1X000
        {
            get
            {
                return _10000_QueryLock;
            }
        }
        #endregion

        #endregion

        #region DBSetToList

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACAssembly> ACAssemblyList
        {
            get
            {
                return ACAssembly.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACChangeLog> ACChangeLogList
        {
            get
            {
                return ACChangeLog.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClass> ACClassList
        {
            get
            {
                return ACClass.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassConfig> ACClassConfigList
        {
            get
            {
                return ACClassConfig.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassDesign> ACClassDesignList
        {
            get
            {
                return ACClassDesign.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassMessage> ACClassMessageList
        {
            get
            {
                return ACClassMessage.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassMethod> ACClassMethodList
        {
            get
            {
                return ACClassMethod.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassMethodConfig> ACClassMethodConfigList
        {
            get
            {
                return ACClassMethodConfig.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassProperty> ACClassPropertyList
        {
            get
            {
                return ACClassProperty.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassPropertyRelation> ACClassPropertyRelationList
        {
            get
            {
                return ACClassPropertyRelation.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassTask> ACClassTaskList
        {
            get
            {
                return ACClassTask.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassTaskValue> ACClassTaskValueList
        {
            get
            {
                return ACClassTaskValue.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassTaskValuePos> ACClassTaskValuePosList
        {
            get
            {
                return ACClassTaskValuePos.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassText> ACClassTextList
        {
            get
            {
                return ACClassText.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassWF> ACClassWFList
        {
            get
            {
                return ACClassWF.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACClassWFEdge> ACClassWFEdgeList
        {
            get
            {
                return ACClassWFEdge.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACPackage> ACPackageList
        {
            get
            {
                return ACPackage.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACProgram> ACProgramList
        {
            get
            {
                return ACProgram.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACProgramConfig> ACProgramConfigList
        {
            get
            {
                return ACProgramConfig.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACProgramLog> ACProgramLogList
        {
            get
            {
                return ACProgramLog.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACProgramLogTask> ACProgramLogTaskList
        {
            get
            {
                return ACProgramLogTask.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACProject> ACProjectList
        {
            get
            {
                return ACProject.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACPropertyLog> ACPropertyLogList
        {
            get
            {
                return ACPropertyLog.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACPropertyLogRule> ACPropertyLogRuleList
        {
            get
            {
                return ACPropertyLogRule.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<MsgAlarmLog> MsgAlarmLogList
        {
            get
            {
                return MsgAlarmLog.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBConfig> VBConfigList
        {
            get
            {
                return VBConfig.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBGroup> VBGroupList
        {
            get
            {
                return VBGroup.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBGroupRight> VBGroupRightList
        {
            get
            {
                return VBGroupRight.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBLanguage> VBLanguageList
        {
            get
            {
                return VBLanguage.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBLicense> VBLicenseList
        {
            get
            {
                return VBLicense.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBNoConfiguration> VBNoConfigurationList
        {
            get
            {
                return VBNoConfiguration.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBSystem> VBSystemList
        {
            get
            {
                return VBSystem.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBSystemColumns> VBSystemColumnsList
        {
            get
            {
                return VBSystemColumns.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBTranslationView> VBTranslationViewList
        {
            get
            {
                return VBTranslationView.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBUser> VBUserList
        {
            get
            {
                return VBUser.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBUserACClassDesign> VBUserACClassDesignList
        {
            get
            {
                return VBUserACClassDesign.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBUserACProject> VBUserACProjectList
        {
            get
            {
                return VBUserACProject.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBUserGroup> VBUserGroupList
        {
            get
            {
                return VBUserGroup.ToArray();
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<VBUserInstance> VBUserInstanceList
        {
            get
            {
                return VBUserInstance.ToArray();
            }
        }

        #endregion

        #region Listen
        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACKindList
        {
            get
            {
                return gip.core.datamodel.Global.ACKindList;
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACKindCLList
        {
            get
            {
                return gip.core.datamodel.Global.ACKindList.Where(c => (short)c.Value < 10000).OrderBy(c => c.Value);
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACKindPSList
        {
            get
            {
                return gip.core.datamodel.Global.ACKindList.Where(c => (short)c.Value >= (short)Global.ACKinds.PSProperty && (short)c.Value <= (short)Global.ACKinds.PSPropertyExt).OrderBy(c => c.Value);
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACKindMSList
        {
            get
            {
                return gip.core.datamodel.Global.ACKindList.Where(c => (short)c.Value >= (short)Global.ACKinds.MSMethod && (short)c.Value < (short)Global.ACKinds.MSWorkflow).OrderBy(c => c.Value);
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACKindDSList
        {
            get
            {
                return gip.core.datamodel.Global.ACKindList.Where(c => (short)c.Value >= (short)Global.ACKinds.DSDesignLayout && (short)c.Value < (short)Global.ACKinds.DSDesignReport).OrderBy(c => c.Value);
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACUsageList
        {
            get
            {
                return gip.core.datamodel.Global.ACUsageList;
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACPropUsageList
        {
            get
            {
                return gip.core.datamodel.Global.ACPropUsageList;
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACUsageReportList
        {
            get
            {
                return gip.core.datamodel.Global.ACUsageList.Where(c => (short)c.Value >= (short)Global.ACUsages.DULLReport && (short)c.Value <= (short)Global.ACUsages.DUReport);
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACStartTypeList
        {
            get
            {
                return gip.core.datamodel.Global.ACStartTypeList;
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACStorableTypeList
        {
            get
            {
                return gip.core.datamodel.Global.ACStorableTypeList;
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ControlModeList
        {
            get
            {
                return Global.ControlModeList;
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> PropertyControlModeList
        {
            get
            {
                return Global.ControlModeList.Where(c => (Global.ControlModes)c.Value == Global.ControlModes.Hidden || (Global.ControlModes)c.Value == Global.ControlModes.Disabled || (Global.ControlModes)c.Value == Global.ControlModes.Enabled);
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> MethodControlModeList
        {
            get
            {
                return Global.ControlModeList.Where(c => (Global.ControlModes)c.Value == Global.ControlModes.Hidden || (Global.ControlModes)c.Value == Global.ControlModes.Enabled);
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> ACProjectTypeList
        {
            get
            {
                return Global.ACProjectTypeList;
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> WorkflowTypeMethodACClassList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.TPWMethod).OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> WorkflowInvokerACClassList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.TPWNodeWorkflow).OrderBy(c => c.ACIdentifier);
                }
            }
        }


        /// <summary>
        /// Liste aller Workflow-Methoden die in einem ACProgram verwendet werden können
        /// </summary>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClassMethod> ProgramMethodList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClassMethod.Where(c => c.ACKindIndex == (short)Global.ACKinds.MSWorkflow
                                                 && c.IsSubMethod == false && c.ACClass.ParentACClassID == null
                                                 && c.ACClass.ACProject.ACProjectTypeIndex == (Int16)Global.ACProjectTypes.AppDefinition)
                                        .OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClassMethod> ProgramACClassMethodList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClassMethod.Where(c => c.ACKindIndex == (short)Global.ACKinds.MSWorkflow).OrderBy(c => c.ACIdentifier);
                }
            }
        }


        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClassDesign> MenuACClassList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClassDesign.Where(c => c.ACKindIndex == (short)Global.ACKinds.DSDesignMenu
                                                    && c.ACClass.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Root)
                                        .OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> PWClassList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => (c.ACKindIndex == (Int16)Global.ACKinds.TPWGroup ||
                                                c.ACKindIndex == (Int16)Global.ACKinds.TPWMethod) && !c.IsAbstract).OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> PAClassList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => (c.ACKindIndex == (Int16)Global.ACKinds.TACApplicationManager ||
                    c.ACKindIndex == (Int16)Global.ACKinds.TPAModule ||
                    c.ACKindIndex == (Int16)Global.ACKinds.TPAProcessModule ||
                    c.ACKindIndex == (Int16)Global.ACKinds.TPAProcessFunction) && !c.IsAbstract).OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> PWNodeList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => (c.ACKindIndex == (Int16)Global.ACKinds.TPWNode ||
                                                c.ACKindIndex == (Int16)Global.ACKinds.TPWNodeStatic ||
                                                c.ACKindIndex == (Int16)Global.ACKinds.TPWNodeStart ||
                                                c.ACKindIndex == (Int16)Global.ACKinds.TPWNodeEnd ||
                                                c.ACKindIndex == (Int16)Global.ACKinds.TPWNodeMethod ||
                                                c.ACKindIndex == (Int16)Global.ACKinds.TPWNodeWorkflow
                                                ) && !c.IsAbstract).OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> PWNodeWorkFlowList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.TPWMethod && !c.IsAbstract).OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> VBControlACClassList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => (c.ACKindIndex == (short)Global.ACKinds.TACVBControl) && !c.IsAbstract).OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> ConfigACClassList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => (c.ACKindIndex == (short)Global.ACKinds.TACClass) && !c.IsAbstract).OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> AssemblyACClassList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => !string.IsNullOrEmpty(c.AssemblyQualifiedName)).OrderBy(c => c.ACIdentifier);
                }
            }
        }

        [ACPropertyInfo(9999)]
        [NotMapped]
        public IQueryable<ACClass> RoleACClassList
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    return ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.TPARole).OrderBy(c => c.ACIdentifier);
                }
            }
        }


        [ACPropertyInfo(9999)]
        [NotMapped]
        public IEnumerable<ACValueItem> InterpolationMethodList
        {
            get
            {
                return gip.core.datamodel.Global.InterpolationMethodList;
            }
        }


        /// <summary>
        /// Liefert die Liste von der übergebenen Klasse und deren Ableitungen
        /// </summary>
        /// <param name="acClass"></param>
        /// <param name="acClassList"></param>
        /// <returns></returns>
        public void GetBasedOnACClassList(ACClass acClass, List<ACClass> acClassList)
        {
            using (ACMonitor.Lock(QueryLock_1X000))
            {
                acClassList.Add(acClass);
                foreach (var childPWACClass in acClass.ACClass_ParentACClass)
                {
                    GetBasedOnACClassList(childPWACClass, acClassList);
                }
            }
        }


        [ACPropertyInfo(9999, "")]
        [NotMapped]
        public IEnumerable<ACClassConfig> ApplicationList
        {
            get
            {
                try
                {
                    ACClass application = this.ACClass.Where(c => c.ACIdentifier == "Resources").First();
                    string keyACUrl = ".\\ACClassProperty(ApplicationList)";

                    return application.ACClassConfig_ACClass.Where(c => c.KeyACUrl == keyACUrl);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Root != null && Root.Messages != null && Root.InitState == ACInitState.Initialized)
                        Root.Messages.LogException("Database", "ApplicationList", msg);
                }
                return null;
            }
        }

        [ACPropertyInfo(9999, "")]
        [NotMapped]
        public ACClassConfig DirectoryApplication
        {
            get
            {
                using (ACMonitor.Lock(QueryLock_1X000))
                {
                    var query = ApplicationList.Where(c => c.LocalConfigACUrl == "directory");
                    if (!query.Any())
                    {
                        // TODO: Automatisch anlegen, wenn nicht vorhanden
                        return null;
                    }
                    return query.First();
                }
            }
        }

        public DbConnection Connection 
        { 
            get { return Database.GetDbConnection(); } 
        }

        public string DefaultContainerName 
        {
            get { throw new NotImplementedException(); }
        }

        // Build Cache over Type-System to avoid Deadlocks when quering Global dabatbase
        // see deadlock18.txt
        //private object _DictLock = new object();
        private ConcurrentDictionary<string, ACClass> _TypesAssQlfName = new ConcurrentDictionary<string, ACClass>();
        static readonly Func<Database, string, ACClass> s_cQry_TypesCAssQlfName =
            EF.CompileQuery<Database, string, ACClass>(
                (db, assQName) =>
                    db.ACClass.Include("ACClass1_BasedOnACClass")
                            .Include("ACClass1_ParentACClass")
                            .Include("ACClass1_PWACClass")
                            .Include("ACClass1_PWMethodACClass")
                            .Include("ACClass_BasedOnACClass")
                            .Include("ACClass_ParentACClass")
                .Where(c => c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary
                && c.ParentACClassID == null
                && c.AssemblyQualifiedName == assQName).FirstOrDefault()
            );

        private ConcurrentDictionary<string, ACClass> _TypesClassName = new ConcurrentDictionary<string, ACClass>();
        static readonly Func<Database, string, ACClass> s_cQry_TypesClassLibrary =
            EF.CompileQuery<Database, string, ACClass>(
                (db, acIdentifier) =>
                    db.ACClass.Include("ACClass1_BasedOnACClass")
                            .Include("ACClass1_ParentACClass")
                            .Include("ACClass1_PWACClass")
                            .Include("ACClass1_PWMethodACClass")
                            .Include("ACClass_BasedOnACClass")
                            .Include("ACClass_ParentACClass")
                .Where(c => c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary 
                && c.ParentACClassID == null 
                && c.ACIdentifier == acIdentifier).FirstOrDefault()
            );

        static readonly Func<Database, string, ACClass> s_cQry_TypesDBAccess =
            EF.CompileQuery<Database, string, ACClass>(
                (db, acIdentifier) =>
                    db.ACClass.Include("ACClass1_BasedOnACClass")
                            .Include("ACClass1_ParentACClass")
                            .Include("ACClass1_PWACClass")
                            .Include("ACClass1_PWMethodACClass")
                            .Include("ACClass_BasedOnACClass")
                            .Include("ACClass_ParentACClass")
                .Where(c => (c.ACKindIndex == (Int16)Global.ACKinds.TACDBA) 
                            && c.ACIdentifier == acIdentifier).FirstOrDefault()
            );

        static readonly Func<Database, string, ACClass> s_cQry_TypesBSO =
            EF.CompileQuery<Database, string, ACClass>(
                (db, acIdentifier) =>
                    db.ACClass.Include("ACClass1_BasedOnACClass")
                            .Include("ACClass1_ParentACClass")
                            .Include("ACClass1_PWACClass")
                            .Include("ACClass1_PWMethodACClass")
                            .Include("ACClass_BasedOnACClass")
                            .Include("ACClass_ParentACClass")
                .Where(c => (c.ACKindIndex == (Int16)Global.ACKinds.TACBSO || c.ACKindIndex == (Int16)Global.ACKinds.TACBSOGlobal)
                            && c.ACIdentifier == acIdentifier).FirstOrDefault()
            );

        private ConcurrentDictionary<string, ACClass> _TypesACUrlComp = new ConcurrentDictionary<string, ACClass>();
        static readonly Func<Database, string, ACClass> s_cQry_TypesACUrlComp =
            EF.CompileQuery<Database, string, ACClass>(
                (db, acUrl) =>
                    db.ACClass.Include("ACClass1_BasedOnACClass")
                        .Include("ACClass1_ParentACClass")
                        .Include("ACClass1_PWACClass")
                        .Include("ACClass1_PWMethodACClass")
                        .Include("ACClass_BasedOnACClass")
                        .Include("ACClass_ParentACClass")
            .Where(c => c.ACURLComponentCached == acUrl).FirstOrDefault()
        );

        private ConcurrentDictionary<Guid, ACClass> _TypesGuid = new ConcurrentDictionary<Guid, ACClass>(5, 1000);
        static readonly Func<Database, Guid, ACClass> s_cQry_TypesGuid =
            EF.CompileQuery<Database, Guid, ACClass>(
                (db, acClassID) =>
                    db.ACClass.Include("ACClass1_BasedOnACClass")
                            .Include("ACClass1_ParentACClass")
                            .Include("ACClass1_PWACClass")
                            .Include("ACClass1_PWMethodACClass")
                            .Include("ACClass_BasedOnACClass")
                            .Include("ACClass_ParentACClass")
                .Where(c => c.ACClassID == acClassID).FirstOrDefault()
            );

        private ConcurrentDictionary<Type, ACClass> _ACTypeCache = new ConcurrentDictionary<Type, ACClass>(5, 1000);
        static readonly Func<Database, string, ACClass> s_cQry_TypeCache =
            EF.CompileQuery<Database, string, ACClass>(
                (db, assemblyQualifiedName) =>
                    db.ACClass.Include("ACClass1_BasedOnACClass")
                            .Include("ACClass1_ParentACClass")
                            .Include("ACClass1_PWACClass")
                            .Include("ACClass1_PWMethodACClass")
                            .Include("ACClass_BasedOnACClass")
                            .Include("ACClass_ParentACClass")
                            .Where(c => c.AssemblyQualifiedName == assemblyQualifiedName).FirstOrDefault()
            );

        /// <summary>
        /// Build Cache for primitive Types from System namespace to avoid Deadlocks when quering Global dabatbase
        /// </summary>
        private void InitTypeDictFromSystemNamespace()
        {
            using (ACMonitor.Lock(QueryLock_1X000))
            {
                var query = ACClass.Include("ACClass1_BasedOnACClass")
                            .Include("ACClass1_ParentACClass")
                            .Include("ACClass1_PWACClass")
                            .Include("ACClass1_PWMethodACClass")
                            .Include("ACClass_BasedOnACClass")
                            .Include("ACClass_ParentACClass")
                            .Where(c => c.AssemblyQualifiedName.StartsWith("System.") 
                            && c.AssemblyQualifiedName.Contains(", mscorlib,") 
                            && c.ACKindIndex == (short)Global.ACKinds.TACLRBaseTypes);
                foreach (ACClass acClass in query)
                {
                    ACClass result = null;

                    if (!_TypesAssQlfName.TryGetValue(acClass.AssemblyQualifiedName, out result))
                        _TypesAssQlfName.TryAdd(acClass.AssemblyQualifiedName, acClass);

                    if (!_TypesClassName.TryGetValue(acClass.ACIdentifier, out result))
                        _TypesClassName.TryAdd(acClass.ACIdentifier, acClass);

                    if (!_TypesGuid.TryGetValue(acClass.ACClassID, out result))
                        _TypesGuid.TryAdd(acClass.ACClassID, acClass);

                    Type objectFullType = acClass.ObjectFullType;
                    if (objectFullType != null)
                    {
                        if (!_ACTypeCache.TryGetValue(objectFullType, out result))
                            _ACTypeCache.TryAdd(objectFullType, acClass);
                    }
                }
            }
        }

        /// <summary>
        /// Ermittelt die ACClass für den übergebenen Typen
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ACClass GetACType(Type type)
        {
            if (type == null)
                return null;

            ACClass acClass = null;
            if (_ACTypeCache.TryGetValue(type, out acClass))
                return acClass;

            using (ACMonitor.Lock(QueryLock_1X000))
            {
                acClass = s_cQry_TypeCache(this, type.AssemblyQualifiedName);
            }
            if (acClass != null)
            {
                _ACTypeCache.TryAdd(type, acClass);

                ACClass result = null;

                if (!_TypesAssQlfName.TryGetValue(acClass.AssemblyQualifiedName, out result))
                    _TypesAssQlfName.TryAdd(acClass.AssemblyQualifiedName, acClass);

                if (!_TypesClassName.TryGetValue(acClass.ACIdentifier, out result))
                    _TypesClassName.TryAdd(acClass.ACIdentifier, acClass);

                if (!_TypesGuid.TryGetValue(acClass.ACClassID, out result))
                    _TypesGuid.TryAdd(acClass.ACClassID, acClass);

                return acClass;
            }
            if (!type.IsGenericType)
                return null;

            Type genericType = type.GetGenericTypeDefinition();
            if (_ACTypeCache.TryGetValue(genericType, out acClass))
                return acClass;

            using (ACMonitor.Lock(QueryLock_1X000))
            {
                acClass = s_cQry_TypeCache(this, genericType.AssemblyQualifiedName);
            }
            if (acClass != null)
            {
                _ACTypeCache.TryAdd(type, acClass);

                ACClass result = null;

                if (!_TypesAssQlfName.TryGetValue(acClass.AssemblyQualifiedName, out result))
                    _TypesAssQlfName.TryAdd(acClass.AssemblyQualifiedName, acClass);

                if (!_TypesClassName.TryGetValue(acClass.ACIdentifier, out result))
                    _TypesClassName.TryAdd(acClass.ACIdentifier, acClass);

                if (!_TypesGuid.TryGetValue(acClass.ACClassID, out result))
                    _TypesGuid.TryAdd(acClass.ACClassID, acClass);

            }
            return acClass;
        }

        /// <summary>
        /// Ermittelt die ACClass für den Klassennamen
        /// </summary>
        /// <param name="acIdentifier"></param>
        /// <returns></returns>
        public ACClass GetACType(string acIdentifier)
        {
            if (String.IsNullOrEmpty(acIdentifier))
                return null;

            ACClass acClass = null;
            if (_TypesClassName.TryGetValue(acIdentifier, out acClass))
                return acClass;

            using (ACMonitor.Lock(QueryLock_1X000))
            {
                acClass = s_cQry_TypesClassLibrary(this, acIdentifier);
                if (acClass == null)
                    acClass = s_cQry_TypesDBAccess(this, acIdentifier);
                if (acClass == null)
                    acClass = s_cQry_TypesBSO(this, acIdentifier);
            }
            if (acClass != null)
            {
                _TypesClassName.TryAdd(acIdentifier, acClass);

                ACClass result = null;

                if (!_TypesAssQlfName.TryGetValue(acClass.AssemblyQualifiedName, out result))
                    _TypesAssQlfName.TryAdd(acClass.AssemblyQualifiedName, acClass);

                Type objectFullType = acClass.ObjectFullType;
                if (objectFullType != null)
                {
                    if (!_ACTypeCache.TryGetValue(objectFullType, out result))
                        _ACTypeCache.TryAdd(objectFullType, acClass);
                }

                if (!_TypesGuid.TryGetValue(acClass.ACClassID, out result))
                    _TypesGuid.TryAdd(acClass.ACClassID, acClass);

            }
            return acClass;
        }

        /// <summary>
        /// Determines the ACClass for the passed class id
        /// </summary>
        /// <param name="acClassID"></param>
        /// <returns></returns>
        public ACClass GetACType(Guid acClassID)
        {
            if (acClassID == Guid.Empty)
                return null;

            ACClass acClass = null;
            if (_TypesGuid.TryGetValue(acClassID, out acClass))
                return acClass;

            using (ACMonitor.Lock(QueryLock_1X000))
            {
                acClass = s_cQry_TypesGuid(this, acClassID);
            }
            if (acClass != null)
            {
                _TypesGuid.TryAdd(acClassID, acClass);

                ACClass result = null;

                if (!string.IsNullOrEmpty(acClass.AssemblyQualifiedName) && !_TypesAssQlfName.TryGetValue(acClass.AssemblyQualifiedName, out result))
                    _TypesAssQlfName.TryAdd(acClass.AssemblyQualifiedName, acClass);

                Type objectFullType = acClass.ObjectFullType;
                if (objectFullType != null)
                {
                    if (!_ACTypeCache.TryGetValue(objectFullType, out result))
                        _ACTypeCache.TryAdd(objectFullType, acClass);
                }

                if (!_TypesClassName.TryGetValue(acClass.ACIdentifier, out result))
                    _TypesClassName.TryAdd(acClass.ACIdentifier, acClass);

            }
            return acClass;
        }

        /// <summary>
        /// Get Type by ACUrlComponent
        /// </summary>
        /// <param name="acUrlComponent">ACUrl</param>
        /// <returns></returns>
        public ACClass GetACTypeByACUrlComp(string acUrlComponent)
        {
            if (String.IsNullOrEmpty(acUrlComponent))
                return null;

            ACClass acClass = null;
            if (_TypesACUrlComp.TryGetValue(acUrlComponent, out acClass))
                return acClass;

            using (ACMonitor.Lock(QueryLock_1X000))
            {
                acClass = s_cQry_TypesACUrlComp(this, acUrlComponent);
            }
            if (acClass != null)
                _TypesACUrlComp.TryAdd(acUrlComponent, acClass);
            return acClass;
        }


#endregion

        /// <summary>
        /// UNSAFE. Use QueryLock_1X000 outside
        /// </summary>
        /// <returns></returns>
        public void DetachAllEntitiesAndDispose(bool detach = false, bool dispose = true)
        {
            if (_ObjectContextHelper != null && detach)
                _ObjectContextHelper.DetachAllEntities();
            if (dispose)
                Dispose();
        }

        public void Detach(object entity)
        {
            _ObjectContextHelper.Detach(entity);
        }

        public void AcceptAllChanges()
        {
            ((IACEntityObjectContext)ContextIPlus).ChangeTracker.AcceptAllChanges();
        }

        public DbSet<TEntity> CreateObjectSet<TEntity>() where TEntity : class
        {
            return ((IACEntityObjectContext)ContextIPlus).CreateObjectSet<TEntity>();
        }

        public DbSet<TEntity> CreateObjectSet<TEntity>(string entitySetName) where TEntity : class
        {
            return ((IACEntityObjectContext)ContextIPlus).CreateObjectSet<TEntity>(entitySetName);
        }

        public object GetObjectByKey(EntityKey key)
        {
            return _ObjectContextHelper.GetObjectByKey(key);
        }

        public bool TryGetObjectByKey(EntityKey key, out object entity)
        {
            return _ObjectContextHelper.TryGetObjectByKey(key, out entity);
        }
    }
}

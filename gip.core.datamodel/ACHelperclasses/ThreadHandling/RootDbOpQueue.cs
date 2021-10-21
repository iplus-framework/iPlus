using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class RootDbOpQueue
    /// </summary>
    public class RootDbOpQueue : ACEntityOpQueue<Database>
    {
        public const string ClassName = "RootDbOpQueue";
        public const string AppContextPropName = "AppContext";

        /// <summary>
        /// Initializes a new instance of the <see cref="ACDelegateQueue" /> class.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <param name="saveChangesWithoutValidation">saveChangesWithoutValidation</param>
        public RootDbOpQueue(string instanceName, bool saveChangesWithoutValidation)
            : base(instanceName, typeof(Database), Database.ConnectionString, saveChangesWithoutValidation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RootDbOpQueue"/> class.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <param name="database">The database.</param>
        /// <param name="autoOpenClose">if set to <c>true</c> [auto open close].</param>
        /// <param name="saveChangesWithoutValidation">saveChangesWithoutValidation</param>
        public RootDbOpQueue(string instanceName, Database database, bool autoOpenClose, bool saveChangesWithoutValidation)
            : base(instanceName, database, autoOpenClose, saveChangesWithoutValidation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RootDbOpQueue"/> class.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <param name="database">The database.</param>
        /// <param name="workerInterval_ms">The worker interval_ms.</param>
        /// <param name="autoOpenClose">if set to <c>true</c> [auto open close].</param>
        /// <param name="saveChangesWithoutValidation">saveChangesWithoutValidation</param>
        public RootDbOpQueue(string instanceName, Database database, int workerInterval_ms, bool autoOpenClose, bool saveChangesWithoutValidation)
            : base(instanceName, database, workerInterval_ms, autoOpenClose, saveChangesWithoutValidation)
        {
        }

        #region AppContext
        private static IACEntityObjectContext _AppContext;
        /// <summary>
        /// 3. Context für Anwendungsprogrammierung (Nur eine Instanz, ausser wenn Applicationmanager überschrieben)
        /// -----------------------------------------------------------
        /// ApplicationManager.Database = RootDbOpQueue.AppContext
        /// ACEntityOpQueue/IACEntityObjectContext/ AppContextQueue = RootDbOpQueue.AppContextQueue
        /// 
        /// Die ApplicationManager (für jedes einzelne Anwedungsprojekt) haben einen gemeinsamen Anwendungs-Datenbankkontext.
        /// Jeder ApplicationManager hat auch eine statische RootDbOpQueue, die den selben Datenbankkontext verwendet.
        /// Auch hier sollte für Threadsichere Zugriffe die Operationen auf dem Kontext über die RootDbOpQueue erfolgen.
        /// 
        /// Die Proxy-Instanzen verhalten sich ebenso
        /// </summary>
        public static IACEntityObjectContext AppContext
        {
            get
            {
                if (_AppContext != null)
                    return _AppContext;

                try
                {
                    Type typeOfIPlusDB = typeof(gip.core.datamodel.Database);
                    Type typeOfDB = typeOfIPlusDB;
                    if (!String.IsNullOrEmpty(Database.Root.TypeNameOfAppContext))
                        typeOfDB = Type.GetType(Database.Root.TypeNameOfAppContext);
                    if (typeOfDB == null)
                        typeOfDB = typeOfIPlusDB;
                    _AppContext = ACObjectContextManager.GetOrCreateContext(typeOfDB, RootDbOpQueue.ClassName + "." + RootDbOpQueue.AppContextPropName, true);
                    if (_AppContext == null)
                        _AppContext = Database.GlobalDatabase;
                    return _AppContext;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("RootDbOpQueue", "AppContext", msg);

                    return Database.GlobalDatabase;
                }
            }
        }

        private static ACEntityOpQueue<IACEntityObjectContext> _AppContextQueue = null;
        public static ACEntityOpQueue<IACEntityObjectContext> AppContextQueue
        {
            get
            {
                if (_AppContextQueue == null)
                {
                    _AppContextQueue = new ACEntityOpQueue<IACEntityObjectContext>(RootDbOpQueue.ClassName + "." + RootDbOpQueue.AppContextPropName, RootDbOpQueue.AppContext, 100, true, false);
                }
                return _AppContextQueue;
            }
        }
#endregion


    }
}

// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACMonitor.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Data.Objects;
using System.Data;
using System.Data.EntityClient;
using System.Reflection;
using System.Data.Objects.DataClasses;
using System.Configuration;
using System.Data.SqlClient;

namespace gip.core.datamodel
{
    /// <summary>
    /// Es gibt vier unterschiedliche Datenbankkontexte:
    /// 
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
    ///     Die Property ACRoot.Database ist ein Verweis zu Database.GlobalDatabase und hat keinen eigenen Kontext
    ///
    ///
    /// 2. Globaler ACClassTask-Context (Nur eine Instanz)
    /// --------------------------------------------------
    /// static RootDbOpQueue.ACClassTaskQueue
    /// Die Klasse RootDbOpQueue besitzt die Eigenschaft Context/Database, die der Datenbankkontext ist.
    ///
    /// Für Datenbankoperationen auf den ACClassTask-Objekten.
    /// ACClassTask-Objekte werden gelesen/ bzw. erzeugt für ACComponent die in Anwendungsprojekten auftauchen und perisistierbar sind
    /// Auch workflows erzeugen ACClassTask-Instanzen.
    /// Die ACClassTaskQueue sorgt für Threadsichere Zugriffe/Speicherung auf diese Objekte.
    /// Bei Abfragen auf ACClassTask-Objekte über Instanzen aus Database.GlobalDatabase muss immer per GUID erfolgen.
    ///
    ///
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
    ///
    ///
    /// 4. Context für BSOs
    /// --------------------------------------------------------------
    /// Für BSOs wird ein ein gemeinsamer Datenbankontext des ACComponentManagers instanziert.
    /// ACObjectContextManager.GetOrCreateContext/Database/("StaticACComponentManager")
    /// Falls individuell ein eigener Kontext erzeugt werden soll, dann muss die virtuelle Property
    /// public override IACEntityObjectContext Database überschrieben werden.
    ///
    /// Für die BSOs die mit dem iPlus.MES-Datenmodell arbeiten wird ein zusätzlich gemeinsamer Kontext über die
    /// Erweiterungsmethode IACComponentExtension.GetAppContextForBSO() erzeugt.
    /// (Da alle BSO von ACBSOvbNav bzw. ACBSOvb abgeleitet werden sollten)
    /// 
    /// </summary>
    public static class ACObjectContextManager
    {
        #region c'tors
        static ACObjectContextManager()
        {
        }
        #endregion

        #region Properties

        #region private
        private class ContextEntry
        {
            public string ContextIdentifier { get; set; }
            public IACEntityObjectContext Context { get; set; }
        }

        private static readonly ACMonitorObject _10005_LockContextList = new ACMonitorObject(10005);
        static List<ContextEntry> _ContextList = new List<ContextEntry>();
        #endregion

        #region public
        static public IEnumerable<IACEntityObjectContext> Contexts
        {
            get
            {

                using (ACMonitor.Lock(_10005_LockContextList))
                {
                    return _ContextList.Select(c => c.Context).ToArray();
                }
            }
        }

        public static IEnumerable<string> NamespacesOfUsedContexts
        {
            get
            {
                return Contexts.Select(c => c.GetType().Namespace).Distinct();
            }
        }
#endregion

#endregion

#region Methods

#region public
        /// <summary>
        /// Databases the specified entity object.
        /// </summary>
        /// <param name="entityObject">The entity object.</param>
        /// <returns>Database.</returns>
        public static TContext GetObjectContext<TContext>(this EntityObject entityObject) where TContext : IACEntityObjectContext
        {
            IACEntityObjectContext databaseContext = GetObjectContext(entityObject);
            if (databaseContext == null)
                return default(TContext);
            return (TContext) databaseContext;
        }

        public static IACEntityObjectContext GetObjectContext(this EntityObject entityObject)
        {
            var relationshipManager = ((System.Data.Objects.DataClasses.IEntityWithRelationships)entityObject).RelationshipManager;
            PropertyInfo piWrappedOwner = relationshipManager.GetType().GetProperty("WrappedOwner", BindingFlags.NonPublic | BindingFlags.Instance);
            var wrappedOwner = piWrappedOwner.GetValue(relationshipManager, null);
            var piContext = wrappedOwner.GetType().GetProperty("Context", BindingFlags.Public | BindingFlags.Instance);
            IACEntityObjectContext databaseContext = piContext.GetValue(wrappedOwner, null) as IACEntityObjectContext;
            return databaseContext;
        }

        public static IACEntityObjectContext GetObjectContext(this RelatedEnd entityObject)
        {
            PropertyInfo piWrappedOwner = entityObject.GetType().GetProperty("WrappedOwner", BindingFlags.NonPublic | BindingFlags.Instance);
            var wrappedOwner = piWrappedOwner.GetValue(entityObject, null);
            var piContext = wrappedOwner.GetType().GetProperty("Context", BindingFlags.Public | BindingFlags.Instance);
            IACEntityObjectContext databaseContext = piContext.GetValue(wrappedOwner, null) as IACEntityObjectContext;
            return databaseContext;
        }

        /// <summary>
        /// Refreshes the EntityObject if not in modified state. Else it leaves it untouched.
        /// </summary>
        /// <param name="entityObject"></param>
        public static void AutoRefresh(this EntityObject entityObject)
        {
            entityObject.GetObjectContext().AutoRefresh(entityObject);
        }

        public static void AutoRefresh(this EntityObject entityObject, IACEntityObjectContext context)
        {
            context.AutoRefresh(entityObject);
        }

        public static void Refresh(this EntityObject entityObject, RefreshMode refreshMode)
        {
            entityObject.GetObjectContext().Refresh(refreshMode,entityObject);
        }

        public static void Refresh(this EntityObject entityObject, RefreshMode refreshMode, IACEntityObjectContext context)
        {
            context.Refresh(refreshMode, entityObject);
        }

        public static void AutoRefresh<T>(this EntityCollection<T> entityCollection) where T : class
        {
            entityCollection.GetObjectContext().AutoRefresh<T>(entityCollection);
        }

        public static void AutoRefresh<T>(this EntityCollection<T> entityCollection, IACEntityObjectContext context) where T : class
        {
            context.AutoRefresh<T>(entityCollection);
        }

        public static void Refresh<T>(this EntityCollection<T> entityCollection, RefreshMode refreshMode) where T : class
        {
            entityCollection.GetObjectContext().Refresh(refreshMode, entityCollection);
        }

        public static void Refresh<T>(this EntityCollection<T> entityCollection, RefreshMode refreshMode, IACEntityObjectContext context) where T : class
        {
            context.Refresh(refreshMode, entityCollection);
        }

        public static void AutoLoad<T>(this EntityCollection<T> entityCollection) where T : class
        {
            entityCollection.GetObjectContext().AutoLoad(entityCollection);
        }

        public static void AutoLoad<T>(this EntityCollection<T> entityCollection, IACEntityObjectContext context) where T : class
        {
            context.AutoLoad(entityCollection);
        }

        static public TContext GetContext<TContext>(string contextIdentifier = "") where TContext : IACEntityObjectContext
        {

            using (ACMonitor.Lock(_10005_LockContextList))
            {
                IACEntityObjectContext context = null;
                if (String.IsNullOrEmpty(contextIdentifier))
                    context = _ContextList.Where(c => c.Context.GetType() == typeof(TContext)).Select(c => c.Context).FirstOrDefault();
                else
                    context = _ContextList.Where(c => c.Context.GetType() == typeof(TContext) && c.ContextIdentifier == contextIdentifier).Select(c => c.Context).FirstOrDefault();
                return context != null ? (TContext)context : default(TContext);
            }
        }

        static public IACEntityObjectContext GetContext(string contextIdentifier = "")
        {

            using (ACMonitor.Lock(_10005_LockContextList))
            {
                ContextEntry entry = _ContextList.Where(c => c.ContextIdentifier == contextIdentifier).FirstOrDefault();
                if (entry != null)
                {
                    return entry.Context;
                }
            }
            return null;
        }

        public static IACEntityObjectContext GetContextFromACUrl(string acQueryACUrl, string acTypeString)
        {
            string contextName = "";
            if (!string.IsNullOrEmpty(acQueryACUrl))
                contextName = (string.IsNullOrEmpty(acQueryACUrl) || acQueryACUrl.StartsWith("acQueryACUrl")) ? Const.GlobalDatabase : RootDbOpQueue.ClassName + "." + RootDbOpQueue.AppContextPropName;
            else
            {
                contextName = (!acTypeString.Contains("gip.mes.datamodel")) ? Const.GlobalDatabase : RootDbOpQueue.ClassName + "." + RootDbOpQueue.AppContextPropName;
            }
            return GetContext(contextName);
        }

        static public bool HasContext(IACEntityObjectContext context)
        {

            using (ACMonitor.Lock(_10005_LockContextList))
            {
                ContextEntry entry = _ContextList.Where(c => c.Context == context).FirstOrDefault();
                if (entry != null)
                {
                    return true;
                }
            }
            return false;
        }


        static public IACEntityObjectContext GetOrCreateContext(string appNameForCS, Type typeOfDB, string contextIdentifier = "", bool autoCreateNewIPlusContext = true)
        {
            using (ACMonitor.Lock(_10005_LockContextList))
            {
                IACEntityObjectContext context = GetContext(contextIdentifier);
                if (context != null)
                    return context;

                string connectionStringApp = "";
                if (!String.IsNullOrEmpty(appNameForCS))
                {
                    var mi = typeOfDB.GetMethod("ModifiedConnectionString", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    if (mi != null)
                    {
                        try
                        {
                            connectionStringApp = mi.Invoke(null, new object[] { appNameForCS }) as string;
                        }
                        catch (Exception e)
                        {
                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACObjectContextManager", "GetOrCreateContenxt", e.Message);
                        }
                    }
                }

                try
                {
                    if (typeOfDB == typeof(gip.core.datamodel.Database))
                    {
                        if (!String.IsNullOrEmpty(connectionStringApp))
                        {
                            context = Activator.CreateInstance(typeOfDB, new Object[] { connectionStringApp }) as IACEntityObjectContext;
                        }
                        else
                            context = Activator.CreateInstance(typeOfDB) as IACEntityObjectContext;
                    }
                    else
                    {
                        if (autoCreateNewIPlusContext)
                        {
                            if (!String.IsNullOrEmpty(connectionStringApp))
                            {
                                string iPlusConnString = Database.ModifiedConnectionString(appNameForCS);
                                Database iPlusContext = new Database(iPlusConnString);
                                context = Activator.CreateInstance(typeOfDB, new Object[] { connectionStringApp, iPlusContext }) as IACEntityObjectContext;
                            }
                            else
                            {
                                Database iPlusContext = new Database();
                                context = Activator.CreateInstance(typeOfDB, new Object[] { iPlusContext }) as IACEntityObjectContext;
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(connectionStringApp))
                            {
                                context = Activator.CreateInstance(typeOfDB, new Object[] { connectionStringApp }) as IACEntityObjectContext;
                            }
                            else
                            {
                                context = Activator.CreateInstance(typeOfDB) as IACEntityObjectContext;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACObjectContextManager", "GetOrCreateContext", msg);
                }
                if (context == null)
                    return null;
                _ContextList.Add(new ContextEntry() { ContextIdentifier = contextIdentifier, Context = context });
                return context;
            }
        }

        static public IACEntityObjectContext GetOrCreateContext(Type typeOfDB, string contextIdentifier = "", bool autoCreateNewIPlusContext = true, bool createSeperateConnection = false)
        {
            using (ACMonitor.Lock(_10005_LockContextList))
            {
                IACEntityObjectContext context = GetContext(contextIdentifier);
                if (context != null)
                    return context;
                try
                {
                    if (typeOfDB == typeof(gip.core.datamodel.Database))
                    {
                        if (createSeperateConnection)
                            context = Activator.CreateInstance(typeOfDB, new Object[] { createSeperateConnection } ) as IACEntityObjectContext;
                        else
                            context = Activator.CreateInstance(typeOfDB) as IACEntityObjectContext;
                    }
                    else
                    {
                        if (autoCreateNewIPlusContext)
                        {
                            if (createSeperateConnection)
                            {
                                Database iPlusContext = new Database(createSeperateConnection);
                                context = Activator.CreateInstance(typeOfDB, new Object[] { createSeperateConnection, iPlusContext } ) as IACEntityObjectContext;
                            }
                            else
                            {
                                Database iPlusContext = new Database();
                                context = Activator.CreateInstance(typeOfDB, new Object[] { iPlusContext }) as IACEntityObjectContext;
                            }
                        }
                        else
                        {
                            if (createSeperateConnection)
                            {
                                context = Activator.CreateInstance(typeOfDB, new Object[] { createSeperateConnection } ) as IACEntityObjectContext;
                            }
                            else
                            {
                                context = Activator.CreateInstance(typeOfDB) as IACEntityObjectContext;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                    {
                        msg += " Inner:" + e.InnerException.Message;
                        if (e.InnerException.InnerException != null && e.InnerException.InnerException.Message != null)
                        {
                            msg += " Inner:" + e.InnerException.InnerException.Message;
                        }
                    }

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACObjectContextManager", "GetOrCreateContenxt", msg);
                }
                if (context == null)
                    return null;
                _ContextList.Add(new ContextEntry() { ContextIdentifier = contextIdentifier, Context = context });
                return context;
            }
        }

        static public TContext GetOrCreateContext<TContext>(string contextIdentifier = "", string connectionString = "", Database parentContextIPlus = null) where TContext : IACEntityObjectContext
        {
            using (ACMonitor.Lock(_10005_LockContextList))
            {
                TContext context = GetContext<TContext>(contextIdentifier);
                if (context != null)
                    return context;
                if (parentContextIPlus == null)
                {
                    if (String.IsNullOrEmpty(connectionString))
                        context = (TContext)Activator.CreateInstance(typeof(TContext));
                    else
                        context = (TContext)Activator.CreateInstance(typeof(TContext), new Object[] { connectionString });
                }
                else
                {
                    if (String.IsNullOrEmpty(connectionString))
                        context = (TContext)Activator.CreateInstance(typeof(TContext), new Object[] { parentContextIPlus });
                    else
                        context = (TContext)Activator.CreateInstance(typeof(TContext), new Object[] { connectionString, parentContextIPlus });
                }
                if (context == null)
                    return default(TContext);
                _ContextList.Add(new ContextEntry() { ContextIdentifier = contextIdentifier, Context = context });
                return context;
            }
        }

        static public TContext GetOrCreateContext<TContext>(EntityConnection connection, string contextIdentifier = "", Database parentContextIPlus = null) where TContext : IACEntityObjectContext
        {
            using (ACMonitor.Lock(_10005_LockContextList))
            {
                TContext context = GetContext<TContext>(contextIdentifier);
                if (context != null)
                    return context;
                if (parentContextIPlus == null)
                {
                    if (connection == null)
                        context = (TContext)Activator.CreateInstance(typeof(TContext));
                    else
                        context = (TContext)Activator.CreateInstance(typeof(TContext), new Object[] { connection });
                }
                else
                {
                    if (connection == null)
                        context = (TContext)Activator.CreateInstance(typeof(TContext), new Object[] { parentContextIPlus });
                    else
                        context = (TContext)Activator.CreateInstance(typeof(TContext), new Object[] { connection, parentContextIPlus });
                }
                if (context == null)
                    return default(TContext);
                _ContextList.Add(new ContextEntry() { ContextIdentifier = contextIdentifier, Context = context });
                return context;
            }
        }

        static public bool Add(IACEntityObjectContext context, string contextIdentifier = "")
        {
            using (ACMonitor.Lock(_10005_LockContextList))
            {
                if (GetContext(contextIdentifier) != null)
                    return false;
                if (Contexts.Any() && Contexts.Contains(context))
                    return false;
                _ContextList.Add(new ContextEntry() { ContextIdentifier = contextIdentifier, Context = context });
                return true;
            }
        }


        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// </summary>
        /// <returns></returns>
        static public bool DisposeAndRemove(IACEntityObjectContext context)
        {
            if (context == null)
                return true;
            ContextEntry entry = null;
            using (ACMonitor.Lock(_10005_LockContextList))
            {
                entry = _ContextList.Where(c => c.Context == context).FirstOrDefault();
            }
            if (entry != null)
            {
                if (context.IsSeparateIPlusContext)
                {
                    using (ACMonitor.Lock(context.ContextIPlus.QueryLock_1X000))
                    {
                        context.ContextIPlus.DetachAllEntitiesAndDispose(false);
                    }
                }

                using (ACMonitor.Lock(context.QueryLock_1X000))
                {
                    context.DetachAllEntitiesAndDispose(false);
                }

                using (ACMonitor.Lock(_10005_LockContextList))
                {
                    _ContextList.Remove(entry);
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// </summary>
        /// <returns></returns>
        static public void DisposeAndRemoveAll()
        {
            List<ContextEntry> copyList = null;

            using (ACMonitor.Lock(_10005_LockContextList))
            {
                copyList = _ContextList.ToList();
            }

            if (copyList != null && copyList.Any())
            {
                foreach (ContextEntry entry in copyList)
                {
                    if (entry.Context.IsSeparateIPlusContext)
                    {
                        using (ACMonitor.Lock(entry.Context.ContextIPlus.QueryLock_1X000))
                        {
                            entry.Context.ContextIPlus.DetachAllEntitiesAndDispose(false);
                        }
                    }

                    using (ACMonitor.Lock(entry.Context.QueryLock_1X000))
                    {
                        entry.Context.DetachAllEntitiesAndDispose(false);
                    }

                    using (ACMonitor.Lock(_10005_LockContextList))
                    {
                        _ContextList.Remove(entry);
                    }
                }
            }
        }

#region Methods -> Factory new context

        public static string FactoryEntityConnectionString(SQLInstanceInfo serverInfo, bool iPlusContext)
        {
            string connStringID = iPlusContext ? "iPlusV4_Entities" : "iPlusMESV4_Entities";
            ConnectionStringSettings csSettings = System.Configuration.ConfigurationManager.ConnectionStrings[connStringID];
            string originalConnectionString = csSettings.ConnectionString;

            var ecsBuilder = new EntityConnectionStringBuilder(originalConnectionString);
            SqlConnectionStringBuilder sqlCsBuilder = new SqlConnectionStringBuilder(ecsBuilder.ProviderConnectionString)
            {
                DataSource = serverInfo.ServerName,
                InitialCatalog = serverInfo.Database,
                UserID = serverInfo.Username,
                Password = serverInfo.Password
            };
            var providerConnectionString = sqlCsBuilder.ToString();
            ecsBuilder.ProviderConnectionString = providerConnectionString;
            string contextConnectionString = ecsBuilder.ToString();
            contextConnectionString = contextConnectionString.Replace("Application Name", "App");
            contextConnectionString = contextConnectionString.Replace(@".\\", @".\");
            return contextConnectionString;
        }

        static public IACEntityObjectContext FactoryContext(SQLInstanceInfo serverInfo, bool iPlusContext)
        {
            string iPlusConnString = FactoryEntityConnectionString(serverInfo, true);
            Database db = Activator.CreateInstance(TypeAnalyser.GetTypeInAssembly("gip.core.datamodel.Database"), iPlusConnString) as Database;
            IACEntityObjectContext context = db;
            if(!iPlusContext)
            {
                string databaseAppConnString = FactoryEntityConnectionString(serverInfo, false);
                context = Activator.CreateInstance(TypeAnalyser.GetTypeInAssembly("gip.mes.datamodel.DatabaseApp"), databaseAppConnString, db) as IACEntityObjectContext;
            }
            return context;
        }

#endregion

#endregion

#endregion

    }
}

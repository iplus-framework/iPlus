// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace gip.core.datamodel
{
    public class ACObjectContextHelper
    {
        #region c'tors
        public ACObjectContextHelper(IACEntityObjectContext objectContext)
        {
            _ObjectContext = objectContext;
            if (ObjectContext == null)
                throw new ArgumentException("Passed IACObjectContext is not a gip.core.datamodel.Database");
            _ObjectContext.Database.SetCommandTimeout(ACObjectContextHelper.CommandTimeout);
            _ObjectContext.SavingChanges += Database_SavingChanges;
        }

        public void Dispose()
        {
            if (_ObjectContext == null)
                return;
            _ObjectContext.SavingChanges -= Database_SavingChanges;

        }
#endregion

#region const
        /// <summary>
        /// Duration in ms for Thread.Sleep() after a transaction error occured. DEfault is 100ms.
        /// </summary>
        public const int C_TimeoutForRetryAfterTransError = 100;

        /// <summary>
        /// Number of Retries if ACSaveChanges fails due to a transaction error. Default is 5
        /// </summary>
        public const int C_NumberOfRetriesOnTransError = 5;

        public static readonly FormattableString SET_READ_UNCOMMITED = $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";
        public static readonly FormattableString SET_READ_COMMITED = $"$SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
#endregion

#region Properties

#region private

        IACEntityObjectContext _ObjectContext;
        private bool _DeactivateEntityCheck = false;
        bool _IsSaving = false;
#if DEBUG
        int _DisconnCounter = 0;
        bool _LogModifiedPropertiesToDebugWindow = false;
#endif

#endregion

#region public
        public DbContext ObjectContext
        {
            get
            {
                return _ObjectContext as DbContext;
            }
        }

        public Database Database
        {
            get
            {
                if (_ObjectContext.ContextIPlus != null)
                    return _ObjectContext.ContextIPlus;
                Database database = _ObjectContext as Database;
                if (database != null)
                    return database;
                return Database.GlobalDatabase;
            }
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// </summary>
        /// <returns></returns>
        public bool IsChanged
        {
            get
            {
                bool isChanged = false;
                if (_ObjectContext.ContextIPlus != null 
                    && _ObjectContext.ContextIPlus != _ObjectContext 
                    && _ObjectContext.IsSeparateIPlusContext)
                    isChanged = _ObjectContext.ContextIPlus.IsChanged;
                if (!isChanged)
                    isChanged = HasModifiedObjectStateEntries();
                return isChanged;
            }
        }


        private static short? _CommandTimeout = null;
        public static short CommandTimeout
        {
            get
            {
                if (_CommandTimeout.HasValue)
                    return _CommandTimeout.Value;
                _CommandTimeout = 30;
                try
                {
                    EFConfiguration coreConfig = (EFConfiguration)CommandLineHelper.ConfigCurrentDir.GetSection("Process/EFConfiguration");
                    if (coreConfig != null)
                    {
                        if (coreConfig.CommandTimeout >= 0)
                            _CommandTimeout = coreConfig.CommandTimeout;
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACObjectContextHelper", "CommandTimeout", msg);
                }
                return _CommandTimeout.Value;
            }
        }

#endregion

#endregion

#region Methods

#region Public

        /// <summary>
        /// Saves all changes in the Custom-Database-Context as well as in the iPlus-Context
        /// If ContextIPlus is not seperate  (Property IsSeperateIPlusContext == false / ContextIPlus == Database.GlobalDatabase) then SaveChanges will not be invoked for the global database.
        /// If you wan't that, then you have to pass an nes iPlus-Context-Instance to the constructor of the Custom-Database-Context!
        /// UNSAFE. Use QueryLock_1X000 outside for Custom-Database-Context as well as for the seperate iPlus-Context
        /// </summary>
        /// <returns></returns>
        public MsgWithDetails ACSaveChanges(bool autoSaveContextIPlus = true, bool acceptAllChangesOnSuccess = true, bool validationOff = false, bool writeUpdateInfo = true)
        {
            // Save-Changes on iPlus-Context is allowed if it's not the Clobal-Context.
            if (autoSaveContextIPlus
                && _ObjectContext.ContextIPlus != null
                && _ObjectContext.ContextIPlus != _ObjectContext
                && _ObjectContext.IsSeparateIPlusContext
                && _ObjectContext.ContextIPlus.IsChanged)
            {
                MsgWithDetails subMessage = _ObjectContext.ContextIPlus.ACSaveChanges(false, acceptAllChangesOnSuccess, validationOff, writeUpdateInfo);
                if (subMessage != null)
                    return subMessage;
            }

            try
            {
                if (!validationOff)
                {
                    IList<Msg> resultList = CheckChangedEntities(writeUpdateInfo);
                    if (resultList != null)
                    {
                        MsgWithDetails msg = new MsgWithDetails(resultList) { Source = _ObjectContext.ACIdentifier, MessageLevel = eMsgLevel.Error, ACIdentifier = _ObjectContext.ACIdentifier, Message = Database.Root.Environment.TranslateMessage(Database.GlobalDatabase, "Error00035") };
                        return msg;
                    }
                }

                _ObjectContext.SaveChanges(acceptAllChangesOnSuccess);
                return null;
            }
            catch (EntityCheckException entityEx)
            {
                MsgWithDetails msg = new MsgWithDetails(entityEx.SubMessages) { Source = _ObjectContext.ACIdentifier, MessageLevel = eMsgLevel.Error, ACIdentifier = _ObjectContext.ACIdentifier, Message = Database.Root.Environment.TranslateMessage(Database.GlobalDatabase, "Error00035") };
                return msg;
            }
            // TODO: Behandlung von DbUpdateConcurrencyException, das ausgelöst wird durch das implizite Ändern des RowVersion-Feldes 
            //catch (DbUpdateConcurrencyException concurrencyException)
            //{
            //}
            catch (Exception e)
            {
                bool isDisconnectedException = IsDisconnectedException(e);
                //#if DEBUG
                //                if (!validationOff)
                //                {
                //                    var modifyList = _ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified | EntityState.Added | EntityState.Deleted).ToArray();

                //                    foreach (var entity in modifyList)
                //                    {
                //                        foreach (var p in entity.GetModifiedProperties())
                //                        {
                //                            System.Diagnostics.Debug.WriteLine(entity.Entity.ToString() + " => " + p);
                //                        }

                //                        if (!isDisconnectedException)
                //                        {
                //                            ACClass acClass = Database.GlobalDatabase.ACClass.Where(c => c.ACIdentifier == entity.EntityKey.EntitySetName).FirstOrDefault();
                //                            if (acClass != null)
                //                            {
                //                                IACObject acObjectSet = entity.Entity as IACObject;
                //                                foreach (var acClassProperty in acClass.ACClassProperty_ACClass.Where(c => !c.IsNullable))
                //                                {
                //                                    if (acObjectSet.ACUrlCommand(acClassProperty.ACIdentifier) == null)
                //                                    {
                //                                        System.Diagnostics.Debug.WriteLine("Null not allowed: " + entity.Entity.ToString() + " => " + acClassProperty.ACIdentifier);

                //                                    }
                //                                }
                //                            }
                //                        }
                //                    }
                //                }
                //#endif

                MsgWithDetails msg = new MsgWithDetails { Source = Const.ContextDatabase, MessageLevel = eMsgLevel.Error, ACIdentifier = Const.ContextDatabase, Message = Database.Root.Environment.TranslateMessage(Database.GlobalDatabase, "Error00035") };
                ParseException(msg, e);
                return msg;
            }
        }

        public MsgWithDetails ACSaveChangesWithRetry(ushort? retries = null, bool autoSaveContextIPlus = true, bool acceptAllChangesOnSuccess = true, bool validationOff = false, bool writeUpdateInfo = true)
        {
            if (   !retries.HasValue 
                || retries == 0 
                || retries > 10)
                retries = C_NumberOfRetriesOnTransError;
            MsgWithDetails msg = null;
            for (ushort i = 0; i < retries; i++)
            {
                msg = ACSaveChanges(autoSaveContextIPlus, acceptAllChangesOnSuccess, validationOff, writeUpdateInfo);
                if (msg == null)
                    break;
                Thread.Sleep(C_TimeoutForRetryAfterTransError);
            }
            return msg;
        }

        /// <summary>
        /// Undoes all changes in the Custom-Database-Context as well as in the iPlus-Context
        /// If ContextIPlus is not seperate  (Property IsSeperateIPlusContext == false / ContextIPlus == Database.GlobalDatabase) then Undo will not be invoked for the global database.
        /// If you wan't that, then you have to pass an new iPlus-Context-Instance to the constructor of the Custom-Database-Context!
        /// UNSAFE. Use QueryLock_1X000 outside for Custom-Database-Context as well as for the seperate iPlus-Context
        /// </summary>
        /// <returns></returns>
        public MsgWithDetails ACUndoChanges(bool autoUndoContextIPlus = true)
        {
            MsgWithDetails msgWithDetails = null;
            try
            {
                IEnumerable<EntityEntry> trackedEntries = _ObjectContext.ChangeTracker.Entries();
                // 1. Hole manipulierte Objekte
                try
                {
                    var entityStateList = trackedEntries.Where(c => c.State == EntityState.Modified);
                    if ((entityStateList != null) && (entityStateList.Any()))
                    {
                        var entityList = entityStateList.Where(c => c.Entity != null).Select(c => c.Entity);
                        if (entityList != null && entityList.Any())
                        {
                            // Leere liste indem die Objekte aus der Datenbank nachgeladen werden (StoreWins)
                            foreach (EntityEntry entity in entityStateList)
                            {
                                entity.Reload();
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
                        Database.Root.Messages.LogException("ACObjectContextHelper", "ACUndoChanges", msg);
                    if (msgWithDetails == null)
                        msgWithDetails = new MsgWithDetails();
                    msgWithDetails.AddDetailMessage(new Msg(eMsgLevel.Exception, msg) { ACIdentifier = "ACUndoChanges(0)" });
                }


                // 2. Hole gelöschte Objekte
                try
                {
                    var entityStateList = trackedEntries.Where(c => c.State == EntityState.Deleted);

                    if ((entityStateList != null) && (entityStateList.Any()))
                    {
                        var entityList = entityStateList.Where(c => c.Entity != null).Select(c => c.Entity);
                        if (entityList != null && entityList.Any())
                        {
                            // Lade Objekte aus der Datenbank nach (StoreWins)
                            //_ObjectContext.Refresh(entityList);
                            foreach (VBEntityObject entity in entityList)
                            {
                                entity.RevertDeleteACObject(_ObjectContext);
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
                        Database.Root.Messages.LogException("ACObjectContextHelper", "ACUndoChanges(10)", msg);
                    if (msgWithDetails == null)
                        msgWithDetails = new MsgWithDetails();
                    msgWithDetails.AddDetailMessage(new Msg(eMsgLevel.Exception, msg) { ACIdentifier = "ACUndoChanges(10)" });
                }

                // 5. Hole vom Kontext hinzugefügte Objekte
                try
                {
                    var entityStateList = trackedEntries.Where(c => c.State == EntityState.Added);

                    if ((entityStateList != null) && (entityStateList.Any()))
                    {
                        var entityStates = entityStateList.ToArray();
                        foreach (EntityEntry entityState in entityStates)
                        {
                            if (entityState.State != EntityState.Detached && entityState.Entity != null)
                            {
                                try
                                {
                                    _ObjectContext.Remove(entityState.Entity);
                                }

                                catch (Exception e)
                                {
                                    string msg = e.Message;
                                    if (e.InnerException != null && e.InnerException.Message != null)
                                        msg += " Inner:" + e.InnerException.Message;

                                    if (Database.Root != null && Database.Root.Messages != null)
                                        Database.Root.Messages.LogException("ACObjectContextHelper", "ACUndoChanges(20)", msg);
                                    if (msgWithDetails == null)
                                        msgWithDetails = new MsgWithDetails();
                                    msgWithDetails.AddDetailMessage(new Msg(eMsgLevel.Exception, msg) { ACIdentifier = "ACUndoChanges(20)" });
                                }
                            }
                            // Else-Fall testhalber eingebaut:
                            // TODO: BUGBEHEBUNG: Nach Auswahl in einer Combobox ist Entity null in der Liste GetObjectStateEntries(System.Data.EntityState.Added)
                            /*else
                            {
                                entityState.ChangeState(System.Data.EntityState.Unchanged);
                            }*/
                        }
                    }

                    entityStateList = trackedEntries.Where(c => c.State == EntityState.Added);

                    if ((entityStateList != null) && (entityStateList.Any()))
                    {
                        var entityList = entityStateList.Where(c => c.Entity != null).Select(c => c.Entity);
                        if (entityList != null && entityList.Any())
                        {
                            // Lade Objekte aus der Datenbank nach (StoreWins)
                            throw new NotImplementedException();
                            //_ObjectContext.Refresh(RefreshMode.StoreWins, entityList);
                        }
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACObjectContextHelper", "ACUndoChanges(30)", msg);
                    if (msgWithDetails == null)
                        msgWithDetails = new MsgWithDetails();
                    msgWithDetails.AddDetailMessage(new Msg(eMsgLevel.Exception, msg) { ACIdentifier = "ACUndoChanges(30)" });
                }

                /* Der StoreWins-Modus bedeutet, dass die Objekte in der Auflistung aktualisiert werden sollen, 
                 * um mit den Datenquellenwerten übereinzustimmen. ClientWins bedeutet, 
                 * dass nur die Änderungen im Objektkontext beibehalten werden, 
                 * selbst wenn in der Datenquelle andere Änderungen durchgeführt wurden.
                 */
                _ObjectContext.ChangeTracker.AcceptAllChanges();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACObjectContextHelper", "ACUndoChanges(40)", msg);
                if (msgWithDetails == null)
                    msgWithDetails = new MsgWithDetails();
                msgWithDetails.AddDetailMessage(new Msg(eMsgLevel.Exception, msg) { ACIdentifier = "ACUndoChanges(40)" });
            }

            if (autoUndoContextIPlus
                && _ObjectContext.ContextIPlus != null
                && _ObjectContext.ContextIPlus != _ObjectContext
                && _ObjectContext.IsSeparateIPlusContext)
            {
                MsgWithDetails subMessage = _ObjectContext.ContextIPlus.ACUndoChanges(autoUndoContextIPlus);
                if (subMessage != null)
                    return subMessage;
            }

            return msgWithDetails;
        }


        public void Refresh(RefreshMode refreshMode, object entity)
        {
            if (entity == null || _ObjectContext == null)
                return;
            EntityEntry entry = _ObjectContext.Entry(entity);
            if (entry == null) 
                return;
            if (   refreshMode == RefreshMode.StoreWins
                || entry.State == EntityState.Unchanged)
                entry.Reload();
        }

        public object GetObjectByKey(EntityKey key)
        {
            return ObjectContext.Find(key.EntityType, key.EntityKeyValues.Select(c => c.Value).ToArray());
        }

        public bool TryGetObjectByKey(EntityKey key, out object entity)
        {
            try
            {
                entity = GetObjectByKey(key);
                return true;
            }
            catch (Exception)
            {
                entity = null;
                return false;
            }
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// </summary>
        /// <returns></returns>
        public bool HasModifiedObjectStateEntries()
        {
            try
            {
                IEnumerable<EntityEntry> entityStateList = null;

                using (ACMonitor.Lock(_ObjectContext.QueryLock_1X000))
                {

                    entityStateList = _ObjectContext.ChangeTracker.Entries().Where(c => c.State == EntityState.Modified || c.State == EntityState.Added || c.State == EntityState.Deleted).ToList();

                }
                if (entityStateList != null && entityStateList.Any())
                    return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACObjectContextHelper", "HasModifiedObjectStateEntries", msg);
            }
            return false;
        }


        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// </summary>
        /// <returns></returns>
        public bool HasAddedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            IList<T> list = GetAddedEntities<T>(selector);
            return list.Any();
        }


        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Added-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetAddedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return GetChangedEntities<T>(EntityState.Added, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Modified-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetModifiedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return GetChangedEntities<T>(EntityState.Modified, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Deleted-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetDeletedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return GetChangedEntities<T>(EntityState.Deleted, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Detached-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetDetachedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return GetChangedEntities<T>(EntityState.Detached, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns Entities with EntityState.Unchanged-State only
        /// </summary>
        /// <returns></returns>
        public IList<T> GetUnchangedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return GetChangedEntities<T>(EntityState.Unchanged, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// returns EntityState.Modified | EntityState.Added | EntityState.Deleted
        /// </summary>
        /// <returns></returns>
        public IList<T> GetChangedEntities<T>(Func<T, bool> selector = null) where T : class
        {
            return GetChangedEntities<T>(EntityState.Modified | EntityState.Added | EntityState.Deleted, selector);
        }

        /// <summary>
        /// THREAD-SAFE. Uses QueryLock_1X000
        /// </summary>
        /// <returns></returns>
        public IList<T> GetChangedEntities<T>(EntityState entityState, Func<T, bool> selector) where T : class
        {
            using (ACMonitor.Lock(_ObjectContext.QueryLock_1X000))
            {
/*
                var query = _ObjectContext.ObjectStateManager.GetObjectStateEntries(entityState)
                                                                .Where(c => c.Entity is T)
                                                                .Select(c => c.Entity as T);
*/
                var query = _ObjectContext.ChangeTracker.Entries()
                    .Where(c => c.State == entityState)
                    .Where(c => c.Entity is T)
                    .Select(c => c.Entity as T);

                if (selector != null)
                    query = query.Where(c => selector(c));
                return query.ToList();
            }
        }

        /// <summary>
        /// UNSAFE. Use QueryLock_1X000 outside
        /// </summary>
        /// <returns></returns>
        public IList<Msg> CheckChangedEntities(bool writeUpdateInfo = true)
        {
            IList<Msg> subResultList = null;
            IList<Msg> resultList = null;
            List<Tuple<ACChangeLog, int>> changeLogList = new List<Tuple<ACChangeLog, int>>();
            try
            {
                IEnumerable<EntityEntry> trackedEntries = _ObjectContext.ChangeTracker.Entries();
                IEnumerable<EntityEntry> modified = trackedEntries.Where(c => c.State == EntityState.Modified);
#if DEBUG
                if (_LogModifiedPropertiesToDebugWindow && modified.Any())
                {
                    foreach (var modifiedItem in modified)
                    {
                        var modifiedProperties = modifiedItem.Properties.Where(c => c.IsModified).ToList();
                        Console.WriteLine("Modified item: " + modifiedItem.ToString());
                        foreach (var propName in modifiedProperties)
                        {
                            Console.WriteLine("Property {0} changed from {1} to {2}",
                                 propName,
                                 propName.OriginalValue,
                                 propName.CurrentValue);
                        }
                    }

                }
#endif
                foreach (EntityEntry ose in modified)
                {
                    IACObjectEntity o = ose.Entity as IACObjectEntity;
                    if (o != null)
                    {
                        if (writeUpdateInfo)
                            subResultList = o.EntityCheckModified(_ObjectContext.UserName, _ObjectContext);
                        if (subResultList != null)
                        {
                            if (resultList == null)
                                resultList = new List<Msg>();
                            foreach (Msg subMsg in subResultList)
                            {
                                resultList.Add(subMsg);
                            }
                        }

                        subResultList = (o as IACObject).CheckACObject(ose, changeLogList);
                        if (subResultList != null)
                        {
                            if (resultList == null)
                                resultList = new List<Msg>();
                            foreach (Msg subMsg in subResultList)
                            {
                                resultList.Add(subMsg);
                            }
                        }
                    }
                }

                IEnumerable<EntityEntry> added = trackedEntries.Where(c => c.State == EntityState.Added);
                foreach (EntityEntry ose in added)
                {
                    if (ose.State != EntityState.Added)
                        continue;
                    IACObjectEntity o = ose.Entity as IACObjectEntity;
                    if (o != null)
                    {
                        if (writeUpdateInfo)
                            subResultList = o.EntityCheckAdded(_ObjectContext.UserName, _ObjectContext);
                        if (subResultList != null)
                        {
                            if (resultList == null)
                                resultList = new List<Msg>();
                            foreach (Msg subMsg in subResultList)
                            {
                                resultList.Add(subMsg);
                            }
                        }

                        subResultList = (o as IACObject).CheckACObject(ose, changeLogList);
                        if (subResultList != null)
                        {
                            if (resultList == null)
                                resultList = new List<Msg>();
                            foreach (Msg subMsg in subResultList)
                            {
                                resultList.Add(subMsg);
                            }
                        }
                    }
                }

                IEnumerable<EntityEntry> deleted = trackedEntries.Where(c => c.State == EntityState.Deleted);
                foreach (EntityEntry ose in deleted)
                {
                    if (ose.State != EntityState.Deleted)
                        continue;

                    IACObjectEntity o = ose.Entity as IACObjectEntity;
                    if (o != null)
                    {
                        IEnumerable<string> modifiedProps = Array.Empty<string>();
                        ACClass entitySchema = o.ACType as ACClass;
                        if (entitySchema != null)
                        {
                            foreach (ACClassProperty property in entitySchema.Properties)
                            {
                                IACObjectReflectionExtension.ProcessChangeLog(o, entitySchema, property, modifiedProps, ose, changeLogList);
                            }
                        }
                    }
                }

                if (resultList == null && changeLogList.Any())
                {
                    using (Database db = new Database())
                    {
                        Guid vbUserID = db.Root().Environment.User.VBUserID;
                        foreach (var item in changeLogList)
                        {
                            item.Item1.VBUserID = vbUserID;

                            if (item.Item2 == 0)
                                db.ACChangeLog.Add(item.Item1);
                            else
                            {
                                var changeLogs = db.ACChangeLog.Where(c => c.ACClassID == item.Item1.ACClassID && c.ACClassPropertyID == item.Item1.ACClassPropertyID
                                                                                              && c.EntityKey == item.Item1.EntityKey).OrderBy(x => x.ChangeDate).ToList();
                                int changeDiff = (changeLogs.Count - item.Item2) + 1;

                                if (changeDiff > 0)
                                {
                                    for (int i = 0; i < changeDiff; i++)
                                        db.ACChangeLog.Remove(changeLogs[i]);
                                }
                                db.ACChangeLog.Add(item.Item1);
                            }
                        }
                        db.ACSaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACObjectContextHelper", "CheckChangedEntities", msg);
            }
            return resultList;
        }

        /// <summary>
        /// Refreshes the VBEntityObject if not in modified state. Else it leaves it untouched.
        /// </summary>
        /// <param name="entityObject"></param>
       public void AutoRefresh(VBEntityObject entityObject)
        {
            if (entityObject.EntityState == EntityState.Unchanged)
                entityObject.Context.Entry(entityObject).Reload();
        }

        /// Refreshes all EntityObjects in the EntityCollection if not in modified state. Else it leaves it untouched.
        /// Attention: This method will only refresh the entities with entity keys that are tracked by the ObjectContext. 
        /// If changes are made in background on the database you shoud use the method AutoLoad, to retrieve new Entries from the Database!
        ///<summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityCollection"></param>
        /// <param name="entry"></param>
        ///</summary>
        public void AutoRefresh<T>(ICollection<T> entityCollection, CollectionEntry entry) where T : class
        {
            if (entry.CurrentValue == null)
                return;
            foreach (var item in entry.CurrentValue)
            {
                VBEntityObject entityObj = item as VBEntityObject;
                if (entityObj != null)
                    entityObj.AutoRefresh();
            }
        }

        /// <summary>
        /// Queries the Database an refreshes the collection if not in modified state. MergeOption.OverwriteChanges
        /// Else if in modified state, then colletion is only refreshed with MergeOption.AppendOnly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityCollection"></param>
        /// <param name="entry"></param>
        public void AutoLoad<T>(ICollection<T> entityCollection, CollectionEntry entry) where T : class
        {
            try
            {
                AutoRefresh<T>(entityCollection, entry);
                // CurrentValue mustn't be set to null! If CurrentValue is set to null all existing Entries in this Collection are automatically set to deleted state.
                //entry.CurrentValue = null;
                // IsLoaded must be set to null to force entity framework to make a SQL-Select to the Server to load the data.
                // New records will be added as new entries
                // Existing entities will not be automatically refreshed if record was modified in the background
                entry.IsLoaded = false;
                entry.Load();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACObjectContextHelper", "AutoLoad<T>", msg);
            }
        }


        public void ParseException(MsgWithDetails msg, Exception e)
        {
            ParseExceptionStatic(msg, e);
        }

        public static bool IsDisconnectedException(Exception e)
        {
            bool isDisconnectedException = (e is DbUpdateException && !(e is DbUpdateConcurrencyException))
                                            && e.HResult == Const.EF_HResult_EntityException
                                            && e.Message.IndexOf("Open", 0, StringComparison.OrdinalIgnoreCase) >= 0;

            if (isDisconnectedException)
                return isDisconnectedException;

            isDisconnectedException = e is InvalidOperationException
                                            && e.HResult == Const.EF_HResult_InvalidOperationException
                                            && e.Message.IndexOf("not closed", 0, StringComparison.OrdinalIgnoreCase) >= 0;
            if (isDisconnectedException)
                return isDisconnectedException;

            isDisconnectedException = e.InnerException != null
                                    && (   (e.InnerException is InvalidOperationException && e.InnerException.HResult == Const.EF_HResult_InvalidOperationException)
                                        || (e.InnerException is Microsoft.Data.SqlClient.SqlException && e.InnerException.HResult == Const.EF_HResult_SqlException));
            return isDisconnectedException;
        }

        public static bool IsDisconnectedException(MsgWithDetails msg)
        {
            if (msg == null)
                return false;
            bool isDisconnectedException = msg.MsgDetails.Where(c =>   c.Row == Const.EF_HResult_EntityException 
                                                                    && c.Message.IndexOf("Open", 0, StringComparison.OrdinalIgnoreCase) >= 0).Any();
            if (isDisconnectedException)
                return isDisconnectedException;

            isDisconnectedException = msg.MsgDetails.Where(c =>    c.Row == Const.EF_HResult_InvalidOperationException
                                                                && c.Message.IndexOf("not closed", 0, StringComparison.OrdinalIgnoreCase) >= 0).Any();
            if (isDisconnectedException)
                return isDisconnectedException;

            isDisconnectedException = msg.MsgDetails.Where(c => c.Row == Const.EF_HResult_EntityException).Any()
                                   && msg.MsgDetails.Where(c => c.Row == Const.EF_HResult_InvalidOperationException || c.Row == Const.EF_HResult_SqlException).Any();
            return isDisconnectedException;
        }

        public static void ParseExceptionStatic(MsgWithDetails msg, Exception e)
        {
            bool isDisconnectedException = IsDisconnectedException(e);

            string constraint = "";
            if (e.InnerException != null)
            {
                int pos1 = -1;
                int pos2 = -1;
                string message = e.InnerException.Message;
                pos1 = message.IndexOf("'FK_");
                if (pos1 != -1)
                {
                    pos2 = message.Substring(pos1 + 1).IndexOf("'");
                    constraint = message.Substring(pos1 + 1, pos2);
                }
                if (string.IsNullOrEmpty(constraint))
                {
                    pos1 = message.IndexOf("'PK_");
                    if (pos1 != -1)
                    {
                        pos2 = message.Substring(pos1 + 1).IndexOf("'");
                        constraint = message.Substring(pos1 + 1, pos2);
                    }
                }
                if (string.IsNullOrEmpty(constraint))
                {
                    pos1 = message.IndexOf("'UIX_");
                    if (pos1 != -1)
                    {
                        pos2 = message.Substring(pos1 + 1).IndexOf("'");
                        constraint = message.Substring(pos1 + 1, pos2);
                    }
                }
            }

            int exceptionType = e.HResult;
            if (exceptionType == 0)
            {
                if (e is DbUpdateException && !(e is DbUpdateConcurrencyException))
                    exceptionType = Const.EF_HResult_EntityException;
                else if (e is InvalidOperationException)
                    exceptionType = Const.EF_HResult_InvalidOperationException;
                else if (e is Microsoft.Data.SqlClient.SqlException)
                    exceptionType = Const.EF_HResult_SqlException;
            }

            msg.AddDetailMessage(new Msg { ACIdentifier = "Error", Message = e.Message, Row = exceptionType });
            if (e.InnerException != null)
                msg.AddDetailMessage(new Msg { ACIdentifier = "Error", Message = e.InnerException.Message, Row = e.InnerException.HResult });

            //DbUpdateConcurrencyException: Store update, insert, or delete statement affected an unexpected number of rows(0). Entities may have been modified or deleted since entities were loaded.Refresh ObjectStateManager entries.
            if (e is DbUpdateConcurrencyException)
            {
                //Error00000: The changes could not be saved because at the same time another process or user changed the same data and stored it in the database. Please close all tabs or update the relevant database objects and redo the transaction.
                //Error00000: Die Änderungen konnten nicht gespeichert werden, weil gleichzeitig ein anderer Prozess oder Benutzer die selben Daten verändert und in der Datenbank gespeichert hat. Bitte schließen Sie alle Registerkarten oder aktualisieren Sie die entsprechenden Datenbankobjekte und führen die Transaktion erneut durch.
                msg.AddDetailMessage(new Msg
                {
                    ACIdentifier = "Error",
                    Message = Database.Root.Environment.TranslateMessage(Database.GlobalDatabase, "Error00000"),
                    MessageLevel = eMsgLevel.Error
               
                });
            }
            if (!string.IsNullOrEmpty(constraint) && !isDisconnectedException)
            {
                string[] parts = constraint.Split('_');
                string key = "";
                string table = Database.GlobalDatabase.IsChanged ? "Unknown" : Database.Root.Environment.TranslateText(Database.GlobalDatabase, "Unknown");
                string column = table;
                switch (parts[0])
                {
                    case "FK":
                        key = Database.GlobalDatabase.IsChanged ? "Foreign Key" : Database.Root.Environment.TranslateText(Database.GlobalDatabase, "Foreign Key");
                        break;
                    case "PK":
                        key = Database.GlobalDatabase.IsChanged ? "Primary Key" : Database.Root.Environment.TranslateText(Database.GlobalDatabase, "Primary Key");
                        break;
                    case "UIX":
                        key = Database.GlobalDatabase.IsChanged ? "Unique Index" : Database.Root.Environment.TranslateText(Database.GlobalDatabase, "Unique Index");
                        break;
                    default:
                        key = Database.GlobalDatabase.IsChanged ? "Unknown Constraint" : Database.Root.Environment.TranslateText(Database.GlobalDatabase, "Unknown Constraint");
                        break;
                }
                if (parts.Any())
                {

                    using (ACMonitor.Lock(Database.Root.Database.ContextIPlus.QueryLock_1X000))
                    {
                        var query = ((ACClass)Database.Root.Database.ContextIPlus.ACType).ACClass_ParentACClass.Where(c => c.ACIdentifier == parts[1]);
                        if (query.Any())
                        {
                            var acClass = query.First();
                            table = acClass.ACCaption;

                            // Corrected to '> 2' (was '> 1')
                            if (parts.Count() > 2)
                            {
                                string columnInfo = parts[2];
                                if (columnInfo.EndsWith("ID"))
                                    columnInfo = columnInfo.Substring(0, columnInfo.Length - 2);
                                var query2 = acClass.ACClassProperty_ACClass.Where(c => c.ACIdentifier == columnInfo);
                                if (query2.Any())
                                {
                                    column = query2.First().ACCaption;
                                }
                                else
                                {
                                    column = parts[2];
                                }
                            }
                        }
                        else
                        {
                            table = parts[1];
                            if (parts.Count() > 2)
                                column = parts[2];
                        }
                    }
                }

                msg.AddDetailMessage(new Msg
                {
                    ACIdentifier = "Error",
                    Message = Database.Root.Environment.TranslateMessage(Database.GlobalDatabase, "Error00002", key, table, column),
                    MessageLevel = eMsgLevel.Error
                });
                if (e.InnerException != null)
                {
                    msg.AddDetailMessage(new Msg { ACIdentifier = "Error", Message = e.InnerException.Message });
                }
            }
        }

        /// <summary>
        /// UNSAFE. Use QueryLock_1X000 outside
        /// </summary>
        /// <returns></returns>
        public void DetachAllEntities()
        {
            foreach (EntityEntry objectStateEntry in this.ObjectContext.ChangeTracker.Entries().Where(c => c.State == EntityState.Unchanged).ToArray())
            {
                objectStateEntry.State = EntityState.Detached;
                InformVBEntityObjectOnDetach(objectStateEntry.Entity);
            }
        }

        public void Detach(object entity)
        {
            ObjectContext.Entry(entity).State = EntityState.Detached;
            InformVBEntityObjectOnDetach(entity);
        }

        private void InformVBEntityObjectOnDetach(object entity)
        {
            VBEntityObject entityObj = entity as VBEntityObject;
            if (entityObj != null)
                entityObj.OnDetached();
        }

        public void SetIsolationLevelUncommitedRead()
        {
            ObjectContext.Database.ExecuteSql(SET_READ_UNCOMMITED);
        }

        public void SetIsolationLevelCommitedRead()
        {
            ObjectContext.Database.ExecuteSql(SET_READ_COMMITED);
        }

        public string GetQualifiedEntitySetNameForEntityKey(string entitySetName)
        {
            return EntityKey.GetQualifiedEntitySetNameForEntityKey(entitySetName, ObjectContext.GetType().AssemblyQualifiedName);
        }

        public string DefaultContainerName
        {
            get
            {
                string containerName, className;
                EntityKey.ExtractContainerNameFromAQN(ObjectContext.GetType().AssemblyQualifiedName, out className, out containerName);
                return containerName;
            }
        }
        #endregion

        #region Critical Section
        public void EnterCS()
        {
            EnterCS(false);
        }

        public void EnterCS(bool DeactivateEntityCheck)
        {
            ACMonitor.Enter(_ObjectContext.QueryLock_1X000);
            if (!_DeactivateEntityCheck && DeactivateEntityCheck)
                _ObjectContext.SavingChanges -= Database_SavingChanges;
            _DeactivateEntityCheck = DeactivateEntityCheck;
        }

        public void LeaveCS()
        {
            if (_DeactivateEntityCheck)
                _ObjectContext.SavingChanges += Database_SavingChanges;
            ACMonitor.Exit(_ObjectContext.QueryLock_1X000);
        }
#endregion

#region ACUrl
        public object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            //ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            //string filter = "ACProjectName == @0";
            //var query = ACProject.Where(filter, new object[] { "Produktion" });
            //return null;


            if (string.IsNullOrEmpty(acUrl))
                return null;

            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    return Database.Root.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                //if (entity.ParentACObject == null)
                //    return _ObjectContext.ACUrlCommand(entity, acUrlHelper.NextACUrl, acParameter); ;
                //return entity.ParentACObject.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                case ACUrlHelper.UrlKeys.Child:
                    {
                        string acName = ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart);
                        object[] filterValues = ACUrlHelper.GetFilterValues(acUrlHelper.ACUrlPart);

                        Type dbType = _ObjectContext.GetType();
                        PropertyInfo piEntity = dbType.GetProperty(acName);
                        if (piEntity == null)
                            return null;
                        Type[] genericArguments = piEntity.PropertyType.GetGenericArguments();
                        Type entityType = piEntity.PropertyType;
                        if (genericArguments != null && genericArguments.Count() > 0)
                            entityType = piEntity.PropertyType.GetGenericArguments()[0];

                        PropertyInfo piDataIdentifier = entityType.GetProperty("KeyACIdentifier");
                        if (piDataIdentifier == null)
                        {
                            if (_ObjectContext != _ObjectContext.ContextIPlus)
                            {
                                return _ObjectContext.ContextIPlus.ACUrlCommand(acUrl, acParameter);
                            }
                            return null;
                        }

                        if (filterValues != null)
                        {
                            string[] filterColumns = ((string)piDataIdentifier.GetValue(null, null)).Split(',');

                            if (filterValues.Count() != filterColumns.Count())
                                return null;
                            string filter = "";
                            int i = 0;
                            foreach (var filterColumn in filterColumns)
                            {
                                if (!string.IsNullOrEmpty(filter))
                                    filter += " && ";
                                filter += filterColumn + " == @" + i.ToString();
                                i++;
                            }


                            using (ACMonitor.Lock(_ObjectContext.QueryLock_1X000))
                            {
                                //No replacement for ObjectQuery, used IQueryable instead
                                IQueryable objectQuery = (IQueryable)piEntity.GetValue(_ObjectContext, null);
                                var resultQuery = objectQuery.Where(filter, filterValues);
                                foreach (object resultObject in resultQuery)
                                {
                                    if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                    {
                                        return resultObject;
                                    }
                                    else
                                    {
                                        return ((IACObject)resultObject).ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                                    }
                                }
                            }
                        }
                        else if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                        {
                            return (IQueryable)piEntity.GetValue(_ObjectContext, null);
                        }
                        return null;
                    }
                case ACUrlHelper.UrlKeys.Parent:
                    return null;
                case ACUrlHelper.UrlKeys.Start:
                    try
                    {
                        // Neue Datenbankobjekte können nicht für Unterentitäten angelegt werden
                        if (!string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                            return null;
                        string acName = ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart);

                        Type dbType = _ObjectContext.GetType();
                        PropertyInfo piEntity = dbType.GetProperty(acName);
                        if (piEntity == null)
                            return null;

                        Type entityType = piEntity.PropertyType.GetGenericArguments()[0];

                        MethodInfo miNewACObject = entityType.GetMethod(Const.MN_NewACObject, Global.bfInvokeMethodStatic);
                        // Erzeugen einer neuen Datenbankentität. ParentACObject ist immer null

                        // fix to operate with NewACObject method with optional params
                        object[] newACobjectMethodParams = new object[miNewACObject.GetParameters().Count()];
                        newACobjectMethodParams[0] = _ObjectContext;
                        newACobjectMethodParams[1] = null;
                        for (int i = 2; i < miNewACObject.GetParameters().Count(); i++)
                        {
                            if (miNewACObject.GetParameters()[i].IsOptional)
                                newACobjectMethodParams[i] = Type.Missing;
                        }

                        IACObject newACObject = miNewACObject.Invoke(entityType, newACobjectMethodParams) as IACObject;
                        _ObjectContext.Add(newACObject);

                        if (acParameter != null && acParameter.Any())
                        {
                            Type t = newACObject.GetType();
                            PropertyInfo pi2 = t.GetProperty("KeyACIdentifier", BindingFlags.Static | BindingFlags.Public);
                            string keyACIdentifier = pi2.GetValue(null, null) as string;
                            PropertyInfo pi3 = t.GetProperty(keyACIdentifier);
                            pi3.SetValue(newACObject, acParameter[0] as string, null);
                        }
                        return newACObject;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACObjectContextHelper", "ACUrlCommand", msg);
                        return null;
                    }
                case ACUrlHelper.UrlKeys.Stop:
                    {
                        return null;
                    }
                default:
                    return null; // TODO: Fehlerbehandlung
            }
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            if (string.IsNullOrEmpty(acUrl))
                return false;
            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    return false;
                case ACUrlHelper.UrlKeys.Child:
                    {
                        acTypeInfo = _ObjectContext.ACType;
                        if (acUrlHelper.ACUrlPart == Const.ContextIPlus)
                        {
                            IACType memberTypeInfo = acTypeInfo.GetMember(acUrlHelper.ACUrlPart);
                            if (memberTypeInfo != null)
                            {
                                acTypeInfo = memberTypeInfo;
                                path += "." + acUrlHelper.ACUrlPart;
                                return _ObjectContext.ContextIPlus.ACUrlBinding(acUrlHelper.NextACUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                            }
                        }

                        object[] filterValues = ACUrlHelper.GetFilterValues(acUrlHelper.ACUrlPart);
                        // Falls Database-ACRUL: Database\\ACProject(...)\\ACClass(...)\\...
                        if (filterValues != null)
                        {
                            string acName = ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart);
                            Type dbType = _ObjectContext.GetType();
                            PropertyInfo piEntity = dbType.GetProperty(acName);
                            if (piEntity == null)
                                return false;
                            Type entityType = piEntity.PropertyType.GetGenericArguments()[0];
                            PropertyInfo piDataIdentifier = entityType.GetProperty("KeyACIdentifier");
                            string[] filterColumns = ((string)piDataIdentifier.GetValue(null, null)).Split(',');

                            if (filterValues.Count() != filterColumns.Count())
                                return false;
                            string filter = "";
                            int i = 0;
                            foreach (var filterColumn in filterColumns)
                            {
                                if (!string.IsNullOrEmpty(filter))
                                    filter += " && ";
                                filter += filterColumn + " == @" + i.ToString();
                                i++;
                            }

                            using (ACMonitor.Lock(_ObjectContext.QueryLock_1X000))
                            {
#if !EFCR
#endif
                                //No replacement for ObjectQuery, used IQueryable instead
                                IQueryable objectQuery = (IQueryable)piEntity.GetValue(_ObjectContext, null);
                                var resultQuery = objectQuery.Where(filter, filterValues);
                                foreach (object resultObject in resultQuery)
                                {
                                    if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                    {
                                        source = resultObject;
                                        path = "";
                                        rightControlMode = Global.ControlModes.Enabled;
                                        return true;
                                    }
                                    else
                                    {
                                        return ((IACObject)resultObject).ACUrlBinding(acUrlHelper.NextACUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string[] parts;
                            if (acUrl.IndexOf('.') >= 0)
                                acUrl = acUrl.Replace('.', '\\');
                            parts = acUrl.Split('\\');

                            object loopValueOfDeclaringType = _ObjectContext;
                            Type loopDeclaringType = loopValueOfDeclaringType.GetType();
                            IACType loopDeclaringACType = acTypeInfo;
                            string acURLtoDeclaringType = "";
                            string acUrlRestFromDeclaringType = acUrl;

                            Type memberType = loopDeclaringType; //.NET-Type
                            IACType memberACType = loopDeclaringACType; // AC-Type
                            object memberValue = loopValueOfDeclaringType;

                            int i = 0;
                            foreach (string part in parts)
                            {
                                loopDeclaringType = memberType;
                                loopDeclaringACType = memberACType;
                                loopValueOfDeclaringType = memberValue;

                                // Bilde URL bis zum letzten Objekt das existiert
                                if (loopValueOfDeclaringType != null && (i > 0))
                                {
                                    if (i == 1)
                                        acURLtoDeclaringType = part;
                                    else
                                        acURLtoDeclaringType += "\\" + part;
                                    acUrlRestFromDeclaringType = new ACUrlHelper(acURLtoDeclaringType).NextACUrl;

                                    IACObject iACObject = loopValueOfDeclaringType as IACObject;
                                    // Falls Child-Object ein IACObject ist, dann kann Childobjekt das ACURLBinding selbst weiter Auflösen
                                    if (iACObject != null && !String.IsNullOrEmpty(acUrlRestFromDeclaringType))
                                    {
                                        acTypeInfo = loopDeclaringACType;
                                        source = iACObject;
                                        path = "";
                                        return iACObject.ACUrlBinding(acUrlRestFromDeclaringType, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                                    }
                                }

                                if (loopDeclaringType == null)
                                    return false;
                                PropertyInfo pi = loopDeclaringType.GetProperty(part);
                                if (pi == null)
                                    return false;

                                // Hole ACType des Members
                                if (loopDeclaringACType != null)
                                    memberACType = loopDeclaringACType.GetMember(part);
                                // Ermittle .NET-Typ des Members
                                if (memberACType == null)
                                    memberType = pi.PropertyType;
                                else
                                    memberType = memberACType.ObjectType;

                                // Hole aktuellen Wert des Members
                                memberValue = null;
                                if (loopValueOfDeclaringType != null && loopValueOfDeclaringType != null)
                                    memberValue = pi.GetValue(loopValueOfDeclaringType, null);

                                // Member nicht mit ACPropertyInfo bekanntgegeben oder 
                                // für dieses Objekt, das im Parent-Objekt seiner Property referenziert hatte keine ACTypeInfo 
                                if (memberACType == null)
                                {
                                    rightControlMode = Global.ControlModes.Enabled;

                                    using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                                    {
                                        memberACType = gip.core.datamodel.Database.GlobalDatabase.GetACType(memberType);
                                        if (memberACType == null && memberType.IsGenericType)
                                            memberACType = gip.core.datamodel.Database.GlobalDatabase.GetACType(memberType.GetGenericArguments()[0]);
                                    }
                                }

                                if (memberType == null)
                                    return false;
                                i++;
                            }

                            if (loopValueOfDeclaringType != null)
                            {
                                source = loopValueOfDeclaringType;
                                path = acUrlRestFromDeclaringType;
                            }
                            else
                            {
                                source = _ObjectContext;
                                path = acUrl;
                            }
                            if (!String.IsNullOrEmpty(path))
                            {
                                if (path.IndexOf('\\') >= 0)
                                    path = path.Replace('\\', '.');
                            }

                            if (memberACType == null)
                                return false;
                            acTypeInfo = memberACType;

                            ACClass acClassRight = null;
                            ACClassProperty acProp = acTypeInfo as ACClassProperty;
                            bool isNullable = true;
                            if (acProp != null)
                            {
                                acClassRight = acProp.Safe_ACClass;
                                isNullable = acProp.IsNullable;
                            }
                            else if (acTypeInfo is ACClassMethod)
                                acClassRight = (acTypeInfo as ACClassMethod).Safe_ACClass;
                            else
                                return false;
                            rightControlMode = acClassRight.RightManager.GetControlMode(acTypeInfo);
                            if (rightControlMode == Global.ControlModes.Enabled)
                            {
                                if (!isNullable)
                                    rightControlMode = Global.ControlModes.EnabledRequired;
                            }
                            return true;
                        }
                        return false;
                    }
                default:
                    return false; // TODO: Fehlerbehandlung
            }
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            if (string.IsNullOrEmpty(acUrl) || acUrlTypeInfo == null)
                return false;
            IACType acTypeInfo = null;
            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    return false;
                case ACUrlHelper.UrlKeys.Child:
                    {
                        acTypeInfo = _ObjectContext.ACType;
                        acUrlTypeInfo.AddSegment(_ObjectContext.GetACUrl(), acTypeInfo, _ObjectContext, Global.ControlModes.Enabled);
                        if (acUrlHelper.ACUrlPart == Const.ContextIPlus)
                        {
                            IACType memberTypeInfo = acTypeInfo.GetMember(acUrlHelper.ACUrlPart);
                            if (memberTypeInfo != null)
                            {
                                acTypeInfo = memberTypeInfo;
                                acUrlTypeInfo.SubPath += "." + acUrlHelper.ACUrlPart;
                                return _ObjectContext.ContextIPlus.ACUrlTypeInfo(acUrlHelper.NextACUrl, ref acUrlTypeInfo);
                            }
                        }

                        object[] filterValues = ACUrlHelper.GetFilterValues(acUrlHelper.ACUrlPart);
                        // Falls Database-ACRUL: Database\\ACProject(...)\\ACClass(...)\\...
                        if (filterValues != null)
                        {
                            string acName = ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart);
                            Type dbType = _ObjectContext.GetType();
                            PropertyInfo piEntity = dbType.GetProperty(acName);
                            if (piEntity == null)
                                return false;
                            Type entityType = piEntity.PropertyType.GetGenericArguments()[0];
                            PropertyInfo piDataIdentifier = entityType.GetProperty("KeyACIdentifier");
                            string[] filterColumns = ((string)piDataIdentifier.GetValue(null, null)).Split(',');

                            if (filterValues.Count() != filterColumns.Count())
                                return false;
                            string filter = "";
                            int i = 0;
                            foreach (var filterColumn in filterColumns)
                            {
                                if (!string.IsNullOrEmpty(filter))
                                    filter += " && ";
                                filter += filterColumn + " == @" + i.ToString();
                                i++;
                            }

                            using (ACMonitor.Lock(_ObjectContext.QueryLock_1X000))
                            {
                                //No replacement for ObjectQuery, used IQueryable instead
                                IQueryable objectQuery = (IQueryable)piEntity.GetValue(_ObjectContext, null);
                                var resultQuery = objectQuery.Where(filter, filterValues);
                                foreach (object resultObject in resultQuery)
                                {
                                    if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                    {
                                        acUrlTypeInfo.AddSegment(_ObjectContext.GetACUrl() + ACUrlHelper.Delimiter_DirSeperator + acUrlHelper.ACUrlPart, acTypeInfo, resultObject, Global.ControlModes.Enabled);
                                        return true;
                                    }
                                    else
                                    {
                                        return ((IACObject)resultObject).ACUrlTypeInfo(acUrlHelper.NextACUrl, ref acUrlTypeInfo);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string baseUrl = _ObjectContext.GetACUrl();

                            string[] parts;
                            if (acUrl.IndexOf('.') >= 0)
                                acUrl = acUrl.Replace('.', '\\');
                            parts = acUrl.Split('\\');

                            object loopValueOfDeclaringType = _ObjectContext;
                            Type loopDeclaringType = loopValueOfDeclaringType.GetType();
                            IACType loopDeclaringACType = acTypeInfo;
                            string acURLtoDeclaringType = "";
                            string acUrlRestFromDeclaringType = acUrl;

                            Type memberType = loopDeclaringType; //.NET-Type
                            IACType memberACType = loopDeclaringACType; // AC-Type
                            object memberValue = loopValueOfDeclaringType;

                            int i = 0;
                            foreach (string part in parts)
                            {
                                baseUrl += ACUrlHelper.Delimiter_DirSeperator + part;
                                loopDeclaringType = memberType;
                                loopDeclaringACType = memberACType;
                                loopValueOfDeclaringType = memberValue;

                                // Bilde URL bis zum letzten Objekt das existiert
                                if (loopValueOfDeclaringType != null && (i > 0))
                                {
                                    if (i == 1)
                                        acURLtoDeclaringType = part;
                                    else
                                        acURLtoDeclaringType += "\\" + part;
                                    acUrlRestFromDeclaringType = new ACUrlHelper(acURLtoDeclaringType).NextACUrl;

                                    IACObject iACObject = loopValueOfDeclaringType as IACObject;
                                    // Falls Child-Object ein IACObject ist, dann kann Childobjekt das ACURLBinding selbst weiter Auflösen
                                    if (iACObject != null && !String.IsNullOrEmpty(acUrlRestFromDeclaringType))
                                    {
                                        acTypeInfo = loopDeclaringACType;
                                        //source = iACObject;
                                        //path = "";
                                        return iACObject.ACUrlTypeInfo(acUrlHelper.NextACUrl, ref acUrlTypeInfo);

                                    }
                                }

                                if (loopDeclaringType == null)
                                    return false;
                                PropertyInfo pi = loopDeclaringType.GetProperty(part);
                                if (pi == null)
                                    return false;

                                // Hole ACType des Members
                                if (loopDeclaringACType != null)
                                    memberACType = loopDeclaringACType.GetMember(part);
                                // Ermittle .NET-Typ des Members
                                if (memberACType == null)
                                    memberType = pi.PropertyType;
                                else
                                    memberType = memberACType.ObjectType;

                                // Hole aktuellen Wert des Members
                                memberValue = null;
                                if (loopValueOfDeclaringType != null && loopValueOfDeclaringType != null)
                                    memberValue = pi.GetValue(loopValueOfDeclaringType, null);

                                // Member nicht mit ACPropertyInfo bekanntgegeben oder 
                                // für dieses Objekt, das im Parent-Objekt seiner Property referenziert hatte keine ACTypeInfo 
                                if (memberACType == null)
                                {
                                    using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                                    {
                                        memberACType = gip.core.datamodel.Database.GlobalDatabase.GetACType(memberType);
                                        if (memberACType == null && memberType.IsGenericType)
                                            memberACType = gip.core.datamodel.Database.GlobalDatabase.GetACType(memberType.GetGenericArguments()[0]);
                                    }
                                }

                                if (memberType == null)
                                    return false;

                                acUrlTypeInfo.AddSegment(_ObjectContext.GetACUrl() + ACUrlHelper.Delimiter_DirSeperator + acUrlHelper.ACUrlPart, memberACType, memberValue, Global.ControlModes.Enabled);
                                i++;
                            }

                            var lastSegement = acUrlTypeInfo.LastOrDefault();
                            if (loopValueOfDeclaringType != null)
                            {
                                //source = loopValueOfDeclaringType;
                                //path = acUrlRestFromDeclaringType;
                                if (lastSegement != null)
                                {
                                    lastSegement.Value = loopValueOfDeclaringType;
                                }
                            }
                            else
                            {
                                //source = _ObjectContext;
                                //path = acUrl;
                            }
                            //if (!String.IsNullOrEmpty(path))
                            //{
                            //    if (path.IndexOf('\\') >= 0)
                            //        path = path.Replace('\\', '.');
                            //}

                            if (memberACType == null)
                                return false;
                            acTypeInfo = memberACType;

                            ACClass acClassRight = null;
                            ACClassProperty acProp = acTypeInfo as ACClassProperty;
                            bool isNullable = true;
                            if (acProp != null)
                            {
                                acClassRight = acProp.Safe_ACClass;
                                isNullable = acProp.IsNullable;
                            }
                            else if (acTypeInfo is ACClassMethod)
                                acClassRight = (acTypeInfo as ACClassMethod).Safe_ACClass;
                            else
                                return false;
                            if (lastSegement != null)
                            {
                                lastSegement.RightControlMode = acClassRight.RightManager.GetControlMode(acTypeInfo);
                                if (lastSegement.RightControlMode == Global.ControlModes.Enabled)
                                {
                                    if (!isNullable)
                                        lastSegement.RightControlMode = Global.ControlModes.EnabledRequired;
                                }
                            }
                            return true;
                        }
                        return false;
                    }
                default:
                    return false; // TODO: Fehlerbehandlung
            }
        }
        #endregion

#endregion

#region Event-Handler

        void Database_SavingChanges(object sender, System.EventArgs e)
        {
            if (_IsSaving)
                return;
            _IsSaving = true;

            IList<Msg> resultList = null;

            _IsSaving = false;

            if (resultList != null)
            {
                throw new EntityCheckException("", resultList);
            }
        }

        void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if ((e.CurrentState == ConnectionState.Broken) || (e.CurrentState == ConnectionState.Closed))
            {
                if ((e.OriginalState == ConnectionState.Executing) || (e.OriginalState == ConnectionState.Fetching) || (e.OriginalState == ConnectionState.Open))
                {
#if DEBUG
                    _DisconnCounter++;
                    //if (sender is EntityConnection)
                    //    System.Diagnostics.Debug.WriteLine("EntityConnection:" + _DisconnCounter.ToString());
                    //else
                    //    System.Diagnostics.Debug.WriteLine("SQLConnection:" + _DisconnCounter.ToString());
                    if (e.CurrentState == ConnectionState.Broken)
                    {
                        if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null)
                        {
                            gip.core.datamodel.Database.Root.Messages.LogFailure("ACObjectContextHelper.Connection_StateChange", "", "Connection Broken");
                        }
                    }
#endif
                }
            }
        }

#endregion
    }
}

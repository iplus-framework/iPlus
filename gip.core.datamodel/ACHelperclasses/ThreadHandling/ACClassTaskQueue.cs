// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="VarioBatchDbOpQueue.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
#if !EFCR
using System.Data.Objects;
using System.Data.EntityClient;
#endif

namespace gip.core.datamodel
{
    /// <summary>
    /// Class RootDbOpQueue
    /// </summary>
    public class ACClassTaskQueue : ACEntityOpQueue<Database>
    {
        public const string ACClassTaskQueuePropName = "ACClassTaskQueue";

        internal ACClassTaskQueue()
            : base(RootDbOpQueue.ClassName + "." + ACClassTaskQueuePropName, new Database(), true, true)
        {
            _ProgramCache = new ACProgramCache(this);
        }

        #region Precompiled Queries
#if !EFCR
        public static readonly Func<Database, Guid, ACProgram> s_cQry_ACProgram =
            CompiledQuery.Compile<Database, Guid, ACProgram>(
                (db, acProgramID) =>
                    db.ACProgram.Where(c => c.ACProgramID == acProgramID).FirstOrDefault()
            );

        public static readonly Func<Database, Guid, ACProgramLog> s_cQry_ACProgramLog =
            CompiledQuery.Compile<Database, Guid, ACProgramLog>(
                (db, acProgramLogID) =>
                    db.ACProgramLog.Where(c => c.ACProgramLogID == acProgramLogID).FirstOrDefault()
            );

        static readonly Func<Database, Guid, ACClassProperty> s_cQry_ACPropertyCache =
            CompiledQuery.Compile<Database, Guid, ACClassProperty>(
                (db, acClassPropertyID) =>
                    db.ACClassProperty.Where(c => c.ACClassPropertyID == acClassPropertyID).FirstOrDefault()
            );

        static readonly Func<Database, Guid, ACClassWF> s_cQry_ACClassWFCache =
            CompiledQuery.Compile<Database, Guid, ACClassWF>(
                (db, acClassWFID) =>
                    db.ACClassWF.Include("ACClassMethod")
                                .Include("RefPAACClass")
                                .Include("RefPAACClassMethod")
                                .Include("RefPAACClassMethod.AttachedFromACClass")
                                .Include("ACClassWF1_ParentACClassWF")
                                .Include("ACClassWF_ParentACClassWF")
                                .Include("PWACClass")
                                .Include("ACClassWFEdge_SourceACClassWF")
                                .Include("ACClassWFEdge_TargetACClassWF")
                    .Where(c => c.ACClassWFID == acClassWFID).FirstOrDefault()
            );
#endif
#endregion

#region TaskQueue
        private static readonly object _LockTaskQueue = new object();
        private static ACClassTaskQueue _TaskQueue = null;

        /// <summary>
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
        /// </summary>
        public static ACClassTaskQueue TaskQueue
        {
            get
            {
                if (_TaskQueue != null)
                    return _TaskQueue;
                lock (_LockTaskQueue)
                {
                    if (_TaskQueue == null)
                    {
                        _TaskQueue = new ACClassTaskQueue();
                    }
                }
                return _TaskQueue;
            }
        }
#endregion

#region Cache

#region Properties
        /// <summary>
        /// Gets or sets a value indicating whether [mass load property values off].
        /// </summary>
        /// <value><c>true</c> if [mass load property values off]; otherwise, <c>false</c>.</value>
        public bool MassLoadPropertyValuesOff
        {
            get;
            set;
        }

        /// <summary>
        /// The _ all property values
        /// </summary>
        private Dictionary<Guid, Dictionary<Guid, ACClassTaskValue[]>> _AllPropertyValues;
        /// <summary>
        /// Gets all property values.
        /// </summary>
        /// <value>All property values.</value>
        public Dictionary<Guid, Dictionary<Guid, ACClassTaskValue[]>> AllPropertyValues
        {
            get
            {
                if (_AllPropertyValues != null)
                    return _AllPropertyValues;
                lock ((Context.QueryLock_1X000))
                {
                    _AllPropertyValues = Context.ACClassTaskValue
                                            .Include(c => c.VBUser)
                                            .Include(c => c.ACClassProperty)
                                            .Include("ACClassTaskValuePos_ACClassTaskValue")
                                            .GroupBy(c => c.ACClassTaskID)
                                            .ToDictionary(g => g.Key,
                                                          g => g.GroupBy(t => t.ACClassPropertyID)
                                                                .ToDictionary(x => x.Key,
                                                                              x => x.ToArray()));
                    //_AllPropertyValues = Context.ACClassTaskValue.Include(c => c.VBUser).Include(c => c.ACClassProperty).ToList();
                    if (_AllPropertyValues == null)
                        _AllPropertyValues = new Dictionary<Guid, Dictionary<Guid, ACClassTaskValue[]>>();
                }
                return _AllPropertyValues;
            }
        }

        private ConcurrentDictionary<Guid, ACClassProperty> _ACPropertyTypeCache = new ConcurrentDictionary<Guid, ACClassProperty>(5, 1000);

        private ConcurrentDictionary<Guid, ACClassWF> _ACClassWFCache = new ConcurrentDictionary<Guid, ACClassWF>(5, 1000);

        private ACProgramCache _ProgramCache;
        public ACProgramCache ProgramCache
        {
            get
            {
                return _ProgramCache;
            }
        }

#endregion

#region Methods
        public ACClassTaskValue GetFromAllPropValues(Guid acClassTaskID, Guid acClassPropertyID, Guid? vbUserID)
        {
            if (AllPropertyValues == null)
                return null;
            Dictionary<Guid, ACClassTaskValue[]> propertyValues = null;
            if (_AllPropertyValues.TryGetValue(acClassTaskID, out propertyValues))
            {
                ACClassTaskValue[] values = null;
                if (propertyValues.TryGetValue(acClassPropertyID, out values))
                {
                    if (vbUserID.HasValue)
                        return values.Where(c => c.VBUserID.HasValue && c.VBUserID == vbUserID.Value).FirstOrDefault();
                    else
                        return values.FirstOrDefault();
                }
            }
            return null;
        }


        public ACClass GetACClassFromTaskQueueCache(Guid acClassID)
        {
            return TaskQueue.Context.GetACType(acClassID);
        }


        public ACClassProperty GetACClassPropertyFromTaskQueueCache(Guid acClassPropertyID)
        {
            ACClassProperty acClassProperty = null;
            if (!_ACPropertyTypeCache.TryGetValue(acClassPropertyID, out acClassProperty))
            {
                using (ACMonitor.Lock(TaskQueue.Context.QueryLock_1X000))
                {
#if !EFCR
                    acClassProperty = s_cQry_ACPropertyCache(TaskQueue.Context, acClassPropertyID);
#endif
                }
                if (acClassProperty != null)
                    _ACPropertyTypeCache.TryAdd(acClassPropertyID, acClassProperty);
            }
            return acClassProperty;
        }

        public ACClassWF GetACClassWFFromTaskQueueCache(Guid acClassWFID)
        {
            ACClassWF acClassWF= null;
            if (!_ACClassWFCache.TryGetValue(acClassWFID, out acClassWF))
            {
                using (ACMonitor.Lock(TaskQueue.Context.QueryLock_1X000))
                {
#if !EFCR
                    acClassWF = s_cQry_ACClassWFCache(TaskQueue.Context, acClassWFID);
#endif
                }
                if (acClassWF != null)
                    _ACClassWFCache.TryAdd(acClassWFID, acClassWF);
            }
            return acClassWF;
        }
#endregion

#endregion

    }

    public class ACProgramCache
    {
        internal ACProgramCache(ACClassTaskQueue taskQueue)
        {
            _TaskQueue = taskQueue;
        }

#region precompiled Queries
#if !EFCR
        public static readonly Func<Database, Guid, IQueryable<ACProgramLog>> s_cQry_LatestProgramLogs =
            CompiledQuery.Compile<Database, Guid, IQueryable<ACProgramLog>>(
                (db, acProgramID) =>
                    db.ACProgramLog
                    .Include("ACProgram")
                    .Include("ACProgramLog1_ParentACProgramLog")
                    .Where(c => c.ACProgramID == acProgramID)
                    .GroupBy(c => c.ACUrl)
                    .Select(c => new { ACUrl = c.Key, InsertDate = c.Max(d => d.InsertDate) })
                    .Join(db.ACProgramLog, ot => new { ot.ACUrl, ot.InsertDate }, inn => new { inn.ACUrl, inn.InsertDate }, (ot, inn) => new { inn = inn })
                    .Select(c => c.inn)
            );

        public static readonly Func<Database, Guid, string, ACProgramLog> s_cQry_LatestProgramLogByParent =
             CompiledQuery.Compile<Database, Guid, string, ACProgramLog>(
                (db, parentACProgramLogID, acUrl) =>
                    db.ACProgramLog
                    .Include("ACProgram")
                    .Include("ACProgramLog1_ParentACProgramLog")
                    .Where(c => c.ParentACProgramLogID == parentACProgramLogID && c.ACUrl == acUrl)
                    .OrderByDescending(c => c.InsertDate)
                .FirstOrDefault()
            );


        public static readonly Func<Database, Guid, string, ACProgramLog> s_cQry_LatestProgramLogByProgramID =
             CompiledQuery.Compile<Database, Guid, string, ACProgramLog>(
                (db, acProgramID, acUrl) =>
                    db.ACProgramLog
                    .Include("ACProgram")
                    .Include("ACProgramLog1_ParentACProgramLog")
                    .Where(c => c.ACProgramID == acProgramID && c.ACUrl == acUrl)
                    .OrderByDescending(c => c.InsertDate)
                .FirstOrDefault()
            );

        public static readonly Func<Database, Guid, ACProgramLog> s_cQry_ProgramLogByLogID =
             CompiledQuery.Compile<Database, Guid, ACProgramLog>(
                (db, programLogID) =>
                    db.ACProgramLog
                    .Include("ACProgram")
                    .Include("ACProgramLog1_ParentACProgramLog")
                    .Where(c => c.ACProgramLogID == programLogID)
                    .OrderByDescending(c => c.InsertDate)
                .FirstOrDefault()
            );


        public static readonly Func<Database, Guid, string, IQueryable<ACProgramLog>> s_cQry_PreviousLogsFromParent =
             CompiledQuery.Compile<Database, Guid, string, IQueryable<ACProgramLog>>(
                (db, parentACProgramLogID, acUrl) =>
                    db.ACProgramLog
                    .Include("ACProgram")
                    .Include("ACProgramLog1_ParentACProgramLog")
                    .Where(c => c.ParentACProgramLogID == parentACProgramLogID && c.ACUrl == acUrl)
                    .OrderBy(c => c.InsertDate)
            );

        public static readonly Func<Database, Guid, string, IQueryable<ACProgramLog>> s_cQry_PreviousLogsFromProgram =
             CompiledQuery.Compile<Database, Guid, string, IQueryable<ACProgramLog>>(
                (db, programID, acUrl) =>
                    db.ACProgramLog
                    .Include("ACProgram")
                    .Include("ACProgramLog1_ParentACProgramLog")
                    .Where(c => c.ACProgramID == programID && c.ACUrl == acUrl)
                    .OrderBy(c => c.InsertDate)
            );
#endif
#endregion

#region Properties
        ACClassTaskQueue _TaskQueue = null;
        ConcurrentDictionary<Guid, ACProgramCacheEntry> _Programs = new ConcurrentDictionary<Guid, ACProgramCacheEntry>();
#endregion

#region Methods
        /// <summary>
        /// Reads from Cache, If not in Cache it rebuilds by querying dababase
        /// </summary>
        /// <param name="parentProgramLog">parentProgramLog</param>
        /// <param name="acUrl">acUrl</param>
        /// <param name="lookupOnlyInCache">lookupOnlyInCache</param>
        /// <param name="checkNewerThanParentProgramLog">checkNewerThanParentProgramLog</param>
        /// <returns></returns>
        public ACProgramLog GetCurrentProgramLog(ACProgramLog parentProgramLog, string acUrl, bool lookupOnlyInCache = false, bool checkNewerThanParentProgramLog = true)
        {
            ACProgramCacheEntry cacheEntry = GetCacheEntry(parentProgramLog);
            if (cacheEntry == null)
                return null;
            ACProgramLog programLog = cacheEntry.GetCurrentProgramLog(acUrl, parentProgramLog);
            if (programLog == null
                && parentProgramLog.EntityState != EntityState.Detached 
                && parentProgramLog.EntityState != EntityState.Added
                && !lookupOnlyInCache)
            {
                _TaskQueue.ProcessAction(() =>
                {
#if !EFCR
                    programLog = s_cQry_LatestProgramLogByParent(_TaskQueue.Context, parentProgramLog.ACProgramLogID, acUrl);
#endif
                    if (programLog != null && checkNewerThanParentProgramLog)
                    {
                        // Programlog was not created, it's a older one
                        if (parentProgramLog.StartDate.HasValue
                            && programLog.StartDate.HasValue)
                        {
                            if (programLog.StartDate < parentProgramLog.StartDate)
                                programLog = null;
                        }
                        // Programlog was not created, it's a older one
                        else if (programLog.InsertDate < parentProgramLog.InsertDate)
                            programLog = null;
                    }
                });
                if (programLog != null)
                    cacheEntry.ReplaceProgramLog(programLog, acUrl, false);
            }
            return programLog;
        }

        /// <summary>
        /// Reads from Cache, If not in Cache it rebuilds by querying dababase
        /// </summary>
        /// <param name="acProgram"></param>
        /// <param name="acUrl"></param>
        /// <returns></returns>
        public ACProgramLog GetCurrentProgramLog(ACProgram acProgram, string acUrl)
        {
            ACProgramCacheEntry cacheEntry = GetCacheEntry(acProgram);
            if (cacheEntry == null)
                return null;
            return cacheEntry.GetCurrentProgramLog(acUrl);
        }


        /// <summary>
        /// Reads from Cache, If not in Cache it rebuilds by querying dababase
        /// </summary>
        /// <param name="acProgramLogID"></param>
        /// <param name="autoLoadCache"></param>
        /// <returns></returns>
        public ACProgramLog GetCurrentProgramLogByLogID(Guid acProgramLogID, bool autoLoadCache = true)
        {
            ACProgramLog programLog = null;
            _TaskQueue.ProcessAction(() =>
            {
#if !EFCR
                programLog = s_cQry_ProgramLogByLogID(_TaskQueue.Context, acProgramLogID);
#endif
            });
            if (programLog == null)
                return null;

            // Load Cache
            if (autoLoadCache)
                GetCacheEntry(programLog.ACProgramID);

            return programLog;
        }

        /// <summary>
        /// Reads from Cache, If not in Cache it rebuilds by querying dababase
        /// </summary>
        /// <param name="acProgramID"></param>
        /// <returns></returns>
        public ACProgram GetProgram(Guid acProgramID)
        {
            ACProgramCacheEntry cacheEntry = GetCacheEntry(acProgramID);
            if (cacheEntry != null)
                return cacheEntry.Program;
            return null;
        }

        /// <summary>
        /// If there was an previous log, it will be replaced and the previous will be returned.
        /// Otherwise the new log will be returned.
        /// </summary>
        public ACProgramLog AddProgramLog(ACProgramLog currentProgramLog, bool autoReplaceExisting = true)
        {
            if (currentProgramLog == null 
                || String.IsNullOrEmpty(currentProgramLog.ACUrl))
                return null;

#if !DIAGNOSE
            if (currentProgramLog.EntityState == EntityState.Deleted
                || (currentProgramLog.EntityState == EntityState.Detached && currentProgramLog.NewACProgramForQueue == null))
#else
            if (   currentProgramLog.EntityState == System.Data.EntityState.Deleted
                || currentProgramLog.EntityState == System.Data.EntityState.Detached)
#endif
            {
                Database.Root.Messages.LogError("ACProgramCache", "AddProgramLog(0)", String.Format("Cant add currentProgramLog {0} because EntityState is {1}", currentProgramLog.ACProgramLogID, currentProgramLog.EntityState));
                return null;
            }

            ACProgram acProgram = null;
            if (currentProgramLog.ACProgramReference.IsLoaded)
                acProgram = (ACProgram) currentProgramLog.ACProgramReference.CurrentValue;
            if (acProgram == null)// && (currentProgramLog.EntityState == System.Data.EntityState.Added || currentProgramLog.EntityState == System.Data.EntityState.Detached))
                acProgram = currentProgramLog.NewACProgramForQueue;
            if (acProgram == null)
            {
                _TaskQueue.ProcessAction(() =>
                {
                    try
                    {
                        acProgram = currentProgramLog.ACProgram;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Database.Root.Messages.LogException("ACProgramCache", "AddProgramLog(1)", String.Format("Cant add currentProgramLog {0}, Exception {1}", currentProgramLog.ACProgramLogID, msg));
                    }
                });
            }
            if (acProgram == null)
                return null;

            ACProgramCacheEntry cacheEntry = GetCacheEntry(acProgram);
            ACProgramLog previousLog = null;
            if (cacheEntry != null)
            {
                previousLog = cacheEntry.ReplaceProgramLog(currentProgramLog, currentProgramLog.ACUrl, autoReplaceExisting);
                if (previousLog != null && previousLog != currentProgramLog)
                {
                    _TaskQueue.Add(() =>
                    {
                        try
                        {
                            if (previousLog.EntityState != EntityState.Deleted
                                && previousLog.EntityState != EntityState.Detached)
                            {
                                if (previousLog.EntityState != EntityState.Unchanged)
                                    _TaskQueue.Context.ACSaveChanges();
                                //_TaskQueue.Context.Detach(previousLog);
                                _TaskQueue.Context.Entry(previousLog).State = EntityState.Detached;
                            }
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            Database.Root.Messages.LogException("ACProgramCache", "AddProgramLog(Detach)", msg);
                        }
                    }
                    );
                }
            }
            return previousLog;
        }

        /// <summary>
        /// Removes ProgramLog from Cache and detaches the Entity
        /// </summary>
        /// <param name="currentProgramLog"></param>
        /// <returns></returns>
        public ACProgramLog RemoveProgramLog(ACProgramLog currentProgramLog)
        {
            if (currentProgramLog == null)
                return null;

            ACProgramCacheEntry cacheEntry = GetCacheEntry(currentProgramLog, false);
            if (cacheEntry == null)
                return null;

            ACProgramLog removedLog = cacheEntry.RemoveProgramLog(currentProgramLog);
            if (removedLog != null)
            {
                _TaskQueue.Add(() =>
                {
                    try
                    {
                        if (currentProgramLog.EntityState != EntityState.Deleted
                            && currentProgramLog.EntityState != EntityState.Detached)
                        {
                            if (currentProgramLog.EntityState != EntityState.Unchanged)
                                _TaskQueue.Context.ACSaveChanges();
                            //_TaskQueue.Context.Detach(currentProgramLog);
                            _TaskQueue.Context.Entry(currentProgramLog).State = EntityState.Detached ;
                        }
                        if (removedLog != currentProgramLog)
                        {
                            if (removedLog.EntityState != EntityState.Deleted
                               && removedLog.EntityState != EntityState.Detached)
                            {
                                if (removedLog.EntityState != EntityState.Unchanged)
                                    _TaskQueue.Context.ACSaveChanges();
                                //_TaskQueue.Context.Detach(removedLog);
                                _TaskQueue.Context.Entry(removedLog).State = EntityState.Detached;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Database.Root.Messages.LogException("ACProgramCache", "RemoveProgramLog(Detach)", msg);
                    }
                }
                );
            }

            return removedLog;
        }

        public ACProgramLog RemoveProgramLog(ACProgram acProgram, string acUrl)
        {
            ACProgramCacheEntry cacheEntry = GetCacheEntry(acProgram, false);
            if (cacheEntry == null)
                return null;

            ACProgramLog removedLog = cacheEntry.RemoveProgramLog(acUrl);
            if (removedLog != null)
            {
                _TaskQueue.Add(() =>
                {
                    try
                    {
                        if (removedLog.EntityState != EntityState.Deleted
                            && removedLog.EntityState != EntityState.Detached)
                        {
                            if (removedLog.EntityState != EntityState.Unchanged)
                                _TaskQueue.Context.ACSaveChanges();
                            //_TaskQueue.Context.Detach(removedLog);
                            _TaskQueue.Context.Entry(removedLog).State = EntityState.Detached;
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Database.Root.Messages.LogException("ACProgramCache", "RemoveProgramLog(Detach)", msg);
                    }
                }
                );
            }

            return removedLog;
        }

        /// <summary>
        /// Removes ACProgram from Cache and all deatches dependent Logs
        /// </summary>
        /// <param name="acProgram"></param>
        /// <returns></returns>
        public bool RemoveProgram(ACProgram acProgram)
        {
            ACProgramCacheEntry cacheEntry = null;
            if (_Programs.TryRemove(acProgram.ACProgramID, out cacheEntry))
            {
                _TaskQueue.Add(() => {
                    try
                    {
                        if (acProgram.EntityState != EntityState.Deleted
                            && acProgram.EntityState != EntityState.Detached)
                        {
                            if (acProgram.EntityState != EntityState.Unchanged)
                                _TaskQueue.Context.ACSaveChanges();
                            //_TaskQueue.Context.Detach(acProgram);
                            _TaskQueue.Context.Entry(acProgram).State = EntityState.Detached;
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Database.Root.Messages.LogException("ACProgramCache", "RemoveProgram(Detach)", msg);
                    }
                }
                );

                ACProgramLog[] logsToDetach = cacheEntry.EmptyLogCache();
                if (logsToDetach != null && logsToDetach.Any())
                {
                    _TaskQueue.Add(() =>
                    {
                        try
                        {
                            foreach (var entry in logsToDetach)
                            {

                                if (entry.ACProgramLog1_ParentACProgramLog != null
                                    && entry.ACProgramLog1_ParentACProgramLog.EntityState != EntityState.Deleted
                                    && entry.ACProgramLog1_ParentACProgramLog.EntityState != EntityState.Detached)
                                {
                                    if (entry.ACProgramLog1_ParentACProgramLog.EntityState != EntityState.Unchanged)
                                        _TaskQueue.Context.ACSaveChanges();
                                    //_TaskQueue.Context.Detach(entry.ACProgramLog1_ParentACProgramLog);
                                    _TaskQueue.Context.Entry(entry.ACProgramLog1_ParentACProgramLog).State = EntityState.Detached;
                                }
                                if (entry.EntityState != EntityState.Deleted
                                    && entry.EntityState != EntityState.Detached)
                                {
                                    if (entry.EntityState != EntityState.Unchanged)
                                        _TaskQueue.Context.ACSaveChanges();
                                    //_TaskQueue.Context.Detach(entry);
                                    _TaskQueue.Context.Entry(entry).State = EntityState.Detached;
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            Database.Root.Messages.LogException("ACProgramCache", "RemoveProgram(Detach)", msg);
                        }
                    }
                    );
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return DETACHED Entities without change tracking!
        /// </summary>
        /// <param name="parentProgramLogID"></param>
        /// <param name="acUrl"></param>
        /// <returns></returns>
        public IEnumerable<ACProgramLog> GetPreviousLogsFromParentLog(Guid parentProgramLogID, string acUrl)
        {
            using (Database db = new Database())
            {
#if !EFCR
                var query = s_cQry_PreviousLogsFromParent(db, parentProgramLogID, acUrl);
                query.SetMergeOption(MergeOption.NoTracking);
                return query.ToArray();
#endif
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Return DETACHED Entities without change tracking!
        /// </summary>
        /// <param name="programID"></param>
        /// <param name="acUrl"></param>
        /// <returns></returns>
        public IEnumerable<ACProgramLog> GetPreviousLogsFromProgram(Guid programID, string acUrl)
        {
            using (Database db = new Database())
            {
#if !EFCR
                var query = s_cQry_PreviousLogsFromProgram(db, programID, acUrl);
                query.SetMergeOption(MergeOption.NoTracking);
                return query.ToArray();
#endif
                throw new NotImplementedException();
            }
        }

        private ACProgramCacheEntry GetCacheEntry(ACProgramLog anyProgramLog, bool autoCreateIfNotExist = true)
        {
            ACProgram acProgram = null;
            if (anyProgramLog.ACProgramReference.IsLoaded)
                acProgram = (ACProgram) anyProgramLog.ACProgramReference.CurrentValue;
            if (acProgram == null) // && (anyProgramLog.EntityState == System.Data.EntityState.Added || anyProgramLog.EntityState == System.Data.EntityState.Detached))
                acProgram = anyProgramLog.NewACProgramForQueue;
            if (acProgram == null)
            {
                _TaskQueue.ProcessAction(() =>
                {
                    try
                    {
                        acProgram = anyProgramLog.ACProgram;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Database.Root.Messages.LogException("ACProgramCache", "GetCacheEntry(1)", String.Format("Cant access ACProgram at anyProgramLog {0}, Exception {1}", anyProgramLog.ACProgramLogID, msg));
                    }
                });
            }
            if (acProgram == null)
                return null;

            return GetCacheEntry(acProgram, autoCreateIfNotExist);
        }

        private ACProgramCacheEntry GetCacheEntry(Guid acProgramID, bool autoCreateIfNotExist = true)
        {
            ACProgramCacheEntry cacheEntry = null;
            if (!_Programs.TryGetValue(acProgramID, out cacheEntry) && autoCreateIfNotExist)
            {
                Dictionary<string, ACProgramLog> latestProgramLogs = null;
                _TaskQueue.ProcessAction(() =>
                {
                    try
                    {
#if !EFCR
                        latestProgramLogs = s_cQry_LatestProgramLogs(_TaskQueue.Context, acProgramID).ToDictionary(g => g.ACUrl);
#endif
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Database.Root.Messages.LogException("ACProgramCache", "GetCacheEntry(0)", String.Format("s_cQry_LatestProgramLogs for ACProgram {0} failed, Exception {1}", acProgramID, msg));
                    }
                });

                if (latestProgramLogs != null && latestProgramLogs.Any())
                {
                    cacheEntry = new ACProgramCacheEntry(latestProgramLogs.FirstOrDefault().Value.ACProgram, latestProgramLogs);
                    if (!_Programs.TryAdd(acProgramID, cacheEntry))
                        _Programs.TryGetValue(acProgramID, out cacheEntry);
                }
            }
            return cacheEntry;
        }

        private ACProgramCacheEntry GetCacheEntry(ACProgram acProgram, bool autoCreateIfNotExist = true)
        {
            ACProgramCacheEntry cacheEntry = null;
            if (acProgram == null)
                return cacheEntry;
            if (!_Programs.TryGetValue(acProgram.ACProgramID, out cacheEntry) && autoCreateIfNotExist)
            {
                Dictionary<string, ACProgramLog> latestProgramLogs = null;
                _TaskQueue.ProcessAction(() =>
                {
                    try
                    {
#if !EFCR
                        latestProgramLogs = s_cQry_LatestProgramLogs(_TaskQueue.Context, acProgram.ACProgramID).ToDictionary(g => g.ACUrl);
#endif
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Database.Root.Messages.LogException("ACProgramCache", "GetCacheEntry(2)", String.Format("s_cQry_LatestProgramLogs for ACProgram {0} failed, Exception {1}", acProgram.ACProgramID, msg));
                    }
                });

#if !DIAGNOSE
                if (latestProgramLogs == null)
                    latestProgramLogs = new Dictionary<string, ACProgramLog>();
                cacheEntry = new ACProgramCacheEntry(acProgram, latestProgramLogs);
                if (!_Programs.TryAdd(acProgram.ACProgramID, cacheEntry))
                    _Programs.TryGetValue(acProgram.ACProgramID, out cacheEntry);
#else
                if (latestProgramLogs != null && latestProgramLogs.Any())
                {
                    cacheEntry = new ACProgramCacheEntry(acProgram, latestProgramLogs);
                    if (!_Programs.TryAdd(acProgram.ACProgramID, cacheEntry))
                        _Programs.TryGetValue(acProgram.ACProgramID, out cacheEntry);
                }
#endif
            }
            return cacheEntry;
        }
#endregion

    }

    internal class ACProgramCacheEntry
    {
        internal ACProgramCacheEntry(ACProgram program, Dictionary<string, ACProgramLog> latestProgramLogs)
        {
            _Program = program;
            _LatestProgramLogs = latestProgramLogs;
        }

#region Properties
        private ACProgram _Program;
        public ACProgram Program
        {
            get
            {
                return _Program;
            }
        }

        Dictionary<string, ACProgramLog> _LatestProgramLogs;
        //public Dictionary<string, ACProgramLog> LatestProgramLogs
        //{
        //    get
        //    {
        //        return _LatestProgramLogs;
        //    }
        //}

        private object _LockLatestPLog = new object();
#endregion

#region Methods
        public ACProgramLog GetCurrentProgramLog(string acUrl, ACProgramLog parentProgramLog = null)
        {
            ACProgramLog currentLog = null;
            lock (_LockLatestPLog)
            {
                if (_LatestProgramLogs == null)
                    return null;
                _LatestProgramLogs.TryGetValue(acUrl, out currentLog);
            }
            if (currentLog != null && parentProgramLog != null && currentLog.ParentACProgramLogID != parentProgramLog.ACProgramLogID)
            {
                // Dieser Fall tritt tritt bei Workflowschritten auf, die in den vergangenen Workflows einmal ausgeführt worden sind,
                // jedoch in dem aktuellen übersprungen worden sind (z.B. Dosierschritte)
                // Dadurch ist kein neuer Eintrag vorhanden und es gibt noch immer den alten im Cache
//#if DEBUG
//                System.Diagnostics.Debugger.Break();
//#endif
                return null;
            }
            return currentLog;
        }

        /// <summary>
        /// If there was an previous log, it will be replaced and the previous will be returned.
        /// Otherwise the new log will be returned.
        /// </summary>
        /// <param name="programLogToAdd"></param>
        /// <param name="acUrl"></param>
        /// <param name="autoReplaceExisting"></param>
        /// <returns></returns>
        public ACProgramLog ReplaceProgramLog(ACProgramLog programLogToAdd, string acUrl, bool autoReplaceExisting)
        {
            ACProgramLog currentLog = null;
            ACProgramLog returnLog = null;
            lock (_LockLatestPLog)
            {
                if (_LatestProgramLogs == null)
                    return null;

                bool exists = _LatestProgramLogs.TryGetValue(acUrl, out returnLog);
                if (exists && autoReplaceExisting)
                    _LatestProgramLogs.Remove(acUrl);

                if (!_LatestProgramLogs.TryGetValue(acUrl, out currentLog))
                {
                    _LatestProgramLogs.Add(acUrl, programLogToAdd);
                    if (returnLog == null)
                        returnLog = programLogToAdd;
                }
//                else
//                {
//#if DEBUG
//                    System.Diagnostics.Debugger.Break();
//#endif
//                }
            }
            return returnLog;
        }

        public ACProgramLog RemoveProgramLog(ACProgramLog programLog)
        {
            ACProgramLog currentLog = null;
            lock (_LockLatestPLog)
            {
                if (_LatestProgramLogs == null)
                    return null;
                _LatestProgramLogs.TryGetValue(programLog.ACUrl, out currentLog);
                if (currentLog != null)
                {
//                    if (currentLog.ACProgramLogID != programLog.ACProgramLogID)
//                    {
//#if DEBUG
//                        System.Diagnostics.Debugger.Break();
//#endif
//                    }
                    if (!_LatestProgramLogs.Remove(programLog.ACUrl))
                        currentLog = null;
                }
            }
            return currentLog;
        }

        public ACProgramLog RemoveProgramLog(string acUrl)
        {
            ACProgramLog currentLog = null;
            lock (_LockLatestPLog)
            {
                if (_LatestProgramLogs == null)
                    return null;
                _LatestProgramLogs.TryGetValue(acUrl, out currentLog);
                if (currentLog != null)
                {
                    if (!_LatestProgramLogs.Remove(acUrl))
                        currentLog = null;
                }
            }
            return currentLog;
        }


        public ACProgramLog[] EmptyLogCache()
        {
            ACProgramLog[] allLogs = null;
            lock (_LockLatestPLog)
            {
                allLogs = _LatestProgramLogs.Values.ToArray();
                _LatestProgramLogs = null;
            }
            return allLogs;
        }
#endregion
    }
}

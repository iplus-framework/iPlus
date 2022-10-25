// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACObjectEntity.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Runtime;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface that must be implemented by a custom ObjectContext to be known by the iPlus-Framework.
    /// Use the class ACObjectContextHelper inside the implemented methods to reuse common logic.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACEntityObjectContext'}de{'IACEntityObjectContext'}", Global.ACKinds.TACInterface)]
    public interface IACEntityObjectContext : IACObject, IDisposable
    {
        #region Methods

#if !EFCR
        /// <summary>
        /// Saves all changes in the Custom-Database-Context as well as in the iPlus-Context
        /// If ContextIPlus is not seperate  (Property IsSeperateIPlusContext == false / ContextIPlus == Database.GlobalDatabase) then SaveChanges will not be invoked for the global database.
        /// If you wan't that, then you have to pass an new iPlus-Context-Instance to the constructor of the Custom-Database-Context!
        /// UNSAFE. Use QueryLock_1X000 outside for Custom-Database-Context as well as for the seperate iPlus-Context
        /// </summary>
        /// <returns></returns>
        [ACMethodInfo("","",9999)]
        MsgWithDetails ACSaveChanges(bool autoSaveContextIPlus = true, SaveOptions saveOptions = SaveOptions.AcceptAllChangesAfterSave, bool validationOff = false, bool writeUpdateInfo = true);

        /// <summary>
        /// Invokes ACSaveChanges. If a transaction error occurs ACSaveChanges is called again.
        /// If parameter retries ist not set, then ACObjectContextHelper.C_NumberOfRetriesOnTransError is used to limit the Retry-Loop.
        /// </summary>
        [ACMethodInfo("", "", 9999)]
        MsgWithDetails ACSaveChangesWithRetry(ushort? retries = null, bool autoSaveContextIPlus = true, SaveOptions saveOptions = SaveOptions.AcceptAllChangesAfterSave, bool validationOff = false, bool writeUpdateInfo = true);
#endif
        /// <summary>
        /// Undoes all changes in the Custom-Database-Context as well as in the iPlus-Context
        /// If ContextIPlus is not seperate  (Property IsSeperateIPlusContext == false / ContextIPlus == Database.GlobalDatabase) then Undo will not be invoked for the global database.
        /// If you wan't that, then you have to pass an new iPlus-Context-Instance to the constructor of the Custom-Database-Context!
        /// UNSAFE. Use QueryLock_1X000 outside for Custom-Database-Context as well as for the seperate iPlus-Context
        /// </summary>
        /// <returns></returns>
        [ACMethodInfo("", "", 9999)]
        MsgWithDetails ACUndoChanges(bool autoUndoContextIPlus = true);

        bool HasModifiedObjectStateEntries();
        bool HasAddedEntities<T>() where T : class;
        /// <summary>
        /// returns Entities with EntityState.Added-State only
        /// </summary>
        /// <returns></returns>
        IList<T> GetAddedEntities<T>(Func<T, bool> selector = null) where T : class;

        /// <summary>
        /// returns Entities with EntityState.Modified-State only
        /// </summary>
        /// <returns></returns>
        IList<T> GetModifiedEntities<T>(Func<T, bool> selector = null) where T : class;

        /// <summary>
        /// returns Entities with EntityState.Deleted-State only
        /// </summary>
        /// <returns></returns>
        IList<T> GetDeletedEntities<T>(Func<T, bool> selector = null) where T : class;

        /// <summary>
        /// returns Entities with EntityState.Detached-State only
        /// </summary>
        /// <returns></returns>
        IList<T> GetDetachedEntities<T>(Func<T, bool> selector = null) where T : class;

        /// <summary>
        /// returns Entities with EntityState.Unchanged-State only
        /// </summary>
        /// <returns></returns>
        IList<T> GetUnchangedEntities<T>(Func<T, bool> selector = null) where T : class;
 
        /// <summary>
        /// returns EntityState.Modified | EntityState.Added | EntityState.Deleted
        /// </summary>
        /// <returns></returns>
        IList<T> GetChangedEntities<T>(Func<T, bool> selector = null) where T : class;

        IList<Msg> CheckChangedEntities();

#if !EFCR
        /// <summary>
        /// Refreshes the EntityObject if not in modified state. Else it leaves it untouched.
        /// </summary>
        /// <param name="entityObject"></param>
        /// <param name="refreshMode"></param>
        void AutoRefresh(EntityObject entityObject, RefreshMode refreshMode = RefreshMode.StoreWins);

        /// <summary>
        /// Refreshes all EntityObjects in the EntityCollection if not in modified state. Else it leaves it untouched.
        /// Attention: This method will only refresh the entities with entity keys that are tracked by the ObjectContext. 
        /// If changes are made in background on the database you shoud use the method AutoLoad, to retrieve new Entries from the Database!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityCollection"></param>
        /// <param name="refreshMode"></param>
        void AutoRefresh<T>(EntityCollection<T> entityCollection, RefreshMode refreshMode = RefreshMode.StoreWins) where T : class;

        /// <summary>
        /// Queries the Database an refreshes the collection if not in modified state. MergeOption.OverwriteChanges
        /// Els if in modified state, then colletion is only refreshed with MergeOption.AppendOnly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityCollection"></param>
        void AutoLoad<T>(EntityCollection<T> entityCollection) where T : class;
#endif
        void ParseException(MsgWithDetails msg, Exception e);

        void EnterCS();
        void EnterCS(bool DeactivateEntityCheck);
        void LeaveCS();
        ACMonitorObject QueryLock_1X000 { get; }

        /// <summary>
        /// UNSAFE. Use QueryLock_1X000 outside
        /// </summary>
        /// <returns></returns>
        void DetachAllEntitiesAndDispose(bool detach = false, bool dispose = true);
#if !EFCR
        void FullDetach(EntityObject obj);
#endif
        event ACChangesEventHandler ACChangesExecuted;
        #endregion

        #region Properties
        [ACPropertyInfo(9999)]
        bool IsChanged { get; }

        [ACPropertyInfo(9999)]
        Database ContextIPlus { get; }

        bool IsSeparateIPlusContext { get; }
#if !EFCR
        MergeOption RecommendedMergeOption { get; }

        EntityConnection SeparateConnection { get; }
#endif
        string UserName { get; set; }

        #endregion

        #region ObjectContext
        int? CommandTimeout { get; set; }
        DbConnection Connection { get; }
#if !EFCR
        ObjectContextOptions ContextOptions { get; }
#endif
        string DefaultContainerName { get; set; }
#if !EFCR
        MetadataWorkspace MetadataWorkspace { get; }
        ObjectStateManager ObjectStateManager { get; }

        event ObjectMaterializedEventHandler ObjectMaterialized;
#endif
        event EventHandler SavingChanges;


        void AcceptAllChanges();
        void AddObject(string entitySetName, object entity);
        TEntity ApplyCurrentValues<TEntity>(string entitySetName, TEntity currentEntity) where TEntity : class;
        TEntity ApplyOriginalValues<TEntity>(string entitySetName, TEntity originalEntity) where TEntity : class;
#if !EFCR
        void Attach(IEntityWithKey entity);
#endif
        void AttachTo(string entitySetName, object entity);
        void CreateDatabase();
        string CreateDatabaseScript();
#if !EFCR
        EntityKey CreateEntityKey(string entitySetName, object entity);
#endif
        T CreateObject<T>() where T : class;
#if !EFCR
        ObjectSet<TEntity> CreateObjectSet<TEntity>() where TEntity : class;
        ObjectSet<TEntity> CreateObjectSet<TEntity>(string entitySetName) where TEntity : class;
#endif
        void CreateProxyTypes(IEnumerable<Type> types);
#if !EFCR
        ObjectQuery<T> CreateQuery<T>(string queryString, params ObjectParameter[] parameters);
#endif
        bool DatabaseExists();
        void DeleteDatabase();
        void DeleteObject(object entity);
        void Detach(object entity);
        void DetectChanges();
#if !EFCR
        int ExecuteFunction(string functionName, params ObjectParameter[] parameters);
        ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, params ObjectParameter[] parameters);
        ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, MergeOption mergeOption, params ObjectParameter[] parameters);
#endif
        int ExecuteStoreCommand(string commandText, params object[] parameters);
#if !EFCR
        ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, params object[] parameters);
        ObjectResult<TEntity> ExecuteStoreQuery<TEntity>(string commandText, string entitySetName, MergeOption mergeOption, params object[] parameters);
        object GetObjectByKey(EntityKey key);
#endif
        void LoadProperty(object entity, string navigationProperty);
        void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector);
#if !EFCR
        void LoadProperty(object entity, string navigationProperty, MergeOption mergeOption);
        void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector, MergeOption mergeOption);
        void Refresh(RefreshMode refreshMode, IEnumerable collection);
        void Refresh(RefreshMode refreshMode, object entity);
#endif
        int SaveChanges();
#if !EFCR
        int SaveChanges(SaveOptions options);
        ObjectResult<TElement> Translate<TElement>(DbDataReader reader);
        ObjectResult<TEntity> Translate<TEntity>(DbDataReader reader, string entitySetName, MergeOption mergeOption);
        bool TryGetObjectByKey(EntityKey key, out object value);
#endif
        #endregion
    }

}

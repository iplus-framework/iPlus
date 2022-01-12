﻿// ***********************************************************************
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
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using System.Runtime;
using System.Data.Objects;

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
        IList<T> GetAddedEntities<T>() where T : class;
        IList<Msg> CheckChangedEntities();

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

        void FullDetach(EntityObject obj);

        event ACChangesEventHandler ACChangesExecuted;
        #endregion

        #region Properties
        [ACPropertyInfo(9999)]
        bool IsChanged { get; }

        [ACPropertyInfo(9999)]
        Database ContextIPlus { get; }

        bool IsSeparateIPlusContext { get; }

        MergeOption RecommendedMergeOption { get; }

        EntityConnection SeparateConnection { get; }

        string UserName { get; set; }

        #endregion

        #region ObjectContext
        int? CommandTimeout { get; set; }
        DbConnection Connection { get; }
        ObjectContextOptions ContextOptions { get; }
        string DefaultContainerName { get; set; }
        MetadataWorkspace MetadataWorkspace { get; }
        ObjectStateManager ObjectStateManager { get; }

        event ObjectMaterializedEventHandler ObjectMaterialized;
        event EventHandler SavingChanges;


        void AcceptAllChanges();
        void AddObject(string entitySetName, object entity);
        TEntity ApplyCurrentValues<TEntity>(string entitySetName, TEntity currentEntity) where TEntity : class;
        TEntity ApplyOriginalValues<TEntity>(string entitySetName, TEntity originalEntity) where TEntity : class;
        void Attach(IEntityWithKey entity);
        void AttachTo(string entitySetName, object entity);
        void CreateDatabase();
        string CreateDatabaseScript();
        EntityKey CreateEntityKey(string entitySetName, object entity);
        T CreateObject<T>() where T : class;
        ObjectSet<TEntity> CreateObjectSet<TEntity>() where TEntity : class;
        ObjectSet<TEntity> CreateObjectSet<TEntity>(string entitySetName) where TEntity : class;
        void CreateProxyTypes(IEnumerable<Type> types);
        ObjectQuery<T> CreateQuery<T>(string queryString, params ObjectParameter[] parameters);
        bool DatabaseExists();
        void DeleteDatabase();
        void DeleteObject(object entity);
        void Detach(object entity);
        void DetectChanges();
        int ExecuteFunction(string functionName, params ObjectParameter[] parameters);
        ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, params ObjectParameter[] parameters);
        ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, MergeOption mergeOption, params ObjectParameter[] parameters);
        int ExecuteStoreCommand(string commandText, params object[] parameters);
        ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, params object[] parameters);
        ObjectResult<TEntity> ExecuteStoreQuery<TEntity>(string commandText, string entitySetName, MergeOption mergeOption, params object[] parameters);
        object GetObjectByKey(EntityKey key);
        void LoadProperty(object entity, string navigationProperty);
        void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector);
        void LoadProperty(object entity, string navigationProperty, MergeOption mergeOption);
        void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector, MergeOption mergeOption);
        void Refresh(RefreshMode refreshMode, IEnumerable collection);
        void Refresh(RefreshMode refreshMode, object entity);
        int SaveChanges();
        int SaveChanges(SaveOptions options);
        ObjectResult<TElement> Translate<TElement>(DbDataReader reader);
        ObjectResult<TEntity> Translate<TEntity>(DbDataReader reader, string entitySetName, MergeOption mergeOption);
        bool TryGetObjectByKey(EntityKey key, out object value);
        #endregion
    }

}

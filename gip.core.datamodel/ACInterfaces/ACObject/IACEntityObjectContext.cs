// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
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
using System.Linq.Expressions;
using System.Runtime;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query;

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
        MsgWithDetails ACSaveChanges(bool autoSaveContextIPlus = true, bool acceptAllChangesOnSuccess = true, bool validationOff = false, bool writeUpdateInfo = true);

        /// <summary>
        /// Invokes ACSaveChanges. If a transaction error occurs ACSaveChanges is called again.
        /// If parameter retries ist not set, then ACObjectContextHelper.C_NumberOfRetriesOnTransError is used to limit the Retry-Loop.
        /// </summary>
        [ACMethodInfo("", "", 9999)]
        MsgWithDetails ACSaveChangesWithRetry(ushort? retries = null, bool acceptAllChangesOnSuccess = true, bool autoSaveContextIPlus = true, bool validationOff = false, bool writeUpdateInfo = true);

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

        public void Refresh(RefreshMode refreshMode, object entity);

        /// <summary>
        /// Refreshes the VBEntityObject if not in modified state. Else it leaves it untouched.
        /// </summary>
        /// <param name="entityObject"></param>
        void AutoRefresh(VBEntityObject entityObject);

        /// <summary>
        /// Refreshes all EntityObjects in the EntityCollection if not in modified state. Else it leaves it untouched.
        /// Attention: This method will only refresh the entities with entity keys that are tracked by the ObjectContext. 
        /// If changes are made in background on the database you shoud use the method AutoLoad, to retrieve new Entries from the Database!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityCollection"></param>
        /// <param name="entry"></param>
        void AutoRefresh<T>(ICollection<T> entityCollection, CollectionEntry entry) where T : class;

        /// <summary>
        /// Queries the Database an refreshes the collection if not in modified state. MergeOption.OverwriteChanges
        /// Els if in modified state, then colletion is only refreshed with MergeOption.AppendOnly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityCollection"></param>
        /// <param name="entry"></param>
        void AutoLoad<T>(ICollection<T> entityCollection, CollectionEntry entry) where T : class;

        void ParseException(MsgWithDetails msg, Exception e);

        void Detach(object entity);

        void EnterCS();
        void EnterCS(bool DeactivateEntityCheck);
        void LeaveCS();
        ACMonitorObject QueryLock_1X000 { get; }

        /// <summary>
        /// UNSAFE. Use QueryLock_1X000 outside
        /// </summary>
        /// <returns></returns>
        void DetachAllEntitiesAndDispose(bool detach = false, bool dispose = true);

        void FullDetach(VBEntityObject obj);

        event ACChangesEventHandler ACChangesExecuted;
#endregion

#region Properties
        [ACPropertyInfo(9999)]
        bool IsChanged { get; }

        [ACPropertyInfo(9999)]
        Database ContextIPlus { get; }
        bool IsSeparateIPlusContext { get; }
        MergeOption RecommendedMergeOption { get; }
        DbConnection SeparateConnection { get; }
        string UserName { get; set; }
		bool PreventOnContextACChangesExecuted { get; set; }
        DbConnection Connection { get; }

        // Compatibility to legacy code that uses EntityKey from EF4
        object GetObjectByKey(EntityKey key);
        bool TryGetObjectByKey(EntityKey key, out object entity);

        /// <summary>
        /// Compatibility for legacy code that uses EntityKey from EF4
        /// used in EF4 to identify the context, now it is the namespace of the DbContext to be able to build a assemby qualified name to consturct an assembly qualified name for the EntityKey
        /// </summary>
        string DefaultContainerName { get; }
        string DefaultContainerNameV4 { get; }

        string GetQualifiedEntitySetNameForEntityKey(string entitySetName);
        #endregion

        #region DBContext

        ChangeTracker ChangeTracker { get; }

        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);

#nullable enable
        event EventHandler<SavingChangesEventArgs>? SavingChanges;
        event EventHandler<SavedChangesEventArgs>? SavedChanges;
        event EventHandler<SaveChangesFailedEventArgs>? SaveChangesFailed;
        object? Find(Type entityType, params object?[]? keyValues);
        TEntity? Find<TEntity>(params object?[]? keyValues) where TEntity : class;
#nullable disable

        DatabaseFacade Database { get; }

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry Entry(object entity);

        EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;

        EntityEntry Add(object entity);
        EntityEntry Attach(object entity);
        EntityEntry Update(object entity);
        EntityEntry Remove(object entity);

        void AddRange(params object[] entities);
        void AddRange(IEnumerable<object> entities);
        void AttachRange(params object[] entities);
        void AttachRange(IEnumerable<object> entities);
        void UpdateRange(params object[] entities);
        void UpdateRange(IEnumerable<object> entities);
        void RemoveRange(params object[] entities);
        void RemoveRange(IEnumerable<object> entities);

        DbSet<TEntity> CreateObjectSet<TEntity>() where TEntity : class;
        DbSet<TEntity> CreateObjectSet<TEntity>(string entitySetName) where TEntity : class;

        #endregion
    }

}

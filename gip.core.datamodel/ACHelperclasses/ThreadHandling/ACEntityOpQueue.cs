// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACEntityOpQueue.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace gip.core.datamodel
{
    public interface IACEntityOpQueue
    {
        IACEntityObjectContext ObjectContext { get; }
    }

    /// <summary>
    /// Class ACEntityOpQueue
    /// </summary>
    public class ACEntityOpQueue<T> : ACDelegateQueue, IACEntityOpQueue where T : IACEntityObjectContext
    {
        /// <summary>
        /// Initializes a new instance of the ACDelegateQueue class.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <param name="objectContextToCreate">The object context to create.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="saveChangesWithoutValidation">saveChangesWithoutValidation</param>
        public ACEntityOpQueue(string instanceName, Type objectContextToCreate, string connectionString, bool saveChangesWithoutValidation)
            : base(instanceName)
        {
            _SaveChangesWithoutValidation = saveChangesWithoutValidation;
            SqlConnection entityConnection = new SqlConnection(connectionString);
            entityConnection.Open();
            _Context = (T)Activator.CreateInstance(objectContextToCreate, new Object[] { entityConnection });
            ACObjectContextManager.Add(_Context, instanceName);
            _AutoOpenClose = false;
        }

        /// <summary>
        /// Initializes a new instance of the ACEntityOpQueue class.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <param name="database">The database.</param>
        /// <param name="autoOpenClose">if set to <c>true</c> [auto open close].</param>
        /// <param name="saveChangesWithoutValidation">saveChangesWithoutValidation</param>
        public ACEntityOpQueue(string instanceName, T database, bool autoOpenClose, bool saveChangesWithoutValidation)
            : base(instanceName)
        {
            _SaveChangesWithoutValidation = saveChangesWithoutValidation;
            _Context = database;
            _AutoOpenClose = autoOpenClose;
            ACObjectContextManager.Add(_Context, instanceName);
        }

        /// <summary>
        /// Initializes a new instance of the ACEntityOpQueue class.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <param name="database">The database.</param>
        /// <param name="workerInterval_ms">The worker interval_ms.</param>
        /// <param name="autoOpenClose">if set to <c>true</c> [auto open close].</param>
        /// <param name="saveChangesWithoutValidation">saveChangesWithoutValidation</param>
        public ACEntityOpQueue(string instanceName, T database, int workerInterval_ms, bool autoOpenClose, bool saveChangesWithoutValidation)
            : base(instanceName, workerInterval_ms)
        {
            _SaveChangesWithoutValidation = saveChangesWithoutValidation;
            _Context = database;
            _AutoOpenClose = autoOpenClose;
            ACObjectContextManager.Add(_Context, instanceName);
        }

        /// <summary>
        /// The _ context
        /// </summary>
        private T _Context;
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public T Context
        {
            get
            {
                return _Context;
            }
        }

        public IACEntityObjectContext ObjectContext
        {
            get
            {
                return _Context as IACEntityObjectContext;
            }
        }

        /// <summary>
        /// The _ auto open close
        /// </summary>
        private bool _AutoOpenClose = true;
        public bool AutoOpenClose
        {
            get
            {
                return _AutoOpenClose;
            }
        }

        private bool _SaveChangesWithoutValidation = false;
        public bool SaveChangesWithoutValidation
        {
            get
            {
                return _SaveChangesWithoutValidation;
            }
        }

        private short _UncommitedRead = 0;
        public virtual bool NeedsUncommitedRead
        {
            get
            {
                return false;
            }
        }

        public virtual System.Transactions.IsolationLevel? DefaultIsolationLevel
        {
            get
            {
                return null;
            }
        }

        public virtual System.Transactions.TransactionScopeOption? DefaultTransactionScopeOption
        {
            get
            {
                return null;
            }
        }

        private TransactionScope _TransScope = null;

        private void CreateTransactionScopeIf()
        {
            if (DefaultTransactionScopeOption.HasValue || DefaultIsolationLevel.HasValue)
            {
                using (ACMonitor.Lock(Context.QueryLock_1X000))
                {
                    if (_TransScope == null)
                    {
                        if (DefaultTransactionScopeOption.HasValue)
                        {
                            if (DefaultIsolationLevel.HasValue)
                                _TransScope = new TransactionScope(DefaultTransactionScopeOption.Value, new TransactionOptions() { IsolationLevel = DefaultIsolationLevel.Value });
                            else
                                _TransScope = new TransactionScope(DefaultTransactionScopeOption.Value);
                        }
                        else if (DefaultIsolationLevel.HasValue)
                            _TransScope = new TransactionScope(System.Transactions.TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = DefaultIsolationLevel.Value });
                    }
                }
            }
            else
            {
                try
                {
                    if (NeedsUncommitedRead
                        && _UncommitedRead != 1
                        && Context.SeparateConnection != null)
                    {
                        SetIsolationLevelUncommitedRead();
                    }
                    else if (!NeedsUncommitedRead
                        && _UncommitedRead != 0
                        && Context.SeparateConnection != null)
                    {
                        SetIsolationLevelCommitedRead();
                    }
                }
                catch 
                {

                }
            }
        }

        private void DisposeTransactionScopeIf()
        {
            if (DefaultTransactionScopeOption.HasValue || DefaultIsolationLevel.HasValue)
            {
                using (ACMonitor.Lock(Context.QueryLock_1X000))
                {
                    if (_TransScope != null)
                    {
                        _TransScope.Dispose();
                        _TransScope = null;
                    }
                }
            }
        }

        protected void SetIsolationLevelUncommitedRead()
        {
            using (ACMonitor.Lock(Context.QueryLock_1X000))
            {
                Context.Database.ExecuteSql(ACObjectContextHelper.SET_READ_UNCOMMITED);
            }
            if (Context.IsSeparateIPlusContext)
            {
                using (ACMonitor.Lock(Context.ContextIPlus.QueryLock_1X000))
                {
                    Context.ContextIPlus.Database.ExecuteSql(ACObjectContextHelper.SET_READ_UNCOMMITED);
                }
            }
            _UncommitedRead = 1;
        }

        protected void SetIsolationLevelCommitedRead()
        {
            using (ACMonitor.Lock(Context.QueryLock_1X000))
            {
                Context.Database.ExecuteSql(ACObjectContextHelper.SET_READ_COMMITED);
            }
            if (Context.IsSeparateIPlusContext)
            {
                using (ACMonitor.Lock(Context.ContextIPlus.QueryLock_1X000))
                {
                    Context.ContextIPlus.Database.ExecuteSql(ACObjectContextHelper.SET_READ_COMMITED);
                }
            }
            _UncommitedRead = 0;
        }

        /// <summary>
        /// Called when [start queue processing].
        /// </summary>
        /// <param name="countActions">The count actions.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        protected override bool OnStartQueueProcessing(int countActions)
        {
            //EnterCS();
            bool isOpen = true;
            if (!_AutoOpenClose)
                isOpen = OpenConnection();
            if (isOpen)
                CreateTransactionScopeIf();
            return isOpen;
        }

        /// <summary>
        /// Processes the action.
        /// </summary>
        /// <param name="action">The action.</param>
        public override void ProcessAction(Action action)
        {
            using (ACMonitor.Lock(Context.QueryLock_1X000))
            {
                bool isOpen = true;
                if (!_AutoOpenClose)
                    isOpen = OpenConnection();
                action();
                //if (openTemporary)
                //    CloseConnection();

            }
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        protected bool OpenConnection()
        {
            bool done = false;
            if (Context.SeparateConnection != null)
            {
                using (ACMonitor.Lock(Context.QueryLock_1X000))
                {
                    if (Context.Connection.State == ConnectionState.Open)
                        done = true;
                    else
                    {
                        try
                        {
                            Context.Connection.Open();
                            _UncommitedRead = 0;
                            done = true;
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACEntityOpQueue", "OpenConnection", msg);

                            return false;
                        }
                    }
                }
            }

            if (Context.IsSeparateIPlusContext && Context.ContextIPlus.SeparateConnection != null)
            {
                using (ACMonitor.Lock(Context.ContextIPlus.QueryLock_1X000))
                {
                    if (Context.ContextIPlus.Connection.State == ConnectionState.Open)
                        done = true;
                    else
                    {
                        try
                        {
                            Context.ContextIPlus.Connection.Open();
                            done = true;
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACEntityOpQueue", "OpenConnection(10)", msg);

                            return false;
                        }
                    }
                }
            }

            return done;
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        protected bool CloseConnection()
        {
            bool done = false;
            if (Context.IsSeparateIPlusContext && Context.ContextIPlus.SeparateConnection != null)
            {
                using (ACMonitor.Lock(Context.ContextIPlus.QueryLock_1X000))
                {
                    try
                    {
                        Context.ContextIPlus.Connection.Close();
                        _UncommitedRead = 0;
                        done = true;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACEntityOpQueue", "CloseConnection", msg);

                        return false;
                    }
                }
            }
            if (Context.SeparateConnection != null)
            {
                using (ACMonitor.Lock(Context.QueryLock_1X000))
                {
                    try
                    {
                        Context.Connection.Close();
                        done = true;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACEntityOpQueue", "CloseConnection(10)", msg);

                        return false;
                    }
                }
            }
            return done;
        }

        private int _SaveChangesRetriesA = 0;
        private int _SaveChangesRetriesB = 0;
        /// <summary>
        /// Called when [queue processed].
        /// </summary>
        /// <param name="countActions">The count actions.</param>
        protected override void OnQueueProcessed(int countActions)
        {
            //IDisposable cookie1 = null;
            //IDisposable cookie2 = null;
            try
            {
                //cookie1 = ACMonitor.Lock(Context.QueryLock_1X000);
                using (ACMonitor.Lock(Context.QueryLock_1X000))
                {
                    try
                    {
                        if (Context.HasModifiedObjectStateEntries())
                        {
                            MsgWithDetails msg = Context.ACSaveChanges(true, SaveChangesWithoutValidation);
                            if (msg != null)
                            {
                                bool isDisconnected = ACObjectContextHelper.IsDisconnectedException(msg);
                                if (isDisconnected)
                                    _UncommitedRead = 0;
                                if (ACThread.PerfLogger.Active
                                    && isDisconnected
                                    && Database.Root != null
                                    && Database.Root.VBDump != null)
                                    Database.Root.VBDump.DumpStackTrace(Thread.CurrentThread, true);
                                Database.Root.Messages.LogError(InstanceName, "ACEntityOpQueue.OnQueueProcessed(0)", "ACSaveChanges failed");
                                Database.Root.Messages.LogMessageMsg(msg);
                                if (_SaveChangesRetriesA <= 3 && isDisconnected)
                                {
                                    Add(() => { Database.Root.Messages.LogError(InstanceName, "ACEntityOpQueue.OnQueueProcessed(1)", "Try to exceute ACSaveChanges again"); });
                                    _SaveChangesRetriesA++;
                                }
                                else
                                {
                                    Database.Root.Messages.LogError(InstanceName, "ACEntityOpQueue.OnQueueProcessed(2)", "ACUndoChanges() invoked, all changes are reverted");
                                    Context.ACUndoChanges();
                                    _SaveChangesRetriesA = 0;
                                }
                            }
                            else
                            {
                                _SaveChangesRetriesA = 0;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Context.ACUndoChanges();
                        _SaveChangesRetriesA = 0;

                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACEntityOpQueue", "OnQueueProcessed", msg);
                    }
                }

                if (Context.IsSeparateIPlusContext)
                {
                    //cookie2 = ACMonitor.Lock(Context.ContextIPlus.QueryLock_1X000);
                    using (ACMonitor.Lock(Context.ContextIPlus.QueryLock_1X000))
                    {
                        try
                        {
                            if (Context.ContextIPlus.HasModifiedObjectStateEntries())
                            {
                                MsgWithDetails msg = Context.ContextIPlus.ACSaveChanges(true, _TransScope == null, SaveChangesWithoutValidation);
                                if (msg != null)
                                {
                                    Database.Root.Messages.LogError(InstanceName, "ACEntityOpQueue.OnQueueProcessed(10)", "ContextIPlus.ACSaveChanges failed");
                                    Database.Root.Messages.LogMessageMsg(msg);
                                    bool isDisconnected = ACObjectContextHelper.IsDisconnectedException(msg);
                                    if (isDisconnected)
                                        _UncommitedRead = 0;
                                    if (_SaveChangesRetriesB <= 3 && isDisconnected)
                                    {
                                        Add(() => { Database.Root.Messages.LogError(InstanceName, "ACEntityOpQueue.OnQueueProcessed(11)", "Try to exceute ACSaveChanges again"); });
                                        _SaveChangesRetriesB++;
                                    }
                                    else
                                    {
                                        Database.Root.Messages.LogError(InstanceName, "ACEntityOpQueue.OnQueueProcessed(12)", "ACUndoChanges() invoked, all changes are reverted");
                                        Context.ContextIPlus.ACUndoChanges();
                                        _SaveChangesRetriesB = 0;
                                    }
                                }
                                else
                                {
                                    _SaveChangesRetriesB = 0;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Context.ContextIPlus.ACUndoChanges();
                            _SaveChangesRetriesB = 0;

                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACEntityOpQueue", "OnQueueProcessed(10)", msg);
                        }
                    }
                }
            }
            finally
            {
                if (   _SaveChangesRetriesA == 0 
                    && _SaveChangesRetriesB == 0 
                    && (DefaultTransactionScopeOption.HasValue || DefaultIsolationLevel.HasValue))
                {
                    if (_TransScope != null)
                    {
                        using (ACMonitor.Lock(Context.QueryLock_1X000))
                        {
                            try
                            {
                                _TransScope.Complete();
                                Context.ChangeTracker.AcceptAllChanges();
                                _TransScope.Dispose();
                            }
                            catch (Exception ex)
                            {
                                if (Database.Root != null && Database.Root.Messages != null)
                                    Database.Root.Messages.LogException("ACEntityOpQueue", "_TransScope", ex);
                            }
                            _TransScope = null;
                        }
                    }
                }
                //cookie1.Dispose();
                //if (cookie2 != null)
                //    cookie2.Dispose();
            }
            //if (!_AutoOpenClose)
                //CloseConnection();
        }

        /// <summary>
        /// Called when [thread stopped].
        /// </summary>
        protected override void OnThreadStopped()
        {
            CloseConnection();
        }

        /// <summary>
        /// Enters Critical Section e.g. if access to Childs-List which could be emptied from framework during access
        /// </summary>
        public void EnterQueryLock()
        {
            ACMonitor.Enter(Context.QueryLock_1X000);
        }

        /// <summary>
        /// Leaves Critical Section
        /// </summary>
        public void ExitQueryLock()
        {
            ACMonitor.Exit(Context.QueryLock_1X000);
        }

        //public void Dispose()
        //{
        //    Dispose(true);
        //    //GC.SuppressFinalize(this);
        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        if (_Context != null)
        //            ACObjectContextManager.DisposeAndRemove(_Context);
        //        if (_Connection != null)
        //        {
        //            _Connection.Close();
        //            _Connection.Dispose();
        //            _Connection = null;
        //        }
        //        _Context = default(T);
        //    }
        //}

    }
}

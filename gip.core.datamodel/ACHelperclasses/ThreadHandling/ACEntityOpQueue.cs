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
#if !EFCR
            var entityConnection = new EntityConnection(connectionString);
            entityConnection.Open();
            _Context = (T)Activator.CreateInstance(objectContextToCreate, new Object[] { entityConnection });
#endif
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

        /// <summary>
        /// Called when [start queue processing].
        /// </summary>
        /// <param name="countActions">The count actions.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        protected override bool OnStartQueueProcessing(int countActions)
        {
            //EnterCS();
            if (!_AutoOpenClose)
                return OpenConnection();
            return true;
        }

        /// <summary>
        /// Processes the action.
        /// </summary>
        /// <param name="action">The action.</param>
        public override void ProcessAction(Action action)
        {
            using (ACMonitor.Lock(Context.QueryLock_1X000))
            {
                bool openTemporary = false;
                if (!_AutoOpenClose && Context.Connection.State != ConnectionState.Open)
                {
                    OpenConnection();
                    openTemporary = true;
                }
                action();
                if (openTemporary)
                    CloseConnection();

            }
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        protected bool OpenConnection()
        {
            bool done = false;

#if !EFCR
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
#endif

#if !EFCR
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
#endif

            throw new NotImplementedException();
            return done;
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        protected bool CloseConnection()
        {
            bool done = false;
#if !EFCR
            if (Context.IsSeparateIPlusContext && Context.ContextIPlus.SeparateConnection != null)
            {
                using (ACMonitor.Lock(Context.ContextIPlus.QueryLock_1X000))
                {
                    try
                    {
                        Context.ContextIPlus.Connection.Close();
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
#endif

#if !EFCR
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
#endif
            throw new NotImplementedException();
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
            using (ACMonitor.Lock(Context.QueryLock_1X000))
            {
                try
                {
                    if (Context.HasModifiedObjectStateEntries())
                    {
                        MsgWithDetails msg = Context.ACSaveChanges(true, SaveChangesWithoutValidation);
                        if (msg != null)
                        {
                            if (   ACThread.PerfLogger.Active
                                && ACObjectContextHelper.IsDisconnectedException(msg)
                                && Database.Root  != null 
                                && Database.Root.VBDump != null)
                                Database.Root.VBDump.DumpStackTrace(Thread.CurrentThread);
                            Database.Root.Messages.LogError(InstanceName, "ACEntityOpQueue.OnQueueProcessed(0)", "ACSaveChanges failed");
                            Database.Root.Messages.LogMessageMsg(msg);
                            if (_SaveChangesRetriesA <= 3 && ACObjectContextHelper.IsDisconnectedException(msg))
                            {
                                Add(() => { Database.Root.Messages.LogError(InstanceName, "ACEntityOpQueue.OnQueueProcessed(1)", "Try to exceute ACSaveChanges again"); } );
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
                using (ACMonitor.Lock(Context.ContextIPlus.QueryLock_1X000))
                {
                    try
                    {
                        if (Context.ContextIPlus.HasModifiedObjectStateEntries())
                        {
                            MsgWithDetails msg = Context.ContextIPlus.ACSaveChanges(true, SaveChangesWithoutValidation);
                            if (msg != null)
                            {
                                Database.Root.Messages.LogError(InstanceName, "ACEntityOpQueue.OnQueueProcessed(10)", "ContextIPlus.ACSaveChanges failed");
                                Database.Root.Messages.LogMessageMsg(msg);
                                if (_SaveChangesRetriesB <= 3 && ACObjectContextHelper.IsDisconnectedException(msg))
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
            if (!_AutoOpenClose)
                CloseConnection();
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

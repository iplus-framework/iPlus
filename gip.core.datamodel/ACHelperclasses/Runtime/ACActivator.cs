// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACActivator.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACActivatorThread
    /// </summary>
    public class ACActivatorThread
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACActivatorThread"/> class.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public ACActivatorThread(Thread thread)
        {
            Thread = thread;
            InstanceDepth = 0;
            ProxyObjectsInvolved = false;
        }

        /// <summary>
        /// Gets the thread.
        /// </summary>
        /// <value>The thread.</value>
        public Thread Thread { get; private set; }
        /// <summary>
        /// Gets or sets the instance depth.
        /// </summary>
        /// <value>The instance depth.</value>
        public int InstanceDepth { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [proxy objects involved].
        /// </summary>
        /// <value><c>true</c> if [proxy objects involved]; otherwise, <c>false</c>.</value>
        public bool ProxyObjectsInvolved { get; set; }

        /// <summary>
        /// The _ new AC object list
        /// </summary>
        private Stack<IACObjectWithInit> _NewACObjectStack = new Stack<IACObjectWithInit>();
        /// <summary>
        /// Gets the new AC object list.
        /// </summary>
        /// <value>The new AC object list.</value>
        public Stack<IACObjectWithInit> NewACObjectStack
        {
            get
            {
                return _NewACObjectStack;
            }
        }

        /// <summary>
        /// The _ post operations queue
        /// </summary>
        private ACDelegateQueue _PostOperationsQueue = null;
        /// <summary>
        /// Gets the post operations queue.
        /// </summary>
        /// <value>The post operations queue.</value>
        public ACDelegateQueue PostOperationsQueue
        {
            get
            {
                if (_PostOperationsQueue == null)
                    _PostOperationsQueue = new ACDelegateQueue("ACDelegateQueue: ACActivator.PostOperationsQueue;");
                return _PostOperationsQueue;
            }
        }
    }

    public class ACCreateException : Exception
    {
        public ACCreateException(IACObjectWithInit invalidObject, string message)
            : base(message)
        {
            _InvalidObject = invalidObject;
        }

        public ACCreateException(IACObjectWithInit invalidObject, string message, Exception innerException)
            : base(message, innerException)
        {
            _InvalidObject = invalidObject;
            if (innerException != null)
                _OSource = Occurrence.CatchedInnerException;
        }

        IACObjectWithInit _InvalidObject;
        public IACObjectWithInit InvalidObject
        {
            get
            {
                return _InvalidObject;
            }
        }

        Occurrence _OSource = Occurrence.ACInit;
        public Occurrence OSource
        {
            get
            {
                return _OSource;
            }
        }

        public enum Occurrence
        {
            ACInit = 0,
            CatchedInnerException = 1
        }
    }

    /// <summary>
    /// Class ACActivator
    /// </summary>
    public static class ACActivator
    {
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="content">The content.</param>
        /// <param name="acComponentParent">The ac component parent.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="startChildMode">The start child mode.</param>
        /// <param name="acObjectType">Type of the ac object.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="propagateException">The ac identifier.</param>
        /// <returns>IACObjectWithInit.</returns>
        public static IACObjectWithInit CreateInstance(ACClass acClass, object content, IACObjectWithInit acComponentParent, ACValueList parameter, Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic, Type acObjectType = null, string acIdentifier = "", bool propagateException = false)
        {
            string parentACIdentifier = acComponentParent != null ? acComponentParent.ACIdentifier : "";
            Global.ControlModes controlMode = acClass.GetRight(acClass);
            if (controlMode < Global.ControlModes.Disabled 
                && acClass.ParentACClassID.HasValue) // Root-Applications are allowed
                return null;
            if (acObjectType == null)
                acObjectType = GetACObjectType(acClass);
            if (acObjectType == null)
                return null;
            IACObjectWithInit newACObject = null;
            ACActivatorThread currentThreadInACInit = null;
            try
            {
                bool recycled = false;
                IACComponent componentParent = acComponentParent as IACComponent;
                if (componentParent != null)
                    newACObject = componentParent.Root.ComponentPool.Pop(acClass);
                if (newACObject != null)
                {
#if DEBUG
                    if (DiagWFLoadingCount > 0 && (Database.Root != null) && (Database.Root.Messages != null))
                        gip.core.datamodel.Database.Root.Messages.LogDebug("ACActivator", "CreateInstance(Pool)",
                            String.Format("Start={0:dd.MM.yyyy HH:mm:ss.ffff}; ThreadID={1}; Class={2};",
                            DateTime.Now, Thread.CurrentThread.ManagedThreadId, acClass.ACIdentifier));
#endif

                    IACComponent newACComponent = newACObject as IACComponent;
                    IACObject contentACObject = content as IACObject;
                    newACComponent.Recycle(contentACObject, acComponentParent, parameter, acIdentifier);
                    recycled = true;
                }
                else
                {
#if DEBUG
                    if (DiagWFLoadingCount > 0 && (Database.Root != null) && (Database.Root.Messages != null))
                        gip.core.datamodel.Database.Root.Messages.LogDebug("ACActivator", "CreateInstance(New)",
                            String.Format("Start={0:dd.MM.yyyy HH:mm:ss.ffff}; ThreadID={1}; Class={2};",
                            DateTime.Now, Thread.CurrentThread.ManagedThreadId, acClass.ACIdentifier));
#endif
                    newACObject = Activator.CreateInstance(acObjectType, new Object[] { acClass, content, acComponentParent, parameter, acIdentifier }) as IACObjectWithInit;
                }

                // Wenn nicht IsMultiInstance und schon instanziiert dann keine neue Instanz möglich
                if (!acClass.IsMultiInstanceInherited && componentParent != null)
                {
                    if (componentParent.ACComponentChilds.Where(c => c.ACIdentifier == newACObject.ACIdentifier).Any())
                    {
                        if (recycled)
                        {
                            if (newACObject.ACPreDeInit(true))
                            {
                                newACObject.ACDeInit(true);
                            }
                        }
                        return null;
                    }
                }

                currentThreadInACInit = ACActivator.CurrentInitializingThread;
                if (currentThreadInACInit.InstanceDepth <= 0)
                    currentThreadInACInit.ProxyObjectsInvolved = false;
                currentThreadInACInit.InstanceDepth++;

#if DEBUG
                if (DiagWFLoadingCount > 0 && (Database.Root != null) && (Database.Root.Messages != null))
                    gip.core.datamodel.Database.Root.Messages.LogDebug("ACActivator", recycled ? "Start ACInit(Pool)" : "Start ACInit(New)",
                        String.Format("Start={0:dd.MM.yyyy HH:mm:ss.ffff}; ThreadID={1}; Class={2}; InstanceCreationDepth={3}; ActivatingThread={4};",
                        DateTime.Now, Thread.CurrentThread.ManagedThreadId, acClass.ACIdentifier, currentThreadInACInit.InstanceDepth, currentThreadInACInit.Thread.ManagedThreadId));
#endif

                if (!newACObject.ACInit(startChildMode))
                {
                    string message = String.Format("ACInit(startChildMode) refused: {0}", acClass.GetACUrl());
                    if ((Database.Root != null) && (Database.Root.Messages != null))
                        Database.Root.Messages.LogError(acClass.GetACUrl(), "ACActivator.CreateInstance()", message);
                    currentThreadInACInit.InstanceDepth--;
                    RunPostInit(currentThreadInACInit, acClass);
                    currentThreadInACInit = null;
                    if (propagateException)
                        throw new ACCreateException(newACObject, message);
                    else
                        return null;
                }

#if DEBUG
                if (DiagWFLoadingCount > 0 && (Database.Root != null) && (Database.Root.Messages != null))
                    gip.core.datamodel.Database.Root.Messages.LogDebug("ACActivator", recycled ? "End ACInit(Pool)" : "End ACInit(New)",
                        String.Format("Start={0:dd.MM.yyyy HH:mm:ss.ffff}; ThreadID={1}; Class={2}; InstanceCreationDepth={3}; ActivatingThread={4};",
                        DateTime.Now, Thread.CurrentThread.ManagedThreadId, acClass.ACIdentifier, currentThreadInACInit.InstanceDepth, currentThreadInACInit.Thread.ManagedThreadId));
#endif

                if (newACObject is IACComponent && ((IACComponent)newACObject).IsProxy)
                    currentThreadInACInit.ProxyObjectsInvolved = true;
                currentThreadInACInit.InstanceDepth--;
                ACActivatorThread currentThreadInPostInit = currentThreadInACInit;
                currentThreadInACInit = null;

                if (newACObject is IACComponent && componentParent != null)
                    componentParent.AddChild(newACObject);
                currentThreadInPostInit.NewACObjectStack.Push(newACObject);

                if (currentThreadInPostInit.ProxyObjectsInvolved && currentThreadInPostInit.InstanceDepth <= 0 && newACObject is IACComponent)
                {
                    Database.Root.SendSubscriptionInfoToServer(true);
                }

                RunPostInit(currentThreadInPostInit, acClass);

                return newACObject;
            }
            catch (Exception e)
            {
                if (currentThreadInACInit != null)
                {
                    currentThreadInACInit.InstanceDepth--;
                    currentThreadInACInit = null;
                }
                if (newACObject != null)
                    newACObject.OnInitFailed(e);
                if (e is ACCreateException)
                    throw e;
                string message = String.Format("ACClass not created: {0}, {1}", acClass.GetACUrl(), e.Message);
                if ((Database.Root != null) && (Database.Root.Messages != null))
                {
                    Database.Root.Messages.LogException(acClass.GetACUrl(), "ACActivator.CreateInstance()", message);
                    if (e.InnerException != null)
                    {
                        string message2 = String.Format("ACClass not created: {0}, {1}", acClass.GetACUrl(), e.InnerException.Message);
                        Database.Root.Messages.LogException(acClass.GetACUrl(), "ACActivator.CreateInstance()", message2);
                    }
                    Database.Root.Messages.LogException(acClass.GetACUrl(), "ACActivator.CreateInstance()", e.StackTrace);
                }
                if (propagateException)
                    throw new ACCreateException(newACObject, message, e);
                else
                    return null;
            }
        }


        /// <summary>
        /// Gets the type of the AC object.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns>Type.</returns>
        public static Type GetACObjectType(ACClass acClass)
        {
            if (acClass.BaseClassWithASQN == null)
            {
                IRoot root = Database.Root;
                if (root != null)
                    root.Messages.LogError(acClass.GetACUrl(), "ACActivator.GetACObjectType()", String.Format("acClass.MyAssemblyACClass is null of {0}", acClass.GetACUrl()));
                return null;
            }
            if (acClass.BaseClassWithASQN.AssemblyQualifiedName.Length <= 0)
            {
                IRoot root = Database.Root;
                if (root != null)
                    root.Messages.LogError(acClass.GetACUrl(), "ACActivator.GetACObjectType()", String.Format("acClass.MyAssemblyACClass.AssemblyQualifiedName is Empty of {0}", acClass.GetACUrl()));
                return null;
            }
            return Type.GetType(acClass.BaseClassWithASQN.AssemblyQualifiedName);
        }

        #region public Members
        /// <summary>
        /// The _ threads initializing
        /// </summary>
        private static readonly ACMonitorObject _10010_LockThreadsInitializing = new ACMonitorObject(10010);
        private static List<ACActivatorThread> _ThreadsInitializing = new List<ACActivatorThread>();
        /// <summary>
        /// Gets the current initializing thread.
        /// </summary>
        /// <value>The current initializing thread.</value>
        public static ACActivatorThread CurrentInitializingThread
        {
            get
            {
                ACActivatorThread currentThread = null;

                using (ACMonitor.Lock(_10010_LockThreadsInitializing)) 
                {
                    var query = _ThreadsInitializing.Where(c => c.Thread == Thread.CurrentThread);
                    if (query.Any())
                        currentThread = query.First();
                    else
                    {
                        currentThread = new ACActivatorThread(Thread.CurrentThread);
                        _ThreadsInitializing.Add(currentThread);
                    }
                }
                return currentThread;
            }
        }

        /// <summary>
        /// The _ threads de initializing
        /// </summary>
        private static readonly ACMonitorObject _10010_LockThreadsDeInitializing = new ACMonitorObject(10010);
        private static List<ACActivatorThread> _ThreadsDeInitializing = new List<ACActivatorThread>();
        /// <summary>
        /// Gets the current de initializing thread.
        /// </summary>
        /// <value>The current de initializing thread.</value>
        public static ACActivatorThread CurrentDeInitializingThread
        {
            get
            {
                ACActivatorThread currentThread = null;

                using (ACMonitor.Lock(_10010_LockThreadsDeInitializing))
                {
                    var query = _ThreadsDeInitializing.Where(c => c.Thread == Thread.CurrentThread);
                    if (query.Any())
                        currentThread = query.First();
                    else
                    {
                        currentThread = new ACActivatorThread(Thread.CurrentThread);
                        _ThreadsDeInitializing.Add(currentThread);
                    }
                }
                return currentThread;
            }
        }

        /// <summary>
        /// Runs the post init.
        /// </summary>
        /// <param name="currentActivatingThread">The current activating thread.</param>
        /// <param name="acClass">acClass</param>
        private static void RunPostInit(ACActivatorThread currentActivatingThread, ACClass acClass)
        {
            if (currentActivatingThread.InstanceDepth <= 0)
            {
#if DEBUG
                if (DiagWFLoadingCount > 0 && (Database.Root != null) && (Database.Root.Messages != null))
                    gip.core.datamodel.Database.Root.Messages.LogDebug("ACActivator", "RunPostInit()",
                        String.Format("Start={0:dd.MM.yyyy HH:mm:ss.ffff}; ThreadID={1}; Class={2}; InstanceCreationDepth={3}; ActivatingThread={4};",
                        DateTime.Now, Thread.CurrentThread.ManagedThreadId, acClass.ACIdentifier, 0, currentActivatingThread.Thread.ManagedThreadId));
#endif

                // Kopie von Liste erstellen und löchen, weil Rekursionsgefahr besteht, wenn eine gerade erzeute Instanz im PostInit eine weitere Instanz versucht zu erzeugen
                var newACObjectList = currentActivatingThread.NewACObjectStack.ToArray();
                currentActivatingThread.NewACObjectStack.Clear();
                foreach (var acObject in newACObjectList)
                {
                    acObject.ACPostInit();
                    //if ((Database.Root != null) && (Database.Root.Messages != null))
                    //    Database.Root.Messages.LogDebug(acObject.GetACUrl(), "ACActivator.CreateInstance()", String.Format("ACPostInit() {0}", acObject.GetACUrl()));
                }

#if DEBUG
                if (DiagWFLoadingCount > 0 && (Database.Root != null) && (Database.Root.Messages != null))
                    gip.core.datamodel.Database.Root.Messages.LogDebug("ACActivator", "RunPostInit()",
                        String.Format("End={0:dd.MM.yyyy HH:mm:ss.ffff}; ThreadID={1}; Class={2}; InstanceCreationDepth={3}; ActivatingThread={4};",
                        DateTime.Now, Thread.CurrentThread.ManagedThreadId, acClass.ACIdentifier, 0, currentActivatingThread.Thread.ManagedThreadId));
#endif

                currentActivatingThread.PostOperationsQueue.Work();
                if (currentActivatingThread.InstanceDepth <= 0)
                {
                    using (ACMonitor.Lock(_10010_LockThreadsInitializing)) 
                    {
                        _ThreadsInitializing.Remove(currentActivatingThread);
                    }
                }

#if DEBUG
                if (DiagWFLoadingCount > 0 && (Database.Root != null) && (Database.Root.Messages != null))
                    gip.core.datamodel.Database.Root.Messages.LogDebug("ACActivator", "RunPostInit()",
                        String.Format("End={0:dd.MM.yyyy HH:mm:ss.ffff}; ThreadID={1}; Class={2}; InstanceCreationDepth={3}; ActivatingThread={4};",
                        DateTime.Now, Thread.CurrentThread.ManagedThreadId, acClass.ACIdentifier, 0, currentActivatingThread.Thread.ManagedThreadId));
#endif

            }
        }

        /// <summary>
        /// Runs the post de init.
        /// </summary>
        /// <param name="currentActivatingThread">The current activating thread.</param>
        public static void RunPostDeInit(ACActivatorThread currentActivatingThread)
        {
            if (currentActivatingThread.InstanceDepth <= 0)
            {
                currentActivatingThread.PostOperationsQueue.Work();
                if (currentActivatingThread.InstanceDepth <= 0)
                {
                    using (ACMonitor.Lock(_10010_LockThreadsDeInitializing))
                    {
                        _ThreadsDeInitializing.Remove(currentActivatingThread);
                    }
                }
            }
        }

        /// <summary>
        /// Adds to post init queue.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void AddToPostInitQueue(Action action)
        {
            CurrentInitializingThread.PostOperationsQueue.Add(action);
        }

        /// <summary>
        /// Adds to post de init queue.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void AddToPostDeInitQueue(Action action)
        {
            CurrentDeInitializingThread.PostOperationsQueue.Add(action);
        }
        #endregion


#if DEBUG
        private static Int64 _DiagWFLoadingCount = 0;
        public static DateTime _DiagStartWFLoading = DateTime.MinValue;
        public static DateTime _DiagEndWFLoading = DateTime.MinValue;
        private static object _DiagLock = new object();
        public static Int64 DiagWFLoadingCount
        {
            get
            {
                return Interlocked.Read(ref _DiagWFLoadingCount);
            }
        }
        private static StringBuilder _DiagSBBuilder = new StringBuilder();

        public static void DiagBeginInstanceNewWorkflow(string acUrlOfInvoker)
        {
            lock (_DiagLock)
            {
                _DiagSBBuilder = new StringBuilder();
                if (_DiagWFLoadingCount <= 0)
                    _DiagStartWFLoading = DateTime.Now;
                Interlocked.Increment(ref _DiagWFLoadingCount);
                gip.core.datamodel.Database.Root.Messages.LogDebug("ACComponent", "BeginInstanceNewWorkflow()",
                    String.Format("Start={0:dd.MM.yyyy HH:mm:ss.ffff}; countWF={1}; ACUrl={2}; ThreadID={3};",
                    DateTime.Now, _DiagWFLoadingCount, acUrlOfInvoker, Thread.CurrentThread.ManagedThreadId));
            }
        }

        public static void DiagEndInstanceNewWorkflow(string acUrlOfInvoker, string acUrlOfNewWF)
        {
            lock (_DiagLock)
            {
                if (_DiagWFLoadingCount <= 0)
                    return;
                Interlocked.Decrement(ref _DiagWFLoadingCount);
                _DiagEndWFLoading = DateTime.Now;
                gip.core.datamodel.Database.Root.Messages.LogDebug("ACComponent", "EndInstanceNewWorkflow()",
                    String.Format("Start={0:dd.MM.yyyy HH:mm:ss.ffff}; End={1:dd.MM.yyyy HH:mm:ss.ffff}; countWF={2}; ACUrl={3}; WFACUrl={4}; ThreadID={5};",
                    _DiagStartWFLoading, _DiagEndWFLoading, _DiagWFLoadingCount, acUrlOfInvoker, acUrlOfNewWF, Thread.CurrentThread.ManagedThreadId));
                gip.core.datamodel.Database.Root.Messages.LogDebug("ACComponent", "Actions()", _DiagSBBuilder.ToString());
            }
        }

        public static void LogActions(string message)
        {
            lock (_DiagLock)
            {
                if (_DiagWFLoadingCount <= 0)
                    return;
                _DiagSBBuilder.AppendFormat("{0:dd.MM.yyyy HH:mm:ss.ffff};{1};{2}|", DateTime.Now, message, Thread.CurrentThread.ManagedThreadId);
            }
        }
#endif

    }


}

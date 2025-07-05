using gip.core.datamodel;
using gip.core.datamodel.ACContainer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.Objects;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Xml;
using static gip.core.datamodel.Global;
using static Microsoft.Isam.Esent.Interop.EnumeratedColumn;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Baseclass for all Components
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACComponent" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IComparable" />
    /// <seealso cref="gip.core.datamodel.IACClassDesignProvider" />
    /// <seealso cref="gip.core.datamodel.IACMenuBuilder" />
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACComponent", "en{'ACComponent'}de{'ACComponent'}", typeof(ACComponent), "ACComponentChilds", Const.ACCaptionPrefix, Const.ACCaptionPrefix)]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Baseclass ACComponent'}de{'Basisklasse ACComponent'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, true)]
    public abstract partial class ACComponent : IACComponent, INotifyPropertyChanged, IComparable, IACClassDesignProvider, IACMenuBuilder
    {

        #region c´tors

        /// <summary>
        /// Every derivation of this class must implement/override a constructor with exactly the same signature
        /// </summary>
        /// <param name="acType">iPlus-Type-Information for constructing this component</param>
        /// <param name="content">Instances that are not derivatives of ACBSO (usually persistent instances in application trees) gets an ACClassTask object as "content" passed. 
        /// ACClassTask table entries ensure that the state of all persistable instances in the application trees is persisted and that after the restart of the iPlus service, 
        /// the last state of the process (all object states before the last shutdown) can be restored again. 
        /// For dynamic instances(derivatives of ACBSO, derivatives of PARole...) that are not persistable this paramater usually set to null.</param>
        /// <param name="parentACObject">Parent-ACComponent under which this instance is created as a child object.</param>
        /// <param name="parameter">Individual construction parameters are passed via ACValueList. An ACValueList is a list that contains entries from ACValue. ACValue is a class that has essentially three properties:<para />
        /// string ACIdentifier: Unique ID/Name of the parameter<para />
        /// object Value: parameter value<para />
        /// Type ObjectFullType: Datatype of parameter value<para />
        /// If you want to start a component yourself you need to instance ACValueList.
        /// So that you don't have to enter the parameter entries yourself, call the ACClass.TypeACSignature () method of the corresponding iPlus type you want to instance (= parameter acType). 
        /// The iPlus framework through the ACClassConstructorInfo-attribute class knows which can be inserted in addition to the ACClassInfo attribute class prior to class declaration.</param>
        /// <param name="acIdentifier">Unique ID, within the child instances of the Parent ACComponent, which this instance will receive. 
        /// If string is empty, the runtime assgins the ID itself using the ACType parameter identifier.</param>
        public ACComponent(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            _ACMemberDict = new ACMemberIndexer<string, IACMember>(i => GetOrCreateACMember(i), (i, m) => AddOrReplaceACMember(m));
            InitState = ACInitState.Constructing;
            Construct(acType, content, parentACObject, parameter, acIdentifier);
            InitState = ACInitState.Constructed;

            _ReferenceList = new ACPointReference(this, "ReferenceList");
            _ACPostInitEvent = new ACPointEvent(this, "ACPostInitEvent", 0);
            _ACDeInitEvent = new ACPointEvent(this, "ACDeInitEvent", 0);
            _ChildAddedEvent = new ACPointEvent(this, "ChildAddedEvent", 0);
            _ChildRemovedEvent = new ACPointEvent(this, "ChildRemovedEvent", 0);
        }


        /// <summary>
        /// This method is called from the iPlus-Framework when the component was taken out of the pool
        /// It calls the method Construct(). 
        /// It sets the InitState to "RecyclingFromPool" before and afterwards to "RecycledFromPool"
        /// </summary>
        /// <param name="content"></param>
        /// <param name="parentACObject"></param>
        /// <param name="parameter"></param>
        /// <param name="acIdentifier"></param>
        public virtual void Recycle(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            if (InitState != ACInitState.DisposedToPool)
                return;
            InitState = ACInitState.RecyclingFromPool;
            foreach (var member in ACMemberList)
            {
                member.RecycleMemberAndAttachTo(this);
            }
            Construct(this.ComponentClass, content, parentACObject, parameter, acIdentifier);
            InitState = ACInitState.RecycledFromPool;
        }

        /// <summary>
        /// This method is called from the iPlus-Framework when the component should be reloaded after changes in the development has taken place.<para />
        /// 1. The InitState switches to "Reloading"<para />
        /// 2. The Stop()-Method will be called<para />
        /// 3. Construct() will be called<para />
        /// 4. The InitState switches to "Reloaded"<para />
        /// 5. ACInit() will be called<para />
        /// </summary>
        /// <param name="content"></param>
        /// <param name="parentACObject"></param>
        /// <param name="parameter"></param>
        /// <param name="acIdentifier"></param>
        public virtual void Reload(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            if (InitState != ACInitState.Initialized)
                return;
            InitState = ACInitState.Reloading;
            Stop();
            ComponentClass.RefreshCachedMethods();
            Construct(this.ComponentClass, content, parentACObject, parameter, acIdentifier);
            ACInit();
            InitState = ACInitState.Reloaded;
            ACPostInit();
        }


        /// <summary>
        /// This method is called from the iPlus-Framework for each member of a ACComponent when a component was recycled from the component-pool (ACInitState.RecycledFromPool) instead of a new creation.
        /// </summary>
        /// <param name="recycledComponent">The recycled component.</param>
        public void RecycleMemberAndAttachTo(IACComponent recycledComponent)
        {
            if (recycledComponent != null && ACRef != null)
            {
                ACRef.ValueT = recycledComponent;
            }
        }


        /// <summary>
        /// Construct() assigns the ACIdentifier the ParentACComponent
        /// </summary>
        /// <param name="acType"></param>
        /// <param name="content"></param>
        /// <param name="parentACObject"></param>
        /// <param name="parameter"></param>
        /// <param name="acIdentifier"></param>
        protected virtual void Construct(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            int creationToken = ACActivator.GetNextCreationToken(this);
            if (parentACObject is ACComponent && content == null)
            {
                ACComponent parentACComponent = parentACObject as ACComponent;

                if (!acType.IsMultiInstanceInherited
                    && acType.ACStorableType >= Global.ACStorableTypes.Optional
                    && parentACComponent.ContentTask != null
                    && parentACComponent.ACOperationMode == ACOperationModes.Live)
                {
                    if (parentACComponent.ContentTask.EntityState != System.Data.EntityState.Detached)
                    {
                        ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                        {
                            content = s_cQry_ContentTaskByIdentifier(ACClassTaskQueue.TaskQueue.Context, parentACComponent.ContentTask.ACClassTaskID, acType.ACIdentifier);
                        });
                    }

                    if (!IsProxy && content == null)
                    {
                        // Dürfte eigentlich nicht vorkommen
                        throw new NotImplementedException();
                    }
                }
            }
            Content = content;

            if (_ACRefParent != null)
            {
                if (parentACObject == null)
                    _ACRefParent = null;
                else
                    _ACRefParent.ValueT = parentACObject as IACComponent;
            }
            else if (parentACObject != null)
                _ACRefParent = new ACRef<IACComponent>(parentACObject as IACComponent, this, false);

            if (ContentTask != null)
            {
                if (ContentTask.TaskTypeACClassReference.IsLoaded)
                {
                    using (ACMonitor.Lock(_20015_LockValue))
                    {
                        if (_ACTypeFromLiveContext == null)
                        {
                            _ACTypeFromLiveContext = ContentTask.TaskTypeACClass;
                        }
                    }
                }
                if (ParentACComponent != null && ParentACComponent.ACOperationMode == ACOperationModes.Simulation)
                    _ACOperationMode = ACOperationModes.Simulation;
                //else if (ContentTask.IsTestmode) // Testmode mit ContentTask darf es nicht geben
                //_ACOperationMode = ACOperationModes.Test;
            }
            else if (ParentACComponent != null)
            {
                _ACOperationMode = ParentACComponent.ACOperationMode;
                if (acIdentifier == Const.ACRootProjectNameTest)
                    _ACOperationMode = ACOperationModes.Test;
            }


            _Parameters = parameter;

            ACClassInfo attributeClass = GetType().GetCustomAttributes(typeof(ACClassInfo), false)[0] as ACClassInfo;

            ACType = acType;
            _ACIdentifier = acIdentifier;

            InitACIdentifier(parameter, ref _ACIdentifier);

            if (String.IsNullOrEmpty(_ACIdentifier))
            {
                if (ContentTask != null)
                {
                    _ACIdentifier = ContentTask.ACIdentifier;
                }
                else
                {
                    _ACIdentifier = ACType.ACIdentifier;
                    if (acType.IsMultiInstanceInherited)
                    {
                        int minInstanceNo = -1;
                        int maxInstanceNo = -1;
                        ACActivator.AquireNextInstanceNo(this, _ACIdentifier, out minInstanceNo, out maxInstanceNo);
                        if (maxInstanceNo <= 0)
                            maxInstanceNo = 1;
                        string acNameInstance = maxInstanceNo.ToString();
                        _ACIdentifier += "(" + acNameInstance + ")";
                    }
                }
            }
        }


        /// <summary>
        /// Returns the highest Instance-No currently in use (for dynamic instances)
        /// </summary>
        /// <param name="acIdentifierPrefix"></param>
        /// <param name="minInstanceNo"></param>
        /// <param name="maxInstanceNo"></param>
        public void GetMaxInstanceNoOfChilds(string acIdentifierPrefix, out int minInstanceNo, out int maxInstanceNo)
        {
            ACActivator.GetMaxInstanceNoOfChilds(ParentACComponent, acIdentifierPrefix, out minInstanceNo, out maxInstanceNo);
        }

        #endregion


        #region Initialisierung und Laden/Entladen
        private class SafeTaskType
        {
            public ACClassTask Task { get; set; }
            public ACClass TaskType { get; set; }
        }

        /// <summary>
        /// The ACInit method is called directly after construction. 
        /// You can also overwrite the ACInit method. 
        /// When booting or dynamically reloading ACComponent trees, such as loading a workflow, the trees pass through the "Depth-First" + "Pre-Order" algorithm. 
        /// This means that the generation of child ACComponents is always carried out in depth first and then the next ACComponent at the same recursion depth.<para />
        /// The algorithm is executed in the ACInit method of the ACComponent class. 
        /// Therefore, you must always make the base call. 
        /// This means that as soon as you execute your initialization logic after the basic call, you know that the child components were created and that they are in initialization state.
        /// </summary>
        /// <param name="startChildMode">The persisted start mode from database</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        public virtual bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (InitState == ACInitState.Constructed)
                InitState = ACInitState.Initializing;

            ACInitScriptEngine();
            ACInitACProperties();
            if (this.ContentTask != null)
                this.ContentTask.IsACComponentInitialized = true;

            // Childs von Proxy-Objekte sollen grundsätzlich nur bei Bedarf/Zugriff instanziiert werden
            // Falls die Programminstanz ein Model-Server ist, dann sollen alle Proxy-Objekte von Models,
            // bei denen er Client ist, sofot instanziiert werden aufgrund von Propery-Bindings.
            if (!IsProxy || this.Root.HasACModelServer)
            {
                if (!this.IsProxy)
                {
                    ApplicationManager appManager = FindParentComponent<ApplicationManager>();
                    if (appManager != null)
                    {
                        appManager.ACCompTypeDict.AddComponent(this);
                        appManager.ACCompUrlDict.AddComponent(this);
                    }
                }
                //#if DEBUG
                //                if (System.Diagnostics.Debugger.IsAttached)
                //                {
                //                    System.Diagnostics.Debug.WriteLine(this.GetACUrl());
                //                }
                //#endif

                // 1. Lade zuerst Modelkomponenten
                foreach (ACClass acClassOfChild in ComponentClass.Childs.Where(c => c.ACStartType > Global.ACStartTypes.None && c.ACStartType <= startChildMode))
                {
                    ACClassTask acClassTaskChild = null;
                    // Starte Komponente ohne ACClassTask
                    if (acClassOfChild.ACStorableType == Global.ACStorableTypes.NotStorable)
                    {
                        acClassTaskChild = null;
                    }
                    // Sonst Persistierbar bzw. Applicationprojects sollten immer persistierbar sein
                    else
                    {
                        // Falls childs überhaupt perisistiert werden können: ContentTask darf nicht null sein 
                        if (this.ContentTask != null)
                        {
                            if (ContentTask.EntityState != System.Data.EntityState.Detached)
                            {
                                ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                                {
                                    acClassTaskChild = s_cQry_ContentTaskByClassID(ACClassTaskQueue.TaskQueue.Context, this.ContentTask.ACClassTaskID, acClassOfChild.ACClassID);
                                });
                            }
                            //var queryT = acClassOfChild.ACClassTask_TaskTypeACClass.Where(c => c.IsTestmode == this.ContentTask.IsTestmode);
                            // Falls keine Child-ACClassTask angelegt, erzeuge eine neue Task
                            if (acClassTaskChild == null)
                            {
                                ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                                {
                                    acClassTaskChild = ACClassTask.NewACObject(ACClassTaskQueue.TaskQueue.Context, this.ContentTask);
                                    acClassTaskChild.TaskTypeACClass = ACClassTaskQueue.TaskQueue.Context.ACClass.Where(c => c.ACClassID == acClassOfChild.ACClassID).First();
                                    acClassTaskChild.ContentACClassWF = null;
                                    acClassTaskChild.ACTaskType = Global.ACTaskTypes.ModelTask;
                                    acClassTaskChild.IsDynamic = false;
                                    acClassTaskChild.ACIdentifier = acClassOfChild.ACIdentifier;
                                    ACClassTaskQueue.TaskQueue.Context.ACClassTask.AddObject(acClassTaskChild);
                                });
                            }
                            //else
                            //{
                            //    acClassTaskChild = queryT.First();
                            //}
                        }
                        else
                        {
                            if (acClassOfChild.ACStorableType == Global.ACStorableTypes.Required)
                            {
                                Messages.LogError(this.GetACUrl(), "ACComponent.ACInit()", String.Format("Child {0} not persistable because parent {1} not storable!", acClassOfChild.ACUrlComponent, this.ComponentClass.ACUrlComponent));
                            }
                        }
                    }

                    bool startComponent = true;
                    if (this is ACRoot)
                        startComponent = !this.ACComponentChilds.Where(c => c.ACIdentifier == acClassOfChild.ACIdentifier).Any();

                    if (startComponent)
                        StartComponent(acClassOfChild, acClassTaskChild != null ? acClassTaskChild : (object)acClassOfChild, null, startChildMode, IsProxy);
                }

                // 2. Lade dynamisch instanzierte ACComponents (Workflows), die zuletzt geladen waren
                if (ContentTask != null && !(this is ACRoot))
                {
                    IEnumerable<ACClassTask> subTasks = null;
                    IEnumerable<SafeTaskType> querySafeTaskType = null;
                    if (ContentTask.EntityState != System.Data.EntityState.Detached)
                    {
                        ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                        {
                            subTasks = s_cQry_SubTasks(ACClassTaskQueue.TaskQueue.Context, this.ContentTask.ACClassTaskID)
                                        .ToArray()
                                        .Where(c => !c.IsACComponentInitialized);
                            if (subTasks != null && subTasks.Any())
                                querySafeTaskType = subTasks.Select(c => new SafeTaskType() { Task = c, TaskType = c.TaskTypeACClass }).ToArray();
                        });
                    }

                    if (querySafeTaskType != null && querySafeTaskType.Any())
                    {
                        foreach (var acClassTaskChild in querySafeTaskType)
                        {
                            if (acClassTaskChild.TaskType == null)
                            {
                                continue;
                            }
                            if (acClassTaskChild.TaskType.ACStartType <= startChildMode)
                            {
                                ACClass acClassOfChild = null;

                                using (ACMonitor.Lock(this.Root.Database.ContextIPlus.QueryLock_1X000))
                                {
                                    acClassOfChild = this.Root.Database.ContextIPlus.GetACType(acClassTaskChild.TaskType.ACClassID);
                                }
                                if (acClassOfChild != null)
                                    StartComponent(acClassOfChild, acClassTaskChild.Task, null, startChildMode, IsProxy);
                            }
                        }
                    }
                }
            }
            else if (ACOperationMode == ACOperationModes.Test)
            {
                foreach (var acEntityChild in ComponentClass.Childs.Where(c => c.ACStartType > Global.ACStartTypes.None &&
                                                                                c.ACStartType <= startChildMode &&
                                                                                c.ACKindIndex != (short)Global.ACKinds.TACDBAManager &&
                                                                                c.ACKindIndex != (short)Global.ACKinds.TACCommunications))
                {
                    StartComponent(acEntityChild, null, null, startChildMode, false);
                }
            }
            return true;
        }

        protected ACPointEvent _ACPostInitEvent;
        /// <summary>
        /// Eventpoint for a Event, that is raised, when ACPostInit() is completed
        /// </summary>
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent ACPostInitEvent
        {
            get
            {
                return _ACPostInitEvent;
            }
            set
            {
                _ACPostInitEvent = value;
            }
        }

        public virtual void OnInitFailed(Exception reason)
        {
        }

        /// <summary>
        /// After all new instances have been initialized by the pre-initialization phase, the post-initialization phase starts.
        /// After passing the same Depth-First + Post-Order algorithm, the ACPostInit method is called.
        /// The parent element is passed if there are no more child elements at the same recursion depth, starting with the lowest recursion depth.<para />
        /// You can also overwrite the ACPostInit method to program the remaining initialization logic. 
        /// If these are dependent on all instances having to exist. 
        /// In the Base-ACPostInit method of the ACComponent, the remaining property bindings are performed so that the target properties then have the value of the bound source property. 
        /// Therefore, you must always execute the base call when overwriting this method.
        /// </summary>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        [ACMethodInfo("ACComponent", "en{'ACPostInit'}de{'ACPostInit'}", 9999)]
        public virtual bool ACPostInit()
        {
            ACPostInitACProperties();
            //ACState = Global.ACStates.Idle;
            //ACProcessPhase = Global.ACProcessPhases.Idle;
            PostExecute("ACPostInit");
            ACPostInitEvent.Raise(new ACEventArgs());
            InitState = ACInitState.Initialized;

            return true;
        }

        protected ACPointEvent _ACDeInitEvent;
        /// <summary>
        /// Eventpoint for a Event, that is raised, when ACDeInit() is completed
        /// </summary>
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent ACDeInitEvent
        {
            get
            {
                return _ACDeInitEvent;
            }
            set
            {
                _ACDeInitEvent = value;
            }
        }

        /// <summary>
        /// Initiates  Disposing to Pool
        /// </summary>
        /// <returns>true if component is disposed to pool, false if iPlus is Shutting down</returns>
        protected bool InitiateDisposingToPool()
        {
            if (InitState == ACInitState.Initialized)
            {
                if (this.Root.ComponentPool.CanBePooled(this))
                {
                    InitState = ACInitState.DisposingToPool;
                    return true;
                }
                else
                    InitState = ACInitState.Destructing;
            }
            return false;
        }

        public bool WillBeDisposedToPool
        {
            get
            {
                if (this.Root.ComponentPool.CanBePooled(this))
                    return true;
                return false;
            }
        }

        /// <summary>
        /// The termination of an ACComponent instance is initiated by the StopComponent() method.
        /// The ACPreDeInit method informs the affected child instances in advance that deinitialization is taking place.
        /// The ACPreDeInit is called after the "Depth-First" + "Pre-Order" algorithm.        
        /// </summary>
        /// <param name="deleteACClassTask">Should instance be removed from persistable application tree.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        virtual public bool ACPreDeInit(bool deleteACClassTask = false)
        {
            foreach (IACComponent acSubACComponent in ACComponentChilds.ToArray())
            {
                if (acSubACComponent == null || !(acSubACComponent is ACComponent))
                    continue;
                if (!acSubACComponent.ACPreDeInit(deleteACClassTask))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// After the notification of all affected instances by calling the ACPreDeInit method, the actual deinitialization starts. 
        /// This runs through the instances according to the "Depth-First" + "Post-Order" algorithm and calls the ACDeInit method. 
        /// You should overwrite this so that you can release references or unsubscribe from events, among other things.<para />
        /// Don't forget to make the base call! The basic ACDeInit method of theCComponent executes some functions to clear up the garbage collector. Before calling this function, first:
        /// the state is set to Destructing.
        /// If the instance is "poolable" (IsDisposable == true), it is given the status DisposingToPool instead.
        /// At the end of the deinitialization process:
        /// the state is set to Destructed,
        /// or for a "poolable" instance on DisposedToPool.In this case, the instance is not cleaned up by the.NET garbage collector, but waits in a pool to be reactivated.
        /// </summary>
        /// <param name="deleteACClassTask">Should instance be removed from persistable application tree.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        public virtual bool ACDeInit(bool deleteACClassTask = false)
        {
            InitiateDisposingToPool();

            ApplicationManager appManager = FindParentComponent<ApplicationManager>();
            if (appManager != null)
            {
                appManager.ACCompTypeDict.RemoveComponent(this);
                appManager.ACCompUrlDict.RemoveComponent(this);
            }

            if (ACDeInitEvent != null)
                ACDeInitEvent.Raise(new ACEventArgs());

            if (ParentACComponent != null)
            {
                ((ACComponent)ParentACComponent).ACComponentChilds_Remove(this);
            }

            ACDeInitACProperties(deleteACClassTask);
            if (_ReferenceList != null)
                _ReferenceList.DetachAndClear();
            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (_ACPropertyConfigValueList != null)
                    _ACPropertyConfigValueList.ForEach(c => c.IsCachedValueSet = false);
            }

            if (InitState == ACInitState.Destructing)
            {
                if (_ACMemberDict != null)
                {
                    _ACMemberDict.ACDeInit();
                    _ACMemberDict = null;
                }
                if (_ReferenceList != null)
                {
                    _ReferenceList.DetachAndClear();
                    _ReferenceList = null;
                }
                if (_ACPostInitEvent != null)
                {
                    _ACPostInitEvent.RemoveAll();
                    _ACPostInitEvent.DetachACObject();
                    _ACPostInitEvent = null;
                }
                if (_ACDeInitEvent != null)
                {
                    _ACDeInitEvent.RemoveAll();
                    _ACDeInitEvent.DetachACObject();
                    _ACDeInitEvent = null;
                }
                if (_ChildAddedEvent != null)
                {
                    _ChildAddedEvent.RemoveAll();
                    _ChildAddedEvent.DetachACObject();
                    _ChildAddedEvent = null;
                }
                if (_ChildRemovedEvent != null)
                {
                    _ChildRemovedEvent.RemoveAll();
                    _ChildRemovedEvent.DetachACObject();
                    _ChildRemovedEvent = null;
                }
            }

            if (_ACRefParent != null)
                _ACRefParent.Detach();
            if (InitState == ACInitState.Destructing)
                _ACRefParent = null;

            PropertiesReceived = false;
            if (InitState == ACInitState.DisposingToPool)
                InitState = ACInitState.DisposedToPool;
            else if (InitState == ACInitState.Destructing)
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    _ACMemberList = null;
                }

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _ACType = null;
                    _ACClassMethodsCached = null;
                    _ACTypeFromLiveContext = null;
                }
                InitState = ACInitState.Destructed;
            }
            _Parameters = null;
            if (_Database != null)
            {
                ACObjectContextManager.DisposeAndRemove(_Database);
                _Database = null;
            }

            if (InitState == ACInitState.DisposedToPool)
            {
                this.Root.ComponentPool.Push(this, true);
            }
            return true;
        }

        #endregion


        #region Searching

        #region Parents
        /// <summary>
        /// Searches for a instance in the Parent-Relation which .NET-Type is a derivation or same .NET-Type as the passed type
        /// </summary>
        /// <param name="type">.NET Type</param>
        /// <returns>First occurrence or null if not found</returns>
        [ACMethodInfo("ACComponent", "en{'Find Parent Component'}de{'Find Parent Component'}", 9999)]
        public IACComponent FindParentComponent(Type type)
        {
            if (ParentACComponent == null)
                return null;
            if (type.IsAssignableFrom(ParentACComponent.GetType()))
                return ParentACComponent;
            return ParentACComponent.FindParentComponent(type);
        }

        /// <summary>
        /// Searches for a instance in the Parent-Relation which is of type TResult or a derivation of it
        /// </summary>
        /// <typeparam name="TResult">Class</typeparam>
        /// <returns>Null if not found</returns>
        public TResult FindParentComponent<TResult>() where TResult : IACComponent
        {
            var found = FindParentComponent(typeof(TResult));
            if (found == null)
                return default(TResult);
            return (TResult)found;
        }

        /// <summary>
        /// Searches for a instance in the Parent-Relation which is of type TResult or a derivation of it 
        /// and matches the passed search-condition
        /// </summary>
        /// <typeparam name="TResult">Class</typeparam>
        /// <param name="selector">Search Condition</param>
        /// <returns>Null if not found</returns>
        public TResult FindParentComponent<TResult>(Func<IACComponent, bool> selector) where TResult : IACComponent
        {
            if (ParentACComponent == null || selector == null)
                return default(TResult);
            if (selector(ParentACComponent))
                return (TResult)ParentACComponent;
            return ParentACComponent.FindParentComponent<TResult>(selector);
        }

        /// <summary>
        /// Searches for a instance in the Parent-Relation which ACClass is derived from the passed ACClass
        /// </summary>
        /// <param name="type">iPlus-Type</param>
        /// <returns>First occurrence or null if not found</returns>
        [ACMethodInfo("ACComponent", "en{'Find Parent Component'}de{'Find Parent Component'}", 9999)]
        public IACComponent FindParentComponent(ACClass type)
        {
            if (ParentACComponent == null)
                return null;
            ACClass parentACClass = ParentACComponent.ComponentClass;
            if (parentACClass.IsDerivedClassFrom(type))
                return ParentACComponent;
            return ParentACComponent.FindParentComponent(type);
        }
        #endregion

        #region Children
        /// <summary>
        /// Searches for a child-instances in the subtree which is of type TResult or a derivation of it
        /// DEPRECATED! Please use FindChildComponents-Method with selector instead
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="maxChildDepth">Max search depth in subtree</param>
        /// <returns>A List of found children</returns>
        public List<TResult> FindChildComponents<TResult>(int maxChildDepth = 0) where TResult : IACComponent
        {
            List<TResult> listToFill = new List<TResult>();
            int currentDepth = 0;
            FindChildACComponentsIntern<TResult>(typeof(TResult), ref listToFill, currentDepth, maxChildDepth);
            return listToFill;
        }

        private void FindChildACComponentsIntern<TResult>(Type type, ref List<TResult> listToFill, int currentDepth, int maxChildDepth) where TResult : IACComponent
        {
            // Must be next recursion depth because starting point is not a child itself
            if (currentDepth > 0)
            {
                if (type.IsAssignableFrom(this.GetType()))
                {
                    listToFill.Add((TResult)(object)this);
                }
            }
            currentDepth++;
            if ((maxChildDepth > 0) && (currentDepth > maxChildDepth))
                return;
            foreach (IACComponent child in this.ACComponentChilds)
            {
                (child as ACComponent).FindChildACComponentsIntern<TResult>(type, ref listToFill, currentDepth, maxChildDepth);
            }
        }

        /// <summary>
        /// Searches for a child-instances in the subtree which is of type TResult or a derivation of it
        /// and matches the passed search-condition (selector) and matches the passed ignore-condition (deselector) 
        /// If ignore-condition is matched, the recursive search-alogrithm breaks and doesn't continue to look further in the current subtree
        /// </summary>
        /// <typeparam name="TResult">Class</typeparam>
        /// <param name="selector">search-condition, can be null</param>
        /// <param name="deselector">ignore/break-condition, can be null. If deselector equals the selector, then the first matching node will be returned.</param>
        /// <param name="maxChildDepth">Max search depth in subtree</param>
        /// <returns>A List of found children</returns>
        public List<TResult> FindChildComponents<TResult>(Func<IACComponent, bool> selector, Func<IACComponent, bool> deselector = null, int maxChildDepth = 0) where TResult : IACComponent
        {
            List<TResult> listToFill = new List<TResult>();
            int currentDepth = 0;
            FindChildACComponentsIntern<TResult>(selector, deselector, ref listToFill, currentDepth, maxChildDepth);
            return listToFill;
        }

        private void FindChildACComponentsIntern<TResult>(Func<IACComponent, bool> selector, Func<IACComponent, bool> deselector, ref List<TResult> listToFill, int currentDepth, int maxChildDepth) where TResult : IACComponent
        {
            if (selector == null)
                return;
            // Must be next recursion depth because starting point is not a child itself
            if (currentDepth > 0)
            {
                if (selector(this))
                {
                    if (typeof(TResult).IsAssignableFrom(this.GetType()))
                        listToFill.Add((TResult)(object)this);
                }
                if (deselector != null && deselector(this))
                    return;
            }
            currentDepth++;
            if ((maxChildDepth > 0) && (currentDepth > maxChildDepth))
                return;
            foreach (IACComponent child in this.ACComponentChilds)
            {
                (child as ACComponent).FindChildACComponentsIntern<TResult>(selector, deselector, ref listToFill, currentDepth, maxChildDepth);
            }
        }

        /// <summary>
        /// Searches for a child-instances in the subtree which .NET-Type is a derivation or same .NET-Type as the passed type
        /// </summary>
        /// <param name="typeOfComponent">.NET Type</param>
        /// <param name="maxChildDepth">Max search depth in subtree</param>
        /// <returns>A List of found children</returns>
        [ACMethodInfo("ACComponent", "en{'Find Child Components'}de{'Find Child Components'}", 9999)]
        public List<IACComponent> FindChildComponents(Type typeOfComponent, int maxChildDepth = 0)
        {
            List<IACComponent> listToFill = new List<IACComponent>();
            int currentDepth = 0;
            FindChildACComponentsIntern(typeOfComponent, ref listToFill, currentDepth, maxChildDepth);
            return listToFill;
        }

        private void FindChildACComponentsIntern(Type type, ref List<IACComponent> listToFill, int currentDepth, int maxChildDepth)
        {
            // Must be next recursion depth because starting point is not a child itself
            if (currentDepth > 0)
            {
                if (type.IsAssignableFrom(this.GetType()))
                {
                    listToFill.Add((IACComponent)(object)this);
                }
            }
            currentDepth++;
            if ((maxChildDepth > 0) && (currentDepth > maxChildDepth))
                return;
            foreach (IACComponent child in this.ACComponentChilds)
            {
                (child as ACComponent).FindChildACComponentsIntern(type, ref listToFill, currentDepth, maxChildDepth);
            }
        }

        /// <summary>
        /// Searches for a child-instances in the subtree which ACClass is derived from the passed ACClass
        /// </summary>
        /// <param name="acClass">iPlus-Type</param>
        /// <param name="maxChildDepth">Max search depth in subtree</param>
        /// <returns>A List of found children</returns>
        [ACMethodInfo("ACComponent", "en{'Find Child Components'}de{'Find Child Components'}", 9999)]
        public List<IACComponent> FindChildComponents(ACClass acClass, int maxChildDepth = 0)
        {
            List<IACComponent> listToFill = new List<IACComponent>();
            int currentDepth = 0;
            FindChildACComponentsIntern(acClass, ref listToFill, currentDepth, maxChildDepth);
            return listToFill;
        }

        private void FindChildACComponentsIntern(ACClass acClass, ref List<IACComponent> listToFill, int currentDepth, int maxChildDepth)
        {
            if (this.ACType == null)
                return;
            // Must be next recursion depth because starting point is not a child itself
            if (currentDepth > 0)
            {
                if (this.ComponentClass.IsDerivedClassFrom(acClass))
                    listToFill.Add(this);
            }
            currentDepth++;
            if ((maxChildDepth > 0) && (currentDepth > maxChildDepth))
                return;
            foreach (IACComponent child in this.ACComponentChilds)
            {
                (child as ACComponent).FindChildACComponentsIntern(acClass, ref listToFill, currentDepth, maxChildDepth);
            }
        }

        protected IACComponent FindMemberACComponent(Type type)
        {
            if (!this.ACComponentChilds.Any())
                return null;
            return this.ACComponentChilds.Where(c => type.IsAssignableFrom(c.GetType())).FirstOrDefault();
        }
        #endregion

        #endregion


        #region Starting & Stopping
        /// <summary>
        /// Stops a Child-Component by acIdentifier
        /// </summary>
        /// <param name="acIdentifier">ID of Child</param>
        /// <param name="deleteACClassTask">Removes this component from the persisted Application-Tree. 
        /// The component will not started any more while iPlus-Service is restarting next time. 
        /// This should only be done for dynamic instances like Workflow-Classes (Derivation of PWBase)</param>
        /// <returns>True if component was found and stopped successfully</returns>
        [ACMethodInfo("ACComponent", "en{'Stop ACComponent'}de{'Stop ACComponent'}", 9999)]
        public bool StopComponent(string acIdentifier, bool deleteACClassTask = false)
        {
            IACComponent acComponent = GetChildComponent(acIdentifier);
            if (acComponent == null)
                return false;
            return StopComponent(acComponent, deleteACClassTask);
        }

        /// <summary>
        /// Stops the passed Component
        /// </summary>
        /// <param name="acComponent">Component to stop</param>
        /// <param name="deleteACClassTask">>Removes this component from the persisted Application-Tree. 
        /// The component will not started any more while iPlus-Service is restarting next time. 
        /// This should only be done for dynamic instances like Workflow-Classes (Derivation of PWBase)</param>
        /// <returns>True if passed component was stopped successfully</returns>
        [ACMethodInfo("ACComponent", "en{'Stop ACComponent'}de{'Stop ACComponent'}", 9999)]
        public bool StopComponent(IACComponent acComponent, bool deleteACClassTask = false)
        {
            ACActivatorThread currentDeInitThread = ACActivator.CurrentDeInitializingThread;

            if (currentDeInitThread.InstanceDepth <= 0)
            {
                currentDeInitThread.ProxyObjectsInvolved = false;
                if (!acComponent.ACPreDeInit(deleteACClassTask))
                    return false;
            }
            if (acComponent == null)
                return false;
            currentDeInitThread.InstanceDepth++;

            foreach (IACComponent acSubACComponent in acComponent.ACComponentChilds.ToArray())
            {
                if (acSubACComponent == null || !(acSubACComponent is ACComponent))
                    continue;
                if (!((ACComponent)acComponent).StopComponent(acSubACComponent, deleteACClassTask))
                {
                    currentDeInitThread.InstanceDepth--;
                    return false;
                }
            }

            bool succ = acComponent.ACDeInit(deleteACClassTask);
            if (succ)
            {
                if (acComponent.IsProxy)
                    currentDeInitThread.ProxyObjectsInvolved = true;
            }

            currentDeInitThread.InstanceDepth--;
            if (succ)
            {
                ACComponentChilds_Remove(acComponent);
            }

            if (currentDeInitThread.ProxyObjectsInvolved && (currentDeInitThread.InstanceDepth <= 0))
            {
                this.Root.SendSubscriptionInfoToServer(true);
            }
            ACActivator.RunPostDeInit(currentDeInitThread);

            return succ;
        }

        /// <summary>
        /// Stops itself
        /// </summary>
        /// <returns>True if stop was successfull</returns>
        [ACMethodInfo("ACComponent", "en{'Stop'}de{'Stop'}", 9999)]
        public virtual bool Stop()
        {
            if (ParentACComponent == null)
                return false;
            return ParentACComponent.StopComponent(this);
        }

        /// <summary>
        /// Reloads itself
        /// </summary>
        [ACMethodInfo("ACComponent", "en{'Reload'}de{'Reload'}", 9999)]
        public void Reload()
        {
            if (ParentACComponent == null)
                return;
            Reload(this.Content, ParentACComponent, this.Parameters, this.ACIdentifier);
        }


        internal virtual void FinalizeComponent()
        {
        }

        internal virtual void SubscribeAtServer(bool force = false, bool autoSendToServer = false)
        {
        }

        internal virtual void UnSubscribeAtServer(bool force = false, bool autoSendToServer = false)
        {
        }

        /// <summary>Gets the ACClass (Metainformation) for the passed acClassName.
        /// The method searches for a ACClass in the Child-Relationship first.
        /// If nothing was found and parameter searchForChildInBaseClasses was set to true, than the search is extended over the entire class-hierarchy.
        /// If nothing was found and parameter searchForGlobals was set to true, than a global BSO (TACBSOGlobal) or global report (TACBSOReport) is searched.</summary>
        /// <param name="acClassName">Classname (ACIdentifier of the class)</param>
        /// <param name="searchForChildInBaseClasses">If set to true, than the search is extended over the entire class-hierarchy.</param>
        /// <param name="searchForGlobals">If set to true, than a global BSO (TACBSOGlobal) or global report (TACBSOReport) is searched.</param>
        /// <returns>ACClass if child-class or global class was found</returns>
        protected ACClass GetACClassFromACClassName(string acClassName, bool searchForChildInBaseClasses = false, bool searchForGlobals = true)
        {
            ACClass acClass = null;
            ACClass acType = ComponentClass;
            if (acType == null)
                return null;
            string acClassNameSearch = acClassName;
            acClass = acType.Childs.Where(c => c.ACIdentifier == acClassNameSearch).FirstOrDefault();
            if (acClass != null)
                return acClass;
            else if (searchForChildInBaseClasses
                    || acType.ACKind == Global.ACKinds.TACBSO
                    || acType.ACKind == Global.ACKinds.TACBSOGlobal)
            {
                ACClass baseClass = acType;
                using (ACMonitor.Lock(baseClass.Database.QueryLock_1X000))
                {
                    while (baseClass.ACClass1_BasedOnACClass != null)
                    {
                        baseClass = baseClass.ACClass1_BasedOnACClass;
                        acClass = baseClass.Childs.Where(c => c.ACIdentifier == acClassNameSearch).FirstOrDefault();
                        if (acClass != null)
                            return acClass;
                    }
                }
            }

            // Global BSO-ACComponent
            if (searchForGlobals
                && Root.Businessobjects != null
                && Root.Businessobjects.ComponentClass != null
                && Root.Businessobjects.ComponentClass.Childs != null)
            {
                acClassNameSearch = Root.Businessobjects.GetDerivedClassnameIfConfigured(acClassNameSearch);
                acClass = Root.Businessobjects.ComponentClass.Childs
                            .Where(c => c.ACIdentifier == acClassNameSearch
                                        && (c.ACKindIndex == (Int16)Global.ACKinds.TACBSOGlobal
                                            || c.ACKindIndex == (Int16)Global.ACKinds.TACBSOReport))
                            .FirstOrDefault();
                acClassName = acClassNameSearch;
                if (acClass != null)
                    return acClass;
            }
            return null;
        }

        protected StartCompResult CreateStartCompResult(string childACIdentifier)
        {
            StartCompResult childInfo = new StartCompResult(childACIdentifier);
            childInfo.ControlMode = Global.ControlModes.Enabled;
            childInfo.ACClass = GetACClassFromACClassName(childInfo.ACClassName);
            if (childInfo.ACClass == null)
                return childInfo;
            childInfo.ControlMode = childInfo.ACClass.GetRight(childInfo.ACClass);
            return childInfo;
        }


        /// <summary>
        /// Starts/Creates a new Instance as a child of this instance
        /// </summary>
        /// <param name="acIdentifier">This parameter, you pass the name of the Child-Class/instance, 
        /// which was defined by the development environment in the application tree below the class in which this instance is currently located (this-pointer). 
        /// If no class could be found in the application tree with this ACIdentifier, then it is checked whether it is a business object class (derivation of ACBSO). 
        /// If so, then a new dynamic "child instance" is created below the instance in which this instance is currently located.</param>
        /// <param name="content">This parameter is passed to the second parameter "IACObject content" of a ACComponent-Constructor.</param>
        /// <param name="acParameter">Ths parameter must contain the parameters that are passed to the fourth constructor parameter as "ACValueList parameter". Here you can do this in two different ways:<para />
        /// A) object[] acParameter contains only one element of the type ACValueList, which already contains all the required parameters according to ACClassConstructorInfo. 
        /// To do this, call the ACClass.TypeACSignature()-Method of the corresponding iPlus type that you want to instance. 
        /// This method returns a ACValueList filled with the necessary parameters, which you pass as the only element in the acparameter array.<para />
        /// B) object[] acParameter contains exactly the parameters required, according to ACClassConstructorInfo.
        /// The StartComponent method internally converts the array into an ACValueList.</param>
        /// <param name="startOptions">Start-Options</param>
        /// <returns></returns>
        [ACMethodInfo("ACComponent", "en{'Start ACComponent'}de{'Start ACComponent'}", 9999)]
        public virtual IACComponent StartComponent(string acIdentifier, object content, Object[] acParameter, ACStartCompOptions startOptions = ACStartCompOptions.Default)
        {
            return StartComponent(CreateStartCompResult(acIdentifier), content, acParameter, startOptions);
        }

        protected virtual IACComponent StartComponent(StartCompResult startCompResult, object content, Object[] acParameter, ACStartCompOptions startOptions = ACStartCompOptions.Default)
        {
            if (startCompResult == null)
                return null;
            if (startCompResult.ACClass != null)
            {
                // Starten von RootTest
                if (startCompResult.ACClass.ACKind == Global.ACKinds.TACRoot)
                {
                    ACValueList acParameter1 = startCompResult.ACClass.ACParameter;
                    acParameter1["User"] = Root.Environment.User;
                    acParameter1["IsRegisterACObjects"] = false;
                    acParameter1["IsPropPersistenceOff"] = false;
                    acParameter1["IsWCFOff"] = true;
                    acParameter1["Simulation"] = false;

                    return StartComponent(startCompResult.ACClass, null, acParameter1, Global.ACStartTypes.Automatic, false, Const.ACRootProjectNameTest);
                }

                if (    (startOptions & ACStartCompOptions.OnlyAutomatic) == ACStartCompOptions.OnlyAutomatic
                    && !(startCompResult.ACClass.ACStartType == Global.ACStartTypes.AutomaticOnDemand || startCompResult.ACClass.ACStartType == Global.ACStartTypes.Automatic))
                    return null;

                ACValueList acValueList = startCompResult.ACClass.GetACParameter(acParameter);
                if (string.IsNullOrEmpty(startCompResult.ACInstance))
                {
                    return StartComponent(startCompResult.ACClass, content, acValueList, Global.ACStartTypes.Automatic, IsProxy);
                }
                else
                {
                    return StartComponent(startCompResult.ACClass, content, acValueList, Global.ACStartTypes.Automatic, IsProxy, startCompResult.ACIdentifier);
                }
            }
            // Falls Proxy und Aufruf ACUrl-Command per ACRef, dann muss zuerst vom Server information über die Child-Instanz abgefragt werden
            else if (IsProxy && !((startOptions & ACStartCompOptions.NoServerReqFromProxy) == ACStartCompOptions.NoServerReqFromProxy))
            {
                var query = GetChildInstanceInfo(1, false, startCompResult.ACIdentifier);
                if (query != null && query.Any())
                {
                    startCompResult.ChildInstanceInfo = query.First();
                    if (startCompResult.ChildInstanceInfo != null)
                    {
                        return StartComponent(startCompResult.ChildInstanceInfo, Global.ACStartTypes.Automatic, true);
                    }
                }
                else if (this.ConnectionState != ACObjectConnectionState.DisConnected)
                {
                    Messages.LogWarning(this.GetACUrl(), "StartComponent(0)", String.Format("Child {0} doesn't exist on Server-Side. IsWorkflowType: {1}.", startCompResult.ACIdentifier, this.ComponentClass.IsWorkflowType));
                }
                else if (this.ConnectionState == ACObjectConnectionState.Connected)
                    startCompResult.ChildInstanceInfo = new ACChildInstanceInfo();
                return null;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Starts/Creates a new Instance as a child of this instance
        /// </summary>
        /// <param name="acClass">Type which should be instantiated</param>
        /// <param name="content">This parameter is passed to the second parameter "IACObject content" of a ACComponent-Constructor.</param>
        /// <param name="parameterList">This parameter is passed to the fourth parameter "ACValueList parameter" of a ACComponent-Constructor. 
        /// Call the ACClass.TypeACSignature()-Method of the corresponding iPlus type that you want to instance to get the right ACValueList (from your passed acClass parameter)</param>
        /// <param name="startChildMode">Controls if further childrens should be created automatically</param>
        /// <param name="asProxy"></param>
        /// <param name="acIdentifier">Identifier which should given to the new created instance</param>
        /// <returns></returns>
        public virtual IACComponent StartComponent(ACClass acClass, object content, ACValueList parameterList, Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic, bool asProxy = false, string acIdentifier = "")
        {
            Type acObjectType = null;
            // ACClass is null, when Component is deleted in Development-Environment ond client side, but server was not restarted
            if (acClass == null)
                return null;
            if (this.Root == null || this.Root.InitState == ACInitState.Destructing || this.Root.InitState == ACInitState.Destructed)
                return null;

            if (asProxy)
            {
                if (acClass.IsWorkflowType)
                    acObjectType = typeof(PWNodeProxy);
                else
                {
                    if (acClass.ACKind == Global.ACKinds.TACApplicationManager)
                    {
                        // Für reale ACComponent : ApplicationManager
                        acObjectType = typeof(ApplicationManagerProxy);
                    }
                    else
                    {
                        //if (acClass.IsApplicationType)
                        //    acObjectType = typeof(ACComponentProxyApplication);
                        //else
                        acObjectType = typeof(ACComponentProxy);
                    }
                }
            }

            ACComponent result = null;
            try
            {
                result = ACActivator.CreateInstance(acClass, content, this, parameterList, startChildMode, acObjectType, acIdentifier, true) as ACComponent;
            }
            catch (ACCreateException e)
            {
                if (acClass.IsWorkflowType)
                {
                    throw new ACCreateException((e as ACCreateException).InvalidObject, e.Message, e);
                }
                result = null;

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACComponent", "StartACComponentByACClass", msg);
            }
            return result;
        }

        /// <summary>
        /// Starts/Creates a new Instance through a ACChildInstanceInfo
        /// </summary>
        /// <param name="instanceInfo">Info which was retrieved from the server first</param>
        /// <param name="startChildMode">Controls if further childrens should be created automatically</param>
        /// <param name="asProxy">Force to create instance as a proxy</param>
        /// <returns></returns>
        public virtual IACComponent StartComponent(ACChildInstanceInfo instanceInfo, Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic, bool asProxy = false)
        {
            if (instanceInfo == null)
                return null;
            IACComponent child = ACComponentChilds.Where(c => c.ACIdentifier == instanceInfo.ACIdentifier).FirstOrDefault();
            if (child != null)
            {
                PWNodeProxy pWNodeProxy = child as PWNodeProxy;
                if (pWNodeProxy != null)
                {
                    pWNodeProxy.CheckAndReplaceContent(instanceInfo);
                }
                return child;
            }
            ACValueList acParameterList = new ACValueList();
            acParameterList.Add(new ACValue("ACChildInstanceInfo", instanceInfo));
            ACClass classOnServerSide = instanceInfo.ACType.ValueT as ACClass;
            if (classOnServerSide == null)
                return null;
            return StartComponent(instanceInfo.ACType.ValueT as ACClass, instanceInfo.Content.ValueT, acParameterList, startChildMode, asProxy, instanceInfo.ACIdentifier);
        }


        private ACMonitorObject _20020_LockMemberList = new ACMonitorObject(20020);
        /// <summary>
        /// Lock-Object for Accessing members of a ACComponent-Instance. 
        /// </summary>
        public ACMonitorObject LockMemberList_20020
        {
            get
            {
                return _20020_LockMemberList;
            }
        }

        protected static void AddToPostInitQueue(Action action)
        {
            ACActivator.AddToPostInitQueue(action);
        }

        protected static void AddToPostDeInitQueue(Action action)
        {
            ACActivator.AddToPostDeInitQueue(action);
        }
        #endregion


        #region precompiled queries
        private static readonly Func<Database, Guid, string, ACClassTask> s_cQry_ContentTaskByIdentifier =
            CompiledQuery.Compile<Database, Guid, string, ACClassTask>(
                (db, parentTaskID, acIdentifier) =>
                    db.ACClassTask
                                    .Include("ContentACClassWF")
                                    .Include("ContentACClassWF.ACClassMethod")
                                    .Include("ContentACClassWF.RefPAACClass")
                                    .Include("ContentACClassWF.RefPAACClassMethod")
                                    .Include("ContentACClassWF.RefPAACClassMethod.AttachedFromACClass")
                                    .Include("ContentACClassWF.ACClassWF1_ParentACClassWF")
                                    .Include("ContentACClassWF.ACClassWF_ParentACClassWF")
                                    .Include("ContentACClassWF.PWACClass")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.SourceACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.TargetACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.SourceACClassProperty")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.TargetACClassProperty")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.SourceACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.TargetACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.SourceACClassProperty")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.TargetACClassProperty")
                                    .Include("ACProgram")
                                    .Include("TaskTypeACClass")
                                    .Include("ACClassTaskValue_ACClassTask")
                                    .Where(c => c.ParentACClassTaskID.HasValue
                                                && c.ParentACClassTaskID == parentTaskID
                                                && c.ACIdentifier == acIdentifier)
                                    .FirstOrDefault()
            );

        private static readonly Func<Database, Guid, Guid, ACClassTask> s_cQry_ContentTaskByClassID =
            CompiledQuery.Compile<Database, Guid, Guid, ACClassTask>(
                (db, taskID, classID) =>
                    db.ACClassTask
                                    .Include("ContentACClassWF")
                                    .Include("ContentACClassWF.ACClassMethod")
                                    .Include("ContentACClassWF.RefPAACClass")
                                    .Include("ContentACClassWF.RefPAACClassMethod")
                                    .Include("ContentACClassWF.RefPAACClassMethod.AttachedFromACClass")
                                    .Include("ContentACClassWF.ACClassWF1_ParentACClassWF")
                                    .Include("ContentACClassWF.ACClassWF_ParentACClassWF")
                                    .Include("ContentACClassWF.PWACClass")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.SourceACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.TargetACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.SourceACClassProperty")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.TargetACClassProperty")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.SourceACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.TargetACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.SourceACClassProperty")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.TargetACClassProperty")
                                    .Include("ACProgram")
                                    .Include("TaskTypeACClass")
                                    .Include("ACClassTaskValue_ACClassTask")
                                    .Where(c => c.ParentACClassTaskID.HasValue
                                                && c.ParentACClassTaskID == taskID
                                                && c.ACTaskTypeIndex != (short)Global.ACTaskTypes.MethodTask
                                                && c.TaskTypeACClassID == classID)
                                    .FirstOrDefault()
            );

        private static readonly Func<Database, Guid, IEnumerable<ACClassTask>> s_cQry_SubTasks =
            CompiledQuery.Compile<Database, Guid, IEnumerable<ACClassTask>>(
                (db, taskID) =>
                    db.ACClassTask
                                    .Include("ContentACClassWF")
                                    .Include("ContentACClassWF.ACClassMethod")
                                    .Include("ContentACClassWF.RefPAACClass")
                                    .Include("ContentACClassWF.RefPAACClassMethod")
                                    .Include("ContentACClassWF.RefPAACClassMethod.AttachedFromACClass")
                                    .Include("ContentACClassWF.ACClassWF1_ParentACClassWF")
                                    .Include("ContentACClassWF.ACClassWF_ParentACClassWF")
                                    .Include("ContentACClassWF.PWACClass")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.SourceACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.TargetACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.SourceACClassProperty")
                                    .Include("ContentACClassWF.ACClassWFEdge_SourceACClassWF.TargetACClassProperty")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.SourceACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.TargetACClassWF")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.SourceACClassProperty")
                                    .Include("ContentACClassWF.ACClassWFEdge_TargetACClassWF.TargetACClassProperty")
                                    .Include("ACProgram")
                                    .Include("TaskTypeACClass")
                                    .Include("ACClassTaskValue_ACClassTask")
                                    .Where(c => c.ParentACClassTaskID.HasValue
                                                && c.ParentACClassTaskID == taskID
                                                && c.ACTaskTypeIndex != (short)Global.ACTaskTypes.MethodTask)
            );
        #endregion


        #region IACObjectWithBinding Member

        private ACClass _ACType;
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [ACPropertyInfo(9999, "ACValueType", "en{'Typeinfo'}de{'Typeinfo'}")]
        public IACType ACType
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _ACType;
                }
            }
            private set
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _ACType = value as ACClass;
                }
            }
        }

        /// <summary>
        /// Returns the type of this Component
        /// ATTENTION: Usage of this Entity must be ensured with using the QueryLock_1X000 of the Global Database-Context
        /// Otherwise it's NOT THREAD-SAFE!
        /// </summary>
        public ACClass ComponentClass
        {
            get
            {
                return ACType as ACClass;
            }
        }


        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return ParentACComponent;
            }
        }

        #region IACMember Member
        /// <summary>Parent ACComponent where this instance belongs to.</summary>
        /// <value>The parent ac component.</value>
        public IACComponent ParentACComponent
        {
            get
            {
                if (ACRef == null)
                    return null;
                return ACRef.ValueT;
            }
        }


        private ACRef<IACComponent> _ACRefParent;
        /// <summary>Smart-Pointer to the Parent ACComponent where this instance belongs to.</summary>
        /// <value>The parent ac component.</value>
        public ACRef<IACComponent> ACRef
        {
            get { return _ACRefParent; }
        }


        /// <summary>Gets or sets the value as a boxed type</summary>
        /// <value>Returns this</value>
        public object Value
        {
            get
            {
                return this;
            }
            set
            {
                OnMemberChanged();
            }
        }

        /// <summary>
        /// Must be called inside the class that implements IACMember every time when the the encapsulated value-Property has changed.
        /// If the implementation implements INotifyPropertyChanged also then OnPropertyChanged() must be called inside the implementation of OnMemberChanged().
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.EventArgs"/> instance containing the event data. Is not null if the change of the encapsulated value was detected by a callback of the PropertyChangedEvent or CollectionChanged-Event. Then the EventArgs will be passed.
        /// </param>
        public void OnMemberChanged(EventArgs e = null)
        {
        }
        #endregion

        protected ACOperationModes _ACOperationMode = ACOperationModes.Live;
        /// <summary>
        /// Operation-Mode
        /// </summary>
        public ACOperationModes ACOperationMode
        {
            get
            {
                return _ACOperationMode;
            }
        }

        /// <summary>
        /// Root of all Application-Trees
        /// </summary>
        [ACPropertyCurrent(9999, "Root", "en{'Root'}de{'Root'}")]
        public IRoot Root
        {
            get
            {
                if (ACOperationMode != ACOperationModes.Test)
                {
                    return gip.core.autocomponent.ACRoot.SRoot;
                }
                else
                {
                    return gip.core.autocomponent.ACRoot.SRootTest;
                }
            }
        }

        public virtual IMessages Messages
        {
            get
            {
                return Root.Messages;
            }
        }

        protected IACEntityObjectContext _Database;
        /// <summary>
        /// Returns the database context that this instance works with. If this instance hasn't overridden the context it returns the context of its parent context. If no parent instance has overwritten the database context, the global context is returned.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        [ACPropertyInfo(9999)]
        public virtual IACEntityObjectContext Database
        {
            get
            {
                if (_Database == null)
                {
                    if (!IsSeperateDBContextForced && ParentACComponent != null)
                        return ParentACComponent.Database;
                    _Database = ACObjectContextManager.GetOrCreateContext<Database>(GetACUrl());
                }
                return _Database;
            }
        }

        public bool IsSeperateDBContextForced
        {
            get
            {
                return ParameterValueT<bool>(Const.ParamSeperateContext);
            }
        }


        private ACInitState _InitState = ACInitState.Destructed;
        /// <summary>
        /// Main-State of a Component (Controls the Lifecycle)
        /// </summary>
        [ACPropertyInfo(9999)]
        public ACInitState InitState
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _InitState;
                }
            }
            set
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _InitState = value;
                }
                OnPropertyChanged("InitState");
            }
        }

        protected ACUrlCmdMessage _ACUrlCmdMessage = null;
        /// <summary>
        /// UI-Controls from gip.core.layoutengine bind this property if they mant to receive Messages from this component.
        /// ACUrlCmdMessage ist changed through the call of the method BroadcastToVBControls()
        /// </summary>
        /// <value>
        /// The ac URL command message.
        /// </value>
        [ACPropertyInfo(9999)]
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get
            {
                return _ACUrlCmdMessage;
            }
            internal set
            {
                _ACUrlCmdMessage = value;
                if (_ACUrlCmdMessage != null)
                    OnPropertyChanged("ACUrlCmdMessage");
            }
        }


        /// <summary>
        /// In order for your ACComponent-Class to be poolable, you must reset all your class members and remove references in the overridden ACDeInit method. 
        /// This corresponds to the state that is present after creation by the constructor.
        /// If this is the case, return true.
        /// </summary>
        public virtual bool IsPoolable
        {
            get
            {
                return false;
            }
        }

        #endregion


        #region Rechte und Identität

        private string _ACIdentifier;
        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get
            {
                return _ACIdentifier;
            }
        }


        /// <summary>
        /// This method is called inside the Construct-Method. Derivations can have influence to the naming of the instance by changing the acIdentifier-Parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        protected virtual void InitACIdentifier(ACValueList parameter, ref string acIdentifier)
        {
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public virtual string ACCaption
        {
            get
            {
                return ACType?.ACCaption;
            }
        }
        #endregion


        #region Parameter
        ACValueList _Parameters = null;
        /// <summary>
        /// Parameters that are passed to the constructor when a new instance is created
        /// </summary>
        public ACValueList Parameters
        {
            get
            {
                return _Parameters;
            }
        }

        /// <summary>
        /// Returns a value from Parameters where the passed property name is matched.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public object ParameterValue(string propertyName)
        {
            if (Parameters == null || !Parameters.Any())
                return null;

            ACValue acValue = Parameters.GetACValue(propertyName);
            if (acValue == null)
                return null;

            return acValue.Value;
        }

        public T ParameterValueT<T>(string propertyName)
        {
            if (Parameters == null || !Parameters.Any())
                return default(T);

            ACValue acValue = Parameters.GetACValue(propertyName);
            if (acValue == null)
                return default(T);

            return acValue.ValueT<T>();
        }
        #endregion


        #region Members

        ObservableCollection<IACMember> _ACMemberList = new ObservableCollection<IACMember>();
        List<IACPropertyConfigValue> _ACPropertyConfigValueList = new List<IACPropertyConfigValue>();

        /// <summary>Access the ACMemberList by the ACIdentifier</summary>
        /// <param name="acIdentifier">Name of the member</param>
        /// <returns>IACMember</returns>
        public IACMember GetMember(string acIdentifier)
        {
            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (_ACMemberList == null)
                    return null;
                return _ACMemberList.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault();
            }
        }


        /// <summary>Access the ACMemberList by the ACIdentifier. If the member was not found it tries to create a Child-ACComponent if it was defined in the Application-Tree.</summary>
        /// <param name="acIdentifier">Name of the member</param>
        /// <returns>IACMember</returns>
        internal IACMember GetOrCreateACMember(string acIdentifier)
        {
            if (String.IsNullOrEmpty(acIdentifier))
                return null;
            IACMember acMember = GetMember(acIdentifier);
            if (acMember != null)
                return acMember;
            acMember = StartComponent(acIdentifier, null, null, ACStartCompOptions.NoServerReqFromProxy | ACStartCompOptions.OnlyAutomatic);
            return acMember;
        }


        /// <summary>
        /// Checks whether this IACMember-Instance belongs to this component.
        /// </summary>
        /// <param name="member">IACMember-Instance</param>
        /// <returns><c>true</c> if member belongs to this component; otherwise, <c>false</c>.</returns>
        public bool HasMember(IACMember member)
        {
            if (member == null)
                return false;

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (_ACMemberList == null)
                    return false;
                return _ACMemberList.Where(c => c == member).Any();
            }
        }


        /// <summary>Adds a member if it doesn't exist. Otherwise it replace a existing one.</summary>
        /// <param name="member">IACMember-Instance</param>
        /// <returns><c>true</c> if sucessful; otherwise, <c>false</c>.</returns>
        internal bool AddOrReplaceACMember(IACMember member)
        {
            if (member == null)
                return false;
            IACMember exists = GetMember(member.ACIdentifier);
            if (exists == member)
                return true;
            if (exists != null)
            {
                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    if (_ACMemberList == null)
                        return false;
                    _ACMemberList.Remove(exists);
                    _ACMemberList.Add(member);
                }
            }
            else
            {
                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    if (_ACMemberList == null)
                        return false;
                    _ACMemberList.Add(member);
                }
            }
            return true;
        }


        /// <summary>Adds a member if it doesn't exist.</summary>
        /// <param name="member">IACMember-Instance</param>
        /// <returns><c>true</c> if sucessful; otherwise, <c>false</c>.</returns>
        internal bool AddACMember(IACMember member)
        {
            if (member == null)
                return false;
            IACMember exists = GetMember(member.ACIdentifier);
            if (exists != null)
                return false;

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (_ACMemberList == null)
                    return false;
                _ACMemberList.Add(member);
            }
            return true;
        }


        /// <summary>Removes a member.</summary>
        /// <param name="member">IACMember-Instance</param>
        /// <returns><c>true</c> if sucessful; otherwise, <c>false</c>.</returns>
        internal bool RemoveACMember(IACMember member)
        {
            if (member == null)
                return false;
            if (_ACMemberList == null)
                return false;
            IACMember exists = GetMember(member.ACIdentifier);
            if (exists == null)
                return false;

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                _ACMemberList.Remove(member);
                if (exists != member)
                    _ACMemberList.Remove(exists);
            }
            return true;
        }


        /// <summary>Gets a ACComponent-child that matches the passed acIdentifier.</summary>
        /// <param name="acIdentifier">The acIdentifier. It's the clasname of the Child.</param>
        /// <param name="includeDerivedDynamicChilds">
        /// If the acIdentifier is a classname of a base-class that wasn't defined in the Application-tree because it was added dynamically during the runtime, then find a child that is a derivation of it. 
        /// In this case the child has a different ACIdentifier in the ACMemberList because it have the classname of the derivation. 
        /// If set to true than the search for this derivation should take place if no instance was found that matched the passed acIdentifier.
        /// </param>
        /// <returns>The child as IACComponent</returns>
        public IACComponent GetChildComponent(string acIdentifier, bool includeDerivedDynamicChilds = false)
        {
            IACComponent child = GetMember(acIdentifier) as IACComponent;
            if (child != null)
                return child;
            if (!includeDerivedDynamicChilds)
                return null;

            string acClassName = ACUrlHelper.ExtractTypeName(acIdentifier);
            string acInstance = ACUrlHelper.ExtractInstanceName(acIdentifier);
            ACClass acClass = GetACClassFromACClassName(acClassName);
            if (!String.IsNullOrEmpty(acInstance))
                acIdentifier = String.Format("{0}({1})", acClassName, acInstance);
            else
                acIdentifier = acClassName;
            return GetMember(acIdentifier) as IACComponent;
        }

        /// <summary>
        /// NOT THREAD-SAFE! Returns all Members of this Instance (Properties, Points and Childs)
        /// For querying this List use using (ACMonitor.Lock(LockMemberList_20020))
        /// </summary>
        [ACPropertyInfo(9999)]
        public IList<IACMember> ACMemberList
        {
            get
            {
                return _ACMemberList;
            }
        }

        ACMemberIndexer<string, IACMember> _ACMemberDict = null;
        /// <summary>
        /// Access the ACMemberList of a Component through a string-index.
        /// It's useful to formulate WPF-Bindings with a indexed Path, e.g: Text="{Binding Path=ContentACComponent.ACMemberDict[CONV].ACMemberDict[AggrNo].Value}"
        /// </summary>
        /// <value>IACMember</value>
        public ACMemberIndexer<string, IACMember> ACMemberDict
        {
            get
            {
                return _ACMemberDict;
            }
        }

        /// <summary>
        /// NOT THREAD-SAFE!
        /// For querying this List use using (ACMonitor.Lock(LockMemberList_20020))
        /// </summary>
        public IList<IACPropertyConfigValue> ACPropertyConfigValueList
        {
            get
            {
                return _ACPropertyConfigValueList;
            }
        }


        /// <summary>
        /// Thread-Safe access to Childs. Returns a new List for threads-safe iteration.
        /// </summary>
        [ACPropertyInfo(9999)]
        public IEnumerable<IACComponent> ACComponentChilds
        {
            get
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    if (_ACMemberList != null)
                        return _ACMemberList.Where(c => c is IACComponent).Select(c => c as IACComponent).ToArray();
                    else
                        return new IACComponent[] { };
                }
            }
        }

        /// <summary>
        /// If this instance is a proxy, then the server is queried for child instances and then the missing instances generated and attached below this proxy as children.
        /// After that, the ACComponentChilds-Property will be returned.
        /// </summary>
        [ACPropertyInfo(9999)]
        public virtual IEnumerable<IACComponent> ACComponentChildsOnServer
        {
            get
            {
                return ACComponentChilds;
            }
        }


        /// <summary>
        /// Queries the server for childs and get a tree of ACChildInstanceInfo-Instances
        /// </summary>
        /// <param name="maxChildDepth">Limits the search to a defined depth. If maxChildDepth=0 then the search goes to the deepest child in the tree</param>
        /// <param name="onlyWorkflows">If set to true, then only derivations of PWBase will be returned (Workflow-Nodes)</param>
        /// <param name="acIdentifier">If set, then only child with this acIdentifer will be returned</param>
        /// <returns>Childs on server-side</returns>
        [ACMethodInfo("", "", 9999)]
        public virtual IEnumerable<ACChildInstanceInfo> GetChildInstanceInfo(int maxChildDepth, bool onlyWorkflows, string acIdentifier = "")
        {
            List<ACChildInstanceInfo> list = new List<ACChildInstanceInfo>();
            foreach (ACComponent child in ACComponentChilds)
            {
                if (onlyWorkflows && !child.ComponentClass.IsWorkflowType)
                    continue;
                if (!String.IsNullOrEmpty(acIdentifier) && child.ACIdentifier != acIdentifier)
                    continue;
                ACChildInstanceInfo childInfo = new ACChildInstanceInfo(child);
                child.FillChildInstanceInfo(childInfo, maxChildDepth, 1, onlyWorkflows);
                list.Add(childInfo);
            }
            return list;
        }


        /// <summary>
        /// Queries the server for childs and get a tree of ACChildInstanceInfo-Instances
        /// </summary>
        /// <param name="maxChildDepth">Limits the search to a defined depth. If maxChildDepth=0 then the search goes to the deepest child in the tree</param>
        /// <param name="searchParam">Complex filter criteria</param>
        /// <returns>Childs on server-side</returns>
        [ACMethodInfo("", "", 9999)]
        public virtual IEnumerable<ACChildInstanceInfo> GetChildInstanceInfo(int maxChildDepth, ChildInstanceInfoSearchParam searchParam)
        {
            if (searchParam == null)
                return null;
            List<ACChildInstanceInfo> list = new List<ACChildInstanceInfo>();
            foreach (ACComponent child in ACComponentChilds)
            {
                if (!searchParam.BottomUpSearch && !CheckInstanceInfoSearchParam(child, searchParam))
                    continue;

                ACChildInstanceInfo childInfo = new ACChildInstanceInfo(child);
                if (searchParam != null && !string.IsNullOrEmpty(searchParam.ContainsPropertyName))
                {
                    childInfo.MemberValues = new ACValueWithCaptionList(child.ACPropertyList
                        .Where(c =>     (     c is IACPropertyNetBase
                                          || (searchParam.ReturnLocalProperties && c.PropertyInfo != null && ACKnownTypes.IsKnownType(c.PropertyInfo.ObjectType)))
                                    && (      CultureInfo.InvariantCulture.CompareInfo.IndexOf(c.ACIdentifier, searchParam.ContainsPropertyName, CompareOptions.IgnoreCase) >= 0
                                          || (   c.PropertyInfo.ACCaptionTranslation != null
                                              && CultureInfo.InvariantCulture.CompareInfo.IndexOf(c.PropertyInfo.ACCaptionTranslation, searchParam.ContainsPropertyName, CompareOptions.IgnoreCase) >= 0))
                                       )
                                    .Select(c => new ACValueWithCaption(c.ACIdentifier, c.ACType.ValueTypeACClass, c.PropertyInfo.ACCaptionTranslation, c.Value))
                                    .ToArray());
                }
                if (!searchParam.ReturnAsFlatList)
                {
                    child.FillChildInstanceInfo(childInfo, maxChildDepth, 1, searchParam.OnlyWorkflows, searchParam);
                    //CheckInstanceInfoSarchParam(child, searchParam);
                    list.Add(childInfo);
                }
                else
                {
                    if (CheckInstanceInfoSearchParam(child, searchParam))
                        list.Add(childInfo);
                    child.FillChildInstanceInfo(childInfo, maxChildDepth, 1, searchParam.OnlyWorkflows, searchParam, list);
                }
            }
            return list;
        }

        protected void FillChildInstanceInfo(ACChildInstanceInfo parent, int maxChildDepth, int currentDepth, bool onlyWorkflows, ChildInstanceInfoSearchParam searchParam = null,
                                             List<ACChildInstanceInfo> mainList = null)
        {
            if (parent == null)
                return;
            if ((maxChildDepth > 0) && (currentDepth >= maxChildDepth))
                return;
            foreach (ACComponent child in ACComponentChilds)
            {
                if (onlyWorkflows && !child.ComponentClass.IsWorkflowType)
                    continue;
                ACChildInstanceInfo childInfo = new ACChildInstanceInfo(child);

                if (searchParam != null && !string.IsNullOrEmpty(searchParam.ContainsPropertyName))
                    childInfo.MemberValues = new ACValueWithCaptionList(child.ACPropertyList
                        .Where(c => (      c is IACPropertyNetBase 
                                        || (searchParam.ReturnLocalProperties && c.PropertyInfo != null && ACKnownTypes.IsKnownType(c.PropertyInfo.ObjectType)))
                                    && (   CultureInfo.InvariantCulture.CompareInfo.IndexOf(c.ACIdentifier, searchParam.ContainsPropertyName, CompareOptions.IgnoreCase) >= 0
                                        || (   c.PropertyInfo.ACCaptionTranslation != null
                                            && CultureInfo.InvariantCulture.CompareInfo.IndexOf(c.PropertyInfo.ACCaptionTranslation, searchParam.ContainsPropertyName, CompareOptions.IgnoreCase) >= 0))
                                       )
                                    .Select(c => new ACValueWithCaption(c.ACIdentifier, c.ACType.ValueTypeACClass, c.PropertyInfo.ACCaptionTranslation, c.Value))
                                    .ToArray());


                if (searchParam != null && !searchParam.BottomUpSearch || mainList != null)
                {
                    if (mainList != null)
                        mainList.Add(childInfo);
                    else
                        parent.Childs.Add(childInfo);
                    currentDepth++;
                    child.FillChildInstanceInfo(childInfo, maxChildDepth, currentDepth, onlyWorkflows, searchParam, mainList);
                }
                else if (searchParam != null)
                {
                    currentDepth++;
                    child.FillChildInstanceInfo(childInfo, maxChildDepth, currentDepth, onlyWorkflows, searchParam, mainList);
                    if (CheckInstanceInfoSearchParam(child, searchParam) || childInfo.Childs.Any())
                        parent.Childs.Add(childInfo);
                }
                else
                {
                    currentDepth++;
                    parent.Childs.Add(childInfo);
                    child.FillChildInstanceInfo(childInfo, maxChildDepth, currentDepth, onlyWorkflows, searchParam, mainList);
                }
            }
        }

        private bool CheckInstanceInfoSearchParam(ACComponent child, ChildInstanceInfoSearchParam searchParam)
        {
            if ((searchParam.OnlyWorkflows && !child.ComponentClass.IsWorkflowType)
                || (!searchParam.WithWorkflows && !searchParam.OnlyWorkflows && child.ComponentClass.IsWorkflowType))
                return false;

            if (!String.IsNullOrEmpty(searchParam.ACIdentifier) && child.ACIdentifier != searchParam.ACIdentifier)
                return false;
            if (searchParam.TypeOfRoots != null)
            {
                if (!searchParam.TypeOfRoots.IsObjLoaded)
                {
                    searchParam.TypeOfRoots.AttachTo(gip.core.datamodel.Database.GlobalDatabase);
                }
                if (searchParam.TypeOfRoots.IsObjLoaded)
                {
                    Type typeOfComponent = searchParam.TypeOfRoots.ValueT.ObjectType;
                    if (typeOfComponent == null)
                        return false;
                    if (!typeOfComponent.IsAssignableFrom(child.GetType()))
                        return false;
                }
                else
                    return false;
            }
            if (searchParam.ACRequestIDs != null && searchParam.ACRequestIDs.Any())
            {
                PWProcessFunction pwProcessFunction = child as PWProcessFunction;
                if (pwProcessFunction == null)
                    return false;
                if (pwProcessFunction.CurrentACMethod.ValueT == null)
                    return false;
                if (!searchParam.ACRequestIDs.Contains(pwProcessFunction.CurrentACMethod.ValueT.ACRequestID))
                    return false;
            }
            if (searchParam.ACProgramIDs != null && searchParam.ACProgramIDs.Any())
            {
                PWProcessFunction pwProcessFunction = child as PWProcessFunction;
                if (pwProcessFunction == null)
                    return false;
                if (pwProcessFunction.CurrentACProgram == null)
                    return false;
                if (!searchParam.ACProgramIDs.Contains(pwProcessFunction.CurrentACProgram.ACProgramID))
                    return false;
            }
            return true;
        }


        protected ACPointEvent _ChildRemovedEvent;
        /// <summary>
        /// This event is raised when a child was removed
        /// </summary>
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent ChildRemovedEvent
        {
            get
            {
                return _ChildRemovedEvent;
            }
            set
            {
                _ChildRemovedEvent = value;
            }
        }

        internal void ACComponentChilds_Remove(IACComponent acComponent)
        {
            if (acComponent == null)
                return;
            if (_ACMemberList == null)
                return;

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                _ACMemberList.Remove(acComponent);
            }
            ACEventArgs acParameterList = new ACEventArgs();
            acParameterList.Add(new ACValue("ACMemberList", acComponent));
            ChildRemovedEvent.Raise(acParameterList);
        }

        protected ACPointEvent _ChildAddedEvent;
        /// <summary>
        /// This event is raised when a child was added in method AddChild()
        /// </summary>
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent ChildAddedEvent
        {
            get
            {
                return _ChildAddedEvent;
            }
            set
            {
                _ChildAddedEvent = value;
            }
        }

        /// <summary>
        /// Adds a child to this instance
        /// </summary>
        /// <param name="acObject">The child</param>
        public void AddChild(IACObject acObject)
        {
            if (acObject == null || !(acObject is IACMember))
                return;
            if (_ACMemberList == null)
                return;

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                _ACMemberList.Add(acObject as IACMember);
            }
            ACEventArgs acParameterList = new ACEventArgs();
            acParameterList.Add(new ACValue("ACMemberList", acObject));

            ChildAddedEvent.Raise(acParameterList);
        }
        #endregion


        #region Messagehandling

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("", "", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            string acUrl = "";
            if (ParentACComponent == null)
            {
                acUrl = "\\";
            }
            else
            {
                if (ParentACComponent != rootACObject)
                {
                    acUrl = ParentACComponent.GetACUrl(rootACObject);
                    if (!acUrl.EndsWith("\\"))
                        acUrl += "\\";
                }
                acUrl += ACIdentifier;
            }
            return acUrl;
        }

        /// <summary>
        /// Returns the absolute ACUrl of this component.
        /// </summary>
        /// <value>Absolute ACUrl</value>
        [ACPropertyInfo(9999)]
        public string ACUrl
        {
            get
            {
                return GetACUrl();
            }
        }

        /// <summary>Returns a ACClassDesign for presenting itself on the gui</summary>
        /// <param name="acUsage">Filter for selecting designs that belongs to this ACUsages-Group</param>
        /// <param name="acKind">Filter for selecting designs that belongs to this ACKinds-Group</param>
        /// <param name="vbDesignName">Optional: The concrete acIdentifier of the design</param>
        /// <returns>ACClassDesign</returns>        
        public virtual ACClassDesign GetDesign(Global.ACUsages acUsage, Global.ACKinds acKind, string vbDesignName = "")
        {
            return ACType.GetDesign(ACType, acUsage, acKind, vbDesignName);
        }

        private datamodel.Licensing.ComponentLicense _HasLicense = datamodel.Licensing.ComponentLicense.TimeCheck;

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public virtual object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            if (_HasLicense != datamodel.Licensing.ComponentLicense.True && acUrl != "Environment" && ACType != null)
            {
                _HasLicense = Root.Environment.License.IsComponentLicensed(((ACClass)ACType).ACPackageID);
                if (_HasLicense == datamodel.Licensing.ComponentLicense.False)
                    return null;
            }

            if (string.IsNullOrEmpty(acUrl))
                return this;
            if (acUrl == Const.CmdFindGUIResult)
            {
                if (acParameter != null && acParameter.Any())
                    _FindGuiResult = acParameter[0] as IACObject;
                return null;
            }

            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    {
                        if (acUrlHelper.NextACUrl.Length <= 0)
                            return Root;
                        else
                            return Root.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                    }
                case ACUrlHelper.UrlKeys.Child:
                    {
                        IACMember acMember = GetMember(acUrlHelper.ACUrlPart);
                        if (acMember == null)
                        {
                            switch (acUrlHelper.UrlType)
                            {
                                case ACUrlHelper.UrlTypes.QueryType:
                                    return null;
                            }

                            StartCompResult startCompResult = CreateStartCompResult(acUrlHelper.ACUrlPart);
                            if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                acMember = StartComponent(startCompResult, null, acParameter, ACStartCompOptions.OnlyAutomatic);
                            else
                                acMember = StartComponent(startCompResult, null, null, ACStartCompOptions.OnlyAutomatic);
                            if (startCompResult.IsComponentChild && acMember == null)
                                return null;
                        }
                        if (acMember != null)
                        {
                            if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                            {
                                if ((acParameter == null) || (!acParameter.Any()))
                                    return acMember.Value;
                                else
                                {
                                    // Componente bereits erzeugt => PArameter kann dem Konstruktor nicht nochmals neu übergeben werden
                                    if (acMember is ACComponent)
                                        return acMember.Value;
                                    // Sonst setze Wert
                                    acMember.Value = acParameter[0];
                                    return null;
                                }
                            }
                            if (acMember is IACObject)
                            {
                                return acMember.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                            }
                        }
                        else if (string.IsNullOrEmpty(acUrlHelper.NextACUrl)) // Konfiguration
                        {
                            if (!IsProxy)
                            {
                                IACPropertyConfigValue configValue = ACPropertyConfigValueList?.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart).FirstOrDefault();
                                if (configValue != null)
                                {
                                    if ((acParameter == null) || (!acParameter.Any()))
                                        return configValue.Value;
                                    else
                                    {
                                        configValue.Value = acParameter[0];
                                        return null;
                                    }
                                }
                                else if (this is IACMyConfigCache && (this as IACMyConfigCache).IsConfigurationLoaded)
                                {
                                    ACValue acValue = (this as IACMyConfigCache).MyConfiguration?.ParameterValueList?.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart).FirstOrDefault();
                                    if (acValue != null)
                                    {
                                        if ((acParameter == null) || (!acParameter.Any()))
                                            return acValue.Value;
                                        else
                                        {
                                            acValue.Value = acParameter[0];
                                            return null;
                                        }
                                    }
                                }
                                if ((acParameter == null) || (!acParameter.Any()))
                                    return this[acUrlHelper.ACUrlPart];
                                else
                                {
                                    this[acUrlHelper.ACUrlPart] = acParameter[0];
                                    return null;
                                }
                            }
                            else
                            {
                                object configResult = (this as ACComponentProxy).InvokeACUrlCommand(this.GetACUrl() + ACUrlHelper.Delimiter_DirSeperator + acUrlHelper.ACUrlPart, acParameter);
                                return configResult;
                            }
                        }
                        return null;
                    }
                case ACUrlHelper.UrlKeys.Parent:
                    return ParentACComponent.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                case ACUrlHelper.UrlKeys.Start:
                    {
                        switch (acUrlHelper.UrlType)
                        {
                            case ACUrlHelper.UrlTypes.QueryType:
                                return null;
                            default:
                                return StartComponent(acUrlHelper.ACUrlPart, null, acParameter);
                        }
                    }
                case ACUrlHelper.UrlKeys.Stop:
                    switch (acUrlHelper.UrlType)
                    {
                        case ACUrlHelper.UrlTypes.QueryType:
                            return null;
                        default:
                            StopComponent(acUrlHelper.ACUrlPart);
                            return null;
                    }
                case ACUrlHelper.UrlKeys.InvokeMethod:
                    {
                        return ExecuteMethod(acUrlHelper.ACUrlPart, acParameter);
                    }
                case ACUrlHelper.UrlKeys.CustomMessage:
                    return null; // Impementierung in Ableitung
                case ACUrlHelper.UrlKeys.TranslationText:
                    string text = Root.Environment.TranslateMessage(this, acUrlHelper.ACUrlPart, acParameter);
                    if (String.IsNullOrEmpty(text) || text.StartsWith("*"))
                        text = Root.Environment.TranslateText(this, acUrlHelper.ACUrlPart);
                    return text;
                default:
                    return null; // TODO: Fehlerbehandlung
            }
        }


        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            if (string.IsNullOrEmpty(acUrl))
                return false;

            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    return Root.IsEnabledACUrlCommand(acUrl.Substring(1), acParameter);
                case ACUrlHelper.UrlKeys.Parent:
                    return ParentACComponent.IsEnabledACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                case ACUrlHelper.UrlKeys.Child:
                    IACMember acMember = GetMember(acUrlHelper.ACUrlPart);
                    if (acMember == null)
                    {
                        acMember = StartComponent(acUrlHelper.ACUrlPart, null, null, ACStartCompOptions.OnlyAutomatic);
                    }
                    if (acMember == null)
                        return false;
                    if (string.IsNullOrEmpty(acUrlHelper.NextACUrl) || !(acMember is IACObject))
                    {
                        if (acMember.Value is IACObject)
                        {
                            return ((IACObject)acMember.Value).IsEnabledACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                        }
                        return false;
                    }
                    return acMember.IsEnabledACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                case ACUrlHelper.UrlKeys.InvokeMethod:
                    return IsEnabledExecuteACMethod(acUrlHelper.ACUrlPart, acParameter);
                case ACUrlHelper.UrlKeys.Start:
                    {
                        string acUrlPart = acUrlHelper.ACUrlPart;
                        ACClass rightItem = GetACClassFromACClassName(acUrlPart);
                        if (rightItem == null)
                            return false;
                        return true;
                    }
                default:
                    return false; // TODO: Fehlerbehandlung
            }
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            if (string.IsNullOrEmpty(acUrl))
                return false;
            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                    {
                        acTypeInfo = this.ACType;
                        source = this;
                        path = "";
                        rightControlMode = Global.ControlModes.Enabled;
                        return true;
                    }
                    else
                    {
                        return Root.ACUrlBinding(acUrlHelper.NextACUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                    }
                case ACUrlHelper.UrlKeys.Child:
                    {
                        IACMember acMember = GetMember(acUrlHelper.ACUrlPart);
                        if (acMember == null)
                        {
                            acMember = StartComponent(acUrlHelper.ACUrlPart, null, null, ACStartCompOptions.OnlyAutomatic);
                        }
                        if (acMember != null)
                        {
                            if (string.IsNullOrEmpty(acUrlHelper.NextACUrl) && acMember is ACComponent)
                            {
                                acTypeInfo = acMember.ACType;
                                source = acMember;
                                path = "";
                                rightControlMode = Global.ControlModes.Enabled;
                                return true;
                            }
                            else
                            {
                                return acMember.ACUrlBinding(acUrlHelper.NextACUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                            }
                        }
                        return false;
                    }
                case ACUrlHelper.UrlKeys.Parent:
                    return ParentACComponent.ACUrlBinding(acUrlHelper.NextACUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                case ACUrlHelper.UrlKeys.InvokeMethod:
                    {
                        string methodName = ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart);
                        acTypeInfo = ACClassMethods.FirstOrDefault(c => c.ACIdentifier == methodName);
                        //acTypeInfo = ACType.MyACClassMethod(methodName);
                        if (acTypeInfo == null)
                            return false;
                        source = this;
                        path = ACUrlHelper.Delimiter_InvokeMethod + methodName;
                        rightControlMode = this.ComponentClass.GetRight(acTypeInfo);
                        return true;
                    }
                case ACUrlHelper.UrlKeys.TranslationText:
                    {
                        string messageID = ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart);
                        ACClassMessage acClassMessage = ComponentClass.GetMessage(messageID);
                        if (acClassMessage == null)
                        {
                            ACClassText acClassText = ComponentClass.GetText(messageID);
                            if (acClassText == null)
                                return false;
                        }
                        source = this;
                        path = ACUrlHelper.Delimiter_Translate + messageID;
                        rightControlMode = this.ComponentClass.GetRight(ACType);
                        return true;
                    }
                default:
                    return false; // TODO: Fehlerbehandlung
            }
        }

        /// <summary>
        /// Resolves the type information for each segment of the passed ACUrl.
        /// </summary>
        /// <param name="acUrl"></param>
        /// <param name="acUrlTypeInfo"></param>
        /// <returns></returns>
        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            if (string.IsNullOrEmpty(acUrl) || acUrlTypeInfo == null)
                return false;
            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                    {
                        acUrlTypeInfo.AddSegment(this.GetACUrl(), this.ACType, this, Global.ControlModes.Enabled);
                        return true;
                    }
                    else
                    {
                        return Root.ACUrlTypeInfo(acUrlHelper.NextACUrl, ref acUrlTypeInfo);
                    }
                case ACUrlHelper.UrlKeys.Child:
                    {
                        IACMember acMember = GetMember(acUrlHelper.ACUrlPart);
                        if (acMember == null)
                        {
                            acMember = StartComponent(acUrlHelper.ACUrlPart, null, null, ACStartCompOptions.OnlyAutomatic);
                        }
                        if (acMember != null)
                        {
                            if (string.IsNullOrEmpty(acUrlHelper.NextACUrl) && acMember is ACComponent)
                            {
                                acUrlTypeInfo.AddSegment(this.GetACUrl(), acMember.ACType, acMember, Global.ControlModes.Enabled);
                                return true;
                            }
                            else
                            {
                                acUrlTypeInfo.AddSegment(this.GetACUrl(), this.ACType, this, Global.ControlModes.Enabled);
                                return acMember.ACUrlTypeInfo(acUrlHelper.NextACUrl, ref acUrlTypeInfo);
                            }
                        }
                        return false;
                    }
                case ACUrlHelper.UrlKeys.Parent:
                    return ParentACComponent.ACUrlTypeInfo(acUrlHelper.NextACUrl, ref acUrlTypeInfo);
                case ACUrlHelper.UrlKeys.InvokeMethod:
                    {
                        string methodName = ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart);
                        IACType acTypeInfo = ACClassMethods.FirstOrDefault(c => c.ACIdentifier == methodName);
                        if (acTypeInfo == null)
                            return false;
                        acUrlTypeInfo.AddSegment(this.GetACUrl() + ACUrlHelper.Delimiter_InvokeMethod + methodName, acTypeInfo, this, this.ComponentClass.GetRight(acTypeInfo));
                        return true;
                    }
                case ACUrlHelper.UrlKeys.TranslationText:
                    {
                        string messageID = ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart);
                        ACClassMessage acClassMessage = ComponentClass.GetMessage(messageID);
                        IACType acTypeInfo = acClassMessage?.ACType;
                        if (acClassMessage == null)
                        {
                            ACClassText acClassText = ComponentClass.GetText(messageID);
                            acTypeInfo = acClassText?.ACType;
                            if (acClassText == null)
                                return false;
                        }

                        acUrlTypeInfo.AddSegment(this.GetACUrl() + ACUrlHelper.Delimiter_Translate + messageID, acTypeInfo, this, this.ComponentClass.GetRight(ACType));

                        return true;
                    }
                default:
                    return false; // TODO: Fehlerbehandlung
            }
        }

        /// <summary> Executes a method via passed method-name</summary>
        /// <param name="acMethodName">Name of the method (Only registered methods with ACClassMethodInfo or ACMethod's)</param>
        /// <param name="acParameter">  Parameterlist for method</param>
        /// <returns>NULL, if void-Method else the result as boxed value</returns>
        public virtual object ExecuteMethod(string acMethodName, params Object[] acParameter)
        {
            AsyncMethodInvocationMode invocationMode = AsyncMethodInvocationMode.SyncNoCallback;
            if (acParameter != null && acParameter.Any())
            {
                bool rebuildParams = false;
                foreach (object param in acParameter)
                {
                    if (param is AsyncMethodInvocationMode)
                    {
                        invocationMode = (AsyncMethodInvocationMode)param;
                        rebuildParams = true;
                        break;
                    }
                }
                if (rebuildParams)
                {
                    int size = acParameter.Count();
                    if (size <= 1)
                    {
                        acParameter = null;
                    }
                    else
                    {
                        Object[] temp = new object[size - 1];
                        int i = 0;
                        foreach (object param in acParameter)
                        {
                            if (!(param is AsyncMethodInvocationMode))
                            {
                                temp[i] = param;
                                i++;
                            }
                        }
                        acParameter = temp;
                    }
                }
            }

            return ExecuteMethod(invocationMode, acMethodName, acParameter);
        }

        /// <summary>
        /// Returns a method for the given acMethodName-Parameter.
        /// </summary>
        /// <param name="acMethodName">Name of the ac method.</param>
        /// <param name="normalizedMethodName">Name of the normalized method.</param>
        /// <returns></returns>
        public ACClassMethod GetACClassMethod(string acMethodName, out string normalizedMethodName)
        {
            string acMethodName1;
            int pos = acMethodName.IndexOf('!');
            if (pos == 0)
                acMethodName1 = acMethodName.Substring(1);
            else
                acMethodName1 = acMethodName;
            normalizedMethodName = acMethodName1;

            return ACClassMethods.FirstOrDefault(c => c.ACIdentifier == acMethodName1);
        }

        internal static readonly ConcurrentDictionary<Type, HandleExecuteACMethodStatic> _StaticExecuteHandlers = new ConcurrentDictionary<Type, HandleExecuteACMethodStatic>();
        /// <summary>
        /// You cannot override static methods. As a result, acComponent cannot provide a virtual method that it is already aware of in advance. o announce these static handler methods to the iPlus framework, you must call the RegisterExecuteHandler method in the static class constructor and pass your handler method as a delegate.
        /// </summary>
        /// <param name="typeOfClass">The type of class.</param>
        /// <param name="refToStaticMethod">The reference to static method.</param>
        public static void RegisterExecuteHandler(Type typeOfClass, HandleExecuteACMethodStatic refToStaticMethod)
        {
            _StaticExecuteHandlers.TryAdd(typeOfClass, refToStaticMethod);
        }
        internal static readonly ConcurrentDictionary<Type, bool> _HasStaticAskMethods = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Invokes a method by the passed methodname.
        /// </summary>
        /// <param name="invocationMode">The invocation mode.</param>
        /// <param name="acMethodName">Name of the ac method.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns></returns>
        public virtual object ExecuteMethod(AsyncMethodInvocationMode invocationMode, string acMethodName, params Object[] acParameter)
        {
            string acMethodName1;
            ACClassMethod acClassMethod = GetACClassMethod(acMethodName, out acMethodName1);

            if (String.IsNullOrEmpty(acMethodName1) || ACType == null)
                return null;

            // 1. Versuche Methoden ohne Reflektion per Helper-Methoden auszuführen
            if (!String.IsNullOrEmpty(acMethodName1)
                && acClassMethod != null)
            {
                AsyncMethodWaitHandle syncInvocationWaitHandle = null;
                try
                {
                    // 1.1 Aufruf von nicht statiche Methoden
                    if (acClassMethod.ACKind != Global.ACKinds.MSMethodExt
                        && acClassMethod.ACKind != Global.ACKinds.MSMethodExtClient
                        && acClassMethod.ACKind != Global.ACKinds.MSMethodClient
                        && acClassMethod.ACKind != Global.ACKinds.MSWorkflow)
                    {
                        switch (acMethodName1)
                        {
                            case nameof(OpenLocalHelp):
                                OpenLocalHelp();
                                return null;
                            case nameof(BrowseOnlineHelp):
                                BrowseOnlineHelp();
                                return null;
                            case nameof(PrintSelf):
                                PrintSelf();
                                return null;
                            case nameof(DiagnosticdialogOn):
                                DiagnosticdialogOn();
                                return null;
                            case nameof(ACComponentExplorerOn):
                                ACComponentExplorerOn();
                                return null;
                            case nameof(CloseTopDialog):
                                CloseTopDialog();
                                return null;
                            case nameof(ResetConfigValuesCache):
                                ResetConfigValuesCache();
                                return null;
                            case nameof(RemoveDuplicateConfigEntries):
                                RemoveDuplicateConfigEntries();
                                return null;
                            //case nameof(IsEnabledResetConfigValuesCache):
                            //    return IsEnabledResetConfigValuesCache();
                            case nameof(GetGUI):
                                return GetGUI();
                            case nameof(ACPostInit):
                                return ACPostInit();
                            case nameof(Stop):
                                return Stop();
                            case nameof(Reload):
                                Reload();
                                return null;
                            case nameof(FindParentComponent):
                                if (acParameter[0] is ACClass)
                                    return FindParentComponent(acParameter[0] as ACClass);
                                else
                                    return FindParentComponent(acParameter[0] as Type);
                            case nameof(FindChildComponents):
                                if (acParameter[0] is ACClass)
                                    return FindChildComponents(acParameter[0] as ACClass, (int)acParameter[1]);
                                else
                                    return FindChildComponents(acParameter[0] as Type, (int)acParameter[1]);
                            case nameof(StopComponent):
                                if (acParameter[0] is string)
                                    return StopComponent(acParameter[0] as string, (bool)acParameter[1]);
                                else
                                    return StopComponent(acParameter[0] as IACComponent, (bool)acParameter[1]);
                            //case "StartComponent":
                            //    return StartComponent(acParameter[0] as string, acParameter[1], acParameter);
                            case nameof(GetChildInstanceInfo):
                                if (acParameter[1] is ChildInstanceInfoSearchParam)
                                    return GetChildInstanceInfo((int)acParameter[0], (ChildInstanceInfoSearchParam)acParameter[1]);
                                else
                                    return GetChildInstanceInfo((int)acParameter[0], (bool)acParameter[1], acParameter[2] as string);
                            case nameof(GetACUrl):
                                return GetACUrl(acParameter[0] as IACObject);
                            case nameof(GetMenu):
                                return GetMenu(acParameter[0] as string, acParameter[1] as string);
                            case nameof(DumpAsXMLString):
                                return DumpAsXMLString((int)acParameter[0]);
                            case nameof(DumpAsXMLDoc):
                                return DumpAsXMLDoc((int)acParameter[0]);
                        }

                        ACMethod acMethod = null;
                        if (acParameter != null && acParameter.Any() && acParameter[0] is ACMethod)
                            acMethod = acParameter[0] as ACMethod;

                        Object[] acParameterCorrected = acParameter;

                        // Falls ACMethod als einziger Parameter und Assemblymethode ist mit einzelnen Parametern
                        if (((acClassMethod.BasedOnACClassMethod == null && !acClassMethod.IsParameterACMethod)
                             || (acClassMethod.BasedOnACClassMethod != null && !acClassMethod.BasedOnACClassMethod.IsParameterACMethod))
                            && acParameter != null
                            && acParameter.Count() == 1
                            && acParameter.FirstOrDefault() != null
                            && typeof(ACMethod).IsAssignableFrom(acParameter.FirstOrDefault().GetType()))
                        {
                            acParameterCorrected = GetParameterFromACMethod(acParameter);
                        }

                        if (invocationMode == AsyncMethodInvocationMode.Synchronous && acClassMethod.IsAsyncProcess && acMethod != null)
                        {
                            syncInvocationWaitHandle = new AsyncMethodWaitHandle(acMethod);
                            SyncInvocationWaitHandleList.Add(syncInvocationWaitHandle);
                        }

                        object result = null;
                        // Versuche Handler mit Methodennamen aufzurufen. Falls virtuelle Methode (ACMethod) aufgerufen werden soll, dann erst mit ACMethod-Identifier aufrufen
                        bool invokedInSubclass = HandleExecuteACMethod(out result, invocationMode, acMethodName1, acClassMethod, acParameterCorrected);
                        // Falls virtuelle Methode und Handler hat methode nicht ausgeführt, dann rufe Handler mit Assemblymethodennamen auf
                        if (!invokedInSubclass && acClassMethod != null && acClassMethod.AssemblyMethodName != acMethodName1)
                            invokedInSubclass = HandleExecuteACMethod(out result, invocationMode, acClassMethod.AssemblyMethodName, acClassMethod, acParameterCorrected);
                        if (invokedInSubclass)
                        {
                            // Falls synchroner aufruf einer asynchronen Methode
                            if (syncInvocationWaitHandle != null)
                            {
                                ACMethodEventArgs eventArgs = result as ACMethodEventArgs;
                                if (!(eventArgs != null && eventArgs.ResultState == Global.ACMethodResultState.Failed))
                                    syncInvocationWaitHandle.WaitOnCallback();
                                SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                                return acMethod.ResultValueList;
                            }
                            // Sonst Asynchroner Aufruf einer synchronen oder asynchronen Methode, oder Synchroner aufruf einer Synchronen Methode
                            else
                            {
                                // Falls void-Methode
                                if (invocationMode == AsyncMethodInvocationMode.Asynchronous && (acClassMethod.IsCommand || result == null))
                                    return true;
                                return result;
                            }
                        }
                        else if (syncInvocationWaitHandle != null)
                        {
                            SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                        }
                    }
                    // Statische Methode in Klasse (Assembly) programmiert
                    else if (acClassMethod.ACKind == Global.ACKinds.MSMethodClient)
                    {
                        // Ermittle Typ: Falls AttachedFromACClassID gesetzt, dann ist eine Erweterungsmethode aus einer anderen Klasse, sonst eigene Methode in dieser Klasse
                        Type targetForInvocation = acClassMethod.AttachedFromACClassID.HasValue ? acClassMethod.AttachedFromACClass.ObjectType : ACType.ObjectType;
                        HandleExecuteACMethodStatic staticHandler = null;
                        if (_StaticExecuteHandlers.TryGetValue(targetForInvocation, out staticHandler))
                        {
                            object result = null;
                            if (staticHandler(out result, this, acMethodName1, acClassMethod, acParameter))
                                return result;
                        }
                    }
                }
                catch (Exception invokeMethodException)
                {
                    string message = string.Format(@"{0} - Method: {1}, ExceptionType: {2} Exception: {3} Trace: {4}", ACIdentifier, acMethodName1, invokeMethodException.GetType().Name, invokeMethodException.Message, invokeMethodException.StackTrace);
                    Messages.LogError(GetACUrl(), nameof(ExecuteMethod) + "(10)", message);
                    if (invokeMethodException.InnerException != null)
                    {
                        message = string.Format(@"Inner-Exception: {0}, Trace: {1}", invokeMethodException.InnerException.Message, invokeMethodException.InnerException.StackTrace);
                        Messages.LogError(GetACUrl(), nameof(ExecuteMethod) + "(20)", message);
                    }
                    OnExceptionCatched(nameof(ExecuteMethod), acMethodName1, invokeMethodException);

                    if (syncInvocationWaitHandle != null)
                    {
                        SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                    }
                    if (invocationMode == AsyncMethodInvocationMode.Asynchronous)
                        return false;

                    return null;
                }
            }


            // Von Db dynamisch nachladen wegen neu angelegten Workflows:
            if (acClassMethod == null && this is IACComponentTaskExec)
                acClassMethod = ComponentClass.GetMethod(acMethodName1, true);

            // Es sind grundsätzlich nur Kommandos erlaubt, die
            // vorher registriert wurden
            if (acClassMethod == null)
            {
                if (acMethodName1.StartsWith(Const.IsEnabledPrefix))
                {
                    return IsEnabledExecuteACMethod(acMethodName1, acParameter);
                }
                return null;
            }
            // 2. Sonst Aufruf per Reflektion
            switch (acClassMethod.ACKind)
            {
                case Global.ACKinds.MSMethod:
                case Global.ACKinds.MSMethodClient:
                case Global.ACKinds.MSMethodPrePost:
                    return InvokeAssemblyMethod(invocationMode, acClassMethod, acParameter);

                case Global.ACKinds.MSMethodExt:
                case Global.ACKinds.MSMethodExtClient:
                    return InvokeScriptMethod(invocationMode, acClassMethod, acParameter);

                case Global.ACKinds.MSMethodExtTrigger:
                    if (ScriptEngine == null)
                        return null;
                    try
                    {
                        return ScriptEngine.Execute(acMethodName1, acParameter);
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException("ACComponent", "ExecuteACMethod", msg);
                        return null;
                    }

                case Global.ACKinds.MSWorkflow:
                    return StartWorkflow(invocationMode, acClassMethod, acParameter);

                case Global.ACKinds.MSMethodFunction:
                    // Methode die durch eine Child-ACComponent vom ACKind="TPAProcessFunction" ausgeführt wird
                    ACComponent childProcessFunction = null;
                    //ACComponent childProcessFunction = GetMember(acClassMethod.ACGroup) as ACComponent;
                    if (acClassMethod.BasedOnACClassMethod != null)
                    {
                        IEnumerable<PAProcessFunction> childFunctions = FindChildComponents<PAProcessFunction>(c => c is PAProcessFunction, null, 1)
                                                                    .Where(c => c.ComponentClass.MethodsCached.Contains(acClassMethod.BasedOnACClassMethod));
                        if (acClassMethod.AttachedFromACClassID.HasValue)
                        {
                            childProcessFunction = childFunctions.Where(c => c.ComponentClass.ACClassID == acClassMethod.AttachedFromACClassID.Value
                                                                            || c.ComponentClass.IsDerivedClassFrom(acClassMethod.AttachedFromACClass)).FirstOrDefault();
                            if (childProcessFunction != null)
                                acMethodName = acClassMethod.BasedOnACClassMethod.ACIdentifier;
                        }
                        if (childProcessFunction == null && childFunctions.Any())
                            childProcessFunction = childFunctions.FirstOrDefault();
                    }

                    if (childProcessFunction != null)
                        return childProcessFunction.ExecuteMethod(invocationMode, acMethodName, acParameter);
                    else
                    {
                        Messages.LogError(this.GetACUrl(), "ACComponent.ExecuteMethod(2)", String.Format("TPAProcessFunction for {0} doesn't exist!", acMethodName));
                        if (invocationMode == AsyncMethodInvocationMode.Asynchronous)
                            return false;
                    }
                    return null;
                default:
                    return null;
            }
        }

        protected virtual void OnExceptionCatched(string methodName, string position, Exception exception)
        {
        }

        /// <summary>
        /// Internally, Reflection is used to call the method. Since Reflection is an elaborate . NET process, this can be bypassed by calling the appropriate method itself when the ExecuteACMethod method is executed.
        /// The ACComponent class provides this virtual method "HandleExecuteACMethod" for this purpose. 
        /// Overwrite the method in your class and invoke the corresponding method itsself to help to avoid reflection.
        /// The method could be either a assembly-method, script method or a workflow-method.
        /// </summary>
        /// <param name="result">The result of the invoked method</param>
        /// <param name="invocationMode">The invocation mode.</param>
        /// <param name="acMethodName">Name of the ac method.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns>true if Method was handled in the subclass.</returns>
        protected virtual bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params Object[] acParameter)
        {
            result = null;
            return false;
        }

        /// <summary>
        /// Calls the IsEnabled-Method for the passed methodname. If the IsEnabled-Method doesn't exist true will be returned by default.
        /// </summary>
        /// <param name="acMethodName">Name of the ac method.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns>
        ///   <c>true</c> if [is enabled execute ac method] [the specified ac method name]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsEnabledExecuteACMethod(string acMethodName, params Object[] acParameter)
        {
            string acMethodName1;
            ACClassMethod acClassMethod = GetACClassMethod(acMethodName, out acMethodName1);

            if (String.IsNullOrEmpty(acMethodName1) || ACType == null)
                return false;

            if (!String.IsNullOrEmpty(acMethodName1))
            {
                bool boolHasEnabledPrefix = acMethodName1.StartsWith(Const.IsEnabledPrefix);
                if (acClassMethod == null
                     || !boolHasEnabledPrefix
                     || (acClassMethod.ACKind != Global.ACKinds.MSMethodExt && acClassMethod.ACKind != Global.ACKinds.MSMethodExtClient)
                   )
                {
                    try
                    {
                        string isEnabledMethodName = acMethodName1;
                        if (!boolHasEnabledPrefix)
                            isEnabledMethodName = Const.IsEnabledPrefix + isEnabledMethodName;
                        else
                            acMethodName = isEnabledMethodName.Substring(Const.IsEnabledPrefix.Length);

                        // Statische Methode in Klasse (Assembly) programmiert
                        if (acClassMethod != null && acClassMethod.ACKind == Global.ACKinds.MSMethodClient)
                        {
                            // Ermittle Typ: Falls AttachedFromACClassID gesetzt, dann ist eine Erweterungsmethode aus einer anderen Klasse, sonst eigene Methode in dieser Klasse
                            Type targetForInvocation = acClassMethod.AttachedFromACClassID.HasValue ? acClassMethod.AttachedFromACClass.ObjectType : ACType.ObjectType;

                            HandleExecuteACMethodStatic staticHandler = null;
                            if (targetForInvocation != null && _StaticExecuteHandlers.TryGetValue(targetForInvocation, out staticHandler))
                            {
                                object result = null;
                                if (staticHandler(out result, this, isEnabledMethodName, acClassMethod, acParameter))
                                    return (bool)result;
                            }
                        }
                        else
                        {
                            switch (isEnabledMethodName)
                            {
                                case Const.IsEnabledPrefix + "OpenHelp":
                                    return IsEnabledOpenHelp();
                                case Const.IsEnabledPrefix + "PrintSelf":
                                    return IsEnabledPrintSelf();
                                case Const.IsEnabledPrefix + "DiagnosticdialogOn":
                                    return IsEnabledDiagnosticdialogOn();
                                case Const.IsEnabledPrefix + "ACComponentExplorerOn":
                                    return IsEnabledACComponentExplorerOn();
                            }
                            object result = null;
                            // Versuche Handler mit Methodennamen aufzurufen. Falls virtuelle Methode (ACMethod) aufgerufen werden soll, dann erst mit ACMethod-Identifier aufrufen
                            bool invokedInSubclass = HandleExecuteACMethod(out result, AsyncMethodInvocationMode.Synchronous, isEnabledMethodName, acClassMethod, acParameter);
                            // Falls virtuelle Methode und Handler hat methode nicht ausgeführt, dann rufe Handler mit Assemblymethodennamen auf
                            if (!invokedInSubclass && acClassMethod != null && acClassMethod.AssemblyMethodName != acMethodName1)
                                invokedInSubclass = HandleExecuteACMethod(out result, AsyncMethodInvocationMode.Synchronous, Const.IsEnabledPrefix + acClassMethod.AssemblyMethodName, acClassMethod, acParameter);
                            if (invokedInSubclass)
                            {
                                if ((bool)result && acClassMethod != null && CurrentRightsOfInvoker != null)
                                {
                                    Global.ControlModes controlMode = CurrentRightsOfInvoker.GetControlMode(acClassMethod);
                                    if (controlMode <= Global.ControlModes.Disabled)
                                        result = false;
                                }
                                return (bool)result;
                            }
                        }
                    }
                    catch (Exception invokeMethodException)
                    {
                        string message = string.Format(@"{0} - Method: {1}, ExceptionType: {2} Exception: {3} Trace: {4}", ACIdentifier, acMethodName1, invokeMethodException.GetType().Name, invokeMethodException.Message, invokeMethodException.StackTrace);
                        Messages.LogError(GetACUrl(), "ACComponent.IsEnabledExecuteACMethod(1)", message);
                        if (invokeMethodException.InnerException != null)
                        {
                            message = string.Format(@"Inner-Exception: {0}, Trace: {1}", invokeMethodException.InnerException.Message, invokeMethodException.InnerException.StackTrace);
                            Messages.LogError(GetACUrl(), "ACComponent.IsEnabledExecuteACMethod(2)", message);
                        }
                        return false;
                    }
                }
            }


            // Von Db dynamisch nachladen wegen neu angelegten Workflows:
            if (acClassMethod == null)
                acClassMethod = ComponentClass.GetMethod(acMethodName1, true);

            // Es sind grundsätzlich nur Kommandos erlaubt, die
            // vorher registriert wurden
            if (acClassMethod == null)
            {
                if (acMethodName1.StartsWith(Const.IsEnabledPrefix))
                {
                    return InvokeIsEnabledMethod(acMethodName1, acClassMethod, acParameter);
                }
                return false;
            }

            // Falls AutoEnabled, dann gibt es keine IsEnabled-Methode und das 
            // Komando ist immer verfügbar
            if (acClassMethod.IsAutoenabled)
            {
                if (acClassMethod != null && CurrentRightsOfInvoker != null)
                {
                    Global.ControlModes controlMode = CurrentRightsOfInvoker.GetControlMode(acClassMethod);
                    if (controlMode <= Global.ControlModes.Disabled)
                        return false;
                }
                return true;
            }

            bool resultOfReflectionCall = false;
            switch (acClassMethod.ACKind)
            {
                case Global.ACKinds.MSMethod:
                case Global.ACKinds.MSMethodClient:
                case Global.ACKinds.MSMethodPrePost:
                    resultOfReflectionCall = InvokeIsEnabledMethod(acMethodName1, acClassMethod, acParameter);
                    break;
                case Global.ACKinds.MSMethodExt:
                case Global.ACKinds.MSMethodExtClient:
                    if (ScriptEngine == null)
                        return false;
                    if (acParameter == null)
                        acParameter = new object[] { this };
                    else
                    {
                        int parameterCount = 0;
                        if (acParameter != null)
                            parameterCount = acParameter.Count();
                        object[] acParamWithThis = new object[parameterCount + 1];
                        acParamWithThis[0] = this;
                        if (parameterCount >= 1)
                        {
                            for (int i = 0; i < acParameter.Count(); i++)
                            {
                                acParamWithThis[i + 1] = acParameter[i];
                            }
                        }
                        acParameter = acParamWithThis;
                    }
                    resultOfReflectionCall = (bool)ScriptEngine.Execute(Const.IsEnabledPrefix + acMethodName1, acParameter);
                    break;
                default:
                    resultOfReflectionCall = true;
                    break;
            }

            if (resultOfReflectionCall && acClassMethod != null && CurrentRightsOfInvoker != null)
            {
                Global.ControlModes controlMode = CurrentRightsOfInvoker.GetControlMode(acClassMethod);
                if (controlMode <= Global.ControlModes.Disabled)
                    resultOfReflectionCall = false;
            }
            return resultOfReflectionCall;
        }

        internal static bool IsACComponentMethod(string acMethodName)
        {
            return acMethodName == "OpenHelp"
                || acMethodName == "PrintSelf"
                || acMethodName == "DiagnosticdialogOn"
                || acMethodName == "CloseTopDialog"
                || acMethodName == "ResetConfigValuesCache"
                || acMethodName == "GetGUI"
                || acMethodName == "ACPostInit"
                || acMethodName == "Stop"
                || acMethodName == "GetACComponent"
                || acMethodName == "FindParentComponent"
                || acMethodName == "FindChildComponents"
                || acMethodName == "StopACComponentByName"
                || acMethodName == "StopACComponent"
                || acMethodName == "GetChildInstanceInfo"
                || acMethodName == "GetACUrl"
                || acMethodName == "GetMenu"
                || acMethodName == "DumpAsXMLString"
                || acMethodName == "DumpAsXMLDoc"
                || acMethodName == Const.IsEnabledPrefix + "OpenHelp"
                || acMethodName == Const.IsEnabledPrefix + "PrintSelf"
                || acMethodName == Const.IsEnabledPrefix + "DiagnosticdialogOn";
        }


        protected Object[] GetParameterFromACMethod(Object[] acParameter)
        {
            ACMethod acMethod = acParameter.FirstOrDefault() as ACMethod;
            if (acMethod == null)
                return null;

            List<object> paramList = new List<object>();
            foreach (var param in acMethod.ParameterValueList)
            {
                paramList.Add(param.Value);
            }

            return paramList.ToArray();
        }

        private bool InvokeIsEnabledMethod(string acMethodName, ACClassMethod acClassMethod, Object[] acParameter)
        {
            // Abfragen eines Werts
            // Mögliche Fehlerquellen:
            // 1. Entsprechende IsEnabled-Methode ist nicht vorhanden
            // 2. Entsprechende IsEnabled-Methode ist nicht public deklariert
            try
            {
                string isEnabledMethodName = acMethodName;
                if (!isEnabledMethodName.StartsWith(Const.IsEnabledPrefix))
                    isEnabledMethodName = Const.IsEnabledPrefix + isEnabledMethodName;
                else
                    acMethodName = isEnabledMethodName.Substring(Const.IsEnabledPrefix.Length);

                MethodInfo mi = GetType().GetMethod(isEnabledMethodName);
                bool isStatic = false;
                bool isAttachedMethod = false;
                if (mi == null)
                {
                    isStatic = true;
                    mi = GetType().GetMethod(isEnabledMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    if (mi == null)
                    {
                        if (ACType.ObjectType == null)
                            return false;
                        if (acClassMethod != null && acClassMethod.AttachedFromACClassID.HasValue)
                        {
                            mi = acClassMethod.AttachedFromACClass.ObjectType.GetMethod(isEnabledMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                            isAttachedMethod = true;
                        }
                        else
                            mi = ACType.ObjectType.GetMethod(isEnabledMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        if (mi == null)
                            return false;
                    }
                }
                if (mi != null)
                {
                    isStatic = mi.IsStatic;
                }
                if (isStatic)
                {
                    int parameterCount = 0;
                    if (acParameter != null)
                        parameterCount = acParameter.Count();
                    object[] acParamWithThis = new object[parameterCount + 1];
                    acParamWithThis[0] = this;
                    if (parameterCount >= 1)
                    {
                        for (int i = 0; i < acParameter.Count(); i++)
                        {
                            acParamWithThis[i + 1] = acParameter[i];
                        }
                    }
                    acParameter = acParamWithThis;
                }

                if (acParameter != null && acParameter.Any())
                {
                    ParameterInfo[] miParams = mi.GetParameters();
                    if (miParams == null || (miParams != null && !miParams.Any()))
                        acParameter = null;
                }

#if DEBUG
                if (!isAttachedMethod)
                    Messages.LogWarning(this.GetACUrl(), "InvokeIsEnabledMethod()", String.Format("Please override HandleExecuteACMethod() and handle Method {0} to avoid reflection!", isEnabledMethodName));
#endif


                return (bool)mi.Invoke(isAttachedMethod ? null : this, acParameter);
            }
            catch (Exception e)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(e.Message);
#endif
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACComponent", "InvokeIsEnabledMethod", msg);

                return false;
            }
        }


        /// <summary>
        /// Calls a assemblymethod for the passed ACClassMethod
        /// </summary>
        /// <param name="invocationMode">The invocation mode.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected object InvokeAssemblyMethod(AsyncMethodInvocationMode invocationMode, ACClassMethod acClassMethod, Object[] acParameter)
        {
            AsyncMethodWaitHandle syncInvocationWaitHandle = null;
            MethodInfo mi = null;
            try
            {
                ACMethod acMethod = null;
                if (acParameter != null && acParameter.Any() && acParameter[0] is ACMethod)
                    acMethod = acParameter[0] as ACMethod;

                // Falls ACMethod als einziger Parameter und Assemblymethode ist mit einzelnen Parametern
                if (((acClassMethod.BasedOnACClassMethod == null && !acClassMethod.IsParameterACMethod)
                     || (acClassMethod.BasedOnACClassMethod != null && !acClassMethod.BasedOnACClassMethod.IsParameterACMethod))
                    && acParameter != null
                    && acParameter.Count() == 1
                    && acParameter.FirstOrDefault() != null
                    && typeof(ACMethod).IsAssignableFrom(acParameter.FirstOrDefault().GetType()))
                {
                    acParameter = GetParameterFromACMethod(acParameter);
                }
                bool isStatic = false;
                bool isAttachedMethod = false;

                Type typeOfThis = GetType();
                try
                {
                    mi = typeOfThis.GetMethod(acClassMethod.AssemblyMethodName);
                }
                catch (AmbiguousMatchException matchEx)
                {
                    string msg = matchEx.Message;
                    if (matchEx.InnerException != null && matchEx.InnerException.Message != null)
                        msg += " Inner:" + matchEx.InnerException.Message;

                    Messages.LogException("ACComponent", "InvokeAssemblyMethod", msg);

                    if (acParameter != null && acParameter.Any())
                    {
                        Type[] paramTypes = acParameter.Select(c => c != null ? c.GetType() : typeof(object)).ToArray();
                        mi = typeOfThis.GetMethod(acClassMethod.AssemblyMethodName, paramTypes);
                    }
                    else
                        return null;
                }

                if ((mi == null) /*&& (acClassMethod.ACKind == Global.ACKinds.MSMethodClient)*/)
                {
                    isStatic = true;
                    try
                    {
                        if (acClassMethod != null && acClassMethod.AttachedFromACClassID.HasValue)
                        {
                            isAttachedMethod = true;
                            mi = acClassMethod.AttachedFromACClass.ObjectType.GetMethod(acClassMethod.AssemblyMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        }
                        else
                            mi = typeOfThis.GetMethod(acClassMethod.AssemblyMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    }
                    catch (AmbiguousMatchException matchEx)
                    {
                        string msg = matchEx.Message;
                        if (matchEx.InnerException != null && matchEx.InnerException.Message != null)
                            msg += " Inner:" + matchEx.InnerException.Message;

                        Messages.LogException("ACComponent", "InvokeAssemblyMethod(10)", msg);

                        if (acParameter != null && acParameter.Any())
                        {
                            Type[] paramTypes = acParameter.Select(c => c != null ? c.GetType() : typeof(object)).ToArray();
                            if (acClassMethod != null && acClassMethod.AttachedFromACClassID.HasValue)
                            {
                                isAttachedMethod = true;
                                mi = acClassMethod.AttachedFromACClass.ObjectType.GetMethod(acClassMethod.AssemblyMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, paramTypes, null);
                            }
                            else
                                mi = typeOfThis.GetMethod(acClassMethod.AssemblyMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, paramTypes, null);
                        }
                        else
                            return null;
                    }
                    if (mi == null && !isAttachedMethod)
                    {
                        if (ACType.ObjectType == null)
                            return null;
                        try
                        {
                            mi = ACType.ObjectType.GetMethod(acClassMethod.AssemblyMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        }
                        catch (AmbiguousMatchException matchEx)
                        {
                            string msg = matchEx.Message;
                            if (matchEx.InnerException != null && matchEx.InnerException.Message != null)
                                msg += " Inner:" + matchEx.InnerException.Message;

                            Messages.LogException("ACComponent", "InvokeAssemblyMethod(20)", msg);

                            if (acParameter != null && acParameter.Any())
                            {
                                Type[] paramTypes = acParameter.Select(c => c.GetType()).ToArray();
                                mi = ACType.ObjectType.GetMethod(acClassMethod.AssemblyMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, paramTypes, null);
                            }
                            else
                                return null;
                        }
                        if (mi == null)
                            return null;
                    }
                }
                if (mi != null)
                {
                    isStatic = mi.IsStatic;
                }

                if (invocationMode == AsyncMethodInvocationMode.Synchronous && acClassMethod.IsAsyncProcess && acMethod != null)
                {
                    syncInvocationWaitHandle = new AsyncMethodWaitHandle(acMethod);
                    SyncInvocationWaitHandleList.Add(syncInvocationWaitHandle);
                }

                // Bei Aufrufe einer statischen Client-Methode muss erster parameter IACComponent sein:
                if (acClassMethod.ACKind == Global.ACKinds.MSMethodClient)
                {
                    int parameterCount = 0;
                    if (acParameter != null)
                        parameterCount = acParameter.Count();
                    object[] acParamWithThis = new object[parameterCount + 1];
                    acParamWithThis[0] = this;
                    if (parameterCount >= 1)
                    {
                        for (int i = 0; i < acParameter.Count(); i++)
                        {
                            acParamWithThis[i + 1] = acParameter[i];
                        }
                    }
                    acParameter = acParamWithThis;
                }

                object result = null;
                if (acParameter == null || !acParameter.Any())
                {
                    ParameterInfo[] miParams = mi.GetParameters();
                    if (miParams != null && miParams.Any())
                    {
                        acParameter = new object[miParams.Count()];
                        bool allOptional = true;
                        int i = 0;
                        foreach (ParameterInfo info in miParams)
                        {
                            if (!info.Attributes.HasFlag(ParameterAttributes.Optional))
                            {
                                allOptional = false;
                                break;
                            }
                            else
                                acParameter[i] = Type.Missing;
                            i++;
                        }
                        if (!allOptional)
                            throw new Exception(string.Format("Wrong parameter count! Parameters: {0}", string.Join(",", miParams.Select(x => x.Name))));
                    }
                    //else if (acParameter != null)
                    //    acParameter = null;
                }

#if DEBUG
                if (!isAttachedMethod)
                    Messages.LogWarning(this.GetACUrl(), "InvokeAssemblyMethod()", String.Format("Please override HandleExecuteACMethod() and handle Method {0} to avoid reflection!", acClassMethod.ACIdentifier));
#endif

                result = mi.Invoke(isAttachedMethod ? null : this, acParameter);

                // Falls synchroner aufruf einer asynchronen Methode
                if (syncInvocationWaitHandle != null)
                {
                    ACMethodEventArgs eventArgs = result as ACMethodEventArgs;
                    if (!(eventArgs != null && eventArgs.ResultState == Global.ACMethodResultState.Failed))
                        syncInvocationWaitHandle.WaitOnCallback();
                    SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                    return acMethod.ResultValueList;
                }
                // Sonst Asynchroner Aufruf einer synchronen oder asynchronen Methode, oder Synchroner aufruf einer Synchronen Methode
                else
                {
                    // Falls void-Methode
                    if (invocationMode == AsyncMethodInvocationMode.Asynchronous && (acClassMethod.IsCommand || result == null))
                        return true;
                    return result;
                }
            }
            catch (Exception invokeMethodException)
            {
                string methodName = "-";
                string paramsName = "";
                if (mi != null)
                {
                    methodName = mi.Name;
                    paramsName = string.Join(",", mi.GetParameters().Select(x => x.ParameterType.Name + " " + x.Name));
                }
                string message = string.Format(@"{0} - Method: {1}, Params: [{2}] ExceptionType: {3} Exception: {4} Trace: {5}", ACIdentifier, methodName, paramsName, invokeMethodException.GetType().Name, invokeMethodException.Message, invokeMethodException.StackTrace);
                Messages.LogError(GetACUrl(), "InvokeAssemblyMethod(0)", message);
                if (invokeMethodException.InnerException != null)
                {
                    message = string.Format(@"Inner-Exception: {0}, Trace: {1}", invokeMethodException.InnerException.Message, invokeMethodException.InnerException.StackTrace);
                    Messages.LogError(GetACUrl(), "InvokeAssemblyMethod(1)", message);
                }
                if (syncInvocationWaitHandle != null)
                {
                    SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                }
                if (invocationMode == AsyncMethodInvocationMode.Asynchronous)
                    return false;
                return null;
            }
        }


        /// <summary>
        /// Invokes the script method.
        /// </summary>
        /// <param name="invocationMode">The invocation mode.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns></returns>
        protected object InvokeScriptMethod(AsyncMethodInvocationMode invocationMode, ACClassMethod acClassMethod, Object[] acParameter)
        {
            AsyncMethodWaitHandle syncInvocationWaitHandle = null;
            try
            {
                if (ScriptEngine == null)
                    return null;

                ACMethod acMethod = null;
                if (acParameter != null && acParameter.Any() && acParameter[0] is ACMethod)
                    acMethod = acParameter[0] as ACMethod;

                // Falls ACMethod als einziger Parameter und Assemblymethode ist mit einzelnen Parametern
                if (((acClassMethod.BasedOnACClassMethod == null && !acClassMethod.IsParameterACMethod)
                     || (acClassMethod.BasedOnACClassMethod != null && !acClassMethod.BasedOnACClassMethod.IsParameterACMethod))
                    && acParameter != null
                    && acParameter.Count() == 1
                    && acParameter.FirstOrDefault() != null
                    && typeof(ACMethod).IsAssignableFrom(acParameter.FirstOrDefault().GetType()))
                {
                    acParameter = GetParameterFromACMethod(acParameter);
                }
                MethodInfo mi = GetType().GetMethod(acClassMethod.ACIdentifier);

                if (invocationMode == AsyncMethodInvocationMode.Synchronous && acClassMethod.IsAsyncProcess && acMethod != null)
                {
                    syncInvocationWaitHandle = new AsyncMethodWaitHandle(acMethod);
                    SyncInvocationWaitHandleList.Add(syncInvocationWaitHandle);
                }


                if (acParameter == null)
                    acParameter = new object[] { this };
                else
                {
                    int parameterCount = 0;
                    if (acParameter != null)
                        parameterCount = acParameter.Count();
                    object[] acParamWithThis = new object[parameterCount + 1];
                    acParamWithThis[0] = this;
                    if (parameterCount >= 1)
                    {
                        for (int i = 0; i < acParameter.Count(); i++)
                        {
                            acParamWithThis[i + 1] = acParameter[i];
                        }
                    }
                    acParameter = acParamWithThis;
                }

                object result = ScriptEngine.Execute(acClassMethod.ACIdentifier, acParameter);

                // Falls synchroner aufruf einer asynchronen Methode
                if (syncInvocationWaitHandle != null)
                {
                    syncInvocationWaitHandle.WaitOnCallback();
                    SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                    return acMethod.ResultValueList;
                }
                // Sonst Asynchroner Aufruf einer synchronen oder asynchronen Methode, oder Synchroner aufruf einer Synchronen Methode
                else
                {
                    // Falls void-Methode
                    if (invocationMode == AsyncMethodInvocationMode.Asynchronous && (acClassMethod.IsCommand || result == null))
                        return true;
                    return result;
                }
            }
            catch (Exception e)
            {
                if (syncInvocationWaitHandle != null)
                {
                    SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                }

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACComponent", "InvokeScriptMethod", msg);

                if (invocationMode == AsyncMethodInvocationMode.Asynchronous)
                    return false;
                return null;
            }
        }


        /// <summary>
        /// Starts the workflow.
        /// </summary>
        /// <param name="invocationMode">The invocation mode.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns></returns>
        protected object StartWorkflow(AsyncMethodInvocationMode invocationMode, ACClassMethod acClassMethod, Object[] acParameter)
        {
            AsyncMethodWaitHandle syncInvocationWaitHandle = null;
            try
            {
                if (acParameter == null || acParameter.Count() < 1 || !(acParameter[0] is ACMethod) /*|| _SyncInvocationWaitHandle != null*/)
                    return false;
                ACMethod acMethod = acParameter[0] as ACMethod;
                PWProcessFunction pwProcessFunction = InstanceNewWorkflow(invocationMode, acClassMethod, acMethod);
                if (pwProcessFunction == null)
                {
                    if (invocationMode == AsyncMethodInvocationMode.Asynchronous)
                        return false;
                    else
                        return null;
                }

                if (invocationMode == AsyncMethodInvocationMode.Synchronous)
                {
                    syncInvocationWaitHandle = new AsyncMethodWaitHandle(acMethod);
                    SyncInvocationWaitHandleList.Add(syncInvocationWaitHandle);
                }

                ACMethodEventArgs result = pwProcessFunction.Start(acParameter[0] as ACMethod);
                if (result.ResultState == Global.ACMethodResultState.Failed || result.ResultState == Global.ACMethodResultState.Notpossible)
                {
                    if (syncInvocationWaitHandle != null)
                        SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                }
                // Falls synchroner aufruf einer asynchronen Methode
                else if (syncInvocationWaitHandle != null)
                {
                    syncInvocationWaitHandle.WaitOnCallback();
                    SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                }
                if (invocationMode == AsyncMethodInvocationMode.Asynchronous)
                    return result;
                else
                {
                    if (result.ResultState == Global.ACMethodResultState.Succeeded)
                    {
                        //return acMethod.ResultValueList;
                        return result;
                    }
                    else
                    {
                        //return null;
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                string message = string.Format(@"{0} - Workflow: {1}, ExceptionType: {2} Exception: {3} Trace: {4}", ACIdentifier, acClassMethod.ACIdentifier, e.GetType().Name, e.Message, e.StackTrace);
                Messages.LogError(GetACUrl(), "StartWorkflow(0)", message);
                if (e.InnerException != null)
                {
                    message = string.Format(@"Inner-Exception: {0}, Trace: {1}", e.InnerException.Message, e.InnerException.StackTrace);
                    Messages.LogError(GetACUrl(), "StartWorkflow(1)", message);
                }

                if (syncInvocationWaitHandle != null)
                {
                    SyncInvocationWaitHandleList.Remove(syncInvocationWaitHandle);
                }

                if (invocationMode == AsyncMethodInvocationMode.Asynchronous)
                    return false;
                else
                    return null;
            }
        }


        /// <summary>
        /// Returns a new virtual method (ACMethod) for the passed acurl. In most cases acUrl is the methodname of the virtual method.
        /// </summary>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="database">The database.</param>
        /// <returns></returns>
        public virtual ACMethod ACUrlACTypeSignature(string acUrl, Database database = null)
        {
            if (database == null)
            {
                ACMethod acMethod = ComponentClass.ACUrlACTypeSignature(acUrl, database);
                //if (acMethod != null && database != null)
                //    acMethod.AttachTo(database);
                return acMethod;
            }

            using (ACMonitor.Lock(database.QueryLock_1X000))
            {
                ACClass acClass = database.ACClass.Where(c => c.ACClassID == ACType.ACTypeID).First();
                return acClass.ACUrlACTypeSignature(acUrl, database);
            }
        }


        /// <summary>Creates a new Workflow-Instance</summary>
        /// <param name="invocationMode"></param>
        /// <param name="acClassMethod"></param>
        /// <param name="acMethod"></param>
        /// <param name="acClassTask">Wird nur benötigt bei Instanziierung anhand SP-Eintrag</param>
        /// <returns>
        ///   <br />
        /// </returns>
        protected PWProcessFunction InstanceNewWorkflow(AsyncMethodInvocationMode invocationMode, ACClassMethod acClassMethod, ACMethod acMethod, ACClassTask acClassTask = null)
        {
            ACValueList acParameter = acMethod.ParameterValueList;
            ACValue paramAsync = acParameter.GetACValue("invocationMode");
            if (paramAsync == null)
            {
                paramAsync = new ACValue() { ACIdentifier = "invocationMode", Value = invocationMode, ValueTypeACClass = gip.core.datamodel.Database.GlobalDatabase.GetACType(typeof(AsyncMethodInvocationMode).Name) };
                acParameter.Add(paramAsync);
            }
            else
                acParameter["invocationMode"] = invocationMode;

            PWProcessFunction pwProcessFunc = null;
            try
            {
#if DEBUG
                ACActivator.DiagBeginInstanceNewWorkflow(this.GetACUrl());
#else
                Messages.LogDebug("ACComponent", "Begin InstanceNewWorkflow()", String.Format("ACUrl={0}; ThreadID={1};", this.GetACUrl(), Thread.CurrentThread.ManagedThreadId));
#endif
                if (acClassMethod.Database != null)
                {
                    using (ACMonitor.Lock(acClassMethod.Database.QueryLock_1X000))
                    {
                        acClassMethod.AutoRefresh();
                    }
                }

                ACClassWF rootWF = acClassMethod.RootWFNode as ACClassWF;
                if (rootWF == null)
                    return null;

                if (acClassMethod.MustRefreshACClassWF && acClassMethod.Database != null)
                {
                    using (ACMonitor.Lock(acClassMethod.Database.QueryLock_1X000))
                    {
                        rootWF.AutoRefresh();
                    }
                }

                // Typ muss vom globalen Context sein
                ACClass typeOfWFRoot = Root.Database.ContextIPlus.GetACType(rootWF.PWACClassID);
                if (typeOfWFRoot == null)
                    return null;

                // ContentACClassWF muss vom Task-Kontext sein
                rootWF = ACClassTaskQueue.TaskQueue.GetACClassWFFromTaskQueueCache(rootWF.ACClassWFID);
                if (rootWF == null)
                    return null;

                if (acClassMethod.MustRefreshACClassWF)
                {
                    ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        if (rootWF.ACClassMethod != null)
                            rootWF.ACClassMethod.AutoRefresh();
                    });
                }

                pwProcessFunc = StartComponent(typeOfWFRoot, rootWF, acParameter, Global.ACStartTypes.Automatic, false) as PWProcessFunction;

                if (acClassMethod.MustRefreshACClassWF)
                {
                    ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        if (rootWF.ACClassMethod != null)
                            rootWF.ACClassMethod.SetACClassWFRefreshed();
                    });
                    acClassMethod.SetACClassWFRefreshed();
                }
            }
            catch (ACCreateException e)
            {
                if (e.InvalidObject != null && e.InvalidObject is PWProcessFunction)
                {
                    pwProcessFunc = e.InvalidObject as PWProcessFunction;
                    pwProcessFunc.UnloadWorkflow();
                    pwProcessFunc = null;
                }

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACComponent", "InstanceNewWorkflow", msg);
            }
            finally
            {
#if DEBUG
                ACActivator.DiagEndInstanceNewWorkflow(this.GetACUrl(), pwProcessFunc != null ? pwProcessFunc.GetACUrl() : "null");
#else
                Messages.LogDebug("ACComponent", "End InstanceNewWorkflow()", String.Format("ACUrl={0}; ThreadID={2}; Result={1}", this.GetACUrl(), pwProcessFunc != null ? pwProcessFunc.GetACUrl() : "null", Thread.CurrentThread.ManagedThreadId));
#endif
            }

            return pwProcessFunc;
        }


        /// <summary>
        /// Returns a new virtual method (ACMethod) for the passed methodName.
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public ACMethod NewACMethod(string methodName)
        {
            ACClassMethod acClassMethod = ACClassMethods.FirstOrDefault(c => c.ACIdentifier == methodName);
            if (acClassMethod != null)
                return acClassMethod.TypeACSignature();
            return null;
        }


        /// <summary>
        /// Gets the Control-Mode for a WPF-Control (that implements IVBContent).
        /// This method should evaluates the RightControlMode-Property (User-Rights) and calls CheckPropertyMinMax() to validate the value.
        /// If user has rights to see the value, then the virtual OnGetControlModes() is called first to ask the derivations.
        /// Every WPF-Control (IVBContent) calls this method every time when the bound value (binding via VBContent) changes and the BSOACComponent is this instance.
        /// </summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public virtual Global.ControlModesInfo GetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return Global.ControlModesInfo.Enabled;

            // 1. Stufe: Wenn Unsichtbarkeit im XAML fest vorgegeben, dann gilt diese Einstellung prior
            if (!string.IsNullOrEmpty(vbControl.DisabledModes))
            {
                if (vbControl.DisabledModes == "Hidden" || vbControl.DisabledModes == "hidden")
                    return Global.ControlModesInfo.Hidden;
                else if (vbControl.DisabledModes == "Collapsed" || vbControl.DisabledModes == "collapsed")
                    return Global.ControlModesInfo.Collapsed;
            }

            // 2. Stufe: Unsichtbarkeit per Benutzerrechte: Falls Benutzer keine Rechte zum Lesen hat dann schalte Control unsichtbar
            if ((vbControl.RightControlMode == Global.ControlModes.Hidden) || (vbControl.RightControlMode == Global.ControlModes.Collapsed))
                return new Global.ControlModesInfo() { Mode = vbControl.RightControlMode };

            // 3.Stufe: Unsichtbarkeit per Programmspezifische Rechte aus Ableitung
            Global.ControlModesInfo newMode = new Global.ControlModesInfo() { Mode = OnGetControlModes(vbControl) };
            if ((newMode.Mode == Global.ControlModes.Hidden) || (newMode.Mode == Global.ControlModes.Collapsed))
            {
                // TODO: Hole evtl. Wert über ACUrlCommand und Checke ob Null;
                return newMode;
            }

            // 4. Stufe: wenn Schreibschutz im XAML fest vorgegeben, dann gilt diese Einstellung prior
            if (!string.IsNullOrEmpty(vbControl.DisabledModes) && (vbControl.DisabledModes == "Disabled" || vbControl.DisabledModes == "disabled"))
            {
                newMode.Mode = Global.ControlModes.Disabled;
                // TODO: Hole evtl. Wert über ACUrlCommand und Checke ob Null;
                return newMode;
            }

            // 5. Stufe: Schreibschutz per Rechteverwaltung
            if (vbControl.RightControlMode == Global.ControlModes.Disabled)
            {
                newMode.Mode = Global.ControlModes.Disabled;
                // TODO: Hole evtl. Wert über ACUrlCommand und Checke ob Null;
                return newMode;
            }

            // 6.Stufe: Programmspezifische Rechte aus Ableitung
            // Global.ControlModes newMode = OnGetControlModes(vbControl);
            if ((newMode.Mode <= Global.ControlModes.Disabled) || (newMode.Mode == Global.ControlModes.EnabledWrong))
            {
                // TODO: Hole evtl. Wert über ACUrlCommand und Checke ob Null;
                return newMode;
            }

            // 6.Stufe: Überprüfe Min/Max-Werte
            Global.ControlModesInfo newMode2 = CheckPropertyMinMax(vbControl);
            if (newMode2.Mode >= newMode.Mode || (newMode2.Mode < Global.ControlModes.Enabled && newMode2.Mode < newMode.Mode))
                newMode = newMode2;

            return newMode;
        }


        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public virtual Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            return Global.ControlModes.Enabled;
        }

        #endregion


        #region ACClassMethod
        IEnumerable<ACClassMethod> _ACClassMethodsCached = null; // Nötig für Produktivbetrieb um zum einen Deadlocks zuvermeiden und zweitens die Performance zu erhöhen
        /// <summary>THREAD-SAFE: Returns a new List of all methods (metadata) of this Instance</summary>
        /// <value>List of ACClassMethod</value>
        public IEnumerable<ACClassMethod> ACClassMethods
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_ACClassMethodsCached != null)
                        return _ACClassMethodsCached;
                    LoadCachedACClassMethods();
                    return _ACClassMethodsCached;
                }
            }
        }

        private void LoadCachedACClassMethods()
        {
            if (_ACClassMethodsCached != null)
                return;
            if (this.ComponentClass == null)
                _ACClassMethodsCached = new ACClassMethod[] { };
            else
                _ACClassMethodsCached = ComponentClass.MethodsCached.ToArray();
        }

        public virtual bool PreExecute([CallerMemberName] string acMethod = "")
        {
            Msg result = PreExecuteMsg(acMethod);
            return result == null;
        }

        public virtual Msg PreExecuteMsg(string acMethod)
        {
            if (ScriptEngine == null || String.IsNullOrEmpty(acMethod))
                return null;
            return ScriptEngine.TriggerScript(ScriptTrigger.Type.PreExecute, acMethod, new object[] { this }) as Msg;
        }

        public virtual void PostExecute([CallerMemberName] string acMethod = "")
        {
            if (ScriptEngine == null || String.IsNullOrEmpty(acMethod))
                return;
            ScriptEngine.TriggerScript(ScriptTrigger.Type.PostExecute, acMethod, new object[] { this });
        }

        public virtual void OnSetACProperty(object value, IACMember callingProperty)
        {
            if (ScriptEngine == null)
                return;
            ScriptEngine.TriggerScript(ScriptTrigger.Type.OnSetACProperty, callingProperty.ACType.ACIdentifier, new object[] { this, value, callingProperty });
        }

        public virtual void OnSetACPropertyNet(IACPropertyNetValueEvent valueEvent, IACPropertyNetBase callingProperty)
        {
            if (ScriptEngine == null)
                return;
            ScriptEngine.TriggerScript(ScriptTrigger.Type.OnSetACPropertyNet, callingProperty.ACType.ACIdentifier, new object[] { this, valueEvent, callingProperty });
        }

        public virtual bool OnSetACPoint(IACPointNetBase changedPoint)
        {
            if (ScriptEngine == null)
                return false;
            return (bool)ScriptEngine.TriggerScript(ScriptTrigger.Type.OnSetACPoint, changedPoint.ACIdentifier, new object[] { this, changedPoint });
        }
        #endregion


        #region ACClassDesign
        /// <summary>Gets a design</summary>
        /// <param name="kindOfDesign">Search-Filter for ACClassDesign.ACKindIndex</param>
        /// <param name="usageOfDesign">Search-Filter for ACClassDesign.ACUsageIndex</param>
        /// <returns>ACClassDesign if found</returns>
        public ACClassDesign GetDesign(Global.ACKinds kindOfDesign, Global.ACUsages usageOfDesign)
        {
            if (ACType == null)
                return null;
            return ACType.Designs.Where(c => c.ACKindIndex == (short)kindOfDesign && c.ACUsageIndex == (int)usageOfDesign).FirstOrDefault();
        }


        /// <summary>Gets a design</summary>
        /// <param name="acIdentifier">ACIdentifier of the Design (ACClassDesign.ACIdentifier)</param>
        /// <returns></returns>
        public ACClassDesign GetDesign(string acIdentifier)
        {
            if (ACType == null)
                return null;
            return ACType.GetDesign(acIdentifier);
        }


        /// <summary>Gets a design</summary>
        /// <param name="kindOfDesign">Search-Filter for ACClassDesign.ACKindIndex</param>
        /// <param name="useDefault">Search-Filter for ACClassDesign.IsDefault</param>
        /// <returns>ACClassDesign if found</returns>
        public ACClassDesign GetDesign(Global.ACKinds kindOfDesign, bool useDefault = true)
        {
            return ACType.Designs.Where(c => (c.ACKindIndex == (short)kindOfDesign) && (useDefault ? c.IsDefault : true)).FirstOrDefault();
        }
        #endregion


        #region Open Dialog/Windows
        /// <summary>Opens a modal dialog with the XAML-Layout of the passed design-name.</summary>
        /// <param name="forObject"></param>
        /// <param name="acClassDesignName"></param>
        /// <param name="acCaption"></param>
        /// <param name="isClosableBSORoot"></param>
        /// <param name="ribbonVisibility"></param>
        /// <param name="closeButtonVisibility"></param>
        public void ShowDialog(IACComponent forObject, string acClassDesignName, string acCaption = "", bool isClosableBSORoot = false,
            Global.ControlModes ribbonVisibility = Global.ControlModes.Hidden, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {

            if (GetGUI() != null)
                GetGUI().ShowDialog(forObject == null ? this : forObject, acClassDesignName, acCaption, isClosableBSORoot, ribbonVisibility, closeButtonVisibility);

            else if (GetGUIMobile() != null)
                GetGUIMobile().ShowDialog(forObject == null ? this : forObject, acClassDesignName, acCaption, isClosableBSORoot, ribbonVisibility, closeButtonVisibility);
        }


        /// <summary>Opens a floating window with the XAML-Layout of the passed design-name.</summary>
        /// <param name="forObject"></param>
        /// <param name="acClassDesignName"></param>
        /// <param name="isClosableBSORoot"></param>
        /// <param name="containerType"></param>
        /// <param name="dockState"></param>
        /// <param name="dockPosition"></param>
        /// <param name="ribbonVisibility"></param>
        /// <param name="dockingManagerName"></param>
        /// <param name="closeButtonVisibility"></param>
        public void ShowWindow(IACComponent forObject, string acClassDesignName, bool isClosableBSORoot, Global.VBDesignContainer containerType, Global.VBDesignDockState dockState,
            Global.VBDesignDockPosition dockPosition, Global.ControlModes ribbonVisibility = Global.ControlModes.Hidden, string dockingManagerName = "",
            Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            IVBGui gui = FindGui("VBDockingManager", dockingManagerName) as IVBGui;

            if (gui == null)
                gui = ((ACComponent)this.ParentACComponent).FindGui("VBDockingManager") as IVBGui;
            if (gui != null)
                gui.ShowWindow(forObject == null ? this : forObject, acClassDesignName, isClosableBSORoot, containerType, dockState, dockPosition, ribbonVisibility, closeButtonVisibility);
        }

        public void CloseWindow(IACComponent forObject, string acClassDesignName)
        {
            IACObject window = FindGui("", "", "*" + acClassDesignName, this.ACIdentifier);
            if (window == null)
                return;

            window.ACUrlCommand(Const.CmdClose);
        }


        /// <summary>
        /// Closes the most top dialog.
        /// </summary>
        [ACMethodInfo("", "en{'Close'}de{'Schließen'}", 9999)]
        public void CloseTopDialog()
        {
            IVBGui gui = FindGui("VBDockingManager", "", "", "", true) as IVBGui;
            if (gui != null)
                gui.CloseTopDialog();
        }

        /// <summary>
        /// Gets a reference to the VBDockingManager which has this instance bound to its DataContext.
        /// </summary>
        /// <returns></returns>
        [ACMethodInfo("", "en{'GUI'}de{'GUI'}", 9999)]
        public IVBGui GetGUI()
        {
            return FindGui("VBDockingManager") as IVBGui;
        }

        /// <summary>
        /// Gets a reference to the VBFrameController which has this instance bound to its DataContext.
        /// </summary>
        /// <returns></returns>
        [ACMethodInfo("", "en{'MobileGUI'}de{'MobileGUI'}", 9999)]
        public IVBGui GetGUIMobile()
        {
            return FindGui("VBFrameController") as IVBGui;
        }

        private IACObject _FindGuiResult = null;
        /// <summary>
        /// Searches for a UI-Control that is bound to instance
        /// </summary>
        /// <param name="filterVBControlClassName">Name of the filter vb control class.</param>
        /// <param name="filterFrameworkElementName">Name of the filter framework element.</param>
        /// <param name="filterVBContent">Content of the filter vb.</param>
        /// <param name="filterACNameOfComponent">The filter ac name of component.</param>
        /// <param name="withDialogStack">if set to <c>true</c> [with dialog stack].</param>
        /// <returns></returns>
        protected IACObject FindGui(string filterVBControlClassName = "",
            string filterFrameworkElementName = "",
            string filterVBContent = "",
            string filterACNameOfComponent = "",
            bool withDialogStack = false)
        {
            bool filterVBControlClassNameSet = !String.IsNullOrEmpty(filterVBControlClassName);
            bool filterFrameworkElementNameSet = !String.IsNullOrEmpty(filterFrameworkElementName);
            bool filterVBContentSet = !String.IsNullOrEmpty(filterVBContent);
            bool filterACNameOfComponentSet = !String.IsNullOrEmpty(filterACNameOfComponent);
            if (!filterVBControlClassNameSet && !filterFrameworkElementNameSet && !filterACNameOfComponentSet && !filterVBContentSet)
                return null;

            IACObject searchedGUI = null;
            _FindGuiResult = null;
            BroadcastToVBControls(Const.CmdFindGUI, "", this, filterVBControlClassName, filterFrameworkElementName, filterVBContent, filterACNameOfComponent, withDialogStack);
            searchedGUI = _FindGuiResult;
            _FindGuiResult = null;
            if (searchedGUI == null && ParentACComponent != null)
            {
                if (searchedGUI == null && ParentACComponent != null)
                {
                    ACComponent parent = ParentACComponent as ACComponent;
                    searchedGUI = parent.FindGui(filterVBControlClassName, filterFrameworkElementName, filterVBContent, filterACNameOfComponent, withDialogStack);

                    //parent._FindGuiResult = null;
                    //parent.BroadcastToVBControls(Const.CmdFindGUI, "", ParentACComponent, filterVBControlClassName, filterFrameworkElementName, filterVBContent, filterACNameOfComponent, withDialogStack);
                    //searchedGUI = parent._FindGuiResult;
                    //parent._FindGuiResult = null;
                }
            }
            return searchedGUI;

        }
        #endregion


        #region Scriptengine
        protected void ACInitScriptEngine()
        {
        }

        public ScriptEngine ScriptEngine
        {
            get
            {
                return ScriptEngine.GetScriptEngine(ACType, IsProxy);
            }
        }
        #endregion


        #region RefCounter
        ACPointReference _ReferenceList = null;
        /// <summary>
        /// A reference-point contains information about objects, that refer to this instance.
        /// In most cases this a Smat-Pointers (ACRef) and WPF-References (Hashcode of a WPF-Object encapsulated in the WPFReferencesToComp-class)
        /// </summary>
        /// <value>A list of references of type IACObject</value>
        [ACPropertyReferenceListPoint(9999, "Reference")]
        public IACPointReference<IACObject> ReferencePoint
        {
            get
            {
                return _ReferenceList;
            }
        }

        /// <summary>
        /// Called when a reference was added to the ReferencePoint
        /// </summary>
        /// <param name="acObject">The reference as IACObject</param>
        [ACMethodInfo("ACComponent", "en{'OnAddReference'}de{'OnAddReference'}", 9999)]
        public virtual void OnAddReference(IACObject acObject)
        {
        }

        /// <summary>
        /// Called when a reference was removed from the ReferencePoint
        /// </summary>
        /// <param name="acObject">The reference as IACObject</param>
        [ACMethodInfo("ACComponent", "en{'OnRemoveReference'}de{'OnRemoveReference'}", 9999)]
        public virtual void OnRemoveReference(IACObject acObject)
        {
        }

        #endregion


        #region Property-Changed
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the INotifyPropertyChanged.PropertyChanged-Event.
        /// </summary>
        /// <param name="name">Name of the property</param>
        [ACMethodInfo("ACComponent", "en{'PropertyChanged'}de{'PropertyChanged'}", 9999)]
        public virtual void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


#if DEBUG
        protected void PrintPropChangedInvocationList(bool fullPrint)
        {
            if (PropertyChanged != null)
            {
                if (fullPrint)
                {
                    foreach (Delegate delegatex in GetPropChangedInvocationList())
                    {
                        IACObject acobject = delegatex.Target as IACObject;
                        if (acobject != null)
                        {
                            try
                            {
                                if (acobject is IVBContent)
                                    System.Diagnostics.Debug.WriteLine(acobject.GetType().Name + ";" + (acobject as IVBContent).VBContent + ";" + acobject.GetHashCode().ToString() + ";");
                                else
                                    System.Diagnostics.Debug.WriteLine(acobject.GetType().Name + ";" + acobject.ACIdentifier + ";" + acobject.GetHashCode().ToString() + ";");
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine(acobject.GetType().Name + ";null;" + acobject.GetHashCode().ToString() + ";");
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                Messages.LogException("ACComponent", "PrintPropChangedInvocationList", msg);
                            }
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Count PropertyChanged: " + PropertyChanged.GetInvocationList().Count().ToString());
                }
            }
        }

        protected Delegate[] GetPropChangedInvocationList()
        {
            if (PropertyChanged == null)
                return null;
            return PropertyChanged.GetInvocationList();
        }
#endif

        public void OnIsEnabledChanged(string methodName)
        {
            BroadcastToVBControls(Const.CmdInvalidateRequerySuggested, methodName);
        }

        public void BroadcastToVBControls(string acUrl, string targetVBContent, params Object[] acParameter)
        {
            if (String.IsNullOrEmpty(acUrl))
                return;
            this.ACUrlCmdMessage = new ACUrlCmdMessage() { ACUrl = acUrl, TargetVBContent = targetVBContent, ACParameter = acParameter };
        }

        #endregion


        #region IComparable Member

        public int CompareTo(object obj)
        {
            if (obj == null)
                return -1;
            else if (!(obj is ACComponent))
                return -1;
            if (this.GetACUrl() == (obj as ACComponent).GetACUrl())
                return 0;
            return -1;
        }

        #endregion


        #region DropData
        /// <summary>
        /// Called from a WPF-Control inside it's ACAction-Method when a relevant interaction-event as occured (e.g. Drag-And-Drop).
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source.</param>
        public virtual void ACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    ACCommand acCommand = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand).FirstOrDefault() as ACCommand;
                    if (acCommand != null)
                    {
                        if (!acCommand.ParameterList.Any())
                        {
                            targetVBDataObject.ContextACObject.ACUrlCommand(acCommand.GetACUrl());
                            return;
                        }
                        else
                        {
                            targetVBDataObject.ContextACObject.ACUrlCommand(acCommand.GetACUrl(), acCommand.ParameterList);
                            return;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Called from a WPF-Control when a relevant interaction-event as occured (e.g. Drag-And-Drop) and the related component should check if this interaction-event should be handled.
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public virtual bool IsEnabledACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            if (targetVBDataObject == null)
                return false;
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    ACCommand acCommand = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand).FirstOrDefault() as ACCommand;
                    if (acCommand != null)
                    {
                        if (!acCommand.ParameterList.Any())
                        {
                            return targetVBDataObject.ContextACObject.IsEnabledACUrlCommand(acCommand.GetACUrl());
                        }
                        else
                        {
                            return targetVBDataObject.ContextACObject.IsEnabledACUrlCommand(acCommand.GetACUrl(), acCommand.ParameterList);
                        }
                    }
                    return false;
                default:
                    return false;
            }
        }
        #endregion


        #region IACInteractiveObject Member
        /// <summary>UNUSED IN ACCOMPONENT. RETURNS NULL.</summary>
        /// <value>Relative or absolute ACUrl</value>
        public string VBContent
        {
            get
            {
                return null;
            }
        }

        /// <summary>UNUSED IN ACCOMPONENT. RETURNS this.</summary>
        /// <value>this</value>
        public IACObject ContextACObject
        {
            get
            {
                return this;
            }
        }

        protected IACObject _Content;
        [ACPropertyInfo(9999, "Content", "en{'Content'}de{'Content'}")]
        public virtual IACObject Content
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _Content;
                }
            }
            set
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _Content = value;
                }
                OnPropertyChanged("Content");
            }
        }

        /// <summary>
        /// Entity which represents the persisted object-state of a ACComponent-Instance
        /// </summary>
        [ACPropertyInfo(9999, "Content", "en{'ContentTask'}de{'ContentTask'}")]
        public ACClassTask ContentTask
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _Content as ACClassTask;
                }
            }
        }

        protected ACCommand _CurrentInvokingACCommand = null;
        public ACCommand CurrentInvokingACCommand
        {
            get
            {
                return _CurrentInvokingACCommand;
            }
        }

        /// <summary>Occurs when ACAction(ACActionArgs actionArgs) is invoked.</summary>
        public event ACActionEventHandler ACActionEvent;

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public virtual void ACAction(ACActionArgs actionArgs)
        {
            ACActionEvent?.Invoke(this, actionArgs);

            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ContextMenu:
                    {
                        if (actionArgs is ACActionMenuArgs)
                        {
                            ACActionMenuArgs acActionMenuArgs = actionArgs as ACActionMenuArgs;
                            acActionMenuArgs.ACMenuItemList = MenuManager.GetMenu(this, actionArgs.DropObject);
                        }
                        return;
                    }
                case Global.ElementActionType.ACCommand:
                    {
                        ACCommand acCommand = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand).FirstOrDefault() as ACCommand;
                        if (acCommand != null)
                        {
                            if (Root != null)
                                Messages.LogDebug(this.GetACUrl(), "ACAction()", acCommand.GetACUrl());
                            if (acCommand.GetACUrl().StartsWith(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start))
                            {
                                _CurrentInvokingACCommand = acCommand;
                                Root.ACAction(actionArgs);
                                _CurrentInvokingACCommand = null;
                                return;
                            }
                            if (!acCommand.ParameterList.Any())
                            {
                                _CurrentInvokingACCommand = acCommand;
                                ACUrlCommand(acCommand.GetACUrl());
                                _CurrentInvokingACCommand = null;
                                return;
                            }
                            else
                            {
                                _CurrentInvokingACCommand = acCommand;
                                ACUrlCommand(acCommand.GetACUrl(), acCommand.ParameterList.ToValueArray());
                                _CurrentInvokingACCommand = null;
                                return;
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public virtual bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    {
                        ACCommand acCommand = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand).FirstOrDefault() as ACCommand;
                        if (acCommand != null)
                        {
                            if (!acCommand.ParameterList.Any())
                            {
                                return IsEnabledACUrlCommand(acCommand.GetACUrl());
                            }
                            else
                            {
                                return IsEnabledACUrlCommand(acCommand.GetACUrl(), acCommand.ParameterList.ToValueArray());
                            }
                        }
                    }
                    break;
            }
            return false;
        }


        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                if (ComponentClass == null)
                    return null;
                var query = ComponentClass.Properties.Where(c => c.IsContent);
                if (!query.Any())
                {
                    return new IACObject[] { ACType };
                }
                List<IACObject> acContentList = new List<IACObject>();
                foreach (var acTypeInfo in query.OrderBy(c => c.SortIndex))
                {
                    IACObject acObject = this.ACUrlCommand(acTypeInfo.ACIdentifier) as IACObject;
                    if (acObject != null)
                        acContentList.Add(acObject);
                }

                return acContentList;
            }
        }
        #endregion


        #region IACMenuBuilder Member

        private ConcurrentDictionary<Guid, ClassRightManager> _RemoteUserRights = null;
        protected ConcurrentDictionary<Guid, ClassRightManager> RemoteUserRights
        {
            get
            {
                if (_RemoteUserRights == null)
                    _RemoteUserRights = new ConcurrentDictionary<Guid, ClassRightManager>();
                return _RemoteUserRights;
            }
        }

        protected ClassRightManager CurrentRightsOfInvoker
        {
            get
            {
                if (this.Root.CurrentInvokingUser != null)
                {
                    ClassRightManager classRightManager = GetRightsForUser(Root.CurrentInvokingUser);
                    return classRightManager;
                }
                else
                {
                    return ComponentClass.RightManager;
                }
            }
        }

        public ClassRightManager GetRightsForUser(VBUser user)
        {
            if (user == null)
                return null;
            ClassRightManager classRightManager;
            if (!RemoteUserRights.TryGetValue(user.VBUserID, out classRightManager))
            {
                classRightManager = new ClassRightManager(ComponentClass, user);
                RemoteUserRights.TryAdd(user.VBUserID, classRightManager);
            }
            return classRightManager;
        }

        /// <summary>
        /// A ACMenuItem contains a ACUrl of the Method that should be invoked.
        /// GetMenu() is called from gip.core.autocomponent.MenuManager.GetMenu()-Method.
        /// The MenuManager calls GetMenu() at all instances that implement IACMenuBuilder and which have a relationship inside the MVVM-Pattern.
        /// All ACMenuItemList's are afterwards merged together to one menu that is displayed as a contextmenu on the GUI.
        /// </summary>
        /// <param name="vbContent">VBContent of the WPF-Control where the user has requested the menu first</param>
        /// <param name="vbControl">Type.FullName of the WPF-Control where the user has requested the menu first</param>
        /// <returns>List of menu entries</returns>
        [ACMethodInfo("", "", 9999)]
        public virtual ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();

            if (string.IsNullOrEmpty(vbContent))
            {
                var methods = ACClassMethods.Where(c => c.IsInteraction && string.IsNullOrEmpty(c.InteractionVBContent)).OrderBy(c => c.SortIndex).ThenBy(c => c.ACCaption);

                foreach (var method in methods)
                {
                    if (IsEnabledExecuteACMethod(method.ACIdentifier, null))
                    {
                        Global.ControlModes controlMode = CurrentRightsOfInvoker.GetControlMode(method);
                        if (controlMode == Global.ControlModes.Enabled)
                        {
                            acMenuItemList = ACMenuItem.CreateParentACMenuItem(method, acMenuItemList);
                            ACMenuItem parentItem = acMenuItemList.FirstOrDefault(c => c.ACUrl == method.ContextMenuCategoryIndex.ToString());

                            string parentItemACUrl = null;
                            if (parentItem != null)
                                parentItemACUrl = parentItem.ACUrl;

                            ACMenuItem item;
                            if (this.Root.CurrentInvokingUser != null)
                                item = new ACMenuItem(method.GetTranslation(this.Root.CurrentInvokingUser.VBLanguage.VBLanguageCode), "!" + method.ACIdentifier, method.SortIndex, null, parentItemACUrl, false, this);
                            else
                                item = new ACMenuItem(method.ACCaption, "!" + method.ACIdentifier, method.SortIndex, null, parentItemACUrl, false, this);

                            item.IconACUrl = method.GetIconACUrl();
                            if (OnNewMenuItemCreated(method.ACIdentifier, item))
                                acMenuItemList.Add(item);
                        }
                    }
                }
            }
            else
            {
                var methods = ACClassMethods.Where(c => c.IsInteraction && (string.IsNullOrEmpty(c.InteractionVBContent) || c.InteractionVBContent == vbContent)).OrderBy(c => c.SortIndex).ThenBy(c => c.ACCaption);

                foreach (var method in methods)
                {
                    if (IsEnabledExecuteACMethod(method.ACIdentifier, null))
                    {
                        Global.ControlModes controlMode = CurrentRightsOfInvoker.GetControlMode(method);
                        if (controlMode == Global.ControlModes.Enabled)
                        {
                            acMenuItemList = ACMenuItem.CreateParentACMenuItem(method, acMenuItemList);
                            ACMenuItem parentItem = acMenuItemList.FirstOrDefault(c => c.ACUrl == method.ContextMenuCategoryIndex.ToString());
                            string parentItemACUrl = null;
                            if (parentItem != null)
                                parentItemACUrl = parentItem.ACUrl;

                            ACMenuItem item;
                            if (this.Root.CurrentInvokingUser != null)
                                item = new ACMenuItem(method.GetTranslation(this.Root.CurrentInvokingUser.VBLanguage.VBLanguageCode), "!" + method.ACIdentifier, method.SortIndex, null, parentItemACUrl, false, this);
                            else
                                item = new ACMenuItem(method.ACCaption, "!" + method.ACIdentifier, method.SortIndex, null, parentItemACUrl, false, this);

                            item.IconACUrl = method.GetIconACUrl();
                            if (OnNewMenuItemCreated(method.ACIdentifier, item))
                                acMenuItemList.Add(item);
                        }
                    }
                }
            }
            return acMenuItemList;
        }

        public virtual bool OnNewMenuItemCreated(string acMethodName, ACMenuItem menuItem)
        {
            if (acMethodName == "DiagnosticdialogOn")
            {
                menuItem.ACCaption = "Show " + ACIdentifier + " in " + menuItem.ACCaption;
            }
            return true;
        }

        #endregion


        #region Savehandling
        /// <summary>ACUndoChanges reverts all changes on the context (Database-Property)</summary>
        /// <returns>True if successfull. Otherwise a Message-Dialog appears.</returns>
        public bool ACUndoChanges()
        {
            if (Database == null)
                return false;
            Msg msg = Database.ACUndoChanges();
            if (msg != null)
            {
                Messages.Msg(msg);
            }
            return msg == null;
        }

        public event EventHandler ACSaveChangesExecuted;
        /// <summary>Commits all changes made on the context (Database-Property) and raises the event ACSaveChangesExecuted.</summary>
        /// <returns>True if successfull. Otherwise a Message-Dialog appears.</returns>
        public bool ACSaveChanges()
        {
            if (Database == null)
                return false;
            if (ACSaveChangesExecuted != null)
                ACSaveChangesExecuted(this, new EventArgs());
            Msg msg = Database.ACSaveChanges();
            if (msg != null)
            {
                Messages.Msg(msg);
            }
            return msg == null;
        }
        #endregion


        #region Configuration

        public readonly ACMonitorObject _20015_LockValue = new ACMonitorObject(20015);
        private ACClass _ACTypeFromLiveContext;
        public ACClass ACTypeFromLiveContext
        {
            get
            {
                ACClass acClass = ComponentClass;
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_ACTypeFromLiveContext != null)
                        return _ACTypeFromLiveContext;
                }
                if (InitState == ACInitState.Destructed || InitState == ACInitState.Destructing || acClass == null)
                    return null;

                ACClass typeFromLiveContext = null;
                try
                {
                    typeFromLiveContext = ACClassTaskQueue.TaskQueue.Context.GetACType(acClass.ACClassID);
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "ACTypeFromLiveContext.get", e.Message);
                    if (e.InnerException != null)
                        Messages.LogException(this.GetACUrl(), "ACTypeFromLiveContext.get", e.InnerException.Message);
                }
                //ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                //{
                //    try
                //    {
                //        typeFromLiveContext = ACClassTaskQueue.TaskQueue.Context.ACClass.Where(c => c.ACClassID == acClass.ACClassID).FirstOrDefault();
                //    }
                //    catch (Exception e)
                //    {
                //        Messages.LogException(this.GetACUrl(), "ACTypeFromLiveContext.get", e.Message);
                //        if (e.InnerException != null)
                //            Messages.LogException(this.GetACUrl(), "ACTypeFromLiveContext.get", e.InnerException.Message);
                //    }
                //});

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_ACTypeFromLiveContext == null && typeFromLiveContext != null)
                        _ACTypeFromLiveContext = typeFromLiveContext;
                }

                return _ACTypeFromLiveContext;
            }
        }

        /// <summary>
        /// Indexer for accessing Configuration-Values.
        /// It reads and writes the ComponentClass.ConfigurationEntries where KeyACUrl is null.
        /// Please keep in mind that the ComponentClass.ConfigurationEntries property is cached and updates that occur on another database context are not visible. 
        /// Use the ResetConfigValuesCache() method to reset the cache to reread the configuration values from the database.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public object this[string configuration]
        {
            get
            {
                try
                {
                    IACConfig acClassConfig = GetConfigurationValue(configuration);
                    if (acClassConfig != null)
                        return acClassConfig.Value;
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "this[].get", e.Message);
                    if (e.InnerException != null)
                        Messages.LogException(this.GetACUrl(), "this[].get", e.InnerException.Message);
                }
                return null;
            }
            set
            {
                SetConfigurationValue(configuration, value);
            }
        }

        public IACConfig GetConfigurationValue(string configuration)
        {
            ACClass acTypeFromLiveContext = ACTypeFromLiveContext;
            if (acTypeFromLiveContext == null)
                return null;
            try
            {
                return acTypeFromLiveContext.ConfigurationEntries.Where(c => c.KeyACUrl == acTypeFromLiveContext.ACConfigKeyACUrl && c.LocalConfigACUrl == configuration).FirstOrDefault();
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "GetConfigurationValue()", e.Message);
                if (e.InnerException != null)
                    Messages.LogException(this.GetACUrl(), "GetConfigurationValue()", e.InnerException.Message);
            }
            //});
            return null;
        }

        internal void SetConfigurationValue(string configuration, object value, Type forceType = null)
        {
            ACClass acTypeFromLiveContext = ACTypeFromLiveContext;
            if (acTypeFromLiveContext == null)
                return;
            //ACClassTaskQueue.TaskQueue.Add(() =>
            //{
                IACConfig acClassConfig = acTypeFromLiveContext.ConfigurationEntries.Where(c => c.KeyACUrl == acTypeFromLiveContext.ACConfigKeyACUrl && c.LocalConfigACUrl == configuration).FirstOrDefault();
            if (acClassConfig == null)
            {
                if (value != null || forceType != null)
                {
                    ACClassConfig baseACClassConfig = null;
                    var queryBaseClasses = acTypeFromLiveContext.ClassHierarchyWithInterfaces;
                    if (queryBaseClasses != null)
                    {
                        foreach (var baseClass in queryBaseClasses)
                        {
                            baseACClassConfig = baseClass.ConfigurationEntries.Where(c => c.KeyACUrl == acTypeFromLiveContext.ACConfigKeyACUrl
                                                                                        && c.LocalConfigACUrl == configuration
                                                                                        && !String.IsNullOrEmpty(c.Comment)).FirstOrDefault() as ACClassConfig;
                            //baseACClassConfig = baseClass.ACClassConfig_ACClass.Where(c => c.KeyACUrl == acTypeFromLiveContext.ACConfigKeyACUrl
                            //                                                            && c.LocalConfigACUrl == configuration
                            //                                                            && !String.IsNullOrEmpty(c.Comment)).FirstOrDefault();
                            if (baseACClassConfig != null)
                                break;
                        }
                    }
                    //ACClassTaskQueue.TaskQueue.Add(() =>
                    ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        acClassConfig = acTypeFromLiveContext.NewACConfig(null, ACClassTaskQueue.TaskQueue.Context.GetACType(value != null ? value.GetType() : forceType)) as ACClassConfig;
                        acClassConfig.LocalConfigACUrl = configuration;
                        if (baseACClassConfig != null)
                            acClassConfig.Comment = baseACClassConfig.Comment;
                        acClassConfig.Value = value;
                    });
                    // Call Add for exceuting Save-Changes afterwards
                    ACClassTaskQueue.TaskQueue.Add(() =>
                    {
                        _ = acClassConfig?.VBiACClassID;
                    });
                }
            }
            else
            {
                ACClassTaskQueue.TaskQueue.Add(() =>
                {
                    acClassConfig.Value = value;
                });
            }
            //});
        }

        /// <summary>
        /// Resets the Configuration-Cache: ComponentClass.ConfigurationEntries (IACConfigStore).
        /// This also resets all configuration properties and reloads them from the database (ACPropertyConfigValue&lt;T&gt;).
        /// This method is recursively. It resets also all childs in the application tree.
        /// </summary>
        [ACMethodCommand("ACComponent", "en{'Reset configuration cache'}de{'Resettiere Konfigurationspuffer'}", 1000)]
        public virtual void ResetConfigValuesCache()
        {
            ACClass acTypeFromLiveContext = ACTypeFromLiveContext;
            if (acTypeFromLiveContext == null)
                return;
            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (_ACPropertyConfigValueList != null)
                {
                    _ACPropertyConfigValueList.ForEach(c => c.IsCachedValueSet = false);
                }
            }

            acTypeFromLiveContext.ClearCacheOfConfigurationEntries();

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (_ACPropertyConfigValueList != null)
                    _ACPropertyConfigValueList.ForEach(c => c.IsCachedValueSet = false);
            }

            foreach (ACComponent child in this.ACComponentChilds)
            {
                child.ResetConfigValuesCache();
            }
        }

        //public virtual bool IsEnabledResetConfigValuesCache()
        //{
        //    if (Root != null)
        //    {
        //        if (Root.CurrentInvokingUser != null)
        //            return Root.CurrentInvokingUser.IsSuperuser;
        //        else if (Root.Environment.User != null)
        //            return Root.Environment.User.IsSuperuser;
        //    }
        //    return false;
        //}

        [ACMethodCommand("ACComponent", "en{'Remove duplicate configurations'}de{'Entferne doppelte Konfigurationseinträge'}", 1001, true)]
        public virtual void RemoveDuplicateConfigEntries()
        {
            RemoveDuplicateConfigEntriesInternal();
        }

        protected virtual void RemoveDuplicateConfigEntriesInternal(int depth = 0)
        {
            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (_ACPropertyConfigValueList != null)
                {
                    _ACPropertyConfigValueList.ForEach(c => c.RemoveDuplicateEntries());
                }
            }
            foreach (ACComponent child in this.ACComponentChilds)
            {
                child.RemoveDuplicateConfigEntriesInternal(depth+1);
            }

            if (depth == 0)
            {
                ResetConfigValuesCache();
                ACClassTaskQueue.TaskQueue.Add(() =>
                {
                    // Force SaveChanges
                    _ = new object();
                });

            }
        }
        #endregion


        #region Diagnostics and Dump
        [ACPropertyInfo(9999)]
        public virtual string ACDiagnoseInfo
        {
            get
            {
                return DumpAsXMLString(2);
            }
        }

        [ACPropertyInfo(9999)]
        public virtual XmlDocument ACDiagnoseXMLDoc
        {
            get
            {
                return DumpAsXMLDoc(2);
            }
        }


        /// <summary>
        /// Dumps the state of this instance and returns the result as a xml-string.
        /// </summary>
        /// <param name="maxChildDepth">The maximum child depth.</param>
        /// <returns></returns>
        [ACMethodInfo("ACComponent", "en{'DumpAsXMLString'}de{'DumpAsXMLString'}", 9999)]
        public string DumpAsXMLString(int maxChildDepth = 0)
        {
            XmlDocument doc = DumpAsXMLDoc(maxChildDepth);
            if (doc == null)
                return "";
            string xmlText = "";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter xmlWriter = XmlTextWriter.Create(stringWriter, settings))
            {
                doc.Save(xmlWriter);
                xmlText = stringWriter.ToString();
            }
            return xmlText;
        }


        /// <summary>
        /// Deserializes a dump-xml-String to XmlDocument
        /// </summary>
        /// <param name="dumpXML">The dump XML.</param>
        /// <returns></returns>
        public XmlDocument DumpFromXMLString(string dumpXML)
        {
            XmlDocument doc = new XmlDocument();
            using (StringReader stringReader = new StringReader(dumpXML))
            using (XmlReader xmlReader = new XmlTextReader(stringReader))
            {
                xmlReader.Read();
                doc.Load(xmlReader);
            }
            return doc;
        }


        /// <summary>
        /// Dumps the state of this instance (Properties, Points) and returns a XmlDocument
        /// </summary>
        /// <param name="maxChildDepth">The maximum child depth.</param>
        /// <returns>XmlDocument</returns>
        [ACMethodInfo("ACComponent", "en{'DumpAsXMLDoc'}de{'DumpAsXMLDoc'}", 9999)]
        public virtual XmlDocument DumpAsXMLDoc(int maxChildDepth = 0)
        {
            XmlDocument doc = new XmlDocument();
            DumpStats dumpStats = new DumpStats();
            DumpCreateXMLElement(doc, null, 0, maxChildDepth, ref dumpStats);
            return doc;
        }

        protected class DumpStats
        {
            public int Bindings { get; set; }
        }

        protected virtual void DumpCreateXMLElement(XmlDocument doc, XmlElement parent, int currentDepth, int maxChildDepth, ref DumpStats dumpStats)
        {
            currentDepth++;
            if ((maxChildDepth > 0) && (currentDepth > maxChildDepth))
                return;

            XmlElement xmlThis = doc.CreateElement(GetType().Name);
            xmlThis.SetAttribute("Type", GetType().FullName);
            xmlThis.SetAttribute(Const.ACUrlPrefix, GetACUrl());

            XmlElement xmlACComponentChilds = doc.CreateElement("ACComponentChilds");
            foreach (ACComponent subComponent in ACComponentChilds)
            {
                subComponent.DumpCreateXMLElement(doc, xmlACComponentChilds, currentDepth, maxChildDepth, ref dumpStats);
            }
            xmlThis.AppendChild(xmlACComponentChilds);

            XmlElement xmlACPropertyList = doc.CreateElement("ACPropertyList");
            DumpPropertyList(doc, xmlACPropertyList, ref dumpStats);
            xmlThis.AppendChild(xmlACPropertyList);

            XmlElement xmlACPointList = doc.CreateElement("ACPointList");
            DumpPointList(doc, xmlACPointList);
            xmlThis.AppendChild(xmlACPointList);

            if (currentDepth == 1)
            {
                XmlElement xmlChild = xmlACPropertyList["BindingCount"];
                if (xmlChild == null)
                {
                    xmlChild = doc.CreateElement("BindingCount");
                    if (xmlChild != null)
                        xmlChild.InnerText = dumpStats.Bindings.ToString();
                    xmlACPropertyList.AppendChild(xmlChild);
                }
            }

            if (parent != null)
                parent.AppendChild(xmlThis);
            else
                doc.AppendChild(xmlThis);
        }

        /// <summary>
        /// Opens the Diagnostic Dialog (Only possible on GUI)
        /// </summary>
        [ACMethodInteraction("", "en{'Diagnosticdialog'}de{'Diagnose-dialog'}", (short)MISort.DiagnosticdialogOn, false, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.Utilities)]
        public virtual void DiagnosticdialogOn()
        {
            if (_CurrentInvokingACCommand == null)
                return;

            IACBSO bso = this as IACBSO;
            if (bso == null && _CurrentInvokingACCommand.BSO != null)
                bso = _CurrentInvokingACCommand.BSO;
            if (bso == null)
                return;
            VBBSODiagnosticDialog dialog = bso.FindChildComponents<VBBSODiagnosticDialog>(c => c is VBBSODiagnosticDialog).FirstOrDefault();
            if (dialog == null)
                dialog = bso.StartComponent(VBBSODiagnosticDialogACIdentifier, null, null) as VBBSODiagnosticDialog;
            if (dialog != null)
                dialog.ShowDialogForComponent(this);
        }

        public virtual string VBBSODiagnosticDialogACIdentifier
        {
            get
            {
                return "VBBSODiagnosticDialog(CurrentDesign)";
            }
        }

        public virtual bool IsEnabledDiagnosticdialogOn()
        {
            if (ACType.GetDesign(this, Global.ACUsages.DUDiagnostic, Global.ACKinds.DSDesignLayout) != null)
                return true;
            return false;
        }

        /// <summary>
        /// Opens the ACComponent explorer.
        /// </summary>
        [ACMethodInteraction("", "en{'Components Explorer'}de{'Komponenten Explorer'}", (short)MISort.ComponentExplorer, false, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.Utilities)]
        public void ACComponentExplorerOn()
        {
            IACBSO bso = this as IACBSO;
            if (bso == null)
            {
                bso = _CurrentInvokingACCommand.BSO;
                if (bso == null)
                    return;
            }

            VBBSOACComponentExplorer explorer = bso.FindChildComponents<VBBSOACComponentExplorer>().FirstOrDefault();
            if (explorer == null)
                explorer = bso.StartComponent(VBBSOACComponentExplorerACIdentifier, null, null) as VBBSOACComponentExplorer;
            if (explorer != null)
                explorer.ShowSelectionDialog(this);
        }

        /// <summary>
        /// Determines is enabled to open ACComponent explorer.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledACComponentExplorerOn()
        {
            return true;
        }

        public virtual string VBBSOACComponentExplorerACIdentifier
        {
            get => "VBBSOACComponentExplorer(CurrentDesign)";
        }

        #endregion


        #region Documentation

        [ACMethodInteraction("BrowseOnlineHelp", "en{'Browse online help'}de{'Durchsuchen Sie die Online-Hilfe'}", (short)MISort.DiagnosticdialogOn, false, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.NoCategory)]
        public virtual void BrowseOnlineHelp()
        {
            if (_CurrentInvokingACCommand == null && !IsEnabledOpenHelp())
                return;

            IACBSO bso = this as IACBSO;
            if (bso == null && _CurrentInvokingACCommand.BSO != null)
                bso = _CurrentInvokingACCommand.BSO;
            if (bso == null)
                return;
            ACRoot vb = this.Root as ACRoot;
            if (bso.ACType is ACClass)
            {
                string fullURL = vb.IPlusDocsServerURL;
                fullURL += @"?filter.WorkspaceURLs={urls}";
                fullURL = fullURL.Replace("{lang}", vb.CurrentInvokingUser.VBLanguage.VBLanguageCode);
                //fullURL = fullURL.Replace("{ACClassID}", bso.ComponentClass.ACClassID.ToString());
                string acclassURL = bso.GetACUrl();
                fullURL = fullURL.Replace("{urls}", acclassURL);
                System.Diagnostics.Process.Start(fullURL);
            }
        }

        [ACMethodInteraction("OpenLocalHelp", "en{'Local Help'}de{'Lokale Hilfe'}", (short)MISort.DiagnosticdialogOn, false, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.NoCategory)]
        public virtual void OpenLocalHelp()
        {
            if (_CurrentInvokingACCommand == null && !IsEnabledOpenHelp())
                return;

            IACBSO bso = this as IACBSO;
            if (bso == null && _CurrentInvokingACCommand.BSO != null)
                bso = _CurrentInvokingACCommand.BSO;
            if (bso == null)
                return;
            ACRoot vb = this.Root as ACRoot;
            if (bso.ACType is ACClass)
            {
                string fullURL = vb.IPlusDocsServerURL;
                fullURL = fullURL.Replace("{lang}", vb.CurrentInvokingUser.VBLanguage.VBLanguageCode);
                fullURL = fullURL.Replace("{ACClassID}", bso.ComponentClass.ACClassID.ToString());
                System.Diagnostics.Process.Start(fullURL);
            }
        }

        public virtual bool IsEnabledOpenHelp()
        {
            return this.Root != null && !String.IsNullOrEmpty(this.Root.IPlusDocsServerURL);
        }
        #endregion


        #region Printing

        /// <summary>Prints the state of the component to a XPSDocument.</summary>
        [ACMethodInteraction("", "en{'Print'}de{'Drucken'}", (short)MISort.PrintSelf, false, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.Utilities)]
        public virtual void PrintSelf()
        {
            ACClassDesign acClassDesign = this.ComponentClass.Designs.Where(c => c.ACKindIndex == (short)Global.ACKinds.DSDesignReport && c.IsDefault).FirstOrDefault();
            if (acClassDesign == null)
            {
                acClassDesign = this.ComponentClass.Designs.Where(c => c.ACKindIndex == (short)Global.ACKinds.DSDesignReport).FirstOrDefault();
                if (acClassDesign == null)
                    return;
            }

            PrintDesign(acClassDesign, "", 1, false);
        }

        public virtual bool IsEnabledPrintSelf()
        {
            ACClassDesign acClassDesign = this.ComponentClass.Designs.Where(c => c.ACKindIndex == (short)Global.ACKinds.DSDesignReport && c.IsDefault).FirstOrDefault();
            if (acClassDesign == null)
            {
                acClassDesign = this.ComponentClass.Designs.Where(c => c.ACKindIndex == (short)Global.ACKinds.DSDesignReport).FirstOrDefault();
                if (acClassDesign == null)
                    return false;
            }

            //ACClass acClassReport = GetACClassFromACClassName("VBBSOReport");
            //if (acClassReport == null)
            //    return false;

            return true;
        }

        /// <summary>Called more times from the report engine (ReportData) during the generation of a XPSDocument for printing.</summary>
        /// <param name="reportEngine">The report engine.</param>
        /// <param name="printingPhase">The printing phase.</param>
        public virtual void OnPrintingPhase(object reportEngine, ACPrintingPhase printingPhase)
        {
        }

        /// <summary>
        /// Prints the given design to a XPSDocument.
        /// </summary>
        /// <param name="design">The design to print.</param>
        /// <param name="printerName">The name of printer.</param>
        /// <param name="numberOfCopies">Number of print copies.</param>
        /// <param name="withDialog">Print with a dialog or without dialog.</param>
        /// <param name="selectMode">Selection mode current or list.</param>
        /// <param name="queryDefinition">Query definiton for a additional query.</param>
        /// <param name="maxPrintJobsInSpooler">Max Print Jobs in Queue</param>
        /// <param name="preventClone">Prevent generating a clone for the BSO</param>
        /// <returns></returns>
        public virtual Msg PrintDesign(ACClassDesign design, string printerName, int numberOfCopies, bool withDialog, Global.CurrentOrList selectMode = Global.CurrentOrList.Current, 
                                       ACQueryDefinition queryDefinition = null, int maxPrintJobsInSpooler = 0, bool preventClone = false)
        {
            if (design == null)
                return new Msg("The parameter design is null!", this, eMsgLevel.Error, nameof(ACComponent), nameof(PrintDesign) + "(10)", 4622);

            try
            {
                string acClassName = "VBBSOReport";
                gip.core.datamodel.ACClass acClass = GetACClassFromACClassName(acClassName);
                if (acClass == null)
                    return new Msg("The parameter design is null!", this, eMsgLevel.Error, nameof(ACComponent), nameof(PrintDesign) + "(20)", 4639);

                IReportHandler acReportComp = StartComponent(acClass, acClass, null, Global.ACStartTypes.Automatic) as IReportHandler;

                if (acReportComp != null)
                {
                    bool cloneInstantiated = false;

                    //var vbDump = Root.VBDump;
                    //PerformanceEvent pEvent = vbDump?.PerfLoggerStart(this.GetACUrl() + "!" + nameof(ReportData.BuildReportData), 150);
                    ReportData reportData = ReportData.BuildReportData(out cloneInstantiated, selectMode, this, queryDefinition, design, preventClone);
                    //vbDump?.PerfLoggerStop(this.GetACUrl() + "!" + nameof(ReportData.BuildReportData), 150, pEvent);
                    Msg msg = null;

                    if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                    {
                        using (EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset))
                        {
                            ACDispatchedDelegateQueue.PrintQueue.Add(() =>
                            {
                                try
                                {
                                    msg = acReportComp.Print(design, withDialog, printerName, reportData, numberOfCopies, maxPrintJobsInSpooler);
                                }
                                catch (Exception ex2)
                                {
                                    string message = ex2.Message;
                                    if (ex2.InnerException != null)
                                        message += ex2.InnerException.Message;
                                    msg = new Msg(ex2.Message, this, eMsgLevel.Exception, this.GetType().AssemblyQualifiedName, "PrintDesign(40)", 40);
                                }
                                finally
                                {
                                    waitHandle.Set();
                                }
                            });
                            waitHandle.WaitOne();
                        }
                    }
                    else
                    {
                        // do work here when calling thread is STA
                        // this removes the overhead of creating
                        // a new thread when it is not necessary

                        //pEvent = vbDump?.PerfLoggerStart(this.GetACUrl() + "!" + nameof(acReportComp.Print), 160);
                        msg = acReportComp.Print(design, withDialog, printerName, reportData, numberOfCopies, maxPrintJobsInSpooler);
                        //vbDump?.PerfLoggerStop(this.GetACUrl() + "!" + nameof(acReportComp.Print), 160, pEvent);
                    }

                    if (cloneInstantiated)
                        reportData.StopACComponents();

                    StopComponent(acReportComp);

                    if (msg != null)
                        return msg;
                }
            }
            catch(Exception e)
            {
                Messages.LogException(this.GetACUrl(), "ACComponent.PrintDesign()", e.Message);
                return new Msg(e.Message, this, eMsgLevel.Error, nameof(ACComponent), nameof(PrintDesign) + "(30)", 4676);
            }
            return null;
        }

        #endregion
    }

    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Invocation mode for asynchronous methods'}de{'Aufrufsmodus für asynchrone Methoden'}", Global.ACKinds.TACEnum)]
    public enum AsyncMethodInvocationMode : short
    {
        /// <summary>
        /// Invokes a asnychronous method synchronous and doesn't wait for completion of ansynchronous process
        /// </summary>
        SyncNoCallback = 0,
        /// <summary>
        /// Invokes a asnychronous method from a RMI-Point. Callback is done asynchronous over Callback in RMI-Point-Entry.
        /// </summary>
        Asynchronous = 1,
        /// <summary>
        /// Invoke a asnychronous method synchronous and wait till ansynchronous process is completed (uses WaitHandle)
        /// </summary>
        Synchronous = 2,
    }

}

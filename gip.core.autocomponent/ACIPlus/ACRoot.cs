using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using gip.core.datamodel;
using System.Data.SqlClient;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Anwendungssteuerungszentrale
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Root of iPlus'}de{'Root of iPlus'}", gip.core.datamodel.Global.ACKinds.TACRoot, Global.ACStorableTypes.Required, false, false)]
    [ACClassConstructorInfo(
        new object[]
        {
            new object[] { Const.StartupParamUser,                   Global.ParamOption.Optional, typeof(VBUser)  },
            new object[] { Const.StartupParamRegisterACObjects,    Global.ParamOption.Optional, typeof(Boolean) },
            new object[] { Const.StartupParamPropPersistenceOff,   Global.ParamOption.Optional, typeof(Boolean) },
            new object[] { Const.StartupParamWCFOff,               Global.ParamOption.Optional, typeof(Boolean) },
            new object[] { Const.StartupParamSimulation,             Global.ParamOption.Optional, typeof(Boolean) },
            new object[] { Const.StartupParamFullscreen,             Global.ParamOption.Optional, typeof(Boolean) }
        }
    )]
    public class ACRoot : ACBSO, IRoot
    {
        public const string ClassName = "ACRoot";

        static private ACRoot _Root = null;
        static public ACRoot SRoot
        {
            get
            {
                return _Root;
            }
        }

        static private ACRoot _RootTest = null;
        static public ACRoot SRootTest
        {
            get
            {
                return _RootTest;
            }
        }

        #region private members
        Businessobjects _Businessobjects;
        Queries _Queries;
        Communications _Communications;
        Messages _Messages;
        IEnvironment _Environment;
        ACPointEventAbsorber _EventAbsorber;
        LocalServiceObjects _LocalServiceObjects;
        bool _RegisterACObjects = false;
        #endregion

        #region c´tors
        public ACRoot(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(gip.core.datamodel.Database.GlobalDatabase.ACClass.Where(c => c.ACClassID == acType.ACClassID).First(), content, parentACObject, parameter, acIdentifier)
        {
            if (ParameterValue(Const.StartupParamSimulation) != null)
            {
                if ((bool)ParameterValue(Const.StartupParamSimulation))
                    _ACOperationMode = ACOperationModes.Simulation;
            }

            if (ACOperationMode != ACOperationModes.Test)
            {
                _Root = this;
                gip.core.datamodel.Database.SetRoot(this);
            }
            else
            {
                _RootTest = this;
            }

            ACRef<IACObject>.SetRoot(this);

            if (ParameterValue(Const.StartupParamRegisterACObjects) != null)
            {
                _RegisterACObjects = (bool)ParameterValue(Const.StartupParamRegisterACObjects);
            }

            if (ParameterValue(Const.StartupParamPropPersistenceOff) != null)
            {
                PropPersistenceOff = (bool)ParameterValue(Const.StartupParamPropPersistenceOff);
            }

            if (ParameterValue(Const.StartupParamWCFOff) != null)
            {
                WCFOff = (bool)ParameterValue(Const.StartupParamWCFOff);
            }

            if (ParameterValue(Const.StartupParamFullscreen) != null)
            {
                Fullscreen = (bool)ParameterValue(Const.StartupParamFullscreen);
            }

            _TypeNameOfAppContext = new ACPropertyConfigValue<string>(this, "TypeNameOfAppContext", "");
            _IPlusDocsServerURL = new ACPropertyConfigValue<string>(this, "IPlusDocsServerURL", @"http://iplus-framework.com/{lang}/Get?filter.Workspaces={ACClassID}");
        }

        /// <summary>
        /// This method is called inside the Construct-Method. Derivations can have influence to the naming of the instance by changing the acIdentifier-Parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        protected override void InitACIdentifier(ACValueList parameter, ref string acIdentifier)
        {
            if (ACOperationMode == ACOperationModes.Test)
            {
                acIdentifier = Const.ACRootProjectNameTest;
            }
            else
            {
                acIdentifier = Const.ACRootProjectName;
            }
        }

        #endregion

        #region IACComponent
        public override bool ACInit(Global.ACStartTypes startChildMode = gip.core.datamodel.Global.ACStartTypes.Automatic)
        {
            Environment environment = this.Environment as Environment;
            if (ParameterValue(Const.StartupParamUser) != null && environment != null)
                environment.LoginUser(ParameterValue(Const.StartupParamUser) as VBUser);

            _Messages = ACUrlCommand(gip.core.autocomponent.Messages.ClassName) as Messages;
            if (ACOperationMode != ACOperationModes.Test)
                _Messages.LoadLoggingConfiguration();

            if (_Root == this)
            {
                ACClassTaskQueue.TaskQueue.StartWorkerThread();
                RootDbOpQueue.AppContextQueue.StartWorkerThread();
                GarbageCollector.Instance.StartWorkerThread();
                Dispatcher.StartWorkerThread();

                try
                {
                    if (_RegisterACObjects)
                        gip.core.autocomponent.Messages.ConsoleMsg("System", "Analyzing Assemblies...");
                    ACClassManager acClassManager = new ACClassManager();
                    acClassManager.RegisterAndUpdateACObjects(_RegisterACObjects);
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;
                    _Messages.LogException(ACRoot.ClassName, "ACInit", msg);
                }
            }

            gip.core.autocomponent.Messages.ConsoleMsg("System", "Initializing iPlus...");

            if (ACOperationMode != ACOperationModes.Test)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string location = "";
                    string assemblyFullName = "";
                    try
                    {
                        assemblyFullName = assembly.FullName;
                        if (assembly.IsDynamic || assemblyFullName.Contains("Anonym") || assemblyFullName.Contains("Microsoft.GeneratedCode"))
                            continue;
                        location = assembly.Location;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException(ACRoot.ClassName, "ACInit", msg);
                    }
                    _Messages.LogDebug(this.GetACUrl(), "ACInit()", String.Format("Assembly {0} from Path {1} loaded.", assemblyFullName, location));
                }

            }

            // !!! WICHTIG !!! Startreihenfolge nicht verändern
            base.ACInit(startChildMode);

            _Businessobjects = FindChildComponents<Businessobjects>(c => c is Businessobjects, null, 1).FirstOrDefault();
            if (_Businessobjects == null)
                _Businessobjects = StartComponent(gip.core.autocomponent.Businessobjects.ClassName, null, new object[] { }) as Businessobjects;

            _LocalServiceObjects = FindChildComponents<LocalServiceObjects>(c => c is LocalServiceObjects, null, 1).FirstOrDefault();
            if (_LocalServiceObjects == null)
                _LocalServiceObjects = StartComponent(LocalServiceObjects.ClassName, null, new object[] { }) as LocalServiceObjects;


            PrepareQueriesAndResoruces();

            if (ACOperationMode == ACOperationModes.Live)
            {
                VBUserInstance userInstance = environment.User.VBUserInstance_VBUser.First();
                try
                {
                    if (userInstance.VBUser.VBUserACProject_VBUser.Where(c => c.IsServer).Any())
                        _HasACModelServer = true;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException(ACRoot.ClassName, "ACInit(10)", msg);
                }

                bool userIntanceStillActive = false;
                if (_HasACModelServer)
                {
                    userIntanceStillActive = Communications.IsUserInstanceStillActive(userInstance);
                    if (userIntanceStillActive)
                        _HasACModelServer = false;
                }

                var queryCommunications = ACComponentChilds.Where(c => c is Communications).Select(c => c as Communications);
                if (queryCommunications.Any() && (ACOperationMode == ACOperationModes.Live))
                {
                    _Communications = queryCommunications.First();
                    _Communications.InitServiceHost();
                }

                // 2. Instanzen von ACProject erzeugen
                foreach (var userACProject in userInstance.VBUser.VBUserACProject_VBUser.OrderBy(c => c.ACProject.ACProjectNo))
                {
                    IACComponent acObjectNew = null;

                    gip.core.autocomponent.Messages.ConsoleMsg("System", "Initializing " + userACProject.ACProject.RootClass.ACIdentifier + (userACProject.IsClient ? " (Client)..." : " (Server)..."));

                    //ACClassTask acClassTaskProject = userACProject.ACProject.RootClass.ACClassTask_TaskTypeACClass.First(); 
                    // Task-Daten kommen aus anderen Datenbankkontext, wegen Multithreading und Dynamischen Laden von Workflows
                    ACClassTask acClassTaskProject = null;

                    ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        acClassTaskProject = ACClassTaskQueue.TaskQueue.Context.ACClassTask
                             .Include(c => c.ContentACClassWF)
                             .Where(c => c.TaskTypeACClassID == userACProject.ACProject.RootClass.ACClassID && c.IsTestmode == false)
                             .FirstOrDefault();
                    });
                    if (acClassTaskProject == null)
                        continue;
                    if (userIntanceStillActive)
                        acObjectNew = StartComponent(userACProject.ACProject.RootClass, acClassTaskProject, null, gip.core.datamodel.Global.ACStartTypes.Automatic, true);
                    else
                        acObjectNew = StartComponent(userACProject.ACProject.RootClass, acClassTaskProject, null, gip.core.datamodel.Global.ACStartTypes.Automatic, !userACProject.IsServer);

                    if (acObjectNew != null)
                    {
                        Console.WriteLine(String.Format("{0}-Instance created of {1}", userACProject.IsServer ? " Server" : " Client", acObjectNew.GetACUrl()));
                    }
                    else
                    {
                        Console.WriteLine(String.Format("{0}-Instance not created of {1}", userACProject.IsServer ? "Server" : "Client", userACProject.ACProject.RootClass.GetACUrl()));
                        if (userACProject.ACProject.RootClass != null)
                            Messages.LogError("ACRoot.InitACMModel()", "0", String.Format("Instance not created of {0}", userACProject.ACProject.RootClass.GetACUrl()));
                    }
                }
            }
            else
            {
                if (ACOperationMode == ACOperationModes.Simulation)
                {
                    _HasACModelServer = true;
                }

                var queryProjects = Database.ContextIPlus.VBUserACProject.Where(c => c.IsServer == true && c.ACProject != null).OrderBy(c => c.ACProject.ACProjectNo).Select(c => c.ACProject);
                if (queryProjects.Any())
                {
                    // 2. Instanzen von ACProject erzeugen
                    foreach (var acProject in queryProjects)
                    {
                        IACComponent acObjectNew = null;

                        gip.core.autocomponent.Messages.ConsoleMsg("System", "Initializing Test " + acProject.RootClass.ACIdentifier);

                        if (ACOperationMode == ACOperationModes.Simulation)
                        {
                            ACClassTask acClassTaskProject = null;
                            ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                            {
                                acClassTaskProject = ACClassTaskQueue.TaskQueue.Context.ACClassTask.Where(c => c.TaskTypeACClassID == acProject.RootClass.ACClassID && c.IsTestmode == false).FirstOrDefault();
                            });
                            if (acClassTaskProject == null)
                                continue;
                            acObjectNew = StartComponent(acProject.RootClass, acClassTaskProject, null, gip.core.datamodel.Global.ACStartTypes.Automatic, false);
                        }
                        else // Test
                        {
                            acObjectNew = StartComponent(acProject.RootClass, null, null, gip.core.datamodel.Global.ACStartTypes.Automatic, false);
                        }

                        if (acObjectNew != null)
                        {
                            Console.WriteLine(String.Format("Test-Server-Instance created of {0}", acObjectNew.GetACUrl()));
                        }
                        else
                        {
                            Console.WriteLine(String.Format("Test-Server-Instance not created of {0}", acProject.RootClass.GetACUrl()));
                            if (acProject.RootClass != null)
                                Messages.LogError("ACRoot.InitACMModel()", "0", String.Format("Instance not created of {0}", acProject.RootClass.GetACUrl()));
                        }
                    }
                }
            }

            if (PropPersistenceOff && ACClassTaskQueue.TaskQueue.Context.IsChanged)
            {
                ACClassTaskQueue.TaskQueue.ProcessAction(() => { ACClassTaskQueue.TaskQueue.Context.ACSaveChanges(); });
            }

            gip.core.autocomponent.Messages.ConsoleMsg("System", "Post initialization...");

            if (Database.ContextIPlus.IsChanged)
            {
                Database.ACSaveChanges();
            }

            if (environment != null)
                environment.RecalcConnectionStatistics();

            return true;
        }

        public void PrepareQueriesAndResoruces()
        {
            _Queries = ACUrlCommand(gip.core.autocomponent.Queries.ClassName) as Queries;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();
            return result;
        }

        private bool _Initialized;
        public bool Initialized
        {
            get
            {
                return _Initialized;
            }
        }

        public void OnStartupSucceeded()
        {
            gip.core.autocomponent.Messages.ConsoleMsg("System", "iPlus initialized");
            if ((_Communications != null) && (ACOperationMode == ACOperationModes.Live))
                _Communications.StartServiceHost();
            _Initialized = true;
            gip.core.autocomponent.Messages.ConsoleClear();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            Environment environment = this.Environment as Environment;
            if (environment != null)
                environment.LogoutUser();
            ComponentPool.PoolingOff = true;
            ComponentPool.ClearPool();
            InitState = ACInitState.Destructing;
            if (_Root == this)
            {
                GarbageCollector.Instance.PrepareShutdown();
            }

            if (_Dump != null)
            {
                _Dump.Detach();
                _Dump = null;
            }

            ACActivatorThread currentDeInitThread = ACActivator.CurrentDeInitializingThread;
            currentDeInitThread.InstanceDepth++;
            foreach (IACComponent acSubACComponent in ACComponentChilds.ToArray())
            {
                if (acSubACComponent == null)
                    continue;
                if (acSubACComponent == Communications)
                    continue;
                StopComponent(acSubACComponent);
            }

            if (Communications != null)
            {
                Thread.Sleep(1000);
                StopComponent(Communications);
            }

            Database.ACSaveChanges();

            if (_Root == this)
            {
                Dispatcher.StopWorkerThread();
                GarbageCollector.Instance.StopWorkerThread();
                ACClassTaskQueue.TaskQueue.StopWorkerThread();
                RootDbOpQueue.AppContextQueue.StopWorkerThread();
            }

            if (this.ACOperationMode == ACOperationModes.Live)
            {
                ACObjectContextManager.DisposeAndRemoveAll();
                gip.core.datamodel.Database.GlobalDatabase.Dispose();
            }

            base.ACDeInit(deleteACClassTask);

            ComponentPool.ClearPool();
            _ComponentPool = null;

            currentDeInitThread.InstanceDepth--;
            ACActivator.RunPostDeInit(currentDeInitThread);

            InitState = ACInitState.Destructed;

            return true;
        }

        /// <summary>
        /// Overriden: Returns the global database (gip.core.datamodel.Database.GlobalDatabase).
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                return gip.core.datamodel.Database.GlobalDatabase;
            }
        }

        private static ACDelegateQueue _Dispatcher = new ACDelegateQueue("ACDelegateQueue: IPlus.Dispatcher;");
        public static ACDelegateQueue Dispatcher
        {
            get
            {
                return _Dispatcher;
            }
        }


        #endregion

        #region Config
        private ACPropertyConfigValue<string> _TypeNameOfAppContext;
        [ACPropertyConfig("en{'Assembly-Qualified name of Application DB'}de{'Assembly-Qualified name of Application DB'}")]
        public string TypeNameOfAppContext
        {
            get
            {
                return _TypeNameOfAppContext.ValueT;
            }
            set
            {
                _TypeNameOfAppContext.ValueT = value;
            }
        }

        private ACPropertyConfigValue<string> _IPlusDocsServerURL;
        [ACPropertyConfig("en{'Documentation Server URL'}de{'Documentation Server URL'}")]
        public string IPlusDocsServerURL
        {
            get
            {
                return _IPlusDocsServerURL.ValueT;
            }
            set
            {
                _IPlusDocsServerURL.ValueT = value;
            }
        }


        #endregion

        #region IRoot
        #region Manager
        public IBusinessobjects Businessobjects
        {
            get
            {
                return _Businessobjects;
            }
        }

        public IQueries Queries
        {
            get
            {
                return _Queries;
            }
        }

        public Communications Communications
        {
            get
            {
                return _Communications;
            }
        }

        public LocalServiceObjects LocalServiceObjects
        {
            get
            {
                return _LocalServiceObjects;
            }
        }

        public ACPointEventAbsorber EventAbsorber
        {
            get
            {
                if (_EventAbsorber != null)
                    return _EventAbsorber;
                if (ACOperationMode == ACOperationModes.Live)
                    return null;
                if (_EventAbsorber == null)
                {
                    gip.core.datamodel.Database db = Database as gip.core.datamodel.Database;
                    ACClass acClass = db.GetACType(typeof(ACPointEventAbsorber));
                    if (acClass != null)
                    {
                        _EventAbsorber = StartComponent(acClass, null, new ACValueList(), Global.ACStartTypes.Automatic) as ACPointEventAbsorber;
                    }
                }
                return _EventAbsorber;
            }
        }

        private ACComponentPool _ComponentPool;
        public ACComponentPool ComponentPool
        {
            get
            {
                if (_ComponentPool == null)
                    _ComponentPool = new ACComponentPool();
                return _ComponentPool;
            }
        }

        #endregion

        #region Environment
        public IRootPageWPF RootPageWPF
        {
            get;
            set;
        }

        public IEnvironment Environment
        {
            get
            {
                if (_Environment == null)
                    _Environment = ACUrlCommand("Environment") as IEnvironment;
                return _Environment;
            }
        }

        public override IMessages Messages
        {
            get
            {
                return _Messages;
            }
        }


        public IACVBNoManager NoManager
        {
            get
            {
                if (LocalServiceObjects == null) return null;
                return LocalServiceObjects.NoManager;
            }
        }
        #endregion

        #region IACInteractiveObject
        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public override void ACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    {
                        var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                        if (query.Any())
                        {
                            ACCommand acCommand = query.First() as ACCommand;
                            if (acCommand.GetACUrl().StartsWith(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start))
                            {
                                RootPageWPF.StartBusinessobjectByACCommand(acCommand);
                                return;
                            }
                        }
                    }
                    break;

            }

            base.ACAction(actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public override bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return base.IsEnabledACAction(actionArgs);
        }
        #endregion
        #endregion

        #region Client-Side-Send-Methods

        /// <summary>Sendet eine Clientseitige Nachricht an den Server</summary>
        /// <param name="acMessage"></param>
        /// <param name="forACComponent"></param>
        /// <exception cref="gip.core.autocomponent.ACWCFException">Thrown when disconnected</exception>
        public void SendACMessage(IWCFMessage acMessage, IACComponent forACComponent)
        {
            if ((_Communications == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            _Communications.SendACMessageToServer(acMessage as WCFMessage, forACComponent);
        }

        /// <summary>Method sends a PropertyValueEvent from this Client/Proxy-Object
        /// to the Real Object on Server-side</summary>
        /// <param name="eventArgs"></param>
        /// <param name="forACComponent"></param>
        public void SendPropertyValue(IACPropertyNetValueEvent eventArgs, IACComponent forACComponent)
        {
            if ((_Communications == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            _Communications.SendPropertyValueToServer(eventArgs, forACComponent);
            SendPropertyValueDone(false, eventArgs, forACComponent);
        }

        /// <summary>
        /// Method subscribes an new generated ACObject for retrieving ValueEvents from the Server
        /// </summary>
        /// <param name="forACComponent"></param>
        public void SubscribeACObject(IACComponent forACComponent)
        {
            if ((_Communications == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            _Communications.SubscribeACObjectOnServer(forACComponent);
        }


        /// <summary>
        /// Method unsubscribes an unloaded ACObject
        /// </summary>
        /// <param name="forACComponent"></param>
        public void UnSubscribeACObject(IACComponent forACComponent)
        {
            if ((_Communications == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            _Communications.UnSubscribeACObjectOnServer(forACComponent);
        }


        /// <summary>
        /// Activates Sending of Subscription to server.
        /// Method will be called, when a common set of Objects are generated
        /// </summary>
        public void SendSubscriptionInfoToServer(bool queued = false)
        {
            if ((_Communications == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            _Communications.SendSubscriptionInfoToServer(queued);
        }

        /// <summary>
        /// Makes an Entry in Dispatcher-List, that a changed Point must be send to the Server
        /// </summary>
        /// <param name="forACComponent"></param>
        public void MarkACObjectOnChangedPointForServer(IACComponent forACComponent)
        {
            if ((_Communications == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            _Communications.MarkACObjectOnChangedPointForServer(forACComponent);
        }


        #endregion

        #region Server-Side-Send-Methods

        /// <summary>
        /// Sendet eine serverseitige Nachricht an alle Clients
        /// </summary>
        /// <param name="acMessage"></param>
        public void BroadcastACMessage(IWCFMessage acMessage)
        {
            if ((_Communications == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            _Communications.BroadcastACMessageToClients(acMessage as WCFMessage);
        }

        /// <summary>Method sends a PropertyValueEvent from this Real/Server-Object
        /// to all Proxy-Object which has subscribed ist</summary>
        /// <param name="eventArgs"></param>
        /// <param name="forACComponent"></param>
        public void BroadcastPropertyValue(IACPropertyNetValueEvent eventArgs, IACComponent forACComponent)
        {
            if ((_Communications == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            var vbDump = VBDump;
            //PerformanceEvent perfEvent = vbDump != null ? vbDump.PerfLogger.Start(ACIdentifier, 100, true) : null;
            _Communications.BroadcastPropertyValueToClients(eventArgs, forACComponent);
            //if (perfEvent != null)
            //    vbDump.PerfLogger.Stop(ACIdentifier, 100, perfEvent);

            //perfEvent = vbDump != null ? vbDump.PerfLogger.Start(ACIdentifier, 101, true) : null;
            SendPropertyValueDone(true, eventArgs, forACComponent);
            //if (perfEvent != null)
            //    vbDump.PerfLogger.Stop(ACIdentifier, 101, perfEvent);
        }

        /// <summary>
        /// Makes an Entry in Dispatcher-List, that a changed Point must be send to Clients
        /// </summary>
        /// <param name="forACComponent"></param>
        public void MarkACObjectOnChangedPointForClient(IACComponent forACComponent)
        {
            if ((_Communications == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            _Communications.MarkACObjectOnChangedPointForClient(forACComponent);
        }

        public VBUser CurrentInvokingUser
        {
            get
            {
                string threadName = Thread.CurrentThread.Name;
                if (String.IsNullOrEmpty(threadName))
                    return Environment.User;
                if (!threadName.StartsWith("ACUrl:"))
                    return Environment.User;
                string acUrl = threadName.Substring(6);
                int indexEnd = acUrl.IndexOf(";");
                if (indexEnd > 0)
                    acUrl = acUrl.Substring(0, indexEnd);
                WCFServiceChannel serviceChannel = ACUrlCommand("?" + acUrl) as WCFServiceChannel;
                if (serviceChannel == null)
                    return Environment.User;
                return serviceChannel.ConnectedUser;
            }
        }

        #endregion

        #region Events
        public event ACPropertyNetSendEventHandler OnSendPropertyValueEvent;
        private void SendPropertyValueDone(bool isBroadcast, IACPropertyNetValueEvent eventArgs, IACComponent forACComponent)
        {
            if (OnSendPropertyValueEvent != null)
            {
                OnSendPropertyValueEvent(this, new ACPropertyNetSendEventArgs(isBroadcast, eventArgs, forACComponent));
            }
        }
        #endregion

        [ACMethodInfo("", "", 9999)]
        public IEnumerable<ACValueItem> GetEnumList(string enumType)
        {
            var enums = enumType.Split('\\');

            Type t1 = Type.GetType(enums[0]);
            Type t2 = null;
            if (enums.Length > 1)
                t2 = Type.GetType(enums[1]);

            if (t2 == null || t2 == t1)
            {
                gip.core.datamodel.Database db = Database as gip.core.datamodel.Database;
                ACClass enumACClass = db.GetACType(t1);
                //using (ACMonitor.Lock(db.QueryLock_1X000))
                //{
                //    enumACClass = db.ACClass.FirstOrDefault(c => c.AssemblyQualifiedName.Contains(t1.FullName));
                //}
                if (enumACClass != null && enumACClass.ACValueListForEnum != null)
                    return enumACClass.ACValueListForEnum;
            }

            return new ACValueItemList(t1, t2);
        }

        /// <summary>
        /// Ermittelt den ACType (ACClass o. ACClassProperty) für die übergebene acUrl
        /// TODO: Derzeit funktioniert die Auflösung nur für Datenbank-ACUrls
        /// </summary>
        /// <param name="acUrl"></param>
        /// <returns></returns>
        public IACType GetACTypeFromACUrl(string acUrl)
        {
            if (acUrl.StartsWith("\\"))
                acUrl = acUrl.Substring(1);
            if (!acUrl.StartsWith(Const.ContextDatabase))
                return null;

            var x = this.Database.ContextIPlus.ACType;
            return null;
        }

        [ACMethodInfo("", "", 9999)]
        public void ClearComponentPool()
        {
            ComponentPool.ClearPool();
        }


        #region private methods
        private bool _HasACModelServer = false;
        public bool HasACModelServer
        {
            get
            {
                return _HasACModelServer;
            }
        }

        [ACPropertyInfo(9999)]
        public bool PropPersistenceOff
        {
            get;
            set;
        }

        public bool WCFOff
        {
            get;
            set;
        }

        public bool Fullscreen
        {
            get;
            set;
        }

        #endregion


        bool _DumpInstanceChecked = false;
        private ACRef<RuntimeDump> _Dump;
        public IRuntimeDump VBDump
        {
            get
            {
                if (!this.Initialized || this.InitState != ACInitState.Initialized)
                    return null;
                if (_DumpInstanceChecked && _Dump == null)
                    return null;
                if (_Dump != null && _Dump.ValueT != null)
                    return _Dump.ValueT;
                if (_Dump == null && !_DumpInstanceChecked)
                {
                    RuntimeDump dump = ACUrlCommand("\\Service\\?VBDump") as RuntimeDump;
                    _DumpInstanceChecked = true;
                    if (dump != null)
                    {
                        _Dump = new ACRef<RuntimeDump>(dump, this);
                        return _Dump.ValueT;
                    }
                }
                return null;
            }
        }

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "IsEnabledACAction":
                    result = IsEnabledACAction((ACActionArgs)acParameter[0]);
                    return true;
                case Const.MN_GetEnumList:
                    result = GetEnumList((String)acParameter[0]);
                    return true;
                case "ClearComponentPool":
                    ClearComponentPool();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }

    public delegate void ACPropertyNetSendEventHandler(object sender, ACPropertyNetSendEventArgs e);
    public class ACPropertyNetSendEventArgs : EventArgs
    {
        public ACPropertyNetSendEventArgs(bool isBroadcast, IACPropertyNetValueEvent eventArgs, IACComponent forACComponent)
        {
            _IsBroadcast = isBroadcast;
            _NetValueEventArgs = eventArgs;
            _ForACComponent = forACComponent;
        }

        private bool _IsBroadcast;
        public bool IsBroadcast
        {
            get
            {
                return _IsBroadcast;
            }
        }

        private IACPropertyNetValueEvent _NetValueEventArgs;
        public IACPropertyNetValueEvent NetValueEventArgs
        {
            get
            {
                return _NetValueEventArgs;
            }
        }

        private IACComponent _ForACComponent;
        public IACComponent ForACComponent
        {
            get
            {
                return _ForACComponent;
            }
        }
    }

}

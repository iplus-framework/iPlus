using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Collections.Concurrent;

namespace gip.core.autocomponent
{
    public enum ProxySubscriptionState : short
    {
        Unsubscribed = 0,
        SubscribePhase = 1,
        Subscribed = 2,
        UnSubscribePhase = 3,
    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'Proxyclass'}de{'Proxyklasse'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACComponentProxy : ACComponent, IACObjectRMI
    {
        #region c´tors
        public ACComponentProxy(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override void Recycle(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                _IsEnabledCache = new ConcurrentDictionary<string, LastIsEnabledResult>(2, 20);
                _SubscriptionState = ProxySubscriptionState.Unsubscribed;
            }
            base.Recycle(content, parentACObject, parameter, acIdentifier);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {

                using (ACMonitor.Lock(_20015_LockValue))
            {
                if (_RMInvoker != null)
                    _RMInvoker.Close();
                _IsEnabledCache = null;
            }
            
            //_InstanceInfo = null;
            bool result = base.ACDeInit(deleteACClassTask);
            _SubscriptionState = ProxySubscriptionState.Unsubscribed;
            return result;
        }

        public void ReloadChildsOverServerInstanceInfo(ACChildInstanceInfo instanceInfo, Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic, int maxChildDepth = 0)
        {
            if (/*(ACComponentChilds.Any() && !forceQueryServer) || */ParentACComponent == null)
                return;
            if (instanceInfo == null)
            {
                IEnumerable<ACChildInstanceInfo> childsWF = (ParentACComponent as ACComponent).GetChildInstanceInfo(maxChildDepth, false, this.ACIdentifier);
                if (childsWF != null && childsWF.Any())
                    instanceInfo = childsWF.First();
            }
            if (instanceInfo == null)
                return;
            else
            {
                PWNodeProxy pwProxy = this as PWNodeProxy;
                if (pwProxy != null)
                    pwProxy.CheckAndReplaceContent(instanceInfo);
            }
            foreach (ACChildInstanceInfo child in instanceInfo.Childs)
            {
                ACComponentProxy compAsProxy = StartComponent(child, startChildMode, true) as ACComponentProxy;
                if (compAsProxy != null && child.Childs != null && child.Childs.Any())
                {
                    compAsProxy.ReloadChildsOverServerInstanceInfo(child, startChildMode, maxChildDepth);
                }
            }
        }

        protected override void ACPostInitACPoints()
        {
            if (Root != null && Root.HasACModelServer)
                base.ACPostInitACPoints();
        }
        #endregion

        private class LastIsEnabledResult
        {
            public DateTime LastQuery { get; set; }
            public bool LastResult { get; set; }
        }

        private ConcurrentDictionary<string, LastIsEnabledResult> _IsEnabledCache;
        private ConcurrentDictionary<string, LastIsEnabledResult> IsEnabledCache
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_IsEnabledCache == null)
                        _IsEnabledCache = new ConcurrentDictionary<string, LastIsEnabledResult>(2, 20);
                    return _IsEnabledCache;
                }
            }
        }

        static TimeSpan _QueryLockTime = new TimeSpan(0, 0, 5);

        private ACObjectRMInvoker _RMInvoker = null;
        internal protected ACObjectRMInvoker RMInvoker
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_RMInvoker == null)
                        _RMInvoker = new ACObjectRMInvoker(this);
                    return _RMInvoker;
                }
            }
        }

        public override bool IsPoolable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// This method is called inside the Construct-Method. Derivations can have influence to the naming of the instance by changing the acIdentifier-Parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        protected override void InitACIdentifier(ACValueList parameter, ref string acIdentifier)
        {
            if (parameter != null)
            {
                var instanceInfo = parameter[nameof(ACChildInstanceInfo)] as ACChildInstanceInfo;
                if (instanceInfo != null)
                {
                    acIdentifier = instanceInfo.ACIdentifier;
                }
            }
        }

        internal override void FinalizeComponent()
        {
            UnSubscribeAtServer(false, true);
            Stop();
        }

        /// <summary>
        /// Subscribes this proxy component at the server to receive all data that has changed
        /// </summary>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <param name="autoSendToServer">if set to <c>true</c> [automatic send to server].</param>
        internal override void SubscribeAtServer(bool force = false, bool autoSendToServer = false)
        {
            if (SubscriptionState == ProxySubscriptionState.Unsubscribed || force)
            {
                SubscriptionState = ProxySubscriptionState.SubscribePhase;
                this.Root.SubscribeACObject(this);
                if (force)
                    this.ReSubscribePoints();
                SubscriptionState = ProxySubscriptionState.Subscribed;
                if (autoSendToServer)
                    Root.SendSubscriptionInfoToServer(true);
            }
        }

        /// <summary>
        /// Unsubscribes this proxy component at the server.
        /// </summary>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <param name="autoSendToServer">if set to <c>true</c> [automatic send to server].</param>
        internal override void UnSubscribeAtServer(bool force = false, bool autoSendToServer = false)
        {
            if (SubscriptionState == ProxySubscriptionState.Subscribed || force)
            {
                SubscriptionState = ProxySubscriptionState.UnSubscribePhase;
                this.Root.UnSubscribeACObject(this);
                SubscriptionState = ProxySubscriptionState.Unsubscribed;
                if (autoSendToServer)
                    Root.SendSubscriptionInfoToServer(true);
            }
        }

        private ProxySubscriptionState _SubscriptionState = ProxySubscriptionState.Unsubscribed;
        public ProxySubscriptionState SubscriptionState
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _SubscriptionState;
                }
            }
            private set
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _SubscriptionState = value;
                }
            }
        }

        public ApplicationManagerProxy ApplicationManager
        {
            get
            {
                if (this is ApplicationManagerProxy)
                    return this as ApplicationManagerProxy;
                return FindParentComponent<ApplicationManagerProxy>(c => c is ApplicationManagerProxy);
            }
        }

        #region Messagehandling
        /// <summary> Executes a method via passed method-name</summary>
        /// <param name="acMethodName">Name of the method (Only registered methods with ACClassMethodInfo or ACMethod's)</param>
        /// <param name="acParameter">  Parameterlist for method</param>
        /// <returns>NULL, if void-Method else the result as boxed value</returns>
        public override object ExecuteMethod(string acMethodName, params Object[] acParameter)
        {
            string acMethodName1;
            int pos = acMethodName.IndexOf('!');
            if (pos == 0)
                acMethodName1 = acMethodName.Substring(1);
            else
                acMethodName1 = acMethodName;

            ACClassMethod acClassMethod = ComponentClass.GetMethod(acMethodName1);
            if (acClassMethod == null)
                return null;
            if (acClassMethod.ACKind == Global.ACKinds.MSMethodExtClient
                || acClassMethod.ACKind == Global.ACKinds.MSMethodClient
                || IsACComponentMethod(acMethodName1))
            {
                if (acClassMethod.ACKind == Global.ACKinds.MSMethodClient)
                    AnalyzeTypeOfProxy();
                return base.ExecuteMethod(acMethodName, acParameter);
            }

            // Ask-User-Handler
            object result = null;
            try
            {
                bool askMethodHandled = false;
                string methodName2Handle = Const.AskUserPrefix + acMethodName1;
                HandleExecuteACMethodStatic staticHandler = null;

                bool hasStaticAskMethod = AnalyzeTypeOfProxy();
                Type typeOfProxy = ComponentClass.ObjectType;
                if (_StaticExecuteHandlers.TryGetValue(typeOfProxy, out staticHandler))
                {
                    askMethodHandled = staticHandler(out result, this, methodName2Handle, acClassMethod, acParameter);
                    if (askMethodHandled && !(bool)result)
                        return null;
                }

                if (!askMethodHandled && hasStaticAskMethod)
                {
                    bool? ok = null;
                    MethodInfo mi = typeOfProxy.GetMethod(methodName2Handle);
                    bool isStatic = false;
                    if (mi == null)
                    {
                        isStatic = true;
                        mi = typeOfProxy.GetMethod(methodName2Handle, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    }
                    if (mi != null)
                    {
                        isStatic = mi.IsStatic;
                        if (isStatic)
                        {
                            hasStaticAskMethod = true;
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

                            if (acParamWithThis != null && acParamWithThis.Any())
                            {
                                ParameterInfo[] miParams = mi.GetParameters();
                                if (miParams == null || (miParams != null && !miParams.Any()))
                                    acParameter = null;
                            }
                            ok = (bool)mi.Invoke(this, acParamWithThis);
                        }
                    }

                    if (ok.HasValue && !ok.Value)
                        return null;
                }
            }
            catch (Exception e)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(e.Message);
#endif
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACComponentProxy", "ExecuteACMethod", msg);
                return null;
            }

            result = RMInvoker.ExecuteMethod(acMethodName, acParameter);

            var isEnabledCache = IsEnabledCache;
            foreach (LastIsEnabledResult isEnabledResult in isEnabledCache.Values.ToArray())
            {
                isEnabledResult.LastQuery = DateTime.Now - _QueryLockTime;
            }
            return result;
        }

        //protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params Object[] acParameter)
        //{
        //    result = null;
        //    if (acClassMethod != null
        //        && (acClassMethod.ACKind == Global.ACKinds.MSMethodExtClient
        //            || acClassMethod.ACKind == Global.ACKinds.MSMethodClient))
        //    {
        //    }
        //    return false;
        //}


        public void OnACMethodExecuted(int MethodInvokeRequestID, object MethodResult)
        {
            RMInvoker.OnMethodExecuted(MethodInvokeRequestID, MethodResult);
        }

        public object InvokeACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return RMInvoker.InvokeACUrlCommand(acUrl, acParameter);
        }

        public override bool IsEnabledExecuteACMethod(string acMethodName, params Object[] acParameter)
        {
            string acMethodName1;
            int pos = acMethodName.IndexOf('!');
            if (pos == 0)
                acMethodName1 = acMethodName.Substring(1);
            else
                acMethodName1 = acMethodName;

            ACClassMethod acClassMethod = ComponentClass.GetMethod(acMethodName1);
            if (acClassMethod == null)
                return false;
            if (acClassMethod.ACKind == Global.ACKinds.MSMethodExtClient
                || acClassMethod.ACKind == Global.ACKinds.MSMethodClient
                || IsACComponentMethod(acMethodName1))
            {
                if (acClassMethod.ACKind == Global.ACKinds.MSMethodClient)
                    AnalyzeTypeOfProxy();
                return base.IsEnabledExecuteACMethod(acMethodName, acParameter);
            }

            LastIsEnabledResult isEnabledResult = null;
            var isEnabledCache = IsEnabledCache;

            if (!isEnabledCache.TryGetValue(acMethodName, out isEnabledResult))
            {
                isEnabledResult = new LastIsEnabledResult() { LastQuery = DateTime.Now, LastResult = false };
                isEnabledCache.TryAdd(acMethodName, isEnabledResult);
            }
            else
            {
                if (isEnabledResult == null)
                    return false;
                if ((isEnabledResult.LastQuery + _QueryLockTime) > DateTime.Now)
                    return isEnabledResult.LastResult;
            }

            var result = RMInvoker.IsEnabledExecuteMethod(acMethodName, acParameter);
            if (result is bool)
            {
                isEnabledResult.LastResult = (bool)result;
                isEnabledResult.LastQuery = DateTime.Now;
                return isEnabledResult.LastResult;
            }
            return false;
        }

        /// <summary>
        /// Invokes the static constructor only one time and returns true if proxy has static AskMethods
        /// </summary>
        /// <returns></returns>
        private bool AnalyzeTypeOfProxy()
        {
            bool hasStaticAskMethod = false;
            Type typeOfProxy = ComponentClass.ObjectType;
            if (!_HasStaticAskMethods.TryGetValue(typeOfProxy, out hasStaticAskMethod))
            {
                try
                {
                    // Rufe statischen Konstruktor auf um evtl. einen Eintrag in _StaticExecuteHandlers zu erhalten
                    ConstructorInfo constructor = typeOfProxy.GetConstructor(BindingFlags.Static | BindingFlags.NonPublic, null, new Type[0], null);
                    if (constructor != null)
                        constructor.Invoke(null, null);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponentProxy", "AnalyzeTypeOfProxy", msg);
                }
                try
                {
                    var methods = typeOfProxy.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    hasStaticAskMethod = methods != null && methods.Where(c => c.Name.StartsWith(Const.AskUserPrefix)).Any();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponentProxy", "AnalyzeTypeOfProxy(10)", msg);
                }
                _HasStaticAskMethods.TryAdd(typeOfProxy, hasStaticAskMethod);
            }
            return hasStaticAskMethod;
        }


        public override IEnumerable<ACChildInstanceInfo> GetChildInstanceInfo(int maxChildDepth, bool onlyWorkflows, string acIdentifier = "")
        {
            IEnumerable<ACChildInstanceInfo> childsOnServer = RMInvoker.ExecuteMethod(nameof(GetChildInstanceInfo), new Object[] { maxChildDepth, onlyWorkflows, acIdentifier }) as IEnumerable<ACChildInstanceInfo>;
            if (childsOnServer == null)
                return null;

            using (ACMonitor.Lock(Root.Database.ContextIPlus.QueryLock_1X000))
                using (ACMonitor.Lock(ACClassTaskQueue.TaskQueue.Context.QueryLock_1X000))
                {
                    foreach (ACChildInstanceInfo childInfo in childsOnServer)
                    {
                        childInfo.AttachTo(Root.Database.ContextIPlus, ACClassTaskQueue.TaskQueue.Context);
                    }
                }
            return childsOnServer;
        }

        public override IEnumerable<ACChildInstanceInfo> GetChildInstanceInfo(int maxChildDepth, ChildInstanceInfoSearchParam searchParam)
        {
            IEnumerable<ACChildInstanceInfo> childsOnServer = RMInvoker.ExecuteMethod(nameof(GetChildInstanceInfo), new Object[] { maxChildDepth, searchParam }) as IEnumerable<ACChildInstanceInfo>;
            if (childsOnServer == null)
                return null;

            using (ACMonitor.Lock(Root.Database.ContextIPlus.QueryLock_1X000))
                using (ACMonitor.Lock(ACClassTaskQueue.TaskQueue.Context.QueryLock_1X000)) 
                {
                    foreach (ACChildInstanceInfo childInfo in childsOnServer)
                    {
                        childInfo.AttachTo(Root.Database.ContextIPlus, ACClassTaskQueue.TaskQueue.Context);
                    }
                }
            return childsOnServer;
        }


        #endregion

        #region IACObjectWithBinding Member
        public override bool IsProxy
        {
            get
            {
                return true;
            }
        }

        [ACPropertyInfo(9999)]
        public override IEnumerable<IACComponent> ACComponentChildsOnServer
        {
            get
            {
                ReloadChildsOverServerInstanceInfo(null, Global.ACStartTypes.Automatic, 2);
                return ACComponentChilds;
            }
        }

        #endregion

        public override PropertyLogListInfo GetArchiveLog(string propertyName, DateTime from, DateTime to)
        {
            return RMInvoker.ExecuteMethod(nameof(GetArchiveLog), new Object[] { propertyName, from, to }) as PropertyLogListInfo;
        }

        public override void RestoreTargetProp(bool onlyUnbound = true)
        {
            RMInvoker.ExecuteMethod(nameof(RestoreTargetProp), new Object[] { onlyUnbound });
        }

        #region IACMenuBuilder Member
        /// <summary>
        /// A ACMenuItem contains a ACUrl of the Method that should be invoked.
        /// GetMenu() is called from gip.core.autocomponent.MenuManager.GetMenu()-Method.
        /// The MenuManager calls GetMenu() at all instances that implement IACMenuBuilder and which have a relationship inside the MVVM-Pattern.
        /// All ACMenuItemList's are afterwards merged together to one menu that is displayed as a contextmenu on the GUI.
        /// </summary>
        /// <param name="vbContent">VBContent of the WPF-Control where the user has requested the menu first</param>
        /// <param name="vbControl">Type.FullName of the WPF-Control where the user has requested the menu first</param>
        /// <returns>List of menu entries</returns>
        public override ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = RMInvoker.ExecuteMethod(nameof(GetMenu), new Object[] { vbContent, vbControl }) as ACMenuItemList;
            if (acMenuItemList != null)
            {
                foreach (var acMenuItem in acMenuItemList)
                {
                    acMenuItem.HandlerACElement = this;
                }
            }
            else
            {
                acMenuItemList = base.GetMenu(vbContent, vbControl);
                foreach (var acMenuItem in acMenuItemList)
                {
                    acMenuItem.HandlerACElement = this;
                }
            }
            return acMenuItemList;
        }
        #endregion

        #region Diagnostics and Dump
        /// <summary>
        /// Dumps the state of this instance (Properties, Points) and returns a XmlDocument
        /// Additionaly it invokes DumpAsXMLDoc on the real instance on server-side.
        /// </summary>
        /// <param name="maxChildDepth">The maximum child depth.</param>
        /// <returns>XmlDocument</returns>
        public override XmlDocument DumpAsXMLDoc(int maxChildDepth = 0)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement xmlDump = doc.CreateElement("Dump");
            doc.AppendChild(xmlDump);
            XmlElement xmlProxy = doc.CreateElement("Proxy");
            xmlDump.AppendChild(xmlProxy);
            DumpCreateXMLElement(doc, xmlProxy, 0, maxChildDepth);

            XmlElement xmlReal = doc.CreateElement("Real");
            xmlDump.AppendChild(xmlReal);
            string xmlResult = RMInvoker.ExecuteMethod(nameof(DumpAsXMLString), new Object[] { maxChildDepth }) as string;
            if (!String.IsNullOrEmpty(xmlResult))
            {
                XmlDocument doc2 = DumpFromXMLString(xmlResult);
                if (doc2 != null)
                {
                    xmlReal.AppendChild(doc.ImportNode(doc2.DocumentElement, true));
                }
            }

            return doc;
        }

        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            base.DumpPropertyList(doc, xmlACPropertyList);

            XmlElement xmlConnectionState = xmlACPropertyList[nameof(ConnectionState)];
            if (xmlConnectionState == null)
            {
                xmlConnectionState = doc.CreateElement(nameof(ConnectionState));
                xmlConnectionState.InnerText = ConnectionState.ToString();
                xmlACPropertyList.AppendChild(xmlConnectionState);
            }

            XmlElement xmlSubscriptionState = xmlACPropertyList[nameof(SubscriptionState)];
            if (xmlSubscriptionState == null)
            {
                xmlSubscriptionState = doc.CreateElement(nameof(SubscriptionState));
                xmlSubscriptionState.InnerText = ConnectionState.ToString();
                xmlACPropertyList.AppendChild(xmlSubscriptionState);
            }
        }

        #endregion

        #region Printing
        /// <summary>Prints the state of the component to a XPSDocument.</summary>
        public override void PrintSelf()
        {
            RMInvoker.ExecuteMethod(nameof(PrintSelf), new Object[] { });
        }
        #endregion

        #region Configuration
        public override void ResetConfigValuesCache()
        {
            RMInvoker.ExecuteMethod(nameof(ResetConfigValuesCache), new Object[] { });
        }
        #endregion
    }
}

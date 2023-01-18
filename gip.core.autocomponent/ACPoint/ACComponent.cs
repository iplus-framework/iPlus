using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using gip.core.datamodel;
using System.Threading;
using System.Xml;
using System.IO;
using System.Data;
using System.Runtime.Serialization;


namespace gip.core.autocomponent
{
    public abstract partial class ACComponent
    {
        #region Initialization
        private void ACInitACPoint(ACClassProperty acClassProperty, Type typeOfThis)
        {
            if (InitState != ACInitState.Initializing)
                return;

            // Falls Point schon zuvor im ACInit von Instanz erzeugt
            if (GetPoint(acClassProperty.ACIdentifier) != null)
                return;

            // Ermittle über ACClassProperty, welcher Typ instanziert werden soll
            Type typeT = Type.GetType(acClassProperty.AssemblyQualifiedName);
            if (typeT == null)
            {
                typeT = Type.GetType(acClassProperty.AssemblyQualifiedName);
                if (typeT == null)
                {
                    // ErrorMessage
                    return;
                }
            }

            Type typeTGeneric = null;
            if (!String.IsNullOrEmpty(acClassProperty.GenericType))
            {
                typeTGeneric = Type.GetType(acClassProperty.GenericType);
                if (typeTGeneric == null)
                {
                    typeTGeneric = TypeAnalyser.GetTypeInAssembly(acClassProperty.GenericType);
                    if (typeTGeneric == null)
                    {
                        // ErrorMessage
                        return;
                    }
                }
            }

            Type TypeToCreate = null;
            //TODO: falls es andere Implementierungen ausser Event, AsyncRMI gibt, wie wir Proxy-Type automatisch ermittelt?
            if (this.IsProxy)
            {
                if (typeTGeneric == null)
                {
                    if (typeT == typeof(ACPointClientACObject))
                        TypeToCreate = typeof(ACPointClientACObjectProxy);
                    else if (typeT == typeof(ACPointServiceACObject))
                        TypeToCreate = typeof(ACPointServiceACObjectProxy);
                    else if (typeT == typeof(ACPointEventSubscr))
                        TypeToCreate = typeof(ACPointEventSubscrProxy);
                    else if (typeT == typeof(ACPointEvent))
                        TypeToCreate = typeof(ACPointEventProxy);
                    else if (typeT == typeof(ACPointAsyncRMISubscr))
                        TypeToCreate = typeof(ACPointAsyncRMISubscrProxy);
                    else if (typeT == typeof(ACPointAsyncRMI))
                        TypeToCreate = typeof(ACPointAsyncRMIProxy);
                    else if (typeT == typeof(ACPointTask))
                        TypeToCreate = typeof(ACPointTaskProxy);
                    else if (typeT == typeof(PWPointEventSubscr))
                        TypeToCreate = typeof(PWPointEventSubscrProxy);
                    else if (typeT == typeof(PWPointIn))
                        TypeToCreate = typeof(PWPointInProxy);
                    // Sonst kein netzwerkfähiger ACPoint
                    // PWPointIn, PWPointOut, PLPointIn, PLPointOut, PAPoint, ACPointReference
                    else
                        TypeToCreate = typeT;
                }
                else
                {
                    if (typeTGeneric == typeof(ACPointServiceObject<>))
                    {
                        Type proxyTypeT = typeof(ACPointServiceObjectProxy<>);
                        TypeToCreate = proxyTypeT.MakeGenericType(new Type[] { typeT });
                    }
                    else if (typeTGeneric == typeof(ACPointClientObject<>))
                    {
                        Type proxyTypeT = typeof(ACPointClientObjectProxy<>);
                        TypeToCreate = proxyTypeT.MakeGenericType(new Type[] { typeT });
                    }
                    // Sonst kein netzwerkfähiger ACPoint
                    else
                    {
                        TypeToCreate = typeTGeneric.MakeGenericType(new Type[] { typeT });
                    }
                }
            }
            // Falls Server-Objekt
            else
            {
                // Falls nicht Generischer Typ, dann ist es ein ACObject-Point, Event, oder AsyncRMI
                // PWPointIn, PWPointOut, PLPointIn, PLPointOut, PAPoint, ACPointReference
                if (typeTGeneric == null)
                    TypeToCreate = typeT;
                // Sonst generischer Typ
                else
                    TypeToCreate = typeTGeneric.MakeGenericType(new Type[] { typeT });
            }

            // Erzeuge Instanz
            if (TypeToCreate != null)
            {
                try
                {
                    IACPointBase pointInstance = (IACPointBase)Activator.CreateInstance(TypeToCreate, new Object[] { (IACComponent)this, (ACClassProperty)acClassProperty, (uint)acClassProperty.ACPointCapacity });
                    // Falls Assembly-Property, dann weise Instanz dem Member zu.
                    if (!this.IsProxy)
                    {
                        typeOfThis.InvokeMember(acClassProperty.ACIdentifier, BindingFlags.SetProperty, null, this, new object[] { pointInstance });

                        if (pointInstance is IACPointNetBase)
                        {
                            IACPointNetBase pointNetInstance = (IACPointNetBase)pointInstance;
                            string setMethodName = "OnSet" + acClassProperty.ACIdentifier;
                            MethodInfo mi = typeOfThis.GetMethod(setMethodName);
                            if (mi != null)
                                pointNetInstance.SetMethod = (ACPointSetMethod)Delegate.CreateDelegate(typeof(ACPointSetMethod), this, mi);
                            else
                                OnAssignACPointToMember(pointNetInstance);
                        }
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponent", "ACInitACPoint", msg);
                }
            }
        }

        protected virtual void ACPostInitACPoints()
        {
            try
            {
                var query = ACPointList.Where(c => (c.ACType != null) && (c is PAPoint)).Select(c => c as PAPoint);
                foreach (PAPoint paPoint in query)
                {
                    if (   (this.InitState == ACInitState.Reloading || this.InitState == ACInitState.Reloaded)
                        && paPoint.ParentACComponent == null)
                    {
                        paPoint.RecycleMemberAndAttachTo(this);
                    }
                    paPoint.ACPostInit();
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACComponent", "ACPostInitACPoints", msg);
            }
        }

        private void ACDeInitACPoints(bool deleteACClassTask = false)
        {
            var pointList = ACPointList;
            if (pointList != null)
            {
                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    foreach (var member in pointList)
                    {
                        member.ACDeInit(deleteACClassTask);
                        if (InitState == ACInitState.Destructing)
                            ACMemberList.Remove(member);
                    }
                }
            }
        }

        protected void ReSubscribePoints()
        {
            if (ACPointNetList == null)
                return;
            foreach (IACPointNetBase point in ACPointNetList)
            {
                point.ReSubscribe();
                //point.PointChangedForBroadcast = true;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="cPoint"></param>
        protected virtual void OnAssignACPointToMember(IACPointNetBase cPoint)
        {
        }

        public List<ACPSubscrObjService> SubscribedProxyObjects
        {
            get
            {
                if (ReferencePoint == null)
                    return null;
                return ReferencePoint.ConnectionList.Where(c => c is ACPSubscrObjService).Select(c => c as ACPSubscrObjService).ToList();
            }
        }

        #endregion

        #region Methods Get/Set Points

        #region Points
        /// <summary>Returns all members that are from type IACPointBase</summary>
        /// <value>A thread safe list of all members from type IACPointBase</value>
        public List<IACPointBase> ACPointList
        {
            get
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    if (ACMemberList == null)
                        return null;
                    return ACMemberList.Where(c => c is IACPointBase).Select(c => c as IACPointBase).ToList();
                }
            }
        }

        /// <summary>
        /// Finds a the point in this instance
        /// </summary>
        /// <param name="acIdentifier">Name of the point</param>
        /// <returns>Point as IACPointBase</returns>
        public IACPointBase GetPoint(string acIdentifier)
        {
            if (String.IsNullOrEmpty(acIdentifier))
                return null;
            try
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    return ACMemberList.Where(c => (c.ACType != null) && (c.ACType.ACIdentifier == acIdentifier && c is IACPointBase)).FirstOrDefault() as IACPointBase;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACComponent", "GetPoint", msg);
            }
            return null;
        }
        #endregion

        #region NetPoints
        /// <summary> Returns all members that are from type IACPointNetBase</summary>
        /// <value> A thread safe list of all members from type IACPointNetBase</value>
        public List<IACPointNetBase> ACPointNetList
        {
            get
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    return ACMemberList.Where(c => c is IACPointNetBase).Select(c => c as IACPointNetBase).ToList();
                }
            }
        }

        /// <summary>
        /// Finds a network-capable point in this instance
        /// </summary>
        /// <param name="acIdentifier">Name of the point</param>
        /// <returns>Point as IACPointNetBase</returns>
        public IACPointNetBase GetPointNet(string acIdentifier)
        {
            // Propertyname per Reflection?
            // Abfrage nach To/From, Nach Event, Request, Object//Nach WrapperType

            if (string.IsNullOrEmpty(acIdentifier))
                return null;
            try
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    //var query = ACMemberList.AsParallel().Where(c => (c.ACType != null) && (c.ACType.ACIdentifier == acName && c is IACPointNetBase));
                    return ACMemberList.Where(c => (c.ACType != null) && (c.ACType.ACIdentifier == acIdentifier && c is IACPointNetBase)).FirstOrDefault() as IACPointNetBase;
                    //if (query.Any())
                    //{
                    //    return query.FirstOrDefault() as IACPointNetBase;
                    //}
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACComponent", "GetPointNet", msg);
            }
            return null;
        }


        /// <summary>
        /// Subscribes all net points.
        /// </summary>
        public void SubscribeAllNetPoints()
        {
            if (ACPointNetList != null)
            {
                foreach (IACPointNetBase point in ACPointNetList)
                {
                    point.Subscribe();
                }
            }
        }

#endregion

#endregion

#region Events

#region Event
        /// <summary>
        /// Returns all Event-Points, that this Class provides
        /// </summary>
        public List<IACPointEvent> Events
        {
            get
            {
                try
                {

                    using (ACMonitor.Lock(LockMemberList_20020))
                    {
                        //return ACMemberList.AsParallel().Where(c => c is IACPointEvent).Select(c => c as IACPointEvent).ToList();
                        return ACMemberList.Where(c => c is IACPointEvent).Select(c => c as IACPointEvent).ToList();
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponent", "Events", msg);
                }
                return null;
            }
        }
#endregion

#region Event-Subscription
        /// <summary>
        /// Returns all Event-Subscription-Points, that this class provides
        /// </summary>
        public List<IACPointEventSubscr> EventSubscriptions
        {
            get
            {
                try
                {

                    using (ACMonitor.Lock(LockMemberList_20020))
                    {
                        return ACMemberList.Where(c => c is IACPointEventSubscr).Select(c => c as IACPointEventSubscr).ToList();
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponent", "EventSubscriptions", msg);
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the first Event-Subscription-Point, that this class provides. It calls EventSubscriptions.FirstOrDefault();
        /// </summary>
        public IACPointEventSubscr EventSubscription
        {
            get
            {
                if (EventSubscriptions == null)
                    return null;
                return EventSubscriptions.FirstOrDefault();
            }
        }
        #endregion

        #endregion

        #region AsyncRMI

        #region AsyncCall
        /// <summary>
        /// Returns all points for asynchronous method invocation, that this class provides
        /// Normally only one Async-Point should be declared in a class.
        /// </summary>
        public List<IACPointAsyncRMI> AsyncCalls
        {
            get
            {
                try
                {

                    using (ACMonitor.Lock(LockMemberList_20020))
                    {
                        return ACMemberList.Where(c => c is IACPointAsyncRMI).Select(c => c as IACPointAsyncRMI).ToList();
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponent", "AsyncCalls", msg);
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the first point for asynchronous method invocation, that this class provides. It calls AsyncCalls.FirstOrDefault();
        /// </summary>
        public IACPointAsyncRMI AsyncCall
        {
            get
            {
                if (AsyncCalls == null)
                    return null;
                return AsyncCalls.FirstOrDefault();
            }
        }
        #endregion

        #region AsyncCall-Subscription
        /// <summary>
        /// Returns all Subscription-Points for asynchronous method invocation, that this class provides
        /// Normally only one Subscription-Point should be declared in a class.
        /// </summary>
        public List<IACPointAsyncRMISubscr> AsyncCallSubscriptions
        {
            get
            {
                try
                {

                    using (ACMonitor.Lock(LockMemberList_20020))
                    {
                        return ACMemberList.Where(c => c is IACPointAsyncRMISubscr).Select(c => c as IACPointAsyncRMISubscr).ToList();
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponent", "AsyncCallSubscriptions", msg);
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the first subscription-point for asynchronous method invocation, that this class provides. It calls AsyncCallSubscriptions.FirstOrDefault();
        /// </summary>
        public IACPointAsyncRMISubscr AsyncCallSubscription
        {
            get
            {
                if (AsyncCallSubscriptions == null)
                    return null;
                return AsyncCallSubscriptions.FirstOrDefault();
            }
        }
#endregion

#region ACMethod-Call
        private SafeList<AsyncMethodWaitHandle> _SyncInvocationWaitHandleList = null;
        internal SafeList<AsyncMethodWaitHandle> SyncInvocationWaitHandleList
        {
            get
            {
                if (_SyncInvocationWaitHandleList == null)
                {

            using (ACMonitor.Lock(LockMemberList_20020))
                    {
                        if (_SyncInvocationWaitHandleList == null)
                            _SyncInvocationWaitHandleList = new SafeList<AsyncMethodWaitHandle>();
                    }
                }
                return _SyncInvocationWaitHandleList;
            }
        }

#endregion

#endregion

#region Diagnostics and Dump
        protected virtual void DumpPointList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            foreach (IACPointBase point in ACPointList)
            {
                XmlElement xmlProperty = doc.CreateElement(point.ACIdentifier);
                string xmlValue = point.ValueSerialized(true);
                if (!String.IsNullOrEmpty(xmlValue))
                {
                    xmlProperty.InnerText = xmlValue;
                }
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            XmlElement xmlStrongReferencesCount = xmlACPropertyList["StrongReferencesCount"];
            if (xmlStrongReferencesCount == null)
            {
                xmlStrongReferencesCount = doc.CreateElement("StrongReferencesCount");
                xmlStrongReferencesCount.InnerText = _ReferenceList.CountStrongReferences.ToString();
                xmlACPropertyList.AppendChild(xmlStrongReferencesCount);
            }
        }
#endregion

    }
}

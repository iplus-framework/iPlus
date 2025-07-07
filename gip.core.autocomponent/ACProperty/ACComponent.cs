// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;

namespace gip.core.autocomponent
{
    public abstract partial class ACComponent
    {
        #region IACObjectWithBinding Member
        /// <summary>
        /// If Proxy, then a real instance exists on Serverside.
        /// This instance can only be a ACComponentProxy or a derivation of it
        /// </summary>
        public virtual bool IsProxy
        {
            get
            {
                return false;
            }
        }

        private bool _PropertiesReceived = false;
        /// <summary>
        /// If this instance is a prox-component, than this prooperty indicates if values are received from the real instance on server side after subscription.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [properties received]; otherwise, <c>false</c>.
        /// </value>
        internal bool PropertiesReceived
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _PropertiesReceived;
                }
            }

            set
            {
                bool changed = false;

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    changed = _PropertiesReceived != value;
                    if (changed)
                        _PropertiesReceived = value;
                }
                if (changed)
                    OnPropertyChanged("ConnectionState");
            }
        }

        internal virtual void OnValueEventReceivedRemote(IACPropertyNetBase property, IACPropertyNetValueEvent eventArgs)
        {
            PropertiesReceived = true;
        }

        /// <summary>
        /// Connection-State (Only for proxies)
        /// </summary>
        public ACObjectConnectionState ConnectionState
        {
            get
            {
                if (!IsProxy)
                    return ACObjectConnectionState.ValuesReceived;
                if (_PropertiesReceived == true)
                    return ACObjectConnectionState.ValuesReceived;
                if (((ACRoot)this.Root).Communications.WCFClientManager == null)
                    return ACObjectConnectionState.DisConnected;
                IACComponent projectService;
                WCFClientChannel clientChannel = ((ACRoot)this.Root).Communications.WCFClientManager.GetChannelForRemoteObject(this, out projectService, false);
                if (clientChannel == null)
                    return ACObjectConnectionState.DisConnected;
                if (clientChannel.IsConnected)
                    return ACObjectConnectionState.Connected;
                return ACObjectConnectionState.DisConnected;
            }
        }

        #endregion

        #region ACClassProperty
        /// <summary>
        /// Finds a network-capable property in this instance
        /// </summary>
        /// <param name="acIdentifier">Name of the property</param>
        /// <returns>Reference to the property. Returns NULL if property could not be found.</returns>
        public IACPropertyNetBase GetPropertyNet(string acIdentifier)
        {
            if (string.IsNullOrEmpty(acIdentifier))
                return null;

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                try
                {
                    return ACMemberList.Where(c => c is IACPropertyNetBase && (c as IACPropertyNetBase).ACIdentifier == acIdentifier).FirstOrDefault() as IACPropertyNetBase;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponent", "GetPropertyNet", msg);
                }
            }
            return null;
        }


        /// <summary>
        /// Finds a property in this instance
        /// </summary>
        /// <param name="acIdentifier">Name of the property</param>
        /// <returns>Reference to the property. Returns NULL if property could not be found.</returns>
        public IACPropertyBase GetProperty(string acIdentifier)
        {
            if (string.IsNullOrEmpty(acIdentifier))
                return null;
            try
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    if (ACMemberList == null)
                        return null;
                    var query = ACMemberList.Where(c => (c.ACType != null) && (c.ACType.ACIdentifier == acIdentifier && c is IACPropertyBase));
                    //var query = ACMemberList.AsParallel().Where(c => (c.ACType != null) && (c.ACType.ACIdentifier == acName && c is IACPropertyBase));
                    if (query.Any())
                    {
                        return query.First() as IACPropertyBase;
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACComponent", "GetProperty", msg);
            }
            return null;
        }

        /// <summary>
        /// THREAD-SAFE: Returns a new List of all Properties of this Instance
        /// </summary>
        [ACPropertyInfo(9999)]
        public List<IACPropertyBase> ACPropertyList
        {
            get
            {
                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    if (ACMemberList == null)
                        return null;
                    return ACMemberList.Where(c => c is IACPropertyBase).Select(c => c as IACPropertyBase).ToList();
                }
            }
        }

        /// <summary>
        /// THREAD-SAFE: Returns a new List of properites that are relevant the VBBSODiagnosticDialog
        /// </summary>
        /// <value>
        /// The ac diag property list.
        /// </value>
        [ACPropertyInfo(9999)]
        public List<IACPropertyBase> ACDiagPropertyList
        {
            get
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    return ACMemberList.Where(c => c is IACPropertyBase && c.ACIdentifier != "ACDiagnoseInfo" && c.ACIdentifier != "ACDiagnoseXMLDoc" && c.ACIdentifier != "ACComponentChildsOnServer").Select(c => c as IACPropertyBase).ToList();
                }
            }
        }


        private bool _LateBindingNeedAtACPostInit = false;
        /// <summary>
        /// Initializes all Properties in this instance. Don't invoke this method. It's called in ACInit() when a this instance is created new or recycled from the pool.
        /// </summary>
        protected void ACInitACProperties()
        {
            if (this.ACType == null)
                return;

            if ((InitState != ACInitState.RecycledFromPool)
                && (InitState != ACInitState.Reloading)
                && (InitState != ACInitState.Initializing))
                return;

            List<IACPropertyNetTarget> targetsBound = new List<IACPropertyNetTarget>();
            if (InitState == ACInitState.RecycledFromPool)
            {
                _LateBindingNeedAtACPostInit = false;
                //foreach (var member in ACMemberList)
                //{
                //    member.RecycleMemberAndAttachTo(this);
                //}

                foreach (IACPropertyBase acProperty in ACPropertyList)
                {
                    if (!IsProxy) // Falls kein IsProxy
                    {
                        if (!(acProperty is IACPropertyNetServer))
                        {
                            acProperty.ReStoreFromDB(true);
                        }
                        else
                        {
                            if (acProperty is IACPropertyNetSource)
                            {
                                acProperty.ReStoreFromDB(true);
                                BindPropertyToSource(acProperty as IACPropertyNetSource, targetsBound, PropBindingMode.BindOnly);
                            }
                            else if (acProperty is IACPropertyNetTarget)
                            {
                                ACClassPropertyRelation propertyBinding = (acProperty.ACType as ACClassProperty).TopBaseACClassProperty.GetMyPropertyBindingToSource(this.ComponentClass);
                                if (propertyBinding == null)
                                    acProperty.ReStoreFromDB(true);
                                else
                                {
                                    if ((propertyBinding.SourceACClass == null || propertyBinding.SourceACClassProperty == null || propertyBinding.SourceACClassProperty.ObjectType == null))
                                        continue;
                                    // Finde Objekt das die Source-Property beeinhaltet
                                    IACComponent sourceACComponent = (IACComponent)ACUrlCommand("?" + propertyBinding.SourceACClass.GetACUrlComponent(), null);
                                    if (sourceACComponent != null)
                                    {
                                        IACPropertyNetSource acPropertySource = sourceACComponent.GetMember(propertyBinding.SourceACClassProperty.ACIdentifier) as IACPropertyNetSource;
                                        if (acPropertySource != null)
                                        {
                                            (acProperty as IACPropertyNetTarget).BindPropertyToSource(acPropertySource, PropBindingMode.BindOnly);
                                            targetsBound.Add(acProperty as IACPropertyNetTarget);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // Falls IsProxy
                    {
                        acProperty.ReStoreFromDB(true);
                        if (Root.HasACModelServer && (acProperty is IACPropertyNetSource))
                        {
                            acProperty.ReStoreFromDB(true);
                            BindPropertyToSource(acProperty as IACPropertyNetSource, targetsBound, PropBindingMode.BindOnly);
                        }
                    }
                }
            }
            else
            {
                IEnumerable<ACClassProperty> properties = null;
                if (InitState == ACInitState.Reloading)
                {
                    using (ACMonitor.Lock(LockMemberList_20020))
                    {
                        if (_ACMemberList != null)
                        {
                            // Lösche alle Properties
                            foreach (var prop in _ACMemberList.Where(c => c is IACPropertyBase).ToArray())
                            {
                                _ACMemberList.Remove(prop);
                            }
                        }
                    }
                    properties = ComponentClass.GetProperties(true);
                }
                else
                    properties = ComponentClass.Properties;

                Type typeOfThis = GetType();

                foreach (ACClassProperty acClassProperty in properties)
                {
                    if (acClassProperty.ACPropUsage == Global.ACPropUsages.ConnectionPoint ||
                        acClassProperty.ACPropUsage == Global.ACPropUsages.EventPoint ||
                        acClassProperty.ACPropUsage == Global.ACPropUsages.EventPointSubscr ||
                        acClassProperty.ACPropUsage == Global.ACPropUsages.AsyncMethodPoint)
                    {
                        ACInitACPoint(acClassProperty, typeOfThis);
                        continue;
                    }
                    //if (!acClassProperty.IsBroadcast)
                    //    continue;
                    //bool memberAssigned = true;
                    IACPropertyBase acProperty = null;
                    IACMember acMember = GetMember(acClassProperty.ACIdentifier);
                    if (acMember != null)
                    {
                        if (!(acMember is IACPropertyBase))
                            continue;
                        acProperty = (IACPropertyBase)acMember;
                    }

                    // Falls Property noch nicht instanziert wurde im Konstruktor oder ACInit oder durch Skript, 
                    // dann erzeuge ein neues
                    if (acProperty == null)
                    {
                        Type typeT = acClassProperty.ObjectFullType;
                        if (typeT == null)
                        {
                            // ErrorMessage
                            continue;
                        }
                        //if (typeT.IsEnum)
                        //{
                        //typeT = typeof(Int16);
                        //}

                        Type genericType = null;
                        if (!IsProxy) // Falls kein IsProxy
                        {
                            // !(acProperty is IACPropertyNetServer)
                            if (!acClassProperty.IsBroadcast)
                            {
                                genericType = ACClassProperty.GetGenericACPropertyType(typeof(ACProperty<>), typeT);
                                acProperty = (IACPropertyBase)Activator.CreateInstance(genericType, new Object[] { this, acClassProperty, false });
                                acProperty.ACInit();
                            }
                            else
                            {
                                // acProperty is IACPropertyNetSource
                                if (!acClassProperty.IsProxyProperty)
                                {
                                    genericType = ACClassProperty.GetGenericACPropertyType(typeof(ACPropertyNetSource<>), typeT);
                                    acProperty = (IACPropertyNetBase)Activator.CreateInstance(genericType, new Object[] { this, acClassProperty });
                                    acProperty.ACInit();
                                    BindPropertyToSource(acProperty as IACPropertyNetSource, targetsBound, PropBindingMode.BindOnly);
                                }
                                else if (acClassProperty.IsProxyProperty)
                                {
                                    ACClassPropertyRelation propertyBinding = acClassProperty.TopBaseACClassProperty.GetMyPropertyBindingToSource(this.ComponentClass);

                                    if (propertyBinding != null)// -> Property ist ein Target und hat einen Source-Wert
                                    {
                                        if (propertyBinding.SourceACClass == null || propertyBinding.SourceACClassProperty == null || propertyBinding.SourceACClassProperty.ObjectType == null)
                                        {
                                            // ErrorMeldung
                                            continue;
                                        }

                                        bool isMemberLive = false;
                                        // Finde Objekt das die Source-Property beeinhaltet
                                        IACComponent sourceACComponent = (IACComponent)ACUrlCommand("?" + propertyBinding.SourceACClass.GetACUrlComponent(), null);
                                        if (sourceACComponent != null)
                                        {
                                            IACMember acPropertySource = sourceACComponent.GetMember(propertyBinding.SourceACClassProperty.ACIdentifier);
                                            if ((acPropertySource != null) && (acPropertySource is IACPropertyNetSource))
                                            {
                                                Type typeTSource = acPropertySource.GetType().GetGenericArguments()[0];
                                                bool needsConverter = propertyBinding.AreUnitConversionParamsSet || typeTSource != typeT;
                                                Type acPropertyType = needsConverter ? typeof(ACPropertyNetTargetConverter<,>) : typeof(ACPropertyNetTarget<>);
                                                if (!needsConverter)
                                                {
                                                    genericType = ACClassProperty.GetGenericACPropertyType(acPropertyType, typeT);
                                                    acProperty = (IACPropertyNetBase)Activator.CreateInstance(genericType, new Object[] { acPropertySource, this, acClassProperty });
                                                }
                                                else
                                                {
                                                    genericType = ACClassProperty.GetConvertibleACPropertyType(acPropertyType, typeT, typeTSource);
                                                    acProperty = (IACPropertyNetBase)Activator.CreateInstance(genericType, new Object[] { acPropertySource, this, acClassProperty, propertyBinding });
                                                }
                                                acProperty.ACInit(Global.ACStartTypes.Automatic);
                                                isMemberLive = true;
                                            }
                                        }
                                        // Falls Objekt nicht gefunden, dann ist Root-Knoten des entsprechenden Models nicht geladen => 
                                        // Das PropertyBinding muss später stattfinden
                                        // => Models sollten am besten in einer bestimmten Reiheinfolge geladen werden
                                        if (!isMemberLive)
                                        {
                                            bool needsConverter = propertyBinding.AreUnitConversionParamsSet || propertyBinding.SourceACClassProperty.ObjectType != typeT;
                                            Type acPropertyType = needsConverter ? typeof(ACPropertyNetTargetConverter<,>) : typeof(ACPropertyNetTarget<>);
                                            if (!needsConverter)
                                            {
                                                genericType = ACClassProperty.GetGenericACPropertyType(acPropertyType, typeT);
                                                // Ohne Source-Objekt instantieren, aber später Binden
                                                acProperty = (IACPropertyNetBase)Activator.CreateInstance(genericType, new Object[] { this, acClassProperty });
                                            }
                                            else
                                            {
                                                genericType = ACClassProperty.GetConvertibleACPropertyType(acPropertyType, typeT, propertyBinding.SourceACClassProperty.ObjectType);
                                                // Ohne Source-Objekt instantieren, aber später Binden
                                                acProperty = (IACPropertyNetBase)Activator.CreateInstance(genericType, new Object[] { this, acClassProperty, propertyBinding });
                                            }
                                            acProperty.ACInit();
                                            // Merke, dass Source-Objekt bei Instanzierung das Late-Binding herstellen soll
                                            propertyBinding.LateBindingNeedDuringACInit = true;
                                            _LateBindingNeedAtACPostInit = true;
                                        }
                                    }
                                    else
                                    {
                                        genericType = ACClassProperty.GetGenericACPropertyType(typeof(ACPropertyNetTarget<>), typeT);
                                        acProperty = (IACPropertyNetBase)Activator.CreateInstance(genericType, new Object[] { this, acClassProperty });
                                        acProperty.ACInit();
                                    }
                                }
                                else
                                    continue;
                            }
                        }
                        else // Falls IsProxy
                        {
                            if (!acClassProperty.IsBroadcast)
                            {
                                genericType = ACClassProperty.GetGenericACPropertyType(typeof(ACProperty<>), typeT);
                                acProperty = (IACPropertyBase)Activator.CreateInstance(genericType, new Object[] { this, acClassProperty, true });
                                acProperty.ACInit();
                            }
                            else
                            {
                                if (Root.HasACModelServer && (!acClassProperty.IsProxyProperty))
                                {
                                    genericType = ACClassProperty.GetGenericACPropertyType(typeof(ACPropertyNetSource<>), typeT);
                                    acProperty = (IACPropertyNetBase)Activator.CreateInstance(genericType, new Object[] { this, acClassProperty, true });
                                    acProperty.ACInit();
                                    BindPropertyToSource(acProperty as IACPropertyNetSource, targetsBound, PropBindingMode.BindOnly);
                                }
                                else
                                {
                                    genericType = ACClassProperty.GetGenericACPropertyType(typeof(ACPropertyNet<>), typeT);
                                    acProperty = (IACPropertyBase)Activator.CreateInstance(genericType, new Object[] { this, acClassProperty });
                                    acProperty.ACInit();
                                }
                            }
                        }
                    }
                    // Falls Property nicht vorher in Member-Liste war, füge hinzu
                    if (acMember == null)
                    {
                        AddOrReplaceACMember(acProperty);
                    }
                }
            }
            if (targetsBound.Any())
                targetsBound.ForEach(c => c.BindPropertyToSource(null, PropBindingMode.BroadcastOnly));

            if (this.IsProxy)
            {
                if ((InitState == ACInitState.RecycledFromPool) || (InitState == ACInitState.Reloading))
                    SubscribeAtServer(true);
                else
                    SubscribeAtServer();
            }
        }

        private void BindPropertyToSource(IACPropertyNetSource acProperty, List<IACPropertyNetTarget> targetsBound, PropBindingMode bindingMode)
        {
            // Finde heraus ob Target-Properties zuvor angelegt wurden, die noch gebunden werden müssen, weil zu dem Zeitpunkt
            // dieses Source-Objekt noch nicht existierte
            if ((acProperty.ACType as ACClassProperty).TopBaseACClassProperty.GetMySourceBindingList(this.ACType as ACClass).Any())
            {
                try
                {
                    IEnumerable<ACClassPropertyRelation> listToBind = (acProperty.ACType as ACClassProperty).TopBaseACClassProperty.GetMySourceBindingList(this.ACType as ACClass).Where(c => !c.LateBindingNeedDuringACInit);
                    if (listToBind != null)
                    {
                        foreach (ACClassPropertyRelation propertyBinding in listToBind)
                        {
                            IACComponent targetACComponent = (IACComponent)ACUrlCommand("?" + propertyBinding.TargetACClass.GetACUrlComponent());
                            if (targetACComponent != null)
                            {
                                IACPropertyNetBase acPropertyTarget = targetACComponent.GetMember("?" + propertyBinding.TargetACClassProperty.ACIdentifier) as IACPropertyNetBase;
                                if (acPropertyTarget != null)
                                {
                                    if (acPropertyTarget is IACPropertyNetTarget)
                                    {
                                        (acPropertyTarget as IACPropertyNetTarget).BindPropertyToSource(acProperty as IACPropertyNetSource, bindingMode);
                                        targetsBound.Add(acPropertyTarget as IACPropertyNetTarget);
                                        propertyBinding.LateBindingNeedDuringACInit = false;
                                    }
                                    else
                                    {
                                        propertyBinding.LateBindingNeedDuringACInit = true;
                                        _LateBindingNeedAtACPostInit = true;
                                    }
                                }
                                else
                                {
                                    propertyBinding.LateBindingNeedDuringACInit = true;
                                    _LateBindingNeedAtACPostInit = true;
                                }
                            }
                            else
                            {
                                propertyBinding.LateBindingNeedDuringACInit = true;
                                _LateBindingNeedAtACPostInit = true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponent", "BindPropertyToSource", msg);
                }
            }
        }

        private void ACPostInitACProperties()
        {
            if (!_LateBindingNeedAtACPostInit)
            {
                ACPostInitACPoints();
                return;
            }

            IACPropertyNetServer[] serverProperties = null;

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                try
                {
                    serverProperties = ACMemberList.Where(c => (c.ACType != null) && (c is IACPropertyNetServer)).Select(c => c as IACPropertyNetServer).ToArray();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponent", "ACPostInitACProperties", msg);
                }
            }
            if (serverProperties != null && serverProperties.Any())
            {
                List<IACPropertyNetTarget> targetsBound = new List<IACPropertyNetTarget>();
                foreach (IACPropertyNetServer acPropertyNet in serverProperties)
                {
                    ACClassProperty acClassProperty = acPropertyNet.ACType as ACClassProperty;
                    if (acPropertyNet is IACPropertyNetSource)
                    {
                        if (acClassProperty.TopBaseACClassProperty.GetMySourceBindingList(this.ACType as ACClass).Any())
                        {
                            try
                            {
                                IEnumerable<ACClassPropertyRelation> listToBind = acClassProperty.TopBaseACClassProperty.GetMySourceBindingList(this.ACType as ACClass).Where(c => c.LateBindingNeedDuringACInit);
                                if (listToBind != null)
                                {
                                    foreach (ACClassPropertyRelation propertyBinding in listToBind)
                                    {
                                        IACComponent targetACComponent = ACUrlCommand(propertyBinding.TargetACClass.GetACUrlComponent()) as IACComponent;
                                        if (targetACComponent != null)
                                        {
                                            IACPropertyNetBase acPropertyTarget = targetACComponent.GetMember(propertyBinding.TargetACClassProperty.ACIdentifier) as IACPropertyNetBase;
                                            if (acPropertyTarget != null)
                                            {
                                                if (acPropertyTarget is IACPropertyNetTarget)
                                                {
                                                    (acPropertyTarget as IACPropertyNetTarget).BindPropertyToSource(acPropertyNet as IACPropertyNetSource, PropBindingMode.BindOnly);
                                                    targetsBound.Add(acPropertyTarget as IACPropertyNetTarget);
                                                    propertyBinding.LateBindingNeedDuringACInit = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                Messages.LogException("ACComponent", "ACPostInitACProperties(10)", msg);
                            }
                        }
                    }
                    else if (acPropertyNet is IACPropertyNetTarget)
                    {
                        if ((acPropertyNet as IACPropertyNetTarget).Source != null)
                            continue;
                        ACClassPropertyRelation propertyBinding = acClassProperty.TopBaseACClassProperty.GetMyPropertyBindingToSource(this.ACType as ACClass);

                        if (propertyBinding != null)// -> Property ist ein Target und hat einen Source-Wert
                        {
                            if (propertyBinding.SourceACClass == null || propertyBinding.SourceACClassProperty == null || propertyBinding.SourceACClassProperty.ObjectType == null)
                            {
                                // ErrorMeldung
                                continue;
                            }

                            //bool isMemberLive = false;
                            // Finde Objekt das die Source-Property beeinhaltet
                            IACComponent sourceACComponent = (IACComponent)ACUrlCommand("?" + propertyBinding.SourceACClass.GetACUrlComponent(), null);
                            if (sourceACComponent != null)
                            {
                                IACMember acPropertySource = sourceACComponent.GetMember(propertyBinding.SourceACClassProperty.ACIdentifier);
                                if ((acPropertySource != null) && (acPropertySource is IACPropertyNetSource))
                                {
                                    (acPropertyNet as IACPropertyNetTarget).BindPropertyToSource(acPropertySource as IACPropertyNetSource, PropBindingMode.BindOnly);
                                    targetsBound.Add(acPropertyNet as IACPropertyNetTarget); 
                                    propertyBinding.LateBindingNeedDuringACInit = false;
                                }
                            }
                        }
                    }
                }
                if (targetsBound.Any())
                    targetsBound.ForEach(c => c.BindPropertyToSource(null, PropBindingMode.BroadcastOnly));
            }
            _LateBindingNeedAtACPostInit = false;
            ACPostInitACPoints();
        }


        private void ACDeInitACProperties(bool deleteACClassTask = false)
        {
            if (this.IsProxy)
            {
                UnSubscribeAtServer();
            }

            var list = this.ACPropertyList;
            if (list != null)
            {
                foreach (IACPropertyBase member in list)
                {
                    member.ACDeInit(deleteACClassTask);
                }

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    foreach (IACPropertyBase member in list)
                    {
                        // Wäre nur notwendig beim Heruntefahren
                        if (InitState == ACInitState.Destructing)
                            ACMemberList.Remove(member);
                    }
                }
            }

            ACDeInitACPoints(deleteACClassTask);
        }

        /// <summary>
        /// If this component is a proxy-object, than a message is send to the real instance on server side, to resend all property-values again.
        /// </summary>
        public void ReSubscribe()
        {
            PropertiesReceived = false;
            if (this.IsProxy)
            {
                SubscribeAtServer(true);
            }
            foreach (ACComponent child in this.ACComponentChilds)
            {
                child.ReSubscribe();
            }
        }

        internal void InformObjectsOnDisconnect()
        {
            PropertiesReceived = false;
            foreach (ACComponent child in this.ACComponentChilds)
            {
                child.InformObjectsOnDisconnect();
            }
        }

        private string _Comment;

        /// <summary>
        /// THREAD-SAFE: Gets or sets the comment of this type
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        [ACPropertyInfo(9999,"","en{'Comment'}de{'Kommentar'}","",true)]
        public string Comment
        {
            get 
            {
                if (_Comment == null)
                    _Comment = ACType.Comment;
                return _Comment;
            }
            set
            {
                _Comment = value;
                if (ACTypeFromLiveContext == null)
                    return;
                ACClassTaskQueue.TaskQueue.Add(() =>
                {
                    ACTypeFromLiveContext.Comment = _Comment;
                });
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method sends a PropertyValueEvent from this Client/Proxy-Object
        /// to the Real Object on Server-side
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        internal void SendPropertyValue(IACPropertyNetValueEvent eventArgs)
        {
            if ((Root == null) || (!IsProxy) || InitState != ACInitState.Initialized)
                return;
            Root.SendPropertyValue(eventArgs, this);
        }

        /// <summary>
        /// Method sends a PropertyValueEvent from this Real/Server-Object
        /// to all Proxy-Object which has subscribed it
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        internal void BroadcastPropertyValue(IACPropertyNetValueEvent eventArgs)
        {
            if (Root == null || this.IsProxy)
                return;
            Root.BroadcastPropertyValue(eventArgs, this);
        }

        internal virtual IEnumerable<IACPropertyNetValueEvent> GetPropertyValuesAsEvents(WCFServiceChannel forChannel)
        {
            List<IACPropertyNetValueEvent> propertyValueEvents = new List<IACPropertyNetValueEvent>();

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                foreach (IACPropertyNetBase property in ACMemberList.Where(c => c is IACPropertyNetBase).Select(c => c as IACPropertyNetBase))
                {
                    IACPropertyNetValueEvent valueEvent = property.GetValueAsEvent();
                    if (valueEvent != null)
                        propertyValueEvents.Add(valueEvent);
                }
            }
            return propertyValueEvents;
        }

        /// <summary>
        /// This method returns the archived or historical values ​​of a property as a time series. 
        /// In SCADA systems, it is also called a historian. 
        /// If the result is empty, then no historical data was recorded because the user did not enable archiving in the iplus engineering environment by setting the refresh rate in the LogRefreshRate property of the table/entity object ACClassProperty. 
        /// If necessary, also search for other suitable property names defined in the base classes that are not immediately recognizable in the context of the derived class. For example ActualValue auf an analog sensor is the gross weight of gravimetric scales.
        /// </summary>
        /// <param name="propertyName">ACIdentifier of the property that should be queried</param>
        /// <param name="from">Filter time from</param>
        /// <param name="to">Filter time to</param>
        /// <returns>PropertyLogListInfo.</returns>
        [ACMethodInfo("ACComponent", "en{'Get archived values (historical data)'}de{'Lese archivierte Werte (Historische Daten)'}", 9999, 
            Description = @"This method returns the archived or historical values ​​of a property as a time series. 
            In SCADA systems, it is also called a historian. 
            If the result is empty, then no historical data was recorded because the user did not enable archiving in the iplus engineering environment by setting the refresh rate in the LogRefreshRate property of the table/entity object ACClassProperty. 
            If necessary, also search for other suitable property names defined in the base classes that are not immediately recognizable in the context of the derived class. For example ActualValue auf an analog sensor is the gross weight of gravimetric scales.")]
        public virtual PropertyLogListInfo GetArchiveLog(string propertyName, DateTime from, DateTime to)
        {
            IACMember member = this.GetMember(propertyName);
            if (member == null)
                return null;
            if (member is IACPropertyNetServer)
                return (member as IACPropertyNetBase).GetArchiveLog(from, to);
            return null;
        }

        /// <summary>
        /// Restores the stored value from the database into the persistable target properties
        /// </summary>
        /// <param name="onlyUnbound">if set to <c>true</c> [only unbound].</param>
        [ACMethodInfo("ACComponent", "en{'Restore target prop.'}de{'Rückspeichern target properties'}", 9999)]
        public virtual void RestoreTargetProp(bool onlyUnbound = true)
        {
            foreach (IACComponent child in ACComponentChilds)
            {
                (child as ACComponent).RestoreTargetProp(onlyUnbound);
            }

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                try
                {
                    var query = ACMemberList.Where(c => c is IACPropertyNetTarget);
                    if (query.Any())
                    {
                        foreach (IACPropertyNetTarget targetProp in query)
                        {
                            if (onlyUnbound && (targetProp.Source != null))
                                continue;
                            targetProp.ReStoreFromDB(false);
                        }
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ACComponent", "RestoreTargetProp", msg);
                }
            }
        }

        /// <summary>Gets the Control-Mode for a WPF-Control (that implements IVBContent).
        /// This method validates the the bound value according the Min-/Max settings in the iplus development Environment for a property.
        /// It resolves the VBContent an invokes CheckPropertyMinMax(ACClassProperty acClassProperty, object valueToCheck, IACObject parentObject).
        /// It's called inside the method GetControlModes(IVBContent vbControl).</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent.</param>
        /// <returns>ControlModesInfo</returns>
        public virtual Global.ControlModesInfo CheckPropertyMinMax(IVBContent vbControl)
        {
            if ((vbControl.ContextACObject == null || vbControl.VBContentPropertyInfo == null))
                return Global.ControlModesInfo.Enabled;

            object value = null;
            IACObject parentObject = null;
            if (String.IsNullOrEmpty(vbControl.VBContent))
                value = this;
            else
            {
                int urlParentPos = vbControl.VBContent.LastIndexOf('\\');
                if (urlParentPos > 0)
                {
                    String urlParent = vbControl.VBContent.Substring(0, urlParentPos);
                    parentObject = vbControl.ContextACObject.ACUrlCommand(urlParent) as IACObject;
                }
                value = vbControl.ContextACObject.ACUrlCommand(vbControl.VBContent);
            }

            return CheckPropertyMinMax(vbControl.VBContentPropertyInfo, value, parentObject);
        }


        /// <summary>Validates a passed value according the Min-/Max settings in the iplus development Environment for a property.</summary>
        /// <param name="acClassProperty">Metadata about the property to check.</param>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="parentObject">The parent object is the object where this value belongs to. It's either a IACPropertyBase or a Entity-Framework-Object.</param>
        /// <returns>ControlModesInfo</returns>
        public virtual Global.ControlModesInfo CheckPropertyMinMax(ACClassProperty acClassProperty, object valueToCheck, IACObject parentObject)
        {
            return IACObjectReflectionExtension.CheckPropertyMinMax(acClassProperty, valueToCheck, parentObject, acClassProperty.ObjectFullType, gip.core.datamodel.Database.GlobalDatabase);
        }


        #endregion

        #region Diagnostics and Dump        
        /// <summary>
        /// Override this method to add private fields to the XMLDump for Diagnostics.
        /// First call base.DumpPropertyList() an then add you values to the xmlACPropertyList via AppendChild        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xmlACPropertyList"></param>
        /// <param name="dumpStats"></param>
        protected virtual void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList, ref DumpStats dumpStats)
        {
            Type typeEnumerable = typeof(IEnumerable);
            foreach (IACPropertyBase property in ACDiagPropertyList)
            {
                XmlElement xmlProperty = doc.CreateElement(property.ACIdentifier);
                string xmlValue = property.ValueSerialized(true);
                if (xmlValue == null)
                {
                    if (property.Value != null)
                    {
                        xmlValue = ACConvert.ObjectToXML(property.Value, true, true);
                        if (String.IsNullOrEmpty(xmlValue))
                        {
                            xmlValue = property.Value.ToString();
                        }
                    }
                }
                if (!String.IsNullOrEmpty(xmlValue))
                {
//#if DEBUG
//                    IACPropertyNetSource sourcePropery = property as IACPropertyNetSource;
//                    if (sourcePropery != null)
//                        xmlValue = String.Format("{0}({1})", xmlValue, sourcePropery.ChangeCounter);
//#endif
                    xmlProperty.InnerText = xmlValue;
                }
                xmlACPropertyList.AppendChild(xmlProperty);
                if (property is IACPropertyNetSource)
                    dumpStats.Bindings += (property as IACPropertyNetSource).Targets != null ? (property as IACPropertyNetSource).Targets.Count : 0;
            }

            XmlElement xmlInitState = xmlACPropertyList["InitState"];
            if (xmlInitState == null)
            {
                xmlInitState = doc.CreateElement("InitState");
                xmlInitState.InnerText = InitState.ToString();
                xmlACPropertyList.AppendChild(xmlInitState);
            }

            XmlElement xmlACOperationMode = xmlACPropertyList["ACOperationMode"];
            if (xmlACOperationMode == null)
            {
                xmlACOperationMode = doc.CreateElement("ACOperationMode");
                xmlACOperationMode.InnerText = ACOperationMode.ToString();
                xmlACPropertyList.AppendChild(xmlACOperationMode);
            }

            XmlElement xmlContentTask = xmlACPropertyList["ContentTask"];
            if (xmlContentTask == null)
            {
                xmlContentTask = doc.CreateElement("ContentTask");
                if (ContentTask != null)
                    xmlContentTask.InnerText = ACConvert.ObjectToXML(ContentTask, true, true);
                xmlACPropertyList.AppendChild(xmlContentTask);
            }

            List<IACPropertyConfigValue> acPropertyConfigValueList = null;
            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (ACPropertyConfigValueList != null)
                    acPropertyConfigValueList = ACPropertyConfigValueList.ToList();
            }
            if (acPropertyConfigValueList != null && acPropertyConfigValueList.Any())
            {
                foreach (IACPropertyConfigValue configValue in acPropertyConfigValueList)
                {
                    string xmlConfiguredValue = null;
                    if (configValue.Value != null)
                    {
                        xmlConfiguredValue = ACConvert.ObjectToXML(configValue.Value, true, true);
                        if (String.IsNullOrEmpty(xmlConfiguredValue))
                        {
                            xmlConfiguredValue = configValue.Value.ToString();
                        }
                    }

                    string xmlValue = null;
                    try
                    {
                        PropertyInfo propertyInfo = this.GetType().GetProperty(configValue.ACIdentifier);
                        if (propertyInfo != null)
                        {
                            object value = propertyInfo.GetValue(this);
                            if (value != null)
                            {
                                xmlValue = ACConvert.ObjectToXML(value, true, true);
                                if (String.IsNullOrEmpty(xmlValue))
                                {
                                    xmlValue = value.ToString();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Messages.LogException(this.GetACUrl(), "DumpPropertyList(10)", ex.Message);
                    }

                    if (xmlConfiguredValue == null)
                        xmlConfiguredValue = "null";
                    if (xmlValue == null)
                        xmlValue = "null";

                    XmlElement xmlProperty = doc.CreateElement(configValue.ACIdentifier);
                    xmlProperty.InnerText = xmlValue;
                    xmlACPropertyList.AppendChild(xmlProperty);

                    xmlProperty = doc.CreateElement(configValue.ACIdentifier + "_Config");
                    xmlProperty.InnerText = xmlConfiguredValue;
                    xmlACPropertyList.AppendChild(xmlProperty);
                }
            }
        }
#endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;
using System.Transactions;
using System.Xml;
using System.Windows.Input;
using System.Collections.Concurrent;

namespace gip.core.autocomponent
{
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAAlarmChangeState'}de{'PAAlarmChangeState'}", Global.ACKinds.TACEnum)]
    public enum PAAlarmChangeState : short
    {
        NoChange = 0,
        NewAlarmOccurred = 1,
        AlarmDisappeared = 2,
    }

    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PANotifyState'}de{'PANotifyState'}", Global.ACKinds.TACEnum)]
    public enum PANotifyState : short
    {
        Off = 0,
        InfoOrActive = 1,
        AlarmOrFault = 2,
    }


    /// <summary>
    /// Baseclass for ACComponents, which has an alarming behaviour
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.ACComponent" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAClassAlarmingBase'}de{'PAClassAlarmingBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAClassAlarmingBase : ACComponent
    {

        private static Dictionary<string, ACEventArgs> _SVirtualEventArgs;

        private IACComponent _Session = null;

        #region Properties

        public static Dictionary<string, ACEventArgs> SVirtualEventArgs
        {
            get { return _SVirtualEventArgs; }
        }

        public virtual Dictionary<string, ACEventArgs> VirtualEventArgs
        {
            get
            {
                return SVirtualEventArgs;
            }
        }

        #endregion

        #region Constructors

        static PAClassAlarmingBase()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string,ACEventArgs>(StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("PropertyName", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue(Const.Value, typeof(Msg), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("AlarmChangedEvent", TMP);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("PropertyName", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue(Const.Value, typeof(Msg), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("SubAlarmsChangedEvent", TMP);
        }

        public PAClassAlarmingBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _AlarmChangedEvent = new ACPointEvent(this, "AlarmChangedEvent", 0);
            _SubAlarmsChangedEvent = new ACPointEvent(this, "SubAlarmsChangedEvent", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Public

        #endregion

        #region Points and Events

        /// <summary>
        /// Gets the reference to the root component if this instance is in a application project
        /// </summary>
        public ApplicationManager ApplicationManager
        {
            get
            {
                if (this is ApplicationManager)
                    return this as ApplicationManager;
                return FindParentComponent<ApplicationManager>(c => c is ApplicationManager);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance runs in simulation mode and the states will be changed automatically by the simulatorlogic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is simulation on; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSimulationOn
        {
            get
            {
                if (ACOperationMode != ACOperationModes.Live)
                    return true;
                if (ApplicationManager == null)
                    return false;
                return ApplicationManager.IsSimulationOn;
            }
        }

        /// <summary>
        /// Gets the reference to the routing service if this instance is in a application project
        /// </summary>
        public virtual ACComponent RoutingService
        {
            get
            {
                if (ApplicationManager != null)
                    return ApplicationManager.RoutingServiceInstance;
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has access to the routing service.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is routing service available; otherwise, <c>false</c>.
        /// </value>
        public bool IsRoutingServiceAvailable
        {
            get
            {
                return RoutingService != null && RoutingService.ConnectionState != ACObjectConnectionState.DisConnected;
            }
        }



        protected ACPointEvent _AlarmChangedEvent;
        /// <summary>
        /// A event point where subscribers are informed if a new alarm has occured or disappeared on this instance
        /// </summary>
        /// <value>
        /// The alarm changed event.
        /// </value>
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent AlarmChangedEvent
        {
            get
            {
                return _AlarmChangedEvent;
            }
            set
            {
                _AlarmChangedEvent = value;
            }
        }


        /// <summary>
        /// Raises the <see cref="E:SubAlarmChanged" /> event.
        /// </summary>
        /// <param name="events">The <see cref="ACEventArgs"/> instance containing the event data.</param>
        /// <param name="childHasAlarms">if set to <c>true</c> [child has alarms].</param>
        protected virtual void OnSubAlarmChanged(ACEventArgs events, bool childHasAlarms)
        {
            SubAlarmsChangedEvent.Raise(events);
            RefreshHasAlarms(childHasAlarms);
        }


        protected ACPointEvent _SubAlarmsChangedEvent;
        /// <summary>
        /// A event point where subscribers are informed if a new alarm has occured or disappeared on one of the child instances
        /// </summary>
        /// <value>
        /// The alarm changed event.
        /// </value>
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent SubAlarmsChangedEvent
        {
            get
            {
                return _SubAlarmsChangedEvent;
            }
            set
            {
                _SubAlarmsChangedEvent = value;
            }
        }

#endregion

        #region Properties, Range 100
        private ConcurrentDictionary<string, Msg> _LocalAlarms = null;

        protected class AlarmTransEntry
        {
            public AlarmTransEntry(string notifyStatePropName, string messageText, bool autoAckForNewAlarm)
            {
                NotifyStatePropName = notifyStatePropName;
                MessageText = messageText;
                AutoAckForNewAlarm = autoAckForNewAlarm;
            }

            public AlarmTransEntry(string notifyStatePropName, Msg innerMessage, bool autoAckForNewAlarm)
            {
                NotifyStatePropName = notifyStatePropName;
                MessageText = innerMessage.Message;
                AutoAckForNewAlarm = autoAckForNewAlarm;
                InnerMessage = innerMessage;
            }

            public string NotifyStatePropName { get; set; }
            public string MessageText { get; set; }
            public bool AutoAckForNewAlarm { get; set; }
            public Msg InnerMessage { get; set; }
        }
        private object _TransLock = new object();
        /// <summary>
        /// List for collecting more Alarms in one Transaction before firing a Alarm
        /// </summary>
        private List<AlarmTransEntry> _TransEntryList = null;

        /// <summary>
        /// Gets a value indicating whether this instance has local alarms.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has local alarms; otherwise, <c>false</c>.
        /// </value>
        public bool HasLocalAlarms
        {
            get
            {
                return _LocalAlarms != null && _LocalAlarms.Any();
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance has unacknowledged local alarms.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has unack local alarms; otherwise, <c>false</c>.
        /// </value>
        public bool HasUnackLocalAlarms
        {
            get
            {
                return _LocalAlarms != null && _LocalAlarms.Any() && _LocalAlarms.Where(c => !c.Value.IsAcknowledged).Any();
            }
        }


        /// <summary>
        /// Returns a reference to a "Source-Component" where the IsSessionConnected-Property of this or a parent instance is bound to the "Source-Property" of the "Source-Component" (Session).
        /// In most cases a derivation of ACSession is returned.
        /// </summary>
        /// <value>
        /// The session
        /// </value>
        public IACComponent Session
        {
            get
            {
                if (_Session == null)
                {
                    if ((this.IsSessionConnected as IACPropertyNetTarget).Source != null)
                    {
                        if (typeof(IACCommSession).IsAssignableFrom((this.IsSessionConnected as IACPropertyNetTarget).Source.ParentACComponent.ACType.ObjectFullType))
                            _Session = (this.IsSessionConnected as IACPropertyNetTarget).Source.ParentACComponent;
                    }
                    else
                    {
                        PAClassAlarmingBase Item = FindParentComponent<PAClassAlarmingBase>(c => c is PAClassAlarmingBase);
                        if (Item != null)
                            return Item.Session;
                    }
                }

                return _Session;
            }
        }

        /// <summary>
        /// This "Target-Property" is used to make a binding to a "Source-Property" ACSession-Component.
        /// This ACSession-Instance can be Acessed via the Session-Property. All childs in the subtree are automatically  assigned to this ACSession-Instance.
        /// </summary>
        /// <value>
        /// The is session connected.
        /// </value>
        [ACPropertyBindingTarget(101, "Values", "en{'Is session connected'}", "", false, false, RemotePropID = 1)]
        public IACContainerTNet<bool> IsSessionConnected { get; set; }


        /// <summary>
        /// A network-Property, that signals if this or any child-component has an active alarm.
        /// </summary>
        /// <value>
        /// The has alarms.
        /// </value>
        [ACPropertyBindingSource(102, "Values", "en{'Has alarms'}de{'Hat alarme'}", "", false, false)]
        public IACContainerTNet<bool> HasAlarms { get; set; }

        /// <summary>
        /// A network-Property, that concatenates all message-texts from this and any child-component that has an active alarm.
        /// </summary>
        /// <value>
        /// The alarms as text.
        /// </value>
        [ACPropertyBindingSource(103, "Values", "en{'Alarms as text'}de{'Alarme als Text'}","",false,false)]
        public IACContainerTNet<string> AlarmsAsText
        {
            get;set;
        }

        #endregion

        #region Execute override

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(AcknowledgeAlarms):
                    AcknowledgeAlarms();
                    return true;
                case nameof(AcknowledgeAllAlarms):
                    AcknowledgeAllAlarms();
                    return true;
                case nameof(AcknowledgeSubAlarms):
                    AcknowledgeSubAlarms();
                    return true;
                case nameof(IsEnabledAcknowledgeAlarms):
                    result = IsEnabledAcknowledgeAlarms();
                    return true;
                case nameof(IsEnabledAcknowledgeAllAlarms):
                    result = IsEnabledAcknowledgeAllAlarms();
                    return true;
                case nameof(IsEnabledAcknowledgeSubAlarms):
                    result = IsEnabledAcknowledgeSubAlarms();
                    return true;
                case nameof(FilterAlarm):
                    result = FilterAlarm(acParameter[0] as string, acParameter[1] as string, (bool) acParameter[2], acParameter[3] != null ? acParameter[3] as Msg : null);
                    return true;
                case nameof(GetAlarms):
                    result = GetAlarms((bool)acParameter[0], (bool)acParameter[1], (bool)acParameter[2]);
                    return true;
                case nameof(GetAlarmsConfig):
                    result = GetAlarmsConfig((bool)acParameter[0], (bool)acParameter[1], (bool)acParameter[2]);
                    return true;
                case nameof(AcknowledgeSubAlarmsMsgList):
                    AcknowledgeSubAlarmsMsgList(acParameter[0] as MsgList);
                    return true;
                case nameof(BackupState):
                    BackupState();
                    return true;
                case nameof(IsEnabledBackupState):
                    result = IsEnabledBackupState();
                    return true;
                case nameof(ClearBackupState):
                    ClearBackupState();
                    return true;
                case nameof(IsEnabledClearBackupState):
                    result = IsEnabledClearBackupState();
                    return true;
                case nameof(RestoreBackupedState):
                    RestoreBackupedState();
                    return true;
                case nameof(IsEnabledRestoreBackupedState):
                    result = IsEnabledRestoreBackupedState();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #region Alarm-Handling Methods for Subclasses        
        /// <summary>
        /// Use the OnNewAlarmOccurred() method to add a new entry to the alarm list. 
        /// If an unacknowledged alarm with the same message text already exists, no new entry is created. 
        /// If the message text is different, the existing message will be replaced with the new message.  
        /// </summary>
        /// <param name="notifyStatePropName">Name of the property that signals the alarm</param>
        /// <param name="messageText">The message text.</param>
        /// <param name="autoAckForNewAlarm">if set to <c>true</c> [automatic ack for new alarm].</param>
        /// <returns></returns>
        public Msg OnNewAlarmOccurred(string notifyStatePropName, string messageText = "", bool autoAckForNewAlarm = false)
        {
            IACPropertyBase property = GetProperty(notifyStatePropName);
            if (property == null)
                return null;
            return OnNewAlarmOccurred(property, messageText, autoAckForNewAlarm);
        }


        /// <summary>
        /// Use the OnNewAlarmOccurred() method to add a new entry to the alarm list. 
        /// If an unacknowledged alarm with the same message text already exists, no new entry is created. 
        /// If the message text is different, the existing message will be replaced with the new message.  
        /// </summary>
        /// <param name="property">A reference to the property that signals the alarm</param>
        /// <param name="messageText">The message text.</param>
        /// <param name="autoAckForNewAlarm">if set to <c>true</c> [automatic ack for new alarm].</param>
        /// <returns></returns>
        public Msg OnNewAlarmOccurred(IACPropertyBase property, string messageText = "", bool autoAckForNewAlarm = false)
        {
            if (property == null || property.ParentACComponent != this)
                return null;

            Msg newAlarm = null;
            try
            {
                newAlarm = new Msg(this.GetACUrl(), property.ACIdentifier) { SourceComponent = new ACRef<IACComponent>(property.ParentACComponent, true) };
                newAlarm.Message = messageText;
                newAlarm.MessageLevel = eMsgLevel.Default;

                bool send = (bool)ACUrlCommand("!FilterAlarm", property.ACIdentifier, messageText, autoAckForNewAlarm, newAlarm);
                if (!send)
                    return null;
                lock (_TransLock)
                {
                    if (_TransEntryList != null)
                    {
                        _TransEntryList.Add(new AlarmTransEntry(property.ACIdentifier, messageText, autoAckForNewAlarm));
                        return new Msg("AlarmTransEntry", property.ACIdentifier);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("PAClassAlarmingBase", "OnNewAlarmOccurred", msg);
            }
            return AddNewAlarm(property, messageText, autoAckForNewAlarm, newAlarm);
        }


        /// <summary>
        /// Use the OnNewAlarmOccurred() method to add a new entry to the alarm list. 
        /// If an unacknowledged alarm with the same message text already exists, no new entry is created. 
        /// If the message text is different, the existing message will be replaced with the new message.  
        /// </summary>
        /// <param name="notifyStatePropName">Name of the notify state property.</param>
        /// <param name="msg">A Msg-object</param>
        /// <param name="autoAckForNewAlarm">if set to <c>true</c> [automatic ack for new alarm].</param>
        /// <returns></returns>
        public Msg OnNewAlarmOccurred(string notifyStatePropName, Msg msg, bool autoAckForNewAlarm = false)
        {
            IACPropertyBase property = GetProperty(notifyStatePropName);
            if (property == null)
                return null;
            return OnNewAlarmOccurred(property, msg, autoAckForNewAlarm);
        }


        /// <summary>
        /// Use the OnNewAlarmOccurred() method to add a new entry to the alarm list. 
        /// If an unacknowledged alarm with the same message text already exists, no new entry is created. 
        /// If the message text is different, the existing message will be replaced with the new message.  
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="msg">A Msg-object</param>
        /// <param name="autoAckForNewAlarm">if set to <c>true</c> [automatic ack for new alarm].</param>
        /// <returns></returns>
        public Msg OnNewAlarmOccurred(IACPropertyBase property, Msg msg, bool autoAckForNewAlarm = false)
        {
            if (property == null || property.ParentACComponent != this)
                return null;

            try
            {
                bool send = (bool)ACUrlCommand("!FilterAlarm", property.ACIdentifier, msg.Message, autoAckForNewAlarm, msg);
                if (!send)
                    return null;
            }
            catch (Exception e)
            {
                string msgE = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msgE += " Inner:" + e.InnerException.Message;

                Messages.LogException("PAClassAlarmingBase", "OnNewAlarmOccurred(10)", msgE);
            }

            if (_TransEntryList != null)
            {
                lock (_TransLock)
                {
                    _TransEntryList.Add(new AlarmTransEntry(property.ACIdentifier, msg, autoAckForNewAlarm));
                    return new Msg("AlarmTransEntry", property.ACIdentifier);
                }
            }
            return AddNewAlarm(property, msg.Message, autoAckForNewAlarm, msg);
        }


        /// <summary>
        /// Override this methof if you want to filter new alarms, that should not be added to the alarmlist.
        /// Return false to filter the alarm.
        /// </summary>
        /// <param name="notifyStatePropName">Name of the notify state property.</param>
        /// <param name="messageText">The message text.</param>
        /// <param name="autoAckForNewAlarm">if set to <c>true</c> [automatic ack for new alarm].</param>
        /// <param name="msg">The MSG.</param>
        /// <returns>If false is returned, than the alarm will not be added to the alarmlist.</returns>
        [ACMethodInfo("Function", "en{'FilterAlarm'}de{'FilterAlarm'}", 9999)]
        public virtual bool FilterAlarm(string notifyStatePropName, string messageText, bool autoAckForNewAlarm, Msg msg)
        {
            return true;
        }

        private Msg AddNewAlarm(string notifyStatePropName, string messageText, bool autoAckForNewAlarm, Msg msg = null)
        {
            IACPropertyBase property = GetProperty(notifyStatePropName);
            if (property == null)
                return null;
            return AddNewAlarm(property, messageText, autoAckForNewAlarm, msg);
        }

        private Msg AddNewAlarm(IACPropertyBase property, string messageText, bool autoAckForNewAlarm, Msg msg = null)
        {
            if (property == null || property.ParentACComponent != this)
                return null;

            Msg newAlarm;
            if (_LocalAlarms == null)
                _LocalAlarms = new ConcurrentDictionary<string, Msg>();
            else
            {
                bool hasUnAckAlarm = _LocalAlarms.TryGetValue(property.ACIdentifier, out newAlarm);
                if (hasUnAckAlarm)
                {
                    if (!autoAckForNewAlarm)
                        return newAlarm;
                    else
                    {
                        if (newAlarm.Message == messageText)
                            return newAlarm;
                        AcknowledgeAlarms();

                        // Falls Alarm von subklasse nicht quittiert, weil OnAlarmDisappeared()-Methode nicht aufgerufen worden ist,
                        // dann lösen keinen neuen alarm aus

                        if (_LocalAlarms != null)
                        {
                            hasUnAckAlarm = _LocalAlarms.TryGetValue(property.ACIdentifier, out newAlarm);
                            if (hasUnAckAlarm)
                            {
                                newAlarm.Message = messageText;
                                newAlarm.TimeStampAcknowledged = DateTime.MinValue;
                                if (!this.HasAlarms.ValueT)
                                {
                                    RefreshHasAlarms();
                                }
                                return newAlarm;
                            }
                        }
                        else
                            _LocalAlarms = new ConcurrentDictionary<string, Msg>();
                    }
                }
            }
            newAlarm = new Msg(this.GetACUrl(), property.ACIdentifier);
            newAlarm.Message = messageText;
            if (msg != null)
            {
                newAlarm.Row = msg.Row;
                newAlarm.Column = msg.Column;
                newAlarm.TranslID = msg.TranslID;
                newAlarm.XMLConfig = msg.XMLConfig;
                newAlarm.MessageLevel = msg.MessageLevel;
                newAlarm.SourceComponent = msg.SourceComponent;
            }
            else
                newAlarm.MessageLevel = eMsgLevel.Default;

            ApplicationManager compManager = ApplicationManager;
            if (compManager != null)
            {
                compManager.AppContextQueue.Add(() => {
                try
                {
                    using (Database db = new Database())
                    {
                        MsgAlarmLog newAlarmDB = MsgAlarmLog.NewMsgAlarmLog(db, newAlarm);
                        OnNewMsgAlarmLogCreated(newAlarmDB);
                        db.MsgAlarmLog.AddObject(newAlarmDB);
                        db.ACSaveChanges();
                    }
                }
                catch (Exception e)
                {
                        string msgEc = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msgEc += " Inner:" + e.InnerException.Message;

                        Messages.LogException("PAClassAlarmingBase", "AddNewAlarm", msgEc);
                    }
                });
            }

            _LocalAlarms[property.ACIdentifier] = newAlarm;

            RaiseAlarmEvents(property.ACIdentifier, newAlarm);

            return newAlarm;
        }


        /// <summary>
        /// Determines it a alarm ist currently active.
        /// If you  want to write the alarm message to the log file, call this IsAlarmActive() method beforehand to check whether the same message is still pending. This will prevent the unnecessary and multiple output of the same message in the log file.
        /// </summary>
        /// <param name="notifyStatePropName">Name of the notify state property.</param>
        /// <param name="messageText">The message text.</param>
        /// <returns></returns>
        public Msg IsAlarmActive(string notifyStatePropName, string messageText = "")
        {
            IACPropertyBase property = GetProperty(notifyStatePropName);
            if (property == null)
                return null;

            return IsAlarmActive(property, messageText);
        }


        /// <summary>
        /// Determines it a alarm ist currently active.
        /// If you  want to write the alarm message to the log file, call this IsAlarmActive() method beforehand to check whether the same message is still pending. This will prevent the unnecessary and multiple output of the same message in the log file.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="messageText">The message text.</param>
        /// <returns></returns>
        public Msg IsAlarmActive(IACPropertyBase property, string messageText = "")
        {
            if (property == null || property.ParentACComponent != this)
                return null;

            Msg newAlarm;
            if (_LocalAlarms == null)
                _LocalAlarms = new ConcurrentDictionary<string, Msg>();
            else
            {
                bool hasUnAckAlarm = _LocalAlarms.TryGetValue(property.ACIdentifier, out newAlarm);
                if (hasUnAckAlarm)
                {
                    // IndexOf()-Check is needed if, Messagetext ist build in Transaction over concatenation of more seperate Alarmtextas
                    if ((!String.IsNullOrEmpty(messageText) && (newAlarm.Message == messageText || (!String.IsNullOrEmpty(newAlarm.Message) && newAlarm.Message.IndexOf(messageText) >= 0)))
                        || String.IsNullOrEmpty(messageText))
                        return newAlarm;
                }
            }
            return null;
        }


        /// <summary>
        /// Removes an active Alarm from the alarmlist.
        /// </summary>
        /// <param name="property">The property.</param>
        public void OnAlarmDisappeared(IACPropertyBase property)
        {
            if (property == null)
                return;
            OnAlarmDisappeared(property.ACIdentifier);
        }


        /// <summary>
        /// Removes an active Alarm from the alarmlist.
        /// </summary>
        /// <param name="notifyStatePropName">Name of the notify state property.</param>
        public void OnAlarmDisappeared(string notifyStatePropName)
        {
            if (_LocalAlarms == null)
                return;
            Msg newAlarm;
            if (!_LocalAlarms.TryGetValue(notifyStatePropName, out newAlarm))
                return;
            Msg oldAlarm;
            _LocalAlarms.TryRemove(notifyStatePropName, out oldAlarm);
            //if (_LocalAlarms.Count <= 0)
            //    _LocalAlarms = null;
            if (!newAlarm.IsAcknowledged)
                newAlarm.TimeStampAcknowledged = DateTime.Now;

            RaiseAlarmEvents(notifyStatePropName, newAlarm);
        }


        protected void OnAlarmUpdateMessage(string notifyStatePropName, string messageText)
        {
            if (_LocalAlarms == null)
                return;
            Msg newAlarm;
            if (!_LocalAlarms.TryGetValue(notifyStatePropName, out newAlarm))
                return;
            newAlarm.Message = messageText;
            RaiseAlarmEvents(notifyStatePropName, newAlarm);
        }


        private void RaiseAlarmEvents(object localAlarmKey, Msg forAlarm)
        {
            String propertyName = "Alarm";
            if (localAlarmKey is String)
                propertyName = localAlarmKey as String;

            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("AlarmChangedEvent", VirtualEventArgs);

            eventArgs.GetACValue("PropertyName").Value = propertyName;
            eventArgs.GetACValue(Const.Value).Value = forAlarm;

            AlarmChangedEvent.Raise(eventArgs);
            RefreshHasAlarms();
            bool childHasAlarms = HasAlarms.ValueT;

            PAClassAlarmingBase childComponent = this;
            IACComponent parentComponent = this.ParentACComponent;
            while (parentComponent != null)
            {
                if (parentComponent is PAClassAlarmingBase)
                {
                    if (childComponent != null && !childHasAlarms)
                        childHasAlarms = childComponent.HasAlarms.ValueT;
                    (parentComponent as PAClassAlarmingBase).OnSubAlarmChanged(eventArgs, childHasAlarms);
                    childComponent = parentComponent as PAClassAlarmingBase;
                }
                else
                    childComponent = null;
                parentComponent = parentComponent.ParentACComponent;
            }
        }


        protected virtual void OnNewMsgAlarmLogCreated(MsgAlarmLog newLog)
        {
        }

        /// <summary>
        /// Refreshes the HasAlarms and AlarmsAsText property.
        /// </summary>
        /// <param name="childHasAlarms">The child has alarms.</param>
        /// <param name="resetIfNoUnackAlarms">if set to <c>true</c> [reset if no unack alarms].</param>
        public virtual void RefreshHasAlarms(bool? childHasAlarms = null, bool resetIfNoUnackAlarms = false)
        {
            bool hasAlarms = HasUnackLocalAlarms;
            if (!hasAlarms && resetIfNoUnackAlarms && _LocalAlarms != null && _LocalAlarms.Any())
            {
                _LocalAlarms.Clear();
            }
            if (!hasAlarms && childHasAlarms.HasValue)
                hasAlarms = childHasAlarms.Value;
            else if (!hasAlarms && !childHasAlarms.HasValue)
                hasAlarms = FindChildComponents<PAClassAlarmingBase>(c => c is PAClassAlarmingBase && (c as PAClassAlarmingBase).HasUnackLocalAlarms).Any();
            HasAlarms.ValueT = hasAlarms;
            if (hasAlarms)
            {
                MsgList msgList = GetAlarms(true, true, true);
                if (msgList == null || !msgList.Any())
                    AlarmsAsText.ValueT = "";
                else
                    AlarmsAsText.ValueT = msgList.ToString();
            }
            else
                AlarmsAsText.ValueT = "";
        }


        /// <summary>
        /// Start a new Transaction for adding mupltiple Alarmtexts
        /// </summary>
        protected void BeginAlarmTrans()
        {
            lock (_TransLock)
            {
                if (_TransEntryList != null)
                    return;
                _TransEntryList = new List<AlarmTransEntry>();
            }
        }


        /// <summary>
        /// Commit collected Alarms in _TransEntryList
        /// </summary>
        protected void EndAlarmTrans()
        {
            lock (_TransLock)
            {
                if (_TransEntryList == null)
                    return;
                if (_TransEntryList.Any())
                {
                    foreach (var groupEntry in _TransEntryList.GroupBy(c => c.NotifyStatePropName))
                    {
                        StringBuilder sb = new StringBuilder();
                        AlarmTransEntry lastEntry = null;
                        foreach (AlarmTransEntry entry in groupEntry)
                        {
                            lastEntry = entry;
                            sb.AppendLine(entry.MessageText);
                        }

                        if (lastEntry != null)
                        {
                            AddNewAlarm(lastEntry.NotifyStatePropName, sb.ToString(), lastEntry.AutoAckForNewAlarm);
                        }
                    }
                }
                _TransEntryList = null;
            }
        }
        #endregion

        #region Interaction Methods, Range: 100

        /// <summary>
        /// Acknowledges all alarms that are active only on this instance.
        /// After acknowledging the alarms are remove from the alarmlist.
        /// </summary>
        [ACMethodInteraction("Alarms", "en{'Acknowledge'}de{'Quittieren'}", 100, true)]
        public virtual void AcknowledgeAlarms()
        {
            if (!HasLocalAlarms)
                return;
            bool raised = false;
            foreach (KeyValuePair<string, Msg> kvpAlarm in _LocalAlarms)
            {
                if (!kvpAlarm.Value.IsAcknowledged)
                {
                    kvpAlarm.Value.Acknowledge(Root.CurrentInvokingUser);

                    ApplicationManager compManager = ApplicationManager;
                    if (compManager != null)
                    {
                        compManager.AppContextQueue.Add(() => {
                            try
                            {
                                using (Database db = new Database())
                                {
                                    MsgAlarmLog msgAlarmLog = db.MsgAlarmLog.Where(c => c.MsgAlarmLogID == kvpAlarm.Value.MsgId).FirstOrDefault();
                                    if (msgAlarmLog != null)
                                    {
                                        msgAlarmLog.CopyFromMsg(kvpAlarm.Value);
                                        db.ACSaveChanges();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                Messages.LogException("PAClassAlarmingBase", "AcknowledgeAlarms", msg);
                            }
                        }
                        );
                    }


                    RaiseAlarmEvents(kvpAlarm.Key, kvpAlarm.Value);
                    raised = true;
                }
            }

            if (!raised || !HasUnackLocalAlarms)
                RefreshHasAlarms(null,true);
        }

        public virtual bool IsEnabledAcknowledgeAlarms()
        {
            return HasUnackLocalAlarms;
        }


        /// <summary>
        /// Acknowledges all alarms that are active on this instance and on all child components as well.
        /// After acknowledging the alarms are remove from the alarmlist.
        /// </summary>
        [ACMethodInteraction("Alarms", "en{'Acknowledge all'}de{'Quittieren alle'}", 101, true)]
        public void AcknowledgeAllAlarms()
        {
            AcknowledgeAlarms();
            AcknowledgeSubAlarmsMsgList(GetAlarms(true, true, true));
        }


        public virtual bool IsEnabledAcknowledgeAllAlarms()
        {
            return HasUnackLocalAlarms || GetAlarms(false, true, true).Any();
        }


        /// <summary>
        /// Acknowledges all alarms that are active on any child component.
        /// After acknowledging the alarms are remove from the alarmlist.
        /// </summary>
        [ACMethodInteraction("Alarms", "en{'Acknowledge subalarms'}de{'Quittiere Unteralarme'}", 102, true)]
        public void AcknowledgeSubAlarms()
        {
            AcknowledgeSubAlarmsMsgList(GetAlarms(true, true, true));
        }

        public virtual bool IsEnabledAcknowledgeSubAlarms()
        {
            return CountSubAlarms > 0;
        }


        /// <summary>
        /// Gets the count of sub alarms.
        /// </summary>
        /// <value>
        /// The count sub alarms.
        /// </value>
        protected int CountSubAlarms
        {
            get
            {
                return GetAlarms(false, true, true).Count;
            }
        }
        #endregion

        #region Methods for Clients        
        /// <summary>
        /// Queries this component and sub components for alarms.
        /// </summary>
        /// <param name="thisAlarms">if set to <c>true</c> [this alarms].</param>
        /// <param name="subAlarms">if set to <c>true</c> [sub alarms].</param>
        /// <param name="onlyUnackAlarms">if set to <c>true</c> [only unack alarms].</param>
        /// <returns></returns>
        [ACMethodInfo("Alarms", "en{'GetAlarms'}de{'GetAlarms'}", 9999)]
        public virtual MsgList GetAlarms(bool thisAlarms, bool subAlarms, bool onlyUnackAlarms)
        {
            MsgList alarmListToFill = new MsgList();
            if (onlyUnackAlarms)
            {
                if (thisAlarms && HasUnackLocalAlarms)
                {
                    foreach (KeyValuePair<string, Msg> kvpAlarm in _LocalAlarms.Where(c => !c.Value.IsAcknowledged))
                    {
                        alarmListToFill.Add(kvpAlarm.Value);
                    }
                }
                if (subAlarms)
                {
                    foreach (PAClassAlarmingBase alarmComponent in FindChildComponents<PAClassAlarmingBase>(c => c is PAClassAlarmingBase && (c as PAClassAlarmingBase).HasUnackLocalAlarms))
                    {
                        foreach (KeyValuePair<string, Msg> kvpAlarm2 in alarmComponent._LocalAlarms.Where(c => !c.Value.IsAcknowledged))
                        {
                            alarmListToFill.Add(kvpAlarm2.Value);
                        }
                    }
                }
            }
            else
            {
                if (thisAlarms && HasLocalAlarms)
                {
                    foreach (KeyValuePair<string, Msg> kvpAlarm in _LocalAlarms)
                    {
                        alarmListToFill.Add(kvpAlarm.Value);
                    }
                }
                if (subAlarms)
                {
                    foreach (PAClassAlarmingBase alarmComponent in FindChildComponents<PAClassAlarmingBase>(c => c is PAClassAlarmingBase && (c as PAClassAlarmingBase).HasLocalAlarms))
                    {
                        foreach (KeyValuePair<string, Msg> kvpAlarm2 in alarmComponent._LocalAlarms)
                        {
                            alarmListToFill.Add(kvpAlarm2.Value);
                        }
                    }
                }
            }
            return alarmListToFill;
        }


        /// <summary>
        /// Queries this component and sub components for alarms that are configured in a PAAlarmMessengerBase
        /// </summary>
        /// <param name="thisAlarms">if set to <c>true</c> [this alarms].</param>
        /// <param name="subAlarms">if set to <c>true</c> [sub alarms].</param>
        /// <param name="onlyUnackAlarms">if set to <c>true</c> [only unack alarms].</param>
        /// <returns></returns>
        [ACMethodInfo("Alarms", "en{'GetAlarmsConfig'}de{'GetAlarmsConfig'}", 9999)]
        public virtual MsgList GetAlarmsConfig(bool thisAlarms, bool subAlarms, bool onlyUnackAlarms)
        {
            var result = GetAlarms(thisAlarms, subAlarms, onlyUnackAlarms);
            var alarmMessenger = this.FindChildComponents<PAAlarmMessengerBase>(c => c is PAAlarmMessengerBase).FirstOrDefault();
            foreach (var item in result)
            {
                List<ACRef<ACComponent>> temp = null;
                item.ConfigIconState = alarmMessenger == null ? Global.ConfigIconState.NoConfig : alarmMessenger.CheckAlarmMsgInConfig(item, out temp);
            }
            return result;
        }

        /// <summary>
        /// Acknowledges all alarms that are active on any child component.
        /// After acknowledging the alarms are remove from the alarmlist.
        /// </summary>
        /// <param name="alarmListToAck">The alarm list to ack.</param>
        [ACMethodInfo("Alarms", "en{'Filtered acknowledgment'}de{'Gefilterte Quittierung'}", 9999)]
        public void AcknowledgeSubAlarmsMsgList(MsgList alarmListToAck)
        {
            if (alarmListToAck == null)
                return;
            if (alarmListToAck.Count <= 0)
                return;
            var query = alarmListToAck.OrderBy(c => c.Source);
            string prevSource = "";
            foreach (Msg alarm in query)
            {
                if (prevSource != alarm.Source)
                {
                    PAClassAlarmingBase component2Ack = ACUrlCommand("?" + alarm.Source) as PAClassAlarmingBase;
                    if (component2Ack != null)
                    {
                        component2Ack.AcknowledgeAlarms();
                    }
                }
                prevSource = alarm.Source;
            }
            // TODO: Alarm-Name!
        }

        #endregion

        #region Backup and Restore
        [ACMethodCommand("Backup", "en{'Backup state'}de{'Zustand sichern'}", 180, true)]
        public virtual void BackupState()
        {
            PersistBackupState(false);
        }

        public virtual bool IsEnabledBackupState()
        {
            return Root.CurrentInvokingUser == null || Root.CurrentInvokingUser.IsSuperuser;
        }

        [ACMethodCommand("Backup", "en{'Clear and Reset backuped state'}de{'Gesicherten Zustand löschen und zurücksetzen'}", 180, true)]
        public virtual void ClearBackupState()
        {
            PersistBackupState(true);
        }

        public virtual bool IsEnabledClearBackupState()
        {
            return Root.CurrentInvokingUser == null || Root.CurrentInvokingUser.IsSuperuser;
        }

        protected virtual void PersistBackupState(bool resetAndClear = false)
        {
            ApplicationManager compManager = ApplicationManager;
            if (compManager != null)
            {
                compManager.AppContextQueue.Add(() =>
                {
                    List<PAClassAlarmingBase> childsForStateBackup = FindChildComponents<PAClassAlarmingBase>(c => c is PAClassAlarmingBase);
                    childsForStateBackup.Add(this);
                    foreach (var child in childsForStateBackup)
                    {
                        foreach (IACPropertyBase prop in child.ACPropertyList)
                        {
                            IACPropertyNetServer serverProp = prop as IACPropertyNetServer;
                            if (serverProp != null)
                                serverProp.BackupValue(resetAndClear);
                        }
                    }
                });
            }
        }

        [ACMethodCommand("Backup", "en{'Restore state'}de{'Zustand wiederherstellen'}", 181, true)]
        public virtual void RestoreBackupedState()
        {
            ApplicationManager compManager = ApplicationManager;
            if (compManager != null)
            {
                compManager.AppContextQueue.Add(() =>
                {
                    List<PAClassAlarmingBase> childsForStateBackup = FindChildComponents<PAClassAlarmingBase>(c => c is PAClassAlarmingBase);
                    childsForStateBackup.Add(this);
                    foreach (var child in childsForStateBackup)
                    {
                        foreach (IACPropertyNetServer serverProp in child.ACPropertyList)
                        {
                            serverProp.RestoreBackupedValue();
                        }
                    }
                });
            }
        }

        public virtual bool IsEnabledRestoreBackupedState()
        {
            return Root.CurrentInvokingUser == null || Root.CurrentInvokingUser.IsSuperuser;
        }
        #endregion

        #region Diagnostics and Dump
        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList, ref DumpStats dumpStats)
        {
            base.DumpPropertyList(doc, xmlACPropertyList, ref dumpStats);

            XmlElement xmlProperty = xmlACPropertyList["HasLocalAlarms"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("HasLocalAlarms");
                if (xmlProperty != null)
                    xmlProperty.InnerText = HasLocalAlarms.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["CountSubAlarms"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("CountSubAlarms");
                if (xmlProperty != null)
                    xmlProperty.InnerText = CountSubAlarms.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }
        }
        #endregion

    }
}

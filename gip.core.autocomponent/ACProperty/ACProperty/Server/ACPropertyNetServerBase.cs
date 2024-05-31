using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Base-Class network capable properties on Server-Side (Real instances)
    /// </summary>
    /// <typeparam name="T">Type of the ValueT-Property which is serializable and registered in the KnownTypes-Property of the DataContract-Serializer.</typeparam>
    /// <seealso cref="gip.core.autocomponent.ACPropertyNet{T}" />
    /// <seealso cref="gip.core.autocomponent.IACPropertyNetServer" />
    public abstract class ACPropertyNetServerBase<T> : ACPropertyNet<T>, IACPropertyNetServer
    {
        #region c'tors
        /// <summary>Universal Constructor</summary>
        /// <param name="PropertyValue"></param>
        /// <param name="acComponent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="IsProxy"></param>
        /// <param name="method"></param>
        public ACPropertyNetServerBase(ACPropertyValueHolder<T> PropertyValue, IACComponent acComponent, ACClassProperty acClassProperty, bool IsProxy, ACPropertySetMethod method)
            : base(PropertyValue, acComponent, acClassProperty, IsProxy)
        {
            SetMethod = method;

            if (!IsProxy)
            {
                // Weise Property der Member-Property der Klasse zu
                try
                {
                    Type typeOfThis = acComponent.GetType();
                    PropertyInfo propertyInfo = typeOfThis.GetProperty(acClassProperty.ACIdentifier);
                    if (propertyInfo != null)
                    {
                        if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.IsInterface)
                            propertyInfo.SetValue(acComponent, this, null);
                        //typeOfThis.InvokeMember(acClassProperty.ACIdentifier, BindingFlags.SetProperty, null, this, new object[] { this });

                        string setMethodName = "OnSet" + acClassProperty.ACIdentifier;
                        MethodInfo mi = typeOfThis.GetMethod(setMethodName);
                        if (mi != null)
                            SetMethod = (ACPropertySetMethod)Delegate.CreateDelegate(typeof(ACPropertySetMethod), acComponent, mi);
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPropertyNetServerBase<T>", "ACPropertyNetServerBase", msg);
                }
                if (!IsSetMethodAssigned)
                {
                    if ((acComponent as ACComponent).ScriptEngine != null)
                    {
                        if ((acComponent as ACComponent).ScriptEngine.ExistsScript(ScriptTrigger.Type.OnSetACPropertyNet, acClassProperty.ACIdentifier))
                            NetSetMethodOfScript = ((ACComponent)acComponent).OnSetACPropertyNet;
                    }
                }
                if (acClassProperty.LogRefreshRate != Global.MaxRefreshRates.Off)
                {
                    _PropertyLog = new ACPropertyValueLog<T>(this);
                }
            }
        }
        #endregion

        #region Properties
        public void ForceCreatePropertyValueLog(Global.MaxRefreshRates maxRefresh = Global.MaxRefreshRates.EventDriven)
        {
            if (_PropertyLog != null)
                return;
            _PropertyLog = new ACPropertyValueLog<T>(this);
            _PropertyLog.LogRefreshRate = maxRefresh;
        }

        private ACPropertyValueLog<T> _PropertyLog = null;
        public ACPropertyValueLog<T> PropertyLog
        {
            get
            {
                return _PropertyLog;
            }
        }

        /// <summary>
        /// PAClassPhysicalBaseProperty values ​​can also be stored in long-term archives. To do this, the update rate must be set in the iPlus development environment. Set a threshold value in the "Log Filter" field so that not every marginal change in value is persisted. This is particularly useful for double and float values. Use this method to query these archived values.
        /// </summary>
        /// <param name="from">Filter time from</param>
        /// <param name="to">Filter time to</param>
        /// <returns>PropertyLogListInfo.</returns>
        public override PropertyLogListInfo GetArchiveLog(DateTime from, DateTime to)
        {
            if (PropertyLog == null)
                return null;
            return PropertyLog.GetValues(from,to);
        }



        /// <summary>
        /// Delegate-Method for the Callback of a Script-Setter-Method
        /// </summary>
        private ACPropertyNetSetMethodScript _NetSetMethodACObject = null;
        public ACPropertyNetSetMethodScript NetSetMethodOfScript
        {
            get
            {
                return _NetSetMethodACObject;
            }

            set
            {
                _NetSetMethodACObject = value;
                // Referenz zu Set-Methode im Objekt hat zur Folge, dass es sich um ein Realse Objekt handelt => _IsProxy = false
                if ((_NetSetMethodACObject != null) && (IsProxy))
                    _IsProxy = false;
            }
        }

        public override bool IsSetMethodAssigned
        {
            get
            {
                if ((_SetMethod == null) && (_NetSetMethodACObject == null))
                    return false;
                return true;
            }
        }


        protected const string ACIdentifier4PerfEvent = "ACPropertyNetServerBase";
        /// <summary>
        /// Broadcast Event an alle Proxy-Objekte
        /// </summary>
        /// <param name="eventArgs"></param>
        protected virtual void BroadcastToProxies(ACPropertyValueEvent<T> eventArgs)
        {
            if ((ACRef == null) || (!ACRef.IsObjLoaded))
                return;
            ACComponent acComp = this.ACRef.ValueT as ACComponent;
            if (acComp == null)
                return;
            acComp.BroadcastPropertyValue(eventArgs);
            if (PropertyLog != null)
                PropertyLog.LogCurrentValue();
        }

        protected override bool CanPersist
        {
            get
            {
                bool canPersist = base.CanPersist;
                if (canPersist)
                {
                    if (PropertyLog != null && (!PropertyLog.IsRefreshCycleElapsed || !PropertyLog.ApplyFilter_IsLogable))
                        return false;
                }
                return canPersist;
            }
        }

        #endregion

        #region IACPropertyNetServer Member
        /// <summary>
        /// Occurs when the Value has changed of a Server-Property. This event is raised twice: Before a broadcast will take place or afterwards.
        /// </summary>
        public event ACPropertyChangedEventHandler ValueUpdatedOnReceival;
        internal void OnValueUpdatedOnReceival(IACPropertyNetValueEvent valueEvent, ACPropertyChangedPhase phase)
        {
            if (ValueUpdatedOnReceival != null)
            {
                ValueUpdatedOnReceival(this, new ACPropertyChangedEventArgs(ACIdentifier, valueEvent), phase);
            }
        }

        public override void ACDeInit(bool deleteACClassTask = false)
        {
            if (PropertyLog != null )
                PropertyLog.SaveChanges(true);
            base.ACDeInit(deleteACClassTask);
        }

        #endregion


        /// <summary>
        /// Method for changing the ACValueT-Property on Serverside to pass additional individual parameters
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="forceSend"></param>
        /// <param name="invokerInfo">Additional individual parameters</param>
        public void ChangeValueServer(object newValue, bool forceSend, object invokerInfo)
        {
            if (newValue != null)
                ChangeValueRequest((T)ACConvert.ChangeType(newValue, (object)this.ValueT, this.ACType.ObjectFullType, true, this.PropertyInfo.Database, true, false), forceSend, invokerInfo);
            else
                ChangeValueRequest(default(T), forceSend, invokerInfo);
        }

        /// <summary>
        /// Method for changing the ACValueT-Property on Serverside to pass additional individual parameters
        /// </summary>
        /// <param name="forceSend"></param>
        /// <param name="invokerInfo"></param>
        /// <param name="newValue"></param>
        public void ChangeValueServer(bool forceSend, object invokerInfo, T newValue)
        {
            ChangeValueRequest(newValue, forceSend, invokerInfo);
        }


        protected static PropBindingBindingResult RepairIncompatibleTypes(IACPropertyNetTarget targetProp, IACPropertyNetSource sourceProp, 
            out IACPropertyNetTarget newTargetProp, out string message, bool bindInDBIfConverterNeeded = true)
        {
            message = null;
            newTargetProp = null;
            if (sourceProp.Targets.Any())
            {
                IACPropertyNetTarget alreadyBoundProp = sourceProp.Targets.FirstOrDefault();
                if (alreadyBoundProp != null)
                {
                    message = String.Format("Can't bind {0} with {1} because it's already bound with {2} that is from another type! One of both types can't be repaired.",
                                  targetProp.GetACUrl(), sourceProp.GetACUrl(), alreadyBoundProp.GetACUrl());
                }
                else
                    message = String.Format("Can't bind {0} with {1} because it's already bound with a property from another type! One of both types can't be repaired.",
                                  targetProp.GetACUrl(), sourceProp.GetACUrl());
                return PropBindingBindingResult.NotPossible | PropBindingBindingResult.AlreadyBound;
            }
            Type typeBitAccess = typeof(IBitAccess);
            if (typeBitAccess.IsAssignableFrom(sourceProp.ACType.ValueTypeACClass.ObjectType) && typeBitAccess.IsAssignableFrom(targetProp.ACType.ValueTypeACClass.ObjectType))
            {
                string underlyingType1 = BitAccess<short>.GetUnderlyingTypeOfBitAccess(sourceProp.ACType.ValueTypeACClass.ObjectType);
                string underlyingType2 = BitAccess<short>.GetUnderlyingTypeOfBitAccess(targetProp.ACType.ValueTypeACClass.ObjectType);
                if (underlyingType1 == underlyingType2)
                {
                    ACClassProperty acClassPropOfSource = sourceProp.ACType as ACClassProperty;
                    if (acClassPropOfSource != null)
                    {
                        using (Database db = new Database())
                        {
                            ACClassProperty acClassPropOfSourceCtxt = db.ACClassProperty.Where(c => c.ACClassPropertyID == acClassPropOfSource.ACClassPropertyID).FirstOrDefault();
                            if (acClassPropOfSourceCtxt != null)
                            {
                                acClassPropOfSourceCtxt.ValueTypeACClassID = targetProp.ACType.ValueTypeACClass.ACClassID;
                                db.ACSaveChanges();
                                message = String.Format("Type of Source-Property {1} was changed to be able to bind {0}. IPlus-Service must be restarted again!",
                                              targetProp.GetACUrl(), sourceProp.GetACUrl());
                                return PropBindingBindingResult.TypeOfSourceWasChanged;
                            }
                        }
                    }
                }
            }
            else if (typeBitAccess.IsAssignableFrom(targetProp.ACType.ValueTypeACClass.ObjectType))
            {
                string underlyingType1 = sourceProp.ACType.ValueTypeACClass.ObjectType.Name;
                string underlyingType2 = BitAccess<short>.GetUnderlyingTypeOfBitAccess(targetProp.ACType.ValueTypeACClass.ObjectType);
                if (underlyingType1 == underlyingType2)
                {
                    ACClassProperty acClassPropOfSource = sourceProp.ACType as ACClassProperty;
                    if (acClassPropOfSource != null)
                    {
                        using (Database db = new Database())
                        {
                            ACClassProperty acClassPropOfSourceCtxt = db.ACClassProperty.Where(c => c.ACClassPropertyID == acClassPropOfSource.ACClassPropertyID).FirstOrDefault();
                            if (acClassPropOfSourceCtxt != null)
                            {
                                acClassPropOfSourceCtxt.ValueTypeACClassID = targetProp.ACType.ValueTypeACClass.ACClassID;
                                db.ACSaveChanges();
                                message = String.Format("Type of Source-Property {1} was changed to be able to bind {0}. IPlus-Service must be restarted again!",
                                              targetProp.GetACUrl(), sourceProp.GetACUrl());
                                return PropBindingBindingResult.TypeOfSourceWasChanged;
                            }
                        }
                    }
                }
            }
            else
            {
                Type typeConvertible = typeof(IConvertible);
                Type typeTimeSpan = typeof(TimeSpan);
                Type typeOfSource = sourceProp.ACType.ObjectType;
                ACClassProperty acClassPropOfTarget = targetProp.PropertyInfo;
                Type typeOfTarget = acClassPropOfTarget.ObjectType;
                bool isConvertibleType = true;
                if (!typeOfTarget.IsEnum
                    && !typeConvertible.IsAssignableFrom(typeOfTarget)
                    && !typeTimeSpan.IsAssignableFrom(typeOfTarget))
                    isConvertibleType = false;
                if (!typeOfSource.IsEnum
                    && !typeConvertible.IsAssignableFrom(typeOfSource)
                    && !typeTimeSpan.IsAssignableFrom(typeOfSource))
                    isConvertibleType = false;
                if (isConvertibleType)
                {
                    if (bindInDBIfConverterNeeded)
                    {
                        using (Database db = new Database())
                        {
                            var classOfSource = db.ACClass.Where(c => c.ACClassID == sourceProp.ParentACComponent.ComponentClass.ACClassID).FirstOrDefault();
                            var propOfSource = db.ACClassProperty.Where(c => c.ACClassPropertyID == sourceProp.ACType.ACTypeID).FirstOrDefault();
                            var classOfTarget = db.ACClass.Where(c => c.ACClassID == targetProp.ParentACComponent.ComponentClass.ACClassID).FirstOrDefault();
                            var propOfTarget = db.ACClassProperty.Where(c => c.ACClassPropertyID == targetProp.ACType.ACTypeID).FirstOrDefault();
                            if (classOfSource != null && propOfSource != null && classOfTarget != null && propOfTarget != null)
                            {
                                ACClassPropertyRelation binding = ACClassPropertyRelation.NewACClassPropertyRelation(db, classOfSource, propOfSource.TopBaseACClassProperty, classOfTarget, propOfTarget.TopBaseACClassProperty);
                                binding.ConnectionType = Global.ConnectionTypes.Binding;
                                db.ACClassPropertyRelation.Add(binding);
                                db.ACSaveChanges();
                            }
                        }
                    }

                    Type acPropertyType = typeof(ACPropertyNetTargetConverter<,>);
                    Type genericType = ACClassProperty.GetConvertibleACPropertyType(acPropertyType, typeOfTarget, typeOfSource);
                    ACComponent componentOfTarget = targetProp.ParentACComponent as ACComponent;

                    componentOfTarget.RemoveACMember(targetProp);
                    targetProp.ACDeInit();
                    newTargetProp = (IACPropertyNetTarget)Activator.CreateInstance(genericType, new Object[] { (IACComponent) componentOfTarget, acClassPropOfTarget, null });
                    if (newTargetProp != null)
                        newTargetProp.ACInit();
                    componentOfTarget.AddOrReplaceACMember(newTargetProp);

                    message = String.Format("ACPropertyNetTargetConverter<T,S> was created for {0} to be able to bind {1}.",
                                  targetProp.GetACUrl(), sourceProp.GetACUrl());
                    if (!bindInDBIfConverterNeeded)
                        message += " Consider to use bindInDBIfConverterNeeded = true to define the binding in the database!";

                    return PropBindingBindingResult.TargetPropReplaced;
                }
            }

            message = String.Format("Can't bind {0} with {1} because types are not compatible and can't be repaired.",
                          targetProp.GetACUrl(), sourceProp.GetACUrl());
            return PropBindingBindingResult.NotPossible | PropBindingBindingResult.NotCompatibleTypes;
        }
    }
}

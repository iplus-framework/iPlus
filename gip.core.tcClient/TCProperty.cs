using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.core.tcClient
{
    public class TCProperty
    {
        public TCProperty(TCItem tcItem, IACPropertyNetServer propertyNetServer, int remotePropID)
        {
            _Parent = tcItem;
            TCNetProperty = propertyNetServer;
            RemotePropID = remotePropID;
            RemoveValueUpdatedOnReceivalEvent();
            TCNetProperty.ValueUpdatedOnReceival += TCNetProperty_ValueUpdatedOnReceival;
        }

        private TCItem _Parent;
        DateTime _firstDT = new DateTime(1970, 1, 1);

        public int RemotePropID;

        private IACPropertyNetServer _tcNetProperty;
        public IACPropertyNetServer TCNetProperty
        {
            get
            {
                return _tcNetProperty;
            }
            set
            {
                _tcNetProperty = value;
            }
        }

        public void SetPropertyValue(object value)
        {
            Type propValueType = TCNetProperty.PropertyType;

            if (propValueType == typeof(bool) && value is byte)
                TCNetProperty.ChangeValueServer(Convert.ToBoolean(value), true, this);
            else if (value is short && propValueType.IsEnum)
                TCNetProperty.ChangeValueServer(Enum.ToObject(propValueType, value), true, this);
            else if (value is short && TCNetProperty.ACIdentifier == Const.ACState)
                TCNetProperty.ChangeValueServer(Enum.ToObject(typeof(TCACState), value).ToString(), true, this);
            else if (_BitAccessType.IsAssignableFrom(propValueType))
            {
                IBitAccess clonedBitAccess = null;
                if (TCNetProperty.Value != null)
                    clonedBitAccess = (IBitAccess)(TCNetProperty.Value as IBitAccess).Clone();
                if (clonedBitAccess != null)
                {
                    clonedBitAccess.Value = value;
                    TCNetProperty.ChangeValueServer(clonedBitAccess, true, this);
                }
            }
            else
                TCNetProperty.ChangeValueServer(value, true, this);
        }

        public void RemoveValueUpdatedOnReceivalEvent()
        {
            TCNetProperty.ValueUpdatedOnReceival -= TCNetProperty_ValueUpdatedOnReceival;
        }

        static Type _BitAccessType = typeof(IBitAccess);
        static Type _TimeSpanType = typeof(TimeSpan);
        static Type _DateTimeType = typeof(DateTime);

        void TCNetProperty_ValueUpdatedOnReceival(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase)
        {
            if (phase == ACPropertyChangedPhase.AfterBroadcast)
                return;
            if (sender == null)
                return;
            // Don't resend when new value receive over OnSetValueFromPLC()
            if (e.ValueEvent.InvokerInfo != null
                && e.ValueEvent.InvokerInfo == this
                && e.ValueEvent.EventType == EventTypes.ValueChangedInSource)
                return;

            object changedValue = e.ValueEvent.ChangedValue;
            if (TCNetProperty.ACIdentifier == Const.ACState && changedValue is string && !string.IsNullOrEmpty(changedValue.ToString()))
                changedValue = (short)Enum.Parse(typeof(TCACState), changedValue.ToString());

            Type propertyType = TCNetProperty.PropertyType;
            if (propertyType.IsEnum)
                changedValue = (short)e.ValueEvent.ChangedValue;
            else if (propertyType == _TimeSpanType)
                changedValue = (int)((TimeSpan)e.ValueEvent.ChangedValue).TotalMilliseconds;
            else if (propertyType == _DateTimeType)
            {
                DateTime eventDT = (DateTime)e.ValueEvent.ChangedValue;
                if (eventDT != null)
                {
                    TimeSpan diff = eventDT - _firstDT;
                    changedValue = (int)diff.TotalSeconds;
                }
            }
            else if (_BitAccessType.IsAssignableFrom(propertyType))
            {
                IBitAccess bitAcc = (IBitAccess)e.ValueEvent.ChangedValue;
                changedValue = bitAcc.Value;
            }

            _Parent.OnReceivedValueUpdated(TCNetProperty.ACIdentifier, RemotePropID, changedValue);
        }

        //todo: implement converter, multiplier and etc....
    }
}

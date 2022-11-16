using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.core.tcClient
{
    public class TCItem
    {
        public TCItem(TCSession tcSession, uint instanceID)
        {
            this.tcSession = tcSession;
            InstanceID = instanceID;
        }

        public TCSession tcSession;

        public uint InstanceID;

        //key represent remote property ID (RemotePropID)
        private Dictionary<int, TCProperty> _TCProperties = new Dictionary<int, TCProperty>();
        public Dictionary<int, TCProperty> TCProperties
        {
            get
            {
                return _TCProperties;
            }
        }

        public void AddProperties(IEnumerable<IACPropertyNetServer> properties)
        {
            TCProperties.Clear();
            foreach(IACPropertyNetServer propServer in properties)
            {
                if  (propServer.ACType is ACClassProperty)
                {
                    int remotePropID = ((ACClassProperty)propServer.ACType).RemotePropID;
                    if (remotePropID == 0)
                    {
                        ACClassProperty baseProp = ((ACClassProperty)propServer.ACType);
                        while (baseProp.ACClassPropertyID != baseProp.ACClassProperty1_BasedOnACClassProperty.ACClassPropertyID)
                        {
                            baseProp = baseProp.ACClassProperty1_BasedOnACClassProperty;
                            if (baseProp.RemotePropID > 0)
                            {
                                remotePropID = baseProp.RemotePropID;
                                break;
                            }
                        }
                        if (remotePropID == 0)
                        {
                            ACClass acClass = baseProp.ACClass.ACClass1_BasedOnACClass;
                            while(acClass.ACClass1_BasedOnACClass != null)
                            {
                                ACClassProperty tempProp = acClass.GetProperty(baseProp.ACIdentifier);
                                if(tempProp != null && tempProp.RemotePropID > 0)
                                {
                                    remotePropID = tempProp.RemotePropID;
                                    break;
                                }
                                acClass = acClass.ACClass1_BasedOnACClass;
                            }
                        }
                    }
                    //else if(remotePropID == 0 && !(propServer is IACPropertyNetSource))
                    //{

                    //}
                    if (remotePropID > 0 && !TCProperties.ContainsKey(remotePropID))
                    {
                        TCProperty tcProp = new TCProperty(this, propServer, remotePropID);
                        TCProperties.Add(remotePropID, tcProp);
                    }
                }
            }
        }

        public void RemovePropertyChangedEvent()
        {
            foreach(var prop in TCProperties)
                prop.Value.RemoveValueUpdatedOnReceivalEvent();
        }

        public void SetPropertyValue(int remotePropID, object value)
        {
            TCProperty outProperty;
            if(TCProperties.TryGetValue(remotePropID, out outProperty))
                outProperty.SetPropertyValue(value);
        }

        public void OnReceivedValueUpdated(string propACIdentifier, int remotePropID, object value)
        {
            tcSession.OnReceivedValueUpdated(InstanceID, propACIdentifier, remotePropID, value);
        }
    }
}

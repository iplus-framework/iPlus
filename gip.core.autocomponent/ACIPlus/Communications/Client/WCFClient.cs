using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Linq;
using System.Threading;
using System.ServiceModel;
using CoreWCF;
using System.Xml;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.autocomponent
{
    public partial class WCFClient : DuplexClientBase<IWCFService>, IWCFService
    {
        #region private members
        //WCFClientChannel _acServiceObject = null;
        #endregion

        #region c'tors
        public WCFClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance)
        {
        }
        
        public WCFClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName)
        {
        }
        
        public WCFClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }
        
        public WCFClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }
        
        public WCFClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress)
        {
        }


        #endregion

        private bool _TimeOutSet = false;
        public void Invoke(WCFMessage acMessage)
        {
            try
            {
                if (!_TimeOutSet)
                {
                    // Timeout wegen asnchronen Bearbeitungsprozessen:
                    // Falls eine ACURl-Command-Aufruf auf Serverseite lange dauert, würde die WCF (Aufrufer meist der Client) den Kanal schliessen
                    // durch auslösen des Events Closing. Das hat zur Folge, dass StopACObject aufgerufen wird.
                    //InnerDuplexChannel.OperationTimeout = new TimeSpan(0, 0, 30);
                    InnerChannel.OperationTimeout = new TimeSpan(0, 0, 30);
                    _TimeOutSet = true;
                }

                base.Channel.Invoke(acMessage);
            }
            catch (Exception e)
            {
                if (ACRoot.SRoot != null)
                    ACRoot.SRoot.Messages.LogDebug("WCFClient.Invoke(): ", "0", e.Message);
            }
        }
    }

}

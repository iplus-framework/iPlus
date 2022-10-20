﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using gip.core.tcShared.WCF;

namespace gip.core.tcClient
{
    public partial class WcfAdsClient : DuplexClientBase<IWcfAdsService>, IWcfAdsService
    {
        public WcfAdsClient(InstanceContext callbackInstance)
            : base(callbackInstance)
        {
        }

        public WcfAdsClient(InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        private bool _TimeOutSet = false;
        public void Invoke(WcfAdsMessage message)
        {
            try
            {
                if (!_TimeOutSet)
                {
                    // Timeout wegen asnchronen Bearbeitungsprozessen:
                    // Falls eine ACURl-Command-Aufruf auf Serverseite lange dauert, würde die WCF (Aufrufer meist der Client) den Kanal schliessen
                    // durch auslösen des Events Closing. Das hat zur Folge, dass StopACObject aufgerufen wird.
                    InnerDuplexChannel.OperationTimeout = new TimeSpan(0, 0, 30);
                    _TimeOutSet = true;
                }

                base.Channel.Invoke(message);
            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                               && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                    core.datamodel.Database.Root.Messages.LogException("WcfAdsClient", "Invoke", msg);
            }
        }
    }
}
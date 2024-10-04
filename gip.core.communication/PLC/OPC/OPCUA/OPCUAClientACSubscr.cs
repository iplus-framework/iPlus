using gip.core.autocomponent;
using gip.core.datamodel;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPCUAClientACSubscr'}de{'OPCUAClientACSubscr'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class OPCUAClientACSubscr : OPCClientACSubscr
    {
        #region c'tors

        public OPCUAClientACSubscr(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool success = base.ACDeInit(deleteACClassTask);
            _UASubscription = null;
            return success;
        }

        #endregion

        #region Properties

        private OPCUAClientSubscr _UASubscription;
        public OPCUAClientSubscr UASubscription
        {
            get
            {
                return _UASubscription;
            }
        }

        public OPCUAClientACSession OPCUASession
        {
            get
            {
                return ParentACComponent as OPCUAClientACSession;
            }
        }

        #endregion

        #region Methods

        #region Methods => Init/Deinit

        public override bool IsEnabledInitSubscription()
        {
            if (OPCUASession != null && OPCUASession.UASession != null)
                return true;

            return false;
        }

        public override bool InitSubscription()
        {
            Messages.LogDebug(this.GetACUrl(), "OPCUAClientACSubscr.InitSubscription(1)", "Creating UA Subscription");
            _UASubscription = new OPCUAClientSubscr(OPCUASession.UASession.DefaultSubscription)
            {
                DisplayName = ACIdentifier,
                PublishingInterval = RequiredUpdateRate
                //LifetimeCount = 0
            };

            OPCUASession.UASession.AddSubscription(_UASubscription);

            _UASubscription.StateChanged += _UASubscription_StateChanged;

            Messages.LogDebug(this.GetACUrl(), "OPCUAClientACSubscr.InitSubscription(2)", "The UA Subscription is created.");

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (OPCProperties != null)
                {
                    Messages.LogDebug(this.GetACUrl(), "OPCUAClientACSubscr.InitSubscription(3)", String.Format("Count OPCProperties: {0}", OPCProperties.Count()));
                    foreach (IACPropertyNetServer opcProperty in OPCProperties)
                    {
                        OPCItemConfig opcConfig = (OPCItemConfig)(opcProperty.ACType as ACClassProperty)["OPCItemConfig"];
                        if ((opcConfig != null) && !String.IsNullOrEmpty(opcConfig.OPCAddr))
                        {
                            OPCUAClientMonitoredItem monitoredItem = new OPCUAClientMonitoredItem(opcProperty, opcConfig.OPCAddr, this);
                            //IACPropertyNetSource opcSourceProperty = opcProperty as IACPropertyNetSource;
                            //if (opcSourceProperty != null)
                            //    opcSourceProperty.AdditionalRefs.Add(daItem);
                        }
                    }
                }
            }
            Messages.LogDebug(this.GetACUrl(), "OPCClientSACSubscr.InitSubscription(5)", "Monitored items (OPCProperties) are created.");

            return true;
        }

        private void _UASubscription_StateChanged(Opc.Ua.Client.Subscription subscription, Opc.Ua.Client.SubscriptionStateChangedEventArgs e)
        {
            if (subscription != null && subscription.PublishingStopped)
            {              
                Messages.LogInfo (this.GetACUrl(), "StateChanged", "OPC UA subscription is stopped.");
            }
        }

        public override bool IsEnabledDeInitSubscription()
        {
            if (UASubscription == null)
                return false;

            return true;
        }

        public override bool DeInitSubscription()
        {
            if (UASubscription == null)
                return true;

            _UASubscription.StateChanged -= _UASubscription_StateChanged;

            DisConnect();

            foreach (OPCUAClientMonitoredItem mItem in UASubscription.MonitoredItems)
                mItem.DeInit();

            UASubscription.Dispose();

            return true;
        }

        #endregion

        #region Methods => Connect/Disconnect

        public override bool IsEnabledConnect()
        {
            if (UASubscription != null && !UASubscription.Created)
                return true;
            return false;
        }

        public override bool Connect()
        {
            if (!IsEnabledConnect())
                return false;

            try
            {
                UASubscription.Create();

                foreach (OPCUAClientMonitoredItem mItem in UASubscription.MonitoredItems)
                {
                    mItem.ResolveAccess();
                    mItem.ReadInitValue(this);
                }

                this.IsReadyForWriting = true;
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "OPCUAClientACSubscr.Connect()", e);
                return false;
            }
            return true;
        }

        public override bool IsEnabledDisConnect()
        {
            if (UASubscription != null)
                return true;
            return false;
        }

        public override bool DisConnect()
        {
            if (!IsEnabledDisConnect())
                return false;

            if (!UASubscription.Session.Disposed && UASubscription.Session.TransportChannel != null)
                UASubscription.Delete(true);

            return true;
        }

        #endregion

        #endregion

        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList, ref DumpStats dumpStats)
        {
            base.DumpPropertyList(doc, xmlACPropertyList, ref dumpStats);

            OPCUAClientSubscr subscr = _UASubscription;

            if (subscr != null)
            {
                XmlElement xmlChild = xmlACPropertyList["CurrentKeepAliveCount"];
                if (xmlChild == null)
                {
                    xmlChild = doc.CreateElement("CurrentKeepAliveCount");
                    if (xmlChild != null)
                        xmlChild.InnerText = subscr.KeepAliveCount.ToString();
                    xmlACPropertyList.AppendChild(xmlChild);
                }

                xmlChild = xmlACPropertyList["CurrentLifetimeCount"];
                if (xmlChild == null)
                {
                    xmlChild = doc.CreateElement("CurrentLifetimeCount");
                    if (xmlChild != null)
                        xmlChild.InnerText = subscr.CurrentLifetimeCount.ToString();
                    xmlACPropertyList.AppendChild(xmlChild);
                }

                xmlChild = xmlACPropertyList["CurrentPublishingEnabled"];
                if (xmlChild == null)
                {
                    xmlChild = doc.CreateElement("CurrentPublishingEnabled");
                    if (xmlChild != null)
                        xmlChild.InnerText = subscr.CurrentPublishingEnabled.ToString();
                    xmlACPropertyList.AppendChild(xmlChild);
                }

                xmlChild = xmlACPropertyList["PublishingStopped"];
                if (xmlChild == null)
                {
                    xmlChild = doc.CreateElement("PublishingStopped");
                    if (xmlChild != null)
                        xmlChild.InnerText = subscr.PublishingStopped.ToString();
                    xmlACPropertyList.AppendChild(xmlChild);
                }

                xmlChild = xmlACPropertyList["LifetimeCount"];
                if (xmlChild == null)
                {
                    xmlChild = doc.CreateElement("LifetimeCount");
                    if (xmlChild != null)
                        xmlChild.InnerText = subscr.LifetimeCount.ToString();
                    xmlACPropertyList.AppendChild(xmlChild);
                }

                xmlChild = xmlACPropertyList["KeepAliveCount"];
                if (xmlChild == null)
                {
                    xmlChild = doc.CreateElement("KeepAliveCount");
                    if (xmlChild != null)
                        xmlChild.InnerText = subscr.KeepAliveCount.ToString();
                    xmlACPropertyList.AppendChild(xmlChild);
                }

                xmlChild = xmlACPropertyList["SequenceNumber"];
                if (xmlChild == null)
                {
                    xmlChild = doc.CreateElement("SequenceNumber");
                    if (xmlChild != null)
                        xmlChild.InnerText = subscr.SequenceNumber.ToString();
                    xmlACPropertyList.AppendChild(xmlChild);
                }   
            }
        }
    }
}

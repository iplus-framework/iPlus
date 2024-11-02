// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;
using gip.core.datamodel;
using System.ComponentModel;

namespace gip.core.autocomponent
{
    #region Basismessage
    [DataContract]
    [KnownType("GetKnownType")]
    public class WCFMessage : IWCFMessage
    {
        #region c´tors
        static WCFMessage()
        {
            ACKnownTypes.RegisterKnownMessageType(typeof(ACConnect));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACDisconnect));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACMethodInvocationResult));
            ACKnownTypes.RegisterKnownMessageType(typeof(EntityKey));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACStartCompOptions));

            // Original aus ACPropertyMessages
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPropertyValueMessage));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACSubscriptionMessage));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACSubscriptionServiceMessage));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPSubscrDispClient));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPSubscrDispService));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPSubscrProjDispClient));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPSubscrProjDispService));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPSubscrObjDispClient));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPSubscrObjDispService));
            //ACKnownTypes.RegisterKnownMessageType(typeof(ACMethodEventArgs));
            //ACKnownTypes.RegisterKnownMessageType(typeof(ACEventArgs));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACValueList));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACValueWithCaptionList));
            ACKnownTypes.RegisterKnownMessageType(typeof(BindingList<ACFilterItem>));
            ACKnownTypes.RegisterKnownMessageType(typeof(BindingList<ACSortItem>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACValue));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACValueWithCaption));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointNetWrapObject<IACObject>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointEventSubscrWrap<ACComponent>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointAsyncRMIWrap<ACComponent>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointAsyncRMISubscrWrap<ACComponent>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointServiceObjectProxy<IACObject>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointServiceACObjectProxy));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointClientObjectProxy<IACObject>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointClientACObjectProxy));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointEventProxy));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointEventSubscrProxy));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointAsyncRMIProxy));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointAsyncRMISubscrProxy));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointTaskProxy));
            ACKnownTypes.RegisterKnownMessageType(typeof(PWPointInProxy));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPointNetWrapObject<IACObject>.RefInitMode));
            ACKnownTypes.RegisterKnownMessageType(typeof(Type));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACChildInstanceInfo));
            ACKnownTypes.RegisterKnownMessageType(typeof(List<ACChildInstanceInfo>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACPropertyValueEvent<List<ACChildInstanceInfo>>));           
            ACKnownTypes.RegisterKnownMessageType(typeof(ChildInstanceInfoSearchParam));
            ACKnownTypes.RegisterKnownMessageType(typeof(PAOrderInfo));
            ACKnownTypes.RegisterKnownMessageType(typeof(PAOrderInfoEntry));
            ACKnownTypes.RegisterKnownMessageType(typeof(List<PAOrderInfoEntry>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACConfigStoreInfo));
            ACKnownTypes.RegisterKnownMessageType(typeof(List<ACConfigStoreInfo>));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACUrlCmdMessage));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACRoutingVertex));
            ACKnownTypes.RegisterKnownMessageType(typeof(List<ACRoutingVertex>));
            ACKnownTypes.RegisterKnownMessageType(typeof(PAEdge));
            ACKnownTypes.RegisterKnownMessageType(typeof(ACRoutingPath));
            ACKnownTypes.RegisterKnownMessageType(typeof(PAPoint));
            ACKnownTypes.RegisterKnownMessageType(typeof(ST_Node));
            ACKnownTypes.RegisterKnownMessageType(typeof(PriorityQueue<ST_Node>));
            ACKnownTypes.RegisterKnownMessageType(typeof(Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>>));
            ACKnownTypes.RegisterKnownMessageType(typeof(List<Route>));
            ACKnownTypes.RegisterKnownMessageType(typeof(RoutingResult));
            ACKnownTypes.RegisterKnownMessageType(typeof(byte[]));
            ACKnownTypes.RegisterKnownMessageType(typeof(Dictionary<string, string>));
            ACKnownTypes.RegisterKnownMessageType(typeof(Dictionary<string, object>));
            ACKnownTypes.RegisterKnownMessageType(typeof(BitAccessForAllocatedByWay));
            ACKnownTypes.RegisterKnownMessageType(typeof(Guid[]));

            ACKnownTypes.RegisterUnKnownType(typeof(ACRef<IACObject>));
            ACKnownTypes.RegisterUnKnownType(typeof(ACRef<ACComponent>));
            ACKnownTypes.RegisterUnKnownType(typeof(Dictionary<string, ACPSubscrObjBase>));
            ACKnownTypes.RegisterUnKnownType(typeof(AsyncMethodInvocationMode));
        }

        public WCFMessage()
        {
            ACUrl = "";
        }

        public static WCFMessage NewACMessage(string acUrl, Object[] acParameter)
        {
            return WCFMessage.NewACMessage("", acUrl, 0, acParameter);
        }

        public static WCFMessage NewACMessage(string acUrlRequester, string acUrl, int methodInvokeRequestID, Object[] acParameter)
        {
            return new WCFMessage { ACUrlRequester = acUrlRequester, ACUrl = acUrl, ACParameter = acParameter, MethodInvokeRequestID = methodInvokeRequestID };
        }
        #endregion

        #region IWCFMessage
        [DataMember]
        public string ACUrl { get; set; }

        [DataMember]
        public Object[] ACParameter { get; set; }

        [DataMember]
        public string ACUrlRequester { get; set; }

        [DataMember]
        public int MethodInvokeRequestID { get; set; }
        #endregion

        #region static methots
        //private static List<Type> _AllTypes = new List<Type>();
        public static Type[] GetKnownType()
        {
            return ACKnownTypes.GetKnownType();
        }

        public static bool IsKnownType(Type t)
        {
            return ACKnownTypes.IsKnownType(t);
        }

        public static bool IsTypeBroadcastable(Type t)
        {
            return ACKnownTypes.IsTypeBroadcastable(t);
        }

        public static string ACUrlItem(string acUrl, int index)
        {
            string[] acUrlList = acUrl.Split('\\');
            if (acUrlList.Count() > index)
            {
                return acUrlList[index];
            }
            else
            {
                return "";
            }
        }

        public static string NextACUrlItem(string acUrl)
        {
            int pos = acUrl.IndexOf("\\");
            if (pos == -1)
                return "";
            return acUrl.Substring(pos);
        }
        #endregion
    }

    [DataContract]
    public class ACConnect
    {
        [DataMember]
        public string UserName { get; set; }
    }

    [DataContract]
    public class ACDisconnect
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public bool ShutdownClient { get; set; }
    }
    #endregion

    [DataContract]
    public class ACMethodInvocationResult
    {
        [DataMember]
        public object MethodResult { get; set; }
    }
}

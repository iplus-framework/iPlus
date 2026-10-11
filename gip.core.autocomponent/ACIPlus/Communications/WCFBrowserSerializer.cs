// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Serializes <see cref="WCFMessage"/> frames for the browser WebSocket transport
    /// (SqlTunnel-OptionB-Implementation.md §8). One WebSocket binary message = one WCFMessage,
    /// serialized with the same DataContractSerializer stack (known types + resolver) that the
    /// WCF bindings use, so the message contracts round-trip identically.
    /// </summary>
    public static class WCFBrowserSerializer
    {
        static DataContractSerializer CreateSerializer()
        {
            return new DataContractSerializer(typeof(WCFMessage), new DataContractSerializerSettings()
            {
                KnownTypes = ACKnownTypes.GetKnownType(),
                MaxItemsInObjectGraph = WCFServiceManager.MaxItemsInObjectGraph,
                IgnoreExtensionDataObject = true,
                PreserveObjectReferences = true,
                DataContractResolver = ACConvert.MyDataContractResolver
            });
        }

        public static byte[] Serialize(WCFMessage message)
        {
            if (message == null)
                return null;
            DataContractSerializer serializer = CreateSerializer();
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, message);
                return ms.ToArray();
            }
        }

        public static WCFMessage Deserialize(byte[] frame)
        {
            if (frame == null || frame.Length == 0)
                return null;
            DataContractSerializer serializer = CreateSerializer();
            using (MemoryStream ms = new MemoryStream(frame))
            {
                return serializer.ReadObject(ms) as WCFMessage;
            }
        }
    }
}

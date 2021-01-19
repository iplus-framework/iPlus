using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [KnownType("GetKnownType")]
    [ACSerializeableInfo]
    public class ACPropertyValueMessage
    {
        [DataMember]
        public List<IACPropertyNetValueEvent> PropertyValues { get; set; }

        private static Type[] GetKnownType()
        {
            return ACPropertyFactoryBase.GenerateUnKnownValueEventTypes().ToArray();
        }
    }


    [DataContract]
    [ACSerializeableInfo]
    public class ACSubscriptionMessage
    {
        [DataMember]
        public ACPSubscrDispClient ACPSubscribe { get; set; }

        [DataMember]
        public ACPSubscrDispClient ACPUnSubscribe { get; set; }

        public void Detach()
        {
            if (ACPSubscribe != null)
                ACPSubscribe.Detach();
            if (ACPUnSubscribe != null)
                ACPUnSubscribe.Detach();
        }
    }


    [DataContract]
    [ACSerializeableInfo]
    public class ACSubscriptionServiceMessage
    {
        [DataMember]
        public ACPSubscrDispService ACPSubscribe { get; set; }

        public void Detach()
        {
            if (ACPSubscribe != null)
                ACPSubscribe.Detach();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace gip.core.webservices
{
    [DataContract]
    public class ACClassMessage
    {
        [DataMember(Name = "ID")]
        public Guid ACClassMessageID
        {
            get;
            set;
        }

        [DataMember(Name = "ACI")]
        public string ACIdentifier
        {
            get;
            set;
        }

        [DataMember(Name = "ACC")]
        public string ACCaption
        {
            get;
            set;
        }
    }
}

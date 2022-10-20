using System;
using System.Runtime.Serialization;
using System.Reflection;
using gip.core.datamodel;

namespace gip.core.webservices
{
    [DataContract(Name ="ACCP")]
    public class ACClassProperty
    {
        [DataMember(Name = "ID")]
        public Guid ACClassPropertyID
        {
            get; set;
        }

        [DataMember(Name = "ACI")]
        public string ACIdentifier
        {
            get; set;
        }

        [DataMember(Name = "CT")]
        public string ACCaptionTranslation
        {
            get; set;
        }

        [IgnoreDataMember]
        public string ACCaption
        {
            get
            {
                return Translator.GetTranslation(ACCaptionTranslation);
            }
        }

    }
}

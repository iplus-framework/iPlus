using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace gip.core.webservices
{
    [DataContract]
    public class VBUserRights
    {
        /// <summary>
        /// Gets or sets the language. I18N-Tag
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        [DataMember]
        public string Language
        {
            get; set;
        }

        [DataMember]
        public string DefaultLanguage
        {
            get; set;
        }

        [DataMember]
        public string UserName
        {
            get;set;
        }

        [DataMember]
        public List<VBMenuItem> Menu
        {
            get; set;
        }

        [DataMember]
        public Guid? SessionID
        {
            get; set;
        }

        [DataMember]
        public WSTranslation Translation
        {
            get;set;
        }
    }
}

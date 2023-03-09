using System;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    [DataContract]
    [ACSerializeableInfo(new Type[] { typeof(RuleValue) })]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'SQLInstanceInfo'}de{'SQLInstanceInfo'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class SQLInstanceInfo
    {
        [DataMember]
        [ACPropertyInfo(1, "IP", "en{'Server Name'}de{'Server Name'}")]
        public string ServerName {get;set;}

        [DataMember]
        [ACPropertyInfo(2, Const.ContextDatabase, "en{'Database'}de{'Datenbank'}")]
	    public string Database {get;set;}

        [DataMember]
        [ACPropertyInfo(3, "Username", "en{'Username'}de{'Benutzername'}")]
	    public string Username {get;set;}

        [DataMember]
        [ACPropertyInfo(4, "Password", "en{'Password'}de{'Kennwort'}")]
        public string Password { get; set; }


        [DataMember]
        [ACPropertyInfo(5, "MESContextFullName", "en{'MES (or custom) Context Fullname'}de{'MES (oder benutzerdefinierter) Kontext Vollständiger Name'}")]
        public string MESContextFullName { get; set; }


    }
}

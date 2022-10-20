using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

       
    }
}

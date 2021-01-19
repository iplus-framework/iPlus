using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'DB Session Info'}de{'DB Sitzungsinfo'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBUserSessionInfo
    {
        public VBUserSessionInfo()
        {
            LoginTime = DateTime.Now;
            LogoutTime = LoginTime;
        }

        public VBUserSessionInfo(string computer, int sessionID) : this()
        {
            Computer = computer;
            SessionID = sessionID;
        }

        [DataMember]
        [ACPropertyInfo(1, "", "en{'Computer'}de{'Computer'}")]
        public string Computer { get; set; }

        [DataMember]
        [ACPropertyInfo(2, "", "en{'SessionID'}de{'SessionID'}")]
        public int SessionID { get; set; }

        [DataMember]
        [ACPropertyInfo(3, "", "en{'Logged on since'}de{'Angemeldet seit'}")]
        public DateTime LoginTime { get; set; }

        [DataMember]
        [ACPropertyInfo(4, "", "en{'Logged out'}de{'Abgemeldet am'}")]
        public DateTime LogoutTime { get; set; }

        [IgnoreDataMember]
        [ACPropertyInfo(5, "", "en{'Session duration'}de{'Sitzungsdauer'}")]
        public TimeSpan SessionDuration
        {
            get
            {
                return IsLoggedOut ? LogoutTime - LoginTime : DateTime.Now - LoginTime;
            }
        }

        [IgnoreDataMember]
        [ACPropertyInfo(6, "", "en{'Is logged out'}de{'Ist Abgemeldet'}")]
        public bool IsLoggedOut
        {
            get
            {
                return LoginTime != LogoutTime;
            }
        }

        [IgnoreDataMember]
        [ACPropertyInfo(7, "", "en{'User'}de{'Benutzer'}")]
        public string UserName
        {
            get;set;
        }
    }
}

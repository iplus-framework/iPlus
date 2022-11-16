using System;

namespace gip.core.datamodel
{
    [Serializable]
#if !EFCR
    [JsonObject(MemberSerialization.OptIn)]
#endif
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Login'}de{'Login'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class Login
    {
#if !EFCR
        [JsonProperty]
#endif
        [ACPropertyInfo(9999, "Email", "en{'Email'}de{'Email'}")]
        public string Email { get; set; }

#if !EFCR
        [JsonProperty]
#endif
        [ACPropertyInfo(9999, "Password", "en{'Password'}de{'Kennwort'}")]
        public string Password { get; set; }

        [ACPropertyInfo(9999, "RememberLogin", "en{'Remeber login'}de{'Login speichern'}")]
        public bool RememberLogin { get; set; }
    }
}

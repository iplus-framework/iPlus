using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'SemaphoreEnum'}de{'SemaphoreEnum'}", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, true, false)]
    [DataContract]
    public enum SemaphoreEnum : short
    {
        [EnumMember(Value = "None")]
        None = 0,

        [EnumMember(Value = "Red")]
        Red = 1,

        [EnumMember(Value = "Yellow")]
        Yellow = 2,

        [EnumMember(Value = "Green")]
        Green = 3
    }
}

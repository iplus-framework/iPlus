using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'SemaphoreEnum'}de{'SemaphoreEnum'}", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, true, false)]
    [DataContract]
    public enum SemaphoreEnum : short
    {
        None,
        Red,
        Yellow,
        Green
    }
}

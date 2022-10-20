using System;

namespace gip.core.datamodel
{
    [Serializable]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'FindMatchingUrlsParam'}de{'FindMatchingUrlsParam'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class FindMatchingUrlsParam
    {
        public Func<IACComponent, bool> Query { get; set; }
    }
}

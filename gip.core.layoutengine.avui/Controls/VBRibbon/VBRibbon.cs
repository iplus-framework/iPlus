using gip.core.layoutengine.avui.AvaloniaRibbon;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBRibbon'}de{'VBRibbon'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBRibbon : Ribbon, ICommandBindingOwner
    {
    }
}

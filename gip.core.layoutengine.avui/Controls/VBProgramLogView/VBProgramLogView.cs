using Avalonia.Controls;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBProgramLogView'}de{'VBProgramLogView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBProgramLogView : VBTreeView
    {

        #region Protected

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return VBProgramLogViewItem.CreateContainerItem(item);
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<VBTreeViewItem>(item, out recycleKey);
        }

        #endregion

    }
}

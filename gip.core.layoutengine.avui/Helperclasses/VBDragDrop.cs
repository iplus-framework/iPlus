using Avalonia.Input;
using gip.core.datamodel;
using System;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    public static class VBDragDrop 
    {
        public static void VBDoDragDrop(PointerEventArgs e, IACInteractiveObject vbControl/*, IACObjectWithBinding acObject, IACComponent acComponent, Point position*/)
        {
            DataObject data = new DataObject();
            data.Set(vbControl.GetType().FullName, vbControl.GetType());
            data.Set(nameof(IACInteractiveObject), vbControl);
            DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
        }

        public static IACInteractiveObject GetDropObject(DragEventArgs e)
        {
            var formats = e.Data.GetDataFormats();
            if (formats == null || !formats.Contains(nameof(IACInteractiveObject)))
                return null;

            return e.Data.Get(nameof(IACInteractiveObject)) as IACInteractiveObject;
        }
    }

}

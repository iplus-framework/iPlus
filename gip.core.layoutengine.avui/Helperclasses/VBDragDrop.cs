using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    public static class VBDragDrop 
    {
        public static void VBDoDragDrop(IACInteractiveObject vbControl/*, IACObjectWithBinding acObject, IACComponent acComponent, Point position*/)
        {
            DataObject data = new DataObject(vbControl.GetType(), vbControl);
            DragDrop.DoDragDrop(vbControl as DependencyObject, data, DragDropEffects.Copy);
        }

        public static IACInteractiveObject GetDropObject(DragEventArgs e)
        {
            if (!e.Data.GetFormats().Any())
                return null;

            foreach (var dataFormat in e.Data.GetFormats())
            {
                var data = e.Data.GetData(dataFormat);
                if (data is IACInteractiveObject)
                {
                    return data as IACInteractiveObject;
                }
            }
            return null;
        }
    }

}

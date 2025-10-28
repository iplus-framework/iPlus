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


    //public static class VBDragDrop
    //{
    //    public static Task<DragDropEffects> VBDoDragDrop(PointerEventArgs e, IACInteractiveObject vbControl/*, IACObjectWithBinding acObject, IACComponent acComponent, Point position*/)
    //    {
    //        var dataTransfer = new DataTransfer();

    //        // Create a data transfer item for the control type
    //        var typeItem = new DataTransferItem();
    //        typeItem.Set(DataFormat.CreateStringApplicationFormat("ControlType"), vbControl.GetType().FullName);
    //        dataTransfer.Add(typeItem);

    //        // Create a data transfer item for the interactive object
    //        var objectItem = new DataTransferItem();
    //        objectItem.Set(DataFormat.CreateStringApplicationFormat("IACInteractiveObject"), nameof(IACInteractiveObject));
    //        dataTransfer.Add(objectItem);

    //        // Store the actual object reference in a custom format
    //        var objectRefItem = new DataTransferItem();
    //        var objectRefFormat = DataFormat.CreateStringApplicationFormat("ObjectReference");
    //        objectRefItem.Set(objectRefFormat, () => vbControl.GetHashCode().ToString());
    //        dataTransfer.Add(objectRefItem);

    //        // Store the object in a static cache for retrieval
    //        CacheObject(vbControl);

    //        return DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Copy);
    //    }

    //    public static IACInteractiveObject GetDropObject(DragEventArgs e)
    //    {
    //        // Use the new DataTransfer property instead of the obsolete Data property
    //        var dataTransfer = e.DataTransfer;
    //        if (dataTransfer?.Items == null || !dataTransfer.Items.Any())
    //            return null;

    //        // Check if we have our custom format
    //        var objectRefFormat = DataFormat.CreateStringApplicationFormat("ObjectReference");
    //        var interactiveObjectFormat = DataFormat.CreateStringApplicationFormat("IACInteractiveObject");

    //        // Check if the format exists in any of the items
    //        bool hasInteractiveObjectFormat = dataTransfer.Items.Any(item =>
    //            item.Formats.Any(f => f.Identifier == interactiveObjectFormat.Identifier));

    //        if (!hasInteractiveObjectFormat)
    //            return null;

    //        // Try to get the object reference from any item that has it
    //        foreach (var item in dataTransfer.Items)
    //        {
    //            var hashCodeStr = item.TryGetRaw(objectRefFormat) as string;
    //            if (hashCodeStr != null && int.TryParse(hashCodeStr, out int hashCode))
    //            {
    //                return GetCachedObject(hashCode);
    //            }
    //        }

    //        return null;
    //    }

    //    // Simple static cache for drag and drop objects
    //    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, WeakReference<IACInteractiveObject>> _objectCache
    //        = new System.Collections.Concurrent.ConcurrentDictionary<int, WeakReference<IACInteractiveObject>>();

    //    private static void CacheObject(IACInteractiveObject obj)
    //    {
    //        _objectCache.AddOrUpdate(obj.GetHashCode(),
    //            new WeakReference<IACInteractiveObject>(obj),
    //            (key, oldValue) => new WeakReference<IACInteractiveObject>(obj));
    //    }

    //    private static IACInteractiveObject GetCachedObject(int hashCode)
    //    {
    //        if (_objectCache.TryGetValue(hashCode, out var weakRef) &&
    //            weakRef.TryGetTarget(out var target))
    //        {
    //            return target;
    //        }
    //        return null;
    //    }

    //    // Clean up old cache entries periodically
    //    public static void CleanupCache()
    //    {
    //        var keysToRemove = new List<int>();
    //        foreach (var kvp in _objectCache)
    //        {
    //            if (!kvp.Value.TryGetTarget(out _))
    //            {
    //                keysToRemove.Add(kvp.Key);
    //            }
    //        }

    //        foreach (var key in keysToRemove)
    //        {
    //            _objectCache.TryRemove(key, out _);
    //        }
    //    }
    //}


}

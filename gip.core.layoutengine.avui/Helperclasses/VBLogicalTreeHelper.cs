using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using gip.core.datamodel;
using gip.ext.design.avui;
using gip.ext.designer.avui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui.Helperclasses
{
    public static class VBLogicalTreeHelper
    {
        public static void DeInitVBControls(IACComponent acObject, Object guiObject)
        {
            if (guiObject == null)
                return;
            ILogical depObject = guiObject as ILogical;
            if (depObject != null)
            {
                foreach (object o in depObject.GetLogicalChildren())
                {
                    DeInitVBControls(acObject, o);
                }
                if (depObject is IVBContent)
                {
                    IVBContent vbContentObj = depObject as IVBContent;
                    if (vbContentObj != null)
                        vbContentObj.DeInitVBControl(null);
                }
                (guiObject as AvaloniaObject)?.ClearAllBindings();
            }

        }

        public static void RemoveAllDialogs(IACComponent acObject)
        {
            List<VBWindowDialogRoot> listDialog = new List<VBWindowDialogRoot>();

            if (acObject.ReferencePoint != null)
            {
                foreach (var context in acObject.ReferencePoint.ConnectionList)
                {
                    if (context is VBWindowDialogRoot)
                    {
                        listDialog.Add(context as VBWindowDialogRoot);
                    }
                }
            }

            foreach (var dialog in listDialog)
            {
                dialog.Close();
            }

        }

        public static AvaloniaObject FindObjectInLogicalTree(AvaloniaObject obj, string PART_Name)
        {
            ILogical depObject = obj as ILogical;
            if (depObject == null)
                return null;
            AvaloniaObject partObj = LogicalTreeHelper.FindLogicalNode(obj, PART_Name);
            if (partObj != null)
                return partObj;
            partObj = VBLogicalTreeHelper.FindParentObjectInLogicalTree(obj, PART_Name);
            if (partObj != null)
                return partObj;
            partObj = VBLogicalTreeHelper.FindChildObjectInLogicalTree(obj, PART_Name);
            if (partObj != null)
                return partObj;
            return null;
        }

        public static AvaloniaObject FindObjectInLogicalTree(AvaloniaObject obj, Type type)
        {
            AvaloniaObject partObj = VBLogicalTreeHelper.FindParentObjectInLogicalTree(obj, type);
            if (partObj != null)
                return partObj;
            partObj = VBLogicalTreeHelper.FindChildObjectInLogicalTree(obj, type);
            if (partObj != null)
                return partObj;
            return null;
        }

        public static AvaloniaObject FindChildObjectInLogicalTree(AvaloniaObject obj, string PART_Name)
        {
            if (obj == null)
                return null;
            if (obj is Control)
            {
                Control partObj = obj as Control;
                if (partObj.Name == PART_Name)
                    return partObj;
            }
            foreach (object childObj in LogicalTreeHelper.GetChildren(obj))
            {
                if (!(childObj is Control))
                    continue;
                AvaloniaObject found = FindChildObjectInLogicalTree(childObj as AvaloniaObject, PART_Name);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static AvaloniaObject FindChildObjectInLogicalTree(AvaloniaObject obj, Type type)
        {
            if (obj == null)
                return null;
            if (obj.GetType() == type)
                return obj;
            else if (type.IsAssignableFrom(obj.GetType()))
                return obj;
            foreach (object childObj in LogicalTreeHelper.GetChildren(obj))
            {
                if (!(childObj is AvaloniaObject))
                    continue;
                AvaloniaObject found = FindChildObjectInLogicalTree(childObj as AvaloniaObject, type);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static AvaloniaObject FindChildObject(AvaloniaObject obj, Type type)
        {
            if (obj == null)
                return null;
            if (obj.GetType() == type)
                return obj;
            else if (type.IsAssignableFrom(obj.GetType()))
                return obj;
            if (obj is ContentPresenter)
            {
                AvaloniaObject contentObj = (obj as ContentPresenter).Content as AvaloniaObject;
                if (contentObj != null)
                {
                    AvaloniaObject found = FindChildObject(contentObj, type);
                    if (found != null)
                        return found;
                }
            }
            else
            {
                foreach (object childObj in LogicalTreeHelper.GetChildren(obj))
                {
                    if (!(childObj is AvaloniaObject))
                        continue;
                    AvaloniaObject found = FindChildObject(childObj as AvaloniaObject, type);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        public static IEnumerable<T> FindChildObjects<T>(AvaloniaObject obj) where T : class
        {
            List<T> result = new List<T>();

            FindChildObjects<T>(obj, result);

            return result;
        }

        private static void FindChildObjects<T>(AvaloniaObject obj, List<T> resultList) where T : class
        {
            if (obj == null)
                return;

            Type type = typeof(T);

            if (obj.GetType() == type)
            {
                resultList.Add(obj as T);
            }
            else if (type.IsAssignableFrom(obj.GetType()))
            {
                resultList.Add(obj as T);
            }

            if (obj is ContentPresenter)
            {
                AvaloniaObject contentObj = (obj as ContentPresenter).Content as AvaloniaObject;
                if (contentObj != null)
                {
                    FindChildObjects<T>(contentObj, resultList);
                }
            }
            else
            {
                foreach (object childObj in LogicalTreeHelper.GetChildren(obj))
                {
                    if (!(childObj is AvaloniaObject))
                        continue;
                    FindChildObjects<T>(childObj as AvaloniaObject, resultList);
                }
            }
        }


        public static AvaloniaObject FindParentObjectInLogicalTree(AvaloniaObject obj, string PART_Name)
        {
            if (obj == null)
                return null;
            if (obj is Control)
            {
                Control partObj = obj as Control;
                if (partObj.Name == PART_Name)
                    return partObj;
            }
            return FindParentObjectInLogicalTree(LogicalTreeHelper.GetParent(obj), PART_Name);
        }

        public static AvaloniaObject FindParentObjectInLogicalTree(AvaloniaObject obj, Type type)
        {
            if (obj == null)
                return null;
            Type typeObj = obj.GetType();
            if (typeObj == type)
                return obj;
            else if (type.IsAssignableFrom(typeObj))
                return obj;
            return FindParentObjectInLogicalTree(LogicalTreeHelper.GetParent(obj), type);
        }

        public static IACComponent FindParentBSO(Control obj)
        {
            IACInteractiveObject acElement = VBLogicalTreeHelper.FindObjectInLogicalTree(obj.Parent, typeof(IACInteractiveObject)) as IACInteractiveObject;
            if (acElement == null)
                return null;
            if (acElement.ContextACObject is IACComponent)
            {
                if (acElement.ContextACObject.ACType.ACKind == Global.ACKinds.TACBSO)
                    return acElement.ContextACObject as IACComponent;
                return VBLogicalTreeHelper.FindParentBSO(acElement as Control);
            }
            return null;
        }

        public static VBDesign GetVBDesign(this AvaloniaObject obj)
        {
            VBDesign result = FindParentObjectInLogicalTree(obj, typeof(VBDesign)) as VBDesign;
            if (result == null)
                result = VBVisualTreeHelper.FindParentObjectInVisualTree(obj, typeof(VBDesign)) as VBDesign;
            return result;
        }

        public static VBDesignBase GetVBDesignBase(this AvaloniaObject obj)
        {
            VBDesignBase result = FindParentObjectInLogicalTree(obj, typeof(VBDesignBase)) as VBDesignBase;
            if (result == null)
                result = VBVisualTreeHelper.FindParentObjectInVisualTree(obj, typeof(VBDesignBase)) as VBDesignBase;
            return result;
        }

        public static void AppendMenu(this IACInteractiveObject obj, string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            try
            {
                IACObject reflectableObject = obj as IACObject;
                if (reflectableObject != null)
                {
                    ACMenuItemList acMenuList = reflectableObject.ReflectGetMenu(obj);
                    if ((acMenuList != null) && (acMenuList.Count > 0))
                    {
                        foreach (var acMenu in acMenuList)
                        {
                            // Gleiche Kommandos nicht doppelt eintragen
                            if (acMenuItemList.Where(c => c.CompareTo(acMenu) == 0).Any())
                                continue;
                            acMenuItemList.Add(acMenu);
                        }
                        //acMenuItemList.Add(new ACMenuItem(null, "-", "", null));
                    }
                }

                if (obj is AvaloniaObject)
                {
                    AvaloniaObject parentObj = LogicalTreeHelper.GetParent(obj as AvaloniaObject);
                    if (parentObj != null)
                    {
                        IACMenuBuilderWPFTree parentMenuBuilder = FindParentObjectInLogicalTree(parentObj as AvaloniaObject, typeof(IACMenuBuilderWPFTree)) as IACMenuBuilderWPFTree;
                        if (parentMenuBuilder != null)
                            parentMenuBuilder.AppendMenu(obj.VBContent, obj.GetType().Name, ref acMenuItemList);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBLogicalTreeHelper", "AppendMenu", msg);
            }
        }

        public static void GetDesignManagerMenu(this AvaloniaObject obj, string vbContent, ref ACMenuItemList acMenuItemList)
        {
            VBDesign vbDesign = obj.GetVBDesign();
            if (vbDesign != null)
            {
                IACComponentDesignManager designManager = vbDesign.GetDesignManager();
                if (designManager is IACMenuBuilder)
                {
                    IACMenuBuilder acMenuBuilder = designManager as IACMenuBuilder;
                    var m1 = acMenuBuilder.GetMenu(vbContent, "");
                    if (m1 != null && m1.Any())
                    {
                        foreach (var acMenutItem in m1)
                        {
                            acMenuItemList.Add(acMenutItem);
                        }
                    }
                }
            }

        }

        public static bool IsChildObjectInLogicalTree(AvaloniaObject objStart, AvaloniaObject objToFind, Type breakSearchAtType = null)
        {
            if ((objStart == null) || (objToFind == null))
                return false;
            if (objStart == objToFind)
                return true;
            foreach (object childObj in LogicalTreeHelper.GetChildren(objStart))
            {
                if (!(childObj is AvaloniaObject))
                    continue;
                if (breakSearchAtType != null && childObj != null && breakSearchAtType.IsAssignableFrom(childObj.GetType()))
                    return false;
                if (IsChildObjectInLogicalTree(childObj as AvaloniaObject, objToFind, breakSearchAtType))
                    return true;
            }
            return false;
        }


    }
}

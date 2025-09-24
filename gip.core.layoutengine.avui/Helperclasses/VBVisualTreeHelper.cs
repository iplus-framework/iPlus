using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using gip.core.datamodel;
using gip.ext.designer.avui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui.Helperclasses
{

    public static class VBVisualTreeHelper
    {
        public static AvaloniaObject FindObjectInVisualTree(AvaloniaObject obj, string PART_Name)
        {
            AvaloniaObject partObj = VBVisualTreeHelper.FindParentObjectInVisualTree(obj, PART_Name);
            if (partObj != null)
                return partObj;
            partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(obj, PART_Name);
            if (partObj != null)
                return partObj;
            return null;
        }

        public static AvaloniaObject FindObjectInVisualTree(AvaloniaObject obj, Type type)
        {
            AvaloniaObject partObj = VBVisualTreeHelper.FindParentObjectInVisualTree(obj, type);
            if (partObj != null)
                return partObj;
            partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(obj, type);
            if (partObj != null)
                return partObj;
            return null;
        }

        public static AvaloniaObject FindChildObjectInVisualTree(AvaloniaObject obj, string PART_Name)
        {
            if (obj == null)
                return null;
            if (obj is StyledElement)
            {
                StyledElement partObj = obj as StyledElement;
                if (partObj.Name == PART_Name)
                    return partObj;
            }

            Visual visual = obj as Visual;
            if (visual == null)
                return null;

            foreach (var child in visual.GetVisualChildren())
            {
                AvaloniaObject found = FindChildObjectInVisualTree(child, PART_Name);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static AvaloniaObject FindChildObjectInVisualTree(AvaloniaObject obj, Type type)
        {
            if (obj == null)
                return null;
            if (obj is StyledElement)
            {
                StyledElement partObj = obj as StyledElement;
                if (type.IsAssignableFrom(partObj.GetType()))
                    return partObj;
            }
            Visual visual = obj as Visual;
            if (visual == null)
                return null;

            foreach (var child in visual.GetVisualChildren())
            {
                AvaloniaObject found = FindChildObjectInVisualTree(child, type);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Sucht ein Child-Objekt bei dem ein Attribut "attributeName" mit Wert "value" vorkommt
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="attributeName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AvaloniaObject FindChildObjectInVisualTree<T>(AvaloniaObject obj, string attributeName, object value) 
        {
            if (obj == null)
                return null;
            if (obj is StyledElement)
            {
                StyledElement partObj = obj as StyledElement;
                Type t = partObj.GetType();
                PropertyInfo pi = t.GetProperty(attributeName);
                if (pi != null)
                {
                    Type tt = typeof(T);
                    T v1 = (T)Convert.ChangeType(pi.GetValue(partObj, null), tt);
                    T v2 = (T)Convert.ChangeType(value, tt);
                    if (((IComparable)v1).CompareTo((IComparable)v2) == 0)
                        return partObj;
                }
            }

            Visual visual = obj as Visual;
            if (visual == null)
                return null;

            foreach (var child in visual.GetVisualChildren())
            {
                AvaloniaObject found = FindChildObjectInVisualTree<T>(child, attributeName, value);
                if (found != null)
                    return found;
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

            Visual visual = obj as Visual;
            if (visual == null)
                return;

            foreach (var child in visual.GetVisualChildren())
            {
                FindChildObjects<T>(child, resultList);
            }
        }


        public static AvaloniaObject FindParentObjectInVisualTree(AvaloniaObject obj, string PART_Name)
        {
            if (obj == null)
                return null;
            if (obj is StyledElement)
            {
                StyledElement partObj = obj as StyledElement;
                if (partObj.Name == PART_Name)
                    return partObj;
            }
            Visual visual = obj as Visual;
            if (visual == null)
                return null;

            return FindParentObjectInVisualTree(visual.GetVisualParent(), PART_Name);
        }

        public static AvaloniaObject FindParentObjectInVisualTree(AvaloniaObject obj, Type[] types)
        {
            if (obj == null)
                return null;

            foreach (var type in types)
            {
                if (obj.GetType() == type)
                    return obj;
            }
            foreach (var type in types)
            {
                if (type.IsAssignableFrom(obj.GetType()))
                    return obj;
            }
            Visual visual = obj as Visual;
            if (visual == null)
                return null;

            return FindParentObjectInVisualTree(visual.GetVisualParent(), types);
        }

        public static AvaloniaObject FindParentObjectInVisualTree(AvaloniaObject obj, Type type)
        {
            if (obj == null)
                return null;
            if (obj.GetType() == type)
                return obj;
            else if (type.IsAssignableFrom(obj.GetType()))
                return obj;
            Visual visual = obj as Visual;
            if (visual == null)
                return null;
            return FindParentObjectInVisualTree(visual.GetVisualParent(), type);
        }

        public static AvaloniaObject FindObjectInLogicalAndVisualTree(AvaloniaObject obj, string PART_Name)
        {
            AvaloniaObject objFound = VBLogicalTreeHelper.FindObjectInLogicalTree(obj, PART_Name);
            if (objFound != null)
                return objFound;
            objFound = VBVisualTreeHelper.FindObjectInVisualTree(obj, PART_Name);
            if (objFound != null)
                return objFound;
            return null;
        }

        //public static VBDesign GetVBDesign(this AvaloniaObject obj)
        //{

        //    return FindParentObjectInVisualTree(obj, typeof(VBDesign)) as VBDesign;
        //}

        public static string GetVBContentAsXName(this IVBContent obj)
        {
            if (String.IsNullOrEmpty(obj.VBContent))
                return "";
            String result = obj.VBContent.Replace("\\", "");
            result = result.Replace("(", "");
            result = result.Replace(")", "");
            result = result.Replace("*", "");
            result = result.Replace("?", "");
            result = result.Replace("!", "");
            result = result.Replace("&", "");
            result = result.Replace("[", "");
            result = result.Replace("]", "");
            result = result.Replace("§", "");
            return result.Replace("_", "");
        }

        public static string GetVBContentsAsXName(AvaloniaObject fromChildObj, AvaloniaObject toParentObj)
        {
            if ((fromChildObj == null) || (fromChildObj == toParentObj))
                return "";
            Visual visual = fromChildObj as Visual;
            if (visual == null)
                return "";
            string xName = GetVBContentsAsXName(visual.GetVisualParent(), toParentObj);
            if ((fromChildObj is IVBContent) && (fromChildObj is Control))
            {
                if (String.IsNullOrEmpty(xName))
                    xName = (fromChildObj as Control).Name;
                else
                    xName += "\\" + (fromChildObj as Control).Name;
            }
            return xName;
        }

        public static Control FindChildVBContentObjectInVisualTree(Control startObj, string vbContentXName)
        {
            string[] result = vbContentXName.Split(new String[]{"\\"}, StringSplitOptions.RemoveEmptyEntries);
            if (result == null || !result.Any())
                return null;

            Control nextStartSearchElement = startObj;
            Control foundSearchElement = null;
            foreach (string nextVBContentXName in result)
            {
                foundSearchElement = SearchControl(nextStartSearchElement, nextVBContentXName);
                if (foundSearchElement == null)
                    return null;
                nextStartSearchElement = foundSearchElement;
            }
            return foundSearchElement;
        }


        /// <summary>
        /// Get the child Control with a given name
        /// from the visual tree of a parent Control.
        /// </summary>
        /// <param name="parentControl">Parent Control</param>
        /// <param name="childControlNameToSearch">Child Control name</param>
        /// <returns>Child Control with a given name</returns>
        public static Control SearchControl(Control parentControl, string childControlNameToSearch)
        {
            Control childControlFound = null;
            SearchControl(parentControl, ref childControlFound, childControlNameToSearch);
            return childControlFound;
        }
        /// <summary>
        /// Get All Child Control of given Control
        ///</summary>
        /// <param name="parentElement">Parent Control whose child Control's will be searched</param>
        /// <returns>List of Child Control</returns>
        public static List<Control> GetAllChildControl(Control parentElement)
        {
            List<Control> childControlFound = new List<Control>();
            SearchAllChildControl(parentElement, childControlFound);
            return childControlFound;
        }
        //SearchControl helper
        private static void SearchControl(Control parentControl, ref Control childControlToFind, string childControlName)
        {
            Visual visual = parentControl as Visual;
            if (visual == null)
                return;

            foreach (var childControl in visual.GetVisualChildren())
            {
                if (childControlToFind != null)
                    return;
                if (childControl is Control)
                {
                    if (childControl != null)
                    {
                        //if (childControl is VBVisual || childControl is VBVisualGroup)                           
                        if (childControl is IVBContent)
                        {
                            if ((childControl as Control).Name.Equals(childControlName))
                            {
                                childControlToFind = (childControl as Control);
                                return;
                            }
                            continue;
                        }
                        if (childControl is VBConnector)
                        {
                            if ((childControl as VBConnector).VBContent.Equals(childControlName))
                            {
                                childControlToFind = (childControl as Control);
                                return;
                            }
                            continue;
                        }
                    }
                    SearchControl((childControl as Control), ref childControlToFind, childControlName);
                    if (childControlToFind != null)
                        return;
                }
                //else if (childControl is CompositionVisual)
                //{
                //    CompositionVisual visualContainer = childControl as CompositionVisual;
                //    foreach (Visual visualChild in visualContainer.Children)
                //    {
                //        if (visualChild is Control)
                //        {
                //            if (visualChild != null && visualChild is IVBContent)
                //            {
                //                if ((visualChild as Control).Name.Equals(childControlName))
                //                {
                //                    childControlToFind = (visualChild as Control);
                //                    return;
                //                }
                //                continue;
                //            }
                //            SearchControl((visualChild as Control), ref childControlToFind, childControlName);
                //            break;
                //        }
                //    }
                //}
            }
        }
        //GetAllChildControl helper
        private static void SearchAllChildControl(Control parentControl, List<Control> allChildControl)
        {
            Visual visual = parentControl as Visual;
            if (visual == null)
                return;

            foreach (var childControl in visual.GetVisualChildren())
            {
                if (childControl is Control)
                {
                    allChildControl.Add((childControl as Control));
                    SearchAllChildControl((childControl as Control), allChildControl);
                }
            }
        }

        public static void DeInitVBControls(IACComponent acObject, Object guiObject)
        {
            if (guiObject == null)
                return;
            Visual visual = guiObject as Visual;
            if (visual == null)
                return;

            //foreach (object o in LogicalTreeHelper.GetChildren(depObject))
            //{
            //    DeInitVBControls(acObject, o);
            //}

            foreach (var childControl in visual.GetVisualChildren())
            {
                if (childControl is Control)
                {
                    DeInitVBControls(acObject, childControl);
                }
                //else if (childControl is ContainerVisual)
                //{
                //    ContainerVisual visualContainer = childControl as ContainerVisual;
                //    foreach (Visual visualChild in visualContainer.Children)
                //    {
                //        DeInitVBControls(acObject, visualChild);
                //    }
                //}
                else
                {
                    DeInitVBControls(acObject, childControl);
                }
            }
            if (visual is IVBContent)
            {
                IVBContent vbContentObj = visual as IVBContent;
                if (vbContentObj != null)
                    vbContentObj.DeInitVBControl(null);
            }
            visual.ClearAllBindings();
        }
    }

    /*public class MouseUtilities
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hwnd, ref Win32Point pt);

        public static Point GetMousePosition(Visual relativeTo)
        {
            Win32Point mouse = new Win32Point();
            GetCursorPos(ref mouse);
            System.Windows.Interop.HwndSource presentationSource =
                (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(relativeTo);
            ScreenToClient(presentationSource.Handle, ref mouse);
            GeneralTransform transform = relativeTo.TransformToAncestor(presentationSource.RootVisual);
            Point offset = transform.Transform(new Point(0, 0));
            return new Point(mouse.X - offset.X, mouse.Y - offset.Y);
        }
    }*/

}

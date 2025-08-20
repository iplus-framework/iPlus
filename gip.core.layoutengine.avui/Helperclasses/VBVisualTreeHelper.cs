using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using gip.core.datamodel;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Presentation;

namespace gip.core.layoutengine.avui.Helperclasses
{

    public static class VBVisualTreeHelper
    {
        public static DependencyObject FindObjectInVisualTree(DependencyObject obj, string PART_Name)
        {
            DependencyObject partObj = VBVisualTreeHelper.FindParentObjectInVisualTree(obj, PART_Name);
            if (partObj != null)
                return partObj;
            partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(obj, PART_Name);
            if (partObj != null)
                return partObj;
            return null;
        }

        public static DependencyObject FindObjectInVisualTree(DependencyObject obj, Type type)
        {
            DependencyObject partObj = VBVisualTreeHelper.FindParentObjectInVisualTree(obj, type);
            if (partObj != null)
                return partObj;
            partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(obj, type);
            if (partObj != null)
                return partObj;
            return null;
        }

        public static DependencyObject FindChildObjectInVisualTree(DependencyObject obj, string PART_Name)
        {
            if (obj == null)
                return null;
            if (obj is FrameworkElement)
            {
                FrameworkElement partObj = obj as FrameworkElement;
                if (partObj.Name == PART_Name)
                    return partObj;
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject found = FindChildObjectInVisualTree(VisualTreeHelper.GetChild(obj, i), PART_Name);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static DependencyObject FindChildObjectInVisualTree(DependencyObject obj, Type type)
        {
            if (obj == null)
                return null;
            if (obj is FrameworkElement)
            {
                FrameworkElement partObj = obj as FrameworkElement;
                if (type.IsAssignableFrom(partObj.GetType()))
                    return partObj;
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject found = FindChildObjectInVisualTree(VisualTreeHelper.GetChild(obj, i), type);
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
        public static DependencyObject FindChildObjectInVisualTree<T>(DependencyObject obj, string attributeName, object value) 
        {
            if (obj == null)
                return null;
            if (obj is FrameworkElement)
            {
                FrameworkElement partObj = obj as FrameworkElement;
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
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject found = FindChildObjectInVisualTree<T>(VisualTreeHelper.GetChild(obj, i), attributeName, value);
                if (found != null)
                    return found;
            }
            return null;
        }


        public static IEnumerable<T> FindChildObjects<T>(DependencyObject obj) where T : class
        {
            List<T> result = new List<T>();

            FindChildObjects<T>(obj, result);

            return result;
        }

        private static void FindChildObjects<T>(DependencyObject obj, List<T> resultList) where T : class
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

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject found = VisualTreeHelper.GetChild(obj, i);
                FindChildObjects<T>(found as DependencyObject, resultList);
            }
        }


        public static DependencyObject FindParentObjectInVisualTree(DependencyObject obj, string PART_Name)
        {
            if (obj == null)
                return null;
            if (obj is FrameworkElement)
            {
                FrameworkElement partObj = obj as FrameworkElement;
                if (partObj.Name == PART_Name)
                    return partObj;
            }
            return FindParentObjectInVisualTree(VisualTreeHelper.GetParent(obj), PART_Name);
        }

        public static DependencyObject FindParentObjectInVisualTree(DependencyObject obj, Type[] types)
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
            return FindParentObjectInVisualTree(VisualTreeHelper.GetParent(obj), types);
        }

        public static DependencyObject FindParentObjectInVisualTree(DependencyObject obj, Type type)
        {
            if (obj == null)
                return null;
            if (obj.GetType() == type)
                return obj;
            else if (type.IsAssignableFrom(obj.GetType()))
                return obj;
            return FindParentObjectInVisualTree(VisualTreeHelper.GetParent(obj), type);
        }

        public static DependencyObject FindObjectInLogicalAndVisualTree(DependencyObject obj, string PART_Name)
        {
            DependencyObject objFound = VBLogicalTreeHelper.FindObjectInLogicalTree(obj, PART_Name);
            if (objFound != null)
                return objFound;
            objFound = VBVisualTreeHelper.FindObjectInVisualTree(obj, PART_Name);
            if (objFound != null)
                return objFound;
            return null;
        }

        //public static VBDesign GetVBDesign(this DependencyObject obj)
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

        public static string GetVBContentsAsXName(DependencyObject fromChildObj, DependencyObject toParentObj)
        {
            if ((fromChildObj == null) || (fromChildObj == toParentObj))
                return "";
            string xName = GetVBContentsAsXName(VisualTreeHelper.GetParent(fromChildObj),toParentObj);
            if ((fromChildObj is IVBContent) && (fromChildObj is FrameworkElement))
            {
                if (String.IsNullOrEmpty(xName))
                    xName = (fromChildObj as FrameworkElement).Name;
                else
                    xName += "\\" + (fromChildObj as FrameworkElement).Name;
            }
            return xName;
        }

        public static FrameworkElement FindChildVBContentObjectInVisualTree(FrameworkElement startObj, string vbContentXName)
        {
            string[] result = vbContentXName.Split(new String[]{"\\"}, StringSplitOptions.RemoveEmptyEntries);
            if (result == null || !result.Any())
                return null;

            FrameworkElement nextStartSearchElement = startObj;
            FrameworkElement foundSearchElement = null;
            foreach (string nextVBContentXName in result)
            {
                foundSearchElement = SearchFrameworkElement(nextStartSearchElement, nextVBContentXName);
                if (foundSearchElement == null)
                    return null;
                nextStartSearchElement = foundSearchElement;
            }
            return foundSearchElement;
        }


        /// <summary>
        /// Get the child FrameworkElement with a given name
        /// from the visual tree of a parent FrameworkElement.
        /// </summary>
        /// <param name="parentFrameworkElement">Parent FrameworkElement</param>
        /// <param name="childFrameworkElementNameToSearch">Child FrameworkElement name</param>
        /// <returns>Child FrameworkElement with a given name</returns>
        public static FrameworkElement SearchFrameworkElement(FrameworkElement parentFrameworkElement, string childFrameworkElementNameToSearch)
        {
            FrameworkElement childFrameworkElementFound = null;
            SearchFrameworkElement(parentFrameworkElement, ref childFrameworkElementFound, childFrameworkElementNameToSearch);
            return childFrameworkElementFound;
        }
        /// <summary>
        /// Get All Child FrameworkElement of given FrameworkElement
        ///</summary>
        /// <param name="parentElement">Parent FrameworkElement whose child FrameworkElement's will be searched</param>
        /// <returns>List of Child FrameworkElement</returns>
        public static List<FrameworkElement> GetAllChildFrameworkElement(FrameworkElement parentElement)
        {
            List<FrameworkElement> childFrameworkElementFound = new List<FrameworkElement>();
            SearchAllChildFrameworkElement(parentElement, childFrameworkElementFound);
            return childFrameworkElementFound;
        }
        //SearchFrameworkElement helper
        private static void SearchFrameworkElement(FrameworkElement parentFrameworkElement, ref FrameworkElement childFrameworkElementToFind, string childFrameworkElementName)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parentFrameworkElement);
            if (childrenCount > 0)
            {
                DependencyObject childFrameworkElement = null;
                for (int i = 0; i < childrenCount; i++)
                {
                    if (childFrameworkElementToFind != null)
                        return;
                    childFrameworkElement = VisualTreeHelper.GetChild(parentFrameworkElement, i);
                    if (childFrameworkElement is FrameworkElement)
                    {
                        if (childFrameworkElement != null)
                        {
                            //if (childFrameworkElement is VBVisual || childFrameworkElement is VBVisualGroup)                           
                            if (childFrameworkElement is IVBContent)
                            {
                                if ((childFrameworkElement as FrameworkElement).Name.Equals(childFrameworkElementName))
                                {
                                    childFrameworkElementToFind = (childFrameworkElement as FrameworkElement);
                                    return;
                                }
                                continue;
                            }
                            if (childFrameworkElement is VBConnector)
                            {
                                if ((childFrameworkElement as VBConnector).VBContent.Equals(childFrameworkElementName))
                                {
                                    childFrameworkElementToFind = (childFrameworkElement as FrameworkElement);
                                    return;
                                }
                                continue;
                            }
                        }
                        SearchFrameworkElement((childFrameworkElement as FrameworkElement), ref childFrameworkElementToFind, childFrameworkElementName);
                        if (childFrameworkElementToFind != null)
                            return;
                    }
                    else if (childFrameworkElement is ContainerVisual)
                    {
                        ContainerVisual visualContainer = childFrameworkElement as ContainerVisual;
                        foreach (Visual visualChild in visualContainer.Children)
                        {
                            if (visualChild is FrameworkElement)
                            {
                                if (visualChild != null && visualChild is IVBContent)
                                {
                                    if ((visualChild as FrameworkElement).Name.Equals(childFrameworkElementName))
                                    {
                                        childFrameworkElementToFind = (visualChild as FrameworkElement);
                                        return;
                                    }
                                    continue;
                                }
                                SearchFrameworkElement((visualChild as FrameworkElement), ref childFrameworkElementToFind, childFrameworkElementName);
                                break;
                            }
                        }
                    }
                }
            }
        }
        //GetAllChildFrameworkElement helper
        private static void SearchAllChildFrameworkElement(FrameworkElement parentFrameworkElement, List<FrameworkElement> allChildFrameworkElement)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parentFrameworkElement);
            if (childrenCount > 0)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    DependencyObject childFrameworkElement = (FrameworkElement)VisualTreeHelper.GetChild(parentFrameworkElement, i);
                    if (childFrameworkElement is FrameworkElement)
                    {
                        allChildFrameworkElement.Add((childFrameworkElement as FrameworkElement));
                        SearchAllChildFrameworkElement((childFrameworkElement as FrameworkElement), allChildFrameworkElement);
                    }
                }
            }
        }

        public static void DeInitVBControls(IACComponent acObject, Object guiObject)
        {
            if (guiObject == null)
                return;
            DependencyObject depObject = guiObject as DependencyObject;
            if (depObject == null)
                return;

            //foreach (object o in LogicalTreeHelper.GetChildren(depObject))
            //{
            //    DeInitVBControls(acObject, o);
            //}

            int childrenCount = VisualTreeHelper.GetChildrenCount(depObject);
            if (childrenCount > 0)
            {
                DependencyObject childFrameworkElement = null;
                for (int i = 0; i < childrenCount; i++)
                {
                    childFrameworkElement = VisualTreeHelper.GetChild(depObject, i);
                    if (childFrameworkElement is FrameworkElement)
                    {
                        DeInitVBControls(acObject, childFrameworkElement);
                    }
                    else if (childFrameworkElement is ContainerVisual)
                    {
                        ContainerVisual visualContainer = childFrameworkElement as ContainerVisual;
                        foreach (Visual visualChild in visualContainer.Children)
                        {
                            DeInitVBControls(acObject, visualChild);
                        }
                    }
                    else
                    {
                        DeInitVBControls(acObject, childFrameworkElement);
                    }
                }
            }
            if (depObject is IVBContent)
            {
                IVBContent vbContentObj = depObject as IVBContent;
                if (vbContentObj != null)
                    vbContentObj.DeInitVBControl(null);
            }
            BindingOperations.ClearAllBindings(guiObject as DependencyObject);
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

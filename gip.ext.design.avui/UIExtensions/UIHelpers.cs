// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace gip.ext.design.avui
{
	/// <summary>
	/// Contains helper methods for UI. 
	/// </summary>
	public static class UIHelpers
	{
		/// <summary>
		/// Gets the parent. Which tree the parent is retrieved from depends on the parameters.
		/// </summary>
		/// <param name="child">The child to get parent for.</param>
		/// <param name="searchCompleteVisualTree">If true the parent in the visual tree is returned, if false the parent may be retrieved from another tree depending on the child type.</param>
		/// <returns>The parent element, and depending on the parameters its retrieved from either visual tree, logical tree or a tree not strictly speaking either the logical tree or the visual tree.</returns>
		public static AvaloniaObject GetParentObject(this AvaloniaObject child, bool searchCompleteVisualTree)
		{
			if (child == null) 
				return null;
            Visual visual = child as Visual;
			if (visual == null) 
				return null;	


            if (!searchCompleteVisualTree) {
                // Flowdocuments not supported in Avalonia:
                //var contentElement = child as ContentElement;
                //if (contentElement != null)
                //{
                //	AvaloniaObject parent = ContentOperations.GetParent(contentElement);
                //	if (parent != null) return parent;

                //	var fce = contentElement as FrameworkContentElement;
                //	return fce != null ? fce.Parent : null;
                //}

                StyledElement frameworkElement = child as StyledElement;
				if (frameworkElement != null)
				{
					AvaloniaObject parent = frameworkElement.Parent;
					if (parent != null) 
						return parent;
				}
			}
#nullable enable
			Visual? visualparent = visual.GetVisualParent();
			if (visualparent == null)
				return null;
			return visualparent;
#nullable disable
        }

        /// <summary>
        /// Gets first parent element of the specified type. Which tree the parent is retrieved from depends on the parameters.
        /// </summary>
        /// <param name="child">The child to get parent for.</param>
        /// <param name="searchCompleteVisualTree">If true the parent in the visual tree is returned, if false the parent may be retrieved from another tree depending on the child type.</param>
        /// <returns>
        /// The first parent element of the specified type, and depending on the parameters its retrieved from either visual tree, logical tree or a tree not strictly speaking either the logical tree or the visual tree.
        /// null is returned if no parent of the specified type is found.
        /// </returns>
        public static T TryFindParent<T>(this AvaloniaObject child, bool searchCompleteVisualTree = false) where T : AvaloniaObject
		{
			AvaloniaObject parentObject = GetParentObject(child, searchCompleteVisualTree);

			if (parentObject == null) return null;

			T parent = parentObject as T;
			if (parent != null)
			{
				return parent;
			}

			return TryFindParent<T>(parentObject);
		}

		/// <summary>
		/// Returns the first child of the specified type found in the visual tree.
		/// </summary>
		/// <param name="parent">The parent element where the search is started.</param>
		/// <returns>The first child of the specified type found in the visual tree, or null if no parent of the specified type is found.</returns>
		public static T TryFindChild<T>(this AvaloniaObject parent) where T : Visual
        {
            if (parent == null)
                return null;
            Visual visual = parent as Visual;
            if (visual == null)
                return null;
            foreach (Visual child in visual.GetVisualChildren())
			{ 
				if (child is T)
				{
					return (T)child;
				}
				T child2 = TryFindChild<T>(child);
				if (child2 != null)
				{
					return (T)child2;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the first child of the specified type and with the specified name found in the visual tree.
		/// </summary>
		/// <param name="parent">The parent element where the search is started.</param>
		/// <param name="childName">The name of the child element to find, or an empty string or null to only look at the type.</param>
		/// <returns>The first child that matches the specified type and child name, or null if no match is found.</returns>
		public static T TryFindChild<T>(this AvaloniaObject parent, string childName) where T : Visual
        {
			if (parent == null) 
				return null;
            Visual visual = parent as Visual;
            if (visual == null)
                return null;
            T foundChild = null;
            foreach (Visual child in visual.GetVisualChildren())
            {           
                var childType = child as T;
				if (childType == null)
				{
					foundChild = TryFindChild<T>(child, childName);
					if (foundChild != null) break;
				}
				else if (!string.IsNullOrEmpty(childName))
				{
					if (child != null && child.Name == childName)
					{
						foundChild = (T)child;
						break;
					}
				}
				else
				{
					foundChild = (T)child;
					break;
				}
			}
			return foundChild;
		}

		/// <summary>
		///   Returns the first ancestor of specified type
		/// </summary>
		public static T FindAncestor<T>(AvaloniaObject current) where T : AvaloniaObject
		{
			current = GetVisualOrLogicalParent(current);

			while (current != null)
			{
				if (current is T)
				{
					return (T)current;
				}
				current = GetVisualOrLogicalParent(current);
			}

			return null;
		}

		private static AvaloniaObject GetVisualOrLogicalParent(AvaloniaObject obj)
		{
			if (obj is Visual)// || obj is Visual3D)
			{
#nullable enable
                Visual? visualparent = (obj as Visual)?.GetVisualParent();
                if (visualparent == null)
                    return null;
                return visualparent;
#nullable disable
            }
			return null;
            //return VisualExtensions.GetParent(obj);
		}
	}
}

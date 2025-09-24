using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;


namespace gip.ext.design.avui
{
    /// <summary>
    /// Helperclass for compatibility with WPF's VisualTreeHelper
    /// </summary>
    public static class LogicalTreeHelper
    {
        /// <summary>
        /// Given an element in the logical tree to start searching from,
        /// searches all its descendent nodes in the logical tree a node whose Name 
        /// matches the specified elementName.
        /// The given DependencyObject must be either a FrameworkElement or FrameworkContentElement.
        /// </summary>
        public static AvaloniaObject FindLogicalNode(AvaloniaObject logicalTreeNode, string elementName)
        {
            ILogical depObject = logicalTreeNode as ILogical;
            if (depObject == null)
                return null;

            // Check current node first
            if (logicalTreeNode is StyledElement styledElement && styledElement.Name == elementName)
                return logicalTreeNode;

            // Recursively search children
            foreach (object child in depObject.GetLogicalChildren())
            {
                if (child is AvaloniaObject childAvaloniaObject)
                {
                    AvaloniaObject result = FindLogicalNode(childAvaloniaObject, elementName);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the logical children of the specified element.
        /// </summary>
        /// <param name="obj">The element to get children for.</param>
        /// <returns>An enumerable of logical children.</returns>
        public static IEnumerable<object> GetChildren(AvaloniaObject obj)
        {
            if (obj is ILogical logical)
            {
                return logical.GetLogicalChildren();
            }
            return Enumerable.Empty<object>();
        }

        /// <summary>
        /// Gets the logical parent of the specified element.
        /// </summary>
        /// <param name="obj">The element to get parent for.</param>
        /// <returns>The logical parent element.</returns>
        public static AvaloniaObject GetParent(AvaloniaObject obj)
        {
            if (obj is ILogical logical)
            {
                return logical.GetLogicalParent() as AvaloniaObject;
            }
            return null;
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using gip.ext.design.avui;
using System;

namespace gip.ext.designer.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Avalonia implementation of WPF's DataTemplateSelector functionality.
    /// 
    /// Key differences from WPF:
    /// - Avalonia doesn't have DataTemplateSelector or ItemTemplateSelector
    /// - IDataTemplate.Build() method creates controls directly without container reference
    /// - No access to visual tree container during template selection
    /// - Uses modern C# pattern matching for cleaner type-based selection
    /// 
    /// This implementation handles:
    /// - Point objects: Displays formatted coordinates
    /// - String objects: Displays the string value
    /// - Other objects: Displays ToString() representation
    /// </summary>
    public class CollectionTemplateSelector : IDataTemplate
    {
        public Control Build(object param)
        {
            if (param is not DesignItem designItem)
                return new TextBlock { Text = "No data" };

            // Handle different types of components using pattern matching
            return designItem.Component switch
            {
                Point point => CreatePointView(point),
                string str => CreateStringView(str),
                _ => CreateDefaultView(designItem.Component)
            };
        }

        public bool Match(object data)
        {
            // This template selector works with DesignItem objects
            return data is DesignItem;
        }

        /// <summary>
        /// Creates a view for Point objects, displaying X and Y coordinates
        /// </summary>
        private Control CreatePointView(Point point)
        {
            var stackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
            stackPanel.Children.Add(new TextBlock { Text = "Point (" });
            stackPanel.Children.Add(new TextBlock { Text = point.X.ToString("F2") });
            stackPanel.Children.Add(new TextBlock { Text = " / " });
            stackPanel.Children.Add(new TextBlock { Text = point.Y.ToString("F2") });
            stackPanel.Children.Add(new TextBlock { Text = ")" });
            return stackPanel;
        }

        /// <summary>
        /// Creates a view for string objects
        /// </summary>
        private Control CreateStringView(string str)
        {
            return new TextBlock { Text = str ?? "String" };
        }

        /// <summary>
        /// Creates a default view for other object types
        /// </summary>
        private Control CreateDefaultView(object component)
        {
            return new TextBlock { Text = component?.ToString() ?? "No data" };
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using System;
using System.Globalization;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Attached behavior that toggles the visibility of a VBRibbonBSODefault 
    /// when a button is clicked. The ribbon is searched in the active document's content.
    /// </summary>
    public class RibbonSwitchBehavior
    {
        public static readonly AttachedProperty<bool> IsRibbonSwitchProperty =
            AvaloniaProperty.RegisterAttached<RibbonSwitchBehavior, Button, bool>(
                "IsRibbonSwitch",
                defaultValue: false);

        static RibbonSwitchBehavior()
        {
            // Register a class handler for Button.ClickEvent that checks our attached property
            Button.ClickEvent.AddClassHandler<Button>((btn, e) =>
            {
                // Only handle clicks from buttons that have IsRibbonSwitch="True"
                if (!btn.GetValue(IsRibbonSwitchProperty).Equals(true))
                    return;

                // Find the DocumentTabStripItem that owns this button (the tab header)
                var tabStripItem = btn.GetVisualAncestors().OfType<DocumentTabStripItem>().FirstOrDefault();
                if (tabStripItem == null)
                    return;

                // Get the IDockable from the tab strip item's DataContext
                var dockable = tabStripItem.DataContext as IDockable;
                if (dockable == null)
                    return;

                // Find the DocumentControl ancestor
                var documentControl = btn.GetVisualAncestors().OfType<DocumentControl>().FirstOrDefault();
                if (documentControl == null)
                    return;

                // Find the VBRibbonBSODefault that belongs to THIS specific dockable
                var ribbon = FindRibbonForDockable(documentControl, dockable);
                if (ribbon != null)
                {
                    ribbon.IsVisible = !ribbon.IsVisible;
                    e.Handled = true;
                }
            });
        }

        /// <summary>
        /// Searches for a VBRibbonBSODefault that belongs to the given dockable.
        /// The ribbon is inside the content area of the dockable, not just anywhere in the DocumentControl.
        /// </summary>
        private static VBRibbonBSODefault FindRibbonForDockable(DocumentControl docControl, IDockable targetDockable)
        {
            // Find the PART_Border which contains the content area
            foreach (var visual in docControl.GetVisualDescendants())
            {
                if (visual is Border border && border.Name == "PART_Border")
                {
                    // Search within PART_Border for a DockableControl/ContentControl whose DataContext matches the target dockable
                    foreach (var descendant in border.GetVisualDescendants())
                    {
                        if (descendant is Control ctrl && ctrl.DataContext == targetDockable)
                        {
                            // Found the content control for this dockable - search within it for the ribbon
                            foreach (var child in ctrl.GetVisualDescendants())
                            {
                                if (child is VBRibbonBSODefault ribbon)
                                    return ribbon;
                            }
                            // Also check logical descendants
                            foreach (var logical in ctrl.GetLogicalDescendants())
                            {
                                if (logical is VBRibbonBSODefault ribbon)
                                    return ribbon;
                            }
                            // Reached this dockable's content area but found no ribbon
                            return null;
                        }
                    }
                    // Found PART_Border but couldn't find a matching content control
                    return null;
                }
            }

            // Fallback: if PART_Border is not found, search all descendants for a control with matching DataContext
            foreach (var visual in docControl.GetVisualDescendants())
            {
                if (visual is VBRibbonBSODefault ribbon)
                {
                    // Check if this ribbon's ancestor has the matching DataContext
                    var ancestorWithDataContext = ribbon.GetVisualAncestors()
                        .OfType<Control>()
                        .FirstOrDefault(c => c.DataContext == targetDockable);
                    if (ancestorWithDataContext != null)
                        return ribbon;
                }
            }

            return null;
        }

        public static void SetIsRibbonSwitch(Button element, bool value)
        {
            element.SetValue(IsRibbonSwitchProperty, value);
        }

        public static bool GetIsRibbonSwitch(Button element)
        {
            return element.GetValue(IsRibbonSwitchProperty);
        }
    }

    /// <summary>
    /// Converter that checks if the IDockable's Content has a ribbon (HasRibbon attached property).
    /// Used to bind the ribbon switch button's IsVisible to whether the document has a ribbon.
    /// </summary>
    public class HasRibbonConverter : IValueConverter
    {
        public static readonly HasRibbonConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // IDockable doesn't have Content property, but Document and Tool (both extending DockableBase) do.
            // Since Document and Tool are Avalonia Objects, we can use GetValue with ContentProperty.
            if (value is AvaloniaObject dockable)
            {
                // Document.ContentProperty and Tool.ContentProperty both define "Content" property
                // They are equivalent AvaloniaProperty<object?> instances
                var content = dockable.GetValue(Tool.ContentProperty);
                if (content is Control ctrl)
                {
                    return VBDockingManager.GetHasRibbon(ctrl);
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

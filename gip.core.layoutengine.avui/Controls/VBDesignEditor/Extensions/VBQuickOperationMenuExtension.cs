using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Controls;
using gip.ext.designer.avui.Extensions;
using System;

namespace gip.core.layoutengine.avui
{
    class VBQuickOperationMenu : QuickOperationMenu
    {
        protected override MenuItem CreateMenuItem(string header)
        {
            return new VBMenuItem() { Header = header };
        }
    }

    [ExtensionFor(typeof(Control), OverrideExtension = typeof(QuickOperationMenuExtension))]
    class VBQuickOperationMenuExtension : QuickOperationMenuExtension
    {
        public override QuickOperationMenu CreateMainMenu()
        {
            return new VBQuickOperationMenu();
        }

        public override MenuItem CreateMenuItem(string header)
        {
            return new VBMenuItem() { Header = header };
        }

        public override Separator CreateSeparator()
        {
            return new VBMenuSeparator();
        }

        public static void MainHeaderClick(object sender, RoutedEventArgs e, DesignItem extendedItem, QuickOperationMenu _menu, VBDockingManager DockingManager, string WindowTitle)
        {
            MenuItem clickedOn = e.Source as MenuItem;
            if (clickedOn == null && e.Source is Visual sourceVisual)
            {
                clickedOn = sourceVisual.FindAncestorOfType<MenuItem>();
            }
            if (clickedOn == null)
            {
                clickedOn = sender as MenuItem;
            }

            if (clickedOn != null)
            {
                var parent = clickedOn.Parent as MenuItem;
                if (parent != null)
                {
                    if ((string)clickedOn.Header == "Edit Style Setter")
                    {
                        if (DockingManager != null)
                        {
                            var styleProp = extendedItem.Properties.GetProperty(Control.ThemeProperty);
                            var settersProp = styleProp?.Value?.Properties.GetProperty("Setters");
                            if (settersProp != null)
                            {
                                var editor = new VBSettersCollectionEditor();
                                editor.InitEditor(extendedItem, settersProp);
                                _ = DockingManager.ShowFloatingWindowAsync(
                                    editor,
                                    "Style Setter: " + WindowTitle,
                                    new Size(750, 400));
                            }
                        }
                        return;
                    }
                    else if ((string)clickedOn.Header == "Edit Style Trigger")
                    {
                        if (DockingManager != null)
                        {
                            DesignItemProperty triggersProp = extendedItem.Properties.GetAttachedProperty(Interaction.BehaviorsProperty);
                            if (triggersProp == null)
                            {
                                var styleProp = extendedItem.Properties.GetProperty(Control.ThemeProperty);
                                if (styleProp != null && styleProp.Value != null)
                                {
                                    triggersProp = styleProp.Value.Properties.GetProperty("Triggers");
                                }
                            }

                            if (triggersProp != null)
                            {
                                var editor = new VBTriggersCollectionEditor();
                                editor.InitEditor(extendedItem, triggersProp);
                                _ = DockingManager.ShowFloatingWindowAsync(
                                    editor,
                                    "Style Trigger: " + WindowTitle,
                                    new Size(750, 500));
                            }
                        }
                        return;
                    }
                    else if ((string)clickedOn.Header == "Delete")
                    {
                        if (extendedItem.Component is VBVisual)
                        {
                            VBVisual item = extendedItem.Component as VBVisual;
                            if (item != null && item.BSOACComponent != null && item.BSOACComponent is IACComponentDesignManager)
                                if(((IACComponentDesignManager)item.BSOACComponent).DeleteItem(item))
                                    return;
                        }
                        else if (extendedItem.Component is VBEdge)
                        {
                            VBEdge item = extendedItem.Component as VBEdge;
                            if (item != null && item.BSOACComponent != null && item.BSOACComponent is IACComponentDesignManager)
                                if (((IACComponentDesignManager)item.BSOACComponent).DeleteItem(item))
                                    return;
                        }
                        else if (extendedItem.Component is VBVisualGroup)
                        {
                            VBVisualGroup item = extendedItem.Component as VBVisualGroup;
                            if(item != null && item.BSOACComponent != null && item.BSOACComponent is IACComponentDesignManager)
                                if (((IACComponentDesignManager)item.BSOACComponent).DeleteItem(item))
                                    return;
                        }
                    }
                }
            }
            QuickOperationMenuExtension.MainHeader_PointerPressed(sender, e, extendedItem, _menu);
        }

        public override void OnSubMenuItemClick(object sender, RoutedEventArgs e)
        {
            VBQuickOperationMenuExtension.MainHeaderClick(sender, e, this.ExtendedItem, _menu, DockingManager, WindowTitle);
        }

        public static string GetWindowTitle(DesignItem extendedItem)
        {
            string title = "";
            if (extendedItem.View is IACInteractiveObject)
                title += extendedItem.Properties["VBContent"].ValueOnInstance as String;
            else
                title += "(" + extendedItem.ComponentType.Name + ")";
            if (!String.IsNullOrEmpty(extendedItem.Name))
                title += ", " + extendedItem.Name;

            return title;
        }

        protected string WindowTitle
        {
            get
            {
                return GetWindowTitle(this.ExtendedItem);
            }
        }

        public static VBDockingManager GetDockingManager(DesignItem extendedItem)
        {
            var panel = extendedItem.Services.GetService(typeof(IDesignPanel)) as IDesignPanel;
            return VBVisualTreeHelper.FindParentObjectInVisualTree(panel as AvaloniaObject, typeof(VBDockingManager)) as VBDockingManager;
        }

        private VBDockingManager _DockingManager;
        protected VBDockingManager DockingManager
        {
            get
            {
                if (_DockingManager != null)
                    return _DockingManager;
                var panel = this.ExtendedItem.Services.GetService(typeof(IDesignPanel)) as IDesignPanel;
                _DockingManager = GetDockingManager(this.ExtendedItem);
                return _DockingManager;
            }
        }
    }
}

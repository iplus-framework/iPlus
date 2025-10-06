using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

        public static void MainHeaderClick(object sender, PointerPressedEventArgs e, DesignItem extendedItem, QuickOperationMenu _menu, VBDockingManager DockingManager, string WindowTitle)
        {
            var clickedOn = e.Source as MenuItem;
            if (clickedOn != null)
            {
                var parent = clickedOn.Parent as MenuItem;
                if (parent != null)
                {
                    if ((string)clickedOn.Header == "Edit Style Setter")
                    {
                        if (DockingManager != null)
                        {
                            var editor = new VBStyleSetterWindow(DockingManager);
                            editor.LoadItemsCollection(extendedItem);
                            editor.Title = "Style Setter: " + WindowTitle;
                            editor.Show(e);
                            Rect wndRect = new Rect(DockingManager.PointToScreen(e.GetPosition(DockingManager)).ToPoint(1.0), new Size(750, 400));
                            (editor.VBDockingPanel as VBDockingPanelToolWindow).FloatingWindow(wndRect);
                        }
                        return;
                    }
                    else if ((string)clickedOn.Header == "Edit Style Trigger")
                    {
                        if (DockingManager != null)
                        {
                            var editor = new VBStyleTriggerWindow(DockingManager);
                            editor.LoadItemsCollection(extendedItem);
                            editor.Title = "Style Trigger: " + WindowTitle;
                            editor.Show(e);
                            Rect wndRect = new Rect(DockingManager.PointToScreen(e.GetPosition(DockingManager)).ToPoint(1.0), new Size(750, 500));
                            (editor.VBDockingPanel as VBDockingPanelToolWindow).FloatingWindow(wndRect);
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

        public override void MainHeader_PointerPressed(object sender, PointerPressedEventArgs e)
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

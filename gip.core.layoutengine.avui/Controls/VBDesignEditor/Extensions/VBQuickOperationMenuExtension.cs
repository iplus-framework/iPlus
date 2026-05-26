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
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    class VBQuickOperationMenu : QuickOperationMenu
    {
        protected override MenuItem CreateMenuItem(string header)
        {
            return new VBMenuItem() { Header = header };
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
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
                            var styleProp = EnsureThemeProperty(extendedItem);
                            var settersProp = styleProp?.Value?.Properties.GetProperty("Setters");
                            if (settersProp != null)
                            {
                                var editor = new VBSettersCollectionEditor();
                                editor.InitEditor(extendedItem, settersProp);
                                _ = DockingManager.ShowFloatingWindowAsync(
                                    editor,
                                    "Style Setter: " + WindowTitle,
                                    new Size(750, 400), 
                                    true, Global.ControlModes.Hidden, Global.ControlModes.Enabled, 
                                    DockingManager.GetOppositeToolDockPosition());
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
                                var styleProp = EnsureThemeProperty(extendedItem);
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
                                    new Size(750, 500),
                                    true, Global.ControlModes.Hidden, Global.ControlModes.Enabled, 
                                    DockingManager.GetOppositeToolDockPosition());
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

        private static DesignItemProperty EnsureThemeProperty(DesignItem extendedItem)
        {
            if (extendedItem == null)
                return null;

            var themeProperty = extendedItem.Properties.GetProperty(Control.ThemeProperty);
            if (themeProperty == null)
                return null;

            if (themeProperty.Value == null)
            {
                var themeInstance = CreateThemeInstance(themeProperty.ReturnType, extendedItem.ComponentType);
                if (themeInstance != null)
                {
                    var themeItem = extendedItem.Services.Component.RegisterComponentForDesigner(themeInstance);
                    EnsureThemeTargetType(themeItem, extendedItem.ComponentType);
                    themeProperty.SetValue(themeItem);
                }
            }
            else
            {
                EnsureThemeTargetType(themeProperty.Value, extendedItem.ComponentType);
            }

            return themeProperty;
        }

        private static void EnsureThemeTargetType(DesignItem themeItem, Type targetType)
        {
            if (themeItem == null || targetType == null)
                return;

            var targetTypeProperty = themeItem.Properties.HasProperty("TargetType");
            if (targetTypeProperty == null)
                return;

            var xamlTypeName = BuildXamlTypeName(themeItem, targetType);
            if (!string.IsNullOrWhiteSpace(xamlTypeName))
                targetTypeProperty.SetValue(xamlTypeName);
        }

        private static string BuildXamlTypeName(DesignItem themeItem, Type targetType)
        {
            // Prefer the shortest valid XAML type name so TargetType persists as text
            // (e.g. "Rectangle" or "local:MyControl") instead of a RuntimeType object node.
            const BindingFlags nonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
            const BindingFlags nonPublicInstancePublic = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            try
            {
                var xamlObjectProperty = themeItem.GetType().GetProperty("XamlObject", nonPublicInstancePublic);
                var xamlObject = xamlObjectProperty?.GetValue(themeItem);
                if (xamlObject == null)
                    return targetType.Name;

                var ownerDocumentProperty = xamlObject.GetType().GetProperty("OwnerDocument", nonPublicInstancePublic);
                var ownerDocument = ownerDocumentProperty?.GetValue(xamlObject);
                if (ownerDocument == null)
                    return targetType.Name;

                var xmlElementProperty = xamlObject.GetType().GetProperty("XmlElement", nonPublicInstancePublic);
                var xmlElement = xmlElementProperty?.GetValue(xamlObject);

                var getNamespaceForMethod = ownerDocument.GetType().GetMethod("GetNamespaceFor", nonPublicInstance, null, new[] { typeof(Type), typeof(bool) }, null);
                var ns = getNamespaceForMethod?.Invoke(ownerDocument, new object[] { targetType, false }) as string;
                if (string.IsNullOrEmpty(ns))
                    return targetType.Name;

                string prefix = string.Empty;
                var getPrefixWithElementMethod = ownerDocument.GetType().GetMethod("GetPrefixForNamespace", nonPublicInstance, null, new[] { xmlElement?.GetType(), typeof(string) }, null);
                if (getPrefixWithElementMethod != null && xmlElement != null)
                {
                    prefix = getPrefixWithElementMethod.Invoke(ownerDocument, new object[] { xmlElement, ns }) as string;
                }
                else
                {
                    var getPrefixMethod = ownerDocument.GetType().GetMethod("GetPrefixForNamespace", nonPublicInstance, null, new[] { typeof(string) }, null);
                    prefix = getPrefixMethod?.Invoke(ownerDocument, new object[] { ns }) as string;
                }

                if (string.IsNullOrEmpty(prefix))
                    return targetType.Name;

                return prefix + ":" + targetType.Name;
            }
            catch
            {
                return targetType.Name;
            }
        }

        private static object CreateThemeInstance(Type themeType, Type targetType)
        {
            if (themeType == null)
                return null;

            object themeInstance = null;

            var ctorWithTargetType = themeType.GetConstructor(new[] { typeof(Type) });
            if (ctorWithTargetType != null)
            {
                try
                {
                    themeInstance = ctorWithTargetType.Invoke(new object[] { targetType });
                }
                catch
                {
                    themeInstance = null;
                }
            }

            if (themeInstance == null)
            {
                var defaultCtor = themeType.GetConstructor(Type.EmptyTypes);
                if (defaultCtor == null)
                    return null;

                try
                {
                    themeInstance = defaultCtor.Invoke(null);
                }
                catch
                {
                    return null;
                }
            }

            var targetTypeProperty = themeType.GetProperty("TargetType");
            if (targetTypeProperty != null && targetTypeProperty.CanWrite)
            {
                try
                {
                    targetTypeProperty.SetValue(themeInstance, targetType);
                }
                catch
                {
                    // Ignore if this Avalonia version uses a different theme model.
                }
            }

            return themeInstance;
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

// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui.Extensions;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.PropertyGrid.Editors;
using gip.ext.design.avui;
using gip.ext.designer.avui.OutlineView;
using System.Windows.Media;
using gip.core.datamodel;

namespace gip.ext.designer.avui.Extensions
{
    public interface IQuickOperationMenuItemBuilder
    {
        QuickOperationMenu CreateMainMenu();
        MenuItem CreateMenuItem(string header);
        Separator CreateSeparator();
        int BuildMenu();
        void MainHeaderClick(object sender, RoutedEventArgs e);
    }


    /// <summary>
    /// Extends the Quick operation menu for the designer.
    /// </summary>
    [ExtensionFor(typeof(FrameworkElement))]
    public class QuickOperationMenuExtension : PrimarySelectionAdornerProvider, IQuickOperationMenuItemBuilder
    {
        protected QuickOperationMenu _menu;
        private KeyBinding _keyBinding;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _menu = CreateMainMenu();
            _menu.Loaded += OnMenuLoaded;

            RelativePlacement placement = new RelativePlacement(HorizontalAlignment.Right, VerticalAlignment.Top) { XOffset = 7, YOffset = 3.5 };

            if (ExtendedItem.View != null && ExtendedItem.View.RenderTransform != null)
            {
                RotateTransform rotateObj = null;
                if (ExtendedItem.View.RenderTransform is TransformGroup)
                {
                    TransformGroup transformGrp = ExtendedItem.View.RenderTransform as TransformGroup;
                    foreach (Transform transfObj in transformGrp.Children)
                    {
                        if (transfObj is RotateTransform)
                        {
                            rotateObj = transfObj as RotateTransform;
                            break;
                        }
                    }
                }
                else if (ExtendedItem.View.RenderTransform is RotateTransform)
                {
                    rotateObj = ExtendedItem.View.RenderTransform as RotateTransform;
                }

                RotateTransform rotateMenu = new RotateTransform();
                if (rotateObj != null)
                {
                    if ((rotateObj.Angle > 45 && rotateObj.Angle <= 135) || (rotateObj.Angle < -225 && rotateObj.Angle >= -315))
                    {
                        placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top) { XOffset = 3.5, YOffset = -7 };
                    }
                    else if ((rotateObj.Angle > 135 && rotateObj.Angle <= 225) || (rotateObj.Angle < -135 && rotateObj.Angle >= -225))
                    {
                        placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Bottom) { XOffset = -7, YOffset = -3.5 };
                    }
                    else if ((rotateObj.Angle > 225 && rotateObj.Angle <= 315) || (rotateObj.Angle < -45 && rotateObj.Angle >= -135))
                    {
                        placement = new RelativePlacement(HorizontalAlignment.Right, VerticalAlignment.Bottom) { XOffset = -3.5, YOffset = 7 };
                    }
                    //else if ((rotateObj.Angle > 315) || (rotateObj.Angle < 0 && rotateObj.Angle >= -45))
                    //{
                    //    placement = new RelativePlacement(HorizontalAlignment.Right, VerticalAlignment.Bottom) { XOffset = -3.5, YOffset = 7 };
                    //}
                    if (rotateObj.Angle > 0 || rotateObj.Angle < 0)
                        rotateMenu.Angle = rotateObj.Angle * (-1);
                    _menu.RenderTransform = rotateMenu;
                }
            }

            this.AddAdorners(placement, _menu);

            var kbs = this.ExtendedItem.Services.GetService(typeof(IKeyBindingService)) as IKeyBindingService;
            var command = new DesignCommand(delegate
            {
                _menu.MainHeader.IsSubmenuOpen = true;
                _menu.MainHeader.Focus();
            }, delegate
            {
                return true;
            });
            _keyBinding = new KeyBinding(command, Key.Enter, ModifierKeys.Alt);
            if (kbs != null)
                kbs.RegisterBinding(_keyBinding);
        }

        public virtual QuickOperationMenu CreateMainMenu()
        {
            return new QuickOperationMenu();
        }

        public virtual MenuItem CreateMenuItem(string header)
        {
            return new MenuItem() { Header = header };
        }

        public virtual Separator CreateSeparator()
        {
            return new Separator();
        }

        public virtual int BuildMenu()
        {
            return BuildMenu(this.ExtendedItem, _menu, this);
        }

        public static int BuildMenu(DesignItem extendedItem, QuickOperationMenu _menu, IQuickOperationMenuItemBuilder menuBuilder)
        {
            int menuItemsAdded = 0;
            var view = extendedItem.View;
            if (_menu.MainHeader == null)
                return menuItemsAdded;
            if (_menu.MainHeader.Items.Count > 0)
                return 0;

            if (view != null)
            {
                string setValue;

                if (extendedItem.View is IACObject)
                {
                    IACObject acObject = extendedItem.View as IACObject;

                    if (acObject.ACType != null)
                    {
                        if (!string.IsNullOrEmpty(acObject.ACType.Comment))
                        {
                            _menu.AddSubMenuInTheHeader(menuBuilder.CreateMenuItem("Help " + acObject.ACType.ACIdentifier));
                            menuItemsAdded++;
                        }
                    }
                }

                _menu.AddSubMenuInTheHeader(menuBuilder.CreateMenuItem("Delete"));
                menuItemsAdded++;

                if (view is ItemsControl)
                {
                    _menu.AddSubMenuInTheHeader(menuBuilder.CreateMenuItem("Edit Items"));
                    menuItemsAdded++;
                }
                if (view is FrameworkElement)
                {
                    _menu.AddSubMenuInTheHeader(menuBuilder.CreateMenuItem("Edit Style Setter"));
                    menuItemsAdded++;
                    _menu.AddSubMenuInTheHeader(menuBuilder.CreateMenuItem("Edit Style Trigger"));
                    menuItemsAdded++;
                }

                if (view is Grid)
                {
                    _menu.AddSubMenuInTheHeader(new MenuItem() { Header = "Edit Rows" });
                    _menu.AddSubMenuInTheHeader(new MenuItem() { Header = "Edit Columns" });
                }

                if (view is StackPanel)
                {
                    var ch = menuBuilder.CreateMenuItem("Change Orientation");
                    _menu.AddSubMenuInTheHeader(ch);
                    setValue = extendedItem.Properties[StackPanel.OrientationProperty].ValueOnInstance.ToString();
                    _menu.AddSubMenuCheckable(ch, Enum.GetValues(typeof(Orientation)), Orientation.Vertical.ToString(), setValue);
                    _menu.MainHeader.Items.Add(menuBuilder.CreateSeparator());
                    menuItemsAdded++;
                }

                if (extendedItem.Parent != null && extendedItem.Parent.View is DockPanel)
                {
                    var sda = menuBuilder.CreateMenuItem("Set Dock to");
                    _menu.AddSubMenuInTheHeader(sda);
                    setValue = extendedItem.Properties.GetAttachedProperty(DockPanel.DockProperty).ValueOnInstance.ToString();
                    _menu.AddSubMenuCheckable(sda, Enum.GetValues(typeof(Dock)), Dock.Left.ToString(), setValue);
                    _menu.MainHeader.Items.Add(menuBuilder.CreateSeparator());
                    menuItemsAdded++;
                }

                if (extendedItem.Parent != null && extendedItem.Parent.View is Panel)
                {
                    var ch = menuBuilder.CreateMenuItem("Rendering Order");
                    _menu.AddSubMenuInTheHeader(ch);
                    var menuItem = menuBuilder.CreateMenuItem("Bring to front");
                    menuItem.IsCheckable = false;
                    ch.Items.Add(menuItem);
                    //menuItem.Click += new RoutedEventHandler(ZOrderClick);
                    menuItem = menuBuilder.CreateMenuItem("Foreward");
                    menuItem.IsCheckable = false;
                    ch.Items.Add(menuItem);
                    menuItem = menuBuilder.CreateMenuItem("Backward");
                    menuItem.IsCheckable = false;
                    ch.Items.Add(menuItem);
                    menuItem = menuBuilder.CreateMenuItem("Send to back");
                    menuItem.IsCheckable = false;
                    ch.Items.Add(menuItem);

                    //setValue = extendedItem.Properties[StackPanel.OrientationProperty].ValueOnInstance.ToString();
                    //_menu.AddSubMenuCheckable(ch, Enum.GetValues(typeof(Orientation)), Orientation.Vertical.ToString(), setValue);
                    if (_menu.MainHeader != null)
                        _menu.MainHeader.Items.Add(menuBuilder.CreateSeparator());
                    menuItemsAdded++;
                }

                var ha = menuBuilder.CreateMenuItem("Horizontal Alignment");
                _menu.AddSubMenuInTheHeader(ha);
                setValue = extendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].ValueOnInstance.ToString();
                _menu.AddSubMenuCheckable(ha, Enum.GetValues(typeof(HorizontalAlignment)), HorizontalAlignment.Stretch.ToString(), setValue);
                menuItemsAdded++;

                var va = menuBuilder.CreateMenuItem("Vertical Alignment");
                _menu.AddSubMenuInTheHeader(va);
                setValue = extendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].ValueOnInstance.ToString();
                _menu.AddSubMenuCheckable(va, Enum.GetValues(typeof(VerticalAlignment)), VerticalAlignment.Stretch.ToString(), setValue);
                menuItemsAdded++;
            }

            return menuItemsAdded;
        }

        protected virtual void OnMenuLoaded(object sender, EventArgs e)
        {
            if (_menu == null || _menu.MainHeader == null || _menu.MainHeader.Items.Count > 0)
                return;
            if (_menu.MainHeader != null)
                _menu.MainHeader.Click += MainHeaderClick;

            int menuItemsAdded = BuildMenu();
            if (menuItemsAdded == 0)
            {
                OnRemove();
            }
        }

        //void ZOrderClick(object sender, RoutedEventArgs e)
        //{
        //}

        public static void MainHeaderClick(object sender, RoutedEventArgs e, DesignItem extendedItem, QuickOperationMenu _menu)
        {
            var clickedOn = e.Source as MenuItem;
            if (clickedOn != null)
            {
                var parent = clickedOn.Parent as MenuItem;
                if (parent != null)
                {

                    if ((string)clickedOn.Header == "Edit Items")
                    {
                        var editor = new CollectionEditor();
                        var itemsControl = extendedItem.View as ItemsControl;
                        if (itemsControl != null)
                            editor.LoadItemsCollection(extendedItem);
                        editor.Show();
                    }
                    else if ((string)clickedOn.Header == "Edit Style Setter")
                    {
                        if (!e.Handled)
                        {
                            var editor = new StyleSetterWindow();
                            editor.LoadItemsCollection(extendedItem);
                            editor.Show();
                        }
                    }
                    else if ((string)clickedOn.Header == "Edit Style Trigger")
                    {
                        if (!e.Handled)
                        {
                            var editor = new StyleTriggerWindow();
                            editor.LoadItemsCollection(extendedItem);
                            editor.Show();
                        }
                    }
                    else if ((string)clickedOn.Header == "Delete")
                    {
                        if (!e.Handled)
                        {
                            ModelTools.DeleteComponents(new DesignItem[] {extendedItem});
                        }
                    }

                    if ((string)clickedOn.Header == "Edit Rows")
                    {
                        var editor = new FlatCollectionEditor(Window.GetWindow(this.ExtendedItem.View));
                        var gd = this.ExtendedItem.View as Grid;
                        if (gd != null)
                            editor.LoadItemsCollection(this.ExtendedItem.Properties["RowDefinitions"]);
                        editor.Show();
                    }

                    if ((string)clickedOn.Header == "Edit Columns")
                    {
                        var editor = new FlatCollectionEditor(Window.GetWindow(this.ExtendedItem.View));
                        var gd = this.ExtendedItem.View as Grid;
                        if (gd != null)
                            editor.LoadItemsCollection(this.ExtendedItem.Properties["ColumnDefinitions"]);
                        editor.Show();
                    }

                    if (parent.Header is string && (string)parent.Header == "Change Orientation")
                    {
                        var value = _menu.UncheckChildrenAndSelectClicked(parent, clickedOn);
                        if (value != null)
                        {
                            var orientation = Enum.Parse(typeof(Orientation), value);
                            if (orientation != null)
                                extendedItem.Properties[StackPanel.OrientationProperty].SetValue(orientation);
                        }
                    }
                    if (parent.Header is string && (string)parent.Header == "Set Dock to")
                    {
                        var value = _menu.UncheckChildrenAndSelectClicked(parent, clickedOn);
                        if (value != null)
                        {
                            var dock = Enum.Parse(typeof(Dock), value);
                            if (dock != null)
                                extendedItem.Properties.GetAttachedProperty(DockPanel.DockProperty).SetValue(dock);
                        }
                    }


                    if (parent.Header is string && (string)parent.Header == "Horizontal Alignment")
                    {
                        var value = _menu.UncheckChildrenAndSelectClicked(parent, clickedOn);
                        if (value != null)
                        {
                            var ha = Enum.Parse(typeof(HorizontalAlignment), value);
                            if (ha != null)
                                extendedItem.Properties[FrameworkElement.HorizontalAlignmentProperty].SetValue(ha);
                        }
                    }

                    if (parent.Header is string && (string)parent.Header == "Vertical Alignment")
                    {
                        var value = _menu.UncheckChildrenAndSelectClicked(parent, clickedOn);
                        if (value != null)
                        {
                            var va = Enum.Parse(typeof(VerticalAlignment), value);
                            if (va != null)
                                extendedItem.Properties[FrameworkElement.VerticalAlignmentProperty].SetValue(va);
                        }
                    }

                    if (parent.Header is string && (string)parent.Header == "Rendering Order")
                    {
                        if ((clickedOn.Header as string) == "Bring to front")
                        {
                            extendedItem.Parent.ContentProperty.CollectionElements.MoveToLast(extendedItem);
                            //this.ExtendedItem.ContentProperty.CollectionElements.MoveToFirst(extendedItem);
                        }
                        else if ((clickedOn.Header as string) == "Foreward")
                        {
                            extendedItem.Parent.ContentProperty.CollectionElements.MoveForward(extendedItem);
                        }
                        else if ((clickedOn.Header as string) == "Backward")
                        {
                            extendedItem.Parent.ContentProperty.CollectionElements.MoveBackward(extendedItem);
                        }
                        else if ((clickedOn.Header as string) == "Send to back")
                        {
                            extendedItem.Parent.ContentProperty.CollectionElements.MoveToFirst(extendedItem);
                        }
                    }
                }
            }
        }

        public virtual void MainHeaderClick(object sender, RoutedEventArgs e)
        {
            MainHeaderClick(sender, e, this.ExtendedItem, _menu);
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            _menu.Loaded -= OnMenuLoaded;
            var kbs = this.ExtendedItem.Services.GetService(typeof(IKeyBindingService)) as IKeyBindingService;
            if (kbs != null)
                kbs.DeregisterBinding(_keyBinding);
        }

        //QuickOperationMenu IQuickOperationMenuItemBuilder.CreateMainMenu()
        //{
        //    throw new NotImplementedException();
        //}

        //MenuItem IQuickOperationMenuItemBuilder.CreateMenuItem(string header)
        //{
        //    throw new NotImplementedException();
        //}

        //Separator IQuickOperationMenuItemBuilder.CreateSeparator()
        //{
        //    throw new NotImplementedException();
        //}

        //void IQuickOperationMenuItemBuilder.MainHeaderClick(object sender, RoutedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

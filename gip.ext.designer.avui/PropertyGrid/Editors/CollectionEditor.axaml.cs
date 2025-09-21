// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using gip.ext.designer.avui.OutlineView;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace gip.ext.designer.avui.PropertyGrid.Editors
{
    public partial class CollectionEditor : Window, ITypeEditorInitItem
    {
        private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();
        static CollectionEditor()
        {
            TypeMappings.Add(typeof(Menu), typeof(MenuItem));
            TypeMappings.Add(typeof(ListBox), typeof(ListBoxItem));
            //TypeMappings.Add(typeof(ListView), typeof(ListViewItem));
            TypeMappings.Add(typeof(ComboBox), typeof(ComboBoxItem));
            TypeMappings.Add(typeof(TreeView), typeof(TreeViewItem));
            TypeMappings.Add(typeof(TabControl), typeof(TabItem));
        }

        private DesignItem _item;
        private Type _type;
        private IComponentService _componentService;
        
        // AvaloniaUI control references
        private Outline _outline;
        private gip.ext.designer.avui.PropertyGrid.PropertyGridView _propertyGridView;
        private Button _addItem;
        private Button _removeItem;
        private Button _moveUpItem;
        private Button _moveDownItem;
        
        public CollectionEditor()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void InitializeControls()
        {
            // Get references to named controls
            _outline = this.FindControl<Outline>("Outline");
            _propertyGridView = this.FindControl<gip.ext.designer.avui.PropertyGrid.PropertyGridView>("PropertyGridView");
            _addItem = this.FindControl<Button>("AddItem");
            _removeItem = this.FindControl<Button>("RemoveItem");
            _moveUpItem = this.FindControl<Button>("MoveUpItem");
            _moveDownItem = this.FindControl<Button>("MoveDownItem");
        }

        public void LoadItemsCollection(DesignItem item)
        {
            Debug.Assert(item.View is ItemsControl);
            _item = item;
            _componentService = item.Services.Component;
            item.Services.Selection.SelectionChanged += delegate { this._propertyGridView.SelectedItems = item.Services.Selection.SelectedItems; };
            var control = item.View as ItemsControl;
            if (control != null)
            {
                TypeMappings.TryGetValue(control.GetType(), out _type);
                if (_type != null)
                {
                    IOutlineNode node = item.CreateOutlineNode();
                    //OutlineNode node = OutlineNode.Create(item);
                    this._outline.Root = node;
                    this._propertyGridView.PropertyGrid.SelectedItems = item.Services.Selection.SelectedItems;
                }
                else
                {
                    this._propertyGridView.IsEnabled = false;
                    this._outline.IsEnabled = false;
                    _addItem.IsEnabled = false;
                    _removeItem.IsEnabled = false;
                    _moveUpItem.IsEnabled = false;
                    _moveDownItem.IsEnabled = false;
                }
            }

        }

        private void OnAddItemClicked(object sender, RoutedEventArgs e)
        {
            DesignItem newItem = _componentService.RegisterComponentForDesigner(Activator.CreateInstance(_type));
            DesignItem selectedItem = _item.Services.Selection.PrimarySelection;
            if (selectedItem.ContentProperty.IsCollection)
                selectedItem.ContentProperty.CollectionElements.Add(newItem);
            else
                selectedItem.ContentProperty.SetValue(newItem);
            _item.Services.Selection.SetSelectedComponents(new[] { newItem });
        }

        private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
        {
            DesignItem selectedItem = _item.Services.Selection.PrimarySelection;
            DesignItem parent = selectedItem.Parent;
            if (parent != null && selectedItem != _item)
            {
                if (parent.ContentProperty.IsCollection)
                    parent.ContentProperty.CollectionElements.Remove(selectedItem);
                else
                    parent.ContentProperty.SetValue(null);
                _item.Services.Selection.SetSelectedComponents(new[] { parent });
            }
        }

        private void OnMoveItemUpClicked(object sender, RoutedEventArgs e)
        {
            DesignItem selectedItem = _item.Services.Selection.PrimarySelection;
            DesignItem parent = selectedItem.Parent;
            if (parent != null && parent.ContentProperty.IsCollection)
            {
                if (parent.ContentProperty.CollectionElements.Count != 1 && parent.ContentProperty.CollectionElements.IndexOf(selectedItem) != 0)
                {
                    int moveToIndex = parent.ContentProperty.CollectionElements.IndexOf(selectedItem) - 1;
                    var itemAtMoveToIndex = parent.ContentProperty.CollectionElements[moveToIndex];
                    parent.ContentProperty.CollectionElements.RemoveAt(moveToIndex);
                    if ((moveToIndex + 1) < (parent.ContentProperty.CollectionElements.Count + 1))
                        parent.ContentProperty.CollectionElements.Insert(moveToIndex + 1, itemAtMoveToIndex);
                }
            }
        }

        private void OnMoveItemDownClicked(object sender, RoutedEventArgs e)
        {
            DesignItem selectedItem = _item.Services.Selection.PrimarySelection;
            DesignItem parent = selectedItem.Parent;
            if (parent != null && parent.ContentProperty.IsCollection)
            {
                var itemCount = parent.ContentProperty.CollectionElements.Count;
                if (itemCount != 1 && parent.ContentProperty.CollectionElements.IndexOf(selectedItem) != itemCount)
                {
                    int moveToIndex = parent.ContentProperty.CollectionElements.IndexOf(selectedItem) + 1;
                    if (moveToIndex < itemCount)
                    {
                        var itemAtMoveToIndex = parent.ContentProperty.CollectionElements[moveToIndex];
                        parent.ContentProperty.CollectionElements.RemoveAt(moveToIndex);
                        if (moveToIndex > 0)
                            parent.ContentProperty.CollectionElements.Insert(moveToIndex - 1, itemAtMoveToIndex);
                    }
                }
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            _item.Services.Selection.SetSelectedComponents(new[] { _item });
            base.OnClosing(e);
        }
    }
}

// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Globalization;
using gip.ext.design.avui.PropertyGrid;
using System.Diagnostics;
using gip.ext.design.avui;
using gip.ext.designer.avui.OutlineView;
using gip.ext.designer.avui.Extensions;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.LogicalTree;

namespace gip.ext.designer.avui.PropertyGrid
{
    [TemplatePart(Name = "PART_clearButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NameTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_TreeView", Type = typeof(TreeView))]
    public class DesignItemTreeView : TemplatedControl, INotifyPropertyChanged
    {
        static DesignItemTreeView()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignItemTreeView), new FrameworkPropertyMetadata(typeof(DesignItemTreeView)));
        }

        public DesignItemTreeView()
        {
            //DataContext = PropertyGrid;
            DataContext = this;
        }


        Button clearButton;
        public TextBox NameTextBox { get; set; }
        public TreeView TreeView { get; set; }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            clearButton = e.NameScope.Find("PART_clearButton") as Button;
            if (clearButton != null)
                clearButton.Click += clearButton_Click;

            NameTextBox = e.NameScope.Find("PART_NameTextBox") as TextBox;
            TreeView = e.NameScope.Find("PART_TreeView") as TreeView;
            if (TreeView != null)
            {
                TreeView.PointerPressed += TreeView_MouseRightButtonDown;
                //TreeView.SelectionChanged += TreeView_SelectionChanged;
            }
        }

        void TreeView_MouseRightButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                return;
            TreeViewItem ClickedTreeViewItem = null;

            var source = e.Source as Visual;
            if (source == null) return;

            Control ClickedItem = source.FindAncestorOfType<Control>();

            while ((ClickedItem != null) && !(ClickedItem is TreeViewItem))
            {
                ClickedItem = ClickedItem.FindAncestorOfType<Control>();
            }

            ClickedTreeViewItem = ClickedItem as TreeViewItem;
            if (ClickedTreeViewItem != null)
            {
                ClickedTreeViewItem.IsSelected = true;
                ClickedTreeViewItem.Focus();
                OutlineNode node = ClickedTreeViewItem.Header as OutlineNode;
                if (node != null)
                {
                    node.PrepareContextMenu();
                    if (node.Menu != null)
                    {
                        ContextMenu menu = BuildContextMenu();
                        menu.Items.Add(node.Menu.MainHeader);
                        ClickedTreeViewItem.ContextMenu = menu;
                        menu.Open(ClickedTreeViewItem);
                        e.Handled = true;
                    }
                }
            }
        }

        protected virtual ContextMenu BuildContextMenu()
        {
            return new ContextMenu();
        }

        void clearButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Implementation for clear button click
        }

        //void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    if (e.NewValue != null)
        //    {
        //        OutlineNode node = e.NewValue as OutlineNode;
        //        if (node != null)
        //        {
        //            //node.DesignItem.Services.Selection.SelectionFromTreeView(node.DesignItem);
        //        }
        //    }
        //}

        //private void SelectItem(OutlineNode node)
        //{
        //    Stack<OutlineNode> nodesStack = new Stack<OutlineNode>();
        //    OutlineNode currentNode = node;

        //    while (currentNode != null)
        //    {
        //        nodesStack.Push(currentNode);
        //        currentNode = currentNode.Parent as OutlineNode;
        //    }

        //    TreeViewItem currentItem = TreeView.ItemContainerGenerator.ContainerFromItem(TreeView.Items[0]) as TreeViewItem;
        //    currentNode = nodesStack.Pop();

        //    if (currentItem.Header != currentNode)
        //        return;

        //    while (nodesStack.Count > 0)
        //    {
        //        if (currentItem.IsExpanded == false)
        //        {
        //            currentItem.IsExpanded = true;
        //            currentItem.UpdateLayout();
        //        }

        //        currentNode = nodesStack.Pop();
        //        foreach (OutlineNode innerItem in currentItem.Items)
        //        {
        //            if (innerItem == currentNode)
        //            {
        //                currentItem = currentItem.ItemContainerGenerator.ContainerFromItem(innerItem) as TreeViewItem;
        //                break;
        //            }
        //        }

        //    }

        //    currentItem.IsSelected = true;
        //}



        public static readonly AvaloniaProperty<IEnumerable<DesignItem>> SelectedItemsProperty =
            AvaloniaProperty.Register<DesignItemTreeView, IEnumerable<DesignItem>>("SelectedItems");

        public IEnumerable<DesignItem> SelectedItems
        {
            get { return (IEnumerable<DesignItem>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly AvaloniaProperty<String> NameSearchProperty =
            AvaloniaProperty.Register<DesignItemTreeView, String>("NameSearch");

        public String NameSearch
        {
            get { return (String)GetValue(NameSearchProperty); }
            set { SetValue(NameSearchProperty, value); }
        }

        List<OutlineNode> _LogicalTree;
        public IEnumerable<OutlineNode> LogicalTree
        {
            get
            {
                return _LogicalTree;
            }
        }

        private DesignItem _RootItem;
        public DesignItem RootItem
        {
            get
            {
                return _RootItem;
            }
            set
            {
                _RootItem = value;
                RaisePropertyChanged("RootItem");
                Dispatcher.UIThread.Invoke(new Action(
                    delegate
                    {
                        _LogicalTree = new List<OutlineNode>();
                        OutlineNode.EmptyOutlineNodeMap();
                        if (_RootItem != null)
                            _LogicalTree.Add(CreateOutlineNode());

                        RaisePropertyChanged("LogicalTree");
                    }), DispatcherPriority.Background);
            }
        }

        public virtual OutlineNode CreateOutlineNode()
        {
            return new OutlineNode(_RootItem);
        }

        #region INotifyPropertyChanged Members

        public new event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}

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
using gip.ext.design.PropertyGrid;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using gip.ext.design;
using gip.ext.designer.OutlineView;
using gip.ext.designer.Extensions;

namespace gip.ext.designer.PropertyGrid
{
    [TemplatePart(Name = "PART_clearButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NameTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_TreeView", Type = typeof(TreeView))]
    public class DesignItemTreeView : Control, INotifyPropertyChanged
    {
        static DesignItemTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignItemTreeView), new FrameworkPropertyMetadata(typeof(DesignItemTreeView)));
        }

        public DesignItemTreeView()
        {
            //DataContext = PropertyGrid;
            DataContext = this;
        }


        Button clearButton;
        public TextBox NameTextBox { get; set; }
        public TreeView TreeView { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            clearButton = Template.FindName("PART_clearButton", this) as Button;
            if (clearButton != null)
                clearButton.Click += new RoutedEventHandler(clearButton_Click);

            NameTextBox = Template.FindName("PART_NameTextBox", this) as TextBox;
            TreeView = Template.FindName("PART_TreeView", this) as TreeView;
            TreeView.MouseRightButtonDown += new MouseButtonEventHandler(TreeView_MouseRightButtonDown);
            //TreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(TreeView_SelectedItemChanged);
        }

        void TreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem ClickedTreeViewItem = new TreeViewItem();

            UIElement ClickedItem = VisualTreeHelper.GetParent(e.OriginalSource as UIElement) as UIElement;

            while ((ClickedItem != null) && !(ClickedItem is TreeViewItem))
            {
                ClickedItem = VisualTreeHelper.GetParent(ClickedItem) as UIElement;
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
                        menu.IsOpen = true;
                        e.Handled = true;
                    }
                }
            }
        }

        protected virtual ContextMenu BuildContextMenu()
        {
            return new ContextMenu();
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



        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IEnumerable<DesignItem>), typeof(DesignItemTreeView));

        public IEnumerable<DesignItem> SelectedItems
        {
            get { return (IEnumerable<DesignItem>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty NameSearchProperty =
            DependencyProperty.Register("NameSearch", typeof(String), typeof(DesignItemTreeView));

        public String NameSearch
        {
            get { return (String)GetValue(NameSearchProperty); }
            set { SetValue(NameSearchProperty, value); }
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
        }

        void clearButton_Click(object sender, RoutedEventArgs e)
        {
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
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                    delegate
                    {
                        _LogicalTree = new List<OutlineNode>();
                        OutlineNode.EmptyOutlineNodeMap();
                        if (_RootItem != null)
                            _LogicalTree.Add(CreateOutlineNode());

                        RaisePropertyChanged("LogicalTree");
                    }));
            }
        }

        public virtual OutlineNode CreateOutlineNode()
        {
            return new OutlineNode(_RootItem);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

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

// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.ext.design;
using System.Collections.ObjectModel;
using System.Collections;
using gip.ext.designer;
using gip.ext.xamldom;
using gip.ext.designer.Extensions;
using System.Windows.Controls;
using gip.ext.designer.Controls;
using System.Windows;
using gip.core.datamodel;

namespace gip.ext.designer.OutlineView
{
    public abstract class OutlineNodeBase : INotifyPropertyChanged
    {
        //Used to check if element can enter other containers
        public static PlacementType DummyPlacementType;

        static OutlineNodeBase()
        {
            DummyPlacementType = PlacementType.Register("DummyPlacement");
        }

        public OutlineNodeBase(DesignItem designItem)
        {
            DesignItem = designItem;
            UpdateChildren();
            Initialize();
        }

        protected virtual void Initialize()
        {
            DesignItem.PropertyChanged += new PropertyChangedEventHandler(DesignItem_PropertyChanged);
        }

        public DesignItem DesignItem { get; protected set; }

        public virtual string Name
        {
            get
            {
                if (DesignItem.HasProperty("Name") == null)
                    return DesignItem.ComponentType.Name;
                if (string.IsNullOrEmpty(DesignItem.Name))
                {
                    return DesignItem.ComponentType.Name;
                }
                return DesignItem.ComponentType.Name + " (" + DesignItem.Name + ")";
            }
        }

        bool isExpanded = true;
        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                isExpanded = value;
                if (isExpanded && Parent != null)
                   Parent.IsExpanded = true;
                RaisePropertyChanged("IsExpanded");
            }
        }

        ObservableCollection<OutlineNodeBase> children = new ObservableCollection<OutlineNodeBase>();

        public ObservableCollection<OutlineNodeBase> Children
        {
            get { return children; }
        }

        public OutlineNodeBase Parent
        {
            get;
            set;
        }

        public int LayerDepth
        {
            get
            {
                if (Parent == null)
                    return 0;
                return Parent.LayerDepth + 1;
            }
        }

        protected void DesignItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == DesignItem.ContentPropertyName)
            {
                UpdateChildren();
            }
            else if (e.PropertyName == "Name")
                RaisePropertyChanged(e.PropertyName);
        }

        void UpdateChildren()
        {
            Children.Clear();

            if (DesignItem.ContentPropertyName != null)
            {
                var content = DesignItem.ContentProperty;
                if (content.IsCollection)
                {
                    UpdateChildrenCore(content.CollectionElements);
                }
                else
                {
                    if (content.Value != null)
                    {
                        UpdateChildrenCore(new[] { content.Value });
                    }
                }
            }
        }

        void UpdateChildrenCore(IEnumerable<DesignItem> items)
        {
            foreach (var item in items)
            {
                if (item == null)
                    continue;
                if (ModelTools.CanSelectComponent(item))
                {
                    var node = OnCreateChildrenNode(item);
                    node.Parent = this;
                    if (node != null)
                        Children.Add(node);
                }
            }
        }

        protected abstract OutlineNodeBase OnCreateChildrenNode(DesignItem child);

        public bool CanInsert(IEnumerable<OutlineNodeBase> nodes, OutlineNodeBase after, bool copy)
        {
            var operation = PlacementOperation.Start(nodes.Select(node => node.DesignItem).ToArray(), DummyPlacementType);
            var placementBehavior = DesignItem.GetBehavior<IPlacementBehavior>();
            if (operation != null)
                return placementBehavior.CanEnterContainer(operation);
            return false;
        }

        public void Insert(IEnumerable<OutlineNodeBase> nodes, OutlineNodeBase after, bool copy)
        {
            if (copy)
            {
                nodes = nodes.Select(n => OnCreateChildrenNode(n.DesignItem.Clone()));
            }
            else
            {
                foreach (var node in nodes)
                {
                    node.DesignItem.Remove();
                }
            }

            var index = after == null ? 0 : Children.IndexOf(after) + 1;

            var content = DesignItem.ContentProperty;
            if (content.IsCollection)
            {
                foreach (var node in nodes)
                {
                    content.CollectionElements.Insert(index++, node.DesignItem);
                }
            }
            else
            {
                content.SetValue(nodes.First().DesignItem);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }

    public class OutlineNode : OutlineNodeBase, IQuickOperationMenuItemBuilder
    {
        public static OutlineNode Create(DesignItem designItem)
        {
            OutlineNode node;
            if (!outlineNodes.TryGetValue(designItem, out node))
            {
                node = new OutlineNode(designItem);
                outlineNodes[designItem] = node;
            }
            return node;
        }

        //TODO: Reset with DesignContext
        protected static Dictionary<DesignItem, OutlineNode> outlineNodes = new Dictionary<DesignItem, OutlineNode>();
        public static void EmptyOutlineNodeMap()
        {
            outlineNodes = new Dictionary<DesignItem, OutlineNode>();
        }

        public OutlineNode(DesignItem designItem)
            : base(designItem)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            if (DesignItem.HasProperty("Name") != null)
                DesignItem.NameChanged += new EventHandler(DesignItem_NameChanged);
            SelectionService.SelectionChanged += new EventHandler<DesignItemCollectionEventArgs>(Selection_SelectionChanged);
        }

        public ISelectionService SelectionService
        {
            get { return DesignItem.Services.Selection; }
        }

        bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    if (_menu != null)
                    {
                        _menu.MainHeader.Loaded -= OnMenuLoaded;
                        _menu.MainHeader.Unloaded -= OnMenuUnloaded;
                    }
                    _menu = null;
                    isSelected = value;
                    SelectionService.SetSelectedComponents(new[] { DesignItem },
                                                           value ? SelectionTypes.Add : SelectionTypes.Remove);
                    if (value)
                    {
                        SelectionService.TemporarySelectionFromTreeView = DesignItem;
                    }
                    //SelectionService.SelectionFromTreeView(DesignItem);
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        public void PrepareContextMenu()
        {
            if (_menu != null)
            {
                _menu.MainHeader.Loaded -= OnMenuLoaded;
                _menu.MainHeader.Unloaded -= OnMenuUnloaded;
            }
            _menu = CreateMainMenu();
            BuildMenu();
            _menu.MainHeader.Loaded += OnMenuLoaded;
            _menu.MainHeader.Unloaded += OnMenuUnloaded;
        }

        public override string Name
        {
            get
            {
                if (DesignItem.View is IVBContent && !string.IsNullOrEmpty( (DesignItem.View as IVBContent).VBContent))
                {
                    return DesignItem.ComponentType.Name + " \"" + (DesignItem.View as IVBContent).VBContent + "\""; ;
                }
                else
                {
                    if (string.IsNullOrEmpty(DesignItem.Name))
                    {
                        return DesignItem.ComponentType.Name;
                    }
                    return DesignItem.ComponentType.Name + " (" + DesignItem.Name + ")";
                }
            }
        }

        protected void Selection_SelectionChanged(object sender, DesignItemCollectionEventArgs e)
        {
            IsSelected = DesignItem.Services.Selection.IsComponentSelected(DesignItem);
        }

        protected void DesignItem_NameChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("Name");
        }

        protected override OutlineNodeBase OnCreateChildrenNode(DesignItem child)
        {
            return OutlineNode.Create(child);
        }


        protected QuickOperationMenu _menu;
        public QuickOperationMenu Menu
        {
            get
            {
                return _menu;
            }
        }

        public virtual QuickOperationMenu CreateMainMenu()
        {
            return new QuickOperationMenu() { MainHeader = CreateMenuItem("Operations") };
        }

        public virtual MenuItem CreateMenuItem(string header)
        {
            return new MenuItem() { Header = header };
        }

        public virtual Separator CreateSeparator()
        {
            return new Separator();
        }

        public virtual void OnMenuLoaded(object sender, EventArgs e)
        {
            if (_menu == null || _menu.MainHeader == null)
                return;
            if (_menu.MainHeader != null)
                _menu.MainHeader.Click += MainHeaderClick;

            //if (_menu.MainHeader.Items.Count <= 0)
            //{
            //    int menuItemsAdded = BuildMenu();
            //    if (menuItemsAdded == 0)
            //    {
            //        if (_menu.MainHeader != null)
            //            _menu.MainHeader.Click -= MainHeaderClick;
            //        _menu.Loaded -= OnMenuLoaded;
            //        _menu.Unloaded -= OnMenuUnloaded;
            //    }
            //}
        }

        public virtual void OnMenuUnloaded(object sender, EventArgs e)
        {
            if (_menu == null || _menu.MainHeader == null || _menu.MainHeader.Items.Count > 0)
                return;
            if (_menu.MainHeader != null)
                _menu.MainHeader.Click -= MainHeaderClick;
        }

        public virtual int BuildMenu()
        {
            var menuCount = QuickOperationMenuExtension.BuildMenu(this.DesignItem, _menu, this);;
            return menuCount;
        }

        public virtual void MainHeaderClick(object sender, RoutedEventArgs e)
        {
            QuickOperationMenuExtension.MainHeaderClick(sender, e, this.DesignItem, _menu);
        }
    }
}

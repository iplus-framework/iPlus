// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.ext.design.avui;
using System.Collections.ObjectModel;
using System.Collections;
using gip.ext.designer.avui;
using gip.ext.xamldom.avui;
using gip.ext.designer.avui.Extensions;
using System.Windows.Controls;
using gip.ext.designer.avui.Controls;
using System.Windows;
using gip.core.datamodel;
using System.Collections.Specialized;

namespace gip.ext.designer.avui.OutlineView
{
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

        public override bool IsSelected
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
                    if (isSelected != value && SelectionService != null)
                    {
                        isSelected = value;
                        SelectionService.SetSelectedComponents(new[] { DesignItem },
                                                               value ? SelectionTypes.Add : SelectionTypes.Remove);
                        RaisePropertyChanged("IsSelected");
                    }
                    if (value)
                    {
                        SelectionService.TemporarySelectionFromTreeView = DesignItem;
                    }
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

        protected override void UpdateChildren()
        {
            Children.Clear();

            foreach (var prp in DesignItem.AllSetProperties)
            {
                if (prp.Name != DesignItem.ContentPropertyName)
                {
                    if (prp.Value != null)
                    {
                        var propertyNode = PropertyOutlineNode.Create(prp);
                        var node = prp.Value.CreateOutlineNode();
                        propertyNode.Children.Add(node);
                        Children.Add(propertyNode);
                    }
                }
            }
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
                        if (!UpdateChildrenCore(new[] { content.Value }))
                        {
                            var propertyNode = PropertyOutlineNode.Create(content);
                            var node = content.Value.CreateOutlineNode();
                            propertyNode.Children.Add(node);
                            Children.Add(propertyNode);
                        }
                    }
                }
            }
        }

        protected override void UpdateChildrenCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    var item = Children.FirstOrDefault(x => x.DesignItem == oldItem);
                    if (item != null)
                    {
                        Children.Remove(item);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                UpdateChildrenCore(e.NewItems.Cast<DesignItem>(), e.NewStartingIndex);
            }
        }

        protected virtual bool UpdateChildrenCore(IEnumerable<DesignItem> items, int index = -1)
        {
            var retVal = false;
            foreach (var item in items)
            {
                if (ModelTools.CanSelectComponent(item))
                {
                    if (Children.All(x => x.DesignItem != item))
                    {
                        var node = item.CreateOutlineNode();
                        if (index > -1)
                        {
                            Children.Insert(index++, node);
                            retVal = true;
                        }
                        else
                        {
                            Children.Add(node);
                            retVal = true;
                        }
                    }
                }
                else
                {
                    var content = item.ContentProperty;
                    if (content != null)
                    {
                        if (content.IsCollection)
                        {
                            UpdateChildrenCore(content.CollectionElements);
                            retVal = true;
                        }
                        else
                        {
                            if (content.Value != null)
                            {
                                UpdateChildrenCore(new[] { content.Value });
                                retVal = true;
                            }
                        }
                    }
                }
            }

            return retVal;
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

        internal class OutlineNodeService : IOutlineNodeService, IDisposable
        {
            readonly Dictionary<DesignItem, IOutlineNode> outlineNodes = new Dictionary<DesignItem, IOutlineNode>();

            public IOutlineNode Create(DesignItem designItem)
            {
                IOutlineNode node = null;
                if (designItem != null && !outlineNodes.TryGetValue(designItem, out node))
                {
                    node = new OutlineNode(designItem);
                    outlineNodes[designItem] = node;
                }

                return node;
            }

            public void Dispose()
            {
                outlineNodes.Clear();
            }
        }
    }
}

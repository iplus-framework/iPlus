﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
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
using gip.ext.design;

namespace gip.ext.designer.PropertyGrid
{
    public class PropertyGrid : INotifyPropertyChanged
    {
        public PropertyGrid()
        {
            Categories = new ObservableCollection<Category>(new[] {
				specialCategory, 
				popularCategory, 				
				otherCategory,
				attachedCategory
			});

            Events = new PropertyNodeCollection();
        }

        protected Category specialCategory = new Category("Special");
        protected Category popularCategory = new Category("Popular");
        protected Category otherCategory = new Category("Other");
        protected Category attachedCategory = new Category("Attached");

        Dictionary<MemberDescriptor, PropertyNode> nodeFromDescriptor = new Dictionary<MemberDescriptor, PropertyNode>();

        public ObservableCollection<Category> Categories { get; protected set; }
        public PropertyNodeCollection Events { get; private set; }

        PropertyGridTab currentTab;

        public PropertyGridTab CurrentTab
        {
            get
            {
                return currentTab;
            }
            set
            {
                currentTab = value;
                RaisePropertyChanged("CurrentTab");
                RaisePropertyChanged("NameBackground");
            }
        }

        string filter;

        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                Reload();
                RaisePropertyChanged("Filter");
            }
        }

        bool _DependencyPropFilter;
        public bool DependencyPropFilter
        {
            get
            {
                return _DependencyPropFilter;
            }
            set
            {
                _DependencyPropFilter = value;
                //Reload();
                RaisePropertyChanged("DependencyPropFilter");
            }
        }

        bool _AttachedPropFilter;
        public bool AttachedPropFilter
        {
            get
            {
                return _AttachedPropFilter;
            }
            set
            {
                _AttachedPropFilter = value;
                //Reload();
                RaisePropertyChanged("AttachedPropFilter");
            }
        }

        DesignItem singleItem;

        public DesignItem SingleItem
        {
            get
            {
                return singleItem;
            }
            private set
            {
                DesignItemProperty property = null;
                if (singleItem != null)
                    property = singleItem.HasProperty("Name");
                if (property != null)
                {
                    if (singleItem != null)
                    {
                        singleItem.NameChanged -= singleItem_NameChanged;
                    }
                }
                singleItem = value;
                if (property != null)
                {
                    if (singleItem != null)
                    {
                        singleItem.NameChanged += singleItem_NameChanged;
                    }
                }
                RaisePropertyChanged("SingleItem");
                RaisePropertyChanged("Name");
                RaisePropertyChanged("IsNameEnabled");
                IsNameCorrect = true;
            }
        }

        void singleItem_NameChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("Name");
        }

        public string Name
        {
            get
            {
                if (SingleItem != null)
                {
                    return SingleItem.Name;
                }
                return null;
            }
            set
            {
                if (SingleItem != null)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            SingleItem.Properties["Name"].Reset();
                        }
                        else
                        {
                            SingleItem.Name = value;
                        }
                        IsNameCorrect = true;
                    }
                    catch
                    {
                        IsNameCorrect = false;
                    }
                    RaisePropertyChanged("Name");
                }
            }
        }

        bool isNameCorrect = true;

        public bool IsNameCorrect
        {
            get
            {
                return isNameCorrect;
            }
            set
            {
                isNameCorrect = value;
                RaisePropertyChanged("IsNameCorrect");
            }
        }

        public bool IsNameEnabled
        {
            get
            {
                return SingleItem != null;
            }
        }

        IEnumerable<DesignItem> selectedItems;

        public IEnumerable<DesignItem> SelectedItems
        {
            get
            {
                return selectedItems;
            }
            set
            {
                selectedItems = value;
                RaisePropertyChanged("SelectedItems");
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                    delegate
                    {
                        Reload();
                    }));
            }
        }

        public void ClearFilter()
        {
            Filter = null;
        }

        bool _CategoriesInitialized = false;
        void Reload()
        {
            Clear();

            if (SelectedItems == null || !SelectedItems.Any()) 
                return;
            if (SelectedItems.Any()) 
                SingleItem = SelectedItems.First();

            foreach (var md in GetDescriptors())
            {
                if (PassesFilter(md.Name))
                    AddNode(md);
            }
        }

        void Clear()
        {
            foreach (var c in Categories)
            {
                if (!_CategoriesInitialized)
                    c.PropertyChanged += new PropertyChangedEventHandler(Categories_PropertyChanged);
                c.IsVisible = false;
                foreach (var p in c.Properties)
                {
                    p.IsVisible = false;
                }
            }
            _CategoriesInitialized = true;

            foreach (var e in Events)
            {
                e.IsVisible = false;
            }

            SingleItem = null;
        }

        void Categories_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedNode")
            {
                SelectedNode = (sender as Category).SelectedNode;
            }
        }

        private PropertyNode _SelectedNode = null;
        public PropertyNode SelectedNode
        {
            get
            {
                return _SelectedNode;
            }

            set
            {
                _SelectedNode = value;
                RaisePropertyChanged("SelectedNode");
            }
        }


        List<MemberDescriptor> GetDescriptors()
        {
            List<MemberDescriptor> list = new List<MemberDescriptor>();

            if (SelectedItems.Count() == 1)
            {
                foreach (MemberDescriptor d in TypeHelper.GetAvailableProperties(SingleItem.Component))
                {
                    list.Add(d);
                }
                foreach (MemberDescriptor d in TypeHelper.GetAvailableEvents(SingleItem.ComponentType))
                {
                    list.Add(d);
                }
            }
            else
            {
                foreach (MemberDescriptor d in TypeHelper.GetCommonAvailableProperties(SelectedItems.Select(t => t.Component)))
                {
                    list.Add(d);
                }
            }

            return list;
        }

        bool PassesFilter(string name)
        {
            if (string.IsNullOrEmpty(Filter))
                return true;
            for (int i = 0; i < name.Length; i++)
            {
                if (i == 0 || char.IsUpper(name[i]))
                {
                    if (string.Compare(name, i, Filter, 0, Filter.Length, true) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void AddNode(MemberDescriptor md)
        {
            var designProperties = SelectedItems.Select(item => item.Properties[md.Name]).ToArray();
            if (!Metadata.IsBrowsable(designProperties[0])) return;

            PropertyNode node;
            if (nodeFromDescriptor.TryGetValue(md, out node))
            {
                node.Load(designProperties);
            }
            else
            {
                node = CreatePropertyNode();
                node.Load(designProperties);
                if (node.IsEvent)
                {
                    Events.AddSorted(node);
                }
                else
                {
                    var cat = PickCategory(node);
                    cat.Properties.AddSorted(node);
                    node.Category = cat;
                }
                nodeFromDescriptor[md] = node;
            }
            if ((DependencyPropFilter && !node.IsDependencyProperty)
                || (AttachedPropFilter && (node.Category == attachedCategory)))
                node.IsVisible = false;
            else
                node.IsVisible = true;
            if (node.Category != null)
                node.Category.IsVisible = true;
        }

        protected virtual PropertyNode CreatePropertyNode()
        {
            return new PropertyNode();
        }

        protected virtual Category PickCategory(PropertyNode node)
        {
            if (Metadata.IsPopularProperty(node.FirstProperty))
                return popularCategory;
            if (node.FirstProperty.Name.Contains("."))
                return attachedCategory;
            var typeName = node.FirstProperty.DeclaringType.FullName;
            if (typeName.StartsWith("System.Windows.") || typeName.StartsWith("gip.ext.designer.Controls."))
                return otherCategory;
            return specialCategory;
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

        //class CategoryNameComparer : IComparer<string>
        //{
        //    public static CategoryNameComparer Instance = new CategoryNameComparer();

        //    public int Compare(string x, string y)
        //    {
        //        int i1 = Array.IndexOf(Metadata.CategoryOrder, x);
        //        if (i1 == -1) i1 = int.MaxValue;
        //        int i2 = Array.IndexOf(Metadata.CategoryOrder, y);
        //        if (i2 == -1) i2 = int.MaxValue;
        //        if (i1 == i2) return x.CompareTo(y);
        //        return i1.CompareTo(i2);
        //    }
        //}
    }

    public enum PropertyGridTab
    {
        Properties,
        Events
    }
}

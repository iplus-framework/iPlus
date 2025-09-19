// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using gip.ext.design.avui;
using gip.ext.designer.avui.Xaml;
using gip.ext.xamldom.avui;

namespace gip.ext.designer.avui.OutlineView
{
    /// <summary>
    /// Description of OutlineNodeBase.
    /// </summary>
    public abstract class OutlineNodeBase : INotifyPropertyChanged, IOutlineNode
	{
		protected virtual void UpdateChildren()
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

        protected virtual void UpdateChildrenCore(IEnumerable<DesignItem> items)
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


		protected virtual OutlineNodeBase OnCreateChildrenNode(DesignItem child)
		{
			return child.CreateOutlineNode() as OutlineNodeBase;

        }

		protected virtual void UpdateChildrenCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
		}
		//Used to check if element can enter other containers
		protected static PlacementType DummyPlacementType;

		private bool _collectionWasChanged;

		private string _name;

		public bool IsVisualNode
		{
			get { return this.DesignItem.Component is Visual ? true : false; }
		}

        static OutlineNodeBase()
        {
            DummyPlacementType = PlacementType.Register("DummyPlacement");
        }

        protected OutlineNodeBase(DesignItem designItem)
		{
			DesignItem = designItem;
            UpdateChildren();
            Initialize();
        }

        protected virtual void Initialize()
        {
            bool hidden = false;
			try
			{
				//hidden = object.Equals(DesignItem.Properties.GetAttachedProperty(DesignTimeProperties.IsHiddenProperty).GetConvertedValueOnInstance<bool>(), true);
			}
			catch (Exception)
			{ 
			}
			
			if (hidden) 
			{
				_isDesignTimeVisible = false;
			}

			bool locked = false;
			try
			{
				//locked = object.Equals(DesignItem.Properties.GetAttachedProperty(DesignTimeProperties.IsLockedProperty).GetConvertedValueOnInstance<bool>(), true);
			}
			catch (Exception)
			{ 
			}
			if (locked) 
			{
				_isDesignTimeLocked = true;
			}

			//TODO

			DesignItem.NameChanged += new EventHandler(DesignItem_NameChanged);

			if (DesignItem.ContentProperty != null && DesignItem.ContentProperty.IsCollection)
			{
				DesignItem.ContentProperty.CollectionElements.CollectionChanged += CollectionElements_CollectionChanged;
				DesignItem.PropertyChanged += new PropertyChangedEventHandler(DesignItem_PropertyChanged);
			}
			else
			{
				DesignItem.PropertyChanged += new PropertyChangedEventHandler(DesignItem_PropertyChanged);
			}
		}

		protected OutlineNodeBase(string name)
		{
			_name = name;
		}

		public DesignItem DesignItem { get; set; }

		public virtual ServiceContainer Services
		{
			get { return this.DesignItem.Services; }
		}

		public ISelectionService SelectionService
		{
			get { return DesignItem != null ? DesignItem.Services.Selection : null; }
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
				RaisePropertyChanged("IsExpanded");
			}
		}

		protected bool isSelected;
		public virtual bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				if (isSelected != value && SelectionService != null) {
					isSelected = value;
					SelectionService.SetSelectedComponents(new[] { DesignItem },
					                                       value ? SelectionTypes.Add : SelectionTypes.Remove);
					RaisePropertyChanged("IsSelected");
				}
			}
		}

		bool _isDesignTimeVisible = true;

		public bool IsDesignTimeVisible
		{
			get
			{
				return _isDesignTimeVisible;
			}
			set
			{
				_isDesignTimeVisible = value;

				RaisePropertyChanged("IsDesignTimeVisible");

				//if (!value)
				//	DesignItem.Properties.GetAttachedProperty(DesignTimeProperties.IsHiddenProperty).SetValue(true);
				//else
				//	DesignItem.Properties.GetAttachedProperty(DesignTimeProperties.IsHiddenProperty).Reset();
			}
		}

		bool _isDesignTimeLocked = false;

		public bool IsDesignTimeLocked
		{
			get
			{
				return _isDesignTimeLocked;
			}
			set
			{
				_isDesignTimeLocked = value;
				//((XamlDesignItem)DesignItem).IsDesignTimeLocked = _isDesignTimeLocked;

				RaisePropertyChanged("IsDesignTimeLocked");
			}
		}

		ObservableCollection<IOutlineNode> children = new ObservableCollection<IOutlineNode>();

		public ObservableCollection<IOutlineNode> Children
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


        public virtual string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(_name))
					return _name;
                if (DesignItem.HasProperty("Name") == null)
                    return DesignItem.ComponentType.Name;
                if (string.IsNullOrEmpty(DesignItem.Name))
                {
                    return DesignItem.ComponentType.Name;
                }
                //return DesignItem.ComponentType.Name + " (" + DesignItem.Name + ")";
                return DesignItem.Services.GetService<IOutlineNodeNameService>().GetOutlineNodeName(DesignItem);
			}
		}

		void DesignItem_NameChanged(object sender, EventArgs e)
		{
			RaisePropertyChanged("Name");
		}

		protected void DesignItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!_collectionWasChanged || e.PropertyName == DesignItem.ContentPropertyName)	{
				UpdateChildren();
			}
            else if (e.PropertyName == "Name")
                RaisePropertyChanged(e.PropertyName);
            _collectionWasChanged = false;
		}

		private void CollectionElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_collectionWasChanged = true;
			UpdateChildrenCollectionChanged(e);
		}

		public bool CanInsert(IEnumerable<IOutlineNode> nodes, IOutlineNode after, bool copy)
		{
			var placementBehavior = DesignItem.GetBehavior<IPlacementBehavior>();
			if (placementBehavior == null)
				return false;
			var operation = PlacementOperation.Start(nodes.Select(node => node.DesignItem).ToArray(), DummyPlacementType);
			if (operation != null) {
				bool canEnter = placementBehavior.CanEnterContainer(operation, true);
				operation.Abort();
				return canEnter;
			}
			return false;
		}

		public virtual void Insert(IEnumerable<IOutlineNode> nodes, IOutlineNode after, bool copy)
		{
			using (var moveTransaction = DesignItem.Context.OpenGroup("Item moved in outline view", nodes.Select(n => n.DesignItem).ToList()))
			{
				if (copy) 
				{
                    nodes = nodes.Select(n => OnCreateChildrenNode(n.DesignItem.Clone())).ToList();
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
				if (content.IsCollection) {
					foreach (var node in nodes) {
						content.CollectionElements.Insert(index++, node.DesignItem);
					}
				} else {
					content.SetValue(nodes.First().DesignItem);
				}
				moveTransaction.Commit();
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
}

// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using gip.ext.designer.avui.themes;
using System.Reflection;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.PropertyGrid.Editors
{
	public partial class FlatCollectionEditor : Window
	{
		private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();
		static FlatCollectionEditor()
		{
			TypeMappings.Add(typeof(ListBox),typeof(ListBoxItem));
			//TypeMappings.Add(typeof(ListView),typeof(ListViewItem));
			TypeMappings.Add(typeof(ComboBox),typeof(ComboBoxItem));
			TypeMappings.Add(typeof(TabControl),typeof(TabItem));
			TypeMappings.Add(typeof(ColumnDefinitions),typeof(ColumnDefinition));
			TypeMappings.Add(typeof(RowDefinitions),typeof(RowDefinition));
		}
		
		private DesignItemProperty _itemProperty;
		private IComponentService _componentService;
		private Type _type;

		// Control references
		private ListBox _listBox;
		private ComboBox _itemDataType;
		private Button _addItem;
		private Border _listBoxBorder;

		public FlatCollectionEditor()
		{
			InitializeComponent();
			InitializeControls();
		}

		public FlatCollectionEditor(Window owner)
			: this()
		{
			// In Avalonia, we don't use Owner property, but can center on screen
			this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void InitializeControls()
		{
			_listBox = this.FindControl<ListBox>("ListBox");
			_itemDataType = this.FindControl<ComboBox>("ItemDataType");
			_addItem = this.FindControl<Button>("AddItem");
			_listBoxBorder = this.FindControl<Border>("ListBoxBorder");
		}
		
		
		public Type GetItemsSourceType(Type t)
		{
			// Use IList<Control> instead of UIElementCollection for Avalonia
			if (t == typeof(IList<Control>))
				return typeof(Control);

			Type tp = t.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));

			return (tp != null ) ? tp.GetGenericArguments()[0] : null;
		}
		
		public void LoadItemsCollection(DesignItemProperty itemProperty)
		{
			_itemProperty = itemProperty;
			_componentService=_itemProperty.DesignItem.Services.Component;
			TypeMappings.TryGetValue(_itemProperty.ReturnType, out _type);
			
			_type = _type ?? GetItemsSourceType(_itemProperty.ReturnType);
			
			if (_type == null) {
				_addItem.IsEnabled = false;
			}
			
			_listBox.ItemsSource = _itemProperty.CollectionElements;
			LoadItemsCombobox();
		}

		public void LoadItemsCombobox()
		{
			if (this._type != null)
			{
				var types = new List<Type>();
				types.Add(_type);

				foreach (var items in GetInheritedClasses(_type))
					types.Add(items);
				_itemDataType.ItemsSource = types;
				_itemDataType.SelectedItem = types[0];

				if (types.Count < 2)
				{
					_itemDataType.IsVisible = false; // Changed from Visibility.Collapsed to IsVisible
					_listBoxBorder.Margin = new Thickness(10);
				}
			}
			else
			{
				_itemDataType.IsVisible = false; // Changed from Visibility.Collapsed to IsVisible
				_listBoxBorder.Margin = new Thickness(10);
			}
		}

		private IEnumerable<Type> GetInheritedClasses(Type type)
		{
			return AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).SelectMany(x => GetLoadableTypes(x).Where(y => y.IsClass && !y.IsAbstract && y.IsSubclassOf(type)));
		}

		private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException("assembly");
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(t => t != null);
			}
		}

		private void OnAddItemClicked(object sender, RoutedEventArgs e)
		{
			var comboboxItem = _itemDataType.SelectedItem;
			DesignItem newItem = _componentService.RegisterComponentForDesigner(Activator.CreateInstance((Type)comboboxItem));
			_itemProperty.CollectionElements.Add(newItem);
		}

		private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
		{
			var selItem = _listBox.SelectedItem as DesignItem;
			if (selItem != null)
				_itemProperty.CollectionElements.Remove(selItem);
		}
		
		private void OnMoveItemUpClicked(object sender, RoutedEventArgs e)
		{
			DesignItem selectedItem = _listBox.SelectedItem as DesignItem;
			if (selectedItem!=null) {
				if(_itemProperty.CollectionElements.Count!=1 && _itemProperty.CollectionElements.IndexOf(selectedItem)!=0){
					int moveToIndex=_itemProperty.CollectionElements.IndexOf(selectedItem)-1;
					var itemAtMoveToIndex=_itemProperty.CollectionElements[moveToIndex];
					_itemProperty.CollectionElements.RemoveAt(moveToIndex);
					if ((moveToIndex + 1) < (_itemProperty.CollectionElements.Count+1))
						_itemProperty.CollectionElements.Insert(moveToIndex+1,itemAtMoveToIndex);
				}
			}
		}

		private void OnMoveItemDownClicked(object sender, RoutedEventArgs e)
		{
			DesignItem selectedItem = _listBox.SelectedItem as DesignItem;
			if (selectedItem!=null) {
				var itemCount=_itemProperty.CollectionElements.Count;
				if(itemCount!=1 && _itemProperty.CollectionElements.IndexOf(selectedItem)!=itemCount){
					int moveToIndex=_itemProperty.CollectionElements.IndexOf(selectedItem)+1;
					if(moveToIndex<itemCount){
						var itemAtMoveToIndex=_itemProperty.CollectionElements[moveToIndex];
						_itemProperty.CollectionElements.RemoveAt(moveToIndex);
						if(moveToIndex>0)
							_itemProperty.CollectionElements.Insert(moveToIndex-1,itemAtMoveToIndex);
					}
				}
			}
		}
		
		void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// TODO: PropertyGridView interaction needs to be implemented when PropertyGridView is converted to Avalonia
			// For now, we skip this functionality as PropertyGridView is still WPF-based
			// Original code was: PropertyGridView.PropertyGrid.SelectedItems = ListBox.SelectedItems.Cast<DesignItem>();
		}
	}
}

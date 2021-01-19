using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using combit.ListLabel17;

namespace gip.core.reporthandler 
{
	internal class ObjectTable<T> : ITable, IEnumerable<ITableRow>, IEnumerable, ICanHandleUsedIdentifiers, ISupplyParentTable
	{
		private string _tableName;
		private string _sourcePropertyName;
		private bool _sourceIsEntityCollection;
		private string _navigationId;
		private ObjectDataProvider _provider;
		private IEnumerable _source;
		private IEnumerable _sourceBackup;
		private IEnumerable _parentEnumerable;
		private ISupplyParentTable _parentTable;
		private int? _count = null;
		private ReadOnlyCollection<string> _usedIdentifiers;
		internal ObjectDataProvider Provider
		{
			get
			{
				return this._provider;
			}
		}
		internal string NavigationId
		{
			get
			{
				return this._navigationId;
			}
			set
			{
				this._navigationId = value;
			}
		}
		internal IEnumerable ParentEnumerable
		{
			get
			{
				return this._parentEnumerable;
			}
		}
		public bool SupportsCount
		{
			get
			{
				return this._count.HasValue;
			}
		}
		public bool SupportsSorting
		{
			get
			{
				IBindingList bindingList = this._source as IBindingList;
				return bindingList != null && bindingList.SupportsSorting;
			}
		}
		public bool SupportsAdvancedSorting
		{
			get
			{
				IBindingListView bindingListView = this._source as IBindingListView;
				return bindingListView != null && bindingListView.SupportsAdvancedSorting;
			}
		}
		public bool SupportsFiltering
		{
			get
			{
				return this._provider.ObjectForStructureParsing == null;
			}
		}
		public int Count
		{
			get
			{
				return this._count.Value;
			}
		}
		public string TableName
		{
			get
			{
				return this._tableName;
			}
		}
		public IEnumerable<ITableRow> Rows
		{
			get
			{
				return this;
			}
		}
		public ReadOnlyCollection<string> SortDescriptions
		{
			get
			{
				if (!(this._source is IBindingList))
				{
					throw new NotImplementedException();
				}
				List<string> list = new List<string>();
				ITypedList typedList = this._source as ITypedList;
				PropertyDescriptorCollection propertyDescriptorCollection;
				if (typedList == null)
				{
					propertyDescriptorCollection = ObjectDataProviderCache.GetPropertyDescriptorCollection(typeof(T));
				}
				else
				{
					propertyDescriptorCollection = typedList.GetItemProperties(null);
				}
				foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
				{
					Type propertyType = propertyDescriptor.PropertyType;
					if (ObjectDataProvider.GetPropertyTypeFromType(propertyType) == PropertyType.Column)
					{
						list.Add(propertyDescriptor.Name + " [+]");
						list.Add(propertyDescriptor.Name + " [-]");
					}
				}
				return list.AsReadOnly();
			}
		}
		public ITableRow SchemaRow
		{
			get
			{
				return new ObjectTableRow<T>(default(T), this._source as ITypedList, this, "1");
			}
		}
		internal ReadOnlyCollection<string> UsedIdentifiers
		{
			get
			{
				return this._usedIdentifiers;
			}
		}
		ISupplyParentTable ISupplyParentTable.ParentTable
		{
			get
			{
				return this._parentTable;
			}
		}
		string ISupplyParentTable.SourcePropertyName
		{
			get
			{
				return this._sourcePropertyName;
			}
		}
		public ObjectTable(ObjectDataProvider provider, ISupplyParentTable parentTable, string tableName, string sourcePropertyName, IEnumerable source, IEnumerable parentEnumerable, string navigationId)
		{
			LlCore.LlDebugOutput(0, ".NET: ObjectTable({0},{1},{2})", new object[]
			{
				tableName,
				sourcePropertyName,
				navigationId
			});
			this._tableName = tableName;
			this._source = source;
			this._parentEnumerable = parentEnumerable;
			this._sourcePropertyName = sourcePropertyName;
			this._navigationId = navigationId;
			this._parentTable = parentTable;
			this._provider = provider;
			if (this._source is ICollection)
			{
				this._count = new int?((source as ICollection).Count);
			}
			if (this._source != null)
			{
				Type type = this._source.GetType();
				if (type.Name == "EntityCollection`1" || type.Name == "EntityReference`1")
				{
					this._sourceIsEntityCollection = true;
				}
			}
		}
		private void GetSortParameters(string sortDescription, out PropertyDescriptor property, out ListSortDirection direction)
		{
			if (sortDescription.EndsWith("[+]"))
			{
				sortDescription = sortDescription.Remove(sortDescription.Length - 4);
				direction = ListSortDirection.Ascending;
			}
			else
			{
				if (sortDescription.EndsWith("[-]"))
				{
					sortDescription = sortDescription.Remove(sortDescription.Length - 4);
					direction = ListSortDirection.Descending;
				}
				else
				{
					direction = ListSortDirection.Ascending;
				}
			}
			ITypedList typedList = this._source as ITypedList;
			PropertyDescriptorCollection propertyDescriptorCollection;
			if (typedList == null)
			{
				propertyDescriptorCollection = ObjectDataProviderCache.GetPropertyDescriptorCollection(typeof(T));
			}
			else
			{
				propertyDescriptorCollection = typedList.GetItemProperties(null);
			}
			property = propertyDescriptorCollection[sortDescription];
		}
		private void ApplyAdvancedSort(string sortDescription)
		{
			IBindingListView bindingListView = this._source as IBindingListView;
			if (bindingListView == null)
			{
				throw new NotImplementedException();
			}
			if (!bindingListView.SupportsAdvancedSorting)
			{
				throw new NotImplementedException("The passed binding list view unexpectedly does not support advanced sorting");
			}
			string[] array = sortDescription.Split(new char[]
			{
				'\t'
			});
			ListSortDescription[] array2 = new ListSortDescription[array.GetUpperBound(0) + 1];
			int num = 0;
			string[] array3 = array;
			for (int i = 0; i < array3.Length; i++)
			{
				string sortDescription2 = array3[i];
				PropertyDescriptor property;
				ListSortDirection direction;
				this.GetSortParameters(sortDescription2, out property, out direction);
				array2[num] = new ListSortDescription(property, direction);
				num++;
			}
			ListSortDescriptionCollection sorts = new ListSortDescriptionCollection(array2);
			bindingListView.ApplySort(sorts);
		}
		public void ApplySort(string sortDescription)
		{
			if (this._source is IBindingListView)
			{
				this.ApplyAdvancedSort(sortDescription);
				return;
			}
			IBindingList bindingList = this._source as IBindingList;
			if (bindingList == null)
			{
				throw new NotImplementedException();
			}
			if (!bindingList.SupportsSorting)
			{
				throw new NotImplementedException("The passed binding list unexpectedly does not support sorting");
			}
			PropertyDescriptor property;
			ListSortDirection direction;
			this.GetSortParameters(sortDescription, out property, out direction);
			bindingList.ApplySort(property, direction);
		}
		public void ApplyFilter(string filter)
		{
			if (string.IsNullOrEmpty(filter))
			{
				this._source = this._sourceBackup;
				return;
			}
			this._sourceBackup = this._source;
			if (this._parentEnumerable == null)
			{
				throw new LL_BadDatabaseStructure_Exception("The parent enumerable expected for Drilldown does not exist.");
			}
			string text = filter.Split(new char[]
			{
				'='
			})[1];
			string[] array = text.Split(new char[]
			{
				'.'
			});
			ArrayList arrayList = new ArrayList();
			arrayList.Insert(0, ((ISupplyParentTable)this).SourcePropertyName);
			for (ISupplyParentTable parentTable = this._parentTable; parentTable != null; parentTable = parentTable.ParentTable)
			{
				arrayList.Insert(0, parentTable.SourcePropertyName);
			}
			string[] array2 = (string[])arrayList.ToArray(typeof(string));
			IEnumerable enumerable = this._provider.RootEnumerable;
			IEnumerator enumerator = enumerable.GetEnumerator();
			object obj = null;
			for (int i = 1; i <= array.GetUpperBound(0); i++)
			{
				for (int j = 0; j < Convert.ToInt32(array[i]); j++)
				{
					enumerator.MoveNext();
					obj = enumerator.Current;
				}
				if (enumerator is IDisposable)
				{
					(enumerator as IDisposable).Dispose();
				}
				try
				{
					PropertyDescriptorCollection propertyDescriptorCollection = ObjectDataProviderCache.GetPropertyDescriptorCollection(obj.GetType());
					object value = propertyDescriptorCollection[array2[i]].GetValue(obj);
					if (value == null)
					{
						this._source = null;
						this._navigationId = text;
						return;
					}
					enumerable = (value as IEnumerable);
					if (enumerable == null)
					{
						Type type = typeof(List<>).MakeGenericType(new Type[]
						{
							value.GetType()
						});
						IList list = Activator.CreateInstance(type) as IList;
						list.Add(value);
						enumerable = list;
					}
					enumerator = enumerable.GetEnumerator();
				}
				catch (NullReferenceException)
				{
					throw new LL_BadDatabaseStructure_Exception(string.Format("Cannot find property {0} in type {1}", array2[i], obj.GetType().FullName));
				}
			}
			this._source = (enumerable as IEnumerable<T>);
			this._navigationId = text;
		}
		public IEnumerator<ITableRow> GetEnumerator()
		{
			if (this._source != null && this._sourceIsEntityCollection)
			{
				Type type = this._source.GetType();
				PropertyInfo property = type.GetProperty("IsLoaded");
				if (property != null && !(bool)property.GetValue(this._source, null))
				{
					MethodInfo method = type.GetMethod("Load", new Type[0]);
					method.Invoke(this._source, null);
				}
			}
			return new ObjectEnumerator<T>(this._source, this);
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		public void SetUsedIdentifiers(ReadOnlyCollection<string> identifiers)
		{
			this._usedIdentifiers = identifiers;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using combit.ListLabel17;

namespace gip.core.reporthandler
{
	internal class ObjectTableRow<T> : ITableRow
	{
		private ObjectTable<T> _table;
		private T _source;
		private string _navigationId;
		private ITypedList _typedList;
		private List<ITableColumn> _columns;
		private PropertyDescriptorCollection _propertyCollection;
		public bool SupportsGetParentRow
		{
			get
			{
				return false;
			}
		}
		public string TableName
		{
			get
			{
				return this._table.TableName;
			}
		}
		public ReadOnlyCollection<ITableColumn> Columns
		{
			get
			{
				if (this._columns == null)
				{
					this.InitColumns();
				}
				return this._columns.AsReadOnly();
			}
		}
		public ObjectTableRow(object source, ITypedList typedList, ObjectTable<T> table, string navigationId)
		{
			this._table = table;
			try
			{
				this._source = (T)((object)source);
			}
			catch (InvalidCastException innerException)
			{
				if (source != DBNull.Value)
				{
					throw new LL_BadDatabaseStructure_Exception("The object type of one of the objects in the enumeration did not match the type of the first object. Consider using an IEnumerable<T> for type safety.", innerException);
				}
				this._source = default(T);
			}
			this._navigationId = navigationId;
			this._typedList = typedList;
		}
		private void InitColumns()
		{
			this._columns = new List<ITableColumn>();
			if (ObjectDataProvider.GetPropertyTypeFromType(typeof(T)) != PropertyType.Column)
			{
				if (this._typedList == null)
				{
					this._propertyCollection = ObjectDataProviderCache.GetPropertyDescriptorCollection(typeof(T));
				}
				else
				{
					this._propertyCollection = this._typedList.GetItemProperties(null);
				}
				IEnumerator enumerator = this._propertyCollection.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						PropertyDescriptor propertyDescriptor = (PropertyDescriptor)enumerator.Current;
						if (this._table.UsedIdentifiers == null || this._table.UsedIdentifiers.Contains(propertyDescriptor.DisplayName))
						{
							Type propertyType = propertyDescriptor.PropertyType;
							if (ObjectDataProvider.GetPropertyTypeFromType(propertyType) == PropertyType.Column)
							{
								if (propertyType.IsGenericType && typeof(IDictionary).IsAssignableFrom(propertyType.GetGenericTypeDefinition()))
								{
									IDictionary dictionary = (IDictionary)propertyDescriptor.GetValue(this._source);
									IEnumerator enumerator2 = dictionary.Keys.GetEnumerator();
									try
									{
										while (enumerator2.MoveNext())
										{
											object current = enumerator2.Current;
											this._columns.Add(new ObjectTableColumn(propertyDescriptor.DisplayName + "." + current.ToString(), dictionary[current].GetType(), dictionary[current]));
										}
										continue;
									}
									finally
									{
										IDisposable disposable = enumerator2 as IDisposable;
										if (disposable != null)
										{
											disposable.Dispose();
										}
									}
								}
								try
								{
									this._columns.Add(new ObjectTableColumn(propertyDescriptor.DisplayName, propertyType, propertyDescriptor.GetValue(this._source)));
								}
								catch (TargetInvocationException)
								{
									this._columns.Add(new ObjectTableColumn(propertyDescriptor.DisplayName, propertyType, DBNull.Value));
								}
							}
						}
					}
					goto IL_20F;
				}
				finally
				{
					IDisposable disposable2 = enumerator as IDisposable;
					if (disposable2 != null)
					{
						disposable2.Dispose();
					}
				}
			}
			if (this._table.UsedIdentifiers == null || this._table.UsedIdentifiers.Contains("Value"))
			{
				this._columns.Add(new ObjectTableColumn("Value", typeof(T), this._source));
			}
			IL_20F:
			this._columns.Add(new ObjectTableColumn("__LL__ID", typeof(int), this._navigationId));
		}
		public ITable GetChildTable(ITableRelation relation)
		{
			if (this._columns == null)
			{
				this.InitColumns();
			}
			Type genericEnumerableTableType = (relation as ObjectTableRelation).GenericEnumerableTableType;
			string childPropertyName = (relation as ObjectTableRelation).ChildPropertyName;
			if (genericEnumerableTableType != null)
			{
				LoadDeferredContentEventArgs e = new LoadDeferredContentEventArgs(this._source, childPropertyName);
				this._table.Provider.OnLoadDeferredContent(e);
				object value = this._propertyCollection[(relation as ObjectTableRelation).ChildPropertyName].GetValue(this._source);
				object obj = null;
				if (value != null)
				{
					Type propertyType = this._propertyCollection[childPropertyName].PropertyType;
					obj = ((propertyType.Name == "EntityCollection`1" || propertyType.Name == "EntityReference`1") ? value : null);
				}
				if (obj == null)
				{
					IListSource listSource = value as IListSource;
					if (listSource != null)
					{
						obj = listSource.GetList();
					}
					else
					{
						obj = (value as IEnumerable);
					}
				}
				object[] args = new object[]
				{
					this._table.Provider,
					this._table,
					relation.ChildTableName,
					childPropertyName,
					obj,
					this._table.ParentEnumerable,
					this._navigationId
				};
				return (ITable)Activator.CreateInstance(genericEnumerableTableType, args);
			}
			Type type = typeof(List<>).MakeGenericType(new Type[]
			{
				this._propertyCollection[(relation as ObjectTableRelation).ChildPropertyName].PropertyType
			});
			IList list = Activator.CreateInstance(type) as IList;
			Type genericObjectTableType = ObjectDataProviderCache.GetGenericObjectTableType(this._propertyCollection[(relation as ObjectTableRelation).ChildPropertyName].PropertyType);
			list.Add(this._propertyCollection[(relation as ObjectTableRelation).ChildPropertyName].GetValue(this._source));
			object[] args2 = new object[]
			{
				this._table.Provider,
				this._table,
				relation.ChildTableName,
				childPropertyName,
				list,
				this._table,
				this._navigationId
			};
			return (ITable)Activator.CreateInstance(genericObjectTableType, args2);
		}
		public ITableRow GetParentRow(ITableRelation relation)
		{
			throw new NotImplementedException();
		}
	}
}

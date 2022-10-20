using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
namespace combit.ListLabel17.DataProviders
{
	public sealed class ObjectDataProvider : IDataProvider, ICanHandleUsedIdentifiers
	{
		private object _source;
		private IEnumerable _rootEnumerable;
		private List<ITable> _tables = new List<ITable>();
		private Dictionary<string, Type> _tableTypes = new Dictionary<string, Type>();
		private List<ITableRelation> _relations = new List<ITableRelation>();
		private static Dictionary<Type, PropertyType> _propertyTypeCache = new Dictionary<Type, PropertyType>();
		private bool _initialized;
		public event EventHandler<HandleEnumerablePropertyEventArgs> HandleEnumerableProperty;
		public event EventHandler<LoadDeferredContentEventArgs> LoadDeferredContent;
		internal IEnumerable RootEnumerable
		{
			get
			{
				return this._rootEnumerable;
			}
		}
		public string RootTableName
		{
			get;
			set;
		}
		public object ObjectForStructureParsing
		{
			get;
			set;
		}
		public int MaximumRecursionDepth
		{
			get;
			set;
		}
		bool IDataProvider.SupportsAnyBaseTable
		{
			get
			{
				return false;
			}
		}
		ReadOnlyCollection<ITable> IDataProvider.Tables
		{
			get
			{
				this.Init();
				return this._tables.AsReadOnly();
			}
		}
		ReadOnlyCollection<ITableRelation> IDataProvider.Relations
		{
			get
			{
				this.Init();
				return this._relations.AsReadOnly();
			}
		}
		public ObjectDataProvider(object source, int maximumRecursionDepth)
		{
			this.MaximumRecursionDepth = maximumRecursionDepth;
			this._source = source;
			Type type = source.GetType();
			if (type.IsGenericType)
			{
				this.RootTableName = type.GetGenericArguments()[0].Name;
				return;
			}
			ITypedList typedList = this._source as ITypedList;
			if (typedList != null)
			{
				this.RootTableName = typedList.GetListName(null);
				return;
			}
			this.RootTableName = type.Name;
		}
		public ObjectDataProvider(object source) : this(source, 10)
		{
		}
		internal static PropertyType GetPropertyTypeFromType(Type type)
		{
			Dictionary<Type, PropertyType> propertyTypeCache;
			Monitor.Enter(propertyTypeCache = ObjectDataProvider._propertyTypeCache);
			PropertyType propertyType;
			try
			{
				if (!ObjectDataProvider._propertyTypeCache.TryGetValue(type, out propertyType))
				{
					if (type.IsValueType || typeof(string).IsAssignableFrom(type) || typeof(byte[]).IsAssignableFrom(type) || typeof(Image).IsAssignableFrom(type))
					{
						propertyType = PropertyType.Column;
					}
					else
					{
						if (typeof(IEnumerable).IsAssignableFrom(type))
						{
							if (type.IsGenericType)
							{
								if (type.GetGenericArguments().GetUpperBound(0) == 0)
								{
									propertyType = PropertyType.GenericEnumerable;
								}
								else
								{
									if (typeof(IDictionary).IsAssignableFrom(type.GetGenericTypeDefinition()))
									{
										propertyType = PropertyType.Column;
									}
								}
							}
							else
							{
								propertyType = PropertyType.Enumerable;
							}
						}
						else
						{
							if (typeof(IListSource).IsAssignableFrom(type))
							{
								propertyType = PropertyType.ListSource;
							}
							else
							{
								if (type.IsClass && type != typeof(string))
								{
									propertyType = PropertyType.Class;
								}
								else
								{
									if (!type.IsInterface)
									{
										throw new LL_BadDatabaseStructure_Exception(string.Format("Type {0} not supported for databinding.", type.FullName));
									}
									propertyType = PropertyType.Class;
								}
							}
						}
					}
					ObjectDataProvider._propertyTypeCache.Add(type, propertyType);
				}
			}
			finally
			{
				Monitor.Exit(propertyTypeCache);
			}
			return propertyType;
		}
		private void Init()
		{
			if (this._initialized)
			{
				return;
			}
			this._initialized = true;
			IEnumerable enumerable = null;
			IEnumerable enumerable2 = null;
			Type type = this._source.GetType();
			Type type2 = null;
			Type sourceType = null;
			switch (ObjectDataProvider.GetPropertyTypeFromType(type))
			{
			case PropertyType.Enumerable:
				enumerable = (this._source as IEnumerable);
				if (this.ObjectForStructureParsing != null)
				{
					enumerable2 = (this.ObjectForStructureParsing as IEnumerable);
				}
				break;
			case PropertyType.GenericEnumerable:
				enumerable = (this._source as IEnumerable);
				if (this.ObjectForStructureParsing != null)
				{
					enumerable2 = (this.ObjectForStructureParsing as IEnumerable);
				}
				type2 = type.GetGenericArguments()[0];
				sourceType = type;
				break;
			case PropertyType.ListSource:
			{
				enumerable = (this._source as IListSource).GetList();
				if (this.ObjectForStructureParsing != null)
				{
					enumerable2 = (this.ObjectForStructureParsing as IListSource).GetList();
				}
				Type type3 = enumerable.GetType();
				if (ObjectDataProvider.GetPropertyTypeFromType(type3) == PropertyType.GenericEnumerable)
				{
					type2 = type3.GetGenericArguments()[0];
					sourceType = type3;
				}
				break;
			}
			case PropertyType.Class:
			{
				Type type4 = typeof(List<>).MakeGenericType(new Type[]
				{
					type
				});
				IList list = Activator.CreateInstance(type4) as IList;
				if (this.ObjectForStructureParsing != null)
				{
					IList list2 = Activator.CreateInstance(type4) as IList;
					list2.Add(this.ObjectForStructureParsing);
					enumerable2 = list2;
				}
				list.Add(this._source);
				enumerable = list;
				type2 = type;
				sourceType = type4;
				break;
			}
			default:
				throw new LL_BadDatabaseStructure_Exception("Cannot use this object as DataSource. It isn't a class or interface type, nor does it implement IEnumerable or IListSource.");
			}
			if (type2 == null)
			{
				IEnumerator enumerator = null;
				try
				{
					enumerator = ((this.ObjectForStructureParsing != null) ? enumerable2.GetEnumerator() : enumerable.GetEnumerator());
					if (!enumerator.MoveNext() || enumerator.Current == null)
					{
						throw new LL_NoData_Exception();
					}
					type2 = enumerator.Current.GetType();
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				sourceType = ObjectDataProviderCache.GetGenericEnumerableType(type2);
			}
			this._rootEnumerable = enumerable;
			this.BuildDom(enumerable2, enumerable, null, sourceType, this.RootTableName, null, "1", null, 0, this.RootTableName);
		}
		private string GetUniqueTableName(string parentTableName, string suggestedChildTableName)
		{
			if (!this._tableTypes.ContainsKey(suggestedChildTableName))
			{
				return suggestedChildTableName;
			}
			return parentTableName + suggestedChildTableName;
		}
		private void OnHandleEnumerableProperty(HandleEnumerablePropertyEventArgs e)
		{
			if (this.HandleEnumerableProperty != null)
			{
				this.HandleEnumerableProperty(this, e);
			}
		}
		internal void OnLoadDeferredContent(LoadDeferredContentEventArgs e)
		{
			if (this.LoadDeferredContent != null)
			{
				this.LoadDeferredContent(this, e);
			}
		}
		private void BuildDom(IEnumerable sourceForStructureParsing, IEnumerable source, IEnumerable parentSource, Type sourceType, string tableName, string propertyName, string navigationId, ISupplyParentTable parentTable, int recursionDepth, string propertyPath)
		{
			IEnumerable enumerable = (sourceForStructureParsing != null) ? sourceForStructureParsing : source;
			LlCore.LlDebugOutput(0, ".NET: ObjectDataProvider.BuildDom({0},{1},{2},{3},{4},{5})", new object[]
			{
				sourceType.Name,
				tableName,
				propertyName,
				navigationId,
				recursionDepth,
				propertyPath
			});
			this._initialized = true;
			if (recursionDepth > this.MaximumRecursionDepth)
			{
				this._relations.RemoveAt(this._relations.Count - 1);
				return;
			}
			if (ObjectDataProvider.GetPropertyTypeFromType(sourceType) == PropertyType.Column)
			{
				return;
			}
			Type t;
			if (sourceType.IsGenericType)
			{
				t = sourceType.GetGenericArguments()[0];
			}
			else
			{
				IEnumerator enumerator = null;
				try
				{
					enumerator = enumerable.GetEnumerator();
					if (!enumerator.MoveNext())
					{
						throw new LL_NoData_Exception();
					}
					if (enumerator.Current == null)
					{
						throw new LL_NoData_Exception();
					}
					t = enumerator.Current.GetType();
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			Type genericObjectTableType = ObjectDataProviderCache.GetGenericObjectTableType(t);
			Type type;
			if (this._tableTypes.TryGetValue(tableName, out type))
			{
				if (type != genericObjectTableType)
				{
					throw new LL_BadDatabaseStructure_Exception(string.Format("The property '{0}' exists multiple times in the item hierarchy with different types. Make sure to name properties with different types differently.", tableName));
				}
			}
			else
			{
				this._tableTypes.Add(tableName, genericObjectTableType);
			}
			object obj = Activator.CreateInstance(genericObjectTableType, new object[]
			{
				this,
				parentTable,
				tableName,
				propertyName,
				source,
				parentSource,
				navigationId
			});
			this._tables.Add((ITable)obj);
			HandleEnumerablePropertyEventArgs handleEnumerablePropertyEventArgs = new HandleEnumerablePropertyEventArgs(propertyPath, false);
			this.OnHandleEnumerableProperty(handleEnumerablePropertyEventArgs);
			if (handleEnumerablePropertyEventArgs.CancelRecursion)
			{
				return;
			}
			object obj2 = null;
			ITypedList typedList = null;
			if (enumerable != null)
			{
				IEnumerator enumerator2 = null;
				try
				{
					enumerator2 = enumerable.GetEnumerator();
					if (enumerator2.MoveNext())
					{
						try
						{
							obj2 = enumerator2.Current;
						}
						catch (ArgumentOutOfRangeException)
						{
						}
					}
					typedList = (enumerable as ITypedList);
				}
				finally
				{
					IDisposable disposable2 = enumerator2 as IDisposable;
					if (disposable2 != null)
					{
						disposable2.Dispose();
					}
				}
			}
			PropertyDescriptorCollection propertyDescriptorCollection;
			if (typedList == null)
			{
				propertyDescriptorCollection = ObjectDataProviderCache.GetPropertyDescriptorCollection(t);
			}
			else
			{
				propertyDescriptorCollection = typedList.GetItemProperties(null);
			}
			foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
			{
				Type propertyType = propertyDescriptor.PropertyType;
				LlCore.LlDebugOutput(0, ".NET: Examining property {0} ({1})", new object[]
				{
					propertyDescriptor.Name,
					propertyType.Name
				});
				switch (ObjectDataProvider.GetPropertyTypeFromType(propertyType))
				{
				case PropertyType.Enumerable:
				{
					LlCore.LlDebugOutput(0, "-> Enumerable");
					string uniqueTableName = this.GetUniqueTableName(tableName, propertyDescriptor.DisplayName);
					string relationName = tableName + "2" + uniqueTableName;
					IEnumerable enumerable2 = propertyDescriptor.GetValue(obj2) as IEnumerable;
					if (enumerable2 != null)
					{
						IEnumerator enumerator4 = null;
						Type type2;
						try
						{
							enumerator4 = enumerable2.GetEnumerator();
							if (enumerator4 == null)
							{
								break;
							}
							if (!enumerator4.MoveNext())
							{
								break;
							}
							try
							{
								if (enumerator4.Current == null)
								{
									break;
								}
							}
							catch (ArgumentOutOfRangeException)
							{
								break;
							}
							type2 = enumerator4.Current.GetType();
						}
						finally
						{
							IDisposable disposable3 = enumerator4 as IDisposable;
							if (disposable3 != null)
							{
								disposable3.Dispose();
							}
						}
						Type genericEnumerableType = ObjectDataProviderCache.GetGenericEnumerableType(type2);
						Type genericObjectTableType2 = ObjectDataProviderCache.GetGenericObjectTableType(type2);
						if (((IDataProvider)this).GetRelation(relationName) == null)
						{
							this._relations.Add(new ObjectTableRelation(tableName, genericObjectTableType2, uniqueTableName, propertyDescriptor.Name, relationName));
							this.BuildDom(null, enumerable2, source, genericEnumerableType, uniqueTableName, propertyDescriptor.Name, navigationId + ".1", obj as ISupplyParentTable, recursionDepth + 1, propertyPath + ">" + uniqueTableName);
						}
					}
					break;
				}
				case PropertyType.GenericEnumerable:
				{
					LlCore.LlDebugOutput(0, "-> GenericEnumerable");
					string uniqueTableName2 = this.GetUniqueTableName(tableName, propertyDescriptor.DisplayName);
					string relationName2 = tableName + "2" + uniqueTableName2;
					Type genericObjectTableType3 = ObjectDataProviderCache.GetGenericObjectTableType(propertyType.GetGenericArguments()[0]);
					if (((IDataProvider)this).GetRelation(relationName2) == null)
					{
						this._relations.Add(new ObjectTableRelation(tableName, genericObjectTableType3, uniqueTableName2, propertyDescriptor.Name, relationName2));
						this.BuildDom(null, propertyDescriptor.GetValue(obj2) as IEnumerable, source, propertyType, uniqueTableName2, propertyDescriptor.Name, navigationId + ".1", obj as ISupplyParentTable, recursionDepth + 1, propertyPath + ">" + uniqueTableName2);
					}
					break;
				}
				case PropertyType.ListSource:
				{
					LlCore.LlDebugOutput(0, "-> ListSource");
					object component = obj2;
					IListSource listSource = propertyDescriptor.GetValue(component) as IListSource;
					if (listSource != null)
					{
						IList list = listSource.GetList();
						Type type3 = list.GetType();
						switch (ObjectDataProvider.GetPropertyTypeFromType(type3))
						{
						case PropertyType.Enumerable:
						{
							LlCore.LlDebugOutput(0, "-> ListSource/Enumerable");
							string uniqueTableName3 = this.GetUniqueTableName(tableName, propertyDescriptor.DisplayName);
							string relationName3 = tableName + "2" + uniqueTableName3;
							if (list.Count != 0)
							{
								Type type4 = list[0].GetType();
								ObjectDataProviderCache.GetGenericEnumerableType(type4);
								Type genericObjectTableType4 = ObjectDataProviderCache.GetGenericObjectTableType(type4);
								if (((IDataProvider)this).GetRelation(relationName3) == null)
								{
									this._relations.Add(new ObjectTableRelation(tableName, genericObjectTableType4, uniqueTableName3, propertyDescriptor.Name, relationName3));
									this.BuildDom(null, list, source, type3, uniqueTableName3, propertyDescriptor.Name, navigationId + ".1", obj as ISupplyParentTable, recursionDepth + 1, propertyPath + ">" + uniqueTableName3);
								}
							}
							break;
						}
						case PropertyType.GenericEnumerable:
						{
							LlCore.LlDebugOutput(0, "-> ListSource/GenericEnumerable");
							string uniqueTableName4 = this.GetUniqueTableName(tableName, propertyDescriptor.DisplayName);
							string relationName4 = tableName + "2" + uniqueTableName4;
							Type genericObjectTableType5 = ObjectDataProviderCache.GetGenericObjectTableType(type3.GetGenericArguments()[0]);
							if (((IDataProvider)this).GetRelation(relationName4) == null)
							{
								this._relations.Add(new ObjectTableRelation(tableName, genericObjectTableType5, uniqueTableName4, propertyDescriptor.Name, relationName4));
								this.BuildDom(null, list, source, type3, uniqueTableName4, propertyDescriptor.Name, navigationId + ".1", obj as ISupplyParentTable, recursionDepth + 1, propertyPath + ">" + uniqueTableName4);
							}
							break;
						}
						default:
							throw new LL_BadDatabaseStructure_Exception(string.Format("The list source {0} did not return an Enumerable result.", listSource.GetType().FullName));
						}
					}
					break;
				}
				case PropertyType.Class:
				{
					LlCore.LlDebugOutput(0, "-> Class");
					string uniqueTableName5 = this.GetUniqueTableName(tableName, propertyDescriptor.DisplayName);
					string relationName5 = tableName + "2" + uniqueTableName5;
					if (((IDataProvider)this).GetRelation(relationName5) == null)
					{
						this._relations.Add(new ObjectTableRelation(tableName, null, uniqueTableName5, propertyDescriptor.Name, relationName5));
						Type type5 = typeof(List<>).MakeGenericType(new Type[]
						{
							propertyType
						});
						IList list2 = Activator.CreateInstance(type5) as IList;
						list2.Add(propertyDescriptor.GetValue(obj2));
						this.BuildDom(null, list2, source, type5, uniqueTableName5, propertyDescriptor.Name, navigationId + ".1", obj as ISupplyParentTable, recursionDepth + 1, propertyPath + ">" + uniqueTableName5);
					}
					break;
				}
				}
			}
		}
		ITable IDataProvider.GetTable(string tableName)
		{
			this.Init();
			foreach (ITable current in this._tables)
			{
				if (current.TableName == tableName)
				{
					return current;
				}
			}
			return null;
		}
		ITableRelation IDataProvider.GetRelation(string relationName)
		{
			this.Init();
			foreach (ITableRelation current in this._relations)
			{
				if (current.RelationName == relationName)
				{
					return current;
				}
			}
			return null;
		}
		void ICanHandleUsedIdentifiers.SetUsedIdentifiers(ReadOnlyCollection<string> identifiers)
		{
			UsedIdentifierHelper usedIdentifierHelper = new UsedIdentifierHelper(identifiers);
			foreach (ITable current in this._tables)
			{
				ReadOnlyCollection<string> identifiersForTable = usedIdentifierHelper.GetIdentifiersForTable(current.TableName);
				if (identifiersForTable.Count > 0)
				{
					(current as ICanHandleUsedIdentifiers).SetUsedIdentifiers(identifiersForTable);
				}
			}
		}
	}
}
